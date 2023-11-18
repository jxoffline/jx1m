using GameServer.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace GameServer.KiemThe.Entities
{
    /// <summary>
    /// Lớp quản lý Avarta nhân vật
    /// </summary>
    public static class KRoleAvarta
    {
        /// <summary>
        /// Danh sách Avarta nhân vật
        /// </summary>
        private static readonly Dictionary<int, RoleAvartaXML> roleAvartas = new Dictionary<int, RoleAvartaXML>();

        /// <summary>
        /// Khởi tạo Avarta nhân vật
        /// </summary>
        public static void Init()
        {
            XElement attribNode = KTGlobal.ReadXMLData("Config/KT_Avarta/RoleAvarta.xml");
            foreach (XElement node in attribNode.Elements("Avarta"))
            {
                RoleAvartaXML roleAvarta = RoleAvartaXML.Parse(node);
                KRoleAvarta.roleAvartas[roleAvarta.ID] = roleAvarta;
            }
        }

        /// <summary>
        /// Kiểm tra Avarta tương ứng có phù hợp với người chơi không
        /// </summary>
        /// <param name="player"></param>
        /// <param name="avartaID"></param>
        /// <returns></returns>
        public static bool IsAvartaValid(KPlayer player, int avartaID)
        {
            lock (KRoleAvarta.roleAvartas)
            {
                if (KRoleAvarta.roleAvartas.TryGetValue(avartaID, out RoleAvartaXML roleAvarta))
                {
                    return roleAvarta.Sex == player.RoleSex;
                }
                return false;
            }
        }

        /// <summary>
        /// Trả về thông tin Avarta nhân vật tương ứng
        /// </summary>
        /// <param name="avartaID"></param>
        /// <returns></returns>
        public static RoleAvartaXML GetRoleAvarta(int avartaID)
        {
            lock (KRoleAvarta.roleAvartas)
            {
                if (KRoleAvarta.roleAvartas.TryGetValue(avartaID, out RoleAvartaXML roleAvarta))
                {
                    return roleAvarta;
                }
                return null;
            }
        }

        /// <summary>
        /// Trả về Avarta mặc định phù hợp bản thân
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static RoleAvartaXML GetMyDefaultAvarta(KPlayer player)
        {
            lock (KRoleAvarta.roleAvartas)
            {
                RoleAvartaXML roleAvarta = KRoleAvarta.roleAvartas.Values.Where(x => x.Sex == player.RoleSex).FirstOrDefault();
                return roleAvarta;
            }
        }
    }
}
