using ProtoBuf;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace GameServer.KiemThe.Core.Activity.LevelUpEvent
{
    [XmlRoot(ElementName = "LevelUpItem")]
    [ProtoContract]
    public class LevelUpItem
    {
        [XmlAttribute(AttributeName = "ID")]
        [ProtoMember(1)]
        public int ID { get; set; }

        [XmlAttribute(AttributeName = "ToLevel")]
        [ProtoMember(2)]
        public int ToLevel { get; set; }

        [XmlAttribute(AttributeName = "LevelUpGift")]
        [ProtoMember(3)]
        public string LevelUpGift { get; set; }
    }

    [XmlRoot(ElementName = "LevelUpGiftConfig")]
    [ProtoContract]
    public class LevelUpGiftConfig
    {

        [XmlElement(ElementName = "LevelUpItem")]
        [ProtoMember(1)]
        public List<LevelUpItem> LevelUpItem { get; set; }

        /// <summary>
        /// 1_x|2_x|3_x|4_x
        /// Chuỗi BitFlags gửi về client quy ước như sau
        /// 1 2 3 4 lần lượt là các mốc ID trả về theo LevelUpItem
        /// x là trạng thái |2 là đã nhận |  1 là chưa thỏa mãn yêu cầu | 0 là có thể nhận
        /// Cốt nào mà có thể nhận thì thay đổi trạng thái nút để người chơi biết là có thể nhận quà này
        /// </summary>
        [ProtoMember(2)]
        public string BitFlags { get; set; }
    }
}
