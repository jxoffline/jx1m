using FS.VLTK.UI.Main.Bag;
using FS.VLTK.UI.Main.ItemBox;
using Server.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace FS.VLTK.UI.Main
{
	/// <summary>
	/// Khung đặt vật phẩm làm gì đó
	/// </summary>
	public class UIInputItems : MonoBehaviour
	{
		#region Define
		/// <summary>
		/// Button đóng khung
		/// </summary>
		[SerializeField]
		private UnityEngine.UI.Button UIButton_Close;

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
		/// Sự kiện xác nhận danh sách vật phẩm
		/// </summary>
		public Action<List<GoodsData>> OK { get; set; }

		/// <summary>
		/// Predicate vật phẩm nào được đặt vào khung
		/// </summary>
		public Predicate<GoodsData> Predicate { get; set; }

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
			/// Nếu chưa đặt vào vật phẩm
			if (inputItems.Count <= 0)
			{
				KTGlobal.AddNotification("Chưa đặt vào vật phẩm!");
				return;
			}

			/// Thực thi sự kiện OK
			this.OK?.Invoke(inputItems);
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

			/// Nếu có thể đặt
			if (this.Predicate == null || this.Predicate.Invoke(itemGD))
			{
                List<KeyValuePair<string, Action>> buttons = new List<KeyValuePair<string, Action>>();
                buttons.Add(new KeyValuePair<string, Action>("Đặt lên", () => {
                    UIItemBox emptySlot = this.FindEmptySlot();
                    if (emptySlot == null)
                    {
                        KTGlobal.AddNotification("Số lượng vật phẩm đặt vào đã tối đa, không thể đặt thêm!");
                        return;
                    }
                    this.UIBag_Grid.RemoveItem(itemGD);
                    emptySlot.Data = itemGD;
                    KTGlobal.CloseItemInfo();
                }));
                KTGlobal.ShowItemInfo(itemGD, buttons);
            }
			/// Nếu không thể đặt
			else
			{
                KTGlobal.ShowItemInfo(itemGD);
            }
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
			for (int i = 1; i <= UIInputItems.MaterialCount; i++)
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
