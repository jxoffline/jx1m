using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace GameServer.KiemThe.Entities
{
    /// <summary>
    /// Đối tượng mô tả thông tin điểm thu thập
    /// </summary>
    public class GrowPointXML
    {
        /// <summary>
        /// ID ngoại hình ở file Npcs.xml
        /// </summary>
        public int ResID { get;  set; }

        /// <summary>
        /// Tên điểm thu thập
        /// </summary>
        public string Name { get;  set; }

        /// <summary>
        /// Thời gian tái tạo (-1 nếu không tái tạo)
        /// <para>Đơn vị Milis</para>
        /// </summary>
        public int RespawnTime { get;  set; }

        /// <summary>
        /// ID Script Lua điều khiển
        /// </summary>
        public int ScriptID { get;  set; }

        /// <summary>
        /// Thời gian thu thập
        /// </summary>
        public int CollectTick { get;  set; }

        /// <summary>
        /// Bị mất trạng thái thu thập nếu chịu sát thương
        /// </summary>
        public bool InteruptIfTakeDamage { get;  set; }

        /// <summary>
        /// Chuyển đối tượng từ XML Node
        /// </summary>
        /// <param name="xmlNode"></param>
        /// <returns></returns>
        public static GrowPointXML Parse(XElement xmlNode)
        {
            return new GrowPointXML()
            {
                ResID = int.Parse(xmlNode.Attribute("Code").Value),
                Name = xmlNode.Attribute("Name").Value,
                RespawnTime = int.Parse(xmlNode.Attribute("RespawnTime").Value),
                ScriptID = int.Parse(xmlNode.Attribute("ScriptID").Value),
                CollectTick = int.Parse(xmlNode.Attribute("CollectTick").Value),
                InteruptIfTakeDamage = bool.Parse(xmlNode.Attribute("InteruptIfTakeDamage").Value),
            };
        }

        /// <summary>
        /// Chuyển đối tượng từ dữ liệu trần
        /// </summary>
        /// <param name="resID"></param>
        /// <param name="name"></param>
        /// <param name="respawnTime"></param>
        /// <param name="scriptID"></param>
        /// <param name="collectTick"></param>
        /// <param name="interuptIfTakeDamage"></param>
        /// <returns></returns>
        public static GrowPointXML Parse(int resID, string name, int respawnTime, int scriptID, int collectTick, bool interuptIfTakeDamage)
        {
            return new GrowPointXML()
            {
                ResID = resID,
                Name = name,
                RespawnTime = respawnTime,
                CollectTick = collectTick,
                InteruptIfTakeDamage = interuptIfTakeDamage,
                ScriptID = scriptID,
            };
        }
    }
}
