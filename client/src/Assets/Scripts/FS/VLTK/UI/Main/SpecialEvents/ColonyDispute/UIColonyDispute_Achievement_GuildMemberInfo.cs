using UnityEngine;
using TMPro;
using Server.Data;

namespace FS.VLTK.UI.Main.SpecialEvents.ColonyDispute
{
	/// <summary>
	/// Thông tin thành tích của thành viên bang hội trong tranh đoạt lãnh thổ
	/// </summary>
	public class UIColonyDispute_Achievement_GuildMemberInfo : MonoBehaviour
	{
		#region Define
		/// <summary>
		/// Text tên thành viên
		/// </summary>
		[SerializeField]
		private TextMeshProUGUI UIText_PlayerName;

		/// <summary>
		/// Text tổng điểm
		/// </summary>
		[SerializeField]
		private TextMeshProUGUI UIText_TotalPoints;
		#endregion

		#region Properties
		private GuildWarRanking _Data;
		/// <summary>
		/// Thông tin thành viên
		/// </summary>
		public GuildWarRanking Data
		{
			get
			{
				return this._Data;
			}
			set
			{
				this._Data = value;

				this.UIText_PlayerName.text = value.RoleName;
				this.UIText_TotalPoints.text = value.Point.ToString();
			}
		}
		#endregion
	}
}
