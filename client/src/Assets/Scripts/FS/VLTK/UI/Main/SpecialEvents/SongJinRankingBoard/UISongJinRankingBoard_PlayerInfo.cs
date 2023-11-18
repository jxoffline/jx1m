using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using Server.Data;
using FS.GameEngine.Logic;

namespace FS.VLTK.UI.Main.SpecialEvents.SongJinRankingBoard
{
    /// <summary>
    /// Thông tin người chơi trong bảng xếp hạng Tống Kim
    /// </summary>
    public class UISongJinRankingBoard_PlayerInfo : MonoBehaviour
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

        /// <summary>
        /// Text phe
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Site;
        #endregion

        #region Private fields

        #endregion

        #region Properties
        private SongJinRanking _Data;
        /// <summary>
        /// Dữ liệu bảng xếp hạng Tống Kim
        /// </summary>
        public SongJinRanking Data
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
                this.UIText_Site.text = value.Camp;
            }
        }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {

        }

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

        }
        #endregion

        #region Private methods

        #endregion

        #region Public methods

        #endregion
    }
}
