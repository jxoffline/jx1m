using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using FS.VLTK.UI.Main.ItemBox;
using FS.VLTK.Entities.Config;
using Server.Data;
using System.Collections;

namespace FS.VLTK.UI.Main.AutoFight
{
	/// <summary>
	/// Tab AutoPK
	/// </summary>
	public class UIAutoFight_AutoPKTab : MonoBehaviour
	{
		#region Define


		/// <summary>
		/// Button tự mời đội
		/// </summary>
		[SerializeField]
		private UnityEngine.UI.Toggle UIToggle_AutoInviteToTeam;

		/// <summary>
		/// Button tự đồng ý vào đội
		/// </summary>
		[SerializeField]
		private UnityEngine.UI.Toggle UIToggle_AutoAcceptInviteToTeam;

		/// <summary>
		/// Toggle tự phản kháng khi bị PK
		/// </summary>
		[SerializeField]
		private UnityEngine.UI.Toggle UIToggle_AutoReflectAttack;

		/// <summary>
		/// Toggle ưu tiên khắc hệ
		/// </summary>
		[SerializeField]
		private UnityEngine.UI.Toggle UIToggle_SeriesConquePriority;

		/// <summary>
		/// Toggle ưu tiên mục tiêu ít máu
		/// </summary>
		[SerializeField]
		private UnityEngine.UI.Toggle UIToggle_LowHPEnemyPriority;

		/// <summary>
		/// Toggle sử dụng kỹ năng tân thủ
		/// </summary>
		[SerializeField]
		private UnityEngine.UI.Toggle UIToggle_UseNewbieSkill;

		/// <summary>
		/// Toggle đuổi mục tiêu
		/// </summary>
		[SerializeField]
		private UnityEngine.UI.Toggle UIToggle_ChaseTarget;

		/// <summary>
		/// Prefab ô kỹ năng
		/// </summary>
		[SerializeField]
		private UIAutoFight_SkillButton UIButton_SkillPrefab;
		#endregion

		#region Private fields
		/// <summary>
		/// RectTransform danh sách kỹ năng
		/// </summary>
		private RectTransform transformSkillList = null;
		#endregion

		#region Properties
		/// <summary>
		/// Khung gốc
		/// </summary>
		public UIAutoFight Parent { get; set; }

		/// <summary>
		/// Tự mời vào nhóm
		/// </summary>
		public bool AutoInviteToTeam
		{
			get
			{
				return this.UIToggle_AutoInviteToTeam.isOn;
			}
			set
			{
				this.UIToggle_AutoInviteToTeam.isOn = value;
			}
		}

		/// <summary>
		/// Tự đồng ý vào nhóm
		/// </summary>
		public bool AutoAcceptInviteToTeam
		{
			get
			{
				return this.UIToggle_AutoAcceptInviteToTeam.isOn;
			}
			set
			{
				this.UIToggle_AutoAcceptInviteToTeam.isOn = value;
			}
		}

		/// <summary>
		/// Tự phản kháng khi bị PK
		/// </summary>
		public bool AutoReflectAttack
		{
			get
			{
				return this.UIToggle_AutoReflectAttack.isOn;
			}
			set
			{
				this.UIToggle_AutoReflectAttack.isOn = value;
			}
		}

		/// <summary>
		/// Ưu tiên khắc hệ
		/// </summary>
		public bool SeriesConquePriority
		{
			get
			{
				return this.UIToggle_SeriesConquePriority.isOn;
			}
			set
			{
				this.UIToggle_SeriesConquePriority.isOn = value;
			}
		}

		/// <summary>
		/// Ưu tiên mục tiêu ít máu
		/// </summary>
		public bool LowHPEnemyPriority
		{
			get
			{
				return this.UIToggle_LowHPEnemyPriority.isOn;
			}
			set
			{
				this.UIToggle_LowHPEnemyPriority.isOn = value;
			}
		}

		/// <summary>
		/// Sử dụng kỹ năng tân thủ
		/// </summary>
		public bool UseNewbieSkill
		{
			get
			{
				return this.UIToggle_UseNewbieSkill.isOn;
			}
			set
			{
				this.UIToggle_UseNewbieSkill.isOn = value;
			}
		}

		/// <summary>
		/// Đuổi mục tiêu
		/// </summary>
		public bool ChaseTarget
		{
			get
			{
				return this.UIToggle_ChaseTarget.isOn;
			}
			set
			{
				this.UIToggle_ChaseTarget.isOn = value;
			}
		}

		/// <summary>
		/// Danh sách kỹ năng
		/// </summary>
		public List<int> Skills { get; set; }
		#endregion

		#region Core MonoBehaviour
		/// <summary>
		/// Hàm này gọi khi đối tượng được tạo ra
		/// </summary>
		private void Awake()
		{
			this.transformSkillList = this.UIButton_SkillPrefab.transform.parent.GetComponent<RectTransform>();
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
		/// Sự kiện khi Button kỹ năng được chọn
		/// </summary>
		/// <param name="idx"></param>
		private void ButtonSkill_Clicked(int idx)
		{
			/// Hiện khung chọn kỹ năng
			this.Parent.ShowSelectSkill((skillData) => {
				/// Vị trí tương ứng
				UIAutoFight_SkillButton uiSkillButton = this.FindSkillButton(idx);
				/// Nếu không tồn tại
				if (uiSkillButton == null)
				{
					return;
				}

				/// Đổ dữ liệu
				uiSkillButton.Data = skillData;
				/// Gắn lại vào Property
				this.Skills[idx - 1] = skillData.ID;
			});
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
			/// Nếu đối tượng không kích hoạt
			if (!this.gameObject.activeSelf)
			{
				return;
			}
			/// Thực hiện xây lại giao diện ở Frame tiếp theo
			this.StartCoroutine(this.ExecuteSkipFrames(1, () => {
				UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.transformSkillList);
			}));
		}

		/// <summary>
		/// Làm rỗng danh sách kỹ năng
		/// </summary>
		private void ClearSkillList()
		{
			foreach (Transform child in this.transformSkillList.transform)
			{
				if (child.gameObject != this.UIButton_SkillPrefab.gameObject)
				{
					GameObject.Destroy(child.gameObject);
				}
			}
		}

		/// <summary>
		/// Khởi tạo các ô mặc định kỹ năng
		/// </summary>
		private void MakeDefaultSlot()
		{
			for (int i = 1; i <= UIAutoFight.NumberOfSkills; i++)
			{
				UIAutoFight_SkillButton uiSkillButton = GameObject.Instantiate<UIAutoFight_SkillButton>(this.UIButton_SkillPrefab);
				uiSkillButton.transform.SetParent(this.transformSkillList, false);
				uiSkillButton.gameObject.SetActive(true);
				uiSkillButton.Data = null;
				uiSkillButton.Slot = i;
				uiSkillButton.Click = () => {
					this.ButtonSkill_Clicked(uiSkillButton.Slot);
				};
				uiSkillButton.ShowArrow = i < UIAutoFight.NumberOfSkills;
			}
		}

		/// <summary>
		/// Tìm ô kỹ năng tại vị trí tương ứng
		/// </summary>
		/// <param name="idx"></param>
		/// <returns></returns>
		private UIAutoFight_SkillButton FindSkillButton(int idx)
		{
			foreach (Transform child in this.transformSkillList.transform)
			{
				if (child.gameObject != this.UIButton_SkillPrefab.gameObject)
				{
					UIAutoFight_SkillButton uiSkillButton = child.GetComponent<UIAutoFight_SkillButton>();
					if (uiSkillButton.Slot == idx)
					{
						return uiSkillButton;
					}
				}
			}
			return null;
		}

		/// <summary>
		/// Làm mới dữ liệu
		/// </summary>
		private void Refresh()
		{
			/// Xóa rỗng danh sách kỹ năng
			this.ClearSkillList();

			/// Tạo mặc định các ô kỹ năng
			this.MakeDefaultSlot();

			/// Nếu không có dữ liệu kỹ năng
			if (this.Skills == null)
			{
				return;
			}

			/// Vị trí
			int idx = 0;
			/// Duyệt danh sách kỹ năng
			foreach (int skillID in this.Skills)
			{
				/// Tăng vị trí lên
				idx++;
				/// Nếu không có kỹ năng ở vị trí này
				if (skillID == -1)
				{
					continue;
				}

				/// Kỹ năng tương ứng
				if (!Loader.Loader.Skills.TryGetValue(skillID, out SkillDataEx skillData))
				{
					continue;
				}

				/// Vị trí tương ứng
				UIAutoFight_SkillButton uiSkillButton = this.FindSkillButton(idx);
				/// Nếu không tồn tại
				if (uiSkillButton == null)
				{
					continue;
				}

				/// Đổ dữ liệu
				uiSkillButton.Data = skillData;
			}

			/// Xây lại giao diện
			this.RebuildLayout();
		}
		#endregion
	}
}
