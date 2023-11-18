using ProtoBuf;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace GameServer.KiemThe.Core.Activity.DaySeriesLoginEvent
{
    [XmlRoot(ElementName = "AwardItem")]
    [ProtoContract]
    public class RollAwardItem
    {
        [XmlAttribute(AttributeName = "ItemID")]
        [ProtoMember(1)]
        public int ItemID { get; set; }

        [XmlAttribute(AttributeName = "Number")]
        [ProtoMember(2)]
        public int Number { get; set; }

        [XmlAttribute(AttributeName = "Rate")]
        [ProtoMember(3)]
        public int Rate { get; set; }
    }

    /// <summary>
    /// Gửi về client cả mả này
    /// </summary>
    [ProtoContract]

    public class SevenDayEvent
    {
        [ProtoMember(1)]
        public SevenDaysLogin SevenDaysLogin { get; set; }
        [ProtoMember(2)]
        public SevenDaysLoginContinus SevenDaysLoginContinus { get; set; }
    }


    [XmlRoot(ElementName = "SevenDaysLoginItem")]
    [ProtoContract]
    public class SevenDaysLoginItem
    {
        [XmlElement(ElementName = "RollAwardItem")]
        [ProtoMember(1)]
        public List<RollAwardItem> RollAwardItem { get; set; }

        [XmlAttribute(AttributeName = "ID")]
        [ProtoMember(2)]
        public int ID { get; set; }

        [XmlAttribute(AttributeName = "Days")]
        [ProtoMember(3)]
        public int Days { get; set; }
    }

    [XmlRoot(ElementName = "SevenDaysLogin")]
    [ProtoContract]
    public class SevenDaysLogin
    {
        [XmlElement(ElementName = "Item")]
        [ProtoMember(1)]
        public List<SevenDaysLoginItem> SevenDaysLoginItem { get; set; }

        [XmlAttribute(AttributeName = "Name")]
        [ProtoMember(2)]
        public string Name { get; set; }

        [XmlAttribute(AttributeName = "IsOpen")]
        [ProtoMember(3)]
        public bool IsOpen { get; set; }

        /// <summary>
        /// Đây đang là ngày thứ mấy tính từ lúc nhân vật được tạo
        /// </summary>
        [ProtoMember(4)]
        public int DayID { get; set; }

        /// <summary>
        /// Thằng này đã nhận những mốc nào rồi
        /// For từ 1 tới DayID nếu mà không nằm trong RevicedHistory tức là thằng này đã bị quá hạn nhận
        /// Cái nào nằm trong ReviceHistory thì tức là đã nhận
        /// Nếu DAYID chưa quá mốc tối đa trong SevenDaysLoginItem thì hiện có thể nhận cho nhân vật nhận
        /// </summary>
        [ProtoMember(5)]
        public List<SevenDayLoginHistoryItem> RevicedHistory { get; set; }


    }

    /// <summary>
    /// Lịch sử lưu lại những gì thằng này đã nhận
    ///
    /// </summary>
    ///
    [ProtoContract]
    public class SevenDayLoginHistoryItem
    {
        /// <summary>
        /// Ngày nào
        /// </summary>
        ///
        [ProtoMember(1)]
        public int DayID { get; set; }

        /// <summary>
        /// Vật phẩm đã nhận
        /// </summary>
        ///
        [ProtoMember(2)]
        public int GoodIDs { get; set; }
        /// <summary>
        /// ID vật phẩm đã nhận
        /// </summary>
        ///
        [ProtoMember(3)]
        public int GoodNum { get; set; }
    }


    [XmlRoot(ElementName = "SevenDaysLoginContinus")]
    [ProtoContract]
    public class SevenDaysLoginContinus
    {
        [XmlElement(ElementName = "Item")]
        [ProtoMember(1)]
        public List<SevenDaysLoginItem> SevenDaysLoginItem { get; set; }

        [XmlAttribute(AttributeName = "Name")]
        [ProtoMember(2)]
        public string Name { get; set; }

        [XmlAttribute(AttributeName = "IsOpen")]
        [ProtoMember(3)]
        public bool IsOpen { get; set; }

        /// <summary>
        /// Số ngày đã đăng nhập liên tiếp
        /// Chỗ này sẽ hiện ở client là bạn đã đăng nhập liên tiếp X ngày
        /// Bạn có thể nhận thưởng mốc tương ứng
        /// </summary>
        [ProtoMember(4)]
        public int TotalDayLoginContinus { get; set; }

        /// <summary>
        /// Đã nhận tới mốc nào rồi
        /// </summary>
        [ProtoMember(5)]
        public int Step { get; set; }

        /// <summary>
        /// Quà đã nhận khi đăng nhập liên tiếp trong 7 ngày | Cái này sẽ ko reset suốt đời
        /// Lịch sử đã mút cái j trước đó
        /// ITEMID_ITEMNUM | ITEMID_ITEMNUM
        /// </summary>
        [ProtoMember(6)]
        public string SevenDayLoginAward { get; set; }
    }
}
