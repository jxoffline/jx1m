using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace GameServer.KiemThe.Entities
{
    /// <summary>
    /// Đối tượng XML mặt nhân vật
    /// </summary>
    public class RoleAvartaXML
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
        /// Giới tính
        /// </summary>
        public int Sex { get; set; }

        /// <summary>
        /// Mặc định có
        /// </summary>
        public bool IsDefaultTake { get; set; }

        /// <summary>
        /// Đường dẫn Bundle chứa mặt
        /// </summary>
        public string BundleDir { get; set; }

        /// <summary>
        /// Tên Atlas trong Bundle chứa mặt
        /// </summary>
        public string AtlasName { get; set; }

        /// <summary>
        /// Sprite chứa mặt
        /// </summary>
        public string SpriteName { get; set; }

        /// <summary>
        /// Chuyển đối tượng từ XML node
        /// </summary>
        /// <param name="xmlNode"></param>
        /// <returns></returns>
        public static RoleAvartaXML Parse(XElement xmlNode)
        {
            return new RoleAvartaXML()
            {
                ID = int.Parse(xmlNode.Attribute("ID").Value),
                Name = xmlNode.Attribute("Name").Value,
                Sex = int.Parse(xmlNode.Attribute("Sex").Value),
                IsDefaultTake = bool.Parse(xmlNode.Attribute("IsDefaultTake").Value),
                BundleDir = xmlNode.Attribute("BundleDir").Value,
                AtlasName = xmlNode.Attribute("AtlasName").Value,
                SpriteName = xmlNode.Attribute("Sprite").Value,
            };
        }
    }
}
