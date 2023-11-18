using System.Collections.Generic;


using Tmsk.Contract;

namespace GameServer.Logic
{
    /// <summary>
    /// Quản lý toàn bộ trò chơi
    /// </summary>
    public class GameManager
    {

        #region Quản lý application

        public static int HttpServiceCode { get; set; } = 8888;



        /// <summary>
        /// Server线路ID
        /// </summary>
        public static int ServerLineID { get; set; } = 1;

        public static string ActiveGiftCodeUrl { get; set; } = "";

        public static string KTCoinService { get; set; } = "http://sdk.vokiem.mobi/UserKTCoinService.aspx";


        /// <summary>
        /// 代表所有线路,包括跨服线路
        /// </summary>
        public const int ServerLineIdAllIncludeKuaFu = 0;

        /// <summary>
        /// 代表所有线路,包括跨服线路,但不包括自己
        /// </summary>
        public const int ServerLineIdAllLineExcludeSelf = -1000;

        /// <summary>
        /// 本服务器的ID,当前等于区号
        /// 数据库操作相关函数的serverId参数,为避免这个值为未正确初始化而影响使用,用LocalServerId,不要用这个值
        /// </summary>
        public static int ServerId { get; set; } = 1;

        /// <summary>
        /// 平台类型
        /// </summary>
        public static PlatformTypes PlatformType = PlatformTypes.APP;

        /// <summary>
        /// 是否启用新的驱动的九宫格模式
        /// </summary>
        public const int Update9GridUsingNewMode = 0;

        /// <summary>
        /// 是否启用多段攻击
        /// </summary>
        public const bool FlagManyAttack = true;

        /// <summary>
        /// 优化多段攻击代码
        /// </summary>
        public const bool FlagManyAttackOp = true;

        /// <summary>
        /// 是否启用锁优化(将锁和Socket绑定)
        /// </summary>
        public const bool FlagOptimizeLock = true;

        /// <summary>
        /// 去掉发送完成时锁BuffLock的过程
        /// </summary>
        public const bool FlagOptimizeLock2 = true;

        /// <summary>
        /// TCPSession锁
        /// </summary>
        public const bool FlagOptimizeLock3 = true;

        /// <summary>
        /// 优化路径字符串处理的消耗
        /// </summary>
        public const bool FlagOptimizePathString = false;

        /// <summary>
        /// 是否禁用记录socket错误状态计数
        /// </summary>
        public const bool FlagOptimizeLockTrace = false;

        /// <summary>
        /// 优化一些运算过程
        /// </summary>
        public const bool FlagOptimizeAlgorithm = true;

        /// <summary>
        /// 线程绑定的缓存(内存)池,本线程用,本线程还,不会跨线程
        /// </summary>
        public const bool FlagOptimizeThreadPool = true;

        /// <summary>
        /// 线程绑定的缓存(内存)池,会跨线程还回
        /// </summary>
        public const bool FlagOptimizeThreadPool2 = false;

        /// <summary>
        /// 线程绑定的缓存(参数)池,会跨线程还回
        /// </summary>
        public const bool FlagOptimizeThreadPool3 = true;

        /// <summary>
        /// 优化BuffLock的锁占用时间
        /// </summary>
        public const bool FlagOptimizeThreadPool4 = true;

        /// <summary>
        /// 优化SendBuff的内存块申请和还回次数,这项优化要求必须开启FlagOptimizeThreadPool4
        /// </summary>
        public const bool FlagOptimizeThreadPool5 = true;

        /// <summary>
        /// 测试参数,禁用所有发送逻辑
        /// </summary>
        public const bool FlagSkipSendDataCall = false;

        /// <summary>
        /// 测试参数,禁用调用AddBuff函数
        /// </summary>
        public const bool FlagSkipAddBuffCall = false;

        /// <summary>
        /// 测试参数,禁用调用TrySend函数
        /// </summary>
        public const bool FlagSkipTrySendCall = false;

        /// <summary>
        /// 测试参数,禁用发送调用
        /// </summary>
        public const bool FlagSkipSocketSend = false;

        /// <summary>
        /// 内存泄漏检测
        /// </summary>
        public const bool FlagTraceMemoryPool = false;

        /// <summary>
        /// 详细记录一些信息
        /// </summary>
        public const bool FlagTraceTCPEvent = false;

        /// <summary>
        /// 详细属性信息
        /// </summary>
        public const bool FlagTracePropsValues = false;

        /// <summary>
        /// 是否禁用名字服务器
        /// </summary>
        public const bool FlagDisableNameServer = true;

        /// <summary>
        /// 不能跳过的发送包数
        /// </summary>
        public const int CostSkipSendCount = 900;

        /// <summary>
        /// 跨服切换时，检查服务器时间是否不同，指定0点前后一段时间（分钟数）
        /// </summary>
        public static int ConstCheckServerTimeDiffMinutes { get; set; } = 3;

        /// <summary>
        /// 指令时间统计记录模式,0 记录较大的时间(不太准确),1 记录所有的指令的时间(不太准确),2 精确记录
        /// </summary>
        public static int StatisticsMode { get; set; } = 1;

        /// <summary>
        /// 优化背包整理性能（禁止保存仅格子位置变更的数据到数据库）
        /// </summary>
        public static bool Flag_OptimizationBagReset { get; set; } = true;

        /// <summary>
        /// 配置文件中的配置,内存池各内存块缓存的数量(Size,Num)
        /// </summary>
        public static Dictionary<int, int> MemoryPoolConfigDict { get; set; } = new Dictionary<int, int>();

        /// <summary>
        /// 开启压力测试模式,在此期间登录的帐号视为压力测试帐号
        /// </summary>
        public static bool TestGamePerformanceMode { get; set; } = false;


        public static bool FlagUseWin32Decrypt { get; set; } = false;//true;


        /// <summary>
        /// 暂时禁用token时间验证的剩余毫秒数，供测试调服务器时间做测试用
        /// </summary>
        public static long GM_NoCheckTokenTimeRemainMS { get; set; } = 0;

        /// <summary>
        /// 优化属性计算
        /// </summary>
        public const bool FlagOptimizeAlgorithm_Props = true;

        public const bool FlagEnableMultiLineServer = false;

        /// <summary>
        /// 本地服务器伪ID
        /// </summary>
        public const int LocalServerId = 0;

        /// <summary>
        /// 本地服务器伪ID
        /// </summary>
        public const int LocalServerIdForNotImplement = 0;


        /// <summary>
        /// 服务器启动完毕
        /// </summary>
        public static bool ServerStarting { get; set; } = true;


        /// <summary>
        /// 是否跨服服务器
        /// </summary>
        public static bool IsKuaFuServer { get; set; } = false;


        #endregion 全局变量

        #region Quản lý các thành phần

        /// <summary>
        /// 暂时关闭皇城战和王城战功能,防止和罗兰城战冲突
        /// </summary>
        public const int OPT_ChengZhanType = 1;

        /// <summary>
        /// 开启货币日志记录
        /// </summary>
        public static bool FlagEnableMoneyEventLog { get; set; } = true;

        /// <summary>
        /// 开启道具日志
        /// </summary>
        public static bool FlagEnableGoodsEventLog { get; set; } = true;

        /// <summary>
        /// 开启活动日志
        /// </summary>
        public static bool FlagEnableGameEventLog { get; set; } = true;

        /// <summary>
        /// 开启操作日志
        /// </summary>
        public static bool FlagEnableOperatorEventLog { get; set; } = true;

        /// <summary>
        /// 开启技能相关日志
        /// </summary>
        public static bool FlagEnableRoleSkillLog { get; set; } = true;

        public static bool FlagEnablePetSkillLog { get; set; } = true;
        public static bool FlagEnableUnionPalaceLog { get; set; } = true;

        public static void SetLogFlags(long flags)
        {
            FlagEnableMoneyEventLog = ((flags & 1) != 0);
            FlagEnableGoodsEventLog = ((flags & 2) != 0);
            FlagEnableGameEventLog = ((flags & 4) != 0);
            FlagEnableOperatorEventLog = ((flags & 8) != 0);
            FlagEnableRoleSkillLog = ((flags & 16) != 0);
            FlagEnablePetSkillLog = ((flags & 32) != 0);
            FlagEnableUnionPalaceLog = ((flags & 64) != 0);
        }

        /// <summary>
        /// 重新计算角色属性的最短时间
        /// </summary>
        public static long FlagRecalcRolePropsTicks = 700;

        /// <summary>
        /// 校验客户端位置
        /// </summary>
        public static int FlagCheckCmdPosition = 1;

        /// <summary>
        /// 从数据库更新配置变量
        /// </summary>
        public static void LoadGameConfigFlags()
        {
            FlagRecalcRolePropsTicks = GameManager.GameConfigMgr.GetGameConfigItemInt("recalcrolepropsticks", 700);
            FlagCheckCmdPosition = GameManager.GameConfigMgr.GetGameConfigItemInt(GameConfigNames.check_cmd_position, 1);
        }

        #endregion 功能开关

        #region 全局对象

        /// <summary>
        /// 在线用户回话管理对象
        /// </summary>
        public static UserSession OnlineUserSession = new UserSession();


      

        /// <summary>
        /// 日志数据库命令队列管理
        /// </summary>
        public static LogDBCmdManager logDBCmdMgr = new LogDBCmdManager();

        /// <summary>
        /// NPC和任务的映射管理
        /// </summary>
        public static NPCTasksManager NPCTasksMgr { get; set; } = new NPCTasksManager();



        public static GameConfig GameConfigMgr = new GameConfig();



        /// <summary>
        /// Cpu And Memory
        /// </summary>
        public static ServerMonitorManager ServerMonitor = new ServerMonitorManager();

        /// <summary>
        /// 上次刷怪时间  TimeUtil.NOW()
        /// </summary>
        public static long LastFlushMonsterMs;

        #endregion 全局对象

        #region 事件日志对象

        /// <summary>
        /// 服务器端普通日志事件
        /// </summary>
        public static ServerEvents SystemServerEvents { get; set; } = new ServerEvents() { EventRootPath = "Events", EventPreFileName = "Event" };

        /// <summary>
        /// 服务器端角色登录日志事件
        /// </summary>
        public static ServerEvents SystemRoleLoginEvents { get; set; } = new ServerEvents() { EventRootPath = "RoleEvents", EventPreFileName = "Login" };

        /// <summary>
        /// 服务器端角色登出日志事件
        /// </summary>
        public static ServerEvents SystemRoleLogoutEvents { get; set; } = new ServerEvents() { EventRootPath = "RoleEvents", EventPreFileName = "Logout" };

        /// <summary>
        /// 服务器端角色完成任务日志事件
        /// </summary>
        public static ServerEvents SystemRoleTaskEvents = new ServerEvents() { EventRootPath = "RoleEvents", EventPreFileName = "Task" };

        /// <summary>
        /// 服务器端角色死亡日志事件
        /// </summary>
        public static ServerEvents SystemRoleDeathEvents = new ServerEvents() { EventRootPath = "RoleEvents", EventPreFileName = "Death" };

        /// <summary>
        /// 服务器端角色铜钱购买日志事件
        /// </summary>
        public static ServerEvents SystemRoleBuyWithTongQianEvents = new ServerEvents() { EventRootPath = "RoleEvents", EventPreFileName = "TongQianBuy" };

        /// <summary>
        /// 服务器端角色银两购买日志事件
        /// </summary>
        public static ServerEvents SystemRoleBuyWithBoundTokenEvents = new ServerEvents() { EventRootPath = "RoleEvents", EventPreFileName = "BoundTokenBuy" };

        /// <summary>
        /// 服务器端角色军贡购买日志事件
        /// </summary>
        public static ServerEvents SystemRoleBuyWithJunGongEvents = new ServerEvents() { EventRootPath = "RoleEvents", EventPreFileName = "JunGongBuy" };

        /// <summary>
        /// 服务器端角色银票购买日志事件
        /// </summary>
        public static ServerEvents SystemRoleBuyWithYinPiaoEvents = new ServerEvents() { EventRootPath = "RoleEvents", EventPreFileName = "YinPiaoBuy" };

        /// <summary>
        /// 服务器端角色元宝购买日志事件
        /// </summary>
        public static ServerEvents SystemRoleBuyWithYuanBaoEvents = new ServerEvents() { EventRootPath = "RoleEvents", EventPreFileName = "YuanBaoBuy" };

        /// <summary>
        /// 服务器端角色元宝奇珍阁购买日志事件
        /// </summary>
        public static ServerEvents SystemRoleQiZhenGeBuyWithYuanBaoEvents = new ServerEvents() { EventRootPath = "RoleEvents", EventPreFileName = "QiZhenGeBuy" };

        /// <summary>
        /// 服务器端角色元宝商城抢购购买日志事件
        /// </summary>
        public static ServerEvents SystemRoleQiangGouBuyWithYuanBaoEvents = new ServerEvents() { EventRootPath = "RoleEvents", EventPreFileName = "QiangGouBuy" };

        /// <summary>
        /// 服务器端角色出售物品日志事件
        /// </summary>
        public static ServerEvents SystemRoleSaleEvents = new ServerEvents() { EventRootPath = "RoleEvents", EventPreFileName = "Sale" };

        /// <summary>
        /// 服务器端角色交易日志事件(物品交易)
        /// </summary>
        public static ServerEvents SystemRoleExchangeEvents1 = new ServerEvents() { EventRootPath = "RoleEvents", EventPreFileName = "Exchange1" };

        /// <summary>
        /// 服务器端角色交易日志事件(银两交易)
        /// </summary>
        public static ServerEvents SystemRoleExchangeEvents2 = new ServerEvents() { EventRootPath = "RoleEvents", EventPreFileName = "Exchange2" };

        /// <summary>
        /// 服务器端角色交易日志事件(元宝交易)
        /// </summary>
        public static ServerEvents SystemRoleExchangeEvents3 = new ServerEvents() { EventRootPath = "RoleEvents", EventPreFileName = "Exchange3" };

        /// <summary>
        /// 服务器端升级日志事件
        /// </summary>
        public static ServerEvents SystemRoleUpgradeEvents = new ServerEvents() { EventRootPath = "RoleEvents", EventPreFileName = "Upgrade" };

        /// <summary>
        /// 服务器端物品相关日志事件
        /// </summary>
        public static ServerEvents SystemRoleGoodsEvents = new ServerEvents() { EventRootPath = "RoleEvents", EventPreFileName = "Goods" };

        /// <summary>
        /// 服务器端掉落被拾取的物品相关日志事件
        /// </summary>
        public static ServerEvents SystemRoleFallGoodsEvents = new ServerEvents() { EventRootPath = "RoleEvents", EventPreFileName = "FallGoods" };

        /// <summary>
        /// 服务器端银两获取和使用的相关日志事件
        /// </summary>
        public static ServerEvents SystemRoleBoundTokenEvents = new ServerEvents() { EventRootPath = "RoleEvents", EventPreFileName = "BoundToken" };

        /// <summary>
        /// 服务器端仓库金币获取和使用的相关日志事件
        /// </summary>
        public static ServerEvents SystemRoleStoreBoundTokenEvents = new ServerEvents() { EventRootPath = "RoleEvents", EventPreFileName = "StoreBoundToken" };

        /// <summary>
        /// 服务器端仓库绑定金币获取和使用的相关日志事件
        /// </summary>
        public static ServerEvents SystemRoleStoreMoneyEvents = new ServerEvents() { EventRootPath = "RoleEvents", EventPreFileName = "StoreMoney" };

        /// <summary>
        /// 服务器端坐骑幸运点日志事件
        /// </summary>
        public static ServerEvents SystemRoleHorseEvents = new ServerEvents() { EventRootPath = "RoleEvents", EventPreFileName = "Horse" };

        /// <summary>
        /// 服务器端帮贡获取和使用的相关日志事件
        /// </summary>
        public static ServerEvents SystemRoleBangGongEvents = new ServerEvents() { EventRootPath = "RoleEvents", EventPreFileName = "BangGong" };

        /// <summary>
        /// 服务器端经脉相关日志事件
        /// </summary>
        public static ServerEvents SystemRoleJingMaiEvents = new ServerEvents() { EventRootPath = "RoleEvents", EventPreFileName = "JingMai" };

        /// <summary>
        /// 服务器端刷新奇珍阁日志事件
        /// </summary>
        public static ServerEvents SystemRoleRefreshQiZhenGeEvents = new ServerEvents() { EventRootPath = "RoleEvents", EventPreFileName = "RefreshQiZhenGe" };

        /// <summary>
        /// 服务器端挖宝事件
        /// </summary>
        public static ServerEvents SystemRoleWaBaoEvents = new ServerEvents() { EventRootPath = "RoleEvents", EventPreFileName = "WaBao" };

        /// <summary>
        /// 服务器端地图进入事件
        /// </summary>
        public static ServerEvents SystemRoleMapEvents = new ServerEvents() { EventRootPath = "RoleEvents", EventPreFileName = "Map" };

        /// <summary>
        /// 副本奖励领取事件
        /// </summary>
        public static ServerEvents SystemRoleFuBenAwardEvents = new ServerEvents() { EventRootPath = "RoleEvents", EventPreFileName = "FuBenAward" };

        /// <summary>
        /// 五行奇阵奖励领取事件
        /// </summary>
        public static ServerEvents SystemRoleWuXingAwardEvents = new ServerEvents() { EventRootPath = "RoleEvents", EventPreFileName = "WuXingAward" };

        /// <summary>
        /// 跑环完成事件
        /// </summary>
        public static ServerEvents SystemRolePaoHuanOkEvents = new ServerEvents() { EventRootPath = "RoleEvents", EventPreFileName = "PaoHuanOk" };

        /// <summary>
        /// 押镖事件
        /// </summary>
        public static ServerEvents SystemRoleYaBiaoEvents = new ServerEvents() { EventRootPath = "RoleEvents", EventPreFileName = "YaBiao" };

        /// <summary>
        /// 连斩事件
        /// </summary>
        public static ServerEvents SystemRoleLianZhanEvents = new ServerEvents() { EventRootPath = "RoleEvents", EventPreFileName = "LianZhan" };

        /// <summary>
        /// 活动怪物的事件
        /// </summary>
        public static ServerEvents SystemRoleHuoDongMonsterEvents = new ServerEvents() { EventRootPath = "RoleEvents", EventPreFileName = "HuoDongMonster" };

        /// <summary>
        /// 服务器端角色精雕细琢[钥匙类]挖宝事件
        /// </summary>
        public static ServerEvents SystemRoleDigTreasureWithYaoShiEvents = new ServerEvents() { EventRootPath = "RoleEvents", EventPreFileName = "DigTreasureWithYaoShi" };

        /// <summary>
        /// 服务器端自动扣除元宝事件
        /// </summary>
        public static ServerEvents SystemRoleAutoSubYuanBaoEvents = new ServerEvents() { EventRootPath = "RoleEvents", EventPreFileName = "AutoSubYuanBao" };

        /// <summary>
        /// 服务器端自动扣除金币事件
        /// </summary>
        public static ServerEvents SystemRoleAutoSubBoundMoneyEvents = new ServerEvents() { EventRootPath = "RoleEvents", EventPreFileName = "AutoSubBoundMoney" };

        /// <summary>
        /// 服务器端自动扣除金币-元宝事件
        /// </summary>
        public static ServerEvents SystemRoleAutoSubEvents = new ServerEvents() { EventRootPath = "RoleEvents", EventPreFileName = "AutoSub" };

        /// <summary>
        /// 角色提取邮件元宝，银两，铜钱事件
        /// </summary>
        public static ServerEvents SystemRoleFetchMailMoneyEvents = new ServerEvents() { EventRootPath = "RoleEvents", EventPreFileName = "MailMoneyFetch" };

        /// <summary>
        /// 服务器端角色天地精元兑换日志事件
        /// </summary>
        public static ServerEvents SystemRoleBuyWithTianDiJingYuanEvents = new ServerEvents() { EventRootPath = "RoleEvents", EventPreFileName = "TianDiJingYuanBuy" };

        /// <summary>
        /// 角色Vip奖励的元宝，银两，铜钱, 灵力事件
        /// </summary>
        public static ServerEvents SystemRoleFetchVipAwardEvents = new ServerEvents() { EventRootPath = "RoleEvents", EventPreFileName = "VipAwardGet" };

        /// <summary>
        /// 服务器端角色金币购买日志事件
        /// </summary>
        public static ServerEvents SystemRoleBuyWithBoundMoneyEvents = new ServerEvents() { EventRootPath = "RoleEvents", EventPreFileName = "BoundMoneyBuy" };

        /// <summary>
        /// 服务器端金币获取和使用的相关日志事件
        /// </summary>
        public static ServerEvents SystemRoleBoundMoneyEvents = new ServerEvents() { EventRootPath = "RoleEvents", EventPreFileName = "BoundMoney" };

        //**************
        /// <summary>
        /// 服务器端角色精元值购买日志事件
        /// </summary>
        public static ServerEvents SystemRoleBuyWithJingYuanZhiEvents = new ServerEvents() { EventRootPath = "RoleEvents", EventPreFileName = "JingYuanZhiBuy" };

        /// <summary>
        /// 服务器端角色猎杀值购买日志事件
        /// </summary>
        public static ServerEvents SystemRoleBuyWithLieShaZhiEvents = new ServerEvents() { EventRootPath = "RoleEvents", EventPreFileName = "LieShaZhiBuy" };

        /// <summary>
        /// 服务器端角色装备积分值购买日志事件
        /// </summary>
        public static ServerEvents SystemRoleBuyWithZhuangBeiJiFenEvents = new ServerEvents() { EventRootPath = "RoleEvents", EventPreFileName = "ZhuangBeiJiFenBuy" };

        /// <summary>
        /// 服务器端角色军功值购买日志事件===》注意，军功值需要和旧的 帮会的 军贡值相区别
        /// </summary>
        public static ServerEvents SystemRoleBuyWithJunGongZhiEvents = new ServerEvents() { EventRootPath = "RoleEvents", EventPreFileName = "JunGongZhiBuy" };

        /// <summary>
        /// 服务器端角色战魂值购买日志事件
        /// </summary>
        public static ServerEvents SystemRoleBuyWithZhanHunEvents = new ServerEvents() { EventRootPath = "RoleEvents", EventPreFileName = "ZhanHunBuy" };

        /// <summary>
        /// 服务器端角色元宝日志事件
        /// </summary>
        public static ServerEvents SystemRoleTokenEvents = new ServerEvents() { EventRootPath = "RoleEvents", EventPreFileName = "Token" };

        /// <summary>
        /// 服务器端角色活动类日志事件
        /// </summary>
        public static ServerEvents SystemRoleGameEvents = new ServerEvents() { EventRootPath = "RoleEvents", EventPreFileName = "RoleGame" };

        /// <summary>
        /// 服务器端全局活动类日志事件
        /// </summary>
        public static ServerEvents SystemGlobalGameEvents = new ServerEvents() { EventRootPath = "Events", EventPreFileName = "GameLog" };

        #endregion 事件日志对象
    }
}