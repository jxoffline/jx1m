using System.Collections.Generic;
using System.Xml.Serialization;

namespace GameServer.KiemThe.Core.Item
{
    [XmlRoot(ElementName = "SuiteActive")]
    public class SuiteActive
    {
        [XmlAttribute(AttributeName = "Index")]
        public int Index { get; set; }

        [XmlAttribute(AttributeName = "RequestNum")]
        public int RequestNum { get; set; }

        [XmlAttribute(AttributeName = "SuiteName")]
        public string SuiteName { get; set; }

        [XmlAttribute(AttributeName = "SuiteMAPA1")]
        public int SuiteMAPA1 { get; set; }

        [XmlAttribute(AttributeName = "SuiteMAPA2")]
        public int SuiteMAPA2 { get; set; }

        [XmlAttribute(AttributeName = "SuiteMAPA3")]
        public int SuiteMAPA3 { get; set; }
    }

    [XmlRoot(ElementName = "SuiteActiveProp")]
    public class SuiteActiveProp
    {
        [XmlAttribute(AttributeName = "SuiteID")]
        public int SuiteID { get; set; }

        [XmlAttribute(AttributeName = "Name")]
        public string Name { get; set; }

        [XmlElement(ElementName = "ListActive")]
        public List<SuiteActive> ListActive { get; set; }

        [XmlAttribute(AttributeName = "Head")]
        public string Head { get; set; }

        [XmlAttribute(AttributeName = "Body")]
        public string Body { get; set; }

        [XmlAttribute(AttributeName = "Belt")]
        public string Belt { get; set; }

        [XmlAttribute(AttributeName = "Weapon")]
        public string Weapon { get; set; }

        [XmlAttribute(AttributeName = "Foot")]
        public string Foot
        { get; set; }

        [XmlAttribute(AttributeName = "Cuff")]
        public string Cuff

        { get; set; }

        [XmlAttribute(AttributeName = "Amulet")]
        public string Amulet

        { get; set; }

        [XmlAttribute(AttributeName = "Ring")]
        public string Ring

        { get; set; }

        [XmlAttribute(AttributeName = "Necklace")]
        public string Necklace

        { get; set; }

        [XmlAttribute(AttributeName = "Pendant")]
        public string Pendant

        { get; set; }
    }
}