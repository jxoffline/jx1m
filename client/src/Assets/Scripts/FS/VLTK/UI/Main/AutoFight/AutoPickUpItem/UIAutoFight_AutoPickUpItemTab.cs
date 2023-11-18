using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using VLTK.UI.Main.AutoFight.AutoPickUpItem;

namespace FS.VLTK.UI.Main.AutoFight
{
	/// <summary>
	/// Tab tự nhặt đồ
	/// </summary>
	public class UIAutoFight_AutoPickUpItemTab : MonoBehaviour
	{
		#region Define
		/// <summary>
		/// Toggle tự nhặt đồ
		/// </summary>
		[SerializeField]
		private UnityEngine.UI.Toggle UIToggle_AutoPickUpItem;

		/// <summary>
		/// Text phạm vi nhặt
		/// </summary>
		[SerializeField]
		private TextMeshProUGUI UIText_PickUpRange;

		/// <summary>
		/// Button nhập phạm vi nhặt
		/// </summary>
		[SerializeField]
		private UnityEngine.UI.Button UIButton_InputPickUpRange;

		/// <summary>
		/// Toggle nhặt Huyền Tinh
		/// </summary>
		[SerializeField]
		private UnityEngine.UI.Toggle UIToggle_PickUpCrystalStone;

		/// <summary>
		/// Text nhặt Huyền Tinh cấp
		/// </summary>
		[SerializeField]
		private TextMeshProUGUI UIText_PickUpCrystalStoneLevel;

		/// <summary>
		/// Button nhập cấp Huyền Tinh sẽ nhặt
		/// </summary>
		[SerializeField]
		private UnityEngine.UI.Button UIButton_InputPickUpCrystalStoneLevel;

		/// <summary>
		/// Toggle nhặt trang bị
		/// </summary>
		[SerializeField]
		private UnityEngine.UI.Toggle UIToggle_PickUpEquip;

		/// <summary>
		/// Text nhặt trang bị số sao trên
		/// </summary>
		[SerializeField]
		private TextMeshProUGUI UIText_PickUpEquipStar;

		/// <summary>
		/// Button nhập số sao trang bị sẽ nhặt
		/// </summary>
		[SerializeField]
		private UnityEngine.UI.Button UIButton_InputPickUpEquipStar;

		/// <summary>
		/// Text nhặt trang bị số dòng trên
		/// </summary>
		[SerializeField]
		private TextMeshProUGUI UIText_PickUpEquipLinesCount;

		/// <summary>
		/// Button nhập số dòng trang bị trên bao nhiêu sẽ nhặt
		/// </summary>
		[SerializeField]
		private UnityEngine.UI.Button UIButton_InputPickUpByLinesCount;

		/// <summary>
		/// Toggle tự sắp xếp túi đồ
		/// </summary>
		[SerializeField]
		private UnityEngine.UI.Toggle UIToggle_AutoSortBag;

		/// <summary>
		/// Toggle tự về thành và bán rác
		/// </summary>
		[SerializeField]
		private UnityEngine.UI.Toggle UIToggle_AutoBackAndSellTrashes;

		/// <summary>
		/// Toggle nhặt các vật phẩm khác
		/// </summary>
		[SerializeField]
		private UnityEngine.UI.Toggle UIToggle_PickUpOtherItems;

		/// <summary>
		/// Text tự bán trang bị dưới sao
		/// </summary>
		[SerializeField]
		private TextMeshProUGUI UIText_AutoSellTrashEquipBelowStar;

		/// <summary>
		/// Button tự bán trang bị dưới sao
		/// </summary>
		[SerializeField]
		private UnityEngine.UI.Button UIButton_InputAutoSellTrashEquipBelowStar;
		#endregion

		#region Properties
		/// <summary>
		/// Tự nhặt vật phẩm
		/// </summary>
		public bool AutoPickUpItem
		{
			get
			{
				return this.UIToggle_AutoPickUpItem.isOn;
			}
			set
			{
				this.UIToggle_AutoPickUpItem.isOn = value;
			}
		}

		private int _PickUpRange = 100;
		/// <summary>
		/// Phạm vi nhặt
		/// </summary>
		public int PickUpRange
		{
			get
			{
				return this._PickUpRange;
			}
			set
			{
				this._PickUpRange = value;
				this.UIText_PickUpRange.text = value.ToString();
			}
		}

		/// <summary>
		/// Tự nhặt Huyền Tinh
		/// </summary>
		public bool PickUpCrystalStone
		{
			get
			{
				return this.UIToggle_PickUpCrystalStone.isOn;
			}
			set
			{
				this.UIToggle_PickUpCrystalStone.isOn = value;
			}
		}

		private int _PickUpCrystalStoneLevel = 1;
		/// <summary>
		/// Tự nhặt Huyền Tinh trên cấp
		/// </summary>
		public int PickUpCrystalStoneLevel
		{
			get
			{
				return this._PickUpCrystalStoneLevel;
			}
			set
			{
				this._PickUpCrystalStoneLevel = value;
				this.UIText_PickUpCrystalStoneLevel.text = value.ToString();
			}
		}

		/// <summary>
		/// Tự nhặt trang bị
		/// </summary>
		public bool PickUpEquip
		{
			get
			{
				return this.UIToggle_PickUpEquip.isOn;
			}
			set
			{
				this.UIToggle_PickUpEquip.isOn = value;
			}
		}

		private int _PickUpEquipStar = 1;
		/// <summary>
		/// Tự nhặt trang bị số sao trên
		/// </summary>
		public int PickUpEquipStar
		{
			get
			{
				return this._PickUpEquipStar;
			}
			set
			{
				this._PickUpEquipStar = value;
				this.UIText_PickUpEquipStar.text = value.ToString();
			}
		}


		private int _PickUpEquipLinesCount = 1;
		/// <summary>
		/// Số dòng sẽ pick
		/// </summary>
		public int PickUpEquipLinesCount
		{
			get
			{
				return this._PickUpEquipLinesCount;
			}
			set
			{
				this._PickUpEquipLinesCount = value;
				this.UIText_PickUpEquipLinesCount.text = value.ToString();
			}
		}

		/// <summary>
		/// Tự sắp xếp túi đồ
		/// </summary>
		public bool AutoSortBag
		{
			get
			{
				return this.UIToggle_AutoSortBag.isOn;
			}
			set
			{
				this.UIToggle_AutoSortBag.isOn = value;
			}
		}

		/// <summary>
		/// Tự về thành và bán rác
		/// </summary>
		public bool AutoBackAndSellTrashes
		{
			get
			{
				return this.UIToggle_AutoBackAndSellTrashes.isOn;
			}
			set
			{
				this.UIToggle_AutoBackAndSellTrashes.isOn = value;
			}
		}

		private int _AutoSellTrashEquipBelowStar = 1;
		/// <summary>
		/// Tự bán trang bị số sao dưới
		/// </summary>
		public int AutoSellTrashEquipBelowStar
		{
			get
			{
				return this._AutoSellTrashEquipBelowStar;
			}
			set
			{
				this._AutoSellTrashEquipBelowStar = value;
				this.UIText_AutoSellTrashEquipBelowStar.text = value.ToString();
			}
		}

		/// <summary>
		/// Danh sách vật phẩm không nhặt
		/// </summary>
		public bool PickUpOtherItems
		{
			get
			{
				return this.UIToggle_PickUpOtherItems.isOn;
			}
			set
			{
				this.UIToggle_PickUpOtherItems.isOn = value;
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
			this.UIButton_InputPickUpRange.onClick.AddListener(this.ButtonInputPickUpRange_Clicked);
			this.UIButton_InputPickUpCrystalStoneLevel.onClick.AddListener(this.ButtonInputPickUpCrystalStoneLevel_Clicked);
			this.UIButton_InputPickUpEquipStar.onClick.AddListener(this.ButtonInputPickUpEquipStar_Clicked);
			this.UIButton_InputPickUpByLinesCount.onClick.AddListener(this.ButtonInputPickUpLinesCount_Clicked);
			this.UIButton_InputAutoSellTrashEquipBelowStar.onClick.AddListener(this.ButtonInputAutoSellTrashEquipBelowStar_Clicked);
		}

		/// <summary>
		/// Sự kiện khi Button nhập phạm vi nhặt được ấn
		/// </summary>
		private void ButtonInputPickUpRange_Clicked()
		{
			KTGlobal.ShowInputNumber("Nhập phạm vi nhặt.", 0, 1000, (number) => {
				this.PickUpRange = number;
			});
		}

		/// <summary>
		/// Sự kiện khi Button nhập cấp độ Huyền tinh sẽ nhặt được ấn
		/// </summary>
		private void ButtonInputPickUpCrystalStoneLevel_Clicked()
		{
			KTGlobal.ShowInputNumber("Nhập cấp độ Huyền Tinh sẽ nhặt.", 1, 12, (number) => {
				this.PickUpCrystalStoneLevel = number;
			});
		}

		/// <summary>
		/// Sự kiện khi Button nhập số sao trang bị sẽ nhặt được ấn
		/// </summary>
		private void ButtonInputPickUpEquipStar_Clicked()
		{
			KTGlobal.ShowInputNumber("Nhập số sao trang bị sẽ nhặt.", 1, 10, (number) => {
				this.PickUpEquipStar = number;
			});
		}


		private void ButtonInputPickUpLinesCount_Clicked()
		{
			KTGlobal.ShowInputNumber("Nhập số dòng sẽ nhặt.", 1, 10, (number) => {
				this.PickUpEquipLinesCount = number;
			});
		}
		/// <summary>
		/// Sự kiện khi Button nhập số sao trang bị sẽ tự bán vào Shop được ấn
		/// </summary>
		private void ButtonInputAutoSellTrashEquipBelowStar_Clicked()
		{
			KTGlobal.ShowInputNumber("Nhập số sao trang bị sẽ tự bán khi về thành.", 1, 10, (number) => {
				this.AutoSellTrashEquipBelowStar = number;
			});
		}
		#endregion
	}
}
