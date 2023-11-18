using FS.VLTK.Entities.Config;
using System;
using TMPro;
using UnityEngine;

namespace VLTK.UI.Main.AutoFight.AutoPickUpItem.SelectNoPickItem
{
	/// <summary>
	/// Thông tin vật phẩm trong khung danh sách vật phẩm không nhặt
	/// </summary>
	public class UIAutoFight_AutoPickItemTab_SelectNoPickItemFrame_ItemInfo : MonoBehaviour
	{
		#region Define
		/// <summary>
		/// Toggle
		/// </summary>
		[SerializeField]
		private UnityEngine.UI.Toggle UIToggle;

		/// <summary>
		/// Tên vật phẩm
		/// </summary>
		[SerializeField]
		private TextMeshProUGUI UIText_ItemName;
		#endregion

		#region Properties
		/// <summary>
		/// Sự kiện chọn vật phẩm
		/// </summary>
		public Action Select { get; set; }

		private ItemData _Data;
		/// <summary>
		/// Thông tin vật phẩm
		/// </summary>
		public ItemData Data
		{
			get
			{
				return this._Data;
			}
			set
			{
				this._Data = value;
				/// Thiết lập tên
				this.UIText_ItemName.text = value.Name;
			}
		}
		#endregion

		#region Core MonoBehaviour
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
			this.UIToggle.onValueChanged.AddListener(this.Toggle_Selected);
		}

		/// <summary>
		/// Sự kiện khi Toggle được chọn
		/// </summary>
		/// <param name="isSelected"></param>
		private void Toggle_Selected(bool isSelected)
		{
			if (isSelected)
			{
				this.Select?.Invoke();
			}
		}
		#endregion
	}
}
