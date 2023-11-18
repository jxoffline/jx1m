using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace FS.VLTK.UI.Main.InputBox
{
	/// <summary>
	/// Khung nhập chuỗi gì đó gửi về Server
	/// </summary>
	public class UIInputString : MonoBehaviour
	{
		#region Define
		/// <summary>
		/// Text mô tả
		/// </summary>
		[SerializeField]
		private TextMeshProUGUI UIText_Description;

		/// <summary>
		/// Input nhập chuỗi đầu vào
		/// </summary>
		[SerializeField]
		private TMP_InputField UIInput_Text;

		/// <summary>
		/// Button xác nhận
		/// </summary>
		[SerializeField]
		private UnityEngine.UI.Button UIButton_OK;

		/// <summary>
		/// Button hủy bỏ
		/// </summary>
		[SerializeField]
		private UnityEngine.UI.Button UIButton_Cancel;
		#endregion

		#region Properties
		/// <summary>
		/// Mô tả khung
		/// </summary>
		public string Description
		{
			get
			{
				return this.UIText_Description.text;
			}
			set
			{
				this.UIText_Description.text = value;
			}
		}

		/// <summary>
		/// Chuỗi nhập
		/// </summary>
		public string Text
		{
			get
			{
				return this.UIInput_Text.text;
			}
			set
			{
				this.UIInput_Text.text = value;
			}
		}

		/// <summary>
		/// Sự kiện xác nhận
		/// </summary>
		public Action<string> OK { get; set; }

		/// <summary>
		/// Sự kiện hủy bỏ
		/// </summary>
		public Action Cancel { get; set; }
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
			this.UIButton_OK.onClick.AddListener(this.ButtonOK_Clicked);
			this.UIButton_Cancel.onClick.AddListener(this.ButtonCancel_Clicked);
		}

		/// <summary>
		/// Sự kiện khi Button xác nhận được ấn
		/// </summary>
		private void ButtonOK_Clicked()
		{
			/// Chuỗi đã nhập vào
			string inputText = this.UIInput_Text.text;
			inputText = Utils.RemoveAllHTMLTags(inputText);
			inputText = Utils.BasicNormalizeString(inputText);
			/// Nếu toạc
			if (string.IsNullOrEmpty(inputText))
			{
				KTGlobal.AddNotification("Hãy nhập giá trị!");
				return;
			}
			/// Thực thi sự kiện
			this.OK?.Invoke(inputText);
			this.Destroy();
		}

		/// <summary>
		/// Sự kiện khi Button hủy bỏ được ấn
		/// </summary>
		private void ButtonCancel_Clicked()
		{
			this.Cancel?.Invoke();
			this.Destroy();
		}
		#endregion

		#region Private methods
		/// <summary>
		/// Hủy đối tượng
		/// </summary>
		private void Destroy()
		{
			GameObject.Destroy(this.gameObject);
		}
		#endregion
	}
}
