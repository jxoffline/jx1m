using GameServer.KiemThe.Core;
using GameServer.KiemThe.Entities;
using GameServer.Logic;
using System.Collections.Concurrent;
using System.IO;
using System.Xml.Linq;

namespace GameServer.KiemThe.Logic.Manager
{
    /// <summary>
    /// Đối tượng quản lý tập hợp các điểm thu thập
    /// </summary>
    public static partial class KTGrowPointManager
    {
        /// <summary>
        /// Danh sách điểm thu thập theo ID
        /// </summary>
        public static readonly ConcurrentDictionary<int, GrowPoint> GrowPoints = new ConcurrentDictionary<int, GrowPoint>();


        /// <summary>
        /// Tải xuống danh sách điểm thu thập trong bản đồ
        /// </summary>
        /// <param name="mapCode"></param>
        /// <param name="mapName"></param>
        /// <returns></returns>
        public static bool LoadMapGrowPoints(int mapCode, string mapName)
        {
            string fileName = KTGlobal.GetDataPath(string.Format("MapConfig/{0}/GrowPoints.xml", mapName));
            /// Nếu File không tồn tại tức Map này không có điểm thu thập
            if (!File.Exists(fileName))
            {
                return true;
            }

            /// Nội dung File XML
            string xmlContent = File.ReadAllText(fileName);
            /// Đối tượng XElement chứa nội dung tương ứng
            XElement xmlNode = XElement.Parse(xmlContent);
            if (xmlNode == null)
            {
                return false;
            }

            /// Duyệt danh sách tương ứng
            foreach (XElement node in xmlNode.Element("GrowPoints").Elements("GrowPoint"))
            {
                GrowPointXML growPointData = GrowPointXML.Parse(node);
                int posX = int.Parse(node.Attribute("PosX").Value);
                int posY = int.Parse(node.Attribute("PosY").Value);
                KTGrowPointManager.Add(mapCode, -1, growPointData, posX, posY);
            }

            return true;
        }
    }
}
