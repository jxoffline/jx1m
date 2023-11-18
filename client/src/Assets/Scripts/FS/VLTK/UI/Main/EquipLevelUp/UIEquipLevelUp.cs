using FS.VLTK.UI.Main.ItemBox;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using Server.Data;
using System.Collections;
using FS.VLTK.UI.Main.Bag;
using FS.VLTK.Entities.Config;

namespace FS.VLTK.UI.Main
{
	/// <summary>
	/// Khung luyện hóa trang bị
	/// </summary>
	public class UIEquipLevelUp : MonoBehaviour
	{
		#region Define
		/// <summary>
		/// Button đóng khung
		/// </summary>
		[SerializeField]
		private UnityEngine.UI.Button UIButton_Close;

		/// <summary>
		/// Ô đặt trang bị
		/// </summary>
		[SerializeField]
		private UIItemBox UI_EquipBox;

		/// <summary>
		/// Ô thông tin sản phẩm
		/// </summary>
		[SerializeField]
		private UIItemBox UI_ProductBox;

		/// <summary>
		/// Ô công thức
		/// </summary>
		[SerializeField]
		private UIItemBox UI_RecipeBox;

		/// <summary>
		/// Prefab ô đặt nguyên liệu
		/// </summary>
		[SerializeField]
		private UIItemBox UI_MaterialPrefab;

		/// <summary>
		/// Text số bạc cần
		/// </summary>
		[SerializeField]
		private TextMeshProUGUI UIText_MoneyNeed;

		/// <summary>
		/// Tỷ lệ thành công
		/// </summary>
		[SerializeField]
		private TextMeshProUGUI UIText_SuccessRate;

		/// <summary>
		/// Button luyện hóa
		/// </summary>
		[SerializeField]
		private UnityEngine.UI.Button UIButton_EquipLevelUp;

		/// <summary>
		/// Toggle chọn sản phẩm 1
		/// </summary>
		[SerializeField]
		private UnityEngine.UI.Toggle UIToggle_Product1;

		/// <summary>
		/// Toggle chọn sản phẩm 2
		/// </summary>
		[SerializeField]
		private UnityEngine.UI.Toggle UIToggle_Product2;

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

		/// <summary>
		/// Tỷ lệ thành công
		/// </summary>
		private int successPercent;
		#endregion

		#region Properties
		/// <summary>
		/// Sự kiện đóng khung
		/// </summary>
		public Action Close { get; set; }

		/// <summary>
		/// Sự kiện luyện hóa trang bị
		/// </summary>
		public Action<GoodsData, GoodsData, List<GoodsData>, int> EquipLevelUp { get; set; }
		#endregion

		#region Core MonoBehaviour
		/// <summary>
		/// Hàm này gọi khi đối tượng được tạo ra
		/// </summary>
		private void Awake()
		{
			this.transformMaterialList = this.UI_MaterialPrefab.transform.parent.GetComponent<RectTransform>();
		}

		/// <summary>
		/// Hàm này gọi ở Frame đầu tiên
		/// </summary>
		private void Start()
		{
			this.InitPrefabs();
			this.MakeDefaultMaterialSlots();
			this.CalculateProductAndMoneyNeed();
		}
		#endregion

		#region Code UI
		/// <summary>
		/// Khởi tạo ban đầu
		/// </summary>
		private void InitPrefabs()
		{
			this.UIButton_Close.onClick.AddListener(this.ButtonClose_Clicked);
			this.UIButton_EquipLevelUp.onClick.AddListener(this.ButtonEquipLevelUp_Clicked);
			this.UIBag_Grid.BagItemClicked = this.ButtonBagItem_Clicked;
			this.UI_EquipBox.Click = this.ButtonEquip_Clicked;
			this.UI_RecipeBox.Click = this.ButtonRecipe_Clicked;
			this.UIToggle_Product1.onValueChanged.AddListener((isSelected) => {
				if (isSelected)
				{
					this.CalculateProductAndMoneyNeed();
				}
			});
			this.UIToggle_Product2.onValueChanged.AddListener((isSelected) => {
				if (isSelected)
				{
					this.CalculateProductAndMoneyNeed();
				}
			});
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
		private void ButtonEquipLevelUp_Clicked()
		{
			/// Nếu tỷ lệ quá cao
			if (this.successPercent > 120)
			{
				KTGlobal.AddNotification("Bạn đã đặt vào quá nhiều Huyền Tinh, xin đừng lãng phí!");
				return;
			}

			/// Danh sách Huyền Tinh đặt vào
			List<GoodsData> crystalStones = new List<GoodsData>();
			/// Duyệt danh sách Huyền Tinh
			foreach (Transform child in this.transformMaterialList.transform)
			{
				if (child.gameObject != this.UI_MaterialPrefab.gameObject)
				{
					UIItemBox uiItemBox = child.GetComponent<UIItemBox>();
					if (uiItemBox.Data != null)
					{
						crystalStones.Add(uiItemBox.Data);
					}
				}
			}

			/// Thực thi sự kiện
			this.EquipLevelUp?.Invoke(this.UI_EquipBox.Data, this.UI_RecipeBox.Data, crystalStones, this.UI_ProductBox.Data.GoodsID);
		}

		/// <summary>
		/// Sự kiện khi Button ô nguyên liệu được ấn
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

				/// Tính toán sản phẩm đầu ra và số bạc cần
				this.CalculateProductAndMoneyNeed();

				/// Đóng Tooltip
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
			/// Nếu là trang bị
			if (KTGlobal.IsEquip(itemGD.GoodsID))
			{
				/// Công thức tương ứng
				EquipRefineXML recipe = Loader.Loader.EquipRefineRecipes.Where(x => x.SourceEquipID == itemGD.GoodsID).FirstOrDefault();
				/// Nếu có công thức
				if (recipe != null)
				{
					/// Hiện Button đặt lên
					buttons.Add(new KeyValuePair<string, Action>("Đặt lên", () => {
						/// Nếu đã có trang bị
						if (this.UI_EquipBox.Data != null)
						{
							KTGlobal.AddNotification("Hãy gỡ trang bị hiện tại xuống trước!");
							return;
						}

						/// Xóa khỏi túi đồ
						this.UIBag_Grid.RemoveItem(itemGD);
						/// Thêm vào ô trang bị
						this.UI_EquipBox.Data = itemGD;

						/// Tính toán sản phẩm đầu ra và số bạc cần
						this.CalculateProductAndMoneyNeed();

						/// Đóng Tooltip
						KTGlobal.CloseItemInfo();
					}));
				}
			}
			/// Nếu không phải trang bị
			else
			{
				/// Nếu có trang bị đặt vào
				if (this.UI_EquipBox.Data != null)
				{
					/// Nếu đã có công thức
					if (this.UI_RecipeBox.Data != null)
					{
						/// Nếu là Huyền Tinh
						if (KTGlobal.ListCrystalStones.ContainsKey(itemGD.GoodsID))
						{
							/// Hiện Button đặt lên
							buttons.Add(new KeyValuePair<string, Action>("Đặt lên", () => {
								/// Vị trí trống trong ô Huyền Tinh
								UIItemBox emptySlot = this.FindEmptySlot();
								/// Nếu không có vị trí trống
								if (emptySlot == null)
								{
									KTGlobal.AddNotification("Ô Huyền Tinh đã đầy!");
									return;
								}
								/// Xóa khỏi túi đồ
								this.UIBag_Grid.RemoveItem(itemGD);
								/// Đặt Huyền Tinh vào
								emptySlot.Data = itemGD;

								/// Tính toán sản phẩm đầu ra và số bạc cần
								this.CalculateProductAndMoneyNeed();

								/// Đóng Tooltip
								KTGlobal.CloseItemInfo();
							}));
						}
					}
					/// Nếu là công thức
					else
					{
						/// Công thức tương ứng
						EquipRefineXML recipe = Loader.Loader.EquipRefineRecipes.Where(x => x.SourceEquipID == this.UI_EquipBox.Data.GoodsID && x.RecipeItemID == itemGD.GoodsID).FirstOrDefault();
						/// Nếu tồn tại
						if (recipe != null)
						{
							/// Hiện Button đặt lên
							buttons.Add(new KeyValuePair<string, Action>("Đặt lên", () => {
								/// Xóa khỏi túi đồ
								this.UIBag_Grid.RemoveItem(itemGD);
								/// Thêm vào ô công thức
								this.UI_RecipeBox.Data = itemGD;

								/// Tính toán sản phẩm đầu ra và số bạc cần
								this.CalculateProductAndMoneyNeed();

								/// Đóng Tooltip
								KTGlobal.CloseItemInfo();
							}));
						}
					}
				}
			}
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
				/// Xóa khỏi túi đồ
				this.UIBag_Grid.AddItem(this.UI_EquipBox.Data);
				this.UI_EquipBox.Data = null;
				/// Gỡ công thức xuống
				this.UIBag_Grid.AddItem(this.UI_RecipeBox.Data);
				this.UI_RecipeBox.Data = null;

				/// Gỡ toàn bộ Huyền Tinh xuống
				foreach (Transform child in this.transformMaterialList.transform)
				{
					if (child.gameObject != this.UI_MaterialPrefab.gameObject)
					{
						UIItemBox uiItemBox = child.GetComponent<UIItemBox>();
						/// Nếu tồn tại
						if (uiItemBox.Data != null)
						{
							/// Trả lại túi đồ
							this.UIBag_Grid.AddItem(this.UI_RecipeBox.Data);
							uiItemBox.Data = null;
						}
					}
				}

				/// Tính toán sản phẩm đầu ra và số bạc cần
				this.CalculateProductAndMoneyNeed();

				/// Đóng Tooltip
				KTGlobal.CloseItemInfo();
			}));
			KTGlobal.ShowItemInfo(this.UI_EquipBox.Data, buttons);
		}

		/// <summary>
		/// Sự kiện khi Button công thức được ấn
		/// </summary>
		private void ButtonRecipe_Clicked()
		{
			if (this.UI_RecipeBox.Data == null)
			{
				return;
			}

			List<KeyValuePair<string, Action>> buttons = new List<KeyValuePair<string, Action>>();
			buttons.Add(new KeyValuePair<string, Action>("Tháo xuống", () => {
				/// Gỡ công thức xuống
				this.UIBag_Grid.AddItem(this.UI_RecipeBox.Data);
				this.UI_RecipeBox.Data = null;

				/// Gỡ toàn bộ Huyền Tinh xuống
				foreach (Transform child in this.transformMaterialList.transform)
				{
					if (child.gameObject != this.UI_MaterialPrefab.gameObject)
					{
						UIItemBox uiItemBox = child.GetComponent<UIItemBox>();
						/// Nếu tồn tại
						if (uiItemBox.Data != null)
						{
							/// Trả lại túi đồ
							this.UIBag_Grid.AddItem(this.UI_RecipeBox.Data);
							uiItemBox.Data = null;
						}
					}
				}

				/// Tính toán sản phẩm đầu ra và số bạc cần
				this.CalculateProductAndMoneyNeed();

				/// Đóng Tooltip
				KTGlobal.CloseItemInfo();
			}));
			KTGlobal.ShowItemInfo(this.UI_RecipeBox.Data, buttons);
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
		private void MakeDefaultMaterialSlots()
		{
			/// Duyệt danh sách
			for (int i = 1; i <= UIEquipLevelUp.MaterialCount; i++)
			{
				UIItemBox uiItemBox = GameObject.Instantiate<UIItemBox>(this.UI_MaterialPrefab);
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
				if (child.gameObject != this.UI_MaterialPrefab.gameObject)
				{
					UIItemBox uiItemBox = child.GetComponent<UIItemBox>();
					if (uiItemBox.Data == null)
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
		private void ClearMaterialSlots()
		{
			foreach (Transform child in this.transformMaterialList.transform)
			{
				if (child.gameObject != this.UI_MaterialPrefab.gameObject)
				{
					UIItemBox uiItemBox = child.GetComponent<UIItemBox>();
					uiItemBox.Data = null;
				}
			}
		}

		/// <summary>
		/// Tính toán sản phẩm đầu ra và số tiền cần
		/// </summary>
		private void CalculateProductAndMoneyNeed()
		{
			/// Reset dữ liệu
			int moneyNeed = 0;
			int successRate = 0;
			GoodsData productGD = null;
			/// Đóng Button chức năng
			this.UIButton_EquipLevelUp.interactable = false;
			/// Ẩn 2 Toggle
			this.UIToggle_Product1.interactable = false;
			this.UIToggle_Product2.interactable = false;

			/// Nếu có trang bị đặt vào
			if (this.UI_EquipBox.Data != null)
			{
				/// Nếu có công thức đặt vào
				if (this.UI_RecipeBox.Data != null)
				{
					/// Danh sách Huyền Tinh đặt vào
					List<GoodsData> crystalStones = new List<GoodsData>();
					/// Duyệt danh sách Huyền Tinh
					foreach (Transform child in this.transformMaterialList.transform)
					{
						if (child.gameObject != this.UI_MaterialPrefab.gameObject)
						{
							UIItemBox uiItemBox = child.GetComponent<UIItemBox>();
							if (uiItemBox.Data != null)
							{
								crystalStones.Add(uiItemBox.Data);
							}
						}
					}

					/// Tỷ lệ thành công
					successRate = KTGlobal.CalculateRefineRate(this.UI_EquipBox.Data, crystalStones);

					/// Công thức tương ứng
					List<EquipRefineXML> recipe = Loader.Loader.EquipRefineRecipes.Where(x => x.SourceEquipID == this.UI_EquipBox.Data.GoodsID && x.RecipeItemID == this.UI_RecipeBox.Data.GoodsID).ToList();
					/// Nếu có 1 sản phẩm
					if (recipe.Count == 1)
					{
						this.UIToggle_Product1.interactable = true;
						this.UIToggle_Product1.isOn = true;
					}
					/// Nếu có 2 sản phẩm
					else if (recipe.Count == 2)
					{
						this.UIToggle_Product1.interactable = true;
						this.UIToggle_Product2.interactable = true;
					}

					/// ID sản phẩm được chọn
					int productEquipID = recipe[this.UIToggle_Product1.isOn ? 0 : 1].ProductEquipID;
					/// Nếu vật phẩm tồn tại
					if (Loader.Loader.Items.TryGetValue(productEquipID, out ItemData itemData))
					{
						/// Cập nhật sản phẩm
						productGD = KTGlobal.CreateItemPreview(itemData);
						productGD.Binding = 1;
						productGD.Forge_level = this.UI_EquipBox.Data.Forge_level;
					}
				}
				/// Số tiền cần
				moneyNeed = KTGlobal.CalcRefineMoney(this.UI_EquipBox.Data);
			}

			/// Đổ dữ liệu
			this.UIText_MoneyNeed.text = KTGlobal.GetDisplayMoney(moneyNeed);
			this.successPercent = successRate;
			this.UIText_SuccessRate.text = string.Format("{0}%", successRate);
			this.UI_ProductBox.Data = productGD;
			/// Mở Button chức năng
			this.UIButton_EquipLevelUp.interactable = productGD != null && successRate >= 100;
		}
		#endregion
	}
}
