using FS.GameEngine.Network;
using HSGameEngine.GameEngine.Network.Protocol;
using Server.Data;
using Server.Tools;
using System.Text;

namespace FS.VLTK.Network
{
    /// <summary>
    /// Quản lý tương tác với Socket
    /// </summary>
    public static partial class KT_TCPHandler
    {
        /// <summary>
        /// Nhận gói tin thông báo mở Captcha từ Server
        /// </summary>
        /// <param name="cmdID"></param>
        /// <param name="cmdData"></param>
        /// <param name="length"></param>
        public static void ReceiveCaptcha(int cmdID, byte[] cmdData, int length)
        {
            /// Thông tin Captcha
            G2C_Captcha captcha = DataHelper.BytesToObject<G2C_Captcha>(cmdData, 0, length);
            /// Nếu toác thì thôi
            if (captcha == null || captcha.Data == null)
            {
                return;
            }
            //KTDebug.LogError("Packet size = " + cmdData.Length);
            /// Hiện khung Captcha
            PlayZone.Instance.ShowUICaptcha(captcha);
        }

        /// <summary>
        /// Gửi gói tin trả lời câu hỏi Captcha
        /// </summary>
        /// <param name="answer"></param>
        public static void SendAnswerCaptcha(string answer)
        {
            string strCmd = string.Format("{0}", answer);
            byte[] bytes = new ASCIIEncoding().GetBytes(strCmd);
            GameInstance.Game.GameClient.SendData(TCPOutPacket.MakeTCPOutPacket(GameInstance.Game.GameClient.OutPacketPool, bytes, 0, bytes.Length, (int) (TCPGameServerCmds.CMD_KT_CAPTCHA)));
        }
    }
}
