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
using System.Threading.Tasks;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Quản lý thư của người chơi
    /// </summary>
    public static partial class KT_TCPHandler
    {
        /// <summary>
        /// Kiểm tra xem có email không
        /// </summary>
        /// <param name="client"></param>
        /// <param name="sendToClient"></param>
        /// <returns></returns>
        public static bool CheckEmailCount(KPlayer client)
        {
            bool result;
            string cmd = string.Format("{0}:{1}:{2}", client.RoleID, 1, 1);
            int emailCount = Global.SendToDB<int, string>((int) TCPGameServerCmds.CMD_SPR_GETUSERMAILCOUNT, cmd, client.ServerId);
            if (emailCount > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Phản hồi yêu cầu lấy danh sách thư của người chơi
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
        public static TCPProcessCmdResults ProcessGetUserMailListCmd(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
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
                    LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit CMD={0}, Client={1}, Recv={2}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket), fields.Length));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// ID người chơi
                int roleID = Convert.ToInt32(fields[0]);

                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client || client.RoleID != roleID)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Can not find player corresponding ID, CMD={0}, Client={1}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket)));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Nếu thao tác quá nhanh
                if (KTGlobal.GetCurrentTimeMilis() - client.LastCheckMailTick < KTGlobal.LimitMailCheckTick)
                {
                    KTPlayerManager.ShowNotification(client, "Thao tác quá nhanh, hãy từ từ!");
                    return TCPProcessCmdResults.RESULT_OK;
                }
                /// Cập nhật thời điểm kiểm tra thư
                client.LastCheckMailTick = KTGlobal.GetCurrentTimeMilis();

                /// Gửi yêu cầu lên GameDB xử lý
                return Global.TransferRequestToDBServer(tcpMgr, socket, tcpClientPool, tcpRandKey, pool, nID, data, count, out tcpOutPacket, client.ServerId);
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        /// <summary>
        /// Phản hồi yêu cầu đọc thư của người chơi
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
        public static TCPProcessCmdResults ProcessGetUserMailDataCmd(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
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

                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client || client.RoleID != roleID)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Can not find player corresponding ID, CMD={0}, Client={1}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket)));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Nếu thao tác quá nhanh
                if (KTGlobal.GetCurrentTimeMilis() - client.LastCheckMailTick < KTGlobal.LimitMailCheckTick)
                {
                    KTPlayerManager.ShowNotification(client, "Thao tác quá nhanh, hãy từ từ!");
                    return TCPProcessCmdResults.RESULT_OK;
                }
                /// Cập nhật thời điểm kiểm tra thư
                client.LastCheckMailTick = KTGlobal.GetCurrentTimeMilis();

                /// Gửi yêu cầu lên GameDB
                TCPProcessCmdResults result = Global.TransferRequestToDBServer(tcpMgr, socket, tcpClientPool, tcpRandKey, pool, nID, data, count, out tcpOutPacket, client.ServerId);
                KT_TCPHandler.CheckEmailCount(client);
                return result;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        /// <summary>
        /// Phản hồi yêu cầu người chơi lấy vật phẩm từ thư
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
        public static TCPProcessCmdResults ProcessFetchMailGoodsCmd(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
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

                /// ID người chơi
                int roleID = Convert.ToInt32(fields[0]);
                /// ID thư
                int mailID = Convert.ToInt32(fields[1]);

                /// Người chơi tương ứng
                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client || client.RoleID != roleID)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Can not find player corresponding ID, CMD={0}, Client={1}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket)));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Nếu thao tác quá nhanh
                if (KTGlobal.GetCurrentTimeMilis() - client.LastCheckMailTick < KTGlobal.LimitMailCheckTick)
                {
                    KTPlayerManager.ShowNotification(client, "Thao tác quá nhanh, hãy từ từ!");
                    return TCPProcessCmdResults.RESULT_OK;
                }
                /// Cập nhật thời điểm kiểm tra thư
                client.LastCheckMailTick = KTGlobal.GetCurrentTimeMilis();

                string strcmd = "";

                byte[] bytes = null;
                int dataStartPos = 0;
                int dataLen = 0;
                /// Gửi yêu cầu lấy nội dung thư lên GameDB
                Global.RequestToDBServer4(tcpClientPool, pool, (int) TCPGameServerCmds.CMD_DB_GETUSERMAILDATA, cmdData, out bytes, out dataStartPos, out dataLen, client.ServerId);
                /// Nếu thao tác thành công
                if (null == bytes || bytes.Length <= 0 || bytes.Length < dataStartPos + dataLen || dataStartPos < 0 || dataLen < 1)
                {
                    strcmd = string.Format("{0}:{1}:{2}", -100, roleID, mailID);
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                /// Thông tin thư tương ứng
                MailData mailData = DataHelper.BytesToObject<MailData>(bytes, dataStartPos, dataLen);
                if (null == mailData)
                {
                    strcmd = string.Format("{0}:{1}:{2}", -110, roleID, mailID);
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                /// Nếu ID người nhận khác bản thân
                if (mailData.ReceiverRID != roleID)
                {
                    strcmd = string.Format("{0}:{1}:{2}", -115, roleID, mailID);
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                /// Nếu thư không cho phép lấy vật phẩm
                if (mailData.HasFetchAttachment != 1)
                {
                    strcmd = string.Format("{0}:{1}:{2}", -121, roleID, mailID);
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                /// Nếu không có gì để nhận
                if ((mailData.GoodsList != null && mailData.GoodsList.Count <= 0) && mailData.Token <= 0 && mailData.Money <= 0)
                {
                    strcmd = string.Format("{0}:{1}:{2}", -120, roleID, mailID);
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                /// Nếu không thể thêm vật phẩm
                if ((mailData.GoodsList != null && mailData.GoodsList.Count > 0) && !KTGlobal.IsHaveSpace(mailData.GoodsList.Count, client))
                {
                    strcmd = string.Format("{0}:{1}:{2}", -125, roleID, mailID);
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                /// Chuyển lên GameDB
                string[] dbFields = Global.ExecuteDBCmd((int) TCPGameServerCmds.CMD_SPR_FETCHMAILGOODS, cmdData, client.ServerId);
                if (null == dbFields || dbFields.Length != 3)
                {
                    strcmd = string.Format("{0}:{1}:{2}", -130, roleID, mailID);
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                /// Nếu có lỗi gì đó
                if (int.Parse(dbFields[2]) != 1)
                {
                    strcmd = string.Format("{0}:{1}:{2}", -140, roleID, mailID);
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                if (mailData.MailType == 0)
                {
                    /// Nếu có đồng khóa đính kèm
                    if (mailData.Token > 0)
                    {
                        /// Thêm đồng khóa cho người chơi
                        KTPlayerManager.AddBoundToken(client, mailData.Token, "SystemMail");

                        GameManager.SystemServerEvents.AddEvent(string.Format("Add role BoundToken, roleID={0}({1}), Money={2}, newMoney={3}", client.RoleID, client.RoleName, client.BoundToken, mailData.Token), EventLevels.Record);
                    }

                    /// Nếu có bạc khóa đính kèm
                    if (mailData.Money > 0)
                    {
                        /// Thêm bạc khóa cho người chơi
                        KTPlayerManager.AddBoundMoney(client, mailData.Money, "SystemMail");

                        GameManager.SystemServerEvents.AddEvent(string.Format("Add role BoundMoney, roleID={0}({1}), Money={2}, newMoney={3}", client.RoleID, client.RoleName, client.Token, mailData.Money), EventLevels.Record);
                    }
                }

                if (mailData.MailType == 1)
                {
                    /// Nếu có đồng khóa đính kèm
                    if (mailData.Token > 0)
                    {
                        /// Thêm đồng khóa cho người chơi
                        KTPlayerManager.AddToken(client, mailData.Token, "SystemMail");

                        GameManager.SystemServerEvents.AddEvent(string.Format("Add role token, roleID={0}({1}), Money={2}, newMoney={3}", client.RoleID, client.RoleName, client.BoundToken, mailData.Token), EventLevels.Record);
                    }

                    /// Nếu có bạc khóa đính kèm
                    if (mailData.Money > 0)
                    {
                        /// Thêm bạc khóa cho người chơi
                        KTPlayerManager.AddMoney(client, mailData.Money, "SystemMail");

                        GameManager.SystemServerEvents.AddEvent(string.Format("Add role Money, roleID={0}({1}), Money={2}, newMoney={3}", client.RoleID, client.RoleName, client.Token, mailData.Money), EventLevels.Record);
                    }
                }



                /// Nếu danh sách vật phẩm tồn tại
                if (null != mailData.GoodsList && mailData.GoodsList.Count > 0)
                {
                    /// Duyệt danh sách và thêm vào túi người chơi
                    foreach (GoodsData itemGD in mailData.GoodsList)
                    {
                        if (!ItemManager.CreateItem(Global._TCPManager.TcpOutPacketPool, client, itemGD.GoodsID, itemGD.GCount, 0, "SYSTEM_EMAIL", true, itemGD.Binding, false, ItemManager.ConstGoodsEndTime, itemGD.Props, itemGD.Series, "", itemGD.Forge_level))
                        {
                            KTPlayerManager.ShowNotification(client, "Có lỗi khi nhận vật phẩm từ thư!");
                        }
                    }
                }

                /// Gửi gói tin lại cho Client
                strcmd = string.Format("1:{0}:{1}", roleID, mailID);

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }


        /// <summary>
        /// Phản hồi yêu cầu xóa thư của người chơi
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
        public static TCPProcessCmdResults ProcessDeleteUserMailCmd(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
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

                /// ID người chơi
                int roleID = Convert.ToInt32(fields[0]);
                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client || client.RoleID != roleID)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Can not find player corresponding ID, CMD={0}, Client={1}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket)));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Nếu thao tác quá nhanh
                if (KTGlobal.GetCurrentTimeMilis() - client.LastCheckMailTick < KTGlobal.LimitMailCheckTick)
                {
                    KTPlayerManager.ShowNotification(client, "Thao tác quá nhanh, hãy từ từ!");
                    return TCPProcessCmdResults.RESULT_OK;
                }
                /// Cập nhật thời điểm kiểm tra thư
                client.LastCheckMailTick = KTGlobal.GetCurrentTimeMilis();

                /// Gửi yêu cầu lên GameDB
                TCPProcessCmdResults result = Global.TransferRequestToDBServer(tcpMgr, socket, tcpClientPool, tcpRandKey, pool, nID, data, count, out tcpOutPacket, client.ServerId);
                KT_TCPHandler.CheckEmailCount(client);
                return result;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }
    }
}
