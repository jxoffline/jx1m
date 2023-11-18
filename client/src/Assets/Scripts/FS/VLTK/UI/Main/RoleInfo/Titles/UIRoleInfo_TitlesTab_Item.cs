using System;
using UnityEngine;
using TMPro;
using FS.VLTK.Entities.Config;
using FS.GameEngine.Logic;

namespace FS.VLTK.UI.Main.RoleInfo
{
	/// <summary>
	/// Danh hiệu trong Tab danh hiệu
	/// </summary>
	public class UIRoleInfo_TitlesTab_Item : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Toggle
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Toggle UIToggle;

        /// <summary>
        /// Text Tên danh hiệu
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Name;

        /// <summary>
        /// Đánh dấu đã là danh hiệu hiện tại
        /// </summary>
        [SerializeField]
        private RectTransform UITransform_SetAsCurrentTitle;
        #endregion

        #region Properties
        private KTitleXML _Data;
        /// <summary>
        /// Dữ liệu
        /// </summary>
        public KTitleXML Data
		{
			get
			{
                return this._Data;
			}
			set
			{
                this._Data = value;
                this.UIText_Name.text = value.Text;
            }
		}

        /// <summary>
        /// Đánh dấu có phải danh hiệu hiện tại không
        /// </summary>
        public bool IsCurrentTitle
		{
			get
			{
                return this.UITransform_SetAsCurrentTitle.gameObject.activeSelf;
            }
			set
			{
                this.UITransform_SetAsCurrentTitle.gameObject.SetActive(value);
			}
		}

        /// <summary>
        /// Sự kiện khi danh hiệu được chọn
        /// </summary>
        public Action Select { get; set; }
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
        /// Sự kiện khi danh hiệu được chọn
        /// </summary>
        /// <param name="isSelected"></param>
        private void Toggle_Selected(bool isSelected)
        {
            if (isSelected)
			{
                this.Select?.Invoke();
            }
        }
        #endregion
    }
}
