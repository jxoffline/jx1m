using ProtoBuf;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace GameServer.VLTK.Core.Activity.CheckPoint
{

    [XmlRoot(ElementName = "CheckPointItem")]
    [ProtoContract]
    public class CheckPointItem
    {
        /// <summary>
        // Số thứ tự của ngày tính từ 1-30 không tính ngày 31
        /// </summary>
        [ProtoMember(1)]
        [XmlAttribute(AttributeName = "Day")]
        public int Day { get; set; }

        /// <summary>
        /// Vật phẩm
        /// </summary>
        [ProtoMember(2)]
        [XmlAttribute(AttributeName = "ItemCard")]
        public string ItemCard { get; set; }
    }

    [ProtoContract]
    [XmlRoot(ElementName = "ConfigCard")]
    public class CheckPontConfig
    {
        [ProtoMember(1)]
        [XmlElement(ElementName = "CheckPointItem")]
        public List<CheckPointItem> CheckPointItem { get; set; }

        [ProtoMember(2)]
        [XmlAttribute(AttributeName = "Name")]
        public string Name { get; set; }

        [ProtoMember(3)]
        [XmlAttribute(AttributeName = "IsOpen")]
        public bool IsOpen { get; set; }

        /// <summary>
        /// Lịch sử các ngày đã nhận của thằng này
        /// Còn các ngày còn lại không nằm trong chuỗi này nếu nhỏ hơn DAYID thì là quá hạn
        /// Cao hơn thì là chưa nhận
        /// = DayID là có thể nhận SIMPLE
        /// Cấu trúc 1_2_3_5 là đã nhận ngày 1 2 3 5
        /// </summary>
        [ProtoMember(4)]
        public string HistoryRevice { get; set; }


        /// <summary>
        /// Ngày hiện tại là ngày nào
        /// </summary>
        [ProtoMember(5)]
        public int DayID { get; set; }
    }
}
