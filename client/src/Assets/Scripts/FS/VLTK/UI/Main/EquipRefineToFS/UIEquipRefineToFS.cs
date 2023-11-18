using FS.VLTK.Entities.Config;
using FS.VLTK.UI.Main.Bag;
using FS.VLTK.UI.Main.ItemBox;
using Server.Data;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FS.VLTK.UI.Main
{
	/// <summary>
	/// Khung tách trang bị chế thành Ngũ Hành Hồn Thạch
	/// </summary>
	public class UIEquipRefineToFS : MonoBehaviour
	{
		#region Define
		/// <summary>
		/// Button đóng khung
		/// </summary>
		[SerializeField]
		private UnityEngine.UI.Button UIButton_Close;

		/// <summary>
		/// Ô trang bị
		/// </summary>
		[SerializeField]
		private UIItemBox UI_Equip;

		/// <summary>
		/// Ô Ngũ hành hồn thạch
		/// </summary>
		[SerializeField]
		private UIItemBox UI_FS;

		/// <summary>
		/// Túi đồ
		/// </summary>
		[SerializeField]
		private UIBag_Grid UIBag_Grid;

		/// <summary>
		/// Button OK
		/// </summary>
		[SerializeField]
		private UnityEngine.UI.Button UIButton_OK;
		#endregion

		#region Properties
		/// <summary>
		/// Sự kiện đóng khung
		/// </summary>
		public Action Close { get; set; }

		/// <summary>
		/// Sự kiện tách
		/// </summary>
		public Action<GoodsData> Refine { get; set; }
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
			this.UIButton_OK.onClick.AddListener(this.ButtonOK_Clicked);
			this.UIBag_Grid.BagItemClicked = this.BagItem_Clicked;
			this.UI_Equip.Click = this.ButtonEquip_Clicked;

			/// Hiện vật phẩm Ngũ Hành Hồn Thạch
			if (Loader.Loader.Items.TryGetValue(KTGlobal.FiveElementStoneID, out ItemData itemData))
			{
				GoodsData itemGD = KTGlobal.CreateItemPreview(itemData);
				itemGD.GCount = 0;
				this.UI_FS.Data = itemGD;
			}

			/// Ẩn Button chức năng
			this.UIButton_OK.interactable = false;
			/// Tính toán số NHHT có được
			this.CalculateTotalFS();
		}

		/// <summary>
		/// Sự kiện khi Button đóng khung được ấn
		/// </summary>
		private void ButtonClose_Clicked()
		{
			this.Close?.Invoke();
		}

		/// <summary>
		/// Sự kiện khi Button OK được ấn
		/// </summary>
		private void ButtonOK_Clicked()
		{
			/// Nếu không có trang bị
			if (this.UI_Equip.Data == null)
			{
				KTGlobal.AddNotification("Hãy đặt vào trang bị!");
				return;
			}

			/// Thực thi sự kiện
			this.Refine?.Invoke(this.UI_Equip.Data);
		}

		/// <summary>
		/// Sự kiện khi Button trang bị được ấn
		/// </summary>
		private void ButtonEquip_Clicked()
		{
			/// Nếu không có vật phẩm
			if (this.UI_Equip.Data == null)
			{
				return;
			}

			List<KeyValuePair<string, Action>> buttons = new List<KeyValuePair<string, Action>>();
			buttons.Add(new KeyValuePair<string, Action>("Gỡ xuống", () => {
				/// Trả lại túi đồ
				this.UIBag_Grid.AddItem(this.UI_Equip.Data);
				/// Xóa khỏi khung
				this.UI_Equip.Data = null;

				/// Tính toán số NHHT có được
				this.CalculateTotalFS();

				/// Đóng khung
				KTGlobal.CloseItemInfo();

				/// Ẩn Button chức năng
				this.UIButton_OK.interactable = false;
			}));
			KTGlobal.ShowItemInfo(this.UI_Equip.Data, buttons);
		}

		/// <summary>
		/// Sự kiện vật phẩm trong túi được ấn
		/// </summary>
		/// <param name="itemGD"></param>
		private void BagItem_Clicked(GoodsData itemGD)
		{
			/// Nếu không có vật phẩm thì thôi
			if (itemGD == null)
			{
				return;
			}

			List<KeyValuePair<string, Action>> buttons = new List<KeyValuePair<string, Action>>();
			/// Nếu là trang bị có thể tách
			if (KTGlobal.IsEquip(itemGD.GoodsID) && KTGlobal.IsEquipAbleToRefineIntoFS(itemGD))
			{
				buttons.Add(new KeyValuePair<string, Action>("Đặt lên", () => {
					/// Nếu đã có trang bị
					if (this.UI_Equip.Data != null)
					{
						KTGlobal.AddNotification("Hãy gỡ trang bị hiện tại xuống trước!");
						return;
					}

					/// Xóa khỏi túi đồ
					this.UIBag_Grid.RemoveItem(itemGD);
					/// Đặt lên khung
					this.UI_Equip.Data = itemGD;

					/// Tính toán số NHHT có được
					this.CalculateTotalFS();

					/// Đóng khung
					KTGlobal.CloseItemInfo();

					/// Hiện Button chức năng
					this.UIButton_OK.interactable = true;
				}));
			}
			KTGlobal.ShowItemInfo(itemGD, buttons);
		}
		#endregion

		#region Private methods
		/// <summary>
		/// Tính toán số lượng Ngũ hành hồn thạch có được sau khi tách trang bị chế
		/// </summary>
		private void CalculateTotalFS()
		{
			/// Nếu không có trang bị
			if (this.UI_Equip.Data == null)
			{
				/// Thiết lập số lượng
				this.UI_FS.Data.GCount = 0;
				this.UI_FS.RefreshQuantity();
				return;
			}

			/// Số lượng
			int totalFS = KTGlobal.CalculateTotalFSByRefiningEquip(this.UI_Equip.Data);
			/// Thiết lập số lượng
			this.UI_FS.Data.GCount = totalFS;
			this.UI_FS.RefreshQuantity();
		}
		#endregion

		#region Public methods
		/// <summary>
		/// Làm rỗng dữ liệu
		/// </summary>
		public void ClearData()
		{
			/// Hủy trang bị
			this.UI_Equip.Data = null;
			/// Thiết lập số lượng NHHT
			this.UI_FS.Data.GCount = 0;
			this.UI_FS.RefreshQuantity();
			/// Ẩn Button chức năng
			this.UIButton_OK.interactable = true;
		}
		#endregion
	}
}
