using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tmsk.Contract
{
    /// <summary>
    /// 活动场景状态
    /// </summary>
    public enum GameSceneStatuses
    {
        STATUS_NULL = 0,    // 战斗状态-无
        STATUS_PREPARE = 1,    // 战斗状态-准备
        STATUS_BEGIN = 2,    // 战斗状态-开始
        STATUS_END = 3,    // 战斗状态-结束
        STATUS_AWARD = 4,    // 战斗状态-奖励
        STATUS_CLEAR = 5,    // 战斗状态-清场
    }

    public enum RegionEventTypes
    {
        SafeRegion = 0, //安全区 参数: 1 安全区,0 非安全区
        JiaoFu = 1, //交付物品区域
        JinQu = 2, //禁区
    }

    public enum PlatformTypes
    {
        Tmsk = 0, //天马
        APP = 1, //苹果Appstore
        YueYu = 2, //苹果越狱
        Android = 3, //安卓
        YYB = 4, //应用宝
        Korea = 5, //韩国
        TaiWan = 6, //台湾
        YueNan = 7, //越南
        Max = 8, //总个数
    }

    public enum ServerStates
    {
        Pause = 1,     //维护状态
        Ready = 2,      //已准备状态
        Normal = 3,     //正常状态
        New = 4,     //New状态         
        Recommand = 5,  //推荐状态
        Merged = 6,  //合服状态
    };

    /// <summary>
    /// 场景UI类型定义,每种类型包括一组mapCode
    /// </summary>
    public enum SceneUIClasses
    {
        Normal = 0, //常规地图
        NormalCopy = 1, //普通副本
        DianJiangCopy = 2, //点将台副本
        CaiShenMiaoCopy = 3, //财神庙副本
        TaskCopy = 4, //任务剧情副本
        BloodCastle = 5, //血色城堡
        Demon = 6,	//恶魔广场
        Battle = 7, //阵营战
        NewPlayerMap = 8, //新手场景地图
        JingYanFuBen = 9, //经验副本 5000-5009
        KaLiMaTemple = 10, //多人副本 - 卡利玛神庙
        EMoLaiXiCopy = 11,//多人副本 - 恶魔来袭
        PKKing = 12, //PK之王
        AngelTemple = 13, //天使神殿
        BossFamily = 14, //Boss之家
        HuangJinShengDian = 15, //黄金圣殿
        JingJiChang = 16, //竞技场[chenshu]
        PaTa = 17, //爬塔
        JinBiFuBen = 18,//个人金币副本
        QiJiMiJing = 19, //奇迹密境
        GuZhanChang = 20, //古战场
        ShuiJingHuanJing = 21, //水晶幻境
        FamilyBoss = 22,//战盟BOSS
        LuolanFazhen = 23,//多人副本-罗兰法阵
        LuoLanChengZhan = 24, //罗兰城战
        HuanYingSiYuan = 25, //幻影寺院（跨服）
        TianTi = 26, //跨服天梯
        YongZheZhanChang = 27, //勇者战场
        ElementWar = 28, //元素试炼
        MoRiJudge = 29, //末日审判(跨服)
        LoveFuBen = 30,//情侣副本
        KuaFuBoss = 31, //跨服Boss
        KaLunTe = 32, //跨服主线地图 - 卡伦特
        CangBaoMiJing = 33, //藏宝密境副本
        CopyWolf = 34,
        LangHunLingYu = 35, //圣域争霸
        KFZhengBa = 36, // 众神争霸
        HuanShuYuan = 37,   // 跨服主线地图 - 幻术园
        CoupleArena = 38,   // 跨服活动 - 夫妻竞技场
        KingOfBattle = 39,  // 王者战场

        //传奇新增内容定义在1000-2000之间
        BaoShiFuBen = 1000, //宝石副本
        VIPFuBen = 1001, //vip副本
        ShaBaKeChengZhan = 1002, //沙巴克城战
        KFXuHuanFuBen = 1003, //跨服副本 --- 虚幻之间

        //其他
        UnDefined = 9999, //未定义

        #region 自定义组仅程序使用-请不要在Settings.xml表中配置这些类型

        All = 10000,        //不区分场景
        KuaFuCopy = 10001,  //跨服副本(仅用于事件类型注册，每个跨服副本定义自己的SceneUIClasses)
        Spread = 10002,     //推广活动-跨服
        KuaFuMap = 10003,   //跨服主线地图
        Ally = 10004,       //战盟结盟
        CoupleWish = 10005, // 情侣祝福

        #endregion 自定义组仅程序使用-请不要在Settings.xml表中配置这些类型
    }

    public enum MapRecordIndexes
    {
        InitGameMapPostion = 0, //进入游戏时恢复地图坐标(如,跨服活动结束后回来)
    }

    public enum DelayExecProcIds
    {
        RecalcProps,
        UpdateOtherProps, //这部分暂时为用到,所以单独拿出来
        NotifyRefreshProps,
        Max,
    }

    public static class ManagerTypes
    {
        public const int ClientGoodsList = 10000;
    }

    /// <summary>
    /// 活动的阶段状态
    /// </summary>
    public static class GameStates
    {
        public const string CommandName = "GameState";

        public const int None = 0;
        public const int SignUp = 1;
        public const int PrepareGame = 2;
        public const int PrepareGame2 = 3;
        public const int GameStart = 4;
    }

    public static class RuntimeVariableNames
    {
        public const string KuaFuUriKeyNamePrefix = "KuaFuUriKeyNamePrefix";
        public const string KuaFuGongNeng = "KuaFuGongNeng";
        public const string HuanYingSiYuanUri = "HuanYingSiYuanUri";
        public const string TianTiUri = "TianTiUri";
        public const string YongZheZhanChangUri = "YongZheZhanChangUri";
        public const string MoRiJudgeUri = "MoRiJudgeUri";
        public const string ElementWarUri = "ElementWarUri";
        public const string KuaFuCopyUri = "KuaFuCopyUri";
        public const string SpreadUri = "SpreadUri";
        public const string LangHunLingYuUri = "LangHunLingYuUri";
        public const string AllyUri = "AllyUri";
    }
}
