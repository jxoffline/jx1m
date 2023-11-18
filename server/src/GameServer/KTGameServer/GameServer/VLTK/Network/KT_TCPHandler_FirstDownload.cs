using GameServer.KiemThe.Core.Activity.DownloadBouns;
using GameServer.KiemThe.Core.Item;
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

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Quản lý gói tin
    /// </summary>
    public static partial class KT_TCPHandler
    {
        /// <summary>
        /// Gửi thông tin tải lần đầu nhận quà về cho ngu
        /// </summary>
        /// <param name="client"></param>
        public static void SendDownloadBonus(KPlayer client)
        {
            try
            {
                BonusDownload ConfigSend = DownloadBounsManager._ConfigBounds;
                //if (client.ReviceBounsDownload == 0)
                //{
                //    ConfigSend.CanRevice = true;
                //}
                //else
                //{
                //    ConfigSend.CanRevice = false;
                //}


                ConfigSend.CanRevice = false;

                byte[] bytesCmd = DataHelper.ObjectToBytes<BonusDownload>(ConfigSend);
                client.SendPacket((int) TCPGameServerCmds.CMD_KT_GET_BONUS_DOWNLOAD, bytesCmd);
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, "Exception", false);
            }
        }

        /// <summary>
        /// Xử lý yêu cầu tải lần đầu nhận quà
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
        public static TCPProcessCmdResults ProcessDownloadBonus(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
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
                if (fields.Length != 2)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit CMD={0}, Client={1}, Recv={2}",
                        (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket), fields.Length));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                int roleID = Convert.ToInt32(fields[0]);
                int type = Convert.ToInt32(fields[1]);

                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client || client.RoleID != roleID)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Can not find player corresponding ID, CMD={0}, Client={1}, RoleID={2}",
                                                                        (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket), roleID));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                BonusDownload ConfigSend = DownloadBounsManager._ConfigBounds;
                if (client.ReviceBounsDownload == 0)
                {
                    if (KTGlobal.IsHaveSpace(ConfigSend.BonusItems.Count, client))
                    {
                        KTPlayerManager.ShowNotification(client, "Nhận thưởng thành công!");
                        client.ReviceBounsDownload = 1;
                        foreach (BonusItem _item in ConfigSend.BonusItems)
                        {
                            if (!ItemManager.CreateItem(Global._TCPManager.TcpOutPacketPool, client, _item.ItemID, _item.Number, 0, "BOUNDSREWARD", false, 1, false, ItemManager.ConstGoodsEndTime,"",-1,"",0,1,false))
                            {
                                KTPlayerManager.ShowNotification(client, "Có lỗi khi thêm vật phẩm!");
                            }
                        }
                    }
                    else
                    {
                        KTPlayerManager.ShowNotification(client, "Túi không đủ chỗ!");
                    }
                }
                else
                {
                    KTPlayerManager.ShowNotification(client, "Bạn đã nhận phần thưởng này rồi!");
                }
                /// Gửi lại Packet trạng thái
                KT_TCPHandler.SendDownloadBonus(client);

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
