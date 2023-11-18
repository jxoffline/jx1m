using FS.VLTK.Utilities.UnityUI;
using Server.Data;
using System;
using TMPro;
using UnityEngine;

namespace FS.VLTK.UI.Main.GuildEx.MemberList
{
    /// <summary>
    /// Thông tin thành viên trong khung danh sách thành viên
    /// </summary>
    public class UIGuildEx_MemberList_MemberInfo : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Toggle
        /// </summary>
        [SerializeField]
        private UIToggleSprite UIToggle;

        /// <summary>
        /// Text tên người chơi
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Name;

        /// <summary>
        /// Text tên phái
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Faction;

        /// <summary>
        /// Text chức vị
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_GuildRank;

        /// <summary>
        /// Text cấp độ
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Level;

        /// <summary>
        /// Text cống hiến
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_TotalDedications;

        /// <summary>
        /// Text tài phú
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_TotalValues;

        /// <summary>
        /// Text trạng thái Online/Offline
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_OnlineState;
        #endregion

        #region Properties
        private GuildMember _Data;
        /// <summary>
        /// Thông tin thành viên
        /// </summary>
        public GuildMember Data
        {
            get
            {
                return this._Data;
            }
            set
            {
                this._Data = value;

                /// Đổ dữ liệu
                this.UIText_Name.text = value.RoleName;
                this.UIText_Level.text = value.Level.ToString();
                this.UIText_Faction.text = KTGlobal.GetFactionName(value.FactionID, out Color factionColor);
                this.UIText_Faction.color = factionColor;
                this.UIText_GuildRank.text = KTGlobal.GetGuildRankName(value.Rank);
                this.UIText_TotalDedications.text = value.GuildMoney.ToString();
                this.UIText_TotalValues.text = KTGlobal.GetDisplayNumber(value.TotalValue);
                this.UIText_OnlineState.text = value.OnlineStatus == 0 ? "<color=#f05151>Offline</color>" : "<color=#51f089>Online</color>";
            }
        }

        /// <summary>
        /// Chọn người chơi này
        /// </summary>
        public bool Active
        {
            get
            {
                return this.UIToggle.Active;
            }
            set
            {
                this.UIToggle.Active = value;
            }
        }

        /// <summary>
        /// Sự kiện chọn người chơi
        /// </summary>
        public Action Select { get; set; }
        #endregion

        #region Core MonoBehaviour
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
            this.UIToggle.OnSelected = this.Toggle_Selected;
        }

        /// <summary>
        /// Sự kiện khi Toggle được chọn
        /// </summary>
        /// <param name="isSelected"></param>
        private void Toggle_Selected(bool isSelected)
        {
            if (isSelected)
            {
                this.Select?.Invoke();
            }
        }
        #endregion
    }
}
