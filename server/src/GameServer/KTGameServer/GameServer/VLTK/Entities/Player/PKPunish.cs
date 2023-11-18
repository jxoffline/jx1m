using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace GameServer.KiemThe.Entities.Player
{

    [XmlRoot(ElementName = "PKPunish")]
    public class PKPunish
    {
        [XmlElement(ElementName = "PKValue")]
        public int PKValue { get; set; }

        [XmlElement(ElementName = "ExpDropRate")]
        public int ExpDropRate { get; set; }

        [XmlElement(ElementName = "ExpDropULimit")]
        public int ExpDropULimit { get; set; }

        [XmlElement(ElementName = "EquipAbradeRate")]
        public int EquipAbradeRate { get; set; }

    }
}
