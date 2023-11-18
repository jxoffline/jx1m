using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace GameServer.KiemThe.Core.BulletinManager
{
    [XmlRoot(ElementName = "NotifyTime")]
    public class NotifyTime
    {
        [XmlAttribute(AttributeName = "Hours")]
        public int Hours { get; set; }

        [XmlAttribute(AttributeName = "Minute")]
        public int Minute { get; set; }

        public bool IsPushNotify { get; set; } = false;
    }

    [XmlRoot(ElementName = "BulletinMsg")]
    public class BulletinMsg
    {
        /// <summary>
        /// ID Bulletin
        /// </summary>
        ///
        [XmlAttribute(AttributeName = "ID")]
        public int ID { get; set; }

        [XmlAttribute(AttributeName = "ServerID")]
        public int ServerID { get; set; }

        [XmlAttribute(AttributeName = "Messenger")]
        public string Messenger { get; set; }

        [XmlElement(ElementName = "DayOfWeek")]
        public List<int> DayOfWeek { get; set; }

        [XmlElement(ElementName = "NotifyTimes")]
        public List<NotifyTime> NotifyTimes { get; set; }

      
    }
}
