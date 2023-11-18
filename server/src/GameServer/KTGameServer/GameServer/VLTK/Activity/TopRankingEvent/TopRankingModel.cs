using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace GameServer.VLTK.Core.Activity.TopRankingEvent
{
    [XmlRoot(ElementName = "TopRankingConfig")]
    [ProtoContract]
    public class TopRankingConfig
    {
        /// <summary>
        /// Đây là kiểu xếp hạng gì
        /// </summary>
        ///
        [ProtoMember(1)]
        [XmlAttribute(AttributeName = "RankType")]
        public int RankType { get; set; }

        /// <summary>
        /// Tên bảng xếp hạng
        /// </summary>
        ///
        [ProtoMember(2)]
        [XmlAttribute(AttributeName = "RankName")]
        public string RankName { get; set; }

        /// <summary>
        ///  Ngày chốt quà
        /// </summary>
        ///

        [XmlAttribute(AttributeName = "EndTime")]
        public DateTime EndTime { get; set; }

        /// <summary>
        /// Dánh sách phần thưởng
        /// </summary>
        ///
        [ProtoMember(4)]
        [XmlElement(ElementName = "AwardConfig")]
        public List<AwardConfig> AwardConfig { get; set; }

        /// <summary>
        /// Bản thân thằng đó đứng ở đâu | Nếu thằng này có nằm trong top 10 thì get phần quà ra và ghi xuống cuối cùng là có thể nhận
        /// </summary>
        [ProtoMember(5)]
        public int SelfIndex { get; set; }

        /// <summary>
        /// Trạng thái để nhận biết sự kiện kết thúc hay chưa
        /// 0 : Đang diễn ra chưa thể nhận phỉa chờ sự kiện kế thúc
        /// 1 : Đã kết thúc | Đéo đạt yêu cầu để nhận
        /// 2 : Là đã nhận rồi
        //  3 : Là là có thể nhận nút nhận sẽ sáng lên
        /// </summary>
        [ProtoMember(6)]
        public int State { get; set; }

        [ProtoMember(7)]
        public List<PlayerRanking> ListPlayer { get; set; }


        [ProtoMember(8)]
        public int TimeLess { get; set; }


        /// <summary>
        /// Status thông báo sự kiện
        /// 0 ; Chưa thông báo gì
        /// 1 : đã thông báo gần kết thúc
        /// 2 : đã kết thúc
        /// </summary>
        public int ArletStatus { get; set; }
    }

    /// <summary>
    /// Thứ hạng người chơi
    /// </summary>
    [ProtoContract]
    public class PlayerRanking
    {
        /// <summary>
        /// Loại thứ hạng
        /// </summary>
        [ProtoMember(1)]
        public int Type { get; set; }

        /// <summary>
        /// ID người chơi
        /// </summary>
        [ProtoMember(2)]
        public int RoleID { get; set; }

        /// <summary>
        /// Tên người chơi
        /// </summary>
        [ProtoMember(3)]
        public string RoleName { get; set; }

        /// <summary>
        /// Môn phái
        /// </summary>
        [ProtoMember(4)]
        public int FactionID { get; set; }

        /// <summary>
        /// Hệ phái
        /// </summary>
        [ProtoMember(5)]
        public int RouteID { get; set; }

        /// <summary>
        /// Cấp độ
        /// </summary>
        [ProtoMember(6)]
        public int Level { get; set; }

        /// <summary>
        /// Giá trị trong hạng
        /// </summary>
        [ProtoMember(7)]
        public int Value { get; set; }

        /// <summary>
        /// Thứ hạng
        /// </summary>
        [ProtoMember(8)]
        public int ID { get; set; }


        /// <summary>
        /// Đã nhận thưởng đua top loại này chưa
        /// </summary>
        [ProtoMember(9)]
        public int Status { get; set; } = -1;

        /// <summary>
        /// Thứ hạng cuối cùng của thằng này sau khi sự kiện kết thúc
        /// </summary>
        [ProtoMember(10)]
        public int LastIndex { get; set; } = -1;
    }

    /// <summary>
    /// Config quà tặng
    /// </summary>
    ///
    [XmlRoot(ElementName = "AwardConfig")]
    [ProtoContract]
    public class AwardConfig
    {
        [ProtoMember(1)]
        [XmlAttribute(AttributeName = "RankStart")]
        public int RankStart { get; set; }

        [ProtoMember(2)]
        [XmlAttribute(AttributeName = "RankEnd")]
        public int RankEnd { get; set; }

        [ProtoMember(3)]
        [XmlAttribute(AttributeName = "AwardList")]
        public string AwardList { get; set; }

        /// <summary>
        /// INdex mấy khi gửi lên server để biết nó muốn nhận mốc thưởng nào
        /// </summary>
        [ProtoMember(4)]
        [XmlAttribute(AttributeName = "Index")]
        public int Index { get; set; }
    }
}