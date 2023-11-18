using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using FS.VLTK.UI.Main.SuperToolTip;
using System.Collections;

namespace FS.VLTK.UI.Main
{
	/// <summary>
	/// Khung thông tin vật phẩm hoặc kỹ năng
	/// </summary>
	public class UISuperToolTip : MonoBehaviour
	{
		/// <summary>
		/// Màu sao của trang bị
		/// </summary>
		public enum SuperToolTipEquipStarColor
		{
			/// <summary>
			/// Cơ bản
			/// </summary>
			Basic,
			/// <summary>
			/// Xanh lam
			/// </summary>
			Blue,
			/// <summary>
			/// Tím
			/// </summary>
			Purple,
			/// <summary>
			/// Cam
			/// </summary>
			Orange,
			/// <summary>
			/// Vàng
			/// </summary>
			Yellow,
		}

		#region Define
		/// <summary>
		/// Button đóng khung
		/// </summary>
		[SerializeField]
		private UnityEngine.UI.Button UIButton_Close;

		/// <summary>
		/// Tooltip chính
		/// </summary>
		[SerializeField]
		private UISuperToolTip_Component UI_MainToolTip;

		/// <summary>
		/// Tooltip phụ
		/// </summary>
		[SerializeField]
		private UISuperToolTip_Component UI_SubToolTip;
		#endregion

		#region Private fields
		/// <summary>
		/// RectTransform chứa Tooltip
		/// </summary>
		private RectTransform transformTooltips = null;
		#endregion

		#region Properties
		/// <summary>
		/// Sự kiện đóng khung
		/// </summary>
		public Action Close { get; set; }

		/// <summary>
		/// Tooltip chính
		/// </summary>
		public UISuperToolTip_Component MainToolTip
		{
			get
			{
				return this.UI_MainToolTip;
			}
		}

		/// <summary>
		/// Tooltip phụ
		/// </summary>
		public UISuperToolTip_Component SubToolTip
		{
			get
			{
				return this.UI_SubToolTip;
			}
		}

		/// <summary>
		/// Có hiện Tooltip phụ không
		/// </summary>
		public bool ShowSubToolTip
		{
			get
			{
				return this.UI_SubToolTip.gameObject.activeSelf;
			}
			set
			{
				this.UI_SubToolTip.gameObject.SetActive(value);
			}
		}
		#endregion

		#region Core MonoBehaviour
		/// <summary>
		/// Hàm này gọi khi đối tượng được tạo ra
		/// </summary>
		private void Awake()
		{
			this.transformTooltips = this.UI_MainToolTip.transform.parent.GetComponent<RectTransform>();
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
		private void RebuildLayout()
		{
			/// Nếu đối tượng không được kích hoạt
			if (!this.gameObject.activeSelf)
			{
				return;
			}
			/// Thực hiện xây lại giao diện ở Frame tiếp theo
			this.StartCoroutine(this.ExecuteSkipFrames(1, () => {
				UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.transformTooltips);
			}));
		}
		#endregion

		#region Public methods
		/// <summary>
		/// Xây ToolTip
		/// </summary>
		public void Build()
		{
			this.UI_MainToolTip.Build();
			if (this.ShowSubToolTip)
			{
				this.UI_SubToolTip.Build();
			}
			this.RebuildLayout();
		}
		#endregion
	}
}
