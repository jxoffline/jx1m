using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace FS.VLTK.UI.Main.GMCommand
{
    /// <summary>
    /// Thông tin lệnh GM trong khung nhập lệnh GM
    /// </summary>
    public class UIGMCommand_Item : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Text tên lệnh
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Command;

        /// <summary>
        /// Text chú thích lệnh
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Description;

        /// <summary>
        /// Button chép lệnh vào ô nhập lệnh GM trong khung
        /// </summary>
        [SerializeField]
        private Button UIButton_Paste;
        #endregion

        #region Properties
        /// <summary>
        /// Tên lệnh
        /// </summary>
        public string Command
        {
            get
            {
                return this.UIText_Command.text;
            }
            set
            {
                this.UIText_Command.text = value;
            }
        }

        /// <summary>
        /// Chú thích lệnh
        /// </summary>
        public string Description
        {
            get
            {
                return this.UIText_Description.text;
            }
            set
            {
                this.UIText_Description.text = value;
            }
        }

        /// <summary>
        /// Sự kiện chép lệnh vào ô nhập lệnh GM trong khung
        /// </summary>
        public Action Paste { get; set; }
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
            this.UIButton_Paste.onClick.AddListener(this.ButtonPaste_Clicked);
        }

        /// <summary>
        /// Sự kiện khi Button chép lệnh được ấn
        /// </summary>
        private void ButtonPaste_Clicked()
        {
            this.Paste?.Invoke();
        }
        #endregion
    }
}
