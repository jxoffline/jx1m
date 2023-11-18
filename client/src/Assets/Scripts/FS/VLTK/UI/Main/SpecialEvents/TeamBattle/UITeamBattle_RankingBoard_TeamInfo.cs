using UnityEngine;
using TMPro;
using Server.Data;

namespace FS.VLTK.UI.Main.SpecialEvents.TeamBattle
{
    /// <summary>
    /// Thông tin chiến đội trong khung danh sách chiến đội Võ lâm liên đấu
    /// </summary>
    public class UITeamBattle_RankingBoard_TeamInfo : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Text hạng
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Rank;

        /// <summary>
        /// Text tên chiến đội
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Name;

        /// <summary>
        /// Text danh sách thành viên
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Members;

        /// <summary>
        /// Text tổng điểm
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Score;

        /// <summary>
        /// Text bậc
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Stage;

        /// <summary>
        /// Text tổng số trận
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_TotalBattles;

        /// <summary>
        /// Text thời gian thắng trận lần cuối
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_LastWinTimes;
        #endregion

        #region Properties
        private TeamBattleInfo _Data;
        /// <summary>
        /// Thông tin chiến đội
        /// </summary>
        public TeamBattleInfo Data
        {
            get
            {
                return this._Data;
            }
            set
            {
                this._Data = value;
                this.UIText_Rank.text = value.Rank.ToString();
                this.UIText_Name.text = value.Name;
                this.UIText_Members.text = string.Join("<color=white>,</color> ", value.Members.Values);
                this.UIText_Score.text = value.Point.ToString();
                this.UIText_Stage.text = value.Stage.ToString();
                this.UIText_TotalBattles.text = value.TotalBattles.ToString();
                /// Nếu chưa cập nhật
                if (value.LastWinTime == System.DateTime.MinValue)
                {
                    this.UIText_LastWinTimes.text = "Chưa có";
                }
                else
                {
                    this.UIText_LastWinTimes.text = value.LastWinTime.ToString("HH:mm - dd/MM/yyyy");
                }
            }
        }
        #endregion
    }
}
