using FS.VLTK.Utilities.UnityUI;
using System;
using UnityEngine;

namespace FS.VLTK.UI.Main.MainUI
{
    public class UIDecorationText : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Hiệu ứng Text
        /// </summary>
        [SerializeField]
        private UIAnimatedImage UIText_Animation;
        #endregion

        #region Properties
        /// <summary>
        /// Đối tượng có đang hiển thị không
        /// </summary>
        public bool Visible
        {
            get
            {
                return this.UIText_Animation.gameObject.activeSelf;
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

        }
        #endregion

        #region Public methods
        /// <summary>
        /// Hiển thị Text
        /// </summary>
        public void Show()
        {
            this.UIText_Animation.gameObject.SetActive(true);
        }

        /// <summary>
        /// Ẩn đối tượng
        /// </summary>
        public void Hide()
        {
            this.UIText_Animation.gameObject.SetActive(false);
        }
        #endregion
    }
}
