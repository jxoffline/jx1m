using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KF.Contract.Data
{
    public static class Consts
    {
        #region 幻影寺院

        /// <summary>
        /// 幻影寺院每个队伍人数
        /// </summary>
        public static int HuanYingSiYuanRoleCountPerSide = 5;//5

        /// <summary>
        /// 幻影寺院一场游戏的总人数
        /// </summary>
        public static int HuanYingSiYuanRoleCountTotal = 10;//10

        /// <summary>
        /// 游戏服务器心跳最大间隔
        /// </summary>
        public static long RenewServerActiveTicks = 35; //秒

        /// <summary>
        /// 长时间无心跳，可能不会再连接上的时间
        /// </summary>
        public static long RenewServerDeadTicks = 86400; //秒

        /// <summary>
        /// 等待进入时间(秒)
        /// </summary>
        public const int HuanYingSiYuanEnterGameSecs = 10;

        /// <summary>
        /// 断线后保留时间(秒),再次期间玩家可以重新连接回到游戏(客户端需保留跨服活动token)
        /// </summary>
        public const int HuanYingSiYuanProtectLinkSecs = 15;

        /// <summary>
        /// 副本存留最大时间(分钟)
        /// </summary>
        public const int HuanYingSiYuanGameFuBenMaxExistMinutes = 15;

        #endregion

        #region 天梯

        /// <summary>
        /// 天梯系统每个队伍人数
        /// </summary>
        public const int TianTiRoleCountPerSide = 1;

        /// <summary>
        /// 天梯系统一场游戏的总人数
        /// </summary>
        public const int TianTiRoleCountTotal = 2;

        /// <summary>
        /// 天梯等待入场时间(秒)
        /// </summary>
        public static int TianTiWaitEnterSecs = 30;

        /// <summary>
        /// 副本存留最大时间(分钟)
        /// </summary>
        public const int TianTiGameFuBenMaxExistMinutes = 8;
        #endregion

        #region 勇者战场
        /// <summary>
        /// 天梯系统每个队伍人数
        /// </summary>
        public const int YongZheZhanChangRoleCountPerSide = 100;

        /// <summary>
        /// 天梯系统一场游戏的总人数, 多留20个空位置
        /// </summary>
        public const int YongZheZhanChangRoleCountTotal = 220;

        /// <summary>
        /// 副本存留最大时间(分钟)
        /// </summary>
        public const int YongZheZhanChangGameFuBenMaxExistMinutes = 65;
        #endregion 勇者战场

        #region 末日审判

        /// <summary>
        /// 每场末日审判的人数
        /// </summary>
        public const int MoRiRoleTotalCount = 5;

        /// <summary>
        /// 副本存留最大时间
        /// </summary>
        public const int MoRiGameFuBenMaxExistMinutes = 20;

        #endregion

        #region spread

        public static int TelMaxCount = 5;
        public static int TelTimeLimit = 20*60;
        public static int TelTimeStop = 120*60;

        public static int VerifyRoleMaxCount = 2;
        public static int VerifyRoleTimeLimit = 5*60;
        public static int VerifyRoleTimeStop = 30*60;

        public static int IsTest = 0;
       
        #endregion

        #region ally

        public static int AllyNumMax = 5;
        public static int AllyRequestClearSecond = 24 * 3600;

        #endregion

        #region 圣域争霸

        public static List<int> LangHunLingYuLevelBangHuiCountList = new List<int>();

        public const int MaxLangHunCityLevel = 10;
        public const int MaxCityCountPerLevel = 1000;
        public const int LangHunLingYuCityOwnerSite = 0;
        public const int LangHunLingYuCityAttackerSite = 1;
        public const int LangHunLingYuCitySiteCount = 4;
        public const int LangHunLingYuMaxWorldCityNum = 31;
        public const int LangHunLingYuMinCityID = 1;
        public const int LangHunLingYuMaxCityID = 1023;
        public const int LangHunLingYuOtherCityCount = 4;
        public const int LangHunLingYuAdmireHistCount = 10;

        #endregion 圣域争霸

        /// <summary>
        /// 副本存留最大时间(分钟)
        /// </summary>
        public const int ElementWarGameFuBenMaxExistMinutes = 20;
    }
}
