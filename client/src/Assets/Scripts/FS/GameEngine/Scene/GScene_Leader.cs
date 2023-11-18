using System.Collections.Generic;
using UnityEngine;
using FS.GameEngine.Logic;
using FS.GameEngine.Network;
using FS.GameEngine.Sprite;
using Server.Data;
using FS.VLTK.Factory;
using FS.GameFramework.Logic;
using FS.VLTK.Utilities.UnityComponent;
using FS.VLTK;
using FS.VLTK.Entities.Config;
using FS.VLTK.Loader;
using static FS.VLTK.Entities.Enum;

namespace FS.GameEngine.Scene
{
	/// <summary>
	/// Quản lý đối tượng Leader
	/// </summary>
	public partial class GScene
    {
        #region Khởi tạo ban đầu
        /// <summary>
        /// Tải xuống đối tượng Leader
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="direction"></param>
        /// <param name="newLifeDeco"></param>
        private void LoadLeader(int x, int y, int direction)
        {
            /// Xóa Leader cũ
            this.Leader?.Destroy();

            /// Hướng quay
            int oldDirection = direction;

            /// Khởi tạo đối tượng
            this.Leader = new GSprite();
            this.Leader.BaseID = Global.Data.RoleData.RoleID;
            this.Leader.SpriteType = GSpriteTypes.Leader;

            /// Tải đối tượng
            this.LoadSprite(
                this.Leader,
                Global.Data.RoleData.RoleID,
                "Leader",
                Global.Data.RoleData,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                (VLTK.Entities.Enum.Direction) oldDirection,
                x,
                y
            );

            /// Bắt đầu đối tượng
            this.Leader.Start();

            /// Tìm đối tượng cũ
            GameObject oldObject = KTObjectPoolManager.Instance.FindSpawn(x => x.name == "Leader");
            /// Nếu tồn tại
            if (oldObject != null)
            {
                /// Trả lại Pool
                KTObjectPoolManager.Instance.ReturnToPool(oldObject);
            }

            /// Khởi tạo Res 2D
            //VLTK.Control.Component.Character role = Object2DFactory.MakeLeader();
            VLTK.Control.Component.Character role = KTObjectPoolManager.Instance.Instantiate<VLTK.Control.Component.Character>("Leader");
            role.name = "Leader";

            GameObject role2D = role.gameObject;
            this.Leader.Role2D = role2D;
            role2D.transform.localPosition = new Vector2(x, y);
            Global.MainCamera.transform.localPosition = new Vector3(x, y, -10);

            /// Gắn các sự kiện Camera theo dõi
            Global.MainCamera.GetComponent<SmoothCamera2D>().Target = role2D.transform;

            /// Gắn đối tượng tham chiếu
            role.RefObject = this.Leader;
            ColorUtility.TryParseHtmlString("#2effb2", out Color nameColor);
            role.NameColor = nameColor;
            ColorUtility.TryParseHtmlString("#2eff5f", out Color hpBarColor);
            role.HPBarColor = hpBarColor;
            role.ShowMPBar = true;
            ColorUtility.TryParseHtmlString("#5fff2e", out Color minimapNameColỏ);
            role.MinimapNameColor = minimapNameColỏ;
            role.ShowMinimapIcon = true;
            role.ShowMinimapName = false;
            role.MinimapIconSize = new Vector2(10, 10);

            GoodsData weapon = null;
            GoodsData helm = null;
            GoodsData armor = null;
            GoodsData mantle = null;
            GoodsData horse = null;
            GoodsData mask = null;

            Dictionary<VLTK.Entities.Enum.KE_EQUIP_POSITION, GoodsData> equips = KTGlobal.GetEquips(Global.Data.RoleData);
            equips.TryGetValue(VLTK.Entities.Enum.KE_EQUIP_POSITION.emEQUIPPOS_WEAPON, out weapon);
            equips.TryGetValue(VLTK.Entities.Enum.KE_EQUIP_POSITION.emEQUIPPOS_HEAD, out helm);
            equips.TryGetValue(VLTK.Entities.Enum.KE_EQUIP_POSITION.emEQUIPPOS_BODY, out armor);
            equips.TryGetValue(VLTK.Entities.Enum.KE_EQUIP_POSITION.emEQUIPPOS_MANTLE, out mantle);
            equips.TryGetValue(VLTK.Entities.Enum.KE_EQUIP_POSITION.emEQUIPPOS_HORSE, out horse);
            equips.TryGetValue(VLTK.Entities.Enum.KE_EQUIP_POSITION.emEQUIPPOS_MASK, out mask);

            /// Đổ dữ liệu vào
            role.Data = new RoleDataMini()
            {
                RoleSex = Global.Data.RoleData.RoleSex,

                IsRiding = Global.Data.RoleData.IsRiding,
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

            /// Thiết lập hướng
            role.Direction = this.Leader.Direction;
            /// Thực hiện động tác đứng
            role.Stand();

            /// Thực hiện Recover lại các hiệu ứng có trên người
            if (Global.Data.RoleData.BufferDataList != null)
            {
                foreach (BufferData buff in Global.Data.RoleData.BufferDataList)
                {
                    if (VLTK.Loader.Loader.Skills.TryGetValue(buff.BufferID, out VLTK.Entities.Config.SkillDataEx skillData))
                    {
                        this.Leader.AddBuff(skillData.StateEffectID, buff.BufferSecs == -1 ? -1 : buff.BufferSecs - (KTGlobal.GetServerTime() - buff.StartTime));
                    }
                }

                if (PlayZone.Instance != null && PlayZone.Instance.UIRolePart != null)
                {
                    PlayZone.Instance.UIRolePart.UIBuffList.RefreshDataList();
                }
            }
            else
            {
                Global.Data.RoleData.BufferDataList = new List<BufferData>();
            }
            /// End

            /// Cập nhật hiển thị thanh máu và khí
            this.RefreshSpriteLife(this.Leader);
            this.RefreshLeaderMagic();
        }

        #endregion

        #region Syns nhân vật đang Online lần trước với GS
        /// <summary>
        /// Thời gian gói tin syns nhân vật đang Online lần trước
        /// </summary>
        private long LastClientHearTicks = 0;

        /// <summary>
        /// Truyền tin tới Server syns nhân vật đang Online
        /// </summary>
        private void SendClientHeart()
        {
            if (!Global.Data.PlayGame)
            {
                return;
            }

            long nowTicks = KTGlobal.GetCurrentTimeMilis();
            if (nowTicks - LastClientHearTicks >= (60 * 1000))
            {
                LastClientHearTicks = nowTicks;
                GameInstance.Game.SpriteHeart();
            }
        }

        #endregion

        #region Tương tác với đối tượng Leader
        /// <summary>
        /// Trả ra đối tượng Leader
        /// </summary>
        public GSprite GetLeader()
        {
            return this.Leader;
        }
        #endregion
    }
}
