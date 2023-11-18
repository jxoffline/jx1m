using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FS.VLTK.UI.Main.Chat
{
	/// <summary>
	/// Biểu tượng trong khung chọn biểu tượng
	/// </summary>
	public class UIChat_SelectStickerFrame_StickerButton : MonoBehaviour
	{
		#region Define
		/// <summary>
		/// Đối tượng Button
		/// </summary>
		[SerializeField]
		private UnityEngine.UI.Button UIButton;

		/// <summary>
		/// Đối tượng Icon
		/// </summary>
		[SerializeField]
		private UnityEngine.UI.Image UIImage;
		#endregion

		#region Properties
		/// <summary>
		/// Icon tương ứng
		/// </summary>
		public Sprite Sprite
		{
			get
			{
				return this.UIImage.sprite;
			}
			set
			{
				this.UIImage.sprite = value;
			}
		}

		/// <summary>
		/// Thứ tự Sprite trong danh sách ban đầu
		/// </summary>
		public int Index { get; set; }

		/// <summary>
		/// Sự kiện chọn hình biểu tượng
		/// </summary>
		public Action Click { get; set; }
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
			this.UIButton.onClick.AddListener(this.Button_Clicked);
		}

		/// <summary>
		/// Sự kiện khi Button được chọn
		/// </summary>
		private void Button_Clicked()
		{
			this.Click?.Invoke();
		}
		#endregion
	}
}
