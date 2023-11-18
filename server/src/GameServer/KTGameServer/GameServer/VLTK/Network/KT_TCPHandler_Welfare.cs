using GameServer.Logic;
using GameServer.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Quản lý gói tin
    /// </summary>
    public static partial class KT_TCPHandler
    {
        /// <summary>
        /// Kiểm tra tổng nạp của người chơi
        /// </summary>
        /// <param name="sl"></param>
        /// <param name="tcpClientPool"></param>
        /// <param name="pool"></param>
        /// <param name="client"></param>
        /// <param name="subMoney"></param>
        /// <returns></returns>
        public static int QueryTotalRecharge(KPlayer client)
        {
            string userID = GameManager.OnlineUserSession.FindUserID(client.ClientSocket);

            int zoneID = client.ZoneID;

            return KT_TCPHandler.QueryTotalRecharge(userID, client.RoleID, zoneID, client.ServerId);
        }

        /// <summary>
        /// Kiểm tra nạp
        /// </summary>
        /// <param name="sl"></param>
        /// <param name="tcpClientPool"></param>
        /// <param name="pool"></param>
        /// <param name="client"></param>
        /// <param name="subMoney"></param>
        /// <returns></returns>
        public static int QueryTotalRecharge(string userID, int RoleiD, int zoneID, int ServerId)
        {
            //Lấy thông tin tổng nạp từ DB SERVER
            string strcmd = string.Format("{0}:{1}:{2}", userID, zoneID, RoleiD);
            string[] dbFields = Global.ExecuteDBCmd((int) TCPGameServerCmds.CMD_DB_QUERYCHONGZHIMONEY, strcmd, ServerId);
            if (null == dbFields)
                return 0;
            if (dbFields.Length != 1)
            {
                return 0;
            }

            return Global.SafeConvertToInt32(dbFields[0]);
        }

        /// <summary>
        /// Kiểm tra tổng nạp hôm nay của người chơi
        /// </summary>
        /// <param name="sl"></param>
        /// <param name="tcpClientPool"></param>
        /// <param name="pool"></param>
        /// <param name="client"></param>
        /// <param name="subMoney"></param>
        /// <returns></returns>
        public static int QueryTotalRechargeToday(KPlayer client)
        {
            string userID = GameManager.OnlineUserSession.FindUserID(client.ClientSocket);
            int zoneID = client.ZoneID;

            string strcmd = string.Format("{0}:{1}:{2}", userID, zoneID, client.RoleID);
            string[] dbFields = Global.ExecuteDBCmd((int) TCPGameServerCmds.CMD_DB_QUERYTODAYCHONGZHIMONEY, strcmd, client.ServerId);
            if (null == dbFields)
                return 0;
            if (dbFields.Length != 1)
            {
                return 0;
            }

            return Global.SafeConvertToInt32(dbFields[0]);
        }
    }
}
