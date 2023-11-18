using System.Xml.Linq;
using static GameServer.KiemThe.Utilities.KTMath;

namespace GameServer.KiemThe.Entities
{
    /// <summary>
    /// Tân thủ thông khi tạo nhân vật sẽ được vào
    /// </summary>
    public class NewbieVillage
    {
        /// <summary>
        /// ID
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// Vị trí dịch vào
        /// </summary>
        public Point2D Position { get; set; }

        /// <summary>
        /// Chuyển đối tượng từ XML Node
        /// </summary>
        /// <param name="xmlNode"></param>
        /// <returns></returns>
        public static NewbieVillage Parse(XElement xmlNode)
        {
            return new NewbieVillage()
            {
                ID = int.Parse(xmlNode.Attribute("ID").Value),
                Position = new Point2D()
                {
                    X = int.Parse(xmlNode.Attribute("PosX").Value),
                    Y = int.Parse(xmlNode.Attribute("PosY").Value),
                },
            };
        }
    }
}