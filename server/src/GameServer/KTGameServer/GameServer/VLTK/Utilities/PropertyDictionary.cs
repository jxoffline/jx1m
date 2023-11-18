using GameServer.KiemThe.Entities;
using GameServer.Logic;
using Server.Tools;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace GameServer.KiemThe.Utilities
{
    /// <summary>
    /// Định nghĩa thuộc tính
    /// </summary>
    public static class PropertyDefine
    {
        /// <summary>
        /// Kiểu thuộc tính
        /// </summary>
        public enum PropertyType
        {
            /// <summary>
            /// Không có
            /// </summary>
            None,

            /// <summary>
            /// Số nguyên 8 bit
            /// </summary>
            Byte,
            /// <summary>
            /// Số nguyên 16 bit
            /// </summary>
            Short,
            /// <summary>
            /// Số nguyên 32 bit
            /// </summary>
            Integer,
            /// <summary>
            /// Số nguyên 64 bit
            /// </summary>
            Long,
            /// <summary>
            /// Số thực 32 bit
            /// </summary>
            Float,
            /// <summary>
            /// Số thực 64 bit
            /// </summary>
            Double,
            /// <summary>
            /// Chuỗi ký tự
            /// </summary>
            String,
            /// <summary>
            /// Mảng chứa thông tin thuộc tính (0: Min, 1: tùy trường hợp, 2: Max)
            /// </summary>
            KMagicAttrib,
        }

        /// <summary>
        /// Định nghĩa thuộc tính
        /// </summary>
        public class Property
        {
            /// <summary>
            /// Symbol ID
            /// </summary>
            public int ID { get; set; }

            /// <summary>
            /// Tên thuộc tính
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Ký hiệu thuộc tính
            /// </summary>
            public string SymbolName { get; set; }

            /// <summary>
            /// Chú thích
            /// </summary>
            public string Description { get; set; }

            /// <summary>
            /// Kiểu giá trị
            /// </summary>
            public PropertyType Type { get; set; }

            /// <summary>
            /// Có phải loại % không
            /// </summary>
            public bool IsPercent { get; set; }
        }

        /// <summary>
        /// Danh sách các thuộc tính được định nghĩa mặc định
        /// </summary>
        public static Dictionary<string, Property> PropertiesBySymbolName { get; private set; } = new Dictionary<string, Property>();

        /// <summary>
        /// Danh sách các thuộc tính được định nghĩa mặc định
        /// </summary>
        public static Dictionary<int, Property> PropertiesByID { get; private set; } = new Dictionary<int, Property>();

        /// <summary>
        /// Đọc giá trị PropertyDictionary từ File
        /// </summary>
        /// <param name="xmlNode"></param>
        public static void Init()
        {
            /// Dữ liệu từ File XML
            XElement xmlNode = KTGlobal.ReadXMLData("Config/KT_Skill/PropertyDictionary.xml");

            /// Xóa danh sách symbol
            PropertyDefine.PropertiesBySymbolName.Clear();
            PropertyDefine.PropertiesByID.Clear();

            /// Duyệt danh sách nút
            foreach (XElement node in xmlNode.Elements("Property"))
            {
                int id = int.Parse(node.Attribute("ID").Value);
                string name = node.Attribute("Name").Value;
                string symbolName = node.Attribute("SymbolName").Value;
                string description = node.Attribute("Description").Value.Replace("&lt;", "<").Replace("&gt;", ">").Replace("<color>", "</color>").Replace("%%", "%");

                int index = 0;
                while (description.IndexOf("%s") != -1)
                {
                    description = Utils.StringReplaceFirst(description, "%s", "{" + index + "}");
                    index++;
                }

                string typeStr;
                if (node.Attribute("Type") == null)
                {
                    typeStr = "KMagicAttrib";
                }
                else
                {
                    typeStr = node.Attribute("Type").Value;
                }
                bool isPercent = bool.Parse(node.Attribute("IsPercent").Value);
                PropertyType type;
                switch (typeStr)
                {
                    case "Byte":
                        type = PropertyType.Byte;
                        break;
                    case "Short":
                        type = PropertyType.Short;
                        break;
                    case "Int":
                        type = PropertyType.Integer;
                        break;
                    case "Long":
                        type = PropertyType.Long;
                        break;
                    case "Float":
                        type = PropertyType.Float;
                        break;
                    case "Double":
                        type = PropertyType.Double;
                        break;
                    case "String":
                        type = PropertyType.String;
                        break;
                    case "KMagicAttrib":
                        type = PropertyType.KMagicAttrib;
                        break;
                    default:
                        type = PropertyType.None;
                        break;
                }

                PropertyDefine.PropertiesBySymbolName[symbolName] = new Property()
                {
                    ID = id,
                    Name = name,
                    SymbolName = symbolName,
                    Type = type,
                    Description = description,
                    IsPercent = isPercent,
                };

                PropertyDefine.PropertiesByID[id] = new Property()
                {
                    ID = id,
                    Name = name,
                    SymbolName = symbolName,
                    Type = type,
                    Description = description,
                    IsPercent = isPercent,
                };
            }
        }
    }

    /// <summary>
    /// Từ điển thuộc tính
    /// </summary>
    public class PropertyDictionary
    {
        /// <summary>
        /// Danh sách
        /// </summary>
        private ConcurrentDictionary<int, object> Dictionary = new ConcurrentDictionary<int, object>();

        /// <summary>
        /// Tổng số phần tử có trong từ điển
        /// </summary>
        public int Count
        {
            get
            {
                return this.Dictionary.Count;
            }
        }

        /// <summary>
        /// Kiểm tra từ điển có chứa khóa tương ứng không
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ContainsKey(int key)
        {
            return this.Dictionary.TryGetValue(key, out object output);
        }

        /// <summary>
        /// Trả về dữ liệu trong từ điển, default nếu không tìm thấy hoặc không thể cast đối tượng về T
        /// </summary>
        /// <typeparam name="T">Kiểu</typeparam>
        /// <param name="key">Tên khóa</param>
        /// <returns></returns>
        public T Get<T>(int key)
        {
            if (this.Dictionary.TryGetValue(key, out object output))
            {
                if (output is T)
                {
                    return (T) output;
                }
                else
                {
                    return default;
                }
            }
            else
            {
                return default;
            }
        }

        /// <summary>
        /// Trả về dữ liệu trong từ điển, trả ra giá trị mặc định đầu vào nếu không tìm thấy hoặc không thể cast đối tượng về T
        /// </summary>
        /// <typeparam name="T">Kiểu</typeparam>
        /// <param name="key">Tên khóa</param>
        /// <returns></returns>
        public T Get<T>(int key, T defaultValue)
        {
            if (this.Dictionary.TryGetValue(key, out object output))
            {
                if (output is T)
                {
                    return (T) output;
                }
                else
                {
                    return defaultValue;
                }
            }
            else
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Thêm dữ liệu vào từ điển, nếu khóa tồn tại
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Set<T>(int key, T value)
        {
            this.Dictionary[key] = value;
        }

        /// <summary>
        /// Xóa khóa tương ứng khỏi từ điển
        /// </summary>
        /// <param name="key"></param>
        public void Remove(int key)
        {
            this.Dictionary.TryRemove(key, out _);
        }

        /// <summary>
        /// Cộng thêm thuộc tính tại vị trí tương ứng vào ProDict
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void AddProperty(int key, object value)
        {
            try
			{
                if (this.Dictionary.TryGetValue(key, out object _value))
                {
                    if (value is byte && _value is byte)
                    {
                        this.Dictionary[key] = (byte) value + (byte) _value;
                    }
                    else if (value is short && _value is short)
                    {
                        this.Dictionary[key] = (short) value + (short) _value;
                    }
                    else if (value is int && _value is int)
                    {
                        this.Dictionary[key] = (int) value + (int) _value;
                    }
                    else if (value is long && _value is long)
                    {
                        this.Dictionary[key] = (long) value + (long) _value;
                    }
                    else if (value is float && _value is float)
                    {
                        this.Dictionary[key] = (float) value + (float) _value;
                    }
                    else if (value is double && _value is double)
                    {
                        this.Dictionary[key] = (double) value + (double) _value;
                    }
                    else if (value is KMagicAttrib && _value is KMagicAttrib)
                    {
                        KMagicAttrib magicAttrib = (KMagicAttrib) this.Dictionary[key];
                        KMagicAttrib addMagicAttrin = (KMagicAttrib) value;
                        if (magicAttrib != null && value != null)
						{
                            magicAttrib.nValue[0] += addMagicAttrin.nValue[0];
                            magicAttrib.nValue[1] += addMagicAttrin.nValue[1];
                            magicAttrib.nValue[2] += addMagicAttrin.nValue[2];
						}
                    }
                    else if (value is string && _value is string)
                    {
                        this.Dictionary[key] = (string) value + (string) _value;
                    }
                }
                else
                {
                    this.Dictionary[key] = value;
                }
            }
            catch (Exception ex)
			{
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
			}
        }

        /// <summary>
        /// Cộng thêm giá trị ở PropertyDictionary khác vào
        /// </summary>
        /// <param name="pd"></param>
        public void AddProperties(PropertyDictionary pd)
        {
            if (pd == null)
			{
                return;
			}

            List<int> keys = pd.Dictionary.Keys.ToList();

            /// Duyệt danh sách khóa
            foreach (int key in keys)
            {
                if (pd.Dictionary.TryGetValue(key, out object value))
				{
                    this.AddProperty(key, value);
                }
            }
        }

        /*
        /// <summary>
        /// Thiết lập giá trị của Property Dictionary khác vào hiện tại
        /// </summary>
        /// <param name="pd"></param>
        public void SetProperties(PropertyDictionary pd)
        {
            foreach (KeyValuePair<int, object> pair in pd.Dictionary)
            {
                this.Dictionary[pair.Key] = pair.Value;
            }
        }

        /// <summary>
        /// Ghép Property Dictionary vào hiện tại
        /// </summary>
        /// <param name="pd"></param>
        /// <param name="isKeepCurrentExistProperties">Giữ thuộc tính đã tồn tại hiện tại (nếu false thì sẽ thay thế bằng thuộc tính ở danh sách sẽ thêm)</param>
        public void MergeProperties(PropertyDictionary pd, bool isKeepCurrentExistProperties)
        {
            foreach (KeyValuePair<int, object> pair in pd.Dictionary)
            {
                if (this.Dictionary.TryGetValue(pair.Key, out object value) && !isKeepCurrentExistProperties)
                {
                    this.Dictionary[pair.Key] = pair.Value;
                }
            }
        }
        */

        /// <summary>
        /// Trả về dữ liệu bản sao của từ điển hiện tại
        /// </summary>
        /// <returns></returns>
        public Dictionary<int, object> GetDictionary()
        {
            /// Tạo mới danh sách
            Dictionary<int, object> dictionary = new Dictionary<int, object>();

            List<int> keys = this.Dictionary.Keys.ToList();

            /// Duyệt danh sách khóa
            foreach (int key in keys)
            {
                if (this.Dictionary.TryGetValue(key, out object value))
                {
                    dictionary[key] = value;
                }
            }

            /// Trả về kết quả
            return dictionary;
        }

        /// <summary>
        /// Tạo bản sao của đối tượng
        /// </summary>
        /// <returns></returns>
        public PropertyDictionary Clone()
        {
            /// Tạo mới Prodict
            PropertyDictionary pd = new PropertyDictionary();

            List<int> keys = this.Dictionary.Keys.ToList();

            /// Duyệt danh sách khóa
            foreach (int key in keys)
            {
                if (this.Dictionary.TryGetValue(key, out object value))
                {
                    if (value is KMagicAttrib)
                    {
                        KMagicAttrib magicAttrib = (value as KMagicAttrib).Clone();
                        pd.Set<KMagicAttrib>(key, magicAttrib);
                    }
                    else
                    {
                        pd.Set<object>(key, value);
                    }
                }
            }

            /// Trả về kết quả
            return pd;
        }

        /// <summary>
        /// Chuyển đối tượng từ XML Node
        /// </summary>
        /// <param name="xmlNode"></param>
        /// <returns></returns>
        public static PropertyDictionary Parse(XElement xmlNode)
        {
            PropertyDictionary pd = new PropertyDictionary();

            foreach (KeyValuePair<string, PropertyDefine.Property> property in PropertyDefine.PropertiesBySymbolName)
            {
                if (xmlNode.Attribute(property.Key) != null)
                {
                    string value = xmlNode.Attribute(property.Key).Value;
                    switch (property.Value.Type)
                    {
                        case PropertyDefine.PropertyType.Byte:
                            byte resByte = byte.Parse(value);
                            pd.Set(property.Value.ID, resByte);
                            break;
                        case PropertyDefine.PropertyType.Short:
                            short resShort = short.Parse(value);
                            pd.Set(property.Value.ID, resShort);
                            break;
                        case PropertyDefine.PropertyType.Integer:
                            int resInt = int.Parse(value);
                            pd.Set(property.Value.ID, resInt);
                            break;
                        case PropertyDefine.PropertyType.Long:
                            long resLong = long.Parse(value);
                            pd.Set(property.Value.ID, resLong);
                            break;
                        case PropertyDefine.PropertyType.Float:
                            float resFloat = float.Parse(value);
                            pd.Set(property.Value.ID, resFloat);
                            break;
                        case PropertyDefine.PropertyType.Double:
                            double resDouble = float.Parse(value);
                            pd.Set(property.Value.ID, resDouble);
                            break;
                        case PropertyDefine.PropertyType.KMagicAttrib:
                            string resKMagicAttrib = value;
                            resKMagicAttrib = Regex.Replace(resKMagicAttrib, " ", "");
                            if (Regex.IsMatch(resKMagicAttrib, @"\{.*\}"))
                            {
                                Match match = Regex.Match(resKMagicAttrib, @"\{(.*)\}");
                                string[] param = match.Groups[1].Value.Split(',');
                                if (param.Length == 3)
                                {
                                    int first = int.Parse(param[0]);
                                    int second = int.Parse(param[1]);
                                    int third = int.Parse(param[2]);
                                    KMagicAttrib attrib = new KMagicAttrib()
                                    {
                                        nAttribType = (MAGIC_ATTRIB) property.Value.ID,
                                        nValue = new int[] { first, second, third, },
                                    };
                                    pd.Set(property.Value.ID, attrib);
                                }
                            }
                            break;
                        case PropertyDefine.PropertyType.String:
                            pd.Set(property.Value.ID, value);
                            break;
                        default:
                            break;
                    }
                }
            }

            return pd;
        }

        /// <summary>
        /// Chuyển ProDict thành dạng String
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            foreach (KeyValuePair<int, object> pair in this.Dictionary)
            {
                if (PropertyDefine.PropertiesByID.TryGetValue(pair.Key, out PropertyDefine.Property property))
                {
                    string symbolName = property.SymbolName;
                    object value = pair.Value;
                    string symbolType;
                    string symbolValue;
                    switch (property.Type)
                    {
                        case PropertyDefine.PropertyType.Byte:
                            byte resByte = (byte) value;
                            symbolType = "Byte";
                            symbolValue = resByte.ToString();
                            break;
                        case PropertyDefine.PropertyType.Short:
                            short resShort = (short) value;
                            symbolType = "Short";
                            symbolValue = resShort.ToString();
                            break;
                        case PropertyDefine.PropertyType.Integer:
                            int resInt = (int) value;
                            symbolType = "Int32";
                            symbolValue = resInt.ToString();
                            break;
                        case PropertyDefine.PropertyType.Long:
                            long resLong = (long) value;
                            symbolType = "Int64";
                            symbolValue = resLong.ToString();
                            break;
                        case PropertyDefine.PropertyType.Float:
                            float resFloat = (float) value;
                            symbolType = "Float";
                            symbolValue = resFloat.ToString();
                            break;
                        case PropertyDefine.PropertyType.Double:
                            double resDouble = (double) value;
                            symbolType = "Double";
                            symbolValue = resDouble.ToString();
                            break;
                        case PropertyDefine.PropertyType.KMagicAttrib:
                            KMagicAttrib resKMagicAttrib = (KMagicAttrib) value;
                            symbolType = "KMagicAttrib";
                            symbolValue = resKMagicAttrib.ToString();
                            break;
                        case PropertyDefine.PropertyType.String:
                            double resString = (double) value;
                            symbolType = "String";
                            symbolValue = resString.ToString();
                            break;
                        default:
                            symbolType = "Undefined";
                            symbolValue = "N/A";
                            break;
                    }

                    builder.AppendFormat("[{0} ({1}) - {2}] = {3}, ", symbolName, pair.Key, symbolType, symbolValue);
                }
            }

            return builder.ToString();
        }

        /// <summary>
        /// Chuyển dữ liệu về dạng chuỗi nén
        /// </summary>
        /// <returns></returns>
        public string ToPortableDBString()
        {
            List<string> data = new List<string>();
            foreach (KeyValuePair<int, object> pair in this.Dictionary)
            {
                if (PropertyDefine.PropertiesByID.TryGetValue(pair.Key, out PropertyDefine.Property property))
                {
                    string symbolName = property.SymbolName;
                    object value = pair.Value;
                    string symbolType;
                    string symbolValue;
                    switch (property.Type)
                    {
                        case PropertyDefine.PropertyType.Byte:
                            byte resByte = (byte) value;
                            symbolType = "Byte";
                            symbolValue = resByte.ToString();
                            break;
                        case PropertyDefine.PropertyType.Short:
                            short resShort = (short) value;
                            symbolType = "Short";
                            symbolValue = resShort.ToString();
                            break;
                        case PropertyDefine.PropertyType.Integer:
                            int resInt = (int) value;
                            symbolType = "Int32";
                            symbolValue = resInt.ToString();
                            break;
                        case PropertyDefine.PropertyType.Long:
                            long resLong = (long) value;
                            symbolType = "Int64";
                            symbolValue = resLong.ToString();
                            break;
                        case PropertyDefine.PropertyType.Float:
                            float resFloat = (float) value;
                            symbolType = "Float";
                            symbolValue = resFloat.ToString();
                            break;
                        case PropertyDefine.PropertyType.Double:
                            double resDouble = (double) value;
                            symbolType = "Double";
                            symbolValue = resDouble.ToString();
                            break;
                        case PropertyDefine.PropertyType.KMagicAttrib:
                            KMagicAttrib resKMagicAttrib = (KMagicAttrib) value;
                            symbolType = "KMagicAttrib";
                            symbolValue = resKMagicAttrib.ToString();
                            break;
                        case PropertyDefine.PropertyType.String:
                            string resString = (string) value;
                            symbolType = "String";
                            symbolValue = resString.ToString();
                            break;
                        default:
                            symbolType = "Undefined";
                            symbolValue = "N/A";
                            break;
                    }

                    if (property.Type != PropertyDefine.PropertyType.KMagicAttrib)
                    {
                        data.Add(string.Format("{0}_{1}_{2}_{3}_{4}", pair.Key, (int) property.Type, symbolValue, 0, 0));
                    }
                    else
                    {
                        KMagicAttrib resKMagicAttrib = (KMagicAttrib) value;
                        data.Add(string.Format("{0}_{1}_{2}_{3}_{4}", pair.Key, (int) property.Type, resKMagicAttrib.nValue[0], resKMagicAttrib.nValue[1], resKMagicAttrib.nValue[2]));
                    }
                }
            }

            string result = string.Join("|", data);
            return result;
        }

        /// <summary>
        /// Chuyển đối tượng từ chuỗi mã hóa trong DB
        /// </summary>
        /// <param name="dbString"></param>
        /// <returns></returns>
        public static PropertyDictionary FromPortableDBString(string dbString)
        {
            if (string.IsNullOrEmpty(dbString))
            {
                return null;
            }

            PropertyDictionary pd = new PropertyDictionary();

            string[] properties = dbString.Split('|');
            foreach (string propertyString in properties)
            {
                try
                {
                    string[] parameters = propertyString.Split('_');
                    if (parameters.Length != 5)
                    {
                        throw new Exception();
                    }
                    int symbolID = int.Parse(parameters[0]);
                    int symbolType = int.Parse(parameters[1]);
                    string value1 = parameters[2];
                    string value2 = parameters[3];
                    string value3 = parameters[4];

                    switch (symbolType)
                    {
                        case (int) PropertyDefine.PropertyType.Byte:
                            byte resByte = byte.Parse(value1);
                            pd.Set<byte>(symbolID, resByte);
                            break;
                        case (int) PropertyDefine.PropertyType.Short:
                            short resShort = short.Parse(value1);
                            pd.Set<short>(symbolID, resShort);
                            break;
                        case (int) PropertyDefine.PropertyType.Integer:
                            int resInt = int.Parse(value1);
                            pd.Set<int>(symbolID, resInt);
                            break;
                        case (int) PropertyDefine.PropertyType.Long:
                            long resLong = long.Parse(value1);
                            pd.Set<long>(symbolID, resLong);
                            break;
                        case (int) PropertyDefine.PropertyType.Float:
                            float resFloat = float.Parse(value1);
                            pd.Set<float>(symbolID, resFloat);
                            break;
                        case (int) PropertyDefine.PropertyType.Double:
                            double resDouble = double.Parse(value1);
                            pd.Set<double>(symbolID, resDouble);
                            break;
                        case (int) PropertyDefine.PropertyType.KMagicAttrib:
                            KMagicAttrib resKMagicAttrib = new KMagicAttrib()
                            {
                                nAttribType = (MAGIC_ATTRIB) symbolID,
                                nValue = new int[]
                                {
                                    int.Parse(value1), int.Parse(value2), int.Parse(value3)
                                },
                            };
                            pd.Set<KMagicAttrib>(symbolID, resKMagicAttrib);
                            break;
                        case (int) PropertyDefine.PropertyType.String:
                            string resString = value1;
                            pd.Set<string>(symbolID, resString);
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception) { }
            }

            /// Nếu ProDict rỗng thì set NULL
            if (pd.Count <= 0)
            {
                return null;
            }
            return pd;
        }
    }
}