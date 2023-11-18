using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Quản lý Captcha chống Auto
    /// </summary>
    public partial class KTCaptchaManager
    {
        #region Singleton - Instance
        /// <summary>
        /// Quản lý Captcha chống Auto
        /// </summary>
        public static KTCaptchaManager Instance { get; private set; }

        /// <summary>
        /// Private constructor
        /// </summary>
        private KTCaptchaManager() : base()
        {
            this.StartTimer();
        }
        #endregion

        #region Initialize
        /// <summary>
        /// Hàm này gọi đến khởi tạo đối tượng
        /// </summary>
        public static void Init()
        {
            KTCaptchaManager.Instance = new KTCaptchaManager();
        }
        #endregion
    }
}
