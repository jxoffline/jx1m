using FS.GameEngine.Logic;
using FS.VLTK.Entities.Config;
using FS.VLTK.Network;
using FS.VLTK.UI.Main;
using Server.Data;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FS.VLTK.Logic.Settings
{
    /// <summary>
    /// Khung thiết lập tự đánh
    /// </summary>
    public static partial class KTAutoAttackSetting
    {
        /// <summary>
        /// Thiết lập Auto
        /// </summary>
        public static AutoSettingData Config { get; set; } = new AutoSettingData();

        /// <summary>
        /// Thiết lập mặc định
        /// </summary>
        public static void SetDefaultConfig()
        {
            /// Tạo mới thiết lập Auto
            KTAutoAttackSetting.Config = new AutoSettingData();

            /// Chung
            {
                /// Tạo mới
                KTAutoAttackSetting.Config.General = new AutoSettingData.AutoGeneral();

                /// Thiết lập mặc định
                KTAutoAttackSetting.Config.EnableAutoPK = false;
                KTAutoAttackSetting.Config.General.RefuseExchange = false;
                KTAutoAttackSetting.Config.General.RefuseChallenge = false;
                KTAutoAttackSetting.Config.General.RefuseTeam = false;
            }

            /// Auto hỗ trợ
            {
                /// Tạo mới
                KTAutoAttackSetting.Config.Support = new AutoSettingData.AutoSupport();

                /// Thiết lập mặc định
                KTAutoAttackSetting.Config.Support.EM_AutoHeal = true;
                KTAutoAttackSetting.Config.Support.EM_AutoHealHPPercent = 20;
                KTAutoAttackSetting.Config.Support.AutoHPPercent = 50;
                KTAutoAttackSetting.Config.Support.AutoHP = true;
                KTAutoAttackSetting.Config.Support.AutoMPPercent = 50;
                KTAutoAttackSetting.Config.Support.AutoMP = true;
                KTAutoAttackSetting.Config.Support.MPMedicine = -1;
                KTAutoAttackSetting.Config.Support.HPMedicine = -1;
                KTAutoAttackSetting.Config.Support.AutoPetHPPercent = 50;
                KTAutoAttackSetting.Config.Support.AutoPetHP = true;
                KTAutoAttackSetting.Config.Support.PetHPMedicine = -1;
                KTAutoAttackSetting.Config.Support.AutoPetLife = true;
                KTAutoAttackSetting.Config.Support.AutoPetJoy = true;
                KTAutoAttackSetting.Config.Support.AutoCallPet = true;

                KTAutoAttackSetting.Config.Support.AutoX2 = true;
                KTAutoAttackSetting.Config.Support.AutoBuyMedicines = true;
                KTAutoAttackSetting.Config.Support.AutoBuyMedicinesQuantity = 100;
                KTAutoAttackSetting.Config.Support.AutoBuyMedicinesUsingBoundMoneyPriority = true;
            }

            /// Auto đánh quái
            {
                /// Tạo mới
                KTAutoAttackSetting.Config.Farm = new AutoSettingData.AutoFarm();

                /// Thiết lập mặc định
                KTAutoAttackSetting.Config.Farm.SingleTarget = true;
                KTAutoAttackSetting.Config.Farm.AutoFireCamp = false;
                KTAutoAttackSetting.Config.Farm.LowHPTargetPriority = false;
                KTAutoAttackSetting.Config.Farm.FarmAround = true;
                KTAutoAttackSetting.Config.Farm.IgnoreBoss = false;
                KTAutoAttackSetting.Config.Farm.ScanRange = 200;
                KTAutoAttackSetting.Config.Farm.Skills = new List<int>();
                /// Tự thêm skill rỗng
                for (int i = 1; i <= UIAutoFight.NumberOfSkills; i++)
                {
                    KTAutoAttackSetting.Config.Farm.Skills.Add(-1);
                }
                KTAutoAttackSetting.Config.Farm.UseNewbieSkill = true;
            }

            /// Auto PK
            {
                /// Tạo mới
                KTAutoAttackSetting.Config.PK = new AutoSettingData.AutoPK();

                /// Thiết lập mặc định
                KTAutoAttackSetting.Config.PK.AutoReflect = true;
                KTAutoAttackSetting.Config.PK.SeriesConquarePriority = false;
                KTAutoAttackSetting.Config.PK.LowHPTargetPriority = false;
                KTAutoAttackSetting.Config.PK.Skills = new List<int>();
                KTAutoAttackSetting.Config.PK.AutoAccectJoinTeam = false;
                KTAutoAttackSetting.Config.PK.AutoInviteToTeam = false;
                KTAutoAttackSetting.Config.PK.Skills = new List<int>();
                /// Tự thêm skill rỗng
                for (int i = 1; i <= UIAutoFight.NumberOfSkills; i++)
                {
                    KTAutoAttackSetting.Config.PK.Skills.Add(-1);
                }
                KTAutoAttackSetting.Config.PK.UseNewbieSkill = false;
                KTAutoAttackSetting.Config.PK.ChaseTarget = true;
            }

            /// Auto nhặt và bán vật phẩm
            {
                /// Tạo mới
                KTAutoAttackSetting.Config.PickAndSell = new AutoSettingData.AutoPickAndSellItem();

                /// Thiết lập mặc định
                KTAutoAttackSetting.Config.PickAndSell.EnableAutoPickUp = true;
                KTAutoAttackSetting.Config.PickAndSell.ScanRadius = 100;
                KTAutoAttackSetting.Config.PickAndSell.PickUpCrystalStoneOnly = false;
                KTAutoAttackSetting.Config.PickAndSell.PickUpCrystalStoneLevel = 1;
                KTAutoAttackSetting.Config.PickAndSell.AutoSortBag = true;
                KTAutoAttackSetting.Config.PickAndSell.PickUpEquip = false;
                KTAutoAttackSetting.Config.PickAndSell.PickUpEquipStar = 1;
                KTAutoAttackSetting.Config.PickAndSell.PickUpItemByLinesCount = 1;
                KTAutoAttackSetting.Config.PickAndSell.AutoSellItems = true;
                KTAutoAttackSetting.Config.PickAndSell.SellEquipStar = 4;
            }

            /// Chuyển về Byte
            byte[] byteArray = DataHelper.ObjectToBytes(Config);
            /// Chuyển về Base64
            string base64Encoding = Convert.ToBase64String(byteArray);

            /// Lưu thiết lập
            Global.Data.RoleData.AutoSettings = base64Encoding;

            /// Gửi yêu cầu lưu thiết lập Auto
            KT_TCPHandler.SendSaveAutoSettings();
        }

        /// <summary>
        /// Tạo thêm hàm này cho đỡ cost | Vì nếu mỗi lần nhặt phải fill data từ DICT thì sẽ rất ỉa chảy
        /// </summary>
        /// <param name="GoodPackID"></param>
        /// <returns></returns>
        public static bool IsCanPickCrytalItem(int GoodPackID, int MinLevelPick)
        {
            int ItemLevel = 0;

            if (GoodPackID >= 183 && GoodPackID <= 194)
            {
                ItemLevel = GoodPackID - 182;

                if (ItemLevel >= MinLevelPick)
                {
                    return true;
                }
            }
            else if (GoodPackID >= 385 && GoodPackID <= 396)
            {
                ItemLevel = GoodPackID - 384;

                if (ItemLevel >= MinLevelPick)
                {
                    return true;
                }
            }
            else
            {
                return false;
            }

            return false;
        }


        public static bool IsEquip(int GooldID)
        {
            return KTGlobal.IsEquip(GooldID);
        }

        public static bool IsCrytalItem(int GoodPackID)
        {
            if (GoodPackID >= 183 && GoodPackID <= 194)
            {
                return true;
            }
            else if (GoodPackID >= 385 && GoodPackID <= 396)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Thiết lập Auto
        /// </summary>
        public static void SetAutoConfig()
        {
            try
            {
                byte[] Base64Decode = Convert.FromBase64String(Global.Data.RoleData.AutoSettings);
                KTAutoAttackSetting.Config = DataHelper.BytesToObject<AutoSettingData>(Base64Decode, 0, Base64Decode.Length);
            }
            catch (Exception ex)
            {
                KTAutoAttackSetting.SetDefaultConfig();
            }

            KTAutoAttackSetting.SkillAutoTrain = new List<SkillDataEx>();

            /// Toác
            if (KTAutoAttackSetting.Config == null)
            {
                KTAutoAttackSetting.SetDefaultConfig();
            }

            /// Thành phần
            if (KTAutoAttackSetting.Config.Farm == null)
            {
                KTAutoAttackSetting.Config.Farm = new AutoSettingData.AutoFarm();
            }
            if (KTAutoAttackSetting.Config.PK == null)
            {
                KTAutoAttackSetting.Config.PK = new AutoSettingData.AutoPK();
            }
            if (KTAutoAttackSetting.Config.PickAndSell == null)
            {
                KTAutoAttackSetting.Config.PickAndSell = new AutoSettingData.AutoPickAndSellItem();
            }
            if (KTAutoAttackSetting.Config.General == null)
            {
                KTAutoAttackSetting.Config.General = new AutoSettingData.AutoGeneral();
            }
            if (KTAutoAttackSetting.Config.Support == null)
            {
                KTAutoAttackSetting.Config.Support = new AutoSettingData.AutoSupport();
            }

            /// Set skill để train quái
            foreach (int skillID in KTAutoAttackSetting.Config.Farm.Skills)
            {
                if (Loader.Loader.Skills.TryGetValue(skillID, out SkillDataEx skillData))
                {
                    SkillData dbSkill = Global.Data.RoleData.SkillDataList.Where(x => x.SkillID == skillID).FirstOrDefault();
                    /// Nếu tồn tại trên người và không phải kỹ năng bị động và vòng sáng
                    if (dbSkill != null && dbSkill.Level > 0 && skillData.Type != 3 && !skillData.IsArua)
                    {
                        KTAutoAttackSetting.SkillAutoTrain.Add(skillData);
                    }
                    else
                    {
                        KTAutoAttackSetting.SkillAutoTrain.Add(null);
                    }
                }
                else
                {
                    KTAutoAttackSetting.SkillAutoTrain.Add(null);
                }
            }

            KTAutoAttackSetting.SkillAutoPK = new List<SkillDataEx>();

            /// set skill để pk
            foreach (int skillID in KTAutoAttackSetting.Config.PK.Skills)
            {
                if (Loader.Loader.Skills.TryGetValue(skillID, out SkillDataEx skillData))
                {
                    SkillData dbSkill = Global.Data.RoleData.SkillDataList.Where(x => x.SkillID == skillID).FirstOrDefault();
                    /// Nếu tồn tại trên người và không phải kỹ năng bị động và vòng sáng
                    if (dbSkill != null && dbSkill.Level > 0 && skillData.Type != 3 && !skillData.IsArua)
                    {
                        KTAutoAttackSetting.SkillAutoPK.Add(skillData);
                    }
                    else
                    {
                        KTAutoAttackSetting.SkillAutoPK.Add(null);
                    }
                }
                else
                {
                    KTAutoAttackSetting.SkillAutoPK.Add(null);
                }
            }
        }

        /// <summary>
        /// Thiết lập danh sách kỹ năng dùng khi Train
        /// </summary>
        /// <param name="skills"></param>
        public static void SetSkillTrain(List<int> skills)
        {
            for (int i = 0; i < skills.Count; i++)
            {
                if (Loader.Loader.Skills.TryGetValue(skills[i], out SkillDataEx skillData))
                {
                    KTAutoAttackSetting.SkillAutoTrain[i] = skillData;
                }
                else
                {
                    KTAutoAttackSetting.SkillAutoTrain[i] = null;
                }
            }

            /// Lưu lại vào Config
            KTAutoAttackSetting.Config.Farm.Skills = skills;
        }

        /// <summary>
        /// Thiết lập danh sách kỹ năng dùng khi PK
        /// </summary>
        /// <param name="skills"></param>
        public static void SetSkillPK(List<int> skills)
        {
            for (int i = 0; i < skills.Count; i++)
            {
                if (Loader.Loader.Skills.TryGetValue(skills[i], out SkillDataEx skillData))
                {
                    KTAutoAttackSetting.SkillAutoPK[i] = skillData;
                }
                else
                {
                    KTAutoAttackSetting.SkillAutoPK[i] = null;
                }
            }

            /// Lưu lại vào Config
            KTAutoAttackSetting.Config.PK.Skills = skills;
        }

        /// <summary>
        /// Danh sách kỹ năng sử dụng để train quái
        /// </summary>
        public static List<SkillDataEx> SkillAutoTrain { get; set; }

        /// <summary>
        /// Danh sách kỹ năng đã sử dụng để AUTO PK
        /// </summary>
        public static List<SkillDataEx> SkillAutoPK { get; set; }

        #region Public methods

        /// <summary>
        /// Chuyển thành dạng chuỗi mã hóa lưu vào DB
        /// </summary>
        /// <returns></returns>
        public static void SaveSettings()
        {
            byte[] byteArray = DataHelper.ObjectToBytes(KTAutoAttackSetting.Config);
            string base64 = Convert.ToBase64String(byteArray);
            Global.Data.RoleData.AutoSettings = base64;
            KT_TCPHandler.SendSaveAutoSettings();
        }

        #endregion Public methods
    }
}