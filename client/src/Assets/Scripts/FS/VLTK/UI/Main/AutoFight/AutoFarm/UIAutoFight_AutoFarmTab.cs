using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Collections;
using FS.VLTK.Entities.Config;

namespace FS.VLTK.UI.Main.AutoFight
{
	/// <summary>
	/// Tab thiết lập Auto đánh quái
	/// </summary>
	public class UIAutoFight_AutoFarmTab : MonoBehaviour
	{
		#region Define
		/// <summary>
		/// Toggle đánh quanh điểm
		/// </summary>
		[SerializeField]
		private UnityEngine.UI.Toggle UIToggle_FarmAround;

		/// <summary>
		/// Toggle chỉ đánh 1 mục tiêu
		/// </summary>
		[SerializeField]
		private UnityEngine.UI.Toggle UIToggle_SingleTarget;

		/// <summary>
		/// Text phạm vi đánh
		/// </summary>
		[SerializeField]
		private TextMeshProUGUI UIText_ScanRange;

		/// <summary>
		/// Button nhập phạm vi đánh
		/// </summary>
		[SerializeField]
		private UnityEngine.UI.Button UIButton_InputScanRange;

		/// <summary>
		/// Prefab ô thiết lập kỹ năng đánh
		/// </summary>
		[SerializeField]
		private UIAutoFight_SkillButton UIButton_SkillPrefab;

		/// <summary>
		/// Toggle tự đốt lửa trại
		/// </summary>
		[SerializeField]
		private UnityEngine.UI.Toggle UIToggle_AutoFireCamp;

		/// <summary>
		/// Toggle bỏ qua Boss
		/// </summary>
		[SerializeField]
		private UnityEngine.UI.Toggle UIToggle_IgnoreBoss;

		/// <summary>
		/// Toggle ưu tiên mục tiêu ít máu
		/// </summary>
		[SerializeField]
		private UnityEngine.UI.Toggle UIToggle_LowHPTargetPriority;

		/// <summary>
		/// Toggle sử dụng kỹ năng tân thủ
		/// </summary>
		[SerializeField]
		private UnityEngine.UI.Toggle UIToggle_UseNewbieSkill;

		/// <summary>
		/// Toggle tự uống rượu
		/// </summary>
		[SerializeField]
		private UnityEngine.UI.Toggle UIToggle_AutoUseWine;
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
		/// Đánh xung quanh
		/// </summary>
		public bool FarmAround
		{
			get
			{
				return this.UIToggle_FarmAround.isOn;
			}
			set
			{
				this.UIToggle_FarmAround.isOn = value;
			}
		}

		private int _ScanRange = 100;
		/// <summary>
		/// Phạm vi quét
		/// </summary>
		public int ScanRange
		{
			get
			{
				return this._ScanRange;
			}
			set
			{
				this._ScanRange = value;
				this.UIText_ScanRange.text = this._ScanRange.ToString();
			}
		}

		/// <summary>
		/// Đơn mục tiêu
		/// </summary>
		public bool SingleTarget
		{
			get
			{
				return this.UIToggle_SingleTarget.isOn;
			}
			set
			{
				this.UIToggle_SingleTarget.isOn = value;
			}
		}

		/// <summary>
		/// Tự đốt lửa trại
		/// </summary>
		public bool AutoFireCamp
		{
			get
			{
				return this.UIToggle_AutoFireCamp.isOn;
			}
			set
			{
				this.UIToggle_AutoFireCamp.isOn = value;
			}
		}

		/// <summary>
		/// Bỏ qua Boss
		/// </summary>
		public bool IgnoreBoss
		{
			get
			{
				return this.UIToggle_IgnoreBoss.isOn;
			}
			set
			{
				this.UIToggle_IgnoreBoss.isOn = value;
			}
		}

		/// <summary>
		/// Ưu tiên mục tiêu máu ít
		/// </summary>
		public bool LowHPTargetPriority
		{
			get
			{
				return this.UIToggle_LowHPTargetPriority.isOn;
			}
			set
			{
				this.UIToggle_LowHPTargetPriority.isOn = value;
			}
		}

		/// <summary>
		/// Dùng kỹ năng tân thủ
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
		/// Tự uống rượu
		/// </summary>
		public bool AutoUseWine
		{
			get
			{
				return this.UIToggle_AutoUseWine.isOn;
			}
			set
			{
				this.UIToggle_AutoUseWine.isOn = value;
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
			this.UIButton_InputScanRange.onClick.AddListener(this.ButtonInputScanRange_Clicked);
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

		/// <summary>
		/// Sự kiện khi Button nhập phạm vi quét được ấn
		/// </summary>
		private void ButtonInputScanRange_Clicked()
		{
			KTGlobal.ShowInputNumber("Nhập phạm vi tìm mục tiêu.", 0, 1000, (number) => {
				this.ScanRange = number;
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
