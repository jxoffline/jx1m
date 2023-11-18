using GameServer.KiemThe.Entities.Player;
using GameServer.KiemThe.Utilities.Libraries;
using GameServer.Logic;
using System.Collections.Generic;
using System.IO;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Logic
    /// </summary>
    public partial class KTCaptchaManager
    {
        /// <summary>
        /// Tạo Captcha cho người chơi tương ứng
        /// </summary>
        /// <returns></returns>
        private void DoGenerate(KPlayer player)
        {
            /// Toác
            if (player == null)
            {
                return;
            }
            /// Nêu đã Offline
            else if (!player.IsOnline())
            {
                return;
            }

            /// Tạo 1 số Random gồm 4 chữ số
            int randomNumber = KTGlobal.GetRandomNumber(1000, 9999);
            /// Captcha
            string captchaText = randomNumber.ToString();

            /// Kích thước
            short width = 40;
            short height = 15;

            /// Giá trị nhiễu ngẫu nhiên
            int randomDistortion = KTGlobal.GetRandomNumber(3, 5);

            /// Tạo Captcha qua MemoryStream
            MemoryStream imageStream = CaptchaImageGenerator.GenerateImage(captchaText, height, width, 8, randomDistortion);
            imageStream.Position = 0;

            /// Chuỗi Byte mã hóa cho ảnh
            byte[] imageByteData = imageStream.ToArray();

            /// Đóng Stream
            imageStream.Close();

            /// Lưu thông tin
            player.CurrentCaptcha = new KPlayer_Captcha()
            {
                Text = randomNumber.ToString(),
                Duration = KTCaptchaManager.CaptchaDuration,
                StartTick = KTGlobal.GetCurrentTimeMilis(),
            };

            /// Tạo 4 câu trả lời
            List<string> answers = new List<string>()
            {
                KTGlobal.GetRandomNumber(1000, 9999).ToString(),
                KTGlobal.GetRandomNumber(1000, 9999).ToString(),
                KTGlobal.GetRandomNumber(1000, 9999).ToString(),
                KTGlobal.GetRandomNumber(1000, 9999).ToString(),
            };
            /// Gắn câu trả lời vào vị trí ngẫu nhiên
            answers[KTGlobal.GetRandomNumber(0, answers.Count - 1)] = captchaText;

            /// Thông báo
            KTPlayerManager.ShowNotification(player, "Kích hoạt Captcha chống Bot, vui lòng trả lời trong 30 giây!");
            /// Gửi về Client
            KT_TCPHandler.SendCaptchaToClient(player, imageByteData, width, height, answers);
        }
    }
}
