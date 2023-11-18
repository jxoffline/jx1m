using System.Text;
using HSGameEngine.GameEngine.Network;
using Server.Tools;

namespace FS.GameEngine.Network
{
	/// <summary>
	/// ID gói tin đăng nhập
	/// </summary>
	public enum TCPLoginServerCmds { CMD_LOGIN_ON1 = 1, CMD_LOGIN_ON2 = 20 };

    /// <summary>
    /// ID gói tin
    /// </summary>
    public enum TCPGameServerCmds
    {
        CMD_UNKNOWN = 0,

        CMD_LOGIN_ON2 = 20,
        CMD_NTF_CMD_BASE_ID = 21,
        CMD_LOG_OUT = 22,
        CMD_SPR_CLIENTHEART = 23,

        CMD_DB_QUERY_SYSPARAM = 25,
        CMD_DB_TEAMBATTLE = 26,

        #region Group Objects
        /// <summary>
        /// Thông báo có đối tượng mới xung quanh
        /// </summary>
        CMD_NEW_OBJECTS = 30,

        /// <summary>
        /// Thông báo xóa đối tượng xung quanh
        /// </summary>
        CMD_REMOVE_OBJECTS = 31,
        #endregion

        #region Pet
        /// <summary>
        /// Gói tin gửi từ GS lên GameDB truy vấn danh sách pet
        /// </summary>
        CMD_KT_DB_GET_PET_LIST = 50,

        /// <summary>
        /// Gói tin gửi từ GS lên GameDB thiết lập cấp độ và kinh nghiệm cho Pet
        /// </summary>
        CMD_KT_DB_PET_UPDATE_LEVEL_AND_EXP = 51,

        /// <summary>
        /// Gói tin gửi từ GS lên GameDB thiết lập ResID cho Pet
        /// </summary>
        CMD_KT_DB_PET_UPDATE_RESID = 52,

        /// <summary>
        /// Gói tin gửi từ GS lên GameDB thiết lập lĩnh ngộ cho Pet
        /// </summary>
        CMD_KT_DB_PET_UPDATE_ENLIGHTENMENT = 53,

        /// <summary>
        /// Gói tin gửi từ GS lên GameDB thiết lập tên cho Pet
        /// </summary>
        CMD_KT_DB_PET_UPDATE_NAME = 54,

        /// <summary>
        /// Gói tin gửi từ GS lên GameDB thiết lập kỹ năng cho Pet
        /// </summary>
        CMD_KT_DB_PET_UPDATE_SKILLS = 55,

        /// <summary>
        /// Gói tin gửi từ GS lên GameDB thiết lập trang bị cho Pet
        /// </summary>
        CMD_KT_DB_PET_UPDATE_EQUIPS = 56,

        /// <summary>
        /// Gói tin gửi từ GS lên GameDB thiết lập thuộc tính pet
        /// </summary>
        CMD_KT_DB_PET_UPDATE_ATTRIBUTES = 57,

        /// <summary>
        /// Gói tin gửi từ GS lên GameDB thiết lập độ vui vẻ pet
        /// </summary>
        CMD_KT_DB_PET_UPDATE_JOYFUL = 58,

        /// <summary>
        /// Gói tin gửi từ GS lên GameDB thiết lập tuổi thọ pet
        /// </summary>
        CMD_KT_DB_PET_UPDATE_LIFE = 59,

        /// <summary>
        /// Gói tin gửi từ GS lên GameDB thiết lập sinh lực pet
        /// </summary>
        CMD_KT_DB_PET_UPDATE_HP = 60,

        /// <summary>
        /// Gói tin gửi từ GS lên GameDB cập nhật thông tin pet trước khi thoát Game
        /// </summary>
        CMD_KT_DB_PET_UPDATE_BEFORE_QUIT_GAME = 61,

        /// <summary>
        /// Gói tin gửi từ GS lên GameDB xóa Pet
        /// </summary>
        CMD_KT_DB_DELETE_PET = 62,

        /// <summary>
        /// Gói tin gửi từ GS lên GameDB thêm Pet cho người chơi tương ứng
        /// </summary>
        CMD_KT_DB_ADD_PET = 63,

        /// <summary>
        /// Gói tin gửi từ GS về Client thông báo có Pet mới
        /// </summary>
        CMD_KT_NEW_PET = 64,

        /// <summary>
        /// Gói tin gửi từ GS về Client thông báo xóa Pet
        /// </summary>
        CMD_KT_DEL_PET = 65,

        /// <summary>
        /// Gói tin gửi từ Client lên GS thông báo Pet thay đổi vị trí
        /// </summary>
        CMD_KT_PET_CHANGEPOS = 66,

        /// <summary>
        /// Gói tin yêu cầu truy vấn thông tin Pet giữa Client và GS
        /// </summary>
        CMD_KT_GET_PET_LIST = 67,

        /// <summary>
        /// Thực hiện xuất chiến hoặc thu hồi Pet giữa Client và GS
        /// </summary>
        CMD_KT_DO_PET_COMMAND = 68,

        /// <summary>
        /// Gói tin yêu cầu đổi tên pet giữa Client và GS
        /// </summary>
        CMD_KT_PET_CHANGENAME = 69,

        /// <summary>
        /// Gói tin yêu cầu học kỹ năng pet giữa Client và GS
        /// </summary>
        CMD_KT_PET_STUDYSKILL = 70,

        /// <summary>
        /// Gói tin thông báo pet dùng kỹ năng giữa Client và GS
        /// </summary>
        CMD_KT_PET_USE_SKILL = 71,

        /// <summary>
        /// Gói tin thông báo cập nhật các thuộc tính cơ bản của pet cho chủ nhân giữa Client và GS
        /// </summary>
        CMD_KT_PET_UPDATE_BASE_ATTRIBUTES = 72,

        /// <summary>
        /// Gói tin thông báo cập nhật các thuộc tính sức, thân, ngoại, nội của pet cho chủ nhân giữa Client và GS
        /// </summary>
        CMD_KT_PET_UPDATE_ATTRIBUTES = 73,

        /// <summary>
        /// Gói tin thông báo phân phối thuộc tính sức, thân, ngoại, nội của pet giữa Client và GS
        /// </summary>
        CMD_KT_PET_ASSIGN_ATTRIBUTES = 74,

        /// <summary>
        /// Gói tin thông báo tẩy điểm tiềm năng của pet giữa Client và GS
        /// </summary>
        CMD_KT_PET_RESET_ATTRIBUTES = 75,

        /// <summary>
        /// Gói tin yêu cầu tăng tuổi thọ hoặc độ vui vẻ cho pet giữa Client và GS
        /// </summary>
        CMD_KT_PET_FEED = 76,

        /// <summary>
        /// Gói tin tặng quà cho pet giữa Client và GS
        /// </summary>
        CMD_KT_PET_GIFT_ITEMS = 77,

        /// <summary>
        /// Gói tin phóng thích pet giữa Client và GS
        /// </summary>
        CMD_KT_PET_RELEASE = 78,

        /// <summary>
        /// Gói tin cập nhật cấp độ pet giữa Client và GS
        /// </summary>
        CMD_KT_PET_UPDATE_LEVEL = 79,

        /// <summary>
        /// Gói tin cập nhật danh sách pet giữa Client và GS
        /// </summary>
        CMD_KT_PET_UPDATE_PETLIST = 80,

        /// <summary>
        /// Gói tin cập nhật ID chủ nhân pet vào GameDB
        /// </summary>
        CMD_KT_DB_PET_UPDATEROLEID = 81,

        /// <summary>
        /// Gói tin truy vấn thông tin pet của người chơi giữa Client và GS
        /// </summary>
        CMD_KT_PET_GET_PLAYER_LIST = 82,
        #endregion

        #region Xe tiêu
        /// <summary>
        /// Gói tin thông báo có xe tiêu mới
        /// </summary>
        CMD_KT_NEW_TRADER_CARRIAGE = 90,

        /// <summary>
        /// Gói tin thông báo xóa xe tiêu
        /// </summary>
        CMD_KT_DEL_TRADER_CARRIAGE = 91,

        /// <summary>
        /// Gói tin thông báo nhiệm vụ vận tiêu mới
        /// </summary>
        CMD_KT_NEW_CARGO_CARRIAGE_TASK = 92,

        /// <summary>
        /// Gói tin thông báo cập nhật trạng thái nhiệm vụ vận tiêu hiện tại
        /// </summary>
        CMD_KT_UPDATE_CARGO_CARRIAGE_TASK_STATE = 93,
        #endregion

        CMD_LOGIN_ON = 100,
        CMD_ROLE_LIST = 101,
        CMD_CREATE_ROLE = 102,

        CMD_INIT_GAME = 104,
        CMD_PLAY_GAME = 106,
        CMD_SPR_MOVE = 107,

        CMD_OTHER_ROLE = 110,
        CMD_OTHER_ROLE_DATA = 111,
        CMD_SPR_POSITION = 112,
        CMD_SPR_LEVEL_CHANGED = 113,

        CMD_SPR_REALIVE = 119,
        CMD_SPR_RELIFE = 120,

        /// <summary>
        /// Cập nhật thông tin các nhãn khu Obs động được mở
        /// </summary>
        CMD_SPR_UPDATE_DYNAMIC_OBJECT_LABELS = 121,

        CMD_SYSTEM_MONSTER = 122,
        CMD_SPR_MAPCHANGE = 123,
        CMD_SPR_ENTERMAP = 124,
        CMD_SPR_NEWTASK = 125,

        CMD_SPR_LEAVE = 127,
        CMD_SPR_NPC_BUY = 128,
        CMD_SPR_NPC_SALEOUT = 129,
        CMD_SPR_ADD_GOODS = 130,
        CMD_SPR_MOD_GOODS = 131,
        CMD_SPR_MERGE_GOODS = 132,

        CMD_SPR_CHGCODE = 137,
        CMD_SPR_MONEYCHANGE = 138,
        CMD_SPR_MODTASK = 139,
        CMD_SPR_COMPTASK = 140,
        CMD_SPR_EXPCHANGE = 141,
        CMD_SPR_GETFRIENDS = 142,
        CMD_SPR_ADDFRIEND = 143,
        CMD_SPR_REMOVEFRIEND = 144,
        CMD_SPR_REJECTFRIEND = 145,
        CMD_SPR_ASKFRIEND = 146,
        CMD_SPR_NEWGOODSPACK = 147,
        CMD_SPR_DELGOODSPACK = 148,

        CMD_SPR_GETTHING = 150,
        CMD_SPR_CHGPKMODE = 151,
        CMD_SPR_CHGPKVAL = 152,
        CMD_SPR_UPDATENPCSTATE = 153,
        CMD_SPR_NPCSTATELIST = 154,

        CMD_SPR_ABANDONTASK = 156,

        CMD_SPR_CHAT = 159,
        CMD_SPR_USEGOODS = 160,
        CMD_SPR_CHANGEPOS = 161,
        CMD_SPR_NOTIFYCHGMAP = 162,
        CMD_SPR_FORGE = 163,

        CMD_SPR_UPDATE_ROLEDATA = 166,

        CMD_SPR_TokenCHANGE = 170,

        CMD_SPR_GOODSEXCHANGE = 172,
        CMD_SPR_EXCHANGEDATA = 173,
        CMD_SPR_MOVEGOODSDATA = 174,

        #region Sạp hàng cá nhân
        /// <summary>
        /// Thao tác gian hàng
        /// </summary>
        CMD_SPR_GOODSSTALL = 175,

        /// <summary>
        /// Thông tin gian hàng
        /// </summary>
        CMD_SPR_STALLDATA = 176,

        /// <summary>
        /// Thông báo có thằng mới mở shop
        /// </summary>
        CMD_SPR_ROLE_START_STALL = 177,

        /// <summary>
        /// Gói tin sử dụng để update đữ liệu vào game db
        /// </summary>
        CMD_SPR_STALL_UDPATE_DB = 178,

        /// <summary>
        /// Lấy toàn bộ dữ liệu mua bán từ gamedb
        /// </summary>
        CMD_SPR_STALL_QUERRY = 179,

        /// <summary>
        /// Xóa dữ liệu cửa hàng ở DB
        /// </summary>
        CMD_SPR_STALL_DEL = 180,

        /// <summary>
        /// Thông báo có thằng xóa sạp hàng
        /// </summary>
        CMD_SPR_ROLE_STOP_STALL = 181,
        #endregion

        CMD_SPR_DEAD = 183,

        CMD_SPR_QUERYIDBYNAME = 197,

        CMD_GETGOODSLISTBYSITE = 206,

        CMD_SPR_LOADALREADY = 211,

        CMD_SPR_MARKETBUYGOODS = 228,

        CMD_SPR_RESETBAG = 237,
        CMD_SPR_DAILYTASKDATA = 238,

        CMD_SPR_PORTABLEBAGDATA = 243,
        CMD_SPR_RESETPORTABLEBAG = 244,

        CMD_SPR_GETHUODONGDATA = 247,

        CMD_SPR_GETSONGLIGIFT = 252,
        CMD_SPR_CHGHUODONGID = 253,

        CMD_SPR_GETPAIHANGLIST = 271,



        CMD_SPR_CHGHALFBoundTokenPERIOD = 295,



        CMD_SPR_GETUSERMAILLIST = 365,
        CMD_SPR_GETUSERMAILDATA = 366,
        CMD_SPR_FETCHMAILGOODS = 367,
        CMD_SPR_DELETEUSERMAIL = 368,


        CMD_SPR_NEWNPC = 408,
        CMD_SPR_DELNPC = 409,

        CMD_SPR_STOPMOVE = 413,

        CMD_SPR_NOTIFYGOODSINFO = 428,

        CMD_SPR_GETUPLEVELGIFTOK = 447,



        CMD_SPR_EXECUTERECOMMENDPROPADDPOINT = 516,

        CMD_SPR_UPDATEEVERYDAYONLINEAWARDGIFTINFO = 541,
        CMD_SPR_GETEVERYDAYONLINEAWARDGIFT = 542,
        CMD_SPR_UPDATEEVERYDAYSERIESLOGININFO = 543,
        CMD_SPR_GETEVERYDAYSERIESLOGINAWARDGIFT = 544,



        CMD_SPR_GETBAITANLOG = 605,

        CMD_SPR_CHECK = 613,

        CMD_SPR_REFRESH_ICON_STATE = 616,

        CMD_SPR_SWEEP_WANMOTA = 618,

        CMD_SPR_GET_WANMOTA_DETAIL = 620,
        CMD_SPR_GET_SWEEP_REWARD = 621,

        CMD_SPR_QUERYUPLEVELGIFTINFO = 634,
        CMD_SPR_GETUPLEVELGIFTAWARD = 635,

        CMD_SPR_QUERY_REPAYACTIVEINFO = 637,
        CMD_SPR_GET_REPAYACTIVEAWARD = 638,
        CMD_SPR_QUERY_ALLREPAYACTIVEINFO = 639,

        CMD_SPR_GETUSERMAILCOUNT = 651,

        CMD_SPR_OPENMARKET2 = 654,
        CMD_SPR_MARKETSALEMONEY2 = 655,
        CMD_SPR_SALEGOODS2 = 656,
        CMD_SPR_SELFSALEGOODSLIST2 = 657,
        CMD_SPR_OTHERSALEGOODSLIST2 = 658,
        CMD_SPR_MARKETROLELIST2 = 659,
        CMD_SPR_MARKETGOODSLIST2 = 660,
        CMD_SPR_MARKETBUYGOODS2 = 661,

        CMD_SPR_UPGRADE_CHENGJIU = 672,

        CMD_SPR_PUSH_VERSION = 675,



        CMD_SPR_GET_STORE_MONEY = 762,

        CMD_SPR_STORE_MONEY_CHANGE = 764,

        CMD_SPR_WING_ZHUHUN = 811,

        CMD_SYNC_CHANGE_DAY_SERVER = 833,

        CMD_SPR_GET_YUEKA_DATA = 850,
        CMD_SPR_GET_YUEKA_AWARD = 851,

        CMD_SECOND_PASSWORD_SET = 861,



        CMD_SPR_LOGIN_WAITING_INFO = 971,

        CMD_SPR_YONGZHEZHANCHANG_JOIN = 1100,
        CMD_SPR_YONGZHEZHANCHANG_ENTER = 1101,

        CMD_SPR_YONGZHEZHANCHANG_AWARD = 1102,
        CMD_SPR_YONGZHEZHANCHANG_STATE = 1103,

        CMD_SPR_YONGZHEZHANCHANG_AWARD_GET = 1108,

        CMD_SPR_KUAFU_MAP_INFO = 1140,
        CMD_SPR_KUAFU_MAP_ENTER = 1141,

        CMD_DB_UPDATE_POS = 10001,
        CMD_DB_UPDATE_EXPLEVEL = 10002,
        CMD_DB_UPDATE_ROLE_AVARTA = 10003,
        CMD_DB_UPDATEMoney_CMD = 10004,
        CMD_DB_ADDGOODS_CMD = 10005,
        CMD_DB_UPDATEGOODS_CMD = 10006,
        CMD_DB_UPDATETASK_CMD = 10007,

        CMD_DB_UPDATEPKVAL_CMD = 10009,
        CMD_DB_UPDATEKEYS = 10010,
        CMD_DB_UPDATEToken_CMD = 10011,
        CMD_DB_UPDATEBoundToken_CMD = 10012,
        CMD_DB_MOVEGOODS_CMD = 10013,

        CMD_DB_ROLE_ONLINE = 10015,

        CMD_DB_ROLE_OFFLINE = 10017,
        CMD_DB_GET_CHATMSGLIST = 10018,

        CMD_DB_REGUSERID = 10025,

        CMD_DB_GETBANROLECATDICT = 10028,

        CMD_DB_UPDATEONLINETIME = 10032,


        CMD_DB_ADDSKILL = 10036,
        CMD_DB_UPSKILLINFO = 10037,


        CMD_DB_UPDATE_WELFARE = 10045,


        CMD_DB_GETFUBENSEQID = 10049,
        CMD_DB_UPDATEROLEDAILYDATA = 10050,
        CMD_DB_UPDATEBUFFERITEM = 10051,


        CMD_DB_UPDATECZTASKID = 10062,
        CMD_DB_GETTOTALONLINENUM = 10063,



        CMD_DB_UPDATEBANGGONG_CMD = 10071,

        CMD_DB_QUERYCHONGZHIMONEY = 10083,

        CMD_DB_SENDUSERMAIL = 10086,
        CMD_DB_GETUSERMAILDATA = 10087,

        CMD_DB_QUERYLIMITGOODSUSEDNUM = 10089,
        CMD_DB_UPDATELIMITGOODSUSEDNUM = 10090,

        CMD_DB_UPDATEUSERBoundMoney_CMD = 10095,


        CMD_DB_UPDATEROLEPARAM = 10100,



        CMD_DB_QUERYTODAYCHONGZHIMONEY = 10120,

        CMD_DB_EXECUTECHANGEOCCUPATION = 10126,

        CMD_DB_QUERY_REPAYACTIVEINFO = 10160,
        CMD_DB_GET_REPAYACTIVEAWARD = 10161,
        CMD_DB_UPDATE_ACCOUNT_ACTIVE = 10162,

        CMD_DB_GAMECONFIGDICT = 10033,


        /// <summary>
        /// Band người chơi
        /// </summary>
        CMD_DB_BAN_USER = 10026,

        /// <summary>
        /// Band người chơi theo chức năng gì đó
        /// </summary>
        CMD_DB_BAN_USER_BY_TYPE = 10027,


        CMD_DB_GAMECONIFGITEM = 10034,

        CMD_DB_SAVECONSUMELOG = 10167,
        CMD_DB_QUERYFIRSTCHONGZHIBYUSERID = 10110,

        CMD_DB_ADD_STORE_MONEY = 10174,

        CMD_DB_REQUESTNEWGMAILLIST = 10177,
        CMD_DB_MODIFYROLEGMAIL = 10178,

        CMD_DB_ROLE_BUY_YUE_KA_BUT_OFFLINE = 10181,

        CMD_DB_QUERY_ROLEMINIINFO = 10220,

        CMD_DB_GET_SERVERID = 11002,

        CMD_DB_ONLINE_SERVERHEART = 11001,


        CMD_SPR_GM_SET_MAIN_TASK = 13000,



        CMD_SPR_KF_SWITCH_SERVER = 14000,
        CMD_SPR_CHANGE_NAME = 14001,

        CMD_NTF_MAGIC_CRASH_UNITY = 14010,

        CMD_LOGDB_ADD_LOG = 20000,

        CMD_LOGDB_UPDATE_ROLE_KUAFU_DAY_LOG = 20003,

        CMD_DB_DEL_SKILL = 20101,

        CMD_SPR_TASKLIST_KEY = 30000,

        CMD_DB_ERR_RETURN = 30767,
        CMD_KT_TESTPACKET = 32123,
        /// <summary>
        /// Thiết lập vật phẩm vào khay dùng nhanh
        /// </summary>
        CMD_KT_C2G_SET_QUICK_ITEMS = 39999,
        CMD_KT_GET_NEWBIE_VILLAGES = 40000,// VỨT
        CMD_KT_G2C_RENEW_SKILLLIST = 45000,
        CMD_KT_C2G_SKILL_ADDPOINT = 45001,
        CMD_KT_C2G_SET_SKILL_TO_QUICKKEY = 45002,
        CMD_KT_C2G_SET_AND_ACTIVATE_AURA = 45003,
        CMD_KT_C2G_USESKILL = 45009,
        CMD_KT_G2C_USESKILL = 45010,
        CMD_KT_G2C_NOTIFYSKILLCOOLDOWN = 45011,
        CMD_KT_G2C_CREATEBULLET = 45012,
        CMD_KT_G2C_BULLETEXPLODE = 45013,
        CMD_KT_G2C_SKILLRESULT = 45014,
        CMD_KT_G2C_SPRITEBUFF = 45015,
        CMD_KT_G2C_BLINKTOPOSITION = 45016,
        CMD_KT_G2C_CREATEBULLETS = 45017,
        CMD_KT_G2C_BULLETEXPLODES = 45018,
        CMD_KT_G2C_MOVESPEEDCHANGED = 45019,
        CMD_KT_G2C_FLYTOPOSITION = 45020,
        CMD_KT_G2C_SPRITESERIESSTATE = 45021,
        CMD_KT_G2C_SKILLRESULTS = 45022,
        CMD_KT_G2C_OBJECTINVISIBLESTATECHANGED = 45023,
        CMD_KT_G2C_RESETSKILLCOOLDOWN = 45024,
        CMD_KT_G2C_ATTACKSPEEDCHANGED = 45025,
        CMD_KT_GM_COMMAND = 50000,
        CMD_KT_ROLE_ATRIBUTES = 50001,
        CMD_KT_SHOW_NOTIFICATIONTIP = 50002,
        CMD_KT_FACTIONROUTE_CHANGED = 50003,
        CMD_KT_CLICKON_NPC = 50004,
        CMD_KT_G2C_NPCDIALOG = 50005,
        CMD_KT_C2G_NPCDIALOG = 50006,
        CMD_KT_G2C_CHANGEACTION = 50007,
        CMD_KT_C2G_CHANGEACTION = 50008,
        CMD_KT_G2C_SHOWDEBUGOBJECTS = 50009,
        CMD_KT_G2C_SHOWREVIVEFRAME = 50010,
        CMD_KT_C2G_CLIENTREVIVE = 50011,
        CMD_KT_SPR_NEWTRAP = 50012,
        CMD_KT_SPR_DELTRAP = 50013,
        CMD_KT_C2G_SAVESYSTEMSETTINGS = 50014,
        CMD_KT_C2G_SAVEAUTOSETTINGS = 50015,
        CMD_KT_INVITETOTEAM = 50016,
        CMD_KT_CREATETEAM = 50017,
        CMD_KT_AGREEJOINTEAM = 50018,
        CMD_KT_REFUSEJOINTEAM = 50019,
        CMD_KT_GETTEAMINFO = 50020,
        CMD_KT_KICKOUTTEAMMATE = 50021,
        CMD_KT_APPROVETEAMLEADER = 50022,
        CMD_KT_REFRESHTEAMMEMBERATTRIBUTES = 50023,
        CMD_KT_TEAMMEMBERCHANGED = 50024,
        CMD_KT_LEAVETEAM = 50025,
        CMD_KT_G2C_UPDATESPRITETEAMDATA = 50026,
        CMD_KT_ASKTOJOINTEAM = 50027,
        CMD_KT_G2C_ITEMDIALOG = 50030,
        CMD_KT_C2G_ITEMDIALOG = 50031,
        CMD_KT_C2G_OPENSHOP = 50032,
        CMD_KT_G2C_CLOSENPCITEMDIALOG = 50033,

        #region Thách đấu
        /// <summary>
        /// Gửi yêu cầu thách đấu
        /// </summary>
        CMD_KT_ASK_CHALLENGE = 50034,

        /// <summary>
        /// Phản hồi yêu cầu thách đấu
        /// </summary>
        CMD_KT_C2G_RESPONSE_CHALLENGE = 50035,

        /// <summary>
        /// Thực hiện thao tác gì đó liên quan đến thách đấu
        /// </summary>
        CMD_KT_DO_CHALLENGE_COMMAND = 50036,

        /// <summary>
        /// Nhận thông báo thông tiên thách đấu
        /// </summary>
        CMD_KT_RECEIVE_CHALLENGE_INFO = 50037,
        #endregion

        CMD_KT_C2G_AUTOPATH_CHANGEMAP = 50038,
        CMD_KT_G2C_NEW_GROWPOINT = 50039,
        CMD_KT_G2C_DEL_GROWPOINT = 50040,
        CMD_KT_C2G_GROWPOINT_CLICK = 50041,
        CMD_KT_G2C_UPDATE_PROGRESSBAR = 50042,
        CMD_KT_OPEN_TOKENSHOP = 50043,
        CMD_KT_G2C_OPEN_UI = 50044,
        CMD_KT_G2C_CLOSE_UI = 50045,
        CMD_KT_TOGGLE_HORSE_STATE = 50046,
        CMD_KT_EQUIP_ENHANCE = 50047,
        CMD_KT_COMPOSE_CRYSTALSTONES = 50048,
        CMD_KT_SPLIT_CRYSTALSTONES = 50049,
        CMD_KT_G2C_NEW_DYNAMICAREA = 50050,
        CMD_KT_G2C_DEL_DYNAMICAREA = 50051,
        CMD_KT_ASK_ACTIVEFIGHT = 50052,
        CMD_KT_G2C_START_ACTIVEFIGHT = 50053,
        CMD_KT_G2C_STOP_ACTIVEFIGHT = 50054,
        CMD_KT_CHANGE_AVARTA = 50055,
        CMD_KT_G2C_UPDATE_ROLE_GATHERMAKEPOINT = 50056,
        CMD_KT_G2C_UPDATE_LIFESKILL_LEVEL = 50057,
        CMD_KT_BEGIN_CRAFT = 50058,
        CMD_KT_G2C_FINISH_CRAFT = 50059,
        CMD_KT_SHOW_MESSAGEBOX = 50060,
        CMD_KT_EVENT_NOTIFICATION = 50061,
        CMD_KT_KILLSTREAK = 50062,
        CMD_KT_EVENT_STATE = 50063,
        CMD_KT_SONGJINBATTLE_RANKING = 50064,
        CMD_KT_BROWSE_PLAYER = 50065,
        CMD_KT_CHECK_PLAYER_LOCATION = 50066,
        CMD_KT_UPDATE_TITLE = 50067,
        CMD_KT_UPDATE_NAME = 50068,
        CMD_KT_UPDATE_REPUTE = 50069,
        CMD_KT_UPDATE_TOTALVALUE = 50070,
        CMD_KT_GET_BONUS_DOWNLOAD = 50071,
        CMD_KT_SEASHELL_CIRCLE = 50072,


        #region Bot
        /// <summary>
        /// Có Bot mới
        /// </summary>
        CMD_KT_G2C_NEW_BOT = 50073,
        /// <summary>
        /// Xóa bot
        /// </summary>
        CMD_KT_G2C_DEL_BOT = 50074,
        #endregion


        CMD_KT_SIGNET_ENHANCE = 50075,
        CMD_KT_QUERY_PLAYERRANKING = 50076,
        CMD_KT_C2G_CHANGE_SUBSET = 50077,
        CMD_KT_SHOW_INPUTITEMS = 50078,
        CMD_KT_SHOW_INPUTEQUIPANDMATERIALS = 50079,
        CMD_KT_FACTION_PVP_RANKING_INFO = 50081,

        CMD_KT_YOULONG = 50095,
        CMD_KT_GET_PLAYER_INFO = 50099,


        #region Bang hội
        /// <summary>
        /// Tạo bang
        /// </summary>
        CMD_KT_GUILD_CREATE = 50100,

        /// <summary>
        /// Danh sách thành viên bang hội
        /// </summary>
        CMD_KT_GUILD_GETMEMBERLIST = 50102,

        /// <summary>
        /// Thay đổi chức vị thành viên bang hội
        /// </summary>
        CMD_KT_GUILD_CHANGERANK = 50103,

        /// <summary>
        /// Trục xuất thành viên bang hội
        /// </summary>
        CMD_KT_GUILD_KICK_ROLE = 50104,

        /// <summary>
        /// Cập nhật chức vị bang hội
        /// </summary>
        CMD_KT_UPDATE_GUILDRANK = 50122,

        /// <summary>
        /// Lấy ra thông tin cơ bản của bang hội
        /// Chỉ sử dụng để giao tiếp với gamedb
        /// </summary>
        CMD_KT_GUILD_GETMINI_INFO = 50125,

        /// <summary>
        /// Lấy ra thông tin bang hội
        /// </summary>
        CMD_KT_GUILD_GETINFO = 50101,

        /// <summary>
        /// Lấy thông tin tài nguyên bang hội
        /// </summary>
        CMD_KT_GUILD_GET_RESOURCE_INFO = 50126,

        /// <summary>
        /// Set thông tin tài nguyên bang hội
        /// </summary>
        CMD_KT_GUILD_SET_RESOURCE_INFO = 50127,

        /// <summary>
        /// Lấy thông tin tài nguyên nhân vật
        /// </summary>
        CMD_KT_GUILD_GET_ROLE_RESOURCE_INFO = 50128,

        /// <summary>
        /// Set tài nguyên cho nhân vật
        /// </summary>
        CMD_KT_GUILD_SET_ROLERESOURCE_INFO = 50129,

        /// <summary>
        /// Trả về danh sách xin vào bang
        /// </summary>
        CMD_KT_GUILD_REQUEST_JOIN_LIST = 60000,

        /// <summary>
        /// Trả về danh sách kỹ năng cho bang
        /// </summary>
        CMD_KT_GUILD_SKILL_LIST = 60001,

        /// <summary>
        /// Gói tin cập nhật nhiệm vụ bang hội
        /// </summary>

        CMD_KT_GUILD_TASKUPDATE = 60002,

        /// <summary>
        /// Lấy ra thông tin nhiệm vụ
        /// </summary>
        CMD_KT_GUILD_QUEST_LIST = 60003,

        /// <summary>
        /// Danh sách các bang hội khác sử dụng để xin vào bang
        /// </summary>
        CMD_KT_GUILD_OTHER_LIST = 60004,

        /// <summary>
        /// Xin vào bang
        /// </summary>
        CMD_KT_GUILD_REQUEST_JOIN = 60005,

        /// <summary>
        /// Duyệt hay ko duyệt thằng xin vào bang
        /// </summary>
        CMD_KT_GUILD_RESPONSE_JOINREQUEST = 60006,

        /// <summary>
        /// Cống hiến cái j đó
        /// </summary>
        CMD_KT_GUILD_DONATE = 50108,

        /// <summary>
        /// Thăng cấp bang hội
        /// </summary>
        CMD_KT_GUILD_LEVELUP = 60007,

        /// <summary>
        /// Thăng cấp kỹ năng bang hội
        /// </summary>
        CMD_KT_GUILD_SKILL_LEVELUP = 60008,

        /// <summary>
        /// Nhiệm vụ bang hội
        /// </summary>
        CMD_KT_GUILD_QUEST_CMD = 60009,

        /// <summary>
        /// Sửa công cáo bang hội
        /// </summary>
        CMD_KT_GUILD_CHANGE_NOTIFY = 60011,

        /// <summary>
        /// Thoát khỏi bang hội
        /// </summary>
        CMD_KT_GUILD_QUIT = 60012,

        /// <summary>
        /// gói tin lấy ra thông tin công thành
        /// </summary>
        CMD_KT_GUILD_WARINFO = 60013,

        /// <summary>
        /// gói tin đăng ký công thành
        /// </summary>
        CMD_KT_GUILD_WAR_REGISTER = 60014,

        /// <summary>
        /// Gói tin mở shop
        /// </summary>
        CMD_KT_GUILD_OPENSHOP = 60015,

        /// <summary>
        /// 1 thằng người chơi xin vào 1 bang hội
        /// </summary>
        CMD_KT_GUILD_ASKJOIN = 50116,

        /// <summary>
        /// Thằng bagn chủ trả lời xin gia nhập
        /// </summary>
        CMD_KT_GUILD_RESPONSEASK = 50117,

        /// <summary>
        /// Mời 1 thằng vào bang hội
        /// </summary>
        CMD_KT_GUILD_INVITE = 50118,


        /// <summary>
        /// Trả lời thằng bang chủ có vào bang hay không
        /// </summary>
        CMD_KT_GUILD_RESPONSEINVITE = 50119,

        /// <summary>
        /// Thiết lập tự duyệt vào bang
        /// </summary>
        CMD_KT_GUILD_AUTO_ACCPECT_SETTING = 60010,
        #endregion


        CMD_KT_UPDATE_CURRENT_ROLETITLE = 50130,
        CMD_KT_G2C_MOD_ROLETITLE = 50131,
        CMD_KT_G2C_RECHAGE = 50132,
        CMD_KT_G2C_UPDATE_PRESTIGE_AND_HONOR = 50133,
        CMD_KT_G2C_UPDATE_OTHERROLE_EQUIP = 50134,
        CMD_KT_G2C_PLAYERPRAY = 50135,
        CMD_KT_CLIENT_SERVER_LUA = 50136,
        CMD_KT_CLIENT_DO_REFINE = 50137,
        CMD_KT_C2G_SPLIT_EQUIP_INTO_FS = 50138,
        CMD_KT_TAKEDAMAGE = 50139,
        CMD_KT_UPDATE_ROLEGUILDMONEY = 50141,
        CMD_KT_RANKING_CHECKING = 50142,
        CMD_KT_GETMARKVALUE = 50143,
        CMD_KT_UPDATEMARKVALUE = 50144,
        CMD_KT_GET_RECORE_BYTYPE = 50145,
        CMD_KT_ADD_RECORE_BYTYPE = 50146,
        CMD_KT_GETRANK_RECORE_BYTYPE = 50147,

        /// <summary>
        /// Vòng quay may mắn - đặc biệt
        /// </summary>
        CMD_KT_TURNPLATE = 50148,

        CMD_KT_GUILD_ALLTERRITORY = 50149,
        CMD_KT_UPDATE_LOCALMAP_MONSTER = 50150,
        CMD_KT_GUILD_UPDATE_TERRITORY = 50151,
        CMD_KT_GUILD_GETMINIGUILDINFO = 50152,
        CMD_KT_CAPTCHA = 50153,
        CMD_KT_LUCKYCIRCLE = 50154,

        /// <summary>
        /// Lấy thông tin của quà điểm danh
        /// </summary>
        CMD_KT_CHECKPOINT_INFO = 50155,
        /// <summary>
        /// Điểm danh ngày
        /// </summary>
        CMD_KT_CHECKPOINT_GETAWARD = 50156,


        /// <summary>
        /// Packet lấy ra top bảng xếp hạng
        /// </summary>
        CMD_KT_TOPRANKING = 50157,

        /// <summary>
        /// Packet lấy ra thông tin bảng xếp hạng
        /// </summary>
        CMD_KT_TOPRANKING_INFO = 50158,

        /// <summary>
        /// Packet nhận thưởng
        /// </summary>
        CMD_KT_TOPRANKING_GETAWARD = 50159,


        /// <summary>
        /// Packet đánh dấu vào DB là thằng này đã bú quà top rồi
        /// </summary>
        CMD_KT_UPDATE_REVICE_STATUS = 50160,

        #region Nhập mật khẩu cấp 2

        /// <summary>
        /// Thông báo nhập mật khẩu cấp 2
        /// </summary>
        CMD_KT_DO_SECONDPASSWORD_CMD = 50161,

        /// <summary>
        /// Gói tin gửi lên DB cập nhật mật khẩu cấp 2
        /// </summary>
        CMD_KT_DB_UPDATE_SECONDPASSWORD = 50162,

        #endregion Nhập mật khẩu cấp 2

        #region Tu luyện kỹ năng
        /// <summary>
        /// Gói tin truy vấn danh sách kỹ năng tu luyện
        /// </summary>
        CMD_KT_GET_EXPSKILLS = 50163,

        /// <summary>
        /// Gói tin thiết lập kỹ năng tu luyện
        /// </summary>
        CMD_KT_SET_EXPSKILL = 50164,
        #endregion

        #region Phong Hỏa Liên Thành
        /// <summary>
        /// Gói tin truy vấn danh sách xếp hạng Phong Hỏa Liên Thành
        /// </summary>
        CMD_KT_FHLC_SCOREBOARD = 50165,
        #endregion

        #region Bot bán hàng
        /// <summary>
        /// Gói tin thông báo có Bot bán hàng mới
        /// </summary>
        CMD_KT_NEW_STALLBOT = 50166,

        /// <summary>
        /// Gói tin thông báo xóa Bot bán hàng
        /// </summary>
        CMD_KT_DEL_STALLBOT = 50167,
        #endregion
    };

    /// <summary>
    /// Quản lý TCPPacket
    /// </summary>
    public class TCPCmdHandler
    {
        /// <summary>
        /// Xử lý gói tin tử Server gửi về
        /// </summary>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static bool ProcessServerCmd(TCPClient client, int nID, byte[] data, int count)
        {
            if (nID == (int) (TCPGameServerCmds.CMD_SPR_CLIENTHEART) || nID == (int) (TCPGameServerCmds.CMD_SPR_POSITION))
            {
                TCPPing.RecordRecCmd(nID);
            }

            bool ret = false;
            switch (nID)
            {
                // ------------------------ 数据流 -----------------------
                case (int) (TCPLoginServerCmds.CMD_LOGIN_ON1):
                case (int) (TCPLoginServerCmds.CMD_LOGIN_ON2):
                {
                    ret = ProcessUserInfoCmd(client, nID, data, count);
                    break;
                }

                case (int) (TCPGameServerCmds.CMD_LOGIN_ON):
                case (int) (TCPGameServerCmds.CMD_ROLE_LIST):
                case (int) (TCPGameServerCmds.CMD_CREATE_ROLE):
                case (int) (TCPGameServerCmds.CMD_PLAY_GAME):
                case (int) (TCPGameServerCmds.CMD_SPR_UPDATE_DYNAMIC_OBJECT_LABELS):
                case (int) (TCPGameServerCmds.CMD_SPR_POSITION):
                case (int) TCPGameServerCmds.CMD_SPR_LEVEL_CHANGED:
                case (int) (TCPGameServerCmds.CMD_LOG_OUT):
                case (int) (TCPGameServerCmds.CMD_SPR_EXECUTERECOMMENDPROPADDPOINT):
                case (int) (TCPGameServerCmds.CMD_SPR_LEAVE):
                case (int) (TCPGameServerCmds.CMD_SPR_NPC_BUY):
                case (int) (TCPGameServerCmds.CMD_SPR_MONEYCHANGE):
                case (int) (TCPGameServerCmds.CMD_SPR_MODTASK):
                case (int) (TCPGameServerCmds.CMD_SPR_EXPCHANGE):
                case (int) (TCPGameServerCmds.CMD_SPR_DELGOODSPACK):
                case (int) (TCPGameServerCmds.CMD_SPR_CHGPKMODE):
                case (int) (TCPGameServerCmds.CMD_SPR_CHGPKVAL):
                case (int) (TCPGameServerCmds.CMD_SPR_UPDATENPCSTATE):
                case (int) (TCPGameServerCmds.CMD_SPR_ABANDONTASK):
                case (int) (TCPGameServerCmds.CMD_SPR_CHANGEPOS):
                case (int) (TCPGameServerCmds.CMD_SPR_NOTIFYCHGMAP):
                case (int) (TCPGameServerCmds.CMD_SPR_FORGE):
                case (int) (TCPGameServerCmds.CMD_SPR_MERGE_GOODS):
                case (int) (TCPGameServerCmds.CMD_SPR_TokenCHANGE):
                case (int) (TCPGameServerCmds.CMD_SPR_GOODSEXCHANGE):
                case (int) (TCPGameServerCmds.CMD_SPR_MOVEGOODSDATA):
                case (int) (TCPGameServerCmds.CMD_SPR_DEAD):
                case (int) (TCPGameServerCmds.CMD_SPR_QUERYIDBYNAME):
                case (int) (TCPGameServerCmds.CMD_SPR_SALEGOODS2):
                case (int) (TCPGameServerCmds.CMD_SPR_MARKETBUYGOODS):
                case (int) (TCPGameServerCmds.CMD_SPR_MARKETBUYGOODS2):
                case (int) (TCPGameServerCmds.CMD_SPR_UPGRADE_CHENGJIU):
                case (int) (TCPGameServerCmds.CMD_SPR_GETSONGLIGIFT):
                case (int) (TCPGameServerCmds.CMD_SPR_CHGHUODONGID):
                case (int) (TCPGameServerCmds.CMD_SPR_CHGHALFBoundTokenPERIOD):
                case (int) (TCPGameServerCmds.CMD_SPR_FETCHMAILGOODS):
                case (int) (TCPGameServerCmds.CMD_SPR_DELETEUSERMAIL):
                case (int) (TCPGameServerCmds.CMD_SPR_DELNPC):
                case (int) (TCPGameServerCmds.CMD_SPR_GETUPLEVELGIFTOK):
                case (int) (TCPGameServerCmds.CMD_SPR_GETEVERYDAYONLINEAWARDGIFT):          // 领取每日在线奖励 [1/12/2014 LiaoWei]
                case (int) (TCPGameServerCmds.CMD_SPR_GETEVERYDAYSERIESLOGINAWARDGIFT):    // 领取连续登陆奖励 [1/12/2014 LiaoWei]
                case (int) (TCPGameServerCmds.CMD_KT_CHECKPOINT_GETAWARD):                 // 领取累计登陆奖励 [2/11/2014 LiaoWei]
                case (int) (TCPGameServerCmds.CMD_SPR_MARKETSALEMONEY2):
                case (int) (TCPGameServerCmds.CMD_SPR_CHECK):                              // 与服务器心跳，每两秒发一次，
                case (int) (TCPGameServerCmds.CMD_SPR_GET_WANMOTA_DETAIL):                 // 请求万魔塔信息 [6/9/2014 DuHai]
                case (int) (TCPGameServerCmds.CMD_SPR_SWEEP_WANMOTA):                      // 扫荡万魔塔 [6/9/2014 DuHai]
                case (int) (TCPGameServerCmds.CMD_SPR_GET_SWEEP_REWARD):                   // 获取万魔塔奖励 [6/9/2014 DuHai]
                case (int) (TCPGameServerCmds.CMD_SPR_GET_REPAYACTIVEAWARD):				  // 领取充值奖励 [liubaiqiang]
                case (int) (TCPGameServerCmds.CMD_SPR_GETUPLEVELGIFTAWARD):                //领取等级奖励
                case (int) (TCPGameServerCmds.CMD_SPR_QUERY_ALLREPAYACTIVEINFO):
                case (int) (TCPGameServerCmds.CMD_SPR_PUSH_VERSION):                     //服务器发送版本号验证消息
                case (int) (TCPGameServerCmds.CMD_SPR_GET_STORE_MONEY):              // 存取仓库绑定金币
                case (int) (TCPGameServerCmds.CMD_SPR_STORE_MONEY_CHANGE):           // 通知客户端仓库绑定金币改变
                case (int) (TCPGameServerCmds.CMD_SPR_WING_ZHUHUN):               //处理注魂命令
                case (int) (TCPGameServerCmds.CMD_SECOND_PASSWORD_SET):              //设置密码
                case (int) (TCPGameServerCmds.CMD_SPR_GET_YUEKA_AWARD):              //月卡获取奖励请求回复
                case (int) (TCPGameServerCmds.CMD_SPR_TASKLIST_KEY):
                case (int) (TCPGameServerCmds.CMD_SPR_LOGIN_WAITING_INFO):
                case (int) (TCPGameServerCmds.CMD_SYNC_CHANGE_DAY_SERVER):         //服务器通知客户端跨天了
                case (int) (TCPGameServerCmds.CMD_NTF_MAGIC_CRASH_UNITY)://服务器通知客户端退出客户端 
                {    //字符串消息                                                               
                    ret = ProcessGameCmd(client, nID, data, count);
                    break;
                }
                // ------------------------实例化 对象 -----------------------
                case (int) (TCPGameServerCmds.CMD_SPR_USEGOODS):
                case (int) (TCPGameServerCmds.CMD_SPR_CLIENTHEART):
                case (int) (TCPGameServerCmds.CMD_SPR_MAPCHANGE):
                case (int) (TCPGameServerCmds.CMD_SPR_COMPTASK):
                case (int) (TCPGameServerCmds.CMD_SPR_MOD_GOODS):
                case (int) (TCPGameServerCmds.CMD_INIT_GAME):
                case (int) (TCPGameServerCmds.CMD_OTHER_ROLE):
                case (int) (TCPGameServerCmds.CMD_OTHER_ROLE_DATA):
                case (int) (TCPGameServerCmds.CMD_SYSTEM_MONSTER):
                case (int) (TCPGameServerCmds.CMD_SPR_NEWTASK):
                case (int) (TCPGameServerCmds.CMD_SPR_EXCHANGEDATA):
                case (int) (TCPGameServerCmds.CMD_GETGOODSLISTBYSITE):
                case (int) (TCPGameServerCmds.CMD_SPR_NPCSTATELIST):
                case (int) (TCPGameServerCmds.CMD_SPR_SELFSALEGOODSLIST2):
                case (int) (TCPGameServerCmds.CMD_SPR_RESETBAG):
                case (int) (TCPGameServerCmds.CMD_SPR_DAILYTASKDATA):
                case (int) (TCPGameServerCmds.CMD_SPR_PORTABLEBAGDATA):
                case (int) (TCPGameServerCmds.CMD_SPR_RESETPORTABLEBAG):
                case (int) (TCPGameServerCmds.CMD_SPR_GETHUODONGDATA):
                case (int) (TCPGameServerCmds.CMD_SPR_GETPAIHANGLIST):
                case (int) (TCPGameServerCmds.CMD_SPR_GETUSERMAILLIST):
                case (int) (TCPGameServerCmds.CMD_SPR_GETUSERMAILDATA):
                case (int) (TCPGameServerCmds.CMD_SPR_NEWNPC):
                case (int) (TCPGameServerCmds.CMD_SPR_CHGCODE): //换装挪到这里来
                case (int) (TCPGameServerCmds.CMD_SPR_GETBAITANLOG):                       // 领取战盟建筑的buffer         
                case (int) (TCPGameServerCmds.CMD_SPR_MOVE):
                case (int) (TCPGameServerCmds.CMD_SPR_RELIFE):
                case (int) (TCPGameServerCmds.CMD_SPR_UPDATE_ROLEDATA):
                case (int) (TCPGameServerCmds.CMD_SPR_MARKETGOODSLIST2):                 // 交易所返回的物品列表 [8/1/2014 lt]        
                case (int) (TCPGameServerCmds.CMD_SPR_REALIVE):                          //复活指令改为data数据格式
                case (int) (TCPGameServerCmds.CMD_SPR_LOADALREADY):                      //改为data数据格式
                case (int) (TCPGameServerCmds.CMD_SPR_NEWGOODSPACK):                     //改为data数据格式
                case (int) (TCPGameServerCmds.CMD_SPR_ADD_GOODS):                     //改为data数据格式
                case (int) (TCPGameServerCmds.CMD_SPR_KF_SWITCH_SERVER):          //切换服务器
                case (int) (TCPGameServerCmds.CMD_SPR_CHANGE_NAME):                         //    客户端向服务器发送 角色请求改名
                case (int) (TCPGameServerCmds.CMD_SPR_YONGZHEZHANCHANG_JOIN):    // 勇者战场
                case (int) (TCPGameServerCmds.CMD_SPR_YONGZHEZHANCHANG_ENTER):    // 勇者战场
                case (int) (TCPGameServerCmds.CMD_SPR_YONGZHEZHANCHANG_STATE):
                case (int) (TCPGameServerCmds.CMD_SPR_YONGZHEZHANCHANG_AWARD):
                case (int) (TCPGameServerCmds.CMD_SPR_YONGZHEZHANCHANG_AWARD_GET):
                case (int) (TCPGameServerCmds.CMD_SPR_KUAFU_MAP_ENTER):         // 跨服主线地图进入
                case (int) (TCPGameServerCmds.CMD_SPR_KUAFU_MAP_INFO):  // 跨服主线地图
                case (int) (TCPGameServerCmds.CMD_SPR_NOTIFYGOODSINFO):
                {
                    ret = ProcessGameStreamCmd(client, nID, data, count);
                    break;
                }

                #region Kiếm Thế
                #region RoleAttribute
                case ((int) TCPGameServerCmds.CMD_KT_ROLE_ATRIBUTES):
                {
                    ret = ProcessGameStreamCmd(client, nID, data, count);
                    break;
                }
                #endregion

                #region NotificationTip
                case (int) TCPGameServerCmds.CMD_KT_SHOW_NOTIFICATIONTIP:
                {
                    ret = ProcessGameStreamCmd(client, nID, data, count);
                    break;
                }
                #endregion

                #region Thông báo môn phái và nhánh tu luyện thay đổi
                case (int) TCPGameServerCmds.CMD_KT_FACTIONROUTE_CHANGED:
                {
                    ret = ProcessGameStreamCmd(client, nID, data, count);
                    break;
                }
                #endregion

                #region NPC Dialog
                case (int) TCPGameServerCmds.CMD_KT_G2C_NPCDIALOG:
                {
                    ret = ProcessGameStreamCmd(client, nID, data, count);
                    break;
                }
                #endregion

                #region Item Dialog
                case (int) TCPGameServerCmds.CMD_KT_G2C_ITEMDIALOG:
                {
                    ret = ProcessGameStreamCmd(client, nID, data, count);
                    break;
                }
                #endregion

                #region Skill
                case (int) TCPGameServerCmds.CMD_KT_C2G_SET_SKILL_TO_QUICKKEY:
                {
                    ret = ProcessGameCmd(client, nID, data, count);
                    break;
                }

                case (int) TCPGameServerCmds.CMD_KT_G2C_RENEW_SKILLLIST:
                {
                    ret = ProcessGameStreamCmd(client, nID, data, count);
                    break;
                }

                case (int) TCPGameServerCmds.CMD_KT_G2C_USESKILL:
                {
                    ret = ProcessGameStreamCmd(client, nID, data, count);
                    break;
                }

                case (int) TCPGameServerCmds.CMD_KT_C2G_USESKILL:
                {
                    ret = ProcessGameCmd(client, nID, data, count);
                    break;
                }

                case (int) TCPGameServerCmds.CMD_KT_G2C_NOTIFYSKILLCOOLDOWN:
                {
                    ret = ProcessGameStreamCmd(client, nID, data, count);
                    break;
                }

                case (int) TCPGameServerCmds.CMD_KT_G2C_CREATEBULLET:
                {
                    ret = ProcessGameStreamCmd(client, nID, data, count);
                    break;
                }

                case (int) TCPGameServerCmds.CMD_KT_G2C_BULLETEXPLODE:
                {
                    ret = ProcessGameStreamCmd(client, nID, data, count);
                    break;
                }

                case (int) TCPGameServerCmds.CMD_KT_G2C_SKILLRESULT:
                {
                    ret = ProcessGameStreamCmd(client, nID, data, count);
                    break;
                }

                case (int) TCPGameServerCmds.CMD_KT_G2C_SKILLRESULTS:
                {
                    ret = ProcessGameStreamCmd(client, nID, data, count);
                    break;
                }

                case (int) TCPGameServerCmds.CMD_KT_G2C_OBJECTINVISIBLESTATECHANGED:
                {
                    ret = ProcessGameStreamCmd(client, nID, data, count);
                    break;
                }

                case (int) TCPGameServerCmds.CMD_KT_G2C_SPRITEBUFF:
                {
                    ret = ProcessGameStreamCmd(client, nID, data, count);
                    break;
                }

                case (int) TCPGameServerCmds.CMD_KT_G2C_BLINKTOPOSITION:
                {
                    ret = ProcessGameStreamCmd(client, nID, data, count);
                    break;
                }

                case (int) TCPGameServerCmds.CMD_KT_G2C_FLYTOPOSITION:
                {
                    ret = ProcessGameStreamCmd(client, nID, data, count);
                    break;
                }

                case (int) TCPGameServerCmds.CMD_KT_G2C_CREATEBULLETS:
                {
                    ret = ProcessGameStreamCmd(client, nID, data, count);
                    break;
                }

                case (int) TCPGameServerCmds.CMD_KT_G2C_BULLETEXPLODES:
                {
                    ret = ProcessGameStreamCmd(client, nID, data, count);
                    break;
                }

                case (int) TCPGameServerCmds.CMD_KT_G2C_MOVESPEEDCHANGED:
                {
                    ret = ProcessGameStreamCmd(client, nID, data, count);
                    break;
                }

                case (int) TCPGameServerCmds.CMD_KT_G2C_ATTACKSPEEDCHANGED:
                {
                    ret = ProcessGameStreamCmd(client, nID, data, count);
                    break;
                }

                case (int) TCPGameServerCmds.CMD_KT_G2C_CHANGEACTION:
                {
                    ret = ProcessGameStreamCmd(client, nID, data, count);
                    break;
                }

                case (int) TCPGameServerCmds.CMD_KT_C2G_CHANGEACTION:
                {
                    ret = ProcessGameStreamCmd(client, nID, data, count);
                    break;
                }

                case (int) TCPGameServerCmds.CMD_KT_G2C_SPRITESERIESSTATE:
                {
                    ret = ProcessGameStreamCmd(client, nID, data, count);
                    break;
                }

                case (int) TCPGameServerCmds.CMD_KT_G2C_SHOWDEBUGOBJECTS:
                {
                    ret = ProcessGameStreamCmd(client, nID, data, count);
                    break;
                }

                case (int) TCPGameServerCmds.CMD_KT_G2C_SHOWREVIVEFRAME:
                {
                    ret = ProcessGameStreamCmd(client, nID, data, count);
                    break;
                }

                case (int) TCPGameServerCmds.CMD_KT_SPR_NEWTRAP:
                {
                    ret = ProcessGameStreamCmd(client, nID, data, count);
                    break;
                }

                case (int) TCPGameServerCmds.CMD_KT_SPR_DELTRAP:
                {
                    ret = ProcessGameCmd(client, nID, data, count);
                    break;
                }

                case (int) TCPGameServerCmds.CMD_SPR_STOPMOVE:
                {
                    ret = ProcessGameStreamCmd(client, nID, data, count);
                    break;
                }

                case (int) TCPGameServerCmds.CMD_SPR_CHAT:
                {
                    ret = ProcessGameStreamCmd(client, nID, data, count);
                    break;
                }

                case (int) TCPGameServerCmds.CMD_KT_INVITETOTEAM:
                {
                    ret = ProcessGameCmd(client, nID, data, count);
                    break;
                }

                case (int) TCPGameServerCmds.CMD_KT_CREATETEAM:
                {
                    ret = ProcessGameCmd(client, nID, data, count);
                    break;
                }

                case (int) TCPGameServerCmds.CMD_KT_AGREEJOINTEAM:
                {
                    ret = ProcessGameStreamCmd(client, nID, data, count);
                    break;
                }

                case (int) TCPGameServerCmds.CMD_KT_REFUSEJOINTEAM:
                {
                    ret = ProcessGameCmd(client, nID, data, count);
                    break;
                }

                case (int) TCPGameServerCmds.CMD_KT_GETTEAMINFO:
                {
                    ret = ProcessGameStreamCmd(client, nID, data, count);
                    break;
                }

                case (int) TCPGameServerCmds.CMD_KT_KICKOUTTEAMMATE:
                {
                    ret = ProcessGameStreamCmd(client, nID, data, count);
                    break;
                }

                case (int) TCPGameServerCmds.CMD_KT_APPROVETEAMLEADER:
                {
                    ret = ProcessGameCmd(client, nID, data, count);
                    break;
                }

                case (int) TCPGameServerCmds.CMD_KT_REFRESHTEAMMEMBERATTRIBUTES:
                {
                    ret = ProcessGameStreamCmd(client, nID, data, count);
                    break;
                }

                case (int) TCPGameServerCmds.CMD_KT_TEAMMEMBERCHANGED:
                {
                    ret = ProcessGameStreamCmd(client, nID, data, count);
                    break;
                }

                case (int) TCPGameServerCmds.CMD_KT_LEAVETEAM:
                {
                    ret = ProcessGameCmd(client, nID, data, count);
                    break;
                }

                case (int) TCPGameServerCmds.CMD_KT_G2C_UPDATESPRITETEAMDATA:
                {
                    ret = ProcessGameCmd(client, nID, data, count);
                    break;
                }

                case (int) TCPGameServerCmds.CMD_KT_ASKTOJOINTEAM:
                {
                    ret = ProcessGameStreamCmd(client, nID, data, count);
                    break;
                }

                case (int) TCPGameServerCmds.CMD_KT_G2C_RESETSKILLCOOLDOWN:
                {
                    ret = ProcessGameCmd(client, nID, data, count);
                    break;
                }

                case (int) TCPGameServerCmds.CMD_KT_G2C_CLOSENPCITEMDIALOG:
                {
                    ret = ProcessGameCmd(client, nID, data, count);
                    break;
                }

                case (int) TCPGameServerCmds.CMD_KT_ASK_CHALLENGE:
                {
                    ret = ProcessGameCmd(client, nID, data, count);
                    break;
                }

                case (int) TCPGameServerCmds.CMD_KT_DO_CHALLENGE_COMMAND:
                {
                    ret = ProcessGameCmd(client, nID, data, count);
                    break;
                }

                case (int) TCPGameServerCmds.CMD_KT_RECEIVE_CHALLENGE_INFO:
                {
                    ret = ProcessGameStreamCmd(client, nID, data, count);
                    break;
                }
                #endregion

                #region OPENSHOP
                case ((int) TCPGameServerCmds.CMD_KT_C2G_OPENSHOP):
                {
                    ret = ProcessGameStreamCmd(client, nID, data, count);
                    break;
                }

                case (int) (TCPGameServerCmds.CMD_SPR_NPC_SALEOUT):
                {
                    ret = ProcessGameStreamCmd(client, nID, data, count);
                    break;
                }
                #endregion

                #region Grow Point
                case (int) (TCPGameServerCmds.CMD_KT_G2C_NEW_GROWPOINT):
                {
                    ret = ProcessGameStreamCmd(client, nID, data, count);
                    break;
                }

                case (int) (TCPGameServerCmds.CMD_KT_G2C_DEL_GROWPOINT):
                {
                    ret = ProcessGameCmd(client, nID, data, count);
                    break;
                }
                #endregion

                #region Dynamic Area
                case (int) (TCPGameServerCmds.CMD_KT_G2C_NEW_DYNAMICAREA):
                {
                    ret = ProcessGameStreamCmd(client, nID, data, count);
                    break;
                }

                case (int) (TCPGameServerCmds.CMD_KT_G2C_DEL_DYNAMICAREA):
                {
                    ret = ProcessGameCmd(client, nID, data, count);
                    break;
                }
                #endregion

                #region Bot
                case (int) (TCPGameServerCmds.CMD_KT_G2C_NEW_BOT):
                {
                    ret = ProcessGameStreamCmd(client, nID, data, count);
                    break;
                }

                case (int) (TCPGameServerCmds.CMD_KT_G2C_DEL_BOT):
                {
                    ret = ProcessGameCmd(client, nID, data, count);
                    break;
                }
                #endregion

                #region Tuyên chiến
                case (int) (TCPGameServerCmds.CMD_KT_G2C_START_ACTIVEFIGHT):
                {
                    ret = ProcessGameCmd(client, nID, data, count);
                    break;
                }
                case (int) (TCPGameServerCmds.CMD_KT_G2C_STOP_ACTIVEFIGHT):
                {
                    ret = ProcessGameCmd(client, nID, data, count);
                    break;
                }
                #endregion

                #region Progress Bar
                case (int) TCPGameServerCmds.CMD_KT_G2C_UPDATE_PROGRESSBAR:
                {
                    ret = ProcessGameStreamCmd(client, nID, data, count);
                    break;
                }
                #endregion

                #region Kỳ Trân Các
                case (int) TCPGameServerCmds.CMD_KT_OPEN_TOKENSHOP:
                {
                    ret = ProcessGameStreamCmd(client, nID, data, count);
                    break;
                }
                #endregion

                #region Mở/đóng khung bất kỳ
                case (int) TCPGameServerCmds.CMD_KT_G2C_OPEN_UI:
                {
                    ret = ProcessGameStreamCmd(client, nID, data, count);
                    break;
                }
                case (int) TCPGameServerCmds.CMD_KT_G2C_CLOSE_UI:
                {
                    ret = ProcessGameStreamCmd(client, nID, data, count);
                    break;
                }
                #endregion

                #region Chuyển trạng thái cưỡi
                case (int) TCPGameServerCmds.CMD_KT_TOGGLE_HORSE_STATE:
                {
                    ret = ProcessGameCmd(client, nID, data, count);
                    break;
                }
                #endregion

                #region Cường hóa trang bị
                case (int) TCPGameServerCmds.CMD_KT_EQUIP_ENHANCE:
                case (int) TCPGameServerCmds.CMD_KT_COMPOSE_CRYSTALSTONES:
                case (int) TCPGameServerCmds.CMD_KT_SPLIT_CRYSTALSTONES:
                {
                    ret = ProcessGameCmd(client, nID, data, count);
                    break;
                }
                #endregion

                #region Cường hóa Ngũ hành ấn
                case (int) TCPGameServerCmds.CMD_KT_SIGNET_ENHANCE:
                {
                    ret = ProcessGameCmd(client, nID, data, count);
                    break;
                }
                #endregion

                #region Thay đổi Avarta
                case (int) TCPGameServerCmds.CMD_KT_CHANGE_AVARTA:
                {
                    ret = ProcessGameCmd(client, nID, data, count);
                    break;
                }
                #endregion

                #region Thay đổi Tinh hoạt lực, cấp độ kỹ năng sống
                case (int) TCPGameServerCmds.CMD_KT_G2C_UPDATE_ROLE_GATHERMAKEPOINT:
                {
                    ret = ProcessGameCmd(client, nID, data, count);
                    break;
                }
                case (int) TCPGameServerCmds.CMD_KT_G2C_UPDATE_LIFESKILL_LEVEL:
                {
                    ret = ProcessGameCmd(client, nID, data, count);
                    break;
                }
                #endregion

                #region Chế đồ
                case (int) TCPGameServerCmds.CMD_KT_BEGIN_CRAFT:
                {
                    ret = ProcessGameCmd(client, nID, data, count);
                    break;
                }
                case (int) TCPGameServerCmds.CMD_KT_G2C_FINISH_CRAFT:
                {
                    ret = ProcessGameCmd(client, nID, data, count);
                    break;
                }
                #endregion

                #region Bạn bè
                case (int) TCPGameServerCmds.CMD_SPR_GETFRIENDS:
                case (int) TCPGameServerCmds.CMD_SPR_ASKFRIEND:
                case (int) TCPGameServerCmds.CMD_SPR_ADDFRIEND:
                {
                    ret = ProcessGameStreamCmd(client, nID, data, count);
                    break;
                }
                case (int) TCPGameServerCmds.CMD_SPR_REMOVEFRIEND:
                case (int) TCPGameServerCmds.CMD_SPR_REJECTFRIEND:
                {
                    ret = ProcessGameCmd(client, nID, data, count);
                    break;
                }
                #endregion

                #region Message Box
                case (int) TCPGameServerCmds.CMD_KT_SHOW_MESSAGEBOX:
                {
                    ret = ProcessGameStreamCmd(client, nID, data, count);
                    break;
                }
                #endregion

                #region Battle
                case (int) TCPGameServerCmds.CMD_KT_EVENT_NOTIFICATION:
                case (int) TCPGameServerCmds.CMD_KT_KILLSTREAK:
                case (int) TCPGameServerCmds.CMD_KT_EVENT_STATE:
                {
                    ret = ProcessGameStreamCmd(client, nID, data, count);
                    break;
                }
                #endregion

                #region Hoạt động đặc biệt
                case (int) TCPGameServerCmds.CMD_KT_SONGJINBATTLE_RANKING:
                {
                    ret = ProcessGameStreamCmd(client, nID, data, count);
                    break;
                }
                #endregion

                #region Tìm người chơi
                case (int) TCPGameServerCmds.CMD_KT_BROWSE_PLAYER:
                {
                    ret = ProcessGameStreamCmd(client, nID, data, count);
                    break;
                }
                case (int) TCPGameServerCmds.CMD_KT_CHECK_PLAYER_LOCATION:
                case (int) TCPGameServerCmds.CMD_KT_GET_PLAYER_INFO:
                {
                    ret = ProcessGameStreamCmd(client, nID, data, count);
                    break;
                }
                #endregion

                #region Danh hiệu, tên thay đổi
                case (int) TCPGameServerCmds.CMD_KT_UPDATE_TITLE:
                {
                    ret = ProcessGameStreamCmd(client, nID, data, count);
                    break;
                }
                case (int) TCPGameServerCmds.CMD_KT_UPDATE_NAME:
                {
                    ret = ProcessGameStreamCmd(client, nID, data, count);
                    break;
                }
                #endregion

                #region Danh vọng, vinh dự
                case (int) TCPGameServerCmds.CMD_KT_UPDATE_TOTALVALUE:
                {
                    ret = ProcessGameCmd(client, nID, data, count);
                    break;
                }
                case (int) TCPGameServerCmds.CMD_KT_UPDATE_REPUTE:
                {
                    ret = ProcessGameStreamCmd(client, nID, data, count);
                    break;
                }
                #endregion

                #region Phúc lợi
                case (int) (TCPGameServerCmds.CMD_SPR_QUERY_REPAYACTIVEINFO):
                case (int) (TCPGameServerCmds.CMD_SPR_UPDATEEVERYDAYONLINEAWARDGIFTINFO):
                case (int) (TCPGameServerCmds.CMD_SPR_UPDATEEVERYDAYSERIESLOGININFO):
                case (int) (TCPGameServerCmds.CMD_KT_CHECKPOINT_INFO):
                case (int) (TCPGameServerCmds.CMD_SPR_QUERYUPLEVELGIFTINFO):
                case (int) (TCPGameServerCmds.CMD_SPR_GET_YUEKA_DATA):
                {
                    ret = ProcessGameStreamCmd(client, nID, data, count);
                    break;
                }
                #endregion

                #region Quản lý ICON
                case (int) TCPGameServerCmds.CMD_SPR_REFRESH_ICON_STATE:
                {
                    ret = ProcessGameStreamCmd(client, nID, data, count);
                    break;
                }
                #endregion

                #region Sự kiện quà tải tài nguyên lần đầu vào Game
                case (int) TCPGameServerCmds.CMD_KT_GET_BONUS_DOWNLOAD:
                {
                    ret = ProcessGameStreamCmd(client, nID, data, count);
                    break;
                }
                #endregion

                #region Bách Bảo Rương
                case (int) TCPGameServerCmds.CMD_KT_SEASHELL_CIRCLE:
                {
                    ret = ProcessGameCmd(client, nID, data, count);
                    break;
                }
                #endregion

                #region Võ lâm liên đấu
                case (int) TCPGameServerCmds.CMD_DB_TEAMBATTLE:
                {
                    ret = ProcessGameStreamCmd(client, nID, data, count);
                    break;
                }
                #endregion

                #region Ranking
                case (int) TCPGameServerCmds.CMD_KT_QUERY_PLAYERRANKING:
                {
                    ret = ProcessGameStreamCmd(client, nID, data, count);
                    break;
                }
                #endregion

                #region Nhập danh sách vật phẩm
                case (int) TCPGameServerCmds.CMD_KT_SHOW_INPUTITEMS:
                {
                    ret = ProcessGameStreamCmd(client, nID, data, count);
                    break;
                }
                #endregion

                #region Du Long Các
                case (int) TCPGameServerCmds.CMD_KT_YOULONG:
                {
                    ret = ProcessGameCmd(client, nID, data, count);
                    break;
                }
                #endregion

                #region Bang hội
                case (int) TCPGameServerCmds.CMD_KT_GUILD_CREATE:
                case (int) TCPGameServerCmds.CMD_KT_GUILD_CHANGERANK:
                case (int) TCPGameServerCmds.CMD_KT_GUILD_KICK_ROLE:
                case (int) TCPGameServerCmds.CMD_KT_GUILD_AUTO_ACCPECT_SETTING:
                case (int) TCPGameServerCmds.CMD_KT_GUILD_ASKJOIN:
                case (int) TCPGameServerCmds.CMD_KT_GUILD_INVITE:
                case (int) TCPGameServerCmds.CMD_KT_UPDATE_GUILDRANK:
                case (int) TCPGameServerCmds.CMD_KT_GUILD_QUIT:
                {
                    ret = ProcessGameCmd(client, nID, data, count);
                    break;
                }
                case (int) TCPGameServerCmds.CMD_KT_GUILD_GETINFO:
                case (int) TCPGameServerCmds.CMD_KT_GUILD_GETMEMBERLIST:
                case (int) TCPGameServerCmds.CMD_KT_GUILD_SKILL_LIST:
                case (int) TCPGameServerCmds.CMD_KT_GUILD_QUEST_LIST:
                case (int) TCPGameServerCmds.CMD_KT_GUILD_CHANGE_NOTIFY:
                case (int) TCPGameServerCmds.CMD_KT_GUILD_REQUEST_JOIN_LIST:
                case (int) TCPGameServerCmds.CMD_KT_GUILD_OTHER_LIST:
                case (int) TCPGameServerCmds.CMD_KT_GUILD_WARINFO:
                {
                    ret = ProcessGameStreamCmd(client, nID, data, count);
                    break;
                }
                #endregion

                #region Danh hiệu nhân vật
                case (int) TCPGameServerCmds.CMD_KT_UPDATE_CURRENT_ROLETITLE:
                case (int) TCPGameServerCmds.CMD_KT_G2C_MOD_ROLETITLE:
                {
                    ret = ProcessGameCmd(client, nID, data, count);
                    break;
                }
                #endregion

                #region Uy danh và vinh dự võ lâm
                case (int) TCPGameServerCmds.CMD_KT_G2C_UPDATE_PRESTIGE_AND_HONOR:
                {
                    ret = ProcessGameCmd(client, nID, data, count);
                    break;
                }
                #endregion

                #region Thi đấu môn phái
                case (int) TCPGameServerCmds.CMD_KT_FACTION_PVP_RANKING_INFO:
                {
                    ret = ProcessGameStreamCmd(client, nID, data, count);
                    break;
                }
                #endregion

                #region Cập nhật thông tin người chơi khác
                case (int) TCPGameServerCmds.CMD_KT_G2C_UPDATE_OTHERROLE_EQUIP:
                {
                    ret = ProcessGameStreamCmd(client, nID, data, count);
                    break;
                }
                #endregion

                #region Cập nhật thông tin người chơi khác
                case (int) TCPGameServerCmds.CMD_KT_G2C_PLAYERPRAY:
                {
                    ret = ProcessGameCmd(client, nID, data, count);
                    break;
                }
                #endregion

                #region Cập nhật thông tin người chơi khác
                case (int) TCPGameServerCmds.CMD_KT_CLIENT_SERVER_LUA:
                {
                    ret = ProcessGameStreamCmd(client, nID, data, count);
                    break;
                }
                #endregion

                #region Luyện hóa trang bị
                case (int) TCPGameServerCmds.CMD_KT_CLIENT_DO_REFINE:
                {
                    ret = ProcessGameCmd(client, nID, data, count);
                    break;
                }
                #endregion


                #region Nhận thông tin bị tấn công
                case (int) TCPGameServerCmds.CMD_KT_TAKEDAMAGE:
                {
                    ret = ProcessGameCmd(client, nID, data, count);
                    break;
                }
                #endregion

                #region Tách Ngũ Hành Hồn Thạch từ trang bị
                case (int) TCPGameServerCmds.CMD_KT_C2G_SPLIT_EQUIP_INTO_FS:
                {
                    ret = ProcessGameCmd(client, nID, data, count);
                    break;
                }
                #endregion

                #region Nhập Mật khẩu cấp 2
                case (int) TCPGameServerCmds.CMD_KT_DO_SECONDPASSWORD_CMD:
                {
                    ret = ProcessGameCmd(client, nID, data, count);
                    break;
                }
                #endregion

                #region Cập nhật vị trí quái trên bản đồ khu vực
                case (int) TCPGameServerCmds.CMD_KT_UPDATE_LOCALMAP_MONSTER:
                {
                    ret = ProcessGameStreamCmd(client, nID, data, count);
                    break;
                }
                #endregion

                #region Captcha
                case (int) TCPGameServerCmds.CMD_KT_CAPTCHA:
                {
                    ret = ProcessGameStreamCmd(client, nID, data, count);
                    break;
                }
                #endregion

                #region Vòng quay may mắn
                case (int) TCPGameServerCmds.CMD_KT_LUCKYCIRCLE:
                {
                    ret = ProcessGameStreamCmd(client, nID, data, count);
                    break;
                }
                case (int) TCPGameServerCmds.CMD_KT_TURNPLATE:
                {
                    ret = ProcessGameCmd(client, nID, data, count);
                    break;
                }
                #endregion

                #region Pet
                case (int) TCPGameServerCmds.CMD_KT_PET_CHANGEPOS:
                case (int) TCPGameServerCmds.CMD_KT_PET_USE_SKILL:
                case (int) TCPGameServerCmds.CMD_KT_DEL_PET:
                case (int) TCPGameServerCmds.CMD_KT_DO_PET_COMMAND:
                case (int) TCPGameServerCmds.CMD_KT_PET_CHANGENAME:
                case (int) TCPGameServerCmds.CMD_KT_PET_STUDYSKILL:
                case (int) TCPGameServerCmds.CMD_KT_PET_UPDATE_BASE_ATTRIBUTES:
                case (int) TCPGameServerCmds.CMD_KT_PET_RELEASE:
                case (int) TCPGameServerCmds.CMD_KT_PET_UPDATE_LEVEL:
                {
                    ret = ProcessGameCmd(client, nID, data, count);
                    break;
                }
                case (int) TCPGameServerCmds.CMD_KT_NEW_PET:
                case (int) TCPGameServerCmds.CMD_KT_GET_PET_LIST:
                case (int) TCPGameServerCmds.CMD_KT_PET_UPDATE_ATTRIBUTES:
                case (int) TCPGameServerCmds.CMD_KT_PET_GET_PLAYER_LIST:
                case (int) TCPGameServerCmds.CMD_KT_PET_UPDATE_PETLIST:
                {
                    ret = ProcessGameStreamCmd(client, nID, data, count);
                    break;
                }
                #endregion

                #region Đua top
                case (int) TCPGameServerCmds.CMD_KT_TOPRANKING_INFO:
                {
                    ret = ProcessGameStreamCmd(client, nID, data, count);
                    break;
                }
                case (int) TCPGameServerCmds.CMD_KT_TOPRANKING_GETAWARD:
                {
                    ret = ProcessGameCmd(client, nID, data, count);
                    break;
                }
                #endregion

                #region Vận tiêu
                case (int) TCPGameServerCmds.CMD_KT_DEL_TRADER_CARRIAGE:
                case (int) TCPGameServerCmds.CMD_KT_UPDATE_CARGO_CARRIAGE_TASK_STATE:
                {
                    ret = ProcessGameCmd(client, nID, data, count);
                    break;
                }
                case (int) TCPGameServerCmds.CMD_KT_NEW_TRADER_CARRIAGE:
                case (int) TCPGameServerCmds.CMD_KT_NEW_CARGO_CARRIAGE_TASK:
                {
                    ret = ProcessGameStreamCmd(client, nID, data, count);
                    break;
                }
                #endregion

                #region Thiết lập kỹ năng tu luyện
                case (int) TCPGameServerCmds.CMD_KT_GET_EXPSKILLS:
                {
                    ret = ProcessGameStreamCmd(client, nID, data, count);
                    break;
                }
                case (int) TCPGameServerCmds.CMD_KT_SET_EXPSKILL:
                {
                    ret = ProcessGameCmd(client, nID, data, count);
                    break;
                }
                #endregion

                #region Phong Hỏa Liên Thành
                case (int) TCPGameServerCmds.CMD_KT_FHLC_SCOREBOARD:
                {
                    ret = ProcessGameStreamCmd(client, nID, data, count);
                    break;
                }
                #endregion

                #region Bot bán hàng
                case (int) TCPGameServerCmds.CMD_KT_DEL_STALLBOT:
                {
                    ret = ProcessGameCmd(client, nID, data, count);
                    break;
                }
                case (int) TCPGameServerCmds.CMD_KT_NEW_STALLBOT:
                {
                    ret = ProcessGameStreamCmd(client, nID, data, count);
                    break;
                }
                #endregion

                #region Sạp hàng cá nhân
                case (int) (TCPGameServerCmds.CMD_SPR_ROLE_STOP_STALL):
                {
                    ret = ProcessGameCmd(client, nID, data, count);
                    break;
                }
                case (int) (TCPGameServerCmds.CMD_SPR_ROLE_START_STALL):
                case (int) (TCPGameServerCmds.CMD_SPR_STALLDATA):
                case (int) (TCPGameServerCmds.CMD_SPR_GOODSSTALL):
                {
                    ret = ProcessGameStreamCmd(client, nID, data, count);
                    break;
                }
                #endregion

                #region Group Objects
                case (int) TCPGameServerCmds.CMD_NEW_OBJECTS:
                case (int) TCPGameServerCmds.CMD_REMOVE_OBJECTS:
                {
                    ret = ProcessGameStreamCmd(client, nID, data, count);
                    break;
                }
                #endregion
                #endregion

                #region Test
                case (int) TCPGameServerCmds.CMD_KT_TESTPACKET:
                {
                    ret = ProcessGameCmd(client, nID, data, count);
                    break;
                }
                #endregion

                default:
                {
                    break;
                }
            }

            if (!ret)
            {
                ret = true;
                KTDebug.LogError("Client received packet ID = " + (TCPGameServerCmds) nID);
            }

            return ret;
        }

        /// <summary>
        /// Xác thực tài khoản vả mật khẩu
        /// </summary>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        private static bool ProcessUserInfoCmd(TCPClient client, int nID, byte[] data, int count)
        {
            string strData = new UTF8Encoding().GetString(data, 0, count);
            string[] fields = strData.Split(':');
            if (fields.Length < 2)
            {
                return false;
            }

            client.NotifyRecvData(new SocketConnectEventArgs()
            {
                RemoteEndPoint = client.GetRemoteEndPoint(),
                Error = "Success",
                NetSocketType = (int) NetSocketTypes.SOCKT_CMD,
                CmdID = nID,
                fields = fields
            });
            return true;
        }

        /// <summary>
        /// Đọc packet dưới dạng String, ngăn cách bởi ký tự ':'
        /// </summary>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        private static bool ProcessGameCmd(TCPClient client, int nID, byte[] data, int count)
        {
            string strData = new UTF8Encoding().GetString(data, 0, count);
            string[] fields = strData.Split(':');
            client.NotifyRecvData(new SocketConnectEventArgs()
            {
                RemoteEndPoint = client.GetRemoteEndPoint(),
                Error = "Success",
                NetSocketType = (int) NetSocketTypes.SOCKT_CMD,
                CmdID = (int) nID,
                fields = fields
            });
            return true;
        }

        /// <summary>
        /// Đọc Packet dưới dạng chuỗi Byte
        /// </summary>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        private static bool ProcessGameStreamCmd(TCPClient client, int nID, byte[] data, int count)
        {
            byte[] bytesData = new byte[count];
            DataHelper.CopyBytes(bytesData, 0, data, 0, count);
            client.NotifyRecvData(new SocketConnectEventArgs()
            {
                RemoteEndPoint = client.GetRemoteEndPoint(),
                Error = "Success",
                NetSocketType = (int) NetSocketTypes.SOCKT_CMD,
                CmdID = (int) nID,
                fields = null,
                bytesData = bytesData,
            });
            return true;
        }
    }
}
