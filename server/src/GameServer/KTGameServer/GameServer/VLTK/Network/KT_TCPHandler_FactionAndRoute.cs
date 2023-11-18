using GameServer.Logic;
using GameServer.Server;
using Server.Data;
using Server.Tools;
using System;
using System.Collections.Generic;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Quản lý môn phái và nhánh
    /// </summary>
    public static partial class KT_TCPHandler
    {
        #region Môn phái và nhánh

        /// <summary>
        /// Thông báo môn phái và nhánh của người chơi thay đổi
        /// </summary>
        /// <param name="client"></param>
        public static void NotificationFactionChanged(KPlayer client)
        {
            try
            {
                RoleFactionChanged factionChanged = new RoleFactionChanged()
                {
                    RoleID = client.RoleID,
                    FactionID = client.m_cPlayerFaction.GetFactionId(),
                };
                byte[] cmdData = DataHelper.ObjectToBytes<RoleFactionChanged>(factionChanged);
                /// Tìm tất cả người chơi xung quanh để gửi gói tin
                List<KPlayer> listObjects = KTRadarMapManager.GetPlayersAround(client);
                if (listObjects == null)
                {
                    return;
                }
                /// Gửi gói tin đến tất cả người chơi xung quanh
                KTPlayerManager.SendPacketToPlayers((int)TCPGameServerCmds.CMD_KT_FACTIONROUTE_CHANGED, listObjects, cmdData, client, null);
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }

        /// <summary>
        /// Lưu lại môn phái và nhánh vào DB
        /// </summary>
        /// <param name="client"></param>
        public static void SaveFactionAndRouteToDBServer(KPlayer player)
        {
            string strcmd = string.Format("{0}:{1}:{2}", player.RoleID, player.m_cPlayerFaction.GetFactionId(), 0);
            try
            {
                Global.ExecuteDBCmd((int)TCPGameServerCmds.CMD_DB_EXECUTECHANGEOCCUPATION, strcmd, player.ServerId);
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }

        #endregion Môn phái và nhánh
    }
}