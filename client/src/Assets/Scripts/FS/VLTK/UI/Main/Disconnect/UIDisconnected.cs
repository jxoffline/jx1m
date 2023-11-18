using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using System.Collections;
using FS.GameEngine.Logic;

namespace FS.VLTK.UI.Main
{
    /// <summary>
    /// Khung mất kết nối
    /// </summary>
    public class UIDisconnected : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Text thời gian đếm lùi tự thoát
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_CountDownAutoQuit;

        /// <summary>
        /// Button kết nối lại
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Reconnect;

        /// <summary>
        /// Button thoát Game
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_QuitGame;
        #endregion

        #region Private fields
        /// <summary>
        /// Luồng thực thi đếm ngược
        /// </summary>
        private Coroutine countDownCoroutine = null;
        #endregion

        #region Properties
        /// <summary>
        /// Sự kiện kết nối lại
        /// </summary>
        public Action Reconnect { get; set; }

        /// <summary>
        /// Sự kiện thoát Game
        /// </summary>
        public Action QuitGame { get; set; }

        /// <summary>
        /// Thời gian duy trì đếm ngược đến khi tự thoát Game
        /// </summary>
        public float DurationTick { get; set; }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.InitPrefabs();
            this.countDownCoroutine = this.StartCoroutine(this.CountDown());
        }
        #endregion

        #region Code UI
        /// <summary>
        /// Khởi tạo ban đầu
        /// </summary>
        private void InitPrefabs()
        {
            this.UIButton_Reconnect.onClick.AddListener(this.ButtonReconnect_Clicked);
            this.UIButton_QuitGame.onClick.AddListener(this.ButtonQuitGame_Clicked);
        }

        /// <summary>
        /// Sự kiện khi Button kết nối lại được ấn
        /// </summary>
        private void ButtonReconnect_Clicked()
        {
            this.Reconnect?.Invoke();
        }

        /// <summary>
        /// Sự kiện khi Button thoát Game được ấn
        /// </summary>
        private void ButtonQuitGame_Clicked()
        {
            this.QuitGame?.Invoke();
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Thực hiện đếm lùi
        /// </summary>
        /// <returns></returns>
        private IEnumerator CountDown()
        {
            /// Thời gian tồn tại
            float lifeTime = this.DurationTick;
            while (true)
            {
                /// Nếu đã hết thời gian
                if (lifeTime < 0)
                {
                    break;
                }

                /// Hiển thị thời gian đếm ngược lên khung
                this.UIText_CountDownAutoQuit.text = KTGlobal.DisplayTime(lifeTime);
                /// Giảm thời gian xuống 1s
                lifeTime--;

                /// Đợi 1s rồi thực thi tiếp
                yield return new WaitForSeconds(1f);
            }
            /// Thực hiện thoát Game
            this.QuitGame?.Invoke();
            /// Xóa dữ liệu luồng đếm ngược
            this.countDownCoroutine = null;
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Làm mới thời gian đếm lùi
        /// </summary>
        public void RefreshCountDown()
        {
            if (this.countDownCoroutine != null)
            {
                this.StopCoroutine(this.countDownCoroutine);
            }
            this.countDownCoroutine = this.StartCoroutine(this.CountDown());
        }
        #endregion
    }
}
