using System.Collections.Generic;
using System.Xml.Serialization;

namespace GameServer.KiemThe.Core.Repute
{
 

    [XmlRoot(ElementName = "level")]
    public class Level
    {
        [XmlAttribute(AttributeName = "id")]
        public int Id { get; set; }

        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }

        [XmlAttribute(AttributeName = "level_up")]
        public int LevelUp { get; set; }

        [XmlAttribute(AttributeName = "require_level")]
        public int RequireLevel { get; set; }
    }

    [XmlRoot(ElementName = "class")]
    public class Class
    {
        [XmlElement(ElementName = "level")]
        public List<Level> Level { get; set; }

        [XmlAttribute(AttributeName = "id")]
        public int Id { get; set; }

        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }

        [XmlAttribute(AttributeName = "default_level")]
        public int DefaultLevel { get; set; }

        [XmlAttribute(AttributeName = "daily_limit")]
        public int DailyLimit { get; set; }

        [XmlAttribute(AttributeName = "tips")]
        public string Tips { get; set; }
    }

    [XmlRoot(ElementName = "camp")]
    public class Camp
    {
        [XmlElement(ElementName = "class")]
        public List<Class> Class { get; set; }

        [XmlAttribute(AttributeName = "id")]
        public int Id { get; set; }

        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
    }

    [XmlRoot(ElementName = "repute")]
    public class Repute
    {
        [XmlElement(ElementName = "camp")]
        public List<Camp> Camp { get; set; }
    }
}