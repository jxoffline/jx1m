using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static FS.VLTK.Entities.Enum;

namespace FS.VLTK.UI.Main.MainUI
{
    /// <summary>
    /// Khung các Button chức năng phía dưới
    /// </summary>
    public class UIControlButtons : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Nút mở khung nhân vật
        /// </summary>
        [SerializeField]
        private UIHintButton UIButton_RoleAttributes;

        /// <summary>
        /// Nút mở khung túi đồ
        /// </summary>
        [SerializeField]
        private UIHintButton UIButton_Bag;

        /// <summary>
        /// Nút mở khung kỹ năng
        /// </summary>
        [SerializeField]
        private UIHintButton UIButton_SkillTree;

        /// <summary>
        /// Nút mở khung nhiệm vụ
        /// </summary>
        [SerializeField]
        private UIHintButton UIButton_QuestManager;

        /// <summary>
        /// Nút mở khung hảo hữu
        /// </summary>
        [SerializeField]
        private UIHintButton UIButton_FriendsFrame;

        /// <summary>
        /// Nút mở khung bang hội
        /// </summary>
        [SerializeField]
        private UIHintButton UIButton_GuildManager;

        /// <summary>
        /// Nút mở khung thiết lập hệ thống
        /// </summary>
        [SerializeField]
        private UIHintButton UIButton_Setting;

        /// <summary>
        /// Nút mở khung Lệnh GM
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_GMCommand;

        /// <summary>
        /// Button kỹ năng sống
        /// </summary>
        [SerializeField]
        private UIHintButton UIButton_LifeSkill;

        /// <summary>
        /// Button tìm người chơi
        /// </summary>
        [SerializeField]
        private UIHintButton UIButton_BrowsePlayer;

        /// <summary>
        /// Button mở hộp thư
        /// </summary>
        [SerializeField]
        private UIHintButton UIButton_OpenMailBox;

        /// <summary>
        /// Button pet
        /// </summary>
        [SerializeField]
        private UIHintButton UIButton_Pet;
        #endregion

        #region Properties
        /// <summary>
        /// Sự kiện mở khung thông tin nhân vật
        /// </summary>
        public Action OpenRoleAttributes { get; set; }

        /// <summary>
        /// Sự kiện mở khung túi đồ
        /// </summary>
        public Action OpenBag { get; set; }

        /// <summary>
        /// Sự kiện mở khung kỹ năng
        /// </summary>
        public Action OpenSkillTree { get; set; }

        /// <summary>
        /// Sự kiện mở khung nhiệm vụ
        /// </summary>
        public Action OpenQuestManager { get; set; }

        /// <summary>
        /// Sự kiện mở khung hảo hữu
        /// </summary>
        public Action OpenFriendsFrame { get; set; }

        /// <summary>
        /// Sự kiện mở khung bang hội
        /// </summary>
        public Action OpenGuildManager { get; set; }

        /// <summary>
        /// Sự kiện mở khung thiết lập hệ thống
        /// </summary>
        public Action OpenSystemSetting { get; set; }

        /// <summary>
        /// Sự kiện mở khung lệnh GM
        /// </summary>
        public Action OpenGMCommand { get; set; }

        /// <summary>
        /// Sự kiện khi nút kỹ năng sống được ấn
        /// </summary>
        public Action OpenLifeSkill { get; set; }

        /// <summary>
        /// Sự kiện khi nút tìm người chơi được ấn
        /// </summary>
        public Action OpenBrowsePlayer { get; set; }

        /// <summary>
        /// Sự kiện mở hộp thư
        /// </summary>
        public Action OpenMailBox { get; set; }

        /// <summary>
        /// Sự kiện mở khung pet
        /// </summary>
        public Action OpenPet { get; set; } 

        /// <summary>
        /// Hiển thị Button nhập lệnh GM
        /// </summary>
        public bool ShowButtonGMCommand
        {
            get
            {
                return this.UIButton_GMCommand.gameObject.activeSelf;
            }
            set
            {
                this.UIButton_GMCommand.gameObject.SetActive(value);
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
            this.UIButton_RoleAttributes.Click = this.ButtonOpenRoleAttributes_Clicked;
            this.UIButton_Bag.Click = this.ButtonOpenBag_Clicked;
            this.UIButton_SkillTree.Click = this.ButtonOpenSkillTree_Clicked;
            this.UIButton_QuestManager.Click = this.ButtonQuestManager_Clicked;
            this.UIButton_FriendsFrame.Click = this.ButtonFriendsFrame_Clicked;
            this.UIButton_GuildManager.Click = this.ButtonOpenGuildManager_Clicked;
            this.UIButton_Setting.Click = this.ButtonOpenSystemSetting_Clicked;
            this.UIButton_GMCommand.onClick.AddListener(this.ButtonOpenGMCommand_Clicked);
            this.UIButton_LifeSkill.Click = this.ButtonLifeSkill_Clicked;
            this.UIButton_BrowsePlayer.Click = this.ButtonBrowsePlayer_Clicked;
            this.UIButton_OpenMailBox.Click = this.ButtonOpenMailBox_Clicked;
            this.UIButton_Pet.Click = this.ButtonOpenPet_Clicked;
        }

        /// <summary>
        /// Sự kiện khi Button mở khung thông tin nhân vật được ấn
        /// </summary>
        private void ButtonOpenRoleAttributes_Clicked()
        {
            this.OpenRoleAttributes?.Invoke();
        }

        /// <summary>
        /// Sự kiện khi Button mở khung túi đồ được ấn
        /// </summary>
        private void ButtonOpenBag_Clicked()
        {
            this.OpenBag?.Invoke();
        }

        /// <summary>
        /// Sự kiện khi Button mở khung kỹ năng được ấn
        /// </summary>
        private void ButtonOpenSkillTree_Clicked()
        {
            this.OpenSkillTree?.Invoke();
        }

        /// <summary>
        /// Sự kiện khi Button mở khung nhiệm vụ được ấn
        /// </summary>
        private void ButtonQuestManager_Clicked()
        {
            this.OpenQuestManager?.Invoke();
        }

        /// <summary>
        /// Sự kiện khi Button mở khung hảo hữu được ấn
        /// </summary>
        private void ButtonFriendsFrame_Clicked()
        {
            this.OpenFriendsFrame?.Invoke();
        }

        /// <summary>
        /// Sự kiện khi Button mở khung bang hội được ấn
        /// </summary>
        private void ButtonOpenGuildManager_Clicked()
        {
            this.OpenGuildManager?.Invoke();
        }

        /// <summary>
        /// Sự kiện khi Button mở khung thiết lập hệ thống được ấn
        /// </summary>
        private void ButtonOpenSystemSetting_Clicked()
        {
            this.OpenSystemSetting?.Invoke();
        }

        /// <summary>
        /// Sự kiện khi Button mở khung lệnh GM được ấn
        /// </summary>
        private void ButtonOpenGMCommand_Clicked()
        {
            this.OpenGMCommand?.Invoke();
        }

        /// <summary>
        /// Sự kiện khi Button kỹ năng sống được ấn
        /// </summary>
        private void ButtonLifeSkill_Clicked()
        {
            this.OpenLifeSkill?.Invoke();
        }

        /// <summary>
        /// Sự kiện khi Button tìm người chơi được ấn
        /// </summary>
        private void ButtonBrowsePlayer_Clicked()
        {
            this.OpenBrowsePlayer?.Invoke();
        }

        /// <summary>
        /// Sự kiện khi Button mở hộp thư được ấn
        /// </summary>
        private void ButtonOpenMailBox_Clicked()
        {
            this.OpenMailBox?.Invoke();
        }

        /// <summary>
        /// Sự kiện khi Button mở khung pet được ấn
        /// </summary>
        private void ButtonOpenPet_Clicked()
        {
            this.OpenPet?.Invoke();
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Thay đổi trạng thái của Button
        /// </summary>
        /// <param name="buttonType"></param>
        /// <param name="action"></param>
        public void ChangeButtonState(FunctionButtonType buttonType, FunctionButtonAction action)
        {
            /// Đổi trạng thái của Button tương ứng
            void ChangeState(UIHintButton button)
            {
                switch (action)
                {
                    case FunctionButtonAction.Show:
                    {
                        button.Visible = true;
                        button.Enable = true;
                        button.Hint = false;
                        break;
                    }
                    case FunctionButtonAction.Hide:
                    {
                        button.Visible = false;
                        button.Enable = false;
                        button.Hint = false;
                        break;
                    }
                    case FunctionButtonAction.Enable:
                    {
                        button.Visible = true;
                        button.Enable = true;
                        button.Hint = false;
                        break;
                    }
                    case FunctionButtonAction.Disable:
                    {
                        button.Visible = true;
                        button.Enable = false;
                        button.Hint = false;
                        break;
                    }
                    case FunctionButtonAction.Hint:
                    {
                        button.Visible = true;
                        button.Enable = true;
                        button.Hint = true;
                        break;
                    }
                }
            }

            switch (buttonType)
            {
                case FunctionButtonType.OpenSystemSetting:
                {
                    ChangeState(this.UIButton_Setting);
                    break;
                }
                case FunctionButtonType.OpenGuildBox:
                {
                    ChangeState(this.UIButton_GuildManager);
                    break;
                }
                case FunctionButtonType.OpenFriendBox:
                {
                    ChangeState(this.UIButton_FriendsFrame);
                    break;
                }
                case FunctionButtonType.OpenTaskBox:
                {
                    ChangeState(this.UIButton_QuestManager);
                    break;
                }
                case FunctionButtonType.OpenBrowsePlayer:
                {
                    ChangeState(this.UIButton_BrowsePlayer);
                    break;
                }
                case FunctionButtonType.OpenMailBox:
                {
                    ChangeState(this.UIButton_OpenMailBox);
                    break;
                }
                case FunctionButtonType.OpenRoleInfo:
                {
                    ChangeState(this.UIButton_RoleAttributes);
                    break;
                }
                case FunctionButtonType.OpenBag:
                {
                    ChangeState(this.UIButton_Bag);
                    break;
                }
                case FunctionButtonType.OpenSkill:
                {
                    ChangeState(this.UIButton_SkillTree);
                    break;
                }
                case FunctionButtonType.OpenLifeSkill:
                {
                    ChangeState(this.UIButton_LifeSkill);
                    break;
                }
            }
        }
        #endregion
    }
}

