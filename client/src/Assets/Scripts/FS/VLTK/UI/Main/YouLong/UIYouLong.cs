using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using FS.VLTK.UI.Main.ItemBox;
using Server.Data;
using System.Collections;

namespace FS.VLTK.UI.Main
{
	/// <summary>
	/// Khung Du Long Các
	/// </summary>
	public class UIYouLong : MonoBehaviour
	{
		#region Define
		/// <summary>
		/// Button đóng khung
		/// </summary>
		[SerializeField]
		private UnityEngine.UI.Button UIButton_Close;

		/// <summary>
		/// Prefab ô vật phẩm thưởng
		/// </summary>
		[SerializeField]
		private UIItemBox UIItem_Prefab;

		/// <summary>
		/// Ô phần thưởng nhận được
		/// </summary>
		[SerializeField]
		private UIItemBox UIItem_Award;

		/// <summary>
		/// Text số Tiền Du Long nhận được khi đổi thưởng
		/// </summary>
		[SerializeField]
		private TextMeshProUGUI UIText_CoinExchangeAmount;

		/// <summary>
		/// Button nhận thưởng
		/// </summary>
		[SerializeField]
		private UnityEngine.UI.Button UIButton_GetAward;

		/// <summary>
		/// Button đổi tiền
		/// </summary>
		[SerializeField]
		private UnityEngine.UI.Button UIButton_ExchangeCoin;

		/// <summary>
		/// Button rời khỏi
		/// </summary>
		[SerializeField]
		private UnityEngine.UI.Button UIButton_Exit;

		/// <summary>
		/// Button thử lại
		/// </summary>
		[SerializeField]
		private UnityEngine.UI.Button UIButton_TryAgain;

		/// <summary>
		/// Button vòng mới
		/// </summary>
		[SerializeField]
		private UnityEngine.UI.Button UIButton_NextRound;
		#endregion

		#region Constants
		/// <summary>
		/// Số lượng vật phẩm tối đa
		/// </summary>
		private const int MaxItems = 20;
		#endregion

		#region Private fields
		/// <summary>
		/// RectTransform danh sách ô vật phẩm
		/// </summary>
		private RectTransform transformItemsList = null;

		/// <summary>
		/// Bước hiện tại
		/// </summary>
		private int currentStep = -1;
		#endregion

		#region Properties
		/// <summary>
		/// Danh sách phần thưởng
		/// </summary>
		public List<GoodsData> Items { get; set; }

		/// <summary>
		/// Phần thưởng hiện tại
		/// </summary>
		public GoodsData CurrentAward { get; set; }

		/// <summary>
		/// Số tiền đổi được
		/// </summary>
		public int ExchangeCoinAmount { get; set; }

		/// <summary>
		/// Sự kiện đóng khung
		/// </summary>
		public Action Close { get; set; }

		/// <summary>
		/// Sự kiện đổi thưởng
		/// </summary>
		public Action<int> GetAward { get; set; }

		/// <summary>
		/// Sự kiện đổi tiền
		/// </summary>
		public Action ExchangeCoin { get; set; }

		/// <summary>
		/// Sự kiện rời khỏi phụ bản
		/// </summary>
		public Action Exit { get; set; }

		/// <summary>
		/// Sự kiện thử lại
		/// </summary>
		public Action TryAgain { get; set; }

		/// <summary>
		/// Sự kiện chơi vòng tiếp theo
		/// </summary>
		public Action NextRound { get; set; }
		#endregion

		#region Core MonoBehaviour
		/// <summary>
		/// Hàm này gọi khi đối tượng được tạo ra
		/// </summary>
		private void Awake()
		{
			this.transformItemsList = this.UIItem_Prefab.transform.parent.GetComponent<RectTransform>();
			this.InitDefaultItemsSlot();
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
			this.UIButton_GetAward.onClick.AddListener(this.ButtonGetAward_Clicked);
			this.UIButton_ExchangeCoin.onClick.AddListener(this.ButtonExchangeCoin_Clicked);
			this.UIButton_Exit.onClick.AddListener(this.ButtonExit_Clicked);
			this.UIButton_TryAgain.onClick.AddListener(this.ButtonTryAgain_Clicked);
			this.UIButton_NextRound.onClick.AddListener(this.ButtonNextRound_Clicked);
		}

		/// <summary>
		/// Sự kiện khi Button đóng khung được ấn
		/// </summary>
		private void ButtonClose_Clicked()
		{
			/// Thực thi sự kiện
			this.Close?.Invoke();
		}

		/// <summary>
		/// Sự kiện khi Button đổi quà được ấn
		/// </summary>
		private void ButtonGetAward_Clicked()
		{
			/// Nếu không phải bước nhận thưởng
			if (this.currentStep != 0 && this.currentStep != 1)
			{
				KTGlobal.AddNotification("Không có phần thưởng có thể nhận!");
				return;
			}
			/// Nếu không có phần thưởng nhận
			if (this.currentStep == 1 && this.CurrentAward == null)
			{
				KTGlobal.AddNotification("Không có phần thưởng có thể nhận!");
				return;
			}

			/// Thực thi sự kiện
			this.GetAward?.Invoke(this.currentStep);
		}

		/// <summary>
		/// Sự kiện khi Button đổi tiền được ấn
		/// </summary>
		private void ButtonExchangeCoin_Clicked()
		{
			/// Nếu không có phần thưởng
			if (this.CurrentAward == null)
			{
				KTGlobal.AddNotification("Không có phần thưởng có thể nhận!");
				return;
			}
			/// Nếu không có số tiền quy đổi
			else if (this.ExchangeCoinAmount <= 0)
			{
				KTGlobal.AddNotification("Không thể đổi Tiền Du Long với phần thưởng này!");
				return;
			}

			/// Thực thi sự kiện
			this.ExchangeCoin?.Invoke();
		}

		/// <summary>
		/// Sự kiện khi Button thoát phụ bản được ấn
		/// </summary>
		private void ButtonExit_Clicked()
		{
			KTGlobal.ShowMessageBox("Thông báo", "Xác nhận thoát khỏi Du Long Các?", () => {
				this.Exit?.Invoke();
			}, true);
		}

		/// <summary>
		/// Sự kiện khi Button thử lại được ấn
		/// </summary>
		private void ButtonTryAgain_Clicked()
		{
			/// Nếu có phần thưởng
			if (this.CurrentAward != null)
			{
				KTGlobal.AddNotification("Hãy nhận phần thưởng hoặc đổi tiền trước!");
				return;
			}

			/// Thực thi sự kiện
			this.TryAgain?.Invoke();
		}

		/// <summary>
		/// Sự kiện khi Button qua vòng tiếp theo được ấn
		/// </summary>
		private void ButtonNextRound_Clicked()
		{
			/// Nếu có phần thưởng
			if (this.CurrentAward != null)
			{
				KTGlobal.AddNotification("Hãy nhận phần thưởng hoặc đổi tiền trước!");
				return;
			}

			/// Thực thi sự kiện
			this.NextRound?.Invoke();
		}
		#endregion

		#region Private methods
		/// <summary>
		/// Khởi tạo mặc định các vị trí vật phẩm
		/// </summary>
		private void InitDefaultItemsSlot()
		{
			for (int i = 1; i <= UIYouLong.MaxItems; i++)
			{
				UIItemBox uiItemBox = GameObject.Instantiate<UIItemBox>(this.UIItem_Prefab);
				uiItemBox.gameObject.SetActive(true);
				uiItemBox.transform.SetParent(this.transformItemsList, false);
				uiItemBox.Data = null;
			}
			this.RebuildLayout();
		}

		/// <summary>
		/// Làm rỗng danh sách vật phẩm
		/// </summary>
		private void ClearItems()
		{
			foreach (Transform child in this.transformItemsList.transform)
			{
				if (child.gameObject != this.UIItem_Prefab.gameObject)
				{
					UIItemBox uiItemBox = child.GetComponent<UIItemBox>();
					uiItemBox.Data = null;
				}
			}
		}

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
			/// Thực hiện xây lại giao diện ở Frame tiếp theo
			this.StartCoroutine(this.ExecuteSkipFrames(1, () => {
				UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.transformItemsList);
			}));
		}

		/// <summary>
		/// Thêm vật phẩm vào danh sách
		/// </summary>
		private void AddItems()
		{
			/// Thứ tự vật phẩm
			int idx = 0;
			/// Duyệt danh sách
			foreach (Transform child in this.transformItemsList.transform)
			{
				if (child.gameObject != this.UIItem_Prefab.gameObject)
				{
					UIItemBox uiItemBox = child.GetComponent<UIItemBox>();
					/// Nếu chưa có vật phẩm
					if (uiItemBox.Data == null)
					{
						/// Đặt vật phẩm vào
						uiItemBox.Data = this.Items[idx];
						/// Tăng thứ tự vật phẩm
						idx++;
						/// Nếu vượt quá kích thước danh sách vật phẩm thì bỏ qua
						if (idx >= this.Items.Count)
						{
							break;
						}
					}
				}
			}
		}

		/// <summary>
		/// Xóa vật phẩm tương ứng khỏi danh sách
		/// </summary>
		/// <param name="itemID"></param>
		/// <param name="number"></param>
		private void RemoveItem(int itemID, int number)
		{
			foreach (Transform child in this.transformItemsList.transform)
			{
				if (child.gameObject != this.UIItem_Prefab.gameObject)
				{
					UIItemBox uiItemBox = child.GetComponent<UIItemBox>();
					if (uiItemBox.Data != null && uiItemBox.Data.GoodsID == itemID && uiItemBox.Data.GCount == number)
					{
						uiItemBox.Data = null;
						break;
					}
				}
			}
		}
		#endregion

		#region Public methods
		/// <summary>
		/// Cập nhật thay đổi
		/// </summary>
		/// <param name="step">0: Khởi tạo, 1: Cập nhật phần thưởng, 2: Đổi thưởng xong</param>
		/// <param name="removeFromList"></param>
		public void Refresh(int step, bool removeFromList)
		{
			/// Ẩn các Button chức năng
			this.UIButton_GetAward.interactable = false;
			this.UIButton_ExchangeCoin.interactable = false;
			this.UIButton_NextRound.interactable = false;
			this.UIButton_TryAgain.interactable = false;
			this.UIButton_Exit.interactable = false;
			/// Cập nhật Text số Tiền nhận được
			this.UIText_CoinExchangeAmount.text = "0";

			/// Nếu là bước khỏi tạo
			if (step == 0)
			{
				/// Nếu danh sách rỗng
				if (this.Items == null)
				{
					return;
				}
				/// Làm rỗng danh sách vật phẩm
				this.ClearItems();
				/// Thực hiện gắn các vật phẩm tương ứng
				this.AddItems();
				/// Hiện Button nhận thưởng
				this.UIButton_GetAward.interactable = true;
				/// Đánh dấu bước hiện tại
				this.currentStep = step;
			}
			/// Nếu là bước cập nhật phần thưởng
			else if (step == 1)
			{
				/// Thiết lập phần thưởng vào ô tương ứng
				this.UIItem_Award.Data = this.CurrentAward;
				/// Cập nhật Text số Tiền đổi được
				this.UIText_CoinExchangeAmount.text = this.ExchangeCoinAmount.ToString();
				/// Hiện Button nhận thưởng và đổi tiền
				this.UIButton_GetAward.interactable = true;
				this.UIButton_ExchangeCoin.interactable = true;
				/// Nếu có đánh dấu xóa khỏi danh sách
				if (removeFromList)
				{
					/// Xóa vật phẩm vừa nhận khỏi danh sách
					this.RemoveItem(this.CurrentAward.GoodsID, this.CurrentAward.GCount);
				}
				/// Đánh dấu bước hiện tại
				this.currentStep = step;
			}
			/// Nếu là bước đổi thưởng xong
			else if (step == 2)
			{
				/// Hiện Button vòng kế tiếp, thử lại và thoát
				this.UIButton_TryAgain.interactable = true;
				this.UIButton_NextRound.interactable = true;
				this.UIButton_Exit.interactable = true;
				/// Hủy phần thưởng
				this.CurrentAward = null;
				/// Đánh dấu bước hiện tại
				this.currentStep = step;
			}
		}
		#endregion
	}
}
