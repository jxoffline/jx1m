using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FS.VLTK.UI.Main.Friend
{
    /// <summary>
    /// Popup thông tin bạn bè được chọn
    /// </summary>
    public class UIFriendBox_InteractionBox : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Button xem thông tin
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_BrowseInfo;

        /// <summary>
        /// Button xóa
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Remove;
        #endregion

        #region Private fields

        #endregion

        #region Properties
        /// <summary>
        /// Sự kiện xem thông tin
        /// </summary>
        public Action BrowseInfo { get; set; }
        
        /// <summary>
        /// Sự kiện xóa
        /// </summary>
        public Action Remove { get; set; }
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
            this.UIButton_BrowseInfo.onClick.AddListener(this.ButtonBrowseInfo_Clicked);
            this.UIButton_Remove.onClick.AddListener(this.ButtonRemove_Clicked);
        }

        /// <summary>
        /// Sự kiện khi Button xem thông tin được ấn
        /// </summary>
        private void ButtonBrowseInfo_Clicked()
        {
            this.BrowseInfo?.Invoke();
        }

        /// <summary>
        /// Sự kiện khi Button xóa được ấn
        /// </summary>
        private void ButtonRemove_Clicked()
        {
            this.Remove?.Invoke();
        }
        #endregion

        #region Private methods

        #endregion

        #region Public methods
        /// <summary>
        /// Hiện khung
        /// </summary>
        public void Show()
        {
            this.gameObject.SetActive(true);
        }

        /// <summary>
        /// Ẩn khung
        /// </summary>
        public void Hide()
        {
            this.gameObject.SetActive(false);
        }
        #endregion
    }
}
