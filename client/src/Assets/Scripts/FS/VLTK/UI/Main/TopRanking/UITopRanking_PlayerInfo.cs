using TMPro;
using UnityEngine;

namespace FS.VLTK.UI.Main.TopRanking
{
    /// <summary>
    /// Thông tin người chơi trong khung đua top
    /// </summary>
    public class UITopRanking_PlayerInfo : MonoBehaviour
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
        /// Text môn phái
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Faction;

        /// <summary>
        /// Text giá trị trong xếp hạng
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Value;
        #endregion

        #region Properties
        private int _Rank = -1;
        /// <summary>
        /// Thứ hạng
        /// </summary>
        public int Rank
        {
            get
            {
                return this._Rank;
            }
            set
            {
                this._Rank = value;
                this.UIText_Rank.text = value.ToString();
            }
        }

        /// <summary>
        /// Tên
        /// </summary>
        public string Name
        {
            get
            {
                return this.UIText_Name.text;
            }
            set
            {
                this.UIText_Name.text = value;
            }
        }

        private int _FactionID = 0;
        /// <summary>
        /// ID phái
        /// </summary>
        public int FactionID
        {
            get
            {
                return this._FactionID;
            }
            set
            {
                this._FactionID = value;
                /// Tên tin phái
                this.UIText_Faction.text = KTGlobal.GetFactionName(value, out Color color);
                this.UIText_Faction.color = color;
            }
        }

        private int _Value = 0;
        /// <summary>
        /// Giá trị trong xếp hạng
        /// </summary>
        public int Value
        {
            get
            {
                return this._Value;
            }
            set
            {
                this._Value = value;
                this.UIText_Value.text = KTGlobal.GetDisplayNumber(value);
            }
        }
        #endregion
    }
}
