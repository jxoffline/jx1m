using ProtoBuf;
using System.Collections.Generic;

namespace Server.Data
{


    /// <summary>
    ///  Thực thể report khi người chơi click vào nút Chiến Báo
    /// </summary>
    [ProtoContract]
    public class GuildWarReport
    {
        /// <summary>
        /// Tích lũy bản thân cá nhân
        /// </summary>
        [ProtoMember(1)]
        public CurrentPoint _CurrentPoint { get; set; }

        /// <summary>
        /// Tích lũy của TOP 20  thành viên trong bang LISTVIEW
        /// </summary>
        [ProtoMember(2)]
        public List<GuildWarRanking> _GuildWarRanking { get; set; }

        /// <summary>
        /// Thông báo tạng thái các lãnh thổ đang chiếm
        /// </summary>
        [ProtoMember(3)]
        public List<TerritoryReport> _TerritoryReport { get; set; }
    }


    /// <summary>
    /// Thành tích của thành viên trong bang
    /// </summary>
    [ProtoContract]
    public class GuildWarRanking
    {
        [ProtoMember(1)]
        public string RoleName { get; set; }
        [ProtoMember(2)]
        public int Point { get; set; }
    }
    /// <summary>
    /// Lãnh thổ report
    /// </summary>
    [ProtoContract]
    public class TerritoryReport
    {
        /// <summary>
        /// Tên bản đồ
        /// </summary>
        [ProtoMember(1)]
        public string MapName { get; set; }

        /// <summary>
        /// Tổng tích lũy
        /// </summary>
        [ProtoMember(2)]
        public int TotalPoint { get; set; }

        /// <summary>
        /// Xếp hạng hiện tại cứ dưới khác 1 là màu đỏ , hạng 1 là màu xanh
        /// </summary>
        [ProtoMember(3)]
        public int Rank { get; set; }
    }

    /// <summary>
    /// Thông tin tích lũy cá nhân
    /// </summary>
    [ProtoContract]
    public class CurrentPoint
    {
        /// <summary>
        /// Tích lũy
        /// </summary>
        [ProtoMember(1)]
        public int Point { get; set; }

        /// <summary>
        /// Số long trụ đã hạ
        /// </summary>
        [ProtoMember(2)]
        public int TowerDesotryCount { get; set; }

        /// <summary>
        /// Số thành viên team địch đã tiê diệt
        /// </summary>
        [ProtoMember(3)]
        public int KillCount { get; set; }
    }
}