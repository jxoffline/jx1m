using GameServer.Core.Executor;
using KF.Contract.Data;
using KF.Contract.Interface;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Tmsk.Contract;

namespace KF.Remoting
{
    internal class _CoupleArenaDuanWeiCfg
    {
        public int DuanWeiType;
        public int DuanWeiLevel;
        public int NeedJiFen;
        public int WinJiFen;
        public int LoseJiFen;
    }

    internal class _CoupleArenaWarTimePoint
    {
        public int Weekday;
        public long DayStartTicks;
        public long DayEndTicks;
    }

    /// <summary>
    /// 情侣竞技Service
    /// </summary>
    public class CoupleArenaService : ICoupleArenaService
    {
        #region Singleton

        private static CoupleArenaService _Instance = new CoupleArenaService();

        private CoupleArenaService()
        { }

        public static CoupleArenaService getInstance()
        { return _Instance; }

        #endregion Singleton

        #region Config

        private List<_CoupleArenaDuanWeiCfg> _DuanWeiCfgList = new List<_CoupleArenaDuanWeiCfg>();
        private List<_CoupleArenaWarTimePoint> _WarTimePointList = new List<_CoupleArenaWarTimePoint>();
        public readonly GameTypes GameType = GameTypes.CoupleArena;
        public readonly GameTypes EvItemGameType = GameTypes.TianTi;

        #endregion Config

        #region Runtime Data

        private object Mutex = new object();
        private DateTime LastUpdateTime = DateTime.MinValue;
        private uint UpdateFrameCount = 0;

        /// <summary>
        /// 需要同步的竞技数据
        /// </summary>
        private CoupleArenaSyncData SyncData = new CoupleArenaSyncData();

        /// <summary>
        /// [优化]是否需要重排排行榜，当pk结束，有分数变更时设为true，定期排序一次
        /// </summary>
        private bool IsNeedSort = false;

        /// <summary>
        /// [优化]排行榜是否变化了，如果变化了，定期刷新排行到数据库
        /// </summary>
        private bool IsRankChanged = false;

        /// <summary>
        /// key: game id
        /// value: fuben data
        /// </summary>
        private Dictionary<long, CoupleArenaFuBenData> GameFuBenDict = new Dictionary<long, CoupleArenaFuBenData>();

        /// <summary>
        /// 同一对夫妻匹配次数限制
        /// </summary>
        private CoupleArenaMatchTimeLimiter MatchTimeLimiter = new CoupleArenaMatchTimeLimiter();

        /// <summary>
        /// join data util
        /// </summary>
        private CoupleArenaJoinDataUtil JoinDataUtil = new CoupleArenaJoinDataUtil();

        /// <summary>
        /// 离婚记录, 记录下来，防止一方战斗，配偶在原服离婚，战斗结果通知给中心的时候，处理一下
        /// </summary>
        private CoupleArenaDivorceRecord DivorceRecord = new CoupleArenaDivorceRecord();

        #endregion Runtime Data

        #region Persistence

        private CoupleArenaPersistence Persistence = CoupleArenaPersistence.getInstance();

        #endregion Persistence

        #region StartUp

        public void StartUp()
        {
            try
            {
                XElement xml = null;
                string file = "";

                file = @"Config\CoupleWar.xml";
                xml = XElement.Load(KuaFuServerManager.GetResourcePath(file, KuaFuServerManager.ResourcePathTypes.GameRes));
                foreach (var xmlItem in xml.Elements())
                {
                    string[] fields = xmlItem.Attribute("TimePoints").Value.Split(new char[] { ',', '-', '|' });
                    for (int i = 0; i < fields.Length; i += 3)
                    {
                        var tp = new _CoupleArenaWarTimePoint();
                        tp.Weekday = Convert.ToInt32(fields[i]);
                        if (tp.Weekday < 1 || tp.Weekday > 7) throw new Exception("weekday error!");
                        tp.DayStartTicks = DateTime.Parse(fields[i + 1]).TimeOfDay.Ticks;
                        tp.DayEndTicks = DateTime.Parse(fields[i + 2]).TimeOfDay.Ticks;

                        _WarTimePointList.Add(tp);
                    }

                    _WarTimePointList.Sort((_l, _r) => { return _l.Weekday - _r.Weekday; });

                    break;
                }

                file = @"Config\CoupleDuanWei.xml";
                xml = XElement.Load(KuaFuServerManager.GetResourcePath(file, KuaFuServerManager.ResourcePathTypes.GameRes));
                foreach (var xmlItem in xml.Elements())
                {
                    var cfg = new _CoupleArenaDuanWeiCfg();
                    cfg.NeedJiFen = Convert.ToInt32(xmlItem.Attribute("NeedCoupleDuanWeiJiFen").Value.ToString());
                    cfg.DuanWeiType = Convert.ToInt32(xmlItem.Attribute("Type").Value.ToString());
                    cfg.DuanWeiLevel = Convert.ToInt32(xmlItem.Attribute("Level").Value.ToString());
                    cfg.WinJiFen = Convert.ToInt32(xmlItem.Attribute("WinJiFen").Value.ToString());
                    cfg.LoseJiFen = Convert.ToInt32(xmlItem.Attribute("LoseJiFen").Value.ToString());

                    _DuanWeiCfgList.Add(cfg);
                }
                _DuanWeiCfgList.Sort((_l, _r) => { return _l.NeedJiFen - _r.NeedJiFen; });

                DateTime now = TimeUtil.NowDateTime();
                Persistence.CheckClearRank(CurrRankWeek(now));
                SyncData.RankList = Persistence.LoadRankFromDb();
                SyncData.BuildRoleDict();
                SyncData.ModifyTime = now;

                this.IsNeedSort = false;
                for (int i = 1; i < SyncData.RankList.Count && !this.IsNeedSort; i++)
                {
                    this.IsNeedSort |= SyncData.RankList[i].CompareTo(SyncData.RankList[i - 1]) < 0;
                    this.IsNeedSort |= this.SyncData.RankList[i].Rank != this.SyncData.RankList[i - 1].Rank + 1;
                }

                CheckRebuildRank(now);
                CheckFlushRank2Db();
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Error, "CoupleArenaService.InitConfig failed!", ex);
            }
        }

        #endregion StartUp

        #region Interface `ICoupleArenaService`

        /// <summary>
        /// 跨服竞技匹配
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public int CoupleArenaJoin(int roleId1, int roleId2, int serverId)
        {
            lock (Mutex)
            {
                // 此处严格的判断夫妻是否有效
                if (!IsValidCoupleIfExist(roleId1, roleId2))
                    return StdErrorCode.Error_Server_Internal_Error;

                if (JoinDataUtil.GetJoinData(roleId1) != null || JoinDataUtil.GetJoinData(roleId2) != null)
                    return StdErrorCode.Error_Operation_Denied;

                CoupleArenaJoinData joinData = JoinDataUtil.Create();
                joinData.ServerId = serverId;
                joinData.RoleId1 = roleId1;
                joinData.RoleId2 = roleId2;
                joinData.StartTime = TimeUtil.NowDateTime();

                CoupleArenaCoupleDataK coupleData1;
                if (SyncData.RoleDict.TryGetValue(roleId1, out coupleData1))
                {
                    joinData.DuanWeiLevel = coupleData1.DuanWeiLevel;
                    joinData.DuanWeiType = coupleData1.DuanWeiType;
                }
                else
                {
                    // 夫妻二人首次参加
                    joinData.DuanWeiLevel = _DuanWeiCfgList[0].DuanWeiLevel;
                    joinData.DuanWeiType = _DuanWeiCfgList[0].DuanWeiType;
                }

                JoinDataUtil.AddJoinData(joinData);

                return StdErrorCode.Error_Success;
            }
        }

        /// <summary>
        /// 跨服竞技取消匹配
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public int CoupleArenaQuit(int roleId1, int roleId2)
        {
            lock (Mutex)
            {
                if (IsValidCoupleIfExist(roleId1, roleId2))
                {
                    JoinDataUtil.DelJoinData(JoinDataUtil.GetJoinData(roleId1));
                    JoinDataUtil.DelJoinData(JoinDataUtil.GetJoinData(roleId2));
                }

                return StdErrorCode.Error_Success;
            }
        }

        /// <summary>
        /// 同步情侣竞技数据
        /// </summary>
        /// <param name="lastSyncTime"></param>
        /// <returns></returns>
        public CoupleArenaSyncData CoupleArenaSync(DateTime lastSyncTime)
        {
            lock (Mutex)
            {
                if (lastSyncTime == SyncData.ModifyTime)
                    return null;

                CoupleArenaSyncData copyData = new CoupleArenaSyncData();
                copyData.RankList = new List<CoupleArenaCoupleDataK>(SyncData.RankList);
                copyData.RoleDict = null;
                copyData.ModifyTime = SyncData.ModifyTime;
                return copyData;
            }
        }

        /// <summary>
        /// 清除情侣数据，离婚了
        /// </summary>
        /// <param name="roleId1"></param>
        /// <param name="roleId2"></param>
        /// <returns></returns>
        public int CoupleArenaPreDivorce(int roleId1, int roleId2)
        {
            lock (Mutex)
            {
                if (!IsValidCoupleIfExist(roleId1, roleId2))
                    return StdErrorCode.Error_Server_Internal_Error;

                CoupleArenaQuit(roleId1, roleId2);

                DateTime now = TimeUtil.NowDateTime();
                CoupleArenaCoupleDataK data = null;
                if (!IsInWeekRangeActTimes(now))
                {
                    // 非活动时间内，离婚不影响排行榜，不用清除
                    // 但是第一名离婚，即使复婚也不能再获得称号
                    data = null;
                    if (SyncData.RoleDict.TryGetValue(roleId1, out data))
                    {
                        // 设置为已离婚，并存入数据库
                        data.IsDivorced = 1;
                        Persistence.WriteCoupleData(data);
                        if (data.Rank == 1)
                        {
                            // 如果是第一名才设置更新时间，让GameServer同步到
                            SyncData.ModifyTime = now;
                        }
                    }

                    return StdErrorCode.Error_Success;
                }

                data = null;
                if (!SyncData.RoleDict.TryGetValue(roleId1, out data))
                {
                    // 加入离婚记录
                    DivorceRecord.Add(roleId1, roleId2);
                    // 夫妻双方不再排行榜上，无效清理
                    return StdErrorCode.Error_Success;
                }

                if (data == null)
                {
                    return StdErrorCode.Error_Success;
                }

                // 必须保证竞技数据清理成功
                if (!Persistence.ClearCoupleData(data.Db_CoupleId))
                {
                    return StdErrorCode.Error_DB_Faild;
                }

                // 优化删除操作
                if (data.Rank - 1 >= 0 && data.Rank - 1 < SyncData.RankList.Count && SyncData.RankList[data.Rank - 1].Db_CoupleId == data.Db_CoupleId)
                {
                    SyncData.RankList.RemoveAt(data.Rank - 1);
                }
                else
                {
                    SyncData.RankList.RemoveAll(_r => _r.Db_CoupleId == data.Db_CoupleId);
                }

                // 加入离婚记录
                DivorceRecord.Add(roleId1, roleId2);
                SyncData.BuildRoleDict();
                SyncData.ModifyTime = now;

                this.IsNeedSort = true;

                return StdErrorCode.Error_Success;
            }
        }

        /// <summary>
        /// 根据gameid查询副本
        /// </summary>
        /// <param name="gameId"></param>
        /// <returns></returns>
        public CoupleArenaFuBenData GetFuBenData(long gameId)
        {
            lock (Mutex)
            {
                CoupleArenaFuBenData data;
                if (!GameFuBenDict.TryGetValue(gameId, out data))
                    data = null;

                return data;
            }
        }

        /// <summary>
        /// 上报pk结果, 中心进行分数计算，因为中心上玩家的数据(段位信息)才是最准确的
        /// </summary>
        /// <param name="gameId"></param>
        /// <param name="couple1"></param>
        /// <param name="couple2"></param>
        /// <param name="winSide"></param>
        /// <returns></returns>
        public CoupleArenaPkResultRsp CoupleArenaPkResult(CoupleArenaPkResultReq req)
        {
            if (req == null)
                return null;

            DateTime now = TimeUtil.NowDateTime();
            lock (Mutex)
            {
                if (!IsValidCoupleIfExist(req.ManRole1, req.WifeRole1)
                       || !IsValidCoupleIfExist(req.ManRole2, req.WifeRole2))
                    return null;

                CoupleArenaFuBenData fuben;
                if (!GameFuBenDict.TryGetValue(req.GameId, out fuben))
                    return null;

                CoupleArenaPkResultRsp rsp = new CoupleArenaPkResultRsp();

                if (req.winSide == 0)
                {
                    rsp.Couple1RetData.Result = (int)ECoupleArenaPkResult.Invalid;
                    rsp.Couple2RetData.Result = (int)ECoupleArenaPkResult.Invalid;
                }
                else if (req.winSide == 1)
                {
                    rsp.Couple1RetData.Result = DivorceRecord.IsDivorce(req.ManRole1, req.WifeRole1) ? (int)ECoupleArenaPkResult.Invalid : (int)ECoupleArenaPkResult.Win;
                    rsp.Couple2RetData.Result = DivorceRecord.IsDivorce(req.ManRole2, req.WifeRole2) ? (int)ECoupleArenaPkResult.Invalid : (int)ECoupleArenaPkResult.Fail;
                }
                else
                {
                    rsp.Couple1RetData.Result = DivorceRecord.IsDivorce(req.ManRole1, req.WifeRole1) ? (int)ECoupleArenaPkResult.Invalid : (int)ECoupleArenaPkResult.Fail;
                    rsp.Couple2RetData.Result = DivorceRecord.IsDivorce(req.ManRole2, req.WifeRole2) ? (int)ECoupleArenaPkResult.Invalid : (int)ECoupleArenaPkResult.Win;
                }

                // 胜利按对方的段位奖励，失败按自己的段位扣除
                int duanweiType1 = _DuanWeiCfgList[0].DuanWeiType, duanweiLevel1 = _DuanWeiCfgList[0].DuanWeiLevel;
                int duanweiType2 = _DuanWeiCfgList[0].DuanWeiType, duanweiLevel2 = _DuanWeiCfgList[0].DuanWeiLevel;
                if (SyncData.RoleDict.ContainsKey(req.ManRole1))
                {
                    duanweiType1 = SyncData.RoleDict[req.ManRole1].DuanWeiType;
                    duanweiLevel1 = SyncData.RoleDict[req.ManRole1].DuanWeiLevel;
                }
                if (SyncData.RoleDict.ContainsKey(req.ManRole2))
                {
                    duanweiType2 = SyncData.RoleDict[req.ManRole2].DuanWeiType;
                    duanweiLevel2 = SyncData.RoleDict[req.ManRole2].DuanWeiLevel;
                }

                HandlePkResult(req.ManRole1, req.ManZoneId1, req.ManSelector1, req.WifeRole1, req.WifeZoneId1, req.WifeSelector1, duanweiType2, duanweiLevel2, rsp.Couple1RetData);
                HandlePkResult(req.ManRole2, req.ManZoneId2, req.ManSelector2, req.WifeRole2, req.WifeZoneId2, req.WifeSelector2, duanweiType1, duanweiLevel1, rsp.Couple2RetData);

                RemoveFuBen(req.GameId);
                Persistence.AddPkLog(fuben.GameId, fuben.StartTime, TimeUtil.NowDateTime(),
                    req.ManRole1, req.WifeRole1, rsp.Couple1RetData.Result,
                    req.ManRole2, req.WifeRole2, rsp.Couple2RetData.Result);
                return rsp;
            }
        }

        private void HandlePkResult(int man, int manzone, byte[] mandata, int wife, int wifezone, byte[] wifedata, int pkDuanWeiType, int pkDuanWeiLevel, CoupleArenaPkResultItem retData)
        {
            CoupleArenaCoupleDataK coupleData = null;
            if (!SyncData.RoleDict.TryGetValue(man, out coupleData))
            {
                coupleData = new CoupleArenaCoupleDataK();
                coupleData.Db_CoupleId = Persistence.GetNextDbCoupleId();
                coupleData.ManRoleId = man;
                coupleData.ManZoneId = manzone;
                coupleData.ManSelectorData = mandata;
                coupleData.WifeRoleId = wife;
                coupleData.WifeZoneId = wifezone;
                coupleData.WifeSelectorData = wifedata;
                coupleData.DuanWeiLevel = _DuanWeiCfgList[0].DuanWeiLevel;
                coupleData.DuanWeiType = _DuanWeiCfgList[0].DuanWeiType;
                coupleData.Rank = SyncData.RankList.Count + 1;
                if (retData.Result != (int)ECoupleArenaPkResult.Invalid)
                {
                    SyncData.RankList.Add(coupleData);
                    SyncData.RoleDict[coupleData.ManRoleId] = coupleData;
                    SyncData.RoleDict[coupleData.WifeRoleId] = coupleData;
                }
            }
            else
            {
                // 更新形象
                coupleData.ManSelectorData = mandata;
                coupleData.WifeSelectorData = wifedata;
            }

            retData.OldDuanWeiType = coupleData.DuanWeiType;
            retData.OldDuanWeiLevel = coupleData.DuanWeiLevel;

            // 失败扣除自己段位的积分
            _CoupleArenaDuanWeiCfg duanweiCfgLose = _DuanWeiCfgList.Find(
                _d => _d.DuanWeiLevel == coupleData.DuanWeiLevel && _d.DuanWeiType == coupleData.DuanWeiType);
            if (duanweiCfgLose == null)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("couplearena.HandlePkResult can't find duanwei cfg ,type={0}, level={1}", coupleData.DuanWeiType, coupleData.DuanWeiLevel));
                return;
            }

            // 胜利奖励对方段位的积分
            _CoupleArenaDuanWeiCfg duanweiCfgWin = _DuanWeiCfgList.Find(
              _d => _d.DuanWeiLevel == pkDuanWeiLevel && _d.DuanWeiType == pkDuanWeiType);
            if (duanweiCfgWin == null)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("couplearena.HandlePkResult can't find duanwei cfg ,type={0}, level={1}", pkDuanWeiType, pkDuanWeiLevel));
                return;
            }

            if (retData.Result == (int)ECoupleArenaPkResult.Invalid)
            {
                retData.NewDuanWeiType = coupleData.DuanWeiType;
                retData.NewDuanWeiLevel = coupleData.DuanWeiLevel;
            }
            else
            {
                coupleData.TotalFightTimes++;
                if (retData.Result == (int)ECoupleArenaPkResult.Win)
                {
                    coupleData.WinFightTimes++;
                    coupleData.LianShengTimes++;
                    coupleData.JiFen += duanweiCfgWin.WinJiFen;

                    retData.GetJiFen = duanweiCfgWin.WinJiFen;
                }
                else
                {
                    coupleData.LianShengTimes = 0;
                    coupleData.JiFen += duanweiCfgLose.LoseJiFen;
                    coupleData.JiFen = Math.Max(coupleData.JiFen, 0);

                    retData.GetJiFen = duanweiCfgLose.LoseJiFen;
                }

                ParseDuanweiByJiFen(coupleData.JiFen, out coupleData.DuanWeiType, out coupleData.DuanWeiLevel);
                SyncData.ModifyTime = TimeUtil.NowDateTime();

                retData.NewDuanWeiLevel = coupleData.DuanWeiLevel;
                retData.NewDuanWeiType = coupleData.DuanWeiType;
                this.IsNeedSort = true;
            }

            if (retData.Result != (int)ECoupleArenaPkResult.Invalid)
            {
                Persistence.WriteCoupleData(coupleData);
            }
        }

        private void ParseDuanweiByJiFen(int jifen, out int duanweiType, out int duanweiLevel)
        {
            duanweiLevel = _DuanWeiCfgList[0].DuanWeiLevel;
            duanweiType = _DuanWeiCfgList[0].DuanWeiType;

            for (int i = 0; i < _DuanWeiCfgList.Count; i++)
            {
                if (jifen >= _DuanWeiCfgList[i].NeedJiFen)
                {
                    if ((i == _DuanWeiCfgList.Count - 1)
                        || (jifen < _DuanWeiCfgList[i + 1].NeedJiFen))
                    {
                        duanweiType = _DuanWeiCfgList[i].DuanWeiType;
                        duanweiLevel = _DuanWeiCfgList[i].DuanWeiLevel;
                    }
                }
            }
        }

        #endregion Interface `ICoupleArenaService`

        #region `Timer Driver`

        public void Update()
        {
            try
            {
                lock (Mutex)
                {
                    DateTime now = TimeUtil.NowDateTime();
                    // 每秒1帧
                    if ((now - LastUpdateTime).TotalMilliseconds < 1000)
                        return;
                    UpdateFrameCount++;

                    if (now.DayOfYear != LastUpdateTime.DayOfYear)
                    {
                        MatchTimeLimiter.Reset();
                        JoinDataUtil.Reset();
                        DivorceRecord.Reset();
                    }

                    // 每1帧检测夫妻匹配
                    CheckRoleMatch(now);

                    // 每30帧检测超时副本
                    if (UpdateFrameCount % 30 == 0)
                        CheckTimeOutFuBen(now);

                    // 本周活动首次开始，检测清理排行榜
                    if (LastUpdateTime.TimeOfDay.Ticks <= _WarTimePointList.First().DayStartTicks
                        && now.TimeOfDay.Ticks >= _WarTimePointList.First().DayStartTicks)
                    {
                        if (Persistence.CheckClearRank(CurrRankWeek(now)))
                        {
                            lock (Mutex)
                            {
                                SyncData.RankList.Clear();
                                SyncData.BuildRoleDict();
                                SyncData.ModifyTime = now;
                            }
                        }
                    }

                    // 每30帧检测重建排行榜
                    if (UpdateFrameCount % 30 == 0)
                        CheckRebuildRank(now);

                    // 每300帧定期检测存库
                    if (UpdateFrameCount % 300 == 0)
                        CheckFlushRank2Db();

                    LastUpdateTime = now;
                }
            }
            catch (Exception ex)
            {
                LogManager.WriteExceptionUseCache("CoupleArenaService.Update failed! " + ex.Message);
            }
        }

        /// <summary>
        /// 夫妻匹配
        /// </summary>
        /// <param name="now"></param>
        private void CheckRoleMatch(DateTime now)
        {
            lock (Mutex)
            {
                var joinDatas = JoinDataUtil.GetJoinList();
                if (joinDatas == null || joinDatas.Count <= 0) return;

                CoupleArenaJoinMatcher joinMatcher = new CoupleArenaJoinMatcher();
                foreach (var joinData in joinDatas)
                {
                    if ((now - joinData.StartTime).TotalSeconds >= 60 || joinData.ToKfServerId > 0)
                        JoinDataUtil.DelJoinData(joinData);
                    else if ((now - joinData.StartTime).TotalSeconds >= 30)
                        joinMatcher.AddGlobalJoinData(joinData);
                    else
                        joinMatcher.AddJoinData(joinData.DuanWeiType, joinData.DuanWeiLevel, joinData);
                }

                foreach (var list in joinMatcher.GetAllMatch())
                {
                    for (int i = 0; i < list.Count - 1;)
                    {
                        var one = list[i];
                        var two = list[i + 1];
                        if (MatchTimeLimiter.GetMatchTimes(one.RoleId1, one.RoleId2, two.RoleId1, two.RoleId2)
                            >= TianTiPersistence.Instance.MaxRolePairFightCount)
                        {
                            // 次数先知 第i个元素，让第i+1个元素和第i+2个元素进行匹配
                            i += 1;
                            continue;
                        }

                        CoupleArenaFuBenData fubenData = new CoupleArenaFuBenData();
                        fubenData.GameId = Persistence.GetNextGameId();
                        fubenData.StartTime = now;
                        fubenData.RoleList = new List<KuaFuFuBenRoleData>();
                        fubenData.RoleList.Add(new KuaFuFuBenRoleData() { ServerId = one.ServerId, RoleId = one.RoleId1, Side = 1 });
                        fubenData.RoleList.Add(new KuaFuFuBenRoleData() { ServerId = one.ServerId, RoleId = one.RoleId2, Side = 1 });
                        fubenData.RoleList.Add(new KuaFuFuBenRoleData() { ServerId = two.ServerId, RoleId = two.RoleId1, Side = 2 });
                        fubenData.RoleList.Add(new KuaFuFuBenRoleData() { ServerId = two.ServerId, RoleId = two.RoleId2, Side = 2 });
                        if (ClientAgentManager.Instance().AssginKfFuben(GameType, fubenData.GameId, 4, out fubenData.KfServerId))
                        {
                            MatchTimeLimiter.AddMatchTimes(one.RoleId1, one.RoleId2, two.RoleId1, two.RoleId2);
                            GameFuBenDict[fubenData.GameId] = fubenData;
                            i += 2;

                            one.ToKfServerId = fubenData.KfServerId;
                            two.ToKfServerId = fubenData.KfServerId;

                            CoupleArenaCanEnterData enterData1 = new CoupleArenaCanEnterData()
                            {
                                GameId = fubenData.GameId,
                                KfServerId = fubenData.KfServerId,
                                RoleId1 = one.RoleId1,
                                RoleId2 = one.RoleId2
                            };
                            ClientAgentManager.Instance().PostAsyncEvent(one.ServerId, EvItemGameType,
                                new AsyncDataItem(KuaFuEventTypes.CoupleArenaCanEnter, enterData1));

                            CoupleArenaCanEnterData enterData2 = new CoupleArenaCanEnterData()
                            {
                                GameId = fubenData.GameId,
                                KfServerId = fubenData.KfServerId,
                                RoleId1 = two.RoleId1,
                                RoleId2 = two.RoleId2
                            };
                            AsyncDataItem evItem2 = new AsyncDataItem(KuaFuEventTypes.CoupleArenaCanEnter, fubenData.GameId, fubenData.KfServerId, two.RoleId1, two.RoleId2);
                            ClientAgentManager.Instance().PostAsyncEvent(two.ServerId, EvItemGameType,
                                new AsyncDataItem(KuaFuEventTypes.CoupleArenaCanEnter, enterData2));
                        }
                        else
                        {
                            LogManager.WriteLog(LogTypes.Error, "CoupleArena 没有跨服可以分配");
                            return;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 检测超时未删除的副本
        /// </summary>
        /// <param name="now"></param>
        private void CheckTimeOutFuBen(DateTime now)
        {
            lock (Mutex)
            {
                foreach (var fuben in GameFuBenDict.Values.ToList())
                {
                    if ((now - fuben.StartTime).TotalMinutes > 5)
                        RemoveFuBen(fuben.GameId);
                }
            }
        }

        private void RemoveFuBen(long gameId)
        {
            lock (Mutex)
            {
                CoupleArenaFuBenData fuben;
                if (!GameFuBenDict.TryGetValue(gameId, out fuben))
                    return;

                ClientAgentManager.Instance().RemoveKfFuben(GameType, fuben.KfServerId, fuben.GameId);
                GameFuBenDict.Remove(fuben.GameId);
            }
        }

        /// <summary>
        /// 检测是否需要重建排行榜
        /// </summary>
        /// <param name="now"></param>
        private void CheckRebuildRank(DateTime now)
        {
            lock (Mutex)
            {
                if (!IsNeedSort) return;

                SyncData.RankList.Sort();
                for (int i = 0; i < SyncData.RankList.Count; i++)
                {
                    SyncData.RankList[i].Rank = i + 1;
                }
                SyncData.BuildRoleDict();
                SyncData.ModifyTime = now;

                this.IsNeedSort = false;
                this.IsRankChanged = true;
            }
        }

        /// <summary>
        /// 检测是否需要刷新排行榜数据到db
        /// </summary>
        private void CheckFlushRank2Db()
        {
            lock (Mutex)
            {
                if (!this.IsRankChanged) return;
                LogManager.WriteLog(LogTypes.Error, "Persistence.FlushRandList2Db begin");
                Persistence.FlushRandList2Db(SyncData.RankList);
                LogManager.WriteLog(LogTypes.Error, "Persistence.FlushRandList2Db end");
                this.IsRankChanged = false;
            }
        }

        #endregion `Timer Driver`

        #region Util

        /// <summary>
        /// 查看当前应该是第几周的排行榜
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        private int CurrRankWeek(DateTime time)
        {
            int currWeekDay = TimeUtil.GetWeekDay1To7(time);
            // 本次首次活动尚未开始，查看上周排行榜
            var first = _WarTimePointList.First();
            if (currWeekDay < first.Weekday
                || (currWeekDay == first.Weekday && time.TimeOfDay.Ticks < first.DayStartTicks))
            {
                return TimeUtil.MakeFirstWeekday(time.AddDays(-7));
            }
            else
            {
                return TimeUtil.MakeFirstWeekday(time);
            }
        }

        /// <summary>
        /// 是否处于1次活动时间内
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        private bool IsInWeekOnceActTimes(DateTime time)
        {
            int wd = TimeUtil.GetWeekDay1To7(time);
            foreach (var tp in _WarTimePointList)
            {
                if (tp.Weekday == wd
                    && time.TimeOfDay.Ticks >= tp.DayStartTicks
                    && time.TimeOfDay.Ticks <= tp.DayEndTicks)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 是否处于本周总的活动时间，[本周第一次开始，本周最后一次结束]
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        private bool IsInWeekRangeActTimes(DateTime time)
        {
            var first = _WarTimePointList.First();
            var last = _WarTimePointList.Last();
            var wd = TimeUtil.GetWeekDay1To7(time);
            if (((wd == first.Weekday && time.TimeOfDay.Ticks > first.DayStartTicks) || wd > first.Weekday)
                && (wd < last.Weekday || (wd == last.Weekday && time.TimeOfDay.Ticks < last.DayEndTicks)))
                return true;

            return false;
        }

        /// <summary>
        /// 检测是否是现有数据中的有效夫妻
        /// 如果尚未有该夫妻数据，那么返回true
        /// </summary>
        /// <param name="roleId1"></param>
        /// <param name="roleId2"></param>
        /// <returns></returns>
        private bool IsValidCoupleIfExist(int roleId1, int roleId2)
        {
            lock (Mutex)
            {
                CoupleArenaCoupleDataK coupleData1, coupleData2;
                SyncData.RoleDict.TryGetValue(roleId1, out coupleData1);
                SyncData.RoleDict.TryGetValue(roleId2, out coupleData2);

                // 夫妻双方数据不存在，认为是有效的夫妻，(首次)
                if (coupleData1 == null && coupleData2 == null)
                    return true;

                // 夫妻双方其中一方数据不存在
                if (coupleData1 == null || coupleData2 == null)
                    return false;

                // 夫妻双方不是同一个数据
                if (!Object.ReferenceEquals(coupleData1, coupleData2))
                    return false;

                if ((coupleData1.ManRoleId == roleId1 && coupleData1.WifeRoleId == roleId2)
                    || (coupleData1.ManRoleId == roleId2 && coupleData1.WifeRoleId == roleId1))
                    return true;
                else
                    return false;
            }
        }

        /// <summary>
        /// 停止服务回调
        /// </summary>
        public void OnStopServer()
        {
            try
            {
                SysConOut.WriteLine("开始检测是否刷新情侣竞技数据到数据库...");
                lock (Mutex)
                {
                    this.CheckRebuildRank(TimeUtil.NowDateTime());
                    this.CheckFlushRank2Db();
                }
                SysConOut.WriteLine("结束检测是否刷新情侣竞技数据到数据库...");
            }
            catch (Exception ex)
            {
                LogManager.WriteException(ex.Message);
            }
        }

        #endregion Util
    }
}