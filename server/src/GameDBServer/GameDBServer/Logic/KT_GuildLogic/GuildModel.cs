using ProtoBuf;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Server.Data
{
    public enum GuildRank
    {
        /// <summary>
        /// Bang chúng
        /// </summary>
        Member = 0,

        /// <summary>
        /// Bang chủ
        /// </summary>
        Master = 1,

        /// <summary>
        /// Phó bang chủ
        /// </summary>
        ViceMaster = 2,

        /// <summary>
        /// Trưởng lão
        /// </summary>
        Ambassador = 3,

        /// <summary>
        /// Đường chủ
        /// </summary>
        ViceAmbassador = 4,

        /// <summary>
        /// Tinh anh
        /// </summary>
        Elite = 5,

        /// <summary>
        /// Tổng số
        /// </summary>
        Count,
    }

    [ProtoContract]
    public class RequestJoin
    {
        [ProtoMember(1)]
        public int ID { get; set; }

        [ProtoMember(2)]
        public int RoleID { get; set; }

        [ProtoMember(3)]
        public string RoleName { get; set; }

        [ProtoMember(4)]
        public int RoleFactionID { get; set; }

        [ProtoMember(5)]
        public long RoleValue { get; set; }

        [ProtoMember(6)]
        public int GuildID { get; set; }

        [ProtoMember(7)]
        public DateTime TimeRequest { get; set; }

        /// <summary>
        /// Cấp độ của thằng người chơi
        /// </summary>
        [ProtoMember(8)]
        public int RoleLevel { get; set; }
    }

    /// <summary>
    /// Thông tin bang cơ bản
    /// </summary>

    [ProtoContract]
    public class MiniGuildInfo
    {
        /// <summary>
        /// ID của bang
        /// </summary>
        [ProtoMember(1)]
        public int GuildId { get; set; }

        /// <summary>
        /// Tên bang hội
        /// </summary>
        [ProtoMember(2)]
        public string GuildName { get; set; }

        /// <summary>
        /// Cấp bang hội
        /// </summary>
        [ProtoMember(3)]
        public int GuilLevel { get; set; }

        /// <summary>
        /// Exp hiện tại của bang
        /// </summary>
        [ProtoMember(4)]
        public int GuildExp { get; set; }

        /// <summary>
        /// Tên bang chủ
        /// </summary>
        [ProtoMember(5)]
        public string HostName { get; set; }

        /// <summary>
        /// Tổng số thành viên
        /// </summary>
        [ProtoMember(6)]
        public int TotalMember { get; set; }

        /// <summary>
        /// Thông tin kỹ năng
        /// </summary>
        [ProtoMember(7)]
        public string SkillNote { get; set; }
        /// <summary>
        /// Quỷ bang hiện có
        /// </summary>
        [ProtoMember(8)]
        public int GuildMoney { get; set; }

        /// <summary>
        /// Danh sách vật phẩm cống hiến vào bang
        /// Mã hóa theo string array
        /// </summary>
        [ProtoMember(9)]
        public string ItemStore { get; set; }
        /// <summary>
        /// Thông báo bang
        /// </summary>
        [ProtoMember(10)]
        public string GuildNotify { get; set; }

        /// <summary>
        ///  Nhiệm vụ bang hiện tại
        /// </summary>
        [ProtoMember(11)]
        public GuildTask Task { get; set; }
        /// <summary>
        /// Thông tin công thành chiến
        /// </summary>
        [ProtoMember(12)]
        public GuildWar GuildWar { get; set; }

        /// <summary>
        /// Thằng này có phải là chủ thành
        /// </summary>
        [ProtoMember(13)]
        public int IsMainCity { get; set; }

        /// <summary>
        /// Số lượt đi phụ bản trong ngày
        /// </summary>
        [ProtoMember(14)]
        public int Total_Copy_Scenes_This_Week { get; set; }


        [ProtoMember(15)]
        public int MoneyBound { get; set; }
    }

    /// <summary>
    /// Thực thể bang hội
    /// </summary>
    public class Guild
    {
        /// <summary>
        /// Id GUild
        /// </summary>
        public int GuildID { get; set; }

        /// <summary>
        /// Tên Bang hội
        /// </summary>
        public string GuildName { get; set; }

        /// <summary>
        /// Quỹ thưởng
        /// </summary>
        public int MoneyBound { get; set; } = 0;

        /// <summary>
        /// Quỹ bang
        /// </summary>
        public int MoneyStore { get; set; } = 0;

        /// <summary>
        /// ZONEID
        /// </summary>
        public int ZoneID { get; set; }

        /// <summary>
        /// Thông Báo Bang
        /// </summary>
        public string Notify { get; set; } = "";

        /// <summary>
        /// Có đang là thành chủ hay không
        /// </summary>
        public int IsMainCity { get; set; } = 0;

        /// <summary>
        /// Thiết lập tỉ lệ tối đa có thể phân phát lợi tức bang hội
        /// </summary>
        public int MaxWithDraw { get; set; } = 0;

        /// <summary>
        /// Bang chủ là ai
        /// </summary>
        public int Leader { get; set; } = -1;

        /// <summary>
        /// Ngày tạo bang
        /// </summary>
        public DateTime DateCreate { get; set; } = DateTime.Now;

        /// <summary>
        /// Cấp độ bang hội
        /// </summary>
        public int GuildLevel { get; set; }

        /// <summary>
        /// Kinh nghiệm bang
        /// </summary>
        public int GuildExp { get; set; }

        /// <summary>
        /// Có bật chế độ tự duyệt thành viên hay không
        /// </summary>
        public int AutoAccept { get; set; }

        /// <summary>
        /// Điều kiện duyệt thành viên mã hóa theo string Arrray
        /// </summary>
        public string RuleAccept { get; set; }

        /// <summary>
        /// Danh sách vật phẩm cống hiến vào bang
        /// Mã hóa theo string array
        /// </summary>
        public string ItemStore { get; set; }

        /// <summary>
        /// Danh sách kỹ năng bang
        /// </summary>
        public string SkillNote { get; set; }

        /// <summary>
        /// Chuẩn bị xóa
        /// </summary>
        public int PreDelete { get; set; }


        /// <summary>
        /// Số lượng đi phụ bản trong tuần
        /// </summary>
        public int Total_Copy_Scenes_This_Week { get; set; } = 0;

        /// <summary>
        /// Ngày bắt đầu ấn xóa là ngày nào
        /// </summary>
        public DateTime DeleteStartDay { get; set; }

        /// <summary>
        /// Danh sách bang
        /// </summary>
        public ConcurrentDictionary<int, GuildMember> GuildMember { get; set; }

        /// <summary>
        /// Danh sách thành viên ĩn vào bang
        /// </summary>
        public ConcurrentDictionary<int, RequestJoin> TotalRequestJoin { get; set; }

        /// <summary>
        /// Nhiệm vụ hiện tại của bang
        /// </summary>
        public GuildTask Task { get; set; }

        public GuildWar GuildWar { get; set; }
    }

    [ProtoContract]
    public class GuildWar
    {
        [ProtoMember(1)]
        public int GuildID { get; set; }

        [ProtoMember(2)]
        public string GuildName { get; set; }

        [ProtoMember(3)]
        public string TeamList { get; set; }

        [ProtoMember(4)]
        public int WeekID { get; set; }

    }

    #region CLIENT TCP

    [ProtoContract]
    public class RequestJoinInfo
    {
        [ProtoMember(1)]
        public List<RequestJoin> TotalRequestJoin { get; set; }

        [ProtoMember(2)]
        public int AutoAccept { get; set; }

        [ProtoMember(3)]
        public string AutoAcceptRule { get; set; }

        /// <summary>
        /// Số trang hiện tại
        /// </summary>
        [ProtoMember(4)]
        public int PageIndex { get; set; }

        /// <summary>
        /// Tổng số trang
        /// </summary>
        [ProtoMember(5)]
        public int TotalPage { get; set; }
    }

    /// <summary>
    /// Thành viên ưu tú
    /// </summary>
    [ProtoContract]
    public class OfficeRankInfo
    {
        /// <summary>
        /// Cấp hiện tại của bang
        /// </summary>
        [ProtoMember(1)]
        public int GuildRank { get; set; }

        [ProtoMember(2)]
        public List<OfficeRankMember> OffcieRankMember { get; set; }
    }

    [ProtoContract]
    public class OfficeRankMember
    {
        [ProtoMember(1)]
        public int ID { get; set; }

        [ProtoMember(2)]
        public string RoleName { get; set; }

        [ProtoMember(3)]
        public string RankTile { get; set; }

        [ProtoMember(4)]
        public string OfficeRankTitle { get; set; }

        [ProtoMember(5)]
        public int RoleID { get; set; }

        [ProtoMember(6)]
        public int RankNum { get; set; }
    }


    [ProtoContract]
    public class GuildTask
    {
        [ProtoMember(1)]
        public int GuildID { get; set; }

        [ProtoMember(2)]
        public int TaskID { get; set; }

        [ProtoMember(3)]
        public int TaskValue { get; set; }

        [ProtoMember(4)]
        public int DayCreate { get; set; } = -1;

        [ProtoMember(5)]
        public int TaskCountInDay { get; set; }
    }

    /// <summary>
    /// Thông tin thành viên bang hội
    /// </summary>
    [ProtoContract]
    public class GuildMemberData
    {
        /// <summary>
        /// Danh sách thành viên
        /// </summary>
        [ProtoMember(1)]
        public List<GuildMember> TotalGuildMember { get; set; }

        /// <summary>
        /// Số trang hiện tại
        /// </summary>
        [ProtoMember(2)]
        public int PageIndex { get; set; }

        /// <summary>
        /// Tổng số trang
        /// </summary>
        [ProtoMember(3)]
        public int TotalPage { get; set; }
    }

    /// <summary>
    /// Thành viên trong bang
    /// </summary>
    ///
    [ProtoContract]
    public class GuildMember
    {
        /// <summary>
        /// ID Role
        /// </summary>
        ///
        [ProtoMember(1)]
        public int RoleID { get; set; }

        /// <summary>
        /// Tên của thành viên
        /// </summary>
        ///
        [ProtoMember(2)]
        public string RoleName { get; set; }

        /// <summary>
        /// Phái nào
        /// </summary>
        ///
        [ProtoMember(3)]
        public int FactionID { get; set; }

        /// <summary>
        /// Cấp bậc
        /// </summary>
        ///
        [ProtoMember(4)]
        public int Rank { get; set; }

        /// <summary>
        /// Cấp độ
        /// </summary>
        ///
        [ProtoMember(5)]
        public int Level { get; set; }

        /// <summary>
        /// ID của GUild hiện tại
        /// </summary>
        [ProtoMember(6)]
        public int GuildID { get; set; }

        /// <summary>
        /// Cống hiến cá nhân
        [ProtoMember(7)]
        public int GuildMoney { get; set; }

        /// <summary>
        /// Trạng thái online
        /// </summary>
        [ProtoMember(8)]
        public int OnlineStatus { get; set; }

        /// <summary>
        /// ZoneID nào
        /// </summary>
        [ProtoMember(9)]
        public int ZoneID { get; set; }

        [ProtoMember(10)]
        public long TotalValue { get; set; }

        [ProtoMember(11)]
        public int WeekPoint { get; set; }
    }

    #endregion CLIENT TCP

    /// <summary>
	/// Sửa tôn chỉ bang hội
	/// </summary>
	[ProtoContract]
    public class GuildChangeSlogan
    {
        /// <summary>
        /// ID bang hội
        /// </summary>
        [ProtoMember(1)]
        public int GuildID { get; set; }

        /// <summary>
        /// Tôn chỉ bang hội
        /// </summary>
        [ProtoMember(2)]
        public string Slogan { get; set; }
    }
}