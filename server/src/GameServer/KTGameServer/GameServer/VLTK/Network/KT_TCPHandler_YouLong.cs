using GameServer.KiemThe.CopySceneEvents;
using GameServer.KiemThe.CopySceneEvents.Model;
using GameServer.KiemThe.CopySceneEvents.YouLongGe;
using GameServer.Logic;
using GameServer.Server;
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
    /// Quản lý phụ bản Du Long
    /// </summary>
    public static partial class KT_TCPHandler
    {
        /// <summary>
        /// Phản hồi thao tác phụ bản Du Long
        /// </summary>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static TCPProcessCmdResults ProcessYouLongRequest(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string cmdData = null;

            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Decode data faild, CMD={0}, Client={1}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                string[] fields = cmdData.Split(':');
                if (fields.Length != 1)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit CMD={0}, Client={1}, Recv={2}",
                        (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket), fields.Length));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Người chơi tương ứng
                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Can not find player, CMD={0}, Client={1}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket)));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Loại thao tác
                int type = Convert.ToInt32(fields[0]);

                /// Script điều khiển phụ bản tương ứng
                CopySceneEvent script = CopySceneEventManager.GetCopySceneScript(client.CurrentCopyMapID, client.CurrentMapCode);
                /// Nếu không tồn tại
                if (script == null)
				{
                    KTPlayerManager.ShowNotification(client, "Phụ bản không tồn tại!");
                    return TCPProcessCmdResults.RESULT_OK;
                }
                /// Nếu không phải phụ bản Du Long
                else if (!(script is YouLong_Script_Main))
				{
                    KTPlayerManager.ShowNotification(client, "Phụ bản không tồn tại!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Script Du Long
                YouLong_Script_Main youLongScript = (script as YouLong_Script_Main);

                /// Nếu là thao tác chọn ngẫu nhiên một vật phẩm thưởng
                if (type == 0)
				{
                    youLongScript.GetRandomAward();
				}
                /// nếu là thao tác nhận thưởng
                else if (type == 1)
				{
                    youLongScript.GetAward();
                }
                /// Nếu là thao tác đổi tiền
                else if (type == 2)
				{
                    youLongScript.ExchangeCoin();
				}
                /// Nếu là thao tác thử lại
                else if (type == 3)
				{
                    youLongScript.Rechallenge();
                }
                /// Nếu là thao tác vòng kế tiếp
                else if (type == 4)
				{
                    youLongScript.StartNewRound();
				}
                /// Nếu là thao tác rời phụ bản
                else if (type == -1)
				{
                    youLongScript.Leave();
				}
                /// Toác
				else
				{
                    KTPlayerManager.ShowNotification(client, "Thao tác bị lỗi, hãy thử lại sau!");
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
        /// Gửi yêu cầu mở khung phần thưởng Du Long
        /// </summary>
        /// <param name="player"></param>
        /// <param name="items"></param>
        public static void SendOpenYouLongAwardsBox(KPlayer player, List<YouLong.AwardInfo.Award> items)
		{
            List<string> itemsInfoStr = new List<string>();
            foreach (YouLong.AwardInfo.Award itemInfo in items)
			{
                itemsInfoStr.Add(string.Format("{0}_{1}", itemInfo.ID, itemInfo.Number));
            }
            string strCmd = string.Format("{0}:{1}", 0, string.Join("|", itemsInfoStr));
            player.SendPacket((int) TCPGameServerCmds.CMD_KT_YOULONG, strCmd);
        }

        /// <summary>
        /// Gửi yêu cầu cập nhật thông tin vật phẩm thưởng Du Long
        /// </summary>
        /// <param name="player"></param>
        /// <param name="itemID"></param>
        /// <param name="number"></param>
        /// <param name="exchangeCoinsAmount"></param>
        public static void SendUpdateYouLongAward(KPlayer player, int itemID, int number, int exchangeCoinsAmount, bool removeFromList)
		{
            string strCmd = string.Format("{0}:{1}:{2}:{3}", 1, string.Format("{0}_{1}", itemID, number), exchangeCoinsAmount, removeFromList ? 1 : 0);
            player.SendPacket((int) TCPGameServerCmds.CMD_KT_YOULONG, strCmd);
        }

        /// <summary>
        /// Gửi yêu cầu thông báo nhận thưởng Du Long thành công
        /// </summary>
        /// <param name="player"></param>
        public static void SendGetYouLongAwardSuccessfully(KPlayer player)
		{
            string strCmd = string.Format("{0}", 2);
            player.SendPacket((int) TCPGameServerCmds.CMD_KT_YOULONG, strCmd);
        }

        /// <summary>
        /// Gửi yêu cầu đóng khung thưởng Du Long
        /// </summary>
        /// <param name="player"></param>
        public static void SendCloseYouLongAwardsBox(KPlayer player)
		{
            string strCmd = string.Format("{0}", -1);
            player.SendPacket((int) TCPGameServerCmds.CMD_KT_YOULONG, strCmd);
        }
    }
}
