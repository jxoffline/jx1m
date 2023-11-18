using Server.Data;
using System.Xml.Linq;

namespace FS.VLTK.Loader
{
    /// <summary>
    /// Đối tượng chứa danh sách các cấu hình trong game
    /// </summary>
    public static partial class Loader
    {
        #region Bản đồ thế giới
        /// <summary>
        /// Thiết lập bang hội
        /// </summary>
        public static GuildInfoXML GuildConfig { get; private set; }

        /// <summary>
        /// Đọc dữ liệu thiết lập bang hội
        /// </summary>
        /// <param name="xmlNode"></param>
        public static void LoadGuildConfig(XElement xmlNode)
        {
            Loader.GuildConfig = GuildInfoXML.Parse(xmlNode);
        }
        #endregion
    }
}
