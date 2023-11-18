using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Server.Data
{
    /// <summary>
    /// Vật phẩm thưởng khi tải lần đầu
    /// </summary>
    [ProtoContract]

    [XmlRoot(ElementName = "BonusItem")]
    public class BonusItem
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
    }

    /// <summary>
    /// Thông tin phần thưởng khi tải lần đầu
    /// </summary>
    [ProtoContract]
    [XmlRoot(ElementName = "BonusDownload")]
    public class BonusDownload
    {
        /// <summary>
        /// Có thể nhận không
        /// </summary>
        [ProtoMember(1)]
        [XmlAttribute(AttributeName = "CanRevice")]
        public bool CanRevice { get; set; }

        /// <summary>
        /// Danh sách vật phẩm thưởng
        /// </summary>
        [ProtoMember(2)]
        [XmlElement(ElementName = "BonusItems")]
        public List<BonusItem> BonusItems { get; set; }

    }
}
