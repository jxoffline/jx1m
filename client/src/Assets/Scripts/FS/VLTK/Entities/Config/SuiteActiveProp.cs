using System.Collections.Generic;
using System.Xml.Serialization;

namespace FS.VLTK.Entities.Config
{
    /// <summary>
    /// Kích hoạt theo bộ
    /// </summary>
    [XmlRoot(ElementName = "SuiteActive")]
    public class SuiteActive
    {
        [XmlAttribute(AttributeName = "Index")]
        public short Index { get; set; }

        [XmlAttribute(AttributeName = "RequestNum")]
        public short RequestNum { get; set; }
        /// <summary>
        /// Tên bộ
        /// </summary>
        [XmlAttribute(AttributeName = "SuiteName")]
        public string SuiteName { get; set; }

        /// <summary>
        /// Thuộc tính 1
        /// </summary>
        [XmlAttribute(AttributeName = "SuiteMAPA1")]
        public short SuiteMAPA1 { get; set; }

        /// <summary>
        /// Thuộc tính 2
        /// </summary>
        [XmlAttribute(AttributeName = "SuiteMAPA2")]
        public short SuiteMAPA2 { get; set; }

        /// <summary>
        /// Thuộc tính 3
        /// </summary>
        [XmlAttribute(AttributeName = "SuiteMAPA3")]
        public short SuiteMAPA3 { get; set; }
    }

    /// <summary>
    /// Thuộc tính kích hoạt bộ
    /// </summary>
    [XmlRoot(ElementName = "SuiteActiveProp")]
    public class SuiteActiveProp
    {
        /// <summary>
        /// ID bộ
        /// </summary>
        [XmlAttribute(AttributeName = "SuiteID")]
        public sbyte SuiteID { get; set; }

        /// <summary>
        /// Tên bộ
        /// </summary>
        [XmlAttribute(AttributeName = "Name")]
        public string Name { get; set; }

        /// <summary>
        /// Danh sách thuộc tính
        /// </summary>
        [XmlElement(ElementName = "ListActive")]
        public List<SuiteActive> ListActive { get; set; }

        /// <summary>
        /// Thuộc tính đầu
        /// </summary>
        [XmlAttribute(AttributeName = "Head")]
        public string Head { get; set; }

        /// <summary>
        /// Thuộc tính áo
        /// </summary>
        [XmlAttribute(AttributeName = "Body")]
        public string Body { get; set; }

        /// <summary>
        /// Thuộc tính lưng
        /// </summary>
        [XmlAttribute(AttributeName = "Belt")]
        public string Belt { get; set; }

        /// <summary>
        /// Thuộc tính vũ khí
        /// </summary>
        [XmlAttribute(AttributeName = "Weapon")]
        public string Weapon { get; set; }

        /// <summary>
        /// Thuộc tính giày
        /// </summary>
        [XmlAttribute(AttributeName = "Foot")]
        public string Foot { get; set; }

        /// <summary>
        /// Thuộc tính tay
        /// </summary>
        [XmlAttribute(AttributeName = "Cuff")]
        public string Cuff { get; set; }

        /// <summary>
        /// Thuộc tính phù
        /// </summary>
        [XmlAttribute(AttributeName = "Amulet")]
        public string Amulet { get; set; }

        /// <summary>
        /// Thuộc tính nhẫn
        /// </summary>
        [XmlAttribute(AttributeName = "Ring")]
        public string Ring { get; set; }

        /// <summary>
        /// Thuộc tính liên
        /// </summary>
        [XmlAttribute(AttributeName = "Necklace")]
        public string Necklace { get; set; }

        /// <summary>
        /// Thuộc tính bội
        /// </summary>
        [XmlAttribute(AttributeName = "Pendant")]
        public string Pendant { get; set; }
    }
}