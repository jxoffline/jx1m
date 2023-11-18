using GameServer.KiemThe.Logic.Manager.ClientLua;
using GameServer.Logic;
using GameServer.Server;
using Server.Data;
using Server.Protocol;
using Server.TCP;
using Server.Tools;
using System;

namespace GameServer.KiemThe.Logic
{
	/// <summary>
	/// Quản lý Chat
	/// </summary>
	public static partial class KT_TCPHandler
    {
        /// <summary>
        /// Xử lý gói tin gửi từ Client về Server dữ liệu làm việc với Lua Client
        /// </summary>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static TCPProcessCmdResults ProcessClientLua(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            ClientLuaPacket packet = null;

            try
            {
                packet = DataHelper.BytesToObject<ClientLuaPacket>(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Decode data faild, CMD={0}, Client={1}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                /// Đối tượng gnười chơi tương ứng
                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Can not find player corresponding ID, CMD={0}, Client={1}, RoleID={2}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket), client.RoleID));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Nếu không có dữ liệu
                if (packet == null)
				{
                    /// Toác
                    return TCPProcessCmdResults.RESULT_FAILED;
				}

                /// Chuyển qua Handler xử lý
                return KTClientLuaPacketManager.Handle(client, packet);
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        /// <summary>
        /// Gửi gói tin về Client thông báo dữ liệu làm việc với Client Lua
        /// </summary>
        /// <param name="player"></param>
        /// <param name="packet"></param>
        public static void SendClientLuaPacket(KPlayer player, ClientLuaPacket packet)
        {
            try
            {
                player.SendPacket<ClientLuaPacket>((int) TCPGameServerCmds.CMD_KT_CLIENT_SERVER_LUA, packet);
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }
    }
}
