using System.Collections.Generic;
using System.Xml.Linq;

namespace FS.VLTK.Entities.Config
{
    /// <summary>
    /// Đối tượng động tác đầu nhân vật đọc từ file
    /// </summary>
    public class MonsterActionSetXML
    {
        /// <summary>
        /// Thành phần
        /// </summary>
        public class Component
        {
            /// <summary>
            /// ID thành phần
            /// </summary>
            public string ID { get; set; }

            /// <summary>
            /// Dùng 8 hướng
            /// </summary>
            public bool Use8Dir { get; set; }

            /// <summary>
            /// Đường dẫn file Bundle chứa thành phần
            /// </summary>
            public string BundleDir { get; set; }

            /// <summary>
            /// Vị trí đặt X
            /// </summary>
            public short PosX { get; set; }

            /// <summary>
            /// Vị trí đặt Y
            /// </summary>
            public short PosY { get; set; }

            /// <summary>
            /// Độ cao cộng thêm
            /// </summary>
            public short Height { get; set; }

            /// <summary>
            /// Tự xoay các hướng bên phải
            /// </summary>
            public bool AutoFlip { get; set; }

            /// <summary>
            /// Chuyển đối tượng từ XMLNode
            /// </summary>
            /// <param name="xmlNode"></param>
            /// <returns></returns>
            public static Component Parse(XElement xmlNode)
			{
                return new Component()
                {
                    ID = xmlNode.Attribute("ID").Value,
                    Use8Dir = bool.Parse(xmlNode.Attribute("Use8Dir").Value),
                    BundleDir = xmlNode.Attribute("BundleDir").Value,
                    PosX = short.Parse(xmlNode.Attribute("PosX").Value),
                    PosY = short.Parse(xmlNode.Attribute("PosY").Value),
                    Height = short.Parse(xmlNode.Attribute("Height").Value),
                    AutoFlip = bool.Parse(xmlNode.Attribute("AutoFlip").Value),
                };
			}
        }

        /// <summary>
        /// Quần áo nhân vật nam
        /// </summary>
        public Dictionary<string, Component> Monsters { get; set; } = new Dictionary<string, Component>();

        /// <summary>
        /// Chuyển đối tượng từ XML Node
        /// </summary>
        /// <param name="xmlNode"></param>
        /// <returns></returns>
        public static MonsterActionSetXML Parse(XElement xmlNode)
        {
            MonsterActionSetXML actionSet = new MonsterActionSetXML();

            foreach (XElement node in xmlNode.Elements("Set"))
            {
                Component component = Component.Parse(node);
                actionSet.Monsters[component.ID] = component;
            }

            return actionSet;
        }
    }
}
