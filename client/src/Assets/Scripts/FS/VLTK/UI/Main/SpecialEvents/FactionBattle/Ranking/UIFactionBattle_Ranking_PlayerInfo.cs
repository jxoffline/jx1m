using Server.Data;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace FS.VLTK.UI.Main.SpecialEvents.FactionBattle
{
    /// <summary>
    /// Thông tin ngươi chơi thi đấu môn phái
    /// </summary>
    public class UIFactionBattle_Ranking_PlayerInfo : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Text Hạng
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Rank;

        /// <summary>
        /// Text Tên
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Name;

        /// <summary>
        /// Text Phái
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_FactionName;

        /// <summary>
        /// Text Cấp
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Level;

        /// <summary>
        /// Text Điểm số
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Score;

        /// <summary>
        /// Text số mạng giết
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_KillsCount;

        /// <summary>
        /// Text liên trảm tối đa
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_MaxStreak;
        #endregion

        #region Properties
        private FACTION_PVP_RANKING _Data;
        /// <summary>
        /// Dữ liệu bảng xếp hạng Tống Kim
        /// </summary>
        public FACTION_PVP_RANKING Data
        {
            get
            {
                return this._Data;
            }
            set
            {
                this._Data = value;

                this.UIText_Rank.text = value.Rank.ToString();
                this.UIText_Name.text = value.PlayerName;
                this.UIText_Level.text = value.Level.ToString();
                this.UIText_FactionName.text = KTGlobal.GetFactionName(value.Faction, out Color factionColor);
                this.UIText_FactionName.color = factionColor;
                this.UIText_Score.text = value.Score.ToString();
                this.UIText_KillsCount.text = value.KillCount.ToString();
                this.UIText_MaxStreak.text = value.MaxKillStreak.ToString();
            }
        }
        #endregion
    }
}