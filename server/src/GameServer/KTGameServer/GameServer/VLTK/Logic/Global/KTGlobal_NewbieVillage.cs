using GameServer.KiemThe.Entities;
using GameServer.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace GameServer.KiemThe
{
    /// <summary>
    /// Các phương thức và đối tượng toàn cục của Kiếm Thế
    /// </summary>
    public static partial class KTGlobal
    {
        /// <summary>
        /// Danh sách các tân thủ thôn khi tạo nhân vật sẽ được vào
        /// </summary>
        public static List<NewbieVillage> NewbieVillages { get; } = new List<NewbieVillage>();

        /// <summary>
        /// Đọc dữ liệu danh sách các tân thủ thôn khi tạo nhân vật sẽ được vào
        /// </summary>
        public static void LoadNewbieVillages()
        {
            KTGlobal.NewbieVillages.Clear();

            XElement xmlNode = KTGlobal.ReadXMLData("Config/KT_Setting/NewbieVillages.xml");
            foreach (XElement node in xmlNode.Elements())
            {
                NewbieVillage newbieVillage = NewbieVillage.Parse(node);
                KTGlobal.NewbieVillages.Add(newbieVillage);
            }
        }
    }
}
