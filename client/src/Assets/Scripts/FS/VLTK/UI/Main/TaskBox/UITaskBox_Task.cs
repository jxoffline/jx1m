using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using FS.VLTK.Utilities.UnityUI;
using Server.Data;
using FS.VLTK.Entities.Config;

namespace FS.VLTK.UI.Main.TaskBox
{
    /// <summary>
    /// Nhiệm vụ trong danh mục
    /// </summary>
    public class UITaskBox_Task : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Toggle
        /// </summary>
        [SerializeField]
        private UIToggleSprite UIToggle;

        /// <summary>
        /// Text tên nhiệm vụ
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Title;

        /// <summary>
        /// Image đánh dấu nhiệm vụ hoàn thành chưa
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Image UIImage_CompletedMark;
        #endregion

        #region Properties
        private TaskDataXML _Data;
        /// <summary>
        /// Cấu hình nhiệm vụ
        /// </summary>
        public TaskDataXML Data
        {
            get
            {
                return this._Data;
            }
            set
            {
                this._Data = value;
                this.UIText_Title.text = value.Title;
            }
        }

        private bool _Completed = false;
        /// <summary>
        /// Đã hoàn thành nhiệm vụ này chưa
        /// </summary>
        public bool Completed
        {
            get
            {
                return this._Completed;
            }
            set
            {
                this._Completed = value;
                this.UIImage_CompletedMark.gameObject.SetActive(value);
            }
        }

        /// <summary>
        /// Sự kiện khi Toggle nhiệm vụ được chọn
        /// </summary>
        public Action Selected { get; set; }

        /// <summary>
        /// Kích hoạt đối tượng
        /// </summary>
        public bool Active
        {
            get
            {
                return this.UIToggle.Active;
            }
            set
            {
                this.UIToggle.Active = value;
            }
        }

        /// <summary>
        /// Đối tượng có tương tác được không
        /// </summary>
        public bool Enable
        {
            get
            {
                return this.UIToggle.Enable;
            }
            set
            {
                this.UIToggle.Enable = value;
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
            this.UIToggle.OnSelected = this.Toggle_Selected;
            this.UIToggle.Group = this.transform.parent.GetComponent<UnityEngine.UI.ToggleGroup>();
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
    }
}
