using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using FS.VLTK.Utilities.UnityUI;
using Server.Data;
using FS.GameEngine.Logic;

namespace FS.VLTK.UI.Main.Friend
{
    /// <summary>
    /// Thông tin người chơi trong danh sách chờ thêm bạn
    /// </summary>
    public class UIFriendBox_AskToBeFriendFrame_PlayerInfo : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Toggle chọn người chơi
        /// </summary>
        [SerializeField]
        private UIToggleSprite UIToggle_SelectFriend;

        /// <summary>
        /// Text tên người chơi
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_PlayerName;

        /// <summary>
        /// Text cấp độ người chơi
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_PlayerLevel;

        /// <summary>
        /// Text tên môn phái người chơi
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_PlayerFactionName;
        #endregion

        #region Private fields

        #endregion

        #region Properties
        private RoleDataMini _Data;
        /// <summary>
        /// Thông tin người chơi
        /// </summary>
        public RoleDataMini Data
        {
            get
            {
                return this._Data;
            }
            set
            {
                this._Data = value;
                /// Nếu có dữ liệu
                if (value != null)
                {
                    this.UIText_PlayerName.text = value.RoleName;
                    this.UIText_PlayerLevel.text = value.Level.ToString();
                    this.UIText_PlayerFactionName.text = KTGlobal.GetFactionName(value.FactionID, out Color color);
                    this.UIText_PlayerFactionName.color = color;
                }
                else
                {
                    this.UIText_PlayerName.text = "";
                    this.UIText_PlayerLevel.text = "";
                    this.UIText_PlayerFactionName.text = "";
                    this.UIText_PlayerFactionName.color = Color.white;
                }
            }
        }

        /// <summary>
        /// Sự kiện khi người chơi được chọn
        /// </summary>
        public Action PlayerSelected { get; set; }
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
            this.UIToggle_SelectFriend.OnSelected = this.TogglePlayer_Selected;
        }

        /// <summary>
        /// Sự kiện khi Toggle người chơi được chọn
        /// </summary>
        /// <param name="isSelected"></param>
        private void TogglePlayer_Selected(bool isSelected)
        {
            if (isSelected)
            {
                this.PlayerSelected?.Invoke();
            }
        }
        #endregion

        #region Private methods

        #endregion

        #region Public methods

        #endregion
    }
}
