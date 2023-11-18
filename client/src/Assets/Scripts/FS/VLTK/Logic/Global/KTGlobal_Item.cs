using FS.GameEngine.Logic;
using FS.VLTK.Entities;
using FS.VLTK.Entities.Config;
using FS.VLTK.UI.Main;
using FS.VLTK.UI.Main.SuperToolTip;
using GameServer.VLTK.Utilities;
using HSGameEngine.GameEngine.Network.Tools;
using Server.Data;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static FS.VLTK.Entities.Config.Equip_Level;
using static FS.VLTK.Entities.Enum;

namespace FS.VLTK
{
    /// <summary>
    /// Các hàm toàn cục dùng trong Kiếm Thế
    /// </summary>
    public static partial class KTGlobal
    {
        /// <summary>
        /// Đường dẫn Bundle chứa dấu hỏi chấm
        /// </summary>
        public const string UnknowItemBundleDir = "Icon/ItemUnknown.unity3d";

        /// <summary>
        /// Tên Atlas chứa dấu hỏi chấm
        /// </summary>
        public const string UnknownItemAtlas = "ItemUnknown";

        /// <summary>
        /// Tên Icon dấu hỏi chấm
        /// </summary>
        public const string UnknownItemIcon = "hoicham";

        #region Equip

        /// <summary>
        /// Code mặc định của Nam
        /// </summary>
        private const int DefaultMaleArmor = 20;

        /// <summary>
        /// Code mặc định của Nữ
        /// </summary>
        private const int DefaultFemaleArmor = 20;

        /// <summary>
        /// Code mặc định mũ của Nam
        /// </summary>
        private const int DefaultMaleHelm = 20;

        /// <summary>
        /// Code mặc định mũ của Nữ
        /// </summary>
        private const int DefaultFemaleHelm = 20;

        /// <summary>
        /// Code mặc định vũ khí của Nam
        /// </summary>
        private const int DefaultMaleWeapon = 2;

        /// <summary>
        /// Code mặc định vũ khí của Nữ
        /// </summary>
        private const int DefaultFemaleWeapon = 2;

        /// <summary>
        /// Trả về tên loại vũ khí
        /// </summary>
        /// <param name="weaponKind"></param>
        /// <returns></returns>
        public static string GetWeaponKind(int weaponKind)
        {
            switch (weaponKind)
            {
                case (int)KE_EQUIP_WEAPON_CATEGORY.emKEQUIP_WEAPON_CATEGORY_HAND:
                    return "Triền thủ";

                case (int)KE_EQUIP_WEAPON_CATEGORY.emKEQUIP_WEAPON_CATEGORY_SWORD:
                    return "Kiếm";

                case (int)KE_EQUIP_WEAPON_CATEGORY.emKEQUIP_WEAPON_CATEGORY_KNIFE:
                    return "Đao";

                case (int)KE_EQUIP_WEAPON_CATEGORY.emKEQUIP_WEAPON_CATEGORY_STICK:
                    return "Côn";

                case (int)KE_EQUIP_WEAPON_CATEGORY.emKEQUIP_WEAPON_CATEGORY_SPEAR:
                    return "Thương";

                case (int)KE_EQUIP_WEAPON_CATEGORY.emKEQUIP_WEAPON_CATEGORY_HAMMER:
                    return "Chùy";

                case (int)KE_EQUIP_WEAPON_CATEGORY.emKEQUIP_WEAPON_CATEGORY_FLYBAR:
                    return "Phi đao";

                case (int)KE_EQUIP_WEAPON_CATEGORY.emKEQUIP_WEAPON_CATEGORY_ARROW:
                    return "Tụ tiễn";

                case (int)KE_EQUIP_WEAPON_CATEGORY.emKEQUIP_WEAPON_CATEGORY_DART:
                    return "Phi tiêu";

                case (int)KE_EQUIP_WEAPON_CATEGORY.emKEQUIP_WEAPON_CATEGORY_DOUBLESWORDS:
                    return "Song đao";

                case (int)KE_EQUIP_WEAPON_CATEGORY.emKEQUIP_WEAPON_CATEGORY_RANGER_KNIFE:
                    return "Trường đao";

                default:
                    return null;
            }
        }

        /// <summary>
        /// Trả về tên vật phẩm có ID tương ứng
        /// </summary>
        /// <param name="itemID"></param>
        /// <returns></returns>
        public static string GetItemName(int itemID)
        {
            /// Nếu vật phẩm không tồn tại
            if (!Loader.Loader.Items.TryGetValue(itemID, out ItemData itemData))
            {
                return "";
            }

            /// Trả về tên vật phẩm
            return itemData.Name;
        }

        /// <summary>
        /// Trả về tên vật phẩm tương ứng
        /// </summary>
        /// <param name="itemGD"></param>
        /// <param name="includeEnhanceName"></param>
        /// <returns></returns>
        public static string GetItemName(GoodsData itemGD, bool includeEnhanceName = true)
        {
            /// Nếu vật phẩm không tồn tại
            if (!Loader.Loader.Items.TryGetValue(itemGD.GoodsID, out ItemData itemData))
            {
                return "";
            }

            /// Tên vật phẩm
            string name = itemData.Name;

            ///// Tên đi kèm đằng sau loại trang bị
            //string suffixName = "";

            ///// Nếu tồn tại danh sách thuộc tính ẩn
            //if (itemData.HiddenProp != null)
            //{
            //    if (itemData.HiddenProp.Count > 0)
            //    {
            //        /// Tìm danh sách thuộc tính tương ứng đảo ngược
            //        List<PropMagic> props = itemData.HiddenProp.OrderByDescending(x => x.Index).ToList();

            //        /// Duyệt danh sách thuộc tính
            //        foreach (PropMagic prop in props)
            //        {
            //            if (prop.MagicName.Length > 0)
            //            {
            //                /// Thuộc tính tương ứng
            //                MagicAttribLevel magicAttrib = Loader.Loader.MagicAttribLevels.Where(x => x.MagicName == prop.MagicName && x.Level == prop.MagicLevel).FirstOrDefault();

            //                /// Nếu tìm thấy
            //                if (magicAttrib != null)
            //                {
            //                    /// Trả về tên đi kèm đằng sau loại trang bị
            //                   // suffixName = magicAttrib.Suffix;
            //                    break;
            //                }
            //            }
            //        }
            //    }
            //}

            ///// Nếu tên đi kèm đằng sau chưa tồn tại
            //if (string.IsNullOrEmpty(suffixName))
            //{
            //    /// Nếu tồn tại danh sách thuộc tính ngẫu nhiên
            //    if (itemData.GreenProp != null)
            //    {
            //        if (itemData.GreenProp.Count > 0)
            //        {
            //            /// Tìm danh sách thuộc tính tương ứng đảo ngược
            //            List<PropMagic> props = itemData.GreenProp.OrderByDescending(x => x.Index).ToList();

            //            /// Duyệt danh sách thuộc tính
            //            foreach (PropMagic prop in props)
            //            {
            //                if (prop.MagicName.Length > 0)
            //                {
            //                    /// Thuộc tính tương ứng
            //                    MagicAttribLevel magicAttrib = Loader.Loader.MagicAttribLevels.Where(x => x.MagicName == prop.MagicName && x.Level == prop.MagicLevel).FirstOrDefault();

            //                    /// Nếu tìm thấy
            //                    if (magicAttrib != null)
            //                    {
            //                        /// Trả về tên đi kèm đằng sau loại trang bị
            //                       // suffixName = magicAttrib.Suffix;
            //                        break;
            //                    }
            //                }
            //            }
            //        }
            //    }
            //}

            ///// Nếu tồn tại tên đi kèm loại trang bị
            //if (!string.IsNullOrEmpty(suffixName))
            //{
            //    name += string.Format(" - {0}", suffixName);
            //}

            /// Nếu là trang bị và có thể cường hóa
            if ((KTGlobal.IsEquip(itemGD.GoodsID) || KTGlobal.IsPetEquipItem(itemGD.GoodsID)) && includeEnhanceName && KTGlobal.CanEquipBeEnhance(itemData))
            {
                /// Cấp cường hóa nếu có
                if (itemGD.Forge_level > 0)
                {
                    name += string.Format(" +{0}", itemGD.Forge_level);
                }
                else
                {
                    name += " - Chưa cường hóa";
                }
            }

            /// Trả về kết quả
            return name;
        }

        /// <summary>
        /// Trả về màu tên vật phẩm
        /// </summary>
        /// <param name="itemGD"></param>
        /// <returns></returns>
        public static Color GetItemColor(GoodsData itemGD)
        {
            /// Nếu là trang bị
            if (KTGlobal.IsEquip(itemGD.GoodsID) || KTGlobal.IsPetEquipItem(itemGD.GoodsID))
            {
                /// Tính màu theo số sao
                StarLevelStruct starLevelStruct = KTGlobal.StarCalculation(itemGD);
                /// Nếu có dữ liệu số sao
                if (starLevelStruct != null)
                {
                    Color color = Color.white;

                    if (starLevelStruct.NameColor == "green")
                    {
                        ColorUtility.TryParseHtmlString("#78ff1f", out color);
                    }
                    else if (starLevelStruct.NameColor == "blue")
                    {
                        ColorUtility.TryParseHtmlString("#14a9ff", out color);
                    }
                    else if (starLevelStruct.NameColor == "purple")
                    {
                        ColorUtility.TryParseHtmlString("#d82efa", out color);
                    }
                    else if (starLevelStruct.NameColor == "orange")
                    {
                        ColorUtility.TryParseHtmlString("#ff8e3d", out color);
                    }
                    else if (starLevelStruct.NameColor == "yellow")
                    {
                        ColorUtility.TryParseHtmlString("#fffb29", out color);
                    }
                    return color;
                }
                /// Nếu không có dữ liệu số sao thì mặc định lấy màu xanh lục
                else
                {
                    ColorUtility.TryParseHtmlString("#78ff1f", out Color color);
                    return color;
                }
            }
            /// Nếu là vật phẩm thì mặc định màu trắng
            else
            {
                return Color.white;
            }
        }

        /// <summary>
        /// Trả về chuỗi mô tả thông tin vật phẩm ở kênh Chat
        /// </summary>
        /// <param name="itemGD"></param>
        /// <returns></returns>
        public static string GetItemDescInfoStringForChat(GoodsData itemGD)
        {
            if (itemGD == null)
            {
                return "";
            }

            /// Dữ liệu vật phẩm
            string itemName = KTGlobal.GetItemName(itemGD);
            string itemDescString = string.Format("<color=#{0}><link=\"ITEM_{1}\">[{2}]</link></color>", ColorUtility.ToHtmlStringRGB(KTGlobal.GetItemColor(itemGD)), itemGD.Id, itemName);

            /// Trả về kết quả
            return itemDescString;
        }

        /// <summary>
        /// Trả về Res áo theo ID
        /// </summary>
        /// <param name="armorID"></param>
        /// <param name="sex"></param>
        /// <returns></returns>
        public static string GetArmorResByID(int armorID, int sex)
        {
            if (Loader.Loader.Items.TryGetValue(armorID, out ItemData itemData))
            {
                if (sex == (int)Sex.MALE)
                {
                    if (Loader.Loader.CharacterActionSetXML.MaleArmorByCode.TryGetValue(itemData.ResManID, out CharacterActionSetXML.Component component))
                    {
                        return component.ID;
                    }
                    else
                    {
                        return Loader.Loader.CharacterActionSetXML.MaleArmorByCode[KTGlobal.DefaultMaleArmor].ID;
                    }
                }
                else
                {
                    if (Loader.Loader.CharacterActionSetXML.FemaleArmorByCode.TryGetValue(itemData.ResWomanID, out CharacterActionSetXML.Component component))
                    {
                        return component.ID;
                    }
                    else
                    {
                        return Loader.Loader.CharacterActionSetXML.FemaleArmorByCode[KTGlobal.DefaultFemaleArmor].ID;
                    }
                }
            }

            if (sex == (int)Sex.MALE)
            {
                return Loader.Loader.CharacterActionSetXML.MaleArmorByCode[KTGlobal.DefaultMaleArmor].ID;
            }
            else
            {
                return Loader.Loader.CharacterActionSetXML.FemaleArmorByCode[KTGlobal.DefaultFemaleArmor].ID;
            }
        }

        /// <summary>
        /// Trả về Res vũ khí theo ID
        /// </summary>
        /// <param name="weaponID"></param>
        /// <param name="sex"></param>
        /// <returns></returns>
        public static string GetWeaponResByID(int weaponID, int sex)
        {
            if (Loader.Loader.Items.TryGetValue(weaponID, out ItemData itemData))
            {
                if (sex == (int)Sex.MALE)
                {
                    if (Loader.Loader.CharacterActionSetXML.MaleWeaponByCode.TryGetValue(itemData.ResManID, out CharacterActionSetXML.Component component))
                    {
                        return component.ID;
                    }
                    else
                    {
                        return Loader.Loader.CharacterActionSetXML.MaleWeaponByCode[KTGlobal.DefaultMaleWeapon].ID;
                    }
                }
                else
                {
                    if (Loader.Loader.CharacterActionSetXML.FemaleWeaponByCode.TryGetValue(itemData.ResWomanID, out CharacterActionSetXML.Component component))
                    {
                        return component.ID;
                    }
                    else
                    {
                        return Loader.Loader.CharacterActionSetXML.FemaleWeaponByCode[KTGlobal.DefaultFemaleWeapon].ID;
                    }
                }
            }

            if (sex == (int)Sex.MALE)
            {
                return Loader.Loader.CharacterActionSetXML.MaleWeaponByCode[KTGlobal.DefaultMaleWeapon].ID;
            }
            else
            {
                return Loader.Loader.CharacterActionSetXML.FemaleWeaponByCode[KTGlobal.DefaultFemaleWeapon].ID;
            }
        }

        /// <summary>
        /// Trả về Res mũ theo ID
        /// </summary>
        /// <param name="helmID"></param>
        /// <param name="sex"></param>
        /// <returns></returns>
        public static string GetHelmResByID(int helmID, int sex)
        {
            if (Loader.Loader.Items.TryGetValue(helmID, out ItemData itemData))
            {
                if (sex == (int)Sex.MALE)
                {
                    if (Loader.Loader.CharacterActionSetXML.MaleHeadByCode.TryGetValue(itemData.ResManID, out CharacterActionSetXML.Component component))
                    {
                        return component.ID;
                    }
                    else
                    {
                        return Loader.Loader.CharacterActionSetXML.MaleHeadByCode[KTGlobal.DefaultMaleHelm].ID;
                    }
                }
                else
                {
                    if (Loader.Loader.CharacterActionSetXML.FemaleHeadByCode.TryGetValue(itemData.ResWomanID, out CharacterActionSetXML.Component component))
                    {
                        return component.ID;
                    }
                    else
                    {
                        return Loader.Loader.CharacterActionSetXML.FemaleHeadByCode[KTGlobal.DefaultFemaleHelm].ID;
                    }
                }
            }

            if (sex == (int)Sex.MALE)
            {
                return Loader.Loader.CharacterActionSetXML.MaleHeadByCode[KTGlobal.DefaultMaleHelm].ID;
            }
            else
            {
                return Loader.Loader.CharacterActionSetXML.FemaleHeadByCode[KTGlobal.DefaultFemaleHelm].ID;
            }
        }

        /// <summary>
        /// Trả về Res phi phong theo ID
        /// </summary>
        /// <param name="mantleID"></param>
        /// <param name="sex"></param>
        /// <returns></returns>
        public static string GetMantleResByID(int mantleID, int sex)
        {
            if (Loader.Loader.Items.TryGetValue(mantleID, out ItemData itemData))
            {
                if (sex == (int)Sex.MALE)
                {
                    if (Loader.Loader.CharacterActionSetXML.MaleMantleByCode.TryGetValue(itemData.ResManID, out CharacterActionSetXML.Component component))
                    {
                        return component.ID;
                    }
                }
                else
                {
                    if (Loader.Loader.CharacterActionSetXML.FemaleMantleByCode.TryGetValue(itemData.ResWomanID, out CharacterActionSetXML.Component component))
                    {
                        return component.ID;
                    }
                }
            }

            return "";
        }

        /// <summary>
        /// Trả về Res ngựa theo ID
        /// </summary>
        /// <param name="horseID"></param>
        /// <returns></returns>
        public static string GetHorseResByID(int horseID)
        {
            if (Loader.Loader.Items.TryGetValue(horseID, out ItemData itemData))
            {
                if (Loader.Loader.CharacterActionSetXML.RiderByCode.TryGetValue(itemData.ResManID, out CharacterActionSetXML.Component component))
                {
                    return component.ID;
                }
            }

            return "";
        }

        /// <summary>
        /// Tạo vật phẩm xem trước
        /// </summary>
        /// <param name="itemData"></param>
        /// <returns></returns>
        public static GoodsData CreateItemPreview(ItemData itemData)
        {
            /// Nếu vật phẩm không tồn tại
            if (itemData == null)
            {
                return null;
            }

            return new GoodsData()
            {
                GoodsID = itemData.ItemID,
                GCount = 1,
                Series = itemData.Series == -1 ? -1 : itemData.Series,
                Strong = 100,
            };
        }

        /// <summary>
        /// Trả về thông tin trang bị tại vị trí chỉ định
        /// </summary>
        /// <param name="roleData"></param>
        /// <param name="equipPos"></param>
        /// <returns></returns>
        public static GoodsData GetEquipData(RoleData roleData, KE_EQUIP_POSITION equipPos)
        {
            /// Nếu dữ liệu nhân vật không tồn tại
            if (roleData == null)
            {
                return null;
            }
            /// Nếu danh sách đồ rỗng
            else if (roleData.GoodsDataList == null)
            {
                return null;
            }

            GoodsData itemGD = roleData.GoodsDataList.Where(x => x.Using == (int)equipPos).FirstOrDefault();
            return itemGD;
        }

        /// <summary>
        /// Trả về thông tin vũ khí tương ứng theo RoleDataMini
        /// </summary>
        /// <param name="roleDataMini"></param>
        /// <returns></returns>
        public static GoodsData GetWeaponData(RoleDataMini roleDataMini)
        {
            /// Nếu dữ liệu nhân vật không tồn tại
            if (roleDataMini == null)
            {
                return null;
            }
            /// Nếu không có vũ khí
            if (!Loader.Loader.Items.TryGetValue(roleDataMini.WeaponID, out ItemData itemData))
            {
                return null;
            }

            GoodsData itemGD = new GoodsData()
            {
                GoodsID = roleDataMini.WeaponID,
                Forge_level = roleDataMini.WeaponEnhanceLevel,
                Series = roleDataMini.WeaponSeries,
                GCount = 1,
            };
            return itemGD;
        }

        /// <summary>
        /// Trả về danh sách trang bị của đối tượng
        /// </summary>
        /// <param name="roleData"></param>
        /// <returns></returns>
        public static Dictionary<KE_EQUIP_POSITION, GoodsData> GetEquips(RoleData roleData)
        {
            Dictionary<KE_EQUIP_POSITION, GoodsData> equips = new Dictionary<KE_EQUIP_POSITION, GoodsData>();
            /// Nếu danh sách đồ rỗng
            if (roleData.GoodsDataList == null)
            {
                return equips;
            }

            /// Duyệt danh sách trang bị trên người
            foreach (GoodsData equip in roleData.GoodsDataList)
            {
                switch (equip.Using)
                {
                    /// Vũ khí
                    case (int)KE_EQUIP_POSITION.emEQUIPPOS_WEAPON:
                        {
                            equips[(KE_EQUIP_POSITION)equip.Using] = equip;
                            break;
                        }
                    /// Liên
                    case (int)KE_EQUIP_POSITION.emEQUIPPOS_RING_2:
                        {
                            equips[(KE_EQUIP_POSITION)equip.Using] = equip;
                            break;
                        }
                    /// Nhẫn
                    case (int)KE_EQUIP_POSITION.emEQUIPPOS_RING:
                        {
                            equips[(KE_EQUIP_POSITION)equip.Using] = equip;
                            break;
                        }
                    /// Nang
                    case (int)KE_EQUIP_POSITION.emEQUIPPOS_PENDANT:
                        {
                            equips[(KE_EQUIP_POSITION)equip.Using] = equip;
                            break;
                        }
                    /// Phù
                    case (int)KE_EQUIP_POSITION.emEQUIPPOS_AMULET:
                        {
                            equips[(KE_EQUIP_POSITION)equip.Using] = equip;
                            break;
                        }
                    /// Mũ
                    case (int)KE_EQUIP_POSITION.emEQUIPPOS_HEAD:
                        {
                            equips[(KE_EQUIP_POSITION)equip.Using] = equip;
                            break;
                        }
                    /// Áo
                    case (int)KE_EQUIP_POSITION.emEQUIPPOS_BODY:
                        {
                            equips[(KE_EQUIP_POSITION)equip.Using] = equip;
                            break;
                        }
                    /// Lưng
                    case (int)KE_EQUIP_POSITION.emEQUIPPOS_BELT:
                        {
                            equips[(KE_EQUIP_POSITION)equip.Using] = equip;
                            break;
                        }
                    /// Tay
                    case (int)KE_EQUIP_POSITION.emEQUIPPOS_CUFF:
                        {
                            equips[(KE_EQUIP_POSITION)equip.Using] = equip;
                            break;
                        }
                    /// Giày
                    case (int)KE_EQUIP_POSITION.emEQUIPPOS_FOOT:
                        {
                            equips[(KE_EQUIP_POSITION)equip.Using] = equip;
                            break;
                        }
                    /// Phi phong
                    case (int)KE_EQUIP_POSITION.emEQUIPPOS_MANTLE:
                        {
                            equips[(KE_EQUIP_POSITION)equip.Using] = equip;
                            break;
                        }
                    /// Ngựa
                    case (int)KE_EQUIP_POSITION.emEQUIPPOS_HORSE:
                        {
                            equips[(KE_EQUIP_POSITION)equip.Using] = equip;
                            break;
                        }
                    /// Trang sức
                    case (int)KE_EQUIP_POSITION.emEQUIPPOS_ORNAMENT:
                        {
                            equips[(KE_EQUIP_POSITION)equip.Using] = equip;
                            break;
                        }
                    /// Ngũ hành ấn
                    case (int)KE_EQUIP_POSITION.emEQUIPPOS_SIGNET:
                        {
                            equips[(KE_EQUIP_POSITION)equip.Using] = equip;
                            break;
                        }
                    /// Quan ấn
                    case (int)KE_EQUIP_POSITION.emEQUIPPOS_CHOP:
                        {
                            equips[(KE_EQUIP_POSITION)equip.Using] = equip;
                            break;
                        }
                    /// Mật tịch
                    case (int)KE_EQUIP_POSITION.emEQUIPPOS_BOOK:
                        {
                            equips[(KE_EQUIP_POSITION)equip.Using] = equip;
                            break;
                        }
                    /// Mặt nạ
                    case (int)KE_EQUIP_POSITION.emEQUIPPOS_MASK:
                        {
                            equips[(KE_EQUIP_POSITION)equip.Using] = equip;
                            break;
                        }
                }
            }

            return equips;
        }

        /// <summary>
        /// Trả về loại cường hóa trang bị
        /// </summary>
        /// <param name="itemGD"></param>
        /// <returns></returns>
        public static int GetWeaponEnhanceType(GoodsData itemGD)
        {
            /// Nếu vật phẩm không tồn tại
            if (!Loader.Loader.Items.TryGetValue(itemGD.GoodsID, out ItemData itemData))
            {
                return 0;
            }

            /// Nếu không phải trang bị
            if (!itemData.IsEquip)
            {
                return 0;
            }

            /// Loại cường hóa
            int enhanceType = 0;
            /// Tạm thời disable phần tích sao
            StarLevelStruct starInfo = KTGlobal.StarCalculation(itemGD);
            float starLevel = starInfo.StarLevel / 2f;
            /// Nếu số sao >= 9.5
            if (starLevel >= 9.5)
            {
                enhanceType = 4;
            }
            else if (starLevel >= 8.5)
            {
                enhanceType = 3;
            }
            else if (starLevel >= 7.5)
            {
                enhanceType = 2;
            }
            else if (starLevel >= 6.5)
            {
                enhanceType = 1;
            }

            /// Trả ra kết quả loại cường hóa
            return enhanceType;
        }

        /// <summary>
        /// Hàm trả về tổng số trang bị trên người cùng ID bộ
        /// </summary>
        /// <param name="InputSuite"></param>
        /// <returns></returns>
        public static int CountAllSuiteID(int InputSuite)
        {
            int SuiteID = 0;
            if (null == Global.Data.RoleData.GoodsDataList)
            {
                return SuiteID;
            }
            foreach (GoodsData _Data in Global.Data.RoleData.GoodsDataList)
            {
                if (_Data.Using >= 0 && _Data.Using < 100)
                {
                    /// Nếu thông tin trang bị không tồn tại
                    if (!Loader.Loader.Items.TryGetValue(_Data.GoodsID, out ItemData ItemTemplate))
                    {
                        continue;
                    }
                    else
                    {
                        int SuiteItem = ItemTemplate.SuiteID;
                        if (InputSuite == SuiteItem)
                        {
                            SuiteID++;
                        }
                    }
                }
            }

            return SuiteID;
        }

        /// <summary>
        /// Vẫn phải dùng cái đầu buồi này để check thuộc tính kích hoạt all trang sức khá là cose
        /// </summary>
        /// <returns></returns>
        public static int GetActiveSuiteStatusLevel()
        {
            int Level = -1;

            // Nếu đéo mặc đồ nào thì thôi
            if (null == Global.Data.RoleData.GoodsDataList)
            {
                return Level;
            }
            /// Duyệt ra toàn bộ đồ đang sử dụng
            foreach (GoodsData _Data in Global.Data.RoleData.GoodsDataList)
            {
                if (_Data.Using >= 0)
                {
                    /// Nếu thông tin trang bị không tồn tại
                    if (!Loader.Loader.Items.TryGetValue(_Data.GoodsID, out ItemData itemData))
                    {
                        continue;
                    }
                    else
                    {
                        // Đọc ra Template của Item
                        if (itemData.SuiteID > 0)
                        {
                            /// Thuộc tính bộ
                            SuiteActiveProp emptyProps = Loader.Loader.SuiteActiveProps.Where(x => x.SuiteID == itemData.SuiteID).FirstOrDefault();
                            /// Đếm xem trên người có bao nhiêu bộ cùng SUTEID
                            int totalActive = KTGlobal.CountAllSuiteID(itemData.SuiteID);

                            foreach (SuiteActive _SUTEACTIVE in emptyProps.ListActive)
                            {
                                int NUMBERREQUEST = _SUTEACTIVE.RequestNum;
                                if (totalActive >= NUMBERREQUEST)
                                {


                                    if (_SUTEACTIVE.SuiteName == "active_suit")
                                    {
                                        if (Level < 2)
                                        {
                                            Level = 2;
                                        }

                                    }

                                    if (_SUTEACTIVE.SuiteName == "active_all_ornament")
                                    {
                                        if (Level < 1)
                                        {
                                            Level = 1;
                                        }

                                    }

                                }

                            }
                        }
                    }
                }

            }
            return Level;
        }

        #endregion Equip

        #region Equip Super ToolTip

        /// <summary>
        /// Trả về ngũ hành dùng cho phái tương ứng
        /// </summary>
        /// <param name="itemData"></param>
        /// <returns></returns>
        public static Elemental GetRecommendSeries(ItemData itemData)
        {
            /// Nếu không có Series
            if (itemData.Series < 1 || itemData.Series > 5)
            {
                return Elemental.NONE;
            }

            return Elemental.NONE;
        }

        /// <summary>
        /// Trả về Text dùng cho môn phái hệ gì đó
        /// </summary>
        /// <param name="itemGD"></param>
        /// <returns></returns>
        private static string GetRecommendSeriesText(GoodsData itemGD)
        {
            /// Nếu vật phẩm không tồn tại
            if (!Loader.Loader.Items.TryGetValue(itemGD.GoodsID, out ItemData itemData))
            {
                return "";
            }

            //itemGD.Series = itemGD.Series;
            /// Nếu không có ngũ hành
            if (itemGD.Series < 1 || itemGD.Series > 5)
            {
                return "";
            }

            return "";
        }

        /// <summary>
        /// Vật phẩm có thể bán được không
        /// </summary>
        /// <param name="bindType"></param>
        /// <returns></returns>
        private static bool IsItemCanBeSold(int bindType)
        {
            return bindType < 3;
        }

        /// <summary>
        /// Trả về tên loại trang bị pet theo vị trí
        /// </summary>
        /// <param name="slot"></param>
        /// <returns></returns>
        private static string GetPetEquipTypeString(PetEquipSlot slot)
        {
            switch (slot)
            {
                case PetEquipSlot.Helm:
                    return "Nón";

                case PetEquipSlot.Armor:
                    return "Giáp";

                case PetEquipSlot.Belt:
                    return "Lưng";

                case PetEquipSlot.Weapon:
                    return "Vũ khí";

                case PetEquipSlot.Necklace:
                    return "Liên";

                case PetEquipSlot.Boot:
                    return "Giày";

                case PetEquipSlot.Cuff:
                    return "Vòng";

                case PetEquipSlot.Ring:
                    return "Nhẫn";

                default:
                    return "Chưa rõ";
            }
        }

        /// <summary>
        /// Trả về loại trang bị
        /// </summary>
        /// <param name="InputType"></param>
        /// <returns></returns>
        private static string GetEquipTypeString(KE_ITEM_EQUIP_DETAILTYPE equipType)
        {
            string ret = "";

            switch (equipType)
            {
                case KE_ITEM_EQUIP_DETAILTYPE.equip_rangeweapon:
                case KE_ITEM_EQUIP_DETAILTYPE.equip_meleeweapon:
                    {
                        ret = "Vũ Khí";
                    }
                    break;

                case KE_ITEM_EQUIP_DETAILTYPE.equip_armor:
                    {
                        ret = "Áo";
                    }
                    break;

                case KE_ITEM_EQUIP_DETAILTYPE.equip_ring:
                    {
                        ret = "Nhẫn";
                    }
                    break;

                case KE_ITEM_EQUIP_DETAILTYPE.equip_amulet:
                    {
                        ret = "Phù";
                    }
                    break;

                case KE_ITEM_EQUIP_DETAILTYPE.equip_boots:
                    {
                        ret = "Giày";
                    }
                    break;

                case KE_ITEM_EQUIP_DETAILTYPE.equip_belt:
                    {
                        ret = "Lưng";
                    }
                    break;

                case KE_ITEM_EQUIP_DETAILTYPE.equip_helm:
                    {
                        ret = "Mũ";
                    }

                    break;

                case KE_ITEM_EQUIP_DETAILTYPE.equip_cuff:
                    {
                        ret = "Tay";
                    }
                    break;

                case KE_ITEM_EQUIP_DETAILTYPE.equip_pendant:
                    {
                        ret = "Bội";
                    }
                    break;

                case KE_ITEM_EQUIP_DETAILTYPE.equip_horse:
                    {
                        ret = "Ngựa";
                    }
                    break;

                case KE_ITEM_EQUIP_DETAILTYPE.equip_mask:
                    {
                        ret = "Mặt Nạ";
                    }
                    break;

                case KE_ITEM_EQUIP_DETAILTYPE.equip_book:
                    {
                        ret = "Mật Tịch";
                    }
                    break;

                case KE_ITEM_EQUIP_DETAILTYPE.equip_ornament:
                    {
                        ret = "Trang sức";
                    }
                    break;

                case KE_ITEM_EQUIP_DETAILTYPE.equip_signet:
                    {
                        ret = "Ấn";
                    }
                    break;

                case KE_ITEM_EQUIP_DETAILTYPE.equip_mantle:
                    {
                        ret = "Phi Phong";
                    }
                    break;

                case KE_ITEM_EQUIP_DETAILTYPE.equip_chop:
                    {
                        ret = "Quan Ấn";
                    }
                    break;

                default:
                    {
                        ret = "Chưa mở";
                    }
                    break;
            }

            return ret;
        }

        private static string GetItemNameByPosition(KE_EQUIP_POSITION Pos_input)
        {
            string ret = "";

            switch (Pos_input)
            {
                case KE_EQUIP_POSITION.emEQUIPPOS_RING_2:
                    {
                        ret = "Nhẫn Trên";
                    }
                    break;

                case KE_EQUIP_POSITION.emEQUIPPOS_AMULET:
                    {
                        ret = "Liên - Phù";
                    }
                    break;

                case KE_EQUIP_POSITION.emEQUIPPOS_BODY:
                    {
                        ret = "Áo";
                    }
                    break;

                case KE_EQUIP_POSITION.emEQUIPPOS_PENDANT:
                    {
                        ret = "Bội";
                    }
                    break;

                case KE_EQUIP_POSITION.emEQUIPPOS_RING:
                    {
                        ret = "Nhẫn Dưới";
                    }
                    break;

                case KE_EQUIP_POSITION.emEQUIPPOS_BELT:
                    {
                        ret = "Lưng";
                    }
                    break;

                case KE_EQUIP_POSITION.emEQUIPPOS_CUFF:
                    {
                        ret = "Bao Tay";
                    }

                    break;

                case KE_EQUIP_POSITION.emEQUIPPOS_HEAD:
                    {
                        ret = "Mũ";
                    }
                    break;

                case KE_EQUIP_POSITION.emEQUIPPOS_WEAPON:
                    {
                        ret = "Vũ Khí";
                    }
                    break;

                case KE_EQUIP_POSITION.emEQUIPPOS_FOOT:
                    {
                        ret = "Giày";
                    }
                    break;

                default:
                    {
                        ret = "Chưa mở";
                    }
                    break;
            }

            return ret;
        }

        /// <summary>
        /// Kiểm tra vật phẩm có ItemGeneral tương ứng có phải trang bị không
        /// </summary>
        /// <param name="itemGeneral"></param>
        /// <returns></returns>
        public static bool IsItemEquip(int itemGeneral)
        {
            if (itemGeneral == (int)KE_ITEM_GENRE.item_equip_general || itemGeneral == (int)KE_ITEM_GENRE.item_equip_gold || itemGeneral == (int)KE_ITEM_GENRE.item_equip_purple)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Kiểm tra vật phẩm có ItemGeneral tương ứng có phải trang bị pet hay không
        /// </summary>
        /// <param name="itemGeneral"></param>
        public static bool IsPetEquip(int itemGeneral)
        {
            return itemGeneral == (int)KE_ITEM_GENRE.item_pet_equip;
        }

        public static bool IsPetEquipItem(int itemID)
        {
            if (Loader.Loader.Items.TryGetValue(itemID, out ItemData itemData))
            {
                return KTGlobal.IsPetEquip(itemData.Genre);
            }
            return false;
        }


        /// <summary>
        /// Kiểm tra vật phẩm có bán được không
        /// </summary>
        /// <param name="itemGD"></param>
        /// <returns></returns>
        public static bool IsCanBeSold(GoodsData itemGD)
        {
            /// Nếu là trang bị và có cường hóa
            if (KTGlobal.IsEquip(itemGD.GoodsID) && itemGD.Forge_level > 0)
            {
                return false;
            }
            /// Nếu thông tin vật phẩm tồn tại
            if (Loader.Loader.Items.TryGetValue(itemGD.GoodsID, out ItemData itemData))
            {
                return KTGlobal.IsItemCanBeSold(itemData.BindType);
            }
            /// Trả về không thể bán
            return false;
        }

        /// <summary>
        /// Kiểm tra vật phẩm có phải trang bị không
        /// </summary>
        /// <param name="itemID"></param>
        /// <returns></returns>
        public static bool IsEquip(int itemID)
        {
            if (Loader.Loader.Items.TryGetValue(itemID, out ItemData itemData))
            {
                return KTGlobal.IsItemEquip(itemData.Genre);
            }
            return false;
        }

        /// <summary>
        /// Trả về loại vũ khí
        /// </summary>
        /// <param name="detailType"></param>
        /// <returns></returns>
        private static KE_ITEM_EQUIP_DETAILTYPE GetEquipType(int detailType)
        {
            return (KE_ITEM_EQUIP_DETAILTYPE)detailType;
        }

        /// <summary>
        /// Nếu là có độ bền
        /// </summary>
        /// <returns></returns>
        public static bool KD_ISNONNAMENT(int detailType)
        {
            if (detailType == (int)KE_ITEM_EQUIP_DETAILTYPE.equip_armor ||
                detailType == (int)KE_ITEM_EQUIP_DETAILTYPE.equip_belt ||
                detailType == (int)KE_ITEM_EQUIP_DETAILTYPE.equip_boots ||
                detailType == (int)KE_ITEM_EQUIP_DETAILTYPE.equip_cuff ||
                detailType == (int)KE_ITEM_EQUIP_DETAILTYPE.equip_rangeweapon ||
                detailType == (int)KE_ITEM_EQUIP_DETAILTYPE.equip_meleeweapon ||
                detailType == (int)KE_ITEM_EQUIP_DETAILTYPE.equip_helm)
            {
                return true;
            }

            return false;
        }

        public static bool KD_ISORNAMENT(int detailType)
        {
            if (detailType == (int)KE_ITEM_EQUIP_DETAILTYPE.equip_amulet ||
                detailType == (int)KE_ITEM_EQUIP_DETAILTYPE.equip_pendant ||
                detailType == (int)KE_ITEM_EQUIP_DETAILTYPE.equip_ring ||
                detailType == (int)KE_ITEM_EQUIP_DETAILTYPE.equip_ornament)

            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Kiểm tra trang bị tương ứng có cường hóa được không
        /// </summary>
        /// <param name="itemData"></param>
        /// <returns></returns>
        private static bool CanEquipBeEnhance(ItemData itemData)
        {
            return itemData.DetailType <= 13;
        }

        /// <summary>
        /// Kiểm tra bản thân có sử dụng được trang bị tương ứng không
        /// </summary>
        /// <param name="itemGD"></param>
        /// <returns></returns>
        public static bool IsCanUseEquip(GoodsData itemGD)
        {
            ItemData itemData = null;
            /// Nếu không tồn tại vật phẩm tương ứng
            if (!Loader.Loader.Items.TryGetValue(itemGD.GoodsID, out itemData))
            {
                return false;
            }
            /// Nếu không phải trang bị
            else if (!itemData.IsEquip)
            {
                return false;
            }

            /// Nếu có thuộc tính yêu cầu
            if (itemData.ListReqProp != null)
            {
                /// Duyệt danh sách các thuộc tính yêu cầu
                foreach (ReqProp request in itemData.ListReqProp)
                {
                    /// Yêu cầu môn phái
                    if (request.ReqPropType == (int)KE_ITEM_REQUIREMENT.emEQUIP_REQ_FACTION)
                    {
                        /// Nếu môn phái không phù hợp
                        if (Global.Data.RoleData.FactionID != request.ReqPropValue)
                        {
                            return false;
                        }
                    }
                    /// Yêu cầu nhánh

                    /// Yêu cầu ngũ hành
                    else if (request.ReqPropType == (int)KE_ITEM_REQUIREMENT.emEQUIP_REQ_SERIES)
                    {
                        /// Nếu có môn phái
                        if (Loader.Loader.Factions.TryGetValue(Global.Data.RoleData.FactionID, out FactionXML factionXML))
                        {
                            /// Nếu ngũ hành không phù hợp
                            if ((int)factionXML.Elemental != request.ReqPropValue)
                            {
                                return false;
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                    /// Yêu cầu giới tính
                    else if (request.ReqPropType == (int)KE_ITEM_REQUIREMENT.emEQUIP_REQ_SEX)
                    {
                        if (Global.Data.RoleData.RoleSex != request.ReqPropValue)
                        {
                            return false;
                        }
                    }
                    else if (request.ReqPropType == (int)KE_ITEM_REQUIREMENT.emEQUIP_REQ_DEX)
                    {
                        if (Global.Data.RoleData.m_wDexterity < request.ReqPropValue)
                        {
                            return false;
                        }
                    }
                    else if (request.ReqPropType == (int)KE_ITEM_REQUIREMENT.emEQUIP_REQ_VIT)
                    {
                        if (Global.Data.RoleData.m_wVitality < request.ReqPropValue)
                        {
                            return false;
                        }
                    }
                    else if (request.ReqPropType == (int)KE_ITEM_REQUIREMENT.emEQUIP_REQ_STR)
                    {
                        if (Global.Data.RoleData.m_wStrength < request.ReqPropValue)
                        {
                            return false;
                        }
                    }
                    else if (request.ReqPropType == (int)KE_ITEM_REQUIREMENT.emEQUIP_REQ_ENG)
                    {
                        if (Global.Data.RoleData.m_wEnergy < request.ReqPropValue)
                        {
                            return false;
                        }
                    }
                    /// Yêu cầu cấp độ
                    else if (request.ReqPropType == (int)KE_ITEM_REQUIREMENT.emEQUIP_REQ_LEVEL)
                    {
                        if (Global.Data.RoleData.Level < request.ReqPropValue)
                        {
                            return false;
                        }
                    }
                }
            }

            /// OK
            return true;
        }

        /// <summary>
        /// Xây Tooltip yêu cầu trang bị
        /// </summary>
        /// <param name="listReq"></param>
        private static string BuildEquipRequiration(List<ReqProp> listReq)
        {
            StringBuilder contentBuilder = new StringBuilder();

            /// Dữ liệu nhân vật
            RoleData role = Global.Data.RoleData;

            int requireRouteID = -1;
            int requireFactionID = -1;
            int requireSeriesID = -1;

            /// Nếu có danh sách thuộc tính yêu cầu
            if (listReq != null)
            {
                /// Duyệt danh sách các thuộc tính yêu cầu
                foreach (ReqProp request in listReq)
                {
                    /// Yêu cầu môn phái
                    if (request.ReqPropType == (int)KE_ITEM_REQUIREMENT.emEQUIP_REQ_FACTION)
                    {
                        requireFactionID = request.ReqPropValue;
                    }
                    /// Yêu cầu nhánh

                    /// Yêu cầu ngũ hành
                    else if (request.ReqPropType == (int)KE_ITEM_REQUIREMENT.emEQUIP_REQ_SERIES)
                    {
                        requireSeriesID = request.ReqPropValue;
                    }
                    /// Yêu cầu giới tính
                    else if (request.ReqPropType == (int)KE_ITEM_REQUIREMENT.emEQUIP_REQ_SEX) // Yêu cầu ngũ hành
                    {
                        if (role.RoleSex != request.ReqPropValue)
                        {
                            contentBuilder.AppendLine("Giới tính: <color=#5d8bda>" + KTGlobal.SexToString(request.ReqPropValue) + "</color>");
                        }
                        else
                        {
                            contentBuilder.AppendLine("Giới tính: " + KTGlobal.SexToString(request.ReqPropValue) + "</color>");
                        }
                    }
                    else if (request.ReqPropType == (int)KE_ITEM_REQUIREMENT.emEQUIP_REQ_DEX) // Yêu cầu ngũ hành
                    {
                        if (request.ReqPropValue > 0)
                        {
                            if (role.m_wDexterity < request.ReqPropValue)
                            {
                                contentBuilder.AppendLine("Yêu cầu thân pháp: " + request.ReqPropValue + " điểm</color>");
                            }
                            else
                            {
                                contentBuilder.AppendLine("Yêu cầu thân pháp: " + request.ReqPropValue + " điểm</color>");
                            }
                        }
                    }
                    else if (request.ReqPropType == (int)KE_ITEM_REQUIREMENT.emEQUIP_REQ_STR) // Yêu cầu ngũ hành
                    {
                        if (request.ReqPropValue > 0)
                        {
                            if (role.m_wStrength < request.ReqPropValue)
                            {
                                contentBuilder.AppendLine("<color=#F44336>Yêu cầu sức mạnh: " + request.ReqPropValue + " điểm</color>");
                            }
                            else
                            {
                                contentBuilder.AppendLine("Yêu cầu sức mạnh: " + request.ReqPropValue + " điểm</color>");
                            }
                        }
                    }
                    else if (request.ReqPropType == (int)KE_ITEM_REQUIREMENT.emEQUIP_REQ_ENG) // Yêu cầu ngũ hành
                    {
                        if (request.ReqPropValue > 0)
                        {
                            if (role.m_wEnergy < request.ReqPropValue)
                            {
                                contentBuilder.AppendLine("<color=#F44336>Yêu cầu nội lực: " + request.ReqPropValue + " điểm</color>");
                            }
                            else
                            {
                                contentBuilder.AppendLine("Yêu cầu nội lực: " + request.ReqPropValue + " điểm</color>");
                            }
                        }
                    }
                    else if (request.ReqPropType == (int)KE_ITEM_REQUIREMENT.emEQUIP_REQ_VIT) // Yêu cầu ngũ hành
                    {
                        if (request.ReqPropValue > 0)
                        {
                            if (role.m_wVitality < request.ReqPropValue)
                            {
                                contentBuilder.AppendLine("<color=#F44336>Yêu cầu thể lực: " + request.ReqPropValue + " điểm</color>");
                            }
                            else
                            {
                                contentBuilder.AppendLine("Yêu cầu thể lực: " + request.ReqPropValue + " điểm</color>");
                            }
                        }
                    }
                    /// Yêu cầu cấp độ
                    else if (request.ReqPropType == (int)KE_ITEM_REQUIREMENT.emEQUIP_REQ_LEVEL) // Yêu cầu cấp độ
                    {
                        if (role.Level < request.ReqPropValue)
                        {
                            contentBuilder.AppendLine("<color=#F44336>Yêu cầu cấp độ: " + request.ReqPropValue + "</color>");
                        }
                        else
                        {
                            contentBuilder.AppendLine("Yêu cầu cấp độ: " + request.ReqPropValue + "</color>");
                        }
                    }
                }
            }

            /// Nếu có phái tồn tại
            if (Loader.Loader.Factions.TryGetValue(requireFactionID, out FactionXML factionData))
            {
                /// Nếu môn phái không phù hợp
                if (requireFactionID != role.FactionID)
                {
                    contentBuilder.AppendLine("Yêu cầu phái: <color=#5d8bda>" + factionData.Name + "</color>");
                }
                else
                {
                    contentBuilder.AppendLine("Yêu cầu phái: " + factionData.Name + "");
                }

                /// Lấy nhánh tương ứng
                if (factionData.Subs.TryGetValue(requireRouteID, out FactionXML.Sub routeData))
                {
                    /// Tên nhánh yêu cầu
                    string routeName = routeData.Name;
                    /// Nếu nhánh của nhân vật khác nhánh yêu cầu
                    if (role.SubID != routeData.ID)
                    {
                        contentBuilder.AppendLine("Yêu cầu nhánh: <color=#5d8bda>" + routeName + "</color>");
                    }
                    else
                    {
                        contentBuilder.AppendLine("Yêu cầu nhánh: " + routeName + "");
                    }
                }

                /// Nếu có yêu cầu ngũ hành
                if (requireSeriesID != -1)
                {
                    /// Ngũ hành
                    int seriesID = (int)factionData.Elemental;
                    string requireSeriesName = KTGlobal.GetElementString((Elemental)requireSeriesID, out Color seriesColor);

                    /// Nếu không đúng ngũ hành
                    if (seriesID != requireSeriesID)
                    {
                        contentBuilder.AppendLine("Yêu cầu ngũ hành: <color=#5d8bda>" + requireSeriesName + "</color>");
                    }
                    else
                    {
                        contentBuilder.AppendLine("Yêu cầu ngũ hành: <color=#" + ColorUtility.ToHtmlStringRGB(seriesColor) + ">" + requireSeriesName + "</color>");
                    }
                }
            }

            return contentBuilder.ToString();
        }

        /// <summary>
        /// Trả về giá trị String kháng theo ngũ hành
        /// </summary>
        /// <param name="series"></param>
        /// <returns></returns>
        private static string GetResValue(Elemental series)
        {
            string resValue = "Kháng ngũ hành tương ứng";

            switch (series)
            {
                case Elemental.METAL:
                    resValue = "Kháng độc công";
                    break;

                case Elemental.WOOD:
                    resValue = "Kháng lôi công";
                    break;

                case Elemental.WATER:
                    resValue = "Kháng hỏa công";
                    break;

                case Elemental.FIRE:
                    resValue = "Kháng vật công";
                    break;

                case Elemental.EARTH:
                    resValue = "Kháng băng công";
                    break;
            }

            return resValue;
        }

        /// <summary>
        /// Trả về String vị trí trang bị
        /// </summary>
        /// <param name="inputType"></param>
        /// <returns></returns>
        private static string GetEquipPositionString(KE_EQUIP_POSITION inputType)
        {
            string ret = "";

            switch (inputType)
            {
                case KE_EQUIP_POSITION.emEQUIPPOS_HEAD:
                    {
                        ret = "Mũ";
                        break;
                    }
                case KE_EQUIP_POSITION.emEQUIPPOS_BODY:
                    {
                        ret = "Áo";
                        break;
                    }
                case KE_EQUIP_POSITION.emEQUIPPOS_RING:
                    {
                        ret = "Nhẫn Dưới";
                        break;
                    }
                case KE_EQUIP_POSITION.emEQUIPPOS_RING_2:
                    {
                        ret = "Nhẫn Trên";
                        break;
                    }
                case KE_EQUIP_POSITION.emEQUIPPOS_AMULET:
                    {
                        ret = "Liên - Phù";
                        break;
                    }
                case KE_EQUIP_POSITION.emEQUIPPOS_FOOT:
                    {
                        ret = "Giày";
                        break;
                    }
                case KE_EQUIP_POSITION.emEQUIPPOS_BELT:
                    {
                        ret = "Lưng";
                        break;
                    }
                case KE_EQUIP_POSITION.emEQUIPPOS_WEAPON:
                    {
                        ret = "Vũ Khí";
                        break;
                    }
                case KE_EQUIP_POSITION.emEQUIPPOS_CUFF:
                    {
                        ret = "Tay";
                        break;
                    }
                case KE_EQUIP_POSITION.emEQUIPPOS_PENDANT:
                    {
                        ret = "Bội";
                        break;
                    }
                case KE_EQUIP_POSITION.emEQUIPPOS_HORSE:
                    {
                        ret = "Ngựa";
                        break;
                    }
                case KE_EQUIP_POSITION.emEQUIPPOS_MASK:
                    {
                        ret = "Mặt Nạ";
                        break;
                    }
                case KE_EQUIP_POSITION.emEQUIPPOS_BOOK:
                    {
                        ret = "Mật Tịch";
                        break;
                    }
                case KE_EQUIP_POSITION.emEQUIPPOS_ORNAMENT:
                    {
                        ret = "Trang sức";
                        break;
                    }
                case KE_EQUIP_POSITION.emEQUIPPOS_SIGNET:
                    {
                        ret = "Ấn";
                        break;
                    }
                case KE_EQUIP_POSITION.emEQUIPPOS_MANTLE:
                    {
                        ret = "Phi Phong";
                        break;
                    }
                case KE_EQUIP_POSITION.emEQUIPPOS_CHOP:
                    {
                        ret = "Quan Ấn";
                        break;
                    }
                default:
                    {
                        ret = "Chưa mở";
                    }
                    break;
            }

            return ret;
        }

        /// <summary>
        /// Trả về tên ngũ hành tương sinh tương ứng
        /// </summary>
        /// <param name="series"></param>
        /// <returns></returns>
        private static string GetAccrueByName(Elemental series)
        {
            string resValue = "";

            switch (series)
            {
                case Elemental.METAL:
                    resValue = "(" + KTGlobal.GetSeriesText((int)Elemental.EARTH) + ")";
                    break;

                case Elemental.WOOD:
                    resValue = "(" + KTGlobal.GetSeriesText((int)Elemental.WATER) + ")";
                    break;

                case Elemental.WATER:
                    resValue = "(" + KTGlobal.GetSeriesText((int)Elemental.METAL) + ")";
                    break;

                case Elemental.FIRE:
                    resValue = "(" + KTGlobal.GetSeriesText((int)Elemental.WOOD) + ")";
                    break;

                case Elemental.EARTH:
                    resValue = "(" + KTGlobal.GetSeriesText((int)Elemental.FIRE) + ")";
                    break;
            }

            return resValue;
        }

        /// <summary>
        /// Trả về tên ngũ hành tương khắc tương ứng
        /// </summary>
        /// <param name="series"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        private static string GetSeriesEnhanceByName(Elemental series, out Color color)
        {
            string resValue = "";
            color = default;

            switch (series)
            {
                case Elemental.METAL:
                    resValue = KTGlobal.GetSeriesText((int)Elemental.WOOD);
                    KTGlobal.GetElementString(Elemental.WOOD, out color);
                    break;

                case Elemental.WOOD:
                    resValue = KTGlobal.GetSeriesText((int)Elemental.EARTH);
                    KTGlobal.GetElementString(Elemental.EARTH, out color);
                    break;

                case Elemental.WATER:
                    resValue = KTGlobal.GetSeriesText((int)Elemental.FIRE);
                    KTGlobal.GetElementString(Elemental.FIRE, out color);
                    break;

                case Elemental.FIRE:
                    resValue = KTGlobal.GetSeriesText((int)Elemental.METAL);
                    KTGlobal.GetElementString(Elemental.METAL, out color);
                    break;

                case Elemental.EARTH:
                    resValue = KTGlobal.GetSeriesText((int)Elemental.WATER);
                    KTGlobal.GetElementString(Elemental.WATER, out color);
                    break;
            }

            return resValue;
        }

        /// <summary>
        /// Trả về vị trí trang bị trên người theo loại trang bị
        /// </summary>
        /// <param name="equipType"></param>
        /// <returns></returns>
        private static KE_EQUIP_POSITION GetEquipPositionByEquipType(KE_ITEM_EQUIP_DETAILTYPE equipType)
        {
            switch (equipType)
            {
                case KE_ITEM_EQUIP_DETAILTYPE.equip_amulet:
                    {
                        return KE_EQUIP_POSITION.emEQUIPPOS_AMULET;
                    }
                case KE_ITEM_EQUIP_DETAILTYPE.equip_armor:
                    {
                        return KE_EQUIP_POSITION.emEQUIPPOS_BODY;
                    }
                case KE_ITEM_EQUIP_DETAILTYPE.equip_belt:
                    {
                        return KE_EQUIP_POSITION.emEQUIPPOS_BELT;
                    }
                case KE_ITEM_EQUIP_DETAILTYPE.equip_book:
                    {
                        return KE_EQUIP_POSITION.emEQUIPPOS_BOOK;
                    }
                case KE_ITEM_EQUIP_DETAILTYPE.equip_boots:
                    {
                        return KE_EQUIP_POSITION.emEQUIPPOS_FOOT;
                    }
                case KE_ITEM_EQUIP_DETAILTYPE.equip_chop:
                    {
                        return KE_EQUIP_POSITION.emEQUIPPOS_CHOP;
                    }
                case KE_ITEM_EQUIP_DETAILTYPE.equip_cuff:
                    {
                        return KE_EQUIP_POSITION.emEQUIPPOS_CUFF;
                    }
                case KE_ITEM_EQUIP_DETAILTYPE.equip_helm:
                    {
                        return KE_EQUIP_POSITION.emEQUIPPOS_HEAD;
                    }
                case KE_ITEM_EQUIP_DETAILTYPE.equip_horse:
                    {
                        return KE_EQUIP_POSITION.emEQUIPPOS_HORSE;
                    }
                case KE_ITEM_EQUIP_DETAILTYPE.equip_mantle:
                    {
                        return KE_EQUIP_POSITION.emEQUIPPOS_MANTLE;
                    }
                case KE_ITEM_EQUIP_DETAILTYPE.equip_mask:
                    {
                        return KE_EQUIP_POSITION.emEQUIPPOS_MASK;
                    }
                case KE_ITEM_EQUIP_DETAILTYPE.equip_meleeweapon:
                case KE_ITEM_EQUIP_DETAILTYPE.equip_rangeweapon:
                    {
                        return KE_EQUIP_POSITION.emEQUIPPOS_WEAPON;
                    }

                case KE_ITEM_EQUIP_DETAILTYPE.equip_pendant:
                    {
                        return KE_EQUIP_POSITION.emEQUIPPOS_PENDANT;
                    }
                case KE_ITEM_EQUIP_DETAILTYPE.equip_ring:
                    {
                        return KE_EQUIP_POSITION.emEQUIPPOS_RING;
                    }
                case KE_ITEM_EQUIP_DETAILTYPE.equip_signet:
                    {
                        return KE_EQUIP_POSITION.emEQUIPPOS_SIGNET;
                    }
                case KE_ITEM_EQUIP_DETAILTYPE.equip_ornament:
                    {
                        return KE_EQUIP_POSITION.emEQUIPPOS_ORNAMENT;
                    }
                default:
                    {
                        return KE_EQUIP_POSITION.emEQUIPPOS_WEAPON;
                    }
            }
        }

        /// <summary>
        /// Trả về tên ngũ hành bị khắc tương ứng
        /// </summary>
        /// <param name="series"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        private static string GetSeriesConqueByName(Elemental series, out Color color)
        {
            string resValue = "";
            color = default;

            switch (series)
            {
                case Elemental.METAL:
                    resValue = KTGlobal.GetSeriesText((int)Elemental.FIRE);
                    KTGlobal.GetElementString(Elemental.FIRE, out color);
                    break;

                case Elemental.WOOD:
                    resValue = KTGlobal.GetSeriesText((int)Elemental.METAL);
                    KTGlobal.GetElementString(Elemental.METAL, out color);
                    break;

                case Elemental.WATER:
                    resValue = KTGlobal.GetSeriesText((int)Elemental.EARTH);
                    KTGlobal.GetElementString(Elemental.EARTH, out color);
                    break;

                case Elemental.FIRE:
                    resValue = KTGlobal.GetSeriesText((int)Elemental.WATER);
                    KTGlobal.GetElementString(Elemental.WATER, out color);
                    break;

                case Elemental.EARTH:
                    resValue = KTGlobal.GetSeriesText((int)Elemental.WOOD);
                    KTGlobal.GetElementString(Elemental.WOOD, out color);
                    break;
            }

            return resValue;
        }

        /// <summary>
        /// Xây Tooltip thuộc tính ẩn
        /// </summary>
        /// <param name="props"></param>
        /// <param name="itemData"></param>
        /// <param name="series"></param>
        /// <param name="enhanceLevel"></param>
        /// <param name="isactive_all_ornament"></param>
        /// <returns></returns>
        private static string InitEquipHiddenProbs(string props, ItemData itemData, int series, int enhanceLevel, bool isactive_all_ornament = false, int PostionUsing = -1, bool active_suit = false)
        {
            StringBuilder contentBuilder = new StringBuilder();
            /// Nếu vật phẩm không tồn tại
            if (itemData == null)
            {
                return "";
            }

            /// Nếu không có thuộc tính thiết lập
            if (string.IsNullOrEmpty(props) || props.Contains("ERORR"))
            {
                /// Danh sách thuộc tính ẩn
                List<PropMagic> emptyProps = itemData.HiddenProp?.OrderBy(x => x.Index)?.ToList();

                if (emptyProps != null)
                {
                    int nPlace = Loader.Loader.g_anEquipPos[itemData.DetailType];

                    // Nếu đây là nhẫn thì xác định xem nó là nhẫn trên hay nhẫn dưới
                    if (nPlace == (int)KE_EQUIP_POSITION.emEQUIPPOS_RING)
                    {
                        if (PostionUsing == (int)KE_EQUIP_POSITION.emEQUIPPOS_RING_2)
                        {
                            nPlace = (int)KE_EQUIP_POSITION.emEQUIPPOS_RING_2;
                        }
                    }

                    if (Loader.Loader.g_anEquipActive.TryGetValue((KE_EQUIP_POSITION)nPlace, out ActiveByItem activeCheck))
                    {
                        int Active = 0;

                        int Postion1 = activeCheck.Pos1;

                        int Postion2 = activeCheck.Pos2;

                        int PositionSeri1 = KTGlobal.GetItemSeriesByPostion(Postion1);

                        int PositionSeri2 = KTGlobal.GetItemSeriesByPostion(Postion2);

                        string BuildPos1 = KTGlobal.GetEquipPositionString((KE_EQUIP_POSITION)Postion1) + " " + GetAccrueByName((Elemental)series);

                        if (Loader.Loader.g_IsAccrue(PositionSeri1, series))
                        {
                            Active++;
                            BuildPos1 = "<color=#ffff05>" + Utils.RemoveAllHTMLTags(BuildPos1) + "</color>";
                        }
                        else
                        {
                            BuildPos1 = "<color=#c2c2c2>" + Utils.RemoveAllHTMLTags(BuildPos1) + "</color>";
                        }
                        string BuildPos2 = KTGlobal.GetEquipPositionString((KE_EQUIP_POSITION)Postion2) + " " + GetAccrueByName((Elemental)series);

                        if (Loader.Loader.g_IsAccrue(PositionSeri2, series))
                        {
                            Active++;
                            BuildPos2 = "<color=#ffff05>" + Utils.RemoveAllHTMLTags(BuildPos2) + "</color>";
                        }
                        else
                        {
                            BuildPos2 = "<color=#c2c2c2>" + Utils.RemoveAllHTMLTags(BuildPos2) + "</color>";
                        }

                        // Check kích hoạt ngũ hành nhân vật
                        RoleData role = Global.Data.RoleData;

                        int PlayerSeri = (int)Elemental.NONE;
                        string BuildPos3 = "Nhân vật " + GetAccrueByName((Elemental)series);

                        if (Loader.Loader.g_IsAccrue(PlayerSeri, series))
                        {
                            Active++;
                            BuildPos3 = "<color=#ffff05>" + Utils.RemoveAllHTMLTags(BuildPos3) + "</color>";
                        }
                        else
                        {
                            BuildPos3 = "<color=#c2c2c2>" + Utils.RemoveAllHTMLTags(BuildPos3) + "</color>";
                        }

                        contentBuilder.AppendLine(string.Format("<color=#057aff>Kích hoạt ngũ hành: ({0}/{1})</color>", 0, itemData.HiddenProp.Count));
                        contentBuilder.AppendLine(BuildPos1 + " - " + BuildPos2 + " - " + BuildPos3);

                        /// Duyệt danh sách thuộc tính ẩn
                        foreach (PropMagic prop in emptyProps)
                        {
                            if (prop.MagicLevel.Contains('|'))
                            {
                                int MinValue = Int32.Parse(prop.MagicLevel.Split('|')[0]);
                                int MaxValue = Int32.Parse(prop.MagicLevel.Split('|')[1]);

                                int levelDescmin = MinValue + enhanceLevel;
                                int levelDescmax = MaxValue + enhanceLevel;

                                MagicAttribLevel magicAttribmin = Loader.Loader.MagicAttribLevels.Where(x => x.MagicName == prop.MagicName && x.Level == levelDescmin).FirstOrDefault();
                                MagicAttribLevel magicAttribmax = Loader.Loader.MagicAttribLevels.Where(x => x.MagicName == prop.MagicName && x.Level == levelDescmax).FirstOrDefault();

                                if (magicAttribmin != null && magicAttribmax != null)
                                {
                                    if (prop.MagicName == "damage_series_resist")
                                    {
                                        string result = string.Format("<color=#c2c2c2>{0}: {1} đến {2}</color>", KTGlobal.GetResValue((Elemental)series), magicAttribmin.MA1Min.AttributeToString(), magicAttribmax.MA1Max);
                                        contentBuilder.AppendLine(result);
                                    }
                                    else
                                    {
                                        if (PropertyDefine.PropertiesBySymbolName.TryGetValue(prop.MagicName, out PropertyDefine.Property property))
                                        {
                                            string result = string.Format("<color=#c2c2c2>" + property.Description + "</color>", string.Format("{0} đến {1}", magicAttribmin.MA1Min.AttributeToString(), magicAttribmax.MA1Max), Utils.Truncate(magicAttribmin.MA2Min / 18f, 1), magicAttribmax.MA3Min);
                                            contentBuilder.AppendLine(result);
                                        }
                                        else
                                        {
                                            contentBuilder.AppendLine("Symbol Not Found :" + prop.MagicName);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                int levelDescmin = Int32.Parse(prop.MagicLevel) + enhanceLevel;
                                /// Thuộc tính tương ứng
                                MagicAttribLevel magicAttrib = Loader.Loader.MagicAttribLevels.Where(x => x.MagicName == prop.MagicName && x.Level == levelDescmin).FirstOrDefault();
                                if (magicAttrib != null)
                                {
                                    if (prop.MagicName == "damage_series_resist")
                                    {
                                        string result = string.Format("<color=#c2c2c2>{0}: {1} đến {2}</color>", KTGlobal.GetResValue((Elemental)series), magicAttrib.MA1Min.AttributeToString(), magicAttrib.MA1Max);
                                        contentBuilder.AppendLine(result);
                                    }
                                    else
                                    {
                                        if (PropertyDefine.PropertiesBySymbolName.TryGetValue(prop.MagicName, out PropertyDefine.Property property))
                                        {
                                            string result = string.Format("<color=#c2c2c2>" + property.Description + "</color>", string.Format("{0} đến {1}", magicAttrib.MA1Min.AttributeToString(), magicAttrib.MA1Max), Utils.Truncate(magicAttrib.MA2Min / 18f, 1), magicAttrib.MA3Min);
                                            contentBuilder.AppendLine(result);
                                        }
                                        else
                                        {
                                            contentBuilder.AppendLine("Symbol Not Found :" + prop.MagicName);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                byte[] base64Decode = Convert.FromBase64String(props);

                // BYTE CODE ITEM
                ItemDataByteCode equipProp = DataHelper.BytesToObject<ItemDataByteCode>(base64Decode, 0, base64Decode.Length);

                // bắt đầu xử lý chuỗi
                if (equipProp != null && equipProp.HiddenProbsCount > 0)
                {
                    int nPlace = Loader.Loader.g_anEquipPos[itemData.DetailType];

                    // Nếu đây là nhẫn thì xác định xem nó là nhẫn trên hay nhẫn dưới
                    if (nPlace == (int)KE_EQUIP_POSITION.emEQUIPPOS_RING)
                    {
                        if (PostionUsing == (int)KE_EQUIP_POSITION.emEQUIPPOS_RING_2)
                        {
                            nPlace = (int)KE_EQUIP_POSITION.emEQUIPPOS_RING_2;
                        }
                    }
                    // Đẩy vào xem có ok không
                    if (Loader.Loader.g_anEquipActive.TryGetValue((KE_EQUIP_POSITION)nPlace, out ActiveByItem activeCheck))
                    {
                        int Active = 0;

                        int Postion1 = activeCheck.Pos1;

                        int Postion2 = activeCheck.Pos2;

                        int PositionSeri1 = KTGlobal.GetItemSeriesByPostion(Postion1);

                        int PositionSeri2 = KTGlobal.GetItemSeriesByPostion(Postion2);

                        string BuildPos1 = KTGlobal.GetEquipPositionString((KE_EQUIP_POSITION)Postion1) + " " + GetAccrueByName((Elemental)series);

                        //KTDebug.LogError("ISALL ACTIVE :" + active_suit.ToString());
                        // Nếu như đồ có quan hệ tương sinh hoặc là trang sức có dòng kích hoạt all thuộc tính trang sức hoặc kích hoạt tất cả dòng ẩn
                        if (Loader.Loader.g_IsAccrue(PositionSeri1, series) || (KD_ISORNAMENT(itemData.DetailType) && isactive_all_ornament == true) || active_suit)
                        {
                            Active++;
                            BuildPos1 = "<color=#ffff05>" + Utils.RemoveAllHTMLTags(BuildPos1) + "</color>";
                        }
                        else
                        {
                            BuildPos1 = "<color=#c2c2c2>" + Utils.RemoveAllHTMLTags(BuildPos1) + "</color>";
                        }
                        string BuildPos2 = KTGlobal.GetEquipPositionString((KE_EQUIP_POSITION)Postion2) + " " + GetAccrueByName((Elemental)series);

                        if (Loader.Loader.g_IsAccrue(PositionSeri2, series) || (KD_ISORNAMENT(itemData.DetailType) && isactive_all_ornament == true) || active_suit)
                        {
                            Active++;
                            BuildPos2 = "<color=#ffff05>" + Utils.RemoveAllHTMLTags(BuildPos2) + "</color>";
                        }
                        else
                        {
                            BuildPos2 = "<color=#c2c2c2>" + Utils.RemoveAllHTMLTags(BuildPos2) + "</color>";
                        }

                        // Check kích hoạt ngũ hành nhân vật
                        RoleData role = Global.Data.RoleData;

                        int PlayerSeri = (int)Elemental.NONE;

                        if (Loader.Loader.Factions.TryGetValue(role.FactionID, out FactionXML factionData))
                        {
                            PlayerSeri = (int)factionData.Elemental;
                        }

                        string BuildPos3 = "Nhân vật " + GetAccrueByName((Elemental)series);

                        if (Loader.Loader.g_IsAccrue(PlayerSeri, series) || (KD_ISORNAMENT(itemData.DetailType) && isactive_all_ornament == true) || active_suit)
                        {
                            Active++;
                            BuildPos3 = "<color=#ffff05>" + Utils.RemoveAllHTMLTags(BuildPos3) + "</color>";
                        }
                        else
                        {
                            BuildPos3 = "<color=#c2c2c2>" + Utils.RemoveAllHTMLTags(BuildPos3) + "</color>";
                        }

                        contentBuilder.AppendLine(string.Format("<color=#057aff>Kích hoạt ngũ hành: ({0}/{1})</color>", Active, equipProp.HiddenProbsCount));
                        contentBuilder.AppendLine(BuildPos1 + " - " + BuildPos2 + " - " + BuildPos3);

                        /// Nếu tồn tại danh sách thuộc tính ẩn
                        if (equipProp.HiddenProbsCount > 0)
                        {
                            // Duyệt 1 vòng tất cả các thuộc tính ẩn
                            foreach (KMagicInfo KmagicInfo in equipProp.HiddenProbs)
                            {
                                /// Nếu là cộng kỹ năng
                                if (KmagicInfo.nAttribType == (int)MAGIC_ATTRIB.magic_skilladdition_addskilllevel)
                                {
                                    /// Kỹ năng
                                    int skillID = KmagicInfo.Value_1;
                                    /// Cấp độ kỹ năng
                                    int skillLevel = KmagicInfo.Value_2;
                                    /// Thông tin kỹ năng
                                    if (Loader.Loader.Skills.TryGetValue(skillID, out SkillDataEx skillData))
                                    {
                                        /// Nếu đã kích hoạt
                                        if (Active > 0 || (KD_ISORNAMENT(itemData.DetailType) && isactive_all_ornament == true) || active_suit)
                                        {
                                            contentBuilder.AppendLine(string.Format("<color=#2df600>Kỹ năng <color=orange>[{0}]</color>: <color=#ffee38>+{1}</color></color>", skillData.Name, skillLevel));
                                        }
                                        /// Nếu chưa kích hoạt
                                        else
                                        {
                                            contentBuilder.AppendLine(string.Format("<color=#c2c2c2>Kỹ năng [{0}]: +{1}</color>", skillData.Name, skillLevel));
                                        }
                                    }
                                }
                                else
                                {
                                    int value0 = 0, value1 = 0, value2 = 0;

                                    // level dịch của magic level = level random của đồ + cấp độ cường hóa
                                    int LevelFinal = KmagicInfo.nLevel + enhanceLevel;

                                    MagicAttribLevel FindOrginalValue = Loader.Loader.MagicAttribLevels.Where(x => x.MAGIC_ID == KmagicInfo.nAttribType && x.Level == KmagicInfo.nLevel).FirstOrDefault();

                                    /// Thuộc tính tương ứng
                                    MagicAttribLevel magicAttrib = Loader.Loader.MagicAttribLevels.Where(x => x.MAGIC_ID == KmagicInfo.nAttribType && x.Level == LevelFinal).FirstOrDefault();

                                    if (magicAttrib != null)
                                    {
                                        if (KmagicInfo.Value_1 != -1)
                                        {
                                            int Percent = RecaculationPercent(FindOrginalValue.MA1Min, FindOrginalValue.MA1Max, KmagicInfo.Value_1);
                                            if (Percent > 0)
                                            {
                                                int AddValue = ((magicAttrib.MA1Max - magicAttrib.MA1Min) * Percent) / 100;

                                                value0 = magicAttrib.MA1Min + AddValue;
                                            }
                                            else
                                            {
                                                value0 = magicAttrib.MA1Min;
                                            }
                                        }
                                        if (KmagicInfo.Value_2 != -1)
                                        {
                                            int Percent1 = RecaculationPercent(FindOrginalValue.MA2Min, FindOrginalValue.MA2Max, KmagicInfo.Value_2);

                                            if (Percent1 > 0)
                                            {
                                                int AddValue = ((magicAttrib.MA2Max - magicAttrib.MA2Min) * Percent1) / 100;

                                                value1 = magicAttrib.MA2Min + AddValue;
                                            }
                                            else
                                            {
                                                value1 = magicAttrib.MA2Min;
                                            }
                                        }
                                        if (KmagicInfo.Value_3 != -1)
                                        {
                                            int Percent2 = RecaculationPercent(FindOrginalValue.MA3Min, FindOrginalValue.MA3Max, KmagicInfo.Value_3);

                                            if (Percent2 > 0)
                                            {
                                                int AddValue = ((magicAttrib.MA3Max - magicAttrib.MA3Min) * Percent2) / 100;

                                                value2 = magicAttrib.MA3Min + AddValue;
                                            }
                                            else
                                            {
                                                value2 = magicAttrib.MA3Min;
                                            }
                                        }

                                        if (FindOrginalValue.MagicName == "damage_series_resist")
                                        {
                                            if (Active > 0 || (KD_ISORNAMENT(itemData.DetailType) && isactive_all_ornament == true) || active_suit)
                                            {
                                                string result = string.Format("<color=#2df600>{0}: {1}</color> <color=#ff4de4>({2} - {3})</color>", KTGlobal.GetResValue((Elemental)series), value0.AttributeToString(), magicAttrib.MA1Min, magicAttrib.MA1Max);
                                                contentBuilder.AppendLine(result);
                                                Active--;
                                            }
                                            else
                                            {
                                                string result = string.Format("<color=#c2c2c2>{0}: {1}</color> <color=#ff4de4>({2} - {3})</color>", KTGlobal.GetResValue((Elemental)series), value0.AttributeToString(), magicAttrib.MA1Min, magicAttrib.MA1Max);
                                                contentBuilder.AppendLine(result);
                                            }
                                        }
                                        else
                                        {
                                            if (PropertyDefine.PropertiesByID.TryGetValue(KmagicInfo.nAttribType, out PropertyDefine.Property property))
                                            {
                                                string result = string.Format(property.Description, property.SymbolName.Contains("_resisttime") || property.SymbolName.Contains("_resistrate") ? (-value0).AttributeToString() : value0.AttributeToString(), Utils.Truncate(value1 / 18f, 1), value2);

                                                if (Active > 0 || (KD_ISORNAMENT(itemData.DetailType) && isactive_all_ornament == true) || active_suit)
                                                {
                                                    Active--;
                                                    contentBuilder.AppendLine("<color=#2df600>" + result + "</color>" + " " + string.Format("<color=#ff4de4>({0} - {1})</color>", magicAttrib.MA1Min, magicAttrib.MA1Max));
                                                }
                                                else
                                                {
                                                    contentBuilder.AppendLine("<color=#c2c2c2>" + Utils.RemoveAllHTMLTags(result) + "</color>" + " " + string.Format("<color=#ff4de4>({0} - {1})</color>", magicAttrib.MA1Min, magicAttrib.MA1Max));
                                                }
                                            }
                                            else
                                            {
                                                contentBuilder.AppendLine("Symbol Not Found :" + KmagicInfo.nAttribType);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        contentBuilder.AppendLine("Symbol Not Found :" + KmagicInfo.nAttribType);
                                    }
                                }
                            }
                            /// Danh sách thuộc tính ẩn
                        }
                    }
                }
            }

            return contentBuilder.ToString();
        }

        /// <summary>
        /// Thuộc tính bộ
        /// </summary>
        /// <param name="props"></param>
        /// <param name="itemData"></param>
        /// <returns></returns>
        private static string InitActiveSuite(ItemData itemData)
        {


            StringBuilder contentBuilder = new StringBuilder();
            /// Nếu vật phẩm không tồn tại
            if (itemData == null)
            {
                return "";
            }

            /// Nếu có thuộc tính bộ
            if (itemData.SuiteID > 0)
            {
                /// Thuộc tính bộ
                SuiteActiveProp emptyProps = Loader.Loader.SuiteActiveProps.Where(x => x.SuiteID == itemData.SuiteID).FirstOrDefault();
                /// Nếu không tồn tại
                if (emptyProps == null)
                {
                    return "";
                }
                /// Tổng số kích hoạt
                int totalActive = KTGlobal.CountAllSuiteID(itemData.SuiteID);

                int readActive = totalActive;

                contentBuilder.AppendLine("");

                contentBuilder.AppendLine("<color=#6060ff>" + emptyProps.Name + "(" + totalActive + "/" + emptyProps.ListActive.Max(x => x.RequestNum) + ")</color>");

                if (emptyProps.Head != "")
                {
                    if (totalActive > 0 && GetActiveSuteIDByPos(itemData.SuiteID, (int)KE_EQUIP_POSITION.emEQUIPPOS_HEAD))
                    {
                        totalActive--;
                        contentBuilder.AppendLine("<color=#fff200>" + emptyProps.Head + "</color>");
                    }
                    else
                    {
                        contentBuilder.AppendLine("<color=#c2c2c2>" + emptyProps.Head + "</color>");
                    }
                }

                if (emptyProps.Body != "")
                {
                    if (totalActive > 0 && GetActiveSuteIDByPos(itemData.SuiteID, (int)KE_EQUIP_POSITION.emEQUIPPOS_BODY))
                    {
                        totalActive--;
                        contentBuilder.AppendLine("<color=#fff200>" + emptyProps.Body + "</color>");
                    }
                    else
                    {
                        contentBuilder.AppendLine("<color=#c2c2c2>" + emptyProps.Body + "</color>");
                    }
                }

                if (emptyProps.Belt != "")
                {
                    if (totalActive > 0 && GetActiveSuteIDByPos(itemData.SuiteID, (int)KE_EQUIP_POSITION.emEQUIPPOS_BELT))
                    {
                        totalActive--;
                        contentBuilder.AppendLine("<color=#fff200>" + emptyProps.Belt + "</color>");
                    }
                    else
                    {
                        contentBuilder.AppendLine("<color=#c2c2c2>" + emptyProps.Belt + "</color>");
                    }
                }

                if (emptyProps.Weapon != "")
                {
                    if (totalActive > 0 && GetActiveSuteIDByPos(itemData.SuiteID, (int)KE_EQUIP_POSITION.emEQUIPPOS_WEAPON))
                    {
                        totalActive--;
                        contentBuilder.AppendLine("<color=#fff200>" + emptyProps.Weapon + "</color>");
                    }
                    else
                    {
                        contentBuilder.AppendLine("<color=#c2c2c2>" + emptyProps.Weapon + "</color>");
                    }
                }

                if (emptyProps.Foot != "")
                {
                    if (totalActive > 0 && GetActiveSuteIDByPos(itemData.SuiteID, (int)KE_EQUIP_POSITION.emEQUIPPOS_FOOT))
                    {
                        totalActive--;
                        contentBuilder.AppendLine("<color=#fff200>" + emptyProps.Foot + "</color>");
                    }
                    else
                    {
                        contentBuilder.AppendLine("<color=#c2c2c2>" + emptyProps.Foot + "</color>");
                    }
                }

                if (emptyProps.Cuff != "")
                {
                    if (totalActive > 0 && GetActiveSuteIDByPos(itemData.SuiteID, (int)KE_EQUIP_POSITION.emEQUIPPOS_CUFF))
                    {
                        totalActive--;
                        contentBuilder.AppendLine("<color=#fff200>" + emptyProps.Cuff + "</color>");
                    }
                    else
                    {
                        contentBuilder.AppendLine("<color=#c2c2c2>" + emptyProps.Cuff + "</color>");
                    }
                }

                if (emptyProps.Amulet != "")
                {
                    if (totalActive > 0 && GetActiveSuteIDByPos(itemData.SuiteID, (int)KE_EQUIP_POSITION.emEQUIPPOS_AMULET))
                    {
                        totalActive--;
                        contentBuilder.AppendLine("<color=#fff200>" + emptyProps.Amulet + "</color>");
                    }
                    else
                    {
                        contentBuilder.AppendLine("<color=#c2c2c2>" + emptyProps.Amulet + "</color>");
                    }
                }

                if (emptyProps.Ring != "")
                {
                    if (totalActive > 0 && GetActiveSuteIDByPos(itemData.SuiteID, (int)KE_EQUIP_POSITION.emEQUIPPOS_RING))
                    {
                        totalActive--;
                        contentBuilder.AppendLine("<color=#fff200>" + emptyProps.Ring + "</color>");
                    }
                    else
                    {
                        contentBuilder.AppendLine("<color=#c2c2c2>" + emptyProps.Ring + "</color>");
                    }
                }

                if (emptyProps.Necklace != "")
                {
                    if (totalActive > 0 && GetActiveSuteIDByPos(itemData.SuiteID, (int)KE_EQUIP_POSITION.emEQUIPPOS_RING_2))
                    {
                        totalActive--;
                        contentBuilder.AppendLine("<color=#fff200>" + emptyProps.Necklace + "</color>");
                    }
                    else
                    {
                        contentBuilder.AppendLine("<color=#c2c2c2>" + emptyProps.Necklace + "</color>");
                    }
                }

                if (emptyProps.Pendant != "")
                {
                    if (totalActive > 0 && GetActiveSuteIDByPos(itemData.SuiteID, (int)KE_EQUIP_POSITION.emEQUIPPOS_PENDANT))
                    {
                        totalActive--;
                        contentBuilder.AppendLine("<color=#fff200>" + emptyProps.Pendant + "</color>");
                    }
                    else
                    {
                        contentBuilder.AppendLine("<color=#c2c2c2>" + emptyProps.Pendant + "</color>");
                    }
                }

                // Duyệt CMN1 VÒNG
                foreach (SuiteActive _SUTEACTIVE in emptyProps.ListActive)
                {
                    int NUMBERREQUEST = _SUTEACTIVE.RequestNum;
                    if (readActive >= NUMBERREQUEST)
                    {


                        string DESC = "";

                        if (PropertyDefine.PropertiesBySymbolName.TryGetValue(_SUTEACTIVE.SuiteName, out PropertyDefine.Property property))
                        {
                            DESC = string.Format(property.Description, _SUTEACTIVE.SuiteName.Contains("_resisttime") || _SUTEACTIVE.SuiteName.Contains("_resistrate") ? (-_SUTEACTIVE.SuiteMAPA1).AttributeToString() : _SUTEACTIVE.SuiteMAPA1.AttributeToString(), _SUTEACTIVE.SuiteMAPA2, _SUTEACTIVE.SuiteMAPA3);
                            contentBuilder.AppendLine("<color=#2df600>(" + _SUTEACTIVE.RequestNum + " cái)" + DESC + "</color>");
                        }
                    }
                    else
                    {
                        string DESC = "";

                        if (PropertyDefine.PropertiesBySymbolName.TryGetValue(_SUTEACTIVE.SuiteName, out PropertyDefine.Property property))
                        {
                            DESC = string.Format(property.Description, _SUTEACTIVE.SuiteName.Contains("_resisttime") || _SUTEACTIVE.SuiteName.Contains("_resistrate") ? (-_SUTEACTIVE.SuiteMAPA1).AttributeToString() : _SUTEACTIVE.SuiteMAPA1.AttributeToString(), _SUTEACTIVE.SuiteMAPA2, _SUTEACTIVE.SuiteMAPA3);
                            contentBuilder.AppendLine("<color=#c2c2c2>(" + _SUTEACTIVE.RequestNum + " cái)" + DESC + "</color>");
                        }
                    }
                }
            }

            return contentBuilder.ToString();
        }

        public static int GetItemSeriesByPostion(int InputPos)
        {
            int SuiteID = 0;
            if (null == Global.Data.RoleData.GoodsDataList)
            {
                return SuiteID;
            }
            foreach (GoodsData _Data in Global.Data.RoleData.GoodsDataList)
            {
                if (_Data.Using == InputPos)
                {
                    /// Nếu thông tin trang bị không tồn tại
                    if (!Loader.Loader.Items.TryGetValue(_Data.GoodsID, out ItemData ItemTemplate))
                    {
                        continue;
                    }
                    else
                    {
                        return _Data.Series;
                    }
                }
            }

            return SuiteID;
        }

        /// <summary>
        /// Trả về vị trí tương ứng có bộ phận của bộ này hayu ko
        /// </summary>
        /// <param name="InputSuite"></param>
        /// <returns></returns>
        public static bool GetActiveSuteIDByPos(int InputSuite, int Postion)
        {
            if (null == Global.Data.RoleData.GoodsDataList)
            {
                return false;
            }
            foreach (GoodsData _Data in Global.Data.RoleData.GoodsDataList)
            {
                if (_Data.Using == Postion)
                {
                    /// Nếu thông tin trang bị không tồn tại
                    if (!Loader.Loader.Items.TryGetValue(_Data.GoodsID, out ItemData ItemTemplate))
                    {
                        continue;
                    }
                    else
                    {
                        int SuiteItem = ItemTemplate.SuiteID;

                        if (InputSuite == SuiteItem)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Xây Tooltip thuộc tính cường hóa
        /// </summary>
        /// <param name="_GoodDatas"></param>
        /// <param name="_ItemData"></param>
        /// <param name="contentBuilder"></param>
        private static string InitEquipEnhance(GoodsData itemGD, ItemData itemData)
        {
            StringBuilder contentBuilder = new StringBuilder();

            /// Nếu trang bị không thể cường hóa được
            if (!KTGlobal.CanEquipEnhance(itemData))
            {
                return "";
            }
            /// Nếu không có dữ liệu vật phẩm
            if (itemData == null)
            {
                return "";
            }

            /// Cấp cường hóa
            int enhanceLevel = itemGD.Forge_level;
            List<ENH> enhanceProps = itemData.ListEnhance;

            /// Nếu thuộc tính cường hóa không tồn tại
            if (enhanceProps == null)
            {
                return "";
            }

            int nMax = enhanceProps.Max(x => x.EnhTimes);
            contentBuilder.AppendLine("");
            contentBuilder.AppendLine("<color=#057aff>Cường hóa: (" + enhanceLevel + "/" + nMax + ")</color>");

            /// Nếu có danh sách thuộc tính cường hóa
            if (enhanceProps.Count > 0)
            {
                /// Duyệt danh sách thuộc tính cường hóa
                foreach (ENH prop in enhanceProps)
                {
                    if (PropertyDefine.PropertiesBySymbolName.TryGetValue(prop.EnhMAName, out PropertyDefine.Property property))
                    {
                        string result;
                        if (prop.EnhMAName == "ignoreresist_p")
                        {
                            int series = itemGD.Series;
                            result = string.Format(property.Description, KTGlobal.GetSeriesText(series), prop.EnhMAPA1Min.AttributeToString());
                        }
                        else
                        {
                            result = string.Format(property.Description, prop.EnhMAName.Contains("_resisttime") || prop.EnhMAName.Contains("_resistrate") ? (-prop.EnhMAPA1Min).AttributeToString() : prop.EnhMAPA1Min.AttributeToString(), Utils.Truncate(prop.EnhMAPA2Min / 18f, 1), prop.EnhMAPA3Min);
                        }

                        if (enhanceLevel >= prop.EnhTimes)
                        {
                            contentBuilder.AppendLine("<color=#2df600>(+" + prop.EnhTimes + ") " + (prop.EnhTimes < 10 && nMax >= 10 ? "  " : "") + result + "</color>");
                        }
                        else
                        {
                            contentBuilder.AppendLine("<color=#c2c2c2>(+" + prop.EnhTimes + ") " + (prop.EnhTimes < 10 && nMax >= 10 ? "  " : "") + Utils.RemoveAllHTMLTags(result) + "</color>");
                        }
                    }
                    else
                    {
                        contentBuilder.AppendLine("Symbol Not Found :" + prop.EnhMAName);
                    }
                }
            }

            return contentBuilder.ToString();
        }

        public static int RecaculationPercent(int MinValue, int MaxValue, int CurenValue)
        {
            if (MinValue == MaxValue)
            {
                return 0;
            }
            return (CurenValue * 100) / (MaxValue - MinValue);
        }

        /// <summary>
        /// Trả về danh sách trang bị yêu cầu để kích hoạt trang bị đang đeo
        /// TRả về hệ tương sinh của trang bị cần để kích hoạt trang bị
        /// </summary>
        /// <param name="props"></param>
        /// <param name="itemData"></param>
        /// <param name="series"></param>
        /// <param name="PostionUsing"></param>
        /// <returns></returns>
        public static string InitRequestActiveItem(string props, ItemData itemData, int series, int PostionUsing = -1)
        {
            StringBuilder contentBuilder = new StringBuilder();

            int nPlace = Loader.Loader.g_anEquipPos[itemData.DetailType];

            // Nếu đây là nhẫn thì xác định xem nó là nhẫn trên hay nhẫn dưới
            if (nPlace == (int)KE_EQUIP_POSITION.emEQUIPPOS_RING)
            {
                if (PostionUsing == (int)KE_EQUIP_POSITION.emEQUIPPOS_RING_2)
                {
                    nPlace = (int)KE_EQUIP_POSITION.emEQUIPPOS_RING_2;
                }
            }

            return contentBuilder.ToString();
        }

        /// <summary>
        /// Xây Tooltip các thuộc tính cơ bản
        /// </summary>
        /// <param name="props"></param>
        /// <param name="itemData"></param>
        /// <param name="series"></param>
        /// <param name="enhanceLevel"></param>
        private static string InitEquipBaseAttributes(string props, ItemData itemData, int series, int enhanceLevel)
        {
            StringBuilder contentBuilder = new StringBuilder();

            /// Nếu vật phẩm không tồn tại
            if (itemData == null)
            {
                return "";
            }

            /// Nếu không có thuộc tính sinh sẵn | tức là lấy trong template
            if (string.IsNullOrEmpty(props) || props.Contains("ERORR"))
            {
                {
                    //contentBuilder.AppendLine("<color=#057aff>Thuộc tính cơ bản:</color>");

                    /// Danh sách thuộc tính ẩn
                    List<BasicProp> emptyProps = itemData.ListBasicProp?.OrderBy(x => x.Index)?.ToList();
                    if (emptyProps != null)
                    {
                        /// Duyệt danh sách thuộc tính ẩn
                        foreach (BasicProp prop in emptyProps)
                        {
                            // bỏ qua symboy này vì đã ghi lên thuộc tính bên trên
                            if (prop.BasicPropType == "durability_v")
                            {
                                continue;
                            }
                            if (prop.BasicPropType == "damage_series_resist")
                            {
                                string result = string.Format("<color=#05ffe6>{0}: {1}</color>", KTGlobal.GetResValue((Elemental)series), prop.BasicPropPA1Min.AttributeToString());
                                contentBuilder.AppendLine(result);
                            }
                            else
                            {
                                if (PropertyDefine.PropertiesBySymbolName.TryGetValue(prop.BasicPropType, out PropertyDefine.Property property))
                                {
                                    string result = string.Format("<color=#05ffe6>" + property.Description + "</color>", prop.BasicPropPA1Min.AttributeToString(), Utils.Truncate(prop.BasicPropPA2Min / 18f, 1), prop.BasicPropPA3Min);
                                    contentBuilder.AppendLine(result);
                                }
                                else
                                {
                                    contentBuilder.AppendLine("Symbol Not Found :" + prop.BasicPropType);
                                }
                            }
                        }
                    }
                }

                {
                    contentBuilder.AppendLine();
                    //contentBuilder.AppendLine("<color=#057aff>Thuộc tính ngẫu nhiên:</color>");

                    /// Danh sách thuộc tính ẩn
                    List<PropMagic> emptyProps = itemData.GreenProp?.OrderBy(x => x.Index)?.ToList();
                    if (emptyProps != null)
                    {
                        /// Duyệt danh sách thuộc tính ẩn
                        foreach (PropMagic prop in emptyProps)
                        {
                            if (prop.MagicLevel.Contains("|"))
                            {
                                int MINVALUE = Int32.Parse(prop.MagicLevel.Split('|')[0]);
                                int MAXVALUE = Int32.Parse(prop.MagicLevel.Split('|')[1]);

                                int levelDescmin = MINVALUE + enhanceLevel;
                                int levelDescmax = MAXVALUE + enhanceLevel;

                                MagicAttribLevel magicAttribmin = Loader.Loader.MagicAttribLevels.Where(x => x.MagicName == prop.MagicName && x.Level == levelDescmin).FirstOrDefault();
                                MagicAttribLevel magicAttribmax = Loader.Loader.MagicAttribLevels.Where(x => x.MagicName == prop.MagicName && x.Level == levelDescmax).FirstOrDefault();

                                if (magicAttribmin != null && magicAttribmax != null)
                                {
                                    if (PropertyDefine.PropertiesBySymbolName.TryGetValue(prop.MagicName, out PropertyDefine.Property property))
                                    {
                                        string result = string.Format("<color=#2df600>" + property.Description + "</color>", string.Format("{0} đến {1}", magicAttribmin.MA1Min.AttributeToString(), magicAttribmax.MA1Max), Utils.Truncate(magicAttribmin.MA2Min / 18f, 1), magicAttribmax.MA3Min);
                                        contentBuilder.AppendLine(result);
                                    }
                                    else
                                    {
                                        contentBuilder.AppendLine("Symbol Not Found :" + prop.MagicName);
                                    }
                                }
                            }
                            else
                            {
                                int levelDesc = Int32.Parse(prop.MagicLevel) + enhanceLevel;
                                /// Thuộc tính tương ứng
                                MagicAttribLevel magicAttrib = Loader.Loader.MagicAttribLevels.Where(x => x.MagicName == prop.MagicName && x.Level == levelDesc).FirstOrDefault();
                                if (magicAttrib != null)
                                {
                                    if (PropertyDefine.PropertiesBySymbolName.TryGetValue(prop.MagicName, out PropertyDefine.Property property))
                                    {
                                        string result = string.Format("<color=#2df600>" + property.Description + "</color>", string.Format("{0} đến {1}", magicAttrib.MA1Min.AttributeToString(), magicAttrib.MA1Max), Utils.Truncate(magicAttrib.MA2Min / 18f, 1), magicAttrib.MA3Min);
                                        contentBuilder.AppendLine(result);
                                    }
                                    else
                                    {
                                        contentBuilder.AppendLine("Symbol Not Found :" + prop.MagicName);
                                    }
                                }
                            }
                        }
                        contentBuilder.AppendLine();
                    }
                }
            }
            else
            {
                /// Chuyển chuỗi mã hóa thuộc tính về dạng Object
                byte[] base64Decode = Convert.FromBase64String(props);
                ItemDataByteCode equipProp = DataHelper.BytesToObject<ItemDataByteCode>(base64Decode, 0, base64Decode.Length);

                /// Nếu tồn tại danh sách thuộc tính sinh ra
                if (equipProp != null)
                {
                    // Nếu có dòng byte code từ GS trả về
                    if (equipProp.BasicPropCount > 0)
                    {
                        // Lock ra basicProbs
                        foreach (KMagicInfo KmagicInfo in equipProp.BasicProp)
                        {
                            // Bỏ qua thuộc tính này vì đã ghi bên trên
                            if (KmagicInfo.nAttribType == 2)
                            {
                                continue;
                            }
                            // Đọc thẳng từ trong Dic symboy ra
                            if (PropertyDefine.PropertiesByID.TryGetValue(KmagicInfo.nAttribType, out PropertyDefine.Property property))
                            {
                                string result = string.Format("<color=#05ffe6>" + property.Description + "</color>", KmagicInfo.Value_1.AttributeToString(), Utils.Truncate(KmagicInfo.Value_2 / 18f, 1), KmagicInfo.Value_3);
                                contentBuilder.AppendLine(result);
                            }
                            else
                            {
                                contentBuilder.AppendLine("Symbol Not Found :" + KmagicInfo.nAttribType);
                            }
                        }
                    }

                    // Nếu cso dòng xanh
                    if (equipProp.GreenPropCount > 0)
                    {
                        // Thuộc tính ngẫu nhiên của đồ
                        contentBuilder.AppendLine("<color=#057aff>Thuộc tính ngẫu nhiên:</color>");
                        // Duyệt tất cả dòng xanh của đồ
                        foreach (KMagicInfo KmagicInfo in equipProp.GreenProp)
                        {
                            /// Nếu là cộng kỹ năng
                            if (KmagicInfo.nAttribType == (int)MAGIC_ATTRIB.magic_skilladdition_addskilllevel)
                            {
                                /// Kỹ năng
                                int skillID = KmagicInfo.Value_1;
                                /// Cấp độ kỹ năng
                                int skillLevel = KmagicInfo.Value_2;
                                /// Thông tin kỹ năng
                                if (Loader.Loader.Skills.TryGetValue(skillID, out SkillDataEx skillData))
                                {
                                    contentBuilder.AppendLine(string.Format("<color=#2df600>Kỹ năng <color=#00d0f5>[{0}]</color>: <color=#ffee38>+{1}</color></color>", skillData.Name, skillLevel));
                                }
                            }
                            else
                            {
                                // Cấp độ cuối cùng của magic là level của đồ + với level của thuộc tính magic
                                int FinalLevel = enhanceLevel + KmagicInfo.nLevel;

                                // lấy ra level cũ của đồ
                                MagicAttribLevel FindOrginalValue = Loader.Loader.MagicAttribLevels.Where(x => x.MAGIC_ID == KmagicInfo.nAttribType && x.Level == KmagicInfo.nLevel).FirstOrDefault();

                                // Lấy ra Level hiện tại của đồ
                                MagicAttribLevel magicAttrib = Loader.Loader.MagicAttribLevels.Where(x => x.MAGIC_ID == KmagicInfo.nAttribType && x.Level == FinalLevel).FirstOrDefault();

                                int Value1 = 0;
                                int Value2 = 0;
                                int Value3 = 0;

                                if (magicAttrib != null)
                                {
                                    if (KmagicInfo.Value_1 != -1)
                                    {
                                        int Percent = RecaculationPercent(FindOrginalValue.MA1Min, FindOrginalValue.MA1Max, KmagicInfo.Value_1);

                                        if (Percent > 0)
                                        {
                                            int AddValue = ((magicAttrib.MA1Max - magicAttrib.MA1Min) * Percent) / 100;

                                            Value1 = magicAttrib.MA1Min + AddValue;
                                        }
                                        else
                                        {
                                            Value1 = magicAttrib.MA1Min;
                                        }
                                    }
                                    if (KmagicInfo.Value_2 != -1)
                                    {
                                        int Percent = RecaculationPercent(FindOrginalValue.MA2Min, FindOrginalValue.MA2Max, KmagicInfo.Value_2);
                                        if (Percent > 0)
                                        {
                                            int AddValue = ((magicAttrib.MA2Max - magicAttrib.MA2Min) * Percent) / 100;

                                            Value2 = magicAttrib.MA2Min + AddValue;
                                        }
                                        else
                                        {
                                            Value2 = magicAttrib.MA2Min;
                                        }
                                    }
                                    if (KmagicInfo.Value_3 != -1)
                                    {
                                        int Percent = RecaculationPercent(FindOrginalValue.MA3Min, FindOrginalValue.MA3Max, KmagicInfo.Value_3);

                                        if (Percent > 0)
                                        {
                                            int AddValue = ((magicAttrib.MA3Max - magicAttrib.MA3Min) * Percent) / 100;

                                            Value3 = magicAttrib.MA3Min + AddValue;
                                        }
                                        else
                                        {
                                            Value3 = magicAttrib.MA3Min;
                                        }
                                    }

                                    // Lấy ra thông tin xem có trong Properties không
                                    if (PropertyDefine.PropertiesByID.TryGetValue(KmagicInfo.nAttribType, out PropertyDefine.Property property))
                                    {
                                        string result = string.Format("<color=#2df600>" + property.Description + "</color>", Value1.AttributeToString(), Utils.Truncate(Value2 / 18f, 1), Value3) + " " + string.Format("<color=#ff4de4>({0} - {1})</color>", magicAttrib.MA1Min, magicAttrib.MA1Max);
                                        contentBuilder.AppendLine(result);
                                    }
                                    else
                                    {
                                        // Không tìm thấy symboy
                                        contentBuilder.AppendLine("Symbol Not Found :" + KmagicInfo.nAttribType);
                                    }
                                }
                                else
                                {
                                    contentBuilder.AppendLine("Symbol Not Found :" + KmagicInfo.nAttribType);
                                }
                            }
                        }

                        // xuống 1 dòng cho đẹp
                        contentBuilder.AppendLine();
                    }
                }
            }

            return contentBuilder.ToString();
        }

        /// <summary>
        /// Check ra độ bền của trang bị
        /// </summary>
        /// <param name="Strong"></param>
        /// <param name="props"></param>
        /// <param name="itemData"></param>
        /// <param name="contentBuilder"></param>
        public static void GetDurabilityItem(int Strong, string props, ItemData itemData, StringBuilder contentBuilder)
        {
            // Nếu là trang bị mới có độ bền
            if (KD_ISNONNAMENT(itemData.DetailType))
            {
                string Value = "";

                if (String.IsNullOrEmpty(props) || props.Contains(("ERORR")))
                {
                    List<BasicProp> emptyProps = itemData.ListBasicProp?.OrderBy(x => x.Index)?.ToList();

                    var FindValue = emptyProps.Where(x => x.BasicPropType == "durability_v").FirstOrDefault();

                    if (FindValue != null)
                    {
                        if (Strong > FindValue.BasicPropPA1Min)
                        {
                            Strong = FindValue.BasicPropPA1Min;
                        }
                        Value = "Độ bền: " + Strong + "/" + FindValue.BasicPropPA1Min + "";
                    }
                }
                else
                {
                    byte[] base64Decode = Convert.FromBase64String(props);
                    ItemDataByteCode equipProp = DataHelper.BytesToObject<ItemDataByteCode>(base64Decode, 0, base64Decode.Length);

                    if (equipProp.BasicPropCount > 0)
                    {
                        var FindValue = equipProp.BasicProp.Where(x => x.nAttribType == 2).FirstOrDefault();
                        if (FindValue != null)
                        {
                            if (Strong > FindValue.Value_1)
                            {
                                Strong = FindValue.Value_1;
                            }

                            Value = "Độ bền: " + Strong + "/" + FindValue.Value_1;
                        }
                    }
                }

                if (Value == "")
                {
                    Value = "<color=yellow>Không thể phá hủy</color>";
                }

                contentBuilder.AppendLine(Value);
            }
            else
            {
                contentBuilder.AppendLine("<color=green>Không thể hỏng</color>");
            }
        }

        /// <summary>
        /// Tính toán dữ liệu trang bị theo số sao
        /// </summary>
        /// <param name="itemGD"></param>
        /// <returns></returns>
        public static StarLevelStruct StarCalculation(GoodsData itemGD)
        {
            long totalValue = 0;

            try
            {
                if (!Loader.Loader.Items.TryGetValue(itemGD.GoodsID, out ItemData _ItemData))
                {
                    return null;
                }

                /// Nếu không phải trang bị hay trang bị pet
                if (!KTGlobal.IsEquip(itemGD.GoodsID) && !KTGlobal.IsPetEquip(_ItemData.Genre))
                {
                    return null;
                }
                int LevelCuongHoa = itemGD.Forge_level;
                int LevelLuyenHoa = 0;

                // List ra cái fighPower
                double TotalValue = _ItemData.FightPower;

                /// Nếu vật phẩm có thể cường hóa
                if (CanEquipBeEnhance(_ItemData))
                {
                    double nLevelRate = 0;
                    double nTypeRate = 0;

                    double nEnhValue = 0;
                    double nStrValue = 0;

                    Equip_Type_Rate TypeRate = Loader.Loader.ItemValues.List_Equip_Type_Rate.Where(x => (int)x.EquipType == _ItemData.DetailType).FirstOrDefault();
                    if (TypeRate != null)
                    {
                        nTypeRate = TypeRate.Value;
                    }

                    Equip_Level LevelRate = Loader.Loader.ItemValues.List_Equip_Level.Where(x => x.Level == _ItemData.Level).FirstOrDefault();
                    if (LevelRate != null)
                    {
                        nLevelRate = LevelRate.Value;
                    }

                    for (int i = 0; i <= LevelCuongHoa; i++)
                    {
                        Enhance_Value _Enhance_Value = Loader.Loader.ItemValues.List_Enhance_Value.Where(x => x.EnhanceTimes == i).FirstOrDefault();
                        if (_Enhance_Value != null)
                        {
                            nEnhValue = nEnhValue + _Enhance_Value.Value;
                        }
                    }

                    if (LevelLuyenHoa > 0)
                    {
                        Strengthen_Value _Strengthen_Value = Loader.Loader.ItemValues.List_Strengthen_Value.Where(x => x.StrengthenTimes == LevelLuyenHoa).FirstOrDefault();
                        if (_Strengthen_Value != null)
                        {
                            nStrValue = _Strengthen_Value.Value;
                        }
                    }

                    List<KMagicInfo> TotalProbs = new List<KMagicInfo>();

                    // Nếu như vật phẩm này có probs rồi
                    // Giải mã level ra
                    if (!string.IsNullOrEmpty(itemGD.Props) && !itemGD.Props.Contains("ERORR"))
                    {
                        byte[] Base64Decode = Convert.FromBase64String(itemGD.Props);

                        // Giải mã toàn bộ chỉ số đã ghi trong gamedb
                        ItemDataByteCode _ItemBuild = DataHelper.BytesToObject<ItemDataByteCode>(Base64Decode, 0, Base64Decode.Length);

                        // Add vào đây thuộc tính dòng xanh
                        if (_ItemBuild.GreenPropCount > 0)
                        {
                            TotalProbs.AddRange(_ItemBuild.GreenProp);
                        }
                        // Add vào đây thuộc tính ẩn
                        if (_ItemBuild.HiddenProbsCount > 0)
                        {
                            TotalProbs.AddRange(_ItemBuild.HiddenProbs);
                        }
                    } // Còn nếu vật phẩm này đéo có dòng nào lưu vào CSDL thì lây ra từ TMP cơ bản
                    else
                    {
                        List<PropMagic> GreenProp = _ItemData.GreenProp;

                        List<PropMagic> HiddenProb = _ItemData.HiddenProp;
                        if (GreenProp != null)
                        {
                            // Nếu như đồ có dòng xanh
                            if (GreenProp.Count > 0)
                            {
                                // Duyệt tất cả dòng xanh
                                foreach (PropMagic _probs in GreenProp)
                                {
                                    if (PropertyDefine.PropertiesBySymbolName.ContainsKey(_probs.MagicName))
                                    {
                                        int MagicID = PropertyDefine.PropertiesBySymbolName[_probs.MagicName].ID;

                                        KMagicInfo _InfoMagic = new KMagicInfo();
                                        _InfoMagic.nAttribType = MagicID;

                                        // Nếu đây là thuộc tính random từ đâu tới dâu
                                        if (_probs.MagicLevel.Contains('|'))
                                        {
                                            int MINVALUE = Int32.Parse(_probs.MagicLevel.Split('|')[0]);
                                            int MAXVALUE = Int32.Parse(_probs.MagicLevel.Split('|')[1]);

                                            int Level = UnityEngine.Random.Range(MINVALUE, MAXVALUE);
                                            _InfoMagic.nLevel = Level;
                                        }
                                        else
                                        {
                                            _InfoMagic.nLevel = Int32.Parse(_probs.MagicLevel);
                                        }

                                        TotalProbs.Add(_InfoMagic);
                                    }
                                }
                            }
                        }
                        if (HiddenProb != null)
                        {
                            // Nếu như đồ có dòng xanh
                            if (HiddenProb.Count > 0)
                            {
                                // Duyệt tất cả dòng xanh
                                foreach (PropMagic _probs in HiddenProb)
                                {
                                    if (PropertyDefine.PropertiesBySymbolName.ContainsKey(_probs.MagicName))
                                    {
                                        int MagicID = PropertyDefine.PropertiesBySymbolName[_probs.MagicName].ID;

                                        KMagicInfo _InfoMagic = new KMagicInfo();
                                        _InfoMagic.nAttribType = MagicID;

                                        // Nếu đây là thuộc tính random từ đâu tới dâu
                                        if (_probs.MagicLevel.Contains('|'))
                                        {
                                            int MINVALUE = Int32.Parse(_probs.MagicLevel.Split('|')[0]);
                                            int MAXVALUE = Int32.Parse(_probs.MagicLevel.Split('|')[1]);

                                            int Level = UnityEngine.Random.Range(MINVALUE, MAXVALUE);
                                            _InfoMagic.nLevel = Level;
                                        }
                                        else
                                        {
                                            _InfoMagic.nLevel = Int32.Parse(_probs.MagicLevel);
                                        }

                                        TotalProbs.Add(_InfoMagic);
                                    }
                                }
                            }
                        }
                    }

                    Dictionary<int, double> tbValue = new Dictionary<int, double>();

                    int MagicCount = 1;

                    // Vòng for thứ nhất để tính tổng VALUE của các symboy trong magicatribute level
                    foreach (KMagicInfo _probs in TotalProbs)
                    {
                        double Rate = 100;

                        Equip_Random_Pos _Rate = Loader.Loader.ItemValues.List_Equip_Random_Pos.Where(x => x.MAGIC_POS == MagicCount).FirstOrDefault();
                        if (_Rate != null)
                        {
                            Rate = (double)_Rate.Value / 100;
                        }

                        MagicAttribLevel _Atribute = Loader.Loader.MagicAttribLevels.Where(x => x.MAGIC_ID == _probs.nAttribType && x.Level == _probs.nLevel).FirstOrDefault();
                        if (_Atribute != null)
                        {
                            long Value = _Atribute.ItemValue;

                            double FinalValue = Math.Floor(Rate * Value);

                            tbValue.Add(MagicCount, FinalValue);

                            TotalValue += FinalValue;
                        }

                        MagicCount++;
                    }

                    //Vòng for thứ 2 để tính điểm giữa các dòng với nhau
                    for (int i = 1; i <= TotalProbs.Count; i++)
                    {
                        KMagicInfo SourceProb = TotalProbs[i - 1];

                        for (int j = 1; j <= TotalProbs.Count; j++)
                        {
                            KMagicInfo DescProb = TotalProbs[j - 1];

                            if (PropertyDefine.PropertiesByID.ContainsKey(SourceProb.nAttribType))
                            {
                                string SourceMagicName = PropertyDefine.PropertiesByID[SourceProb.nAttribType].SymbolName;

                                MagicSource _FindMagicSource = Loader.Loader.ItemValues.Magic_Combine_Def.MagicSourceDef.Where(x => x.MagicName == SourceMagicName).FirstOrDefault();

                                if (_FindMagicSource != null)
                                {
                                    int SelectValue = _FindMagicSource.Index;

                                    string DestMagicName = PropertyDefine.PropertiesByID[DescProb.nAttribType].SymbolName;

                                    MagicDesc _FindMagicDest = Loader.Loader.ItemValues.Magic_Combine_Def.MagicDescDef.Where(x => x.MagicName == DestMagicName).FirstOrDefault();
                                    if (_FindMagicDest != null)
                                    {
                                        try
                                        {
                                            if (_FindMagicDest.ListValue.Count() > SelectValue)
                                            {
                                                //Console.WriteLine("CHECK : " + SourceProb.MagicName + "===>" + DescProb.MagicName);

                                                double Value = _FindMagicDest.ListValue[SelectValue];

                                                double nRate = Math.Sqrt(Value) / 10;

                                                nRate = (nRate - 1) * SourceProb.nLevel * DescProb.nLevel / 400;

                                                double FinalValue = Math.Floor((tbValue[i] + tbValue[j]) * nRate);

                                                //  Console.WriteLine("FINAL VALUE :" + FinalValue);

                                                TotalValue += FinalValue;
                                            }
                                        }
                                        catch (Exception exx)
                                        {
                                            // Console.WriteLine(exx.ToString());
                                        }
                                    }
                                }
                            }
                        }
                    }

                    TotalValue = Math.Floor(TotalValue / 100 * nLevelRate);

                    TotalValue = Math.Floor(TotalValue / 100 * nTypeRate);

                    TotalValue = TotalValue + Math.Floor(nEnhValue / 100 * nTypeRate);

                    TotalValue = TotalValue + Math.Floor(nStrValue / 100 * nTypeRate);

                    //  Console.WriteLine("FINAL TAI PHU :" + TotalValue);
                }
                else if (KTGlobal.IsSignet(itemGD.GoodsID))
                {
                    try
                    {
                        /// Cường hóa ngũ hành tương khắc
                        {
                            string[] param = itemGD.OtherParams[ItemPramenter.Pram_1].Split('|');
                            int seriesEnhance = int.Parse(param[0]);
                            int seriesEnhanceExp = int.Parse(param[1]);
                            /// Tăng tài phú tương ứng
                            TotalValue += Loader.Loader.SignetExps[seriesEnhance - 1].Value;
                            ///// Cấp tiếp theo
                            //int nextLevel = seriesEnhance + 1;
                            ///// Nếu cấp tiếp theo tồn tại
                            //if (Loader.Loader.SignetExps.TryGetValue(nextLevel, out SingNetExp nextLevelInfo))
                            //{
                            //    /// Độ lệch Exp
                            //    int subtract = nextLevelInfo.Value - Loader.Loader.SignetExps[seriesEnhance].Value;
                            //    /// Phần trăm kinh nghiệm hiện tại
                            //    float percent = seriesEnhanceExp / Loader.Loader.SignetExps[seriesEnhance].UpgardeExp;
                            //    /// Tăng lượng tài phú cộng thêm
                            //    TotalValue += subtract * percent;
                            //}
                        }
                        /// Nhược hóa ngũ hành tương khắc
                        {
                            string[] param = itemGD.OtherParams[ItemPramenter.Pram_2].Split('|');
                            int seruesConque = int.Parse(param[0]);
                            int seriesConqueExp = int.Parse(param[1]);
                            /// Tăng tài phú tương ứng
                            TotalValue += Loader.Loader.SignetExps[seruesConque - 1].Value;
                            ///// Cấp tiếp theo
                            //int nextLevel = seruesConque + 1;
                            ///// Nếu cấp tiếp theo tồn tại
                            //if (Loader.Loader.SignetExps.TryGetValue(nextLevel, out SingNetExp nextLevelInfo))
                            //{
                            //    /// Độ lệch Exp
                            //    int subtract = nextLevelInfo.Value - Loader.Loader.SignetExps[seruesConque].Value;
                            //    /// Phần trăm kinh nghiệm hiện tại
                            //    float percent = seriesConqueExp / Loader.Loader.SignetExps[seruesConque].UpgardeExp;
                            //    /// Tăng lượng tài phú cộng thêm
                            //    TotalValue += subtract * percent;
                            //}
                        }
                    }
                    catch (Exception ex)
                    {
                        KTDebug.LogError(ex.ToString());
                    }
                }

                totalValue = (long)TotalValue;

                int ItemType = _ItemData.DetailType;
                int ItemLevel = _ItemData.Level;

                if(_ItemData.Genre==7)
                {
                    ItemType = ItemType + 100;
                }    

                Equip_StarLevel List_Equip_StarLevel = null;
                int LevelStart = 0;

                if (ItemLevel == 1)
                {
                    List_Equip_StarLevel = Loader.Loader.ItemValues.List_Equip_StarLevel.Where(x => x.EQUIP_DETAIL_TYPE == ItemType && TotalValue >= x.EQUIP_LEVEL_1).LastOrDefault();
                }
                else if (ItemLevel == 2)
                {
                    List_Equip_StarLevel = Loader.Loader.ItemValues.List_Equip_StarLevel.Where(x => x.EQUIP_DETAIL_TYPE == ItemType && TotalValue >= x.EQUIP_LEVEL_2).LastOrDefault();
                }
                else if (ItemLevel == 3)
                {
                    List_Equip_StarLevel = Loader.Loader.ItemValues.List_Equip_StarLevel.Where(x => x.EQUIP_DETAIL_TYPE == ItemType && TotalValue >= x.EQUIP_LEVEL_3).LastOrDefault();
                }
                else if (ItemLevel == 4)
                {
                    List_Equip_StarLevel = Loader.Loader.ItemValues.List_Equip_StarLevel.Where(x => x.EQUIP_DETAIL_TYPE == ItemType && TotalValue >= x.EQUIP_LEVEL_4).LastOrDefault();
                }
                else if (ItemLevel == 5)
                {
                    List_Equip_StarLevel = Loader.Loader.ItemValues.List_Equip_StarLevel.Where(x => x.EQUIP_DETAIL_TYPE == ItemType && TotalValue >= x.EQUIP_LEVEL_5).LastOrDefault();
                }
                else if (ItemLevel == 6)
                {
                    List_Equip_StarLevel = Loader.Loader.ItemValues.List_Equip_StarLevel.Where(x => x.EQUIP_DETAIL_TYPE == ItemType && TotalValue >= x.EQUIP_LEVEL_6).LastOrDefault();
                }
                else if (ItemLevel == 7)
                {
                    List_Equip_StarLevel = Loader.Loader.ItemValues.List_Equip_StarLevel.Where(x => x.EQUIP_DETAIL_TYPE == ItemType && TotalValue >= x.EQUIP_LEVEL_7).LastOrDefault();
                }
                else if (ItemLevel == 8)
                {
                    List_Equip_StarLevel = Loader.Loader.ItemValues.List_Equip_StarLevel.Where(x => x.EQUIP_DETAIL_TYPE == ItemType && TotalValue >= x.EQUIP_LEVEL_8).LastOrDefault();
                }
                else if (ItemLevel == 9)
                {
                    List_Equip_StarLevel = Loader.Loader.ItemValues.List_Equip_StarLevel.Where(x => x.EQUIP_DETAIL_TYPE == ItemType && TotalValue >= x.EQUIP_LEVEL_9).LastOrDefault();
                }
                else if (ItemLevel == 10)
                {
                    List_Equip_StarLevel = Loader.Loader.ItemValues.List_Equip_StarLevel.Where(x => x.EQUIP_DETAIL_TYPE == ItemType && TotalValue >= x.EQUIP_LEVEL_10).LastOrDefault();
                }
                if (List_Equip_StarLevel != null)
                {
                    LevelStart = List_Equip_StarLevel.STAR_LEVEL;
                }
                /// Nếu NULL thì lấy 1
                else
                {
                    LevelStart = 1;
                }

                StarLevelStruct _LevelSelect = Loader.Loader.ItemValues.List_StarLevelStruct.Where(x => x.StarLevel == LevelStart).FirstOrDefault();
                if (_LevelSelect == null)
                {
                    return null;
                }

                _LevelSelect.Value = (long)TotalValue;
                return _LevelSelect;
            }
            catch
            {
                return null;
            }
        }
        /// <summary>
        /// Init thuộc tính ngựa
        /// </summary>
        /// <param name="itemData"></param>
        /// <returns></returns>
        private static string InitHorseProperty(ItemData itemData)
        {
            StringBuilder contentBuilder = new StringBuilder();

            /// Nếu vật phẩm không tồn tại
            if (itemData == null)
            {
                return "";
            }

            contentBuilder.AppendLine("<color=#057aff>Thuộc tính thú cưỡi:</color>");
            
            List<RiderProp> RiderProp = itemData.RiderProp;
            if (RiderProp != null)
            {
                if (RiderProp.Count > 0)
                {
                    foreach (RiderProp _Prob in RiderProp)
                    {
                        if (PropertyDefine.PropertiesBySymbolName.TryGetValue(_Prob.RidePropType, out PropertyDefine.Property property))
                        {
                            string result = string.Format("<color=#05ffe6>" + property.Description + "</color>", _Prob.RidePropPA1Min, _Prob.RidePropPA2Min, _Prob.RidePropPA3Min);
                            contentBuilder.AppendLine(result);
                        }
                        else
                        {
                            contentBuilder.AppendLine("Symbol Not Found :" + _Prob.RidePropType);
                        }
                    }
                }
            }
            contentBuilder.AppendLine("");

            return contentBuilder.ToString();
        }
        /// <summary>
        /// Xây ToolTip vật phẩm tương ứng
        /// </summary>
        /// <param name="uiSuperToolTip"></param>
        /// <param name="itemGD"></param>
        /// <param name="buttons"></param>
        /// <param name="shopItem"></param>
        private static void BuildToolTipItemInfo(UISuperToolTip_Component uiSuperToolTip, GoodsData itemGD, List<KeyValuePair<string, Action>> buttons = null, ShopItem shopItem = null)
        {
            if (itemGD == null)
            {
                return;
            }

            /// Nếu thông tin trang bị không tồn tại
            if (!Loader.Loader.Items.TryGetValue(itemGD.GoodsID, out ItemData item))
            {
                return;
            }

            uiSuperToolTip.Title = KTGlobal.GetItemName(itemGD);
            uiSuperToolTip.ShowItemBoxBackground = true;
            uiSuperToolTip.ShortDesc = "";
            uiSuperToolTip.IconBundleDir = item.MapSpriteBundleDir;
            uiSuperToolTip.IconAtlasName = item.MapSpriteAtlasName;
            uiSuperToolTip.IconSpriteName = item.Image;
            uiSuperToolTip.IconPixelPerfect = true;
            uiSuperToolTip.Buttons = buttons;
            uiSuperToolTip.ShowFunctionButtons = buttons != null && buttons.Count > 0;
            uiSuperToolTip.ShowBottomDesc = true;

            StringBuilder contentBuilder = new StringBuilder();

            /// Nếu là trang bị hoặc tang bị pet
            if (KTGlobal.IsItemEquip(item.Genre) || KTGlobal.IsPetEquip(item.Genre))
            {
                /// Dữ liệu số sao
                StarLevelStruct starLevelStruct = KTGlobal.StarCalculation(itemGD);
                uiSuperToolTip.TitleColor = KTGlobal.GetItemColor(itemGD);

                /// Nếu có cấu trúc sao
                if (starLevelStruct != null)
                {
                    string totalValueStr = (starLevelStruct.Value / 10000f).ToString();
                    /// Nếu không có dấu chấm động
                    if (!totalValueStr.Contains('.'))
                    {
                        totalValueStr += ".0000";
                    }
                    /// Nếu số phần tử sau dấu chấm động không đủ 4
                    while (totalValueStr.Substring(totalValueStr.IndexOf('.') + 1).Length < 4)
                    {
                        totalValueStr += "0";
                    }
                    contentBuilder.AppendLine(string.Format("<color=#05ffe6>Tài phú:</color> <color=#50ff29>{0}</color>", totalValueStr));

                    /// Tổng số sao
                    uiSuperToolTip.TotalStar = starLevelStruct.StarLevel / 2f;

                    /// Màu sao
                    switch (starLevelStruct.NameColor)
                    {
                        case "green":
                            {
                                uiSuperToolTip.EquipStarColor = UISuperToolTip.SuperToolTipEquipStarColor.Basic;
                                break;
                            }
                        case "blue":
                            {
                                uiSuperToolTip.EquipStarColor = UISuperToolTip.SuperToolTipEquipStarColor.Blue;
                                break;
                            }
                        case "purple":
                            {
                                uiSuperToolTip.EquipStarColor = UISuperToolTip.SuperToolTipEquipStarColor.Purple;
                                break;
                            }
                        case "orange":
                            {
                                uiSuperToolTip.EquipStarColor = UISuperToolTip.SuperToolTipEquipStarColor.Orange;
                                break;
                            }
                        case "yellow":
                            {
                                uiSuperToolTip.EquipStarColor = UISuperToolTip.SuperToolTipEquipStarColor.Yellow;
                                break;
                            }
                    }
                }
                else
                {
                    contentBuilder.AppendLine(string.Format("<color=#05ffe6>Tài phú:</color> <color=#50ff29>{0}</color>", "0.0000"));
                }

                string lockString;
                if (itemGD.Binding == 1)
                {
                    lockString = "Đã khóa";
                }
                else
                {
                    lockString = "Không khóa";
                }
                /// Nếu là trang bị pet
                if (KTGlobal.IsPetEquip(item.Genre))
                {
                    PetEquipSlot itemType = (PetEquipSlot)item.DetailType;
                    contentBuilder.AppendLine(string.Format("{0} - {1} - {2}", KTGlobal.GetPetEquipTypeString(itemType), lockString, KTGlobal.IsItemCanBeSold(item.BindType) ? "Có thể bán" : "Không thể bán"));
                }
                /// Nếu là trang bị thường
                else
                {
                    KE_ITEM_EQUIP_DETAILTYPE itemType = KTGlobal.GetEquipType(item.DetailType);
                    contentBuilder.AppendLine(string.Format("{0} - {1} - {2}", KTGlobal.GetEquipTypeString(itemType), lockString, KTGlobal.IsItemCanBeSold(item.BindType) ? "Có thể bán" : "Không thể bán"));
                }

                contentBuilder.AppendLine("");
                contentBuilder.AppendLine("<color=#057aff>Yêu cầu trang bị:</color>");
                contentBuilder.Append(KTGlobal.BuildEquipRequiration(item.ListReqProp));
                contentBuilder.AppendLine("");

                /// Nếu không phải trang bị pet
                if (!KTGlobal.IsPetEquip(item.Genre))
                {
                    KE_ITEM_EQUIP_DETAILTYPE itemType = KTGlobal.GetEquipType(item.DetailType);

                    /// Dùng cho môn phái hệ gì đó
                    string recommendText = KTGlobal.GetRecommendSeriesText(itemGD);
                    if (!string.IsNullOrEmpty(recommendText))
                    {
                        contentBuilder.AppendLine(recommendText);
                    }

                    // Build độ bền của vật phẩm
                    GetDurabilityItem(itemGD.Strong, itemGD.Props, item, contentBuilder);

                    /// Cấp trang bị
                    contentBuilder.AppendLine(string.Format("Cấp trang bị: <color=#ffc124>{0}</color>", item.Level));
                    /// Thuộc tính ngũ hành
                    string seriesName = KTGlobal.GetElementString((Elemental)itemGD.Series, out Color seriesColor);
                    contentBuilder.AppendLine(string.Format("Ngũ hành trang bị: <color=#{0}>{1}</color>", ColorUtility.ToHtmlStringRGB(seriesColor), seriesName));

                    contentBuilder.AppendLine("");

                    int Level = GetActiveSuiteStatusLevel();

                    //KTDebug.LogError("ITEM LEVEL :" + Level);

                    /// Kích hoạt all trang sức không
                    bool active_all_ornament = false;

                    bool active_suit = false;

                    if (Level == 1)
                    {
                        active_all_ornament = true;
                    }

                    if (Level == 2)
                    {
                        active_suit = true;
                    }

                    switch (itemType)
                    {
                        // Ngựa cũng vứt vào đây
                       
                        case KE_ITEM_EQUIP_DETAILTYPE.equip_meleeweapon:
                        case KE_ITEM_EQUIP_DETAILTYPE.equip_rangeweapon:
                        case KE_ITEM_EQUIP_DETAILTYPE.equip_armor:
                        case KE_ITEM_EQUIP_DETAILTYPE.equip_ring:
                        case KE_ITEM_EQUIP_DETAILTYPE.equip_amulet:
                        case KE_ITEM_EQUIP_DETAILTYPE.equip_boots:
                        case KE_ITEM_EQUIP_DETAILTYPE.equip_belt:
                        case KE_ITEM_EQUIP_DETAILTYPE.equip_helm:
                        case KE_ITEM_EQUIP_DETAILTYPE.equip_cuff:
                        case KE_ITEM_EQUIP_DETAILTYPE.equip_mask:
                        case KE_ITEM_EQUIP_DETAILTYPE.equip_mantle:
                        case KE_ITEM_EQUIP_DETAILTYPE.equip_ornament:
                        case KE_ITEM_EQUIP_DETAILTYPE.equip_chop:
                        case KE_ITEM_EQUIP_DETAILTYPE.equip_pendant:
                            {
                                contentBuilder.Append(KTGlobal.InitEquipBaseAttributes(itemGD.Props, item, itemGD.Series, itemGD.Forge_level));


                                string hiddenProps = KTGlobal.InitEquipHiddenProbs(itemGD.Props, item, itemGD.Series, itemGD.Forge_level, active_all_ornament, itemGD.Using, active_suit);
                                if (!string.IsNullOrEmpty(hiddenProps))
                                {
                                    contentBuilder.Append(hiddenProps);
                                }

                                // Đẩy cái thuộc tính bộ lên trên cùng để lấy ra dòng kích hoạt
                                contentBuilder.Append(KTGlobal.InitActiveSuite(item));

                                contentBuilder.Append(KTGlobal.InitEquipEnhance(itemGD, item));
                                break;
                            }

                        case KE_ITEM_EQUIP_DETAILTYPE.equip_signet:
                            {
                               contentBuilder.Append(KTGlobal.InitSignetProperty(itemGD, item));
                                break;
                            }
                        case KE_ITEM_EQUIP_DETAILTYPE.equip_horse:
                            {
                                contentBuilder.Append(KTGlobal.InitEquipBaseAttributes(itemGD.Props, item, itemGD.Series, itemGD.Forge_level));
                                contentBuilder.Append(KTGlobal.InitHorseProperty(item));
                                break;
                            }
                    }
                }
                /// Nếu là trang bị pet
                else
                {
                    /// Cấp trang bị
                    contentBuilder.AppendLine(string.Format("Cấp trang bị: <color=#ffc124>{0}</color>", item.Level));
                    /// Thuộc tính cơ bản
                    contentBuilder.Append(KTGlobal.InitEquipBaseAttributes(itemGD.Props, item, itemGD.Series, itemGD.Forge_level));
                }
            }
            /// Các loại vật phẩm khác
            else
            {
                uiSuperToolTip.TitleColor = Color.white;

                string lockString;
                if (itemGD.Binding == 1)
                {
                    lockString = "Đã khóa";
                }
                else
                {
                    lockString = "Không khóa";
                }
                contentBuilder.AppendLine(string.Format("{0} - {1}", lockString, KTGlobal.IsItemCanBeSold(item.BindType) ? "Có thể bán" : "Không thể bán"));
            }

            ///// Nếu có thể đổi Ngũ Hành Hồn Thạch
            //if (KTGlobal.IsEquipAbleToRefineIntoFS(itemGD))
            //{
            //    contentBuilder.AppendLine();
            //    /// Số lượng Ngũ Hành Hồn Thạch phân giải được
            //    int refineFSCount = KTGlobal.CalculateTotalFSByRefiningEquip(itemGD);
            //    contentBuilder.AppendLine(string.Format("<color=#64ff0a>Phân giải thu được <color=yellow>{0} Ngũ Hành Hồn Thạch</color></color>", refineFSCount));
            //}

            /// Nếu là trang bị
            if (KTGlobal.IsEquip(item.ItemID))
            {
                KE_ITEM_EQUIP_DETAILTYPE itemType = KTGlobal.GetEquipType(item.DetailType);
                // Add thêm tip rquest
                switch (itemType)
                {
                    // Nếu là đồ có kích hoạt 5 hành thì sẽ đưa ra yêu cầu cần đồ kích
                    case KE_ITEM_EQUIP_DETAILTYPE.equip_meleeweapon:
                    case KE_ITEM_EQUIP_DETAILTYPE.equip_rangeweapon:
                    case KE_ITEM_EQUIP_DETAILTYPE.equip_armor:
                    case KE_ITEM_EQUIP_DETAILTYPE.equip_ring:
                    case KE_ITEM_EQUIP_DETAILTYPE.equip_amulet:
                    case KE_ITEM_EQUIP_DETAILTYPE.equip_boots:
                    case KE_ITEM_EQUIP_DETAILTYPE.equip_belt:
                    case KE_ITEM_EQUIP_DETAILTYPE.equip_helm:
                    case KE_ITEM_EQUIP_DETAILTYPE.equip_cuff:
                    case KE_ITEM_EQUIP_DETAILTYPE.equip_pendant:
                        {
                            // Set ở đây
                            contentBuilder.Append(KTGlobal.InitRequestActiveItem(itemGD.Props, item, itemGD.Series,
                                itemGD.Using));
                            break;
                        }
                }
            }

            /// Hạn sử dụng
            if (!string.IsNullOrEmpty(itemGD.Endtime))
            {
                DateTime endTime = DateTime.Parse(itemGD.Endtime);
                if (endTime > DateTime.Now)
                {
                    TimeSpan diff = endTime - DateTime.Now;
                    int diffSec = (int)diff.TotalSeconds;

                    string text;
                    if (diffSec == -1)
                    {
                        text = "";
                    }
                    else if (diffSec <= 3600)
                    {
                        text = "dưới 1 giờ";
                    }
                    else if (diffSec < 86400)
                    {
                        text = string.Format("{0} giờ", diffSec / 3600);
                    }
                    else
                    {
                        text = string.Format("{0} ngày", diffSec / 86400);
                    }

                    if (!string.IsNullOrEmpty(text))
                    {
                        contentBuilder.AppendLine(string.Format("<color=#ffee2e>Thời gian sử dụng còn <color=#ff42e0>{0}</color></color>", text));
                    }
                }
            }

            /// Tác giả
            if (!string.IsNullOrEmpty(itemGD.Creator))
            {
                uiSuperToolTip.AuthorName = itemGD.Creator;
            }
            else if (shopItem != null)
            {
                uiSuperToolTip.AuthorName = KTGlobal.GetShopItemRequiration(shopItem);
            }
            else
            {
                uiSuperToolTip.AuthorName = "";
            }

            /// Giá bán
            if (shopItem != null)
            {
                /// Nếu có bang cống
                if (shopItem.TongFund > 0)
                {
                    uiSuperToolTip.PriceText = string.Format("Giá mua: {0} Bang cống", shopItem.TongFund);
                }
                /// Nếu có vật phẩm
                else if (shopItem.GoodsIndex > 0)
                {
                    /// Nếu vật phẩm tồn tại
                    if (Loader.Loader.Items.TryGetValue(shopItem.GoodsIndex, out ItemData itemData))
                    {
                        uiSuperToolTip.PriceText = string.Format("Giá mua: {0} {1}", shopItem.GoodsPrice, itemData.Name);
                    }
                }
                else
                {
                    if (shopItem.ShopTab.MoneyType == (int)MoneyType.Dong || shopItem.ShopTab.MoneyType == (int)MoneyType.DongKhoa)
                    {
                        uiSuperToolTip.PriceText = string.Format("Giá mua: {0} {1}", KTGlobal.GetDisplayMoney(Math.Max(0, shopItem.GoodsPrice)), KTGlobal.GetMoneyName((MoneyType)shopItem.ShopTab.MoneyType));
                    }
                    else
                    {
                        uiSuperToolTip.PriceText = string.Format("Giá mua: {0} {1}", KTGlobal.GetDisplayMoney(Math.Max(0, item.Price)), KTGlobal.GetMoneyName((MoneyType)shopItem.ShopTab.MoneyType));
                    }
                }
            }
            else
            {
                uiSuperToolTip.PriceText = string.Format("Giá bán: {0} {1}", KTGlobal.GetDisplayMoney(Math.Max(0, item.Price / 2)), KTGlobal.GetMoneyName(itemGD.Binding == 1 ? MoneyType.BacKhoa : MoneyType.Bac));
            }

            /// Nếu không phải vật phẩm thì mới hiện DESC cho nó đỡ rối mắt
            /// Chỉ hiện desc với vật phẩm nhiệm vụ
            if (!KTGlobal.IsEquip(item.ItemID) && !KTGlobal.IsPetEquip(item.ItemID))
            {
                /// Nếu có dòng giới thiệu vật phẩm
                if (!string.IsNullOrEmpty(item.Descript))
                {
                    string introStr = item.Descript;
                    introStr = introStr.Replace("<color>", "</color>").Replace("<color=gold>", "<color=yellow>")
                        .Replace("<enter>", "<br>");
                    contentBuilder.AppendLine(introStr);
                }
            }

            uiSuperToolTip.Content = contentBuilder.ToString();
            uiSuperToolTip.Close = () =>
            {
                uiSuperToolTip.Content = "";
                uiSuperToolTip.Title = "";
                uiSuperToolTip.ShowItemBoxBackground = false;
                uiSuperToolTip.ShortDesc = "";
                uiSuperToolTip.IconBundleDir = "";
                uiSuperToolTip.IconAtlasName = "";
                uiSuperToolTip.IconSpriteName = "";
                uiSuperToolTip.IconPixelPerfect = false;
                uiSuperToolTip.Buttons = null;
                uiSuperToolTip.ShowFunctionButtons = false;
                uiSuperToolTip.ShowBottomDesc = false;
            };

            uiSuperToolTip.Build();
        }

        /// <summary>
        /// Show thông tin ấn ra
        /// </summary>
        /// <param name="itemGD"></param>
        /// <param name="itemData"></param>
        /// <returns></returns>
        private static string InitSignetProperty(GoodsData itemGD, ItemData itemData)
        {
            StringBuilder contentBuilder = new StringBuilder();

            /// Nếu không có OtherParam
            if (itemGD.OtherParams == null)
            {
                return "";
            }

            /// Thông tin cường hóa ngũ hành tương khắc
            int seriesEnhance = 0, seriesEnhanceExp = 0, maxLevelEnhanceExp = 0;
            /// Thông tin nhược hóa ngũ hành tương khắc
            int seriesConque = 0, seriesConqueExp = 0, maxLevelConqueExp = 0;

            try
            {
                {
                    string[] param = itemGD.OtherParams[ItemPramenter.Pram_1].Split('|');
                    seriesEnhance = int.Parse(param[0]);
                    seriesEnhanceExp = int.Parse(param[1]);
                    if (Loader.Loader.SignetExps.TryGetValue(seriesEnhance, out SingNetExp signetExpInfo))
                    {
                        maxLevelEnhanceExp = signetExpInfo.UpgardeExp;
                    }
                }
                {
                    string[] param = itemGD.OtherParams[ItemPramenter.Pram_2].Split('|');
                    seriesConque = int.Parse(param[0]);
                    seriesConqueExp = int.Parse(param[1]);
                    if (Loader.Loader.SignetExps.TryGetValue(seriesConque, out SingNetExp signetExpInfo))
                    {
                        maxLevelConqueExp = signetExpInfo.UpgardeExp;
                    }
                }
            }
            catch (Exception ex)
            {
                KTDebug.LogError(ex.ToString());
            }

            /// Xây Tooltip
            contentBuilder.AppendLine(string.Format("<color=#58e600>Cường hóa ngũ hành tương khắc:</color> <color=#ffdd1f>{0}</color> <color=#1ffff8>(Luyện thành: {1}/{2})</color>", seriesEnhance.AttributeToString(), seriesEnhanceExp, maxLevelEnhanceExp));
            /// Nếu có ngũ hành
            if (itemGD.Series > (int)Elemental.NONE && itemGD.Series < (int)Elemental.COUNT)
            {
                string enhanceSeries = KTGlobal.GetSeriesEnhanceByName((Elemental)itemGD.Series, out Color color);
                contentBuilder.AppendLine(string.Format("  Tăng cường hiệu quả khắc chế đối với <color=#{0}>Môn phái hệ {1}</color>", ColorUtility.ToHtmlStringRGB(color), enhanceSeries));
            }

            contentBuilder.AppendLine();

            contentBuilder.AppendLine(string.Format("<color=#58e600>Nhược hóa ngũ hành tương khắc:</color> <color=#ffdd1f>{0}</color> <color=#1ffff8>(Luyện thành: {1}/{2})</color>", seriesConque.AttributeToString(), seriesConqueExp, maxLevelConqueExp));
            /// Nếu có ngũ hành
            if (itemGD.Series > (int)Elemental.NONE && itemGD.Series < (int)Elemental.COUNT)
            {
                string conqueSeries = KTGlobal.GetSeriesConqueByName((Elemental)itemGD.Series, out Color color);
                contentBuilder.AppendLine(string.Format("  Giảm thiểu hiệu quả khắc chế của <color=#{0}>Môn phái hệ {1}</color>", ColorUtility.ToHtmlStringRGB(color), conqueSeries));
            }

            return contentBuilder.ToString();
        }

        /// <summary>
        /// Hiển thị SuperToolTip thông tin vật phẩm tương ứng
        /// </summary>
        /// <param name="item"></param>
        /// <param name="buttons"></param>
        /// <param name="shopItem"></param>
        public static void ShowItemInfo(GoodsData itemGD, List<KeyValuePair<string, Action>> buttons = null, ShopItem shopItem = null)
        {
            if (itemGD == null)
            {
                return;
            }

            /// Nếu thông tin trang bị không tồn tại
            if (!Loader.Loader.Items.TryGetValue(itemGD.GoodsID, out ItemData itemData))
            {
                return;
            }

            /// Mở Khung SuperToolTip
            PlayZone.Instance.OpenUISuperToolTip();
            UISuperToolTip uiSuperToolTip = PlayZone.Instance.UISuperToolTip;

            /// Nếu không phải trang bị
            if (!KTGlobal.IsEquip(itemGD.GoodsID))
            {
                /// Chỉ hiện 1 Tooltip chính
                uiSuperToolTip.ShowSubToolTip = false;
            }
            /// Nếu là trang bị
            else
            {
                /// Nếu là trang bị đang mặc trên người
                if (itemGD.Using >= 0)
                {
                    /// Nếu là trang bị ở Set chính
                    if (itemGD.Using < 100)
                    {
                        /// Loại trang bị
                        KE_ITEM_EQUIP_DETAILTYPE itemType = KTGlobal.GetEquipType(itemData.DetailType);
                        /// Vị trí trang bị tương ứng mặc trên người
                        KE_EQUIP_POSITION equipPos = KTGlobal.GetEquipPositionByEquipType(itemType);
                        /// Trang bị trên người
                        GoodsData mainEquipData = KTGlobal.GetEquipData(Global.Data.RoleData, equipPos);
                        /// Nếu khác nhau nghĩa là đang soi đồ của thằng khác
                        if (mainEquipData != null && mainEquipData != itemGD)
                        {
                            /// Hiện Tooltip phụ
                            uiSuperToolTip.ShowSubToolTip = true;
                            /// Xây Tooltip phụ
                            KTGlobal.BuildToolTipItemInfo(uiSuperToolTip.SubToolTip, mainEquipData, null, null);
                        }
                        /// Nếu giống nhau nghĩa là đang soi đồ của chính mình
                        else
                        {
                            /// Chỉ hiện 1 Tooltip chính
                            uiSuperToolTip.ShowSubToolTip = false;
                        }
                    }
                    /// Nếu là trang bị ở Set phụ
                    else
                    {
                        /// Loại trang bị
                        KE_ITEM_EQUIP_DETAILTYPE itemType = KTGlobal.GetEquipType(itemData.DetailType);
                        /// Vị trí trang bị tương ứng mặc trên người
                        KE_EQUIP_POSITION equipPos = KTGlobal.GetEquipPositionByEquipType(itemType);
                        /// Trang bị trên người
                        GoodsData mainEquipData = KTGlobal.GetEquipData(Global.Data.RoleData, equipPos);
                        /// Nếu tồn tại
                        if (mainEquipData != null)
                        {
                            /// Hiện Tooltip phụ
                            uiSuperToolTip.ShowSubToolTip = true;
                            /// Xây Tooltip phụ
                            KTGlobal.BuildToolTipItemInfo(uiSuperToolTip.SubToolTip, mainEquipData, null, null);
                        }
                        /// Nếu không tồn tại
                        else
                        {
                            /// Ẩn Tooltip phụ
                            uiSuperToolTip.ShowSubToolTip = false;
                        }
                    }
                }
                /// Nếu không phải trang bị đang mặc trên người
                else
                {
                    /// Loại trang bị
                    KE_ITEM_EQUIP_DETAILTYPE itemType = KTGlobal.GetEquipType(itemData.DetailType);
                    /// Vị trí trang bị tương ứng mặc trên người
                    KE_EQUIP_POSITION equipPos = KTGlobal.GetEquipPositionByEquipType(itemType);
                    /// Trang bị trên người
                    GoodsData mainEquipData = KTGlobal.GetEquipData(Global.Data.RoleData, equipPos);
                    /// Nếu tồn tại
                    if (mainEquipData != null)
                    {
                        /// Hiện Tooltip phụ
                        uiSuperToolTip.ShowSubToolTip = true;
                        /// Xây Tooltip phụ
                        KTGlobal.BuildToolTipItemInfo(uiSuperToolTip.SubToolTip, mainEquipData, null, null);
                    }
                    /// Nếu không tồn tại
                    else
                    {
                        /// Ẩn Tooltip phụ
                        uiSuperToolTip.ShowSubToolTip = false;
                    }
                }
            }

            /// Xây Tooltip chính
            KTGlobal.BuildToolTipItemInfo(uiSuperToolTip.MainToolTip, itemGD, buttons, shopItem);

            /// Tạo Tooltip
            uiSuperToolTip.Build();
        }

        /// <summary>
        /// Trả về yêu cầu mua vật phẩm
        /// </summary>
        /// <param name="shopItem"></param>
        /// <returns></returns>
        public static string GetShopItemRequiration(ShopItem shopItem)
        {
            /// Danh vọng
            if (shopItem.ReputeDBID > 0 && shopItem.ReputeLevel > 0)
            {
                int reputeClassID = shopItem.ReputeDBID % 100;
                int reputeCampID = shopItem.ReputeDBID / 100;

                /// Thông tin danh vọng hiện tại
                ReputeInfo myRepute = Global.Data.RoleData.Repute.Where(x => x.DBID == shopItem.ReputeDBID).FirstOrDefault();

                ReputeCamp reputeCamp = Loader.Loader.Reputes.Camp.Where(x => x.Id == reputeCampID).FirstOrDefault();
                if (reputeCamp != null)
                {
                    ReputeClass reputeClass = reputeCamp.Class.Where(x => x.Id == reputeClassID).FirstOrDefault();
                    if (reputeClass != null)
                    {
                        ReputeLevel reputeLevel = reputeClass.Level.Where(x => x.Id == shopItem.ReputeLevel).FirstOrDefault();
                        if (reputeLevel != null)
                        {
                            /// Nếu danh vọng hiện tại đủ
                            if (myRepute != null && myRepute.Level >= shopItem.ReputeLevel)
                            {
                                return string.Format("Danh vọng {0} (Cấp {1}) - {2}", reputeClass.Name, shopItem.ReputeLevel, reputeLevel.Name);
                            }
                            else
                            {
                                return string.Format("<color=red>Danh vọng {0} (Cấp {1}) - {2}</color>", reputeClass.Name, shopItem.ReputeLevel, reputeLevel.Name);
                            }
                        }
                    }
                }
            }

            /// TODO-Quan hàm

            /// Vinh dự tài phú
            if (shopItem.Honor > 0)
            {
                /// Danh hiệu Phi phong tương ứng
                MantleTitleXML mantleTitle = Loader.Loader.MantleTitles.OrderByDescending(x => x.RoleValue).Where(x => x.RoleValue <= Global.Data.RoleData.TotalValue / 10000f).FirstOrDefault();
                /// Cấp tài phú hiện tại
                int honorLevel = Loader.Loader.MantleTitles.IndexOf(mantleTitle) + 1;
                /// Nếu đủ cấp
                if (honorLevel >= shopItem.Honor)
                {
                    return string.Format("Vinh dự tài phú cấp {0}", shopItem.Honor);
                }
                else
                {
                    return string.Format("<color=red>Vinh dự tài phú cấp {0}</color>", shopItem.Honor);
                }
            }

            /// Không có gì trả ra NULL
            return "";
        }

        /// <summary>
        /// Đóng khung thông tin vật phẩm
        /// </summary>
        public static void CloseItemInfo()
        {
            PlayZone.Instance.CloseUISuperToolTip();
        }


        #endregion Equip Super ToolTip

        #region Medicine

        /// <summary>
        /// ID Buff rượu
        /// </summary>
        public const int WineBuffID = 378;

        /// <summary>
        /// Danh sách thuốc phục hồi sinh lực cho pet
        /// </summary>
        private static Dictionary<int, ItemData> listPetHPMedicines = null;

        /// <summary>
        /// Danh sách thuốc phục hồi sinh lực cho pet
        /// </summary>
        public static Dictionary<int, ItemData> ListPetHPMedicines
        {
            get
            {
                /// Nếu tồn tại danh sách
                if (KTGlobal.listPetHPMedicines != null)
                {
                    return KTGlobal.listPetHPMedicines;
                }

                /// Tạo mới danh sách thuốc
                KTGlobal.listPetHPMedicines = new Dictionary<int, ItemData>();

                /// Duyệt danh sách vật phẩm hệ thống
                foreach (ItemData item in Loader.Loader.Items.Values)
                {
                    /// Nếu là thuốc thì sẽ có MedicineProp
                    if (item.MedicineProp != null)
                    {
                        /// Duyệt danh sách thuộc tính của thuốc
                        foreach (Medicine prop in item.MedicineProp)
                        {
                            /// Nếu tồn tại Symbol của thuốc hồi sinh lực pet
                            if (prop.MagicName == "skill_param2_v")
                            {
                                KTGlobal.listPetHPMedicines[item.ItemID] = item;
                                break;
                            }
                        }
                    }
                }

                /// Trả về giá trị
                return KTGlobal.listPetHPMedicines;
            }
        }

        /// <summary>
        /// Danh sách thuốc phục hồi sinh lực
        /// </summary>
        private static Dictionary<int, ItemData> listHPMedicines = null;

        /// <summary>
        /// Danh sách thuốc phục hồi sinh lực
        /// </summary>
        public static Dictionary<int, ItemData> ListHPMedicines
        {
            get
            {
                /// Nếu tồn tại danh sách
                if (KTGlobal.listHPMedicines != null)
                {
                    return KTGlobal.listHPMedicines;
                }

                /// Tạo mới danh sách thuốc
                KTGlobal.listHPMedicines = new Dictionary<int, ItemData>();

                /// Duyệt danh sách vật phẩm hệ thống
                foreach (ItemData item in Loader.Loader.Items.Values)
                {
                    /// Nếu là thuốc của Pet thì thôi
                    if (KTGlobal.ListPetHPMedicines.ContainsKey(item.ItemID))
                    {
                        /// Bỏ qua
                        continue;
                    }

                    /// Nếu là thuốc thì sẽ có MedicineProp
                    if (item.MedicineProp != null)
                    {
                        /// Duyệt danh sách thuộc tính của thuốc
                        foreach (Medicine prop in item.MedicineProp)
                        {
                            /// Nếu tồn tại Symbol của thuốc hồi sinh lực
                            if (prop.MagicName == "lifepotion_v")
                            {
                                KTGlobal.listHPMedicines[item.ItemID] = item;
                                break;
                            }
                        }
                    }
                }

                /// Trả về giá trị
                return KTGlobal.listHPMedicines;
            }
        }

        /// <summary>
        /// Danh sách thuốc phục hồi nội lực
        /// </summary>
        private static Dictionary<int, ItemData> listMPMedicines = null;

        /// <summary>
        /// Danh sách thuốc phục hồi nội lực
        /// </summary>
        public static Dictionary<int, ItemData> ListMPMedicines
        {
            get
            {
                /// Nếu tồn tại danh sách
                if (KTGlobal.listMPMedicines != null)
                {
                    return KTGlobal.listMPMedicines;
                }

                /// Tạo mới danh sách thuốc
                KTGlobal.listMPMedicines = new Dictionary<int, ItemData>();

                /// Duyệt danh sách vật phẩm hệ thống
                foreach (ItemData item in Loader.Loader.Items.Values)
                {
                    /// Nếu là thuốc thì sẽ có MedicineProp
                    if (item.MedicineProp != null)
                    {
                        /// Duyệt danh sách thuộc tính của thuốc
                        foreach (Medicine prop in item.MedicineProp)
                        {
                            /// Nếu tồn tại Symbol của thuốc hồi sinh lực
                            if (prop.MagicName == "manapotion_v")
                            {
                                KTGlobal.listMPMedicines[item.ItemID] = item;
                                break;
                            }
                        }
                    }
                }

                /// Trả về giá trị
                return KTGlobal.listMPMedicines;
            }
        }

        /// <summary>
        /// Danh sách rượu
        /// </summary>
        private static Dictionary<int, ItemData> listWines = null;

        /// <summary>
        /// Danh sách rượu
        /// </summary>
        public static Dictionary<int, ItemData> ListWines
        {
            get
            {
                /// Nếu tồn tại trong danh sách
                if (KTGlobal.listWines != null)
                {
                    return KTGlobal.listWines;
                }

                /// Tạo mới danh sách thuốc
                KTGlobal.listWines = new Dictionary<int, ItemData>();

                /// Duyệt danh sách vật phẩm hệ thống
                foreach (ItemData item in Loader.Loader.Items.Values)
                {
                    /// Nếu là rượu
                    if (item.DetailType == 1 && item.ParticularType >= 48 && item.ParticularType <= 52)
                    {
                        /// Thêm vào danh sách
                        KTGlobal.listWines[item.ItemID] = item;
                    }
                }

                /// Trả về giá trị
                return KTGlobal.listWines;
            }
        }

        /// <summary>
        /// Danh sách Huyền Tinh
        /// </summary>
        private static Dictionary<int, ItemData> listCrystalStones = null;

        /// <summary>
        /// Danh sách Huyền Tinh
        /// </summary>
        public static Dictionary<int, ItemData> ListCrystalStones
        {
            get
            {
                /// Nếu tồn tại trong danh sách
                if (KTGlobal.listCrystalStones != null)
                {
                    return KTGlobal.listCrystalStones;
                }

                /// Tạo mới danh sách Huyền Tinh tương ứng
                KTGlobal.listCrystalStones = Loader.Loader.Items.Where(x => x.Value.Genre == 18 && x.Value.DetailType == 1 && (x.Value.ParticularType == 1 || x.Value.ParticularType == 114)).OrderByDescending(x => x.Value.Level).ToDictionary(t => t.Key, t => t.Value);

                /// Trả về giá trị
                return KTGlobal.listCrystalStones;
            }
        }

        /// <summary>
        /// Kiểm tra Leader có vật phẩm tương ứng trong người không
        /// </summary>
        /// <param name="itemID"></param>
        /// <returns></returns>
        public static bool HaveItem(int itemID)
        {
            GoodsData itemGD = Global.Data.RoleData.GoodsDataList?.Where(x => x.GoodsID == itemID && x.GCount > 0).FirstOrDefault();
            return itemGD != null;
        }

        #endregion Medicine

        #region Cường hóa

        private const int MIN_COMMON_EQUIP = 0;
        private const int MAX_COMMON_EQUIP = 13;

        /// <summary>
        /// Kiểm tra trang bị có cường hóa được không
        /// </summary>
        /// <param name="itemData"></param>
        /// <returns></returns>
        public static bool CanEquipEnhance(ItemData itemData)
        {
            return itemData.DetailType >= MIN_COMMON_EQUIP && itemData.DetailType <= MAX_COMMON_EQUIP;
        }

        /// <summary>
        /// Tính toán cấp cường hóa tối đa của trang bị
        /// </summary>
        /// <param name="itemData"></param>
        /// <returns></returns>
        public static int CalculateEquipMaxEnhanceLevel(ItemData itemData)
        {
            int nMax;
            int nLevel = itemData.Level;

            if (nLevel <= 3)
            {
                nMax = 4;
            }
            else if (nLevel <= 6)
            {
                nMax = 8;
            }
            else if (nLevel <= 8)
            {
                nMax = 12;
            }
            else if (nLevel <= 9)
            {
                nMax = 14;
            }
            else
            {
                /// Nếu là trang bị luyện hóa hoặc vũ khí hoàng kim
                if (KTGlobal.IsRefinedEquip(itemData) || itemData.IsArtifact)
                {
                    nMax = 16;
                }
                else
                {
                    nMax = 14;
                }
            }

            return nMax;
        }

        /// <summary>
        /// Tính toán tỷ lệ, số bạc mất khi cường hóa trang bị
        /// </summary>
        /// <param name="crystalStones"></param>
        /// <param name="equipGD"></param>
        /// <returns></returns>
        public static CalcProb GetEquipEnhanceRequirement(List<GoodsData> crystalStones, GoodsData equipGD)
        {
            CalcProb prop = new CalcProb();
            prop.nMoney = 0;
            prop.nProb = 0;
            prop.nTrueProb = 0;

            /// Tổng giá trị cường hóa có được từ Huyền Tinh
            long allValue = 0;

            /// Duyệt danh sách Huyền Tinh và tính toán tỷ lệ
            foreach (GoodsData itemGD in crystalStones)
            {
                if (Loader.Loader.Items.TryGetValue(itemGD.GoodsID, out ItemData itemData))
                {
                    if (KTGlobal.ListCrystalStones.ContainsKey(itemGD.GoodsID))
                    {
                        allValue += itemData.ItemValue;
                    }
                }
                else
                {
                    return prop;
                }
            }

            /// Nếu không tồn tại trang bị tương ứng
            if (!Loader.Loader.Items.TryGetValue(equipGD.GoodsID, out ItemData equipItemData))
            {
                return prop;
            }
            else if (!KTGlobal.IsItemEquip(equipItemData.Genre) && !KTGlobal.IsPetEquip(equipItemData.Genre))
            {
                return prop;
            }

            /// Cấp cường hóa hiện tại
            int currentEnhanceLevel = equipGD.Forge_level;

            /// Cấp cường hóa tiếp theo
            int nextEnhanceLevel = currentEnhanceLevel + 1;

            /// Giá trị cường hóa cần
            long needValue = 0;

            /// Giá trị cường hóa cấp kế
            Enhance_Value enhanceValue = Loader.Loader.ItemValues.List_Enhance_Value.Where(x => x.EnhanceTimes == nextEnhanceLevel).FirstOrDefault();
            /// Nếu tìm thấy
            if (enhanceValue != null)
            {
                needValue = enhanceValue.Value;
            }

            /// Tỷ lệ
            double nTypeRate = 100;
            /// Tỷ lệ tương ứng cần
            Equip_Type_Rate typeRate = Loader.Loader.ItemValues.List_Equip_Type_Rate.Where(x => (int)x.EquipType == equipItemData.DetailType).FirstOrDefault();
            if (typeRate != null)
            {
                nTypeRate = typeRate.Value;
            }
            nTypeRate = nTypeRate / 100;

            /// Số bạc cần
            double nCostValue = nTypeRate * needValue;
            long nMoney = (long)(nCostValue * 0.1);
            nCostValue = nCostValue - nMoney;

            /// Tính ra tỷ lệ tương ứng
            double nProb = Math.Floor(allValue / nCostValue * 100);
            /// Tỷ lệ thực
            double nTrueProb = nProb;
            if (nProb > 100)
            {
                nProb = 100;
            }

            /// Cập nhật giá trị vào Prop
            prop.nMoney = (long)(nMoney*3.6);
            prop.nProb = nProb;
            prop.nTrueProb = nTrueProb;

            /// Trả về kết quả
            return prop;
        }

        /// <summary>
        /// Trả về danh sách sản phẩm hợp thành Huyền Tinh và phí tương ứng mất
        /// </summary>
        /// <param name="crystalStones"></param>
        /// <param name="isBound"></param>
        /// <returns></returns>
        public static ComposeItem GetComposeCrystalStonesProduct(List<GoodsData> crystalStones, bool isBound)
        {
            /// Kết quả tương ứng
            ComposeItem composeItem = new ComposeItem();
            composeItem.nFee = 0;
            composeItem.nMaxLevelRate = 0;
            composeItem.nMinLevelRate = 0;

            /// Tổng giá trị của Huyền Tinh
            int allValue = 0;

            /// Duyệt danh sách Huyền Tinh
            foreach (GoodsData itemGD in crystalStones)
            {
                if (Loader.Loader.Items.TryGetValue(itemGD.GoodsID, out ItemData itemData))
                {
                    if (KTGlobal.ListCrystalStones.ContainsKey(itemGD.GoodsID))
                    {
                        /// Nếu đầu vào khóa
                        if (itemGD.Binding == 1)
                        {
                            /// Sản phẩm sẽ khóa
                            isBound = true;
                        }
                        allValue += itemData.ItemValue;
                    }
                    else
                    {
                        return composeItem;
                    }
                }
            }

            /// Cấp độ sản phẩm 1
            int nMinLevel = 0;
            /// Cấp độ sản phẩm 2
            int nMaxLevel = 0;

            /// Tỷ lệ ra sản phẩm 1
            int nMinLevelRate = 0;
            /// Tỷ lệ ra sản phẩm 2
            int nMaxLevelRate = 0;

            /// Duyệt danh sách Huyền Tinh, tìm ra Huyền Tinh có cấp cao nhất thỏa mãn hợp thành được
            foreach (ItemData item in KTGlobal.ListCrystalStones.Values)
            {
                if (allValue >= item.ItemValue)
                {
                    nMinLevel = item.Level;
                    break;
                }
            }
            /// Thiết lập cấp độ sản phẩm 2
            nMaxLevel = nMinLevel + 1;

            /// Sản phẩm 1
            ItemData nMinLevelItem = KTGlobal.ListCrystalStones.Values.Where(x => x.Level == nMinLevel).FirstOrDefault();
            /// Sản phẩm 2
            ItemData nMaxLevelItem = KTGlobal.ListCrystalStones.Values.Where(x => x.Level == nMaxLevel).FirstOrDefault();

            /// Số bạc cần
            int nMoneyNeed = allValue / 10;

            /// Tính toán tỷ lệ
            if (nMinLevel >= 12)
            {
                nMinLevel = 11;
                nMinLevelRate = 0;
                nMaxLevelRate = 1;
            }
            else
            {
                nMinLevelRate = nMaxLevelItem.ItemValue - allValue;
                nMaxLevelRate = allValue - nMinLevelItem.ItemValue;

                /// Quy đổi ra đơn vị % tương ứng
                float nMinLevelRateP = nMinLevelRate * 100f / (nMinLevelRate + nMaxLevelRate);
                float nMaxLevelRateP = nMaxLevelRate * 100f / (nMinLevelRate + nMaxLevelRate);
                if (nMinLevelRateP - (int)nMinLevelRateP >= 0.5f)
                {
                    nMinLevelRateP += 1f;
                }
                if (nMaxLevelRateP - (int)nMaxLevelRateP >= 0.5f)
                {
                    nMaxLevelRateP += 1f;
                }

                nMinLevelRate = (int)nMinLevelRateP;
                nMaxLevelRate = (int)nMaxLevelRateP;
            }

            /// Cập nhật thông tin sản phẩm
            composeItem.nItemMinLevel = nMinLevelItem;
            composeItem.nItemMaxLevel = nMaxLevelItem;
            composeItem.nMinLevelRate = nMinLevelRate;
            composeItem.nMaxLevelRate = nMaxLevelRate;
            composeItem.nFee = nMoneyNeed;

            /// Trả về kết quả
            return composeItem;
        }

        /// <summary>
        /// Trả về danh sách Huyền Tinh có được sau khi tách khỏi trang bị tương ứng
        /// </summary>
        /// <param name="equipGD"></param>
        /// <param name="nRate"></param>
        /// <returns></returns>
        public static List<ItemData> GetCrystalStonesBySplitingEquip(GoodsData equipGD, float nRate)
        {
            /// Thông tin vật phẩm tương ứng
            if (!Loader.Loader.Items.TryGetValue(equipGD.GoodsID, out ItemData itemData))
            {
                return new List<ItemData>();
            }

            /// Giá trị cường hóa ở cấp tương ứng
            List<Enhance_Value> enhanceValues = Loader.Loader.ItemValues.List_Enhance_Value.Where(x => x.EnhanceTimes <= equipGD.Forge_level).ToList();

            /// Tỷ lệ này tương đương với sự khác biệt giữa vũ khí, phòng cụ và trang sức
            double nTypeRate = 100;
            Equip_Type_Rate rate = Loader.Loader.ItemValues.List_Equip_Type_Rate.Where(x => (int)x.EquipType == itemData.DetailType).FirstOrDefault();
            if (rate != null)
            {
                nTypeRate = rate.Value;
            }
            /// Chia 100 cho cẩn thận
            nTypeRate /= 100;

            /// Tổng tài phú tương ứng
            double nValue = enhanceValues.Sum(x => x.Value) * nTypeRate * nRate;

            /// Danh sách Huyền Tinh có được
            List<ItemData> outputCrystalStones = new List<ItemData>();

            /// Huyền Tinh tương ứng cấp nhỏ nhất
            ItemData crystalLevel1 = KTGlobal.ListCrystalStones.Values.Where(x => x.Level == 1).FirstOrDefault();

            /// Tối đa tách ra 10 huyền tinh cấp thấp hơn
            for (int nCount = 1; nCount < 10; nCount++)
            {
                /// Duyệt danh sách Huyền Tinh
                foreach (ItemData item in KTGlobal.ListCrystalStones.Values)
                {
                    /// Nếu không trùng trạng thái khóa
                    //if (item.BindType != equipGD.Binding)
                    //{
                    //    continue;
                    //}

                    if (nValue / item.ItemValue > 1)
                    {
                        double nNum = Math.Floor(nValue / item.ItemValue);
                        if (nNum > 1)
                        {
                            for (int i = 0; i < nNum; i++)
                            {
                                /// Chỉ lấy các huyền tinh có level > 4 còn lại thì người chơi phải chịu lỗ VALUE coi như phí
                                if (item.Level > 3)
                                {
                                    outputCrystalStones.Add(item);
                                }
                            }

                            nValue %= item.ItemValue;
                            break;
                        }
                    }
                }
                if ((nValue / crystalLevel1.ItemValue < 1) || nValue == 0)
                {
                    break;
                }
            }

            /// Trả về kết quả
            return outputCrystalStones;
        }

        /// <summary>
        /// Trả về số Huyền Tinh có được bằng việc tách Huyền Tinh cấp cao
        /// </summary>
        /// <param name="InputCrystal"></param>
        /// <param name="_Input"></param>
        /// <returns></returns>
        public static List<ItemData> GetCrystalStonesBySplitingHighLevelCrystalStone(GoodsData crystalStoneGD, float nRate)
        {
            /// Danh sách kết quả
            List<ItemData> outputCrystalStones = new List<ItemData>();

            /// Thông tin Huyền Tinh tương ứng
            if (!Loader.Loader.Items.TryGetValue(crystalStoneGD.GoodsID, out ItemData itemData))
            {
                return outputCrystalStones;
            }

            /// Giá trị có thể tách
            double nValue = itemData.ItemValue * nRate;

            /// Huyền Tinh tương ứng cấp nhỏ nhất
            ItemData crystalLevel1 = KTGlobal.ListCrystalStones.Values.Where(x => x.Level == 1).FirstOrDefault();

            /// Tối đa tách ra 10 huyền tinh cấp thấp hơn
            for (int nCount = 1; nCount < 10; nCount++)
            {
                /// Duyệt danh sách Huyền Tinh
                foreach (ItemData item in KTGlobal.ListCrystalStones.Values)
                {
                    /// Nếu không trùng trạng thái khóa
                    //if (item.BindType != crystalStoneGD.Binding)
                    //{
                    //    continue;
                    //}

                    if (nValue / item.ItemValue > 1)
                    {
                        double nNum = Math.Floor(nValue / item.ItemValue);
                        if (nNum > 1)
                        {
                            for (int i = 0; i < nNum; i++)
                            {
                                /// Chỉ lấy các huyền tinh có level > 4 còn lại thì người chơi phải chịu lỗ VALUE coi như phí
                                if (item.Level > 3)
                                {
                                    outputCrystalStones.Add(item);
                                }
                            }

                            nValue %= item.ItemValue;
                            break;
                        }
                    }
                }
                if ((nValue / crystalLevel1.ItemValue < 1) || nValue == 0)
                {
                    break;
                }
            }

            /// Trả về kết quả
            return outputCrystalStones;
        }

        /// <summary>
        /// Trả về tổng số vật phẩm có trong túi đồ
        /// </summary>
        /// <param name="itemID"></param>
        /// <returns></returns>
        public static int GetItemCountInBag(int itemID)
        {
            if (Global.Data.RoleData.GoodsDataList == null)
            {
                return 0;
            }
            return Global.Data.RoleData.GoodsDataList.Where(x => x.GoodsID == itemID).Sum(x => x.GCount);
        }

        /// <summary>
        /// Kiểm tra vật phẩm có ID tương ứng có dùng được không
        /// </summary>
        /// <param name="itemID"></param>
        /// <returns></returns>
        public static bool IsItemCanUse(int itemID)
        {
            /// Nếu vật phẩm không tồn tại
            if (!Loader.Loader.Items.TryGetValue(itemID, out ItemData itemData))
            {
                return false;
            }

            /// Nếu là thuốc
            if (itemData.IsMedicine)
            {
                return true;
            }
            /// Nếu là ScriptItem
            else if (itemData.IsScriptItem)
            {
                return true;
            }

            /// Trả ra không thể dùng được
            return false;
        }

        /// <summary>
        /// Có phải phiếu giảm giá Kỳ Trân Các không
        /// </summary>
        /// <param name="itemID"></param>
        /// <returns></returns>
        public static bool IsDiscountCoupon(int itemID)
        {
            /// Nếu không tồn tại
            if (!Loader.Loader.Items.TryGetValue(itemID, out ItemData itemData))
            {
                return false;
            }
            /// Trả về kết quả
            return itemData.ParticularType >= 1550 && itemData.ParticularType <= 1554;
        }

        #endregion Cường hóa

        #region Vỏ sò

        /// <summary>
        /// ID vỏ sò vàng
        /// </summary>
        public const int SeashellItemID = 746;

        /// <summary>
        /// ID rương xấu xí
        /// </summary>
        public const int SeashellTreasureID = 745;

        #endregion Vỏ sò

        #region Ngũ Hành Ấn

        /// <summary>
        /// ID vật phẩm Ngũ Hành Hồn Thạch
        /// </summary>
        public const int FiveElementStoneID = 506;

        /// <summary>
        /// Kiểm tra vật phẩm có phải Ngũ Hành Ấn không
        /// </summary>
        /// <param name="itemID"></param>
        /// <returns></returns>
        public static bool IsSignet(int itemID)
        {
            /// Nếu không tồn tại
            if (!Loader.Loader.Items.TryGetValue(itemID, out ItemData itemData))
            {
                return false;
            }
            /// Trả về kết quả
            return itemData.DetailType == (int)KE_ITEM_EQUIP_DETAILTYPE.equip_signet;
        }

        #endregion Ngũ Hành Ấn

        #region Luyện hóa trang bị

        public static float PEEL_RESTORE_RATE_12 = 3 / 100f;
        public static float PEEL_RESTORE_RATE_14 = 5 / 100f;
        public static float ENHANCE_COST_RATE = 10 / 100f;

        /// <summary>
        /// Trả về tổng cường hóa
        /// </summary>
        /// <param name="itemGD"></param>
        /// <returns></returns>
        private static double CalcEnhanceValue(GoodsData itemGD)
        {
            /// Nếu vật phẩm không tồn tại
            if (!Loader.Loader.Items.TryGetValue(itemGD.GoodsID, out ItemData itemData))
            {
                return 0;
            }

            /// Cấp cường hóa
            int nEnhTimes = itemGD.Forge_level;
            /// Tổng giá trị
            double nPeelValue = 0;

            /// Nếu có cường
            if (nEnhTimes > 0)
            {
                nPeelValue = Loader.Loader.ItemValues.List_Enhance_Value.Where(x => x.EnhanceTimes <= nEnhTimes).Sum(x => x.Value);
            }

            Equip_Type_Rate typeRate = Loader.Loader.ItemValues.List_Equip_Type_Rate.Where(x => (int)x.EquipType == itemData.DetailType).FirstOrDefault();
            if (typeRate != null)
            {
                double nRate = ((double)typeRate.Value / 100);
                nPeelValue = nPeelValue * nRate;
            }

            return nPeelValue;
        }

        /// <summary>
        /// Tính số tiền cần để luyện hóa trang bị tương ứng
        /// </summary>
        /// <param name="itemGD"></param>
        /// <returns></returns>
        public static int CalcRefineMoney(GoodsData itemGD)
        {
            /// Số tiền cần
            double nMoneyNeed = 100;

            /// Nếu vật phẩm không tồn tại
            if (!Loader.Loader.Items.TryGetValue(itemGD.GoodsID, out ItemData itemData))
            {
                return (int)nMoneyNeed;
            }

            if (KTGlobal.IsEquip(itemData.ItemID))
            {
                double nEnhanceValue = KTGlobal.CalcEnhanceValue(itemGD);
                int nEnhLevel = itemGD.Forge_level;
                double nRefineMoney;
                int nJbPrice = 1;

                if (nEnhLevel >= 12 && nEnhLevel <= 13)
                {
                    nRefineMoney = nEnhanceValue * ENHANCE_COST_RATE * nJbPrice - nEnhanceValue * PEEL_RESTORE_RATE_12;
                }
                else if (nEnhLevel >= 14 && nEnhLevel <= 16)
                {
                    nRefineMoney = nEnhanceValue * ENHANCE_COST_RATE * nJbPrice - nEnhanceValue * PEEL_RESTORE_RATE_14;
                }
                else
                {
                    nRefineMoney = nEnhanceValue * ENHANCE_COST_RATE * nJbPrice;
                }

                nMoneyNeed = nRefineMoney;
            }

            return (int)nMoneyNeed;
        }

        /// <summary>
        /// Tính toán tỷ lệ luyện hóa
        /// </summary>
        /// <param name="itemGD"></param>
        /// <param name="cyrstalStones"></param>
        /// <returns></returns>
        public static int CalculateRefineRate(GoodsData itemGD, List<GoodsData> cyrstalStones)
        {
            /// Tổng tài phú của danh sách Huyền Tinh
            int nCrytalValue = cyrstalStones.Sum((gd) =>
            {
                if (Loader.Loader.Items.TryGetValue(gd.GoodsID, out ItemData itemData))
                {
                    return itemData.ItemValue;
                }
                return 0;
            });

            double nPercent = 0;
            if (itemGD.Forge_level > 0)
            {
                nPercent = Math.Floor((KTGlobal.CalcEnhanceValue(itemGD) * 8 + nCrytalValue * 10) / (KTGlobal.CalcEnhanceValue(itemGD) * 9) * 100);
            }
            else
            {
                nPercent = 100;
            }

            /// Trả ra kết quả
            return (int)nPercent;
        }

        /// <summary>
        /// Kiểm tra trang bị tương ứng có luyện hóa được không
        /// </summary>
        /// <param name="itemData"></param>
        /// <returns></returns>
        public static bool IsRefinedEquip(ItemData itemData)
        {
            /// Toác
            if (itemData == null)
            {
                return false;
            }

            return Loader.Loader.EquipRefineRecipes.Any(x => x.ProductEquipID == itemData.ItemID);
        }

        #endregion Luyện hóa trang bị

        #region Tách trang bị chế

        /// <summary>
        /// Kiểm tra trang bị có thể tách được thành Ngũ Hành Hồn Thạch không
        /// </summary>
        /// <param name="equipGD"></param>
        /// <returns></returns>
        public static bool IsEquipAbleToRefineIntoFS(GoodsData equipGD)
        {
            /// Nếu không phải trang bị
            if (!KTGlobal.IsEquip(equipGD.GoodsID))
            {
                return false;
            }
            /// Nếu trang bị có cường hóa
            if (equipGD.Forge_level > 0)
            {
                return false;
            }
            /// Nếu thông tin trang bị không tồn tại
            if (!Loader.Loader.Items.TryGetValue(equipGD.GoodsID, out ItemData itemData))
            {
                return false;
            }

            /// Loại trang bị
            KE_ITEM_EQUIP_DETAILTYPE itemType = KTGlobal.GetEquipType(itemData.DetailType);
            /// Nếu là Phi Phong
            if (itemType == KE_ITEM_EQUIP_DETAILTYPE.equip_mantle)
            {
                /// Nếu có hạn sử dụng
                if (!string.IsNullOrEmpty(equipGD.Endtime))
                {
                    /// Thời gian hết hạn
                    DateTime endTime = DateTime.Parse(equipGD.Endtime);
                    if (endTime > DateTime.Now)
                    {
                        TimeSpan diff = endTime - DateTime.Now;
                        int diffSec = (int)diff.TotalSeconds;

                        /// Toạch
                        if (diffSec == -1)
                        {
                            return false;
                        }
                        /// Nếu còn dưới 1 ngày thì toạch
                        else if (diffSec < 86400)
                        {
                            return false;
                        }
                        else
                        {
                            /// Thỏa mãn
                            return true;
                        }
                    }
                    else
                    {
                        /// Toạch
                        return false;
                    }
                }
                /// Nếu không có hạn sử dụng thì ok luôn
                else
                {
                    return true;
                }
            }

            /// Nếu không phải trang bị chế
            if (string.IsNullOrEmpty(equipGD.Creator))
            {
                return false;
            }
            /// Nếu đã khóa
            if (equipGD.Binding == 1)
            {
                return false;
            }
            /// Nếu trang bị dưới cấp 7 thì không tách được
            if (itemData.Level < 7)
            {
                return false;
            }
            /// Nếu không phân giải được
            if (itemData.Price / 1000 <= 0)
            {
                return false;
            }
            /// Trả về kết qủa
            return true;
        }

        /// <summary>
        /// Trả về tổng số Ngũ hành hồn thạch nhận được khi phân giải trang bị
        /// </summary>
        /// <param name="equipGD"></param>
        /// <returns></returns>
        public static int CalculateTotalFSByRefiningEquip(GoodsData equipGD)
        {
            /// Nếu không thể tách
            if (!KTGlobal.IsEquipAbleToRefineIntoFS(equipGD))
            {
                return 0;
            }
            /// Thông tin trang bị
            Loader.Loader.Items.TryGetValue(equipGD.GoodsID, out ItemData itemData);

            /// Loại trang bị
            KE_ITEM_EQUIP_DETAILTYPE itemType = KTGlobal.GetEquipType(itemData.DetailType);
            /// Nếu là Phi Phong
            if (itemType == KE_ITEM_EQUIP_DETAILTYPE.equip_mantle)
            {
                /// Hạn dùng Phi Phong
                int maxDays = 30;
                /// Nếu có hạn sử dụng
                if (!string.IsNullOrEmpty(equipGD.Endtime))
                {
                    /// Thời gian hết hạn
                    DateTime endTime = DateTime.Parse(equipGD.Endtime);
                    if (endTime > DateTime.Now)
                    {
                        TimeSpan diff = endTime - DateTime.Now;
                        int diffSec = (int)diff.TotalSeconds;

                        /// Toạch
                        if (diffSec == -1)
                        {
                            return 0;
                        }
                        /// Nếu còn dưới 1 ngày thì toạch
                        else if (diffSec < 86400)
                        {
                            return 0;
                        }
                        else
                        {
                            /// Số ngày còn lại
                            int daysLeft = diffSec / 86400;

                            /// Số Ngũ Hành Hồn Thạch đổi được = số ngày còn lại
                            return daysLeft * itemData.Price / 1000 / maxDays;
                        }
                    }
                    else
                    {
                        /// Toạch
                        return 0;
                    }
                }
                /// Nếu không có hạn sử dụng thì trả ra giá trị Max
                else
                {
                    return itemData.Price / 1000;
                }
            }

            /// Số lượng ngũ hành hồn thạch nhận lại
            int fsCount = itemData.Price / 1000;

            /// Trả về kết quả
            return fsCount;
        }

        #endregion Tách trang bị chế
    }
}