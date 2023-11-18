using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace FS.VLTK.UI.Main.MessageBox
{
    /// <summary>
    /// Bảng thông báo IN-GAME
    /// </summary>
    public class UIMessageBox : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Text tiêu đề khung
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Title;

        /// <summary>
        /// Text nội dung thông báo
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Content;

        /// <summary>
        /// Button đồng ý
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_OK;

        /// <summary>
        /// Button hủy bỏ
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Cancel;
        #endregion

        #region Properties
        /// <summary>
        /// Tiêu đề khung
        /// </summary>
        public string Title
        {
            get
            {
                return this.UIText_Title.text;
            }
            set
            {
                this.UIText_Title.text = value;
            }
        }

        /// <summary>
        /// Nội dung
        /// </summary>
        public string Content
        {
            get
            {
                return this.UIText_Content.text;
            }
            set
            {
                this.UIText_Content.text = value;
            }
        }

        /// <summary>
        /// Hiện Button hủy bỏ
        /// </summary>
        public bool ShowButtonCancel
        {
            get
            {
                return this.UIButton_Cancel.gameObject.activeSelf;
            }
            set
            {
                this.UIButton_Cancel.gameObject.SetActive(value);
            }
        }

        /// <summary>
        /// Hiện Button đồng ý
        /// </summary>
        public bool ShowButtonOK
        {
            get
            {
                return this.UIButton_OK.gameObject.activeSelf;
            }
            set
            {
                this.UIButton_OK.gameObject.SetActive(value);
            }
        }

        /// <summary>
        /// Sự kiện khi Button OK được ấn
        /// </summary>
        public Action OK { get; set; }

        /// <summary>
        /// Sự kiện khi Button Cancel được ấn
        /// </summary>
        public Action Cancel { get; set; }
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
            this.UIButton_OK.onClick.AddListener(this.ButtonOK_Clicked);
            this.UIButton_Cancel.onClick.AddListener(this.ButtonCancel_Clicked);
        }

        /// <summary>
        /// Sự kiện khi Button đồng ý được ấn
        /// </summary>
        private void ButtonOK_Clicked()
        {
            this.OK?.Invoke();
            this.Close();
        }

        /// <summary>
        /// Sự kiện khi Button hủy bỏ được ấn
        /// </summary>
        private void ButtonCancel_Clicked()
        {
            this.Cancel?.Invoke();
            this.Close();
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

        /// <summary>
        /// Hủy khung
        /// </summary>
        public void Close()
        {
            GameObject.Destroy(this.gameObject);
        }
        #endregion
    }
}
