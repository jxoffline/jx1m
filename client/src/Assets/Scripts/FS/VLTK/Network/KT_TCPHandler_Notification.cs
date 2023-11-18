using FS.GameEngine.Logic;
using FS.GameEngine.Network;
using HSGameEngine.GameEngine.Network.Protocol;
using Server.Data;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FS.VLTK.Network
{
    /// <summary>
    /// Quản lý tương tác với Socket
    /// </summary>
    public static partial class KT_TCPHandler
    {
        #region NotificationTip
        /// <summary>
        /// Nhận dữ liệu hiển thị NotificationTip
        /// </summary>
        /// <param name="cmdID"></param>
        /// <param name="bytes"></param>
        /// <param name="length"></param>
        public static void ReceiveShowNotificationTip(int cmdID, byte[] bytes, int length)
        {
            if (!Global.Data.PlayGame)
            {
                return;
            }

            string message = new UTF8Encoding().GetString(bytes);
            KTGlobal.AddNotification(message);
        }
        #endregion

        #region Message Box
        /// <summary>
        /// Hiện bảng thông báo
        /// </summary>
        /// <param name="cmdID"></param>
        /// <param name="bytes"></param>
        /// <param name="length"></param>
        public static void ReceiveMessageBox(int cmdID, byte[] bytes, int length)
        {
            try
            {
                G2C_ShowMessageBox showMessageBox = DataHelper.BytesToObject<G2C_ShowMessageBox>(bytes, 0, length);
                if (showMessageBox == null)
                {
                    return;
                }

                /// ID bảng thông báo
                int id = showMessageBox.ID;
                /// Loại thông báo
                int type = showMessageBox.MessageType;

                /// Nếu là mở bảng thông báo thường
                if (type == 0)
                {
                    /// Nếu có tham biến đi kèm
                    if (showMessageBox.Params != null)
					{
                        bool showButtonCancel = int.Parse(showMessageBox.Params[0]) == 1;
                        if (showButtonCancel)
						{
                            KTGlobal.ShowMessageBox(showMessageBox.Title, showMessageBox.Text, () => {
                                string strCmd = string.Format("{0}:{1}:{2}:{3}", id, type, "OK", -1);
                                byte[] bytes = new ASCIIEncoding().GetBytes(strCmd);
                                GameInstance.Game.GameClient.SendData(TCPOutPacket.MakeTCPOutPacket(GameInstance.Game.GameClient.OutPacketPool, bytes, 0, bytes.Length, (int) (TCPGameServerCmds.CMD_KT_SHOW_MESSAGEBOX)));
                            }, () => {
                                string strCmd = string.Format("{0}:{1}:{2}:{3}", id, type, "Cancel", -1);
                                byte[] bytes = new ASCIIEncoding().GetBytes(strCmd);
                                GameInstance.Game.GameClient.SendData(TCPOutPacket.MakeTCPOutPacket(GameInstance.Game.GameClient.OutPacketPool, bytes, 0, bytes.Length, (int) (TCPGameServerCmds.CMD_KT_SHOW_MESSAGEBOX)));
                            });
                        }
						else
						{
                            KTGlobal.ShowMessageBox(showMessageBox.Title, showMessageBox.Text, () => {
                                string strCmd = string.Format("{0}:{1}:{2}:{3}", id, type, "OK", -1);
                                byte[] bytes = new ASCIIEncoding().GetBytes(strCmd);
                                GameInstance.Game.GameClient.SendData(TCPOutPacket.MakeTCPOutPacket(GameInstance.Game.GameClient.OutPacketPool, bytes, 0, bytes.Length, (int) (TCPGameServerCmds.CMD_KT_SHOW_MESSAGEBOX)));
                            });
                        }
					}
                }
                /// Nếu là mở bảng nhập số
                else if (type == 1)
                {
                    int minValue = int.MinValue, maxValue = int.MaxValue, value = 0;
                    if (showMessageBox.Params != null)
                    {
                        string[] parameters = showMessageBox.Params.ToArray();
                        try
                        {
                            minValue = int.Parse(parameters[0]);
                            maxValue = int.Parse(parameters[1]);
                            value = int.Parse(parameters[2]);
                        }
                        catch (Exception) { }
                    }

                    KTGlobal.ShowInputNumber(showMessageBox.Text, minValue, maxValue, value, (value) => {
                        string strCmd = string.Format("{0}:{1}:{2}:{3}", id, type, "OK", value);
                        byte[] bytes = new ASCIIEncoding().GetBytes(strCmd);
                        GameInstance.Game.GameClient.SendData(TCPOutPacket.MakeTCPOutPacket(GameInstance.Game.GameClient.OutPacketPool, bytes, 0, bytes.Length, (int) (TCPGameServerCmds.CMD_KT_SHOW_MESSAGEBOX)));
                    }, () => {
                        string strCmd = string.Format("{0}:{1}:{2}:{3}", id, type, "Cancel", -1);
                        byte[] bytes = new ASCIIEncoding().GetBytes(strCmd);
                        GameInstance.Game.GameClient.SendData(TCPOutPacket.MakeTCPOutPacket(GameInstance.Game.GameClient.OutPacketPool, bytes, 0, bytes.Length, (int) (TCPGameServerCmds.CMD_KT_SHOW_MESSAGEBOX)));
                    });
                }
                /// Nếu là mở bảng nhập chuỗi
                else if (type == 2)
				{
                    string initValue = "";
                    if (showMessageBox.Params != null)
                    {
                        string[] parameters = showMessageBox.Params.ToArray();
                        try
						{
                            initValue = parameters[0];
						}
                        catch (Exception) { }
                    }

                    KTGlobal.ShowInputString(showMessageBox.Text, initValue, (value) => {
                        string strCmd = string.Format("{0}:{1}:{2}:{3}", id, type, "OK", value);
                        byte[] bytes = new ASCIIEncoding().GetBytes(strCmd);
                        GameInstance.Game.GameClient.SendData(TCPOutPacket.MakeTCPOutPacket(GameInstance.Game.GameClient.OutPacketPool, bytes, 0, bytes.Length, (int) (TCPGameServerCmds.CMD_KT_SHOW_MESSAGEBOX)));
                    }, () => {
                        string strCmd = string.Format("{0}:{1}:{2}:{3}", id, type, "Cancel", -1);
                        byte[] bytes = new ASCIIEncoding().GetBytes(strCmd);
                        GameInstance.Game.GameClient.SendData(TCPOutPacket.MakeTCPOutPacket(GameInstance.Game.GameClient.OutPacketPool, bytes, 0, bytes.Length, (int) (TCPGameServerCmds.CMD_KT_SHOW_MESSAGEBOX)));
                    });
                }
            }
            catch (Exception) { }
        }
        #endregion

        #region Công cáo sự kiện, hoạt động, phụ bản
        /// <summary>
        /// Nhận gói tin cập nhật thông tin khung Mini hoạt động phụ bản
        /// </summary>
        /// <param name="cmdID"></param>
        /// <param name="bytes"></param>
        /// <param name="length"></param>
        public static void ReceiveEventNotification(int cmdID, byte[] bytes, int length)
        {
            try
            {
                G2C_EventNotification notification = DataHelper.BytesToObject<G2C_EventNotification>(bytes, 0, length);

                /// Nếu khung tồn tại
                if (PlayZone.Instance.UIEventBroadboardMini != null)
                {
                    /// Cập nhật hiển thị
                    PlayZone.Instance.UIEventBroadboardMini.EventName = notification.EventName;
                    PlayZone.Instance.UIEventBroadboardMini.ShortDesc = notification.ShortDetail;
                    PlayZone.Instance.UIEventBroadboardMini.CustomContents = notification.TotalInfo;
                }
            }
            catch (Exception) { }
        }

        /// <summary>
        /// Nhận gói tin thông báo trạng thái hoạt động phụ bản sự kiện thay đổi
        /// </summary>
        /// <param name="cmdID"></param>
        /// <param name="bytes"></param>
        /// <param name="length"></param>
        public static void ReceiveEventStateChange(int cmdID, byte[] bytes, int length)
        {
            try
            {
                G2C_EventState state = DataHelper.BytesToObject<G2C_EventState>(bytes, 0, length);

                /// Nếu khung tồn tại
                if (PlayZone.Instance.UIEventBroadboardMini != null)
                {
                    /// Đánh dấu có đang hiện khung hoạt động
                    Global.Data.ShowUIMiniEventBroadboard = state.State == 1;

                    /// Nếu là hiện
                    if (state.State == 1)
                    {
                        /// Hiện lên
                        PlayZone.Instance.UIEventBroadboardMini.gameObject.SetActive(true);
                        PlayZone.Instance.UIEventBroadboardMini.EventName = "";
                        PlayZone.Instance.UIEventBroadboardMini.CustomContents = new List<string>();
                        PlayZone.Instance.UIEventBroadboardMini.ShortDesc = "";
                        /// Ẩn khung Mini
                        PlayZone.Instance.UIMiniTaskAndTeamFrame.gameObject.SetActive(false);
                        /// Ẩn khung người chơi xung quanh
                        PlayZone.Instance.UINearbyPlayer.Hide();

                        /// Hiện Button tương ứng
                        PlayZone.Instance.UISpecialEventButtons.SetButtonState(state.EventID, true);
                    }
                    /// Nếu là ẩn
                    else
                    {
                        /// Ẩn đi
                        PlayZone.Instance.UIEventBroadboardMini.gameObject.SetActive(false);
                        /// Hiện khung Mini
                        PlayZone.Instance.UIMiniTaskAndTeamFrame.gameObject.SetActive(true);
                        /// Ẩn khung người chơi xung quanh
                        PlayZone.Instance.UINearbyPlayer.Hide();

                        /// Ẩn Button tương ứng
                        PlayZone.Instance.UISpecialEventButtons.SetButtonState(state.EventID, false);
                    }
                }
            }
            catch (Exception) { }
        }

        /// <summary>
        /// Nhận gói tin thông báo số liên trảm có được
        /// </summary>
        /// <param name="cmdID"></param>
        /// <param name="bytes"></param>
        /// <param name="length"></param>
        public static void ReceiveStreakKill(int cmdID, byte[] bytes, int length)
        {
            try
            {
                G2C_KillStreak streakInfo = DataHelper.BytesToObject<G2C_KillStreak>(bytes, 0, length);

                /// Hiển thị khung
                if (PlayZone.Instance.UIStreakKillNotification != null)
                {
                    PlayZone.Instance.UIStreakKillNotification.KillNumber = streakInfo.KillNumber;
                    PlayZone.Instance.UIStreakKillNotification.Show();
                }
            }
            catch (Exception) { }
        }
        #endregion
    }
}
