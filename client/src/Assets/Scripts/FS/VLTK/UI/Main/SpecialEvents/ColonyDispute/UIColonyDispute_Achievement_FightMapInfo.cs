using UnityEngine;
using TMPro;
using Server.Data;

namespace FS.VLTK.UI.Main.SpecialEvents.ColonyDispute
{
	/// <summary>
	/// Thông tin lãnh thổ trong tranh đoạt lãnh thổ
	/// </summary>
	public class UIColonyDispute_Achievement_FightMapInfo : MonoBehaviour
	{
		#region Define
		/// <summary>
		/// Tên lãnh thổ
		/// </summary>
		[SerializeField]
		private TextMeshProUGUI UIText_MapName;

		/// <summary>
		/// Tổng tích lũy bang hội
		/// </summary>
		[SerializeField]
		private TextMeshProUGUI UIText_TotalPoints;

		/// <summary>
		/// Thứ hạng bang hội
		/// </summary>
		[SerializeField]
		private TextMeshProUGUI UIText_Rank;
		#endregion

		#region Properties
		private TerritoryReport _Data;
		/// <summary>
		/// Dữ liệu lãnh thổ
		/// </summary>
		public TerritoryReport Data
		{
			get
			{
				return this._Data;
			}
			set
			{
				this._Data = value;
				this.UIText_MapName.text = value.MapName;
				this.UIText_TotalPoints.text = value.TotalPoint.ToString();
				this.UIText_Rank.text = value.Rank.ToString();
				this.UIText_Rank.color = value.Rank == 1 ? Color.green : Color.red;
			}
		}
		#endregion
	}
}
