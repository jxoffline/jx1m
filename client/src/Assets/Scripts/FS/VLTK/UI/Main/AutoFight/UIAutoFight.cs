using System;
using System.Collections.Generic;
using UnityEngine;
using FS.VLTK.UI.Main.AutoFight;
using FS.VLTK.Entities.Config;
using FS.VLTK.UI.Main.MainUI.RadarMap;
using Server.Data;

namespace FS.VLTK.UI.Main
{
	/// <summary>
	/// Khung thiết lập tự đánh
	/// </summary>
	public class UIAutoFight : MonoBehaviour
	{
		#region Define
		/// <summary>
		/// Button đóng khung
		/// </summary>
		[SerializeField]
		private UnityEngine.UI.Button UIButton_Close;

		/// <summary>
		/// Khung chọn kỹ năng
		/// </summary>
		[SerializeField]
		private UIAutoFight_SelectSkill UI_SelectSkillFrame;

		/// <summary>
		/// Khung thiết lập chung
		/// </summary>
		[SerializeField]
		private UIAutoFight_General UI_General;

		/// <summary>
		/// Khung hỗ trợ
		/// </summary>
		[SerializeField]
		private UIAutoFight_AutoSupportTab UI_AutoSupportTab;

        /// <summary>
        /// Khung đánh quái
        /// </summary>
        [SerializeField]
		private UIAutoFight_AutoFarmTab UI_AutoFarmTab;

		/// <summary>
		/// Khung tự nhặt
		/// </summary>
		[SerializeField]
		private UIAutoFight_AutoPickUpItemTab UI_AutoPickUpItemTab;

		/// <summary>
		/// Khung AutoPK
		/// </summary>
		[SerializeField]
		private UIAutoFight_AutoPKTab UI_AutoPKTab;

		/// <summary>
		/// Button lưu thiết lập
		/// </summary>
		[SerializeField]
		private UnityEngine.UI.Button UIButton_SaveSetting;

		/// <summary>
		/// Khung chọn kỹ năng
		/// </summary>
		[SerializeField]
		private UISelectItem UI_SelectItem;
		#endregion

		#region Constants
		/// <summary>
		/// Số kỹ năng mặc định
		/// </summary>
		public const int NumberOfSkills = 8;
		#endregion

		#region Properties
		/// <summary>
		/// Sự kiện đóng khung
		/// </summary>
		public Action Close { get; set; }

		/// <summary>
		/// Khung thiết lập chung
		/// </summary>
		public UIAutoFight_General General
		{
			get
			{
				return this.UI_General;
			}
		}

        /// <summary>
        /// Khung hỗ trợ
        /// </summary>
        public UIAutoFight_AutoSupportTab AutoSupport
		{
			get
			{
				return this.UI_AutoSupportTab;
			}
		}

		/// <summary>
		/// Khung tự đánh quái
		/// </summary>
		public UIAutoFight_AutoFarmTab AutoFarm
		{
			get
			{
				return this.UI_AutoFarmTab;
			}
		}
		
		/// <summary>
		/// Khung tự nhặt đồ
		/// </summary>
		public UIAutoFight_AutoPickUpItemTab AutoPickUpItem
		{
			get
			{
				return this.UI_AutoPickUpItemTab;
			}
		}

		/// <summary>
		/// Khung AutoPK
		/// </summary>
		public UIAutoFight_AutoPKTab AutoPK
		{
			get
			{
				return this.UI_AutoPKTab;
			}
		}

		/// <summary>
		/// Sự kiện lưu thiết lập
		/// </summary>
		public Action SaveSetting { get; set; }
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
			this.UIButton_Close.onClick.AddListener(this.ButtonClose_Clicked);
			this.UIButton_SaveSetting.onClick.AddListener(this.ButtonSaveSetting_Clicked);

			this.UI_AutoSupportTab.Parent = this;
			this.UI_AutoFarmTab.Parent = this;
			this.UI_AutoPKTab.Parent = this;
		}

		/// <summary>
		/// Sự kiện khi Button đóng khung được ấn
		/// </summary>
		private void ButtonClose_Clicked()
		{
			this.Close?.Invoke();
		}

		/// <summary>
		/// Sự kiện khi Button lưu thiết lập được ấn
		/// </summary>
		private void ButtonSaveSetting_Clicked()
		{
			this.SaveSetting?.Invoke();
		}
		#endregion

		#region Public methods
		/// <summary>
		/// Hiện khung chọn kỹ năng
		/// </summary>
		/// <param name="skillSelected"></param>
		public void ShowSelectSkill(Action<SkillDataEx> skillSelected)
		{
			this.UI_SelectSkillFrame.SkillSelected = skillSelected;
			this.UI_SelectSkillFrame.Close = this.UI_SelectSkillFrame.Hide;
			this.UI_SelectSkillFrame.Show();
		}

		/// <summary>
		/// Hiện khung chọn vật phẩm
		/// </summary>
		/// <param name="items"></param>
		/// <param name="itemSelected"></param>
		public void ShowSelectItem(List<GoodsData> items, Action<GoodsData> itemSelected)
		{
			this.UI_SelectItem.Title = "Chọn thuốc.";
			this.UI_SelectItem.Items = items;
			this.UI_SelectItem.ItemSelected = itemSelected;
			this.UI_SelectItem.Close = this.UI_SelectItem.Hide;
			this.UI_SelectItem.Show();
		}
		#endregion
	}
}
