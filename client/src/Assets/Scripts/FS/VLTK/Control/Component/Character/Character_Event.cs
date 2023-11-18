using FS.GameEngine.Logic;
using FS.VLTK.Logic;
using FS.VLTK.Utilities.UnityComponent;
using UnityEngine;

namespace FS.VLTK.Control.Component
{
    /// <summary>
    /// Quản lý các sự kiện xảy ra với đối tượng
    /// </summary>
    public partial class Character : IEvent
    {
        /// <summary>
        /// Hàm này gọi đến ngay khi đối tượng được tạo ra
        /// </summary>
        private void InitEvents()
        {
            if (this.Model.transform.parent.GetComponent<ClickableCollider2D>() != null)
            {
                this.Model.transform.parent.GetComponent<ClickableCollider2D>().OnClick = () => {
                    this.OnClick();
                };
            }
        }

        /// <summary>
        /// Sự kiện khi đối tượng được chọn
        /// </summary>
        public void OnClick()
        {
            /// Nếu không phải Leader
            if (Global.Data.RoleData.RoleID != this.RefObject.RoleID)
            {
                /// Nếu là Bot
                if (this.RefObject.SpriteType == GSpriteTypes.Bot)
                {
                    /// Bỏ qua
                    return;
                }

                /// Nếu là Bot bán hàng
                if (this.RefObject.SpriteType == GSpriteTypes.StallBot)
                {
                    /// Thực thi sự kiện mở sạp
                    Global.Data.GameScene.PlayerShopClick(this.RefObject);
                    /// Bỏ qua
                    return;
                }
                /// Nếu là người chơi khác và có sạp hàng
                else if (!string.IsNullOrEmpty(this.RefObject.RoleData.StallName))
                {
                    /// Thực thi sự kiện mở sạp
                    Global.Data.GameScene.PlayerShopClick(this.RefObject);
                }

                SkillManager.SelectedTarget = this.RefObject;
                KTAutoFightManager.Instance.ChangeAutoFightTarget(this.RefObject);
                Global.Data.GameScene.OtherRoleClick(this.RefObject);
            }
        }

        /// <summary>
        /// Sự kiện khi vị trí của đối tượng thay đổi
        /// </summary>
        public void OnPositionChanged()
        {
            if (this.UIMinimapReference != null)
            {
                this.UIMinimapReference.UpdatePosition();
            }
        }
    }
}
