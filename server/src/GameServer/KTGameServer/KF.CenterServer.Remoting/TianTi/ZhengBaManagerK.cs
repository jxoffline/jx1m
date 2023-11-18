using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.Diagnostics;

using KF.Contract.Interface;
using KF.Contract.Data;
using Tmsk.Contract;
using GameServer.Core.Executor;
using Server.Tools;
using KF.Remoting.Data;

namespace KF.Remoting
{
    class ZhengBaManagerK : IZhengBaService
    {
        class JoinRolePkData
        {
            public int RoleID; // 我
            public int ZoneId;
            public string RoleName;
            public int Group;

            public int ToServerID;
            public int CurrGameID;
            public bool WaitReqEnter;
            public bool WaitKuaFuLogin;

            public int WinTimes;
        }

        #region singleton
        private ZhengBaManagerK() 
        {
            StateMachine.Install(new ZhengBaStateMachine.StateHandler(ZhengBaStateMachine.StateType.Idle, null, MS_Idle_Update, null));
            StateMachine.Install(new ZhengBaStateMachine.StateHandler(ZhengBaStateMachine.StateType.TodayPkStart, MS_TodayPkStart_Enter, null, null));
            StateMachine.Install(new ZhengBaStateMachine.StateHandler(ZhengBaStateMachine.StateType.InitPkLoop, MS_InitPkLoop_Enter, null, null));
            StateMachine.Install(new ZhengBaStateMachine.StateHandler(ZhengBaStateMachine.StateType.NotifyEnter, null, MS_NotifyEnter_Update, null));
            StateMachine.Install(new ZhengBaStateMachine.StateHandler(ZhengBaStateMachine.StateType.PkLoopStart, null, MS_PkLoopStart_Update, null));
            StateMachine.Install(new ZhengBaStateMachine.StateHandler(ZhengBaStateMachine.StateType.PkLoopEnd, null, MS_PkLoopEnd_Update, null));
            StateMachine.Install(new ZhengBaStateMachine.StateHandler(ZhengBaStateMachine.StateType.TodayPkEnd, MS_TodayPkEnd_Enter, null, null));

            StateMachine.SetCurrState(ZhengBaStateMachine.StateType.Idle, TimeUtil.NowDateTime());

            Persistence.MonthRankFirstCreate = (selectRoleIfNewCreate) =>
            {
                // 本月参赛的排行榜第一次构建出来，那么通知各个GameServer全服播放广播，以及通知所有人可以参赛
                lock (Mutex)
                {
                    AsyncEvQ.Enqueue(new AsyncDataItem(KuaFuEventTypes.ZhengBaButtetinJoin, 
                        new ZhengBaBulletinJoinData() { NtfType = ZhengBaBulletinJoinData.ENtfType.BulletinServer, Args1 = selectRoleIfNewCreate }));
                    AsyncEvQ.Enqueue(new AsyncDataItem(KuaFuEventTypes.ZhengBaButtetinJoin, 
                        new ZhengBaBulletinJoinData() { NtfType = ZhengBaBulletinJoinData.ENtfType.MailJoinRole }));
                }
            };
        }
        private static ZhengBaManagerK _instance = new ZhengBaManagerK();
        public static ZhengBaManagerK Instance() { return _instance; }
        #endregion

        #region Runtime Data
        private ZhengBaSyncData SyncData = new ZhengBaSyncData() { Month = ZhengBaUtils.MakeMonth(TimeUtil.NowDateTime()) };
        private DateTime lastUpdateTime = TimeUtil.NowDateTime();

        private Queue<AsyncDataItem> AsyncEvQ = new Queue<AsyncDataItem>();
        private int HadUpGradeRoleNum = 0;
        private List<int> RandomGroup = new List<int>();
        private ZhengBaConfig _Config = new ZhengBaConfig();
        
        private List<JoinRolePkData> TodayJoinRoleDatas = new List<JoinRolePkData>();
        private Dictionary<int, ZhengBaPkLogData> ThisLoopPkLogs = new Dictionary<int, ZhengBaPkLogData>();
        private int CurrLoopIndex = 0;

        private ZhengBaStateMachine StateMachine = new ZhengBaStateMachine();
        
        private object Mutex = new object();
        #endregion

        #region db proxy
        private ZhengBaPersistence Persistence = ZhengBaPersistence.Instance;
        #endregion

        #region Update Loop
        public AsyncDataItem[] Update()
        {
            DateTime now = TimeUtil.NowDateTime();

            if (now.Month != lastUpdateTime.Month) 
            { ReloadSyncData(now); } // 跨月的时候，由天梯排序的时候触发reload
            else if (now.Day != lastUpdateTime.Day)
            { FixSyncData(now); }
            else
            {
                lock (Mutex)
                {
                    StateMachine.Tick(now);
                }
            }

            AsyncDataItem[] asyncEvArray = null;
            lock (Mutex)
            {
                asyncEvArray = AsyncEvQ.ToArray();
                AsyncEvQ.Clear();
            }

            lastUpdateTime = now;
            return asyncEvArray;
        }

        /// <summary>
        /// Idle状态机 --- update，检测时间切入 TodayPkStart 或 TodayPkEnd
        /// </summary>
        /// <param name="now"></param>
        private void MS_Idle_Update(DateTime now)
        {
            if (SyncData.RealActDay >= 1 && SyncData.RealActDay <= ZhengBaConsts.ContinueDays)
            {
                ZhengBaMatchConfig matchConfig = _Config.MatchConfigList.Find(_m => _m.Day == SyncData.RealActDay);
                if (lastUpdateTime.TimeOfDay.Ticks < matchConfig.DayBeginTick && now.TimeOfDay.Ticks >= matchConfig.DayBeginTick)
                    StateMachine.SetCurrState(ZhengBaStateMachine.StateType.TodayPkStart, now);
                else if (lastUpdateTime.TimeOfDay.Ticks < matchConfig.DayEndTick && now.TimeOfDay.Ticks >= matchConfig.DayEndTick)
                    StateMachine.SetCurrState(ZhengBaStateMachine.StateType.TodayPkEnd, now);
            }
        }

        /// <summary>
        /// TodayPkStart状态机 --- enter，初始化今日参赛信息，切入 InitPkLoop
        /// </summary>
        /// <param name="now"></param>
        private void MS_TodayPkStart_Enter(DateTime now)
        {
            SyncData.TodayIsPking = true;
            TodayJoinRoleDatas.Clear();
            CurrLoopIndex = 0;
            HadUpGradeRoleNum = 0;

            foreach (var role in SyncData.RoleList)
            {
                if (role.State == (int)EZhengBaState.None || role.State == (int)EZhengBaState.UpGrade)
                {
                    TodayJoinRoleDatas.Add(new JoinRolePkData()
                    {
                        RoleID = role.RoleId,
                        ZoneId = role.ZoneId,
                        RoleName = role.RoleName,
                        Group = role.Group,
                    });
                }
            }

            if (SyncData.RealActDay == 3) // 第3天pk开始，生成随机Group列表
            {
                RandomGroup.Clear();
                RandomGroup.AddRange(Enumerable.Range(1, ZhengBaConsts.MaxGroupNum));

                Random r = new Random((int)now.Ticks);
                // 如果人数不足16人，那么这些人必定会晋级到16强，防止人数少于16人的时候，真的随机分组的话，会导致一半的玩家pk不到对手
                // 例如，只有4人，结果分到的编号是1,3,5,7，那么16--->8的时候就没法匹配
                // 其实这里就是为了测试，外网肯定>16人
                for (int i = 0; TodayJoinRoleDatas.Count >= ZhengBaConsts.MaxGroupNum && i < 50; i++)
                {
                    int idx1 = r.Next(0, RandomGroup.Count), idx2 = r.Next(0, RandomGroup.Count);

                    int tmp = RandomGroup[idx1];
                    RandomGroup[idx1] = RandomGroup[idx2];
                    RandomGroup[idx2] = tmp;
                }
            }
            
            StateMachine.SetCurrState(ZhengBaStateMachine.StateType.InitPkLoop, now);
        }

        /// <summary>
        /// InitPkLoop 状态机 --- enter，初始化本轮pk信息，分组，切入 NotifyEnter
        /// </summary>
        /// <param name="now"></param>
        private void MS_InitPkLoop_Enter(DateTime now)
        {
            ThisLoopPkLogs.Clear();
            CurrLoopIndex++;

            ZhengBaMatchConfig matchConfig = _Config.MatchConfigList.Find(_m => _m.Day == SyncData.RealActDay);
            if (this.HadUpGradeRoleNum >= (int)matchConfig.MaxUpGradeNum || TodayJoinRoleDatas.Count <= 1)
            {
                StateMachine.SetCurrState(ZhengBaStateMachine.StateType.TodayPkEnd, now);
                return;
            }

            if (matchConfig.Mathching == EZhengBaMatching.Random) // 随机分组
            {
                Random r = new Random((int)now.Ticks);
                for (int i = 0; TodayJoinRoleDatas.Count > 0 && i < TodayJoinRoleDatas.Count * 2; i++)
                {
                    int idx1 = r.Next(0, TodayJoinRoleDatas.Count), idx2 = r.Next(0, TodayJoinRoleDatas.Count);

                    var tmp = TodayJoinRoleDatas[idx1];
                    TodayJoinRoleDatas[idx1] = TodayJoinRoleDatas[idx2];
                    TodayJoinRoleDatas[idx2] = tmp;
                }
            }
            else if (matchConfig.Mathching == EZhengBaMatching.Group)
            {
                List<JoinRolePkData> tmpRoleDatas = new List<JoinRolePkData>();
                foreach (var range in ZhengBaUtils.GetDayPkGroupRange(SyncData.RealActDay))
                {
                    var groupRoles = TodayJoinRoleDatas.FindAll(_r => _r.Group >= range.Left && _r.Group <= range.Right);
                    if (groupRoles != null && groupRoles.Count == 2)
                    {
                        tmpRoleDatas.AddRange(groupRoles);
                    }
                }

                TodayJoinRoleDatas.Clear();
                TodayJoinRoleDatas.AddRange(tmpRoleDatas);
            }
            else { Debug.Assert(false, "unknown pk match type"); }

            // 两两分组
            int currIdx = 0;
            for (int i = 0; i < TodayJoinRoleDatas.Count / 2; i++)
            {
                var joinRole1 = TodayJoinRoleDatas[currIdx++];
                var joinRole2 = TodayJoinRoleDatas[currIdx++];

                int toServerId = 0, gameId = 0;
                gameId = TianTiPersistence.Instance.GetNextGameId();
                if (ClientAgentManager.Instance().AssginKfFuben(TianTiService.Instance.GameType, gameId, 2, out toServerId))
                {
                    joinRole1.ToServerID = joinRole2.ToServerID = toServerId;
                    joinRole1.CurrGameID = joinRole2.CurrGameID = gameId;
                    joinRole1.WaitReqEnter = joinRole2.WaitReqEnter = true;
                    joinRole1.WaitKuaFuLogin = joinRole2.WaitKuaFuLogin = false;

                    ZhengBaNtfEnterData data = new ZhengBaNtfEnterData();
                    data.RoleId1 = joinRole1.RoleID;
                    data.RoleId2 = joinRole2.RoleID;
                    data.ToServerId = toServerId;
                    data.GameId = gameId;
                    data.Day = SyncData.RealActDay;
                    data.Loop = CurrLoopIndex;
                    AsyncEvQ.Enqueue(new AsyncDataItem(KuaFuEventTypes.ZhengBaNtfEnter, data));

                    ZhengBaPkLogData log = new ZhengBaPkLogData();
                    log.Month = SyncData.Month;
                    log.Day = SyncData.RealActDay;
                    log.RoleID1 = joinRole1.RoleID;
                    log.ZoneID1 = joinRole1.ZoneId;
                    log.RoleName1 = joinRole1.RoleName;
                    log.RoleID2 = joinRole2.RoleID;
                    log.ZoneID2 = joinRole2.ZoneId;
                    log.RoleName2 = joinRole2.RoleName;
                    log.StartTime = now;

                    ThisLoopPkLogs[gameId] = log;
                }
                else
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("众神争霸第{0}天第{1}轮分配游戏服务器失败，role1={2},role2={3}", SyncData.RealActDay,CurrLoopIndex, joinRole1.RoleID, joinRole2.RoleID));
                }
            }

            // 匹配不到对手！！！，人数为奇数了
            while (currIdx < TodayJoinRoleDatas.Count)
            {
                var joinRole = TodayJoinRoleDatas[currIdx++];
                joinRole.ToServerID = 0;
                joinRole.CurrGameID = 0;
                joinRole.WaitReqEnter = false;
                joinRole.WaitKuaFuLogin = false;
            }
            StateMachine.SetCurrState(ZhengBaStateMachine.StateType.NotifyEnter, now);
        }

        /// <summary>
        /// NotifyEnter状态机 --- update，超时检测未进入的玩家，中心发起镜像出战, 切入PkLoopStart
        /// </summary>
        /// <param name="now"></param>
        private void MS_NotifyEnter_Update(DateTime now)
        {
            ZhengBaMatchConfig matchConfig = _Config.MatchConfigList.Find(_m => _m.Day == SyncData.RealActDay);
            if (StateMachine.ContinueTicks(now) < matchConfig.WaitSeconds * TimeSpan.TicksPerSecond) return;

            // 等待进入超时后，对位未进入的玩家(例如不在线，或者未作出任何点击的在线玩家)，中心发起镜像出战的通知
            for (int i = 0, currIdx = 0; i < TodayJoinRoleDatas.Count / 2; i++)
            {
                var joinRole1 = TodayJoinRoleDatas[currIdx++];
                if (joinRole1.WaitReqEnter) // 等待客户端点击进入或者镜像出战
                {
                    joinRole1.WaitReqEnter = false;
                    joinRole1.WaitKuaFuLogin = false;

                    ZhengBaPkLogData log = null;
                    if (ThisLoopPkLogs.TryGetValue(joinRole1.CurrGameID, out log))
                    {
                        log.IsMirror1 = true;
                        AsyncEvQ.Enqueue(new AsyncDataItem(KuaFuEventTypes.ZhengBaMirrorFight, new ZhengBaMirrorFightData()
                        {
                            GameId = joinRole1.CurrGameID,
                            RoleId = joinRole1.RoleID,
                            ToServerId = joinRole1.ToServerID
                        }));
                    }
                }

                var joinRole2 = TodayJoinRoleDatas[currIdx++];
                if (joinRole2.WaitReqEnter) // 等待客户端点击进入或者镜像出战
                {
                    joinRole2.WaitReqEnter = false;
                    joinRole2.WaitKuaFuLogin = false;

                    ZhengBaPkLogData log = null;
                    if (ThisLoopPkLogs.TryGetValue(joinRole2.CurrGameID, out log))
                    {
                        log.IsMirror2 = true;
                        AsyncEvQ.Enqueue(new AsyncDataItem(KuaFuEventTypes.ZhengBaMirrorFight, new ZhengBaMirrorFightData()
                        {
                            GameId = joinRole2.CurrGameID,
                            RoleId = joinRole2.RoleID,
                            ToServerId = joinRole2.ToServerID
                        }));
                    }
                }
            }

            StateMachine.SetCurrState(ZhengBaStateMachine.StateType.PkLoopStart, now);
        }

        /// <summary>
        /// PkLoopStart 状态机 --- update，检测战斗超时，切入PkLoopEnd
        /// </summary>
        /// <param name="now"></param>
        private void MS_PkLoopStart_Update(DateTime now)
        {
            ZhengBaMatchConfig matchConfig = _Config.MatchConfigList.Find(_m => _m.Day == SyncData.RealActDay);
            if (StateMachine.ContinueTicks(now) < (matchConfig.FightSeconds + matchConfig.ClearSeconds) * TimeSpan.TicksPerSecond)
                return;
            StateMachine.SetCurrState(ZhengBaStateMachine.StateType.PkLoopEnd, now);
        }

        /// <summary>
        /// PkLoopEnd 状态机 ---update，根据时间检测切入TodayPkEnd还是InitPkLoop
        /// </summary>
        /// <param name="now"></param>
        private void MS_PkLoopEnd_Update(DateTime now)
        {
            ZhengBaMatchConfig matchConfig = _Config.MatchConfigList.Find(_m => _m.Day == SyncData.RealActDay);
            if (StateMachine.ContinueTicks(now) < matchConfig.IntervalSeconds * TimeSpan.TicksPerSecond)
                return;

            // 清空超时未上报结果的战斗
            foreach (var kvp in ThisLoopPkLogs)
            {
                kvp.Value.PkResult = (int)EZhengBaPKResult.Invalid;
                kvp.Value.UpGrade = false;
                kvp.Value.EndTime = now;

                Persistence.SavePkLog(kvp.Value);
                AsyncEvQ.Enqueue(new AsyncDataItem(KuaFuEventTypes.ZhengBaPkLog, kvp.Value));
            }

            ThisLoopPkLogs.Clear();
            foreach (var role in TodayJoinRoleDatas)
            {
                if (role.CurrGameID > 0 || role.ToServerID > 0)
                {
                    ClientAgentManager.Instance().RemoveKfFuben(TianTiService.Instance.GameType, role.ToServerID, role.CurrGameID);
                }
                role.ToServerID = 0;
                role.CurrGameID = 0;
            }

            if (now.TimeOfDay.Ticks >= matchConfig.DayEndTick)
                StateMachine.SetCurrState(ZhengBaStateMachine.StateType.TodayPkEnd, now);
            else
                StateMachine.SetCurrState(ZhengBaStateMachine.StateType.InitPkLoop, now);
        }

        /// <summary>
        /// TodayPkEnd 状态机 ---enter，修复晋级信息，切入Idle状态
        /// </summary>
        /// <param name="now"></param>
        private void MS_TodayPkEnd_Enter(DateTime now)
        {
            SyncData.TodayIsPking = false;
            FixSyncData(now);
            StateMachine.SetCurrState(ZhengBaStateMachine.StateType.Idle, now);

            AsyncEvQ.Enqueue(new AsyncDataItem(KuaFuEventTypes.ZhengBaButtetinJoin,
                new ZhengBaBulletinJoinData() {  NtfType = ZhengBaBulletinJoinData.ENtfType.DayLoopEnd, Args1 = SyncData.RealActDay}
                ));
        }

        #endregion

        #region Init
        public void InitConfig()
        {
            bool bOk = _Config.Load(
                  KuaFuServerManager.GetResourcePath(ZhengBaConsts.MatchConfigFile, KuaFuServerManager.ResourcePathTypes.GameRes),
                  KuaFuServerManager.GetResourcePath(ZhengBaConsts.SupportConfigFile, KuaFuServerManager.ResourcePathTypes.GameRes),
                  KuaFuServerManager.GetResourcePath(ZhengBaConsts.BirthPointConfigFile, KuaFuServerManager.ResourcePathTypes.GameRes)
                  );

            if (!bOk)
            {
                LogManager.WriteLog(LogTypes.Error, "ZhengBaManagerK.InitConfig failed!");
            }
        }

        public void ReloadSyncData(DateTime now)
        {
            int selectRoleIfNewCreate = ZhengBaConsts.DefaultJoinRoleNum;
            long dayBeginTicks = 0;
            ZhengBaMatchConfig matchConfig = _Config.MatchConfigList.Find(_m => _m.Day == 1);
            if (matchConfig.MinRank > 0) 
                selectRoleIfNewCreate = matchConfig.MinRank;
            dayBeginTicks = matchConfig.DayBeginTick;

            ZhengBaSyncData syncData = Persistence.LoadZhengBaSyncData(now, selectRoleIfNewCreate, dayBeginTicks);
            lock (Mutex)
            {
                SyncData = syncData;
                FixSyncData(now);
            }
        }

        /// <summary>
        /// 修复日期
        /// </summary>
        /// <param name="now"></param>
        /// <returns></returns>
        private bool FixSyncData_State(DateTime now)
        {
            bool bForceModify = false;
            int nowDay = now.Day - ZhengBaConsts.StartMonthDay + 1;

            lock (Mutex)
            {
                // 检测已有的排行数据应该是活动的第几天的战斗结果
                int rankOfDay = ZhengBaConsts.ContinueDays;
                for (; rankOfDay >= 1; rankOfDay--)
                {
                    EZhengBaGrade willUpGrade = ZhengBaUtils.GetDayUpGrade(rankOfDay);
                    ZhengBaMatchConfig matchConfig = _Config.MatchConfigList.Find(_m => _m.Day == rankOfDay);
                    List<ZhengBaRoleInfoData> roleList = SyncData.RoleList.FindAll(_r => _r.Grade == (int)willUpGrade);
                    if (roleList.Count <= 0) continue;
     
                    int needUpNum = matchConfig.MaxUpGradeNum - roleList.Count;
                    if (needUpNum > 0)
                    {
                        List<ZhengBaRoleInfoData> upGradeList = new List<ZhengBaRoleInfoData>();
                        if (rankOfDay <= 3)
                        {
                            //补位选手 --- 被淘汰的角色按照晋级、段位进行排序
                            List<ZhengBaRoleInfoData> luckList = SyncData.RoleList.FindAll(_r => _r.Grade > (int)willUpGrade);
                            luckList.Sort((_l, _r) =>
                            {
                                if (_l.Grade < _r.Grade) return -1;
                                else if (_l.Grade > _r.Grade) return 1;
                                else return _l.DuanWeiRank - _r.DuanWeiRank;
                            });

                            foreach (var luckRole in luckList.GetRange(0, Math.Min(needUpNum, luckList.Count)))
                            {
                                upGradeList.Add(luckRole);
                                LogManager.WriteLog(LogTypes.Error,  string.Format("晋级补位 [s{0}.{1}] {2}->{3}", luckRole.ZoneId, luckRole.RoleId, luckRole.Grade, (int)willUpGrade));
                                luckRole.Grade = (int)willUpGrade;
                                bForceModify = true;
                            }
                        }
                        else
                        {
                            // 第4天开始就是固定分组pk了，要防止同一个组的两个人都补位晋级的情况
                            foreach (var range in ZhengBaUtils.GetDayPkGroupRange(rankOfDay))
                            {
                                // 找出这个group中，在rankOfDay-1天晋级的玩家列表，尝试选取一个晋级到rankOfDay中
                                var groupRoleList = SyncData.RoleList.FindAll(_r => _r.Group >= range.Left && _r.Group <= range.Right);
                                if (groupRoleList.Exists(_r => _r.Grade <= (int)ZhengBaUtils.GetDayUpGrade(rankOfDay)))
                                    continue; // 本分组已有人在本天已晋级

                                // 只从rankOfDay-1天晋级的玩家中挑选
                                groupRoleList.RemoveAll(_r => _r.Grade != (int)ZhengBaUtils.GetDayUpGrade(rankOfDay - 1));
                                if (groupRoleList.Count <= 0) continue;

                                // 段位高者晋级
                                groupRoleList.Sort((_l, _r) => { return _l.DuanWeiRank - _r.DuanWeiRank; });
                                ZhengBaRoleInfoData selectRole = groupRoleList[0];
                                LogManager.WriteLog(LogTypes.Error, string.Format("晋级补位 [s{0}.{1}] {2}->{3}", selectRole.ZoneId, selectRole.RoleId, selectRole.Grade, (int)willUpGrade));
                                selectRole.Grade = (int)ZhengBaUtils.GetDayUpGrade(rankOfDay);
                                bForceModify = true;
                                upGradeList.Add(selectRole);
                            }
                        }

                        foreach (var luckRole in upGradeList)
                        {
                            AsyncEvQ.Enqueue(new AsyncDataItem(KuaFuEventTypes.ZhengBaButtetinJoin, new ZhengBaBulletinJoinData()
                            {
                                NtfType = ZhengBaBulletinJoinData.ENtfType.MailUpgradeRole,
                                Args1 = luckRole.RoleId,
                            }));
                        }
                    }

                    break;
                }

                // rankOfDay 表示当前是第几天结束时的结果
                SyncData.RankResultOfDay = rankOfDay;
                SyncData.RealActDay = rankOfDay;
                foreach (var role in SyncData.RoleList)
                {
                    if (rankOfDay <= 0)
                    {
                        if (role.Grade != (int)EZhengBaGrade.Grade100 
                            || role.State != (int)EZhengBaState.None
                            || role.Group != (int)ZhengBaConsts.NoneGroup)
                        {
                            role.Grade = (int)EZhengBaGrade.Grade100;
                            role.State = (int)EZhengBaState.None;
                            role.Group = (int)ZhengBaConsts.NoneGroup;
                            bForceModify = true;
                        }
                    }
                    else
                    {
                        EZhengBaGrade upGrade = _Config.MatchConfigList.Find(_m => _m.Day == rankOfDay).WillUpGrade;
                        if (role.Grade <= (int)upGrade && role.State != (int)EZhengBaState.UpGrade)
                        {
                            role.State = (int)EZhengBaState.UpGrade;
                            bForceModify = true;
                        }

                        if (role.Grade > (int)upGrade && role.State != (int)EZhengBaState.Failed)
                        {
                            role.State = (int)EZhengBaState.Failed;
                            bForceModify = true;
                        }
                    }
                }
     
                // 检测是否推进到后一天，进入活动开始日期 , 并且当前的实际活动天数 小于 现实天数
                if (nowDay > 0 && SyncData.RealActDay < nowDay)
                {
                    // 不是活动的最后一天, 才能够尝试推进到下一天
                    if (SyncData.RealActDay < ZhengBaConsts.ContinueDays)
                    {
                        ZhengBaMatchConfig matchConfig = _Config.MatchConfigList.Find(_m => _m.Day == SyncData.RealActDay + 1);
                        if (now.TimeOfDay.Ticks < matchConfig.DayBeginTick)
                        {
                            // 当前时间小于下一天活动的开始时间，ok，推进到下一天, 这里的时间按照相对于凌晨的Ticks而言
                            SyncData.RealActDay++;
                        }
                    }
                    else
                    {
                        // 当前是第7天的数据，可以直接推进到下一天了
                        SyncData.RealActDay = ZhengBaConsts.ContinueDays + 1;
                    }
                }

            }

            return bForceModify;
        }

        private bool FixSyncData_Group(DateTime now)
        {
            bool bForceModify = false;

            lock (Mutex)
            {
                if (SyncData.RealActDay < 3)
                    return false;

                List<int> unUsedGroupList = Enumerable.Range(1, ZhengBaConsts.MaxGroupNum).ToList();

                List<ZhengBaRoleInfoData> unGroupRoleList = new List<ZhengBaRoleInfoData>();
                foreach (var role in SyncData.RoleList)
                {
                    if (role.Grade > (int)EZhengBaGrade.Grade16)
                    {
                        continue;
                    }

                    if (role.Group >= 1 && role.Group <= ZhengBaConsts.MaxGroupNum)
                    {
                        unUsedGroupList.Remove(role.Group);
                    }
                    else
                    {
                        role.Group = 0;
                        unGroupRoleList.Add(role);
                    }
                }

                if (unGroupRoleList.Count <= unUsedGroupList.Count 
                    && unGroupRoleList.Count > 0)
                {
                    // 瞎排的
                    unGroupRoleList.Sort((_l, _r) => {
                        return _l.ZoneId * _l.DuanWeiRank * now.TimeOfDay.Minutes % _r.RoleId -
                            _r.ZoneId * _r.DuanWeiRank * now.TimeOfDay.Minutes % _l.RoleId;
                    });

                    foreach (var role in unGroupRoleList)
                    {
                        role.Group = unUsedGroupList.Last();
                        unUsedGroupList.RemoveAt(unUsedGroupList.Count() - 1);
                        LogManager.WriteLog(LogTypes.Error, string.Format("晋级补分组 [s{0}.{1}] Group:{2}", role.ZoneId, role.RoleId, role.Group));
                        bForceModify = true;
                    }
                }
                else if (unGroupRoleList.Count > 0)
                {
                    LogManager.WriteLog(LogTypes.Fatal, string.Format("晋级补分组发生异常，待补人数={0}，可用分组数={1}", unGroupRoleList.Count, unUsedGroupList.Count), null, false);
                }
            }
           

            return bForceModify;
        }

        private void FixSyncData(DateTime now)
        {
            lock (Mutex)
            {
                bool bModify = false;
                bModify |= FixSyncData_State(now);
                bModify |= FixSyncData_Group(now);

                if (bModify)
                {
                    foreach (var role in SyncData.RoleList)
                    {
                        Persistence.UpdateRole(SyncData.Month, role.RoleId, role.Grade, role.State, role.Group);
                    }
                }

                // 每次修复都强制设置时间，让GameServer来同步
                SyncData.RoleModTime = now;
                SyncData.SupportModTime = now;
            }
        }

        #endregion

        #region Implement Interface `IZhengBaService`
        public ZhengBaSyncData SyncZhengBaData(ZhengBaSyncData lastSyncData)
        {
            ZhengBaSyncData result = new ZhengBaSyncData();
            lock (Mutex)
            {
                result.Month = SyncData.Month;
                result.RealActDay = SyncData.RealActDay;
                result.RoleModTime = SyncData.RoleModTime;
                result.SupportModTime = SyncData.SupportModTime;
                result.TodayIsPking = SyncData.TodayIsPking;
                result.IsThisMonthInActivity = SyncData.IsThisMonthInActivity;
                result.RankResultOfDay = SyncData.RankResultOfDay;
                result.CenterTime = TimeUtil.NowDateTime();

                if (result.RoleModTime > lastSyncData.RoleModTime && SyncData.RoleList != null)
                {
                    result.RoleList = new List<ZhengBaRoleInfoData>(SyncData.RoleList);
                }

                if (result.SupportModTime > lastSyncData.SupportModTime && SyncData.SupportList != null)
                {
                    result.SupportList = new List<ZhengBaSupportAnalysisData>(SyncData.SupportList);
                }
            }

            return result;
        }

        public int ZhengBaSupport(ZhengBaSupportLogData data)
        {
            if (data == null || !Persistence.SaveSupportLog(data))
            {
                return StdErrorCode.Error_DB_Faild;
            }

            lock (Mutex)
            {
                ZhengBaSupportAnalysisData support = null;
                if ((support = SyncData.SupportList.Find(_s => _s.UnionGroup == data.ToUnionGroup && _s.Group == data.ToGroup))== null)
                {
                    support = new ZhengBaSupportAnalysisData() { UnionGroup = data.ToUnionGroup, Group = data.ToGroup, RankOfDay = data.RankOfDay };
                    SyncData.SupportList.Add(support);
                }

                if (data.SupportType == (int)EZhengBaSupport.Support) support.TotalSupport++;
                else if (data.SupportType == (int)EZhengBaSupport.Oppose) support.TotalOppose++;
                else if (data.SupportType == (int)EZhengBaSupport.YaZhu) support.TotalYaZhu++;

                SyncData.SupportModTime = TimeUtil.NowDateTime();
                AsyncEvQ.Enqueue(new AsyncDataItem(KuaFuEventTypes.ZhengBaSupport, data));
            }

            return StdErrorCode.Error_Success;
        }

        public int ZhengBaRequestEnter(int roleId, int gameId, EZhengBaEnterType enter)
        {
            lock (Mutex)
            {
                if (StateMachine.GetCurrState() !=  ZhengBaStateMachine.StateType.NotifyEnter)
                    return StdErrorCode.Error_Not_In_valid_Time;

                JoinRolePkData roleData = TodayJoinRoleDatas.Find(_r => _r.RoleID == roleId && _r.CurrGameID == gameId);
                ZhengBaPkLogData logData = null;
                ThisLoopPkLogs.TryGetValue(gameId, out logData);
                if (roleData == null || logData == null)
                    return StdErrorCode.Error_Operation_Denied;

                if (!roleData.WaitReqEnter)
                    return StdErrorCode.Error_Operation_Denied;

                roleData.WaitReqEnter = false;
                roleData.WaitKuaFuLogin = true;
                if (enter == EZhengBaEnterType.Mirror)
                {
                    if (logData.RoleID1 == roleId) logData.IsMirror1 = true;
                    else if (logData.RoleID2 == roleId) logData.IsMirror2 = true;

                    AsyncEvQ.Enqueue(new AsyncDataItem(KuaFuEventTypes.ZhengBaMirrorFight, new ZhengBaMirrorFightData()
                    {
                        RoleId = roleId, GameId = gameId, ToServerId = roleData.ToServerID
                    }));
                }
            }

            return StdErrorCode.Error_Success_No_Info;
        }

        public int ZhengBaKuaFuLogin(int roleid, int gameId)
        {
            lock (Mutex)
            {
                JoinRolePkData roleData = TodayJoinRoleDatas.Find(_r => _r.RoleID == roleid && _r.CurrGameID == gameId);
                ZhengBaPkLogData logData = null;
                ThisLoopPkLogs.TryGetValue(gameId, out logData);
                if (roleData == null || logData == null)
                    return StdErrorCode.Error_Operation_Denied;

                if (!roleData.WaitKuaFuLogin)
                    return StdErrorCode.Error_Operation_Denied;

                // 这句话会导致CMD_LOGIN_ON 无法通过第二次验证， (原服未下线，跨服已上线，前几次CMD_LOGIN_ON会失败)
               // roleData.WaitKuaFuLogin = false;
            }

            return StdErrorCode.Error_Success_No_Info;
        }

        public List<ZhengBaNtfPkResultData> ZhengBaPkResult(int gameId, int winner1, int FirstLeaveRoleId)
        {
            lock (Mutex)
            {

                ZhengBaPkLogData log = null;
                if (!ThisLoopPkLogs.TryGetValue(gameId, out log)) return null;

                // 一个人走，另一个人赢
                if (FirstLeaveRoleId == log.RoleID1) winner1 = log.RoleID2;
                else if (FirstLeaveRoleId == log.RoleID2) winner1 = log.RoleID1;

                if (winner1 != log.RoleID1 && winner1 != log.RoleID2)
                {
                    // what's the fuck.
                    return null;
                }

                JoinRolePkData joinRole1 = TodayJoinRoleDatas.Find(_r => _r.RoleID == log.RoleID1 && _r.CurrGameID == gameId);
                JoinRolePkData joinRole2 = TodayJoinRoleDatas.Find(_r => _r.RoleID == log.RoleID2 && _r.CurrGameID == gameId);
                if (joinRole1 == null || joinRole2 == null)
                {
                    // what's the fuck
                    return null;
                }

                ZhengBaMatchConfig matchConfig = _Config.MatchConfigList.Find(_m => _m.Day == SyncData.RealActDay);
                ZhengBaNtfPkResultData ntf1 = new ZhengBaNtfPkResultData() { RoleID = joinRole1.RoleID };
                ZhengBaNtfPkResultData ntf2 = new ZhengBaNtfPkResultData() { RoleID = joinRole2.RoleID };
                JoinRolePkData winJoinRole = null;
                ZhengBaNtfPkResultData winNtf = null;

                if (winner1 > 0 && winner1 == joinRole1.RoleID)
                {
                    winJoinRole = joinRole1;
                    winNtf = ntf1;
                }
                else if (winner1 > 0 && winner1 == joinRole2.RoleID)
                {
                    winJoinRole = joinRole2;
                    winNtf = ntf2;
                }

                if (winJoinRole != null && winNtf != null)
                {
                    winNtf.IsWin = true;
                    winJoinRole.WinTimes++;
                    if (winJoinRole.WinTimes >= matchConfig.NeedWinTimes && HadUpGradeRoleNum < matchConfig.MaxUpGradeNum)
                    {
                        int calcGroup = RandomGroup.Count > 0 ? RandomGroup.Last() : ZhengBaConsts.NoneGroup;
                        bool bSaveUpdate = false;
                        ZhengBaRoleInfoData roleData = SyncData.RoleList.Find(_r => _r.RoleId == winJoinRole.RoleID);
                        if (roleData != null)
                        {
                            int newGrade = (int)matchConfig.WillUpGrade;
                            int newState = (int)EZhengBaState.UpGrade;
                            int newGroup = calcGroup != ZhengBaConsts.NoneGroup ? calcGroup : roleData.Group;

                            if (Persistence.UpdateRole(SyncData.Month, roleData.RoleId, newGrade, newState, newGroup))
                            {
                                // 必须保证把晋级情况落地到db之后，才能更新缓存
                                roleData.Grade = newGrade;
                                roleData.State = newState;
                                roleData.Group = newGroup;
                                SyncData.RoleModTime = TimeUtil.NowDateTime();
                                bSaveUpdate = true;

                                if (newGrade != (int)EZhengBaGrade.Grade1)
                                    AsyncEvQ.Enqueue(new AsyncDataItem(KuaFuEventTypes.ZhengBaButtetinJoin, new ZhengBaBulletinJoinData()
                                    {
                                        NtfType = ZhengBaBulletinJoinData.ENtfType.MailUpgradeRole,
                                        Args1 = roleData.RoleId,
                                    }));
                            }
                        }

                        if (bSaveUpdate)
                        {
                            // 晋级已落地到db，修改缓存
                            winNtf.RandGroup = calcGroup;
                            winNtf.IsUpGrade = true;
                            log.UpGrade = true;
                            HadUpGradeRoleNum++;

                            RandomGroup.Remove(calcGroup);
                            TodayJoinRoleDatas.RemoveAll(_r => _r.RoleID == winJoinRole.RoleID);
                        }
                    }
                }

                log.EndTime = TimeUtil.NowDateTime();
                if (winner1 > 0 && winner1 == log.RoleID1) log.PkResult = (int)EZhengBaPKResult.Win;
                else if (winner1 > 0 && winner1 == log.RoleID2) log.PkResult = (int)EZhengBaPKResult.Fail;
                else log.PkResult = (int)EZhengBaPKResult.Invalid;

                ntf1.StillNeedWin = Math.Max(0, matchConfig.NeedWinTimes - joinRole1.WinTimes);
                ntf1.LeftUpGradeNum = matchConfig.MaxUpGradeNum - HadUpGradeRoleNum;
                ntf2.StillNeedWin = Math.Max(0, matchConfig.NeedWinTimes - joinRole2.WinTimes);
                ntf2.LeftUpGradeNum = matchConfig.MaxUpGradeNum - HadUpGradeRoleNum;

                Persistence.SavePkLog(log);
                ThisLoopPkLogs.Remove(gameId);
                AsyncEvQ.Enqueue(new AsyncDataItem(KuaFuEventTypes.ZhengBaPkLog, log));

                return new List<ZhengBaNtfPkResultData>() { ntf1, ntf2};
            }
        }
        #endregion
    }
}
