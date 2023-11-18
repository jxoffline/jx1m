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
	/// Thông tin đường đi trong bản đồ thế giới
	/// </summary>
	public class UILocalMap_WorldMapTab_WayPortInfo : MonoBehaviour
	{
		#region Define
		/// <summary>
		/// Text tên đường đi
		/// </summary>
		[SerializeField]
		private TextMeshProUGUI UIText_WayPortName;
		#endregion

		#region Properties
		private WorldMapXML.WayPort _Data;
		/// <summary>
		/// Thông tin đường đi
		/// </summary>
		public WorldMapXML.WayPort Data
		{
			get
			{
				return this._Data;
			}
			set
			{
				this._Data = value;
				if (Loader.Loader.Maps.TryGetValue(value.MapCode, out Map mapData))
				{
					this.UIText_WayPortName.text = string.Format("Đến {0}", mapData.Name);
				}
				else
				{
					this.UIText_WayPortName.text = string.Format("Đến {0}", "Chưa rõ");
				}
				this.transform.localPosition = new Vector3(value.IconPosX, value.IconPosY);
			}
		}
		#endregion
	}
}
