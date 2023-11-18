using ProtoBuf;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Server.Data
{
    [XmlRoot(ElementName = "Card")]
    [ProtoContract]
    public class Card
    {
        [ProtoMember(1)]
        [XmlAttribute(AttributeName = "Day")]
        public int Day { get; set; }

        [ProtoMember(2)]
        [XmlAttribute(AttributeName = "ItemCard")]
        public string ItemCard { get; set; }
    }

    [ProtoContract]
    [XmlRoot(ElementName = "ConfigCard")]
    public class ConfigCard
    {
        [ProtoMember(1)]
        [XmlElement(ElementName = "Card")]
        public List<Card> Card { get; set; }
    }

    [ProtoContract]
    public class YueKaData
    {
        /// <summary>
        /// Có thẻ tháng hay không
        /// </summary>
        [ProtoMember(1)]
        public bool HasYueKa;

        /// <summary>
        ///  Ngày thứ bao nhiêu
        /// </summary>
        [ProtoMember(2)]
        public int CurrDay;

        /// <summary>
        /// Đánh dấu đã nhận những ngày nào rồi | Cơ chế mã hóa cần check lại ở client
        /// </summary>
        [ProtoMember(3)]
        public string AwardInfo;

        /// <summary>
        /// Còn lại bao nhiêu ngày
        /// </summary>
        [ProtoMember(4)]
        public int RemainDay;

        /// <summary>
        /// Config của nó
        /// </summary>
        [ProtoMember(5)]
        public ConfigCard Config;

        /// <summary>
        /// Số đồng khóa nhận thheme
        /// </summary>
        [ProtoMember(6)]
        public int BoundToken;

        /// <summary>
        /// Tiêu chí bán hàng
        /// </summary>
        [ProtoMember(7)]
        public string Slogan;

        public YueKaData()
        {
            HasYueKa = false;
            CurrDay = 0;
            AwardInfo = "";
            RemainDay = 0;
        }
    }
}