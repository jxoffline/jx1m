using GameServer.Logic;
using GameServer.Server;
using Server.Data;
using Server.Protocol;
using Server.TCP;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Quản lý danh hiệu và tên
    /// </summary>
    public static partial class KT_TCPHandler
    {
        #region Danh hiệu
        /// <summary>
        /// Gửi gói tin thông báo danh hiệu của đối tượng thay đổi tới tất cả người chơi xung quanh
        /// </summary>
        /// <param name="go"></param>
        public static void NotifyOthersMyTitleChanged(GameObject go)
        {
            try
            {
                G2C_RoleTitleChanged titleChanged = new G2C_RoleTitleChanged()
                {
                    RoleID = go.RoleID,
                    Title = string.IsNullOrEmpty(go.TempTitle) ? go.Title : go.TempTitle,
                    GuildTitle = (go is KPlayer player) ? (string.IsNullOrEmpty(player.GuildTitle) ? player.FamilyTitle : player.GuildTitle) : "",
                };
                byte[] cmdData = DataHelper.ObjectToBytes<G2C_RoleTitleChanged>(titleChanged);

                /// Tìm tất cả người chơi xung quanh để gửi gói tin
                List<KPlayer> listObjects = KTRadarMapManager.GetPlayersAround(go);
                if (listObjects == null)
                {
                    return;
                }
                /// Gửi gói tin đến tất cả người chơi xung quanh
                KTPlayerManager.SendPacketToPlayers((int) TCPGameServerCmds.CMD_KT_UPDATE_TITLE, listObjects, cmdData, go, null);
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }
        #endregion

        #region Tên
        /// <summary>
        /// Gửi gói tin thông báo tên của đối tượng thay đổi tới tất cả người chơi xung quanh
        /// </summary>
        /// <param name="go"></param>
        public static void NotifyOthersMyNameChanged(GameObject go)
        {
            try
            {
                G2C_RoleNameChanged nameChanged = new G2C_RoleNameChanged()
                {
                    RoleID = go.RoleID,
                    RoleName = go.RoleName,
                };
                byte[] cmdData = DataHelper.ObjectToBytes<G2C_RoleNameChanged>(nameChanged);

                /// Tìm tất cả người chơi xung quanh để gửi gói tin
                List<KPlayer> listObjects = KTRadarMapManager.GetPlayersAround(go);
                if (listObjects == null)
                {
                    return;
                }
                /// Gửi gói tin đến tất cả người chơi xung quanh
                KTPlayerManager.SendPacketToPlayers((int) TCPGameServerCmds.CMD_KT_UPDATE_NAME, listObjects, cmdData, go, null);
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }
        #endregion

        #region Danh hiệu nhân vật
        /// <summary>
        /// Phản hồi yêu cầu thay đổi danh hiệu nhân vật
        /// </summary>
        /// <param name="tcpMgr"></param>
        /// <param name="socket"></param>
        /// <param name="tcpClientPool"></param>
        /// <param name="tcpRandKey"></param>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <param name="tcpOutPacket"></param>
        /// <returns></returns>
        public static TCPProcessCmdResults ResponseChangeCurrentRoleTitle(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;

            string cmdData = "";
            try
            {
                /// Giải mã gói tin đẩy về dạng string
                cmdData = new ASCIIEncoding().GetString(data);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Error while getting DATA, CMD={0}, Client={1}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                string[] fields = cmdData.Split(':');
                if (fields.Length != 1)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit CMD={0}, Client={1}, Recv={2}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket), cmdData.Length));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// ID danh hiệu tương ứng
                int titleID = int.Parse(fields[0]);

                /// Tìm chủ nhân của gói tin tương ứng
                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Không tìm thấy thông tin người chơi, CMD={0}, Client={1}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket)));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Chuyển danh hiệu tương ứng
                bool ret = client.SetAsCurrentRoleTitle(titleID);

                /// Nếu thành công
                if (ret)
				{
                    KTPlayerManager.ShowNotification(client, "Thiết lập danh hiệu thành công!");
				}

                return TCPProcessCmdResults.RESULT_OK;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        /// <summary>
        /// Gửi gói tin thông báo danh hiệu nhân vật của bản thân thay đổi tới tất cả người chơi xung quanh
        /// </summary>
        /// <param name="player"></param>
        public static void NotifyOthersMyCurrentRoleTitleChanged(KPlayer player)
		{
            string strCmd = string.Format("{0}:{1}:{2}", player.RoleID, player.CurrentRoleTitleID, player.SpecialTitleID);
            /// Tìm tất cả người chơi xung quanh để gửi gói tin
            List<KPlayer> listObjects = KTRadarMapManager.GetPlayersAround(player);
            if (listObjects == null)
            {
                return;
            }
            /// Gửi gói tin đến tất cả người chơi xung quanh
            KTPlayerManager.SendPacketToPlayers((int) TCPGameServerCmds.CMD_KT_UPDATE_CURRENT_ROLETITLE, listObjects, strCmd, player, null);
        }

        /// <summary>
        /// Thông báo thêm/xóa danh hiệu tương ứng của bản thân
        /// </summary>
        /// <param name="player"></param>
        /// <param name="titleID"></param>
        /// <param name="method"></param>
        public static void SendModifyMyselfCurrentRoleTitle(KPlayer player, int titleID, int method)
		{
            string strCmd = string.Format("{0}:{1}", method, titleID);
            player.SendPacket((int) TCPGameServerCmds.CMD_KT_G2C_MOD_ROLETITLE, strCmd);
		}
		#endregion
	}
}
