using Server.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace FS.VLTK.UI.Main.Ranking
{
    /// <summary>
    /// Khung bảng xếp hạng
    /// </summary>
    public class UIRanking_PlayerInfo : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Text thứ hạng
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Rank;

        /// <summary>
        /// Text tên người chơi
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Name;

        /// <summary>
        /// Cấp độ
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Level;

        /// <summary>
        /// Tên môn phái
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_FactionName;

        /// <summary>
        /// Giá trị trong xếp hạng
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Value;
        #endregion

        #region Properties
        private PlayerRanking _Data;
        /// <summary>
        /// Thông tin người chơi
        /// </summary>
        public PlayerRanking Data
        {
            get
            {
                return this._Data;
            }
            set
            {
                if (value == null)
                {
                    this.UIText_Rank.text = "";
                    this.UIText_Name.text = "";
                    this.UIText_Level.text = "";
                    this.UIText_FactionName.text = "";
                    this.UIText_Value.text = "";
                    return;
                }
                this.UIText_Rank.text = string.Format("{0}", value.ID+1);
                this.UIText_Name.text = value.RoleName;
                this.UIText_Level.text = string.Format("{0}", value.Level);
                this.UIText_FactionName.text = KTGlobal.GetFactionName(value.FactionID, out Color factionColor);
                this.UIText_FactionName.color = factionColor;
                this.UIText_Value.text = string.Format("{0}", value.Value);
            }
        }
        #endregion
    }
}
