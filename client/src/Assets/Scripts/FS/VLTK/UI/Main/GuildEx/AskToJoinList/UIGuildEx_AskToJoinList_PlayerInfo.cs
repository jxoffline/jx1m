using FS.VLTK.Utilities.UnityUI;
using Server.Data;
using System;
using TMPro;
using UnityEngine;

namespace FS.VLTK.UI.Main.GuildEx.AskToJoinList
{
    /// <summary>
    /// Thông tin người chơi trong khung danh sách xin vào bang
    /// </summary>
    public class UIGuildEx_AskToJoinList_PlayerInfo : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Toggle
        /// </summary>
        [SerializeField]
        private UIToggleSprite UIToggle;

        /// <summary>
        /// Text tên nhân vật
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_RoleName;

        /// <summary>
        /// Text cấp độ
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_RoleLevel;

        /// <summary>
        /// Text môn phái
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_RoleFaction;

        /// <summary>
        /// Text tài phú
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_TotalValues;

        /// <summary>
        /// Text thời gian xin vào
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_RequestTime;
        #endregion

        #region Properties
        private RequestJoin _Data;
        /// <summary>
        /// Thông tin người chơi
        /// </summary>
        public RequestJoin Data
        {
            get
            {
                return this._Data;
            }
            set
            {
                this._Data = value;
                /// Đổ dữ liệu
                this.UIText_RoleName.text = value.RoleName;
                this.UIText_RoleLevel.text = value.Level.ToString();
                this.UIText_RoleFaction.text = KTGlobal.GetFactionName(value.RoleFactionID, out Color color);
                this.UIText_RoleFaction.color = color;
                this.UIText_TotalValues.text = KTGlobal.GetDisplayNumber(value.RoleValue);
                this.UIText_RequestTime.text = value.TimeRequest.ToString("HH:mm dd/MM/yyyy");
            }
        }

        /// <summary>
        /// Sự kiện chọn người chơi
        /// </summary>
        public Action Select { get; set; }

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
