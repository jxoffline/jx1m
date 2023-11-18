using ProtoBuf;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace GameServer.KiemThe.Core.Activity.RechageEvent
{
    [ProtoContract]
    [XmlRoot(ElementName = "TotalItem")]
    public class TotalItem
    {
        [ProtoMember(1)]
        [XmlAttribute(AttributeName = "ItemID")]
        public int ItemID { get; set; }

        [ProtoMember(2)]
        [XmlAttribute(AttributeName = "Number")]
        public int Number { get; set; }
    }

    /// <summary>
    /// Sự kiện nạp đầu
    /// </summary>
    [ProtoContract]
    [XmlRoot(ElementName = "FistRechage")]
    public class FistRechage
    {
        /// <summary>
        /// Vật phảm sẽ nhận sau khi nạp
        /// </summary>
        [ProtoMember(1)]
        [XmlElement(ElementName = "TotalItem")]
        public List<TotalItem> TotalItem { get; set; }

        /// <summary>
        /// Tên sự kiện
        /// </summary>
        [ProtoMember(2)]
        [XmlAttribute(AttributeName = "Title")]
        public string Title { get; set; }

        /// <summary>
        /// AcitivyID để sau này tham chiếu TABLE đó có sự kiện hay ko có
        /// </summary>
        [ProtoMember(3)]
        [XmlAttribute(AttributeName = "AtivityID")]
        public int AtivityID { get; set; }

        /// <summary>
        /// Trạng thái nút nhận
        /// </summary>
        [ProtoMember(4)]
        [XmlIgnore]
        public string BtnState { get; set; }
    }

    /// <summary>
    /// Danh sách quà tặng nạp ngày
    /// </summary>
    [ProtoContract]
    [XmlRoot(ElementName = "DayRechageAward")]
    public class DayRechageAward
    {
        /// <summary>
        /// ID của mốc
        /// </summary>
        [ProtoMember(1)]
        [XmlAttribute(AttributeName = "ID")]
        public int ID { get; set; }

        /// <summary>
        /// Số tiền nạp vào tối thiểu
        /// </summary>
        [ProtoMember(2)]
        [XmlAttribute(AttributeName = "MinYuanBao")]
        public int MinYuanBao { get; set; }

        /// <summary>
        /// Danh sach svaja tphaarm sẽ nhận
        /// </summary>
        [ProtoMember(3)]
        [XmlAttribute(AttributeName = "GoodsIDs")]
        public string GoodsIDs { get; set; }
    }

    /// <summary>
    /// Sự kiện nạp ngày
    /// </summary>
    ///
    [ProtoContract]
    [XmlRoot(ElementName = "DayRechage")]
    public class DayRechage
    {
        /// <summary>
        /// Danh sách phần thưởng
        /// </summary>
        ///
        [ProtoMember(1)]
        [XmlElement(ElementName = "DayRechageAward")]
        public List<DayRechageAward> DayRechageAward { get; set; }

        /// <summary>
        /// Trạng thái nút nhận
        /// </summary>
        [ProtoMember(2)]
        [XmlIgnore]
        public string BtnState { get; set; }
    }

    [ProtoContract]
    [XmlRoot(ElementName = "TotalRechageAward")]
    public class TotalRechageAward
    {
        [ProtoMember(1)]
        [XmlAttribute(AttributeName = "ID")]
        public int ID { get; set; }

        [ProtoMember(2)]
        [XmlAttribute(AttributeName = "MinYuanBao")]
        public int MinYuanBao { get; set; }

        [ProtoMember(3)]
        [XmlAttribute(AttributeName = "GoodsIDs")]
        public string GoodsIDs { get; set; }
    }

    [ProtoContract]
    [XmlRoot(ElementName = "TotalRechage")]
    public class TotalRechage
    {
        [ProtoMember(1)]
        [XmlElement(ElementName = "TotalRechageAward")]
        public List<TotalRechageAward> TotalRechageAward { get; set; }

        /// <summary>
        /// Trạng thái nút nhận
        /// </summary>
        [ProtoMember(2)]
        [XmlIgnore]
        public string BtnState { get; set; }
    }

    [ProtoContract]
    [XmlRoot(ElementName = "ConsumeAward")]
    public class ConsumeAward
    {
        [ProtoMember(1)]
        [XmlAttribute(AttributeName = "ID")]
        public int ID { get; set; }

        [ProtoMember(2)]
        [XmlAttribute(AttributeName = "MinYuanBao")]
        public int MinYuanBao { get; set; }

        [ProtoMember(3)]
        [XmlAttribute(AttributeName = "GoodsIDs")]
        public string GoodsIDs { get; set; }
    }

    [ProtoContract]
    [XmlRoot(ElementName = "TotalConsume")]
    public class TotalConsume
    {
        [ProtoMember(1)]
        [XmlElement(ElementName = "ConsumeAward")]
        public List<ConsumeAward> ConsumeAward { get; set; }

        /// <summary>
        /// Trạng thái nút nhận
        /// </summary>
        [ProtoMember(2)]
        [XmlIgnore]
        public string BtnState { get; set; }
    }

    [ProtoContract]
    public class RechageAcitivty
    {
        /// <summary>
        /// Tích tiêu
        /// </summary>
        ///
        [ProtoMember(1)]
        public TotalConsume _TotalConsume { get; set; }

        /// <summary>
        /// Tích nạp
        /// </summary>
        ///
        [ProtoMember(2)]
        public TotalRechage _TotalRechage { get; set; }

        /// <summary>
        /// Nạp đầu
        /// </summary>
        ///
        [ProtoMember(3)]
        public FistRechage _FistRechage { get; set; }

        /// <summary>
        /// Nạp ngày
        /// </summary>
        ///
        [ProtoMember(4)]
        public DayRechage _DayRechage { get; set; }
    }
}