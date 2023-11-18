using ProtoBuf;
using System.Collections.Generic;

namespace Server.Data
{
    /// <summary>
    /// Đối tượng RoleDataEx chứa thêm thông tin cho các sự kiện event
    /// </summary>
    [ProtoContract]
    public class RoleDataEx
    {
        /// <summary>
        /// ID của nhân vật
        /// </summary>
        [ProtoMember(1)]
        public int RoleID = 0;

        /// <summary>
        /// Tên của nhân vật
        /// </summary>
        [ProtoMember(2)]
        public string RoleName = "";

        /// <summary>
        /// Giới tính của nhân vật
        /// </summary>
        [ProtoMember(3)]
        public int RoleSex = 0;

        /// <summary>
        /// Phái
        /// </summary>
        [ProtoMember(4)]
        public int FactionID = 0;

        /// <summary>
        /// Cấp độ
        /// </summary>
        [ProtoMember(5)]
        public int Level = 1;

        /// <summary>
        /// Bạc Khóa
        /// </summary>
        [ProtoMember(6)]
        public int Money = 0;


        /// <summary>
        /// Kinh nghiệm
        /// </summary>
        [ProtoMember(7)]
        public long Experience = 0;

        /// <summary>
        /// Chế độ PK
        /// </summary>
        [ProtoMember(8)]
        public int PKMode = 0;

        /// <summary>
        /// Giá trị PK
        /// </summary>
        [ProtoMember(9)]
        public int PKValue = 0;

        /// <summary>
        /// Bản đồ hiện tịa
        /// </summary>
        [ProtoMember(10)]
        public int MapCode = 0;

        /// <summary>
        /// Tọa độ X hiện tại
        /// </summary>
        [ProtoMember(11)]
        public int PosX { get; set; } = 0;

        /// <summary>
        /// Tọa độ Y hiện tại
        /// </summary>
        [ProtoMember(12)]
        public int PosY = 0;

        /// <summary>
        /// Hướng của nhân vật
        /// </summary>
        [ProtoMember(13)]
        public int RoleDirection = 0;


        /// <summary>
        /// Danh sách nhiệm vụ
        /// </summary>
        [ProtoMember(14)]
        public List<OldTaskData> OldTasks = null;

        /// <summary>
        /// Ảnh Avata của nhân vật
        /// </summary>
        [ProtoMember(15)]
        public int RolePic = 0;

        /// <summary>
        /// Danh Sách nhiệm vụ
        /// </summary>
        [ProtoMember(16)]
        public List<TaskData> TaskDataList = null;

        /// <summary>
        /// Danh sách vật phẩm đang có
        /// </summary>
        [ProtoMember(17)]
        public List<GoodsData> GoodsDataList = null;

        /// <summary>
        /// Danh sách SKill Tay trái
        /// </summary>
        [ProtoMember(18)]
        public string MainQuickBarKeys = "";

        /// <summary>
        /// Danh sách skill tay phải
        /// </summary>
        [ProtoMember(19)]
        public string OtherQuickBarKeys = "";

        /// <summary>
        /// Số lần đăng nhập
        /// </summary>
        [ProtoMember(20)]
        public int LoginNum = 0;

        /// <summary>
        /// Tiền đồng
        /// </summary>
        [ProtoMember(21)]
        public int Token = 0;

        /// <summary>
        /// Danh sách bạn bè
        /// </summary>
        [ProtoMember(22)]
        public List<FriendData> FriendDataList = null;

        /// <summary>
        /// Tổng số thời gian online
        /// </summary>
        [ProtoMember(23)]
        public int TotalOnlineSecs = 0;

        /// <summary>
        /// Thời gian đăng nhập gần đây
        /// </summary>
        [ProtoMember(24)]
        public long LastOfflineTime = 0;

        /// <summary>
        /// Danh sách kỹ năng
        /// </summary>
        [ProtoMember(25)]
        public List<SkillData> SkillDataList = null;

        /// <summary>
        /// Thời gian đăng ký
        /// </summary>
        [ProtoMember(26)]
        public long RegTime = 0;

        /// <summary>
        /// Bán hàng
        /// </summary>
        [ProtoMember(27)]
        public List<GoodsData> SaleGoodsDataList = null;

        /// <summary>
        /// Danh sách bufff
        /// </summary>
        [ProtoMember(28)]
        public List<BufferData> BufferDataList = null;

        /// <summary>
        /// Nhiệm vú chính tuyến
        /// </summary>
        [ProtoMember(29)]
        public int MainTaskID = 0;

        /// <summary>
        /// Điểmmm PK
        /// </summary>
        [ProtoMember(30)]
        public int PKPoint = 0;

        /// <summary>
        /// ZoneID của máy chủ
        /// </summary>
        [ProtoMember(31)]
        public int ZoneID = 0;

        /// <summary>
        ///  Bạc khóa
        /// </summary>
        [ProtoMember(32)]
        public int BoundToken = 0;

        /// <summary>
        /// ID Email cuối cùng
        /// </summary>
        [ProtoMember(33)]
        public int LastMailID = 0;

        /// <summary>
        ///  Vàng
        /// </summary>
        [ProtoMember(34)]
        public int BoundMoney = 0;

        /// <summary>
        /// Sử dụng vật phẩm limit
        /// </summary>
        [ProtoMember(35)]
        public List<GoodsLimitData> GoodsLimitDataList = null;

        /// <summary>
        /// Dữ liệu nhân vật
        /// </summary>
        [ProtoMember(36)]
        public Dictionary<string, RoleParamsData> RoleParamsDict = null;

        /// <summary>
        /// Thời điểm bắt đầu bị cấm chat
        /// </summary>
        [ProtoMember(37)]
        public long BanChatStartTime { get; set; } = 0;

        /// <summary>
        /// Thời gian duy trì cấm chat
        /// </summary>
        [ProtoMember(38)]
        public long BanChatDuration { get; set; } = 0;

        /// <summary>
        /// Tiền ở thủ khố
        /// </summary>
        [ProtoMember(39)]
        public int Store_Money = 0;

        [ProtoMember(40)]
        public List<int> GroupMailRecordList = null;

        /// <summary>
        /// ID Nhánh
        /// </summary>

        [ProtoMember(41)]
        public int SubID = 0;

        [ProtoMember(42)]
        public UserMiniData userMiniData;

        /// <summary>
        /// ID Bang hội
        /// </summary>
        [ProtoMember(43)]
        public int GuildID { get; set; } = 0;

        /// <summary>
        /// Tên bang hội
        /// </summary>
        [ProtoMember(44)]
        public string GuildName = "";

        /// <summary>
        /// Xếp hạng rank
        /// </summary>
        [ProtoMember(45)]
        public int GuildRank { get; set; } = 0;

        /// <summary>
        /// Tài sản cá nhân trong bang
        /// </summary>
        [ProtoMember(46)]
        public int RoleGuildMoney { get; set; } = 0;

        /// <summary>
        /// Uy Danh Giang Hồ
        /// </summary>
        [ProtoMember(47)]
        public int Prestige = 0;

        /// <summary>
        ///Quan hàm của nhân vật
        /// </summary>
        [ProtoMember(48)]
        public int OfficeRank = 0;

        /// <summary>
        /// Phúc lợi
        /// </summary>
        [ProtoMember(49)]
        public RoleWelfare RoleWelfare { get; set; }

        /// <summary>
        /// Danh sách vật phẩm dùng ở khay dùng nhanh
        /// </summary>
        [ProtoMember(50)]
        public string QuickItems { get; set; } = "";

        /// <summary>
        /// Danh sách pet
        /// </summary>
        [ProtoMember(51)]
        public List<PetData> Pets { get; set; }

        /// <summary>
        /// Mật khẩu cấp 2
        /// </summary>
        [ProtoMember(52)]
        public string SecondPassword { get; set; }

        /// <summary>
        /// Danh sách chức năng bị cấm
        /// </summary>
        [ProtoMember(53)]
        public Dictionary<int, BanUserByType> BannedList { get; set; }
    }
}