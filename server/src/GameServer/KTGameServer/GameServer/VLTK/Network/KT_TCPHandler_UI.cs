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
    /// Quản lý UI
    /// </summary>
    public static partial class KT_TCPHandler
    {
        #region Thông báo NotificationTip

        /// <summary>
        /// Thông báo NotificationTip gửi về Client
        /// </summary>
        /// <param name="client"></param>
        /// <param name="message"></param>
        public static void ShowClientNotificationTip(KPlayer client, string message)
        {
            try
            {
                byte[] cmdData = new UTF8Encoding().GetBytes(message);
                client.SendPacket((int) TCPGameServerCmds.CMD_KT_SHOW_NOTIFICATIONTIP, cmdData);
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }

        #endregion Thông báo NotificationTip

        #region Mở/đóng khung bất kỳ

        /// <summary>
        /// Gửi yêu cầu mở khung bất kỳ
        /// </summary>
        /// <param name="player"></param>
        /// <param name="uiName"></param>
        /// <param name="parameters"></param>
        public static void SendOpenUI(KPlayer player, string uiName, int[] parameters)
        {
            G2C_OpenUI openUI = new G2C_OpenUI()
            {
                UIName = uiName,
                Parameters = parameters.ToList(),
            };
            byte[] cmdData = DataHelper.ObjectToBytes<G2C_OpenUI>(openUI);
            player.SendPacket((int) TCPGameServerCmds.CMD_KT_G2C_OPEN_UI, cmdData);
        }

        /// <summary>
        /// Gửi yêu cầu đóng khung bất kỳ
        /// </summary>
        /// <param name="player"></param>
        /// <param name="uiName"></param>
        public static void SendCloseUI(KPlayer player, string uiName)
        {
            byte[] cmdData = new ASCIIEncoding().GetBytes(uiName);
            player.SendPacket((int) TCPGameServerCmds.CMD_KT_G2C_CLOSE_UI, cmdData);
        }

        #endregion Mở/đóng khung bất kỳ

        #region Message Box
        /// <summary>
        /// Kết quả tương tác của người chơi ở bảng thông báo MessageBox
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
        public static TCPProcessCmdResults ResponseUIMessageBoxResult(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
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
                if (fields.Length != 4)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit CMD={0}, Client={1}, Recv={2}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket), cmdData.Length));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// ID MessageBox
                int msgBoxID = int.Parse(fields[0]);
                /// Loại gói tin
                int type = int.Parse(fields[1]);
                /// Button được Click
                string buttonType = fields[2];
                /// Giá trị trả về
                string retValue = fields[3];

                /// Tìm chủ nhân của gói tin tương ứng
                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Không tìm thấy thông tin người chơi, CMD={0}, Client={1}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket)));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Nếu là mở MessageBox thông thường
                if (type == 0)
                {
                    KMessageBox box = KTMessageBoxManager.FindMessageBox(msgBoxID);
                    /// Nếu không tìm thấy thì toác
                    if (box == null)
                    {
                        return TCPProcessCmdResults.RESULT_FAILED;
                    }

                    /// Nếu không phải Type 0 thì toác
                    if (!(box is KTMessageBox))
                    {
                        return TCPProcessCmdResults.RESULT_FAILED;
                    }

                    KTMessageBox msgBox = box as KTMessageBox;
                    /// Thực hiện sự kiện tương ứng
                    if (buttonType == "OK")
                    {
                        msgBox.OK?.Invoke();
                    }
                    else if (buttonType == "Cancel")
                    {
                        msgBox.Cancel?.Invoke();
                    }
                    else
                    {
                        return TCPProcessCmdResults.RESULT_FAILED;
                    }
                }
                /// Nếu là mở InputNumber
                else if (type == 1)
                {
                    KMessageBox box = KTMessageBoxManager.FindMessageBox(msgBoxID);
                    /// Nếu không tìm thấy thì toác
                    if (box == null)
                    {
                        return TCPProcessCmdResults.RESULT_FAILED;
                    }

                    /// Nếu không phải Type 1 thì toác
                    if (!(box is KTInputNumberBox))
                    {
                        return TCPProcessCmdResults.RESULT_FAILED;
                    }

                    KTInputNumberBox msgBox = box as KTInputNumberBox;
                    /// Thực hiện sự kiện tương ứng
                    if (buttonType == "OK")
                    {
                        msgBox.OK?.Invoke(int.Parse(retValue));
                    }
                    else if (buttonType == "Cancel")
                    {
                        msgBox.Cancel?.Invoke();
                    }
                    else
                    {
                        return TCPProcessCmdResults.RESULT_FAILED;
                    }
                }
                /// Nếu là mở InputString
                else if (type == 2)
				{
                    KMessageBox box = KTMessageBoxManager.FindMessageBox(msgBoxID);
                    /// Nếu không tìm thấy thì toác
                    if (box == null)
                    {
                        return TCPProcessCmdResults.RESULT_FAILED;
                    }

                    /// Nếu không phải Type 2 thì toác
                    if (!(box is KTInputStringBox))
                    {
                        return TCPProcessCmdResults.RESULT_FAILED;
                    }

                    KTInputStringBox msgBox = box as KTInputStringBox;
                    /// Thực hiện sự kiện tương ứng
                    if (buttonType == "OK")
                    {
                        msgBox.OK?.Invoke(retValue);
                    }
                    else if (buttonType == "Cancel")
                    {
                        msgBox.Cancel?.Invoke();
                    }
                    else
                    {
                        return TCPProcessCmdResults.RESULT_FAILED;
                    }
                }
                /// Không phải loại nào thì cho toác
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

        /// <summary>
        /// Gửi yêu cầu mở bảng thông báo MessageBox
        /// </summary>
        /// <param name="player"></param>
        /// <param name="type"></param>
        /// <param name="title"></param>
        /// <param name="text"></param>
        /// <param name="parameters"></param>
        public static void SendOpenMessageBox(KPlayer player, int id, int type, string title, string text, List<string> parameters)
        {
            G2C_ShowMessageBox showMessageBox = new G2C_ShowMessageBox()
            {
                ID = id,
                MessageType = type,
                Text = text,
                Title = title,
                Params = parameters,
            };
            byte[] cmdData = DataHelper.ObjectToBytes<G2C_ShowMessageBox>(showMessageBox);
            player.SendPacket((int) TCPGameServerCmds.CMD_KT_SHOW_MESSAGEBOX, cmdData);
        }
        #endregion
    }
}
