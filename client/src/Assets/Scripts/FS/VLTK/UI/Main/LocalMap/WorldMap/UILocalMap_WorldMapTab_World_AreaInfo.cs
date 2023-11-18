using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using FS.VLTK.Entities.Config;

namespace FS.VLTK.UI.Main.LocalMap
{
	/// <summary>
	/// Điểm chỉ khu vực trong bản đồ thế giới
	/// </summary>
	public class UILocalMap_WorldMapTab_World_AreaInfo : MonoBehaviour
	{
		#region Define
		/// <summary>
		/// Button
		/// </summary>
		[SerializeField]
		private UnityEngine.UI.Button UIButton;

		/// <summary>
		/// Text tên khu vực
		/// </summary>
		[SerializeField]
		private TextMeshProUGUI UIText_AreaName;
		#endregion

		#region Properties
		private WorldMapXML.World.Area _Data;
		/// <summary>
		/// Dữ liệu
		/// </summary>
		public WorldMapXML.World.Area Data
		{
			get
			{
				return this._Data;
			}
			set
			{
				this._Data = value;
				this.UIText_AreaName.text = value.Name;
				this.transform.localPosition = new Vector3(value.PosX, value.PosY);
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
