using ProtoBuf;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace GameServer.KiemThe.Core.Activity.EveryDayOnlineEvent
{

    /// <summary>
    /// Thực thể vật phẩm sẽ ROLL
    /// </summary>
    [ProtoContract]
    [XmlRoot(ElementName = "AwardItem")]
    public class AwardItem

    {
        /// <summary>
        /// ID vật phẩm
        /// </summary>
        [ProtoMember(1)]
        [XmlAttribute(AttributeName = "ItemID")]
        public int ItemID { get; set; }

        /// <summary>
        /// Số lượng
        /// </summary>
        [ProtoMember(2)]
        [XmlAttribute(AttributeName = "Number")]
        public int Number { get; set; }

        /// <summary>
        /// RATE sẽ random ra
        /// </summary>
        [ProtoMember(3)]
        [XmlAttribute(AttributeName = "Rate")]
        public int Rate { get; set; }

    }

    /// <summary>
    /// Danh sách vật phẩm
    /// </summary>
    [ProtoContract]
    [XmlRoot(ElementName = "EveryDayOnLine")]
    public class EveryDayOnLine
    {
        /// <summary>
        /// Mốc nào
        /// </summary>
        [ProtoMember(1)]
        [XmlAttribute(AttributeName = "StepID")]
        public int StepID { get; set; }
        /// <summary>
        /// Thời gian online
        /// </summary>
        [ProtoMember(2)]
        [XmlAttribute(AttributeName = "TimeSecs")]
        public int TimeSecs { get; set; }
        /// <summary>
        /// Danh sách vật phẩm sẽ ROLLL
        /// </summary>
        [ProtoMember(3)]
        [XmlElement(ElementName = "RollAwardItem")]
        public List<AwardItem> RollAwardItem { get; set; }
    }

    [ProtoContract]
    [XmlRoot(ElementName = "EveryDayOnLineEvent")]
    public class EveryDayOnLineEvent
    {
        /// <summary>
        /// Tên sự kiện
        /// </summary>
        [ProtoMember(1)]
        [XmlAttribute(AttributeName = "Name")]
        public string Name { get; set; }

        /// <summary>
        /// Sự kiện có đang mở hay không
        /// </summary>
        [ProtoMember(2)]
        [XmlAttribute(AttributeName = "IsOpen")]
        public bool IsOpen { get; set; }

        /// <summary>
        /// Danh sách vật phẩm
        /// </summary>
        [ProtoMember(3)]
        [XmlElement(ElementName = "Item")]
        public List<EveryDayOnLine> Item { get; set; }

        /// <summary>
        /// Thằng này đã onlilne được bao nhiêu giây rồi
        /// </summary>
        [ProtoMember(4)]
        [XmlIgnore]
        public int DayOnlineSecond { get; set; }
        /// <summary>
        /// Đã nhận mốc nào rồi
        /// </summary>
        [ProtoMember(5)]
        [XmlIgnore]
        public int EveryDayOnLineAwardStep { get; set; }
        /// <summary>
        /// Item đã nhận trước đó là gì
        /// </summary>
        [ProtoMember(6)]
        [XmlIgnore]
        public string EveryDayOnLineAwardGoodsID { get; set; }

    }
}
