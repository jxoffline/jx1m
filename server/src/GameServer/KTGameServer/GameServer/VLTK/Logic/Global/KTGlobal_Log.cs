using GameServer.Logic;
using GameServer.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.KiemThe
{
    /// <summary>
    /// Các phương thức và đối tượng toàn cục của Kiếm Thế
    /// </summary>
    public static partial class KTGlobal
    {
        /// <summary>
        /// Ghi lại nhật ký đăng nhập của người chơi
        /// </summary>
        /// <param name="client"></param>
        public static void AddRoleLoginEvent(KPlayer client)
        {
            string userID = GameManager.OnlineUserSession.FindUserID(client.ClientSocket);
            string ip = Global.GetSocketRemoteEndPoint(client.ClientSocket).Replace(":", ".");

            string msg = "{0} {1}	{2}	{4}	{5}";

            string eventMsg = string.Format(msg,
                GameManager.ServerLineID,
                userID,
                client.RoleID,
                client.RoleName,
                client.m_Level,
                ip
                );

            GameManager.SystemRoleLoginEvents.AddEvent(eventMsg, EventLevels.Important);
        }

        /// <summary>
        /// Ghi lại nhật ký đăng xuất của người chơi
        /// </summary>
        /// <param name="client"></param>
        public static void AddRoleLogoutEvent(KPlayer client)
        {
            string userID = GameManager.OnlineUserSession.FindUserID(client.ClientSocket);
            string ip = Global.GetSocketRemoteEndPoint(client.ClientSocket).Replace(":", ".");

            string msg = "{0}	{1}	{2}	{4}	{5} {6}";

            string eventMsg = string.Format(msg,
                GameManager.ServerLineID,
                userID,
                client.RoleID,
                client.RoleName,
                client.m_Level,
                ip,
                0
                );

            GameManager.SystemRoleLogoutEvents.AddEvent(eventMsg, EventLevels.Important);
        }

        public static void AddRoleTaskEvent(KPlayer client, int completeTaskID)
        {
            string userID = GameManager.OnlineUserSession.FindUserID(client.ClientSocket);

            string msg = "{0}	{1}	{2}	{4}	{5}";

            string eventMsg = string.Format(msg,
                GameManager.ServerLineID,
                userID,
                client.RoleID,
                client.RoleName,
                client.m_Level,
                completeTaskID
                );

            GameManager.SystemRoleTaskEvents.AddEvent(eventMsg, EventLevels.Important);
        }

        /// <summary>
        /// Ghi lại nhật ký sử dụng tiền của người chơi
        /// </summary>
        /// <param name="client"></param>
        public static void AddRoleBoundMoneyEvent(KPlayer client, int oldBoundMoney)
        {
            string userID = GameManager.OnlineUserSession.FindUserID(client.ClientSocket);
            string ip = Global.GetSocketRemoteEndPoint(client.ClientSocket).Replace(":", ".");

            string msg = "{0}	{1}	{2}	{4}	{5}";
            string eventMsg = string.Format(msg,
                GameManager.ServerLineID,
                userID,
                client.RoleID,
                client.RoleName,
                oldBoundMoney,
                client.BoundMoney
                );

            GameManager.SystemRoleBoundMoneyEvents.AddEvent(eventMsg, EventLevels.Important);
        }

        public static void AddRoleStoreMoneyEvent(KPlayer client, long oldBoundToken)
        {
            string userID = GameManager.OnlineUserSession.FindUserID(client.ClientSocket);
            string ip = Global.GetSocketRemoteEndPoint(client.ClientSocket).Replace(":", ".");

            string eventMsg = string.Format("{0}	{1}	{2}	{3}	{4}	{5}",
                GameManager.ServerLineID,
                userID,
                client.RoleID,
                client.RoleName,
                oldBoundToken,
                0
                );

            GameManager.SystemRoleStoreMoneyEvents.AddEvent(eventMsg, EventLevels.Important);
        }

        public static void AddRoleUpgradeEvent(KPlayer client, int oldLevel)
        {
            string userID = GameManager.OnlineUserSession.FindUserID(client.ClientSocket);

            string msg = "{0}	{1}	{2}	{4}	{5}	{6}	{7}	{8}	{9}";

            string eventMsg = string.Format(msg,
                GameManager.ServerLineID,
                userID,
                client.RoleID,
                client.RoleName,
                oldLevel,
                client.m_Level,
                client.m_Experience,
                client.MapCode,
                client.PosX,
                client.PosY
                );

            GameManager.SystemRoleUpgradeEvents.AddEvent(eventMsg, EventLevels.Important);
        }

        /// <summary>
        /// Có thể nhận quà nạp lần đầu không
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public static bool CanGetWelfareFirstRecharge(KPlayer client)
        {
            string[] dbRoleFields = Global.ExecuteDBCmd((int) TCPGameServerCmds.CMD_DB_QUERYFIRSTCHONGZHIBYUSERID, string.Format("{0}", client.RoleID), client.ServerId);
            if (null == dbRoleFields || dbRoleFields.Length != 1 || int.Parse(dbRoleFields[0]) <= 0)
            {
                return true;
            }

            return false;
        }
    }
}
