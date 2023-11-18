using GameServer.Entities.Skill.Other;
using GameServer.KiemThe.CopySceneEvents;
using GameServer.KiemThe.Core.Activity.CardMonth;
using GameServer.KiemThe.Core.Activity.DaySeriesLoginEvent;
using GameServer.KiemThe.Core.Activity.DownloadBouns;
using GameServer.KiemThe.Core.Activity.EveryDayOnlineEvent;
using GameServer.KiemThe.Core.Activity.LevelUpEvent;
using GameServer.KiemThe.Core.Activity.LuckyCircle;
using GameServer.KiemThe.Core.Activity.PlayerPray;
using GameServer.KiemThe.Core.Activity.RechageEvent;
using GameServer.KiemThe.Core.Activity.SeashellCircle;
using GameServer.KiemThe.Core.Activity.TurnPlate;
using GameServer.KiemThe.Core.BulletinManager;
using GameServer.KiemThe.Core.Item;
using GameServer.KiemThe.Core.MonsterAIScript;
using GameServer.KiemThe.Core.Rechage;
using GameServer.KiemThe.Core.Repute;
using GameServer.KiemThe.Core.Task;
using GameServer.KiemThe.Core.Title;
using GameServer.KiemThe.Entities;
using GameServer.KiemThe.GameEvents;
using GameServer.KiemThe.Logic;
using GameServer.KiemThe.Logic.Manager;
using GameServer.KiemThe.Logic.Manager.Shop;
using GameServer.KiemThe.Logic.Manager.Skill.PoisonTimer;
using GameServer.KiemThe.LuaSystem;
using GameServer.KiemThe.Utilities;
using GameServer.Logic;
using GameServer.Logic.RefreshIconState;
using GameServer.VLTK.Core.Activity.CheckPoint;
using GameServer.VLTK.Core.Activity.TopRankingEvent;
using GameServer.VLTK.Core.Activity.X2ExpEvent;
using GameServer.VLTK.Core.GuildManager;
using GameServer.VLTK.Core.StallManager;
using GameServer.VLTK.Entities.Pet;
using GameServer.VLTK.GameEvents.GrowTree;
using System;
using System.Threading;

namespace GameServer.KiemThe
{
    /// <summary>
    /// Gọi đến ở quá trình đọc dữ liệu
    /// </summary>
    public static class KTMain
    {
        /// <summary>
        /// Tải danh sách bản đồ
        /// </summary>
        public static void LoadMaps()
        {
            KTMapManager.Load();
        }

        /// <summary>
        /// Khởi tạo dữ liệu trước khi tải danh sách bản đồ
        /// </summary>
        public static void InitFirst()
        {
            /// Thiết lập hệ thống
            Console.WriteLine("Loading ServerConfig...");
            ServerConfig.Init();

            /// Đọc dữ liệu NPC hệ thống
            Console.WriteLine("Loading NPCs list...");
            KTNPCManager.LoadNPCTemplates();

            /// Đọc dữ liệu quái hệ thống
            Console.WriteLine("Loading Monsters list...");
            KTMonsterManager.LoadMonsterTemplates();

            /// Đọc dữ liệu thuộc tính cộng của môn phái và nhánh
            Console.WriteLine("Loading Factions list...");
            KFaction.Init();

            /// Đọc dữ liệu thuộc tính cơ bản của đối tượng
            Console.WriteLine("Loading KNPCSetting...");
            KNpcSetting.Init();

            /// Đọc dữ liệu thuộc tính cơ bản khi thăng cấp của nhân vật
            Console.WriteLine("Loading PlayerSetting...");
            KPlayerSetting.Init();

            /// Tải danh sách Tân Thủ Thôn cho phép người chơi mới tạo được vào
            Console.WriteLine("Loading NewbieVillage...");
            KTGlobal.LoadNewbieVillages();

            /// Đọc dữ liệu trừ điểm khi PK
            Console.WriteLine("Loading PKPunish...");
            KTGlobal.LoadPKPunish();

            /// Đọc dữ liệu lọc nội dung Chat
            Console.WriteLine("Loading ChatFilter...");
            KTChatFilter.Init();

            /// Đọc dữ liệu danh hiệu
            Console.WriteLine("Loading Titles list...");
            KTTitleManager.Init();

            /// Ngũ hành tương khắc
            Console.WriteLine("Loading Series data...");
            KTGlobal.LoadAccrueSeries();
            KSpecialStateManager.Init();

            /// Đọc dữ liệu PropertyDictionary
            Console.WriteLine("Loading PropertyDictionary...");
            PropertyDefine.Init();

            /// Đọc dữ liệu kỹ năng
            Console.WriteLine("Loading SkillData...");
            KSkill.LoadSkillData();
            Console.WriteLine("Loading SkillExp...");
            KSkillExp.Init();

            /// Đọc dữ liệu Pet
            Console.WriteLine("Loading Pets list...");
            KPet.LoadPets();

            /// Đọc dữ liệu Avarta nhân vật
            Console.WriteLine("Loading RoleAvartas list...");
            KRoleAvarta.Init();

            /// Đọc dữ liệu Boss thế giới
            Console.WriteLine("Loading WorldBosses list...");
            KTMonsterManager.LoadWorldBoss();

            #region Timers
            ///// Khởi tạo luồng quản lý Captcha
            //Console.WriteLine("Starting KTCaptchaManager...");
            //KTCaptchaManager.Init();

            /// Khởi tạo luồng quản lý thời gian của Người
            Console.WriteLine("Starting KTPlayerTimerManager...");
            KTPlayerTimerManager.Init();

            /// Khởi tạo luồng quản lý thời gian của Quái
            Console.WriteLine("Starting KTMonsterTimerManager...");
            KTMonsterTimerManager.Init();

            /// Khởi tạo luồng quản lý thời gian của GrowPoint
            Console.WriteLine("Starting KTGrowPointTimerManager...");
            KTGrowPointTimerManager.Init();

            /// Khởi tạo luồng quản lý thời gian của Pet
            Console.WriteLine("Starting KTPetTimerManager...");
            KTPetTimerManager.Init();

            /// Khởi tạo luồng quản lý thời gian của vật phẩm rơi
            Console.WriteLine("Starting KTGoodsPackTimerManager...");
            KTGoodsPackTimerManager.Init();

            /// Khởi tạo luồng quản lý thời gian của xe tiêu
            Console.WriteLine("Starting KTTraderCarriageTimerManager...");
            KTTraderCarriageTimerManager.Init();

            /// Khởi tạo luồng quản lý thời gian của Bot biểu diễn
            Console.WriteLine("Starting KTDecoBotTimerManager...");
            KTDecoBotTimerManager.Init();

            /// Khởi tạo luồng quản lý Bot bán hàng
            Console.WriteLine("Start KTStallBotTimerManager...");
            KTStallBotTimerManager.Init();

            /// Khởi tạo luồng quản lý kỹ năng
            Console.WriteLine("Starting KTSkillManager...");
            KTSkillManager.Init();

            /// Khởi tạo luồng quản lý đạn bay
            Console.WriteLine("Starting KTBulletManager...");
            KTBulletManager.Init();

            /// Khởi tạo luông quản lý Task
            Console.WriteLine("Starting KTTaskManager...");
            KTTaskManager.Init();

            /// Khởi tạo luông quản lý ScheduleTask
            Console.WriteLine("Starting KTScheduleTaskManager...");
            KTScheduleTaskManager.Init();

            /// Khởi tạo luông quản lý Trúng độc
            Console.WriteLine("Starting KTPoisonTimerManager...");
            KTPoisonTimerManager.Init();

            /// Khởi tạo luồng quản lý Buff
            Console.WriteLine("Starting KTBuffManager...");
            KTBuffManager.Init();

            /// Khởi tạo luồng quản lý di chuyển Quái
            Console.WriteLine("Starting KTMonsterStoryBoard...");
            KTMonsterStoryBoardEx.Init();

            /// Khởi tạo luồng quản lý di chuyển xe tiêu
            Console.WriteLine("Starting KTTraderCarriageStoryBoard...");
            KTTraderCarriageStoryBoardEx.Init();

            /// Khởi tạo luồng quản lý di chuyển Người
            Console.WriteLine("Starting KTPlayerStoryBoard...");
            KTPlayerStoryBoardEx.Init();

            /// Khởi tạo luồng quản lý di chuyển các đối tượng khác
            Console.WriteLine("Starting KTOtherObjectStoryBoardEx...");
            KTBotStoryBoardEx.Init();

            /// Khởi tạo quản lý phụ bản
            Console.WriteLine("Starting KTCopySceneTimerManager...");
            CopySceneEventManager.Init();

            /// Khởi tạo luồng quản lý khu vực động
            Console.WriteLine("Starting KTDynamicAreaTimerManager...");
            KTDynamicAreaTimerManager.Init();
            #endregion

            /// Khởi tạo Monster AI Script
            Console.WriteLine("Initializing Monster AI Scripts...");
            KTMonsterAIScriptManager.Init();

            Console.WriteLine("Starting Lua-Env...");
            /// Khởi tạo môi trường Lua
            KTLuaEnvironment.Init();

            /// Tải danh sách GM
            Console.WriteLine("Loading GMList...");
            KTGMCommandManager.LoadGMList();

            /// Tải danh sách vật phẩm
            Console.WriteLine("Loading Items...");
            ItemManager.ItemSetup();

            /// Tải danh sách kỹ năng sống
            Console.WriteLine("Loading LifeSkill.....");
            ItemCraftingManager.Setup();

            /// Tải danh sách phân giải trang bị
            Console.WriteLine("Loading ItemRefine.....");
            ItemRefine.Setup();

            /// Tải danh sách nhiệm vụ
            Console.WriteLine("Loading SystemTask.....");
            TaskManager.getInstance().Setup();

            Console.WriteLine("Loading Randombox.....");
            ItemRandomBox.Setup();

            /// Tải danh sách vật phẩm Drop
            Console.WriteLine("Loading MonsterDrop...");
            KTMonsterManager.MonsterDropManager.Init();

            /// Tải dữ liệu Tu Luyện Châu
            Console.WriteLine("Loading XiuLianZhu...");
            ItemXiuLianZhuManager.Init();


            /// Tải danh sách cửa hàng
            Console.WriteLine("Loading Shops...");
            ShopManager.Setup();

            /// Quản lý nạp thẻ
            Console.WriteLine("Loading Recharge Manager...");
            RechageServiceManager.StartService();

            /// Tải danh sách sự kiện
            Console.WriteLine("Loading Activities...");
            KTActivityManager.Init();

            Console.WriteLine("Loading BulletinManager...");
            BulletinManager.Setup();

            Console.WriteLine("Loading ExpMutipleEvent...");
            ExpMutipleEvent.Setup();

            #region Activity
            /// Khởi tạo dữ liệu vòng quay sò
            KTSeashellCircleManager.Init();
            /// Khởi tạo dữ liệu vòng quay may mắn
            KTLuckyCircleManager.Init();
            KTTurnPlateManager.Init();
            /// Khởi tạo dữ liệu chúc phúc
            KTPlayerPrayManager.Init();

            EveryDayOnlineManager.Setup();
            SevenDayLoginManager.Setup();
            CheckPointManager.Setup();
            //TotalLoginManager.Setup();
            CardMonthManager.Setup();
            LevelUpEventManager.Setup();
            RechageManager.Setup();
            IconStateManager.Setup();
            ReputeManager.Setup();


            //Load Guild SKill
            GuildManager.LoadGuildSkill();

            // Lấy ra toàn bộ thông tin bang hội từ DB
            GuildManager.getInstance().GetAllGuildInfo();


            GuildWarCity.getInstance().Setup();

           
            //Loading RANKING
            TopRankingManager.Setup();






            DownloadBounsManager.Setup();
            #endregion

            #region Sự kiện Game
            /// Khởi tạo các sự kiện trong Game
            GameMapEventsManager.Init();
            GrowTreeManager.Setup();

            //   GuidWarManager.getInstance().Setup();


            #endregion
        }

        /// <summary>
        /// Khởi tạo dữ liệu sau khi tải danh sách bản đồ
        /// </summary>
        public static void InitAfterLoadingMap()
        {
            /// Tải danh sách tự tìm đường
            Console.WriteLine("Initializing AutoPath...");
            KTAutoPathManager.Init();

            // Lấy toàn bộ dữ liệu bán hàng
            StallManager.InitServerData();

            /// Thông báo thông tin số luồng hệ thống
            KTMain.ReportThreadsCount();
        }

        /// <summary>
        /// Thông báo thông tin số luồng của hệ thống
        /// </summary>
        private static void ReportThreadsCount()
        {
            Console.WriteLine("============SYSTEM THREADING REPORTS============");
            ThreadPool.GetMaxThreads(out int workerThreads, out int completionPortThreads);
            Console.WriteLine("Thread pool limitation: Workers = {0}, CompletionPorts = {1}", workerThreads, completionPortThreads);
            Console.WriteLine("================================================");
        }
    }
}
