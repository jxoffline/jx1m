using Server.Data;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;

namespace GameDBServer.Logic
{
    public enum SaleGoodsConsts
    {
        SaleGoodsID = -1,

        PortableGoodsID = -1000,
    }

    public enum FriendsConsts
    {
        MaxFriendsNum = 50,

        MaxBlackListNum = 20,

        MaxEnemiesNum = 20,
    }

    /// <summary>
    ///  Danh sách các hoạt động ghi vào DB
    /// </summary>
    public enum ActivityTypes
    {
        MeiRiChongZhiHaoLi = 27,
        TotalCharge = 38,
        TotalConsume = 39,

        MaxVal,
    }

    /// <summary>
    /// 角色创建常量信息
    /// </summary>
    public enum RoleCreateConstant
    {
        GridNum = 50,//角色创建时背包有50个格子
    }

    public class RoleParamName
    {
        /// <summary>
        /// Lưu thông tin có đang cưỡi ngựa không
        /// </summary>
        public const string HorseToggleOn = "HorseToggleOn";

        /// <summary>
        /// Lưu thông tin người chơi ở phụ bản
        /// </summary>
        public const string CopySceneRecord = "CopySceneRecord";

        /// <summary>
        /// Điểm hồi sinh ở thành thôn tương ứng
        /// </summary>
        public const string DefaultRelivePos = "DefaultRelivePos";

        /// <summary>
        /// Thông tin Chúc phúc
        /// </summary>
        public const string PrayData = "PrayData";

        /// <summary>
        /// Thông tin Tu Luyện Châu
        /// </summary>
        public const string XiuLianZhu = "XiuLianZhu";

        /// <summary>
        /// Tổng thời gian Tu Luyện Châu có
        /// </summary>
        public const string XiuLianZhu_TotalTime = "XiuLianZhu_TotalTime";
    }

    public class RoleParamNameInfo
    {
        private static ReaderWriterLockSlim readerWriterLock = new ReaderWriterLockSlim();

        #region RoleParamNameTypeExtDict

        private static Dictionary<string, RoleParamType> RoleParamNameTypeExtDict = new Dictionary<string, RoleParamType>()
        {
            {"ReturnCode", new RoleParamType("ReturnCode", "ReturnCode", "t_roleparams_2", "pname", "pvalue", 0, 0, 0)},
            {"SpreadCode", new RoleParamType("SpreadCode", "SpreadCode", "t_roleparams_2", "pname", "pvalue", 0, 0, 0)},
            {"VerifyCode", new RoleParamType("VerifyCode", "VerifyCode", "t_roleparams_2", "pname", "pvalue", 0, 0, 0)},

            {"WeekEndInputOD", new RoleParamType("WeekEndInputOpenDay", "WeekEndInputOD", "t_roleparams_2", "pname", "pvalue", 0, 0, 0)},
            {"LangHunLingYuDayAwards", new RoleParamType("LangHunLingYuDayAwardsDay", "LangHunLingYuDayAwards", "t_roleparams_2", "pname", "pvalue", 0, 0, 0)},

            {"MeiRiChongZhiHaoLi1", new RoleParamType("DayGift1", "MeiRiChongZhiHaoLi1", "t_roleparams_2", "pname", "pvalue", 0, 0, 0)},
            {"MeiRiChongZhiHaoLi2", new RoleParamType("DayGift2", "MeiRiChongZhiHaoLi2", "t_roleparams_2", "pname", "pvalue", 0, 0, 0)},
            {"MeiRiChongZhiHaoLi3", new RoleParamType("DayGift3", "MeiRiChongZhiHaoLi3", "t_roleparams_2", "pname", "pvalue", 0, 0, 0)},

            {"ZiKaDayID", new RoleParamType("ZiKaDayID", "ZiKaDayID", "t_roleparams_2", "pname", "pvalue", 0, 0, -1)},
            {"ZiKaDayNum", new RoleParamType("ZiKaDayNum", "ZiKaDayNum", "t_roleparams_2", "pname", "pvalue", 0, 0, -1)},
            {"PictureJudgeFlags", new RoleParamType("PictureJudgeFlags", "PictureJudgeFlags", "t_roleparams_2", "pname", "pvalue", 0, 0, -1)},
            {"BloodCastleSceneTimer", new RoleParamType("BloodCastleSceneTimer", "BloodCastleSceneTimer", "t_roleparams_2", "pname", "pvalue", 0, 0, -1)},
            {"TreasureLogUpdate0", new RoleParamType("TreasureLogUpdate0", "TreasureLogUpdate0", "t_roleparams_2", "pname", "pvalue", 0, 0, -1)},
            {"BuildLogUpdate", new RoleParamType("BuildLogUpdate", "BuildLogUpdate", "t_roleparams_2", "pname", "pvalue", 0, 0, -1)},
            {"BuildLogUpdate0", new RoleParamType("BuildLogUpdate0", "BuildLogUpdate0", "t_roleparams_2", "pname", "pvalue", 0, 0, -1)},
            {"BuildLogUpdate2", new RoleParamType("BuildLogUpdate2", "BuildLogUpdate2", "t_roleparams_2", "pname", "pvalue", 0, 0, -1)},
            {"BuildLogUpdate4", new RoleParamType("BuildLogUpdate4", "BuildLogUpdate4", "t_roleparams_2", "pname", "pvalue", 0, 0, -1)},

            {"ChangeLifeCount", new RoleParamType("ChangeLifeCount", "ChangeLifeCount", "t_roleparams_2", "pname", "pvalue", 0, 0, -1)},
            {"YongZheZhanChangGroupId", new RoleParamType("YongZheZhanChangGroupId", "YongZheZhanChangGroupId", "t_roleparams_2", "pname", "pvalue", 0, 0, -1)},


            {"InputPointExchg", new RoleParamType("InputPointExchg", "InputPointExchg", "t_roleparams_2", "pname", "pvalue", 0, 0, -2)},
            {"JRExcharge", new RoleParamType("JRExcharge", "JRExcharge", "t_roleparams_2", "pvalue", "pname", 0, 0, -2)},
            {"KaiFuOnlineDayTimes_", new RoleParamType("KaiFuOnlineDayTimes_", "KaiFuOnlineDayTimes_", "t_roleparams_2", "pname", "pvalue", 0, 0, -2)},
            {"MapLimitSecs_", new RoleParamType("MapLimitSecs", "MapLimitSecs_", "t_roleparams_2", "pname", "pvalue", 0, 0, -2)},
            {"MeiRiChongZhiHaoLi", new RoleParamType("MeiRiChongZhiHaoLi", "MeiRiChongZhiHaoLi", "t_roleparams_2", "pname", "pvalue", 0, 0, -2)},
        };

        #endregion RoleParamNameTypeExtDict

        #region RoleParamNameTypeDict

        private static Dictionary<string, RoleParamType> OldRoleParamNameTypeDict = new Dictionary<string, RoleParamType>();

        private static Dictionary<string, RoleParamType> RoleParamNameTypeDict = new Dictionary<string, RoleParamType>()
        {
            // common type

            #region KIEMTHE

            {"LifeSkill", new RoleParamType("LifeSkill", "LifeSkill", "t_roleparams_2", "pname", "pvalue", -1, -1, 0)},
            {"AutoSettings", new RoleParamType("AutoSettings", "AutoSettings", "t_roleparams_2", "pname", "pvalue", -1, -1, 0)},
            {"SystemSettings", new RoleParamType("SystemSettings", "SystemSettings", "t_roleparams_2", "pname", "pvalue", -1, -1, 0)},
            {"RoleTitles", new RoleParamType("RoleTitles", "RoleTitles", "t_roleparams_2", "pname", "pvalue", -1, -1, 0)},
            {"ReputeInfo", new RoleParamType("ReputeInfo", "ReputeInfo", "t_roleparams_2", "pname", "pvalue", -1, -1, 0)},
            {"DailyRecore", new RoleParamType("DailyRecore", "DailyRecore", "t_roleparams_2", "pname", "pvalue", -1, -1, 0)},
            {"WeekRecore", new RoleParamType("WeekRecore", "WeekRecore", "t_roleparams_2", "pname", "pvalue", -1, -1, 0)},
            {"ForeverRecore", new RoleParamType("ForeverRecore", "ForeverRecore", "t_roleparams_2", "pname", "pvalue", -1, -1, 0)},

            #endregion KIEMTHE

            {"KaiFuOnlineDayTimes_", new RoleParamType("KaiFuOnlineDayTimes", "KaiFuOnlineDayTimes_", "t_roleparams_2", "pname", "pvalue", -1, -1, 0)},
            {"MeiRiChongZhiHaoLi1", new RoleParamType("DayGift1", "MeiRiChongZhiHaoLi1", "t_roleparams_2", "pname", "pvalue", 0, 0, 0)},
            {"MeiRiChongZhiHaoLi2", new RoleParamType("DayGift2", "MeiRiChongZhiHaoLi2", "t_roleparams_2", "pname", "pvalue", 0, 0, 0)},
            {"MeiRiChongZhiHaoLi3", new RoleParamType("DayGift3", "MeiRiChongZhiHaoLi3", "t_roleparams_2", "pname", "pvalue", 0, 0, 0)},
            {"ReturnCode", new RoleParamType("ReturnCode", "ReturnCode", "t_roleparams_2", "pname", "pvalue", 0, 0, 0)},
            {"SpreadCode", new RoleParamType("SpreadCode", "SpreadCode", "t_roleparams_2", "pname", "pvalue", 0, 0, 0)},
            {"VerifyCode", new RoleParamType("VerifyCode", "VerifyCode", "t_roleparams_2", "pname", "pvalue", 0, 0, 0)},
            {"ZiKaDayNum", new RoleParamType("ZiKaDayNum", "ZiKaDayNum", "t_roleparams_2", "pname", "pvalue", 0, 0, -1)},
            {"ZiKaDayID", new RoleParamType("ZiKaDayID", "ZiKaDayID", "t_roleparams_2", "pname", "pvalue", 0, 0, -1)},
            {"BuildLogUpdate", new RoleParamType("BuildLogUpdate", "BuildLogUpdate", "t_roleparams_2", "pname", "pvalue", 0, 0, -1)},
            {"PictureJudgeFlags", new RoleParamType("PictureJudgeFlags", "PictureJudgeFlags", "t_roleparams_2", "pname", "pvalue", 0, 0, -1)},
            {"MapLimitSecs_", new RoleParamType("MapLimitSecs", "MapLimitSecs_", "t_roleparams_2", "pname", "pvalue", 0, 0, -2)},
            {"JRExcharge", new RoleParamType("JRExcharge", "JRExcharge", "t_roleparams_2", "pvalue", "pname", 0, 0, -2)},
            {"InputPointExchg", new RoleParamType("InputPointExchg", "InputPointExchg", "t_roleparams_2", "pname", "pvalue", 0, 0, -2)},
#if 移植
            {"ShiPinLev", new RoleParamType("ShiPinLev", "ShiPinLev", "t_roleparams_2", "pname", "pvalue", 0, 0, 0)},
            {"MeiLi", new RoleParamType("MeiLi", "MeiLi", "t_roleparams_2", "pname", "pvalue", 0, 0, 0)},
            {"ShenQi", new RoleParamType("ShenQi", "ShenQi", "t_roleparams_2", "pname", "pvalue", 0, 0, 0)},
            {"ShenLiJingHua", new RoleParamType("ShenLiJingHua", "ShenLiJingHua", "t_roleparams_2", "pname", "pvalue", 0, 0, 0)},
            {"ElementhBag", new RoleParamType("ElementhBag", "ElementhBag", "t_roleparams_2", "pname", "pvalue", 0, 0, 0)},
#endif
			/// type1
			{"ChengJiuData", new RoleParamType("ChengJiuExtraData", "ChengJiuData", "t_roleparams_char", "idx", "v0", 0, 0, 1)},
            {"DailyActiveFlag", new RoleParamType("DailyActiveFlag", "DailyActiveFlag", "t_roleparams_char", "idx", "v1", 0, 1, 1)},
            {"DailyActiveInfo1", new RoleParamType("DailyActiveInfo1", "DailyActiveInfo1", "t_roleparams_char", "idx", "v2", 0, 2, 1)},
            {"RoleLoginRecorde", new RoleParamType("RoleLoginRecorde", "RoleLoginRecorde", "t_roleparams_char", "idx", "v3", 0, 3, 1)},
            {"ChengJiuFlags", new RoleParamType("ChengJiuFlags", "ChengJiuFlags", "t_roleparams_char", "idx", "v4", 0, 4, 1)},
            {"UpLevelGiftFlags", new RoleParamType("UpLevelGiftFlags", "UpLevelGiftFlags", "t_roleparams_char", "idx", "v5", 0, 5, 1)},
            {"AchievementRune", new RoleParamType("AchievementRune", "AchievementRune", "t_roleparams_char", "idx", "v6", 0, 6, 1)},
            {"AchievementRuneUpCount", new RoleParamType("AchievementRuneUpCount", "AchievementRuneUpCount", "t_roleparams_char", "idx", "v7", 0, 7, 1)},
            {"DailyShare", new RoleParamType("DailyShare", "DailyShare", "t_roleparams_char", "idx", "v8", 0, 8, 1)},
            {"WeekEndInputFlag", new RoleParamType("WeekEndInputFlag", "WeekEndInputFlag", "t_roleparams_char", "idx", "v9", 0, 9, 1)},

            {"MapPosRecord", new RoleParamType("MapPosRecord", "MapPosRecord", "t_roleparams_char", "idx", "v0", 10, 10, 1)},
            {"ChongJiGiftList", new RoleParamType("ChongJiGiftList", "ChongJiGiftList", "t_roleparams_char", "idx", "v1", 10, 11, 1)},
            {"DailyChargeGiftFlags", new RoleParamType("DailyChargeGiftFlags", "DailyChargeGiftFlags", "t_roleparams_char", "idx", "v2", 10, 12, 1)},
            {"YueKaInfo", new RoleParamType("YueKaInfo", "YueKaInfo", "t_roleparams_char", "idx", "v3", 10, 13, 1)},
            {"TotalCostMoney", new RoleParamType("TotalCostMoney", "TotalCostMoney", "t_roleparams_char", "idx", "v4", 10, 14, 1)},
            {"PrestigeMedal", new RoleParamType("PrestigeMedal", "PrestigeMedal", "t_roleparams_char", "idx", "v5", 10, 15, 1)},
            {"PrestigeMedalUpCount", new RoleParamType("PrestigeMedalUpCount", "PrestigeMedalUpCount", "t_roleparams_char", "idx", "v6", 10, 16, 1)},
            {"TreasureData", new RoleParamType("TreasureData", "TreasureData", "t_roleparams_char", "idx", "v7", 10, 17, 1)},
            {"QingGongYanJoinFlag", new RoleParamType("QingGongYanJoinFlag", "QingGongYanJoinFlag", "t_roleparams_char", "idx", "v8", 10, 18, 1)},
            {"YongZheZhanChangAwards", new RoleParamType("YongZheZhanChangAwards", "YongZheZhanChangAwards", "t_roleparams_char", "idx", "v9", 10, 19, 1)},

            {"BuildQueueData", new RoleParamType("BuildQueueData", "BuildQueueData", "t_roleparams_char", "idx", "v0", 20, 20, 1)},
            {"BuildAllLevAward", new RoleParamType("BuildAllLevAward", "BuildAllLevAward", "t_roleparams_char", "idx", "v1", 20, 21, 1)},
            {"UnionPalaceUpCount", new RoleParamType("UnionPalaceUpCount", "UnionPalaceUpCount", "t_roleparams_char", "idx", "v2", 20, 22, 1)},
            {"UnionPalace", new RoleParamType("UnionPalace", "UnionPalace", "t_roleparams_char", "idx", "v3", 20, 23, 1)},
            {"PetSkillUpCount", new RoleParamType("PetSkillUpCount", "PetSkillUpCount", "t_roleparams_char", "idx", "v4", 20, 24, 1)},
            {"EnterKuaFuMapFlag", new RoleParamType("EnterKuaFuMapFlag", "EnterKuaFuMapFlag", "t_roleparams_char", "idx", "v5", 20, 25, 1)},
            {"SiegeWarfareEveryDayAwardDayID", new RoleParamType("SiegeWarfareEveryDayAwardDayID", "SiegeWarfareEveryDayAwardDayID", "t_roleparams_char", "idx", "v6", 20, 26, 1)},

#region Kiếm Thế

            {RoleParamName.PrayData, new RoleParamType(RoleParamName.PrayData, RoleParamName.PrayData, "t_roleparams_char", "idx", "v7", 20, 27, 1)},
            {RoleParamName.CopySceneRecord, new RoleParamType(RoleParamName.CopySceneRecord, RoleParamName.CopySceneRecord, "t_roleparams_char", "idx", "v1", 30, 29, 1)},
            {RoleParamName.DefaultRelivePos, new RoleParamType(RoleParamName.DefaultRelivePos, RoleParamName.DefaultRelivePos, "t_roleparams_char", "idx", "v2", 30, 30, 1)},

#endregion Kiếm Thế

            /// type2
            {"AddProPointForLevelUp", new RoleParamType("AddProPointForLevelUp", "AddProPointForLevelUp", "t_roleparams_long", "idx", "v0", 10000, 10000, 2)},
            {"AdmireCount", new RoleParamType("AdmireCount", "AdmireCount", "t_roleparams_long", "idx", "v1", 10000, 10001, 2)},
            {"AdmireDayID", new RoleParamType("AdmireDayID", "AdmireDayID", "t_roleparams_long", "idx", "v2", 10000, 10002, 2)},
            {"DailyActiveDayID", new RoleParamType("DailyActiveDayID", "DailyActiveDayID", "t_roleparams_long", "idx", "v3", 10000, 10003, 2)},
            {"PKKingAdmireCount", new RoleParamType("PKKingAdmireCount", "PKKingAdmireCount", "t_roleparams_long", "idx", "v4", 10000, 10004, 2)},
            {"PKKingAdmireDayID", new RoleParamType("PKKingAdmireDayID", "PKKingAdmireDayID", "t_roleparams_long", "idx", "v5", 10000, 10005, 2)},
            {"DailyActiveAwardFlag", new RoleParamType("DailyActiveAwardFlag", "DailyActiveAwardFlag", "t_roleparams_long", "idx", "v6", 10000, 10006, 2)},
            {"PropConstitution", new RoleParamType("sPropConstitution", "PropConstitution", "t_roleparams_long", "idx", "v7", 10000, 10007, 2)},
            {"PropDexterity", new RoleParamType("sPropDexterity", "PropDexterity", "t_roleparams_long", "idx", "v8", 10000, 10008, 2)},
            {"PropIntelligence", new RoleParamType("sPropIntelligence", "PropIntelligence", "t_roleparams_long", "idx", "v9", 10000, 10009, 2)},
            {"PropStrength", new RoleParamType("sPropStrength", "PropStrength", "t_roleparams_long", "idx", "v10", 10000, 10010, 2)},
            {"MeditateTime", new RoleParamType("MeditateTime", "MeditateTime", "t_roleparams_long", "idx", "v11", 10000, 10011, 2)},
            {"DefaultSkillLev", new RoleParamType("DefaultSkillLev", "DefaultSkillLev", "t_roleparams_long", "idx", "v13", 10000, 10013, 2)},
            {"DefaultSkillUseNum", new RoleParamType("DefaultSkillUseNum", "DefaultSkillUseNum", "t_roleparams_long", "idx", "v14", 10000, 10014, 2)},
            {"CurHP", new RoleParamType("CurHP", "CurHP", "t_roleparams_long", "idx", "v15", 10000, 10015, 2)},
            {"CurMP", new RoleParamType("CurMP", "CurMP", "t_roleparams_long", "idx", "v16", 10000, 10016, 2)},
            {"DayOnlineSecond", new RoleParamType("DayOnlineSecond", "DayOnlineSecond", "t_roleparams_long", "idx", "v17", 10000, 10017, 2)},
            {"NotSafeMeditateTime", new RoleParamType("NotSafeMeditateTime", "NotSafeMeditateTime", "t_roleparams_long", "idx", "v18", 10000, 10018, 2)},
            {"SeriesLoginCount", new RoleParamType("SeriesLoginCount", "SeriesLoginCount", "t_roleparams_long", "idx", "v19", 10000, 10019, 2)},
            {"OpenGridTick", new RoleParamType("OpenGridTick", "OpenGridTick", "t_roleparams_long", "idx", "v20", 10000, 10020, 2)},
            {"OpenPortableGridTick", new RoleParamType("OpenPortableGridTick", "OpenPortableGridTick", "t_roleparams_long", "idx", "v21", 10000, 10021, 2)},
            {"GuMuAwardDayID", new RoleParamType("GuMuAwardDayID", "GuMuAwardDayID", "t_roleparams_long", "idx", "v22", 10000, 10022, 2)},
            {"FightGetThings", new RoleParamType("FightGetThings", "FightGetThings", "t_roleparams_long", "idx", "v23", 10000, 10023, 2)},
            {"JieriLoginDayID", new RoleParamType("JieriLoginDayID", "JieriLoginDayID", "t_roleparams_long", "idx", "v24", 10000, 10024, 2)},
            {"JieriLoginNum", new RoleParamType("JieriLoginNum", "JieriLoginNum", "t_roleparams_long", "idx", "v25", 10000, 10025, 2)},
            {"VerifyBuffProp", new RoleParamType("VerifyBuffProp", "VerifyBuffProp", "t_roleparams_long", "idx", "v26", 10000, 10026, 2)},
            {"CallPetFreeTime", new RoleParamType("CallPetFreeTime", "CallPetFreeTime", "t_roleparams_long", "idx", "v27", 10000, 10027, 2)},
            {"MaxTongQianNum", new RoleParamType("MaxTongQianNum", "MaxTongQianNum", "t_roleparams_long", "idx", "v28", 10000, 10028, 2)},
            {"ShengWang", new RoleParamType("ShengWang", "ShengWang", "t_roleparams_long", "idx", "v29", 10000, 10029, 2)},
            {"TianDiJingYuan", new RoleParamType("TianDiJingYuan", "TianDiJingYuan", "t_roleparams_long", "idx", "v30", 10000, 10030, 2)},
            {"To60or100", new RoleParamType("To60or100", "To60or100", "t_roleparams_long", "idx", "v31", 10000, 10031, 2)},
            {"StarSoul", new RoleParamType("StarSoul", "StarSoul", "t_roleparams_long", "idx", "v32", 10000, 10032, 2)},
            {"TotalLoginAwardFlag", new RoleParamType("TotalLoginAwardFlag", "TotalLoginAwardFlag", "t_roleparams_long", "idx", "v33", 10000, 10033, 2)},
            {"TianTiDayScore", new RoleParamType("TianTiDayScore", "TianTiDayScore", "t_roleparams_long", "idx", "v34", 10000, 10034, 2)},
            {"ImpetrateTime", new RoleParamType("ImpetrateTime", "ImpetrateTime", "t_roleparams_long", "idx", "v35", 10000, 10035, 2)},
            {"ChengJiuLevel", new RoleParamType("ChengJiuLevel", "ChengJiuLevel", "t_roleparams_long", "idx", "v36", 10000, 10036, 2)},
            {"DaimonSquareSceneFinishFlag", new RoleParamType("DaimonSquareSceneFinishFlag", "DaimonSquareSceneFinishFlag", "t_roleparams_long", "idx", "v37", 10000, 10037, 2)},
            {"DaimonSquareSceneid", new RoleParamType("DaimonSquareSceneid", "DaimonSquareSceneid", "t_roleparams_long", "idx", "v38", 10000, 10038, 2)},
            {"DaimonSquareSceneTimer", new RoleParamType("DaimonSquareSceneTimer", "DaimonSquareSceneTimer", "t_roleparams_long", "idx", "v39", 10000, 10039, 2)},
            {"DaimonSquarePlayerPoint", new RoleParamType("DaimonSquarePlayerPoint", "DaimonSquarePlayerPoint", "t_roleparams_long", "idx", "v0", 10040, 10040, 2)},
            {"PropIntelligenceChangeless", new RoleParamType("sPropIntelligenceChangeless", "PropIntelligenceChangeless", "t_roleparams_long", "idx", "v1", 10040, 10041, 2)},
            {"BuChangFlag", new RoleParamType("BuChangFlag", "BuChangFlag", "t_roleparams_long", "idx", "v2", 10040, 10042, 2)},
            {"PropDexterityChangeless", new RoleParamType("sPropDexterityChangeless", "PropDexterityChangeless", "t_roleparams_long", "idx", "v3", 10040, 10043, 2)},
            {"PropConstitutionChangeless", new RoleParamType("sPropConstitutionChangeless", "PropConstitutionChangeless", "t_roleparams_long", "idx", "v4", 10040, 10044, 2)},
            {"DaimonSquareFuBenSeqID", new RoleParamType("DaimonSquareFuBenSeqID", "DaimonSquareFuBenSeqID", "t_roleparams_long", "idx", "v5", 10040, 10045, 2)},
            {"PropStrengthChangeless", new RoleParamType("sPropStrengthChangeless", "PropStrengthChangeless", "t_roleparams_long", "idx", "v6", 10040, 10046, 2)},
            {"ShengWangLevel", new RoleParamType("ShengWangLevel", "ShengWangLevel", "t_roleparams_long", "idx", "v7", 10040, 10047, 2)},
            {"MUMoHe", new RoleParamType("MUMoHe", "MUMoHe", "t_roleparams_long", "idx", "v8", 10040, 10048, 2)},
            {"WanMoTaCurrLayerOrder", new RoleParamType("WanMoTaCurrLayerOrder", "WanMoTaCurrLayerOrder", "t_roleparams_long", "idx", "v9", 10040, 10049, 2)},
            {"LeftFreeChangeNameTimes", new RoleParamType("LeftFreeChangeNameTimes", "LeftFreeChangeNameTimes", "t_roleparams_long", "idx", "v10", 10040, 10050, 2)},
            {"BloodCastleSceneFinishFlag", new RoleParamType("BloodCastleSceneFinishFlag", "BloodCastleSceneFinishFlag", "t_roleparams_long", "idx", "v11", 10040, 10051, 2)},
            {"BloodCastleSceneid", new RoleParamType("BloodCastleSceneid", "BloodCastleSceneid", "t_roleparams_long", "idx", "v12", 10040, 10052, 2)},
            {"BloodCastlePlayerPoint", new RoleParamType("BloodCastlePlayerPoint", "BloodCastlePlayerPoint", "t_roleparams_long", "idx", "v13", 10040, 10053, 2)},
            {"ElementPowder", new RoleParamType("ElementPowderCount", "ElementPowder", "t_roleparams_long", "idx", "v14", 10040, 10054, 2)},
            {"CaiJiCrystalDayID", new RoleParamType("CaiJiCrystalDayID", "CaiJiCrystalDayID", "t_roleparams_long", "idx", "v15", 10040, 10055, 2)},
            {"CaiJiCrystalNum", new RoleParamType("CaiJiCrystalNum", "CaiJiCrystalNum", "t_roleparams_long", "idx", "v16", 10040, 10056, 2)},
            {"LianZhiJinBiCount", new RoleParamType("LianZhiJinBiCount", "LianZhiJinBiCount", "t_roleparams_long", "idx", "v17", 10040, 10057, 2)},
            {"LianZhiJinBiDayID", new RoleParamType("LianZhiJinBiDayID", "LianZhiJinBiDayID", "t_roleparams_long", "idx", "v18", 10040, 10058, 2)},
            {"FTFTradeCount", new RoleParamType("FTFTradeCount", "FTFTradeCount", "t_roleparams_long", "idx", "v19", 10040, 10059, 2)},
            {"FTFTradeDayID", new RoleParamType("FTFTradeDayID", "FTFTradeDayID", "t_roleparams_long", "idx", "v20", 10040, 10060, 2)},
            {"ZhuangBeiJiFen", new RoleParamType("ZhuangBeiJiFen", "ZhuangBeiJiFen", "t_roleparams_long", "idx", "v21", 10040, 10061, 2)},
            {"LieShaZhi", new RoleParamType("LieShaZhi", "LieShaZhi", "t_roleparams_long", "idx", "v22", 10040, 10062, 2)},
            {"WuXingZhi", new RoleParamType("WuXingZhi", "WuXingZhi", "t_roleparams_long", "idx", "v23", 10040, 10063, 2)},
            {"ZhenQiZhi", new RoleParamType("ZhenQiZhi", "ZhenQiZhi", "t_roleparams_long", "idx", "v24", 10040, 10064, 2)},
            {"ShiLianLing", new RoleParamType("ShiLianLing", "ShiLianLing", "t_roleparams_long", "idx", "v25", 10040, 10065, 2)},
            {"JingMaiLevel", new RoleParamType("JingMaiLevel", "JingMaiLevel", "t_roleparams_long", "idx", "v26", 10040, 10066, 2)},
            {"WuXueLevel", new RoleParamType("WuXueLevel", "WuXueLevel", "t_roleparams_long", "idx", "v27", 10040, 10067, 2)},
            {"ZuanHuangLevel", new RoleParamType("ZuanHuangLevel", "ZuanHuangLevel", "t_roleparams_long", "idx", "v28", 10040, 10068, 2)},
            {"ZHAwardTime", new RoleParamType("ZuanHuangAwardTime", "ZHAwardTime", "t_roleparams_long", "idx", "v29", 10040, 10069, 2)},
            {"SystemOpenValue", new RoleParamType("SystemOpenValue", "SystemOpenValue", "t_roleparams_long", "idx", "v30", 10040, 10070, 2)},
            {"JunGong", new RoleParamType("JunGong", "JunGong", "t_roleparams_long", "idx", "v31", 10040, 10071, 2)},
            {"BossFuBenNum", new RoleParamType("BossFuBenExtraEnterNum", "BossFuBenNum", "t_roleparams_long", "idx", "v32", 10040, 10072, 2)},
            {"KaiFuOnlineDayID", new RoleParamType("KaiFuOnlineDayID", "KaiFuOnlineDayID", "t_roleparams_long", "idx", "v33", 10040, 10073, 2)},
            {"KaiFuOnlineDayBit", new RoleParamType("KaiFuOnlineDayBit", "KaiFuOnlineDayBit", "t_roleparams_long", "idx", "v34", 10040, 10074, 2)},
            {"FreeCSNum", new RoleParamType("FreeCSNum", "FreeCSNum", "t_roleparams_long", "idx", "v35", 10040, 10075, 2)},
            {"FreeCSDayID", new RoleParamType("FreeCSDayID", "FreeCSDayID", "t_roleparams_long", "idx", "v36", 10040, 10076, 2)},
            {"ErGuoTouNum", new RoleParamType("ErGuoTouNum", "ErGuoTouNum", "t_roleparams_long", "idx", "v37", 10040, 10077, 2)},
            {"ErGuoTouDayID", new RoleParamType("ErGuoTouDayID", "ErGuoTouDayID", "t_roleparams_long", "idx", "v38", 10040, 10078, 2)},
            {"ZhanHun", new RoleParamType("ZhanHun", "ZhanHun", "t_roleparams_long", "idx", "v39", 10040, 10079, 2)},
            {"RongYu", new RoleParamType("RongYu", "RongYu", "t_roleparams_long", "idx", "v0", 10080, 10080, 2)},
            {"ZhanHunLevel", new RoleParamType("ZhanHunLevel", "ZhanHunLevel", "t_roleparams_long", "idx", "v1", 10080, 10081, 2)},
            {"RongYuLevel", new RoleParamType("RongYuLevel", "RongYuLevel", "t_roleparams_long", "idx", "v2", 10080, 10082, 2)},
            {"ZJDJiFen", new RoleParamType("ZJDJiFen", "ZJDJiFen", "t_roleparams_long", "idx", "v3", 10080, 10083, 2)},
            {"ZJDJiFenDayID", new RoleParamType("ZJDJiFenDayID", "ZJDJiFenDayID", "t_roleparams_long", "idx", "v4", 10080, 10084, 2)},
            {"ZJDJiFenBits", new RoleParamType("ZJDJiFenBits", "ZJDJiFenBits", "t_roleparams_long", "idx", "v5", 10080, 10085, 2)},
            {"ZJDJiFenBitsDayID", new RoleParamType("ZJDJiFenBitsDayID", "ZJDJiFenBitsDayID", "t_roleparams_long", "idx", "v6", 10080, 10086, 2)},
            {"FuHuoJieZhiCD", new RoleParamType("FuHuoJieZhiCD", "FuHuoJieZhiCD", "t_roleparams_long", "idx", "v7", 10080, 10087, 2)},
            {"VIPExp", new RoleParamType("VIPExp", "VIPExp", "t_roleparams_long", "idx", "v8", 10080, 10088, 2)},
            {"BloodCastleFuBenSeqID", new RoleParamType("BloodCastleFuBenSeqID", "BloodCastleFuBenSeqID", "t_roleparams_long", "idx", "v9", 10080, 10089, 2)},
            {"LiXianBaiTanTicks", new RoleParamType("LiXianBaiTanTicks", "LiXianBaiTanTicks", "t_roleparams_long", "idx", "v10", 10080, 10090, 2)},
            {"LianZhiBangZuanCount", new RoleParamType("LianZhiBangZuanCount", "LianZhiBangZuanCount", "t_roleparams_long", "idx", "v11", 10080, 10091, 2)},
            {"LianZhiZuanShiCount", new RoleParamType("LianZhiZuanShiCount", "LianZhiZuanShiCount", "t_roleparams_long", "idx", "v12", 10080, 10092, 2)},
            {"LianZhiBangZuanDayID", new RoleParamType("LianZhiBangZuanDayID", "LianZhiBangZuanDayID", "t_roleparams_long", "idx", "v13", 10080, 10093, 2)},
            {"LianZhiZuanShiDayID", new RoleParamType("LianZhiZuanShiDayID", "LianZhiZuanShiDayID", "t_roleparams_long", "idx", "v14", 10080, 10094, 2)},
            {"HeFuLoginFlag", new RoleParamType("HeFuLoginFlag", "HeFuLoginFlag", "t_roleparams_long", "idx", "v15", 10080, 10095, 2)},
            {"HeFuTotalLoginDay", new RoleParamType("HeFuTotalLoginDay", "HeFuTotalLoginDay", "t_roleparams_long", "idx", "v16", 10080, 10096, 2)},
            {"HeFuTotalLoginNum", new RoleParamType("HeFuTotalLoginNum", "HeFuTotalLoginNum", "t_roleparams_long", "idx", "v17", 10080, 10097, 2)},
            {"HeFuTotalLoginFlag", new RoleParamType("HeFuTotalLoginFlag", "HeFuTotalLoginFlag", "t_roleparams_long", "idx", "v18", 10080, 10098, 2)},
            {"HeFuPKKingFlag", new RoleParamType("HeFuPKKingFlag", "HeFuPKKingFlag", "t_roleparams_long", "idx", "v19", 10080, 10099, 2)},
            {"GuildCopyMapAwardDay", new RoleParamType("GuildCopyMapAwardDay", "GuildCopyMapAwardDay", "t_roleparams_long", "idx", "v20", 10080, 10100, 2)},
            {"GuildCopyMapAwardFlag", new RoleParamType("GuildCopyMapAwardFlag", "GuildCopyMapAwardFlag", "t_roleparams_long", "idx", "v21", 10080, 10101, 2)},
            {"ElementGrade", new RoleParamType("ElementGrade", "ElementGrade", "t_roleparams_long", "idx", "v22", 10080, 10102, 2)},
            {"PetJiFen", new RoleParamType("PetJiFen", "PetJiFen", "t_roleparams_long", "idx", "v23", 10080, 10103, 2)},
            {"FashionWingsID", new RoleParamType("FashionWingsID", "FashionWingsID", "t_roleparams_long", "idx", "v24", 10080, 10104, 2)},
            {"FashionTitleID", new RoleParamType("FashionTitleID", "FashionTitleID", "t_roleparams_long", "idx", "v25", 10080, 10105, 2)},
            {"ArtifactFailCount", new RoleParamType("ArtifactFailCount", "ArtifactFailCount", "t_roleparams_long", "idx", "v26", 10080, 10106, 2)},
            {"ZaiZaoPoint", new RoleParamType("ZaiZaoPoint", "ZaiZaoPoint", "t_roleparams_long", "idx", "v27", 10080, 10107, 2)},
            {"LLCZAdmireCount", new RoleParamType("LLCZAdmireCount", "LLCZAdmireCount", "t_roleparams_long", "idx", "v28", 10080, 10108, 2)},
            {"LLCZAdmireDayID", new RoleParamType("LLCZAdmireDayID", "LLCZAdmireDayID", "t_roleparams_long", "idx", "v29", 10080, 10109, 2)},
            {"HysySuccessCount", new RoleParamType("HysySuccessCount", "HysySuccessCount", "t_roleparams_long", "idx", "v30", 10080, 10110, 2)},
            {"HysySuccessDayId", new RoleParamType("HysySuccessDayId", "HysySuccessDayId", "t_roleparams_long", "idx", "v31", 10080, 10111, 2)},
            {"HysyYTDSuccessCount", new RoleParamType("HysyYTDSuccessCount", "HysyYTDSuccessCount", "t_roleparams_long", "idx", "v32", 10080, 10112, 2)},
            {"HysyYTDSuccessDayId", new RoleParamType("HysyYTDSuccessDayId", "HysyYTDSuccessDayId", "t_roleparams_long", "idx", "v33", 10080, 10113, 2)},
            {"SaleTradeDayID", new RoleParamType("SaleTradeDayID", "SaleTradeDayID", "t_roleparams_long", "idx", "v34", 10080, 10114, 2)},
            {"SaleTradeCount", new RoleParamType("SaleTradeCount", "SaleTradeCount", "t_roleparams_long", "idx", "v35", 10080, 10115, 2)},
            {"HeFuLuoLanAwardFlag", new RoleParamType("HeFuLuoLanAwardFlag", "HeFuLuoLanAwardFlag", "t_roleparams_long", "idx", "v36", 10080, 10116, 2)},
            {"LHLYAdmireCount", new RoleParamType("LHLYAdmireCount", "LHLYAdmireCount", "t_roleparams_long", "idx", "v37", 10080, 10117, 2)},
            {"LHLYAdmireDayID", new RoleParamType("LHLYAdmireDayID", "LHLYAdmireDayID", "t_roleparams_long", "idx", "v38", 10080, 10118, 2)},
            {"ZhengBaPoint", new RoleParamType("ZhengBaPoint", "ZhengBaPoint", "t_roleparams_long", "idx", "v39", 10080, 10119, 2)},
            {"ZhengBaAwardFlag", new RoleParamType("ZhengBaAwardFlag", "ZhengBaAwardFlag", "t_roleparams_long", "idx", "v0", 10120, 10120, 2)},
            {"ZhengBaHintFlag", new RoleParamType("ZhengBaHintFlag", "ZhengBaHintFlag", "t_roleparams_long", "idx", "v1", 10120, 10121, 2)},
            {"ZhengBaJoinIconFlag", new RoleParamType("ZhengBaJoinIconFlag", "ZhengBaJoinIconFlag", "t_roleparams_long", "idx", "v2", 10120, 10122, 2)},
            {"BanRobotCount", new RoleParamType("BanRobotCount", "BanRobotCount", "t_roleparams_long", "idx", "v3", 10120, 10123, 2)},
            {"TreasureJiFen", new RoleParamType("TreasureJiFen", "TreasureJiFen", "t_roleparams_long", "idx", "v4", 10120, 10124, 2)},
            {"TreasureXueZuan", new RoleParamType("TreasureXueZuan", "TreasureXueZuan", "t_roleparams_long", "idx", "v5", 10120, 10125, 2)},
            {"WeekEndInputOD", new RoleParamType("WeekEndInputOpenDay", "WeekEndInputOD", "t_roleparams_long", "idx", "v6", 10120, 10126, 2)},
            {"LangHunLingYuDayAwards", new RoleParamType("LangHunLingYuDayAwardsDay", "LangHunLingYuDayAwards", "t_roleparams_long", "idx", "v7", 10120, 10127, 2)},
            {"LangHunLingYuDayAwardsFlags", new RoleParamType("LangHunLingYuDayAwardsFlags", "LangHunLingYuDayAwardsFlags", "t_roleparams_long", "idx", "v8", 10120, 10128, 2)},
            {"EnterBangHuiUnixSecs", new RoleParamType("EnterBangHuiUnixSecs", "EnterBangHuiUnixSecs", "t_roleparams_long", "idx", "v9", 10120, 10129, 2)},
            {"LastAutoReviveTicks", new RoleParamType("LastAutoReviveTicks", "LastAutoReviveTicks", "t_roleparams_long", "idx", "v10", 10120, 10130, 2)},
            {"AlreadyZuanShiChangeNameTimes", new RoleParamType("AlreadyZuanShiChangeNameTimes", "AlreadyZuanShiChangeNameTimes", "t_roleparams_long", "idx", "v11", 10120, 10131, 2)},
            {"CannotJoinKFCopyEndTicks", new RoleParamType("CannotJoinKFCopyEndTicks", "CannotJoinKFCopyEndTicks", "t_roleparams_long", "idx", "v12", 10120, 10132, 2)},
            {"ElementWarCount", new RoleParamType("ElementWarCount", "ElementWarCount", "t_roleparams_long", "idx", "v13", 10120, 10133, 2)},

#region KT_TANDUNG

            {"TotalGuildMoneyWithDraw", new RoleParamType("TotalGuildMoneyWithDraw", "TotalGuildMoneyWithDraw", "t_roleparams_long", "idx", "v14", 10120, 10134, 2)},
            {"TotalGuildMoneyAdd", new RoleParamType("TotalGuildMoneyAdd", "TotalGuildMoneyAdd", "t_roleparams_long", "idx", "v15", 10120, 10135, 2)},

#endregion KT_TANDUNG

            /// Tiềm năng có được từ bánh
            {"TotalPropPoint", new RoleParamType("TotalPropPoint", "TotalPropPoint", "t_roleparams_long", "idx", "v16", 10120, 10136, 2)},
            /// Kỹ năng có được từ bánh
            {"TotalSkillPoint", new RoleParamType("TotalSkillPoint", "TotalSkillPoint", "t_roleparams_long", "idx", "v17", 10120, 10137, 2)},
            /// Lượng kinh nghiệm Tu Luyện Châu có
            {RoleParamName.XiuLianZhu, new RoleParamType(RoleParamName.XiuLianZhu, RoleParamName.XiuLianZhu, "t_roleparams_long", "idx", "v18", 10120, 10138, 2)},
            {RoleParamName.XiuLianZhu_TotalTime, new RoleParamType(RoleParamName.XiuLianZhu_TotalTime, RoleParamName.XiuLianZhu_TotalTime, "t_roleparams_long", "idx", "v19", 10120, 10139, 2)},

            {"KaiFuOnlineDayTimes_1", new RoleParamType("KaiFuOnlineDayTimes_1", "KaiFuOnlineDayTimes_1", "t_roleparams_long", "idx", "v20", 10120, 10140, 2)},
            {"KaiFuOnlineDayTimes_2", new RoleParamType("KaiFuOnlineDayTimes_2", "KaiFuOnlineDayTimes_2", "t_roleparams_long", "idx", "v21", 10120, 10141, 2)},
            {"KaiFuOnlineDayTimes_3", new RoleParamType("KaiFuOnlineDayTimes_3", "KaiFuOnlineDayTimes_3", "t_roleparams_long", "idx", "v22", 10120, 10142, 2)},
            {"KaiFuOnlineDayTimes_4", new RoleParamType("KaiFuOnlineDayTimes_4", "KaiFuOnlineDayTimes_4", "t_roleparams_long", "idx", "v23", 10120, 10143, 2)},
            {"KaiFuOnlineDayTimes_5", new RoleParamType("KaiFuOnlineDayTimes_5", "KaiFuOnlineDayTimes_5", "t_roleparams_long", "idx", "v24", 10120, 10144, 2)},
            {"KaiFuOnlineDayTimes_6", new RoleParamType("KaiFuOnlineDayTimes_6", "KaiFuOnlineDayTimes_6", "t_roleparams_long", "idx", "v25", 10120, 10145, 2)},
            {"KaiFuOnlineDayTimes_7", new RoleParamType("KaiFuOnlineDayTimes_7", "KaiFuOnlineDayTimes_7", "t_roleparams_long", "idx", "v26", 10120, 10146, 2)},
            {"KaiFuOnlineDayTimes_8", new RoleParamType("KaiFuOnlineDayTimes_8", "KaiFuOnlineDayTimes_8", "t_roleparams_long", "idx", "v27", 10120, 10147, 2)},

#region Kiếm Thế

             // TINH HOẠT LỰC
            {"CurStamina", new RoleParamType("CurStamina", "CurStamina", "t_roleparams_long", "idx", "v28", 10120, 10148, 2)},
            {"GatherPoint", new RoleParamType("GatherPoint", "GatherPoint", "t_roleparams_long", "idx", "v29", 10120, 10149, 2)},
            {"MakePoint", new RoleParamType("MakePoint", "MakePoint", "t_roleparams_long", "idx", "v30", 10120, 10150, 2)},
            {RoleParamName.HorseToggleOn, new RoleParamType(RoleParamName.HorseToggleOn, RoleParamName.HorseToggleOn, "t_roleparams_long", "idx", "v31", 10120, 10151, 2)},

            // BẠO VĂN ĐỒNG
            {"CurBVDTaskID", new RoleParamType("CurBVDTaskID", "CurBVDTaskID", "t_roleparams_long", "idx", "v32", 10120, 10152, 2)},
            {"QuestBVDTodayCount", new RoleParamType("QuestBVDTodayCount", "QuestBVDTodayCount", "t_roleparams_long", "idx", "v33", 10120, 10153, 2)},
            {"CanncelQuestBVD", new RoleParamType("CanncelQuestBVD", "CanncelQuestBVD", "t_roleparams_long", "idx", "v34", 10120, 10154, 2)},
            {"QuestBVDStreakCount", new RoleParamType("QuestBVDStreakCount", "QuestBVDStreakCount", "t_roleparams_long", "idx", "v36", 10120, 10155, 2)},

            // HẢI TẶC
            {"PirateQuestSum", new RoleParamType("PirateQuestSum", "PirateQuestSum", "t_roleparams_long", "idx", "v37", 10120, 10156, 2)},
            {"CurPirateQuestID", new RoleParamType("CurPirateQuestID", "CurPirateQuestID", "t_roleparams_long", "idx", "v38", 10120, 10157, 2)},
            {"CurPrestigePoint", new RoleParamType("CurPrestigePoint", "CurPrestigePoint", "t_roleparams_long", "idx", "v39", 10120, 10158, 2)},

#endregion Kiếm Thế
        };

        #endregion RoleParamNameTypeDict

        public const int LongParamKey = 10000;
        public const int SeldomStringParamKey = 20000;

        private const string LongParamTableName = "t_roleparams_long";
        private const string StringParamTableName = "t_roleparams_char";
        private const string SeldomStringParamTableName = "t_roleparams_2";
        private const string OldParamTableName = "t_roleparams";

        private static Dictionary<int, RoleParamType> RoleParamNameIndexDict = new Dictionary<int, RoleParamType>();

        private static object[] PrefixNameTree = new object[128];

        static RoleParamNameInfo()
        {
            try
            {
                foreach (var v in RoleParamNameTypeDict.Values)
                {
                    if (string.IsNullOrEmpty(v.IdxName) || string.IsNullOrEmpty(v.KeyString))
                    {
                        throw new Exception("BUG :RoleParamNameTypeDict " + v.ParamName);
                    }

                    if (v.Type == 2 || v.Type == 1)
                    {
                        RoleParamNameIndexDict.Add(v.ParamIndex, v);
                    }

                    if (v.Type == -2)
                    {
                        string name = v.ParamName.ToLower();
                        object[] nextPtr = PrefixNameTree;
                        foreach (var c in name)
                        {
                            if (null == nextPtr[c] as object[])
                            {
                                nextPtr[c] = new object[128];
                            }

                            nextPtr = nextPtr[c] as object[];
                        }

                        nextPtr[0] = v;
                    }
                }
            }
            catch (System.Exception ex)
            {
                LogManager.WriteException(ex.ToString());
                throw;
            }
        }

        public static RoleParamType GetPrefixParamNameType(string paramName)
        {
            string name = paramName.ToLower();
            object[] nextPtr = PrefixNameTree;
            foreach (var c in name)
            {
                if (!char.IsDigit(c))
                {
                    nextPtr = nextPtr[c] as object[];
                    if (null == nextPtr)
                    {
                        return null;
                    }
                }
            }

            return nextPtr[0] as RoleParamType;
        }

        public static RoleParamType GetRoleParamType(string paramName, string value = null)
        {
            RoleParamType roleParamType;

            if (GameDBManager.Flag_Splite_RoleParams_Table != 0)
            {
                if (RoleParamNameTypeDict.TryGetValue(paramName, out roleParamType))
                {
                    return roleParamType;
                }

                int key;
                if (int.TryParse(paramName, NumberStyles.None, NumberFormatInfo.InvariantInfo, out key) && key >= 0)
                {
                    int rem;
                    if (key < LongParamKey)
                    {
                        Math.DivRem(key, 10, out rem);
                        roleParamType = new RoleParamType(paramName, paramName, StringParamTableName, "idx", "v" + rem, key - rem, key, (int)RoleParamType.ValueTypes.Long);
                    }
                    else if (key < SeldomStringParamKey)
                    {
                        Math.DivRem(key, 40, out rem);
                        roleParamType = new RoleParamType(paramName, paramName, LongParamTableName, "idx", "v" + rem, key - rem, key, (int)RoleParamType.ValueTypes.Long);
                    }
                    else
                    {
                        roleParamType = new RoleParamType(paramName, paramName, SeldomStringParamTableName, "pname", "pvalue", key, key, (int)RoleParamType.ValueTypes.Normal);
                    }
                }
                else
                {
                    if (null == GetPrefixParamNameType(paramName) && (!RoleParamNameTypeExtDict.TryGetValue(paramName, out roleParamType) || roleParamType.Type != -1))
                    {
                        string msg = string.Format("Unknow role parameters used: {0},{1}", paramName, value);
                        MyConsole.WriteLine(msg);
                        LogManager.WriteLog(LogTypes.Error, msg);
                    }

                    roleParamType = new RoleParamType(paramName, paramName, SeldomStringParamTableName, "pname", "pvalue", 0, 0, 0);
                }

                RoleParamNameTypeDict[paramName] = roleParamType;
            }
            else
            {
                if (OldRoleParamNameTypeDict.TryGetValue(paramName, out roleParamType))
                {
                    return roleParamType;
                }

                roleParamType = new RoleParamType(paramName, paramName, OldParamTableName, "pname", "pvalue", 0, 0, 0);
                OldRoleParamNameTypeDict[paramName] = roleParamType;
            }

            return roleParamType;
        }

        public static RoleParamType GetRoleParamType(int idx, int column)
        {
            RoleParamType roleParamType = null;
            int paramIndex = idx + column;
            string varName = paramIndex.ToString();

            if (RoleParamNameIndexDict.TryGetValue(paramIndex, out roleParamType))
            {
                return roleParamType;
            }

            if (idx < LongParamKey)
            {
                roleParamType = new RoleParamType(varName, varName, StringParamTableName, "idx", "v" + column, idx, paramIndex, 2);
            }
            else
            {
                roleParamType = new RoleParamType(varName, varName, LongParamTableName, "idx", "v" + column, idx, paramIndex, 1);
            }

            if (roleParamType != null)
            {
                RoleParamNameIndexDict[paramIndex] = roleParamType;
            }

            return roleParamType;
        }
    }
}