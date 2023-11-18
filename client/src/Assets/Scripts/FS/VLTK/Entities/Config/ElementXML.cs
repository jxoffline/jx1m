using System.Linq;
using System.Xml.Linq;

namespace FS.VLTK.Entities.Config
{
    public class ElementXML
    {
        /// <summary>
        /// Đường dẫn Bundle chứa ngũ hành
        /// </summary>
        public string BundleDir { get; set; }

        /// <summary>
        /// Tên Atlas
        /// </summary>
        public string AtlasName { get; set; }

        public class ElementDataXML
        {
            /// <summary>
            /// ID mặt
            /// </summary>
            public int ID { get; set; }

            /// <summary>
            /// Tên mặt
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Tên ảnh kích thước nhỏ trong Atlas
            /// </summary>
            public string SmallImage { get; set; }

            /// <summary>
            /// Tên ảnh kích thước vừa trong Atlas
            /// </summary>
            public string NormalImage { get; set; }

            /// <summary>
            /// Tên ảnh kích thước lớn trong Atlas
            /// </summary>
            public string BigImage { get; set; }

            public static ElementDataXML Parse(XElement xmlNode)
            {
                return new ElementDataXML()
                {
                    ID = int.Parse(xmlNode.Attribute("ID").Value),
                    Name = xmlNode.Attribute("Name").Value,
                    SmallImage = xmlNode.Attribute("SmallImage").Value,
                    NormalImage = xmlNode.Attribute("NormalImage").Value,
                    BigImage = xmlNode.Attribute("BigImage").Value,
                };
            }
        }

        /// <summary>
        /// Ngũ hành Kim
        /// </summary>
        public ElementDataXML Metal { get; set; }

        /// <summary>
        /// Ngũ hành Mộc
        /// </summary>
        public ElementDataXML Wood { get; set; }

        /// <summary>
        /// Ngũ hành Thổ
        /// </summary>
        public ElementDataXML Earth { get; set; }

        /// <summary>
        /// Ngũ hành Thủy
        /// </summary>
        public ElementDataXML Water { get; set; }

        /// <summary>
        /// Ngũ hành Hỏa
        /// </summary>
        public ElementDataXML Fire { get; set; }

        /// <summary>
        /// Chuyển đối tượng từ XML node
        /// </summary>
        /// <param name="xmlNode"></param>
        /// <returns></returns>
        public static ElementXML Parse(XElement xmlNode)
        {
            XElement[] elements = xmlNode.Elements("Element").ToArray<XElement>();
            return new ElementXML()
            {
                BundleDir = xmlNode.Attribute("BundleDir").Value,
                AtlasName = xmlNode.Attribute("AtlasName").Value,
                Metal = ElementDataXML.Parse(elements[0]),
                Wood = ElementDataXML.Parse(elements[1]),
                Earth = ElementDataXML.Parse(elements[2]),
                Water = ElementDataXML.Parse(elements[3]),
                Fire = ElementDataXML.Parse(elements[4]),
            };
        }
    }
}