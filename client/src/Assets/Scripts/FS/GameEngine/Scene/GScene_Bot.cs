using FS.GameEngine.Logic;
using FS.GameEngine.Network;
using FS.GameEngine.Sprite;
using FS.GameFramework.Logic;
using FS.VLTK;
using FS.VLTK.Factory;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FS.GameEngine.Scene
{
    /// <summary>
    /// Quản lý BOT
    /// </summary>
    public partial class GScene
    {
        #region Hệ thống BOT
        /// <summary>
        /// Danh sách BOT
        /// </summary>
        private List<RoleData> waitToBeAddedBot = new List<RoleData>();

        /// <summary>
        /// Tải BOT
        /// </summary>
        /// <param name="rd"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="direction"></param>
        public void ToLoadBot(RoleData rd)
        {
            this.waitToBeAddedBot.Add(rd);
        }

        /// <summary>
        /// Thêm BOT vào bản đồ
        /// </summary>
        private void AddListBot()
        {
            if (this.waitToBeAddedBot.Count <= 0)
            {
                return;
            }

            RoleData rd = this.waitToBeAddedBot[0];
            this.waitToBeAddedBot.RemoveAt(0);
            this.AddListBot(rd);
        }

        /// <summary>
        /// Tải danh sách BOT
        /// </summary>
        /// <param name="rd"></param>
        private void AddListBot(RoleData rd)
        {
            if (rd == null)
			{
                return;
			}

            /// Nếu đối tượng không tồn tại thì bỏ qua
            if (!Global.Data.Bots.TryGetValue(rd.RoleID, out _))
            {
                return;
            }

            /// Tên đối tượng
            string name = string.Format("Bot_{0}", rd.RoleID);

            /// Đối tượng cũ
            GSprite sprite = this.FindSprite(name);
            /// Nếu đối tượng có tồn tại
            if (sprite != null)
            {
                /// Xóa đối tượng
                KTGlobal.RemoveObject(sprite, true);
            }

            /// Tải xuống đối tượng
            sprite = this.LoadBot(rd);
            /// Thực hiện gửi gói tin tải xuống hoàn tất
            GameInstance.Game.SpriteLoadAlready(sprite.RoleID);
        }

        /// <summary>
        /// Tải xuống BOT
        /// </summary>
        public GSprite LoadBot(RoleData rd)
        {
            string name = string.Format("Bot_{0}", rd.RoleID);
            /// Tìm đối tượng tương ứng
            GSprite sprite = KTGlobal.FindSpriteByID(rd.RoleID);
            /// Nếu đối tượng có tồn tại
            if (sprite != null)
            {
                /// Thực hiện xóa đối tượng
                sprite.Destroy();
            }

            /// Tạo mới đối tượng
            sprite = new GSprite();
            sprite.BaseID = rd.RoleID;
            sprite.SpriteType = GSpriteTypes.Bot;
            /// Tải đối tượng
			this.LoadSprite(
                sprite,
                rd.RoleID,
                name,
                rd,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                (VLTK.Entities.Enum.Direction) rd.RoleDirection,
                rd.PosX,
                rd.PosY
            );

            /// Bắt đầu
            sprite.Start();

            /// Tìm đối tượng cũ
            GameObject oldObject = KTObjectPoolManager.Instance.FindSpawn(x => x.name == name);
            /// Nếu tồn tại
            if (oldObject != null)
            {
                /// Trả lại Pool
                KTObjectPoolManager.Instance.ReturnToPool(oldObject);
            }

            //VLTK.Control.Component.Character role = Object2DFactory.MakeOtherRole();
            VLTK.Control.Component.Character role = KTObjectPoolManager.Instance.Instantiate<VLTK.Control.Component.Character>("Bot");
            role.name = name;

            /// Gắn đối tượng tham chiếu
            role.RefObject = sprite;
            /// TODO check state with Leader
            ColorUtility.TryParseHtmlString("#2effd9", out Color nameColor);
            role.NameColor = nameColor;
            ColorUtility.TryParseHtmlString("#2eff5f", out Color hpBarColor);
            role.HPBarColor = hpBarColor;

            /// Không hiển thị thanh nội lực
            role.ShowMPBar = false;

            /// TODO check type of OtherRole vs Leader
            ColorUtility.TryParseHtmlString("#2ebdff", out Color minimapNameColor);
            role.MinimapNameColor = minimapNameColor;
            role.ShowMinimapIcon = true;
            role.ShowMinimapName = false;
            role.MinimapIconSize = new Vector2(10, 10);

            GoodsData weapon = null;
            GoodsData helm = null;
            GoodsData armor = null;
            GoodsData mantle = null;
            GoodsData horse = null;

            Dictionary<VLTK.Entities.Enum.KE_EQUIP_POSITION, GoodsData> equips = KTGlobal.GetEquips(rd);
            equips.TryGetValue(VLTK.Entities.Enum.KE_EQUIP_POSITION.emEQUIPPOS_WEAPON, out weapon);
            equips.TryGetValue(VLTK.Entities.Enum.KE_EQUIP_POSITION.emEQUIPPOS_HEAD, out helm);
            equips.TryGetValue(VLTK.Entities.Enum.KE_EQUIP_POSITION.emEQUIPPOS_BODY, out armor);
            equips.TryGetValue(VLTK.Entities.Enum.KE_EQUIP_POSITION.emEQUIPPOS_MANTLE, out mantle);
            equips.TryGetValue(VLTK.Entities.Enum.KE_EQUIP_POSITION.emEQUIPPOS_HORSE, out horse);

            /// Đổ dữ liệu vào
            role.Data = new RoleDataMini()
            {
                RoleSex = rd.RoleSex,

                IsRiding = rd.IsRiding,
                ArmorID = armor == null ? -1 : armor.GoodsID,
                HelmID = helm == null ? -1 : helm.GoodsID,
                WeaponID = weapon == null ? -1 : weapon.GoodsID,
                WeaponEnhanceLevel = weapon == null ? 0 : weapon.Forge_level,
                WeaponSeries = weapon == null ? 0 : weapon.Series,
                MantleID = mantle == null ? -1 : mantle.GoodsID,
                HorseID = horse == null ? -1 : horse.GoodsID,
            };
            role.UpdateRoleData();

            GameObject role2D = role.gameObject;
            sprite.Role2D = role2D;
            role2D.transform.localPosition = new Vector2((float) rd.PosX, (float) rd.PosY);

            /// Tải xuống các hiệu ứng
            if (rd.BufferDataList != null)
            {
                foreach (BufferData buff in rd.BufferDataList)
                {
                    if (VLTK.Loader.Loader.Skills.TryGetValue(buff.BufferID, out VLTK.Entities.Config.SkillDataEx skillData))
                    {
                        sprite.AddBuff(skillData.StateEffectID, buff.BufferSecs == -1 ? -1 : buff.BufferSecs - (KTGlobal.GetServerTime() - buff.StartTime));
                    }
                }
            }
            /// End

            /// Làm mới giá trị máu của đối tượng
            this.RefreshSpriteLife(sprite);

            return sprite;
        }

        /// <summary>
        /// Xóa BOT tương ứng khỏi hệ thống
        /// </summary>
        /// <param name="mapCode"></param>
        /// <param name="roleID"></param>
        /// <returns></returns>
        public bool DelBot(int roleID)
        {
            GSprite bot = this.FindSprite(roleID);
            /// Xóa khỏi danh sách theo dõi của Leader
            Global.Data.Bots.Remove(roleID);

            if (null != bot)
            {
                /// Xóa khỏi danh sách quản lý
                KTGlobal.RemoveObject(bot, true);
            }

            return true;
        }
        #endregion
    }
}
