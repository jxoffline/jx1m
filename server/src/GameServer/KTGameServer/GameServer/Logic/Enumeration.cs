using System;
using System.Collections.Generic;

namespace GameServer.Logic
{
    /// <summary>
    /// Kiểu đối tượng
    /// </summary>
    public enum GSpriteTypes
    {
        /// <summary>
        /// 主角
        /// </summary>
        Leader = 0,

        /// <summary>
        /// 其他玩家
        /// </summary>
        Other,

        /// <summary>
        /// 怪物
        /// </summary>
        Monster,

        /// <summary>
        /// NPC
        /// </summary>
        NPC,

        /// <summary>
        /// 宠物
        /// </summary>
        Pet,

        /// <summary>
        /// 镖车
        /// </summary>
        BiaoChe,

        /// <summary>
        /// 帮旗
        /// </summary>
        JunQi,

        /// <summary>
        /// 假人
        /// </summary>
        FakeRole,
    }

    /// <summary>
    /// Kiểu Nhiệm Vụ
    /// </summary>
    public enum TaskTypes
    {
        /// <summary>
        /// Không có
        /// </summary>
        None = -1,

        /// <summary>
        /// Nói chuyện với NPC
        /// </summary>
        Talk = 0,

        /// <summary>
        /// Giết quái
        /// </summary>
        KillMonster = 1,

        /// <summary>
        /// Giết quái và nhặt vật phẩm
        /// </summary>
        MonsterSomething = 2,

        /// <summary>
        /// Mua vật phẩm từ SHOP
        /// </summary>
        BuySomething = 3,

        /// <summary>
        /// Sử dụng vật phẩm
        /// </summary>
        UseSomething = 4,

        /// <summary>
        /// Giao vật phẩm cho NPC
        /// </summary>
        TransferSomething = 5,

        /// <summary>
        /// Nhận vật phẩm từ NPC
        /// </summary>
        GetSomething = 6,

        /// <summary>
        /// Nhận tiền từ NPC
        /// </summary>
        Collect = 7,

        /// <summary>
        /// Trả lời câu hỏi
        /// </summary>
        AnswerQuest = 8,

        /// <summary>
        /// Vào phái
        /// </summary>
        JoinFaction = 9,

        /// <summary>
        /// Chế đồ
        /// </summary>
        Crafting = 10,

        /// <summary>
        /// Cường hóa
        /// </summary>
        Enhance = 11,


        /// <summary>
        /// Cường hóa tổng cộng bao nhiêu lần
        /// </summary>
        EnhanceTime = 12,


        /// <summary>
        /// Tham gia hoạt động nào đó X lần
        /// </summary>
        JoinActivity = 13,

        /// <summary>
        /// Tham gia hoạt động
        /// </summary>
        JoinBattleSongJinEvent = 14,

        /// <summary>
        /// Giết người
        /// </summary>
        KillOtherGuildRole = 15,

        /// <summary>
        /// mua vật phẩm gì đó
        /// </summary>
        BuyItemInShopGuild = 16,


        /// <summary>
        /// Tìm kiếm vật phẩm với dòng chỉ định
        /// </summary>
        GetItemWithSpcecialLine = 17,


        /// <summary>
        /// Giết người map khác tại bản đồ nào đó
        /// </summary>
        KillOtherGuildRoleTargetMapcode = 18,


        /// <summary>
        /// Vận tiêu bao nhiêu lần
        /// </summary>
        CarriageTotalCount = 19,
    };

    public enum ColorType
    {
        Normal = -1,
        Accpect = 0,
        Done = 1, //Nói chuyện
        Importal = 2, //Nói chuyện
        Green = 3, //Nói chuyện
        Pure = 4, //Nói chuyện
        Blue = 5, //Nói chuyện
        Yellow = 6, //Nói chuyện
    };

    /// <summary>
    /// Toàn bộ danh sách nhiệm vụ
    /// </summary>
    public enum TaskClasses
    {
        MainTask = 0,
        MonPhai = 1,
        NghiaQuan = 2,
        ThuongHoi = 3,
        HaiTac = 4,
        TheGioi = 5,
        BangHoi = 6,

    }

    /// <summary>
    /// Trạng thái nhiệm vụ của NPC
    /// </summary>
    public enum NPCTaskStates
    {
        /// <summary>
        /// Không có trạng thái
        /// </summary>
        None,

        /// <summary>
        /// Có nhiệm vụ chính tuyến để nhận
        /// </summary>
        ToReceive_MainQuest,

        /// <summary>
        /// Có nhiệm vụ chính tuyến để trả
        /// </summary>
        ToReturn_MainQuest,

        /// <summary>
        /// Có nhiệm vụ phụ để nhận
        /// </summary>
        ToReceive_SubQuest,

        /// <summary>
        /// Có nhiệm vụ phụ để trả
        /// </summary>
        ToReturn_SubQuest,

        /// <summary>
        /// Có nhiệm vụ tuần hoàn để nhận
        /// </summary>
        ToReceive_DailyQuest,

        /// <summary>
        /// Có nhiệm vụ tuần hoàn để trả
        /// </summary>
        ToReturn_DailyQuest,
    };

    /// <summary>
    /// Kiểu notify
    /// </summary>
    public enum GameInfoTypeIndexes
    {
        Normal = 0, //一般提示
        Error = 1, //错误
        Hot = 2, //重点提示
        Max = 3, //最大
    };

    /// <summary>
    /// Các thao tác với vật phẩm
    /// </summary>
    public enum ModGoodsTypes
    {
        Abandon = 0, //vỨT ĐI
        EquipLoad = 1, //MẶC ĐỒ
        EquipUnload = 2, //tHÁO ĐỒ
        ModValue = 3, //sỬA GIÁ TRỊ
        Destroy = 4, //hỦY BỎ VẬT PHẨM
        SaleToNpc = 5,//bÁN VÀO npc
        SplitItem = 6,//bÁN VÀO npc
    }

    /// <summary>
    /// Giao dịch
    /// </summary>
    public enum GoodsExchangeCmds
    {
        /// <summary>
        /// Không có
        /// </summary>
        None = 0,

        /// <summary>
        /// Yêu cầu giao dịch
        /// </summary>
        Request,

        /// <summary>
        /// Từ chối
        /// </summary>
        Refuse,

        /// <summary>
        /// Đồng ý
        /// </summary>
        Agree,

        /// <summary>
        /// Hủy giao dịch
        /// </summary>
        Cancel,

        /// <summary>
        /// Thêm vật phẩm lên phiên
        /// </summary>
        AddGoods,

        /// <summary>
        /// Gỡ bỏ vật phẩm khỏi phiên
        /// </summary>
        RemoveGoods,

        /// <summary>
        /// Thêm bạc vào
        /// </summary>
        UpdateMoney,

        /// <summary>
        /// Thêm đồng vào
        /// </summary>
        UpdateYuanBao,


        /// <summary>
        /// Giao dịch Pet
        /// </summary>
        AddPet,

        /// <summary>
        /// Gỡ bỏ vật phẩm khỏi phiên
        /// </summary>
        RemovePet,

        /// <summary>
        /// Khóa
        /// </summary>
        Lock,

        /// <summary>
        /// Bỏ Khóa
        /// </summary>
        Unlock,

        /// <summary>
        /// Hoàn Tất Giao dịch
        /// </summary>
        Done,
    }

    /// <summary>
    /// Thực thể mua hàng
    /// </summary>
    public enum GoodsStallCmds
    {
        /// <summary>
        /// Không có
        /// </summary>
        None = 0,

        /// <summary>
        /// Yêu cầu mở cửa hàng
        /// </summary>
        Request,

        /// <summary>
        /// Bắt đầu ngồi bán
        /// </summary>
        Start,

        /// <summary>
        /// Hủy Bán
        /// </summary>
        Cancel,

        /// <summary>
        /// Thêm vật phẩm lên sập hàng
        /// </summary>
        AddGoods,

        /// <summary>
        /// Gỡ bỏ vật phẩm khỏi sập hàng
        /// </summary>
        RemoveGoods,

        /// <summary>
        /// Update lại tên cửa hàng
        /// </summary>
        UpdateMessage,

        /// <summary>
        /// Xem cửa hàng của ai đó
        /// </summary>
        ShowStall,

        /// <summary>
        /// Mua vật phẩm của cửa hàng nào đó
        /// </summary>
        BuyGoods,
    }

    /// <summary>
    /// Định nghĩa LOẠI BẢN ĐỒ
    /// </summary>
    public enum MapTypes
    {
        Normal = 0, //Bản đồ thông thường
        NormalCopy = 1, //Bản sao của bản đồ thông thường
        DianJiangCopy = 2, //Bản sao của phụ bản
        CaiShenMiaoCopy = 3, //FU TEMPLATE-> Khả năng là tháp
        TaskCopy = 4, //Bản sao nhiệm vụ
        JingJiChang = 5,//Đấu trường-> hình như đa xóa
        HuanYingSiYuan = 6,//Bản đồ trận chiến trên máy chủ liên server
        MarriageCopy = 7,//[bing] Bản đồ cưới
        Max,
    }

    /// <summary>
    /// Kiểu thông báo về clientn
    /// </summary>
    public enum ShowGameInfoTypes
    {
        None = 0,
        OnlySysHint = 1,
        OnlyBox = 2,
        OnlyErr = 3,
        ErrAndBox = 4,
        HintAndBox = 5,
        LittleSysHint = 6,
        SysHintAndChatBox = 7,
        OnlyChatBox = 8,
        OnlyChatBoxNoError = 9,
        PiaoFuZi = 10,
    };


    public enum SaleGoodsConsts
    {
        /// <summary>
        /// ID mặt hàng được bán
        /// </summary>
        SaleGoodsID = -1,

        /// <summary>
        /// Trong kho sách tay
        /// </summary>
        PortableGoodsID = 1,

        /// <summary>
        /// 同时出售的物品数量
        /// </summary>
        MaxSaleNum = 16,

        /// <summary>
        /// 返回列表的最大数量
        /// </summary>
        MaxReturnNum = 250,

        /// <summary>
        /// 金蛋仓库位置【0表示背包，-1000表示随身仓库，这个值2000表示砸金蛋的仓库】
        /// </summary>
        JinDanGoodsID = 2000,

        /// <summary>
        /// 元素之心背包
        /// </summary>
        ElementhrtsGoodsID = 3000,

        /// <summary>
        /// 元素之心装备栏
        /// </summary>
        UsingElementhrtsGoodsID = 3001,

        /// <summary>
        /// 精灵背包
        /// </summary>
        PetBagGoodsID = 4000,

        /// <summary>
        /// 精灵装备栏
        /// </summary>
        UsingDemonGoodsID = 5000,

        /// <summary>
        /// 时装装备栏 包括称号、翅膀、时装
        /// </summary>
        FashionGoods = 6000,

        /// <summary>
        /// 荧光宝石背包 [XSea 2015/8/6]
        /// </summary>
        FluorescentGemBag = 7000,

        /// <summary>
        /// 荧光宝石装备栏 [XSea 2015/8/6]
        /// </summary>
        FluorescentGemEquip = 7001,

        /// <summary>
        /// 魂石背包
        /// </summary>
        SoulStoneBag = 8000,

        /// <summary>
        /// 饰品装备栏
        /// </summary>
        OrnamentGoods = 9000,

        /// <summary>
        /// 魂石装备栏
        /// </summary>
        SoulStoneEquip = 8001,

        /// <summary>
        /// 特殊的摆摊金币物品ID
        /// </summary>
        BaiTanJinBiGoodsID = 50200,
    }

    /// <summary>
    /// 交易市场搜索类型
    /// </summary>
    public enum MarketSearchTypes
    {
        SearchAll = 0,
        SearchRoleName = 1,
        SearchGoodsIDs = 2,
        TypeAndFilterOpts = 3,
    };

    /// <summary>
    /// Loại quái vật
    /// </summary>
    public enum MonsterTypes
    {
        /// <summary>
        /// Bình thường
        /// </summary>
        Normal = 0,

        /// <summary>
        /// Tinh anh
        /// </summary>
        Elite = 1,

        /// <summary>
        /// Thủ lĩnh
        /// </summary>
        Leader = 2,

        /// <summary>
        /// Boss
        /// </summary>
        Boss = 3,

        /// <summary>
        /// Hải tặc
        /// </summary>
        Pirate = 4,
    }

    /// <summary>
    /// Số bạn bè tối đa
    /// </summary>
    public enum FriendsConsts
    {
        /// <summary>
        /// Số bạn tối đa
        /// </summary>
        MaxFriendsNum = 50,

        /// <summary>
        /// Sổ đen tối đa
        /// </summary>
        MaxBlackListNum = 20,

        /// <summary>
        /// Kẻ thù tối đa
        /// </summary>
        MaxEnemiesNum = 20,
    }

    /// <summary>
    /// Điều kiện tìm kiếmm
    /// </summary>
    public enum SearchResultConsts
    {
        /// <summary>
        /// 搜索角色的返回个数
        /// </summary>
        MaxSearchRolesNum = 10,

        /// <summary>
        /// 搜索队伍的返回个数
        /// </summary>
        MaxSearchTeamsNum = 10,
    };

    /// <summary>
    /// Thông báo lỗi về client
    /// </summary>
    public enum HintErrCodeTypes
    {
        None = 0, //无定义
        NoBagGrid = 1, //背包中无空格
        NoChongXueNum = 2, //提示无冲穴次数
        NoLingLi = 3, //提示无灵力
        NoLingFu = 4, //提示无灵符
        NoYuanBao = 5, //提示无灵符
        CangBaoGeStart = 6, //藏宝阁副本开放了
        YaBiaoStart = 7, //押镖活动开始了
        NoXiaoLaBa = 8, //提示无小喇叭
        NoYaBiaoLing = 9, //提示无押镖令牌
        WuXingStart = 10, //五行奇阵活动开始了
        DblExpAndLingLiStart = 11, //双倍经验和灵力时间开始
        LingDiZhanStart = 12, //领地战开始了
        HuangChengZhanStart = 13, //皇城战开始了
        AddShenFenZheng = 14, //完善身份信息
        EnterAFProtect = 15, //进入自动战斗保护状态
        HintReloadXML = 16, //提示重新加载xml
        ForceShenFenZheng = 17, //强制完善身份信息
        HorseLuckyFull = 18, //坐骑幸运值已经满
        ToBuyGoodsID = 19, //要购买的物品ID
        ToBuyFromQiZhenGe = 20, //从奇珍阁购买的物品ID
        ToBuyFromMarket = 21, //从交易市场购买的物品ID
        ToBuyFromBoundToken = 22, //从银两商购买的物品ID
        ToBuyMagicDrugs = 23, //从药店购买蓝药
        NoShiLianLing = 24, //提示无试炼令
        NoChuanSongJuan = 25, //提示无传送卷
        ToVip = 26, //成为vip
        NoTongQian = 27, //没有铜钱
        CallAutoSkill = 28, //自动使用技能
        NoBindZuanShi = 29, //绑定钻石不足
        NoZuanShi = 30, //钻石不足
        NoYuMao = 31, //羽毛不足
        NoShenYingHuoZhong = 32, //神鹰火种不足
        NoZhuFuJingShi = 33, //祝福晶石不足
        NoLingHunJingShi = 34, //灵魂晶石不足
        NoMaYaJingShi = 35, //玛雅晶石不足
        NoShengMingJingShi = 36, //生命晶石不足
        NoChuangZaoJingShi = 37, //创造晶石不足
        NoShenYouJingShi = 38, //神佑晶石不足
        LevelNotEnough = 39, //等级不足
        LevelOutOfRange = 40, //等级超出范围(也可能是不足)
        NeedZhuanSheng = 41, //等级达到转生条件,需要转生
        VIPNotEnough = 41, //VIP等级不足
        NeedUpdateApp = 42, //需要更新App
        NeedUpdateRes = 43, //需要更新Res

        TeamChatTypeIndex = 200, //组队聊天信息,非错误

        MaxVal, //最大值
    }

    /// <summary>
    /// Danh sách sự kiện
    /// </summary>
    public enum ActivityTypes
    {
        None = 0, //  Chưa xác định
        InputFirst = 1, //Quà nạp lần đầu

        JieriBossAttack = 17,

        HeFuBossAttack = 26,

        MeiRiChongZhiHaoLi = 27,

        TotalCharge = 38,
        TotalConsume = 39,

        MaxVal,
    }

    /// <summary>
    /// Hướng di chuyển
    /// </summary>
    public enum Direction
    {
        DR_UP = 0,
        DR_UPRIGHT = 1,
        DR_RIGHT = 2,
        DR_DOWNRIGHT = 3,
        DR_DOWN = 4,
        DR_DOWNLEFT = 5,
        DR_LEFT = 6,
        DR_UPLEFT = 7,
        DR_UNKNOW = 8,
    }

    /// <summary>
    /// Loại đối tượng
    /// </summary>
    public enum ObjectTypes
    {
        /// <summary>
        /// Người chơi
        /// </summary>
        OT_CLIENT = 0,

        /// <summary>
        /// Quái
        /// </summary>
        OT_MONSTER = 1,

        /// <summary>
        /// Vật phẩm
        /// </summary>
        OT_GOODSPACK = 2,

        /// <summary>
        /// Xe tiêu
        /// </summary>
        OT_TRADER_CARRIAGE = 3,

        /// <summary>
        /// Pet
        /// </summary>
        OT_PET = 4,

        /// <summary>
        /// NPC
        /// </summary>
        OT_NPC = 5,

        /// <summary>
        /// Bot bán hàng
        /// </summary>
        OT_STALLBOT = 6,

        OT_FAKEROLE = 7,

        /// <summary>
        /// Bẫy
        /// </summary>
        OT_TRAP = 8,

        /// <summary>
        /// Điểm thu thập
        /// </summary>
        OT_GROWPOINT = 9,

        /// <summary>
        /// Khu vực động
        /// </summary>
        OT_DYNAMIC_AREA = 10,

        /// <summary>
        /// Đối tượng BOT
        /// </summary>
        OT_BOT = 11,
    }

    /// <summary>
    /// Ghi lại các tích lũy theo tuần
    /// </summary>
    public enum WeekRecord
    {
        FirmMoneyExchange = 1,
        FirmTaskCurNum =2,
        FirmTaskCurID =3,

        /// <summary>
        /// Số lượt đã tham gia phụ bản Thần Bí Bảo Khố trong tuần
        /// </summary>
        ShenMiBaoKu_TotalParticipated = 100000,

        /// <summary>
        /// Điểm cống hiến tuần của thành viên bang
        /// </summary>
        GuildWeekPoint = 100001,
    }

    /// <summary>
    /// Lưu trữ vĩnh viễn thực thể
    /// </summary>
    public enum ForeverRecord
    {
        NULL = -1,

        // Đánh dấu đã nhận thưởng gói nạp 1
        IAP_Pack1 = 2220001,

        // Đánh dấu đã nhận thưởng gói nạp 2
        IAP_Pack2 = 2220002,

        // Đánh dấu đã nhận thưởng gói nạp 3
        IAP_Pack3 = 2220003,

        // Đánh dấu đã nhận thưởng gói nạp 4
        IAP_Pack4 = 2220004,

        // Đánh dấu đã nhận thưởng gói nạp 5
        IAP_Pack5 = 2220005,

        // Đánh dấu đã nhận thưởng gói nạp 6
        IAP_Pack6 = 2220006,

        // Đánh dấu đã nhận thưởng gói nạp 7
        FirmTaskCurNum = 100007,

        //Nhiệm vụ hiện tại
        FirmTaskCurID = 100008,

        //Số bạc đã đổi ở thương hội
        FirmMoneyExchange = 100009,

        //Sử dụng sách tăng điểm tiềm năng
        UsingPotentialBook = 100010,

        //Sử dụng sách tăng điểm kỹ năng
        UsingSkillBoundBook = 100011,

        GetReviceGiftCode = 100012,

        ResetSkillRecore = 100013,

        ExChangeCostume = 100014,

        /// <summary>
        /// Đánh dấu vị trí dừng lại lần trước ở vòng sò
        /// </summary>
        SeashellCircle_LastSeashellCircleStopPos = 1000015,

        /// <summary>
        /// Đánh dấu tầng dừng lại lần trước ở vòng sò
        /// </summary>
        SeashellCircle_LastSeashellCircleStage = 1000016,

        /// <summary>
        /// Đánh dấu số cược lần trước ở vòng sò
        /// </summary>
        SeashellCircle_LastSeashellCircleBet = 1000017,

        LevelUpEventRecore = 1000018,

        TotalSeriesLogin = 1000019,

        /// <summary>
        /// Tổng số lượt đã quay Vòng quay may mắn
        /// </summary>
        LuckyCircle_TotalTurn = 1000020,

        /// <summary>
        /// ID pet đang tham chiến
        /// </summary>
        CurrentPetID = 1000021,

        /// <summary>
        /// Thời điểm yêu cầu xóa mật khẩu cấp 2
        /// </summary>
        RequestRemoveSecondPasswordTicks = 1000022,

        /// <summary>
        /// Nhiệm vụ hiện tại đang làm là nhiệm vụ nào
        /// </summary>
        TaskDailyArmyCurentTaskID = 1000023,

        /// <summary>
        /// Tổng số nhiệm vụ đã làm trong ngày
        /// </summary>
        TaskDailyArmyTotalCount = 1000024,


        /// <summary>
        /// Tổng số nhiệm vụ đã hủy
        /// </summary>
        TaskDailyArmyCancelQuest = 1000025,

        /// <summary>
        /// Chuỗi nhiệm vụ dài nhất đã từng làm
        /// </summary>
        TaskDailyArmyMaxStreakCount = 1000026,

        /// <summary>
        /// Đã nhận thưởng chuỗi tới mốc nào rồi
        /// </summary>
        TaskDailyArmyCurentAward = 1000027,

        /// <summary>
        /// ID kỹ năng đang luyện hiện tại
        /// </summary>
        CurrentExpSkill = 1000028,

        /// <summary>
        /// Thứ hạng bản thân lần trước trong sự kiện Phong Hỏa Liên Thành
        /// </summary>
        FengHuoLianCheng_LastRank = 1000029,

        /// <summary>
        /// ID phụ bản lần trước
        /// </summary>
        LastCopySceneID = 1000030,

        /// <summary>
        /// Thời điểm khởi tạo phụ bản lần trước
        /// </summary>
        LastCopySceneCreatedTicks = 1000031,

        /// <summary>
        /// ID bản đồ lần trước
        /// </summary>
        LastMapID = 1000031,
        /// <summary>
        /// Vị trí X bản đồ lần trước
        /// </summary>
        LastMapPosX = 1000032,
        /// <summary>
        /// Vị trí Y bản đồ lần trước
        /// </summary>
        LastMapPosY = 1000033,

        /// <summary>
        /// ID bản đồ hồi thành
        /// </summary>
        DefaultReliveMapID = 1000034,
        /// <summary>
        /// Vị trí X bản đồ hồi thành
        /// </summary>
        DefaultRelivePosX = 1000035,
        /// <summary>
        /// Vị trí Y bản đồ hồi thành
        /// </summary>
        DefaultRelivePosY = 1000036,

        /// <summary>
        /// Tổng số giây đã Online
        /// </summary>
        TotalOnlineSec = 1000037,


        /// <summary>
        /// Tổng số thời gian đã bán hàng bởi BOT
        /// </summary>
        TimeSaleByBot = 1000038,

        /// <summary>
        /// ID danh hiệu đặc biệt
        /// </summary>
        SpecialTitleID = 1000039,
        /// <summary>
        /// Thời điểm nhận danh hiệu đặc biệt (giờ)
        /// </summary>
        SpecialTitleInitHour = 1000040,

        /// <summary>
        /// Tổng số lượt đã quay Vòng quay may mắn - đặc biệt
        /// </summary>
        TurnPlate_TotalTurn = 1000041,


        ResetTurnPlateMark = 1000042,
    }


    
    /// <summary>
    /// Thực thể bảng xếp hạng
    /// </summary>
    public enum RankMode
    {
        CapDo = 0, // Xếp hạng
        TaiPhu = 1,     // Tài phú
        VoLam = 3,    // Xếp hạng võ lâm
        LienDau = 4,    // Xếp hạng võ lâm
        UyDanh = 5,    // Xếp hạng võ lâm

        // Bảng xếp hạng môn phái
        ThieuLam = 11,

        ThienVuong = 12,
        DuongMon = 13,
        NguDoc = 14,
        NgaMy = 15,
        ThuyYen = 16,
        CaiBang = 17,
        ThienNhan = 18,
        VoDang = 19,
        ConLon = 20,
        MinGiao = 21,
        DoanThi = 22,
    }

    /// <summary>
    /// Ghi lại thông tin hoạt động
    /// </summary>
    public enum DailyRecord
    {
      

        AreadyGetTen = 11113,

        PirateTaskID = 11114,

        PirateTaskNumber = 11115,

        /// <summary>
        /// Số lượt quay chúc phúc trong ngày
        /// </summary>
        PlayerPray = 900000,

        /// <summary>
        /// Đánh dấu đã tăng thêm số giờ Tu luyện hôm nay chưa
        /// </summary>
        XiuLianZhu_TodayTimeAdded = 900001,

        /// <summary>
        /// Kinh nghiệm kỹ năng đã đạt được trong ngày
        /// </summary>
        DailySkillExpGet = 900002,

        /// <summary>
        /// Tổng số lượt đã nhập sai mật khẩu cấp 2
        /// </summary>
        TotalInputIncorrectSecondPasswordTimes = 900003,

        /// <summary>
        /// Tiêu Dao Cốc
        /// </summary>
        XoYo = 100000,

        /// <summary>
        /// Tổng số ải đã vượt qua ngày hôm nay
        /// </summary>
        XoYo_StagesPassedToday = 100001,

        /// <summary>
        /// Bí cảnh
        /// </summary>
        MiJing = 100002,

        /// <summary>
        /// Du Long Các
        /// </summary>
        YouLong = 100003,

        /// <summary>
        /// Bạch Hổ Đường
        /// </summary>
        BaiHuTang = 100004,

        BuyDiscount_1 = 100005,

        BuyDiscount_2 = 100006,

        BuyDiscount_3 = 100007,

        /// <summary>
        /// Tổng số câu hỏi đoán hoa đăng đã được hỏi
        /// </summary>
        KnowledgeChallenge_TotalQuestions = 100008,

        /// <summary>
        /// Thông tin Tần Lăng trong ngày
        /// </summary>
        EmperorTomb_TodayData = 100009,

        /// <summary>
        /// Thời gian còn lại được ở trong Tần Lăng trong ngày
        /// </summary>
        EmperorTomb_TodaySecLeft = 100010,

        /// <summary>
        /// Hệ số nhân phi phong lúc mới vào Tần Lăng
        /// </summary>
        EmperorTomb_LastMantleMultiply = 100011,

        /// <summary>
        /// Số lượt vận tiêu đã tham gia trong ngày
        /// </summary>
        CargoCarriage_TotalRoundToday = 100012,


        /// <summary>
        /// Tổng số nhiệm vụ dã tẩu đã làm trong ngày
        /// </summary>
        TaskDailyArmyCurentCount = 100013,

        TaskDailyArmyCurentCountWithCancel = 100014,

        /// <summary>
        /// Tổng số giây đã Online trong ngày
        /// </summary>
        TodayOnlineSec = 100015,

        /// <summary>
        /// Tổng số lượt tham gia quân doanh trong tuần
        /// </summary>
        MilitaryCamp_TotalParticipatedTimes = 100016,
    }

    /// <summary>
    /// Nhật ký hoạt động theo tháng
    /// </summary>
    public enum MonthlyActivityRecord
    {
        /// <summary>
        /// Tổng số điểm Tiêu Dao Cốc có được
        /// </summary>
        XoYo_StoragePoint = 1,

        /// <summary>
        /// Đã nhận thưởng Tiêu Dao Cốc tháng vừa rồi chưa
        /// </summary>
        XoYo_AlreadyGottenLastMonthAward = 2,
    }

    /// <summary>
    /// Nhật ký hoạt động theo tuần
    /// </summary>
    public enum WeeklyActivityRecord
    {
        /// <summary>
        /// Tổng số lượt tham gia vượt ải gia tộc trong tuần
        /// </summary>
        FamilyFuBen_TotalParticipatedTimes = 1000000,

        
    }

    /// <summary>
    /// Các tham biến của nhân vật được lưu lại
    /// </summary>
    public class RoleParamName
    {
        public static List<int> SkipResetRecore = new List<int> { 297, 298 };

        /// <summary>
        /// Chuỗi lưu danh sách danh hiệu của người chơi
        /// </summary>
        public const string RoleTitles = "RoleTitles";

        /// <summary>
        /// Chuỗi lưu thiết lập hệ thống
        /// </summary>
        public const string SystemSettings = "SystemSettings";

        /// <summary>
        /// Chuỗi lưu thiết lập Auto
        /// </summary>
        public const string AutoSettings = "AutoSettings";

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
        /// Thông tin Bách Bảo Rương
        /// </summary>
        public const string SeashellCircleInfo = "SeashellCircleInfo";

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

        /// <summary>
        /// Lưu trữ thành tích của người chơi
        /// </summary>
        public const String ChengJiuExtraData = "ChengJiuData";// Dữ liệu bổ trợ liên quan đến thành tích, chẳng hạn như điểm thành tích, tổng số lần tiêu diệt, số lần đăng nhập hàng ngày liên tiếp, tổng số lần đăng nhập hàng ngày, mỗi dữ liệu được lưu trữ trong 4 byte

        public const String TotalCostMoney = "TotalCostMoney";         // Ghi lại tổng số tiền tiêu của người chơi
        public const String MapPosRecord = "MapPosRecord";// Lưu lại tọa độ hiện tại của người chơi mapid,x,y,mapid,x,y Kiểu SHORT

        public const String YueKaInfo = "YueKaInfo"; // Lưu trữ thông tin thẻ tháng của người chơi

        public const String UpLevelGiftFlags = "UpLevelGiftFlags"; // Thăng cấp nhận thưởng

        public const String ChengJiuFlags = "ChengJiuFlags";//成就完成与否 和 奖励领取标志位 每两bit一小分组表示一个成就

        public const String ZhuangBeiJiFen = "ZhuangBeiJiFen";//装备积分 单个整数
        public const String LieShaZhi = "LieShaZhi";//猎杀值 单个整数
        public const String WuXingZhi = "WuXingZhi";//悟性值 单个整数
        public const String ZhenQiZhi = "ZhenQiZhi";//真气值 单个整数
        public const String TianDiJingYuan = "TianDiJingYuan";//天地精元值 单个整数
        public const String ShiLianLing = "ShiLianLing";//试炼令值 单个整数 ===>通天令值
        public const String MapLimitSecs = "MapLimitSecs_";//地图时间限制前缀, 存储格式为: MapLimitSecs_XXX(地图编号), 日ID,今日已经停留时间(秒),道具额外加的时间(秒)
        public const String JingMaiLevel = "JingMaiLevel";//经脉等级值 单个整数
        public const String WuXueLevel = "WuXueLevel";//武学等级值 单个整数
        public const String ZuanHuangLevel = "ZuanHuangLevel";//砖皇等级值 单个整数
        public const String ZuanHuangAwardTime = "ZHAwardTime";//上次领取钻皇奖励的时间 相对 1970年的毫秒数字符串
        public const String SystemOpenValue = "SystemOpenValue";//系统激活项，主要用于辅助客户端记忆经脉等随等级提升的图标显示 单个整数 按位表示各个激活项，最多32个
        public const String JunGong = "JunGong";//军功值 单个整数
        public const String GuMuAwardDayID = "GuMuAwardDayID";//古墓限时奖励 单个整数
        public const String BossFuBenExtraEnterNum = "BossFuBenNum";//boss副本额外进入次数 单个整数
        public const String KaiFuOnlineDayID = "KaiFuOnlineDayID";//开服在线奖励天ID
        public const String KaiFuOnlineDayBit = "KaiFuOnlineDayBit";//开服在线奖励天的位标志
        public const String KaiFuOnlineDayTimes = "KaiFuOnlineDayTimes_";//开服在线奖励每天的在线时长
        public const String To60or100 = "To60or100"; //达到60或者100级的记忆
        public const String DayGift1 = "MeiRiChongZhiHaoLi1";  //每日充值豪礼1 [7/16/2013 LiaoWei]
        public const String DayGift2 = "MeiRiChongZhiHaoLi2";  //每日充值豪礼2 [7/16/2013 LiaoWei]
        public const String DayGift3 = "MeiRiChongZhiHaoLi3";  //每日充值豪礼3 [7/16/2013 LiaoWei]
        public const String JieriLoginNum = "JieriLoginNum"; //节日的登录次数
        public const String JieriLoginDayID = "JieriLoginDayID"; //节日的登录天ID
        public const String ZiKaDayNum = "ZiKaDayNum"; //当日已经兑换字卡的数量
        public const String ZiKaDayID = "ZiKaDayID"; //当日已经兑换字卡的天ID
        public const String FreeCSNum = "FreeCSNum"; //当日已经免费传送的次数
        public const String FreeCSDayID = "FreeCSDayID"; //当日已经免费传送的天ID
        public const String MaxTongQianNum = "MaxTongQianNum"; //角色的最大铜钱值
        public const String ErGuoTouNum = "ErGuoTouNum"; //二锅头今日的消费次数
        public const String ErGuoTouDayID = "ErGuoTouDayID"; //二锅头今日的天ID
        public const String BuChangFlag = "BuChangFlag"; //补偿的标志
        public const String ZhanHun = "ZhanHun"; //战魂       -- MU 改成 声望 add by liaowei
        public const String RongYu = "RongYu"; //荣誉
        public const String ZhanHunLevel = "ZhanHunLevel"; //战魂等级
        public const String RongYuLevel = "RongYuLevel"; //荣誉等级
        public const String ZJDJiFen = "ZJDJiFen"; //砸金蛋的积分
        public const String ZJDJiFenDayID = "ZJDJiFenDayID"; //砸金蛋的积分天ID
        public const String ZJDJiFenBits = "ZJDJiFenBits"; //砸金蛋的积分领取记录
        public const String ZJDJiFenBitsDayID = "ZJDJiFenBitsDayID"; //砸金蛋的积分领取记录

        public const String TreasureXueZuan = "TreasureXueZuan"; // 藏宝血钻
        public const String TreasureData = "TreasureData"; // 藏宝秘境数据
        public const String FuHuoJieZhiCD = "FuHuoJieZhiCD";            // 复活戒指COOLDOWN[7/31/2013 LiaoWei]
        public const String CurHP = "CurHP";    // Lượng máu hiện tại [7/31/2013 LiaoWei]
        public const String CurMP = "CurMP";    // Lượng mana hiện tại [7/31/2013 LiaoWei]

        /// <summary>
        /// Tổng số điểm tiềm năng có thêm từ bánh
        /// </summary>
        public const String TotalPropPoint = "TotalPropPoint";

        /// <summary>
        /// Tổng số điểm kỹ năng có thêm từ bánh
        /// </summary>
        public const String TotalSkillPoint = "TotalSkillPoint";

        public const String CurStamina = "CurStamina";              // Tổng lượng điểm còn lại [8/16/2013 LiaoWei]

        //Tổng số tiền member ddax nhận được
        public const String TotalGuildMoneyAdd = "TotalGuildMoneyAdd";

        //Tổng số tiền mà người chơi đã rút ra khỏi bang
        public const String TotalGuildMoneyWithDraw = "TotalGuildMoneyWithDraw";

        #region KT ADD _23_11_2020

        public const String GatherPoint = "GatherPoint";              // Hoạt lực của nhân vật
        public const String MakePoint = "MakePoint";              // Tinh lực của nhân vật
        public const String LifeSkill = "LifeSkill";            // Kỹ năng sôngs
        public const String ReputeInfo = "ReputeInfo";          // Thông tin danh vọng


        public const String DailyRecore = "DailyRecore"; // Ghi lại nhật ký ngày
        public const String WeekRecore = "WeekRecore"; // Ghi lại nhật ký tuần
        public const String ForeverRecore = "ForeverRecore"; // Ghi lại nhật ký vĩnh viễn của nhân vật

        public const String MeditateTime = "MeditateTime";              // Ủy thác bạch cầu hoàn
        public const String NotSafeMeditateTime = "NotSafeMeditateTime"; // ỦY thác đại bạch cầu hoàn

        public const String TreasureJiFen = "TreasureJiFen"; // Đã nhận quà chưa

        #endregion KT ADD _23_11_2020

        public const String sPropStrength = "PropStrength";                 // 力量 [8/19/2013 LiaoWei]
        public const String sPropIntelligence = "PropIntelligence";         // 智力 [8/19/2013 LiaoWei]
        public const String sPropDexterity = "PropDexterity";               // 敏捷 [8/19/2013 LiaoWei]
        public const String sPropConstitution = "PropConstitution";         // 体力 [8/19/2013 LiaoWei]
        public const String sPropStrengthChangeless = "PropStrengthChangeless";         // 不变的力量 [1/27/2014 LiaoWei]
        public const String sPropIntelligenceChangeless = "PropIntelligenceChangeless"; // 不变的智力 [1/27/2014 LiaoWei]
        public const String sPropDexterityChangeless = "PropDexterityChangeless";       // 不变的敏捷 [1/27/2014 LiaoWei]
        public const String sPropConstitutionChangeless = "PropConstitutionChangeless"; // 不变的体力 [1/27/2014 LiaoWei]
        public const String AdmireCount = "AdmireCount";                // 崇拜计数 [11/19/2013 LiaoWei]
        public const String AdmireDayID = "AdmireDayID";                // 崇拜日期 [11/19/2013 LiaoWei]
        public const String DayOnlineSecond = "DayOnlineSecond";            // 每日在线时长--秒[1/16/2014 LiaoWei]
        public const String SeriesLoginCount = "SeriesLoginCount";          // 连续登陆计数[1/16/2014 LiaoWei]
        public const String TotalLoginAwardFlag = "TotalLoginAwardFlag";    // 累计登陆奖励领取标记 [2/11/2014 LiaoWei]

        //public const String VIPLevel = "VIPLevel";                          // VIP等级 [2/19/2014 LiaoWei]
        public const String VIPExp = "VIPExp";                              // VIP经验值--充值以外获得的VIP经验值 比如使用物品 [2/19/2014 LiaoWei]

        //public const String VIPGetAwardFlag = "VIPGetAwardFlag";            // VIP领奖标记 [2/19/2014 LiaoWei]
        public const String BloodCastlePlayerPoint = "BloodCastlePlayerPoint"; // 玩家血色城堡积分 [12/14/2013 LiaoWei]

        public const String BloodCastleFuBenSeqID = "BloodCastleFuBenSeqID";      // 玩家血色城堡场景ID [12/14/2013 LiaoWei]
        public const String BloodCastleSceneid = "BloodCastleSceneid";      // 玩家血色城堡场景ID [12/14/2013 LiaoWei]
        public const String BloodCastleSceneFinishFlag = "BloodCastleSceneFinishFlag";      // 玩家血色城堡完成标记 [12/14/2013 LiaoWei]
        public const String DaimonSquarePlayerPoint = "DaimonSquarePlayerPoint"; // 玩家恶魔广场积分 [12/14/2013 LiaoWei]
        public const String DaimonSquareFuBenSeqID = "DaimonSquareFuBenSeqID";
        public const String DaimonSquareSceneid = "DaimonSquareSceneid";      // 玩家恶魔广场场景ID [12/14/2013 LiaoWei]
        public const String DaimonSquareSceneFinishFlag = "DaimonSquareSceneFinishFlag";      // 玩家恶魔广场完成标记 [12/14/2013 LiaoWei]
        public const String DaimonSquareSceneTimer = "DaimonSquareSceneTimer";      // 玩家恶魔广场完成时的剩余时长 [12/14/2013 LiaoWei]

        public const String FightGetThings = "FightGetThings"; //挂机拾取选项的记录
        public const String DailyActiveDayID = "DailyActiveDayID";      // 每日活跃DayID [2/27/2014 LiaoWei]
        public const String DailyActiveInfo1 = "DailyActiveInfo1";      // 每日活跃信息 [2/27/2014 LiaoWei]
        public const String DailyActiveFlag = "DailyActiveFlag";        // 活跃完成与否 和 奖励领取标志位 每两bit一小分组表示一个活跃 [2/27/2014 LiaoWei]
        public const String DailyActiveAwardFlag = "DailyActiveAwardFlag"; // 活跃奖励的领取状态[2/27/2014 LiaoWei]
        public const String DefaultSkillLev = "DefaultSkillLev"; // 默认技能的等级 [3/15/2014 LiaoWei]
        public const String DefaultSkillUseNum = "DefaultSkillUseNum"; // 默认技能的熟练度 [3/15/2014 LiaoWei]

        public const String PKKingAdmireCount = "PKKingAdmireCount";    // Pk之王崇拜计数 [11/19/2013 LiaoWei]
        public const String PKKingAdmireDayID = "PKKingAdmireDayID";    // Pk之王崇拜日期 [11/19/2013 LiaoWei]
        public const String LHLYAdmireCount = "LHLYAdmireCount";    // 狼魂领域圣域城主膜拜计数
        public const String LHLYAdmireDayID = "LHLYAdmireDayID";    // 狼魂领域圣域城主膜拜日期
        public const String ShengWang = "ShengWang"; //声望
        public const String ShengWangLevel = "ShengWangLevel"; //声望等级
        public const String LiXianBaiTanTicks = "LiXianBaiTanTicks"; //离线摆摊的市场(毫秒)
        public const String OpenGridTick = "OpenGridTick"; // 开启包裹的时间戳 [4/7/2014 LiaoWei]
        public const String OpenPortableGridTick = "OpenPortableGridTick"; // 开启随身仓库包裹的时间戳 [4/7/2014 LiaoWei]

        //public const String PictureJudgeFlags = "PictureJudgeFlags"; // 图鉴标记 [5/3/2014 LiaoWei] (废弃 modify by chenjingui 2015-05-29)
        public const String WanMoTaCurrLayerOrder = "WanMoTaCurrLayerOrder"; // 万魔塔当前层编号 [6/6/2014 ChenXiaojun]

        public const String ImpetrateTime = "ImpetrateTime"; // MU祈福时间 [7/30/2014 LiaoWei]
        public const String ChongJiGiftList = "ChongJiGiftList";//新服冲级狂人是否领取标示
        public const String DailyChargeGiftFlags = "DailyChargeGiftFlags";//日常充值奖励是否领取标示
        public const String StarSoul = "StarSoul";// 星魂值 [8/4/2014 LiaoWei]
        public const String DailyShare = "DailyShare";
        public const String LianZhiJinBiCount = "LianZhiJinBiCount"; //当日炼制金币次数
        public const String LianZhiBangZuanCount = "LianZhiBangZuanCount"; //当日炼制绑钻次数
        public const String LianZhiZuanShiCount = "LianZhiZuanShiCount"; //当日炼制钻石次数
        public const String LianZhiJinBiDayID = "LianZhiJinBiDayID"; //炼制金币的日期
        public const String LianZhiBangZuanDayID = "LianZhiBangZuanDayID"; //炼制绑钻的日期
        public const String LianZhiZuanShiDayID = "LianZhiZuanShiDayID"; //炼制钻石的日期
        public const String ChengJiuLevel = "ChengJiuLevel";             //成就等级
        public const String CaiJiCrystalDayID = "CaiJiCrystalDayID";     //水晶幻境采集的日期
        public const String CaiJiCrystalNum = "CaiJiCrystalNum";         //水晶幻境采集的次数
        public const String HeFuLoginFlag = "HeFuLoginFlag";             // 合服登陆好礼标记
        public const String HeFuTotalLoginDay = "HeFuTotalLoginDay";     // 合服累计登陆最后一次在哪天登陆的
        public const String HeFuTotalLoginNum = "HeFuTotalLoginNum";     // 合服累计登陆记录
        public const String HeFuTotalLoginFlag = "HeFuTotalLoginFlag";   // 合服累计登陆领取记录
        public const String HeFuPKKingFlag = "HeFuPKKingFlag";   // 领取合服战场之神的奖励
        public const String JieRiExchargeFlag = "JRExcharge"; // 节日兑换
        public const String InputPointExchargeFlag = "InputPointExchg"; // 充值点兑换次数
        public const String WeekEndInputFlag = "WeekEndInputFlag"; // 周末充值奖励活动随机数
        public const String WeekEndInputOpenDay = "WeekEndInputOD"; // 周末充值奖励活动UI打开日期
        public const String VerifyBuffProp = "VerifyBuffProp"; // 校验BUFF属性标记
        public const String GuildCopyMapAwardDay = "GuildCopyMapAwardDay";
        public const String GuildCopyMapAwardFlag = "GuildCopyMapAwardFlag";
        public const String ElementPowderCount = "ElementPowder"; // 元素粉末
        public const String ElementGrade = "ElementGrade"; // 过去元素的档次
        public const String QingGongYanJoinFlag = "QingGongYanJoinFlag"; // 庆功宴参加次数
        public const String CallPetFreeTime = "CallPetFreeTime"; // 精灵召唤的免费计时
        public const String PetJiFen = "PetJiFen"; // 精灵积分
        public const String MUMoHe = "MUMoHe";//魔核
        public const String SiegeWarfareEveryDayAwardDayID = "SiegeWarfareEveryDayAwardDayID"; // 奖励领取dayid
        public const String FashionWingsID = "FashionWingsID"; // 时装翅膀ID
        public const String FashionTitleID = "FashionTitleID"; // 时装称号id  //panghui add
        public const String RoleLoginRecorde = "RoleLoginRecorde";
        public const String AchievementRune = "AchievementRune";//成就符文
        public const String AchievementRuneUpCount = "AchievementRuneUpCount";//成就符文提升次数

        public const String ArtifactFailCount = "ArtifactFailCount";//神器再造失败次数
        public const String ZaiZaoPoint = "ZaiZaoPoint";//再造点数量

        public const String LLCZAdmireCount = "LLCZAdmireCount";    // Pk之王崇拜计数 [11/19/2013 LiaoWei]
        public const String LLCZAdmireDayID = "LLCZAdmireDayID";    // Pk之王崇拜日期 [11/19/2013 LiaoWei]
        public const String HysySuccessCount = "HysySuccessCount";    // 幻影寺院胜利次数
        public const String HysySuccessDayId = "HysySuccessDayId";    // 幻影寺院计次日期
        public const String HysyYTDSuccessCount = "HysyYTDSuccessCount";    // 幻影寺院昨天胜利次数
        public const String HysyYTDSuccessDayId = "HysyYTDSuccessDayId";    // 幻影寺院昨天日期
        public const String TianTiDayScore = "TianTiDayScore";    // 天梯每日获得积分累计

        public const String YongZheZhanChangAwards = "YongZheZhanChangAwards";    // 勇者战场战斗奖励信息

        public const String LangHunLingYuDayAwardsDay = "LangHunLingYuDayAwards";  //圣域争霸领取日期
        public const String LangHunLingYuDayAwardsFlags = "LangHunLingYuDayAwardsFlags";  //圣域争霸领取档次记录
        public const String EnterBangHuiUnixSecs = "EnterBangHuiUnixSecs";  //进入帮会的时间

        public const String SaleTradeDayID = "SaleTradeDayID";
        public const String SaleTradeCount = "SaleTradeCount";
        public const String FTFTradeDayID = "FTFTradeDayID";
        public const String FTFTradeCount = "FTFTradeCount";

        public const String HeFuLuoLanAwardFlag = "HeFuLuoLanAwardFlag";   // 领取合服罗兰城主的奖励

        public const String PrestigeMedal = "PrestigeMedal";//声望勋章
        public const String PrestigeMedalUpCount = "PrestigeMedalUpCount";//声望勋章提升次数
        public const String BanRobotCount = "BanRobotCount";//封外挂次数
        public const String LastAutoReviveTicks = "LastAutoReviveTicks";// 最后一次自动复活时间 [XSea 2015/6/26]

        public const String LeftFreeChangeNameTimes = "LeftFreeChangeNameTimes"; //剩余免费改名次数
        public const String AlreadyZuanShiChangeNameTimes = "AlreadyZuanShiChangeNameTimes"; //已经使用钻石改名的次数

        public const String CannotJoinKFCopyEndTicks = "CannotJoinKFCopyEndTicks"; // 截止到这个时间为止，禁止参加跨服副本

        public const String ElementWarCount = "ElementWarCount";    // 元素试炼次数

        public const String BuildQueueData = "BuildQueueData";     // 领地建造队列数据" openpaynum:queueid|builid|taskid:queueid|buildid|taskid"
        public const String BuildAllLevAward = "BuildAllLevAward";  // 领地总等级奖励领取状态

        public const String ReturnCode = "ReturnCode";

        public const String SpreadCode = "SpreadCode";
        public const String VerifyCode = "VerifyCode";

        public const String SpreadIsVip = "SpreadIsVip";

        public const String LangHunFenMo = "LangHunFenMo"; // 狼魂粉末
        public const String SoulStoneRandId = "SoulStoneRandId"; // 魂石随机组
        public const String ZhengBaPoint = "ZhengBaPoint"; // 争霸点
        public const String ZhengBaAwardFlag = "ZhengBaAwardFlag"; // 众神争霸领奖标识
        public const String ZhengBaHintFlag = "ZhengBaHintFlag"; // 争霸可参与提示框
        public const String ZhengBaJoinIconFlag = "ZhengBaJoinIconFlag"; //争霸可参与提示感叹号
        public const String SettingBitFlags = "SettingBitFlags"; // 一些功能设置, 参考ESettingBitFlag

        public const string UnionPalaceUpCount = "UnionPalaceUpCount";
        public const string UnionPalace = "UnionPalace";

        public const String PetSkillUpCount = "PetSkillUpCount";

        public const String EnterKuaFuMapFlag = "EnterKuaFuMapFlag"; // 进入跨服地图的标识

        //以后添加的存储,,必须是整数为Key,分为3类,必须按照需要并按顺序追加,数字之间不要有间隔;
        //优先用整数类型,其次可用字符串类型

        // 8字节整数类型,起始Key值为 10134,必须连续添加,不能跳跃
        public const string FirstLongParamKey = "10148";

        public const string KingOfBattleStoreTm = "10149"; // 王者商店刷新时间
        public const string KingOfBattlePoint = "10150"; // 王者点数

        //ascii字符串类型,上限为120个,起始Key值为 23,必须连续添加,不能跳跃
        public const string CoupleArenaWeekRongYao = "27"; // 夫妻竞技场周荣耀次数

        public const string CoupleArenaWeekAward = "28"; // 夫妻竞技场周排行奖励
        public const string CoupleWishWeekAward = "29"; // 情侣祝福榜榜周排行奖励
        public const string CoupleWishAdmireFlag = "30"; // 情侣祝福榜膜拜情况
        public const string CoupleWishYanHuiFlag = "31"; // 情侣祝福榜宴会情况
        public const string KingOfBattleAwards = "32"; // 王者战场战斗奖励信息
        public const string KingOfBattleStore = "33"; // 王者商店数据
        public const string CoupleWishEffectAward = "34"; // 情侣祝福特效奖励

        //少数高端玩家或者少量玩家才会用到用的,ascii字符串类型,上限为120个,起始Key值为 20000,必须连续添加,不能跳跃
        public const string SeldomStringParamKey = "20000";
    }

    /// <summary>
    /// 按bit的二态功能设置，方便扩展，共支持63种二态功能设置
    /// 例子：对一个long型的value，检测是否隐藏了时装
    /// if ((value & (1UL << (int)ESettingBitFlag.HideFashion)) != 0)
    /// {
    ///     Console.Writeline("隐藏时装");
    /// }
    /// </summary>
    public enum ESettingBitFlag
    {
        HideFashion = 0, // 是否隐藏了时装

        TopLimit = 62, //上限， 63位为符号位，不可使用
    }

    /// <summary>
    /// 角色常用整形参数索引 对应 RoleData中的变量RoleCommonUseIntPamamValueList
    /// </summary>
    public enum RoleCommonUseIntParamsIndexs
    {
        ChengJiu = 0,//成就
        ZhuangBeiJiFen = 1,//装备积分===>用道具杀怪获得
        LieShaZhi = 2,//猎杀值
        WuXingZhi = 3,//悟性值
        ZhenQiZhi = 4,//真气值
        TianDiJingYuan = 5,//天地精元值
        ShiLianLing = 6,//试炼令值===>通天令值
        JingMaiLevel = 7,//经脉等级值---通过真气值升级
        WuXueLevel = 8,//武学等级值---通过悟性值升级
        ZuanHuangLevel = 9,//钻皇等级值---通过累积充值元宝值升级
        SystemOpenValue = 10,//系统激活项，主要用于辅助客户端记忆经脉等随等级提升的图标显示 单个整数 按位表示各个激活项，最多32个
        JunGong = 11,//军功值，玩家做任务获取
        KaiFuOnlineDayID = 12,//开服在线奖励DayID
        To60or100 = 13,//达到60或者100级的记忆
        ZhanHun = 14,//战魂
        RongYu = 15,//荣誉
        ZhanHunLevel = 16,//战魂等级
        RongYuLevel = 17,//荣誉等级
        ShengWang = 18,//声望
        ShengWangLevel = 19,//声望等级
        WanMoTaCurrLayerOrder = 20,  // 万魔塔当前层编号
        StarSoulValue = 21,  // 星座值 [8/5/2014 LiaoWei]
        ChengJiuLevel = 22,  // 成就等级
        YuansuFenmo = 23,   // 元素粉末
        PetJiFen = 24,      // 精灵积分
        MUMoHe = 25,   // 魔核
        FashionWingsID = 26,   // 时装-翅膀
        ZaiZaoPoint = 27,//再造点
        YueKaRemainDay = 28, //月卡剩余天数, 0无 >0表示剩余天数，之前的名字叫HasYueKa
        TotalGuardPoint = 29, //拥有的守护点，客户端货币的地方需要显示
        FashionTitleID = 30, //时装 称号panghui add
        FluorescentGem = 31, // 荧光宝石 [XSea 2015/8/19]
        TreasureJiFen = 32, // 藏宝积分
        TreasureXueZuan = 33, // 藏宝血钻
        LangHunFenMo = 34, // 狼魂粉末
        ZhengBaPoint = 35,   // 争霸点
        VideoButton = 36,//视频聊天按钮
        KingOfBattlePoint = 37, //王者战场点数
#if 移植
        CharmPoint = 38, //魅力点数
        ShenLiJingHua = 39, //神力精华

        BuffFashionID = 40,
        NengLiangSmall = 42,
        NengLiangMedium = 43,
        NengLiangBig = 44,
        NengLiangSuper = 45,
        ShenJiPoint = 46,
        ShenJiJiFen = 47,
        ShenJiJiFenAdd = 48,
        MaxCount = 49,
#else
        MaxCount = 38,
#endif
    }

    /// <summary>
    /// 角色ChengJiuExtraData 参数逗号隔开的字段 不要乱改每一个值，可以添加
    /// </summary>
    public enum ChengJiuExtraDataField
    {
        ChengJiuPoints = 0, //成就点数
        TotalKilledMonsterNum = 1,//总杀怪数量
        ContinuousDayLogin = 2,//连续日登录次数
        TotalDayLogin = 3, //总日登录次数

        // MU新增 Begin [3/12/2014 LiaoWei]
        TotalKilledBossNum = 4,    // 击杀指定的BOSS

        CompleteNormalCopyMapNum = 5,    // 通过普通副本的计数
        CompleteHardCopyMapNum = 6,    // 通过精英副本的计数
        CompleteDifficltCopyMapNum = 7,    // 通过炼狱副本的计数
        //         ForgeNum                    = 8,    // 强化计数
        //         AppendNum                   = 9,    // 追加计数
        //         MergeData                   = 10,   // 合成数据
        // MU新增 End [3/12/2014 LiaoWei]
    }

    /// <summary>
    /// 成就类型 太多 用class，不用enum 程序内部不用类型转换
    /// </summary>
    public class ChengJiuTypes
    {
        // 成就系统改造 [3/12/2014 LiaoWei]

        public const int FirstKillMonster = 100; // 第一次杀怪
        public const int FirstAddFriend = 101; // 第一次加好友
        public const int FirstInTeam = 102; // 第一次组队
        public const int FirstInFaction = 103; // 第一次入会
        public const int FirstHeCheng = 104; // 第一次合成
        public const int FirstQiangHua = 105; // 第一次强化
        public const int FirstZhuiJia = 106; // 第一次追加 [3/12/2014 LiaoWei] //第一次洗炼
        public const int FirstJiCheng = 107; // 第一次强化继承 [3/12/2014 LiaoWei]
        public const int FirstBaiTan = 108; // 第一次摆摊 [3/12/2014 LiaoWei]

        public const int LevelStart = 200;  // 等级需求成就开始
        public const int LevelEnd = 204;  // 等级需求成就结束

        public const int LevelChengJiuStart = 300;  // 转生等级需求成就开始
        public const int LevelChengJiuEnd = 304;  // 转生等级需求成就结束

        public const int SkillLevelUpStart = 350;  // 技能升级成就开始 MU新增 [3/30/2014 LiaoWei]
        public const int SkillLevelUpEnd = 356;  // 技能升级成就结束 MU新增 [3/30/2014 LiaoWei]

        public const int ContinuousLoginChengJiuStart = 400;  // 连续登录成就开始
        public const int ContinuousLoginChengJiuEnd = 405;  // 连续登录成就结束

        public const int TotalLoginChengJiuStart = 500;  // 累积登录成就开始
        public const int TotalLoginChengJiuEnd = 508;  // 累积登录成就结束

        public const int ToQianChengJiuStart = 600;  // 铜钱成就开始 -->财富积累--财富之路
        public const int ToQianChengJiuEnd = 608;  // 铜钱成就结束 -->财富积累

        public const int MonsterChengJiuStart = 700;  // 怪物成就开始-->浴血沙场---历练之路
        public const int MonsterChengJiuEnd = 708;  // 怪物成就结束-->浴血沙场

        public const int BossChengJiuStart = 800;  // boss成就开始-->浴血沙场---历练之路
        public const int BossChengJiuEnd = 803;  // boss 成就结束-->浴血沙场

        public const int CompleteCopyMapCountNormalStart = 900;  // 副本通关--普通[3/12/2014 LiaoWei]
        public const int CompleteCopyMapCountNormalEnd = 905;
        public const int CompleteCopyMapCountHardStart = 1000;  // 副本通关--精英[3/12/2014 LiaoWei]
        public const int CompleteCopyMapCountHardEnd = 1005;
        public const int CompleteCopyMapCountDifficltStart = 1100;  // 副本通关--炼狱[3/12/2014 LiaoWei]
        public const int CompleteCopyMapCountDifficltEnd = 1105;  // 副本通关--炼狱[3/12/2014 LiaoWei]

        public const int QiangHuaChengJiuStart = 1200;   // 强化成就开始
        public const int QianHuaChengJiuEnd = 1210;   // 强化成就结束

        public const int ZhuiJiaChengJiuStart = 1300;   // 追加成就开始
        public const int ZhuiJiaChengJiuEnd = 1308;   // 追加成就结束

        public const int HeChengChengJiuStart = 1400;  // 合成成就开始
        public const int HeChengChengJiuEnd = 1411;  // 合成成就结束

        public const int GuildChengJiuStart = 2000;  // 战盟成就开始 MU新增 [3/30/2014 LiaoWei]
        public const int GuildChengJiuEnd = 2004;  // 战盟成就结束 MU新增 [3/30/2014 LiaoWei]

        public const int JunXianChengJiuStart = 2050;  // 军衔成就开始 MU新增 [3/30/2014 LiaoWei]
        public const int JunXianChengJiuEnd = 2061;  // 军衔成就结束 MU新增 [3/30/2014 LiaoWei]

        public static int MainLineTaskStart = 2100;  // 主线任务成就开始 MU新增 [8/5/2014 LiaoWei]
        public static int MainLineTaskEnd = 2112;  // 主线任务成就结束 MU新增 [8/5/2014 LiaoWei]

        /*public const int JingMaiChengJiuStart = 700;//经脉成就开始
        public const int JingMaiChengJiuEnd = 709;//经脉成就结束

        public const int WuXueChengJiuStart = 800;//武学成就开始
        public const int WuXueChengJiuEnd = 809;//武学成就结束*/

        //要求策划在表中配置类型
        public const int First = 1;

        public const int Level = 2;
        public const int Task = 8;
    }

    /// <summary>
    /// Kiểu hồi sinh của quái
    /// </summary>
    public enum MonsterBirthTypes
    {
        TimeSpan = 0,    //Theo thời gian
        TimePoint = 1,   //Theo thời điểm cố định trong ngày
        CopyMapLike = 2,     //Kiểu phụ bản
        CrossMap = 3,    //Kiểu liên máy chủ
        AfterKaiFuDays = 4,//Sau khai mở máy chủ x ngày
        AfterHeFuDays = 5,
        AfterJieRiDays = 6,
        CreateDayOfWeek = 7, // Ngày nào trong tuần
    };

    /// <summary>
    /// 全局的配置相关的名称
    /// </summary>
    public class GameConfigNames
    {
        public const String ChongJiGift1 = "ChongJiLingQuShenZhuang1";      //冲级领取神装1 [7/16/2013 LiaoWei]
        public const String ChongJiGift2 = "ChongJiLingQuShenZhuang2";      //冲级领取神装2 [7/16/2013 LiaoWei]
        public const String ChongJiGift3 = "ChongJiLingQuShenZhuang3";      //冲级领取神装3 [7/16/2013 LiaoWei]
        public const String ChongJiGift4 = "ChongJiLingQuShenZhuang4";      //冲级领取神装4 [7/16/2013 LiaoWei]
        public const String ChongJiGift5 = "ChongJiLingQuShenZhuang5";      //冲级领取神装5 [7/16/2013 LiaoWei]
        public const String ShenZhuangHuiKuiGift = "ShenZhuangHuiKuiGift";//神装回馈 [7/16/2013 LiaoWei]
        public const String PKKingRole = "PKKingRole";          // PK之王  [3/24/2014 LiaoWei]
        public const String AngelTempleRole = "AngelTempleRole";     // 天使神殿击杀BOSS [3/24/2014 LiaoWei]
        public const String BloodCastlePushMsgDayID = "BloodCastlePushMsgDayID";// 血色堡垒消息推送Dayid [1/24/2014 LiaoWei]
        public const String DemoSquarePushMsgDayID = "DemoSquarePushMsgDayID"; // 恶魔广场推送Dayid [1/24/2014 LiaoWei]
        public const String BattlePushMsgDayID = "BattlePushMsgDayID";     // 阵营战消息推送Dayid [1/24/2014 LiaoWei]
        public const String PKKingPushMsgDayID = "PKKingPushMsgDayID";     // PK之王消息推送Dayid [1/24/2014 LiaoWei]
        public const String AngelTempleMonsterUpgradeNumber = "AngelTempleMonsterUpgradeNumber";     // 天使神殿击杀BOSS [3/24/2014 LiaoWei]
        public const String ChongJiGiftList = "ChongJiGiftList";

        public const String QGYRoleID = "qinggongyan_roleid";
        public const String QGYGuildName = "qinggongyan_guildname";
        public const String QGYStartDay = "qinggongyan_startday";
        public const String QGYGrade = "qinggongyan_grade";
        public const String QGYJuBanMoney = "qinggongyan_jubanmoney";
        public const String QGYJoinCount = "qinggongyan_joincount";
        public const String QGYJoinMoney = "qinggongyan_joinmoney";

        public const String tradelog_num_minamount = "tradelog_num_minamount";  // 统计金额的最小数量 5000
        public const String tradelog_freq_sale = "tradelog_freq_sale";          // 统计交易行最小次数 10
        public const String tradelog_freq_ftf = "tradelog_freq_ftf";          // 统计面对面最小次数 10
        public const String enter_sjhj_interval = "enter_sjhj_interval";  // 进入水晶幻境的时间间隔(秒)

        public const String check_cmd_position = "check_cmd_position";  // 判断玩家速度参数的开关(秒)

        // 合服活动期间记录罗兰城主的帮会id以及当时的城主id，因为第二次比赛之后 第一次的帮主是可能更换的，玩家可能利用这个替换帮主来领取第一次的城主的奖励
        public const String hefu_luolan_guildid = "hefu_luolan_guildid";

        public const String kl_giftcode_u_r_l = "kl_giftcode_u_r_l";            // 昆仑礼包码转换的url
        public const String kl_giftcode_md5key = "kl_giftcode_md5key";            // 昆仑礼包码转换的MD5Key
        public const String kl_giftcode_timeout = "kl_giftcode_timeout";            // 昆仑礼包码转换的超时时间

        public const String userwaitconfig = "userwaitconfig";
        public const String vipwaitconfig = "vipwaitconfig";

        public const String ZhongShenZhiShenRole = "ZhongShenZhiShenRole";
        public const String ZhengBaOpenedFlag = "ZhengBaOpenedFlag";                // 众神争霸开启标识
        public const String CoupleArenaFengHuo = "CoupleArenaFengHuo";              // 情侣竞技，烽火佳人称号
        public const String DeleteRoleNeedTime = "DeleteRoleNeedTime";          // MU防恶意操作，删除角色所需时间，单位：分钟
    }

    // Trạng thái về sự kiện
    internal enum ActivityErrorType
    {
        // Không phải vip
        HEFULOGIN_NOTVIP = -100,

        FATALERROR = -60,
        AWARDCFG_ERROR = -50,
        AWARDTIME_OUT = -40,

        // Không đủ điều kiện
        NOTCONDITION = -30,

        BAG_NOTENOUGH = -20,
        ALREADY_GETED = -10,
        MINAWARDCONDIONVALUE = -5,
        ACTIVITY_NOTEXIST = -1,
        RECEIVE_SUCCEED = 0,
    };
}