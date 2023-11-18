using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace FS.VLTK.UI.Main.GiftCode
{
	/// <summary>
	/// Khung nhập GiftCode
	/// </summary>
	public class UIGiftCode : MonoBehaviour
	{
		#region Define
		/// <summary>
		/// Button đóng khung
		/// </summary>
		[SerializeField]
		private UnityEngine.UI.Button UIButton_Close;

		/// <summary>
		/// Khung nhập GiftCode
		/// </summary>
		[SerializeField]
		private TMP_InputField UIInput_GiftCode;

		/// <summary>
		/// Button đồng ý
		/// </summary>
		[SerializeField]
		private UnityEngine.UI.Button UIButton_Enter;
		#endregion

		#region Properties
		/// <summary>
		/// Sự kiện đóng khung
		/// </summary>
		public Action Close { get; set; }

		/// <summary>
		/// Sự kiện đồng ý
		/// </summary>
		public Action<string> Enter { get; set; }
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
			this.UIButton_Close.onClick.AddListener(this.ButtonClose_Clicked);
			this.UIButton_Enter.onClick.AddListener(this.ButtonEnter_Clicked);
		}

		/// <summary>
		/// Sự kiện đóng khung
		/// </summary>
		private void ButtonClose_Clicked()
		{
			/// Thực thi sự kiện
			this.Close?.Invoke();
		}

		/// <summary>
		/// Sự kiện đồng ý
		/// </summary>
		private void ButtonEnter_Clicked()
		{
			/// Chuỗi nhập vào
			string inputText = this.UIInput_GiftCode.text;
			/// Nếu chưa nhập gì
			if (string.IsNullOrEmpty(inputText))
			{
				KTGlobal.AddNotification("Hãy nhập GiftCode!");
				return;
			}
			/// Thực thi sự kiện
			this.Enter?.Invoke(inputText);
		}
		#endregion
	}
}
