using System;
using UnityEngine;
using TMPro;
using FS.VLTK.UI.Main.SpecialEvents.ColonyDispute;
using Server.Data;
using System.Collections;

namespace FS.VLTK.UI.Main.SpecialEvents
{
	/// <summary>
	/// Chiến báo Tranh đoạt lãnh thổ
	/// </summary>
	public class UIColonyDispute_Achievement : MonoBehaviour
	{
		#region Define
		/// <summary>
		/// Button đóng khung
		/// </summary>
		[SerializeField]
		private UnityEngine.UI.Button UIButton_Close;

		/// <summary>
		/// Text công trạng bản thân
		/// </summary>
		[SerializeField]
		private TextMeshProUGUI UIText_SelfTotalPoints;

		/// <summary>
		/// Text tổng số Long trụ bản thân phá được
		/// </summary>
		[SerializeField]
		private TextMeshProUGUI UIText_SelfTotalDestroyedColumns;

		/// <summary>
		/// Text tổng số kẻ địch bản thân đã giết
		/// </summary>
		[SerializeField]
		private TextMeshProUGUI UIText_SelfTotalKillEnemies;

		/// <summary>
		/// Prefab công trạng thành viên trong bang hội
		/// </summary>
		[SerializeField]
		private UIColonyDispute_Achievement_GuildMemberInfo UI_GuildMemberInfoPrefab;

		/// <summary>
		/// Prefab chiến báo lãnh thổ
		/// </summary>
		[SerializeField]
		private UIColonyDispute_Achievement_FightMapInfo UI_FightMapReportPrefab;
		#endregion

		#region Private fields
		/// <summary>
		/// RectTransform danh sách chiến công của thành viên bang hội
		/// </summary>
		private RectTransform transformGuildMembersList = null;

		/// <summary>
		/// RectTransform danh sách chiến báo lãnh thổ
		/// </summary>
		private RectTransform transformFightMapReportsList = null;
		#endregion

		#region Properties
		/// <summary>
		/// Dữ liệu tranh đoạt chiến
		/// </summary>
		public GuildWarReport Data { get; set; }

		/// <summary>
		/// Sự kiện đóng khung
		/// </summary>
		public Action Close { get; set; }
		#endregion

		#region Core MonoBehaviour
		/// <summary>
		/// Hàm này gọi khi đối tượng được tạo ra
		/// </summary>
		private void Awake()
		{
			this.transformGuildMembersList = this.UI_GuildMemberInfoPrefab.transform.parent.GetComponent<RectTransform>();
			this.transformFightMapReportsList = this.UI_FightMapReportPrefab.transform.parent.GetComponent<RectTransform>();
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
			this.UIButton_Close.onClick.AddListener(this.ButtonClose_Clicked);
		}

		/// <summary>
		/// Sự kiện khi Button đóng khung được ấn
		/// </summary>
		private void ButtonClose_Clicked()
		{
			this.Close?.Invoke();
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
			/// Xây lại giao diện ở Frame tiếp theo
			this.StartCoroutine(this.ExecuteSkipFrames(1, () => {
				UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(transform);
			}));
		}

		/// <summary>
		/// Xây lại giao diện chiến công thành viên bang hội
		/// </summary>
		private void RebuildGuildMemberList()
		{
			this.RebuildLayout(this.transformGuildMembersList);
		}

		/// <summary>
		/// Xây lại giao diện lãnh thổ chiến báo
		/// </summary>
		private void RebuildFightMapReportList()
		{
			this.RebuildLayout(this.transformFightMapReportsList);
		}

		/// <summary>
		/// Làm rỗng danh sách chiến công thành viên bang hội
		/// </summary>
		private void ClearGuildMemberList()
		{
			foreach (Transform child in this.transformGuildMembersList.transform)
			{
				if (child.gameObject != this.UI_GuildMemberInfoPrefab.gameObject)
				{
					GameObject.Destroy(child.gameObject);
				}
			}
		}

		/// <summary>
		/// Làm rỗng danh sách chiến công thành viên bang hội
		/// </summary>
		private void ClearFightMapReportList()
		{
			foreach (Transform child in this.transformFightMapReportsList.transform)
			{
				if (child.gameObject != this.UI_FightMapReportPrefab.gameObject)
				{
					GameObject.Destroy(child.gameObject);
				}
			}
		}

		/// <summary>
		/// Thêm chiến tích thành viên bang hội tương ứng
		/// </summary>
		/// <param name="data"></param>
		private void AddGuildMember(GuildWarRanking data)
		{
			UIColonyDispute_Achievement_GuildMemberInfo uiMemberInfo = GameObject.Instantiate<UIColonyDispute_Achievement_GuildMemberInfo>(this.UI_GuildMemberInfoPrefab);
			uiMemberInfo.transform.SetParent(this.transformGuildMembersList, false);
			uiMemberInfo.gameObject.SetActive(true);
			uiMemberInfo.Data = data;
		}

		/// <summary>
		/// Thêm chiến báo lãnh thổ tương ứng
		/// </summary>
		/// <param name="data"></param>
		private void AddFightMapReport(TerritoryReport data)
		{
			UIColonyDispute_Achievement_FightMapInfo uiFightMapInfo = GameObject.Instantiate<UIColonyDispute_Achievement_FightMapInfo>(this.UI_FightMapReportPrefab);
			uiFightMapInfo.transform.SetParent(this.transformFightMapReportsList, false);
			uiFightMapInfo.gameObject.SetActive(true);
			uiFightMapInfo.Data = data;
		}

		/// <summary>
		/// Làm mới giao diện
		/// </summary>
		private void Refresh()
		{
			/// Xóa danh sách
			this.ClearGuildMemberList();
			this.ClearFightMapReportList();

			/// Xóa Text
			this.UIText_SelfTotalPoints.text = "";
			this.UIText_SelfTotalKillEnemies.text = "";
			this.UIText_SelfTotalDestroyedColumns.text = "";

			/// Nếu không có dữ liệu
			if (this.Data == null)
			{
				return;
			}

			/// Đổ dữ liệu Text
			this.UIText_SelfTotalPoints.text = this.Data._CurrentPoint.Point.ToString();
			this.UIText_SelfTotalDestroyedColumns.text = this.Data._CurrentPoint.TowerDesotryCount.ToString();
			this.UIText_SelfTotalKillEnemies.text = this.Data._CurrentPoint.KillCount.ToString();

			/// Duyệt danh sách chiến công thành viên bang
			foreach (GuildWarRanking memberInfo in this.Data._GuildWarRanking)
			{
				/// Thêm thành viên tương ứng
				this.AddGuildMember(memberInfo);
			}
			/// Duyệt danh sách chiến báo lãnh thổ
			foreach (TerritoryReport fightMapInfo in this.Data._TerritoryReport)
			{
				/// Thêm chiến báo lãnh thổ tương ứng
				this.AddFightMapReport(fightMapInfo);
			}

			/// Xây lại giao diện
			this.RebuildGuildMemberList();
			this.RebuildFightMapReportList();
		}
		#endregion
	}
}
