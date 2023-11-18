using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using Server.Data;
using FS.VLTK.Utilities.UnityUI;

namespace FS.VLTK.UI.Main.Welfare.EverydayOnline
{
    /// <summary>
    /// Ô vật phẩm trong khung phúc lợi Online mỗi ngày
    /// </summary>
    public class UIWelfare_EverydayOnline_SlotItemBox : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Ô vật phẩm dạng Slot
        /// </summary>
        [SerializeField]
        private UIItemSlotBox UISlotItemBox;

        /// <summary>
        /// Text thứ tự ngày
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Day;

        /// <summary>
        /// Đánh dấu đã nhận chưa
        /// </summary>
        [SerializeField]
        private RectTransform UI_AlreadyGetMark;

        /// <summary>
        /// Slider tiến độ
        /// </summary>
        [SerializeField]
        private UISliderText UISlider_Progress;

        /// <summary>
        /// Hiệu ứng có thể lấy được
        /// </summary>
        [SerializeField]
        private UIAnimatedSprite UIAnimation_CanGet;

        /// <summary>
        /// Hiệu ứng sắp lấy được
        /// </summary>
        [SerializeField]
        private UIAnimatedSprite UIAnimation_WillGet;

        /// <summary>
        /// Hiệu ứng đã lấy được
        /// </summary>
        [SerializeField]
        private UIAnimatedSprite UIAnimation_AlreadyGotten;
        #endregion

        #region Private fields

        #endregion

        #region Properties
        /// <summary>
        /// ID bước
        /// </summary>
        public int StepID { get; set; }

        /// <summary>
        /// Danh sách vật phẩm
        /// </summary>
        public List<GoodsData> Items
        {
            get
            {
                return this.UISlotItemBox.Items;
            }
            set
            {
                this.UISlotItemBox.Items = value;
            }
        }

        private int _TimeSec;
        /// <summary>
        /// Thời gian (giây)
        /// </summary>
        public int TimeSec
        {
            get
            {
                return this._TimeSec;
            }
            set
            {
                this._TimeSec = value;
                this.UIText_Day.text = KTGlobal.DisplayTimeHourMinuteSecondOnly(value);
            }
        }

        private int _CurrentOnlineSec;
        /// <summary>
        /// Thời gian đã Online hiện tại
        /// </summary>
        public int CurrentOnlineSec
        {
            get
            {
                return this._CurrentOnlineSec;
            }
            set
            {
                this._CurrentOnlineSec = value;
                if (value > this._TimeSec)
                {
                    value = this._TimeSec;
                }
                /// Tiến độ hiện tại
                int percent = value * 100 / this._TimeSec;
                /// Cập nhật giá trị tiến độ
                this.UISlider_Progress.Value = percent;
            }
        }

        /// <summary>
        /// Đã nhận chưa
        /// </summary>
        public bool AlreadyGotten
        {
            get
            {
                return this.UI_AlreadyGetMark.gameObject.activeSelf;
            }
            set
            {
                this.UI_AlreadyGetMark.gameObject.SetActive(value);
            }
        }

        /// <summary>
        /// Hiện tại có thể lấy được không
        /// </summary>
        public bool CanGet
        {
            get
            {
                /// Nếu đã nhận rồi thì thôi
                if (this.AlreadyGotten)
                {
                    return false;
                }
                return this._CurrentOnlineSec >= this._TimeSec;
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
            this.UISlotItemBox.Click = this.ButtonSlotItem_Clicked;
        }

        /// <summary>
        /// Sự kiện khi Button SlotItem được ấn
        /// </summary>
        private void ButtonSlotItem_Clicked()
        {
            KTGlobal.ShowItemListBox("Danh sách vật phẩm khi nhận sẽ nhận được <color=orange>ngẫu nhiên</color> <color=green>một món</color>.", this.Items);
        }
        #endregion

        #region Private methods

        #endregion

        #region Public methods
        /// <summary>
        /// Làm mới hiển thị
        /// </summary>
        public void Refresh()
        {
            /// Nếu có thể nhận
            if (this.CanGet)
            {
                this.UIAnimation_AlreadyGotten.Visible = false;
                this.UIAnimation_WillGet.Visible = false;
                /// Nếu chưa thực thi thì bắt đầu thực thi
                if (!this.UIAnimation_CanGet.Visible)
                {
                    this.UIAnimation_CanGet.Visible = true;
                }
            }
            else if (this.AlreadyGotten)
            {
                this.UIAnimation_CanGet.Visible = false;
                this.UIAnimation_WillGet.Visible = false;
                /// Nếu chưa thực thi thì bắt đầu thực thi
                if (!this.UIAnimation_AlreadyGotten.Visible)
                {
                    this.UIAnimation_AlreadyGotten.Visible = true;
                }
            }
            else
            {
                this.UIAnimation_CanGet.Visible = false;
                this.UIAnimation_AlreadyGotten.Visible = false;
                /// Nếu chưa thực thi thì bắt đầu thực thi
                if (!this.UIAnimation_WillGet.Visible)
                {
                    this.UIAnimation_WillGet.Visible = true;
                }
            }
        }

        /// <summary>
        /// Thực thi hiệu ứng
        /// </summary>
        /// <param name="stopAt"></param>
        public void Play(GoodsData stopAt)
        {
            this.UISlotItemBox.Play(stopAt);
        }

        /// <summary>
        /// Thực thi hiệu ứng
        /// </summary>
        /// <param name="itemID"></param>
        public void Play(int itemID, int itemNumber)
        {
            /// Vật phẩm tương ứng
            GoodsData itemGD = this.Items.Where(x => x.GoodsID == itemID && x.GCount == itemNumber).FirstOrDefault();
            /// Nếu tìm thấy
            if (itemGD != null)
            {
                this.Play(itemGD);
            }
        }
        #endregion
    }
}
