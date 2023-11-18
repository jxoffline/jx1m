using System.Collections.Generic;
using System.Xml.Linq;
using System.Xml.Serialization;
using static FS.VLTK.Entities.Enum;

namespace FS.VLTK.Entities.Config
{
    [XmlRoot(ElementName = "ItemValueCaculation")]
    public class ItemValueCaculation
    {
        [XmlElement(ElementName = "Magic_Combine_Def")]
        public Magic_Combine Magic_Combine_Def { get; set; }

        [XmlElement(ElementName = "List_Equip_Type_Rate")]
        public List<Equip_Type_Rate> List_Equip_Type_Rate { get; set; }

        [XmlElement(ElementName = "List_Enhance_Value")]
        public List<Enhance_Value> List_Enhance_Value { get; set; }

        [XmlElement(ElementName = "List_Strengthen_Value")]
        public List<Strengthen_Value> List_Strengthen_Value { get; set; }

        [XmlElement(ElementName = "List_Equip_StarLevel")]
        public List<Equip_StarLevel> List_Equip_StarLevel { get; set; }

        [XmlElement(ElementName = "List_Equip_Random_Pos")]
        public List<Equip_Random_Pos> List_Equip_Random_Pos { get; set; }

        [XmlElement(ElementName = "List_Equip_Level")]
        public List<Equip_Level> List_Equip_Level { get; set; }


        [XmlElement(ElementName = "List_StarLevelStruct")]
        public List<StarLevelStruct> List_StarLevelStruct { get; set; }

        /// <summary>
        /// Chuyển đối tượng từ XML Node
        /// </summary>
        /// <param name="xmlNode"></param>
        /// <returns></returns>
        public static ItemValueCaculation Parse(XElement xmlNode)
        {
            ItemValueCaculation itemValueCaculation = new ItemValueCaculation()
            {
                List_Enhance_Value = new List<Enhance_Value>(),
                List_Equip_Level = new List<Equip_Level>(),
                List_Equip_Random_Pos = new List<Equip_Random_Pos>(),
                List_Equip_StarLevel = new List<Equip_StarLevel>(),
                List_Equip_Type_Rate = new List<Equip_Type_Rate>(),
                List_StarLevelStruct = new List<StarLevelStruct>(),
                List_Strengthen_Value = new List<Strengthen_Value>(),
                Magic_Combine_Def = Magic_Combine.Parse(xmlNode.Element("Magic_Combine_Def")),
            };

            foreach (XElement node in xmlNode.Elements("List_Enhance_Value"))
            {
                itemValueCaculation.List_Enhance_Value.Add(Enhance_Value.Parse(node));
            }

            foreach (XElement node in xmlNode.Elements("List_Equip_Level"))
            {
                itemValueCaculation.List_Equip_Level.Add(Equip_Level.Parse(node));
            }

            foreach (XElement node in xmlNode.Elements("List_Equip_Random_Pos"))
            {
                itemValueCaculation.List_Equip_Random_Pos.Add(Equip_Random_Pos.Parse(node));
            }

            foreach (XElement node in xmlNode.Elements("List_Equip_StarLevel"))
            {
                itemValueCaculation.List_Equip_StarLevel.Add(Equip_StarLevel.Parse(node));
            }

            foreach (XElement node in xmlNode.Elements("List_Equip_Type_Rate"))
            {
                itemValueCaculation.List_Equip_Type_Rate.Add(Equip_Type_Rate.Parse(node));
            }



            foreach (XElement node in xmlNode.Elements("List_StarLevelStruct"))
            {
                itemValueCaculation.List_StarLevelStruct.Add(StarLevelStruct.Parse(node));
            }

            foreach (XElement node in xmlNode.Elements("List_Strengthen_Value"))
            {
                itemValueCaculation.List_Strengthen_Value.Add(Strengthen_Value.Parse(node));
            }

            return itemValueCaculation;
        }

    }

    [XmlRoot(ElementName = "MagicSource")]

    public class MagicSource
    {
        [XmlAttribute(AttributeName = "MagicName")]
        public string MagicName { get; set; }

        [XmlAttribute(AttributeName = "Index")]
        public int Index { get; set; }

        /// <summary>
        /// Chuyển đối tượng từ XML Node
        /// </summary>
        /// <param name="xmlNode"></param>
        /// <returns></returns>
        public static MagicSource Parse(XElement xmlNode)
        {
            return new MagicSource()
            {
                MagicName = xmlNode.Attribute("MagicName").Value,
                Index = int.Parse(xmlNode.Attribute("Index").Value),
            };
        }

    }
    [XmlRoot(ElementName = "StarLevelStruct")]
    public class StarLevelStruct
    {
        [XmlAttribute(AttributeName = "Value")]
        public long Value { get; set; }
        [XmlAttribute(AttributeName = "StarLevel")]
        public int StarLevel { get; set; }
        [XmlAttribute(AttributeName = "NameColor")]

        public string NameColor { get; set; }
        [XmlAttribute(AttributeName = "EmptyStar")]
        public int EmptyStar { get; set; }
        [XmlAttribute(AttributeName = "FillStar")]
        public int FillStar { get; set; }

        /// <summary>
        /// Chuyển đối tượng từ XML Node
        /// </summary>
        /// <param name="xmlNode"></param>
        /// <returns></returns>
        public static StarLevelStruct Parse(XElement xmlNode)
        {
            return new StarLevelStruct()
            {
                Value = long.Parse(xmlNode.Attribute("Value").Value),
                StarLevel = int.Parse(xmlNode.Attribute("StarLevel").Value),
                EmptyStar = int.Parse(xmlNode.Attribute("EmptyStar").Value),
                FillStar = int.Parse(xmlNode.Attribute("FillStar").Value),
                NameColor = xmlNode.Attribute("NameColor").Value,
            };
        }
    }




    [XmlRoot(ElementName = "MagicDesc")]
    public class MagicDesc
    {
        [XmlAttribute(AttributeName = "MagicName")]
        public string MagicName { get; set; }

        [XmlAttribute(AttributeName = "ListValue")]
        public List<int> ListValue { get; set; }

        /// <summary>
        /// Chuyển đối tượng từ XML Node
        /// </summary>
        /// <param name="xmlNode"></param>
        /// <returns></returns>
        public static MagicDesc Parse(XElement xmlNode)
        {
            string[] listValueString = xmlNode.Attribute("ListValue").Value.Split(' ');
            List<int> listValue = new List<int>();
            foreach (string valueString in listValueString)
            {
                listValue.Add(int.Parse(valueString));
            }
            return new MagicDesc()
            {
                MagicName = xmlNode.Attribute("MagicName").Value,
                ListValue = listValue,
            };
        }
    }


    [XmlRoot(ElementName = "Magic_Combine")]
    public class Magic_Combine
    {
        [XmlElement(ElementName = "MagicSourceDef")]
        public List<MagicSource> MagicSourceDef { get; set; }

        [XmlElement(ElementName = "MagicDescDef")]
        public List<MagicDesc> MagicDescDef { get; set; }

        /// <summary>
        /// Chuyển đối tượng từ XML Node
        /// </summary>
        /// <param name="xmlNode"></param>
        /// <returns></returns>
        public static Magic_Combine Parse(XElement xmlNode)
        {
            Magic_Combine magicCombine = new Magic_Combine()
            {
                MagicDescDef = new List<MagicDesc>(),
                MagicSourceDef = new List<MagicSource>(),
            };

            foreach (XElement node in xmlNode.Elements("MagicSourceDef"))
            {
                magicCombine.MagicSourceDef.Add(MagicSource.Parse(node));
            }

            foreach (XElement node in xmlNode.Elements("MagicDescDef"))
            {
                magicCombine.MagicDescDef.Add(MagicDesc.Parse(node));
            }

            return magicCombine;
        }
    }
    [XmlRoot(ElementName = "Equip_Type_Rate")]
    public class Equip_Type_Rate
    {
        [XmlAttribute(AttributeName = "EquipType")]
        public KE_ITEM_EQUIP_DETAILTYPE EquipType { get; set; }

        [XmlAttribute(AttributeName = "Value")]
        public int Value { get; set; }

        /// <summary>
        /// Chuyển đối tượng từ XML Node
        /// </summary>
        /// <param name="xmlNode"></param>
        /// <returns></returns>
        public static Equip_Type_Rate Parse(XElement xmlNode)
        {
            string Rate = xmlNode.Attribute("EquipType").Value;

            return new Equip_Type_Rate()
            {
                EquipType = (KE_ITEM_EQUIP_DETAILTYPE)System.Enum.Parse(typeof(KE_ITEM_EQUIP_DETAILTYPE), xmlNode.Attribute("EquipType").Value),
                Value = int.Parse(xmlNode.Attribute("Value").Value),
            };
        }
    }
    [XmlRoot(ElementName = "Enhance_Value")]
    public class Enhance_Value
    {
        [XmlAttribute(AttributeName = "EnhanceTimes")]
        public int EnhanceTimes { get; set; }

        [XmlAttribute(AttributeName = "Value")]
        public int Value { get; set; }

        /// <summary>
        /// Chuyển đối tượng từ XML Node
        /// </summary>
        /// <param name="xmlNode"></param>
        /// <returns></returns>
        public static Enhance_Value Parse(XElement xmlNode)
        {
            return new Enhance_Value()
            {
                EnhanceTimes = int.Parse(xmlNode.Attribute("EnhanceTimes").Value),
                Value = int.Parse(xmlNode.Attribute("Value").Value),
            };
        }
    }
    [XmlRoot(ElementName = "Equip_StarLevel")]
    public class Equip_StarLevel
    {
        [XmlAttribute(AttributeName = "EQUIP_DETAIL_TYPE")]
        public int EQUIP_DETAIL_TYPE { get; set; }
        [XmlAttribute(AttributeName = "STAR_LEVEL")]
        public int STAR_LEVEL { get; set; }
        [XmlAttribute(AttributeName = "EQUIP_LEVEL_1")]
        public long EQUIP_LEVEL_1 { get; set; }

        [XmlAttribute(AttributeName = "EQUIP_LEVEL_2")]
        public long EQUIP_LEVEL_2 { get; set; }

        [XmlAttribute(AttributeName = "EQUIP_LEVEL_3")]
        public long EQUIP_LEVEL_3 { get; set; }

        [XmlAttribute(AttributeName = "EQUIP_LEVEL_4")]
        public long EQUIP_LEVEL_4 { get; set; }

        [XmlAttribute(AttributeName = "EQUIP_LEVEL_5")]
        public long EQUIP_LEVEL_5 { get; set; }

        [XmlAttribute(AttributeName = "EQUIP_LEVEL_6")]
        public long EQUIP_LEVEL_6 { get; set; }

        [XmlAttribute(AttributeName = "EQUIP_LEVEL_7")]
        public long EQUIP_LEVEL_7 { get; set; }

        [XmlAttribute(AttributeName = "EQUIP_LEVEL_8")]
        public long EQUIP_LEVEL_8 { get; set; }

        [XmlAttribute(AttributeName = "EQUIP_LEVEL_9")]
        public long EQUIP_LEVEL_9 { get; set; }

        [XmlAttribute(AttributeName = "EQUIP_LEVEL_10")]
        public long EQUIP_LEVEL_10 { get; set; }

        /// <summary>
        /// Chuyển đối tượng từ XML Node
        /// </summary>
        /// <param name="xmlNode"></param>
        /// <returns></returns>
        public static Equip_StarLevel Parse(XElement xmlNode)
        {
            return new Equip_StarLevel()
            {
                EQUIP_DETAIL_TYPE = int.Parse(xmlNode.Attribute("EQUIP_DETAIL_TYPE").Value),
                STAR_LEVEL = int.Parse(xmlNode.Attribute("STAR_LEVEL").Value),
                EQUIP_LEVEL_1 = long.Parse(xmlNode.Attribute("EQUIP_LEVEL_1").Value),
                EQUIP_LEVEL_2 = long.Parse(xmlNode.Attribute("EQUIP_LEVEL_2").Value),
                EQUIP_LEVEL_3 = long.Parse(xmlNode.Attribute("EQUIP_LEVEL_3").Value),
                EQUIP_LEVEL_4 = long.Parse(xmlNode.Attribute("EQUIP_LEVEL_4").Value),
                EQUIP_LEVEL_5 = long.Parse(xmlNode.Attribute("EQUIP_LEVEL_5").Value),
                EQUIP_LEVEL_6 = long.Parse(xmlNode.Attribute("EQUIP_LEVEL_6").Value),
                EQUIP_LEVEL_7 = long.Parse(xmlNode.Attribute("EQUIP_LEVEL_7").Value),
                EQUIP_LEVEL_8 = long.Parse(xmlNode.Attribute("EQUIP_LEVEL_8").Value),
                EQUIP_LEVEL_9 = long.Parse(xmlNode.Attribute("EQUIP_LEVEL_9").Value),
                EQUIP_LEVEL_10 = long.Parse(xmlNode.Attribute("EQUIP_LEVEL_10").Value),
            };
        }
    }

    [XmlRoot(ElementName = "Equip_Random_Pos")]
    public class Equip_Random_Pos
    {
        [XmlAttribute(AttributeName = "MAGIC_POS")]
        public int MAGIC_POS { get; set; }
        [XmlAttribute(AttributeName = "Value")]
        public int Value { get; set; }

        /// <summary>
        /// Chuyển đối tượng từ XML Node
        /// </summary>
        /// <param name="xmlNode"></param>
        /// <returns></returns>
        public static Equip_Random_Pos Parse(XElement xmlNode)
        {
            return new Equip_Random_Pos()
            {
                MAGIC_POS = int.Parse(xmlNode.Attribute("MAGIC_POS").Value),
                Value = int.Parse(xmlNode.Attribute("Value").Value),
            };
        }
    }
    [XmlRoot(ElementName = "Strengthen_Value")]
    public class Strengthen_Value
    {
        [XmlAttribute(AttributeName = "StrengthenTimes")]
        public int StrengthenTimes { get; set; }
        [XmlAttribute(AttributeName = "Value")]
        public int Value { get; set; }

        /// <summary>
        /// Chuyển đối tượng từ XML Node
        /// </summary>
        /// <param name="xmlNode"></param>
        /// <returns></returns>
        public static Strengthen_Value Parse(XElement xmlNode)
        {
            return new Strengthen_Value()
            {
                StrengthenTimes = int.Parse(xmlNode.Attribute("StrengthenTimes").Value),
                Value = int.Parse(xmlNode.Attribute("Value").Value),
            };
        }
    }
    [XmlRoot(ElementName = "Equip_Level")]
    public class Equip_Level
    {
        [XmlAttribute(AttributeName = "Level")]
        public int Level { get; set; }
        [XmlAttribute(AttributeName = "Value")]
        public int Value { get; set; }

        /// <summary>
        /// Chuyển đối tượng từ XML Node
        /// </summary>
        /// <param name="xmlNode"></param>
        /// <returns></returns>
        public static Equip_Level Parse(XElement xmlNode)
        {
            return new Equip_Level()
            {
                Level = int.Parse(xmlNode.Attribute("Level").Value),
                Value = int.Parse(xmlNode.Attribute("Value").Value),
            };
        }

        /// <summary>
        /// Đối tượng lưu các thuộc tính tính toán liên quan đến cường hóa
        /// </summary>
        public class CalcProb
        {
            /// <summary>
            /// Tỷ lệ tối đa 100%
            /// </summary>
            public double nProb { get; set; }
            /// <summary>
            /// Số bạc mất
            /// </summary>
            public long nMoney { get; set; }
            /// <summary>
            /// Tỷ lệ thực dựa vào số Huyền Tinh đặt vào
            /// </summary>
            public double nTrueProb { get; set; }
        }

        /// <summary>
        /// Sản phẩm ghép Huyền Tinh
        /// </summary>
        public class ComposeItem
        {
            /// <summary>
            /// Huyền Tinh số 1
            /// </summary>
            public ItemData nItemMinLevel { get; set; }
            /// <summary>
            /// Tỷ lệ ra Huyền Tinh số 1
            /// </summary>
            public int nMinLevelRate { get; set; }

            /// <summary>
            /// Huyền Tinh số 2
            /// </summary>
            public ItemData nItemMaxLevel { get; set; }
            /// <summary>
            /// Tỷ lệ ra Huyền Tinh số 2
            /// </summary>
            public int nMaxLevelRate { get; set; }

            /// <summary>
            /// Phí mất
            /// </summary>
            public int nFee { get; set; }
        }
    }
}
