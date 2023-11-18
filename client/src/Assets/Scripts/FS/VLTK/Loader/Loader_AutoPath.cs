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
        #region Auto tìm đường liên bản đồ
        /// <summary>
        /// Đối tượng tự tìm đường liên bản đồ
        /// </summary>
        public static AutoPathXML AutoPaths { get; private set; } = null;

        /// <summary>
        /// Tải xuống danh sách thiết lập tự tìm đường liên bản đồ
        /// </summary>
        /// <param name="xmlNode"></param>
        public static void LoadAutoPaths(XElement xmlNode)
        {
            Loader.AutoPaths = AutoPathXML.Parse(xmlNode);

            //int totalEdges = Loader.AutoPaths.Teleports.Count + Loader.AutoPaths.TransferNPCs.Count + Loader.AutoPaths.TeleportItems.Sum(x => x.Value.Count);
            //KTDebug.LogError("Auto Path total edges = " + totalEdges);
        }
        #endregion
    }
}
