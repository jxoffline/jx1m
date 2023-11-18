using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace FS.VLTK.Entities.Config
{
    /// <summary>
    /// Danh hiệu phi phong
    /// </summary>
    public class MantleTitleXML
    {
        /// <summary>
        /// Giá trị tài phú nhân vật
        /// </summary>
        public int RoleValue { get; set; }

        /// <summary>
        /// Thời gian thực thi hiệu ứng
        /// </summary>
        public float AnimationSpeed { get; set; }

        /// <summary>
        /// Danh sách Sprite tạo nên hiệu ứng
        /// </summary>
        public List<string> SpriteNames { get; set; }

        /// <summary>
        /// Chuyển đối tượng từ XMLNode
        /// </summary>
        /// <param name="xmlNode"></param>
        /// <returns></returns>
        public static MantleTitleXML Parse(XElement xmlNode)
        {
            MantleTitleXML mantleTitle = new MantleTitleXML()
            {
                RoleValue = int.Parse(xmlNode.Attribute("RoleValue").Value),
                AnimationSpeed = float.Parse(xmlNode.Attribute("AnimationSpeed").Value),
                SpriteNames = new List<string>(),
            };

            foreach (XElement node in xmlNode.Elements("Sprite"))
            {
                string spriteName = node.Attribute("Name").Value;
                mantleTitle.SpriteNames.Add(spriteName);
            }

            return mantleTitle;
        }
    }
}
