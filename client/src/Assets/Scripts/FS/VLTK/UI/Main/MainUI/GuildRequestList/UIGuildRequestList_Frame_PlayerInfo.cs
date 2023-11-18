using FS.GameEngine.Logic;
using Server.Data;
using System;
using TMPro;
using UnityEngine;

namespace FS.VLTK.UI.Main.MainUI.GuildRequestList
{
    /// <summary>
    /// Thông tin người chơi trong khung danh sách mời hoặc xin vào bang
    /// </summary>
    public class UIGuildRequestList_Frame_PlayerInfo : MonoBehaviour
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
        /// Text bang hội người chơi
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_PlayerGuildName;

        /// <summary>
        /// Button đồng ý
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Agree;

        /// <summary>
        /// Button từ chối
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
                    if (string.IsNullOrEmpty(value.GuildName))
                    {
                        this.UIText_PlayerGuildName.text = "";
                    }
                    else
                    {
                        this.UIText_PlayerGuildName.text = string.Format("Bang hội: <color=yellow>{0}</color>", value.GuildName);
                    }
                }
            }
        }

        /// <summary>
        /// Sự kiện đồng ý
        /// </summary>
        public Action Agree { get; set; }

        /// <summary>
        /// Sự kiện từ chối
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
        /// Sự kiện đồng ý người chơi
        /// </summary>
        private void ButtonAgree_Clicked()
        {
            /// Sự kiện
            this.Agree?.Invoke();
        }

        /// <summary>
        /// Sự kiện từ chối người chơi
        /// </summary>
        private void ButtonReject_Clicked()
        {
            /// Sự kiện
            this.Reject?.Invoke();
        }
        #endregion
    }
}
