using GameServer.Core.Executor;
using GameServer.KiemThe;
using GameServer.KiemThe.Core.Item;
using GameServer.KiemThe.Logic;
using GameServer.Logic;
using GameServer.Server;
using Server.Protocol;
using Server.TCP;
using Server.Tools;
using System;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace GameServer.VLTK.Core.Activity.CheckPoint
{
    public class CheckPointManager
    {
        /// <summary>
        /// Def
        /// </summary>
        private static CheckPontConfig _ConfigCard = new CheckPontConfig();

        public static string CHECK_POINT_CONFIG_FILE = "Config/KT_Activity/KTCheckPontConfig.xml";

        /// <summary>
        /// Load config
        /// </summary>
        public static void Setup()
        {
            string Files = KTGlobal.GetDataPath(CHECK_POINT_CONFIG_FILE);

            using (var stream = System.IO.File.OpenRead(Files))
            {
                var serializer = new XmlSerializer(typeof(CheckPontConfig));
                _ConfigCard = serializer.Deserialize(stream) as CheckPontConfig;
            }
        }

        #region TCP_NETWORK

        /// <summary>
        /// Funtion thực hiện trả về dữ liệu điểm danh
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
        public static TCPProcessCmdResults CheckPointGetData(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string cmdData = null;
            string[] fields = null;
            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Không thể phân giải dữ liệu, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                fields = cmdData.Split(':');
                if (1 != fields.Length)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Pram gửi lên không hợp lệ, CMD={0}, Client={1}, Recv={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), fields.Length));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                int roleID = Convert.ToInt32(fields[0]);
                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client || client.RoleID != roleID)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("RoleID gửi lên toang, CMD={0}, Client={1}, RoleID={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), roleID));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }
                string CHECKPOINT = client.RoleWelfareData.checkpoint;
                if (CHECKPOINT == "NONE")
                {
                    CHECKPOINT = "";
                }

                _ConfigCard.HistoryRevice = CHECKPOINT;
                _ConfigCard.DayID = TimeUtil.NowDateTime().Day;

                // Gửi đống dữ liệu này về client
                client.SendPacket(nID, DataHelper.ObjectToBytes<CheckPontConfig>(_ConfigCard));
                // DOSOMETHING
                return TCPProcessCmdResults.RESULT_OK;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, "ProcessGetYueKaData", false);
            }

            return TCPProcessCmdResults.RESULT_DATA;
        }

        /// <summary>
        /// Funtion thực hiện điểm danh
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
        public static TCPProcessCmdResults CheckPointGetAward(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string cmdData = null;
            string[] fields = null;
            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Có lỗi khi khi phân giải giữ liệu, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                fields = cmdData.Split(':');
                if (2 != fields.Length)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Dữ liệu gửi lên có lỗi, CMD={0}, Client={1}, Recv={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), fields.Length));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                int roleID = Convert.ToInt32(fields[0]);
                int day = Convert.ToInt32(fields[1]);

                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client || client.RoleID != roleID)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Client ROLEID không đúng, CMD={0}, Client={1}, RoleID={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), roleID));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                int Today = TimeUtil.NowDateTime().Day;

                if (day != Today)
                {
                    KTPlayerManager.ShowNotification(client, "Ngày muốn nhận không hợp lệ");
                    string cmd = string.Format("{0}:{1}:{2}", roleID, -1, day);
                    client.SendPacket(nID, cmd);
                }
                else
                {
                    string ReviceHistory = client.RoleWelfareData.checkpoint;

                    if (ReviceHistory.Split('_').Contains(Today + ""))
                    {
                        KTPlayerManager.ShowNotification(client, "Bạn đã nhận mốc này rồi");
                        string cmd = string.Format("{0}:{1}:{2}", roleID, -2, day);
                        client.SendPacket(nID, cmd);
                    }
                    else
                    {
                        if (client.RoleWelfareData.checkpoint != "NONE")
                        {
                            client.RoleWelfareData.checkpoint = client.RoleWelfareData.checkpoint + "_" + Today;
                        }
                        else
                        {
                            client.RoleWelfareData.checkpoint = Today + "";
                        }

                        // Thực hiện ghi lại dữ liệu vào DB
                        Global.WriterWelfare(client);


                        CheckPointItem _Item = _ConfigCard.CheckPointItem.Where(x => x.Day == day).FirstOrDefault();
                        if (_Item != null)
                        {
                            int ItemID = Int32.Parse(_Item.ItemCard.Split(',')[0]);
                            int Number = Int32.Parse(_Item.ItemCard.Split(',')[1]);

                            if (!ItemManager.CreateItem(Global._TCPManager.TcpOutPacketPool, client, ItemID, Number, 0, "CHECKPOINTEVENT", false, 1, false, ItemManager.ConstGoodsEndTime, "", -1, "", 0, 1, false))
                            {
                                KTPlayerManager.ShowNotification(client, "Có lỗi khi nhận vật phẩm chế tạo");
                            }
                            else
                            {
                                // Gửi về client là mút thành công
                                string cmd = string.Format("{0}:{1}:{2}", roleID, 0, day);
                                client.SendPacket(nID, cmd);
                            }
                        }
                    }
                }

                return TCPProcessCmdResults.RESULT_OK;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, "ProcessGetYueKaData", false);
            }

            return TCPProcessCmdResults.RESULT_DATA;
        }

        #endregion TCP_NETWORK
    }
}