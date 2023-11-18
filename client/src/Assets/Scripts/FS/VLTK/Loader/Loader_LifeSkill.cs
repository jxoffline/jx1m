using FS.VLTK.Entities.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace FS.VLTK.Loader
{
    /// <summary>
    /// Đối tượng chứa danh sách các cấu hình trong game
    /// </summary>
    public static partial class Loader
    {
        #region Kỹ năng sống
        /// <summary>
        /// Kỹ năng sống
        /// </summary>
        public static LifeSkill LifeSkills { get; private set; } = null;

        /// <summary>
        /// Tải danh sách kỹ năng sống
        /// </summary>
        /// <param name="xmlNode"></param>
        public static void LoadLifeSkills(XElement xmlNode)
        {
            Loader.LifeSkills = LifeSkill.Parse(xmlNode);
        }
        #endregion
    }
}
