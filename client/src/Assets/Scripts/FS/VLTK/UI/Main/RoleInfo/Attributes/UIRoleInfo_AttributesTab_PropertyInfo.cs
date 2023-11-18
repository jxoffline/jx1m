using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

namespace FS.VLTK.UI.Main.RoleInfo
{
    /// <summary>
    /// Thông tin thuộc tính ở Tab thuộc tính khác trong khung nhân vật
    /// </summary>
    public class UIRoleInfo_AttributesTab_PropertyInfo : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Text tên thuộc tính
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_PropertyName;

        /// <summary>
        /// Text giá trị thuộc tính
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_PropertyValue;
        #endregion

        #region Properties
        /// <summary>
        /// Tên thuộc tính
        /// </summary>
        public string PropertyName
        {
            get
            {
                return this.UIText_PropertyName.text;
            }
            set
            {
                this.UIText_PropertyName.text = value;
            }
        }

        private int _PropertyValue;
        /// <summary>
        /// Giá trị thuộc tính
        /// </summary>
        public int PropertyValue
        {
            get
            {
                return this._PropertyValue;
            }
            set
            {
                this._PropertyValue = value;
                this.UIText_PropertyValue.text = value.ToString();
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

        #region Private methods
        /// <summary>
        /// Khởi tạo ban đầu
        /// </summary>
        private void InitPrefabs()
        {

        }
        #endregion
    }
}
