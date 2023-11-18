using System.Collections.Generic;
using UnityEngine;
using FS.GameEngine.Logic;
using FS.GameEngine.Network;
using FS.GameEngine.Sprite;
using Server.Data;
using FS.VLTK.Factory;
using FS.VLTK;
using FS.VLTK.Loader;
using FS.VLTK.Entities.Config;
using static FS.VLTK.Entities.Enum;

namespace FS.GameEngine.Scene
{
    /// <summary>
    /// Quản lý đối tượng người chơi khác
    /// </summary>
    public partial class GScene
    {
        #region Khởi tạo
        /// <summary>
        /// Danh sách người chơi khác, con này sẽ cache từ từ load từng thằng một đảm bảo không bị giật
        /// </summary>
        private List<RoleData> waitToBeAddedRole = new List<RoleData>();

        /// <summary>
        /// Tải đối tượng người chơi vào bản đồ
        /// </summary>
        /// <param name="roleData"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="direction"></param>
        public void ToLoadOtherRole(RoleData roleData, int x, int y, int direction)
        {
            roleData.PosX = x;
            roleData.PosY = y;
            roleData.RoleDirection = direction;

            this.waitToBeAddedRole.Add(roleData);
        }

        /// <summary>
        /// Thêm đối tượng vào danh sách
        /// <para>Con này gọi liên tục ở hàm Update</para>
        /// </summary>
        private void AddListRole()
        {
            if (waitToBeAddedRole.Count <= 0)
            {
                return;
            }

            RoleData rd = this.waitToBeAddedRole[0];
            this.waitToBeAddedRole.RemoveAt(0);
            this.AddListRole(rd);
        }

        /// <summary>
        /// Thêm đối tượng vào bản đồ
        /// </summary>
        private void AddListRole(RoleData rd)
        {
            /// Nếu không có dữ liệu
            if (rd == null)
            {
                //KTGlobal.AddNotification("Player DATA NULL, return => " + otherRoleItem.RoleData.RoleName);
                return;
            }

            /// Nếu đối tượng không tồn tại thì bỏ qua
            if (!Global.Data.OtherRoles.TryGetValue(rd.RoleID, out _))
			{
                //KTGlobal.AddNotification("Player not exist, return => " + otherRoleItem.RoleData.RoleName);
                return;
			}

            /// Tìm đối tượng cũ
            GSprite oldObject = KTGlobal.FindSpriteByID(rd.RoleID);
            /// Nếu tồn tại
            if (oldObject != null)
			{
                /// Xóa
                oldObject.Destroy();
			}

            /// Tải xuống đối tượng
            GSprite sprite = this.LoadRole(rd);
            /// Thực hiện gửi gói tin tải xuống hoàn tất
            GameInstance.Game.SpriteLoadAlready(sprite.RoleID);
        }

        /// <summary>
        /// Tải xuống đối tượng
        /// </summary>
        private GSprite LoadRole(RoleData roleData)
        {
            //KTDebug.LogError("Begin load PLAYER => " + roleData.RoleName);

            string name =  string.Format("Role_{0}", roleData.RoleID);

            GSprite sprite = new GSprite();


            /// Tạo mới đối tượng
            sprite.BaseID = roleData.RoleID;
			sprite.SpriteType = GSpriteTypes.Other;

            /// Tải đối tượng
			this.LoadSprite(
				sprite, 
				roleData.RoleID,
				name,
				roleData,
                null,
				null,
				null,
				null,
                null,
                null,
                null,
                (Direction) roleData.RoleDirection, 
				roleData.PosX,
                roleData.PosY
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
                //KTGlobal.AddNotification("Duplicate PLAYER obj => " + roleData.RoleName);
            }

            //VLTK.Control.Component.Character role = Object2DFactory.MakeOtherRole();
            VLTK.Control.Component.Character role = KTObjectPoolManager.Instance.Instantiate<VLTK.Control.Component.Character>("OtherRole");
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
            GoodsData mask = null;

            Dictionary<VLTK.Entities.Enum.KE_EQUIP_POSITION, GoodsData> equips = KTGlobal.GetEquips(roleData);
            equips.TryGetValue(VLTK.Entities.Enum.KE_EQUIP_POSITION.emEQUIPPOS_WEAPON, out weapon);
            equips.TryGetValue(VLTK.Entities.Enum.KE_EQUIP_POSITION.emEQUIPPOS_HEAD, out helm);
            equips.TryGetValue(VLTK.Entities.Enum.KE_EQUIP_POSITION.emEQUIPPOS_BODY, out armor);
            equips.TryGetValue(VLTK.Entities.Enum.KE_EQUIP_POSITION.emEQUIPPOS_MANTLE, out mantle);
            equips.TryGetValue(VLTK.Entities.Enum.KE_EQUIP_POSITION.emEQUIPPOS_HORSE, out horse);
            equips.TryGetValue(VLTK.Entities.Enum.KE_EQUIP_POSITION.emEQUIPPOS_MASK, out mask);

            /// Đổ dữ liệu vào
            role.Data = new RoleDataMini()
            {
                RoleSex = roleData.RoleSex,

                IsRiding = roleData.IsRiding,
                ArmorID = armor == null ? -1 : armor.GoodsID,
                HelmID = helm == null ? -1 : helm.GoodsID,
                WeaponID = weapon == null ? -1 : weapon.GoodsID,
                WeaponEnhanceLevel = weapon == null ? 0 : weapon.Forge_level,
                WeaponSeries = weapon == null ? 0 : weapon.Series,
                MantleID = mantle == null ? -1 : mantle.GoodsID,
                HorseID = horse == null ? -1 : horse.GoodsID,
            };
            role.UpdateRoleData();
            /// Nếu có mặt nạ
            if (mask != null)
            {
                /// Thông tin vật phẩm
                if (Loader.Items.TryGetValue(mask.GoodsID, out ItemData itemData))
                {
                    /// Thiết lập mặt nạ
                    role.SetMaskID(itemData.MaskResID);
                }
            }
            /// Nếu không có mặt nạ
            else
            {
                role.SetMaskID("");
            }

            GameObject role2D = role.gameObject;
            sprite.Role2D = role2D;
            role2D.transform.localPosition = new Vector2(roleData.PosX, roleData.PosY);

            /// Tải xuống các hiệu ứng
            if (roleData.BufferDataList != null)
            {
                foreach (BufferData buff in roleData.BufferDataList)
                {
                    if (VLTK.Loader.Loader.Skills.TryGetValue(buff.BufferID, out VLTK.Entities.Config.SkillDataEx skillData))
                    {
                        sprite.AddBuff(skillData.StateEffectID, buff.BufferSecs == -1 ? -1 : buff.BufferSecs - (KTGlobal.GetServerTime() - buff.StartTime));
                    }
                }
            }
            /// End
            
            /// Thực hiện động tác đứng 
            sprite.DoStand(true);

            /// Làm mới giá trị máu của đối tượng
            this.RefreshSpriteLife(sprite);

            //KTDebug.LogError("Load PLAYER done => " + roleData.RoleName);

            return sprite;
        }
        #endregion
    }
}
