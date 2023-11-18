using FS.GameEngine.Logic;
using FS.GameEngine.Sprite;
using FS.VLTK.Control.Component;
using FS.VLTK.Entities.Config;
using Server.Data;
using System.Collections.Generic;
using UnityEngine;
using static FS.VLTK.Entities.Enum;

namespace FS.VLTK
{
    /// <summary>
    /// Các hàm toàn cục dùng trong Kiếm Thế
    /// </summary>
    public static partial class KTGlobal
    {
        #region Thăng cấp

        /// <summary>
        /// ID hiệu ứng thăng cấp nhân vật
        /// </summary>
        public const int RoleLevelUpEffect = 1086;

        /// <summary>
        /// ID hiệu ứng thăng cấp danh vọng nhân vật
        /// </summary>
        public const int RoleAchievementUpEffect = 1088;

        /// <summary>
        /// Thực thi hiệu ứng nhân vật thăng cấp
        /// </summary>
        public static void PlayRoleLevelUpEffect(GSprite sprite)
        {
            /// Toác
            if (sprite.ComponentCharacter == null)
            {
                return;
            }

            sprite.ComponentCharacter.AddEffect(KTGlobal.RoleLevelUpEffect, EffectType.CastEffect);
        }

        /// <summary>
        /// Thực thi hiệu ứng pet thăng cấp
        /// </summary>
        public static void PlayPetLevelUpEffect()
        {
            /// Nếu không có Pet đang tham chiến
            if (Global.Data.RoleData.CurrentPetID == -1)
            {
                /// Bỏ qua
                return;
            }
            /// Thông tin pet đang tham chiến
            GSprite pet = KTGlobal.FindSpriteByID(Global.Data.RoleData.CurrentPetID + (int) ObjectBaseID.Pet);
            /// Nếu không tìm thấy
            if (pet == null)
            {
                /// Bỏ qua
                return;
            }
            /// Thực thi hiệu ứng
            pet.ComponentMonster.AddEffect(KTGlobal.RoleLevelUpEffect, EffectType.CastEffect);
        }

        /// <summary>
        /// Thực thi hiệu ứng nhân vật thăng cấp danh vọng
        /// </summary>
        public static void PlayRoleAchievementUpEffect()
        {
            Global.Data.Leader.ComponentCharacter.AddEffect(KTGlobal.RoleAchievementUpEffect, EffectType.CastEffect);
        }

        #endregion Thăng cấp

        #region Tốc chạy và tốc đánh

        #region Tốc chạy

        /// <summary>
        /// Chuyển tốc độ di chuyển sang dạng lưới Pixel
        /// </summary>
        /// <param name="moveSpeed"></param>
        /// <returns></returns>
        public static int MoveSpeedToPixel(int moveSpeed)
        {
            return moveSpeed * 15;
        }

        /// <summary>
        /// Thời gian thực hiện động tác chạy hoặc đi bộ tối thiểu
        /// </summary>
        public const float MinWalkRunActionDuration = 0.1f;

        /// <summary>
        /// Thời gian thực hiện động tác chạy hoặc đi bộ tối đa
        /// </summary>
        public const float MaxWalkRunActionDuration = 1.0f;


        /// <summary>
        /// Thời gian thực hiện động tác chạy khi cưỡi ngựa
        /// </summary>
        public const float MaxWalkRunActionRiderDuration = 0.8f;

        /// <summary>
        /// Tốc chạy tối thiểu
        /// </summary>
        public const int MinWalkRunSpeed = 0;

        /// <summary>
        /// Tốc chạy tối đa
        /// </summary>
        public const int MaxWalkRunSpeed = 30;

        /// <summary>
        /// Chuyển tốc độ chạy sang thời gian thực hiện động tác chạy
        /// </summary>
        /// <param name="moveSpeed"></param>
        /// <returns></returns>
        public static float MoveSpeedToFrameDuration(int moveSpeed)
        {
            /// Nếu tốc đánh nhỏ hơn tốc tối thiểu
            if (moveSpeed < KTGlobal.MinWalkRunSpeed)
            {
                moveSpeed = KTGlobal.MinWalkRunSpeed;
            }
            /// Nếu tốc đánh vượt quá tốc tối đa
            if (moveSpeed > KTGlobal.MaxWalkRunSpeed)
            {
                moveSpeed = KTGlobal.MaxWalkRunSpeed;
            }

            /// Tỷ lệ % so với tốc đánh tối đa
            float percent = moveSpeed / (float)KTGlobal.MaxWalkRunSpeed;

            bool IsRider = Global.Data.RoleData.IsRiding;

            float animationDuration = 0;

            if (IsRider)
            {
                animationDuration = KTGlobal.MinWalkRunActionDuration + (KTGlobal.MaxWalkRunActionRiderDuration - KTGlobal.MinWalkRunActionDuration) * (1f - percent);
            }
            else
            {
                animationDuration = KTGlobal.MinWalkRunActionDuration + (KTGlobal.MaxWalkRunActionDuration - KTGlobal.MinWalkRunActionDuration) * (1f - percent);
            }
            /// Thời gian thực hiện động tác chạy hoặc đi bộ

            if (animationDuration < KTGlobal.MinWalkRunActionDuration)
            {
                animationDuration = KTGlobal.MinWalkRunActionDuration;
            }

            /// Trả về kết quả
            return animationDuration;
        }

        #endregion Tốc chạy

        #region Tốc đánh
        /// <summary>
        /// ID kỹ năng dùng lần trước
        /// </summary>
        public static int LastUseSkillID { get; set; }

        /// <summary>
        /// Thời điểm dùng kỹ năng lần trước
        /// </summary>
        public static long LastUseSkillTick { get; set; }

        /// <summary>
        /// Thời điểm dùng kỹ năng không ảnh hưởng tốc đánh lần trước
        /// </summary>
        public static long LastUseSkillNoAffectAtkSpeedTick { get; set; }

        /// <summary>
        /// Đã kết thúc thực thi động tác xuất chiêu chưa
        /// </summary>
        public static bool FinishedUseSkillAction
        {
            get
            {
                /// Nếu đang đợi dùng kỹ năng thì bỏ qua
                if (SkillManager.IsWaitingToUseSkill)
                {
                    return false;
                }

                /// Nếu vừa dùng kỹ năng không ảnh hưởng bởi tốc đánh
                if (KTGlobal.GetCurrentTimeMilis() - KTGlobal.LastUseSkillNoAffectAtkSpeedTick < 100)
                {
                    return false;
                }

                /// Đối tượng Leader
                GSprite leader = Global.Data.Leader;

                /// Tốc độ xuất chiêu hệ ngoại công hiện tại
                int attackSpeed = leader.AttackSpeed;
                /// Tốc độ xuất chiêu hệ nội công hiện tại
                int castSpeed = leader.CastSpeed;

                /// Tổng thời gian
                float frameDuration = 0f;
                /// Kỹ năng lần trước
                if (Loader.Loader.Skills.TryGetValue(KTGlobal.LastUseSkillID, out SkillDataEx skillData))
                {
                    /// Kỹ năng nội hay ngoại
                    bool isPhysical = skillData.IsPhysical;
                    frameDuration = KTGlobal.AttackSpeedToFrameDuration(isPhysical ? attackSpeed : castSpeed);
                }

                /// Trả ra kết quả
                return KTGlobal.GetCurrentTimeMilis() - KTGlobal.LastUseSkillTick >= frameDuration * 1000;
            }
        }

        /// <summary>
        /// Thời gian thực hiện động tác xuất chiêu tối thiểu
        /// </summary>
        public const float MinAttackActionDuration = 0.2f;

        /// <summary>
        /// Thời gian thực hiện động tác xuất chiêu tối đa
        /// </summary>
        public const float MaxAttackActionDuration = 0.8f;

        /// <summary>
        /// Thời gian cộng thêm giãn cách giữa các lần ra chiêu
        /// </summary>
        public const float AttackSpeedAdditionDuration = 0.1f;

        /// <summary>
        /// Tốc đánh tối thiểu
        /// </summary>
        public const int MinAttackSpeed = 0;

        /// <summary>
        /// Tốc đánh tối đa
        /// </summary>
        public const int MaxAttackSpeed = 100;

        /// <summary>
        /// Chuyển tốc độ đánh sang thời gian thực hiện động tác xuất chiêu
        /// </summary>
        /// <param name="attackSpeed"></param>
        /// <returns></returns>
        public static float AttackSpeedToFrameDuration(int attackSpeed)
        {
            /// Nếu tốc đánh nhỏ hơn tốc tối thiểu
            if (attackSpeed < KTGlobal.MinAttackSpeed)
            {
                attackSpeed = KTGlobal.MinAttackSpeed;
            }
            /// Nếu tốc đánh vượt quá tốc tối đa
            if (attackSpeed > KTGlobal.MaxAttackSpeed)
            {
                attackSpeed = KTGlobal.MaxAttackSpeed;
            }

            /// Tỷ lệ % so với tốc đánh tối đa
            float percent = attackSpeed / (float)KTGlobal.MaxAttackSpeed;

            /// Thời gian thực hiện động tác xuất chiêu
            float animationDuration = KTGlobal.MinAttackActionDuration + (KTGlobal.MaxAttackActionDuration - KTGlobal.MinAttackActionDuration) * (1f - percent);

            /// Trả về kết quả
            return animationDuration;
        }

        #endregion Tốc đánh

        #endregion Tốc chạy và tốc đánh

        #region Môn phái và ngũ hành

        /// <summary>
        /// Trả về giá trị tên môn phái theo ID
        /// </summary>
        /// <param name="factionID"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        public static string GetFactionName(int factionID, out Color color)
        {
            Color _color = Color.white;
            if (VLTK.Loader.Loader.Factions.TryGetValue(factionID, out VLTK.Entities.Config.FactionXML faction))
            {
                string factionName = faction.Name;
                switch (faction.Elemental)
                {
                    case VLTK.Entities.Enum.Elemental.METAL:
                        ColorUtility.TryParseHtmlString("#ffff1a", out _color);
                        break;

                    case VLTK.Entities.Enum.Elemental.WOOD:
                        ColorUtility.TryParseHtmlString("#00ff00", out _color);
                        break;

                    case VLTK.Entities.Enum.Elemental.EARTH:
                        ColorUtility.TryParseHtmlString("#c8bcbc", out _color);
                        break;

                    case VLTK.Entities.Enum.Elemental.WATER:
                        ColorUtility.TryParseHtmlString("#8abdff", out _color);
                        break;

                    case VLTK.Entities.Enum.Elemental.FIRE:
                        ColorUtility.TryParseHtmlString("#ffac47", out _color);
                        break;
                }
                color = _color;
                return factionName;
            }
            color = _color;
            return "";
        }

        /// <summary>
        /// Trả về giá trị ngũ hành môn phái
        /// </summary>
        /// <param name="factionID"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        public static string GetFactionElementString(int factionID, out Color color)
        {
            Color _color = Color.white;
            if (VLTK.Loader.Loader.Factions.TryGetValue(factionID, out VLTK.Entities.Config.FactionXML faction))
            {
                if (VLTK.Loader.Loader.Elements.TryGetValue(faction.Elemental, out VLTK.Entities.Object.ElementData elementData))
                {
                    string elementName = elementData.Name;
                    switch (faction.Elemental)
                    {
                        case VLTK.Entities.Enum.Elemental.METAL:
                            ColorUtility.TryParseHtmlString("#ffff1a", out _color);
                            break;

                        case VLTK.Entities.Enum.Elemental.WOOD:
                            ColorUtility.TryParseHtmlString("#00ff00", out _color);
                            break;

                        case VLTK.Entities.Enum.Elemental.EARTH:
                            ColorUtility.TryParseHtmlString("#c8bcbc", out _color);
                            break;

                        case VLTK.Entities.Enum.Elemental.WATER:
                            ColorUtility.TryParseHtmlString("#8abdff", out _color);
                            break;

                        case VLTK.Entities.Enum.Elemental.FIRE:
                            ColorUtility.TryParseHtmlString("#ffac47", out _color);
                            break;
                    }
                    color = _color;
                    return elementName;
                }
            }
            color = _color;
            return "Vô";
        }

        /// <summary>
        /// Trả về giá trị ngũ hành môn phái
        /// </summary>
        /// <param name="factionID"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        public static VLTK.Entities.Enum.Elemental GetFactionElement(int factionID)
        {
            if (VLTK.Loader.Loader.Factions.TryGetValue(factionID, out VLTK.Entities.Config.FactionXML faction))
            {
                return faction.Elemental;
            }
            return VLTK.Entities.Enum.Elemental.NONE;
        }

        public static bool g_IsConquer(int FactionSource, int FactionDesc)
        {
            int Source = 0;
            int Dest = 0;

            if (VLTK.Loader.Loader.Factions.TryGetValue(FactionSource, out VLTK.Entities.Config.FactionXML faction))
            {
                Source = (int)faction.Elemental;
            }
            else
            {
                Source = (int)VLTK.Entities.Enum.Elemental.NONE;
            }

            if (VLTK.Loader.Loader.Factions.TryGetValue(FactionDesc, out VLTK.Entities.Config.FactionXML factiondest))
            {
                Dest = (int)factiondest.Elemental;
            }
            else
            {
                Dest = (int)VLTK.Entities.Enum.Elemental.NONE;
            }

            if (Loader.Loader.g_IsConquer(Source, Dest))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Trả về tên trạng thái ngũ hành
        /// </summary>
        /// <param name="elemental"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        public static string GetElementString(VLTK.Entities.Enum.Elemental elemental, out Color color)
        {
            Color _color = Color.white;
            if (VLTK.Loader.Loader.Elements.TryGetValue(elemental, out VLTK.Entities.Object.ElementData elementData))
            {
                string elementName = elementData.Name;
                switch (elemental)
                {
                    case VLTK.Entities.Enum.Elemental.METAL:
                        ColorUtility.TryParseHtmlString("#ffff1a", out _color);
                        break;

                    case VLTK.Entities.Enum.Elemental.WOOD:
                        ColorUtility.TryParseHtmlString("#00ff00", out _color);
                        break;

                    case VLTK.Entities.Enum.Elemental.EARTH:
                        ColorUtility.TryParseHtmlString("#c8bcbc", out _color);
                        break;

                    case VLTK.Entities.Enum.Elemental.WATER:
                        ColorUtility.TryParseHtmlString("#8abdff", out _color);
                        break;

                    case VLTK.Entities.Enum.Elemental.FIRE:
                        ColorUtility.TryParseHtmlString("#ffac47", out _color);
                        break;
                }
                color = _color;
                return elementName;
            }
            color = _color;
            return "Vô";
        }

        #endregion Môn phái và ngũ hành

        #region Quan hàm

        /// <summary>
        /// Thiết lập hiệu ứng quan hàm cho đối tượng
        /// </summary>
        /// <param name="sprite"></param>
        public static void RefreshOfficeRankEffect(GSprite sprite)
        {
            /// Nếu không phải người chơi
            if (sprite == null || sprite.ComponentCharacter == null)
            {
                return;
            }

            /// Quan hàm tương ứng
            int officeRank = sprite.RoleData.OfficeRank;

            /// Thông tin quan hàm tương ứng
            OfficeTitleXML currentOfficeTitleInfo = null;

            /// Duyệt danh sách lấy toàn bộ các danh hiệu cũ xóa đi
            foreach (OfficeTitleXML officeTitle in Loader.Loader.OfficeTitles)
            {
                /// Nếu là quan hàm hiện tại
                if (officeTitle.ID == officeRank)
                {
                    /// Cập nhật thông tin quan hàm tương ứng
                    currentOfficeTitleInfo = officeTitle;
                }
                /// Xóa Buff
                sprite.RemoveBuff(officeTitle.EffectID);
            }

            /// Nếu tồn tại quan hàm tương ứng
            if (currentOfficeTitleInfo != null)
            {
                /// Thêm Buff
                sprite.AddBuff(currentOfficeTitleInfo.EffectID, -1);
            }
        }

        #endregion Quan hàm

        #region Danh hiệu đặc biệt

        /// <summary>
        /// Thiết lập hiệu ứng danh hiệu đặc biệt cho đối tượng
        /// </summary>
        /// <param name="sprite"></param>
        public static void RefreshSpecialTitleEffect(GSprite sprite)
        {
            /// Nếu không phải người chơi
            if (sprite == null || sprite.ComponentCharacter == null)
            {
                return;
            }

            /// Thông tin
            if (!Loader.Loader.SpecialTitles.TryGetValue(sprite.SpecialTitleID, out KSpecialTitleXML data))
            {
                return;
            }

            /// Duyệt danh sách danh hiệu đặc biệt
            foreach (KSpecialTitleXML otherData in Loader.Loader.SpecialTitles.Values)
            {
                /// Xóa Buff cũ
                sprite.RemoveBuff(otherData.EffectID);
            }

            /// Không có effect
            if (data.EffectID == -1)
            {
                /// Bỏ qua
                return;
            }
            
            /// Thêm Buff mới
            sprite.AddBuff(data.EffectID, -1);
        }

        #endregion Quan hàm

        #region RoleDataMini, DecoBotData => RoleData
        /// <summary>
        /// Chuyển đổi RoleDataMini sang RoleData
        /// </summary>
        /// <param name="roleDataMini"></param>
        /// <returns></returns>
        public static RoleData RoleDataMiniToRoleData(RoleDataMini roleDataMini)
        {
            /// Toác
            if (roleDataMini == null)
            {
                return null;
            }

            RoleData roleData = new RoleData()
            {
                ZoneID = roleDataMini.ZoneID,
                RoleID = roleDataMini.RoleID,
                RoleName = roleDataMini.RoleName,
                RoleSex = roleDataMini.RoleSex,
                FactionID = roleDataMini.FactionID,
                SubID = roleDataMini.RouteID,
                Level = roleDataMini.Level,
                PosX = roleDataMini.PosX,
                PosY = roleDataMini.PosY,
                RoleDirection = roleDataMini.CurrentDir,
                CurrentHP = roleDataMini.HP,
                MaxHP = roleDataMini.MaxHP,
                MoveSpeed = roleDataMini.MoveSpeed,
                AttackSpeed = roleDataMini.AttackSpeed,
                CastSpeed = roleDataMini.CastSpeed,
                BufferDataList = roleDataMini.BufferDataList,
                MapCode = roleDataMini.MapCode,
                RolePic = roleDataMini.AvartaID,

                TeamID = roleDataMini.TeamID,
                TeamLeaderRoleID = roleDataMini.TeamLeaderID,

                GoodsDataList = new List<GoodsData>(),
                IsRiding = roleDataMini.IsRiding,

                PKMode = roleDataMini.PKMode,
                PKValue = roleDataMini.PKValue,
                Camp = roleDataMini.Camp,

                StallName = roleDataMini.StallName,
                Title = roleDataMini.Title,
                GuildTitle = roleDataMini.GuildTitle,
                SpecialTitleID = roleDataMini.SpecialTitle,
                TotalValue = roleDataMini.TotalValue,

                GuildID = roleDataMini.GuildID,
                FamilyID = roleDataMini.FamilyID,
                FamilyRank = roleDataMini.FamilyRank,
                GuildRank = roleDataMini.GuildRank,
                OfficeRank = roleDataMini.OfficeRank,

                SelfCurrentTitleID = roleDataMini.SelfCurrentTitleID,
            };

            static GoodsData GetItemGD(int itemID)
            {
                if (Loader.Loader.Items.TryGetValue(itemID, out ItemData itemData))
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
            GoodsData weapon = GetItemGD(roleDataMini.WeaponID);
            if (weapon != null)
            {
                weapon.Using = (int) KE_EQUIP_POSITION.emEQUIPPOS_WEAPON;
                weapon.Forge_level = roleDataMini.WeaponEnhanceLevel;
                weapon.Series = roleDataMini.WeaponSeries;
                roleData.GoodsDataList.Add(weapon);
            }
            GoodsData armor = GetItemGD(roleDataMini.ArmorID);
            if (armor != null)
            {
                armor.Using = (int) KE_EQUIP_POSITION.emEQUIPPOS_BODY;
                roleData.GoodsDataList.Add(armor);
            }
            GoodsData helm = GetItemGD(roleDataMini.HelmID);
            if (helm != null)
            {
                helm.Using = (int) KE_EQUIP_POSITION.emEQUIPPOS_HEAD;
                roleData.GoodsDataList.Add(helm);
            }
            GoodsData mantle = GetItemGD(roleDataMini.MantleID);
            if (mantle != null)
            {
                mantle.Using = (int) KE_EQUIP_POSITION.emEQUIPPOS_MANTLE;
                roleData.GoodsDataList.Add(mantle);
            }
            GoodsData horse = GetItemGD(roleDataMini.HorseID);
            if (horse != null)
            {
                horse.Using = (int) KE_EQUIP_POSITION.emEQUIPPOS_HORSE;
                roleData.GoodsDataList.Add(horse);
            }
            GoodsData mask = GetItemGD(roleDataMini.MaskID);
            if (mask != null)
            {
                mask.Using = (int) KE_EQUIP_POSITION.emEQUIPPOS_MASK;
                roleData.GoodsDataList.Add(mask);
            }

            /// Trả về kết quả
            return roleData;
        }

        /// <summary>
        /// Chuyển DecoBotData sang RoleData
        /// </summary>
        /// <param name="botData"></param>
        /// <returns></returns>
        public static RoleData DecoBotDataToRoleData(DecoBotData botData)
        {
            /// Toác
            if (botData == null)
            {
                return null;
            }

            /// Tạo mới RoleData
            RoleData roleData = new RoleData()
            {
                RoleID = botData.RoleID,
                RoleName = botData.RoleName,
                RoleSex = botData.Sex,
                PosX = botData.PosX,
                PosY = botData.PosY,
                BufferDataList = botData.Buffs,
                GoodsDataList = new List<GoodsData>(),
                Title = botData.Title,
                IsRiding = false,
                MaxHP = 1000,
                CurrentHP = 1000,
                MoveSpeed = botData.MoveSpeed,
                AttackSpeed = botData.AtkSpeed,
                CastSpeed = botData.CastSpeed,
            };

            static GoodsData GetItemGD(int itemID)
            {
                if (Loader.Loader.Items.TryGetValue(itemID, out ItemData itemData))
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
            GoodsData weapon = GetItemGD(botData.WeaponID);
            if (weapon != null)
            {
                weapon.Using = (int) KE_EQUIP_POSITION.emEQUIPPOS_WEAPON;
                weapon.Forge_level = botData.WeaponEnhanceLevel;
                weapon.Series = botData.WeaponSeries;
                roleData.GoodsDataList.Add(weapon);
            }
            GoodsData armor = GetItemGD(botData.ArmorID);
            if (armor != null)
            {
                armor.Using = (int) KE_EQUIP_POSITION.emEQUIPPOS_BODY;
                roleData.GoodsDataList.Add(armor);
            }
            GoodsData helm = GetItemGD(botData.HelmID);
            if (helm != null)
            {
                helm.Using = (int) KE_EQUIP_POSITION.emEQUIPPOS_HEAD;
                roleData.GoodsDataList.Add(helm);
            }
            GoodsData mantle = GetItemGD(botData.MantleID);
            if (mantle != null)
            {
                mantle.Using = (int) KE_EQUIP_POSITION.emEQUIPPOS_MANTLE;
                roleData.GoodsDataList.Add(mantle);
            }
            GoodsData horse = GetItemGD(botData.HorseID);
            if (horse != null)
            {
                horse.Using = (int) KE_EQUIP_POSITION.emEQUIPPOS_HORSE;
                roleData.GoodsDataList.Add(horse);
                roleData.IsRiding = true;
            }

            /// Trả về kết quả
            return roleData;
        }
        #endregion
    }
}