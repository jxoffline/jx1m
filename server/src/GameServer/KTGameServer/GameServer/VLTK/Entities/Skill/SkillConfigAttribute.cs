using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace GameServer.KiemThe.Entities
{
    /// <summary>
    /// Lớp định nghĩa các thuộc tính của kỹ năng
    /// </summary>
    public class SkillConfigAttribute
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
                public float Value { get; set; }
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
        /// Tên thuộc tính
        /// </summary>
        public string PropertyName { get; set; }

        /// <summary>
        /// Danh sách thuộc tính con
        /// </summary>
        public Dictionary<string, Symbol> Symbols { get; set; }

        /// <summary>
        /// Chuyển đối tượng từ XML Node
        /// </summary>
        /// <param name="xmlNode"></param>
        /// <returns></returns>
        public static SkillConfigAttribute Parse(XElement xmlNode)
        {
            SkillConfigAttribute configAttribute = new SkillConfigAttribute()
            {
                PropertyName = xmlNode.Attribute("Name").Value,
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
                    symbol.Values[valueID] = new Dictionary<int, Symbol.ValueByLevel>();

                    foreach (XElement valueByLevelNode in valueNode.Elements("SkillLevelValue"))
                    {
                        Symbol.ValueByLevel valueByLevel = new Symbol.ValueByLevel()
                        {
                            Level = int.Parse(valueByLevelNode.Attribute("Level").Value),
                            Value = int.Parse(valueByLevelNode.Attribute("Value").Value),
                        };
                        symbol.Values[valueID][valueByLevel.Level] = valueByLevel;
                    }

                    List<Symbol.ValueByLevel> childLevels = SkillConfigAttribute.MakeChildLevel(symbol.Values[valueID].Values.ToList());
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
            /// Duyệt danh sách thiết lập
            for (int i = 1; i < array.Count; i++)
            {
                /// Độ lệch cấp
                int dLevel = array[i].Level - array[i - 1].Level;
                /// Độ lệch giá trị
                float dValue = array[i].Value - array[i - 1].Value;
                /// Độ lệch giá trị mỗi cấp
                float delta = dValue / dLevel;
                /// Duyệt danh sách các cấp con
                for (int j = array[i - 1].Level + 1; j < array[i].Level; j++)
                {
                    /// Giá trị tại cấp này
                    float curLevelValue = array[i - 1].Value + delta * (j - array[i - 1].Level);
                    /// Thêm vào danh sách
                    list.Add(new Symbol.ValueByLevel()
                    {
                        Level = j,
                        Value = curLevelValue,
                    });
                }
                list.Add(array[i]);
                /// Cập nhật giá trị Delta trước đó
                lastDelta = delta;
            }

            for (int j = array[array.Count - 1].Level + 1; j <= SkillDataEx.SystemMaxLevel; j++)
            {
                /// Giá trị cấp này
                float curLevelValue = array[array.Count - 1].Value + lastDelta * (j - array[array.Count - 1].Level);
                list.Add(new Symbol.ValueByLevel()
                {
                    Level = j,
                    Value = curLevelValue,
                });
            }

            return list;
        }
    }
}
