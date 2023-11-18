using System;
using TMPro;
using UnityEngine;

namespace FS.VLTK.UI.Main.GuildEx.Battle
{
    /// <summary>
    /// Thông tin thành viên trong khung Công Thành Chiến
    /// </summary>
    public class UIGuildEx_Battle_MemberInfo : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Button thêm thành viên
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Add;

        /// <summary>
        /// Text tên thành viên
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Name;

        /// <summary>
        /// Button xóa thành viên
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Remove;
        #endregion

        #region Properties
        /// <summary>
        /// ID nhân vật
        /// </summary>
        public int RoleID { get; set; }

        private string _Data;
        /// <summary>
        /// Thông tin thành viên
        /// </summary>
        public string Data
        {
            get
            {
                return this._Data;
            }
            set
            {
                this._Data = value;

                /// Nếu không có thành viên
                if (value == null)
                {
                    /// Hiện Button thêm
                    this.UIButton_Add.gameObject.SetActive(true);
                    /// Ẩn Button xóa thành viên
                    this.UIButton_Remove.gameObject.SetActive(false);
                    /// Ẩn Text
                    this.UIText_Name.gameObject.SetActive(false);
                }
                /// Nếu có thành viên
                else
                {
                    /// Ẩn Button thêm
                    this.UIButton_Add.gameObject.SetActive(false);
                    /// Hiện Button xóa thành viên
                    this.UIButton_Remove.gameObject.SetActive(true);
                    /// Hiện Text
                    this.UIText_Name.gameObject.SetActive(true);

                    /// Cập nhật dữ liệu
                    this.UIText_Name.text = value;
                }
            }
        }

        private bool _EnableEdit;
        /// <summary>
        /// Cho phép chỉnh sửa
        /// </summary>
        public bool EnableEdit
        {
            get
            {
                return this._EnableEdit;
            }
            set
            {
                this._EnableEdit = value;
                /// Button tương ứng
                this.UIButton_Add.interactable = value;
                this.UIButton_Remove.interactable = value;
            }
        }

        /// <summary>
        /// Sự kiện thêm thành viên
        /// </summary>
        public Action Add { get; set; }

        /// <summary>
        /// Sự kiện xóa thành viên
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
            this.UIButton_Add.onClick.AddListener(this.ButtonAdd_Clicked);
            this.UIButton_Remove.onClick.AddListener(this.ButtonRemove_Clicked);
        }

        /// <summary>
        /// Sự kiện khi Button thêm thành viên được ấn
        /// </summary>
        private void ButtonAdd_Clicked()
        {
            this.Add?.Invoke();
        }

        /// <summary>
        /// Sự kiện khi Button xóa thành viên được ấn
        /// </summary>
        private void ButtonRemove_Clicked()
        {
            this.Remove?.Invoke();
        }
        #endregion
    }
}
