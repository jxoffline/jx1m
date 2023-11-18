using System.Collections.Generic;
using FS.GameEngine.Logic;
using FS.GameEngine.Sprite;
using Server.Data;
using FS.VLTK.Logic;
using FS.VLTK;
using FS.VLTK.Entities.Config;
using FS.VLTK.Loader;
using System.Data;

namespace FS.GameEngine.Scene
{
	/// <summary>
	/// Quản lý thông tin khác
	/// </summary>
	public partial class GScene
    {
        #region Máu biến đổi của đối tượng
        /// <summary>
        /// Cập nhật lượng máu của đối tượng
        /// </summary>
        /// <param name="sprite"></param>
        private void RefreshSpriteLife(GSprite sprite)
        {
            if (sprite.ComponentCharacter != null)
            {
                sprite.ComponentCharacter.UpdateHP();
            }
            else if (sprite.ComponentMonster != null)
            {
                sprite.ComponentMonster.UpdateHP();
            }
        }

        /// <summary>
        /// Cập nhật lượng Mana của Leader
        /// </summary>
        /// <param name="sprite"></param>
        private void RefreshLeaderMagic()
        {
            if (this.Leader == null)
            {
                return;
            }

            if (this.Leader.ComponentCharacter != null)
			{
                this.Leader.ComponentCharacter.UpdateMP();
            }
        }
        #endregion

        #region Thay đổi trang bị

        /// <summary>
        /// Thay đổi trang bị
        /// </summary>
        /// <param name="sprite"></param>
        public void ChangeBodyCode(GSprite sprite)
        {
            if (sprite.SpriteType == GSpriteTypes.Leader)
            {
                KTLeaderMovingManager.StopMove();
                KTLeaderMovingManager.StopChasingTarget();
            }

            RoleData roleData = null;

            if (sprite.SpriteType == GSpriteTypes.Leader)
            {
                roleData = Global.Data.RoleData;
            }
            else
            {
                if (Global.Data.OtherRoles.ContainsKey(sprite.RoleID))
                {
                    roleData = Global.Data.OtherRoles[sprite.RoleID];
                }
            }

            /// Sói viết thêm cho KT
            if (roleData == null)
            {
                return;
            }

            GoodsData weapon = null;
            GoodsData helm = null;
            GoodsData armor = null;
            GoodsData mantle = null;
            GoodsData horse = null;
            GoodsData mask = null;

            Dictionary<VLTK.Entities.Enum.KE_EQUIP_POSITION, GoodsData> equips = KTGlobal.GetEquips(roleData);
            equips.TryGetValue(VLTK.Entities.Enum.KE_EQUIP_POSITION.emEQUIPPOS_WEAPON, out weapon);
            equips.TryGetValue(VLTK.Entities.Enum.KE_EQUIP_POSITION.emEQUIPPOS_HEAD, out helm);
            equips.TryGetValue(VLTK.Entities.Enum.KE_EQUIP_POSITION.emEQUIPPOS_BODY, out armor);
            equips.TryGetValue(VLTK.Entities.Enum.KE_EQUIP_POSITION.emEQUIPPOS_MANTLE, out mantle);
            equips.TryGetValue(VLTK.Entities.Enum.KE_EQUIP_POSITION.emEQUIPPOS_HORSE, out horse);
            equips.TryGetValue(VLTK.Entities.Enum.KE_EQUIP_POSITION.emEQUIPPOS_MASK, out mask);

            sprite.ComponentCharacter.Data.WeaponEnhanceLevel = weapon == null ? 0 : weapon.Forge_level;
            sprite.ComponentCharacter.Data.WeaponSeries = weapon == null ? 0 : weapon.Series;
            sprite.ComponentCharacter.ChangeWeapon(weapon == null ? -1 : weapon.GoodsID);
            sprite.ComponentCharacter.ChangeArmor(armor == null ? -1 : armor.GoodsID);
            sprite.ComponentCharacter.ChangeHelm(helm == null ? -1 : helm.GoodsID);
            sprite.ComponentCharacter.ChangeMantle(mantle == null ? -1 : mantle.GoodsID);
            sprite.ComponentCharacter.Data.IsRiding = roleData.IsRiding;
            sprite.ComponentCharacter.ChangeHorse(horse == null ? -1 : horse.GoodsID);

            /// Nếu có mặt nạ
            if (mask != null)
            {
                /// Thông tin vật phẩm
                if (Loader.Items.TryGetValue(mask.GoodsID, out ItemData itemData))
                {
                    /// Thiết lập mặt nạ
                    sprite.ComponentCharacter.SetMaskID(itemData.MaskResID);
                }
            }
            /// Nếu không có mặt nạ
            else
            {
                /// Thiết lập mặt nạ
                sprite.ComponentCharacter.SetMaskID("");
            }

            /// Nếu là Leader
            if (sprite == Global.Data.Leader)
            {
                /// Nếu có khung BottomBar
                if (PlayZone.Instance.UIBottomBar != null)
                {
                    PlayZone.Instance.UIBottomBar.UISkillBar.RefreshSkillIcon();
                }
            }
        }

        #endregion
    }
}
