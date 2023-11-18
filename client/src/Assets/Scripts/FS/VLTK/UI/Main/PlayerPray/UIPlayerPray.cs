using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using FS.VLTK.UI.Main.ItemBox;
using System.Collections;
using FS.VLTK.Entities.Config;
using System.Text;
using Server.Data;

namespace FS.VLTK.UI.Main
{
	/// <summary>
	/// Khung vòng quay chúc phúc
	/// </summary>
	public class UIPlayerPray : MonoBehaviour
	{
		#region Define
		/// <summary>
		/// Button đóng khung
		/// </summary>
		[SerializeField]
		private UnityEngine.UI.Button UIButton_Close;

		/// <summary>
		/// Text bói toán việc nên làm
		/// </summary>
		[SerializeField]
		private TextMeshProUGUI UIText_PrayThing;

		/// <summary>
		/// Text giải thích kết quả
		/// </summary>
		[SerializeField]
		private TextMeshProUGUI UIText_PrayWord;

		/// <summary>
		/// Prefab phần thưởng chúc phúc
		/// </summary>
		[SerializeField]
		private UIItemBox UIItem_AwardPrefab;

		/// <summary>
		/// Text Buff tương ứng nhận được
		/// </summary>
		[SerializeField]
		private TextMeshProUGUI UIText_Buffs;

		/// <summary>
		/// Button bắt đầu quay
		/// </summary>
		[SerializeField]
		private UnityEngine.UI.Button UIButton_StartPray;
		
		/// <summary>
		/// Button nhận thưởng chúc phúc
		/// </summary>
		[SerializeField]
		private UnityEngine.UI.Button UIButton_GetAward;

		/// <summary>
		/// Kim quay
		/// </summary>
		[SerializeField]
		private RectTransform UI_Needle;

		/// <summary>
		/// Icon KIM
		/// </summary>
		[SerializeField]
		private RectTransform UI_MetalIcon;

		/// <summary>
		/// Icon MỘC
		/// </summary>
		[SerializeField]
		private RectTransform UI_WoodIcon;

		/// <summary>
		/// Icon THỦY
		/// </summary>
		[SerializeField]
		private RectTransform UI_WaterIcon;

		/// <summary>
		/// Icon HỎA
		/// </summary>
		[SerializeField]
		private RectTransform UI_FireIcon;

		/// <summary>
		/// Icon THỔ
		/// </summary>
		[SerializeField]
		private RectTransform UI_EarthIcon;

		/// <summary>
		/// Gia tốc quay
		/// </summary>
		[SerializeField]
		private float Acceleration = 0.01f;

		/// <summary>
		/// Text số lượt quay còn lại
		/// </summary>
		[SerializeField]
		private TextMeshProUGUI UIText_TotalTurnLeft;

		/// <summary>
		/// Text lịch sử quay
		/// </summary>
		[SerializeField]
		private TextMeshProUGUI UIText_History;
		#endregion

		#region Private fields
		/// <summary>
		/// Tọa độ dừng lại lần trước
		/// </summary>
		private int lastStopAngle = 0;

		/// <summary>
		/// RectTransform luận giải
		/// </summary>
		private RectTransform extensionTransform = null;

		/// <summary>
		/// RectTransform vật phẩm thưởng
		/// </summary>
		private RectTransform awardItemsTransform = null;
		#endregion

		#region Properties
		/// <summary>
		/// Sự kiện đóng khung
		/// </summary>
		public Action Close { get; set; }

		/// <summary>
		/// Sự kiện bắt đầu quay
		/// </summary>
		public Action StartPray { get; set; }

		/// <summary>
		/// Sự kiện nhận thưởng
		/// </summary>
		public Action GetAward { get; set; }
		#endregion

		#region Core MonoBehaviour
		/// <summary>
		/// Hàm này gọi khi đối tượng được tạo ra
		/// </summary>
		private void Awake()
		{
			this.extensionTransform = this.UIText_PrayThing.transform.parent.GetComponent<RectTransform>();
			this.awardItemsTransform = this.UIItem_AwardPrefab.transform.parent.GetComponent<RectTransform>();
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
			this.UIButton_StartPray.onClick.AddListener(this.ButtonStartPray_Clicked);
			this.UIButton_GetAward.onClick.AddListener(this.ButtonGetAward_Clicked);
		}

		/// <summary>
		/// Sự kiện khi Button đóng khung được ấn
		/// </summary>
		private void ButtonClose_Clicked()
		{
			this.Close?.Invoke();
		}

		/// <summary>
		/// Sự kiện khi Button bắt đầu chúc phúc được ấn
		/// </summary>
		private void ButtonStartPray_Clicked()
		{
			/// Thực thi sự kiện
			this.StartPray?.Invoke();
		}

		/// <summary>
		/// Sự kiện khi Button nhận thưởng được ấn
		/// </summary>
		private void ButtonGetAward_Clicked()
		{
			/// Thực thi sự kiện
			this.GetAward?.Invoke();
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
		/// Làm rỗng danh sách quà thưởng
		/// </summary>
		private void ClearAwardList()
		{
			foreach (Transform child in this.awardItemsTransform.transform)
			{
				if (child.gameObject != this.UIItem_AwardPrefab.gameObject)
				{
					GameObject.Destroy(child.gameObject);
				}
			}
		}

		/// <summary>
		/// Thêm quà thưởng
		/// </summary>
		/// <param name="itemID"></param>
		/// <param name="count"></param>
		private void AddAward(int itemID, int count)
		{
			/// Nếu vật phẩm không tồn tại thì thôi
			if (!Loader.Loader.Items.TryGetValue(itemID, out ItemData itemData))
			{
				return;
			}

			UIItemBox uiItemBox = GameObject.Instantiate<UIItemBox>(this.UIItem_AwardPrefab);
			uiItemBox.transform.SetParent(this.awardItemsTransform, false);
			uiItemBox.gameObject.SetActive(true);
			GoodsData itemGD = KTGlobal.CreateItemPreview(itemData);
			itemGD.GCount = count;
			itemGD.Binding = 1;
			uiItemBox.Data = itemGD;
		}

		/// <summary>
		/// Xây lại giao diện danh sách vật phẩm thưởng
		/// </summary>
		private void RebuildAwardList()
		{
			this.RebuildLayout(this.awardItemsTransform);
		}

		#region Core Roll
		/// <summary>
		/// Chuyển góc quay sang loại ngũ hành
		/// </summary>
		/// <param name="angle"></param>
		/// <returns></returns>
		private int AngleToPos(int angle)
		{
			while (angle > 360)
			{
				angle %= 360;
			}
			while (angle < 0)
			{
				angle = -angle % 360;
			}

			if (angle > 0 && angle <= 72)
			{
				return 5;
			}
			else if (angle > 72 && angle <= 144)
			{
				return 4;
			}
			else if (angle > 144 && angle <= 216)
			{
				return 3;
			}
			else if (angle > 216 && angle <= 288)
			{
				return 2;
			}
			else
			{
				return 1;
			}
		}

		/// <summary>
		/// Chuyển loại ngũ hành sang góc quay đầu tiên của ngũ hành này
		/// </summary>
		/// <param name="pos"></param>
		/// <returns></returns>
		private int PosToStartAngle(int pos)
		{
			if (pos == 1)
			{
				return 0;
			}
			else if (pos == 2)
			{
				return -72;
			}
			else if (pos == 3)
			{
				return -144;
			}
			else if (pos == 4)
			{
				return -216;
			}
			else
			{
				return -288;
			}
		}

		/// <summary>
		/// Kích hoạt ảnh tương ứng vị trí đang dừng lại
		/// </summary>
		/// <param name="pos"></param>
		private void ActivateIconCorrespondingToPos(int pos)
		{
			this.UI_MetalIcon.gameObject.SetActive(pos == 1);
			this.UI_WoodIcon.gameObject.SetActive(pos == 2);
			this.UI_WaterIcon.gameObject.SetActive(pos == 3);
			this.UI_FireIcon.gameObject.SetActive(pos == 4);
			this.UI_EarthIcon.gameObject.SetActive(pos == 5);
		}

		/// <summary>
		/// Bắt đầu quay
		/// </summary>
		/// <param name="round"></param>
		/// <param name="stopPos"></param>
		/// <param name="callback"></param>
		/// <returns></returns>
		private IEnumerator StartRoll(int round, int stopPos, Action callback)
		{
			/// Hủy tương tác với Button chức năng
			this.UIButton_GetAward.interactable = false;
			this.UIButton_StartPray.interactable = false;

			/// Góc dừng lần trước
			int lastStopAngle = this.lastStopAngle;
			/// Góc dừng lần này
			int currentStopAngle = this.PosToStartAngle(stopPos) - UnityEngine.Random.Range(20, 52);
			/// Tổng góc quay
			int totalAngle = this.lastStopAngle + round * 360 - currentStopAngle;
			/// Đánh dấu góc dừng lần này
			this.lastStopAngle = currentStopAngle;

			/// Ẩn toàn bộ Icon
			this.ActivateIconCorrespondingToPos(-1);

			/// Nửa số góc quay
			int halfAngle = totalAngle / 2;
			/// Tổng số góc đã quay
			float rolledAngle = 0;
			/// Vận tốc tức thời
			float dv = 0;
			/// Tổng thời gian đã qua
			float lifeTime = 0f;
			/// Lặp liên tục
			while (true)
			{
				/// Bỏ qua Frame đầu tiên
				yield return null;

				/// Tăng tổng thời gian đã qua
				lifeTime += Time.deltaTime;

				/// Nếu là nửa quãng đường đầu
				if (rolledAngle <= halfAngle)
				{
					/// Vận tốc tức thời (dv = a * dt)
					dv += lifeTime * this.Acceleration;
					/// Vị trí tiếp theo (ds = dv * dt)
					rolledAngle += dv * lifeTime;
					/// Thiết lập góc quay
					this.UI_Needle.localRotation = Quaternion.Euler(new Vector3(0, 0, lastStopAngle - rolledAngle));
				}
				/// Nếu là nửa quãng đường sau
				else
				{
					/// Vận tốc tức thời (dv = a * dt)
					dv -= lifeTime * this.Acceleration;
					/// Vị trí tiếp theo (ds = dv * dt)
					rolledAngle += dv * lifeTime;
					if (rolledAngle > totalAngle)
					{
						rolledAngle = totalAngle;
					}
					/// Thiết lập góc quay
					this.UI_Needle.localRotation = Quaternion.Euler(new Vector3(0, 0, lastStopAngle - rolledAngle));

					/// Nếu đã đến đích
					if (dv <= 0)
					{
						if (rolledAngle < totalAngle)
						{
							yield return null;
							rolledAngle = totalAngle;

							/// Thiết lập góc quay
							this.UI_Needle.localRotation = Quaternion.Euler(new Vector3(0, 0, lastStopAngle - rolledAngle));
						}
						break;
					}
				}

				/// Góc hiện tại
				int curAngle = (int) this.UI_Needle.localRotation.eulerAngles.z;
				/// Vị trí tương ứng
				int curPos = this.AngleToPos(curAngle);
				/// Kích hoạt ảnh tương ứng
				this.ActivateIconCorrespondingToPos(curPos);
			}

			/// Thực thi sự kiện khi hoàn tất
			callback?.Invoke();
		}
		#endregion
		#endregion

		#region Public methods
		/// <summary>
		/// Làm rỗng dữ liệu
		/// </summary>
		public void ClearData(int totalTurnLeft)
		{
			/// Thiết lập Button chức năng
			this.UIButton_StartPray.interactable = totalTurnLeft > 0;
			this.UIButton_GetAward.interactable = false;

			/// Ẩn Text
			this.UIText_PrayThing.text = "";
			this.UIText_PrayWord.text = "";
			this.UIText_Buffs.text = "";
			this.UIText_History.text = "Chưa Chúc phúc";

			/// Khôi phục lại vị trí của kim
			this.lastStopAngle = 0;
			/// Thiết lập góc quay
			this.UI_Needle.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));
			/// Ẩn toàn bộ Icon
			this.ActivateIconCorrespondingToPos(-1);

			/// Xóa danh sách vật phẩm thưởng
			this.ClearAwardList();

			/// Số lượt quay trong ngày
			this.UIText_TotalTurnLeft.text = string.Format("Số lượt còn lại: <color=green>{0}</color>", totalTurnLeft);

			this.RebuildAwardList();
			this.RebuildLayout(this.UIText_PrayWord.GetComponent<RectTransform>());
			this.RebuildLayout(this.UIText_PrayThing.GetComponent<RectTransform>());
			this.RebuildLayout(this.extensionTransform);
		}

		/// <summary>
		/// Làm mới dữ liệu
		/// </summary>
		/// <param name="lastTurnResult"></param>
		/// <param name="enableGetAward"></param>
		/// <param name="enableStartNewTurn"></param>
		/// <param name="totalTurnLeft"></param>
		public void RefreshData(string lastTurnResult, bool enableGetAward, bool enableStartNewTurn, int totalTurnLeft, bool isOpenPhrase)
		{
			/// Thiết lập Button chức năng
			this.UIButton_StartPray.interactable = enableStartNewTurn;
			this.UIButton_GetAward.interactable = enableGetAward;

			/// Ẩn Text
			this.UIText_PrayThing.text = "";
			this.UIText_PrayWord.text = "";
			this.UIText_Buffs.text = "";
			this.UIText_History.text = "Chưa Chúc phúc";

			/// Xóa danh sách vật phẩm thưởng
			this.ClearAwardList();

			/// Kết quả đã quay lần trước
			if (!string.IsNullOrEmpty(lastTurnResult))
			{
				string[] fields = lastTurnResult.Split('_');

				/// Nếu là mở khung
				if (isOpenPhrase)
				{
					/// Khôi phục lại vị trí của kim
					this.lastStopAngle = 0;
					/// Thiết lập góc quay
					this.UI_Needle.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));
					/// Ẩn toàn bộ Icon
					this.ActivateIconCorrespondingToPos(-1);

					/// Nếu đang quay tiếp
					if (fields.Length < 5)
					{
						/// Vị trí dừng lại lần trước
						int lastStopPos = int.Parse(fields.Last());
						/// Khôi phục lại vị trí của kim
						this.lastStopAngle = this.PosToStartAngle(lastStopPos) - UnityEngine.Random.Range(20, 52);
						/// Thiết lập góc quay
						this.UI_Needle.localRotation = Quaternion.Euler(new Vector3(0, 0, this.lastStopAngle));
						/// Ẩn toàn bộ Icon
						this.ActivateIconCorrespondingToPos(lastStopPos);
					}
				}

				/// Lịch sử quay
				List<string> historyText = new List<string>();
				/// Duyệt danh sách lịch sử
				foreach (string idString in fields)
				{
					if (idString == "1")
					{
						historyText.Add("<color=#ffea29>Kim</color>");
					}
					else if (idString == "2")
					{
						historyText.Add("<color=#5eff29>Mộc</color>");
					}
					else if (idString == "3")
					{
						historyText.Add("<color=#29b4ff>Thủy</color>");
					}
					else if (idString == "4")
					{
						historyText.Add("<color=#ff2929>Hỏa</color>");
					}
					else if (idString == "5")
					{
						historyText.Add("<color=#d3d1cf>Thổ</color>");
					}
				}
				/// Thiết lập lịch sử
				this.UIText_History.text = string.Join(" - ", historyText);

				/// Nếu tồn tại kết quả tương ứng
				if (Loader.Loader.PlayerPrays.TryGetValue(lastTurnResult, out PlayerPrayXML playerPrayData))
				{
					/// Luận giải kết quả
					this.UIText_PrayWord.text = playerPrayData.PrayWord;
					this.UIText_PrayThing.text = playerPrayData.PrayThing;

					/// Danh sách Buff nhận được
					StringBuilder buffInfo = new StringBuilder();
					/// Duyệt danh sách Buff tương ứng kết quả
					foreach (KeyValuePair<int, int> pair in playerPrayData.Buffs)
					{
						/// Nếu tồn tại kỹ năng tương ứng
						if (Loader.Loader.Skills.TryGetValue(pair.Key, out SkillDataEx skillData))
						{
							/// Thêm vào danh sách
							buffInfo.AppendLine(string.Format("{0} (Cấp {1})", skillData.Name, pair.Value));
						}
					}
					/// Thiết lập dữ liệu Buff hỗ trợ
					this.UIText_Buffs.text = buffInfo.ToString();

					/// Duyệt danh sách vật phẩm thưởng nhận được
					foreach (KeyValuePair<int, int> pair in playerPrayData.Items)
					{
						/// Thêm vào danh sách
						this.AddAward(pair.Key, pair.Value);
					}
				}
			}

			/// Số lượt quay trong ngày
			this.UIText_TotalTurnLeft.text = string.Format("Số lượt còn lại: <color=green>{0}</color>", totalTurnLeft);

			this.RebuildAwardList();
			this.RebuildLayout(this.UIText_PrayWord.GetComponent<RectTransform>());
			this.RebuildLayout(this.UIText_PrayThing.GetComponent<RectTransform>());
			this.RebuildLayout(this.extensionTransform);
		}

		/// <summary>
		/// Bắt đầu quay
		/// </summary>
		/// <param name="round"></param>
		/// <param name="stopPos"></param>
		public void Roll(int round, int stopPos, Action callback)
		{
			this.StopAllCoroutines();
			this.StartCoroutine(this.StartRoll(round, stopPos, callback));
		}
		#endregion
	}
}
