using FS.GameEngine.Logic;
using FS.GameEngine.Network;
using FS.GameEngine.Sprite;
using FS.VLTK;
using FS.VLTK.Entities.Config;
using FS.VLTK.Factory;
using FS.VLTK.Loader;
using Server.Data;
using System.Collections.Generic;
using UnityEngine;
using static FS.VLTK.Entities.Enum;

namespace FS.GameEngine.Scene
{
    /// <summary>
    /// Quản lý BOT
    /// </summary>
    public partial class GScene
    {
        #region Hệ thống Bot bán hàng
        /// <summary>
        /// Danh sách Bot bán hàng
        /// </summary>
        private List<StallBotData> waitToBeAddedStallBot = new List<StallBotData>();

        /// <summary>
        /// Tải Bot bán hàng
        /// </summary>
        /// <param name="data"></param>
        public void ToLoadStallBot(StallBotData data)
        {
            this.waitToBeAddedStallBot.Add(data);
        }

        /// <summary>
        /// Thêm Bot bán hàng vào bản đồ
        /// </summary>
        private void AddListStallBot()
        {
            if (this.waitToBeAddedStallBot.Count <= 0)
            {
                return;
            }

            StallBotData data = this.waitToBeAddedStallBot[0];
            this.waitToBeAddedStallBot.RemoveAt(0);
            this.AddListStallBot(data);
        }

        /// <summary>
        /// Tải danh sách Bot bán hàng
        /// </summary>
        /// <param name="data"></param>
        private void AddListStallBot(StallBotData data)
        {
            if (data == null)
            {
                return;
            }

            /// Nếu đối tượng không tồn tại thì bỏ qua
            if (!Global.Data.StallBots.TryGetValue(data.RoleID, out _))
            {
                return;
            }

            /// Tên đối tượng
            string name = string.Format("StallBot_{0}", data.RoleID);

            /// Đối tượng cũ
            GSprite sprite = this.FindSprite(name);
            /// Nếu đối tượng có tồn tại
            if (sprite != null)
            {
                /// Xóa đối tượng
                KTGlobal.RemoveObject(sprite, true);
            }

            /// Tải xuống đối tượng
            sprite = this.LoadStallBot(data);
            /// Thực hiện gửi gói tin tải xuống hoàn tất
            GameInstance.Game.SpriteLoadAlready(sprite.RoleID);
        }

        /// <summary>
        /// Tải xuống BOT
        /// </summary>
        public GSprite LoadStallBot(StallBotData data)
        {
            string name = string.Format("StallBot_{0}", data.RoleID);
            /// Tìm đối tượng tương ứng
            GSprite sprite = KTGlobal.FindSpriteByID(data.RoleID);
            /// Nếu đối tượng có tồn tại
            if (sprite != null)
            {
                /// Thực hiện xóa đối tượng
                sprite.Destroy();
            }

            /// Tạo RoleData
            RoleData rd = new RoleData()
            {
                RoleID = data.RoleID,
                RoleSex = data.Sex,
                RoleName = string.Format("<color=#c458ee>Ủy thác</color> - {0}", data.OwnerRoleName),

                IsRiding = false,

                PosX = data.PosX,
                PosY = data.PosY,
                RoleDirection = (int) VLTK.Entities.Enum.Direction.DOWN,
                CurrentHP = 1,
                MaxHP = 1,
                GoodsDataList = new List<GoodsData>(),
            };

            static GoodsData GetItemGD(int itemID)
            {
                if (Loader.Items.TryGetValue(itemID, out ItemData itemData))
                {
                    return new GoodsData()
                    {
                        GoodsID = itemData.ItemID,
                        GCount = 1,
                        Forge_level = 0,
                    };
                }
                else
                {
                    return null;
                }
            }
            GoodsData armor = GetItemGD(data.ArmorID);
            if (armor != null)
            {
                armor.Using = (int) KE_EQUIP_POSITION.emEQUIPPOS_BODY;
                rd.GoodsDataList.Add(armor);
            }
            GoodsData helm = GetItemGD(data.HelmID);
            if (helm != null)
            {
                helm.Using = (int) KE_EQUIP_POSITION.emEQUIPPOS_HEAD;
                rd.GoodsDataList.Add(helm);
            }
            GoodsData mantle = GetItemGD(data.MantleID);
            if (mantle != null)
            {
                mantle.Using = (int) KE_EQUIP_POSITION.emEQUIPPOS_MANTLE;
                rd.GoodsDataList.Add(mantle);
            }

            /// Tạo mới đối tượng
            sprite = new GSprite();
            sprite.BaseID = data.RoleID;
            sprite.SpriteType = GSpriteTypes.StallBot;
            /// Tải đối tượng
			this.LoadSprite(
                sprite,
                data.RoleID,
                name,
                rd,
                null,
                null,
                null,
                null,
                null,
                null,
                data,
                VLTK.Entities.Enum.Direction.DOWN,
                data.PosX,
                data.PosY
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
            VLTK.Control.Component.Character role = KTObjectPoolManager.Instance.Instantiate<VLTK.Control.Component.Character>("StallBot");
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

            /// Đổ dữ liệu vào
            role.Data = new RoleDataMini()
            {
                RoleSex = data.Sex,

                IsRiding = false,
                ArmorID = armor == null ? -1 : armor.GoodsID,
                HelmID = helm == null ? -1 : helm.GoodsID,
                MantleID = mantle == null ? -1 : mantle.GoodsID,
                HP = 1,
                MaxHP = 1,
            };
            role.UpdateRoleData();

            GameObject role2D = role.gameObject;
            sprite.Role2D = role2D;
            role2D.transform.localPosition = new Vector2(data.PosX, data.PosY);

            return sprite;
        }

        /// <summary>
        /// Xóa Bot bán hàng tương ứng khỏi hệ thống
        /// </summary>
        /// <param name="roleID"></param>
        /// <returns></returns>
        public bool DelStallBot(int roleID)
        {
            /// Nếu không tồn tại trong danh sách
            if (!Global.Data.StallBots.ContainsKey(roleID))
            {
                /// Bỏ qua
                return false;
            }

            /// Bot tương ứng
            GSprite bot = this.FindSprite(roleID);
            /// Xóa khỏi danh sách theo dõi của Leader
            Global.Data.StallBots.Remove(roleID);

            /// Nếu tìm thấy
            if (bot != null)
            {
                /// Xóa khỏi danh sách quản lý
                KTGlobal.RemoveObject(bot, true);
            }

            /// OK
            return true;
        }

        /// <summary>
        /// Xóa Bot bán hàng tương ứng khỏi hệ thống đồng thời thực thi hiệu ứng trước khi xóa
        /// </summary>
        /// <param name="roleID"></param>
        public bool DelStallBotWithAnimation(int roleID)
        {
            /// Nếu không tồn tại trong danh sách
            if (!Global.Data.StallBots.ContainsKey(roleID))
            {
                /// Bỏ qua
                return false;
            }

            /// Bot tương ứng
            GSprite bot = this.FindSprite(roleID);
            /// Xóa khỏi danh sách theo dõi của Leader
            Global.Data.StallBots.Remove(roleID);

            /// Nếu tìm thấy
            if (bot != null)
            {
                /// Hủy sạp
                bot.ComponentCharacter.HideMyselfShopName();
                /// Thực thi động tác đứng dậy 3s xong mới xóa
                bot.DoStand(3f, () =>
                {
                    /// Xóa khỏi danh sách quản lý
                    KTGlobal.RemoveObject(bot, true);
                });
            }

            /// OK
            return true;
        }
        #endregion
    }
}
