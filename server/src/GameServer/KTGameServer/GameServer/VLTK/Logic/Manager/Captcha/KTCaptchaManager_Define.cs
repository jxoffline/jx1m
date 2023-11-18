using GameServer.Logic;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Định nghĩa
    /// </summary>
    public partial class KTCaptchaManager
    {
        #region Define
        /// <summary>
        /// Thông tin yêu cầu
        /// </summary>
        private class QueueItem
        {
            /// <summary>
            /// Người chơi tương ứng
            /// </summary>
            public KPlayer Player { get; set; }
        }
        #endregion

        #region Consts
        /// <summary>
        /// Thời gian duy trì Captcha
        /// </summary>
        private const int CaptchaDuration = 30000;
        #endregion
    }
}
