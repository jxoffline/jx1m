using FS.VLTK.UI.Main.ItemBox;
using FS.VLTK.Utilities.UnityUI;
using Server.Data;
using System;
using TMPro;
using UnityEngine;

namespace FS.VLTK.UI.Main.Welfare.MonthCard
{
	/// <summary>
	/// Vật phẩm trong khung phúc lợi thẻ tháng
	/// </summary>
	public class UIWelfare_MonthCard_ItemBox : MonoBehaviour
	{
		#region Define
		/// <summary>
		/// Text ngày tháng
		/// </summary>
		[SerializeField]
		private TextMeshProUGUI UIText_DateTime;

		/// <summary>
		/// Ô vật phẩm
		/// </summary>
		[SerializeField]
		private UIItemBox UI_Item;

		/// <summary>
		/// Hiệu ứng có thể lấy được
		/// </summary>
		[SerializeField]
		private UIAnimatedSprite UIAnimation_CanGet;

		/// <summary>
		/// Ký hiệu đã lấy được
		/// </summary>
		[SerializeField]
		private RectTransform UIMark_AlreadyGotten;

        /// <summary>
        /// Ký hiệu đã quá hạn
        /// </summary>
        [SerializeField]
        private RectTransform UIMark_OutOfDate;
        #endregion

        #region Properties
        /// <summary>
        /// Dữ liệu vật phẩm
        /// </summary>
        public GoodsData Data
		{
			get
			{
				return this.UI_Item.Data;
			}
			set
			{
				this.UI_Item.Data = value;
			}
		}

		/// <summary>
		/// Có thể nhận
		/// </summary>
		public bool CanGet
		{
			get
			{
				return this.UIAnimation_CanGet.gameObject.activeSelf;
			}
			set
			{
				this.UIAnimation_CanGet.gameObject.SetActive(value);
			}
		}

		/// <summary>
		/// Đã nhận rồi
		/// </summary>
		public bool AlreadyGotten
		{
			get
			{
				return this.UIMark_AlreadyGotten.gameObject.activeSelf;
			}
			set
			{
				this.UIMark_AlreadyGotten.gameObject.SetActive(value);
			}
		}

        /// <summary>
        /// Quá hạn
        /// </summary>
        public bool OutOfDate
        {
            get
            {
                return this.UIMark_OutOfDate.gameObject.activeSelf;
            }
            set
            {
                this.UIMark_OutOfDate.gameObject.SetActive(value);
            }
        }

        /// <summary>
        /// Ngày tháng
        /// </summary>
        public string DateTimeText
		{
			get
			{
				return this.UIText_DateTime.text;
			}
			set
			{
				this.UIText_DateTime.text = value;
			}
		}

		/// <summary>
		/// Sự kiện Click
		/// </summary>
		public Action Click { get; set; }
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
			this.UI_Item.Click = this.ButtonItem_Clicked;
		}

		/// <summary>
		/// Sự kiện khi Button vật phẩm được ấn
		/// </summary>
		private void ButtonItem_Clicked()
		{
			this.Click?.Invoke();
		}
		#endregion
	}
}
