using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using Server.Data;
using FS.GameEngine.Logic;
using FS.VLTK.Utilities.UnityUI;

namespace FS.VLTK.UI.Main.BrowsePlayer
{
    /// <summary>
    /// Thông tin người chơi trong khung tìm kiếm người chơi
    /// </summary>
    public class UIBrowsePlayer_PlayerInfo : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Toggle
        /// </summary>
        [SerializeField]
        private UIToggleSprite UIToggle;

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
        /// Text trạng thái
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_State;
        #endregion

        #region Private fields

        #endregion

        #region Properties
        /// <summary>
        /// Sự kiện khi người chơi được chọn
        /// </summary>
        public Action Selected { get; set; }

        private RoleDataMini _Data;
        /// <summary>
        /// Dữ liệu bảng xếp hạng Tống Kim
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

                this.UIText_Name.text = value.RoleName;
                this.UIText_Level.text = value.Level.ToString();
                this.UIText_FactionName.text = KTGlobal.GetFactionName(value.FactionID, out Color factionColor);
                this.UIText_FactionName.color = factionColor;
                this.UIText_State.text = string.Format("<color=green>{0}</color>", "ONLINE");
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
                this.Selected?.Invoke();
            }
        }
        #endregion

        #region Private methods

        #endregion

        #region Public methods

        #endregion
    }
}
