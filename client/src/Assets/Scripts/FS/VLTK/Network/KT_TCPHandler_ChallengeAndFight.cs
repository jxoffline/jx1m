using FS.GameEngine.Logic;
using FS.GameEngine.Network;
using FS.VLTK.Logic.Settings;
using HSGameEngine.GameEngine.Network.Protocol;
using Server.Data;
using Server.Tools;
using System.Collections.Generic;
using System.Text;

namespace FS.VLTK.Network
{
    /// <summary>
    /// Quản lý tương tác với Socket
    /// </summary>
    public static partial class KT_TCPHandler
    {
        #region Thách đấu
        /// <summary>
        /// Gửi lời mời thách đấu đến người chơi ID tương ứng
        /// </summary>
        /// <param name="roleID"></param>
        public static void SendAskChallenge(int roleID)
        {
            string strCmd = string.Format("{0}", roleID);
            byte[] bytes = new ASCIIEncoding().GetBytes(strCmd);
            GameInstance.Game.GameClient.SendData(TCPOutPacket.MakeTCPOutPacket(GameInstance.Game.GameClient.OutPacketPool, bytes, 0, bytes.Length, (int) (TCPGameServerCmds.CMD_KT_ASK_CHALLENGE)));
        }

        /// <summary>
        /// Nhận gói tin thông báo lời mời thách đấu
        /// </summary>
        /// <param name="fields"></param>
        public static void ReceiveAskChallenge(string[] fields)
        {
            if (!Global.Data.PlayGame)
            {
                return;
            }

            /// ID đối tượng gửi lời mời
            int inviterID = int.Parse(fields[0]);
            /// Tên đối tượng gửi lời mời
            string inviterName = fields[1];

            /// Nếu có thiết lập từ chối thách đấu
            if (KTAutoAttackSetting.Config.General.RefuseChallenge)
            {
                KT_TCPHandler.SendRefuseChallenge(inviterID);
                return;
            }

            KTGlobal.ShowMessageBox("Mời thách đấu", string.Format("Người chơi <color=#66daf4>[{0}]</color> muốn thách đấu với bạn, đồng ý không?", inviterName), () => {
                KT_TCPHandler.SendAgreeChallenge(inviterID);
            }, () => {
                KT_TCPHandler.SendRefuseChallenge(inviterID);
            });
        }

        /// <summary>
        /// Gửi yêu cầu đồng ý lời mời thách đấu
        /// </summary>
        /// <param name="inviterID"></param>
        public static void SendAgreeChallenge(int inviterID)
        {
            string strCmd = string.Format("{0}:{1}", inviterID, 1);
            byte[] bytes = new ASCIIEncoding().GetBytes(strCmd);
            GameInstance.Game.GameClient.SendData(TCPOutPacket.MakeTCPOutPacket(GameInstance.Game.GameClient.OutPacketPool, bytes, 0, bytes.Length, (int) (TCPGameServerCmds.CMD_KT_C2G_RESPONSE_CHALLENGE)));
        }

        /// <summary>
        /// Gửi yêu cầu từ chối lời mời thách đấu
        /// </summary>
        /// <param name="inviterID"></param>
        public static void SendRefuseChallenge(int inviterID)
        {
            string strCmd = string.Format("{0}:{1}", inviterID, 0);
            byte[] bytes = new ASCIIEncoding().GetBytes(strCmd);
            GameInstance.Game.GameClient.SendData(TCPOutPacket.MakeTCPOutPacket(GameInstance.Game.GameClient.OutPacketPool, bytes, 0, bytes.Length, (int) (TCPGameServerCmds.CMD_KT_C2G_RESPONSE_CHALLENGE)));
        }

        /// <summary>
        /// Nhận thông báo có dữ liệu thách đấu
        /// </summary>
        /// <param name="cmdID"></param>
        /// <param name="bytes"></param>
        /// <param name="length"></param>
        public static void ReceiveNewChallengeInfo(int cmdID, byte[] bytes, int length)
        {
            /// Dữ liệu
            RoleChallengeData data = DataHelper.BytesToObject<RoleChallengeData>(bytes, 0, length);
            /// Toác
            if (data == null)
            {
                /// Thông báo lỗi
                KTGlobal.AddNotification("Có lỗi khi nhận dữ liệu thách đấu, hãy thử lại!");
                /// Bỏ qua
                return;
            }

            /// Nếu chưa mở khung
            if (PlayZone.Instance.UIChallenge == null)
            {
                /// Mở khung
                PlayZone.Instance.OpenUIChallenge(data);
            }
            /// Nếu đã mở khung
            else
            {
                /// Đồng bộ dữ liệu
                PlayZone.Instance.UIChallenge.Data = data;
            }
        }

        /// <summary>
        /// Nhận yêu cầu xử lý dữ liệu liên quan đến thách đấu
        /// </summary>
        /// <param name="fields"></param>
        public static void ReceiveDoChallengeCommand(string[] fields)
        {
            /// Nếu chưa mở khung
            if (PlayZone.Instance.UIChallenge == null)
            {
                /// Bỏ qua
                return;
            }

            /// Loại
            int type = int.Parse(fields[0]);

            /// Xem là loại gì
            switch (type)
            {
                /// Đóng khung
                case 0:
                {
                    PlayZone.Instance.CloseUIChallenge();

                    break;
                }
                /// Cập nhật số tiền
                case 1:
                {
                    int firstTeamMoney = int.Parse(fields[1]);
                    int secondTeamMoney = int.Parse(fields[2]);
                    /// Cập nhật vào khung
                    PlayZone.Instance.UIChallenge.UpdateMoney(new Dictionary<int, int>()
                    {
                        { 1, firstTeamMoney },
                        { 2, secondTeamMoney },
                    });

                    break;
                }
                /// Cập nhật trạng thái xác nhận
                case 2:
                {
                    bool firstTeamReadyState = int.Parse(fields[1]) == 1;
                    bool secondTeamReadyState = int.Parse(fields[2]) == 1;
                    /// Cập nhật vào khung
                    PlayZone.Instance.UIChallenge.UpdateReadyState(new Dictionary<int, bool>()
                    {
                        { 1, firstTeamReadyState },
                        { 2, secondTeamReadyState },
                    });

                    break;
                }
            }
        }

        /// <summary>
        /// Gửi yêu cầu thiết lập số tiền cược thách đấu
        /// </summary>
        /// <param name="money"></param>
        public static void SendSetChallengeMoney(int money)
        {
            string strCmd = string.Format("{0}:{1}", 1, money);
            byte[] bytes = new ASCIIEncoding().GetBytes(strCmd);
            GameInstance.Game.GameClient.SendData(TCPOutPacket.MakeTCPOutPacket(GameInstance.Game.GameClient.OutPacketPool, bytes, 0, bytes.Length, (int) (TCPGameServerCmds.CMD_KT_DO_CHALLENGE_COMMAND)));
        }

        /// <summary>
        /// Gửi yêu cầu xác nhận yêu cầu thách đấu
        /// </summary>
        public static void SendChallengeReadyState()
        {
            string strCmd = string.Format("{0}", 2);
            byte[] bytes = new ASCIIEncoding().GetBytes(strCmd);
            GameInstance.Game.GameClient.SendData(TCPOutPacket.MakeTCPOutPacket(GameInstance.Game.GameClient.OutPacketPool, bytes, 0, bytes.Length, (int) (TCPGameServerCmds.CMD_KT_DO_CHALLENGE_COMMAND)));
        }

        /// <summary>
        /// Gửi yêu cầu bắt đầu chiến đấu
        /// </summary>
        public static void SendBeginChallenge()
        {
            string strCmd = string.Format("{0}", 0);
            byte[] bytes = new ASCIIEncoding().GetBytes(strCmd);
            GameInstance.Game.GameClient.SendData(TCPOutPacket.MakeTCPOutPacket(GameInstance.Game.GameClient.OutPacketPool, bytes, 0, bytes.Length, (int) (TCPGameServerCmds.CMD_KT_DO_CHALLENGE_COMMAND)));
        }

        /// <summary>
        /// Gửi yêu cầu hủy thách đấu
        /// </summary>
        public static void SendCancelChallenge()
        {
            string strCmd = string.Format("{0}", -1);
            byte[] bytes = new ASCIIEncoding().GetBytes(strCmd);
            GameInstance.Game.GameClient.SendData(TCPOutPacket.MakeTCPOutPacket(GameInstance.Game.GameClient.OutPacketPool, bytes, 0, bytes.Length, (int) (TCPGameServerCmds.CMD_KT_DO_CHALLENGE_COMMAND)));
        }
        #endregion

        #region Tuyên chiến
        /// <summary>
        /// Gửi yêu cầu tuyên chiến với người chơi có ID tương ứng
        /// </summary>
        /// <param name="roleID"></param>
        public static void SendActiveFight(int roleID)
        {
            string strCmd = string.Format("{0}", roleID);
            byte[] bytes = new ASCIIEncoding().GetBytes(strCmd);
            GameInstance.Game.GameClient.SendData(TCPOutPacket.MakeTCPOutPacket(GameInstance.Game.GameClient.OutPacketPool, bytes, 0, bytes.Length, (int) (TCPGameServerCmds.CMD_KT_ASK_ACTIVEFIGHT)));
        }

        /// <summary>
        /// Nhận thông báo bắt đầu tuyên chiến
        /// </summary>
        /// <param name="fields"></param>
        public static void ReceiveBeginActiveFight(string[] fields)
        {
            if (!Global.Data.PlayGame)
            {
                return;
            }

            /// ID đối tượng tuyên chiến
            int targetID = int.Parse(fields[0]);
            /// Tên đối tượng tuyên chiến
            string targetName = fields[1];

            if (!KTGlobal.ActiveFightWith.Contains(targetID))
            {
                KTGlobal.ActiveFightWith.Add(targetID);
            }
        }

        /// <summary>
        /// Nhận thông báo kết thúc tuyên chiến
        /// </summary>
        /// <param name="fields"></param>
        public static void RecieveStopActiveFight(string[] fields)
        {
            if (!Global.Data.PlayGame)
            {
                return;
            }

            /// ID đối tượng tuyên chiến
            int targetID = int.Parse(fields[0]);
            /// Tên đối tượng tuyên chiến
            string targetName = fields[1];

            if (KTGlobal.ActiveFightWith.Contains(targetID))
            {
                KTGlobal.ActiveFightWith.Remove(targetID);
            }
        }
        #endregion
    }
}
