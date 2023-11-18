using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using FS.VLTK.Utilities.UnityUI;

namespace FS.VLTK.UI.Main
{
    /// <summary>
    /// Khung trạng thái đang tải dữ liệu
    /// </summary>
    public class UILoadingProgress : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Ảnh hiệu ứng tải
        /// </summary>
        [SerializeField]
        private UIAnimatedSprite UIImage_ProgressBar;

        /// <summary>
        /// Text Hint thông tin tải
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_LoadingHint;
        #endregion

        #region Properties
        /// <summary>
        /// Hint thông tin tải
        /// </summary>
        public string Hint
        {
            get
            {
                return this.UIText_LoadingHint.text;
            }
            set
            {
                this.UIText_LoadingHint.text = value;
            }
        }

        /// <summary>
        /// Đối tượng có đang hiển thị không
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
            this.UIImage_ProgressBar.AutoPlay = true;
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Hiện khung
        /// </summary>
        public void Show()
        {
            this.gameObject.SetActive(true);
            this.UIImage_ProgressBar.Play();
        }

        /// <summary>
        /// Ẩn khung
        /// </summary>
        public void Hide()
        {
            this.gameObject.SetActive(false);
            this.UIImage_ProgressBar.Stop();
        }
        #endregion
    }
}
