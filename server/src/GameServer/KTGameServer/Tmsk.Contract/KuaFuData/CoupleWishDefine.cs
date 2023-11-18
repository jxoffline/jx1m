using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Xml.Linq;

using Server.Tools;
using GameServer.Core.Executor;
using ProtoBuf;

/*
 * 夫妻祝福排行榜定义
 */
namespace KF.Contract.Data
{
    /// <summary>
    /// 夫妻祝福排行榜常量定义
    /// </summary>
    public static class CoupleWishConsts
    {
        // 配置相关
        public static readonly string RankAwardCfgFile = @"Config\WishAward.xml";
        public static readonly string WishTypeCfgFile = @"Config\WishType.xml";
        public static readonly string YanHuiCfgFile = @"Config\WishFeasttAward.xml";

        // 时间配置相关
        public static readonly int AwardWeekday = 7;
        public static readonly long AwardDayStartTicks = TimeSpan.TicksPerDay - 30 * TimeSpan.TicksPerMinute;
        public static readonly long AwardDayEndTicks = TimeSpan.TicksPerDay;

        // 排行榜
        public static readonly int MaxRankNum = 20;

        // 祝福
        public static readonly int MaxWishTxtLen = 15;
        public static readonly int MaxWishRecordNum = 100;

        // 
        public static readonly int NeedDelCoupleIdFlag = -1;
    }

    /// <summary>
    /// 夫妻祝福排行榜 排行、奖励
    /// </summary>
    public class CoupleWishRankAwardConfig
    {
        public int Id; // 流水号
        public int StartRank; // 起始排名
        public int EndRank; // 结束排名
        public object GoodsOneTag; // 全职业奖励
        public object GoodsTwoTag; // 分职业奖励
        public int MinWishNum;  // 最小祝福数
    }

    /// <summary>
    /// 夫妻祝福榜 祝福类型
    /// </summary>
    public class CoupleWishTypeConfig
    {
        public int WishType; // 流水号
        public int CostGoodsId; // 消耗道具id
        public int CostGoodsNum; // 消耗道具个数
        public int CostZuanShi; // 消耗钻石
        public int GetWishNum; // 增加祝福次数
        public int CanHaveWishTxt; // 是否可以有寄语
        public int IsHaveEffect; // 是否有本服特效
        public int CooldownTime; // 全服cd时间
    }

    /// <summary>
    /// 夫妻祝福榜 宴会配置
    /// </summary>
    public class CoupleWishYanHuiConfig
    {
        public int Id; // 流水号
        public int TotalMaxJoinNum; // 总参与次数上限
        public int EachRoleMaxJoinNum; // 每个角色参与次数上限
        public int CostBindJinBi; // 消耗绑金
        public int GetExp; // 奖励经验
        public int GetXingHun; // 奖励星魂
        public int GetShengWang; // 奖励声望
    }

    /// <summary>
    /// 夫妻祝福榜 配置管理
    /// </summary>
    public class CoupleWishConfig
    {
        public readonly List<CoupleWishRankAwardConfig> RankAwardCfgList = new List<CoupleWishRankAwardConfig>();
        public readonly List<CoupleWishTypeConfig> WishTypeCfgList = new List<CoupleWishTypeConfig>();
        public readonly CoupleWishYanHuiConfig YanHuiCfg = new CoupleWishYanHuiConfig();

        public CoupleWishConfig()
        {

        }

        /// <summary>
        /// 加载配置
        /// </summary>
        /// <param name="rankAwardCfgFile"></param>
        /// <param name="wishTypeCfgFile"></param>
        /// <param name="feastCfgFile"></param>
        /// <returns></returns>
        public bool Load(string rankAwardCfgFile, string wishTypeCfgFile, string feastCfgFile)
        {
            return LoadRankAward(rankAwardCfgFile) 
                && LoadWishType(wishTypeCfgFile) 
                && LoadFeast(feastCfgFile);
        }

        /// <summary>
        /// 是否在可赠送时间内
        /// 周一的前10分钟不可赠送 即00:10 - 23:59可赠，防止时间差导致赠送周出错
        /// 周二 --- 周六全天可赠送
        /// 周日的最后30分钟不可赠送，级00:00 - 23:30可增，23:40之后为结算时间，10分钟时间差
        /// 
        /// 可赠送时间内离婚，才会从排行榜移除
        /// </summary>
        /// <param name="time"></param>
        /// <param name="wishWeek"></param>
        /// <returns></returns>
        public bool IsInWishTime(DateTime time, out int wishWeek)
        {
            wishWeek = 0;
            bool isInTime = false;
            int weekday = TimeUtil.GetWeekDay1To7(time);
            if (weekday == 1 && time.TimeOfDay.Ticks >= TimeSpan.TicksPerMinute * 10)
                isInTime = true;
            else if (weekday == 7 && time.TimeOfDay.Ticks <= TimeSpan.TicksPerDay - TimeSpan.TicksPerMinute * 30)
                isInTime = true;
            else if (weekday > 1 && weekday < 7)
                isInTime = true;

            if (isInTime)
                wishWeek = TimeUtil.MakeFirstWeekday(time);

            return isInTime;
        }

        /// <summary>
        /// 是否在领奖时间
        /// </summary>
        /// <param name="time"></param>
        /// <param name="week"></param>
        /// <returns></returns>
        public bool IsInAwardTime(DateTime time, out int awardWeek)
        {
            awardWeek = 0;
            bool isInTime = false;
            int weekday = TimeUtil.GetWeekDay1To7(time);
            if (weekday == 1 && time.TimeOfDay.Ticks >= TimeSpan.TicksPerMinute * 10)
                isInTime = true;
            else if (weekday == 7 && time.TimeOfDay.Ticks <= TimeSpan.TicksPerDay - TimeSpan.TicksPerMinute * 30)
                isInTime = true;
            else if (weekday > 1 && weekday < 7)
                isInTime = true;

            if (isInTime)
                awardWeek = TimeUtil.MakeFirstWeekday(time.AddDays(-7));

            return isInTime;
        }

        #region 辅助函数
        private bool LoadRankAward(string cfgFile)
        {
            try
            {
                RankAwardCfgList.Clear();

                foreach (var xmlItem in XElement.Load(cfgFile).Elements())
                {
                    var cfg = new CoupleWishRankAwardConfig();
                    cfg.Id = Convert.ToInt32(xmlItem.Attribute("ID").Value.ToString());
                    cfg.StartRank = Convert.ToInt32(xmlItem.Attribute("StarNum").Value.ToString());
                    cfg.EndRank = Convert.ToInt32(xmlItem.Attribute("EndNum").Value.ToString());
                    cfg.GoodsOneTag = xmlItem.Attribute("GoodsOne").Value.ToString();
                    cfg.GoodsTwoTag = xmlItem.Attribute("GoodsTwo").Value.ToString();
                    cfg.MinWishNum = Convert.ToInt32(xmlItem.Attribute("MinWishNum").Value.ToString());

                    if (cfg.EndRank <= 0)
                        Debug.Assert(cfg.MinWishNum == 0);
                    RankAwardCfgList.Add(cfg);
                }

                RankAwardCfgList.Sort((_l, _r) => { return _l.Id - _r.Id; });
                for (int i = 0; i < RankAwardCfgList.Count; i++)
                {
                    Debug.Assert(RankAwardCfgList[i].StartRank <= RankAwardCfgList[i].EndRank);
                    if (i > 0)
                    {
                        Debug.Assert(RankAwardCfgList[i].StartRank == RankAwardCfgList[i - 1].EndRank + 1);
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Error, "CoupleWishConfig.LoadRankAward failed", ex);
                return false;
            }

            return true;
        }

        private bool LoadWishType(string cfgFile)
        {
            try
            {
                WishTypeCfgList.Clear();

                foreach (var xmlItem in XElement.Load(cfgFile).Elements())
                {
                    var cfg = new CoupleWishTypeConfig();
                    cfg.WishType = Convert.ToInt32(xmlItem.Attribute("ID").Value.ToString());
                    string[] costFields = xmlItem.Attribute("ItemNum").Value.ToString().Split(',');
                    if (costFields != null && costFields.Length == 2)
                    {
                        cfg.CostGoodsId = Convert.ToInt32(costFields[0]);
                        cfg.CostGoodsNum = Convert.ToInt32(costFields[1]);
                    }
                    cfg.CostZuanShi = Convert.ToInt32(xmlItem.Attribute("ZhuanShi").Value.ToString());
                    cfg.GetWishNum = Convert.ToInt32(xmlItem.Attribute("WishNum").Value.ToString());
                    cfg.CanHaveWishTxt = Convert.ToInt32(xmlItem.Attribute("WishIntro").Value.ToString());
                    cfg.IsHaveEffect = Convert.ToInt32(xmlItem.Attribute("Effect").Value.ToString());
                    cfg.CooldownTime = Convert.ToInt32(xmlItem.Attribute("EffectCD").Value.ToString());

                    WishTypeCfgList.Add(cfg);
                }
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Error, "CoupleWishConfig.LoadWishType failed", ex);
                return false;
            }

            return true;
        }

        private bool LoadFeast(string cfgFile)
        {
            try
            {
                foreach (var xmlItem in XElement.Load(cfgFile).Elements())
                {
                    YanHuiCfg.Id = Convert.ToInt32(xmlItem.Attribute("ID").Value.ToString());
                    YanHuiCfg.TotalMaxJoinNum = Convert.ToInt32(xmlItem.Attribute("SumNum").Value.ToString());
                    YanHuiCfg.EachRoleMaxJoinNum = Convert.ToInt32(xmlItem.Attribute("UseNum").Value.ToString());
                    YanHuiCfg.CostBindJinBi = Convert.ToInt32(xmlItem.Attribute("BindJinBi").Value.ToString());
                    YanHuiCfg.GetExp = Convert.ToInt32(xmlItem.Attribute("EXPAward").Value.ToString());
                    YanHuiCfg.GetXingHun = Convert.ToInt32(xmlItem.Attribute("XingHunAward").Value.ToString());
                    YanHuiCfg.GetShengWang = Convert.ToInt32(xmlItem.Attribute("ShengWangAward").Value.ToString());

                    break; // just use 1 child elem
                }
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Error, "CoupleWishConfig.LoadFeast failed", ex);
                return false;
            }

            return true;
        }
        #endregion
    }

    #region Serializable
    [Serializable]
    public class CoupleWishCoupleDataK : IComparable<CoupleWishCoupleDataK>
    {
        public int DbCoupleId;
        public KuaFuRoleMiniData Man;
        public byte[] ManSelector; // 排行榜形象
        public KuaFuRoleMiniData Wife;
        public byte[] WifeSelector;
        public int BeWishedNum;
        public int Rank;

        public int CompareTo(CoupleWishCoupleDataK other)
        {
            if (this.BeWishedNum > other.BeWishedNum) return -1;
            else if (this.BeWishedNum < other.BeWishedNum) return 1;
            else return this.Man.RoleId - other.Man.RoleId;
        }
    }

    [Serializable]
    public class CoupleWishReportStatueData
    {
        public int DbCoupleId;
        public byte[] ManStatue;
        public byte[] WifeStatue;
    }

    [Serializable]
    public class CoupleWishWishRoleReq
    {
        public KuaFuRoleMiniData From = new KuaFuRoleMiniData();
        public int WishType;
        public string WishTxt;

        /// <summary>
        /// 是否是跨服排行榜祝福
        /// </summary>
        public bool IsWishRank;

        // 跨服赠送, 这个值始终是自增的，可以有效防止赠送的时候，刚好已经离婚了
        public int ToCoupleId;

        // 本服赠送
        public KuaFuRoleMiniData ToMan = new KuaFuRoleMiniData();
        public KuaFuRoleMiniData ToWife = new KuaFuRoleMiniData();
        public byte[] ToManSelector;
        public byte[] ToWifeSelector;
    }

    /// <summary>
    /// 同步的雕像数据
    /// </summary>
    [Serializable]
    public class CoupleWishSyncStatueData
    {
        public DateTime ModifyTime;
        public int DbCoupleId;
        public KuaFuRoleMiniData Man;
        public KuaFuRoleMiniData Wife;
        public byte[] ManRoleDataEx;
        public byte[] WifeRoleDataEx;
        public int IsDivorced;
        public int BeAdmireCount;
        public int YanHuiJoinNum;
        public int Week;

        public CoupleWishSyncStatueData SimpleClone()
        {
            CoupleWishSyncStatueData clone = new CoupleWishSyncStatueData();
            clone.ModifyTime = this.ModifyTime;
            clone.DbCoupleId = this.DbCoupleId;
            clone.Man = this.Man;
            clone.Wife = this.Wife;
            clone.ManRoleDataEx = this.ManRoleDataEx;
            clone.WifeRoleDataEx = this.WifeRoleDataEx;
            clone.IsDivorced = this.IsDivorced;
            clone.BeAdmireCount = this.BeAdmireCount;
            clone.YanHuiJoinNum = this.YanHuiJoinNum;
            clone.Week = this.Week;
            return clone;
        }
    }

    /// <summary>
    /// 每周的同步数据
    /// </summary>
    [Serializable]
    public class CoupleWishSyncWeekData
    {
        public DateTime ModifyTime;
        public int Week;
        public List<CoupleWishCoupleDataK> RankList = new List<CoupleWishCoupleDataK>();
        public Dictionary<int, int> RoleIndex = new Dictionary<int, int>();
        public Dictionary<int, int> CoupleIdex = new Dictionary<int, int>();
        public CoupleWishSyncStatueData StatueData = null;

        public CoupleWishSyncWeekData SimpleClone()
        {
            CoupleWishSyncWeekData clone = new CoupleWishSyncWeekData();
            clone.ModifyTime = this.ModifyTime;
            clone.Week = this.Week;
            clone.RankList.AddRange(this.RankList);
            clone.RoleIndex = new Dictionary<int, int>(this.RoleIndex);
            clone.CoupleIdex = new Dictionary<int, int>(this.CoupleIdex);
            clone.StatueData = this.StatueData != null ? this.StatueData.SimpleClone() : null;
            return clone;
        }

        public void BuildIndex()
        {
            this.RoleIndex.Clear();
            this.CoupleIdex.Clear();
            for (int i = 0; i < RankList.Count; i++)
            {
                this.RoleIndex[this.RankList[i].Man.RoleId] = i;
                this.RoleIndex[this.RankList[i].Wife.RoleId] = i;
                this.CoupleIdex[this.RankList[i].DbCoupleId] = i;
            }
        }
    }

    [Serializable]
    public class CoupleWishSyncData
    {
        public CoupleWishSyncWeekData ThisWeek = new CoupleWishSyncWeekData();
        public CoupleWishSyncWeekData LastWeek = new CoupleWishSyncWeekData();
        public CoupleWishSyncStatueData Statue = new CoupleWishSyncStatueData();
    }

    /// <summary>
    /// 情侣祝福 --- 祝福记录
    /// </summary>
    [ProtoContract]
    [Serializable]
    public class CoupleWishWishRecordData
    {
        /// <summary>
        /// 祝福来源角色, 客户端检测FromRole.RoleId决定是否替换FromRole.RoleName为"你"
        /// 注： 由于允许改名，客户端必须使用roleid判断，不可使用角色名
        /// </summary>
        [ProtoMember(1)]
        public KuaFuRoleMiniData FromRole;

        /// <summary>
        /// 祝福目标角色, 客户端检测TargetRole.RoleId决定是否替换TargetRole.RoleName为"你"
        /// 注： 由于允许改名，客户端必须使用roleid判断，不可使用角色名
        /// 注： 被祝福的可以是跨服排行榜中的一对角色，也可以是本服的一个角色，所以用list
        /// </summary>
        [ProtoMember(2)]
        public List<KuaFuRoleMiniData> TargetRoles;

        /// <summary>
        /// 祝福类型，参考WishType.xml的ID字段
        /// </summary>
        [ProtoMember(3)]
        public int WishType;

        /// <summary>
        /// 祝福寄语
        /// </summary>
        [ProtoMember(4)]
        public string WishTxt;
    }

    #endregion
}
