using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using FS.GameEngine.Logic;
using System;
using FS.VLTK.Utilities.UnityUI;

namespace FS.VLTK.UI.Main.MainUI.RolePart
{
    public class UIRolePart_PingInfo : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Text thông tin kết nối
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_PingInfo;

        /// <summary>
        /// Slider thời lượng PIN
        /// </summary>
        [SerializeField]
        private UISliderText UISlider_BaterryLevel;
        #endregion

        #region Private fields
        /// <summary>
        /// Trạng thái
        /// </summary>
        private bool state = false;

        /// <summary>
        /// Khoảng thời gian cộng thêm
        /// </summary>
        private float onceTicks = 0;

        /// <summary>
        /// Thời điểm gửi
        /// </summary>
        private long sendTick = 0;

        /// <summary>
        /// Số PING (ms)
        /// </summary>
        private long serviceRespondTick = 20;
        #endregion

        #region Properties
        /// <summary>
        /// Thời điểm lần trước 
        /// </summary>
        public static long LastReceivePingTick { get; set; } = 0;
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi đến mỗi khi có tương tác vật lý
        /// </summary>
        private void FixedUpdate()
        {
            if (this.state)
            {
                this.onceTicks += Time.fixedDeltaTime;

                if (this.onceTicks >= 10)
                {
                    this.ShowText(9999);
                }
            }
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Hiển thị Text
        /// </summary>
        /// <param name="ticks"></param>
        private void ShowText(float ticks)
        {
            string strColor = "";
            if (ticks <= 300)
            {
                strColor = "#17e43e";
            }
            else if (ticks >= 301 && ticks <= 800)
            {
                strColor = "#fac60d";
            }
            else if (ticks >= 801)
            {
                strColor = "#ff0000";
            }

            string sFPS = MainGame.Instance.FPS.ToString("f" + Mathf.Clamp(1, 0, 10));

            ColorUtility.TryParseHtmlString(strColor, out Color color);
            this.UIText_PingInfo.color = color;
            this.UIText_PingInfo.text = string.Format("Ver: {3} | Ping: {0}ms | FPS: {1} | {2}", ticks, sFPS, DateTime.Now.ToString("HH:mm"), MainGame.Instance.Version);

            /// Cập nhật thời lượng PIN
            this.UISlider_BaterryLevel.Value = (int) (SystemInfo.batteryLevel * 100);
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Gửi Ping đến máy chủ để kiểm tra
        /// </summary>
        public void SendPing()
        {
            this.sendTick = KTGlobal.GetCurrentTimeMilis();
            this.onceTicks = 0;
            this.state = true;
        }

        /// <summary>
        /// Nhận thông tin Ping từ máy chủ
        /// </summary>
        public void ReceivePing()
        {
            this.state = false;
            this.serviceRespondTick = UIRolePart_PingInfo.LastReceivePingTick - this.sendTick;
            this.serviceRespondTick = this.serviceRespondTick > 0 ? this.serviceRespondTick : 20;
            this.ShowText(this.serviceRespondTick);
        }
        #endregion
    }
}

