using GameServer.Logic;
using System.IO;
using System.Xml.Linq;

namespace GameServer.KiemThe.CopySceneEvents.DynamicArena
{
    /// <summary>
    /// Phụ bản lôi đài di động
    /// </summary>
    public static class DynamicArena
    {
        #region Define
        /// <summary>
        /// Dữ liệu
        /// </summary>
        public class DynamicArenaData
        {
            /// <summary>
            /// Dữ liệu bản đồ vào
            /// </summary>
            public class EnterMapData
            {
                /// <summary>
                /// ID bản đồ
                /// </summary>
                public int ID { get; set; }

                /// <summary>
                /// Vị trí X
                /// </summary>
                public int PosX { get; set; }

                /// <summary>
                /// Vị trí Y
                /// </summary>
                public int PosY { get; set; }

                /// <summary>
                /// Chuyển đối tượng từ XMLNode
                /// </summary>
                /// <param name="xmlNode"></param>
                /// <returns></returns>
                public static EnterMapData Parse(XElement xmlNode)
                {
                    return new EnterMapData()
                    {
                        ID = int.Parse(xmlNode.Attribute("ID").Value),
                        PosX = int.Parse(xmlNode.Attribute("PosX").Value),
                        PosY = int.Parse(xmlNode.Attribute("PosY").Value),
                    };
                }
            }

            /// <summary>
            /// Thông tin bản đồ vào
            /// </summary>
            public EnterMapData Map { get; set; }

            /// <summary>
            /// Chuyển đối tượng từ XMLNode
            /// </summary>
            /// <param name="xmlNode"></param>
            /// <returns></returns>
            public static DynamicArenaData Parse(XElement xmlNode)
            {
                /// Tạo mới
                DynamicArenaData data = new DynamicArenaData()
                {
                    Map = EnterMapData.Parse(xmlNode.Element("Map")),
                };

                /// Trả về kết quả
                return data;
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// Dữ liệu lôi đài di động
        /// </summary>
        public static DynamicArenaData Data { get; private set; }
        #endregion

        #region Init
        /// <summary>
        /// Khởi tạo dữ liệu
        /// </summary>
        public static void Init()
        {
            XElement xmlNode = KTGlobal.ReadXMLData("Config/KT_CopyScenes/DynamicArena.xml");
            DynamicArena.Data = DynamicArenaData.Parse(xmlNode);
        }
        #endregion
    }
}
