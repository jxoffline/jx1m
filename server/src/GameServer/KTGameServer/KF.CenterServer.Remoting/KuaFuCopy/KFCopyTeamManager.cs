using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using KF.Contract.Data;
using GameServer.Core.Executor;
using Server.Tools;
using Tmsk.Contract;

namespace KF.Remoting
{
    /// <summary>
    /// 跨服多人副本房间管理
    /// </summary>
    public class KFCopyTeamManager
    {
        /// <summary>
        /// 单程线大锁！！！
        /// </summary>
        private object Mutex = new object();

        /// <summary>
        /// roleid ---> teamid
        /// </summary>
        private Dictionary<int, long> RoleId2JoinedTeam = new Dictionary<int, long>();

        /// <summary>
        /// teamid ---> team data
        /// </summary>
        private Dictionary<long, CopyTeamData> CopyTeamDict = new Dictionary<long, CopyTeamData>();

        /// <summary>
        /// copyid ---> teams
        /// </summary>
        private Dictionary<int, HashSet<long>> CopyId2Teams = new Dictionary<int, HashSet<long>>();

        /// <summary>
        /// teamid ---> 超时时间ms
        /// </summary>
        private long TimeLimitCopyLastCheckMs = 0;
        private Dictionary<long, long> TimeLimitCopy = new Dictionary<long, long>();

        /// <summary>
        /// 所有新产生的房间事件
        /// </summary>
        private Queue<AsyncDataItem> RoomEventQ = new Queue<AsyncDataItem>();

        /// <summary>
        /// 跨服RPC Server对象
        /// </summary>
        private KuaFuCopyService _KFCopyService = null;

        public KFCopyTeamManager()
        {
        }

        public void SetService(KuaFuCopyService service)
        {
            _KFCopyService = service;
        }

        /// <summary>
        /// 创建房间
        /// </summary>
        public KFCopyTeamCreateRsp CreateTeam(KFCopyTeamCreateReq req)
        {
            KFCopyTeamCreateRsp rsp = new KFCopyTeamCreateRsp();
            try
            {
                lock (Mutex)
                {
                    // 这里有个很微妙的情况，因为创建队伍时，首先GameServer要检查是否已经加入了一个队伍
                    // 但是如果GameServer重启了，那么必然能通过检查，但是其实在中心上已经为该角色加入了一个房间，必须强制踢掉
                    ForceLeaveRoom(req.Member.RoleID);

                    if (!ClientAgentManager.Instance().IsAnyKfAgentAlive())
                    {
                        rsp.ErrorCode = CopyTeamErrorCodes.KFServerIsBusy;
                        return rsp;
                    }

                    // 房间数量和 跨服活动服务器负载的检查
                    KFTeamCountControl control = _KFCopyService.dbMgr.TeamControl;
                    if (control == null)
                    {
                        LogManager.WriteLog(LogTypes.Error, string.Format("跨服队伍创建失败,  丢失副本上线控制的配置文件 KFTeamCountControl"));
                        rsp.ErrorCode = CopyTeamErrorCodes.ServerException;
                        return rsp;
                    }

                    HashSet<long> teamList = null;
                    if (!CopyId2Teams.TryGetValue(req.CopyId, out teamList))
                    {
                        teamList = new HashSet<long>();
                        CopyId2Teams[req.CopyId] = teamList;
                    }

                    CopyTeamData td = new CopyTeamData();
                    td.TeamID = req.TeamId;
                    td.LeaderRoleID = req.Member.RoleID;
                    td.FuBenId = req.CopyId;
                    td.MinZhanLi = req.MinCombat;
                    td.AutoStart = req.AutoStart > 0;
                    td.TeamRoles.Add(req.Member);
                    td.TeamRoles[0].IsReady = true;
                    td.TeamName = td.TeamRoles[0].RoleName;
                    td.MemberCount = td.TeamRoles.Count;

                    CopyTeamDict.Add(td.TeamID, td);
                    teamList.Add(td.TeamID);
                    TimeLimitCopy.Add(td.TeamID, TimeUtil.NOW() + control.TeamMaxWaitMinutes * 60 * 1000);

                    RoleId2JoinedTeam[req.Member.RoleID] = td.TeamID;

                    CopyTeamCreateData data = new CopyTeamCreateData();
                    data.Member = req.Member;
                    data.MinCombat = req.MinCombat;
                    data.CopyId = req.CopyId;
                    data.TeamId = td.TeamID;
                    data.AutoStart = req.AutoStart;

                    // 异步广播创建房间的事件
                    AsyncDataItem evItem = new AsyncDataItem();
                    evItem.EventType = KuaFuEventTypes.KFCopyTeamCreate;
                    evItem.Args = new object[2] { req.Member.ServerId, data };
                    AddAsyncEvent(evItem);

                    rsp.ErrorCode = CopyTeamErrorCodes.Success;
                    rsp.Data = data;
                }
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("跨服队伍创建异常, serverid={0}, role={1}, copyid={2}", req.Member.ServerId, req.Member.RoleID, req.CopyId), ex);
                rsp.ErrorCode = CopyTeamErrorCodes.CenterServerFailed;
            }

            return rsp;
        }

        /// <summary>
        /// 加入房间
        /// </summary>
        public KFCopyTeamJoinRsp JoinTeam(KFCopyTeamJoinReq req)
        {
            KFCopyTeamJoinRsp rsp = new KFCopyTeamJoinRsp();
            try
            {
                lock (Mutex)
                {
                    // 这里有个很微妙的情况，因为加入队伍时，首先GameServer要检查
                    // 但是如果GameServer重启了，那么必然能通过检查，但是其实在中心上已经为该角色加入了一个队伍，必须强制踢掉
                    ForceLeaveRoom(req.Member.RoleID);

                    CopyTeamData td = null;
                    if (!CopyTeamDict.TryGetValue(req.TeamId, out td))
                    {
                        // 房间不存在！！！ GameServer要检查这个错误码，可能出现的情况是 跨服中心重启了，那么GameServer要把这个房间清掉
                        rsp.ErrorCode = CopyTeamErrorCodes.TeamIsDestoryed;
                        return rsp;
                    }

                    if (td.StartTime > 0)
                    {
                        // 房间已经开始了
                        rsp.ErrorCode = CopyTeamErrorCodes.TeamAlreadyStart;
                        return rsp;
                    }

                    // 擦，写死了5个人
                    if (td.MemberCount >= ConstData.CopyRoleMax(req.CopyId))
                    {
                        rsp.ErrorCode = CopyTeamErrorCodes.TeamIsFull;
                        return rsp;
                    }

                    req.Member.IsReady = false;
                    td.TeamRoles.Add(req.Member);
                    td.MemberCount = td.TeamRoles.Count;

                    RoleId2JoinedTeam[req.Member.RoleID] = td.TeamID;

                    // 异步广播加入队伍的事件
                    CopyTeamJoinData data = new CopyTeamJoinData();
                    data.Member = req.Member;
                    data.TeamId = req.TeamId;
                    AsyncDataItem evItem = new AsyncDataItem();
                    evItem.EventType = KuaFuEventTypes.KFCopyTeamJoin;
                    evItem.Args = new object[2] { req.Member.ServerId, data };
                    AddAsyncEvent(evItem);

                    rsp.ErrorCode = CopyTeamErrorCodes.Success;
                    rsp.Data = data;
                }
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("加入跨服副本队伍异常, serverid={0}, role={1}, teamid={2}", req.Member.ServerId, req.Member.RoleID, req.TeamId), ex);
                rsp.ErrorCode = CopyTeamErrorCodes.CenterServerFailed;
            }

            return rsp;
        }

        /// <summary>
        /// 踢出队伍
        /// </summary>
        public KFCopyTeamKickoutRsp KickoutTeam(KFCopyTeamKickoutReq req)
        {
            KFCopyTeamKickoutRsp rsp = new KFCopyTeamKickoutRsp();
            try
            {
                lock (Mutex)
                {
                    CopyTeamData td = null;
                    if (!CopyTeamDict.TryGetValue(req.TeamId, out td))
                    {
                        // 房间不存在！！！ GameServer要检查这个错误码，可能出现的情况是 跨服中心重启了，那么GameServer要把这个房间清掉
                        rsp.ErrorCode = CopyTeamErrorCodes.TeamIsDestoryed;
                        return rsp;
                    }

                    if (td.StartTime > 0)
                    {
                        // 已经开始了，别踢
                        rsp.ErrorCode = CopyTeamErrorCodes.TeamAlreadyStart;
                        return rsp;
                    }

                    if (td.LeaderRoleID != req.FromRoleId)
                    {
                        // 不是队长
                        rsp.ErrorCode = CopyTeamErrorCodes.NotTeamLeader;
                        return rsp;
                    }

                    CopyTeamMemberData leader = td.TeamRoles.Find(_role => _role.RoleID == req.FromRoleId);
                    if (leader == null || leader.RoleID != req.FromRoleId)
                    {
                        rsp.ErrorCode = CopyTeamErrorCodes.NotTeamLeader;
                        return rsp;
                    }

                    CopyTeamMemberData member = td.TeamRoles.Find(_role => _role.RoleID == req.ToRoleId);
                    if (member == null)
                    {
                        // 不在本队伍？？？
                        rsp.ErrorCode = CopyTeamErrorCodes.NotInMyTeam;
                        return rsp;
                    }

                    td.TeamRoles.Remove(member);
                    td.MemberCount = td.TeamRoles.Count;
                    RoleId2JoinedTeam.Remove(req.ToRoleId);

                    CopyTeamKickoutData data = new CopyTeamKickoutData();
                    data.FromRoleId = req.FromRoleId;
                    data.ToRoleId = req.ToRoleId;
                    data.TeamId = req.TeamId;
                    AsyncDataItem evItem = new AsyncDataItem();
                    evItem.EventType = KuaFuEventTypes.KFCopyTeamKickout;
                    evItem.Args = new object[2] { leader.ServerId, data };
                    AddAsyncEvent(evItem);

                    rsp.ErrorCode = CopyTeamErrorCodes.Success;
                    rsp.Data = data;
                }
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("踢出跨服副本队伍异常, role={0}, teamid={1}", req.FromRoleId, req.TeamId), ex);
                rsp.ErrorCode = CopyTeamErrorCodes.CenterServerFailed;
            }
            return rsp;
        }

        /// <summary>
        /// 退出队伍
        /// </summary>
        public KFCopyTeamLeaveRsp LeaveTeam(KFCopyTeamLeaveReq req)
        {
            KFCopyTeamLeaveRsp rsp = new KFCopyTeamLeaveRsp();
            try
            {
                lock (Mutex)
                {
                    CopyTeamData td = null;
                    if (!CopyTeamDict.TryGetValue(req.TeamId, out td))
                    {
                        // 房间不存在！！！ GameServer要检查这个错误码，可能出现的情况是 跨服中心重启了，那么GameServer要把这个房间清掉
                        rsp.ErrorCode = CopyTeamErrorCodes.TeamIsDestoryed;
                        return rsp;
                    }

                    if (td.StartTime > 0)
                    {
                        // 已经开始了，别走
                        //rsp.ErrorCode = (int)CopyTeamErrorCodes.TeamAlreadyStart;
                        //return rsp;
                    }

                    CopyTeamMemberData member = td.TeamRoles.Find(_role => _role.RoleID == req.RoleId);
                    if (member == null)
                    {
                        rsp.ErrorCode = CopyTeamErrorCodes.NotInMyTeam;
                        return rsp;
                    }

                    RoleId2JoinedTeam.Remove(member.RoleID);
                    td.TeamRoles.Remove(member);
                    td.MemberCount = td.TeamRoles.Count;
                    if (td.MemberCount <= 0)
                    {
                        RemoveTeam(td.TeamID);
                    }
                    else
                    {
                        if (td.LeaderRoleID == member.RoleID)
                        {
                            // 移交队长
                            td.LeaderRoleID = td.TeamRoles[0].RoleID;
                            td.TeamRoles[0].IsReady = true;
                            td.TeamName = td.TeamRoles[0].RoleName;
                        }
                    }

                    // 异步广播离开队伍的事件
                    CopyTeamLeaveData data = new CopyTeamLeaveData();
                    data.TeamId = req.TeamId;
                    data.RoleId = req.RoleId;
                    AsyncDataItem evItem = new AsyncDataItem();
                    evItem.EventType = KuaFuEventTypes.KFCopyTeamLeave;
                    evItem.Args = new object[2] { req.ReqServerId, data };
                    AddAsyncEvent(evItem);

                    rsp.ErrorCode = CopyTeamErrorCodes.Success;
                    rsp.Data = data;
                }
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("离开跨服副本队伍异常, role={0}, teamid={1}", req.RoleId, req.TeamId), ex);
                rsp.ErrorCode = CopyTeamErrorCodes.CenterServerFailed;
            }

            return rsp;
        }

        /// <summary>
        /// 更新准备状态
        /// </summary>
        public KFCopyTeamSetReadyRsp TeamSetReady(KFCopyTeamSetReadyReq req)
        {
            KFCopyTeamSetReadyRsp rsp = new KFCopyTeamSetReadyRsp();
            try
            {
                lock (Mutex)
                {
                    CopyTeamData td = null;
                    if (!CopyTeamDict.TryGetValue(req.TeamId, out td))
                    {
                        // 房间不存在！！！ GameServer要检查这个错误码，可能出现的情况是 跨服中心重启了，那么GameServer要把这个房间清掉
                        rsp.ErrorCode = CopyTeamErrorCodes.TeamIsDestoryed;
                        return rsp;
                    }

                    CopyTeamMemberData member = td.TeamRoles.Find(_role => _role.RoleID == req.RoleId);
                    if (member == null)
                    {
                        rsp.ErrorCode = CopyTeamErrorCodes.NotInMyTeam;
                        return rsp;
                    }

                    if (td.StartTime > 0)
                    {
                        rsp.ErrorCode = CopyTeamErrorCodes.TeamAlreadyStart;
                        return rsp;
                    }

                    member.IsReady = req.Ready > 0;

                    // 异步广播准备事件
                    CopyTeamReadyData data = new CopyTeamReadyData();
                    data.RoleId = req.RoleId;
                    data.TeamId = req.TeamId;
                    data.Ready = req.Ready;
                    AsyncDataItem evItem = new AsyncDataItem();
                    evItem.EventType = KuaFuEventTypes.KFCopyTeamSetReady;
                    evItem.Args = new object[2] { member.ServerId, data };
                    AddAsyncEvent(evItem);

                    rsp.ErrorCode = CopyTeamErrorCodes.Success;
                    rsp.Data = data;
                }
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("更新跨服副本队伍准备状态异常, role={0}, teamid={1}", req.RoleId, req.TeamId), ex);
                rsp.ErrorCode = CopyTeamErrorCodes.CenterServerFailed;
            }

            return rsp;
        }

        /// <summary>
        /// 开始游戏
        /// </summary>
        public KFCopyTeamStartRsp StartGame(KFCopyTeamStartReq req)
        {
            KFCopyTeamStartRsp rsp = new KFCopyTeamStartRsp();
            try
            {
                lock (Mutex)
                {
                    CopyTeamData td = null;
                    if (!CopyTeamDict.TryGetValue(req.TeamId, out td))
                    {
                        rsp.ErrorCode = CopyTeamErrorCodes.TeamIsDestoryed;
                        return rsp;
                    }

                    if (td.StartTime > 0)
                    {
                        rsp.ErrorCode = CopyTeamErrorCodes.TeamAlreadyStart;
                        return rsp;
                    }

                    if (td.LeaderRoleID != req.RoleId)
                    {
                        rsp.ErrorCode = CopyTeamErrorCodes.NotTeamLeader;
                        return rsp;
                    }

                    CopyTeamMemberData leader = td.TeamRoles.Find(_role => _role.RoleID == req.RoleId);
                    if (leader == null || leader.RoleID != req.RoleId)
                    {
                        rsp.ErrorCode = CopyTeamErrorCodes.NotTeamLeader;
                        return rsp;
                    }

                    if (td.TeamRoles.Exists(_role => _role.IsReady == false))
                    {
                        rsp.ErrorCode = CopyTeamErrorCodes.MemeberNotReady;
                        return rsp;
                    }

                    int kfSrvId;
                    if (!ClientAgentManager.Instance().AssginKfFuben(GameTypes.KuaFuCopy, td.TeamID, td.TeamRoles.Count, out kfSrvId))
                    {
                        rsp.ErrorCode = CopyTeamErrorCodes.KFServerIsBusy;
                        return rsp;
                    }

                    td.StartTime = TimeUtil.NOW();
                    td.KFServerId = kfSrvId;
                    td.FuBenSeqID = 0; // not set

                    CopyTeamStartData data = new CopyTeamStartData();
                    data.TeamId = req.TeamId;
                    data.StartMs = td.StartTime;
                    data.ToServerId = kfSrvId;
                    data.FuBenSeqId = td.FuBenSeqID;
                    AsyncDataItem evItem = new AsyncDataItem();
                    evItem.EventType = KuaFuEventTypes.KFCopyTeamStart;
                    evItem.Args = new object[2] { leader.ServerId, data };
                    AddAsyncEvent(evItem);

                    // 副本超时强制关闭的时间  副本开始(客户端点击开始) + 副本持续时间 + 3分钟额外时间
                    TimeLimitCopy[td.TeamID] = td.StartTime + req.LastMs + 3 * 60 * 1000;
                    rsp.ErrorCode = CopyTeamErrorCodes.Success;
                    rsp.Data = data;
                }
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("开始跨服副本队伍异常, role={0}, teamid={1}", req.RoleId, req.TeamId), ex);
                rsp.ErrorCode = CopyTeamErrorCodes.CenterServerFailed;
            }

            return rsp;
        }

        private void ForceLeaveRoom(int roleId)
        {
            lock (Mutex)
            {
                long joinedTeam = -1;
                if (!RoleId2JoinedTeam.TryGetValue(roleId, out joinedTeam))
                {
                    return;
                }
                KFCopyTeamLeaveReq req = new KFCopyTeamLeaveReq();
                req.ReqServerId = int.MaxValue; //!!! 魔数，中心踢出认为serverid无限大
                req.RoleId = roleId;
                req.TeamId = joinedTeam;

                // 强行离开
                LeaveTeam(req);
                RoleId2JoinedTeam.Remove(roleId);
            }
        }

        #region 房间事件
        /// <summary>
        /// 投递一个房间事件 创建、解散、开始游戏，进人，踢人等等
        /// NOTE!!! This is Import!!!
        /// 投递房间事件的时候，要lock住KFCopyRoom，保证对同一个房间的修改和投递是个原子操作
        /// </summary>
        private void AddAsyncEvent(AsyncDataItem evItem)
        {
            if (evItem == null) return;
            lock (RoomEventQ)
            {
                RoomEventQ.Enqueue(evItem);
            }
        }

        /// <summary>
        /// 获取房间事件 KuaFuCopyService定时获取，然后投递到所有的GameServerAgent
        /// </summary>
        public AsyncDataItem[] PopAsyncEvent()
        {
            AsyncDataItem[] itemArray = null;
            lock (RoomEventQ)
            {
                itemArray = RoomEventQ.ToArray();
                RoomEventQ.Clear();
            }
            return itemArray;
        }
        #endregion

        /// <summary>
        /// 副本房间更新，检测结束？
        /// </summary>
        public void Update()
        {
            long nowMs = TimeUtil.NOW();

            lock (Mutex)
            {
                // 检测所有超时还没有关闭的副本 30S检测一次
                const int OVER_TIME_COPY_CHECK_INTERVAL = 30 * 1000;
                if (nowMs >= TimeLimitCopyLastCheckMs + OVER_TIME_COPY_CHECK_INTERVAL)
                {
                    TimeLimitCopyLastCheckMs = nowMs;

                    List<KeyValuePair<long, long>> overTimeList = TimeLimitCopy.ToList();
                    overTimeList.Sort((_left, _right) =>
                    {
                        if (_left.Value - _right.Value < 0) return -1;
                        else if (_left.Value - _right.Value > 0) return 1;
                        else return 0;
                    });

                    for (int i = 0; i < overTimeList.Count; ++i)
                    {
                        long teamId = overTimeList[i].Key;
                        long deadlineMs = overTimeList[i].Value;

                        if (deadlineMs > nowMs) break;

                        RemoveTeam(teamId);
                        TimeLimitCopy.Remove(teamId);
                    }
                }
            }
        }

        public void RemoveTeam(long teamId)
        {
            lock (Mutex)
            {
                CopyTeamData td = null;
                if (!CopyTeamDict.TryGetValue(teamId, out td))
                {
                    return;
                }

                CopyTeamDict.Remove(teamId);
                HashSet<long> teamList = null;
                if (CopyId2Teams.TryGetValue(td.FuBenId, out teamList))
                {
                    teamList.Remove(teamId);
                }

                TimeLimitCopy.Remove(td.TeamID);

                if (td.KFServerId > 0)
                {
                    _KFCopyService.RemoveGameTeam(td.KFServerId, td.TeamID);
                }

                foreach (var role in td.TeamRoles)
                {
                    RoleId2JoinedTeam.Remove(role.RoleID);
                }

                // 异步广播队伍摧毁的事件
                CopyTeamDestroyData data = new CopyTeamDestroyData();
                data.TeamId = teamId;
                AsyncDataItem evItem = new AsyncDataItem();
                evItem.EventType = KuaFuEventTypes.KFCopyTeamDestroty;
                evItem.Args = new object[1] { data };
                AddAsyncEvent(evItem);
            }
        }

        public CopyTeamData GetTeamData(long teamid)
        {
            lock (Mutex)
            {
                CopyTeamData td = null;
                if (!CopyTeamDict.TryGetValue(teamid, out td))
                {
                    return null;
                }

                return td;
           }
        }

        public KFCopyTeamAnalysis BuildAnalysisData()
        {
            KFCopyTeamAnalysis data = new KFCopyTeamAnalysis();
            lock (Mutex)
            {
                foreach (var kvp in CopyTeamDict)
                {
                    long teamId = kvp.Key;
                    CopyTeamData td = kvp.Value;

                    KFCopyTeamAnalysis.Item item = null;
                    if (!data.AnalysisDict.TryGetValue(td.FuBenId, out item))
                    {
                        item = new KFCopyTeamAnalysis.Item();
                        data.AnalysisDict[td.FuBenId] = item;
                    }

                    item.TotalCopyCount += 1;
                    item.TotalRoleCount += td.TeamRoles.Count;
                    if (td.StartTime > 0)
                    {
                        item.StartCopyCount += 1;
                        item.StartRoleCount += td.TeamRoles.Count;
                    }
                    else
                    {
                        item.UnStartCopyCount += 1;
                        item.UnStartRoleCount += td.TeamRoles.Count;
                    }
                }
            }

            return data;
        }
    }
}
