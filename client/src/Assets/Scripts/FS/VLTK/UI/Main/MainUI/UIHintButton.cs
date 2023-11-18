using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FS.VLTK.UI.Main.MainUI
{
    /// <summary>
    /// Button và Hint trạng thái
    /// </summary>
    public class UIHintButton : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Button
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton;

        /// <summary>
        /// Hint
        /// </summary>
        [SerializeField]
        private RectTransform HintDecoration;
        #endregion

        #region Properties
        /// <summary>
        /// Sự kiện Click vào Button
        /// </summary>
        public Action Click { get; set; }

        /// <summary>
        /// Có đang hiển thị không
        /// </summary>
        public bool Visible
        {
            get
            {
                return this.gameObject.activeSelf;
            }
            set
            {
                this.gameObject.SetActive(value);
            }
        }

        /// <summary>
        /// Điểm nhấn
        /// </summary>
        public bool Hint
        {
            get
            {
                return this.HintDecoration.gameObject.activeSelf;
            }
            set
            {
                this.HintDecoration.gameObject.SetActive(value);
            }
        }

        /// <summary>
        /// Kích hoạt không
        /// </summary>
        public bool Enable
        {
            get
            {
                return this.UIButton.interactable;
            }
            set
            {
                this.UIButton.interactable = value;
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
        private void InitPrefabs()
        {
            this.UIButton.onClick.AddListener(this.Button_Clicked);
        }

        /// <summary>
        /// Sự kiện khi Button được ấn
        /// </summary>
        private void Button_Clicked()
        {
            this.Click?.Invoke();
        }
        #endregion
    }
}
