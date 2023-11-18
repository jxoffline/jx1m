using GameDBServer.Core;
using GameDBServer.DB;
using GameDBServer.Logic;
using GameDBServer.Logic.GuildLogic;
using GameDBServer.Logic.KT_Recore;
using GameDBServer.Logic.Name;
using GameDBServer.Logic.Pet;
using GameDBServer.Logic.StallManager;
using GameDBServer.Logic.SystemParameters;
using GameDBServer.Logic.TeamBattle;
using GameDBServer.Server.Network;
using Server.Data;
using Server.Protocol;
using Server.Tools;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameDBServer.Server
{
    /// <summary>
    ///     Toàn bộ ID gói tin giao tiếp trong trò chơi
    /// </summary>
    public enum TCPGameServerCmds
    {
        /// <summary>
        /// Lấy ra pramenter lưu trong db
        /// </summary>
        CMD_DB_QUERY_SYSPARAM = 25,

        /// <summary>
        /// Gói tin xử lý liên đấu
        /// </summary>
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

        #endregion Group Objects

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
        /// Gói tin gửi từ GS về Client thông báo Pet thay đổi vị trí
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

        #endregion Pet

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

        #endregion Xe tiêu

        /// <summary>
        /// Lấy ra danh sách nhân vật
        /// </summary>
        CMD_ROLE_LIST = 101,

        /// <summary>
        /// Tạo nhân vật mới
        /// </summary>
        CMD_CREATE_ROLE = 102,

        /// <summary>
        /// Gói tin trả lại dữ liệu nhân vật khi nhân vật thực hiện vào game
        /// </summary>
        CMD_INIT_GAME = 104,

        /// <summary>
        /// Gói tin khi nhận một nhiệm vụ mới
        /// </summary>
        CMD_SPR_NEWTASK = 125,

        /// <summary>
        /// Gói tin khi hoàn thành nhiệm vụ
        /// </summary>
        CMD_SPR_COMPTASK = 140,

        /// <summary>
        /// Gói tin lấy ra danh sách bạn
        /// </summary>
        CMD_SPR_GETFRIENDS = 142,

        /// <summary>
        /// Gói tin thêm một bạn mới
        /// </summary>
        CMD_SPR_ADDFRIEND = 143,

        /// <summary>
        /// Gói tin xóa bạn bè
        /// </summary>
        CMD_SPR_REMOVEFRIEND = 144,

        /// <summary>
        /// Gói tin từ bỏ nhiệm vụ
        /// </summary>
        CMD_SPR_ABANDONTASK = 156,

        /// <summary>
        /// Gói tin xử lý chát liên máy chủ
        /// Khi được add vào DB sẽ trả về các GS khác theo ZONEID của gói tin chát được gửi vào
        /// </summary>
        CMD_SPR_CHAT = 159,

        /// <summary>
        /// Lấy thông tin nhân vật của 1 thằng khác
        /// </summary>
        CMD_SPR_GETOTHERATTRIB = 165,

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
        /// Thông báo có thằng xóa sạp hàng
        /// </summary>
        CMD_SPR_ROLE_STOP_STALL = 181,

        #endregion Sạp hàng cá nhân

        /// <summary>
        /// Trả về ID và trạng thái online hoặc offline theo tên nhân vật
        /// </summary>
        CMD_SPR_QUERYIDBYNAME = 197,

        /// <summary>
        /// Trả về danh sách vật phẩm theo kiểu lưu trữ |
        /// 0 : Trên người
        /// 1 : Túi đồ
        /// 2 : Thủ khố
        /// </summary>
        CMD_GETGOODSLISTBYSITE = 206,

        /// <summary>
        /// Lấy ra thông tin nhân vật của thằng khác
        /// </summary>
        CMD_SPR_GETOTHERATTRIB2 = 277,

        /// <summary>
        /// Tìm ra nhân vật ở trong DB theo RoleID
        /// </summary>
        CMD_SPR_SEARCHROLESFROMDB = 312,

        /// <summary>
        /// Lấy ra thông tin của hoàng đế
        /// Cứ để đây có thể sau này sẽ dùng
        /// </summary>
        CMD_SPR_GETHUANGDIROLEDATA = 342,

        /// <summary>
        /// Lấy ra thông tin vật phẩm theo ID
        /// </summary>
        CMD_SPR_GETGOODSBYDBID = 357,

        /// <summary>
        /// Lấy ra danh sách thư hiện tại của nhân vật
        /// </summary>
        CMD_SPR_GETUSERMAILLIST = 365,

        /// <summary>
        /// Lây ra dữ liệu của 1 thư chỉ định theo ID
        /// </summary>
        CMD_SPR_GETUSERMAILDATA = 366,

        /// <summary>
        /// Lấy ra tất cả vật phẩm đính kèm trong thư
        /// </summary>
        CMD_SPR_FETCHMAILGOODS = 367,

        /// <summary>
        /// Thực hiện xóa thư
        /// </summary>
        CMD_SPR_DELETEUSERMAIL = 368,

        /// <summary>
        /// Xem là có tổng bao nhiêu thư để hiện bag arr
        /// </summary>
        CMD_SPR_GETUSERMAILCOUNT = 651,

        /// <summary>
        /// Cập nhật vị trí của nhân vật
        /// </summary>
        CMD_DB_UPDATE_POS = 10001,

        /// <summary>
        /// Cập nhật exp và cấp độ
        /// </summary>
        CMD_DB_UPDATE_EXPLEVEL = 10002,

        /// <summary>
        /// Cập nhật avata
        /// </summary>
        CMD_DB_UPDATE_ROLE_AVARTA = 10003,

        /// <summary>
        /// Thêm bạc cho người chơi
        /// </summary>
        CMD_DB_UPDATEMONEY1_CMD = 10004,

        /// <summary>
        /// Thêm bạc khóa cho người chơi
        /// </summary>
        CMD_DB_UPDATEUSERGOLD_CMD = 10095,

        /// <summary>
        /// Thêm 1 vật phẩm mới
        /// </summary>
        CMD_DB_ADDGOODS_CMD = 10005,

        /// <summary>
        /// Cập nhật 1 vật phẩm
        /// </summary>
        CMD_DB_UPDATEGOODS_CMD = 10006,

        /// <summary>
        /// Cập nhật tiến trình nhiệm vụ
        /// </summary>
        CMD_DB_UPDATETASK_CMD = 10007,

        /// <summary>
        /// Cập nhật chế độ PK
        /// </summary>
        CMD_DB_UPDATEPKMODE_CMD = 10008,

        /// <summary>
        /// Cập nhật chỉ số PK
        /// </summary>
        CMD_DB_UPDATEPKVAL_CMD = 10009,

        /// <summary>
        /// Cập nhật kỹ năng vào ô phím tắt
        /// </summary>
        CMD_DB_UPDATEKEYS = 10010,

        /// <summary>
        /// Thêm đồng cho người chơi
        /// </summary>
        CMD_DB_UPDATEUSERMONEY_CMD = 10011,

        /// <summary>
        /// Thêm đồng khóa cho người chơi
        /// </summary>
        CMD_DB_UPDATEUSERYINLIANG_CMD = 10012,

        /// <summary>
        /// Hàm chuyển vật phẩm từ thằng này sang cho thằng khác
        /// </summary>

        CMD_DB_MOVEGOODS_CMD = 10013,

        /// <summary>
        /// Xử lý  các thao tác ghi logs khi nhân vật online
        /// </summary>
        CMD_DB_ROLE_ONLINE = 10015,

        /// <summary>
        /// Hold connect với role
        /// </summary>
        CMD_DB_ROLE_HEART = 10016,

        /// <summary>
        /// Khi nhân vật offline
        /// </summary>
        CMD_DB_ROLE_OFFLINE = 10017,

        /// <summary>
        /// Lấy ra danh sách chát liên máy chủ
        /// </summary>
        CMD_DB_GET_CHATMSGLIST = 10018,

        /// <summary>
        /// Đăng ký người chơi mới được gửi lên từ SDK
        /// </summary>
        CMD_DB_REGUSERID = 10025,

        /// <summary>
        /// Band người chơi
        /// </summary>
        CMD_DB_BAN_USER = 10026,

        /// <summary>
        /// Band người chơi theo chức năng gì đó
        /// </summary>
        CMD_DB_BAN_USER_BY_TYPE = 10027,

        /// <summary>
        /// Cập nhật thời gian online của người chơi
        /// </summary>
        CMD_DB_UPDATEONLINETIME = 10032,

        /// <summary>
        /// Lấy ra danh sách config của gs
        /// </summary>
        CMD_DB_GAMECONFIGDICT = 10033,

        /// <summary>
        /// Capah nhật config của máy chủ
        /// </summary>
        CMD_DB_GAMECONIFGITEM = 10034,

        /// <summary>
        /// Thêm 1 kỹ năng vào DB
        /// </summary>
        CMD_DB_ADDSKILL = 10036,

        /// <summary>
        /// Cật nhật thông tin kỹ năng vào db
        /// Cấp độ exp vvv
        /// </summary>
        CMD_DB_UPSKILLINFO = 10037,

        /// <summary>
        ///  Cập nhật thông tin thành tích đăng nhập, tích lũy
        /// </summary>
        CMD_DB_UPDATE_WELFARE = 10045,

        /// <summary>
        /// Ghi lại các thông tin của nhân như vinh dự, UY danh vv
        /// Gói tin này có thể cân nhắc bỏ nếu con võ lâm ít thông tin để lưu
        /// </summary>
        CMD_DB_UPDATEROLEDAILYDATA = 10050,

        /// <summary>
        /// CMD lưu trữ lại buff vào trong DB
        /// </summary>
        CMD_DB_UPDATEBUFFERITEM = 10051,

        /// <summary>
        /// Update nhiệm vụ phụ tuyến
        /// </summary>
        CMD_DB_UPDATECZTASKID = 10062,

        /// <summary>
        /// Check số người chơi online
        /// COunt Active role từ trong cache của DB
        /// </summary>
        CMD_DB_GETTOTALONLINENUM = 10063,

        /// <summary>
        /// Cống hiến bang hội
        /// </summary>
        CMD_DB_UPDATEBANGGONG_CMD = 10071,

        /// <summary>
        /// Lấy ra tổng nạp của DB
        /// </summary>
        CMD_DB_QUERYCHONGZHIMONEY = 10083,

        /// <summary>
        /// Gửi thư cho ai đó
        /// </summary>
        CMD_DB_SENDUSERMAIL = 10086,

        /// <summary>
        /// Lấy ra thông tin thư của người chơi
        /// </summary>
        CMD_DB_GETUSERMAILDATA = 10087,

        /// <summary>
        /// Lấy ra xem vật phẩm đã mua hết số lượt chưa
        /// </summary>
        CMD_DB_QUERYLIMITGOODSUSEDNUM = 10089,

        /// <summary>
        /// Xem đã sử dụng hết số lượt trong ngày chưa
        /// Cái này đã có mark trong dayli recory có thể cân nhắc bỏ
        /// </summary>
        CMD_DB_UPDATELIMITGOODSUSEDNUM = 10090,

        /// <summary>
        /// Ghi lại các chỉ số trên nhân vật theo roleparam
        /// </summary>
        CMD_DB_UPDATEROLEPARAM = 10100,

        /// <summary>
        /// Lấy ra dữ liệu nạp lần đầu của người chơi
        /// </summary>
        CMD_DB_QUERYFIRSTCHONGZHIBYUSERID = 10110,

        /// <summary>
        /// Ghi lại nhật ký giao dịch
        /// </summary>
        CMD_DB_ADDEXCHANGE1ITEM = 10115,

        /// <summary>
        /// Lấy ra tổng nạp ngày hôm nay
        ///
        /// </summary>
        CMD_DB_QUERYTODAYCHONGZHIMONEY = 10120,

        /// <summary>
        /// Lưu lại nhánh môn phái vào DB
        /// </summary>
        CMD_DB_EXECUTECHANGEOCCUPATION = 10126,

        /// <summary>
        /// Check ra tích lũy nạp,Các hoạt động Tích nạp Tích tiêu
        /// </summary>
        CMD_DB_QUERY_REPAYACTIVEINFO = 10160,

        /// <summary>
        /// Lấy ra quà tặng của các hoạt động tích lũy
        /// </summary>
        CMD_DB_GET_REPAYACTIVEAWARD = 10161,

        /// <summary>
        /// Ghi lại đăng nhập ngày
        /// </summary>
        CMD_DB_UPDATE_ACCOUNT_ACTIVE = 10162,

        /// <summary>
        /// Lưu lại tích tiêu của nhân vật
        /// </summary>
        CMD_DB_SAVECONSUMELOG = 10167,

        /// <summary>
        /// Cất đồng vào kho
        /// </summary>
        CMD_DB_ADD_STORE_YINLIANG = 10173,

        /// <summary>
        /// Thêm bạc vào kho
        /// </summary>
        CMD_DB_ADD_STORE_MONEY = 10174,

        /// <summary>
        /// Lấy danh sách thư theo nhóm
        ///
        /// </summary>
        CMD_DB_REQUESTNEWGMAILLIST = 10177,

        /// <summary>
        /// Sửa thư gửi
        /// </summary>
        CMD_DB_MODIFYROLEGMAIL = 10178,

        /// <summary>
        /// Mua thẻ tháng từ webserice
        /// </summary>
        CMD_DB_ROLE_BUY_YUE_KA_BUT_OFFLINE = 10181,

        /// <summary>
        /// Lấy ra thông tin cơ bản của nhân vật
        /// </summary>
        CMD_DB_QUERY_ROLEMINIINFO = 10220,

        /// <summary>
        /// Check kết nối giữa GS và DB
        /// </summary>
        CMD_DB_ONLINE_SERVERHEART = 11001,

        /// <summary>
        /// Lấy ra ZONEID của gameDB phục vụ cho liên máy chủ
        /// </summary>
        CMD_DB_GET_SERVERID = 11002,

        /// <summary>
        /// Set main task từ lệnh GM
        /// </summary>
        CMD_SPR_GM_SET_MAIN_TASK = 13000,

        /// <summary>
        /// Đổi tên nhân vật
        /// </summary>
        CMD_SPR_CHANGE_NAME = 14001,

        /// <summary>
        /// Cập nhật thông tin từ liên máy chủ
        /// </summary>
        CMD_LOGDB_UPDATE_ROLE_KUAFU_DAY_LOG = 20003,

        /// <summary>
        /// Xóa kỹ năng khỏi DB
        /// </summary>
        CMD_DB_DEL_SKILL = 20101,

        /// <summary>
        /// Báo lỗi khi truy xuất dữ liệu sang toác
        /// </summary>
        CMD_DB_ERR_RETURN = 30767,

        #region Kiếm Thế

        #region CreateRole

        /// <summary>
        /// Lấy danh sách Tân Thủ Thôn
        /// </summary>
        CMD_KT_GET_NEWBIE_VILLAGES = 40000,

        #endregion CreateRole

        #region Skill

        /// <summary>
        /// Gói tin gửi về Client làm mới lại danh sách kỹ năng
        /// </summary>
        CMD_KT_G2C_RENEW_SKILLLIST = 45000,

        /// <summary>
        /// Gói tin gửi về Server yêu cầu cộng điểm kỹ năng của Client
        /// </summary>
        CMD_KT_C2G_SKILL_ADDPOINT = 45001,

        /// <summary>
        /// Gói tin gửi từ Client lên Server lưu thiết lập kỹ năng tay vào khung sử dụng
        /// </summary>
        CMD_KT_C2G_SET_SKILL_TO_QUICKKEY = 45002,

        /// <summary>
        /// Gói tin gửi từ Client lên Server lưu thiết lập và kích hoạt vòng sáng tại ô tương ứng
        /// </summary>
        CMD_KT_C2G_SET_AND_ACTIVATE_AURA = 45003,

        /// <summary>
        /// Gói tin gửi từ Client về Server yêu cầu sử dụng kỹ năng
        /// </summary>
        CMD_KT_C2G_USESKILL = 45009,

        /// <summary>
        /// Gói tin gửi từ Server về Client thông báo đối tượng sử dụng kỹ năng
        /// </summary>
        CMD_KT_G2C_USESKILL = 45010,

        /// <summary>
        /// Gói tin gửi từ Server về Client thông báo kỹ năng thiết lập trạng thái chờ phục hồi
        /// </summary>
        CMD_KT_G2C_NOTIFYSKILLCOOLDOWN = 45011,

        /// <summary>
        /// Gói tin gửi từ Server về Client thông báo tạo đạn
        /// </summary>
        CMD_KT_G2C_CREATEBULLET = 45012,

        /// <summary>
        /// Gói tin gửi từ Server về Client thông báo đạn nổ
        /// </summary>
        CMD_KT_G2C_BULLETEXPLODE = 45013,

        /// <summary>
        /// Gói tin gửi từ Server về Client thông báo đạn nổ
        /// </summary>
        CMD_KT_G2C_SKILLRESULT = 45014,

        /// <summary>
        /// Gói tin gửi từ Server về Client thông báo thao tác Buff
        /// </summary>
        CMD_KT_G2C_SPRITEBUFF = 45015,

        /// <summary>
        /// Gói tin gửi từ Server về Client thông báo đối tượng tốc biến tới vị trí chỉ định
        /// </summary>
        CMD_KT_G2C_BLINKTOPOSITION = 45016,

        /// <summary>
        /// Gói tin gửi từ Server về Client thông báo tạo nhiều tia đạn
        /// </summary>
        CMD_KT_G2C_CREATEBULLETS = 45017,

        /// <summary>
        /// Gói tin gửi từ Server về Client thông báo đạn nổ nhiều vị trí
        /// </summary>
        CMD_KT_G2C_BULLETEXPLODES = 45018,

        /// <summary>
        /// Gói tin gửi từ Server về Client thông báo tốc độ di chuyển của đối tượng thay đổi
        /// </summary>
        CMD_KT_G2C_MOVESPEEDCHANGED = 45019,

        /// <summary>
        /// Gói tin gửi từ Server về Client thông báo đối tượng khinh công tới vị trí chỉ định
        /// </summary>
        CMD_KT_G2C_FLYTOPOSITION = 45020,

        /// <summary>
        /// Gói tin gửi từ Server về Client thông báo trạng thái ngũ hành của đối tượng thay đổi
        /// </summary>
        CMD_KT_G2C_SPRITESERIESSTATE = 45021,

        /// <summary>
        /// Gói tin gửi từ Server về Client thông báo danh sách kết quả kỹ năng
        /// </summary>
        CMD_KT_G2C_SKILLRESULTS = 45022,

        /// <summary>
        /// Gói tin gửi từ Server về Client thông báo trạng thái tàng hình của đối tượng thay đổi
        /// </summary>
        CMD_KT_G2C_OBJECTINVISIBLESTATECHANGED = 45023,

        /// <summary>
        /// Gói tin gửi từ Server về Client thông báo làm mới tất cả thời gian hồi kỹ năng
        /// </summary>
        CMD_KT_G2C_RESETSKILLCOOLDOWN = 45024,

        /// <summary>
        /// Gói tin gửi từ Server về Client thông báo tốc độ xuất chiêu của đối tượng thay đổi
        /// </summary>
        CMD_KT_G2C_ATTACKSPEEDCHANGED = 45025,

        #endregion Skill

        /// <summary>
        /// Lệnh GM
        /// </summary>
        CMD_KT_GM_COMMAND = 50000,

        #region RoleAttributes

        /// <summary>
        /// Lấy thông tin thuộc tính nhân vật
        /// </summary>
        CMD_KT_ROLE_ATRIBUTES = 50001,

        #endregion RoleAttributes

        #region Notification Tips

        /// <summary>
        /// Hiển thị NotificationTip
        /// </summary>
        CMD_KT_SHOW_NOTIFICATIONTIP = 50002,

        #endregion Notification Tips

        #region Faction and Route changed

        /// <summary>
        /// Thông báo môn phái người chơi đã thay đổi
        /// </summary>
        CMD_KT_FACTIONROUTE_CHANGED = 50003,

        #endregion Faction and Route changed

        #region Click NPC

        /// <summary>
        /// Người chơi ấn vào NPC
        /// </summary>
        CMD_KT_CLICKON_NPC = 50004,

        #endregion Click NPC

        #region NPCDialog

        /// <summary>
        /// Server gửi lệnh mở khung NPC Dialog cho Client
        /// </summary>
        CMD_KT_G2C_NPCDIALOG = 50005,

        /// <summary>
        /// Client phản hồi về Server về sự lựa chọn của người chơi vào thành phần trong khung (nếu có)
        /// </summary>
        CMD_KT_C2G_NPCDIALOG = 50006,

        #endregion NPCDialog

        #region Change Action

        /// <summary>
        /// Server gửi lệnh cho Client thay đổi động tác
        /// </summary>
        CMD_KT_G2C_CHANGEACTION = 50007,

        /// <summary>
        /// Client gửi yêu cầu thay đổi động tác cho Server
        /// </summary>
        CMD_KT_C2G_CHANGEACTION = 50008,

        #endregion Change Action

        #region Debug

        /// <summary>
        /// Server gửi lệnh cho Client hiện khối Debug Object ở các vị trí
        /// </summary>
        CMD_KT_G2C_SHOWDEBUGOBJECTS = 50009,

        #endregion Debug

        #region UI

        /// <summary>
        /// Server gửi lệnh cho Client hiện khung hồi sinh
        /// </summary>
        CMD_KT_G2C_SHOWREVIVEFRAME = 50010,

        /// <summary>
        /// Gói tin phản hồi từ Client về Server phương thức hồi sinh được người chơi lựa chọn
        /// </summary>
        CMD_KT_C2G_CLIENTREVIVE = 50011,

        #endregion UI

        #region Trap

        /// <summary>
        /// Gói tin gửi từ Server về Client thông báo có bẫy tại vị trí tương ứng
        /// </summary>
        CMD_KT_SPR_NEWTRAP = 50012,

        /// <summary>
        /// Gói tin gửi từ Server về Client thông báo có bẫy tại vị trí tương ứng
        /// </summary>
        CMD_KT_SPR_DELTRAP = 50013,

        #endregion Trap

        #region Settings

        /// <summary>
        /// Gói tin phản hồi từ Client về Server lưu thiết lập hệ thống
        /// </summary>
        CMD_KT_C2G_SAVESYSTEMSETTINGS = 50014,

        /// <summary>
        /// Gói tin phản hồi từ Client về Server lưu thiết lập Auto
        /// </summary>
        CMD_KT_C2G_SAVEAUTOSETTINGS = 50015,

        #endregion Settings

        #region Team

        /// <summary>
        /// Gói tin thông báo mời vào nhóm
        /// </summary>
        CMD_KT_INVITETOTEAM = 50016,

        /// <summary>
        /// Gói tin yêu cầu tạo nhóm
        /// </summary>
        CMD_KT_CREATETEAM = 50017,

        /// <summary>
        /// Gói tin đồng ý thêm vào nhóm tương ứng
        /// </summary>
        CMD_KT_AGREEJOINTEAM = 50018,

        /// <summary>
        /// Gói tin từ chối thêm vào nhóm tương ứng
        /// </summary>
        CMD_KT_REFUSEJOINTEAM = 50019,

        /// <summary>
        /// Gói tin lấy thông tin nhóm tương ứng
        /// </summary>
        CMD_KT_GETTEAMINFO = 50020,

        /// <summary>
        /// Gói tin trục xuất người chơi khỏi nhóm
        /// </summary>
        CMD_KT_KICKOUTTEAMMATE = 50021,

        /// <summary>
        /// Gói tin bổ nhiệm đội trưởng
        /// </summary>
        CMD_KT_APPROVETEAMLEADER = 50022,

        /// <summary>
        /// Gói tin gửi từ Server về Client thông báo thay đổi thông tin đội viên
        /// </summary>
        CMD_KT_REFRESHTEAMMEMBERATTRIBUTES = 50023,

        /// <summary>
        /// Gói tin gửi từ Server về Client thông báo thành viên thay đổi
        /// </summary>
        CMD_KT_TEAMMEMBERCHANGED = 50024,

        /// <summary>
        /// Gói tin thông báo bản thân rời nhóm
        /// </summary>
        CMD_KT_LEAVETEAM = 50025,

        /// <summary>
        /// Gói tin gửi từ Server về Client thông báo cập nhật thông tin tổi đội của người chơi tương ứng
        /// </summary>
        CMD_KT_G2C_UPDATESPRITETEAMDATA = 50026,

        /// <summary>
        /// Gói tin thông báo yêu cầu xin vào nhóm của người chơi tương ứng
        /// </summary>
        CMD_KT_ASKTOJOINTEAM = 50027,

        #endregion Team

        #region ItemDialog

        /// <summary>
        /// Server gửi lệnh mở khung Item Dialog cho Client
        /// </summary>
        CMD_KT_G2C_ITEMDIALOG = 50030,

        /// <summary>
        /// Client phản hồi về Server về sự lựa chọn của người chơi vào thành phần trong khung Item Dialog (nếu có)
        /// </summary>
        CMD_KT_C2G_ITEMDIALOG = 50031,

        #endregion ItemDialog

        #region SHOPCMD

        /// <summary>
        /// Gói tin gửi từ Client lên Server yêu cầu mở Shop tương ứng
        /// </summary>
        CMD_KT_C2G_OPENSHOP = 50032,

        #endregion SHOPCMD

        /// <summary>
        /// Server lệnh cho Client đóng khung NPCDialog hoặc ItemDialog
        /// </summary>
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

        #endregion Thách đấu

        #region Auto Path

        /// <summary>
        /// Gói tin gửi từ Client lên Server do Auto tìm đường gửi, yêu cầu dịch chuyển đến vị trí tương ứng
        /// </summary>
        CMD_KT_C2G_AUTOPATH_CHANGEMAP = 50038,

        #endregion Auto Path

        #region GrowPoint

        /// <summary>
        /// Gói tin gửi từ Server về Client thông báo có điểm thu thập mới
        /// </summary>
        CMD_KT_G2C_NEW_GROWPOINT = 50039,

        /// <summary>
        /// Gói tin gửi từ Server về Client thông báo xóa điểm thu thập
        /// </summary>
        CMD_KT_G2C_DEL_GROWPOINT = 50040,

        /// <summary>
        /// Gói tin gửi từ Client về Server thông báo người chơi ấn vào điểm thu thập
        /// </summary>
        CMD_KT_C2G_GROWPOINT_CLICK = 50041,

        #endregion GrowPoint

        #region Progress Bar

        /// <summary>
        /// Gói tin gửi từ Server về Client thông báo cập nhật trạng thái chạy thanh Progess
        /// </summary>
        CMD_KT_G2C_UPDATE_PROGRESSBAR = 50042,

        #endregion Progress Bar

        #region Kỳ Trân Các

        /// <summary>
        /// Gói tin gửi từ Client lên Server yêu cầu mở Kỳ Trân Các
        /// </summary>
        CMD_KT_OPEN_TOKENSHOP = 50043,

        #endregion Kỳ Trân Các

        #region Mở/đóng khung bất kỳ

        /// <summary>
        /// Gói tin gửi từ Server về Client yêu cầu mở khung bất kỳ
        /// </summary>
        CMD_KT_G2C_OPEN_UI = 50044,

        /// <summary>
        /// Gói tin gửi từ Server về Client yêu cầu đóng khung bất kỳ
        /// </summary>
        CMD_KT_G2C_CLOSE_UI = 50045,

        #endregion Mở/đóng khung bất kỳ

        #region Chuyển trạng thái cưỡi

        /// <summary>
        /// Gói tin thông báo trạng thái cưỡi thay đổi
        /// </summary>
        CMD_KT_TOGGLE_HORSE_STATE = 50046,

        #endregion Chuyển trạng thái cưỡi

        #region Cường hóa trang bị

        /// <summary>
        /// Gói tin cường hóa trang bị
        /// </summary>
        CMD_KT_EQUIP_ENHANCE = 50047,

        /// <summary>
        /// Gói tin ghép Huyền Tinh
        /// </summary>
        CMD_KT_COMPOSE_CRYSTALSTONES = 50048,

        /// <summary>
        /// Gói tin tách Huyền Tinh
        /// </summary>
        CMD_KT_SPLIT_CRYSTALSTONES = 50049,

        #endregion Cường hóa trang bị

        #region Khu vực động

        /// <summary>
        /// Gói tin gửi từ Server về Client thông báo có khu vực động mới
        /// </summary>
        CMD_KT_G2C_NEW_DYNAMICAREA = 50050,

        /// <summary>
        /// Gói tin gửi từ Server về Client thông báo xóa khu vực động
        /// </summary>
        CMD_KT_G2C_DEL_DYNAMICAREA = 50051,

        #endregion Khu vực động

        #region Tuyên chiến

        /// <summary>
        /// Gói tin thông báo tuyên chiến
        /// </summary>
        CMD_KT_ASK_ACTIVEFIGHT = 50052,

        /// <summary>
        /// Gói tin gửi từ Server về Client bắt đầu thiết lập trạng thái tuyên chiến
        /// </summary>
        CMD_KT_G2C_START_ACTIVEFIGHT = 50053,

        /// <summary>
        /// Gói tin gửi từ Server về Client thông báo kết thúc tuyên chiến
        /// </summary>
        CMD_KT_G2C_STOP_ACTIVEFIGHT = 50054,

        #endregion Tuyên chiến

        #region Avarta

        /// <summary>
        /// Gói tin thông báo Avarta nhân vật thay đổi
        /// </summary>
        CMD_KT_CHANGE_AVARTA = 50055,

        #endregion Avarta

        #region Tinh hoạt lực, kỹ năng sống

        /// <summary>
        /// Gói tin gửi từ Server về Client thông báo giá trị Tinh lực, hoạt lực nhân vật thay đổi
        /// </summary>
        CMD_KT_G2C_UPDATE_ROLE_GATHERMAKEPOINT = 50056,

        /// <summary>
        /// Gói tin từ Server gửi về Client thông báo cấp độ kỹ năng sống và kinh nghiệm thay đổi
        /// </summary>
        CMD_KT_G2C_UPDATE_LIFESKILL_LEVEL = 50057,

        #endregion Tinh hoạt lực, kỹ năng sống

        #region Chế đồ

        /// <summary>
        /// Gói tin bắt đầu chế đồ
        /// </summary>
        CMD_KT_BEGIN_CRAFT = 50058,

        /// <summary>
        /// Gói tin kết thúc chế đồ
        /// </summary>
        CMD_KT_G2C_FINISH_CRAFT = 50059,

        #endregion Chế đồ

        #region Message Box

        /// <summary>
        /// Hiển thị bảng thông báo về Client
        /// </summary>
        CMD_KT_SHOW_MESSAGEBOX = 50060,

        #endregion Message Box

        #region BATTLE

        /// <summary>
        /// Thông báo Text sự kiện hoạt động phụ bản
        /// </summary>
        CMD_KT_EVENT_NOTIFICATION = 50061,

        /// <summary>
        /// Thông báo số liên trảm
        /// </summary>
        CMD_KT_KILLSTREAK = 50062,

        /// <summary>
        /// Thông báo trạng thái đóng mở khung Mini sự kiện hoạt động phụ bản
        /// </summary>
        CMD_KT_EVENT_STATE = 50063,

        #endregion BATTLE

        #region Hoạt động đặc biệt

        /// <summary>
        /// Bảng điểm Tống Kim
        /// </summary>
        CMD_KT_SONGJINBATTLE_RANKING = 50064,

        #endregion Hoạt động đặc biệt

        #region Tìm người chơi

        /// <summary>
        /// Gói tin tìm kiếm người chơi
        /// </summary>
        CMD_KT_BROWSE_PLAYER = 50065,

        /// <summary>
        /// Gói tin kiểm tra vị trí người chơi
        /// </summary>
        CMD_KT_CHECK_PLAYER_LOCATION = 50066,

        /// <summary>
        /// Gói tin kiểm tra thông tin người chơi
        /// </summary>
        CMD_KT_GET_PLAYER_INFO = 50099,

        #endregion Tìm người chơi

        #region Danh hiệu

        /// <summary>
        /// Cập nhật hiển thị danh hiệu
        /// </summary>
        CMD_KT_UPDATE_TITLE = 50067,

        /// <summary>
        /// Cập nhật hiển thị tên
        /// </summary>
        CMD_KT_UPDATE_NAME = 50068,

        #endregion Danh hiệu

        #region Danh vọng

        /// <summary>
        /// Cập nhật danh vọng
        /// </summary>
        CMD_KT_UPDATE_REPUTE = 50069,

        /// <summary>
        /// Cập nhật giá trị tài phú
        /// </summary>
        CMD_KT_UPDATE_TOTALVALUE = 50070,

        #endregion Danh vọng

        #region Quà Downlaod

        /// <summary>
        /// Gói tin thao tác với sự kiện tải lần đầu nhận quà
        /// </summary>
        CMD_KT_GET_BONUS_DOWNLOAD = 50071,

        #endregion Quà Downlaod

        #region Bách Bảo Rương

        /// <summary>
        /// Gói tin thao tác với hoạt động Bách Bảo Rương
        /// </summary>
        CMD_KT_SEASHELL_CIRCLE = 50072,

        #endregion Bách Bảo Rương

        #region BOT

        /// <summary>
        /// Gói tin gửi từ Server về Client thông báo có BOT mới
        /// </summary>
        CMD_KT_G2C_NEW_BOT = 50073,

        /// <summary>
        /// Gói tin gửi từ Server về Client thông báo xóa BOT
        /// </summary>
        CMD_KT_G2C_DEL_BOT = 50074,

        #endregion BOT

        #region Cường hóa Ngũ hành ấn

        /// <summary>
        /// Gói tin cường hóa Ngũ hành ấn
        /// </summary>
        CMD_KT_SIGNET_ENHANCE = 50075,

        #endregion Cường hóa Ngũ hành ấn

        #region Bảng xếp hạng

        /// <summary>
        /// Gói tin truy vấn bảng xếp hạng
        /// </summary>
        CMD_KT_QUERY_PLAYERRANKING = 50076,

        #endregion Bảng xếp hạng

        #region Set dự phòng

        /// <summary>
        /// Đổi set dự phòng
        /// </summary>
        CMD_KT_C2G_CHANGE_SUBSET = 50077,

        #endregion Set dự phòng

        #region Khung nhập vật phẩm

        /// <summary>
        /// Nhập danh sách vật phẩm
        /// </summary>
        CMD_KT_SHOW_INPUTITEMS = 50078,

        /// <summary>
        /// Nhập danh sách vật phẩm
        /// </summary>
        CMD_KT_SHOW_INPUTEQUIPANDMATERIALS = 50079,

        #endregion Khung nhập vật phẩm

        #region Thi Đấu môn phái

        /// <summary>
        /// Bảng Điểm Thi Đấu môn Phái
        /// </summary>
        CMD_KT_FACTION_PVP_RANKING_INFO = 50081,

        #endregion Thi Đấu môn phái

        #region Gia tộc

        /// <summary>
        /// Tạo tộc
        /// </summary>
        CMD_KT_FAMILY_CREATE = 50082,

        /// <summary>
        /// Yêu cầu vào tộc
        /// </summary>
        CMD_KT_FAMILY_REQUESTJOIN = 50083,

        /// <summary>
        /// Đuổi thành viên khỏi tộc
        /// </summary>
        CMD_KT_FAMILY_KICKMEMBER = 50084,

        /// <summary>
        /// Lấy danh sách tộc
        /// </summary>
        CMD_KT_FAMILY_GETLISTFAMILY = 50085,

        /// <summary>
        /// Giải tán tộc
        /// </summary>
        CMD_KT_FAMILY_DESTROY = 50086,

        /// <summary>
        /// Đổi tôn chỉ tộc
        /// </summary>
        CMD_KT_FAMILY_CHANGENOTIFY = 50087,

        /// <summary>
        /// Đổi thông báo gia nhập tộc
        /// </summary>
        CMD_KT_FAMILY_CHANGE_REQUESTJOIN_NOTIFY = 50088,

        /// <summary>
        /// Đổi chức vị thành viên
        /// </summary>
        CMD_KT_FAMILY_CHANGE_RANK = 50089,

        /// <summary>
        /// Phản hồi yêu cầu vào tộc
        /// </summary>
        CMD_KT_FAMILY_RESPONSE_REQUEST = 50090,

        /// <summary>
        /// Thoát gia tộc
        /// </summary>
        CMD_KT_FAMILY_QUIT = 50091,

        /// <summary>
        /// Mở giao diện gia tộc
        /// </summary>
        CMD_KT_FAMILY_OPEN = 50092,

        #endregion Gia tộc

        #region Bang hội

        /// <summary>
        /// Tạo 1 bang hội
        /// </summary>
        CMD_KT_GUILD_CREATE = 50100,

        /// <summary>
        /// Lấy thông tin bang hội
        /// </summary>
        CMD_KT_GUILD_GETINFO = 50101,

        /// <summary>
        /// Lấy ra thành viên bang hội
        /// </summary>
        CMD_KT_GUILD_GETMEMBERLIST = 50102,

        /// <summary>
        /// Trả về danh sách xin vào bang
        /// </summary>
        CMD_KT_GUILD_REQUEST_JOIN_LIST = 60000,

        /// <summary>
        /// Trả về danh sách kỹ năng bang
        /// </summary>
        CMD_KT_GUILD_SKILL_LIST = 60001,

        /// <summary>
        /// Gói tin cập nhật nhiệm vụ bang hội
        /// </summary>
        CMD_KT_GUILD_TASKUPDATE = 60002,

        /// <summary>
        /// Xin vào bang
        /// </summary>
        CMD_KT_GUILD_REQUEST_JOIN = 60005,

        CMD_KT_GUILD_RESPONSE_JOINREQUEST = 60006,

        /// <summary>
        /// Thăng cấp bang hội
        /// </summary>
        CMD_KT_GUILD_LEVELUP = 60007,

        CMD_KT_GUILD_SKILL_LEVELUP = 60008,

        CMD_KT_GUILD_QUEST_CMD = 60009,

        CMD_KT_GUILD_AUTO_ACCPECT_SETTING = 60010,

        /// <summary>
        /// Sửa công cáo bang hội
        /// </summary>
        CMD_KT_GUILD_CHANGE_NOTIFY = 60011,

        /// <summary>
        /// Thoát khỏi bang hội
        /// </summary>
        CMD_KT_GUILD_QUIT = 60012,

        /// <summary>
        /// gói tin lấy ra thông tin bang hội
        /// </summary>
        CMD_KT_GUILD_WARINFO = 60013,

        /// <summary>
        /// gói tin đăng ký công thành
        /// </summary>
        CMD_KT_GUILD_WAR_REGISTER = 60014,

        /// <summary>
        /// Thay đổi chức vị của 1 thành viên
        /// Thay đổi chức của 1 thành viên
        /// </summary>
        CMD_KT_GUILD_CHANGERANK = 50103,

        /// <summary>
        /// Kick 1 gia tộc
        /// Packet khi ấn vào ĐUỔI 1 tộc ở khung thành viên
        /// </summary>
        CMD_KT_GUILD_KICK_ROLE = 50104,

        /// <summary>
        /// Trả về danh sách ưu tú
        /// Packet khi mở khung ưu tú
        /// </summary>
        CMD_KT_GUILD_GETGIFTED = 50105,

        /// <summary>
        /// Packet khi mở giao diện quan hàm
        /// </summary>
        CMD_KT_GUILD_OFFICE_RANK = 50106,

        /// <summary>
        /// Packet bầu ưu tú cho 1 thành viên
        /// </summary>

        CMD_KT_GUILD_VOTEGIFTED = 50107,

        /// <summary>
        /// Cống hiến vào bang
        /// </summary>
        CMD_KT_GUILD_DONATE = 50108,

        /// <summary>
        /// Khi click vào khung hoạt động tranh đoạt lãnh thổ
        /// </summary>
        CMD_KT_GUILD_TERRITORY = 50109,

        /// <summary>
        /// PACKET SET THÀNH CHÍNH
        /// </summary>
        CMD_KT_GUILD_SETCITY = 50110,

        /// <summary>
        /// PACKET THIẾT LẬP THUẾ
        /// </summary>
        CMD_KT_GUILD_SETTAX = 50111,

        /// <summary>
        /// ĐIỀU CHỈNH QUỸ THƯỞNG
        /// </summary>
        CMD_KT_GUILD_CHANGE_MAXWITHDRAW = 50113,

        /// <summary>
        /// MỞ UI CỔ TỨC
        /// </summary>
        CMD_KT_GUILD_GETSHARE = 50115,

        /// <summary>
        /// Xin gia nhập bang hội
        /// </summary>
        CMD_KT_GUILD_ASKJOIN = 50116,

        /// <summary>
        /// Trả lời đơn xin gia nhập
        /// </summary>
        CMD_KT_GUILD_RESPONSEASK = 50117,

        /// <summary>
        /// Mời vào bang
        /// </summary>
        CMD_KT_GUILD_INVITE = 50118,

        /// <summary>
        /// Phản hồi yêu cầu mời vào bang
        /// </summary>
        CMD_KT_GUILD_RESPONSEINVITE = 50119,

        /// <summary>
        /// Rút tài sản cá nhân
        /// </summary>
        CMD_KT_GUILD_DOWTIHDRAW = 50120,

        /// <summary>
        /// Gia tộc rời khỏi bang hội
        /// </summary>
        CMD_KT_GUILD_FAMILYQUIT = 50121,

        /// <summary>
        /// Cập nhật thông tin hạng bang hội và gia tộc
        /// </summary>
        CMD_KT_UPDATE_GUILDANDFAMILY_RANK = 50122,

        /// <summary>
        /// Dữ liệu lãnh thổ
        /// </summary>
        CMD_KT_GETTERRORY_DATA = 50123,

        /// <summary>
        /// Lấy thông tin bảng chiến công Tranh đoạt lãnh thổ
        /// </summary>
        CMD_KT_GUILDWAR_RANKING = 50124,

        /// <summary>
        /// Trả về thông tin rút gọn cho gameserver
        /// </summary>
        CMD_KT_GUILD_GETMINI_INFO = 50125,

        CMD_KT_GUILD_GET_RESOURCE_INFO = 50126,

        CMD_KT_GUILD_SET_RESOURCE_INFO = 50127,

   

     

        #endregion Bang hội

        #region Khung Du Long Các

        /// <summary>
        /// Gói tin thông tin Du Long Các
        /// </summary>
        CMD_KT_YOULONG = 50095,

        #endregion Khung Du Long Các

        #region Danh hiệu nhân vật

        /// <summary>
        /// Gói tin thông báo thay đổi danh hiệu nhân vật hiện tại
        /// </summary>
        CMD_KT_UPDATE_CURRENT_ROLETITLE = 50130,

        /// <summary>
        /// Gói tin thông báo thêm/xóa danh hiệu nhân vật
        /// </summary>
        CMD_KT_G2C_MOD_ROLETITLE = 50131,

        #endregion Danh hiệu nhân vật

        /// <summary>
        /// Gói tin gửi lưu tích lũy nạp
        /// </summary>
        CMD_KT_G2C_RECHAGE = 50132,

        #region Uy danh và vinh dự thay đổi

        /// <summary>
        /// Cập nhật thông tin uy danh và vinh dự thay đổi
        /// </summary>
        CMD_KT_G2C_UPDATE_PRESTIGE_AND_HONOR = 50133,

        #endregion Uy danh và vinh dự thay đổi

        #region Thông báo cập nhật thông tin người chơi khác

        /// <summary>
        /// Gói tin cập nhật thông tin trang bị người chơi khác
        /// </summary>
        CMD_KT_G2C_UPDATE_OTHERROLE_EQUIP = 50134,

        #endregion Thông báo cập nhật thông tin người chơi khác

        #region Chúc phúc

        /// <summary>
        /// Chúc phúc
        /// </summary>
        CMD_KT_G2C_PLAYERPRAY = 50135,

        #endregion Chúc phúc

        #region Lua

        /// <summary>
        /// Giao tiếp giữa Lua ở Client với Server
        /// </summary>
        CMD_KT_CLIENT_SERVER_LUA = 50136,

        #endregion Lua

        #region Luyện hóa trang bị

        /// <summary>
        /// Gói tin luyện hóa trang bị
        /// </summary>
        CMD_KT_CLIENT_DO_REFINE = 50137,

        #endregion Luyện hóa trang bị

        #region Tách Ngũ Hành Hồn Thạch từ trang bị

        /// <summary>
        /// Gói tin luyện hóa trang bị
        /// </summary>
        CMD_KT_C2G_SPLIT_EQUIP_INTO_FS = 50138,

        #endregion Tách Ngũ Hành Hồn Thạch từ trang bị

        #region Báo cho đối phương đang bị tấn công,

        /// <summary>
        /// Thông báo đang bị tấn công
        /// </summary>
        CMD_KT_TAKEDAMAGE = 50139,

        #endregion Báo cho đối phương đang bị tấn công,

        #region GuildUpdateMoney

        /// <summary>
        /// Cập nhật tài sản cá nhân
        /// </summary>
        CMD_KT_UPDATE_ROLEGUILDMONEY = 50141,

        #endregion GuildUpdateMoney

        /// <summary>
        /// Kiểm tra thứ hạng bản thân
        /// </summary>
        CMD_KT_RANKING_CHECKING = 50142,

        #region GHICHEP_RECRORE

        /// <summary>
        /// Lấy ra 1 biến đánh dấu theo time ranger
        /// </summary>
        CMD_KT_GETMARKVALUE = 50143,

        /// <summary>
        /// Update 1 biến đánh dấu theo time Ranger
        /// </summary>
        CMD_KT_UPDATEMARKVALUE = 50144,

        /// <summary>
        /// Lấy ra tổng giá trị đã ghi chép trong 1 khoảng thời gain
        /// </summary>
        CMD_KT_GET_RECORE_BYTYPE = 50145,

        /// <summary>
        /// Thêm vào 1 biến ghi chép trong 1 khoảng thời gian
        /// </summary>
        CMD_KT_ADD_RECORE_BYTYPE = 50146,

        /// <summary>
        /// Lấy ra bảng xếp hạng đã ghic hép trong 1 khoảng thời gian
        /// </summary>
        CMD_KT_GETRANK_RECORE_BYTYPE = 50147,

        /// <summary>
        /// Tranh đoạt lãnh thổ
        /// </summary>
        CMD_KT_GUILD_ALLTERRITORY = 50149,

        #endregion GHICHEP_RECRORE

        #region Quái đặc biệt - Bản đồ khu vực

        /// <summary>
        /// Thông tin quái ở bản đồ khu vực
        /// </summary>
        CMD_KT_UPDATE_LOCALMAP_MONSTER = 50150,

        #endregion Quái đặc biệt - Bản đồ khu vực

        #region Captcha

        /// <summary>
        /// Captcha chống BOT
        /// </summary>
        CMD_KT_CAPTCHA = 50153,

        #endregion Captcha

        #region Vòng quay may mắn

        /// <summary>
        /// Vòng quay may mắn - đặc biệt
        /// </summary>
        CMD_KT_TURNPLATE = 50148,

        /// <summary>
        /// Vòng quay may mắn
        /// </summary>
        CMD_KT_LUCKYCIRCLE = 50154,

        #endregion Vòng quay may mắn

        #endregion Kiếm Thế

        #region Bang Hội

        /// <summary>
        /// Cập nhật danh sách lãnh thổ
        /// </summary>
        CMD_KT_GUILD_UPDATE_TERRITORY = 50151,

        #endregion Bang Hội

        #region Test

        /// <summary>
        /// Gói tin Test
        /// </summary>
        CMD_KT_TESTPACKET = 32123,

        #endregion Test

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

        #endregion Tu luyện kỹ năng

        #region Phong Hỏa Liên Thành

        /// <summary>
        /// Gói tin truy vấn danh sách xếp hạng Phong Hỏa Liên Thành
        /// </summary>
        CMD_KT_FHLC_SCOREBOARD = 50165,

        #endregion Phong Hỏa Liên Thành

        #region Bot bán hàng

        /// <summary>
        /// Gói tin thông báo có Bot bán hàng mới
        /// </summary>
        CMD_KT_NEW_STALLBOT = 50166,

        /// <summary>
        /// Gói tin thông báo xóa Bot bán hàng
        /// </summary>
        CMD_KT_DEL_STALLBOT = 50167,

        #endregion Bot bán hàng
    }

    /// <summary>
    /// Cá giao thức kết quả trả về
    /// </summary>
    public enum TCPProcessCmdResults
    {
        RESULT_OK = 0,
        RESULT_FAILED = 1,
        RESULT_DATA = 2,
        RESULT_UNREGISTERED = 3
    }

    /// <summary>
    ///     Lớp xử lý toàn bộ các thông tin liên quan tới TCP HANDLER
    /// </summary>
    internal class TCPCmdHandler
    {
        /// <summary>
        ///  Xử lý gói tin vào
        /// </summary>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static TCPProcessCmdResults ProcessCmd(GameServerClient client, DBManager dbMgr, TCPOutPacketPool pool,
            int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            var result = TCPProcessCmdResults.RESULT_FAILED;
            tcpOutPacket = null;

            var enumDisplayStatus = (TCPGameServerCmds)nID;
            var stringValue = enumDisplayStatus.ToString();

            switch (nID)
            {
                case (int)TCPGameServerCmds.CMD_DB_REGUSERID:
                    {
                        result = ProcessRegUserIDCmd(dbMgr, pool, nID, data, count, out tcpOutPacket);
                        break;
                    }

                case (int)TCPGameServerCmds.CMD_ROLE_LIST:
                    {
                        result = ProcessGetRoleListCmd(dbMgr, pool, nID, data, count, out tcpOutPacket);
                        break;
                    }

                case (int)TCPGameServerCmds.CMD_CREATE_ROLE:
                    {
                        result = ProcessCreateRoleCmd(dbMgr, pool, nID, data, count, out tcpOutPacket);
                        break;
                    }

                case (int)TCPGameServerCmds.CMD_INIT_GAME:
                    {
                        result = ProcessInitGameCmd(dbMgr, pool, nID, data, count, out tcpOutPacket);
                        break;
                    }

                case (int)TCPGameServerCmds.CMD_DB_QUERY_ROLEMINIINFO:
                    {
                        result = ProcessQueryRoleMiniInfoCmd(dbMgr, pool, nID, data, count, out tcpOutPacket);
                        break;
                    }

                case (int)TCPGameServerCmds.CMD_DB_ROLE_ONLINE:
                    {
                        result = ProcessRoleOnLineGameCmd(dbMgr, pool, nID, data, count, out tcpOutPacket);
                        break;
                    }

                case (int)TCPGameServerCmds.CMD_DB_UPDATE_ACCOUNT_ACTIVE:
                    {
                        result = ProcessUpdateAccountActiveCmd(dbMgr, pool, nID, data, count, out tcpOutPacket);
                        break;
                    }

                case (int)TCPGameServerCmds.CMD_DB_ROLE_HEART:
                    {
                        result = ProcessRoleHeartGameCmd(dbMgr, pool, nID, data, count, out tcpOutPacket);
                        break;
                    }

                case (int)TCPGameServerCmds.CMD_DB_ROLE_OFFLINE:
                    {
                        result = ProcessRoleOffLineGameCmd(dbMgr, pool, nID, data, count, out tcpOutPacket);
                        break;
                    }

                case (int)TCPGameServerCmds.CMD_DB_ONLINE_SERVERHEART:
                    {
                        result = ProcessOnlineServerHeartCmd(client, dbMgr, pool, nID, data, count, out tcpOutPacket);
                        break;
                    }

                case (int)TCPGameServerCmds.CMD_DB_GET_SERVERID:
                    {
                        result = ProcessGetServerIdCmd(dbMgr, pool, nID, data, count, out tcpOutPacket);
                        break;
                    }

                case (int)TCPGameServerCmds.CMD_SPR_NEWTASK:
                    {
                        result = ProcessNewTaskCmd(dbMgr, pool, nID, data, count, out tcpOutPacket);
                        break;
                    }

                case (int)TCPGameServerCmds.CMD_DB_UPDATE_POS:
                    {
                        result = ProcessUpdatePosCmd(dbMgr, pool, nID, data, count, out tcpOutPacket);
                        break;
                    }

                case (int)TCPGameServerCmds.CMD_DB_UPDATE_EXPLEVEL:
                    {
                        result = ProcessUpdateExpLevelCmd(dbMgr, pool, nID, data, count, out tcpOutPacket);
                        break;
                    }

                case (int)TCPGameServerCmds.CMD_DB_UPDATE_ROLE_AVARTA:
                    {
                        result = ProcessUpdateRoleAvartaCmd(dbMgr, pool, nID, data, count, out tcpOutPacket);
                        break;
                    }

                case (int)TCPGameServerCmds.CMD_DB_UPDATEMONEY1_CMD:
                    {
                        result = ProcessUpdateMoney1Cmd(dbMgr, pool, nID, data, count, out tcpOutPacket);
                        break;
                    }

                case (int)TCPGameServerCmds.CMD_DB_ADDGOODS_CMD:
                    {
                        result = ProcessAddGoodsCmd(dbMgr, pool, nID, data, count, out tcpOutPacket);
                        break;
                    }

                case (int)TCPGameServerCmds.CMD_DB_UPDATEGOODS_CMD:
                    {
                        result = ProcessUpdateGoodsCmd(dbMgr, pool, nID, data, count, out tcpOutPacket);
                        break;
                    }

                case (int)TCPGameServerCmds.CMD_DB_UPDATETASK_CMD:
                    {
                        result = ProcessUpdateTaskCmd(dbMgr, pool, nID, data, count, out tcpOutPacket);
                        break;
                    }

                case (int)TCPGameServerCmds.CMD_SPR_COMPTASK:
                    {
                        result = ProcessCompleteTaskCmd(dbMgr, pool, nID, data, count, out tcpOutPacket);
                        break;
                    }

                case (int)TCPGameServerCmds.CMD_SPR_GM_SET_MAIN_TASK:
                    {
                        result = ProcessGMSetTaskCmd(dbMgr, pool, nID, data, count, out tcpOutPacket);
                        break;
                    }

                case (int)TCPGameServerCmds.CMD_SPR_GETFRIENDS:
                    {
                        result = ProcessGetFriendsCmd(dbMgr, pool, nID, data, count, out tcpOutPacket);
                        break;
                    }

                case (int)TCPGameServerCmds.CMD_SPR_ADDFRIEND:
                    {
                        result = ProcessAddFriendCmd(dbMgr, pool, nID, data, count, out tcpOutPacket);
                        break;
                    }

                case (int)TCPGameServerCmds.CMD_SPR_REMOVEFRIEND:
                    {
                        result = ProcessRemoveFriendCmd(dbMgr, pool, nID, data, count, out tcpOutPacket);
                        break;
                    }

                case (int)TCPGameServerCmds.CMD_DB_UPDATEPKMODE_CMD:
                    {
                        result = ProcessUpdatePKModeCmd(dbMgr, pool, nID, data, count, out tcpOutPacket);
                        break;
                    }

                case (int)TCPGameServerCmds.CMD_DB_UPDATEPKVAL_CMD:
                    {
                        result = ProcessUpdatePKValCmd(dbMgr, pool, nID, data, count, out tcpOutPacket);
                        break;
                    }

                case (int)TCPGameServerCmds.CMD_SPR_ABANDONTASK:
                    {
                        result = ProcessAbandonTaskCmd(dbMgr, pool, nID, data, count, out tcpOutPacket);
                        break;
                    }

                case (int)TCPGameServerCmds.CMD_DB_UPDATEKEYS:
                    {
                        result = ProcessModKeysCmd(dbMgr, pool, nID, data, count, out tcpOutPacket);
                        break;
                    }

                case (int)TCPGameServerCmds.CMD_DB_UPDATEUSERMONEY_CMD:
                    {
                        result = ProcessUpdateUserMoneyCmd(dbMgr, pool, nID, data, count, out tcpOutPacket);
                        break;
                    }

                case (int)TCPGameServerCmds.CMD_DB_UPDATEUSERYINLIANG_CMD:
                    {
                        result = ProcessUpdateUserYinLiangCmd(dbMgr, pool, nID, data, count, out tcpOutPacket);
                        break;
                    }
                case (int)TCPGameServerCmds.CMD_DB_UPDATEUSERGOLD_CMD:
                    {
                        result = ProcessUpdateUserGoldCmd(dbMgr, pool, nID, data, count, out tcpOutPacket);
                        break;
                    }
                case (int)TCPGameServerCmds.CMD_DB_ROLE_BUY_YUE_KA_BUT_OFFLINE:
                    {
                        result = ProcessRoleBuyYueKaButOffline(dbMgr, pool, nID, data, count, out tcpOutPacket);
                        break;
                    }
                case (int)TCPGameServerCmds.CMD_DB_MOVEGOODS_CMD:
                    {
                        result = ProcessMoveGoodsCmd(dbMgr, pool, nID, data, count, out tcpOutPacket);
                        break;
                    }

                case (int)TCPGameServerCmds.CMD_SPR_QUERYIDBYNAME:
                    {
                        result = ProcessQueryNameByIDCmd(dbMgr, pool, nID, data, count, out tcpOutPacket);
                        break;
                    }

                case (int)TCPGameServerCmds.CMD_SPR_CHAT:
                    {
                        result = ProcessSpriteChatCmd(dbMgr, pool, nID, data, count, out tcpOutPacket);
                        break;
                    }

                case (int)TCPGameServerCmds.CMD_DB_GET_CHATMSGLIST:
                    {
                        result = ProcessGetChatMsgListCmd(client, dbMgr, pool, nID, data, count, out tcpOutPacket);
                        break;
                    }

                case (int)TCPGameServerCmds.CMD_GETGOODSLISTBYSITE:
                    {
                        result = ProcessGetGoodsListBySiteCmd(dbMgr, pool, nID, data, count, out tcpOutPacket);
                        break;
                    }

                case (int)TCPGameServerCmds.CMD_DB_BAN_USER:
                    {
                        result = KTBanManager.ProcessBanUser(dbMgr, pool, nID, data, count, out tcpOutPacket);
                        break;
                    }

                case (int)TCPGameServerCmds.CMD_DB_BAN_USER_BY_TYPE:
                    {
                        result = KTBanManager.ProcessBanUserByType(dbMgr, pool, nID, data, count, out tcpOutPacket);
                        break;
                    }

                case (int)TCPGameServerCmds.CMD_DB_UPDATEONLINETIME:
                    {
                        result = ProcessUpdateOnlineTimeCmd(dbMgr, pool, nID, data, count, out tcpOutPacket);
                        break;
                    }

                case (int)TCPGameServerCmds.CMD_DB_GAMECONFIGDICT:
                    {
                        result = ProcessGameConfigDictCmd(dbMgr, pool, nID, data, count, out tcpOutPacket);
                        break;
                    }

                case (int)TCPGameServerCmds.CMD_DB_GAMECONIFGITEM:
                    {
                        result = ProcessGameConfigItemCmd(dbMgr, pool, nID, data, count, out tcpOutPacket);
                        break;
                    }

                case (int)TCPGameServerCmds.CMD_DB_ADDSKILL:
                    {
                        result = ProcessAddSkillCmd(dbMgr, pool, nID, data, count, out tcpOutPacket);
                        break;
                    }

                case (int)TCPGameServerCmds.CMD_DB_UPSKILLINFO:
                    {
                        result = ProcessUpSkillInfoCmd(dbMgr, pool, nID, data, count, out tcpOutPacket);
                        break;
                    }

                case (int)TCPGameServerCmds.CMD_DB_DEL_SKILL:
                    {
                        result = ProcessDelSkillCmd(dbMgr, pool, nID, data, count, out tcpOutPacket);
                        break;
                    }

                case (int)TCPGameServerCmds.CMD_DB_UPDATEBUFFERITEM:
                    {
                        result = ProcessUpdateBufferItemCmd(dbMgr, pool, nID, data, count, out tcpOutPacket);
                        break;
                    }

                case (int)TCPGameServerCmds.CMD_DB_UPDATE_WELFARE:
                    {
                        result = ProcessUpdateWelfare(dbMgr, pool, nID, data, count, out tcpOutPacket);
                        break;
                    }

                case (int)TCPGameServerCmds.CMD_DB_UPDATEROLEDAILYDATA:
                    {
                        result = ProseccUpdateRanking(dbMgr, pool, nID, data, count, out tcpOutPacket);
                        break;
                    }

                case (int)TCPGameServerCmds.CMD_KT_QUERY_PLAYERRANKING:
                    {
                        result = ProcessGetPaiHangListCmd(dbMgr, pool, nID, data, count, out tcpOutPacket);
                        break;
                    }

                case (int)TCPGameServerCmds.CMD_SPR_GETOTHERATTRIB:
                case (int)TCPGameServerCmds.CMD_SPR_GETOTHERATTRIB2:
                    {
                        result = ProcessGetOtherAttrib2DataCmd(dbMgr, pool, nID, data, count, out tcpOutPacket);
                        break;
                    }

                case (int)TCPGameServerCmds.CMD_DB_UPDATECZTASKID:
                    {
                        result = ProcessUpdateCZTaskIDCmd(dbMgr, pool, nID, data, count, out tcpOutPacket);
                        break;
                    }

                case (int)TCPGameServerCmds.CMD_DB_GETTOTALONLINENUM:
                    {
                        result = ProcessGetTotalOnlineNumCmd(dbMgr, pool, nID, data, count, out tcpOutPacket);
                        break;
                    }

                case (int)TCPGameServerCmds.CMD_SPR_SEARCHROLESFROMDB:
                    {
                        result = ProcessSearchRolesFromDBCmd(dbMgr, pool, nID, data, count, out tcpOutPacket);
                        break;
                    }

                case (int)TCPGameServerCmds.CMD_DB_UPDATEBANGGONG_CMD:
                    {
                        result = ProseccUpdateGuildMoney(dbMgr, pool, nID, data, count, out tcpOutPacket);
                        break;
                    }

                case (int)TCPGameServerCmds.CMD_SPR_GETHUANGDIROLEDATA:
                    {
                        result = ProcessGetHuangDiRoleDataCmd(dbMgr, pool, nID, data, count, out tcpOutPacket);
                        break;
                    }

                case (int)TCPGameServerCmds.CMD_SPR_GETGOODSBYDBID:
                    {
                        result = ProcessGetGoodsByDbIDCmd(dbMgr, pool, nID, data, count, out tcpOutPacket);
                        break;
                    }
                case (int)TCPGameServerCmds.CMD_DB_QUERYCHONGZHIMONEY:
                    {
                        result = ProcessQueryChongZhiMoneyCmd(dbMgr, pool, nID, data, count, out tcpOutPacket);
                        break;
                    }
                case (int)TCPGameServerCmds.CMD_DB_QUERYTODAYCHONGZHIMONEY:
                    {
                        result = ProcessQueryDayChongZhiMoneyCmd(dbMgr, pool, nID, data, count, out tcpOutPacket);
                        break;
                    }

                case (int)TCPGameServerCmds.CMD_SPR_GETUSERMAILLIST:
                    {
                        result = ProcessGetUserMailListCmd(dbMgr, pool, nID, data, count, out tcpOutPacket);
                        break;
                    }
                case (int)TCPGameServerCmds.CMD_SPR_GETUSERMAILCOUNT:
                    {
                        result = ProcessGetUserMailCountCmd(dbMgr, pool, nID, data, count, out tcpOutPacket);
                        break;
                    }
                case (int)TCPGameServerCmds.CMD_SPR_GETUSERMAILDATA:
                    {
                        result = ProcessGetUserMailDataCmd(dbMgr, pool, nID, data, count, out tcpOutPacket);
                        break;
                    }
                case (int)TCPGameServerCmds.CMD_DB_GETUSERMAILDATA:
                    {
                        result = ProcessGetUserMailDataCmd(dbMgr, pool, nID, data, count, out tcpOutPacket);
                        break;
                    }

                case (int)TCPGameServerCmds.CMD_DB_SENDUSERMAIL:
                    {
                        result = ProcessSendUserMailCmd(dbMgr, pool, nID, data, count, out tcpOutPacket);
                        break;
                    }
                case (int)TCPGameServerCmds.CMD_SPR_FETCHMAILGOODS:
                    {
                        result = ProcessFetchMailGoodsCmd(dbMgr, pool, nID, data, count, out tcpOutPacket);
                        break;
                    }
                case (int)TCPGameServerCmds.CMD_SPR_DELETEUSERMAIL:
                    {
                        result = ProcessDeleteUserMailCmd(dbMgr, pool, nID, data, count, out tcpOutPacket);
                        break;
                    }

                case (int)TCPGameServerCmds.CMD_DB_QUERYLIMITGOODSUSEDNUM:
                    {
                        result = ProcessDBQueryLimitGoodsUsedNumCmd(dbMgr, pool, nID, data, count, out tcpOutPacket);
                        break;
                    }
                case (int)TCPGameServerCmds.CMD_DB_UPDATELIMITGOODSUSEDNUM:
                    {
                        result = ProcessDBUpdateLimitGoodsUsedNumCmd(dbMgr, pool, nID, data, count, out tcpOutPacket);
                        break;
                    }

                case (int)TCPGameServerCmds.CMD_DB_UPDATEROLEPARAM:
                    {
                        result = ProcessUpdateRoleParamCmd(dbMgr, pool, nID, data, count, out tcpOutPacket);
                        break;
                    }

                case (int)TCPGameServerCmds.CMD_DB_QUERYFIRSTCHONGZHIBYUSERID:
                    {
                        result = ProcessQueryFirstChongZhiDaLiByUserIDCmd(dbMgr, pool, nID, data, count, out tcpOutPacket);
                        break;
                    }

                case (int)TCPGameServerCmds.CMD_DB_ADDEXCHANGE1ITEM:
                    {
                        result = ProcessAddExchange1ItemCmd(dbMgr, pool, nID, data, count, out tcpOutPacket);
                        break;
                    }

                case (int)TCPGameServerCmds.CMD_DB_EXECUTECHANGEOCCUPATION:
                    {
                        result = ProcessExecuteChangeOccupationCmd(dbMgr, pool, nID, data, count, out tcpOutPacket);
                        break;
                    }

                case (int)TCPGameServerCmds.CMD_DB_QUERY_REPAYACTIVEINFO:
                    {
                        result = RechargeRepayActiveMgr.ProcessQueryActiveInfo(dbMgr, pool, nID, data, count,
                            out tcpOutPacket);
                        break;
                    }

                case (int)TCPGameServerCmds.CMD_DB_GET_REPAYACTIVEAWARD:
                    {
                        result = RechargeRepayActiveMgr.ProcessGetActiveAwards(dbMgr, pool, nID, data, count,
                            out tcpOutPacket);
                        break;
                    }

                case (int)TCPGameServerCmds.CMD_DB_SAVECONSUMELOG:
                    {
                        result = Global.SaveConsumeLog(dbMgr, pool, nID, data, count, out tcpOutPacket);
                        break;
                    }

                case (int)TCPGameServerCmds.CMD_LOGDB_UPDATE_ROLE_KUAFU_DAY_LOG:
                    {
                        result = ProcessUpdateRoleKuaFuDayLogCmd(dbMgr, pool, nID, data, count, out tcpOutPacket);
                        break;
                    }

                case (int)TCPGameServerCmds.CMD_DB_ADD_STORE_YINLIANG:
                    {
                        result = ProcessAddRoleStoreYinliang(dbMgr, pool, nID, data, count, out tcpOutPacket);
                        break;
                    }
                case (int)TCPGameServerCmds.CMD_DB_ADD_STORE_MONEY:
                    {
                        result = ProcessAddRoleStoreMoney(dbMgr, pool, nID, data, count, out tcpOutPacket);
                        break;
                    }
                case (int)TCPGameServerCmds.CMD_DB_REQUESTNEWGMAILLIST:
                    {
                        result = GroupMailManager.RequestNewGroupMailList(dbMgr, pool, nID, data, count, out tcpOutPacket);
                        break;
                    }
                case (int)TCPGameServerCmds.CMD_DB_MODIFYROLEGMAIL:
                    {
                        result = GroupMailManager.ModifyGMailRecord(dbMgr, pool, nID, data, count, out tcpOutPacket);
                        break;
                    }
                case (int)TCPGameServerCmds.CMD_SPR_CHANGE_NAME:
                    {
                        result = NameManager.Instance().ProcChangeName(dbMgr, pool, nID, data, count, out tcpOutPacket);
                        break;
                    }

                case (int)TCPGameServerCmds.CMD_DB_QUERY_SYSPARAM:
                    {
                        result = ProcessQuerySystemGlobalParameters(dbMgr, pool, nID, data, count, out tcpOutPacket);
                        break;
                    }
                case (int)TCPGameServerCmds.CMD_DB_TEAMBATTLE:
                    {
                        result = KTTeamBattleManager.Instance.ProcessTeamBattleCmd(dbMgr, pool, nID, data, count,
                            out tcpOutPacket);
                        break;
                    }

                case (int)TCPGameServerCmds.CMD_KT_FAMILY_CREATE:
                    {
                        // result = KT_TCPHandler_Family.CMD_KT_FAMILY_CREATE(dbMgr, pool, nID, data, count, out tcpOutPacket);
                        break;
                    }

                case (int)TCPGameServerCmds.CMD_KT_FAMILY_REQUESTJOIN:
                    {
                        //result = KT_TCPHandler_Family.CMD_KT_FAMILY_REQUESTJOIN(dbMgr, pool, nID, data, count,
                        //    out tcpOutPacket);
                        break;
                    }

                case (int)TCPGameServerCmds.CMD_KT_FAMILY_KICKMEMBER:
                    {
                        //result = KT_TCPHandler_Family.CMD_KT_FAMILY_KICKMEMBER(dbMgr, pool, nID, data, count,
                        //    out tcpOutPacket);
                        break;
                    }

                case (int)TCPGameServerCmds.CMD_KT_FAMILY_GETLISTFAMILY:
                    {
                        //result = KT_TCPHandler_Family.CMD_KT_FAMILY_GETLISTFAMILY(dbMgr, pool, nID, data, count,
                        //    out tcpOutPacket);
                        break;
                    }

                case (int)TCPGameServerCmds.CMD_KT_FAMILY_DESTROY:
                    {
                        //result = KT_TCPHandler_Family.CMD_KT_FAMILY_DESTROY(dbMgr, pool, nID, data, count,
                        //    out tcpOutPacket);
                        break;
                    }

                case (int)TCPGameServerCmds.CMD_KT_FAMILY_CHANGENOTIFY:
                    {
                        ////result = KT_TCPHandler_Family.CMD_KT_FAMILY_CHANGENOTIFY(dbMgr, pool, nID, data, count,
                        //    out tcpOutPacket);
                        break;
                    }

                case (int)TCPGameServerCmds.CMD_KT_FAMILY_CHANGE_REQUESTJOIN_NOTIFY:
                    {
                        //// result = KT_TCPHandler_Family.CMD_KT_FAMILY_CHANGE_REQUESTJOIN_NOTIFY(dbMgr, pool, nID, data, count,
                        //     out tcpOutPacket);
                        break;
                    }

                case (int)TCPGameServerCmds.CMD_KT_FAMILY_CHANGE_RANK:
                    {
                        //result = KT_TCPHandler_Family.CMD_KT_FAMILY_CHANGE_RANK(dbMgr, pool, nID, data, count,
                        //    out tcpOutPacket);
                        break;
                    }

                case (int)TCPGameServerCmds.CMD_KT_FAMILY_RESPONSE_REQUEST:
                    {
                        //result = KT_TCPHandler_Family.CMD_KT_FAMILY_RESPONSE_REQUEST(dbMgr, pool, nID, data, count,
                        //    out tcpOutPacket);
                        break;
                    }

                case (int)TCPGameServerCmds.CMD_KT_FAMILY_QUIT:
                    {
                        //result = KT_TCPHandler_Family.CMD_KT_FAMILY_QUIT(dbMgr, pool, nID, data, count, out tcpOutPacket);
                        break;
                    }

                case (int)TCPGameServerCmds.CMD_KT_FAMILY_OPEN:
                    {
                        //result = KT_TCPHandler_Family.CMD_KT_FAMILY_OPEN(dbMgr, pool, nID, data, count, out tcpOutPacket);
                        break;
                    }

                ///Tạm thời không cần dùng tới bởi vì đã cache thông tin guild bên gs
                case (int)TCPGameServerCmds.CMD_KT_GUILD_GETINFO:
                    {
                        // result = GuildManager.CMD_KT_GUILD_GETINFO(dbMgr, pool, nID, data, count, out tcpOutPacket);
                        break;
                    }

                case (int)TCPGameServerCmds.CMD_KT_GUILD_CHANGERANK:
                    {
                        result = GuildManager.CMD_KT_GUILD_CHANGERANK(dbMgr, pool, nID, data, count,
                            out tcpOutPacket);
                        break;
                    }
                case (int)TCPGameServerCmds.CMD_KT_GUILD_KICK_ROLE:
                    {
                        result = GuildManager.CMD_KT_GUILD_KICK_ROLE(dbMgr, pool, nID, data, count,
                            out tcpOutPacket);
                        break;
                    }
                case (int)TCPGameServerCmds.CMD_KT_GUILD_GETGIFTED:
                    {
                        result = KT_TCPHandler_Family.CMD_KT_GUILD_GETGIFTED(dbMgr, pool, nID, data, count,
                            out tcpOutPacket);
                        break;
                    }
                case (int)TCPGameServerCmds.CMD_KT_GUILD_OFFICE_RANK:
                    {
                        result = KT_TCPHandler_Family.CMD_KT_GUILD_OFFICE_RANK(dbMgr, pool, nID, data, count,
                            out tcpOutPacket);
                        break;
                    }
                case (int)TCPGameServerCmds.CMD_KT_GUILD_VOTEGIFTED:
                    {
                        result = KT_TCPHandler_Family.CMD_KT_GUILD_VOTEGIFTED(dbMgr, pool, nID, data, count,
                            out tcpOutPacket);
                        break;
                    }

                case (int)TCPGameServerCmds.CMD_KT_GUILD_TERRITORY:
                    {
                        result = KT_TCPHandler_Family.CMD_KT_GUILD_TERRITORY(dbMgr, pool, nID, data, count,
                            out tcpOutPacket);
                        break;
                    }

                //Update thông tin lãnh thổ
                case (int)TCPGameServerCmds.CMD_KT_GUILD_UPDATE_TERRITORY:
                    {
                        result = KT_TCPHandler_Family.CMD_KT_GUILD_UPDATE_TERRITORY(dbMgr, pool, nID, data, count,
                            out tcpOutPacket);
                        break;
                    }

                case (int)TCPGameServerCmds.CMD_KT_GUILD_ALLTERRITORY:
                    {
                        result = KT_TCPHandler_Family.CMD_KT_GUILD_ALLTERRITORY(dbMgr, pool, nID, data, count,
                            out tcpOutPacket);
                        break;
                    }
                case (int)TCPGameServerCmds.CMD_KT_GUILD_SETCITY:
                    {
                        result = KT_TCPHandler_Family.CMD_KT_GUILD_SETCITY(dbMgr, pool, nID, data, count, out tcpOutPacket);
                        break;
                    }

                case (int)TCPGameServerCmds.CMD_KT_GUILD_SETTAX:
                    {
                        result = KT_TCPHandler_Family.CMD_KT_GUILD_SETTAX(dbMgr, pool, nID, data, count, out tcpOutPacket);
                        break;
                    }
                case (int)TCPGameServerCmds.CMD_KT_GUILD_QUIT:
                    {
                        result = GuildManager.CMD_KT_GUILD_QUIT(dbMgr, pool, nID, data, count, out tcpOutPacket);
                        break;
                    }

                // Đăng ký công thành chiến
                case (int)TCPGameServerCmds.CMD_KT_GUILD_WAR_REGISTER:
                    {
                        result = GuildManager.CMD_KT_GUILD_WAR_REGISTER(dbMgr, pool, nID, data, count, out tcpOutPacket);
                        break;
                    }

                case (int)TCPGameServerCmds.CMD_KT_GUILD_CHANGE_MAXWITHDRAW:
                    {
                        result = KT_TCPHandler_Family.CMD_KT_GUILD_CHANGE_MAXWITHDRAW(dbMgr, pool, nID, data, count,
                            out tcpOutPacket);
                        break;
                    }
                case (int)TCPGameServerCmds.CMD_KT_GUILD_CHANGE_NOTIFY:
                    {
                        result = GuildManager.CMD_KT_GUILD_CHANGE_NOTIFY(dbMgr, pool, nID, data, count,
                            out tcpOutPacket);
                        break;
                    }

                case (int)TCPGameServerCmds.CMD_KT_GUILD_GETSHARE:
                    {
                        result = KT_TCPHandler_Family.CMD_KT_GUILD_GETSHARE(dbMgr, pool, nID, data, count,
                            out tcpOutPacket);
                        break;
                    }

                case (int)TCPGameServerCmds.CMD_KT_GUILD_RESPONSEASK:
                    {
                        result = GuildManager.CMD_KT_GUILD_RESPONSEASK(dbMgr, pool, nID, data, count,
                            out tcpOutPacket);
                        break;
                    }

                case (int)TCPGameServerCmds.CMD_KT_GUILD_DOWTIHDRAW:
                    {
                        result = KT_TCPHandler_Family.CMD_KT_GUILD_DOWTIHDRAW(dbMgr, pool, nID, data, count,
                            out tcpOutPacket);
                        break;
                    }

                case (int)TCPGameServerCmds.CMD_KT_GETTERRORY_DATA:
                    {
                        result = KT_TCPHandler_Family.CMD_KT_GETTERRORY_DATA(dbMgr, pool, nID, data, count,
                            out tcpOutPacket);
                        break;
                    }

                case (int)TCPGameServerCmds.CMD_KT_G2C_RECHAGE:
                    {
                        result = UserMoneyMgr.CMT_KT_LOG_RECHAGE(dbMgr, pool, nID, data, count, out tcpOutPacket);
                        break;
                    }

                case (int)TCPGameServerCmds.CMD_KT_GUILD_RESPONSEINVITE:
                    {
                        result = GuildManager.CMD_KT_GUILD_RESPONSEINVITE(dbMgr, pool, nID, data, count,
                            out tcpOutPacket);
                        break;
                    }

                case (int)TCPGameServerCmds.CMD_KT_UPDATE_ROLEGUILDMONEY:
                    {
                        result = KT_TCPHandler_Family.CMD_KT_UPDATE_ROLEGUILDMONEY(dbMgr, pool, nID, data, count,
                            out tcpOutPacket);
                        break;
                    }

                case (int)TCPGameServerCmds.CMD_KT_RANKING_CHECKING:
                    {
                        result = RankingManager.CMD_KT_RANKING_CHECKING(dbMgr, pool, nID, data, count, out tcpOutPacket);
                        break;
                    }
                //Lấy ra top ranking
                case (int)TCPGameServerCmds.CMD_KT_TOPRANKING:
                    {
                        result = RankingManager.CMD_KT_TOPRANKING(dbMgr, pool, nID, data, count, out tcpOutPacket);
                        break;
                    }

                case (int)TCPGameServerCmds.CMD_KT_UPDATE_REVICE_STATUS:
                    {
                        result = RankingManager.CMD_KT_UPDATE_REVICE_STATUS(dbMgr, pool, nID, data, count, out tcpOutPacket);
                        break;
                    }
                case (int)TCPGameServerCmds.CMD_KT_GETRANK_RECORE_BYTYPE:
                    {
                        result = KTRecoreManager.CMD_KT_GETRANK_RECORE_BYTYPE(dbMgr, pool, nID, data, count,
                            out tcpOutPacket);
                        break;
                    }

                case (int)TCPGameServerCmds.CMD_KT_GETMARKVALUE:
                    {
                        result = KTRecoreManager.CMD_KT_GETMARKVALUE(dbMgr, pool, nID, data, count, out tcpOutPacket);
                        break;
                    }

                case (int)TCPGameServerCmds.CMD_KT_UPDATEMARKVALUE:
                    {
                        result = KTRecoreManager.CMD_KT_UPDATEMARKVALUE(dbMgr, pool, nID, data, count, out tcpOutPacket);
                        break;
                    }

                case (int)TCPGameServerCmds.CMD_KT_GET_RECORE_BYTYPE:
                    {
                        result = KTRecoreManager.CMD_KT_GET_RECORE_BYTYPE(dbMgr, pool, nID, data, count, out tcpOutPacket);
                        break;
                    }

                case (int)TCPGameServerCmds.CMD_KT_ADD_RECORE_BYTYPE:
                    {
                        result = KTRecoreManager.CMD_KT_ADD_RECORE_BYTYPE(dbMgr, pool, nID, data, count, out tcpOutPacket);
                        break;
                    }

                //Viết mới bang hội 04/10/2022
                case (int)TCPGameServerCmds.CMD_KT_GUILD_GET_RESOURCE_INFO:
                    {
                        result = GuildManager.CMD_KT_GUILD_GET_RESOURCE_INFO(dbMgr, pool, nID, data, count, out tcpOutPacket);
                        break;
                    }

                case (int)TCPGameServerCmds.CMD_KT_GUILD_SET_RESOURCE_INFO:
                    {
                        result = GuildManager.CMD_KT_GUILD_SET_RESOURCE_INFO(dbMgr, pool, nID, data, count, out tcpOutPacket);
                        break;
                    }

            
            

                case (int)TCPGameServerCmds.CMD_KT_GUILD_CREATE:
                    {
                        result = GuildManager.CMD_KT_GUILD_CREATE(dbMgr, pool, nID, data, count, out tcpOutPacket);
                        break;
                    }

                case (int)TCPGameServerCmds.CMD_KT_GUILD_GETMINI_INFO:
                    {
                        result = GuildManager.CMD_KT_GUILD_GETMINI_INFO(dbMgr, pool, nID, data, count, out tcpOutPacket);
                        break;
                    }

                // Lấy ra danh sách người chơi
                case (int)TCPGameServerCmds.CMD_KT_GUILD_GETMEMBERLIST:
                    {
                        result = GuildManager.CMD_KT_GUILD_GETMEMBERLIST(dbMgr, pool, nID, data, count,
                            out tcpOutPacket);
                        break;
                    }

                case (int)TCPGameServerCmds.CMD_KT_GUILD_REQUEST_JOIN_LIST:
                    {
                        result = GuildManager.CMD_KT_GUILD_REQUEST_JOIN_LIST(dbMgr, pool, nID, data, count,
                            out tcpOutPacket);
                        break;
                    }

                case (int)TCPGameServerCmds.CMD_KT_GUILD_RESPONSE_JOINREQUEST:
                    {
                        result = GuildManager.CMD_KT_GUILD_RESPONSE_JOINREQUEST(dbMgr, pool, nID, data, count,
                            out tcpOutPacket);
                        break;
                    }

                // tỰ ĐỘNG DUYỆT VÀO BANG
                case (int)TCPGameServerCmds.CMD_KT_GUILD_AUTO_ACCPECT_SETTING:
                    {
                        result = GuildManager.CMD_KT_GUILD_AUTO_ACCPECT_SETTING(dbMgr, pool, nID, data, count,
                            out tcpOutPacket);
                        break;
                    }
                //Viết mới bang hội 06/10/2022
                case (int)TCPGameServerCmds.CMD_KT_GUILD_TASKUPDATE:
                    {
                        result = GuildManager.CMD_KT_GUILD_TASKUPDATE(dbMgr, pool, nID, data, count,
                            out tcpOutPacket);
                        break;
                    }

                case (int)TCPGameServerCmds.CMD_KT_GUILD_REQUEST_JOIN:
                    {
                        result = GuildManager.CMD_KT_GUILD_REQUEST_JOIN(dbMgr, pool, nID, data, count,
                            out tcpOutPacket);
                        break;
                    }

                case (int)TCPGameServerCmds.CMD_KT_DB_UPDATE_SECONDPASSWORD:
                    {
                        result = TCPCmdHandler.ProcessUpdateSecondPasswordCmd(dbMgr, pool, nID, data, count,
                            out tcpOutPacket);
                        break;
                    }

                #region STall

                case (int)TCPGameServerCmds.CMD_SPR_STALL_QUERRY:
                    {
                        result = StallManager.CMD_SPR_STALL_QUERRY(dbMgr, pool, nID, data, count, out tcpOutPacket);
                        break;
                    }

                case (int)TCPGameServerCmds.CMD_SPR_STALL_UDPATE_DB:
                    {
                        result = StallManager.CMD_SPR_STALL_UDPATE_DB(dbMgr, pool, nID, data, count, out tcpOutPacket);
                        break;
                    }

                #endregion STall

                #region Pet

                case (int)TCPGameServerCmds.CMD_KT_DB_GET_PET_LIST:
                    {
                        result = KTPetManager.Instance.ProcessGetPetList(dbMgr, pool, nID, data, count,
                            out tcpOutPacket);
                        break;
                    }
                case (int)TCPGameServerCmds.CMD_KT_DB_PET_UPDATE_LEVEL_AND_EXP:
                    {
                        result = KTPetManager.Instance.ProcessPetUpdateLevelAndExp(dbMgr, pool, nID, data, count,
                            out tcpOutPacket);
                        break;
                    }
                case (int)TCPGameServerCmds.CMD_KT_DB_PET_UPDATE_RESID:
                    {
                        result = KTPetManager.Instance.ProcessPetUpdateResID(dbMgr, pool, nID, data, count,
                            out tcpOutPacket);
                        break;
                    }
                case (int)TCPGameServerCmds.CMD_KT_DB_PET_UPDATE_ENLIGHTENMENT:
                    {
                        result = KTPetManager.Instance.ProcessPetUpdateEnlightenment(dbMgr, pool, nID, data, count,
                            out tcpOutPacket);
                        break;
                    }
                case (int)TCPGameServerCmds.CMD_KT_DB_PET_UPDATE_NAME:
                    {
                        result = KTPetManager.Instance.ProcessPetUpdateName(dbMgr, pool, nID, data, count,
                            out tcpOutPacket);
                        break;
                    }
                case (int)TCPGameServerCmds.CMD_KT_DB_PET_UPDATE_SKILLS:
                    {
                        result = KTPetManager.Instance.ProcessPetUpdateSkills(dbMgr, pool, nID, data, count,
                            out tcpOutPacket);
                        break;
                    }
                case (int)TCPGameServerCmds.CMD_KT_DB_PET_UPDATE_EQUIPS:
                    {
                        result = KTPetManager.Instance.ProcessPetUpdateEquips(dbMgr, pool, nID, data, count,
                            out tcpOutPacket);
                        break;
                    }
                case (int)TCPGameServerCmds.CMD_KT_DB_PET_UPDATE_ATTRIBUTES:
                    {
                        result = KTPetManager.Instance.ProcessPetUpdateAttributes(dbMgr, pool, nID, data, count,
                            out tcpOutPacket);
                        break;
                    }
                case (int)TCPGameServerCmds.CMD_KT_DB_PET_UPDATE_JOYFUL:
                    {
                        result = KTPetManager.Instance.ProcessPetUpdateJoyful(dbMgr, pool, nID, data, count,
                            out tcpOutPacket);
                        break;
                    }
                case (int)TCPGameServerCmds.CMD_KT_DB_PET_UPDATE_LIFE:
                    {
                        result = KTPetManager.Instance.ProcessPetUpdateLife(dbMgr, pool, nID, data, count,
                            out tcpOutPacket);
                        break;
                    }
                case (int)TCPGameServerCmds.CMD_KT_DB_PET_UPDATE_HP:
                    {
                        result = KTPetManager.Instance.ProcessPetUpdateHP(dbMgr, pool, nID, data, count,
                            out tcpOutPacket);
                        break;
                    }
                case (int)TCPGameServerCmds.CMD_KT_DB_PET_UPDATE_BEFORE_QUIT_GAME:
                    {
                        result = KTPetManager.Instance.ProcessPetUpdateBeforeQuitGame(dbMgr, pool, nID, data, count,
                            out tcpOutPacket);
                        break;
                    }
                case (int)TCPGameServerCmds.CMD_KT_DB_DELETE_PET:
                    {
                        result = KTPetManager.Instance.ProcessDeletePet(dbMgr, pool, nID, data, count,
                            out tcpOutPacket);
                        break;
                    }

                case (int)TCPGameServerCmds.CMD_KT_DB_PET_UPDATEROLEID:
                    {
                        result = KTPetManager.Instance.MovePetToOtherRole(dbMgr, pool, nID, data, count,
                            out tcpOutPacket);
                        break;
                    }
                case (int)TCPGameServerCmds.CMD_KT_DB_ADD_PET:
                    {
                        result = KTPetManager.Instance.ProcessAddPet(dbMgr, pool, nID, data, count,
                            out tcpOutPacket);
                        break;
                    }

                #endregion Pet

                default:
                    {
                        //Console.WriteLine("UNKNOW PACKET ID :" + nID);
                        //LogManager.WriteLog(LogTypes.Trace, "UNKNOW PACKETID :" + nID);
                        result = TCPCmdDispatcher.getInstance().dispathProcessor(client, nID, data, count);
                        break;
                    }
            }

            return result;
        }

        /// <summary>
        ///     Xử lý truy vấn thông tin biến toàn cục hệ thống
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <param name="tcpOutPacket"></param>
        /// <returns></returns>
        private static TCPProcessCmdResults ProcessQuerySystemGlobalParameters(DBManager dbMgr, TCPOutPacketPool pool,
            int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string cmdData = null;

            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Wrong socket data CMD={0}", (TCPGameServerCmds)nID));

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                return TCPProcessCmdResults.RESULT_DATA;
            }

            try
            {
                var fields = cmdData.Split(':');
                if (fields.Length != 3)
                {
                    LogManager.WriteLog(LogTypes.Error,
                        string.Format("Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}",
                            (TCPGameServerCmds)nID, fields.Length, cmdData));
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                var type = Convert.ToInt32(fields[0]);
                var id = Convert.ToInt32(fields[1]);
                var value = fields[2];

                switch (type)
                {
                    /// GET
                    case 0:
                        {
                            value = SystemGlobalParametersManager.GetSystemGlobalParameter(id);
                            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, string.Format("{0}:{1}", id, value), nID);
                            return TCPProcessCmdResults.RESULT_DATA;
                        }
                    /// SET
                    case 1:
                        {
                            SystemGlobalParametersManager.SetSystemGlobalParameter(id, value);
                            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "", nID);
                            return TCPProcessCmdResults.RESULT_DATA;
                        }
                }
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, "", false);
            }

            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
            return TCPProcessCmdResults.RESULT_DATA;
        }

        /// <summary>
        /// Gói tin thực hiện mua thẻ tháng.
        /// Người chơi có thể offline
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <param name="tcpOutPacket"></param>
        /// <returns></returns>
        private static TCPProcessCmdResults ProcessRoleBuyYueKaButOffline(DBManager dbMgr, TCPOutPacketPool pool,
            int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string cmdData = null;

            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Wrong socket data CMD={0}", (TCPGameServerCmds)nID));

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, string.Format("{0}", -1), nID);
                return TCPProcessCmdResults.RESULT_DATA;
            }

            try
            {
                var fields = cmdData.Split(':');
                if (fields.Length != 3)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format(
                        "Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}",
                        (TCPGameServerCmds)nID, fields.Length, cmdData));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, string.Format("{0}", -1), nID);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                var roleID = Convert.ToInt32(fields[0]);
                var beginOffsetDay = Convert.ToInt32(fields[1]);
                var key = fields[2];

                var dbRoleInfo = dbMgr.GetDBRoleInfo(roleID);
                if (null == dbRoleInfo)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Source player is not exist, CMD={0}, RoleID={1}",
                        (TCPGameServerCmds)nID, roleID));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, string.Format("{0}", -1), nID);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                lock (dbRoleInfo)
                {
                    string[] oldFields = null;
                    var oldParamValue = "";
                    string paramValue;

                    var oldParamValueStr = Global.GetRoleParamByName(dbRoleInfo, key);
                    if (!string.IsNullOrEmpty(oldParamValueStr))
                    {
                        oldFields = oldParamValueStr.Split(',');
                        if (oldFields.Length == 5 && oldFields[0] == "1") oldParamValue = oldParamValueStr;
                    }

                    RoleParamsData value = null;
                    //月卡过期或者无月卡, 新加一个月卡
                    if (string.IsNullOrEmpty(oldParamValue))
                        paramValue = string.Format("1,{0},{1},{2},0", beginOffsetDay, beginOffsetDay + 30,
                            beginOffsetDay);
                    else //有月卡且月卡有效, 直接添加30天
                        paramValue = string.Format("{0},{1},{2},{3},{4}", oldFields[0], oldFields[1],
                            Convert.ToInt32(oldFields[2]) + 30, oldFields[3], oldFields[4]);

                    Global.UpdateRoleParamByName(dbMgr, dbRoleInfo, key, paramValue);
                }

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, string.Format("{0}", 0), nID);
                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, "", false);
            }

            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, string.Format("{0}", -1), nID);
            return TCPProcessCmdResults.RESULT_DATA;
        }

        /// <summary>
        /// Đăng ký 1 userID mới theo zoneID chỉ định gửi về từ GS
        ///
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private static TCPProcessCmdResults ProcessRegUserIDCmd(DBManager dbMgr, TCPOutPacketPool pool, int nID,
            byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string cmdData = null;

            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Wrong socket data CMD={0}", (TCPGameServerCmds)nID));

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                return TCPProcessCmdResults.RESULT_DATA;
            }

            try
            {
                //解析用户名称和用户密码
                var fields = cmdData.Split(':');
                if (fields.Length != 3)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format(
                        "Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}",
                        (TCPGameServerCmds)nID, fields.Length, cmdData));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                var userID = fields[0];
                var serverLineID = Convert.ToInt32(fields[1]);
                var state = Convert.ToInt32(fields[2]);

                var ret = 1;
                long logoutServerTicks = 0;
                if (!UserOnlineManager.RegisterUserID(userID, serverLineID, state)) ret = 0;

                var dbUserInfo = dbMgr.GetDBUserInfo(userID);
                if (dbUserInfo != null)
                    lock (dbUserInfo)
                    {
                        logoutServerTicks = dbUserInfo.LogoutServerTicks;
                    }

                var strcmd = string.Format("{0}:{1}", ret, logoutServerTicks);
                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                //System.Windows.Application.Current.Dispatcher.Invoke((MethodInvoker)delegate
                //{
                //
                DataHelper.WriteFormatExceptionLog(ex, "", false);
                //throw ex;
                //});
            }

            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
            return TCPProcessCmdResults.RESULT_DATA;
        }

        /// <summary>
        ///     Truy vấn danh sách nhân vật
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private static TCPProcessCmdResults ProcessGetRoleListCmd(DBManager dbMgr, TCPOutPacketPool pool, int nID,
            byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string cmdData = null;

            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Wrong socket data CMD={0}", (TCPGameServerCmds)nID));

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                return TCPProcessCmdResults.RESULT_DATA;
            }

            try
            {
                var fields = cmdData.Split(':');
                if (fields.Length != 2)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format(
                        "Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}",
                        (TCPGameServerCmds)nID, fields.Length, cmdData));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                var userID = fields[0];
                var zoneID = Convert.ToInt32(fields[1]);

                var dbUserInfo = dbMgr.GetDBUserInfo(userID);
                var strcmd = "";
                if (null == dbUserInfo)
                {
                    strcmd = string.Format("{0}:{1}", 0, "");
                }
                else
                {
                    var nRoleCount = 0;
                    var roleList = "";
                    //lock (dbUserInfo)
                    {
                        for (var i = 0; i < dbUserInfo.ListRoleIDs.Count; i++)
                        {
                            if (dbUserInfo.ListRoleZoneIDs[i] != zoneID) continue;

                            // Thời gian xóa nhân vật để là 0 luôn đỡ phải querry
                            var PreDelLeftSeconds =
                                0; //;GameDBManager.PreDelRoleMgr.CalcPreDeleteRoleLeftSeconds(dbUserInfo.ListRolePreRemoveTime[i]);

                            // Lấy ra role
                            var dbRoleInfo = dbMgr.GetDBRoleInfo(dbUserInfo.ListRoleIDs[i]);

                            /// Danh sách trang bị
                            int armorID = -1, helmID = -1, weaponID = -1, weaponEnhanceLevel = 0, mantleID = -1;
                            /// Nếu có tồn tại danh sách vật phẩm trên người
                            if (dbRoleInfo.GoodsDataList != null)
                            {
                                var listRoleEquipMini = dbRoleInfo.GoodsDataList.Values.Where(x =>
                                    x.Using == 1 || x.Using == 0 || x.Using == 3 || x.Using == 15).ToList();
                                foreach (var itemGD in listRoleEquipMini)
                                    switch (itemGD.Using)
                                    {
                                        case 0:
                                            {
                                                helmID = itemGD.GoodsID;
                                                break;
                                            }
                                        case 1:
                                            {
                                                armorID = itemGD.GoodsID;
                                                break;
                                            }
                                        case 3:
                                            {
                                                weaponID = itemGD.GoodsID;
                                                weaponEnhanceLevel = itemGD.Forge_level;
                                                break;
                                            }
                                        case 15:
                                            {
                                                mantleID = itemGD.GoodsID;
                                                break;
                                            }
                                    }
                            }

                            var roleEquipMiniString = string.Format("{0}_{1}_{2}_{3}_{4}", armorID, helmID, weaponID,
                                weaponEnhanceLevel, mantleID);

                            /// ID bản đồ đang đứng
                            int mapCode = -1;
                            try
                            {
                                mapCode = int.Parse(dbUserInfo.ListRolePositions[i].Split(':')[0]);
                            }
                            catch (Exception) { }

                            nRoleCount++;
                            roleList += string.Format("{0}${1}${2}${3}${4}${5}${6}|",
                                dbUserInfo.ListRoleIDs[i], dbUserInfo.ListRoleSexes[i], dbUserInfo.ListRoleOccups[i],
                                dbUserInfo.ListRoleNames[i].Replace('|', '*'),
                                dbUserInfo.ListRoleLevels[i], mapCode, roleEquipMiniString);
                        }
                    }

                    roleList = roleList.Trim('|');
                    strcmd = string.Format("{0}:{1}", nRoleCount, roleList);
                }

                var bytesCmd = new UTF8Encoding().GetBytes(strcmd);
                tcpOutPacket = pool.Pop();
                tcpOutPacket.PacketCmdID = (int)TCPGameServerCmds.CMD_ROLE_LIST;
                tcpOutPacket.FinalWriteData(bytesCmd, 0, bytesCmd.Length);

                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, "", false);
            }

            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
            return TCPProcessCmdResults.RESULT_DATA;
        }

        /// <summary>
        ///     Thực hiện tạo nhân vật
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private static TCPProcessCmdResults ProcessCreateRoleCmd(DBManager dbMgr, TCPOutPacketPool pool, int nID,
            byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string cmdData = null;

            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Wrong socket data CMD={0}", (TCPGameServerCmds)nID));

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                return TCPProcessCmdResults.RESULT_DATA;
            }

            try
            {
                var fields = cmdData.Split(':');
                if (fields.Length != 8)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}", (TCPGameServerCmds)nID, fields.Length, cmdData));
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                /// UserID
                string userID = fields[0];
                /// UserName
                string userName = fields[1];
                /// Giới tính
                int sex = Convert.ToInt32(fields[2]);
                /// ID phái
                int factionID = Convert.ToInt32(fields[3]);
                /// Tên nhân vật
                string rolename = fields[4];
                /// ID Server
                int serverID = Convert.ToInt32(fields[5]);
                /// ID Tân thủ thôn
                int villageID = Convert.ToInt32(fields[6]);
                /// Thông tin vị trí
                string positionInfo = fields[7].Replace(',', ':');

                /// Thông tin vị trí đầy đủ
                string roleDBPosition = string.Format("{0}:0:{1}", villageID, positionInfo);

                var strcmd = "";

                /// Kiểm tra tổng số nhân vật đã đầy chưa
                if (DBWriter.CheckRoleCountFull(dbMgr))
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Server Role data is full，CMD={0}, RoleID={1}", (TCPGameServerCmds) nID, -1));

                    strcmd = string.Format("{0}:{1}", -2, string.Format("{0}${1}${2}${3}${4}${5}", "", "", "", "", "", ""));
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, (int)TCPGameServerCmds.CMD_CREATE_ROLE);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                /// Kiểm tra tên nhân vật có thể được sử dụng không
                if (!NameManager.Instance().IsNameCanUseInDb(dbMgr, rolename))
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Name is invalid or already exist CMD={0}, RoleName={1}", (TCPGameServerCmds) nID, rolename));

                    strcmd = string.Format("{0}:{1}", -3, string.Format("{0}${1}${2}${3}${4}${5}", "", "", "", "", "", ""));
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, (int)TCPGameServerCmds.CMD_CREATE_ROLE);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                if (!NameUsedMgr.Instance().AddCannotUse_Ex(rolename) || dbMgr.IsRolenameExist(rolename))
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Name is already exist or param is invalid CMD={0}, RoleID={1}", (TCPGameServerCmds) nID, -1));

                    strcmd = string.Format("{0}:{1}", -4, string.Format("{0}${1}${2}${3}${4}${5}", "", "", "", "", "", ""));
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, (int)TCPGameServerCmds.CMD_CREATE_ROLE);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                /// Cấp độ ban đầu
                var nInitLevel = 1;
                /// Tổng số nhân vật
                var totalRoleCount = 0;
                /// Thông tin nhân vật tương ứng
                var dbUserInfo = dbMgr.GetDBUserInfo(userID);
                if (null != dbUserInfo)
                    //lock (dbUserInfo)
                    for (var i = 0; i < dbUserInfo.ListRoleIDs.Count; i++)
                    {
                        if (dbUserInfo.ListRoleZoneIDs[i] != serverID) continue;

                        totalRoleCount++;
                    }

                /// Nếu tổng số nhân vật >= 4 thì toác
                if (totalRoleCount >= 4)
                {
                    LogManager.WriteLog(LogTypes.Warning, string.Format("Can not add more role, full 4 roles already on this account CMD={0}, RoleID={1}", (TCPGameServerCmds) nID, -1000));

                    NameUsedMgr.Instance().DelCannotUse_Ex(rolename);

                    strcmd = string.Format("{0}:{1}", -1000, string.Format("{0}${1}${2}${3}${4}${5}", "", "", "", "", "", ""));
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, (int)TCPGameServerCmds.CMD_CREATE_ROLE);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                /// Thực hiện tạo nhân vật
                var roleID = DBWriter.CreateRole(dbMgr, userID, userName, sex, factionID, 0, rolename, serverID, (int)RoleCreateConstant.GridNum, positionInfo, 1);
                if (0 > roleID)
                {
                    NameUsedMgr.Instance().DelCannotUse_Ex(rolename);
                    strcmd = string.Format("{0}:{1}", roleID, string.Format("{0}${1}${2}${3}${4}${5}", "", "", "", "", "", ""));
                    LogManager.WriteLog(LogTypes.Error, string.Format("Create new role faild CMD={0}, RoleID={1}", (TCPGameServerCmds)nID, roleID));
                }
                else
                {
                    dbUserInfo = dbMgr.GetDBUserInfo(userID);
                    if (null != dbUserInfo)
                        lock (dbUserInfo)
                        {
                            dbUserInfo.ListRoleIDs.Add(roleID);
                            dbUserInfo.ListRoleSexes.Add(sex);
                            dbUserInfo.ListRoleOccups.Add(factionID);
                            dbUserInfo.ListRoleNames.Add(rolename);
                            dbUserInfo.ListRoleLevels.Add(nInitLevel);
                            dbUserInfo.ListRolePositions.Add(roleDBPosition);
                            dbUserInfo.ListRoleZoneIDs.Add(serverID);
                        }

                    strcmd = string.Format("{0}:{1}", 1, string.Format("{0}${1}${2}${3}${4}${5}${6}", roleID, sex, factionID, rolename, nInitLevel, 0, "-1_-1_-1_0_-1"));
                }

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, (int)TCPGameServerCmds.CMD_CREATE_ROLE);
                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, "", false);
            }

            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
            return TCPProcessCmdResults.RESULT_DATA;
        }

        /// <summary>
        /// Sự kiện người chơi vào Game
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private static TCPProcessCmdResults ProcessInitGameCmd(DBManager dbMgr, TCPOutPacketPool pool, int nID,
            byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string cmdData = null;

            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Wrong socket data CMD={0}", (TCPGameServerCmds)nID));
                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                return TCPProcessCmdResults.RESULT_DATA;
            }

            try
            {
                var fields = cmdData.Split(':');
                if (fields.Length != 3 && fields.Length != 2 && fields.Length != 4)
                {
                    LogManager.WriteLog(LogTypes.Error,
                        string.Format("Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}",
                            (TCPGameServerCmds)nID, fields.Length, cmdData));
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                var userID = fields[0];

                var roleID = Convert.ToInt32(fields[1]);

                var roleDataEx = new RoleDataEx();

                var failed = false;

                if (KTBanManager.IsBannedLogin(roleID))
                {
                    failed = true;
                    roleDataEx.RoleID = -70;
                    LogManager.WriteLog(LogTypes.Error,
                        string.Format("User is forbidden to log in，CMD={0}, UserID={1}", (TCPGameServerCmds)nID,
                            userID));
                    tcpOutPacket = DataHelper.ObjectToTCPOutPacket(roleDataEx, pool, nID);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                var dbUserInfo = dbMgr.GetDBUserInfo(userID);
                if (null == dbUserInfo)
                {
                    failed = true;
                    roleDataEx.RoleID = -2;

                    LogManager.WriteLog(LogTypes.Error, string.Format("Failed to get user data，CMD={0}, UserID={1}",
                        (TCPGameServerCmds)nID, userID));
                }
                else
                {
                    var hasrole = false;
                    foreach (var role in dbUserInfo.ListRoleIDs)
                        if (role == roleID)
                        {
                            hasrole = true;
                            break;
                        }

                    if (!hasrole)
                    {
                        failed = true;
                        roleDataEx.RoleID = -2;
                        LogManager.WriteLog(LogTypes.Error,
                            string.Format("Failed to get role data，CMD={0}, UserID={1}, RoleID={2}",
                                (TCPGameServerCmds)nID, userID, roleID));
                    }
                    else
                    {
                        lock (dbUserInfo)
                        {
                            roleDataEx.Token = dbUserInfo.Money;
                        }
                    }
                }

                if (null != dbUserInfo && !failed)
                {
                    var dbRoleInfo = dbMgr.GetDBRoleInfo(roleID);
                    if (null == dbRoleInfo)
                    {
                        roleDataEx.RoleID = -1;
                        LogManager.WriteLog(LogTypes.Error,
                            string.Format("Failed to get user data，CMD={0}, RoleID={1}", (TCPGameServerCmds)nID,
                                roleID));
                    }
                    else if (KTBanManager.IsBannedLogin(dbRoleInfo.RoleID))
                    {
                        roleDataEx.RoleID = -10;
                        LogManager.WriteLog(LogTypes.Error,
                            string.Format("User is banned by Admin，CMD={0}, RoleID={1}", (TCPGameServerCmds)nID,
                                roleID));
                    }
                    else
                    {
                        dbRoleInfo.LastTime = TimeUtil.NOW();

                        /// Lưu lại thông tin nhân vật
                        CacheManager.AddRoleMiniInfo(roleID, dbRoleInfo.ZoneID, dbRoleInfo.UserID);

                        /// Chuyển thông tin nhân vật thành RoleDataEx
                        Global.DBRoleInfo2RoleDataEx(dbRoleInfo, roleDataEx);

                        /// Gắn dữ liệu
                        roleDataEx.userMiniData = dbUserInfo.GetUserMiniData(userID, roleID, dbRoleInfo.ZoneID);
                    }
                }
                //Đóng gói RoleDataEx và trả về GS
                tcpOutPacket = DataHelper.ObjectToTCPOutPacket(roleDataEx, pool, nID);
                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, "", false);
            }

            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
            return TCPProcessCmdResults.RESULT_DATA;
        }

        /// <summary>
        ///  Gói tin thông báo khi người chơi online
        /// Sử dụng để ghi lại logs
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private static TCPProcessCmdResults ProcessRoleOnLineGameCmd(DBManager dbMgr, TCPOutPacketPool pool, int nID,
            byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string cmdData = null;

            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Wrong socket data CMD={0}", (TCPGameServerCmds)nID));

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                return TCPProcessCmdResults.RESULT_DATA;
            }

            try
            {
                //解析用户名称和用户密码
                var fields = cmdData.Split(':');
                if (fields.Length != 4)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format(
                        "Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}",
                        (TCPGameServerCmds)nID, fields.Length, cmdData));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                var roleID = Convert.ToInt32(fields[0]);
                var serverLineID = Convert.ToInt32(fields[1]);
                var loginNum = Convert.ToInt32(fields[2]);
                var ip = fields[3];

                var strcmd = "";
                var dbRoleInfo = dbMgr.GetDBRoleInfo(roleID);
                if (null == dbRoleInfo)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Source player is not exist, CMD={0}, RoleID={1}",
                        (TCPGameServerCmds)nID, roleID));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                var dayID = DateTime.Now.DayOfYear;

                var loginDayID = 0;
                var loginDayNum = 0;
                lock (dbRoleInfo)
                {
                    dbRoleInfo.ServerLineID = serverLineID;
                    dbRoleInfo.LoginNum = loginNum;
                    dbRoleInfo.LastTime = DateTime.Now.Ticks / 10000;

                    if (dayID != dbRoleInfo.LoginDayID)
                    {
                        dbRoleInfo.LoginDayNum++;
                        dbRoleInfo.LoginDayID = dayID;
                    }

                    loginDayID = dbRoleInfo.LoginDayID;
                    loginDayNum = dbRoleInfo.LoginDayNum;

                    dbRoleInfo.LastIP = ip;
                }

                // DBWriter.UpdateRoleLoginInfo(dbMgr, roleID, loginNum, loginDayID, loginDayNum, dbRoleInfo.UserID, dbRoleInfo.ZoneID, ip);

                RoleOnlineManager.UpdateRoleOnlineTicks(roleID);

                //DBWriter.InsertCityInfo(dbMgr, ip, dbRoleInfo.UserID);

                // Global.WriteRoleInfoLog(dbMgr, dbRoleInfo);

                strcmd = string.Format("{0}:{1}", roleID, 0);
                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                //System.Windows.Application.Current.Dispatcher.Invoke((MethodInvoker)delegate
                //{
                //
                DataHelper.WriteFormatExceptionLog(ex, "", false);
                //throw ex;
                //});
            }

            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
            return TCPProcessCmdResults.RESULT_DATA;
        }

        /// <summary>
        /// Cập nhật thông tin active của account
        /// Sử dụng để đo lương trong maketing
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private static TCPProcessCmdResults ProcessUpdateAccountActiveCmd(DBManager dbMgr, TCPOutPacketPool pool,
            int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string cmdData = null;

            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Wrong socket data CMD={0}", (TCPGameServerCmds)nID));

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                return TCPProcessCmdResults.RESULT_DATA;
            }

            try
            {
                //解析用户名称和用户密码
                var fields = cmdData.Split(':');
                if (fields.Length != 1)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format(
                        "Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}",
                        (TCPGameServerCmds)nID, fields.Length, cmdData));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                DBQuery.UpdateAccountActiveInfo(dbMgr, fields[0]);
                var strcmd = string.Format("{0}", 0);
                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                //System.Windows.Application.Current.Dispatcher.Invoke((MethodInvoker)delegate
                //{
                //
                DataHelper.WriteFormatExceptionLog(ex, "", false);
                //throw ex;
                //});
            }

            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
            return TCPProcessCmdResults.RESULT_DATA;
        }

        /// <summary>
        ///  Hàm giữ liên lạc chắc chắn là nhân vật có online
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private static TCPProcessCmdResults ProcessRoleHeartGameCmd(DBManager dbMgr, TCPOutPacketPool pool, int nID,
            byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string cmdData = null;

            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Wrong socket data CMD={0}", (TCPGameServerCmds)nID));

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                return TCPProcessCmdResults.RESULT_DATA;
            }

            try
            {
                //解析用户名称和用户密码
                var fields = cmdData.Split(':');
                if (fields.Length != 1)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format(
                        "Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}",
                        (TCPGameServerCmds)nID, fields.Length, cmdData));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                var roleID = Convert.ToInt32(fields[0]);

                var strcmd = "";
                var dbRoleInfo = dbMgr.GetDBRoleInfo(roleID);
                if (null == dbRoleInfo)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Source player is not exist, CMD={0}, RoleID={1}",
                        (TCPGameServerCmds)nID, roleID));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                //记录心跳信息
                RoleOnlineManager.UpdateRoleOnlineTicks(roleID);

                strcmd = string.Format("{0}:{1}", roleID, 0);
                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                //System.Windows.Application.Current.Dispatcher.Invoke((MethodInvoker)delegate
                //{
                //
                DataHelper.WriteFormatExceptionLog(ex, "", false);
                //throw ex;
                //});
            }

            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
            return TCPProcessCmdResults.RESULT_DATA;
        }

        /// <summary>
        ///     Gói tin thông báo đối tượng rời mạng
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private static TCPProcessCmdResults ProcessRoleOffLineGameCmd(DBManager dbMgr, TCPOutPacketPool pool, int nID,
            byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string cmdData = null;

            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Wrong socket data CMD={0}", (TCPGameServerCmds)nID));

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                return TCPProcessCmdResults.RESULT_DATA;
            }

            try
            {
                //解析用户名称和用户密码
                var fields = cmdData.Split(':');
                if (fields.Length < 4)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format(
                        "Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}",
                        (TCPGameServerCmds)nID, fields.Length, cmdData));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                var roleID = Convert.ToInt32(fields[0]);
                var serverLineID = Convert.ToInt32(fields[1]);
                var ip = fields[2];
                var activeVal = Convert.ToInt32(fields[3]);

                long logoutServerTicks = 0;
                if (fields.Length >= 5)
                    //来自GameServer的离线时间
                    logoutServerTicks = Convert.ToInt64(fields[4]);

                var strcmd = "";
                var dbRoleInfo = dbMgr.GetDBRoleInfo(roleID);
                if (null == dbRoleInfo)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Source player is not exist, CMD={0}, RoleID={1}",
                        (TCPGameServerCmds)nID, roleID));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                int pkMode = 0, horseDbID = 0, petDbID = 0;
                var onlineSecs = 0;
                lock (dbRoleInfo)
                {
                    dbRoleInfo.ServerLineID = -1;
                    dbRoleInfo.LogOffTime = DateTime.Now.Ticks / 10000;
                    pkMode = dbRoleInfo.PKMode;

                    onlineSecs = Math.Min((int)((dbRoleInfo.LogOffTime - dbRoleInfo.LastTime) / 1000), 86400);
                }

                var dbUserInfo = dbMgr.GetDBUserInfo(dbRoleInfo.UserID);
                if (null != dbUserInfo)
                    lock (dbUserInfo)
                    {
                        dbUserInfo.LogoutServerTicks = logoutServerTicks;
                    }

                DBWriter.UpdateRoleLogOff(dbMgr, roleID, dbRoleInfo.UserID, dbRoleInfo.ZoneID, ip, onlineSecs);

                DBWriter.UpdatePKMode(dbMgr, roleID, pkMode);

                RoleOnlineManager.RemoveRoleOnlineTicks(roleID);

                strcmd = string.Format("{0}:{1}", roleID, 0);
                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, "", false);
            }

            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
            return TCPProcessCmdResults.RESULT_DATA;
        }

        /// <summary>
        ///     Packet ping giữ liên lạc
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private static TCPProcessCmdResults ProcessOnlineServerHeartCmd(GameServerClient client, DBManager dbMgr,
            TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string cmdData = null;

            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("解析指令字符串错误 , CMD={0}", (TCPGameServerCmds)nID));

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                return TCPProcessCmdResults.RESULT_DATA;
            }

            try
            {
                //解析用户名称和用户密码
                var fields = cmdData.Split(':');
                if (fields.Length != 3)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format(
                        "Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}",
                        (TCPGameServerCmds)nID, fields.Length, cmdData));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                var serverLineID = Convert.ToInt32(fields[0]);
                var serverLineNum = Convert.ToInt32(fields[1]);
                var serverLineCount = Convert.ToInt32(fields[2]);

                if (serverLineCount <= 0) //刚上线的服务器
                    //清空指定线路ID对应的所有用户数据
                    UserOnlineManager.ClearUserIDsByServerLineID(serverLineID);

                //更新服务器状态
                LineManager.UpdateLineHeart(client, serverLineID, serverLineNum);

                var strcmd = string.Format("{0}", 0);
                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                //System.Windows.Application.Current.Dispatcher.Invoke((MethodInvoker)delegate
                //{
                //
                DataHelper.WriteFormatExceptionLog(ex, "", false);
                //throw ex;
                //});
            }

            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
            return TCPProcessCmdResults.RESULT_DATA;
        }

        /// <summary>
        ///     Lấy ra ID máy chủ
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <param name="tcpOutPacket"></param>
        /// <returns></returns>
        private static TCPProcessCmdResults ProcessGetServerIdCmd(DBManager dbMgr, TCPOutPacketPool pool, int nID,
            byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            byte[] bytes = null;
            try
            {
                bytes = DataHelper.ObjectToBytes(GameDBManager.ZoneID);
                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, bytes, 0, bytes.Length, nID);
                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                //System.Windows.Application.Current.Dispatcher.Invoke((MethodInvoker)delegate
                //{
                //
                DataHelper.WriteFormatExceptionLog(ex, "", false);
                //throw ex;
                //});
            }

            bytes = DataHelper.ObjectToBytes(GameDBManager.ZoneID);
            tcpOutPacket =
                TCPOutPacket.MakeTCPOutPacket(pool, bytes, 0, bytes.Length, (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
            return TCPProcessCmdResults.RESULT_DATA;
        }

        /// <summary>
        ///    Nhận nhiệm vụ mới
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private static TCPProcessCmdResults ProcessNewTaskCmd(DBManager dbMgr, TCPOutPacketPool pool, int nID,
            byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string cmdData = null;

            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Wrong socket data CMD={0}", (TCPGameServerCmds)nID));

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                return TCPProcessCmdResults.RESULT_DATA;
            }

            try
            {
                var fields = cmdData.Split(':');
                if (fields.Length != 5)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format(
                        "Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}",
                        (TCPGameServerCmds)nID, fields.Length, cmdData));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                var roleID = Convert.ToInt32(fields[0]);
                var npcID = Convert.ToInt32(fields[1]);
                var taskID = Convert.ToInt32(fields[2]);
                var focus = Convert.ToInt32(fields[3]);
                var nStarLevel = Convert.ToInt32(fields[4]); // 任务星级 [12/5/2013 LiaoWei]

                var now = DateTime.Now;
                var today = now.ToString("yyyy-MM-dd HH:mm:ss");
                var ticks = now.Ticks / 10000;

                var strcmd = "";
                var dbRoleInfo = dbMgr.GetDBRoleInfo(roleID);
                if (null == dbRoleInfo)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Source player is not exist, CMD={0}, RoleID={1}",
                        (TCPGameServerCmds)nID, roleID));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                //将用户的请求发起写数据库的操作
                var ret = DBWriter.NewTask(dbMgr, roleID, npcID, taskID, today, focus, nStarLevel);
                if (ret < 0)
                {
                    //添加任务失败
                    strcmd = string.Format("{0}:{1}:{2}:{3}", roleID, taskID, ticks, ret);

                    LogManager.WriteLog(LogTypes.Error, string.Format("添加任务失败，CMD={0}, RoleID={1}",
                        (TCPGameServerCmds)nID, roleID));
                }
                else
                {
                    if (null == dbRoleInfo.DoingTaskList)
                        dbRoleInfo.DoingTaskList = new ConcurrentDictionary<int, TaskData>();

                    var task = new TaskData
                    {
                        DbID = ret,
                        DoingTaskID = taskID,
                        DoingTaskVal1 = 0,
                        DoingTaskVal2 = 0,
                        DoingTaskFocus = focus,
                        AddDateTime = ticks,
                        TaskType = nStarLevel
                    };

                    dbRoleInfo.DoingTaskList[task.DbID] = task;

                    strcmd = string.Format("{0}:{1}:{2}:{3}", roleID, taskID, ticks, ret);
                }

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                //System.Windows.Application.Current.Dispatcher.Invoke((MethodInvoker)delegate
                //{
                //
                DataHelper.WriteFormatExceptionLog(ex, "", false);
                //throw ex;
                //});
            }

            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
            return TCPProcessCmdResults.RESULT_DATA;
        }

        /// <summary>
        /// Cập nhật vị trí của người chơi
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private static TCPProcessCmdResults ProcessUpdatePosCmd(DBManager dbMgr, TCPOutPacketPool pool, int nID,
            byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string cmdData = null;

            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Wrong socket data CMD={0}", (TCPGameServerCmds)nID));

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                return TCPProcessCmdResults.RESULT_DATA;
            }

            try
            {
                var fields = cmdData.Split(':');
                if (fields.Length != 5)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format(
                        "Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}",
                        (TCPGameServerCmds)nID, fields.Length, cmdData));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                var roleID = Convert.ToInt32(fields[0]);
                var mapCode = Convert.ToInt32(fields[1]);
                var direction = Convert.ToInt32(fields[2]);
                var posX = Convert.ToInt32(fields[3]);
                var posY = Convert.ToInt32(fields[4]);

                var strcmd = "";
                var dbRoleInfo = dbMgr.GetDBRoleInfo(roleID);
                if (null == dbRoleInfo)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Source player is not exist, CMD={0}, RoleID={1}",
                        (TCPGameServerCmds)nID, roleID));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                var ticks = DateTime.Now.Ticks / 10000;
                strcmd = string.Format("{0}:{1}:{2}:{3}", mapCode, direction, posX, posY);

                var updateDBRolePosition = true;

                lock (dbRoleInfo)
                {
                    dbRoleInfo.Position = strcmd;
                }

                var ret = true;

                if (updateDBRolePosition) ret = DBWriter.UpdateRolePosition(dbMgr, roleID, strcmd);

                if (!ret)
                {
                    strcmd = string.Format("{0}:{1}", roleID, -1);

                    LogManager.WriteLog(LogTypes.Error, string.Format("Update role position failed，CMD={0}, RoleID={1}",
                        (TCPGameServerCmds)nID, roleID));
                }
                else
                {
                    strcmd = string.Format("{0}:{1}", roleID, 0);
                }

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, "", false);
            }

            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
            return TCPProcessCmdResults.RESULT_DATA;
        }

        /// <summary>
        ///     Thêm kinh nghiệm cho người chơi
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private static TCPProcessCmdResults ProcessUpdateExpLevelCmd(DBManager dbMgr, TCPOutPacketPool pool, int nID,
            byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string cmdData = null;

            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Wrong socket data CMD={0}", (TCPGameServerCmds)nID));

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                return TCPProcessCmdResults.RESULT_DATA;
            }

            try
            {
                var fields = cmdData.Split(':');
                if (fields.Length != 4)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format(
                        "Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}",
                        (TCPGameServerCmds)nID, fields.Length, cmdData));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                var roleID = Convert.ToInt32(fields[0]);
                var level = Convert.ToInt32(fields[1]);
                var experience = Convert.ToInt64(fields[2]);
                var Prestige = Convert.ToInt32(fields[3]);

                Console.WriteLine("SAVE ROLE :" + roleID + "LEVEL : " + level + " | experience :" + experience);

                var strcmd = "";
                var dbRoleInfo = dbMgr.GetDBRoleInfo(roleID);
                if (null == dbRoleInfo)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Source player is not exist, CMD={0}, RoleID={1}",
                        (TCPGameServerCmds)nID, roleID));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                var ret = DBWriter.UpdateRoleInfo(dbMgr, roleID, level, experience, Prestige);
                if (!ret)
                {
                    strcmd = string.Format("{0}:{1}", roleID, -1);

                    LogManager.WriteLog(LogTypes.Error, string.Format("Có lỗ khi ghi vào csdl，CMD={0}, RoleID={1}",
                        (TCPGameServerCmds)nID, roleID));
                }
                else
                {
                    var userID = "";
                    lock (dbRoleInfo)
                    {
                        dbRoleInfo.Level = level;
                        dbRoleInfo.Experience = experience;
                        dbRoleInfo.Prestige = Prestige;
                        userID = dbRoleInfo.UserID;
                    }

                    // DISABLE REDUCT COST
                    //if (userID != "")
                    //{
                    //    DBUserInfo dbUserInfo = dbMgr.GetDBUserInfo(userID);
                    //    if (null != dbUserInfo)
                    //    {
                    //        lock (dbUserInfo)
                    //        {
                    //            for (int i = 0; i < dbUserInfo.ListRoleLevels.Count; i++)
                    //            {
                    //                if (dbUserInfo.ListRoleIDs[i] == roleID)
                    //                {
                    //                    dbUserInfo.ListRoleLevels[i] = level;
                    //                }
                    //            }
                    //        }
                    //    }
                    //}

                    strcmd = string.Format("{0}:{1}", roleID, 0);
                }

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, "", false);
            }

            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
            return TCPProcessCmdResults.RESULT_DATA;
        }

        /// <summary>
        ///     Cập nhật Avarta nhân vật
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private static TCPProcessCmdResults ProcessUpdateRoleAvartaCmd(DBManager dbMgr, TCPOutPacketPool pool, int nID,
            byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string cmdData = null;

            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Wrong socket data CMD={0}", (TCPGameServerCmds)nID));

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                return TCPProcessCmdResults.RESULT_DATA;
            }

            try
            {
                var fields = cmdData.Split(':');
                if (fields.Length != 2)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format(
                        "Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}",
                        (TCPGameServerCmds)nID, fields.Length, cmdData));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                var roleID = Convert.ToInt32(fields[0]);
                var avartaID = Convert.ToInt32(fields[1]);

                var strcmd = "";
                var dbRoleInfo = dbMgr.GetDBRoleInfo(roleID);
                if (null == dbRoleInfo)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Source player is not exist, CMD={0}, RoleID={1}",
                        (TCPGameServerCmds)nID, roleID));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                var updateDBRoleAvarta = true;

                dbRoleInfo.RolePic = avartaID;

                var ret = true;
                if (updateDBRoleAvarta) ret = DBWriter.UpdateRoleAvarta(dbMgr, roleID, avartaID);

                if (!ret)
                {
                    strcmd = string.Format("{0}:{1}", roleID, -1);
                    LogManager.WriteLog(LogTypes.Error,
                        string.Format("Update role avarta faild，CMD={0}, RoleID={1}", (TCPGameServerCmds)nID, roleID));
                }
                else
                {
                    strcmd = string.Format("{0}:{1}", roleID, 0);
                }

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, "", false);
            }

            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
            return TCPProcessCmdResults.RESULT_DATA;
        }

        /// <summary>
        ///     Cật nhật bạc cho người chơi
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private static TCPProcessCmdResults ProcessUpdateMoney1Cmd(DBManager dbMgr, TCPOutPacketPool pool, int nID,
            byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            /*
             *  注意：如果协议修改了，一定不要忘记确认下是否需要修改ProcessRoleHuobiOffline这个函数
             *  chenjg 20150422
             */
            tcpOutPacket = null;
            string cmdData = null;

            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Wrong socket data CMD={0}", (TCPGameServerCmds)nID));

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                return TCPProcessCmdResults.RESULT_DATA;
            }

            try
            {
                var fields = cmdData.Split(':');
                if (fields.Length != 2)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format(
                        "Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}",
                        (TCPGameServerCmds)nID, fields.Length, cmdData));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                var roleID = Convert.ToInt32(fields[0]);
                var meney1 = Convert.ToInt32(fields[1]);

                var strcmd = "";
                var dbRoleInfo = dbMgr.GetDBRoleInfo(roleID);
                if (null == dbRoleInfo)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Source player is not exist, CMD={0}, RoleID={1}",
                        (TCPGameServerCmds)nID, roleID));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                var ret = DBWriter.UpdateRoleMoney1(dbMgr, roleID, meney1);
                if (!ret)
                {
                    strcmd = string.Format("{0}:{1}", roleID, -1);

                    LogManager.WriteLog(LogTypes.Error, string.Format("更新角色金币失败，CMD={0}, RoleID={1}",
                        (TCPGameServerCmds)nID, roleID));
                }
                else
                {
                    lock (dbRoleInfo.GetMoneyLock)
                    {
                        dbRoleInfo.Money1 = meney1;
                    }

                    strcmd = string.Format("{0}:{1}", roleID, meney1);
                }

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, "", false);
            }

            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
            return TCPProcessCmdResults.RESULT_DATA;
        }

        /// <summary>
        ///     Thêm vật phẩm
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private static TCPProcessCmdResults ProcessAddGoodsCmd(DBManager dbMgr, TCPOutPacketPool pool, int nID,
            byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string cmdData = null;

            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Wrong socket data CMD={0}", (TCPGameServerCmds)nID));

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                return TCPProcessCmdResults.RESULT_DATA;
            }

            try
            {
                var fields = cmdData.Split(':');
                if (fields.Length != 13)
                {
                    LogManager.WriteLog(LogTypes.Error,
                        string.Format("Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}",
                            (TCPGameServerCmds)nID, fields.Length, cmdData));
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                var roleID = Convert.ToInt32(fields[0]);
                var goodsID = Convert.ToInt32(fields[1]);
                var goodsNum = Convert.ToInt32(fields[2]);
                var props = fields[3];
                var forgeLevel = Convert.ToInt32(fields[4]);
                var binding = Convert.ToInt32(fields[5]);
                var site = Convert.ToInt32(fields[6]);
                var bagindex = Convert.ToInt32(fields[7]);
                var startTime = fields[8];
                startTime = startTime.Replace("$", ":");
                var endTime = fields[9];
                endTime = endTime.Replace("$", ":");
                var strong = Convert.ToInt32(fields[10]);
                var series = Convert.ToInt32(fields[11]);
                var otherpramer = fields[12];

                var Base64Decode = Convert.FromBase64String(otherpramer);

                var _OtherParams =
                    DataHelper.BytesToObject<Dictionary<ItemPramenter, string>>(Base64Decode, 0, Base64Decode.Length);

                var strcmd = "";
                var dbRoleInfo = dbMgr.GetDBRoleInfo(roleID);
                if (null == dbRoleInfo)
                {
                    LogManager.WriteLog(LogTypes.Error,
                        string.Format("Source player is not exist, CMD={0}, RoleID={1}", (TCPGameServerCmds)nID,
                            roleID));
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                //Ghi vào DB
                var ret = DBWriter.NewGoods(dbMgr, roleID, goodsID, goodsNum, props, forgeLevel, binding, site,
                    bagindex, startTime, endTime, strong, series, otherpramer);
                if (ret < 0)
                {
                    strcmd = string.Format("{0}:{1}", ret, cmdData);
                    LogManager.WriteLog(LogTypes.Error,
                        string.Format("Có lỗi khi ghi vật phẩm vào DB，CMD={0}, RoleID={1}", (TCPGameServerCmds)nID,
                            roleID));
                }
                else
                {
                    var itemGD = new GoodsData
                    {
                        Id = ret,
                        GoodsID = goodsID,
                        Using = -1,
                        Forge_level = forgeLevel,
                        Starttime = startTime,
                        Endtime = endTime,
                        Site = site,
                        Props = props,
                        GCount = goodsNum,
                        Binding = binding,
                        BagIndex = bagindex,
                        Strong = strong,
                        OtherParams = _OtherParams,
                        Series = series
                    };

                    /// Nếu danh sách chưa tồn taiọ
                    if (null == dbRoleInfo.GoodsDataList)
                        /// Tạo mới
                        dbRoleInfo.GoodsDataList = new ConcurrentDictionary<int, GoodsData>();

                    /// Thêm vật phẩm vào danh sách
                    dbRoleInfo.GoodsDataList[itemGD.Id] = itemGD;

                    /// NẾu mà sử dụng túi thì POrtalBangData tăng thêm ô
                   // if ((int)SaleGoodsConsts.PortableGoodsID == site) dbRoleInfo.MyPortableBagData.GoodsUsedGridNum++;

                    strcmd = string.Format("{0}:{1}", ret, cmdData);
                }

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, "", false);
            }

            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
            return TCPProcessCmdResults.RESULT_DATA;
        }

        /// <summary>
        ///     Hàm update Good CMD
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private static TCPProcessCmdResults ProcessUpdateGoodsCmd(DBManager dbMgr, TCPOutPacketPool pool, int nID,
            byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string cmdData = null;

            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Wrong socket data CMD={0}", (TCPGameServerCmds)nID));

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                return TCPProcessCmdResults.RESULT_DATA;
            }

            try
            {
                var fields = cmdData.Split(':');

                if (fields.Length != 16)
                {
                    LogManager.WriteLog(LogTypes.Error,
                        string.Format("Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}",
                            (TCPGameServerCmds)nID, fields.Length, cmdData));
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                var roleID = Convert.ToInt32(fields[0]);
                var id = Convert.ToInt32(fields[1]);

                var strcmd = "";

                // nếu sử dụng quee
                //if (GameDBManager.IsUsingQueeItem)
                //{
                //    //var _CacheModel = new ItemCacheModel();
                //    //_CacheModel.Fields = fields;
                //    //_CacheModel.ItemID = id;
                //    //_CacheModel.RoleId = roleID;

                //    //ItemManager.getInstance().AddItemProsecc(_CacheModel);

                //    strcmd = string.Format("{0}:{1}", id, 0);
                //}
                //else
                {
                    var dbRoleInfo = dbMgr.GetDBRoleInfo(roleID);
                    if (null == dbRoleInfo)
                    {
                        LogManager.WriteLog(LogTypes.Error, string.Format(
                            "Source player is not exist, CMD={0}, RoleID={1}",
                            (TCPGameServerCmds)nID, roleID));

                        tcpOutPacket =
                            TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                        return TCPProcessCmdResults.RESULT_DATA;
                    }

                    /// Thông tin vật phẩm tương ứng
                    var itemGD = Global.GetGoodsDataByDbID(dbRoleInfo, id);

                    //Lấy thông tin GOOD TỪ DATA RA Nếu không tìm thấy vật phẩm thì return lỗi -1000
                    if (null == itemGD)
                    {
                        strcmd = string.Format("{0}:{1}", id, -1000);
                        tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                        return TCPProcessCmdResults.RESULT_DATA;
                    }

                    //Thực hiện update vào DB theo lệnh
                    var ret = DBWriter.UpdateGoods(dbMgr, id, fields, 2);
                    if (ret < 0)
                    {
                        strcmd = string.Format("{0}:{1}", id, ret);
                        LogManager.WriteLog(LogTypes.Error,
                            string.Format("Update role item failed，CMD={0}, RoleID={1}", (TCPGameServerCmds)nID,
                                roleID));
                    }
                    else
                    {
                        var gcount = DataHelper.ConvertToInt32(fields[8], itemGD.GCount);

                        if (gcount > 0)
                        {
                            var newSite = DataHelper.ConvertToInt32(fields[6], itemGD.Site);

                            itemGD.Using = DataHelper.ConvertToInt32(fields[2], itemGD.Using);
                            itemGD.Forge_level = DataHelper.ConvertToInt32(fields[3], itemGD.Forge_level);
                            itemGD.Starttime = DataHelper.ConvertToStr(fields[4].Replace("#", ":"), itemGD.Starttime);
                            itemGD.Endtime = DataHelper.ConvertToStr(fields[5].Replace("#", ":"), itemGD.Endtime);
                            itemGD.Site = newSite;
                            itemGD.Props = DataHelper.ConvertToStr(fields[7], itemGD.Props);
                            itemGD.GCount = gcount;
                            itemGD.Binding = DataHelper.ConvertToInt32(fields[9], itemGD.Binding);
                            itemGD.BagIndex = DataHelper.ConvertToInt32(fields[10], itemGD.BagIndex);
                            itemGD.Strong = DataHelper.ConvertToInt32(fields[11], itemGD.Strong);
                            itemGD.Series = DataHelper.ConvertToInt32(fields[12], itemGD.Series);

                            var otherpramer = fields[13];
                            if (otherpramer.Length > 10)
                            {
                                var Base64Decode = Convert.FromBase64String(otherpramer);

                                var _OtherParams =
                                    DataHelper.BytesToObject<Dictionary<ItemPramenter, string>>(Base64Decode, 0,
                                        Base64Decode.Length);
                                itemGD.OtherParams = _OtherParams;
                            }

                            itemGD.GoodsID = DataHelper.ConvertToInt32(fields[14], itemGD.GoodsID);
                        }
                        else
                        {
                            // Nếu như là đồ vứt ra đất thì ta sẽ backup cho nó vào bak
                            if (DataHelper.ConvertToInt32(fields[6], itemGD.Site) < 0)
                            {
                                if (dbRoleInfo.DropDataList == null)
                                {
                                    dbRoleInfo.DropDataList = new ConcurrentDictionary<int, GoodsData>();
                                    dbRoleInfo.DropDataList.TryAdd(itemGD.Id, itemGD);
                                }
                                else
                                {
                                    //ADd Item Này vào
                                    dbRoleInfo.DropDataList.TryAdd(itemGD.Id, itemGD);
                                }
                                // thực hiện backup nếu thằng này vứt ra đất
                                DBWriter.BackUpAndDelete(dbMgr, id);
                            }
                            else
                            {
                                //Xóa vĩnh viên khỏi DB
                                DBWriter.DeleteItemInDb(dbMgr, id);
                            }
                            /// Xóa vật phẩm khỏi danh sách
                            dbRoleInfo.GoodsDataList.TryRemove(itemGD.Id, out _);
                        }

                        strcmd = string.Format("{0}:{1}", id, 0);
                    }
                }

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, "", false);
            }

            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
            return TCPProcessCmdResults.RESULT_DATA;
        }

        /// <summary>
        ///     Cập nhật thông tin nhiệm vụ
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private static TCPProcessCmdResults ProcessUpdateTaskCmd(DBManager dbMgr, TCPOutPacketPool pool, int nID,
            byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string cmdData = null;

            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Wrong socket data CMD={0}", (TCPGameServerCmds)nID));

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                return TCPProcessCmdResults.RESULT_DATA;
            }

            try
            {
                var fields = cmdData.Split(':');
                if (fields.Length != 6)
                {
                    LogManager.WriteLog(LogTypes.Error,
                        string.Format("Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}",
                            (TCPGameServerCmds)nID, fields.Length, cmdData));
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                var roleID = Convert.ToInt32(fields[0]);
                var taskID = Convert.ToInt32(fields[1]);
                var dbID = Convert.ToInt32(fields[2]);

                var strcmd = "";
                var dbRoleInfo = dbMgr.GetDBRoleInfo(roleID);
                if (null == dbRoleInfo)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Source player is not exist, CMD={0}, RoleID={1}",
                        (TCPGameServerCmds)nID, roleID));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                var ret = DBWriter.UpdateTask(dbMgr, dbID, fields, 3);
                if (ret < 0)
                {
                    strcmd = string.Format("{0}:{1}:{2}", roleID, taskID, -1);
                    LogManager.WriteLog(LogTypes.Error,
                        string.Format("Update role task failed，CMD={0}, RoleID={1}", (TCPGameServerCmds)nID, roleID));
                }
                else
                {
                    if (null != dbRoleInfo.DoingTaskList)
                        if (dbRoleInfo.DoingTaskList.TryGetValue(dbID, out var task))
                        {
                            task.DoingTaskFocus = DataHelper.ConvertToInt32(fields[3], task.DoingTaskFocus);
                            task.DoingTaskVal1 = DataHelper.ConvertToInt32(fields[4], task.DoingTaskVal1);
                            task.DoingTaskVal2 = DataHelper.ConvertToInt32(fields[4], task.DoingTaskVal2);
                        }

                    strcmd = string.Format("{0}:{1}:{2}", roleID, taskID, 0);
                }

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, "", false);
            }

            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
            return TCPProcessCmdResults.RESULT_DATA;
        }

        /// <summary>
        ///     Hoàn thành nhiệm vụ
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private static TCPProcessCmdResults ProcessCompleteTaskCmd(DBManager dbMgr, TCPOutPacketPool pool, int nID,
            byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string cmdData = null;

            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Wrong socket data CMD={0}", (TCPGameServerCmds)nID));

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                return TCPProcessCmdResults.RESULT_DATA;
            }

            try
            {
                var fields = cmdData.Split(':');
                if (fields.Length != 6)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format(
                        "Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}",
                        (TCPGameServerCmds)nID, fields.Length, cmdData));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                var roleID = Convert.ToInt32(fields[0]);
                var npcID = Convert.ToInt32(fields[1]);
                var taskID = Convert.ToInt32(fields[2]);
                var dbID = Convert.ToInt32(fields[3]);
                var isMainTask = Convert.ToInt32(fields[4]);
                var taskclass = Convert.ToInt32(fields[5]);

                var strcmd = "";
                var dbRoleInfo = dbMgr.GetDBRoleInfo(roleID);
                if (null == dbRoleInfo)
                {
                    LogManager.WriteLog(LogTypes.Error,
                        string.Format("Source player is not exist, CMD={0}, RoleID={1}", (TCPGameServerCmds)nID,
                            roleID));
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                var ret = DBWriter.CompleteTask(dbMgr, roleID, npcID, taskID, dbID, taskclass);
                if (!ret)
                {
                    strcmd = string.Format("{0}:{1}:{2}", roleID, taskID, -1);
                    LogManager.WriteLog(LogTypes.Error,
                        string.Format("Complete task failed，CMD={0}, RoleID={1}", (TCPGameServerCmds)nID, roleID));
                }
                else
                {
                    var needUpdateMainTaskID = false;

                    if (isMainTask > 0 && taskID > dbRoleInfo.MainTaskID)
                    {
                        dbRoleInfo.MainTaskID = taskID;
                        needUpdateMainTaskID = true;
                    }

                    if (null != dbRoleInfo.DoingTaskList)
                        if (dbRoleInfo.DoingTaskList.ContainsKey(dbID))
                            dbRoleInfo.DoingTaskList.TryRemove(dbID, out _);

                    if (null == dbRoleInfo.OldTasks) dbRoleInfo.OldTasks = new List<OldTaskData>();

                    var findIndex = -1;
                    for (var i = 0; i < dbRoleInfo.OldTasks.Count; i++)
                        if (dbRoleInfo.OldTasks[i].TaskID == taskID)
                        {
                            findIndex = i;
                            break;
                        }

                    if (findIndex >= 0)
                        dbRoleInfo.OldTasks[findIndex].DoCount++;
                    else
                        dbRoleInfo.OldTasks.Add(new OldTaskData
                        {
                            TaskID = taskID,
                            DoCount = 1
                        });

                    if (needUpdateMainTaskID && isMainTask > 0) DBWriter.UpdateRoleMainTaskID(dbMgr, roleID, taskID);

                    strcmd = string.Format("{0}:{1}:{2}", roleID, taskID, 0);
                }

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, "", false);
            }

            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
            return TCPProcessCmdResults.RESULT_DATA;
        }

        /// <summary>
        ///  Lấy ra thông tin hảo hữu
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private static TCPProcessCmdResults ProcessGetFriendsCmd(DBManager dbMgr, TCPOutPacketPool pool, int nID,
            byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string cmdData = null;

            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Wrong socket data CMD={0}", (TCPGameServerCmds)nID));

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                return TCPProcessCmdResults.RESULT_DATA;
            }

            try
            {
                var fields = cmdData.Split(':');
                if (fields.Length != 1)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format(
                        "Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}",
                        (TCPGameServerCmds)nID, fields.Length, cmdData));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                var roleID = Convert.ToInt32(fields[0]);

                var dbRoleInfo = dbMgr.GetDBRoleInfo(roleID);
                if (null == dbRoleInfo)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Source player is not exist, CMD={0}, RoleID={1}",
                        (TCPGameServerCmds)nID, roleID));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                List<FriendData> friendDataList = null;

                friendDataList = new List<FriendData>();

                if (null == dbRoleInfo.FriendDataList) dbRoleInfo.FriendDataList = new List<FriendData>();

                for (var i = 0; i < dbRoleInfo.FriendDataList.Count; i++)
                    friendDataList.Add(new FriendData
                    {
                        DbID = dbRoleInfo.FriendDataList[i].DbID,
                        OtherRoleID = dbRoleInfo.FriendDataList[i].OtherRoleID,
                        FriendType = dbRoleInfo.FriendDataList[i].FriendType,
                        Relationship = dbRoleInfo.FriendDataList[i].Relationship
                    });

                var toSendFriendDataList = new List<FriendData>();
                for (var i = 0; i < friendDataList.Count; i++)
                {
                    var otherDbRoleInfo = dbMgr.GetDBRoleInfo(friendDataList[i].OtherRoleID);
                    if (null == otherDbRoleInfo) continue;

                    // chenjingui. 20150703 获取好友名字从dbroleinfo获取，改名系统不做修改
                    friendDataList[i].OtherRoleName = Global.FormatRoleName(otherDbRoleInfo);
                    friendDataList[i].OtherLevel = otherDbRoleInfo.Level;
                    friendDataList[i].FactionID = otherDbRoleInfo.Occupation;
                    friendDataList[i].OnlineState = Global.GetRoleOnlineState(otherDbRoleInfo);
                    var positionInfo = otherDbRoleInfo.Position.Split(':');
                    friendDataList[i].MapCode = Convert.ToInt32(positionInfo[0]);

                    friendDataList[i].GuildID = otherDbRoleInfo.GuildID;
                    friendDataList[i].PicCode = otherDbRoleInfo.RolePic;
                    friendDataList[i].SpouseId = 0;
                    toSendFriendDataList.Add(friendDataList[i]);
                }

                tcpOutPacket = DataHelper.ObjectToTCPOutPacket(toSendFriendDataList, pool, nID);
                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, "", false);
            }

            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
            return TCPProcessCmdResults.RESULT_DATA;
        }

        /// <summary>
        ///     Thêm hảo hữu
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private static TCPProcessCmdResults ProcessAddFriendCmd(DBManager dbMgr, TCPOutPacketPool pool, int nID,
            byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string cmdData = null;

            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Wrong socket data CMD={0}", (TCPGameServerCmds)nID));

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                return TCPProcessCmdResults.RESULT_DATA;
            }

            try
            {
                var fields = cmdData.Split(':');
                if (fields.Length != 4)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format(
                        "Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}",
                        (TCPGameServerCmds)nID, fields.Length, cmdData));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                var dbID = Convert.ToInt32(fields[0]);
                var roleID = Convert.ToInt32(fields[1]);
                var otherName = fields[2];
                var friendType = Convert.ToInt32(fields[3]);

                var dbRoleInfo = dbMgr.GetDBRoleInfo(roleID);
                if (null == dbRoleInfo)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Source player is not exist, CMD={0}, RoleID={1}",
                        (TCPGameServerCmds)nID, roleID));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                var alreadyExists = false;
                int type0Count = 0, type1Count = 0, type2Count = 0;

                var findFriendDataList = new List<FriendData>();

                if (null != dbRoleInfo.FriendDataList)
                    for (var i = 0; i < dbRoleInfo.FriendDataList.Count; i++)
                        findFriendDataList.Add(dbRoleInfo.FriendDataList[i]);
                for (var i = 0; i < findFriendDataList.Count; i++)
                {
                    var existsOtherRoleName = string.Empty;

                    var otherDbRoleInfo = dbMgr.GetDBRoleInfo(findFriendDataList[i].OtherRoleID);
                    if (null != otherDbRoleInfo) existsOtherRoleName = Global.FormatRoleName(otherDbRoleInfo);

                    if (!string.IsNullOrEmpty(existsOtherRoleName) && existsOtherRoleName == otherName &&
                        findFriendDataList[i].FriendType == friendType) alreadyExists = true;

                    if (findFriendDataList[i].FriendType == 0)
                        type0Count++;
                    else if (findFriendDataList[i].FriendType == 1)
                        type1Count++;
                    else
                        type2Count++;
                }

                var canAdded = !alreadyExists;
                if (canAdded)
                {
                    if (friendType == 0)
                    {
                        if (type0Count >= (int)FriendsConsts.MaxFriendsNum) canAdded = false;
                    }
                    else if (friendType == 1)
                    {
                        if (type1Count >= (int)FriendsConsts.MaxBlackListNum) canAdded = false;
                    }
                    else
                    {
                        if (type2Count >= (int)FriendsConsts.MaxEnemiesNum) canAdded = false;
                    }
                }

                FriendData friendData = null;

                if (canAdded)
                {
                    var otherID = dbMgr.DBRoleMgr.FindDBRoleID(otherName);
                    if (-1 == otherID)
                    {
                        friendData = new FriendData
                        {
                            DbID = -10000,
                            OtherRoleID = 0,
                            OtherRoleName = "",
                            OtherLevel = 1,
                            FactionID = 0,
                            OnlineState = 0,
                            MapCode = -1,
                            PosX = -1,
                            PosY = -1,
                            FriendType = friendType,
                            GuildID = -1,
                            PicCode = 0,
                            Relationship = 0
                        };

                        LogManager.WriteLog(LogTypes.Error, string.Format(
                            "Add friend failed. Role name not found，CMD={0}, RoleID={1}, OtherName={2}",
                            (TCPGameServerCmds)nID, roleID, otherName));
                    }
                    else
                    {
                        var ret = DBWriter.AddFriend(dbMgr, dbID, roleID, otherID, friendType, 0);
                        if (ret < 0)
                        {
                            friendData = new FriendData
                            {
                                DbID = ret,
                                OtherRoleID = 0,
                                OtherRoleName = "",
                                OtherLevel = 1,
                                FactionID = 0,
                                OnlineState = 0,
                                MapCode = -1,
                                PosX = -1,
                                PosY = -1,
                                FriendType = 0,
                                GuildID = -1,
                                PicCode = 0,
                                Relationship = 0
                            };

                            LogManager.WriteLog(LogTypes.Error, string.Format("Add friend failed, DB writer error，CMD={0}, RoleID={1}",
                                (TCPGameServerCmds)nID, roleID));
                        }
                        else
                        {
                            lock (dbRoleInfo)
                            {
                                if (null == dbRoleInfo.FriendDataList)
                                    dbRoleInfo.FriendDataList = new List<FriendData>();

                                var findIndex = -1;
                                for (var i = 0; i < dbRoleInfo.FriendDataList.Count; i++)
                                    if (dbRoleInfo.FriendDataList[i].DbID == ret)
                                    {
                                        findIndex = i;
                                        break;
                                    }

                                if (findIndex >= 0)
                                {
                                    dbRoleInfo.FriendDataList[findIndex].OtherRoleID = otherID;
                                    dbRoleInfo.FriendDataList[findIndex].FriendType = friendType;
                                }
                                else
                                {
                                    dbRoleInfo.FriendDataList.Add(new FriendData
                                    {
                                        DbID = ret,
                                        OtherRoleID = otherID,
                                        FriendType = friendType
                                    });
                                }
                            }

                            var otherDbRoleInfo = dbMgr.GetDBRoleInfo(otherID);
                            /// Thêm bạn cho thằng này
                            if (otherDbRoleInfo != null)
                                lock (otherDbRoleInfo)
                                {
                                    if (null == otherDbRoleInfo.FriendDataList)
                                        otherDbRoleInfo.FriendDataList = new List<FriendData>();

                                    var findIndex = -1;
                                    for (var i = 0; i < otherDbRoleInfo.FriendDataList.Count; i++)
                                        if (otherDbRoleInfo.FriendDataList[i].DbID == ret)
                                        {
                                            findIndex = i;
                                            break;
                                        }

                                    if (findIndex >= 0)
                                    {
                                        otherDbRoleInfo.FriendDataList[findIndex].OtherRoleID = roleID;
                                        otherDbRoleInfo.FriendDataList[findIndex].FriendType = friendType;
                                    }
                                    else
                                    {
                                        otherDbRoleInfo.FriendDataList.Add(new FriendData
                                        {
                                            DbID = ret,
                                            OtherRoleID = roleID,
                                            FriendType = friendType
                                        });
                                    }
                                }

                            if (null == otherDbRoleInfo)
                            {
                                friendData = new FriendData
                                {
                                    DbID = -10000,
                                    OtherRoleID = 0,
                                    OtherRoleName = "",
                                    OtherLevel = 1,
                                    FactionID = 0,
                                    OnlineState = 0,
                                    MapCode = -1,
                                    PosX = -1,
                                    PosY = -1,
                                    FriendType = friendType,
                                    GuildID = -1,
                                    PicCode = 0,
                                    Relationship = 0
                                };
                            }
                            else
                            {
                                var positionInfo = otherDbRoleInfo.Position.Split(':');
                                friendData = new FriendData
                                {
                                    DbID = ret,
                                    OtherRoleID = otherDbRoleInfo.RoleID,
                                    OtherRoleName = Global.FormatRoleName(otherDbRoleInfo),
                                    OtherLevel = otherDbRoleInfo.Level,
                                    FactionID = otherDbRoleInfo.Occupation,
                                    OnlineState = Global.GetRoleOnlineState(otherDbRoleInfo),
                                    MapCode = Convert.ToInt32(positionInfo[0]),
                                    PosX = Convert.ToInt32(positionInfo[2]),
                                    PosY = Convert.ToInt32(positionInfo[3]),
                                    FriendType = friendType,
                                    SpouseId = 0,
                                    GuildID = otherDbRoleInfo.GuildID,
                                    PicCode = otherDbRoleInfo.RolePic,
                                    Relationship = 0
                                };
                            }
                        }
                    }
                }
                else
                {
                    friendData = new FriendData
                    {
                        DbID = alreadyExists ? -10002 : -10001,
                        OtherRoleID = 0,
                        OtherRoleName = "",
                        OtherLevel = 1,
                        FactionID = 0,
                        OnlineState = 0,
                        MapCode = -1,
                        PosX = -1,
                        PosY = -1,
                        FriendType = friendType,
                        GuildID = -1,
                        PicCode = 0,
                        Relationship = 0
                    };

                    LogManager.WriteLog(LogTypes.Error, string.Format("Add friend failed, already exist，CMD={0}, RoleID={1}",
                        (TCPGameServerCmds)nID, roleID));
                }

                /// 将对象转为TCP协议流
                tcpOutPacket = DataHelper.ObjectToTCPOutPacket(friendData, pool, nID);
                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, "", false);
            }

            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
            return TCPProcessCmdResults.RESULT_DATA;
        }

        /// <summary>
        ///     Xóa bỏ hảo hữu
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private static TCPProcessCmdResults ProcessRemoveFriendCmd(DBManager dbMgr, TCPOutPacketPool pool, int nID,
            byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string cmdData = null;

            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Wrong socket data CMD={0}", (TCPGameServerCmds)nID));

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                return TCPProcessCmdResults.RESULT_DATA;
            }

            try
            {
                var fields = cmdData.Split(':');
                if (fields.Length != 2)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format(
                        "Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}",
                        (TCPGameServerCmds)nID, fields.Length, cmdData));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                var dbID = Convert.ToInt32(fields[0]);
                var roleID = Convert.ToInt32(fields[1]);

                var dbRoleInfo = dbMgr.GetDBRoleInfo(roleID);
                if (null == dbRoleInfo)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Source player is not exist, CMD={0}, RoleID={1}",
                        (TCPGameServerCmds)nID, roleID));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                var strcmd = "";

                /// Xóa của thằng
                var ret = DBWriter.RemoveFriend(dbMgr, dbID, roleID);
                if (!ret)
                {
                    //删除朋友失败
                    strcmd = string.Format("{0}:{1}:{2}", dbID, roleID, -1);

                    LogManager.WriteLog(LogTypes.Error, string.Format("删除好友时失败，CMD={0}, RoleID={1}",
                        (TCPGameServerCmds)nID, roleID));
                }
                else
                {
                    /// ID thằng kia
                    var otherID = -1;
                    lock (dbRoleInfo)
                    {
                        if (null == dbRoleInfo.FriendDataList) dbRoleInfo.FriendDataList = new List<FriendData>();

                        var findIndex = -1;
                        for (var i = 0; i < dbRoleInfo.FriendDataList.Count; i++)
                            if (dbRoleInfo.FriendDataList[i].DbID == dbID)
                            {
                                findIndex = i;
                                otherID = dbRoleInfo.FriendDataList[i].OtherRoleID;
                                break;
                            }

                        if (findIndex >= 0) dbRoleInfo.FriendDataList.RemoveAt(findIndex);
                    }

                    /// Thông tin thằng kia
                    var otherDbRoleInfo = dbMgr.GetDBRoleInfo(otherID);
                    /// Xóa bạn cho cả thằng kia
                    lock (otherDbRoleInfo)
                    {
                        if (null == otherDbRoleInfo.FriendDataList)
                            otherDbRoleInfo.FriendDataList = new List<FriendData>();

                        var findIndex = -1;
                        for (var i = 0; i < otherDbRoleInfo.FriendDataList.Count; i++)
                            if (otherDbRoleInfo.FriendDataList[i].DbID == dbID)
                            {
                                findIndex = i;
                                break;
                            }

                        if (findIndex >= 0) otherDbRoleInfo.FriendDataList.RemoveAt(findIndex);
                    }

                    strcmd = string.Format("{0}:{1}:{2}", dbID, roleID, 0);
                }

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, "", false);
            }

            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
            return TCPProcessCmdResults.RESULT_DATA;
        }

        /// <summary>
        ///  Cập nhật chế độ PK
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private static TCPProcessCmdResults ProcessUpdatePKModeCmd(DBManager dbMgr, TCPOutPacketPool pool, int nID,
            byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string cmdData = null;

            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Wrong socket data CMD={0}", (TCPGameServerCmds)nID));

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                return TCPProcessCmdResults.RESULT_DATA;
            }

            try
            {
                var fields = cmdData.Split(':');
                if (fields.Length != 2)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format(
                        "Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}",
                        (TCPGameServerCmds)nID, fields.Length, cmdData));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                var roleID = Convert.ToInt32(fields[0]);
                var pkMode = Convert.ToInt32(fields[1]);

                var dbRoleInfo = dbMgr.GetDBRoleInfo(roleID);
                if (null == dbRoleInfo)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Source player is not exist, CMD={0}, RoleID={1}",
                        (TCPGameServerCmds)nID, roleID));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                var strcmd = "";

                //将用户的请求发起写数据库的操作, 2011-05-31 精简指令，放到offline中写入
                //bool ret = DBWriter.UpdatePKMode(dbMgr, roleID, pkMode);
                var ret = true;
                if (!ret)
                {
                    //删除朋友失败
                    strcmd = string.Format("{0}:{1}:{2}", roleID, pkMode, -1);

                    LogManager.WriteLog(LogTypes.Error, string.Format("更新角色PK模式时失败，CMD={0}, RoleID={1}",
                        (TCPGameServerCmds)nID, roleID));
                }
                else
                {
                    dbRoleInfo.PKMode = pkMode;

                    strcmd = string.Format("{0}:{1}:{2}", roleID, pkMode, -1);
                }

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                //System.Windows.Application.Current.Dispatcher.Invoke((MethodInvoker)delegate
                //{
                //
                DataHelper.WriteFormatExceptionLog(ex, "", false);
                //throw ex;
                //});
            }

            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
            return TCPProcessCmdResults.RESULT_DATA;
        }

        /// <summary>
        ///     Cập nhật trị PK
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private static TCPProcessCmdResults ProcessUpdatePKValCmd(DBManager dbMgr, TCPOutPacketPool pool, int nID,
            byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string cmdData = null;

            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Wrong socket data CMD={0}", (TCPGameServerCmds)nID));

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                return TCPProcessCmdResults.RESULT_DATA;
            }

            try
            {
                var fields = cmdData.Split(':');
                if (fields.Length != 3)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format(
                        "Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}",
                        (TCPGameServerCmds)nID, fields.Length, cmdData));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                var roleID = Convert.ToInt32(fields[0]);
                var pkValue = Convert.ToInt32(fields[1]);
                var pkPoint = Convert.ToInt32(fields[2]);

                var dbRoleInfo = dbMgr.GetDBRoleInfo(roleID);
                if (null == dbRoleInfo)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Source player is not exist, CMD={0}, RoleID={1}",
                        (TCPGameServerCmds)nID, roleID));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                var strcmd = "";

                var ret = DBWriter.UpdatePKValues(dbMgr, roleID, pkValue, pkPoint);
                if (!ret)
                {
                    strcmd = string.Format("{0}:{1}:{2}:{3}", roleID, pkValue, pkPoint, -1);

                    LogManager.WriteLog(LogTypes.Error,
                        string.Format("Update role PKValue failed，CMD={0}, RoleID={1}", (TCPGameServerCmds)nID,
                            roleID));
                }
                else
                {
                    lock (dbRoleInfo)
                    {
                        dbRoleInfo.PKValue = pkValue;
                        dbRoleInfo.PKPoint = pkPoint;
                    }

                    strcmd = string.Format("{0}:{1}:{2}:{3}", roleID, pkValue, pkPoint, 0);
                }

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, "", false);
            }

            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
            return TCPProcessCmdResults.RESULT_DATA;
        }

        /// <summary>
        ///     Bỏ nhiệm vụ
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private static TCPProcessCmdResults ProcessAbandonTaskCmd(DBManager dbMgr, TCPOutPacketPool pool, int nID,
            byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string cmdData = null;

            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Wrong socket data CMD={0}", (TCPGameServerCmds)nID));

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                return TCPProcessCmdResults.RESULT_DATA;
            }

            try
            {
                var fields = cmdData.Split(':');
                if (fields.Length != 3)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format(
                        "Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}",
                        (TCPGameServerCmds)nID, fields.Length, cmdData));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                var roleID = Convert.ToInt32(fields[0]);
                var dbID = Convert.ToInt32(fields[1]);
                var taskID = Convert.ToInt32(fields[2]);

                var dbRoleInfo = dbMgr.GetDBRoleInfo(roleID);
                if (null == dbRoleInfo)
                {
                    LogManager.WriteLog(LogTypes.Error,
                        string.Format("Source player is not exist, CMD={0}, RoleID={1}", (TCPGameServerCmds)nID,
                            roleID));
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                var strcmd = "";

                var ret = DBWriter.DeleteTask(dbMgr, roleID, taskID, dbID);
                if (!ret)
                {
                    strcmd = string.Format("{0}", -1);
                    LogManager.WriteLog(LogTypes.Error,
                        string.Format("Abandon task failed，CMD={0}, RoleID={1}, TaskID={2}", (TCPGameServerCmds)nID,
                            roleID, taskID));
                }
                else
                {
                    if (null != dbRoleInfo.DoingTaskList)
                        if (dbRoleInfo.DoingTaskList.ContainsKey(dbID))
                            dbRoleInfo.DoingTaskList.TryRemove(dbID, out _);

                    strcmd = string.Format("{0}", 0);
                }

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                //System.Windows.Application.Current.Dispatcher.Invoke((MethodInvoker)delegate
                //{
                //
                DataHelper.WriteFormatExceptionLog(ex, "", false);
                //throw ex;
                //});
            }

            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
            return TCPProcessCmdResults.RESULT_DATA;
        }

        /// <summary>
        ///     Set maintask = lệnh GM
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <param name="tcpOutPacket"></param>
        /// <returns></returns>
        private static TCPProcessCmdResults ProcessGMSetTaskCmd(DBManager dbMgr, TCPOutPacketPool pool, int nID,
            byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;

            try
            {
                List<int> taskList = DataHelper.BytesToObject<List<int>>(data, 0, count);
                if (null != taskList && taskList.Count >= 2)
                {
                    int roleID = taskList[0];
                    int taskID = taskList[taskList.Count - 1];

                    DBRoleInfo dbRoleInfo = dbMgr.GetDBRoleInfo(roleID);
                    if (null != dbRoleInfo)
                    {
                        lock (dbRoleInfo)
                        {
                            DBWriter.GMSetTask(dbMgr, roleID, taskID, taskList);
                            DBWriter.UpdateRoleMainTaskID(dbMgr, roleID, taskID);

                            dbRoleInfo.MainTaskID = taskID;
                            dbRoleInfo.OldTasks = new List<OldTaskData>();

                            //Update cái này nhé
                            dbRoleInfo.DoingTaskList = new ConcurrentDictionary<int, TaskData>();

                            for (int i = 1; i < taskList.Count; i++)
                            {
                                dbRoleInfo.OldTasks.Add(new OldTaskData() { TaskID = taskList[i], DoCount = 1 });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, "", false);
            }

            var bytes = DataHelper.ObjectToBytes(0);
            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, bytes, 0, bytes.Length, nID);
            return TCPProcessCmdResults.RESULT_DATA;
        }

        /// <summary>
        /// Thực hiện thiết lập kỹ năng vào khung dùng nhanh
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private static TCPProcessCmdResults ProcessModKeysCmd(DBManager dbMgr, TCPOutPacketPool pool, int nID,
            byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string cmdData = null;

            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Wrong socket data CMD={0}", (TCPGameServerCmds)nID));

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                return TCPProcessCmdResults.RESULT_DATA;
            }

            try
            {
                var fields = cmdData.Split(':');
                if (fields.Length != 3)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format(
                        "Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}",
                        (TCPGameServerCmds)nID, fields.Length, cmdData));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                var roleID = Convert.ToInt32(fields[0]);
                var type = Convert.ToInt32(fields[1]);

                var dbRoleInfo = dbMgr.GetDBRoleInfo(roleID);
                if (null == dbRoleInfo)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Source player is not exist, CMD={0}, RoleID={1}",
                        (TCPGameServerCmds)nID, roleID));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                var keys = fields[2];
                var strcmd = "";

                /// Thực hiện lưu vào DB
                var ret = DBWriter.UpdateRoleKeys(dbMgr, roleID, type, keys);
                if (!ret)
                {
                    strcmd = string.Format("{0}", -1);

                    LogManager.WriteLog(LogTypes.Error,
                        string.Format("Update role Keys failed，CMD={0}, RoleID={1}",
                            (TCPGameServerCmds)nID, roleID));
                }
                else
                {
                    //lock (dbRoleInfo)
                    {
                        if (type == 0)
                        {
                            dbRoleInfo.MainQuickBarKeys = keys;
                        }
                        else if (type == 1)
                        {
                            dbRoleInfo.OtherQuickBarKeys = keys;
                        }
                        else if (type == 2)
                        {
                            dbRoleInfo.QuickItems = keys;
                        }
                    }

                    strcmd = string.Format("{0}", 0);
                }

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, "", false);
            }

            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
            return TCPProcessCmdResults.RESULT_DATA;
        }

        /// <summary>
        /// Thực hiện thiết lập mật khẩu cấp 2
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private static TCPProcessCmdResults ProcessUpdateSecondPasswordCmd(DBManager dbMgr, TCPOutPacketPool pool, int nID,
            byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string cmdData = null;

            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Wrong socket data CMD={0}", (TCPGameServerCmds)nID));

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                return TCPProcessCmdResults.RESULT_DATA;
            }

            try
            {
                var fields = cmdData.Split(':');
                if (fields.Length != 2)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format(
                        "Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}",
                        (TCPGameServerCmds)nID, fields.Length, cmdData));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                var roleID = Convert.ToInt32(fields[0]);
                var secondPassword = fields[1];

                var dbRoleInfo = dbMgr.GetDBRoleInfo(roleID);
                if (null == dbRoleInfo)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Source player is not exist, CMD={0}, RoleID={1}",
                        (TCPGameServerCmds)nID, roleID));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                /// Thực hiện lưu vào DB
                var ret = DBWriter.UpdateRoleSecondPassword(dbMgr, roleID, secondPassword);
                if (!ret)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Update role second password failed，CMD={0}, RoleID={1}", (TCPGameServerCmds)nID, roleID));
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", nID);
                }
                else
                {
                    //lock (dbRoleInfo)
                    {
                        dbRoleInfo.SecondPassword = secondPassword;
                    }
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "1", nID);
                }

                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, "", false);
            }

            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
            return TCPProcessCmdResults.RESULT_DATA;
        }

        /// <summary>
        /// Cập nhật đồng cho người chơi
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <param name="tcpOutPacket"></param>
        /// <returns></returns>
        private static TCPProcessCmdResults ProcessUpdateUserMoneyCmd(DBManager dbMgr, TCPOutPacketPool pool, int nID,
            byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string cmdData = null;

            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Wrong socket data CMD={0}", (TCPGameServerCmds)nID));

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                return TCPProcessCmdResults.RESULT_DATA;
            }

            try
            {
                var fields = cmdData.Split(':');
                // roleid, addmoney, result
                if (fields.Length != 2 && fields.Length != 3 && fields.Length != 4)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format(
                        "Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}",
                        (TCPGameServerCmds)nID, fields.Length, cmdData));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                var roleID = Convert.ToInt32(fields[0]);
                var addOrSubUserMoney = Convert.ToInt32(fields[1]);
                var activeid = 0;
                var param = "";
                if (fields.Length >= 3)
                    activeid = Global.SafeConvertToInt32(fields[2]);
                if (fields.Length >= 4)
                    param = fields[3];

                var dbRoleInfo = dbMgr.GetDBRoleInfo(roleID);
                if (null == dbRoleInfo)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Source player is not exist, CMD={0}, RoleID={1}",
                        (TCPGameServerCmds)nID, roleID));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                var strcmd = "";
                var userID = dbRoleInfo.UserID;

                var dbUserInfo = dbMgr.GetDBUserInfo(userID);
                if (null == dbUserInfo)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("查找用户数据失败，CMD={0}, UserID={1}",
                        (TCPGameServerCmds)nID, userID));

                    strcmd = string.Format("{0}:{1}", roleID, -2);

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                var failed = false;
                var userMoney = 0;
                lock (dbUserInfo)
                {
                    if (addOrSubUserMoney < 0 && dbUserInfo.Money < Math.Abs(addOrSubUserMoney))
                    {
                        failed = true;
                    }
                    else
                    {
                        dbUserInfo.Money = Math.Max(0, dbUserInfo.Money + addOrSubUserMoney);
                        userMoney = dbUserInfo.Money;
                    }
                }

                if (failed)
                {
                    strcmd = string.Format("{0}:{1}", roleID, -3);
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                if (addOrSubUserMoney != 0)
                {
                    var ret = DBWriter.UpdateUserMoney(dbMgr, userID, userMoney);
                    if (!ret)
                    {
                        LogManager.WriteLog(LogTypes.Error, string.Format("更新用户的元宝失败，CMD={0}, UserID={1}",
                            (TCPGameServerCmds)nID, userID));

                        strcmd = string.Format("{0}:{1}", roleID, -4);

                        tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                        return TCPProcessCmdResults.RESULT_DATA;
                    }
                }

                var nUserMoney = 0;
                var nRealMoney = 0;

                DBQuery.QueryUserMoneyByUserID(dbMgr, dbUserInfo.UserID, out nUserMoney, out nRealMoney);

                strcmd = string.Format("{0}:{1}:{2}", roleID, userMoney, nRealMoney);
                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, "", false);
            }

            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
            return TCPProcessCmdResults.RESULT_DATA;
        }

        /// <summary>
        ///     Cập nhật bạc khóa
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private static TCPProcessCmdResults ProcessUpdateUserYinLiangCmd(DBManager dbMgr, TCPOutPacketPool pool,
            int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            /*
             *  注意：如果协议修改了，一定不要忘记确认下是否需要修改ProcessRoleHuobiOffline这个函数
             *  chenjg 20150422
             */
            tcpOutPacket = null;
            string cmdData = null;

            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Wrong socket data CMD={0}", (TCPGameServerCmds)nID));

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                return TCPProcessCmdResults.RESULT_DATA;
            }

            try
            {
                var fields = cmdData.Split(':');
                if (fields.Length != 2)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format(
                        "Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}",
                        (TCPGameServerCmds)nID, fields.Length, cmdData));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                var roleID = Convert.ToInt32(fields[0]);
                var addOrSubUserYinLiang = Convert.ToInt32(fields[1]);

                var strcmd = "";
                var dbRoleInfo = dbMgr.GetDBRoleInfo(roleID);
                if (null == dbRoleInfo)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Source player is not exist, CMD={0}, RoleID={1}",
                        (TCPGameServerCmds)nID, roleID));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                var failed = false;
                var userYinLiang = 0;
                lock (dbRoleInfo.GetMoneyLock)
                {
                    //判断如果是扣除操作，则可能是银两余额不足而失败
                    if (addOrSubUserYinLiang < 0 && dbRoleInfo.YinLiang < Math.Abs(addOrSubUserYinLiang))
                    {
                        failed = true;
                    }
                    else //处理元宝的加减
                    {
                        dbRoleInfo.YinLiang = Math.Max(0, dbRoleInfo.YinLiang + addOrSubUserYinLiang);
                        userYinLiang = dbRoleInfo.YinLiang;
                    }
                }

                //如果扣除失败
                if (failed)
                {
                    //添加任务失败
                    strcmd = string.Format("{0}:{1}", roleID, -1);
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                //不等于0，才更新数据库
                if (addOrSubUserYinLiang != 0)
                {
                    //将用户的请求发起写数据库的操作
                    var ret = DBWriter.UpdateRoleYinLiang(dbMgr, roleID, userYinLiang);
                    if (!ret)
                    {
                        LogManager.WriteLog(LogTypes.Error, string.Format("更新角色银两失败，CMD={0}, RoleID={1}",
                            (TCPGameServerCmds)nID, roleID));

                        //添加任务失败
                        strcmd = string.Format("{0}:{1}", roleID, -2);

                        tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                        return TCPProcessCmdResults.RESULT_DATA;
                    }
                }

                strcmd = string.Format("{0}:{1}", roleID, userYinLiang);
                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                //System.Windows.Application.Current.Dispatcher.Invoke((MethodInvoker)delegate
                //{
                //
                DataHelper.WriteFormatExceptionLog(ex, "", false);
                //throw ex;
                //});
            }

            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
            return TCPProcessCmdResults.RESULT_DATA;
        }

        /// <summary>
        ///     Hàm cập nhật lại tiền tệ bang hội
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <param name="tcpOutPacket"></param>
        /// <returns></returns>
        private static TCPProcessCmdResults ProseccUpdateGuildMoney(DBManager dbMgr, TCPOutPacketPool pool, int nID,
            byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string cmdData = null;

            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Wrong socket data CMD={0}", (TCPGameServerCmds)nID));

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                return TCPProcessCmdResults.RESULT_DATA;
            }

            try
            {
                var fields = cmdData.Split(':');
                if (fields.Length != 2)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format(
                        "Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}",
                        (TCPGameServerCmds)nID, fields.Length, cmdData));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                var roleID = Convert.ToInt32(fields[0]);

                var AddOrSubGuildMoney = Convert.ToInt32(fields[1]);

                // Lấy ra thông tin nhân vật
                // Nếu đéo lấy được thì thông báo lỗi về
                var strcmd = "";
                var dbRoleInfo = dbMgr.GetDBRoleInfo(roleID);
                if (null == dbRoleInfo)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Source player is not exist, CMD={0}, RoleID={1}",
                        (TCPGameServerCmds)nID, roleID));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                var failed = false;
                var RoleGuildMoney = 0;
                lock (dbRoleInfo.GetMoneyLock)
                {
                    // Check xem có đủ tiền để trừ không
                    if (AddOrSubGuildMoney < 0 && dbRoleInfo.RoleGuildMoney < Math.Abs(AddOrSubGuildMoney))
                        failed = true;
                }

                // Nếu ko có bang thì cũng cho toạch luôn
                if (dbRoleInfo.GuildID <= 0) failed = true;
                // Nếu mà toạch thì báo luôn là toạch

                if (failed)
                {
                    strcmd = string.Format("{0}:{1}", roleID, -1);
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                if (AddOrSubGuildMoney != 0)
                {
                    // Thực hiện ghi vào gameDB trước
                    var ret = GuildManager.getInstance().UpdateRoleGuildMoneyFromTCP(AddOrSubGuildMoney, dbRoleInfo.GuildID, dbRoleInfo.RoleID);
                    if (!ret)
                    {
                        LogManager.WriteLog(LogTypes.Error, string.Format(
                            "Không cập nhật được tiền bang hội lỗi nghiêm trọng，CMD={0}, RoleID={1}",
                            (TCPGameServerCmds)nID, roleID));

                        strcmd = string.Format("{0}:{1}", roleID, -2);

                        tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                        return TCPProcessCmdResults.RESULT_DATA;
                    }

                    //Thực hiện update lại thực thể tiền bang hội trên người
                    dbRoleInfo.RoleGuildMoney = Math.Max(0, dbRoleInfo.RoleGuildMoney + AddOrSubGuildMoney);
                    RoleGuildMoney = dbRoleInfo.RoleGuildMoney;
                }

                strcmd = string.Format("{0}:{1}", roleID, RoleGuildMoney);
                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, "", false);
            }

            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
            return TCPProcessCmdResults.RESULT_DATA;
        }

        /// <summary>
        ///     Di chuyển vật phẩm
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private static TCPProcessCmdResults ProcessMoveGoodsCmd(DBManager dbMgr, TCPOutPacketPool pool, int nID,
            byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string cmdData = null;

            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Wrong socket data CMD={0}", (TCPGameServerCmds)nID));

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                return TCPProcessCmdResults.RESULT_DATA;
            }

            try
            {
                var fields = cmdData.Split(':');
                if (fields.Length != 4)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format(
                        "Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}",
                        (TCPGameServerCmds)nID, fields.Length, cmdData));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                // RoleiD của thằng nhận
                var roleID = Convert.ToInt32(fields[0]);
                // Chủ nhân vật phẩm
                var goodsOwnerRoleID = Convert.ToInt32(fields[1]);
                // DBID của vật phẩm
                var goodsDbID = Convert.ToInt32(fields[2]);
                // Kiểu di chuyển | Nhặt hay là chuyển thông thường
                var Type = Convert.ToInt32(fields[3]);

                var strcmd = "";

                if (Type == 0)
                {
                    // Lấy ra roleinfo của thằng nhận
                    var dbRoleInfo = dbMgr.GetDBRoleInfo(roleID);
                    if (null == dbRoleInfo)
                    {
                        LogManager.WriteLog(LogTypes.Error, string.Format("Toác khi chuyển vật phẩm，CMD={0}, RoleID={1}",
                            (TCPGameServerCmds)nID, roleID));

                        strcmd = string.Format("{0}:{1}:{2}:{3}", roleID, goodsOwnerRoleID, goodsDbID, -1);

                        tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                        return TCPProcessCmdResults.RESULT_DATA;
                    }

                    // lấy ra thông tin chủ nhân vật phẩm
                    var dbRoleInfo2 = dbMgr.GetDBRoleInfo(goodsOwnerRoleID);
                    if (null == dbRoleInfo2)
                    {
                        LogManager.WriteLog(LogTypes.Error, string.Format("Toác không tìm thấy chủ nhân vật phẩm，CMD={0}, RoleID={1}",
                            (TCPGameServerCmds)nID, goodsOwnerRoleID));

                        strcmd = string.Format("{0}:{1}:{2}:{3}", roleID, goodsOwnerRoleID, goodsDbID, -2);

                        tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                        return TCPProcessCmdResults.RESULT_DATA;
                    }

                    // nếu đéo tìm thấy vật phẩm
                    if (null == Global.GetGoodsDataByDbID(dbRoleInfo2, goodsDbID))
                    {
                        strcmd = string.Format("{0}:{1}:{2}:{3}", roleID, goodsOwnerRoleID, goodsDbID, -1000);
                        tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                        return TCPProcessCmdResults.RESULT_DATA;
                    }

                    // Di chuyển vật phẩm sql script
                    var ret = DBWriter.MoveGoods(dbMgr, roleID, goodsDbID);
                    if (ret < 0)
                    {
                        LogManager.WriteLog(LogTypes.Error, string.Format("Bug khi update mysql，CMD={0}, RoleID={1}",
                            (TCPGameServerCmds)nID, roleID));

                        strcmd = string.Format("{0}:{1}:{2}:{3}", roleID, goodsOwnerRoleID, goodsDbID, ret);

                        tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                        return TCPProcessCmdResults.RESULT_DATA;
                    }

                    /// Vật phẩm tương ứng
                    var gd = Global.GetGoodsDataByDbID(dbRoleInfo2, goodsDbID);
                    /// Toác
                    if (null == gd)
                    {
                        strcmd = string.Format("{0}:{1}:{2}:{3}", roleID, goodsOwnerRoleID, goodsDbID, -1000);
                        tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                        return TCPProcessCmdResults.RESULT_DATA;
                    }

                    /// Xóa khỏi danh sách
                    dbRoleInfo2.GoodsDataList.TryRemove(goodsDbID, out _);
                    gd.Site = 0;

                    if (null == dbRoleInfo.GoodsDataList)
                        dbRoleInfo.GoodsDataList = new ConcurrentDictionary<int, GoodsData>();

                    // Add vật phẩm này cho thằng mới
                    dbRoleInfo.GoodsDataList[goodsDbID] = gd;

                    strcmd = string.Format("{0}:{1}:{2}:{3}", roleID, goodsOwnerRoleID, goodsDbID, 0);
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                    return TCPProcessCmdResults.RESULT_DATA;
                }
                else if (Type == 1) // Đây là thằng A nhặt vật phẩm của thằng B
                {
                    // lấy info của thằng nhận
                    var dbRoleInfo = dbMgr.GetDBRoleInfo(roleID);
                    if (null == dbRoleInfo)
                    {
                        LogManager.WriteLog(LogTypes.Error, string.Format("Toác khi chuyển vật phẩm，CMD={0}, RoleID={1}",
                            (TCPGameServerCmds)nID, roleID));

                        strcmd = string.Format("{0}:{1}:{2}:{3}", roleID, goodsOwnerRoleID, goodsDbID, -1);

                        tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                        return TCPProcessCmdResults.RESULT_DATA;
                    }

                    // lấy ra thông tin chủ nhân vật phẩm
                    var dbRoleInfo2 = dbMgr.GetDBRoleInfo(goodsOwnerRoleID);
                    if (null == dbRoleInfo2)
                    {
                        LogManager.WriteLog(LogTypes.Error, string.Format("Toác không tìm thấy chủ nhân vật phẩm，CMD={0}, RoleID={1}",
                            (TCPGameServerCmds)nID, goodsOwnerRoleID));

                        strcmd = string.Format("{0}:{1}:{2}:{3}", roleID, goodsOwnerRoleID, goodsDbID, -2);

                        tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                        return TCPProcessCmdResults.RESULT_DATA;
                    }

                    // Tìm vật phẩm mà thằng này đã vứt trước đó
                    if (null == Global.GetDropItemDataDbID(dbRoleInfo2, goodsDbID))
                    {
                        strcmd = string.Format("{0}:{1}:{2}:{3}", roleID, goodsOwnerRoleID, goodsDbID, -1000);
                        tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                        return TCPProcessCmdResults.RESULT_DATA;
                    }

                    // Di chuyển vật phẩm sql script
                    var ret = DBWriter.MoveGoodsByDropTake(dbMgr, roleID, goodsDbID);
                    if (ret < 0)
                    {
                        LogManager.WriteLog(LogTypes.Error, string.Format("Bug khi update mysql，CMD={0}, RoleID={1}",
                            (TCPGameServerCmds)nID, roleID));

                        strcmd = string.Format("{0}:{1}:{2}:{3}", roleID, goodsOwnerRoleID, goodsDbID, ret);

                        tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                        return TCPProcessCmdResults.RESULT_DATA;
                    }

                    /// Vật phẩm tương ứng
                    var gd = Global.GetGoodDropByDBID(dbRoleInfo2, goodsDbID);
                    /// Toác
                    if (null == gd)
                    {
                        strcmd = string.Format("{0}:{1}:{2}:{3}", roleID, goodsOwnerRoleID, goodsDbID, -1000);
                        tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                        return TCPProcessCmdResults.RESULT_DATA;
                    }

                    /// Xóa khỏi danh sách
                    dbRoleInfo2.DropDataList?.TryRemove(goodsDbID, out _);
                    gd.Site = 0;

                    if (null == dbRoleInfo.GoodsDataList)
                        dbRoleInfo.GoodsDataList = new ConcurrentDictionary<int, GoodsData>();

                    // Add vật phẩm này cho thằng mới
                    dbRoleInfo.GoodsDataList[goodsDbID] = gd;

                    strcmd = string.Format("{0}:{1}:{2}:{3}", roleID, goodsOwnerRoleID, goodsDbID, 0);
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                    return TCPProcessCmdResults.RESULT_DATA;
                }
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, "", false);
            }

            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
            return TCPProcessCmdResults.RESULT_DATA;
        }

        /// <summary>
        ///     Trả về tên và trạng thái online offline theo ID nhân vật
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private static TCPProcessCmdResults ProcessQueryNameByIDCmd(DBManager dbMgr, TCPOutPacketPool pool, int nID,
            byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string cmdData = null;

            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Wrong socket data CMD={0}", (TCPGameServerCmds)nID));

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                return TCPProcessCmdResults.RESULT_DATA;
            }

            try
            {
                var fields = cmdData.Split(':');
                if (fields.Length != 3)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format(
                        "Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}",
                        (TCPGameServerCmds)nID, fields.Length, cmdData));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                var roleID = Convert.ToInt32(fields[0]);
                var otherName = fields[1];
                var opCode = Convert.ToInt32(fields[2]);

                var myServerLineID = -1;

                if (roleID > 0)
                {
                    var dbRoleInfo = dbMgr.GetDBRoleInfo(roleID);
                    if (null == dbRoleInfo)
                    {
                        LogManager.WriteLog(LogTypes.Error, string.Format(
                            "Source player is not exist, CMD={0}, RoleID={1}",
                            (TCPGameServerCmds)nID, roleID));

                        tcpOutPacket =
                            TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                        return TCPProcessCmdResults.RESULT_DATA;
                    }

                    myServerLineID = dbRoleInfo.ServerLineID;
                }

                var strcmd = "";

                var onlineState = -1;
                var otherRoleID = dbMgr.DBRoleMgr.FindDBRoleID(otherName); //角色不存在
                if (otherRoleID == -1)
                {
                    var otherDdbRoleInfo = dbMgr.GetDBRoleInfo(otherName);
                    if (otherDdbRoleInfo != null) otherRoleID = otherDdbRoleInfo.RoleID;
                }

                if (-1 != otherRoleID)
                {
                    var otherDbRoleInfo = dbMgr.GetDBRoleInfo(otherRoleID);
                    if (null != otherDbRoleInfo)
                    {
                        var roleOnlineState = Global.GetRoleOnlineState(otherDbRoleInfo);
                        if (1 == roleOnlineState)
                        {
                            onlineState = 0;

                            if (otherDbRoleInfo.ServerLineID != myServerLineID)
                                onlineState = otherDbRoleInfo.ServerLineID;
                        }
                    }
                }

                strcmd = string.Format("{0}:{1}:{2}:{3}:{4}", roleID, otherName, opCode, otherRoleID, onlineState);
                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                //System.Windows.Application.Current.Dispatcher.Invoke((MethodInvoker)delegate
                //{
                //
                DataHelper.WriteFormatExceptionLog(ex, "", false);
                //throw ex;
                //});
            }

            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
            return TCPProcessCmdResults.RESULT_DATA;
        }

        /// <summary>
        ///     Thực hiện gửi chát về liên máy chủ
        /// </summary>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private static TCPProcessCmdResults ProcessSpriteChatCmd(DBManager dbMgr, TCPOutPacketPool pool, int nID,
            byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string cmdData = null;

            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Wrong socket data CMD={0}", (TCPGameServerCmds)nID));

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                return TCPProcessCmdResults.RESULT_DATA;
            }

            try
            {
                var fields = cmdData.Split(':');
                if (fields.Length != 9)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format(
                        "Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}",
                        (TCPGameServerCmds)nID, fields.Length, cmdData));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                var serverLineID = Convert.ToInt32(fields[8]);
                var itemList = LineManager.GetLineItemList();
                if (null != itemList)
                    for (var i = 0; i < itemList.Count; i++)
                    {
                        if (itemList[i].LineID == serverLineID) continue;

                        if (serverLineID == GameDBManager.ServerLineIdAllIncludeKuaFu)
                        {
                            ChatMsgManager.AddChatMsg(itemList[i].LineID, cmdData);
                        }
                        else if (serverLineID == GameDBManager.ServerLineIdAllLineExcludeSelf)
                        {
                            if (null != TCPManager.CurrentClient &&
                                TCPManager.CurrentClient.LineId != itemList[i].LineID)
                                ChatMsgManager.AddChatMsg(itemList[i].LineID, cmdData);
                        }
                        else if (itemList[i].LineID < GameDBManager.KuaFuServerIdStartValue)
                        {
                            ChatMsgManager.AddChatMsg(itemList[i].LineID, cmdData);
                        }
                    }

                var strcmd = string.Format("{0}", 0);
                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                //System.Windows.Application.Current.Dispatcher.Invoke((MethodInvoker)delegate
                //{
                //
                DataHelper.WriteFormatExceptionLog(ex, "", false);
                //throw ex;
                //});
            }

            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
            return TCPProcessCmdResults.RESULT_DATA;
        }

        /// <summary>
        /// Lấy ra danh sách chát từ máy chủ
        /// Phục vụ cho chát liên máy chủ
        /// </summary>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private static TCPProcessCmdResults ProcessGetChatMsgListCmd(GameServerClient client, DBManager dbMgr,
            TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string cmdData = null;

            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Wrong socket data CMD={0}", (TCPGameServerCmds)nID));

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                return TCPProcessCmdResults.RESULT_DATA;
            }

            try
            {
                var fields = cmdData.Split(':');
                if (fields.Length != 4)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format(
                        "Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}",
                        (TCPGameServerCmds)nID, fields.Length, cmdData));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                var serverLineID = Convert.ToInt32(fields[0]);
                var serverLineNum = Convert.ToInt32(fields[1]);
                var serverLineCount = Convert.ToInt32(fields[2]);

                if (serverLineCount <= 0) //刚上线的服务器
                                          //清空指定线路ID对应的所有用户数据
                    UserOnlineManager.ClearUserIDsByServerLineID(serverLineID);

                //更新服务器状态
                LineManager.UpdateLineHeart(client, serverLineID, serverLineNum, fields[3]);

                tcpOutPacket = ChatMsgManager.GetWaitingChatMsg(pool, nID, serverLineID);
                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                //System.Windows.Application.Current.Dispatcher.Invoke((MethodInvoker)delegate
                //{
                //
                DataHelper.WriteFormatExceptionLog(ex, "", false);
                //throw ex;
                //});
            }

            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
            return TCPProcessCmdResults.RESULT_DATA;
        }

        /// <summary>
        ///  Lấy ra danh sách vật phẩm theo ngăn
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private static TCPProcessCmdResults ProcessGetGoodsListBySiteCmd(DBManager dbMgr, TCPOutPacketPool pool,
            int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string cmdData = null;

            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Wrong socket data CMD={0}", (TCPGameServerCmds)nID));

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                return TCPProcessCmdResults.RESULT_DATA;
            }

            try
            {
                var fields = cmdData.Split(':');
                if (fields.Length != 2)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format(
                        "Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}",
                        (TCPGameServerCmds)nID, fields.Length, cmdData));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                var roleID = Convert.ToInt32(fields[0]);
                var site = Convert.ToInt32(fields[1]);

                var dbRoleInfo = dbMgr.GetDBRoleInfo(roleID);
                if (null == dbRoleInfo)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Source player is not exist, CMD={0}, RoleID={1}",
                        (TCPGameServerCmds)nID, roleID));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                List<GoodsData> goodsDataList = null;

                //将用户的请求更新内存缓存
                lock (dbRoleInfo)
                {
                    goodsDataList = Global.GetGoodsDataListBySite(dbRoleInfo, site);
                }

                /// 将对象转为TCP协议流
                tcpOutPacket = DataHelper.ObjectToTCPOutPacket(goodsDataList, pool, nID);
                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                //System.Windows.Application.Current.Dispatcher.Invoke((MethodInvoker)delegate
                //{
                //
                DataHelper.WriteFormatExceptionLog(ex, "", false);
                //throw ex;
                //});
            }

            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
            return TCPProcessCmdResults.RESULT_DATA;
        }

        /// <summary>
        ///     角色更新在线时长相关的字段
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private static TCPProcessCmdResults ProcessUpdateOnlineTimeCmd(DBManager dbMgr, TCPOutPacketPool pool, int nID,
            byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string cmdData = null;

            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Wrong socket data CMD={0}", (TCPGameServerCmds)nID));

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                return TCPProcessCmdResults.RESULT_DATA;
            }

            try
            {
                var fields = cmdData.Split(':');
                if (fields.Length != 3)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format(
                        "Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}",
                        (TCPGameServerCmds)nID, fields.Length, cmdData));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                var roleID = Convert.ToInt32(fields[0]);
                var totalOnlineSecs = Convert.ToInt32(fields[1]);
                var antiAddictionSecs = Convert.ToInt32(fields[2]);

                var dbRoleInfo = dbMgr.GetDBRoleInfo(roleID);
                if (null == dbRoleInfo)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Source player is not exist, CMD={0}, RoleID={1}",
                        (TCPGameServerCmds)nID, roleID));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                //long ticks = DateTime.Now.Ticks / 10000;
                var updateDBTime = true;
                dbRoleInfo.TotalOnlineSecs = totalOnlineSecs;
                dbRoleInfo.AntiAddictionSecs = antiAddictionSecs;

                if (updateDBTime) DBWriter.UpdateRoleOnlineSecs(dbMgr, roleID, totalOnlineSecs, antiAddictionSecs);

                var strcmd = string.Format("{0}", 0);
                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                //System.Windows.Application.Current.Dispatcher.Invoke((MethodInvoker)delegate
                //{
                //
                DataHelper.WriteFormatExceptionLog(ex, "", false);
                //throw ex;
                //});
            }

            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
            return TCPProcessCmdResults.RESULT_DATA;
        }

        /// <summary>
        ///     Lấy ra config từ gamedb
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private static TCPProcessCmdResults ProcessGameConfigDictCmd(DBManager dbMgr, TCPOutPacketPool pool, int nID,
            byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string cmdData = null;

            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Wrong socket data CMD={0}", (TCPGameServerCmds)nID));

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                return TCPProcessCmdResults.RESULT_DATA;
            }

            try
            {
                var fields = cmdData.Split(':');
                if (fields.Length != 1)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format(
                        "Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}",
                        (TCPGameServerCmds)nID, fields.Length, cmdData));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                var serverLineID = Convert.ToInt32(fields[0]);

                tcpOutPacket = GameDBManager.GameConfigMgr.GetGameConfigDictTCPOutPacket(pool, nID);
                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                //System.Windows.Application.Current.Dispatcher.Invoke((MethodInvoker)delegate
                //{
                //
                DataHelper.WriteFormatExceptionLog(ex, "", false);
                //throw ex;
                //});
            }

            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
            return TCPProcessCmdResults.RESULT_DATA;
        }

        /// <summary>
        ///     Thay đổi config vật phẩm từ gs
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private static TCPProcessCmdResults ProcessGameConfigItemCmd(DBManager dbMgr, TCPOutPacketPool pool, int nID,
            byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string cmdData = null;

            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Wrong socket data CMD={0}", (TCPGameServerCmds)nID));

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                return TCPProcessCmdResults.RESULT_DATA;
            }

            try
            {
                var fields = cmdData.Split(':');
                if (fields.Length != 2)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format(
                        "Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}",
                        (TCPGameServerCmds)nID, fields.Length, cmdData));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                var paramName = fields[0];
                var paramValue = fields[1];

                //设置游戏参数
                GameDBManager.GameConfigMgr.UpdateGameConfigItem(paramName, paramValue);

                //更新设置游戏参数
                DBWriter.UpdateGameConfig(dbMgr, paramName, paramValue);

                //添加GM命令消息
                var gmCmdData = string.Format("-config {0} {1}", paramName, paramValue);
                ChatMsgManager.AddGMCmdChatMsg(-1, gmCmdData);

                var strcmd = string.Format("{0}", 0);
                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                //System.Windows.Application.Current.Dispatcher.Invoke((MethodInvoker)delegate
                //{
                //
                DataHelper.WriteFormatExceptionLog(ex, "", false);
                //throw ex;
                //});
            }

            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
            return TCPProcessCmdResults.RESULT_DATA;
        }

        /// <summary>
        ///     Thực hiện thêm kỹ năng vào DB
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private static TCPProcessCmdResults ProcessAddSkillCmd(DBManager dbMgr, TCPOutPacketPool pool, int nID,
            byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string cmdData = null;

            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Wrong socket data CMD={0}", (TCPGameServerCmds)nID));

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                return TCPProcessCmdResults.RESULT_DATA;
            }

            try
            {
                var fields = cmdData.Split(':');
                if (fields.Length != 2)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format(
                        "Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}",
                        (TCPGameServerCmds)nID, fields.Length, cmdData));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                var roleID = Convert.ToInt32(fields[0]);
                var skillID = Convert.ToInt32(fields[1]);

                var dbRoleInfo = dbMgr.GetDBRoleInfo(roleID);
                if (null == dbRoleInfo)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Source player is not exist, CMD={0}, RoleID={1}",
                        (TCPGameServerCmds)nID, roleID));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                var ret = DBWriter.AddSkill(dbMgr, roleID, skillID);
                if (ret > 0)
                {
                    if (null == dbRoleInfo.SkillDataList)
                        dbRoleInfo.SkillDataList = new ConcurrentDictionary<int, SkillData>();

                    var skillData = new SkillData
                    {
                        DbID = ret,
                        SkillID = skillID,
                        Cooldown = 0,
                        Exp = 0,
                        LastUsedTick = 0,
                        SkillLevel = 0
                    };
                    dbRoleInfo.SkillDataList[skillData.SkillID] = skillData;
                }

                var strcmd = string.Format("{0}", ret);
                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                //System.Windows.Application.Current.Dispatcher.Invoke((MethodInvoker)delegate
                //{
                //
                DataHelper.WriteFormatExceptionLog(ex, "", false);
                //throw ex;
                //});
            }

            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
            return TCPProcessCmdResults.RESULT_DATA;
        }

        /// <summary>
        ///     Thực hiện xóa kỹ năng khỏi DB
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private static TCPProcessCmdResults ProcessDelSkillCmd(DBManager dbMgr, TCPOutPacketPool pool, int nID,
            byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string cmdData = null;

            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Wrong socket data CMD={0}", (TCPGameServerCmds)nID));

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                return TCPProcessCmdResults.RESULT_DATA;
            }

            try
            {
                var fields = cmdData.Split(':');
                if (fields.Length != 2)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format(
                        "Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}",
                        (TCPGameServerCmds)nID, fields.Length, cmdData));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                var roleID = Convert.ToInt32(fields[0]);
                var skillID = Convert.ToInt32(fields[1]);

                var dbRoleInfo = dbMgr.GetDBRoleInfo(roleID);
                if (null == dbRoleInfo)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Source player is not exist, CMD={0}, RoleID={1}",
                        (TCPGameServerCmds)nID, roleID));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                var ret = DBWriter.DeleteSkill(dbMgr, roleID, skillID);
                if (ret)
                {
                    if (null == dbRoleInfo.SkillDataList)
                        dbRoleInfo.SkillDataList = new ConcurrentDictionary<int, SkillData>();

                    if (dbRoleInfo.SkillDataList.TryGetValue(skillID, out var skillData))
                        dbRoleInfo.SkillDataList.TryRemove(skillID, out _);
                }

                var strcmd = string.Format("{0}", ret ? 1 : 0);
                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, "", false);
            }

            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
            return TCPProcessCmdResults.RESULT_DATA;
        }

        /// <summary>
        ///     Thực hiện thay đổi thông tin kỹ năng
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private static TCPProcessCmdResults ProcessUpSkillInfoCmd(DBManager dbMgr, TCPOutPacketPool pool, int nID,
            byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string cmdData = null;

            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Wrong socket data CMD={0}", (TCPGameServerCmds)nID));

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                return TCPProcessCmdResults.RESULT_DATA;
            }

            try
            {
                var fields = cmdData.Split(':');
                if (fields.Length != 6)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format(
                        "Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}",
                        (TCPGameServerCmds)nID, fields.Length, cmdData));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                var roleID = Convert.ToInt32(fields[0]);
                var skillID = Convert.ToInt32(fields[1]);
                var skillLevel = Convert.ToInt32(fields[2]);
                var lastUsedTick = Convert.ToInt64(fields[3]);
                var cooldownTick = Convert.ToInt32(fields[4]);
                var exp = Convert.ToInt32(fields[5]);

                var dbRoleInfo = dbMgr.GetDBRoleInfo(roleID);
                if (null == dbRoleInfo)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Source player is not exist, CMD={0}, RoleID={1}",
                        (TCPGameServerCmds)nID, roleID));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                if (null != dbRoleInfo.SkillDataList)
                    if (dbRoleInfo.SkillDataList.TryGetValue(skillID, out var skill))
                    {
                        skill.SkillLevel = skillLevel;
                        skill.LastUsedTick = lastUsedTick;
                        skill.Cooldown = cooldownTick;
                        skill.Exp = exp;
                    }

                var ret = DBWriter.UpdateSkillInfo(dbMgr, roleID, skillID, skillLevel, lastUsedTick, cooldownTick, exp);

                var strcmd = string.Format("{0}", ret ? 1 : 0);
                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, "", false);
            }

            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
            return TCPProcessCmdResults.RESULT_DATA;
        }

        /// <summary>
        /// Cập nhật thông tin Buff
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private static TCPProcessCmdResults ProcessUpdateBufferItemCmd(DBManager dbMgr, TCPOutPacketPool pool, int nID,
            byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string cmdData = null;

            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Wrong socket data CMD={0}", (TCPGameServerCmds)nID));

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                return TCPProcessCmdResults.RESULT_DATA;
            }

            try
            {
                var fields = cmdData.Split(':');
                if (fields.Length < 3)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}", (TCPGameServerCmds)nID, fields.Length, cmdData));
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                /// ID nhân vật
                int roleID = int.Parse(fields[0]);
                /// ID buff
                int bufferID = int.Parse(fields[1]);
                /// Loại thao tác (1: Thêm, -1: Xóa)
                int cmdType = int.Parse(fields[2]);

                var strcmd = "";
                var dbRoleInfo = dbMgr.GetDBRoleInfo(roleID);
                if (null == dbRoleInfo)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Source player is not exist, CMD={0}, RoleID={1}", (TCPGameServerCmds)nID, roleID));
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                /// Thao tác gì
                switch (cmdType)
                {
                    /// Cập nhật
                    case 1:
                        {
                            if (fields.Length != 7)
                            {
                                LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}", (TCPGameServerCmds)nID, fields.Length, cmdData));
                                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                                return TCPProcessCmdResults.RESULT_DATA;
                            }

                            long startTime = long.Parse(fields[3]);
                            long bufferSecs = long.Parse(fields[4]);
                            long bufferVal = long.Parse(fields[5]);
                            string customProperty = fields[6];

                            int ret = DBWriter.UpdateRoleBufferItem(dbMgr, roleID, bufferID, startTime, bufferSecs, bufferVal, customProperty);
                            if (ret < 0)
                            {
                                strcmd = string.Format("{0}:{1}", roleID, ret);
                                LogManager.WriteLog(LogTypes.Error, string.Format("Update buff data failed, CMD={0}, RoleID={1}", (TCPGameServerCmds)nID, roleID));
                            }
                            else
                            {
                                /// Danh sách Buff chưa tồn tại thì tạo mới
                                if (null == dbRoleInfo.BufferDataList)
                                {
                                    dbRoleInfo.BufferDataList = new ConcurrentDictionary<int, BufferData>();
                                }

                                /// Nếu tồn tại
                                if (dbRoleInfo.BufferDataList.TryGetValue(bufferID, out var buff))
                                {
                                    buff.BufferID = bufferID;
                                    buff.StartTime = startTime;
                                    buff.BufferSecs = bufferSecs;
                                    buff.BufferVal = bufferVal;
                                    buff.CustomProperty = customProperty;
                                }
                                /// Nếu không tồn tại
                                else
                                {
                                    buff = new BufferData
                                    {
                                        BufferID = bufferID,
                                        StartTime = startTime,
                                        BufferSecs = bufferSecs,
                                        BufferVal = bufferVal,
                                        CustomProperty = customProperty
                                    };
                                    dbRoleInfo.BufferDataList[buff.BufferID] = buff;
                                }

                                strcmd = string.Format("{0}:{1}", roleID, ret);
                            }

                            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                            break;
                        }
                    /// Xóa
                    case -1:
                        {
                            int ret = DBWriter.DeleteRoleBufferItem(dbMgr, roleID, bufferID);
                            if (ret < 0)
                            {
                                strcmd = string.Format("{0}:{1}", roleID, ret);
                                LogManager.WriteLog(LogTypes.Error, string.Format("Delete buff data failed, CMD={0}, RoleID={1}", (TCPGameServerCmds)nID, roleID));
                            }
                            else
                            {
                                /// Danh sách Buff chưa tồn tại thì tạo mới
                                if (null == dbRoleInfo.BufferDataList)
                                {
                                    dbRoleInfo.BufferDataList = new ConcurrentDictionary<int, BufferData>();
                                }

                                /// Xóa khỏi danh sách
                                dbRoleInfo.BufferDataList.TryRemove(bufferID, out _);

                                strcmd = string.Format("{0}:{1}", roleID, ret);
                            }

                            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                            break;
                        }
                }

                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, "", false);
            }

            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
            return TCPProcessCmdResults.RESULT_DATA;
        }

        /// <summary>
        /// Cập nhật thông tin liên quan tới phúc lợi của nhân vật
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private static TCPProcessCmdResults ProcessUpdateWelfare(DBManager dbMgr, TCPOutPacketPool pool, int nID,
            byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            RoleWelfare cmdData = null;

            try
            {
                // Giãi mã chuỗi info gửi từ server
                cmdData = DataHelper.BytesToObject<RoleWelfare>(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Wrong socket data CMD={0}", (TCPGameServerCmds)nID));

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                return TCPProcessCmdResults.RESULT_DATA;
            }

            try
            {
                int Ret = -1;

                var dbRoleInfo = dbMgr.GetDBRoleInfo(cmdData.RoleID);

                if (null == dbRoleInfo)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Source player is not exist, CMD={0}, RoleID={1}",
                        (TCPGameServerCmds)nID, cmdData.RoleID));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                // Nếu mà ở DB chưa có thì thực hiện ghi mới vào DB
                if (dbRoleInfo.RoleWelfare.RoleID == -1)
                {
                    LogManager.WriteLog(LogTypes.SQL, "CREATE NEW  CreateHuoDong :" + cmdData.RoleID);

                    Ret = DBWriter.CreateWelfare(dbMgr, cmdData);
                }
                else // Nếu có bản ghi rồi thì thực hiện update vào DB
                {
                    Ret = DBWriter.UpdateWelfare(dbMgr, cmdData);
                }

                if (Ret != -1)
                {
                    // Nếu mà update thành công thì set lại thông tin vào RoleINFO
                    lock (dbRoleInfo)
                    {
                        // Set lại RoleWelfare cho RoleInfo
                        dbRoleInfo.RoleWelfare = cmdData;
                    }
                }

                string strcmd = string.Format("{0}:{1}", cmdData.RoleID, Ret);

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, "", false);
            }

            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
            return TCPProcessCmdResults.RESULT_DATA;
        }

        private static TCPProcessCmdResults ProseccUpdateRanking(DBManager dbMgr, TCPOutPacketPool pool, int nID,
            byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string cmdData = null;

            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Wrong socket data CMD={0}", (TCPGameServerCmds)nID));

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                return TCPProcessCmdResults.RESULT_DATA;
            }

            try
            {
                var fields = cmdData.Split(':');
                if (fields.Length != 11)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format(
                        "Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}",
                        (TCPGameServerCmds)nID, fields.Length, cmdData));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                var roleID = Convert.ToInt32(fields[0]);
                var rname = fields[1];
                var level = Convert.ToInt32(fields[2]);
                var occupation = Convert.ToInt32(fields[3]);
                var sub_id = Convert.ToInt32(fields[4]);
                var monphai = Convert.ToInt32(fields[5]);
                var taiphu = Convert.ToInt64(fields[6]);
                var volam = Convert.ToInt32(fields[7]);
                var liendau = Convert.ToInt32(fields[8]);
                var uydanh = Convert.ToInt32(fields[9]);
                var expire = Convert.ToInt64(fields[10]);

                var dbRoleInfo = dbMgr.GetDBRoleInfo(roleID);
                if (null == dbRoleInfo)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Source player is not exist, CMD={0}, RoleID={1}",
                        (TCPGameServerCmds)nID, roleID));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                var strcmd = "";

                var ret = DBWriter.UpdateRoleRanking(dbMgr, roleID, rname, level, occupation, sub_id, monphai, taiphu,
                    volam, liendau, uydanh, expire);
                if (ret < 0)
                {
                    strcmd = string.Format("{0}:{1}", roleID, ret);

                    LogManager.WriteLog(LogTypes.Error, string.Format("更新角色日常数据值时失败，CMD={0}, RoleID={1}",
                        (TCPGameServerCmds)nID, roleID));
                }

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, "", false);
            }

            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
            return TCPProcessCmdResults.RESULT_DATA;
        }

        /// <summary>
        ///     Lấy danh sách bảng xếp hạng
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private static TCPProcessCmdResults ProcessGetPaiHangListCmd(DBManager dbMgr, TCPOutPacketPool pool, int nID,
            byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string cmdData = null;

            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Wrong socket data CMD={0}", (TCPGameServerCmds)nID));

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                return TCPProcessCmdResults.RESULT_DATA;
            }

            try
            {
                var fields = cmdData.Split(':');
                if (fields.Length != 3)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format(
                        "Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}",
                        (TCPGameServerCmds)nID, fields.Length, cmdData));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                var roleID = Convert.ToInt32(fields[0]);
                var paiHangType = Convert.ToInt32(fields[1]);
                var pagennumber = Convert.ToInt32(fields[2]);

                var dbRoleInfo = dbMgr.GetDBRoleInfo(roleID);
                if (roleID != 0 && null == dbRoleInfo)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Source player is not exist, CMD={0}, RoleID={1}",
                        (TCPGameServerCmds)nID, roleID));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                var _RankMode = (RankMode)paiHangType;

                var RankData = RankingManager.getInstance().GetRank(_RankMode, roleID, pagennumber);

                var _Rank = new Ranking();
                _Rank.Players = RankData;
                _Rank.Type = paiHangType;
                _Rank.TotalPlayers = RankData.Count;
                _Rank.TotalPlayers = RankingManager.getInstance().Count(_RankMode);

                tcpOutPacket = DataHelper.ObjectToTCPOutPacket(_Rank, pool, nID);

                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                //System.Windows.Application.Current.Dispatcher.Invoke((MethodInvoker)delegate
                //{
                //
                DataHelper.WriteFormatExceptionLog(ex, "", false);
                //throw ex;
                //});
            }

            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
            return TCPProcessCmdResults.RESULT_DATA;
        }

        /// <summary>
        ///     Lấy thông tin nhân vật của 1 thằng khác
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private static TCPProcessCmdResults ProcessGetOtherAttrib2DataCmd(DBManager dbMgr, TCPOutPacketPool pool,
            int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string cmdData = null;

            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Wrong socket data CMD={0}", (TCPGameServerCmds)nID));

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                return TCPProcessCmdResults.RESULT_DATA;
            }

            try
            {
                var fields = cmdData.Split(':');
                if (fields.Length != 2)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format(
                        "Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}",
                        (TCPGameServerCmds)nID, fields.Length, cmdData));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                //int roleID = Convert.ToInt32(fields[0]);
                var otherRoleID = Convert.ToInt32(fields[1]);

                var roleDataEx = new RoleDataEx();

                var dbRoleInfo = dbMgr.GetDBAllRoleInfo(otherRoleID);
                if (null == dbRoleInfo)
                    roleDataEx.RoleID = -1;
                else

                    Global.DBRoleInfo2RoleDataEx(dbRoleInfo, roleDataEx);

                tcpOutPacket = DataHelper.ObjectToTCPOutPacket(roleDataEx, pool, nID);

                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                //System.Windows.Application.Current.Dispatcher.Invoke((MethodInvoker)delegate
                //{
                //
                DataHelper.WriteFormatExceptionLog(ex, "", false);
                //throw ex;
                //});
            }

            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
            return TCPProcessCmdResults.RESULT_DATA;
        }

        /// <summary>
        ///     Cập nhật tiến trình nhiệm vụ
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private static TCPProcessCmdResults ProcessUpdateCZTaskIDCmd(DBManager dbMgr, TCPOutPacketPool pool, int nID,
            byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string cmdData = null;

            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Wrong socket data CMD={0}", (TCPGameServerCmds)nID));

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                return TCPProcessCmdResults.RESULT_DATA;
            }

            try
            {
                var fields = cmdData.Split(':');
                if (fields.Length != 2)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format(
                        "Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}",
                        (TCPGameServerCmds)nID, fields.Length, cmdData));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                var roleID = Convert.ToInt32(fields[0]);
                var czTaskID = Convert.ToInt32(fields[1]);

                var dbRoleInfo = dbMgr.GetDBRoleInfo(roleID);
                if (null == dbRoleInfo)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Source player is not exist, CMD={0}, RoleID={1}",
                        (TCPGameServerCmds)nID, roleID));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                var strcmd = "";

                //更新一个用户角色的充值TaskID
                var ret = DBWriter.UpdateRoleCZTaskID(dbMgr, roleID, czTaskID);
                if (!ret)
                {
                    //删除朋友失败
                    strcmd = string.Format("{0}:{1}", roleID, -1);

                    LogManager.WriteLog(LogTypes.Error, string.Format("更新角色充值任务ID时失败，CMD={0}, RoleID={1}",
                        (TCPGameServerCmds)nID, roleID));
                }
                else
                {
                    //将用户的请求更新内存缓存
                    lock (dbRoleInfo)
                    {
                        dbRoleInfo.CZTaskID = czTaskID;
                    }

                    strcmd = string.Format("{0}:{1}", roleID, 0);
                }

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                //System.Windows.Application.Current.Dispatcher.Invoke((MethodInvoker)delegate
                //{
                //
                DataHelper.WriteFormatExceptionLog(ex, "", false);
                //throw ex;
                //});
            }

            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
            return TCPProcessCmdResults.RESULT_DATA;
        }

        /// <summary>
        ///     Lấy ra tổng số session onlien trong gamedb
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private static TCPProcessCmdResults ProcessGetTotalOnlineNumCmd(DBManager dbMgr, TCPOutPacketPool pool, int nID,
            byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string cmdData = null;

            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Wrong socket data CMD={0}", (TCPGameServerCmds)nID));

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                return TCPProcessCmdResults.RESULT_DATA;
            }

            try
            {
                var fields = cmdData.Split(':');
                if (fields.Length != 1)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format(
                        "Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}",
                        (TCPGameServerCmds)nID, fields.Length, cmdData));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                /*int roleID = Convert.ToInt32(fields[0]);

                DBRoleInfo dbRoleInfo = dbMgr.GetDBRoleInfo(roleID);
                if (null == dbRoleInfo)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Source player is not exist, CMD={0}, RoleID={1}",
                        (TCPGameServerCmds)nID, roleID));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }*/

                var totalOnlineNum = LineManager.GetTotalOnlineNum();
                var strcmd = string.Format("{0}", totalOnlineNum);
                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                //System.Windows.Application.Current.Dispatcher.Invoke((MethodInvoker)delegate
                //{
                //
                DataHelper.WriteFormatExceptionLog(ex, "", false);
                //throw ex;
                //});
            }

            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
            return TCPProcessCmdResults.RESULT_DATA;
        }

        /// <summary>
        ///     Tìm ra danh sách nhân vật theo ROLEID
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private static TCPProcessCmdResults ProcessSearchRolesFromDBCmd(DBManager dbMgr, TCPOutPacketPool pool, int nID,
            byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string cmdData = null;

            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Wrong socket data CMD={0}", (TCPGameServerCmds)nID));

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                return TCPProcessCmdResults.RESULT_DATA;
            }

            try
            {
                var fields = cmdData.Split(':');
                if (fields.Length != 3)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format(
                        "Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}",
                        (TCPGameServerCmds)nID, fields.Length, cmdData));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                var roleID = Convert.ToInt32(fields[0]);
                var searchText = fields[1];
                var startIndex = Convert.ToInt32(fields[2]);

                List<SearchRoleData> searchRoleDataList = null;
                var otherID = -1;

                if (searchText.Length > 0)
                {
                    otherID = dbMgr.DBRoleMgr.FindDBRoleID(searchText);
                    if (-1 != otherID)
                    {
                        var dbRoleInfo = dbMgr.GetDBRoleInfo(otherID);
                        if (null != dbRoleInfo)
                        {
                            searchRoleDataList = new List<SearchRoleData>();
                            var searchRoleData = new SearchRoleData
                            {
                                RoleID = dbRoleInfo.RoleID,
                                RoleName = Global.FormatRoleName(dbRoleInfo.ZoneID, dbRoleInfo.RoleName),
                                RoleSex = dbRoleInfo.RoleSex,
                                Level = dbRoleInfo.Level,
                                Occupation = dbRoleInfo.Occupation,
                                Faction = dbRoleInfo.GuildID,
                                BHName = dbRoleInfo.GuildName
                            };

                            searchRoleDataList.Add(searchRoleData);
                        }
                    }
                }

                tcpOutPacket = DataHelper.ObjectToTCPOutPacket(searchRoleDataList, pool, nID);
                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
            }

            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
            return TCPProcessCmdResults.RESULT_DATA;
        }

        /// <summary>
        ///  Lấy thông tin của nhân vật hoàng đế
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private static TCPProcessCmdResults ProcessGetHuangDiRoleDataCmd(DBManager dbMgr, TCPOutPacketPool pool,
            int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string cmdData = null;

            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Wrong socket data CMD={0}", (TCPGameServerCmds)nID));

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                return TCPProcessCmdResults.RESULT_DATA;
            }

            try
            {
                var fields = cmdData.Split(':');
                if (fields.Length != 2)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format(
                        "Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}",
                        (TCPGameServerCmds)nID, fields.Length, cmdData));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                var roleID = Convert.ToInt32(fields[0]);
                var huangDiRoleID = Convert.ToInt32(fields[1]);

                var roleDataEx = new RoleDataEx();

                /// 获取指定的角色信息
                var dbRoleInfo = dbMgr.GetDBRoleInfo(huangDiRoleID);
                if (null == dbRoleInfo)
                    roleDataEx.RoleID = -1;
                else
                    //数据库角色信息到用户数据的转换
                    Global.DBRoleInfo2RoleDataEx(dbRoleInfo, roleDataEx);

                /// 将对象转为TCP协议流
                tcpOutPacket = DataHelper.ObjectToTCPOutPacket(roleDataEx, pool, nID);

                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                //System.Windows.Application.Current.Dispatcher.Invoke((MethodInvoker)delegate
                //{
                //
                DataHelper.WriteFormatExceptionLog(ex, "", false);
                //throw ex;
                //});
            }

            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
            return TCPProcessCmdResults.RESULT_DATA;
        }

        /// <summary>
        ///     Lấy ra thông tin vật phẩm theo ID
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private static TCPProcessCmdResults ProcessGetGoodsByDbIDCmd(DBManager dbMgr, TCPOutPacketPool pool, int nID,
            byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string cmdData = null;

            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Wrong socket data CMD={0}", (TCPGameServerCmds)nID));

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                return TCPProcessCmdResults.RESULT_DATA;
            }

            try
            {
                var fields = cmdData.Split(':');
                if (fields.Length != 2)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format(
                        "Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}",
                        (TCPGameServerCmds)nID, fields.Length, cmdData));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                var roleID = Convert.ToInt32(fields[0]);
                var goodsDbID = Convert.ToInt32(fields[1]);

                GoodsData goodsData = null;
                var dbRoleInfo = dbMgr.GetDBRoleInfo(roleID);
                if (null == dbRoleInfo)
                {
                    tcpOutPacket = DataHelper.ObjectToTCPOutPacket(goodsData, pool, nID);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                goodsData = Global.GetGoodsDataByDbID(dbRoleInfo, goodsDbID);

                //ĐónDdongois và trả về cho client
                tcpOutPacket = DataHelper.ObjectToTCPOutPacket(goodsData, pool, nID);
                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, "", false);
            }

            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
            return TCPProcessCmdResults.RESULT_DATA;
        }

        /// <summary>
        ///  Lấy ra tổng số tiền đã nạp
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private static TCPProcessCmdResults ProcessQueryChongZhiMoneyCmd(DBManager dbMgr, TCPOutPacketPool pool,
            int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string cmdData = null;

            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Wrong socket data CMD={0}", (TCPGameServerCmds)nID));

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                return TCPProcessCmdResults.RESULT_DATA;
            }

            try
            {
                var fields = cmdData.Split(':');
                if (fields.Length != 3)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format(
                        "Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}",
                        (TCPGameServerCmds)nID, fields.Length, cmdData));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                var userID = fields[0];
                var zoneID = Convert.ToInt32(fields[1]);
                var Roleid = Convert.ToInt32(fields[2]);

                var userMoney = 0;
                var realMoney = 0;

                DBQuery.QueryTotalMoneyRechage(dbMgr, userID, Roleid, zoneID, out userMoney, out realMoney);

                var strcmd = string.Format("{0}", realMoney);
                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, "", false);
            }

            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
            return TCPProcessCmdResults.RESULT_DATA;
        }

        /// <summary>
        ///     Lấy ra tổng số tiền nạp trong ngày
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private static TCPProcessCmdResults ProcessQueryDayChongZhiMoneyCmd(DBManager dbMgr, TCPOutPacketPool pool,
            int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string cmdData = null;

            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Wrong socket data CMD={0}", (TCPGameServerCmds)nID));

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                return TCPProcessCmdResults.RESULT_DATA;
            }

            try
            {
                var fields = cmdData.Split(':');
                if (fields.Length != 3)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format(
                        "Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}",
                        (TCPGameServerCmds)nID, fields.Length, cmdData));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                var userID = fields[0];
                var zoneID = Convert.ToInt32(fields[1]);
                var roleID = Convert.ToInt32(fields[2]);

                var userMoney1 = 0;
                var realMoney1 = 0;

                DBQuery.QueryTodayUserMoneyByUserID(dbMgr, userID, roleID, zoneID, out userMoney1, out realMoney1);

                var strcmd = string.Format("{0}", realMoney1);
                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, "", false);
            }

            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
            return TCPProcessCmdResults.RESULT_DATA;
        }

        /// <summary>
        ///     Lấy dữ liệu danh sách thư của người chơi
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private static TCPProcessCmdResults ProcessGetUserMailListCmd(DBManager dbMgr, TCPOutPacketPool pool, int nID,
            byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string cmdData = null;

            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Wrong socket data CMD={0}", (TCPGameServerCmds)nID));

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                return TCPProcessCmdResults.RESULT_DATA;
            }

            try
            {
                var fields = cmdData.Split(':');
                if (fields.Length != 1)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format(
                        "Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}",
                        (TCPGameServerCmds)nID, fields.Length, cmdData));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                var roleID = Convert.ToInt32(fields[0]);

                /// Danh sách thư
                var mailItemDataList = Global.LoadUserMailItemDataList(dbMgr, roleID);

                tcpOutPacket = DataHelper.ObjectToTCPOutPacket(mailItemDataList, pool, nID);
                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, "", false);
            }

            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
            return TCPProcessCmdResults.RESULT_DATA;
        }

        /// <summary>
        ///     Đêm số thư của nhân vật để hiện trên bag
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <param name="tcpOutPacket"></param>
        /// <returns></returns>
        private static TCPProcessCmdResults ProcessGetUserMailCountCmd(DBManager dbMgr, TCPOutPacketPool pool, int nID,
            byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string cmdData = null;

            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Wrong socket data CMD={0}", (TCPGameServerCmds)nID));

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                return TCPProcessCmdResults.RESULT_DATA;
            }

            try
            {
                var fields = cmdData.Split(':');
                if (fields.Length != 3)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format(
                        "Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}",
                        (TCPGameServerCmds)nID, fields.Length, cmdData));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                var roleID = Convert.ToInt32(fields[0]);
                var excludeReadState = Convert.ToInt32(fields[1]);
                var limitCount = Convert.ToInt32(fields[2]);

                //获取邮件列表数据
                var emailCount = Global.LoadUserMailItemDataCount(dbMgr, roleID, excludeReadState, limitCount);

                /// 将对象转为TCP协议流
                tcpOutPacket = DataHelper.ObjectToTCPOutPacket(emailCount, pool, nID);
                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                //System.Windows.Application.Current.Dispatcher.Invoke((MethodInvoker)delegate
                //{
                //
                DataHelper.WriteFormatExceptionLog(ex, "", false);
                //throw ex;
                // });
            }

            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
            return TCPProcessCmdResults.RESULT_DATA;
        }

        /// <summary>
        ///     Lấy dữ liệu thư tương ứng
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private static TCPProcessCmdResults ProcessGetUserMailDataCmd(DBManager dbMgr, TCPOutPacketPool pool, int nID,
            byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string cmdData = null;

            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Wrong socket data CMD={0}", (TCPGameServerCmds)nID));

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                return TCPProcessCmdResults.RESULT_DATA;
            }

            try
            {
                var fields = cmdData.Split(':');
                if (fields.Length != 2)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format(
                        "Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}",
                        (TCPGameServerCmds)nID, fields.Length, cmdData));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                var roleID = Convert.ToInt32(fields[0]);
                var mailID = Convert.ToInt32(fields[1]);

                /// Thông tin thư tương ứng
                var mailItemData = Global.LoadMailItemData(dbMgr, roleID, mailID);

                tcpOutPacket = DataHelper.ObjectToTCPOutPacket(mailItemData, pool, nID);
                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, "", false);
            }

            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
            return TCPProcessCmdResults.RESULT_DATA;
        }

        /// <summary>
        ///     Xử lý gửi thư cho người chơi
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private static TCPProcessCmdResults ProcessSendUserMailCmd(DBManager dbMgr, TCPOutPacketPool pool, int nID,
            byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string cmdData = null;

            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Wrong socket data CMD={0}", (TCPGameServerCmds)nID));

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                return TCPProcessCmdResults.RESULT_DATA;
            }

            try
            {
                var fields = cmdData.Split(':');

                if (fields.Length != 10)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format(
                        "Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}",
                        (TCPGameServerCmds)nID, fields.Length, cmdData));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                /// ID đối tượng gửi thư
                var roleID = Convert.ToInt32(fields[0]);
                var receiverrid = Convert.ToInt32(fields[2]);
                /// Nếu độ dài 10 nghĩa là giá trị cuối quy định phải kiểm tra người nhận tồn tại không

                var addGoodsCount = 0;
                var mailID = Global.AddMail(dbMgr, fields, out addGoodsCount);

                var strcmd = string.Format("{0}:{1}:{2}", roleID, mailID, addGoodsCount);
                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, "", false);
            }

            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
            return TCPProcessCmdResults.RESULT_DATA;
        }

        /// <summary>
        ///  Lấy vật phẩm trong thư
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private static TCPProcessCmdResults ProcessFetchMailGoodsCmd(DBManager dbMgr, TCPOutPacketPool pool, int nID,
            byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string cmdData = null;

            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Wrong socket data CMD={0}", (TCPGameServerCmds)nID));

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                return TCPProcessCmdResults.RESULT_DATA;
            }

            try
            {
                var fields = cmdData.Split(':');
                if (fields.Length != 2)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format(
                        "Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}",
                        (TCPGameServerCmds)nID, fields.Length, cmdData));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                var roleID = Convert.ToInt32(fields[0]);
                var mailID = Convert.ToInt32(fields[1]);

                /// Thực hiện xóa vật phẩm và cập nhật trạng thái cho thư
                var ret = Global.UpdateHasFetchMailGoodsStat(dbMgr, roleID, mailID);

                var strcmd = string.Format("{0}:{1}:{2}", roleID, mailID, ret ? 1 : -1);
                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, "", false);
            }

            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
            return TCPProcessCmdResults.RESULT_DATA;
        }

        /// <summary>
        ///     Xóa thư
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private static TCPProcessCmdResults ProcessDeleteUserMailCmd(DBManager dbMgr, TCPOutPacketPool pool, int nID,
            byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string cmdData = null;

            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Wrong socket data CMD={0}", (TCPGameServerCmds)nID));

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                return TCPProcessCmdResults.RESULT_DATA;
            }

            try
            {
                var fields = cmdData.Split(':');
                if (fields.Length != 2)
                {
                    LogManager.WriteLog(LogTypes.Error,
                        string.Format("Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}",
                            (TCPGameServerCmds)nID, fields.Length, cmdData));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                var roleID = Convert.ToInt32(fields[0]);
                var mailID = Convert.ToInt32(fields[1]);

                /// Thông tin thư tương ứng
                var mailItemData = Global.LoadMailItemData(dbMgr, roleID, mailID);
                /// Nếu thư không tồn tại
                if (mailItemData == null)
                {
                    var strcmd = string.Format("{0}:{1}:{2}", roleID, mailItemData.MailID, -100);
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                /// Nếu chưa lấy vật phẩm đính kèm
                if (mailItemData.HasFetchAttachment == 1)
                {
                    var strcmd = string.Format("{0}:{1}:{2}", roleID, mailItemData.MailID, -101);
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                /// Thực hiện xóa thư
                var ret = Global.DeleteMail(dbMgr, roleID, mailItemData.MailID.ToString());

                var _strcmd = string.Format("{0}:{1}:{2}", roleID, mailItemData.MailID, ret ? 1 : -1);
                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, _strcmd, nID);
                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                //System.Windows.Application.Current.Dispatcher.Invoke((MethodInvoker)delegate
                //{
                //
                DataHelper.WriteFormatExceptionLog(ex, "", false);
                //throw ex;
                //});
            }

            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
            return TCPProcessCmdResults.RESULT_DATA;
        }

        /// <summary>
        ///     Lấy ra số lần sử dụng vật phẩm
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private static TCPProcessCmdResults ProcessDBQueryLimitGoodsUsedNumCmd(DBManager dbMgr, TCPOutPacketPool pool,
            int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string cmdData = null;

            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Wrong socket data CMD={0}", (TCPGameServerCmds)nID));

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                return TCPProcessCmdResults.RESULT_DATA;
            }

            try
            {
                var fields = cmdData.Split(':');
                if (fields.Length != 2)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format(
                        "Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}",
                        (TCPGameServerCmds)nID, fields.Length, cmdData));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                var roleID = Convert.ToInt32(fields[0]);
                var goodsID = Convert.ToInt32(fields[1]);

                var dayID = 0;
                var usedNum = 0;

                var strcmd = "";

                //通过角色ID和物品ID查询物品每日的已经购买数量
                var ret = DBQuery.QueryLimitGoodsUsedNumByRoleID(dbMgr, roleID, goodsID, out dayID, out usedNum);
                if (ret < 0)
                {
                    //删除朋友失败
                    strcmd = string.Format("{0}:{1}:{2}:{3}", roleID, ret, dayID, usedNum);

                    LogManager.WriteLog(LogTypes.Error, string.Format("通过角色ID和物品ID查询物品每日的已经购买数量失败，CMD={0}, RoleID={1}",
                        (TCPGameServerCmds)nID, roleID));
                }
                else
                {
                    strcmd = string.Format("{0}:{1}:{2}:{3}", roleID, 0, dayID, usedNum);
                }

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                //System.Windows.Application.Current.Dispatcher.Invoke((MethodInvoker)delegate
                //{
                //
                DataHelper.WriteFormatExceptionLog(ex, "", false);
                //throw ex;
                //});
            }

            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
            return TCPProcessCmdResults.RESULT_DATA;
        }

        /// <summary>
        ///     Cập nhật số lần sử dụng vật phẩm
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private static TCPProcessCmdResults ProcessDBUpdateLimitGoodsUsedNumCmd(DBManager dbMgr, TCPOutPacketPool pool,
            int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string cmdData = null;

            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Wrong socket data CMD={0}", (TCPGameServerCmds)nID));

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                return TCPProcessCmdResults.RESULT_DATA;
            }

            try
            {
                var fields = cmdData.Split(':');
                if (fields.Length != 4)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format(
                        "Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}",
                        (TCPGameServerCmds)nID, fields.Length, cmdData));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                var roleID = Convert.ToInt32(fields[0]);
                var goodsID = Convert.ToInt32(fields[1]);
                var dayID = Convert.ToInt32(fields[2]);
                var usedNum = Convert.ToInt32(fields[3]);

                var dbRoleInfo = dbMgr.GetDBRoleInfo(roleID);
                if (null == dbRoleInfo)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Source player is not exist, CMD={0}, RoleID={1}",
                        (TCPGameServerCmds)nID, roleID));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                var strcmd = "";

                //添加限购物品的历史记录
                var ret = DBWriter.AddLimitGoodsBuyItem(dbMgr, roleID, goodsID, dayID, usedNum);
                if (ret < 0)
                {
                    //删除朋友失败
                    strcmd = string.Format("{0}:{1}", roleID, -1);

                    LogManager.WriteLog(LogTypes.Error, string.Format("添加限购物品的历史记录失败，CMD={0}, RoleID={1}",
                        (TCPGameServerCmds)nID, roleID));
                }
                else
                {
                    strcmd = string.Format("{0}:{1}", roleID, 0);
                }

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                //System.Windows.Application.Current.Dispatcher.Invoke((MethodInvoker)delegate
                //{
                //
                DataHelper.WriteFormatExceptionLog(ex, "", false);
                //throw ex;
                //});
            }

            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
            return TCPProcessCmdResults.RESULT_DATA;
        }

        /// <summary>
        ///   Cập nhật thông tin đồng
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private static TCPProcessCmdResults ProcessUpdateUserGoldCmd(DBManager dbMgr, TCPOutPacketPool pool, int nID,
            byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            /*
             *  注意：如果协议修改了，一定不要忘记确认下是否需要修改ProcessRoleHuobiOffline这个函数
             *  chenjg 20150422
             */
            tcpOutPacket = null;
            string cmdData = null;

            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Wrong socket data CMD={0}", (TCPGameServerCmds)nID));

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                return TCPProcessCmdResults.RESULT_DATA;
            }

            try
            {
                var fields = cmdData.Split(':');
                if (fields.Length != 2)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format(
                        "Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}",
                        (TCPGameServerCmds)nID, fields.Length, cmdData));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                var roleID = Convert.ToInt32(fields[0]);
                var addOrSubUserGold = Convert.ToInt32(fields[1]);

                var strcmd = "";
                var dbRoleInfo = dbMgr.GetDBRoleInfo(roleID);
                if (null == dbRoleInfo)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Source player is not exist, CMD={0}, RoleID={1}",
                        (TCPGameServerCmds)nID, roleID));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                var failed = false;
                var userGold = 0;
                lock (dbRoleInfo)
                {
                    //判断如果是扣除操作，则可能是银两余额不足而失败
                    if (addOrSubUserGold < 0 && dbRoleInfo.Gold < Math.Abs(addOrSubUserGold))
                    {
                        failed = true;
                    }
                    else //处理元宝的加减
                    {
                        dbRoleInfo.Gold = Math.Max(0, dbRoleInfo.Gold + addOrSubUserGold);
                        userGold = dbRoleInfo.Gold;
                    }
                }

                //如果扣除失败
                if (failed)
                {
                    //添加任务失败
                    strcmd = string.Format("{0}:{1}", roleID, -1);
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                //不等于0，才更新数据库
                if (addOrSubUserGold != 0)
                {
                    //将用户的请求发起写数据库的操作
                    var ret = DBWriter.UpdateRoleGold(dbMgr, roleID, userGold);
                    if (!ret)
                    {
                        LogManager.WriteLog(LogTypes.Error, string.Format("更新角色金币失败，CMD={0}, RoleID={1}",
                            (TCPGameServerCmds)nID, roleID));

                        //添加任务失败
                        strcmd = string.Format("{0}:{1}", roleID, -2);

                        tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                        return TCPProcessCmdResults.RESULT_DATA;
                    }
                }

                strcmd = string.Format("{0}:{1}", roleID, userGold);
                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                //System.Windows.Application.Current.Dispatcher.Invoke((MethodInvoker)delegate
                //{
                //
                DataHelper.WriteFormatExceptionLog(ex, "", false);
                //throw ex;
                //});
            }

            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
            return TCPProcessCmdResults.RESULT_DATA;
        }

        /// <summary>
        ///     Thực hiện update RolePRamMenter
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private static TCPProcessCmdResults ProcessUpdateRoleParamCmd(DBManager dbMgr, TCPOutPacketPool pool, int nID,
            byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string cmdData = null;

            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Wrong socket data CMD={0}", (TCPGameServerCmds)nID));

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                return TCPProcessCmdResults.RESULT_DATA;
            }

            try
            {
                var fields = cmdData.Split(':');
                if (fields.Length != 3)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format(
                        "Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}",
                        (TCPGameServerCmds)nID, fields.Length, cmdData));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                var roleID = Convert.ToInt32(fields[0]);
                var name = fields[1];
                var value = fields[2];

                var dbRoleInfo = dbMgr.GetDBRoleInfo(roleID);
                if (null == dbRoleInfo)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Source player is not exist, CMD={0}, RoleID={1}",
                        (TCPGameServerCmds)nID, roleID));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                var strcmd = "";

                Global.UpdateRoleParamByName(dbMgr, dbRoleInfo, name, value);

                strcmd = string.Format("{0}:{1}", roleID, 0);

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                //System.Windows.Application.Current.Dispatcher.Invoke((MethodInvoker)delegate
                //{
                //
                DataHelper.WriteFormatExceptionLog(ex, "", false);
                //throw ex;
                //});
            }

            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
            return TCPProcessCmdResults.RESULT_DATA;
        }

        /// <summary>
        ///     Lấy ra nạp đầu
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private static TCPProcessCmdResults ProcessQueryFirstChongZhiDaLiByUserIDCmd(DBManager dbMgr,
            TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string cmdData = null;

            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Wrong socket data CMD={0}", (TCPGameServerCmds)nID));

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                return TCPProcessCmdResults.RESULT_DATA;
            }

            try
            {
                var fields = cmdData.Split(':');
                if (fields.Length != 1)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format(
                        "Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}",
                        (TCPGameServerCmds)nID, fields.Length, cmdData));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                //参数0是roleid
                var roleID = Convert.ToInt32(fields[0]);

                var strcmd = "";
                var dbRoleInfo = dbMgr.GetDBRoleInfo(roleID);
                if (null == dbRoleInfo)
                {
                    strcmd = string.Format("{0}", -1);
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                /// 通过用户ID，查询是否已经领取过首充大礼
                var totalNum = DBQuery.GetFirstChongZhiDaLiNum(dbMgr, dbRoleInfo.UserID);

                strcmd = string.Format("{0}", totalNum);
                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                //System.Windows.Application.Current.Dispatcher.Invoke((MethodInvoker)delegate
                //{
                //
                DataHelper.WriteFormatExceptionLog(ex, "", false);
                //throw ex;
                //});
            }

            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
            return TCPProcessCmdResults.RESULT_DATA;
        }

        /// <summary>
        ///     添加系统交易1日志
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private static TCPProcessCmdResults ProcessAddExchange1ItemCmd(DBManager dbMgr, TCPOutPacketPool pool, int nID,
            byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string cmdData = null;

            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Wrong socket data CMD={0}", (TCPGameServerCmds)nID));

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                return TCPProcessCmdResults.RESULT_DATA;
            }

            try
            {
                var fields = cmdData.Split(':');
                if (fields.Length != 6)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format(
                        "Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}",
                        (TCPGameServerCmds)nID, fields.Length, cmdData));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                var rid = Convert.ToInt32(fields[0]);
                var goodsid = Convert.ToInt32(fields[1]);
                var goodsnum = Convert.ToInt32(fields[2]);
                var leftgoodsnum = Convert.ToInt32(fields[3]);
                var otherroleid = Convert.ToInt32(fields[4]);
                var result = fields[5];

                var ret = DBWriter.AddExchange1Item(dbMgr, rid, goodsid, goodsnum, leftgoodsnum, otherroleid, result);

                var strcmd = "";
                strcmd = string.Format("{0}", ret);
                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                //System.Windows.Application.Current.Dispatcher.Invoke((MethodInvoker)delegate
                //{
                //
                DataHelper.WriteFormatExceptionLog(ex, "", false);
                //throw ex;
                //});
            }

            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
            return TCPProcessCmdResults.RESULT_DATA;
        }

        /// <summary>
        ///     Đổi phái
        /// </summary>
        private static TCPProcessCmdResults ProcessExecuteChangeOccupationCmd(DBManager dbMgr, TCPOutPacketPool pool,
            int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string cmdData = null;

            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Wrong socket data CMD={0}", (TCPGameServerCmds)nID));

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                return TCPProcessCmdResults.RESULT_DATA;
            }

            try
            {
                var fields = cmdData.Split(':');
                if (fields.Length != 3)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format(
                        "Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}",
                        (TCPGameServerCmds)nID, fields.Length, cmdData));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                var roleID = Convert.ToInt32(fields[0]);
                var factionID = Convert.ToInt32(fields[1]);
                var routeID = Convert.ToInt32(fields[2]);

                var dbRoleInfo = dbMgr.GetDBRoleInfo(roleID);
                if (null == dbRoleInfo)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Source player is not exist, CMD={0}, RoleID={1}",
                        (TCPGameServerCmds)nID, roleID));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                var ret = DBWriter.UpdateRoleFactionAndRoute(dbMgr, roleID, factionID, routeID);
                if (!ret)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format(
                        "Update role faction and route faild. CMD={0}, RoleID={1}",
                        (TCPGameServerCmds)nID, roleID));
                }
                else
                {
                    var userID = "";
                    lock (dbRoleInfo)
                    {
                        dbRoleInfo.Occupation = factionID;
                        dbRoleInfo.SubID = routeID;
                        userID = dbRoleInfo.UserID;
                    }

                    //将用户的请求更新内存缓存
                    if (userID != "")
                    {
                        var dbUserInfo = dbMgr.GetDBUserInfo(userID);
                        if (null != dbUserInfo)
                            lock (dbUserInfo)
                            {
                                for (var i = 0; i < dbUserInfo.ListRoleLevels.Count; i++)
                                    if (dbUserInfo.ListRoleIDs[i] == roleID)
                                        dbUserInfo.ListRoleOccups[i] = factionID;
                            }
                    }
                }

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "1", nID);

                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                //System.Windows.Application.Current.Dispatcher.Invoke((MethodInvoker)delegate
                //{
                //
                DataHelper.WriteFormatExceptionLog(ex, "", false);
                //throw ex;
                //});
            }

            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
            return TCPProcessCmdResults.RESULT_DATA;
        }

        /// <summary>
        /// Lấy ra thông tin cơ bản của người chơi
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <param name="tcpOutPacket"></param>
        /// <returns></returns>
        private static TCPProcessCmdResults ProcessQueryRoleMiniInfoCmd(DBManager dbMgr, TCPOutPacketPool pool, int nID,
            byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            long rid = 0;
            RoleMiniInfo roleMiniInfo = null;

            try
            {
                rid = DataHelper.BytesToObject<long>(data, 0, count);
                if (rid > 0) roleMiniInfo = CacheManager.GetRoleMiniInfo(rid);

                tcpOutPacket = DataHelper.ObjectToTCPOutPacket(roleMiniInfo, pool, nID);
                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, "", false);
            }

            tcpOutPacket =
                DataHelper.ObjectToTCPOutPacket(roleMiniInfo, pool,
                    (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
            return TCPProcessCmdResults.RESULT_DATA;
        }

        private static TCPProcessCmdResults ProcessUpdateRoleKuaFuDayLogCmd(DBManager dbMgr, TCPOutPacketPool pool,
            int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            RoleKuaFuDayLogData cmdData = null;

            try
            {
                cmdData = DataHelper.BytesToObject<RoleKuaFuDayLogData>(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Wrong socket data CMD={0}", (TCPGameServerCmds)nID));

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                return TCPProcessCmdResults.RESULT_DATA;
            }

            try
            {
                if (null == cmdData)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("解析数据结果RoleKuaFuDayLogData失败, CMD={0}, Recv={1}",
                        (TCPGameServerCmds)nID, data.Length));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                DBWriter.UpdateRoleKuaFuDayLog(dbMgr, cmdData);

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "1", nID);
                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, "", false);
            }

            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
            return TCPProcessCmdResults.RESULT_DATA;
        }

        /// <summary>
        ///     Thêm đồng vào thương khố
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private static TCPProcessCmdResults ProcessAddRoleStoreYinliang(DBManager dbMgr, TCPOutPacketPool pool, int nID,
            byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string cmdData = null;

            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Wrong socket data CMD={0}", (TCPGameServerCmds)nID));

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                return TCPProcessCmdResults.RESULT_DATA;
            }

            try
            {
                var fields = cmdData.Split(':');
                if (fields.Length != 2)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format(
                        "Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}",
                        (TCPGameServerCmds)nID, fields.Length, cmdData));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                var roleID = Convert.ToInt32(fields[0]);
                var value = Convert.ToInt64(fields[1]);

                var strcmd = "";
                var dbRoleInfo = dbMgr.GetDBRoleInfo(roleID);
                if (null == dbRoleInfo)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Source player is not exist, CMD={0}, RoleID={1}",
                        (TCPGameServerCmds)nID, roleID));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                var failed = false;
                long userYinLiang = 0;

                lock (dbRoleInfo.GetMoneyLock)
                {
                    if (value < 0 && dbRoleInfo.store_yinliang < Math.Abs(value))
                    {
                        failed = true;
                    }
                    else
                    {
                        dbRoleInfo.store_yinliang = Math.Max(0, dbRoleInfo.store_yinliang + value);
                        userYinLiang = dbRoleInfo.store_yinliang;
                    }
                }

                if (failed)
                {
                    strcmd = string.Format("{0}:{1}", roleID, -1);
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                if (value != 0)
                {
                    var ret = DBWriter.UpdateRoleStoreYinLiang(dbMgr, roleID, userYinLiang);
                    if (!ret)
                    {
                        LogManager.WriteLog(LogTypes.Error, string.Format("更新角色仓库金币失败，CMD={0}, RoleID={1}",
                            (TCPGameServerCmds)nID, roleID));

                        //添加失败
                        strcmd = string.Format("{0}:{1}", roleID, -2);

                        tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                        return TCPProcessCmdResults.RESULT_DATA;
                    }
                }

                strcmd = string.Format("{0}:{1}", roleID, userYinLiang);
                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, "", false);
            }

            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
            return TCPProcessCmdResults.RESULT_DATA;
        }

        /// <summary>
        ///     Thêm bạc vào thương khố cho người chơi
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private static TCPProcessCmdResults ProcessAddRoleStoreMoney(DBManager dbMgr, TCPOutPacketPool pool, int nID,
            byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string cmdData = null;

            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Wrong socket data CMD={0}", (TCPGameServerCmds)nID));

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                return TCPProcessCmdResults.RESULT_DATA;
            }

            try
            {
                var fields = cmdData.Split(':');
                if (fields.Length != 2)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format(
                        "Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}",
                        (TCPGameServerCmds)nID, fields.Length, cmdData));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                /// ID người chơi
                var roleID = Convert.ToInt32(fields[0]);
                /// Số bạc thêm vào thương khố
                var value = Convert.ToInt32(fields[1]);

                var strcmd = "";
                var dbRoleInfo = dbMgr.GetDBRoleInfo(roleID);
                if (null == dbRoleInfo)
                {
                    LogManager.WriteLog(LogTypes.Error,
                        string.Format("Source player is not exist, CMD={0}, RoleID={1}", (TCPGameServerCmds)nID,
                            roleID));
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                var failed = false;
                long userMoney = 0;
                lock (dbRoleInfo)
                {
                    if (value < 0 && dbRoleInfo.store_money < Math.Abs(value))
                    {
                        failed = true;
                    }
                    else
                    {
                        dbRoleInfo.store_money = Math.Max(0, dbRoleInfo.store_money + value);
                        userMoney = dbRoleInfo.store_money;
                    }
                }

                /// Nếu toang
                if (failed)
                {
                    /// Gửi gói tin thao tác thất bại
                    strcmd = string.Format("{0}:{1}", roleID, -1);
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                /// Nếu khác 0 thì cập nhật DB
                if (value != 0)
                {
                    var ret = DBWriter.UpdateRoleStoreMoney(dbMgr, roleID, userMoney);
                    if (!ret)
                    {
                        LogManager.WriteLog(LogTypes.Error,
                            string.Format("Update role store money faild，CMD={0}, RoleID={1}", (TCPGameServerCmds)nID,
                                roleID));
                        strcmd = string.Format("{0}:{1}", roleID, -2);
                        tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                        return TCPProcessCmdResults.RESULT_DATA;
                    }
                }

                /// Gửi lại gói tin về GS
                strcmd = string.Format("{0}:{1}", roleID, userMoney);
                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, "", false);
            }

            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
            return TCPProcessCmdResults.RESULT_DATA;
        }
    }
}