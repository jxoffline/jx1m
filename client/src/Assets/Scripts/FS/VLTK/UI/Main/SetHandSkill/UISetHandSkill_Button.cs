using FS.VLTK.Entities.Config;
using FS.VLTK.Utilities.UnityUI;
using System;
using UnityEngine;

namespace FS.VLTK.UI.Main.SetHandSkill
{
	/// <summary>
	/// Button kỹ năng trong khung chọn kỹ năng 2 tay
	/// </summary>
	public class UISetHandSkill_Button : MonoBehaviour
	{
		#region Define
		/// <summary>
		/// Image Icon kỹ năng
		/// </summary>
		[SerializeField]
		private SpriteFromAssetBundle UIImage_SkillIcon;

		/// <summary>
		/// Button kỹ năng
		/// </summary>
		[SerializeField]
		private UnityEngine.UI.Button UIButton;
		#endregion

		#region Properties
		private SkillDataEx _Data;
		/// <summary>
		/// Dữ liệu kỹ năng
		/// </summary>
		public SkillDataEx Data
		{
			get
			{
				return this._Data;
			}
			set
			{
				this._Data = value;
				/// Nếu không có dữ liệu kỹ năng
				if (value == null)
				{
					/// Ẩn Icon
					this.UIImage_SkillIcon.gameObject.SetActive(false);
				}
				/// Nếu có dữ liệu kỹ năng
				else
				{
					/// Hiện Icon
					this.UIImage_SkillIcon.gameObject.SetActive(true);
					/// Thiết lập thông tin ảnh
					this.UIImage_SkillIcon.BundleDir = value.IconBundleDir;
					this.UIImage_SkillIcon.AtlasName = value.IconAtlasName;
					this.UIImage_SkillIcon.SpriteName = value.Icon;
					/// Tải lại Icon kỹ năng
					this.UIImage_SkillIcon.Load();
				}
			}
		}
		
		/// <summary>
		/// Sự kiện Click
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
		/// Sự kiện khi Button được ấn
		/// </summary>
		private void Button_Clicked()
		{
			this.Click?.Invoke();
		}
		#endregion
	}
}
