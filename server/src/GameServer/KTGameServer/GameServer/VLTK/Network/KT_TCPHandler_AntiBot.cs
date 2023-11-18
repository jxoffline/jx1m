using GameServer.Logic;
using GameServer.Server;
using Server.Data;
using Server.Protocol;
using Server.TCP;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Quản lý động tác của người chơi
    /// </summary>
    public static partial class KT_TCPHandler
    {
        /// <summary>
        /// Xử lý yêu cầu từ Client trả lời câu hỏi Captcha
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
        public static TCPProcessCmdResults ResponseCaptchaAnswer(TCPManager tcpMgr, TMSKSocket socket, TCPClientPool tcpClientPool, TCPRandKey tcpRandKey, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string cmdData = null;

            try
            {
                cmdData = new ASCIIEncoding().GetString(data);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Error while getting DATA, CMD={0}, Client={1}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket)));
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            try
            {
                /// Người chơi tương ứng
                KPlayer client = KTPlayerManager.Find(socket);
                if (null == client)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Can not find player, CMD={0}, Client={1}", (TCPGameServerCmds) nID, Global.GetSocketRemoteEndPoint(socket)));
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                /// Nếu không có câu trả lời
                if (string.IsNullOrEmpty(cmdData))
                {
                    /// Vào nhà lao
                    client.SendToJail();
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Nếu không có Captcha
                if (client.CurrentCaptcha == null)
                {
                    /// Vào nhà lao
                    client.SendToJail();
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Nếu câu trả lời không khớp
                if (client.CurrentCaptcha.Text != cmdData)
                {
                    /// Nếu đang ở nhà lao
                    if (client.CurrentMapCode == KTGlobal.JailMapCode)
                    {
                        KTPlayerManager.ShowNotification(client, "Câu trả lời không chính xác, thẩm vấn lại sau!");
                    }
                    /// Nếu không phải đang ở nhà lao
                    else
                    {
                        KTPlayerManager.ShowNotification(client, "Câu trả lời không chính xác, tự chuyển vào nhà lao!");
                    }
                    
                    /// Hủy Captcha
                    client.RemoveCaptcha();
                    /// Vào nhà lao
                    client.SendToJail();
                    /// Thực thi hàm khi trả lời sai
                    client.AnswerCaptcha?.Invoke(false);
                    /// Hủy hàm Callback khi trả lời Captcha
                    client.AnswerCaptcha = null;
                    /// Trả về kết quả
                    return TCPProcessCmdResults.RESULT_OK;
                }

                /// Nếu đang ở nhà lao
                if (client.CurrentMapCode == KTGlobal.JailMapCode)
                {
                    KTPlayerManager.ShowNotification(client, "Câu trả lời chính xác. Ngươi có thể rời đi rồi!");
                }
                /// Nếu không phải đang ở nhà lao
                else
                {
                    KTPlayerManager.ShowNotification(client, "Câu trả lời chính xác. Cáo lỗi đã làm phiền bằng hữu!");
                }

                /// Cấp độ người chơi
                int nLevel = client.m_Level;
                if (nLevel < 1)
                {
                    nLevel = 1;
                }
                else if (nLevel > 119)
                {
                    nLevel = 119;
                }
                /// Thêm bạc khóa
                KTPlayerManager.AddBoundMoney(client, ServerConfig.Instance.CaptchaBoundMoneyAddPerLevel * nLevel, "AnswerCaptcha");
                /// Thêm kinh nghiệm
                KTPlayerManager.AddExp(client, ServerConfig.Instance.CaptchaExpAddPerLevel * nLevel);
                /// Xóa Captcha
                client.RemoveCaptcha();
                /// Thực thi hàm khi trả lời sai
                client.AnswerCaptcha?.Invoke(true);
                /// Hủy hàm Callback khi trả lời Captcha
                client.AnswerCaptcha = null;
                /// Trả về kết quả
                return TCPProcessCmdResults.RESULT_OK;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(socket), false);
            }

            return TCPProcessCmdResults.RESULT_FAILED;
        }

        /// <summary>
        /// Gửi Captcha về Client
        /// </summary>
        /// <param name="player"></param>
        /// <param name="captchaImgByteData"></param>
        /// <param name="answers"></param>
        public static void SendCaptchaToClient(KPlayer player, byte[] captchaImgByteData, short width, short height, List<string> answers)
        {
            /// Tạo đối tượng tương ứng
            G2C_Captcha captcha = new G2C_Captcha()
            {
                Data = captchaImgByteData,
                Answers = answers,
                Width = width,
                Height = height,
            };
            /// Gửi gói tin đi
            player.SendPacket<G2C_Captcha>((int) TCPGameServerCmds.CMD_KT_CAPTCHA, captcha);
        }
    }
}
