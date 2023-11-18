using Server.Data;
using TMPro;
using UnityEngine;

namespace FS.VLTK.UI.Main.SpecialEvents.FHLCScoreboard
{
    /// <summary>
    /// Thông tin người chơi trong khung bảng xếp hạng Phong Hỏa Liên Thành
    /// </summary>
    public class UIFHLCScoreboard_PlayerInfo : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Text hạng
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Rank;

        /// <summary>
        /// Text tên người chơi
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Name;

        /// <summary>
        /// Text cấp độ
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Level;

        /// <summary>
        /// Text môn phái
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Faction;

        /// <summary>
        /// Text điểm số
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Score;
        #endregion

        #region Properties
        private FHLC_Record _Data;
        /// <summary>
        /// Thông tin người chơi
        /// </summary>
        public FHLC_Record Data
        {
            get
            {
                return this._Data;
            }
            set
            {
                this._Data = value;

                /// Hạng
                this.UIText_Rank.text = value.Rank.ToString();
                /// Tên
                this.UIText_Name.text = value.RoleName;
                /// Cấp
                this.UIText_Level.text = value.Level.ToString();
                /// Phái
                this.UIText_Faction.text = KTGlobal.GetFactionName(value.FactionID, out Color color);
                this.UIText_Faction.color = color;
                /// Tổng điểm
                this.UIText_Score.text = KTGlobal.GetDisplayNumber(value.Score);
            }
        }
        #endregion
    }
}
