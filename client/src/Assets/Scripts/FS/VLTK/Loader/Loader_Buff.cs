using FS.VLTK.Entities.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace FS.VLTK.Loader
{
    /// <summary>
    /// Đối tượng chứa danh sách các cấu hình trong game
    /// </summary>
    public static partial class Loader
    {
        #region Buff
        /// <summary>
        /// Danh sách Buff
        /// </summary>
        public static Dictionary<int, BuffXML> Buffs { get; private set; } = new Dictionary<int, BuffXML>();

        /// <summary>
        /// Đọc dữ liệu từ file Buff.xml trong Bundle
        /// </summary>
        /// <param name="xmlNode"></param>
        public static void LoadBuff(XElement xmlNode)
        {
            Loader.Buffs.Clear();

            foreach (XElement node in xmlNode.Elements("Buff"))
            {
                BuffXML buff = BuffXML.Parse(node);
                Loader.Buffs[buff.ID] = buff;
            }
        }
        #endregion
    }
}
