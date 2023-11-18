using FS.VLTK.Entities.Config;
using FS.VLTK.Entities.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using static FS.VLTK.Entities.Enum;

namespace FS.VLTK.Loader
{
    /// <summary>
    /// Đối tượng chứa danh sách các cấu hình trong game
    /// </summary>
    public static partial class Loader
    {
        #region Ngũ hành
        /// <summary>
        /// File XML chứa danh sách ngũ hành
        /// </summary>
        public static ElementXML ElementXML { get; private set; }

        /// <summary>
        /// Đọc dữ liệu file Element.xml trong Bundle
        /// </summary>
        /// <param name="xmlNode"></param>
        public static void LoadElement(XElement xmlNode)
        {
            Loader.ElementXML = ElementXML.Parse(xmlNode);
        }

        /// <summary>
        /// Danh sách ngũ hành trong Game
        /// </summary>
        public static Dictionary<Elemental, ElementData> Elements { get; private set; } = new Dictionary<Elemental, ElementData>();
        #endregion
    }
}
