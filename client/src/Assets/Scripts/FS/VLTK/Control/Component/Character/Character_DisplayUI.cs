using FS.GameEngine.Logic;
using FS.VLTK.UI.CoreUI;
using UnityEngine;
using FS.VLTK.UI;
using FS.VLTK.Factory;
using FS.VLTK.UI.UICore;
using FS.VLTK.Logic.Settings;

namespace FS.VLTK.Control.Component
{
    /// <summary>
    /// Quản lý hiển thị UI
    /// </summary>
    public partial class Character : IDisplayUI
    {
        #region Define
        /// <summary>
        /// Khung trên đầu nhân vật
        /// </summary>
        [SerializeField]
        private UIRoleHeader UIHeader;

        /// <summary>
        /// Khung biểu diễn trên Minimap
        /// </summary>
        private UIMinimapReference UIMinimapReference;
        #endregion

        #region Kế thừa IDisplayUI
        /// <summary>
        /// Hiển thị UI
        /// </summary>
        public void DisplayUI()
        {
            if (this.UIMinimapReference == null)
            {
                this.UIMinimapReference = KTObjectPoolManager.Instance.Instantiate<UIMinimapReference>("Minimap - Reference");
                this.UIMinimapReference.ReferenceObject = this.gameObject;
                this.UIMinimapReference.gameObject.SetActive(true);
            }

            if (this.RefObject != null)
            {
                {
                    this.UIHeader.NameColor = this._NameColor;
                    this.UIHeader.HPBarColor = this._HPBarColor;
                    this.UIHeader.IsShowMPBar = this._ShowMPBar;
                    this.UIHeader.Name = this.RefObject.RoleName;
                    this.UIHeader.RoleID = this.RefObject.RoleID;
                }

                if (this.UIMinimapReference != null)
                {
                    this.UIMinimapReference.ShowIcon = this._ShowMinimapIcon;
                    this.UIMinimapReference.ShowName = this._ShowMinimapName;
                    this.UIMinimapReference.Name = this.RefObject.RoleName;
                    this.UIMinimapReference.NameColor = this._MinimapNameColor;
                    this.UIMinimapReference.BundleDir = KTGlobal.MinimapIconBundleDir;
                    this.UIMinimapReference.AtlasName = KTGlobal.MinimapIconAtlasName;
                    if (this.RefObject == Global.Data.Leader)
                    {
                        this.UIMinimapReference.SpriteName = KTGlobal.MinimapLeaderIconSpriteName;
                    }
                    this.UIMinimapReference.IconSize = this._MinimapIconSize;
                    this.UIMinimapReference.UpdatePosition();
                }
            }

            this.UpdateGuildTitle();
            this.UpdateTitle();
            this.UpdateSpecialTitle();
            this.UpdateHP();
            this.UpdateRoleValue();
            this.UpdateMyselfRoleTitle();
            if (Global.Data.RoleData.RoleID == this.RefObject.RoleID)
            {
                this.UpdateMP();
            }
        }

        /// <summary>
        /// Xóa UI
        /// </summary>
        public void DestroyUI()
        {
            this.UIHeader.Destroy();

            if (this.UIMinimapReference != null)
            {
                this.UIMinimapReference.Destroy();
            }
        }

        /// <summary>
        /// Cập nhật màu tên và thanh máu căn cứ trạng thái PK
        /// </summary>
        public void UpdateUIHeaderColor()
        {
            /// Nếu đối tượng là Leader
            if (this.RefObject == Global.Data.Leader)
            {
                return;
            }

            /// Nếu mục tiêu tiềm ẩn nguy hiểm
            if (KTGlobal.IsDangerous(this.RefObject))
            {
                this.UIHeader.NameColor = KTGlobal.DangerousPlayerNameColor;
                this.UIHeader.HPBarColor = KTGlobal.DangerousPlayerNameColor;
            }
            /// Nếu là kẻ địch
            else if (KTGlobal.IsEnemy(this.RefObject))
            {
                this.UIHeader.NameColor = KTGlobal.EnemyPlayerNameColor;
                this.UIHeader.HPBarColor = KTGlobal.EnemyPlayerNameColor;

                if (this.UIMinimapReference.SpriteName != KTGlobal.MinimapEnemyRoleIconSpriteName)
                {
                    this.UIMinimapReference.SpriteName = KTGlobal.MinimapEnemyRoleIconSpriteName;
                }

            }
            /// Nếu là đồng đội
            else if (KTGlobal.IsTeammate(this.RefObject))
            {
                this.UIHeader.RestoreColor();

                if (this.UIMinimapReference.SpriteName != KTGlobal.MinimapTeammateRoleIconSpriteName)
                {
                    this.UIMinimapReference.SpriteName = KTGlobal.MinimapTeammateRoleIconSpriteName;
                }
            }
            /// Nếu không phải kẻ địch
            else
            {
                this.UIHeader.RestoreColor();

                if (this.UIMinimapReference.SpriteName != KTGlobal.MinimapOtherRoleIconSpriteName)
                {
                    this.UIMinimapReference.SpriteName = KTGlobal.MinimapOtherRoleIconSpriteName;
                }
            } 
        }

        /// <summary>
        /// Hiển thị tên sạp hàng bản thân
        /// </summary>
        /// <param name="shopName"></param>
        public void ShowMyselfShopName(string shopName)
        {
            /// Nếu không có sạp hàng
            if (string.IsNullOrEmpty(shopName))
            {
                return;
            }

            this.UIHeader.StallName = shopName;
        }

        /// <summary>
        /// Ẩn sạp hàng bản thân
        /// </summary>
        public void HideMyselfShopName()
        {
            this.UIHeader.StallName = "";
        }
        #endregion

        #region Update changes
        /// <summary>
        /// Cập nhật trạng thái cưỡi
        /// </summary>
        public void UpdateRidingState()
        {
            this.UIHeader.UpdateRideState(this.RefObject.RoleData.IsRiding);
        }

        /// <summary>
        /// Cập nhật máu
        /// </summary>
        /// <param name="hp"></param>
        /// <param name="maxHP"></param>
        public void UpdateHP()
        {
            if (this.RefObject.HPMax != 0)
            {
                this.UIHeader.HPPercent = this.RefObject.HP * 100 / this.RefObject.HPMax;
            }
        }

        /// <summary>
        /// Cập nhật Mana
        /// </summary>
        /// <param name="mp"></param>
        /// <param name="maxMP"></param>
        public void UpdateMP()
        {
            if (this.UIHeader.IsShowMPBar)
            {
                if (this.RefObject.MPMax != 0)
                {
                    this.UIHeader.MPPercent = this.RefObject.MP * 100 / this.RefObject.MPMax;
                }
            }
        }

        /// <summary>
        /// Cập nhật tên đối tượng
        /// </summary>
        public void UpdateName()
        {
            this.UIHeader.Name = this.RefObject.RoleName;
        }

        /// <summary>
        /// Cập nhật danh hiệu đối tượng
        /// </summary>
        public void UpdateTitle()
        {
            this.UIHeader.Title = this.RefObject.Title;
        }
        
        /// <summary>
        /// Cập nhật danh hiệu đặc biệt
        /// </summary>
        public void UpdateSpecialTitle()
        {
            this.UIHeader.SpecialTitleID = this.RefObject.SpecialTitleID;

            /// Thêm hiệu ứng tương ứng
            KTGlobal.RefreshSpecialTitleEffect(this.RefObject);
        }

        /// <summary>
        /// Cập nhật danh hiệu bang hội đối tượng
        /// </summary>
        public void UpdateGuildTitle()
        {
            this.UIHeader.GuildTitle = this.RefObject.GuildTitle;
        }

        /// <summary>
        /// Cập nhật vinh dự tài phú
        /// </summary>
        public void UpdateRoleValue()
        {
            this.UIHeader.RoleValue = this.RefObject.RoleData.TotalValue;
        }

        /// <summary>
        /// Cập nhật danh hiệu nhân vật hiện tại của bản thân
        /// </summary>
        public void UpdateMyselfRoleTitle()
		{
            this.UIHeader.CurrentRoleTitle = this.RefObject.RoleData.SelfCurrentTitleID;
		}

        /// <summary>
        /// Hiển thị Chat nhanh trên đầu nhân vật
        /// </summary>
        /// <param name="message"></param>
        public void ShowChat(string message)
        {
            /// Nếu có thiết lập ẩn khung chat trên đầu người chơi
            if (KTSystemSetting.HidePlayerChat)
            {
                /// Bỏ qua
                return;
            }

            this.UIHeader.ChatContent = message;
        }
        #endregion
    }
}
