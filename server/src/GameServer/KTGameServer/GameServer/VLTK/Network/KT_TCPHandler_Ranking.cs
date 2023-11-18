using GameServer.Logic;
using GameServer.Server;
using Server.Protocol;
using Server.TCP;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Bảng xếp hạng
    /// </summary>
    public static partial class KT_TCPHandler
    {
        /// <summary>
        /// Xử lý yêu cầu từ Client truy vấn thông tin bảng xếp hạng
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
        public static TCPProcessCmdResults ResponseQueryPlayerRanking(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string cmdData;

            try
            {
                cmdData = new ASCIIEncoding().GetString(data);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Error while getting DATA, CMD={0}, Client={1}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                /// Người chơi tương ứng
                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Can not find player, CMD={0}, Client={1}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket)));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                return Global.TransferRequestToDBServer(tcpMgr, socket, tcpClientPool, tcpRandKey, pool, nID, data, count, out tcpOutPacket, socket.ServerId);
                //return TCPProcessCmdResults.RESULT_OK;
            }
            catch (Exception ex)
            {

                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);

            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }
    }
}
