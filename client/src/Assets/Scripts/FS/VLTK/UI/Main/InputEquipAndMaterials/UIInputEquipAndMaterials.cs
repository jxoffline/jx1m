using FS.VLTK.UI.Main.Bag;
using FS.VLTK.UI.Main.ItemBox;
using Server.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace FS.VLTK.UI.Main
{
	/// <summary>
	/// Khung đặt vào trang bị cùng danh sách vật phẩm bên dưới
	/// </summary>
	public class UIInputEquipAndMaterials : MonoBehaviour
	{
		#region Define
		/// <summary>
		/// Button đóng khung
		/// </summary>
		[SerializeField]
		private UnityEngine.UI.Button UIButton_Close;

		/// <summary>
		/// Trang bị
		/// </summary>
		[SerializeField]
		private UIItemBox UI_EquipBox;

		/// <summary>
		/// Prefab ô đặt nguyên liệu
		/// </summary>
		[SerializeField]
		private UIItemBox UI_ItemPrefab;

		/// <summary>
		/// Text tiêu đề khung
		/// </summary>
		[SerializeField]
		private TextMeshProUGUI UIText_Title;

		/// <summary>
		/// Text mô tả
		/// </summary>
		[SerializeField]
		private TextMeshProUGUI UIText_Description;

		/// <summary>
		/// Text thông tin khác
		/// </summary>
		[SerializeField]
		private TextMeshProUGUI UIText_OtherDetail;

		/// <summary>
		/// Button xác nhận
		/// </summary>
		[SerializeField]
		private UnityEngine.UI.Button UIButton_OK;

		/// <summary>
		/// Lưới ô túi đồ
		/// </summary>
		[SerializeField]
		private UIBag_Grid UIBag_Grid;
		#endregion

		#region Constants
		/// <summary>
		/// Số nguyên liệu đặt vào tối đa
		/// </summary>
		private const int MaterialCount = 15;
		#endregion

		#region Private fields
		/// <summary>
		/// RectTransform danh sách nguyên liệu
		/// </summary>
		private RectTransform transformMaterialList = null;
		#endregion

		#region Properties
		/// <summary>
		/// Sự kiện đóng khung
		/// </summary>
		public Action Close { get; set; }

		/// <summary>
		/// Sự kiện luyện hóa trang bị
		/// </summary>
		public Action<GoodsData, List<GoodsData>> OK { get; set; }

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
		/// Mô tả khung
		/// </summary>
		public string Description
		{
			get
			{
				return this.UIText_Description.text;
			}
			set
			{
				this.UIText_Description.text = value;
			}
		}

		/// <summary>
		/// Thông tin khác
		/// </summary>
		public string OtherDetail
		{
			get
			{
				return this.UIText_OtherDetail.text;
			}
			set
			{
				this.UIText_OtherDetail.text = value;
			}
		}

		/// <summary>
		/// Có yêu cầu phải có nguyên liệu phía dưới không
		/// </summary>
		public bool MustIncludeMaterials { get; set; }
		#endregion

		#region Core MonoBehaviour
		/// <summary>
		/// Hàm này gọi khi đối tượng được tạo ra
		/// </summary>
		private void Awake()
		{
			this.transformMaterialList = this.UI_ItemPrefab.transform.parent.GetComponent<RectTransform>();
		}

		/// <summary>
		/// Hàm này gọi ở Frame đầu tiên
		/// </summary>
		private void Start()
		{
			this.InitPrefabs();
			this.MakeDefaultItemSlots();
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
			this.UIBag_Grid.BagItemClicked = this.ButtonBagItem_Clicked;
			this.UI_EquipBox.Click = this.ButtonEquip_Clicked;
		}

		/// <summary>
		/// Sự kiện khi Button đóng khung được ấn
		/// </summary>
		private void ButtonClose_Clicked()
		{
			this.Close?.Invoke();
		}

		/// <summary>
		/// Sự kiện khi Button luyện hóa được ấn
		/// </summary>
		private void ButtonOK_Clicked()
		{
			/// Nếu chưa đặt vào trang bị
			if (this.UI_EquipBox.Data == null)
			{
				KTGlobal.AddNotification("Hãy đặt vào trang bị!");
				return;
			}

			/// Danh sách vật phẩm đặt vào
			List<GoodsData> inputItems = new List<GoodsData>();
			foreach (Transform child in this.transformMaterialList.transform)
			{
				if (child.gameObject != this.UI_ItemPrefab.gameObject)
				{
					UIItemBox uiItemBox = child.GetComponent<UIItemBox>();
					if (uiItemBox.Data != null)
					{
						inputItems.Add(uiItemBox.Data);
					}
				}
			}

			/// Nếu yêu cầu phải đặt vào vật phẩm
			if (this.MustIncludeMaterials)
			{
				/// Nếu chưa đặt vào vật phẩm
				if (inputItems.Count <= 0)
				{
					KTGlobal.AddNotification("Chưa đặt vào vật phẩm!");
					return;
				}
			}

			/// Thực thi sự kiện OK
			this.OK?.Invoke(this.UI_EquipBox.Data, inputItems);
			/// Đóng khung
			this.Close?.Invoke();
		}

		/// <summary>
		/// Sự kiện khi Button ô vật phẩm được ấn
		/// </summary>
		/// <param name="uiItemBox"></param>
		private void ButtonMaterial_Clicked(UIItemBox uiItemBox)
		{
			if (uiItemBox == null)
			{
				return;
			}

			List<KeyValuePair<string, Action>> buttons = new List<KeyValuePair<string, Action>>();
			buttons.Add(new KeyValuePair<string, Action>("Tháo xuống", () => {
				this.UIBag_Grid.AddItem(uiItemBox.Data);
				uiItemBox.Data = null;
				KTGlobal.CloseItemInfo();
			}));

			KTGlobal.ShowItemInfo(uiItemBox.Data, buttons);
		}

		/// <summary>
		/// Sự kiện khi Button trong lưới vật phẩm túi đồ được ấn
		/// </summary>
		/// <param name="itemGD"></param>
		private void ButtonBagItem_Clicked(GoodsData itemGD)
		{
			if (itemGD == null)
			{
				return;
			}

			List<KeyValuePair<string, Action>> buttons = new List<KeyValuePair<string, Action>>();
			buttons.Add(new KeyValuePair<string, Action>("Đặt lên", () => {
				/// Nếu không phải trang bị
				if (!KTGlobal.IsEquip(itemGD.GoodsID))
				{
					UIItemBox emptySlot = this.FindEmptySlot();
					if (emptySlot == null)
					{
						KTGlobal.AddNotification("Số lượng nguyên liệu đặt vào đã tối đa, không thể đặt thêm!");
						return;
					}
					this.UIBag_Grid.RemoveItem(itemGD);
					emptySlot.Data = itemGD;
				}
				/// Nếu là trang bị
				else
				{
					/// Nếu vị trí đã có trang bị thì yêu cầu gỡ xuống
					if (this.UI_EquipBox.Data != null)
					{
						KTGlobal.AddNotification("Hãy gỡ trang bị hiện tại xuống trước!");
						return;
					}

					this.UIBag_Grid.RemoveItem(itemGD);
					this.UI_EquipBox.Data = itemGD;
					KTGlobal.CloseItemInfo();
				}
			}));
			KTGlobal.ShowItemInfo(itemGD, buttons);
		}

		/// <summary>
		/// Sự kiện khi Button trang bị đang cường hóa được ấn
		/// </summary>
		private void ButtonEquip_Clicked()
		{
			if (this.UI_EquipBox.Data == null)
			{
				return;
			}

			List<KeyValuePair<string, Action>> buttons = new List<KeyValuePair<string, Action>>();
			buttons.Add(new KeyValuePair<string, Action>("Tháo xuống", () => {
				this.UIBag_Grid.AddItem(this.UI_EquipBox.Data);
				this.UI_EquipBox.Data = null;
				KTGlobal.CloseItemInfo();
			}));
			KTGlobal.ShowItemInfo(this.UI_EquipBox.Data, buttons);
		}
		#endregion

		#region Private methods
		/// <summary>
		/// Thực thi sự kiện bỏ qua một số Frame
		/// </summary>
		/// <param name="skip"></param>
		/// <param name="work"></param>
		/// <returns></returns>
		private IEnumerator ExecuteSkipFrames(int skip, Action work)
		{
			for (int i = 1; i <= skip; i++)
			{
				yield return null;
			}
			work?.Invoke();
		}

		/// <summary>
		/// Xây lại giao diện
		/// </summary>
		private void RebuildLayout()
		{
			/// Nếu đối tượng không được kích hoạt
			if (!this.gameObject.activeSelf)
			{
				return;
			}
			/// Xây lại giao diện ở Frame tiếp theo
			this.StartCoroutine(this.ExecuteSkipFrames(1, () => {
				UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.transformMaterialList);
			}));
		}

		/// <summary>
		/// Tạo mặc định các vị trí đặt nguyên liệu
		/// </summary>
		private void MakeDefaultItemSlots()
		{
			/// Duyệt danh sách
			for (int i = 1; i <= UIInputEquipAndMaterials.MaterialCount; i++)
			{
				UIItemBox uiItemBox = GameObject.Instantiate<UIItemBox>(this.UI_ItemPrefab);
				uiItemBox.transform.SetParent(this.transformMaterialList, false);
				uiItemBox.gameObject.SetActive(true);
				uiItemBox.Data = null;
				uiItemBox.Click = () => {
					this.ButtonMaterial_Clicked(uiItemBox);
				};
			}
			/// Xây lại giao diện
			this.RebuildLayout();
		}

		/// <summary>
		/// Tìm vị trí trống
		/// </summary>
		/// <returns></returns>
		private UIItemBox FindEmptySlot()
		{
			foreach (Transform child in this.transformMaterialList.transform)
			{
				if (child.gameObject != this.UI_ItemPrefab.gameObject)
				{
					UIItemBox uiItemBox = child.GetComponent<UIItemBox>();
					if (uiItemBox != null)
					{
						return uiItemBox;
					}
				}
			}
			return null;
		}

		/// <summary>
		/// Làm rỗng danh sách vị trí đặt nguyên liệu
		/// </summary>
		private void ClearItemSlots()
		{
			foreach (Transform child in this.transformMaterialList.transform)
			{
				if (child.gameObject != this.UI_ItemPrefab.gameObject)
				{
					UIItemBox uiItemBox = child.GetComponent<UIItemBox>();
					uiItemBox.Data = null;
				}
			}
		}
		#endregion
	}
}
