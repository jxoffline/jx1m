using GameServer.KiemThe.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace GameServer.KiemThe.Entities
{
    /// <summary>
    /// Đối tượng biểu diễn kỹ năng hỗ trợ kỹ năng khác
    /// </summary>
    public class EnchantSkill
    {
        /// <summary>
        /// Lớp định nghĩa các thuộc tính của kỹ năng
        /// </summary>
        public class EnchantSkillAttribute
        {
            /// <summary>
            /// Symbol
            /// </summary>
            public class Symbol
            {
                /// <summary>
                /// Giá trị theo cấp độ
                /// </summary>
                public class ValueByLevel
                {
                    /// <summary>
                    /// Cấp độ
                    /// </summary>
                    public int Level { get; set; }

                    /// <summary>
                    /// Giá trị
                    /// </summary>
                    public int Value { get; set; }

                    /// <summary>
                    /// Loại
                    /// <para>1: Cộng</para>
                    /// <para>2: Nhân</para>
                    /// <para>3: Thiết lập</para>
                    /// </summary>
                    public int Type { get; set; }
                }

                /// <summary>
                /// Tên
                /// </summary>
                public string Name { get; set; }

                /// <summary>
                /// Danh sách giá trị
                /// </summary>
                public Dictionary<int, Dictionary<int, ValueByLevel>> Values { get; set; }
            }

            /// <summary>
            /// ID kỹ năng
            /// </summary>
            public int ID { get; set; }

            /// <summary>
            /// Danh sách thuộc hỗ trợ
            /// </summary>
            public Dictionary<string, Symbol> Symbols { get; set; }

            /// <summary>
            /// Chuyển đối tượng từ XML Node
            /// </summary>
            /// <param name="xmlNode"></param>
            /// <returns></returns>
            public static EnchantSkillAttribute Parse(XElement xmlNode)
            {
                EnchantSkillAttribute configAttribute = new EnchantSkillAttribute()
                {
                    ID = int.Parse(xmlNode.Attribute("ID").Value),
                    Symbols = new Dictionary<string, Symbol>(),
                };

                foreach (XElement symbolNode in xmlNode.Elements("Symbol"))
                {
                    Symbol symbol = new Symbol()
                    {
                        Name = symbolNode.Attribute("Name").Value,
                        Values = new Dictionary<int, Dictionary<int, Symbol.ValueByLevel>>(),
                    };
                    configAttribute.Symbols[symbol.Name] = symbol;

                    foreach (XElement valueNode in symbolNode.Elements("Value"))
                    {
                        int valueID = int.Parse(valueNode.Attribute("ID").Value);
                        int type = int.Parse(valueNode.Attribute("Type").Value);
                        symbol.Values[valueID] = new Dictionary<int, Symbol.ValueByLevel>();

                        foreach (XElement valueByLevelNode in valueNode.Elements("SkillLevelValue"))
                        {
                            Symbol.ValueByLevel valueByLevel = new Symbol.ValueByLevel()
                            {
                                Level = int.Parse(valueByLevelNode.Attribute("Level").Value),
                                Value = int.Parse(valueByLevelNode.Attribute("Value").Value),
                                Type = type,
                            };
                            symbol.Values[valueID][valueByLevel.Level] = valueByLevel;
                        }

                        List<Symbol.ValueByLevel> childLevels = EnchantSkillAttribute.MakeChildLevel(symbol.Values[valueID].Values.ToList());
                        foreach (Symbol.ValueByLevel valueByLevel in childLevels)
                        {
                            symbol.Values[valueID][valueByLevel.Level] = valueByLevel;
                        }
                    }
                }

                return configAttribute;
            }

            /// <summary>
            /// Tạo danh sách giá trị các cấp độ con
            /// </summary>
            /// <param name="array"></param>
            /// <returns></returns>
            private static List<Symbol.ValueByLevel> MakeChildLevel(List<Symbol.ValueByLevel> array)
            {
                List<Symbol.ValueByLevel> list = new List<Symbol.ValueByLevel>();

                array.Sort((o1, o2) =>
                {
                    return o1.Level - o2.Level;
                });

                list.Add(array[0]);

                float lastDelta = 0;
                for (int i = 1; i < array.Count; i++)
                {
                    int dLevel = array[i].Level - array[i - 1].Level;
                    int dValue = array[i].Value - array[i - 1].Value;
                    float delta = dValue / dLevel;
                    for (int j = array[i - 1].Level + 1; j < array[i].Level; j++)
                    {
                        list.Add(new Symbol.ValueByLevel()
                        {
                            Level = j,
                            Value = (int)(array[i - 1].Value + (j - array[i - 1].Level) * delta),
                            Type = array[0].Type,
                        });
                    }
                    list.Add(array[i]);

                    lastDelta = Math.Max(lastDelta, delta);
                }

                for (int j = array[array.Count - 1].Level + 1; j <= SkillDataEx.SystemMaxLevel; j++)
                {
                    list.Add(new Symbol.ValueByLevel()
                    {
                        Level = j,
                        Value = (int)(array[array.Count - 1].Value + (j - array[array.Count - 1].Level) * lastDelta),
                        Type = array[0].Type,
                    });
                }

                return list;
            }
        }

        /// <summary>
        /// Kỹ năng được hỗ trợ
        /// </summary>
        public class RelatedSkill
        {
            /// <summary>
            /// ID kỹ năng được hỗ trợ
            /// </summary>
            public int ID { get; set; }

            /// <summary>
            /// Danh sách thuộc tính bổ trợ theo cấp
            /// </summary>
            public Dictionary<int, PropertyDictionary> Properties { get; set; }

            /// <summary>
            /// Chuyển đối tượng từ XML Node
            /// </summary>
            /// <param name="xmlNode"></param>
            /// <returns></returns>
            public static RelatedSkill Parse(XElement xmlNode)
            {
                RelatedSkill relatedSkill = new RelatedSkill()
                {
                    ID = int.Parse(xmlNode.Attribute("ID").Value),
                    Properties = new Dictionary<int, PropertyDictionary>(),
                };

                for (int i = 1; i <= SkillDataEx.SystemMaxLevel; i++)
                {
                    relatedSkill.Properties[i] = new PropertyDictionary();
                }

                EnchantSkillAttribute attributes = EnchantSkillAttribute.Parse(xmlNode);
                if (attributes != null)
                {
                    foreach (KeyValuePair<string, EnchantSkillAttribute.Symbol> pair in attributes.Symbols)
                    {
                        string propertySymbol = pair.Key;
                        EnchantSkillAttribute.Symbol propertyValue = pair.Value;

                        if (PropertyDefine.PropertiesBySymbolName.TryGetValue(propertySymbol, out PropertyDefine.Property property))
                        {
                            for (int i = 1; i <= SkillDataEx.SystemMaxLevel; i++)
                            {
                                KMagicAttrib kMagicAttrib = new KMagicAttrib();
                                kMagicAttrib.nAttribType = (MAGIC_ATTRIB)property.ID;
                                kMagicAttrib.nValue = new int[] { 0, 0, 0 };
                                kMagicAttrib.nType = new int[] { 0, 0, 0 };

                                if (propertyValue.Values.TryGetValue(1, out Dictionary<int, EnchantSkillAttribute.Symbol.ValueByLevel> values1))
                                {
                                    if (values1.TryGetValue(i, out EnchantSkillAttribute.Symbol.ValueByLevel value))
                                    {
                                        kMagicAttrib.nValue[0] = value.Value;
                                        kMagicAttrib.nType[0] = value.Type;
                                    }
                                }

                                if (propertyValue.Values.TryGetValue(2, out Dictionary<int, EnchantSkillAttribute.Symbol.ValueByLevel> values2))
                                {
                                    if (values2.TryGetValue(i, out EnchantSkillAttribute.Symbol.ValueByLevel value))
                                    {
                                        kMagicAttrib.nValue[1] = value.Value;
                                        kMagicAttrib.nType[1] = value.Type;
                                    }
                                }

                                if (propertyValue.Values.TryGetValue(3, out Dictionary<int, EnchantSkillAttribute.Symbol.ValueByLevel> values3))
                                {
                                    if (values3.TryGetValue(i, out EnchantSkillAttribute.Symbol.ValueByLevel value))
                                    {
                                        kMagicAttrib.nValue[2] = value.Value;
                                        kMagicAttrib.nType[2] = value.Type;
                                    }
                                }

                                relatedSkill.Properties[i].Set<KMagicAttrib>(property.ID, kMagicAttrib);
                            }
                        }
                    }
                }

                return relatedSkill;
            }
        }

        /// <summary>
        /// ID nhóm bổ trợ
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// Tên nhóm bổ trợ
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Danh sách kỹ năng được bổ trợ
        /// </summary>
        public Dictionary<int, RelatedSkill> RelatedSkills { get; set; }

        /// <summary>
        /// Chuyển đối tượng từ XML Node
        /// </summary>
        /// <param name="xmlNode"></param>
        /// <returns></returns>
        public static EnchantSkill Parse(XElement xmlNode)
        {
            EnchantSkill enchantSkill = new EnchantSkill()
            {
                ID = int.Parse(xmlNode.Attribute("ID").Value),
                Name = xmlNode.Attribute("Name").Value,
                RelatedSkills = new Dictionary<int, RelatedSkill>(),
            };

            foreach (XElement node in xmlNode.Elements("RelatedSkill"))
            {
                RelatedSkill relatedSkill = RelatedSkill.Parse(node);
                enchantSkill.RelatedSkills[relatedSkill.ID] = relatedSkill;
            }

            return enchantSkill;
        }
    }
}
