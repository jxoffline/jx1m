using FS.VLTK.Utilities.UnityUI;
using System;
using UnityEngine;

namespace FS.VLTK.UI.Main.MainUI
{
    /// <summary>
    /// Các Button khác
    /// </summary>
    public class UIMainButtons : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Toggle Auto
        /// </summary>
        [SerializeField]
        private UIToggleSprite UIToggle_Auto;
        #endregion

        #region Properties
        /// <summary>
        /// Kích hoạt Auto
        /// </summary>
        public bool ActiveAuto
        {
            get
            {
                return this.UIToggle_Auto.Active;
            }
            set
            {
                /// Nếu trạng thái lần trước khác trạng thái hiện tại
                if (this.lastAutoActiveState != value)
                {
                    /// Đánh dấu trạng thái lần trước
                    this.lastAutoActiveState = value;
                    /// Ghi lại dữ liệu
                    this.UIToggle_Auto.Active = value;
                }
            }
        }

        /// <summary>
        /// Sự kiện kích hoạt Auto
        /// </summary>
        public Action<bool> OnAutoActivated { get; set; }
        #endregion

        #region Private fields
        /// <summary>
        /// Trạng thái lần trước có kích hoạt Auto không
        /// </summary>
        private bool lastAutoActiveState = false;
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
            this.UIToggle_Auto.OnSelected = this.ToggleAuto_Selected;
            this.UIToggle_Auto.GetComponent<UIHoverableObject>().Hover = () =>
            {
                /// Mở khung thiết lập Auto
                PlayZone.Instance.OpenUIAutoFight();
            };
        }

        /// <summary>
        /// Sự kiện khi Toggle Auto được ấn
        /// </summary>
        /// <param name="isSelected"></param>
        private void ToggleAuto_Selected(bool isSelected)
        {
            /// Nếu trạng thái lần trước khác trạng thái hiện tại
            if (this.lastAutoActiveState != isSelected)
            {
                /// Đánh dấu trạng thái lần trước
                this.lastAutoActiveState = isSelected;
                /// Thực thi sự kiện
                this.OnAutoActivated?.Invoke(isSelected);
            }
        }
        #endregion
    }
}
