using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using KF.Contract.Interface;
using KF.Contract.Data;
using Server.Tools;
using GameServer.Core.Executor;
using Tmsk.Contract;

namespace KF.Remoting
{
    /// <summary>
    /// 情侣祝福榜
    /// </summary>
    class CoupleWishService : ICoupleWishService
    {
        #region Singleton
        private static readonly CoupleWishService _Instance = new CoupleWishService();
        private CoupleWishService() { }
        public static CoupleWishService getInstance() { return _Instance; }
        #endregion

        #region Member
        private object Mutex = new object();
        private CoupleWishSyncData SyncData = new CoupleWishSyncData();
        private DateTime LastUpdateTime = DateTime.MinValue;
        private uint UpdateFrameCount = 0;

        private CoupleWishRecordManager WishRecordMgr = null;
        private CoupleWishConfig _Config = new CoupleWishConfig();
        private CoupleWishPersistence Persistence = CoupleWishPersistence.getInstance();
        private Dictionary<int, long> WishCdControls = new Dictionary<int, long>();
        private bool IsNeedSort = false;
        private bool IsNeedSaveRank = false;
        #endregion

        #region StartUp
        /// <summary>
        /// 服务启动
        /// </summary>
        public void StartUp()
        {
            try
            {
                _Config.Load(KuaFuServerManager.GetResourcePath(CoupleWishConsts.RankAwardCfgFile, KuaFuServerManager.ResourcePathTypes.GameRes),
                    KuaFuServerManager.GetResourcePath(CoupleWishConsts.WishTypeCfgFile, KuaFuServerManager.ResourcePathTypes.GameRes),
                    KuaFuServerManager.GetResourcePath(CoupleWishConsts.YanHuiCfgFile, KuaFuServerManager.ResourcePathTypes.GameRes));
                ReloadSyncData();
                WishRecordMgr = new CoupleWishRecordManager(this.SyncData.ThisWeek.Week);
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Error, "CoupleWishService.StartUp failed!", ex);
            }
        }

        /// <summary>
        /// 加载排行数据
        /// </summary>
        private void ReloadSyncData()
        {
            lock (Mutex)
            {
                DateTime now = TimeUtil.NowDateTime();

                SyncData.ThisWeek.ModifyTime = now;
                SyncData.ThisWeek.Week = CurrRankWeek(now);
                SyncData.ThisWeek.RankList = Persistence.LoadRankFromDb(SyncData.ThisWeek.Week);
                SyncData.ThisWeek.BuildIndex();
                for (int i = 1; i < SyncData.ThisWeek.RankList.Count && !this.IsNeedSort; i++)
                    this.IsNeedSort = SyncData.ThisWeek.RankList[i].CompareTo(SyncData.ThisWeek.RankList[i - 1]) < 0;

                CheckSortRank();
                CheckSaveRank();

                SyncData.LastWeek.ModifyTime = now;
                SyncData.LastWeek.Week = CurrRankWeek(now.AddDays(-7));
                SyncData.LastWeek.RankList = Persistence.LoadRankFromDb(SyncData.LastWeek.Week);
                SyncData.LastWeek.BuildIndex();

                SyncData.Statue = Persistence.LoadCoupleStatue(SyncData.LastWeek.Week);
                if (SyncData.LastWeek.RankList.Count > 0 && SyncData.LastWeek.RankList.First().Rank == 1
                    && SyncData.Statue.DbCoupleId != SyncData.LastWeek.RankList.First().DbCoupleId)
                {
                    SyncData.Statue = new CoupleWishSyncStatueData();
                    SyncData.Statue.ModifyTime = TimeUtil.NowDateTime();
                    SyncData.Statue.Week = SyncData.LastWeek.Week;
                    SyncData.Statue.DbCoupleId = this.SyncData.LastWeek.RankList.First().DbCoupleId;
                    SyncData.Statue.Man = this.SyncData.LastWeek.RankList.First().Man;
                    SyncData.Statue.Wife = this.SyncData.LastWeek.RankList.First().Wife;

                    Persistence.WriteStatueData(SyncData.Statue);
                }
            }
        }
        #endregion

        #region Update
        /// <summary>
        /// 定时更新
        /// </summary>
        public void Update()
        {
            try
            {
                DateTime now = TimeUtil.NowDateTime();
                if ((now - LastUpdateTime).TotalMilliseconds < 1000)
                    return;
                // 1秒1帧
                UpdateFrameCount++;

                if (LastUpdateTime.DayOfYear != now.DayOfYear && TimeUtil.GetWeekDay1To7(now) == 1)
                {
                    lock (Mutex)
                    {
                        // 周一，跨周了，要立即检测刷新一次排行榜
                        CheckSortRank();
                        CheckSaveRank();
                        ReloadSyncData();
                        WishRecordMgr.UpdateWeek(this.SyncData.ThisWeek.Week);
                    }
                }

                // 30帧检测排序
                if (UpdateFrameCount % 30 == 0)
                    CheckSortRank();

                // 600帧检测刷库
                if (UpdateFrameCount % 600 == 0)
                {
                    CheckSaveRank();
                    WishRecordMgr.ClearUnActiveRecord();
                }

                LastUpdateTime = now;
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Error, "CoupleWishService.Update failed!", ex);
            }
        }
        #endregion

        #region Interface `ICoupleWishService`
        /// <summary>
        /// 祝福情侣
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public int CoupleWishWishRole(CoupleWishWishRoleReq req)
        {
            DateTime now = TimeUtil.NowDateTime();
            long nowMs = now.Ticks / TimeSpan.TicksPerMillisecond;

            try
            {
                lock (Mutex)
                {
                    if (this.SyncData.ThisWeek.Week != CurrRankWeek(now))
                        return StdErrorCode.Error_Server_Busy;

                    CoupleWishTypeConfig wishCfg = _Config.WishTypeCfgList.Find(_w => _w.WishType == req.WishType);
                    if (wishCfg == null)
                        return StdErrorCode.Error_Config_Fault;

                    if (wishCfg.CooldownTime > 0)
                    {
                        if (WishCdControls.ContainsKey(req.WishType)
                            && nowMs - WishCdControls[req.WishType] < wishCfg.CooldownTime * 1000)
                            return StdErrorCode.Error_Wish_Type_Is_In_CD;
                    }

                    CoupleWishCoupleDataK coupleData = null;
                    if (req.IsWishRank)
                    {
                        int idx;
                        if (!SyncData.ThisWeek.CoupleIdex.TryGetValue(req.ToCoupleId, out idx))
                            return StdErrorCode.Error_Operation_Faild;

                        coupleData = SyncData.ThisWeek.RankList[idx];
                        coupleData.BeWishedNum += wishCfg.GetWishNum;

                        if (req.ToManSelector != null && req.ToWifeSelector != null)
                        {
                            // 排行榜祝福，如果祝福者和被祝福者是同一服务器的，那么也会更新被祝福者的形象
                            coupleData.Man = req.ToMan;
                            coupleData.ManSelector = req.ToManSelector;
                            coupleData.Wife = req.ToWife;
                            coupleData.WifeSelector = req.ToWifeSelector;
                            Persistence.WriteCoupleData(this.SyncData.ThisWeek.Week, coupleData);
                        }
                    }
                    else
                    {
                        // 本服祝福
                        if (req.ToManSelector == null || req.ToWifeSelector == null)
                            return StdErrorCode.Error_Server_Internal_Error;

                        if (!IsValidCoupleIfExist(req.ToMan.RoleId, req.ToWife.RoleId))
                            return StdErrorCode.Error_Server_Internal_Error;

                        bool bFirstCreate = false;
                        int idx;
                        if (!this.SyncData.ThisWeek.RoleIndex.TryGetValue(req.ToMan.RoleId, out idx))
                        {
                            // 首次被祝福
                            bFirstCreate = true;
                            coupleData = new CoupleWishCoupleDataK();
                            coupleData.DbCoupleId = Persistence.GetNextDbCoupleId();
                            coupleData.Rank = this.SyncData.ThisWeek.RankList.Count + 1;
                        }
                        else
                        {
                            coupleData = this.SyncData.ThisWeek.RankList[idx];
                        }

                        // 本服祝福会更新被祝福者形象
                        coupleData.Man = req.ToMan;
                        coupleData.ManSelector = req.ToManSelector;
                        coupleData.Wife = req.ToWife;
                        coupleData.WifeSelector = req.ToWifeSelector;
                        coupleData.BeWishedNum += wishCfg.GetWishNum;

                        // 本服祝福，保证把形象更新到db
                        if (!Persistence.WriteCoupleData(this.SyncData.ThisWeek.Week, coupleData))
                        {
                            coupleData.BeWishedNum -= wishCfg.GetWishNum;
                            return StdErrorCode.Error_DB_Faild;
                        }

                        if (bFirstCreate)
                        {
                            this.SyncData.ThisWeek.RankList.Add(coupleData);
                            this.SyncData.ThisWeek.BuildIndex();
                        }
                    }

                    this.IsNeedSort = true;
                    if (this.SyncData.ThisWeek.RankList.Count <= CoupleWishConsts.MaxRankNum
                        || this.SyncData.ThisWeek.RankList.Last().Rank <= CoupleWishConsts.MaxRankNum)
                    {
                        // 不足20名时，立即刷新
                        this.CheckSortRank();
                    }

                    WishCdControls[req.WishType] = nowMs;
                    WishRecordMgr.AddWishRecord(req.From, req.WishType, req.WishTxt, coupleData.DbCoupleId, coupleData.Man, coupleData.Wife);
                    return StdErrorCode.Error_Success;
                }
            }
            catch (Exception ex)
            {
                LogManager.WriteExceptionUseCache(ex.Message);
                return StdErrorCode.Error_Server_Internal_Error;
            }
        }

        /// <summary>
        /// 获取排行榜
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public List<CoupleWishWishRecordData> CoupleWishGetWishRecord(int roleId)
        {
            try
            {
                lock (Mutex)
                {
                    return WishRecordMgr.GetWishRecord(roleId);
                }
            }
            catch (Exception ex)
            {
                LogManager.WriteExceptionUseCache(ex.Message);
                return null;
            }
        }

        /// <summary>
        /// gameserver 同步数据
        /// </summary>
        /// <param name="lastSyncTime"></param>
        /// <returns></returns>
        public CoupleWishSyncData CoupleWishSyncCenterData(DateTime oldThisWeek, DateTime oldLastWeek, DateTime oldStatue)
        {
            try
            {
                lock (Mutex)
                {
                    CoupleWishSyncData syncData = new CoupleWishSyncData();
                    if (oldThisWeek != this.SyncData.ThisWeek.ModifyTime)
                        syncData.ThisWeek = this.SyncData.ThisWeek.SimpleClone();
                    else
                        syncData.ThisWeek.ModifyTime = oldThisWeek;

                    if (oldLastWeek != this.SyncData.LastWeek.ModifyTime)
                        syncData.LastWeek = this.SyncData.LastWeek.SimpleClone();
                    else
                        syncData.LastWeek.ModifyTime = oldLastWeek;

                    if (oldStatue != this.SyncData.Statue.ModifyTime)
                        syncData.Statue = this.SyncData.Statue.SimpleClone();
                    else
                        syncData.Statue.ModifyTime = oldStatue;

                    return syncData;
                }
            }
            catch (Exception ex)
            {
                LogManager.WriteExceptionUseCache(ex.Message);
                return null;
            }
        }

        /// <summary>
        /// 离婚前清除数据
        /// </summary>
        /// <param name="man"></param>
        /// <param name="wife"></param>
        /// <returns></returns>
        public int CoupleWishPreDivorce(int man, int wife)
        {
            lock (Mutex)
            {
                // 是否是有效的夫妻
                if (!IsValidCoupleIfExist(man, wife))
                    return StdErrorCode.Error_Server_Internal_Error;

                // 非祝福活动时间，离婚不影响排行榜
                int week;
                DateTime now = TimeUtil.NowDateTime();
                if (_Config.IsInWishTime(now, out week))
                {
                    int idx;
                    if (this.SyncData.ThisWeek.RoleIndex.TryGetValue(man, out idx))
                    {
                        CoupleWishCoupleDataK data = this.SyncData.ThisWeek.RankList[idx];

                        // 清除本周数据
                        if (!Persistence.ClearCoupleData(data.DbCoupleId))
                            return StdErrorCode.Error_DB_Faild;

                        this.SyncData.ThisWeek.RankList.RemoveAt(idx);
                        this.SyncData.ThisWeek.BuildIndex();
                        this.IsNeedSort = true;
                    }                  
                }

                if (SyncData.Statue.DbCoupleId > 0 && SyncData.Statue.Man != null && SyncData.Statue.Wife != null)
                {
                    if (SyncData.Statue.Man.RoleId == man && SyncData.Statue.Wife.RoleId == wife && SyncData.Statue.IsDivorced != 1)
                    {
                        int oldDivorced = SyncData.Statue.IsDivorced;
                        SyncData.Statue.IsDivorced = 1;
                        if (!Persistence.WriteStatueData(SyncData.Statue))
                        {
                            SyncData.Statue.IsDivorced = oldDivorced;
                            return StdErrorCode.Error_DB_Faild;
                        }

                        SyncData.Statue.ModifyTime = now;
                    }
                }


                return StdErrorCode.Error_Success;
            }
        }

        /// <summary>
        /// 膜拜雕像
        /// </summary>
        /// <param name="fromRole"></param>
        /// <param name="fromZone"></param>
        /// <param name="admireType"></param>
        /// <param name="toCoupleId"></param>
        /// <returns></returns>
        public int CoupleWishAdmire(int fromRole, int fromZone, int admireType, int toCoupleId)
        {
            lock (Mutex)
            {
                // 不管有没有雕像，都可以膜拜
                if (this.SyncData.Statue.DbCoupleId > 0 
                    && this.SyncData.Statue.DbCoupleId == toCoupleId
                    && this.SyncData.Statue.ManRoleDataEx != null 
                    && this.SyncData.Statue.WifeRoleDataEx != null)
                {
                    this.SyncData.Statue.BeAdmireCount++;
                    Persistence.WriteStatueData(this.SyncData.Statue);
                }

                Persistence.AddAdmireLog(fromRole, fromZone, admireType, toCoupleId, this.SyncData.LastWeek.Week);
                this.SyncData.Statue.ModifyTime = TimeUtil.NowDateTime();
                return StdErrorCode.Error_Success;
            }
        }

        /// <summary>
        /// 参加宴会
        /// </summary>
        /// <param name="fromRole"></param>
        /// <param name="fromZone"></param>
        /// <param name="toCoupleId"></param>
        /// <returns></returns>
        public int CoupleWishJoinParty(int fromRole, int fromZone, int toCoupleId)
        {
            lock (Mutex)
            {
                if (this.SyncData.Statue.DbCoupleId != toCoupleId)
                    return StdErrorCode.Error_Operation_Denied;

                if (this.SyncData.Statue.YanHuiJoinNum >= _Config.YanHuiCfg.TotalMaxJoinNum)
                    return StdErrorCode.Error_No_Residue_Degree;

                this.SyncData.Statue.YanHuiJoinNum++;
                if (!Persistence.WriteStatueData(this.SyncData.Statue))
                {
                    this.SyncData.Statue.YanHuiJoinNum--;
                    return StdErrorCode.Error_DB_Faild;
                }

                Persistence.AddYanHuiJoinLog(fromRole, fromZone, toCoupleId, this.SyncData.LastWeek.Week);
                this.SyncData.Statue.ModifyTime = TimeUtil.NowDateTime();
                return StdErrorCode.Error_Success;
            }
        }

        /// <summary>
        /// 上报某对couple的雕像数据
        /// </summary>
        /// <param name="req"></param>
        public void CoupleWishReportCoupleStatue(CoupleWishReportStatueData req)
        {
            if (req == null || req.ManStatue == null || req.WifeStatue == null)
                return;

            lock (Mutex)
            {
                if (SyncData.Statue.DbCoupleId == req.DbCoupleId)
                {
                    var oldManEx = SyncData.Statue.ManRoleDataEx;
                    var oldWifeEx = SyncData.Statue.WifeRoleDataEx;
                    SyncData.Statue.ManRoleDataEx = req.ManStatue;
                    SyncData.Statue.WifeRoleDataEx = req.WifeStatue;
                    if (!Persistence.WriteStatueData(SyncData.Statue))
                    {
                        SyncData.Statue.ManRoleDataEx = oldManEx;
                        SyncData.Statue.WifeRoleDataEx = oldWifeEx;
                    }
                }

                SyncData.Statue.ModifyTime = TimeUtil.NowDateTime();
            }
        }
        #endregion

        #region Util
        /// <summary>
        /// 检测重排
        /// </summary>
        private void CheckSortRank()
        {
            lock (Mutex)
            {
                if (!IsNeedSort) return;
                IsNeedSort = false;

                this.SyncData.ThisWeek.RankList.Sort();

                // 下标0,1,2,3,4分别表示第一名，第二名，第三名。。。的最小祝福数限制
                List<int> eachRankMinLimit = new List<int>();
                foreach (var cfg in _Config.RankAwardCfgList)
                {
                    if (cfg.EndRank <= 0) break;
                    for (int i = cfg.StartRank; i <= cfg.EndRank; i++)
                    {
                        eachRankMinLimit.Add(cfg.MinWishNum);
                    }
                }

                int currRank = 1, currIdx = 0;
                while (currIdx < this.SyncData.ThisWeek.RankList.Count)
                {
                    if (currRank - 1 >= 0 && currRank - 1 < eachRankMinLimit.Count)
                    {
                        // currRank 有最小祝福数限制
                        if (this.SyncData.ThisWeek.RankList[currIdx].BeWishedNum >= eachRankMinLimit[currRank - 1])
                        {
                            // currIdx 处的couple满足currRank的最小祝福数限制
                            this.SyncData.ThisWeek.RankList[currIdx].Rank = currRank;
                            currRank++;
                            currIdx++;
                        }
                        else
                        {
                            currRank++;
                        }
                    }
                    else
                    {
                        // currRank 没有最小祝福数限制
                        this.SyncData.ThisWeek.RankList[currIdx].Rank = currRank;
                        currRank++;
                        currIdx++;
                    }
                }

                this.SyncData.ThisWeek.ModifyTime = TimeUtil.NowDateTime();
                this.SyncData.ThisWeek.BuildIndex();
                this.IsNeedSaveRank = true;
            }
        }

        /// <summary>
        /// 检测刷新排行榜到db
        /// </summary>
        private void CheckSaveRank()
        {
            lock (Mutex)
            {
                if (!IsNeedSaveRank) return;
                LogManager.WriteLog(LogTypes.Error, "CoupleWishService.CheckSaveRank begin");
                Persistence.UpdateRand2Db(this.SyncData.ThisWeek.RankList);
                LogManager.WriteLog(LogTypes.Error, "CoupleWishService.CheckSaveRank end");
                IsNeedSaveRank = false;
            }
        }

        /// <summary>
        /// 是否是有效的情侣组合，如何不存在，认为有效(首次)
        /// </summary>
        /// <param name="man"></param>
        /// <param name="wife"></param>
        /// <returns></returns>
        private bool IsValidCoupleIfExist(int man, int wife)
        {
            lock (Mutex)
            {
                int manIdx, wifeIdx;

                if (!this.SyncData.ThisWeek.RoleIndex.TryGetValue(man, out manIdx))
                    manIdx = -1;
                if (!this.SyncData.ThisWeek.RoleIndex.TryGetValue(wife, out wifeIdx))
                    wifeIdx = -1;

                // 丈夫和妻子存储的数据位置不同，即不是同一个数据
                if (manIdx != wifeIdx)
                    return false;

                if (manIdx != -1)
                {
                    // 如果有数据，检查数据正确性
                    CoupleWishCoupleDataK coupleData = this.SyncData.ThisWeek.RankList[manIdx];
                    if (coupleData.Man.RoleId != man || coupleData.Wife.RoleId != wife)
                        return false;
                }

                return true;
            }
        }

        /// <summary>
        /// 当前第几周的排行
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        private int CurrRankWeek(DateTime time)
        {
            return TimeUtil.MakeFirstWeekday(time);
        }

        /// <summary>
        /// 停止服务回调
        /// </summary>
        public void OnStopServer()
        {
            try
            {
                SysConOut.WriteLine("开始检测是否刷新情侣排行榜到数据库...");
                lock (Mutex)
                {
                    this.CheckSortRank();
                    this.CheckSaveRank();
                }
                SysConOut.WriteLine("结束检测是否刷新情侣排行榜到数据库...");
            }
            catch (Exception ex)
            {
                LogManager.WriteException(ex.Message);
            }
        }
        #endregion
    }
}
