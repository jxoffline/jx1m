using GameServer.KiemThe.Core.Item;
using GameServer.KiemThe.GameEvents;
using GameServer.KiemThe.GameEvents.CargoCarriage;
using GameServer.KiemThe.GameEvents.FengHuoLianCheng;
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
using System.Threading;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Quản lý UI
    /// </summary>
    public static partial class KT_TCPHandler
    {
        #region GiftCode

        /// <summary>
        /// Kích hoạt GiftCode
        /// </summary>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static TCPProcessCmdResults ProcessSpriteGetGiftCodeCmd(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string cmdData = null;

            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Decode data faild, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                string[] fields = cmdData.Split(':');
                if (fields.Length != 2)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit CMD={0}, Client={1}, Recv={2}",
                        (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), fields.Length));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                int roleID = Convert.ToInt32(fields[0]);
                string liPinMa = fields[1];

                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client || client.RoleID != roleID)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Can not find player corresponding ID, CMD={0}, Client={1}, RoleID={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), roleID));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                // Mở 1 luồng mới để kích hoạt giftcode
                // Vì webrequest phụ thuộc vào webserver để response nên không thể bắt luồng hệ thống đợi theo sẽ gây tắc ngẽn tầng TCP
                Thread _Threading = new Thread(() => GiftCodeManager.ActiveGiftCode(client, liPinMa));
                _Threading.Start();

                return TCPProcessCmdResults.RESULT_OK;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        #endregion GiftCode

        #region Bảng xếp hạng

        /// <summary>
        /// Mở bảng xếp hạng
        /// </summary>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static TCPProcessCmdResults ProcessGetPaiHangListCmd(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;

            try
            {
                return Global.TransferRequestToDBServer(tcpMgr, socket, tcpClientPool, tcpRandKey, pool, nID, data, count, out tcpOutPacket, socket.ServerId);
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        #endregion Bảng xếp hạng

        #region Vận tiêu

        /// <summary>
        /// Gửi thông tin nhiệm vụ vận tiêu về Client
        /// </summary>
        /// <param name="player"></param>
        public static void SendReceiveNewCargoCarriageTask(KPlayer player)
        {
            /// Nếu không có nhiệm vụ
            if (player.CurrentCargoCarriageTask == null)
            {
                /// Bỏ qua
                return;
            }

            /// Thông tin quãng đường tương ứng
            CargoCarriage.CargoCarriageData.CargoData pathData = CargoCarriage.Data.Paths.Where(x => x.Type == player.CurrentCargoCarriageTask.Type).FirstOrDefault();
            /// Nếu không tồn tại
            if (pathData == null)
            {
                /// Bỏ qua
                return;
            }

            /// Thông tin phần thưởng tương ứng
            CargoCarriage.CargoCarriageData.AwardData awardData = CargoCarriage.Data.Awards.Where(x => x.Type == player.CurrentCargoCarriageTask.Type).FirstOrDefault();
            /// Nếu không tồn tại
            if (awardData == null)
            {
                /// Bỏ qua
                return;
            }

            /// Dữ liệu tương ứng
            G2C_CargoCarriageTaskData taskData = new G2C_CargoCarriageTaskData()
            {
                SourceNPCName = player.CurrentCargoCarriageTask.BeginNPC.Name,
                SourceMapCode = player.CurrentCargoCarriageTask.MovePath.FromMapID,
                SourceNPCPosX = player.CurrentCargoCarriageTask.BeginNPC.PosX,
                SourceNPCPosY = player.CurrentCargoCarriageTask.BeginNPC.PosY,
                DestinationNPCName = player.CurrentCargoCarriageTask.DoneNPC.Name,
                DestinationMapCode = player.CurrentCargoCarriageTask.MovePath.ToMapID,
                DestinationNPCPosX = player.CurrentCargoCarriageTask.DoneNPC.PosX,
                DestinationNPCPosY = player.CurrentCargoCarriageTask.DoneNPC.PosY,
                Type = player.CurrentCargoCarriageTask.Type,
                RequireMoney = pathData.RequireMoney,
                RequireBoundMoney = pathData.RequireBoundMoney,
                RequireToken = pathData.RequireToken,
                RequireBoundToken = pathData.RequireBoundToken,
                AwardMoney = awardData.Money,
                AwardBoundMoney = awardData.BoundMoney,
                AwardToken = awardData.Token,
                AwardBoundToken = awardData.BoundToken,
                AwardItems = new List<CargoCarriageAwardItemData>(),
            };

            /// Duyệt danh sách quà thưởng
            foreach (CargoCarriage.CargoCarriageData.EventItem awardInfo in awardData.AwardItems)
            {
                /// Thêm vào danh sách
                taskData.AwardItems.Add(new CargoCarriageAwardItemData()
                {
                    ItemID = awardInfo.ItemID,
                    Quantity = awardInfo.Quantity,
                    Bound = awardInfo.Bound,
                });
            }
            /// Gửi gói tin
            player.SendPacket<G2C_CargoCarriageTaskData>((int)TCPGameServerCmds.CMD_KT_NEW_CARGO_CARRIAGE_TASK, taskData);
        }

        /// <summary>
        /// Gửi yêu cầu cập nhật trạng thái nhiệm vụ vận tiêu tới người chơi tương ứng
        /// </summary>
        /// <param name="player"></param>
        /// <param name="state">Trạng thái - 0: Xóa, 1: Đã hoàn thành, 2: Bắt đầu vận tiêu</param>
        /// <param name="otherParam">Các tham biến đi kèm</param>
        public static void SendUpdateCargoCarriageTaskState(KPlayer player, int state, params int[] otherParam)
        {
            /// Chuỗi khác
            string parameterData = "";
            /// Nếu có tham biến khác
            if (otherParam != null && otherParam.Length > 0)
            {
                parameterData = ":" + string.Join(":", otherParam);
            }
            /// Dữ liệu
            string cmdData = string.Format("{0}", state) + parameterData;
            /// Gửi gói tin
            player.SendPacket((int)TCPGameServerCmds.CMD_KT_UPDATE_CARGO_CARRIAGE_TASK_STATE, cmdData);
        }

        #endregion Vận tiêu

        #region Phong Hỏa Liên Thành

        /// <summary>
        /// Truy vấn danh sách bảng xếp hạng Phong Hỏa Liên Thành
        /// </summary>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static TCPProcessCmdResults ProcessGetFHLCScoreboard(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string cmdData = null;

            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Decode data faild, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                /// Toác
                if (!string.IsNullOrEmpty(cmdData))
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit CMD={0}, Client={1}, Recv={2}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket), cmdData));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Người chơi tương ứng
                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Can not find player, CMD={0}, Client={1}", (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(socket)));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Spam click
                if (client.IsSpamClick())
                {
                    KTPlayerManager.ShowNotification(client, "Thao tác quá nhanh, hãy đợi giây lát và thử lại!");
                    return TCPProcessCmdResults.RESULT_OK;
                }
                client.SendClick();

                /// Script điều khiển phụ bản tương ứng
                FengHuoLianCheng_ActivityScript activity = GameMapEventsManager.GetActivityScript<FengHuoLianCheng_ActivityScript>(601);
                /// Nếu không tồn tại
                if (activity == null)
                {
                    KTPlayerManager.ShowNotification(client, "Sự kiện không tồn tại hoặc chưa bắt đầu!");
                    return TCPProcessCmdResults.RESULT_OK;
                }
                /// Nếu không có Script điều khiển hoạt động
                else if (activity.Script == null)
                {
                    KTPlayerManager.ShowNotification(client, "Sự kiện không tồn tại hoặc chưa bắt đầu!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Thông tin xếp hạng tương ứng
                FHLCScoreboardData scoreboard = activity.Script.GetScoreboard(client);
                /// Toác
                if (scoreboard == null)
                {
                    KTPlayerManager.ShowNotification(client, "Bảng xếp hạng chưa được cập nhật!");
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Chuỗi byte gửi đi
                byte[] byteData = DataHelper.ObjectToBytes<FHLCScoreboardData>(scoreboard);

                /// Gửi gói tin
                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, byteData, nID);
                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        #endregion Phong Hỏa Liên Thành
    }
}