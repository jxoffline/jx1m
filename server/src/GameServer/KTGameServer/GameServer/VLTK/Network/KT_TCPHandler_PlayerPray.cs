using GameServer.KiemThe.Core.Activity.PlayerPray;
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
    /// Quản lý gói tin
    /// </summary>
    public static partial class KT_TCPHandler
    {
        /// <summary>
        /// Phản hồi thao tác chúc phúc
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
        public static TCPProcessCmdResults ResponsePlayerPray(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string strCmd;

            try
            {
                strCmd = new ASCIIEncoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Error while getting DATA, CMD={0}, Client={1}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                string[] fields = strCmd.Split(':');
                /// Nếu số lượng gửi về không thỏa mãn
                if (fields.Length != 1)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit CMD={0}, Client={1}, Recv={2}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket), fields.Length));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Loại thao tác
                int type = int.Parse(fields[0]);

                /// Người chơi tương ứng gửi gói tin
                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Can not find player, CMD={0}, Client={1}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket)));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Nếu thao tác quá nhanh
                if (KTGlobal.GetCurrentTimeMilis() - client.LastPrayTicks < 500)
				{
                    KTPlayerManager.ShowNotification(client, "Thao tác quá nhanh, hãy bình tĩnh!");
                    return TCPProcessCmdResults.RESULT_OK;
				}
                /// Đánh dấu thời điểm quay
                client.LastPrayTicks = KTGlobal.GetCurrentTimeMilis();

                /// Nếu là yêu cầu mở khung
                if (type == 0)
				{
                    /// Thực hiện mở khung
                    bool ret = KTPlayerPrayManager.Open(client, out List<string> lastResult, out bool enableGetAward, out bool enableStartTurn);
                    /// Nếu không thể mở khung
                    if (!ret)
					{
                        return TCPProcessCmdResults.RESULT_OK;
					}
                    /// Số lượt còn lại ngày hôm nay
                    int totalTurnLeft = KTPlayerPrayManager.GetTotalTurnLeft(client);

                    /// Gói tin gửi về Client
                    string responseData = string.Format("{0}:{1}:{2}:{3}:{4}", 0, lastResult == null ? "" : string.Join("_", lastResult), enableGetAward ? 1 : 0, enableStartTurn ? 1 : 0, totalTurnLeft);
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, new UTF8Encoding().GetBytes(responseData), nID);
                    return TCPProcessCmdResults.RESULT_DATA;
				}
                /// Nếu là yêu cầu quay
                else if (type == 1)
				{
                    /// Thực hiện quay
                    bool ret = KTPlayerPrayManager.StartTurn(client, out int round, out int stopPos, out bool enableGetAward, out bool enableStartTurn);
                    /// Nếu thất bại
                    if (!ret)
					{
                        return TCPProcessCmdResults.RESULT_OK;
					}
                    /// Số lượt còn lại ngày hôm nay
                    int totalTurnLeft = KTPlayerPrayManager.GetTotalTurnLeft(client);

                    /// Gói tin gửi về Client
                    string responseData = string.Format("{0}:{1}:{2}:{3}:{4}:{5}:{6}", 1, round, stopPos, client.LastPrayResult == null ? "" : string.Join("_", client.LastPrayResult), enableGetAward ? 1 : 0, enableStartTurn ? 1 : 0, totalTurnLeft);
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, new UTF8Encoding().GetBytes(responseData), nID);
                    return TCPProcessCmdResults.RESULT_DATA;
                }
                /// Nếu là yêu cầu nhận thưởng
                else if (type == 2)
				{
                    /// Thực hiện nhận thưởng
                    bool ret = KTPlayerPrayManager.GetAward(client);
                    /// Nếu thất bại
                    if (!ret)
                    {
                        return TCPProcessCmdResults.RESULT_OK;
                    }

                    /// Số lượt còn lại ngày hôm nay
                    int totalTurnLeft = KTPlayerPrayManager.GetTotalTurnLeft(client);

                    /// Gói tin gửi về Client
                    string responseData = string.Format("{0}:{1}", 2, totalTurnLeft);
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, new UTF8Encoding().GetBytes(responseData), nID);
                    return TCPProcessCmdResults.RESULT_DATA;
                }
                /// Toác
                else
				{
                    return TCPProcessCmdResults.RESULT_FAILED;
				}

                return TCPProcessCmdResults.RESULT_OK;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }
    }
}
