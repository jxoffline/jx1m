using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using Server.Data;
using FS.VLTK.Utilities.UnityUI;

namespace FS.VLTK.UI.Main.Mail
{
    /// <summary>
    /// Thư trong hộp thư người chơi
    /// </summary>
    public class UIMailBox_Mail : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Button
        /// </summary>
        [SerializeField]
        private UIButtonSprite UIButton;

        /// <summary>
        /// Text tiêu đề thư
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_MailTitle;

        /// <summary>
        /// Text tên người gửi
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_MailSenderName;

        /// <summary>
        /// Text thời gian gửi
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_SentDateTime;

        /// <summary>
        /// Image đánh dấu đã đọc thư
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Image UIImage_ReadMark;

        /// <summary>
        /// Image đánh dấu chưa đọc thư
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Image UIImage_UnreadMark;

        /// <summary>
        /// Sprite trạng thái thường
        /// </summary>
        [SerializeField]
        private string NormalSprite;

        /// <summary>
        /// Sprite trạng thái kích hoạt
        /// </summary>
        [SerializeField]
        private string ActiveSprite;
        #endregion

        #region Private fields

        #endregion

        #region Properties
        /// <summary>
        /// Sự kiện khi thư được chọn
        /// </summary>
        public Action Selected { get; set; }

        private MailData _Data;
        /// <summary>
        /// Dữ liệu thư
        /// </summary>
        public MailData Data
        {
            get
            {
                return this._Data;
            }
            set
            {
                this._Data = value;
                this.UIText_MailTitle.text = value.Subject;
                this.UIText_MailSenderName.text = value.SenderRName;

                /// Thời gian gửi
                DateTime dt = DateTime.Parse(value.SendTime);
                DateTime now = DateTime.Now;
                /// Nếu trong ngày
                if (dt.Date == now.Date && dt.Month == now.Month && dt.Year == now.Year)
                {
                    this.UIText_SentDateTime.text = string.Format("<color=#70fffd>{0}</color>", dt.ToString("HH:mm"));
                }
                /// Nếu khác ngày
                else
                {
                    this.UIText_SentDateTime.text = string.Format("<color=#70bcff>{0}</color> - <color=#70fffd>{1}</color>", dt.ToString("dd/MM/yyyy"), dt.ToString("HH:mm"));
                }

                /// Nếu đã xem thư
                if (value.IsRead == 1)
                {
                    this.UIImage_ReadMark.gameObject.SetActive(true);
                    this.UIImage_UnreadMark.gameObject.SetActive(false);
                }
                /// Nếu chưa xem thư
                else
                {
                    this.UIImage_ReadMark.gameObject.SetActive(false);
                    this.UIImage_UnreadMark.gameObject.SetActive(true);
                }
            }
        }

        private bool _Active = false;
        /// <summary>
        /// Kích hoạt đối tượng
        /// </summary>
        public bool Active
        {
            get
            {
                return this._Active;
            }
            set
            {
                this._Active = value;

                if (value)
                {
                    this.UIButton.NormalSpriteName = this.ActiveSprite;
                    this.UIButton.DisabledSpriteName = this.ActiveSprite;
                }
                else
                {
                    this.UIButton.NormalSpriteName = this.NormalSprite;
                    this.UIButton.DisabledSpriteName = this.NormalSprite;
                }
                this.UIButton.Refresh(true);
            }
        }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {

        }

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
            this.UIButton.Click = this.ButtonMail_Clicked;
        }

        /// <summary>
        /// Sự kiện khi Button thư được ấn
        /// </summary>
        private void ButtonMail_Clicked()
        {
            this.Selected?.Invoke();
        }
        #endregion

        #region Private fields

        #endregion

        #region Public fields

        #endregion
    }
}
