using UnityEngine;
using TMPro;
using System;

namespace FS.VLTK.UI.Main.Captcha
{
    /// <summary>
    /// Toggle câu trả lời Captcha
    /// </summary>
    public class UICaptcha_ToggleAnswer : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Toggle
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Toggle UIToggle;

        /// <summary>
        /// Text câu trả lời
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Answer;
        #endregion

        #region Properties
        /// <summary>
        /// Câu trả lời
        /// </summary>
        public string Text
        {
            get
            {
                return this.UIText_Answer.text;
            }
            set
            {
                this.UIText_Answer.text = value;
            }
        }

        /// <summary>
        /// Sự kiện khi đối tượng được chọn
        /// </summary>
        public Action Selected { get; set; }
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
            this.UIToggle.onValueChanged.AddListener(this.Toggle_Selected);
        }

        /// <summary>
        /// Sự kiện khi Toggle được chọn
        /// </summary>
        /// <param name="isSelected"></param>
        private void Toggle_Selected(bool isSelected)
        {
            if (isSelected)
            {
                this.Selected?.Invoke();
            }
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Chọn
        /// </summary>
        public void Select()
        {
            this.UIToggle.isOn = true;
        }
        #endregion
    }
}
