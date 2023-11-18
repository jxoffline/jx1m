using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using Server.Data;
using FS.GameEngine.Logic;

namespace FS.VLTK.UI.Main.BrowsePlayer
{
    /// <summary>
    /// Khung POPUP thông tin người chơi
    /// </summary>
    public class UIBrowsePlayer_PlayerInfoPopup : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Button đóng khung
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Close;

        /// <summary>
        /// Image Avarta
        /// </summary>
        [SerializeField]
        private UIRoleAvarta UIImage_Avarta;

        /// <summary>
        /// Text tên
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Name;

        /// <summary>
        /// Text môn phái
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_FactionName;

        /// <summary>
        /// Text cấp độ
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Level;

        /// <summary>
        /// Text tên bang hội
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_GuildName;

        /// <summary>
        /// Button Chat mật
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_PrivateChat;

        /// <summary>
        /// Button thêm bạn
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_AddFriend;

        /// <summary>
        /// Button thêm vào danh sách đen
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_AddToBlackList;

        /// <summary>
        /// Button thêm vào danh sách kẻ thù
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIbutton_AddEnemy;

        /// <summary>
        /// Button kiểm tra vị trí
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_CheckLocation;

        /// <summary>
        /// Button xin vào nhóm
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_AskToJoinTeam;

        /// <summary>
        /// Button mời vào nhóm
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_InviteToTeam;
        #endregion

        #region Properties
        /// <summary>
        /// Sự kiện Chat mật
        /// </summary>
        public Action PrivateChat { get; set; }

        /// <summary>
        /// Sự kiện thêm bạn
        /// </summary>
        public Action AddFriend { get; set; }

        /// <summary>
        /// Sự kiện thêm vào danh sách đen
        /// </summary>
        public Action AddToBlackList { get; set; }
        
        /// <summary>
        /// Sự kiện thêm vào danh sách kẻ thù
        /// </summary>
        public Action AddEnemy { get; set; }

        /// <summary>
        /// Sự kiện kiểm tra vị trí
        /// </summary>
        public Action CheckLocation { get; set; }

        /// <summary>
        /// Sự kiện mời vào nhóm
        /// </summary>
        public Action InviteToTeam { get; set; }

        /// <summary>
        /// Sự kiện xin gia nhập nhóm
        /// </summary>
        public Action AskToJoinTeam { get; set; }

        private RoleDataMini _Data;
        /// <summary>
        /// Dữ liệu người chơi
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
                this.UIImage_Avarta.RoleID = value.RoleID;
                this.UIImage_Avarta.AvartaID = value.AvartaID;
                this.UIText_Name.text = value.RoleName;
                this.UIText_GuildName.text = value.GuildName;
                this.UIText_Level.text = value.Level.ToString();
                this.UIText_FactionName.text = KTGlobal.GetFactionName(value.FactionID, out Color factionColor);
                this.UIText_FactionName.color = factionColor;
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
            this.UIButton_Close.onClick.AddListener(this.ButtonClose_Clicked);
            this.UIButton_PrivateChat.onClick.AddListener(this.ButtonPrivateChat_Clicked);
            this.UIButton_AddFriend.onClick.AddListener(this.ButtonAddFriend_Clicked);
            this.UIbutton_AddEnemy.onClick.AddListener(this.ButtonAddEnemy_Clicked);
            this.UIButton_AddToBlackList.onClick.AddListener(this.ButtonAddToBlackList_Clicked);
            this.UIButton_CheckLocation.onClick.AddListener(this.ButtonCheckLocation_Clicked);
            this.UIButton_AskToJoinTeam.onClick.AddListener(this.ButtonAskToJoinTeam_Clicked);
            this.UIButton_InviteToTeam.onClick.AddListener(this.ButtonInviteToTeam_Clicked);

            /// Nếu bản thân chưa có nhóm
            if (Global.Data.RoleData.TeamID == -1)
            {
                /// Nếu đối phương chưa có nhóm
                if (this._Data.TeamID == -1)
                {
                    this.UIButton_AskToJoinTeam.gameObject.SetActive(false);
                    this.UIButton_InviteToTeam.gameObject.SetActive(false);
                }
                /// Nếu đối phương đã có nhóm
                else
                {
                    this.UIButton_AskToJoinTeam.gameObject.SetActive(true);
                    this.UIButton_InviteToTeam.gameObject.SetActive(false);
                }
            }
            /// Nếu bản thân đã có nhóm
            else
            {
                /// Nếu đối phương chưa có nhóm
                if (this._Data.TeamID == -1)
                {
                    this.UIButton_AskToJoinTeam.gameObject.SetActive(false);
                    this.UIButton_InviteToTeam.gameObject.SetActive(true);
                }
                /// Nếu đối phương đã có nhóm
                else
                {
                    this.UIButton_AskToJoinTeam.gameObject.SetActive(false);
                    this.UIButton_InviteToTeam.gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Sự kiện khi Button đóng khung được ấn
        /// </summary>
        private void ButtonClose_Clicked()
        {
            this.Hide();
        }

        /// <summary>
        /// Sự kiện khi Button chat mật được ấn
        /// </summary>
        private void ButtonPrivateChat_Clicked()
        {
            this.Hide();
            this.PrivateChat?.Invoke();
        }

        /// <summary>
        /// Sự kiện khi Button thêm bạn được ấn
        /// </summary>
        private void ButtonAddFriend_Clicked()
        {
            this.Hide();
            /// Nếu đã có trong danh sách bạn bè
            if (Global.Data.FriendDataList != null && Global.Data.FriendDataList.Any(x => x.OtherRoleID == this._Data.RoleID))
            {
                KTGlobal.AddNotification("Người chơi đã tồn tại trong danh sách bạn bè, kẻ thù hoặc danh sách chặn!");
                return;
            }

            this.AddFriend?.Invoke();
        }

        /// <summary>
        /// Sự kiện khi Button thêm kẻ thù được ấn
        /// </summary>
        private void ButtonAddEnemy_Clicked()
        {
            this.Hide();
            /// Nếu đã có trong danh sách bạn bè
            if (Global.Data.FriendDataList != null && Global.Data.FriendDataList.Any(x => x.OtherRoleID == this._Data.RoleID))
            {
                KTGlobal.AddNotification("Người chơi đã tồn tại trong danh sách bạn bè, kẻ thù hoặc danh sách chặn!");
                return;
            }

            this.AddEnemy?.Invoke();
        }

        /// <summary>
        /// Sự kiện khi Button thêm vào danh sách đen được ấn
        /// </summary>
        private void ButtonAddToBlackList_Clicked()
        {
            this.Hide();
            /// Nếu đã có trong danh sách bạn bè
            if (Global.Data.FriendDataList != null && Global.Data.FriendDataList.Any(x => x.OtherRoleID == this._Data.RoleID))
            {
                KTGlobal.AddNotification("Người chơi đã tồn tại trong danh sách bạn bè, kẻ thù hoặc danh sách chặn!");
                return;
            }

            this.AddToBlackList?.Invoke();
        }

        /// <summary>
        /// Sự kiện khi Button kiểm tra vị trí được ấn
        /// </summary>
        private void ButtonCheckLocation_Clicked()
        {
            this.Hide();
            this.CheckLocation?.Invoke();
        }

        /// <summary>
        /// Sự kiện khi Button mời vào nhóm được ấn
        /// </summary>
        private void ButtonInviteToTeam_Clicked()
        {
            this.Hide();
            this.InviteToTeam?.Invoke();
        }

        /// <summary>
        /// Sự kiện khi Button xin gia nhập nhóm được ấn
        /// </summary>
        private void ButtonAskToJoinTeam_Clicked()
        {
            this.Hide();
            this.AskToJoinTeam?.Invoke();
        }
        #endregion

        #region Private fields

        #endregion

        #region Public methods
        /// <summary>
        /// Hiện khung
        /// </summary>
        public void Show()
        {
            this.gameObject.SetActive(true);
        }

        /// <summary>
        /// Đóng khung
        /// </summary>
        public void Hide()
        {
            this.gameObject.SetActive(false);
        }
        #endregion
    }
}
