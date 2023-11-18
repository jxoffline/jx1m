using FS.VLTK.Utilities.UnityUI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using System.Collections;

namespace FS.VLTK.UI.Main.MainUI
{
    /// <summary>
    /// Thanh Progress Bar dùng để thu thập hoặc thao tác
    /// </summary>
    public class UIProgressBar : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Slider
        /// </summary>
        [SerializeField]
        private UISliderText UISlider;

        /// <summary>
        /// Text mô tả thao tác
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Desc;
        #endregion

        #region Properties
        /// <summary>
        /// Sự kiện hoàn tất
        /// </summary>
        public Action Complete { get; set; }

        /// <summary>
        /// Sự kiện khi giá trị thanh Progress thay đổi
        /// </summary>
        public Action<int> ProgressChanged { get; set; }

        /// <summary>
        /// Thời gian tồn tại
        /// </summary>
        public float Duration { get; set; }

        /// <summary>
        /// Thời gian hiện tại
        /// </summary>
        public float CurrentLifeTime { get; set; }

        /// <summary>
        /// Text nội dung
        /// </summary>
        public string Text
        {
            get
            {
                return this.UIText_Desc.text;
            }
            set
            {
                this.UIText_Desc.text = value;
            }
        }

        /// <summary>
        /// Có đang hiển thị không
        /// </summary>
        public bool Visible
        {
            get
            {
                return this.gameObject.activeSelf;
            }
        }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.InitPrefabs();
        }
        #endregion

        #region Code UI
        /// <summary>
        /// Khởi tạo ban đầu
        /// </summary>
        private void InitPrefabs()
        {
            this.UISlider.ProgressTextFormat = "%";
            this.UISlider.ValueChanged = this.Slider_ValueChanged;
        }

        /// <summary>
        /// Sự kiện khi giá trị của Slider thay đổi
        /// </summary>
        /// <param name="value"></param>
        private void Slider_ValueChanged(int value)
        {
            this.ProgressChanged?.Invoke(value);
        }
        #endregion

        #region Private fields
        /// <summary>
        /// Thực thi Progress Bar
        /// </summary>
        /// <returns></returns>
        private IEnumerator DoPlay()
        {
            float currentLifeTime = this.CurrentLifeTime;
            
            while (true)
            {
                if (currentLifeTime > this.Duration)
                {
                    break;
                }
                float percent = currentLifeTime / this.Duration;
                this.UISlider.Value = (int) (percent * 100f);

                currentLifeTime += Time.deltaTime;
                yield return null;
            }

            this.UISlider.Value = 100;
            this.Complete?.Invoke();
        }

        /// <summary>
        /// Làm mới lại tham biến
        /// </summary>
        private void ResetParams()
        {
            this.Complete = null;
            this.ProgressChanged = null;
            this.CurrentLifeTime = 0f;
            this.Duration = 0f;
            this.Text = "";
        }
        #endregion

        #region Public fields
        /// <summary>
        /// Bắt đầu chạy Progress
        /// </summary>
        public void StartProgress()
        {
            this.gameObject.SetActive(true);
            this.StartCoroutine(this.DoPlay());
        }

        /// <summary>
        /// Đóng thanh chạy Progress
        /// </summary>
        public void CloseProgress()
        {
            this.gameObject.SetActive(false);
            this.StopAllCoroutines();
            this.ResetParams();
        }
        #endregion
    }
}
