using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

namespace FS.VLTK.Utilities.UnityUI
{
    /// <summary>
    /// Đối tượng Slider có Text báo cáo %
    /// </summary>
    public class UISliderText : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Đối tượng Slider
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Slider UISlider;

        /// <summary>
        /// Text báo cáo tiến trình
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_ReportProgress;

        /// <summary>
        /// Format dữ liệu
        /// </summary>
        [SerializeField]
        private string _ProgressTextFormat = "";
        #endregion

        #region Private fields
        /// <summary>
        /// Đã thực hiện hàm Start chưa
        /// </summary>
        private bool isStarted = false;
        #endregion

        #region Properties
        /// <summary>
        /// Giá trị tối thiểu
        /// </summary>
        public int MinValue
        {
            get
            {
                return (int) this.UISlider.minValue;
            }
            set
            {
                this.UISlider.minValue = value;
            }
        }

        /// <summary>
        /// Giá trị tối đa
        /// </summary>
        public int MaxValue
        {
            get
            {
                return (int) this.UISlider.maxValue;
            }
            set
            {
                this.UISlider.maxValue = value;
            }
        }

        /// <summary>
        /// Giá trị hiện tại của Slider
        /// </summary>
        public int Value
        {
            get
            {
                return (int) this.UISlider.value;
            }
            set
            {
                this.UISlider.value = value;
                if (!this.isStarted)
                {
                    this.Slider_ValueChanged(value);
                }
            }
        }

        /// <summary>
        /// Format dữ liệu đầu vào
        /// </summary>
        public string ProgressTextFormat
        {
            get
            {
                return this._ProgressTextFormat;
            }
            set
            {
                this._ProgressTextFormat = value;
            }
        }

        /// <summary>
        /// Sự kiện khi giá trị Slider thay đổi
        /// </summary>
        public Action<int> ValueChanged { get; set; }

        /// <summary>
        /// Text giá trị Slider
        /// </summary>
        public string ProgressText
        {
            get
            {
                return this.UIText_ReportProgress.text;
            }
        }

        /// <summary>
        /// Text giá trị Slider đặc biệt
        /// </summary>
        public Func<string> CustomProgressText { get; set; } = null;
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.InitPrefabs();
            this.isStarted = true;
        }
        #endregion

        #region Code UI
        /// <summary>
        /// Khởi tạo ban đầu
        /// </summary>
        private void InitPrefabs()
        {
            this.UISlider.onValueChanged.AddListener(this.Slider_ValueChanged);
        }

        /// <summary>
        /// Sự kiện khi giá trị của Slider thay đổi
        /// </summary>
        /// <param name="value"></param>
        private void Slider_ValueChanged(float value)
        {
            if (this.CustomProgressText == null)
            {
                this.UIText_ReportProgress.text = string.Format("{0}" + this._ProgressTextFormat, (int) value);
            }
            else
            {
                this.UIText_ReportProgress.text = this.CustomProgressText?.Invoke();
            }
            
            this.ValueChanged?.Invoke((int) value);
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Buộc thiết lập hiển thị Text
        /// </summary>
        public void ForceUpdateText()
        {
            this.Slider_ValueChanged(this.Value);
        }
        #endregion
    }
}
