using FS.GameEngine.Logic;
using Server.Data;
using System;
using TMPro;
using UnityEngine;

namespace FS.VLTK.UI.Main.MainUI.InvitedToTeamList
{
    /// <summary>
    /// Thông tin người chơi trong khung danh sách mời vào nhóm
    /// </summary>
    public class UIInvitedToTeamList_Frame_PlayerInfo : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Avarta người chơi
        /// </summary>
        [SerializeField]
        private UIRoleAvarta UI_PlayerAvarta;

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
        /// Text môn phái người chơi
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_PlayerFaction;

        /// <summary>
        /// Button đồng ý vào nhóm
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Agree;

        /// <summary>
        /// Button từ chối vào nhóm
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Reject;
        #endregion

        #region Properties
        private RoleDataMini _Data;
        /// <summary>
        /// Dữ liệu nhân vật
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
                /// Nếu tồn tại
                if (value != null)
                {
                    /// Đổ dữ liệu
                    this.UI_PlayerAvarta.AvartaID = value.AvartaID;
                    this.UIText_PlayerName.text = value.RoleName;
                    this.UIText_PlayerLevel.text = value.Level.ToString();
                    this.UIText_PlayerFaction.text = KTGlobal.GetFactionName(value.FactionID, out Color color);
                    this.UIText_PlayerFaction.color = color;
                }
            }
        }

        /// <summary>
        /// Sự kiện đồng ý vào nhóm
        /// </summary>
        public Action Agree { get; set; }

        /// <summary>
        /// Sự kiện từ chối vào nhóm
        /// </summary>
        public Action Reject { get; set; }
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
            this.UIButton_Agree.onClick.AddListener(this.ButtonAgree_Clicked);
            this.UIButton_Reject.onClick.AddListener(this.ButtonReject_Clicked);
        }

        /// <summary>
        /// Sự kiện đồng ý vào nhóm người chơi
        /// </summary>
        private void ButtonAgree_Clicked()
        {
            /// Nếu bản thân đã có nhóm
            if (Global.Data.RoleData.TeamID != -1)
            {
                /// Bỏ qua
                return;
            }

            /// Sự kiện
            this.Agree?.Invoke();
        }

        /// <summary>
        /// Sự kiện từ chối vào nhóm người chơi
        /// </summary>
        private void ButtonReject_Clicked()
        {
            /// Nếu bản thân đã có nhóm
            if (Global.Data.RoleData.TeamID != -1)
            {
                /// Bỏ qua
                return;
            }

            /// Sự kiện
            this.Reject?.Invoke();
        }
        #endregion
    }
}
