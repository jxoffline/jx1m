using Server.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FS.VLTK.UI.Main.TokenShop.StoreProduct
{
	/// <summary>
	/// Khung mua gói hàng trên Store
	/// </summary>
	public class UITokenShop_StoreProductBuy : MonoBehaviour
	{
		#region Define
		/// <summary>
		/// Prefab ô vật phẩm
		/// </summary>
		[SerializeField]
		private UITokenShop_StoreProductBuy_Item UI_ItemPrefab;
		#endregion

		#region Private fields
		/// <summary>
		/// RectTransform danh sách vật phẩm
		/// </summary>
		private RectTransform transformItemsList = null;
		#endregion

		#region Properties
		/// <summary>
		/// Danh sách gói hàng
		/// </summary>
		public List<TokenShopStoreProduct> Data { get; set; }

		/// <summary>
		/// Sự kiện Click mua
		/// </summary>
		public Action<TokenShopStoreProduct> Click { get; set; }
		#endregion

		#region Core MonoBehaviour
		/// <summary>
		/// Hàm này gọi khi đối tượng được tạo ra
		/// </summary>
		private void Awake()
		{
			this.transformItemsList = this.UI_ItemPrefab.transform.parent.GetComponent<RectTransform>();
		}

		/// <summary>
		/// Hàm này gọi ở Frame đầu tiên
		/// </summary>
		private void Start()
		{
			this.InitPrefabs();
			this.Refresh();
		}
		#endregion

		#region Code UI
		/// <summary>
		/// Khởi tạo ban đầu
		/// </summary>
		private void InitPrefabs()
		{

		}

		/// <summary>
		/// Sự kiện khi Button mua hàng được ấn
		/// </summary>
		/// <param name="product"></param>
		private void ButtonProductBuy_Clicked(TokenShopStoreProduct product)
		{
			/// Nếu không có danh sách vật phẩm
			if (product == null)
			{
				KTGlobal.AddNotification("Gói hàng không tồn tại!");
				return;
			}

			/// Thực hiện sự kiện mua
			this.Click?.Invoke(product);
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
			if (!this.gameObject.activeSelf)
			{
				return;
			}
			this.StartCoroutine(this.ExecuteSkipFrames(1, () => {
				UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.transformItemsList);
			}));
		}

		/// <summary>
		/// Làm rỗng danh sách vật phẩm
		/// </summary>
		private void ClearItemsList()
		{
			foreach (Transform child in this.transformItemsList.transform)
			{
				if (child.gameObject != this.UI_ItemPrefab.gameObject)
				{
					GameObject.Destroy(child.gameObject);
				}
			}
		}

		/// <summary>
		/// Thêm gói hàng vào danh sách
		/// </summary>
		/// <param name="data"></param>
		private void AddProduct(TokenShopStoreProduct data)
		{
			UITokenShop_StoreProductBuy_Item uiProduct = GameObject.Instantiate<UITokenShop_StoreProductBuy_Item>(this.UI_ItemPrefab);
			uiProduct.transform.SetParent(this.transformItemsList, false);
			uiProduct.gameObject.SetActive(true);
			uiProduct.Data = data;
			uiProduct.Click = () => {
				this.ButtonProductBuy_Clicked(data);
			};
		}

		/// <summary>
		/// Làm mới dữ liệu
		/// </summary>
		private void Refresh()
		{
			/// Làm rỗng danh sách vật phẩm
			this.ClearItemsList();

			/// Nếu không có dữ liệu
			if (this.Data == null)
			{
				return;
			}

			/// Duyệt danh sách
			foreach (TokenShopStoreProduct productData in this.Data)
			{
				/// Thêm gói hàng
				this.AddProduct(productData);
			}

			/// Xây lại giao diện
			this.RebuildLayout();
		}
		#endregion
	}
}
