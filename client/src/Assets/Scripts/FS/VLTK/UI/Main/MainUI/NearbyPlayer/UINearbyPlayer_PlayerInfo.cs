using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using Server.Data;
using FS.GameEngine.Logic;
using FS.VLTK.Utilities.UnityUI;
using FS.GameEngine.Sprite;

namespace FS.VLTK.UI.Main.MainUI.NearbyPlayer
{
    /// <summary>
    /// Ô thông tin người chơi
    /// </summary>
    public class UINearbyPlayer_PlayerInfo : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Button
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton;

        /// <summary>
        /// Màu tên kẻ địch
        /// </summary>
        [SerializeField]
        private Color _EnemyNameColor;

        /// <summary>
        /// Màu tên thành viên đội
        /// </summary>
        [SerializeField]
        private Color _TeammateNameColor;

        /// <summary>
        /// Màu tên người chơi thường
        /// </summary>
        [SerializeField]
        private Color _NormalPlayerNameColor;

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
        /// Avarta người chơi
        /// </summary>
        [SerializeField]
        private UIRoleAvarta UI_Avarta;

        /// <summary>
        /// Icon đội trưởng
        /// </summary>
        [SerializeField]
        private SpriteFromAssetBundle UIImage_TeamLeaderIcon;

        /// <summary>
        /// Icon đội viên
        /// </summary>
        [SerializeField]
        private SpriteFromAssetBundle UIImage_TeamMemberIcon;

        /// <summary>
        /// Button mời vào nhóm
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_InviteToTeam;

        /// <summary>
        /// Button xin vào nhóm
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_AskToJoinTeam;

        /// <summary>
        /// Slider sinh lực người chơi
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Slider UISlider_HPBar;

        /// <summary>
        /// Text sinh lực
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_HP;
        #endregion

        #region Properties
        private RoleData _RoleData;
        /// <summary>
        /// Dữ liệu người chơi
        /// </summary>
        public RoleData RoleData
        {
            get
            {
                return this._RoleData;
            }
            set
            {
                this._RoleData = value;
                if (value == null)
                {
                    return;
                }
                /// Đối tượng tương ứng
                GSprite sprite = KTGlobal.FindSpriteByID(value.RoleID);
                /// Nếu là kẻ địch
                if (KTGlobal.IsEnemy(sprite))
                {
                    this.UIText_PlayerName.color = this._EnemyNameColor;
                }
                /// Nếu là thành viên đội
                else if (KTGlobal.IsTeammate(sprite))
                {
                    this.UIText_PlayerName.color = this._TeammateNameColor;
                }
                /// Nếu là người chơi thường
                else
                {
                    this.UIText_PlayerName.color = this._NormalPlayerNameColor;
                }
                this.UIText_PlayerName.text = value.RoleName;
                this.UIText_PlayerName.text = value.RoleName;
                this.UIText_PlayerLevel.text = value.Level.ToString();
                this.UIText_PlayerFaction.text = KTGlobal.GetFactionName(value.FactionID, out Color factionColor);
                this.UIText_PlayerFaction.color = factionColor;
                this.UI_Avarta.AvartaID = value.RolePic;
                /// Cập nhật sinh lực
                this.UpdateHP(value.CurrentHP, value.MaxHP);

                /// Ẩn icon đội
                this.UIImage_TeamLeaderIcon.gameObject.SetActive(false);
                this.UIImage_TeamMemberIcon.gameObject.SetActive(false);
                /// Ẩn Button
                this.UIButton_InviteToTeam.gameObject.SetActive(false);
                this.UIButton_AskToJoinTeam.gameObject.SetActive(false);
                /// Nếu không có nhóm
                if (value.TeamID == -1)
                {
                    /// Nếu bản thân có nhóm
                    if (Global.Data.RoleData.TeamID != -1)
                    {
                        /// Hiện Button mời vào nhóm
                        this.UIButton_InviteToTeam.gameObject.SetActive(true);
                    }
                }
                /// Nếu có nhóm
                else
                {
                    /// Nếu là đội trưởng
                    if (value.TeamLeaderRoleID == value.RoleID)
                    {
                        this.UIImage_TeamLeaderIcon.gameObject.SetActive(true);
                    }
                    /// Nếu là đội viên
                    else
                    {
                        this.UIImage_TeamMemberIcon.gameObject.SetActive(true);
                    } 
                    
                    /// Nếu bản thân không có nhóm
                    if (Global.Data.RoleData.TeamID == -1)
                    {
                        /// Hiện Button xin vào nhóm
                        this.UIButton_AskToJoinTeam.gameObject.SetActive(true);
                    }
                }
            }
        }

        /// <summary>
        /// Sự kiện khi người chơi được chọn
        /// </summary>
        public Action Click { get; set; }

        /// <summary>
        /// Sự kiện mời vào nhóm
        /// </summary>
        public Action InviteToTeam { get; set; }

        /// <summary>
        /// Sự kiện xin gia nhập nhóm
        /// </summary>
        public Action AskToJoinTeam { get; set; }

        /// <summary>
        /// Màu tên đối tượng
        /// </summary>
        public Color NameColor
        {
            get
            {
                return this.UIText_PlayerName.color;
            }
            set
            {
                this.UIText_PlayerName.color = value;
            }
        }

        /// <summary>
        /// Có đang hiển thị không
        /// </summary>
        public bool Visible
        {
            get
            {
                return this.gameObject.activeSelf;
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

        /// <summary>
        /// Hàm này gọi khi đối tượng được kích hoạt
        /// </summary>
        private void OnEnable()
        {
            this.UIText_PlayerName.fontSize = 26;
            this.UIText_PlayerLevel.fontSize = 26;
            this.UIText_PlayerFaction.fontSize = 26;
        }
        #endregion

        #region Code UI
        /// <summary>
        /// Khởi tạo ban đầu
        /// </summary>
        private void InitPrefabs()
        {
            this.UIButton.onClick.AddListener(this.Button_Clicked);
            this.UIButton_InviteToTeam.onClick.AddListener(this.ButtonInviteToTeam_Clicked);
            this.UIButton_AskToJoinTeam.onClick.AddListener(this.ButtonAskToJoinTeam_Clicked);

            this.UIText_PlayerName.autoSizeTextContainer = false;
            this.UIText_PlayerLevel.autoSizeTextContainer = false;
            this.UIText_PlayerFaction.autoSizeTextContainer = false;

            this.UIText_PlayerName.fontSize = 26;
            this.UIText_PlayerLevel.fontSize = 26;
            this.UIText_PlayerFaction.fontSize = 26;
        }

        /// <summary>
        /// Sự kiện khi Button được click
        /// </summary>
        private void Button_Clicked()
        {
            this.Click?.Invoke();
        }

        /// <summary>
        /// Sự kiện khi Button mời vào nhóm được ấn
        /// </summary>
        private void ButtonInviteToTeam_Clicked()
        {
            this.InviteToTeam?.Invoke();
        }

        /// <summary>
        /// Sự kiện khi Button xin gia nhập nhóm được ấn
        /// </summary>
        private void ButtonAskToJoinTeam_Clicked()
        {
            this.AskToJoinTeam?.Invoke();
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Cập nhật sinh lực
        /// </summary>
        /// <param name="currentHP"></param>
        /// <param name="maxHP"></param>
        public void UpdateHP(int currentHP, int maxHP)
        {
            /// % sinh lực
            float percent = currentHP / (float) maxHP;
            /// Giá trị Slider
            this.UISlider_HPBar.value = percent;
            /// Text
            this.UIText_HP.text = string.Format("{0}/{1}", currentHP, maxHP);
        }
        #endregion
    }
}
