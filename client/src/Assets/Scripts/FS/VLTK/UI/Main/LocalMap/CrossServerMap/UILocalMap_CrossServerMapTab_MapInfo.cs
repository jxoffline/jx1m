using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using FS.VLTK.Utilities.UnityUI;
using FS.VLTK.Entities.Config;

namespace FS.VLTK.UI.Main.LocalMap
{
	/// <summary>
	/// Điểm chỉ địa danh trong bản đồ liên máy chủ
	/// </summary>
	public class UILocalMap_CrossServerMapTab_MapInfo : MonoBehaviour
	{
		#region Define
		/// <summary>
		/// Button
		/// </summary>
		[SerializeField]
		private UnityEngine.UI.Button UIButton;

		/// <summary>
		/// Text tên địa danh
		/// </summary>
		[SerializeField]
		private TextMeshProUGUI UIText_PlaceName;

		/// <summary>
		/// Icon địa danh
		/// </summary>
		[SerializeField]
		private SpriteFromAssetBundle UIImage_Icon;
		#endregion

		#region Properties
		private CrossServerMapXML.MapInfo _Data;
		/// <summary>
		/// Thông tin địa danh
		/// </summary>
		public CrossServerMapXML.MapInfo Data
		{
			get
			{
				return this._Data;
			}
			set
			{
				this._Data = value;
				this.UIText_PlaceName.text = value.MapName;
				this.UIImage_Icon.SpriteName = value.IconName;
				this.UIImage_Icon.Load();
				this.transform.localPosition = new Vector3(value.IconPosX, value.IconPosY);

				switch (value.Type)
				{
					case PlaceType.Village:
					{
						ColorUtility.TryParseHtmlString("#52ff33", out Color color);
						this.UIText_PlaceName.color = color;
						this.UIText_PlaceName.fontSize = 24;
						break;
					}
					case PlaceType.City:
					{
						ColorUtility.TryParseHtmlString("#33beff", out Color color);
						this.UIText_PlaceName.color = color;
						this.UIText_PlaceName.fontSize = 24;
						break;
					}
					case PlaceType.Faction:
					{
						ColorUtility.TryParseHtmlString("#ff66c4", out Color color);
						this.UIText_PlaceName.color = color;
						this.UIText_PlaceName.fontSize = 24;
						break;
					}
					case PlaceType.OutMap:
					{
						ColorUtility.TryParseHtmlString("#ffffff", out Color color);
						this.UIText_PlaceName.color = color;
						this.UIText_PlaceName.fontSize = 24;
						break;
					}
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
