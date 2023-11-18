using FS.VLTK;
using FS.VLTK.Entities.Config;
using FS.VLTK.Loader;
using VLTK.UI.Main.AutoFight.AutoPickUpItem.SelectNoPickItem;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace VLTK.UI.Main.AutoFight.AutoPickUpItem
{
	/// <summary>
	/// Khung chọn danh sách vật phẩm không nhặt
	/// </summary>
	public class UIAutoFight_AutoPickItemTab_SelectNoPickItemFrame : MonoBehaviour
	{
		#region Define
		/// <summary>
		/// Button đóng khung
		/// </summary>
		[SerializeField]
		private UnityEngine.UI.Button UIButton_Close;

		/// <summary>
		/// Prefab vật phẩm ở danh sách tìm kiếm
		/// </summary>
		[SerializeField]
		private UIAutoFight_AutoPickItemTab_SelectNoPickItemFrame_ItemInfo UI_SearchItemInfoPrefab;

		/// <summary>
		/// Prefab vật phẩm ở danh sách kết quả
		/// </summary>
		[SerializeField]
		private UIAutoFight_AutoPickItemTab_SelectNoPickItemFrame_ItemInfo UI_OutputItemInfoPrefab;

		/// <summary>
		/// Thêm vào danh sách kết quả
		/// </summary>
		[SerializeField]
		private UnityEngine.UI.Button UIButton_AddToOutput;

		/// <summary>
		/// Xóa khỏi danh sách kết quả
		/// </summary>
		[SerializeField]
		private UnityEngine.UI.Button UIButton_RemoveFromOutput;

		/// <summary>
		/// Input nhập tên tìm kiếm
		/// </summary>
		[SerializeField]
		private TMP_InputField UIInput_SearchString;

		/// <summary>
		/// Button tìm kiếm
		/// </summary>
		[SerializeField]
		private UnityEngine.UI.Button UIButton_SearchItem;
		#endregion

		#region Private fields
		/// <summary>
		/// RectTransform danh sách tìm kiếm vật phẩm
		/// </summary>
		private RectTransform transformSearchItemList = null;

		/// <summary>
		/// RectTransform danh sách vật phẩm không nhặt
		/// </summary>
		private RectTransform transformOutputItemList = null;

		/// <summary>
		/// Vật phẩm tìm kiếm được chọn
		/// </summary>
		private UIAutoFight_AutoPickItemTab_SelectNoPickItemFrame_ItemInfo selectedSearchItem = null;

		/// <summary>
		/// Vật phẩm không nhặt được chọn
		/// </summary>
		private UIAutoFight_AutoPickItemTab_SelectNoPickItemFrame_ItemInfo selectedOutputItem = null;

		/// <summary>
		/// Danh sách vật phẩm không nhặt theo tên
		/// </summary>
		private readonly HashSet<string> outputItemsName = new HashSet<string>();
		#endregion

		#region Properties
		/// <summary>
		/// Danh sách vật phẩm không nhặt
		/// </summary>
		public List<int> OutputItems { get; set; }
		#endregion

		#region Core MonoBehaviour
		/// <summary>
		/// Hàm này gọi khi đối tượng được tạo ra
		/// </summary>
		private void Awake()
		{
			this.transformSearchItemList = this.UI_SearchItemInfoPrefab.transform.parent.GetComponent<RectTransform>();
			this.transformOutputItemList = this.UI_OutputItemInfoPrefab.transform.parent.GetComponent<RectTransform>();
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
			this.UIButton_Close.onClick.AddListener(this.ButtonClose_Clicked);
			this.UIButton_AddToOutput.onClick.AddListener(this.ButtonAddToOutputList_Clicked);
			this.UIButton_RemoveFromOutput.onClick.AddListener(this.ButtonRemoveFromOutputList_Clicked);
			this.UIButton_SearchItem.onClick.AddListener(this.ButtonSearchItem_Clicked);
		}

		/// <summary>
		/// Sự kiện khi Button đóng khung được ấn
		/// </summary>
		private void ButtonClose_Clicked()
		{
			this.Hide();
		}

		/// <summary>
		/// Sự kiện khi Button tìm kiếm vật phẩm được ấn
		/// </summary>
		private void ButtonSearchItem_Clicked()
		{
			/// Chuỗi nhập vào
			string input = this.UIInput_SearchString.text;
			/// Chuẩn hóa
			input = Utils.RemoveAllHTMLTags(input);
			input = Utils.BasicNormalizeString(input);
			/// Chuyển về dạng in thường
			input = input.ToLower();
			/// Nếu chuỗi rỗng
			if (string.IsNullOrEmpty(input))
			{
				KTGlobal.AddNotification("Nhập tên vật phẩm cần tìm!");
				return;
			}

			/// Xóa vật phẩm tìm kiếm được chọn
			this.selectedSearchItem = null;

			/// Làm rỗng danh sách
			this.ClearSearchItemList();

			/// Danh sách vật phẩm thỏa mãn (chỉ lấy 10 vật phẩm)
			List<ItemData> items = Loader.Items.Values.Where(x => !KTGlobal.IsEquip(x.ItemID) && x.Name.ToLower().Contains(input) && !this.outputItemsName.Contains(x.Name.ToLower())).GroupBy(x => x.Name.ToLower()).Select(x => x.First()).Take(10).ToList();

			/// Nếu không tìm thấy
			if (items.Count <= 0)
			{
				KTGlobal.AddNotification("Không tìm thấy kết quả!");
				return;
			}

			/// Duyệt danh sách
			foreach (ItemData itemData in items)
			{
				/// Thêm vào danh sách tìm kiếm
				this.AddSearchItem(itemData);
			}

			/// Xây lại giao diện
			this.RebuildSearchLayout();
		}

		/// <summary>
		/// Sự kiện khi Button thêm vào danh sách kết quả được ấn
		/// </summary>
		private void ButtonAddToOutputList_Clicked()
		{
			/// Nếu không có vật phẩm nào được chọn
			if (this.selectedSearchItem)
			{
				KTGlobal.AddNotification("Chưa có vật phẩm nào được chọn!");
				return;
			}

			/// Xóa khỏi danh sách tìm kiếm
			this.RemoveSearchItem(this.selectedSearchItem);
			/// Thêm vào danh sách kết quả
			this.AddOutputItem(this.selectedSearchItem.Data);
			/// Xây lại giao diện kết quả
			this.RebuildOutputLayout();

			/// Xóa vật phẩm được chọn
			this.selectedSearchItem = null;
			/// Hủy Button chức năng
			this.UIButton_AddToOutput.interactable = false;
		}

		/// <summary>
		/// Sự kiện khi Button xóa khỏi danh sách không nhặt được ấn
		/// </summary>
		private void ButtonRemoveFromOutputList_Clicked()
		{
			/// Nếu không có vật phẩm nào được chọn
			if (this.selectedOutputItem)
			{
				KTGlobal.AddNotification("Chưa có vật phẩm nào được chọn!");
				return;
			}

			/// Thêm vào danh sách kết quả
			this.RemoveOutputItem(this.selectedOutputItem);
			/// Xây lại giao diện kết quả
			this.RebuildOutputLayout();

			/// Xóa vật phẩm được chọn
			this.selectedOutputItem = null;
			/// Hủy Button chức năng
			this.UIButton_RemoveFromOutput.interactable = false;
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
		/// <param name="transform"></param>
		private void RebuildLayout(RectTransform transform)
		{
			/// Nếu đối tượng không được kích hoạt
			if (!this.gameObject.activeSelf)
			{
				return;
			}

			/// Thực hiện xây lại giao diện ở Frame tiếp theo
			this.StartCoroutine(this.ExecuteSkipFrames(1, () => {
				UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(transform);
			}));
		}

		/// <summary>
		/// Xây lại giao diện danh sách tìm kiếm vật phẩm
		/// </summary>
		private void RebuildSearchLayout()
		{
			this.RebuildLayout(this.transformSearchItemList);
		}

		/// <summary>
		/// Làm rỗng danh sách tìm kiếm vật phẩm
		/// </summary>
		private void ClearSearchItemList()
		{
			foreach (Transform child in this.transformSearchItemList.transform)
			{
				if (child.gameObject != this.UI_SearchItemInfoPrefab.gameObject)
				{
					GameObject.Destroy(child.gameObject);
				}
			}
		}

		/// <summary>
		/// Thêm vật phẩm vào danh sách tìm kiếm
		/// </summary>
		/// <param name="itemData"></param>
		private void AddSearchItem(ItemData itemData)
		{
			UIAutoFight_AutoPickItemTab_SelectNoPickItemFrame_ItemInfo uiItemInfo = GameObject.Instantiate<UIAutoFight_AutoPickItemTab_SelectNoPickItemFrame_ItemInfo>(this.UI_SearchItemInfoPrefab);
			uiItemInfo.transform.SetParent(this.transformSearchItemList, false);
			uiItemInfo.gameObject.SetActive(true);
			uiItemInfo.Data = itemData;
			uiItemInfo.Select = () => {
				/// Hiện Button thêm
				this.UIButton_AddToOutput.interactable = true;
				/// Đánh dấu vật phẩm được chọn
				this.selectedSearchItem = uiItemInfo;
			};
		}

		/// <summary>
		/// Xóa vật phẩm khỏi danh sách tìm kiếm
		/// </summary>
		/// <param name="uiItemInfo"></param>
		private void RemoveSearchItem(UIAutoFight_AutoPickItemTab_SelectNoPickItemFrame_ItemInfo uiItemInfo)
		{
			GameObject.Destroy(uiItemInfo.gameObject);
			this.RebuildSearchLayout();
		}

		/// <summary>
		/// Xây lại giao diện danh sách kết quả
		/// </summary>
		private void RebuildOutputLayout()
		{
			this.RebuildLayout(this.transformOutputItemList);
		}

		/// <summary>
		/// Làm rỗng danh sách kết quả
		/// </summary>
		private void ClearOutputItemList()
		{
			foreach (Transform child in this.transformOutputItemList.transform)
			{
				if (child.gameObject != this.UI_OutputItemInfoPrefab.gameObject)
				{
					GameObject.Destroy(child.gameObject);
				}
			}
		}

		/// <summary>
		/// Thêm vật phẩm vào danh sách tìm kiếm
		/// </summary>
		/// <param name="itemData"></param>
		private void AddOutputItem(ItemData itemData)
		{
			UIAutoFight_AutoPickItemTab_SelectNoPickItemFrame_ItemInfo uiItemInfo = GameObject.Instantiate<UIAutoFight_AutoPickItemTab_SelectNoPickItemFrame_ItemInfo>(this.UI_OutputItemInfoPrefab);
			uiItemInfo.transform.SetParent(this.transformOutputItemList, false);
			uiItemInfo.gameObject.SetActive(true);
			uiItemInfo.Data = itemData;
			uiItemInfo.Select = () => {
				/// Hiện Button thêm
				this.UIButton_RemoveFromOutput.interactable = true;
				/// Đánh dấu vật phẩm được chọn
				this.selectedOutputItem = uiItemInfo;
			};
		}

		/// <summary>
		/// Xóa vật phẩm khỏi danh sách kết quả
		/// </summary>
		/// <param name="uiItemInfo"></param>
		private void RemoveOutputItem(UIAutoFight_AutoPickItemTab_SelectNoPickItemFrame_ItemInfo uiItemInfo)
		{
			GameObject.Destroy(uiItemInfo.gameObject);
			this.RebuildOutputLayout();
		}

		/// <summary>
		/// Làm mới danh sách
		/// </summary>
		private void Refresh()
		{
			/// Làm rỗng giao diện
			this.ClearOutputItemList();
			this.ClearSearchItemList();

			/// Nếu không tồn tại danh sách kết quả
			if (this.OutputItems == null)
			{
				this.OutputItems = new List<int>();
			}

			/// Làm rỗng danh sách không nhặt theo tên
			this.outputItemsName.Clear();

			/// Duyệt danh sách kết quả
			foreach (int itemID in this.OutputItems)
			{
				/// Thông tin vật phẩm
				if (Loader.Items.TryGetValue(itemID, out ItemData itemData))
				{
					/// Thêm vào danh sách không nhặt
					this.AddOutputItem(itemData);
					/// Thêm vào danh sách không nhặt theo tên
					this.outputItemsName.Add(itemData.Name.ToLower());
				}
			}

			/// Xây lại giao diện
			this.RebuildOutputLayout();
			this.RebuildSearchLayout();
		}
		#endregion

		#region Public methods
		/// <summary>
		/// Hiện khung
		/// </summary>
		public void Show()
		{
			this.gameObject.SetActive(true);
			this.UIInput_SearchString.text = "";
			this.UIButton_AddToOutput.interactable = false;
			this.UIButton_RemoveFromOutput.interactable = false;
			this.selectedOutputItem = null;
			this.selectedSearchItem = null;
			this.Refresh();
		}

		/// <summary>
		/// Ẩn khung
		/// </summary>
		public void Hide()
		{
			this.gameObject.SetActive(false);
		}
		#endregion
	}
}
