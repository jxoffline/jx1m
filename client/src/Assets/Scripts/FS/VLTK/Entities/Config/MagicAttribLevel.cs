using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace FS.VLTK.Entities.Config
{
    [XmlRoot(ElementName = "MagicAttribLevel")]
    public class MagicAttribLevel
    {
        [XmlAttribute(AttributeName = "Name")]
        public string Name { get; set; }
        [XmlAttribute(AttributeName = "Suffix")]
        public int Suffix { get; set; }

        [XmlAttribute(AttributeName = "Level")]
        public int Level { get; set; }

        [XmlAttribute(AttributeName = "MAGIC_ID")]
        public int MAGIC_ID { get; set; }

        [XmlAttribute(AttributeName = "MagicName")]
        public string MagicName { get; set; }


        [XmlAttribute(AttributeName = "MA1Min")]
        public int MA1Min { get; set; }
        [XmlAttribute(AttributeName = "MA1Max")]
        public int MA1Max { get; set; }
        [XmlAttribute(AttributeName = "MA2Min")]
        public int MA2Min { get; set; }
        [XmlAttribute(AttributeName = "MA2Max")]
        public int MA2Max { get; set; }
        [XmlAttribute(AttributeName = "MA3Min")]
        public int MA3Min { get; set; }
        [XmlAttribute(AttributeName = "MA3Max")]
        public int MA3Max { get; set; }
        [XmlAttribute(AttributeName = "ReqLevel")]
        public double ReqLevel { get; set; }
        [XmlAttribute(AttributeName = "ItemValue")]
        public long ItemValue { get; set; }

        [XmlAttribute(AttributeName = "Series")]
        public int Series { get; set; }


        [XmlAttribute(AttributeName = "SWORD")]
        public long SWORD { get; set; }

        [XmlAttribute(AttributeName = "BLADE")]
        public long BLADE { get; set; }

        [XmlAttribute(AttributeName = "WAND")]
        public long WAND { get; set; }


        [XmlAttribute(AttributeName = "SPEAR")]
        public long SPEAR { get; set; }

        [XmlAttribute(AttributeName = "HAMMER")]
        public long HAMMER { get; set; }


        [XmlAttribute(AttributeName = "DUALBLADES")]
        public long DUALBLADES { get; set; }

        [XmlAttribute(AttributeName = "DARTS")]
        public long DARTS { get; set; }

        [XmlAttribute(AttributeName = "KNIFE")]
        public long KNIFE { get; set; }

        [XmlAttribute(AttributeName = "CROSSBOW")]
        public long CROSSBOW { get; set; }

        [XmlAttribute(AttributeName = "ARMOR")]
        public long ARMOR { get; set; }


        [XmlAttribute(AttributeName = "RING")]
        public long RING { get; set; }

        [XmlAttribute(AttributeName = "NECKLACE")]
        public long NECKLACE { get; set; }


        [XmlAttribute(AttributeName = "AMULET")]
        public long AMULET { get; set; }


        [XmlAttribute(AttributeName = "BOOT")]
        public long BOOT { get; set; }

        [XmlAttribute(AttributeName = "BELT")]
        public long BELT { get; set; }



        [XmlAttribute(AttributeName = "HELM")]
        public long HELM { get; set; }

        [XmlAttribute(AttributeName = "CUFF")]
        public long CUFF { get; set; }


        [XmlAttribute(AttributeName = "SACHET")]
        public long SACHET { get; set; }


        [XmlAttribute(AttributeName = "PENDANT")]
        public long PENDANT { get; set; }

        [XmlAttribute(AttributeName = "METAL")]
        public long METAL { get; set; }


        [XmlAttribute(AttributeName = "WOOD")]
        public long WOOD { get; set; }

        [XmlAttribute(AttributeName = "WATER")]
        public long WATER { get; set; }

        [XmlAttribute(AttributeName = "FIRE")]
        public long FIRE { get; set; }


        [XmlAttribute(AttributeName = "EARTH")]
        public long EARTH { get; set; }

        [XmlAttribute(AttributeName = "PET_HELM")]
        public long PET_HELM { get; set; }

        [XmlAttribute(AttributeName = "PET_ARMOR")]
        public long PET_ARMOR { get; set; }

        [XmlAttribute(AttributeName = "PET_AMULET")]
        public long PET_AMULET { get; set; }

        [XmlAttribute(AttributeName = "PET_BELT")]
        public long PET_BELT { get; set; }

        [XmlAttribute(AttributeName = "PET_WEAPON")]
        public long PET_WEAPON { get; set; }

        [XmlAttribute(AttributeName = "PET_BOOT")]
        public long PET_BOOT { get; set; }

        [XmlAttribute(AttributeName = "PET_CUFF")]
        public long PET_CUFF { get; set; }

        [XmlAttribute(AttributeName = "PET_NECK")]
        public long PET_NECK { get; set; }

        [XmlAttribute(AttributeName = "PET_RING")]
        public long PET_RING { get; set; }
    }
}
