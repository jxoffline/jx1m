using UnityEngine;
using TMPro;
using Server.Data;

namespace FS.VLTK.UI.Main.SpecialEvents.FactionBattle
{
    /// <summary>
    /// Prefab của các Round
    /// </summary>
    public class UIFactionBattle_Round_RoundInfo : MonoBehaviour 
    {
        #region Define
        /// <summary>
        /// Người chơi thi đấu số 1
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Role1;

        /// <summary>
        /// Người chơi thi đấu số 2
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Role2;
        #endregion

        #region Properties
        private ELIMINATION_SCOREBOARD _Data;
        /// <summary>
        /// Thông tin trận
        /// </summary>
        public ELIMINATION_SCOREBOARD Data
        {
            get
            {
                return this._Data;
            }
            set
            {
                /// Nếu không có dữ liệu
                if (value == null)
				{
                    this.UIText_Role1.text = "";
                    this.UIText_Role2.text = "";
				}
				else
				{
                    this.UIText_Role1.text = value.Player_1;
                    this.UIText_Role2.text = value.Player_2;
				}
            }
        }
        #endregion
    }
}
