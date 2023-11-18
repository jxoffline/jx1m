using System;
using UnityEngine;
using TMPro;
using System.Collections;

namespace FS.VLTK.UI.Main.Revive
{
    /// <summary>
    /// Khung hồi sinh
    /// </summary>
    public class UIReviveFrame : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Text nội dung
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Message;

        /// <summary>
        /// Button về thành
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_BackToCity;

        /// <summary>
        /// Text thời gian đếm lui
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_CountDown;
        #endregion

        #region Properties
        /// <summary>
        /// Nội dung
        /// </summary>
        public string Message
        {
            get
            {
                return this.UIText_Message.text;
            }
            set
            {
                this.UIText_Message.text = value;
            }
        }

        /// <summary>
        /// Thời gian đếm lùi trước khi tự về thành
        /// </summary>
        public const float CountDownTime = 300;

        /// <summary>
        /// Sự kiện khi Button về thành được ấn
        /// </summary>
        public Action BackToCity { get; set; }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.InitPrefabs();
            this.StartCountDown();
        }
        #endregion

        #region Code UI
        /// <summary>
        /// Khởi tạo ban đầu
        /// </summary>
        private void InitPrefabs()
        {
            this.UIButton_BackToCity.onClick.AddListener(this.ButtonBackToCity_Clicked);
        }

        /// <summary>
        /// Sự kiện khi Button về thành được ấn
        /// </summary>
        private void ButtonBackToCity_Clicked()
        {
            this.BackToCity?.Invoke();
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Thực hiện đếm lùi trước khi mặc định chọn chức năng tự về thành
        /// </summary>
        private void StartCountDown()
        {
            IEnumerator CountDown()
            {
                float lifeTime = UIReviveFrame.CountDownTime;
                this.UIText_CountDown.text = KTGlobal.DisplayTime(lifeTime);
                yield return new WaitForSeconds(1f);
                while (lifeTime > 0)
                {
                    lifeTime -= 1f;
                    this.UIText_CountDown.text = KTGlobal.DisplayTime(lifeTime);
                    yield return new WaitForSeconds(1f);
                }
                lifeTime = 0f;
                this.UIText_CountDown.text = KTGlobal.DisplayTime(lifeTime);

                this.BackToCity?.Invoke();
            }
            this.StartCoroutine(CountDown());
        }
        #endregion
    }
}
