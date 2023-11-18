using FS.GameEngine.Logic;
using FS.GameFramework.Logic;
using FS.VLTK;
using FS.VLTK.Entities.Config;
using FS.VLTK.Loader;
using FS.VLTK.Logic;
using FS.VLTK.Logic.Settings;
using FS.VLTK.UI;
using FS.VLTK.UI.Main;
using System.Linq;

/// <summary>
/// Quản lý các khung giao diện trong màn chơi
/// </summary>
public partial class PlayZone
{
    #region Thiết lập Auto
    /// <summary>
    /// Khung thiết lập Auto
    /// </summary>
    public UIAutoFight UIAutoFight { get; protected set; } = null;

    /// <summary>
    /// Mở khung thiết lập tự đánh
    /// </summary>
    public void OpenUIAutoFight()
	{
        /// Nếu khung đang hiển thị
        if (this.UIAutoFight != null)
        {
            return;
        }

        CanvasManager canvas = Global.MainCanvas.GetComponent<CanvasManager>();
        this.UIAutoFight = canvas.LoadUIPrefab<UIAutoFight>("MainGame/UIAutoFight");
        canvas.AddUI(this.UIAutoFight);

        #region Dữ liệu
        /// Chung
        this.UIAutoFight.General.EnableAutoPK = KTAutoAttackSetting.Config.EnableAutoPK;
        this.UIAutoFight.General.RefuseChallenge = KTAutoAttackSetting.Config.General.RefuseChallenge;
        this.UIAutoFight.General.RefuseExchange = KTAutoAttackSetting.Config.General.RefuseExchange;
        this.UIAutoFight.General.RefuseTeam = KTAutoAttackSetting.Config.General.RefuseTeam;

        /// Auto support
        this.UIAutoFight.AutoSupport.AutoHP = KTAutoAttackSetting.Config.Support.AutoHP;
        this.UIAutoFight.AutoSupport.AutoMP = KTAutoAttackSetting.Config.Support.AutoMP;
        this.UIAutoFight.AutoSupport.AutoHPPercent = KTAutoAttackSetting.Config.Support.AutoHPPercent;
        this.UIAutoFight.AutoSupport.AutoMPPercent = KTAutoAttackSetting.Config.Support.AutoMPPercent;
        this.UIAutoFight.AutoSupport.AutoBuyMedicines = KTAutoAttackSetting.Config.Support.AutoBuyMedicines;
        this.UIAutoFight.AutoSupport.AutoBuyMedicinesQuantity = KTAutoAttackSetting.Config.Support.AutoBuyMedicinesQuantity;
        this.UIAutoFight.AutoSupport.AutoBuyMedicinesUsingBoundMoneyPriority = KTAutoAttackSetting.Config.Support.AutoBuyMedicinesUsingBoundMoneyPriority;

        this.UIAutoFight.AutoSupport.AutoX2 = KTAutoAttackSetting.Config.Support.AutoX2;

        this.UIAutoFight.AutoSupport.HPMedicineID = KTAutoAttackSetting.Config.Support.HPMedicine;
        this.UIAutoFight.AutoSupport.MPMedicineID = KTAutoAttackSetting.Config.Support.MPMedicine;
        this.UIAutoFight.AutoSupport.EM_AutoHeal = KTAutoAttackSetting.Config.Support.EM_AutoHeal;
        this.UIAutoFight.AutoSupport.EM_AutoHealHPPercent = KTAutoAttackSetting.Config.Support.EM_AutoHealHPPercent;
        this.UIAutoFight.AutoSupport.AutoPetHPPercent = KTAutoAttackSetting.Config.Support.AutoPetHPPercent;
        this.UIAutoFight.AutoSupport.AutoPetHP = KTAutoAttackSetting.Config.Support.AutoPetHP;
        this.UIAutoFight.AutoSupport.PetHPMedicineID = KTAutoAttackSetting.Config.Support.PetHPMedicine;
        this.UIAutoFight.AutoSupport.AutoPetLife = KTAutoAttackSetting.Config.Support.AutoPetLife;
        this.UIAutoFight.AutoSupport.AutoPetJoy = KTAutoAttackSetting.Config.Support.AutoPetJoy;
        this.UIAutoFight.AutoSupport.AutoCallPet = KTAutoAttackSetting.Config.Support.AutoCallPet;

        /// Auto farm
        this.UIAutoFight.AutoFarm.AutoFireCamp = KTAutoAttackSetting.Config.Farm.AutoFireCamp;
        this.UIAutoFight.AutoFarm.FarmAround = KTAutoAttackSetting.Config.Farm.FarmAround;
        this.UIAutoFight.AutoFarm.ScanRange = KTAutoAttackSetting.Config.Farm.ScanRange;
        this.UIAutoFight.AutoFarm.SingleTarget = KTAutoAttackSetting.Config.Farm.SingleTarget;
        this.UIAutoFight.AutoFarm.IgnoreBoss = KTAutoAttackSetting.Config.Farm.IgnoreBoss;
        this.UIAutoFight.AutoFarm.LowHPTargetPriority = KTAutoAttackSetting.Config.Farm.LowHPTargetPriority;
        this.UIAutoFight.AutoFarm.UseNewbieSkill = KTAutoAttackSetting.Config.Farm.UseNewbieSkill;
        this.UIAutoFight.AutoFarm.AutoUseWine = KTAutoAttackSetting.Config.Farm.AutoDrinkWine;
        this.UIAutoFight.AutoFarm.Skills = KTAutoAttackSetting.Config.Farm.Skills;

        /// Auto nhặt
        this.UIAutoFight.AutoPickUpItem.AutoPickUpItem = KTAutoAttackSetting.Config.PickAndSell.EnableAutoPickUp;
        this.UIAutoFight.AutoPickUpItem.PickUpRange = KTAutoAttackSetting.Config.PickAndSell.ScanRadius;
        this.UIAutoFight.AutoPickUpItem.PickUpCrystalStone = KTAutoAttackSetting.Config.PickAndSell.PickUpCrystalStoneOnly;
        this.UIAutoFight.AutoPickUpItem.PickUpCrystalStoneLevel = KTAutoAttackSetting.Config.PickAndSell.PickUpCrystalStoneLevel;
        this.UIAutoFight.AutoPickUpItem.PickUpEquip = KTAutoAttackSetting.Config.PickAndSell.PickUpEquip;
        this.UIAutoFight.AutoPickUpItem.PickUpEquipStar = KTAutoAttackSetting.Config.PickAndSell.PickUpEquipStar;
        this.UIAutoFight.AutoPickUpItem.PickUpEquipLinesCount = KTAutoAttackSetting.Config.PickAndSell.PickUpItemByLinesCount;
        this.UIAutoFight.AutoPickUpItem.AutoSortBag = KTAutoAttackSetting.Config.PickAndSell.AutoSortBag;
        this.UIAutoFight.AutoPickUpItem.AutoBackAndSellTrashes = KTAutoAttackSetting.Config.PickAndSell.AutoSellItems;
        this.UIAutoFight.AutoPickUpItem.AutoSellTrashEquipBelowStar = KTAutoAttackSetting.Config.PickAndSell.SellEquipStar;
        this.UIAutoFight.AutoPickUpItem.PickUpOtherItems = KTAutoAttackSetting.Config.PickAndSell.PickUpOtherItems;

        /// Auto PK
        this.UIAutoFight.AutoPK.AutoInviteToTeam = KTAutoAttackSetting.Config.PK.AutoInviteToTeam;
        this.UIAutoFight.AutoPK.AutoAcceptInviteToTeam = KTAutoAttackSetting.Config.PK.AutoAccectJoinTeam;
        this.UIAutoFight.AutoPK.AutoReflectAttack = KTAutoAttackSetting.Config.PK.AutoReflect;
        this.UIAutoFight.AutoPK.SeriesConquePriority = KTAutoAttackSetting.Config.PK.SeriesConquarePriority;
        this.UIAutoFight.AutoPK.LowHPEnemyPriority = KTAutoAttackSetting.Config.PK.LowHPTargetPriority;
        this.UIAutoFight.AutoPK.UseNewbieSkill = KTAutoAttackSetting.Config.PK.UseNewbieSkill;
        this.UIAutoFight.AutoPK.ChaseTarget = KTAutoAttackSetting.Config.PK.ChaseTarget;
        this.UIAutoFight.AutoPK.Skills = KTAutoAttackSetting.Config.PK.Skills;
		#endregion

		this.UIAutoFight.Close = this.CloseUIAutoFight;
        this.UIAutoFight.SaveSetting = () => {
            /// Chung
            KTAutoAttackSetting.Config.EnableAutoPK = this.UIAutoFight.General.EnableAutoPK;
            KTAutoAttackSetting.Config.General.RefuseChallenge = this.UIAutoFight.General.RefuseChallenge;
            KTAutoAttackSetting.Config.General.RefuseExchange = this.UIAutoFight.General.RefuseExchange;
            KTAutoAttackSetting.Config.General.RefuseTeam = this.UIAutoFight.General.RefuseTeam;

            /// Auto support
            KTAutoAttackSetting.Config.Support.AutoHP = this.UIAutoFight.AutoSupport.AutoHP;
            KTAutoAttackSetting.Config.Support.AutoMP = this.UIAutoFight.AutoSupport.AutoMP;
            KTAutoAttackSetting.Config.Support.AutoHPPercent = this.UIAutoFight.AutoSupport.AutoHPPercent;
            KTAutoAttackSetting.Config.Support.AutoMPPercent = this.UIAutoFight.AutoSupport.AutoMPPercent;
            KTAutoAttackSetting.Config.Support.AutoBuyMedicines = this.UIAutoFight.AutoSupport.AutoBuyMedicines;
            KTAutoAttackSetting.Config.Support.AutoBuyMedicinesQuantity = this.UIAutoFight.AutoSupport.AutoBuyMedicinesQuantity;
            KTAutoAttackSetting.Config.Support.AutoBuyMedicinesUsingBoundMoneyPriority = this.UIAutoFight.AutoSupport.AutoBuyMedicinesUsingBoundMoneyPriority;

            KTAutoAttackSetting.Config.Support.AutoX2 = this.UIAutoFight.AutoSupport.AutoX2;

            KTAutoAttackSetting.Config.Support.HPMedicine = this.UIAutoFight.AutoSupport.HPMedicineID;
            KTAutoAttackSetting.Config.Support.MPMedicine = this.UIAutoFight.AutoSupport.MPMedicineID;
            KTAutoAttackSetting.Config.Support.EM_AutoHeal = this.UIAutoFight.AutoSupport.EM_AutoHeal;
            KTAutoAttackSetting.Config.Support.EM_AutoHealHPPercent = this.UIAutoFight.AutoSupport.EM_AutoHealHPPercent;
            KTAutoAttackSetting.Config.Support.AutoPetHPPercent = this.UIAutoFight.AutoSupport.AutoPetHPPercent;
            KTAutoAttackSetting.Config.Support.AutoPetHP = this.UIAutoFight.AutoSupport.AutoPetHP;
            KTAutoAttackSetting.Config.Support.PetHPMedicine = this.UIAutoFight.AutoSupport.PetHPMedicineID;
            KTAutoAttackSetting.Config.Support.AutoPetLife = this.UIAutoFight.AutoSupport.AutoPetLife;
            KTAutoAttackSetting.Config.Support.AutoPetJoy = this.UIAutoFight.AutoSupport.AutoPetJoy;
            KTAutoAttackSetting.Config.Support.AutoCallPet = this.UIAutoFight.AutoSupport.AutoCallPet;

            /// Auto farm
            KTAutoAttackSetting.Config.Farm.AutoFireCamp = this.UIAutoFight.AutoFarm.AutoFireCamp;
            KTAutoAttackSetting.Config.Farm.FarmAround = this.UIAutoFight.AutoFarm.FarmAround;
            KTAutoAttackSetting.Config.Farm.ScanRange = this.UIAutoFight.AutoFarm.ScanRange;
            KTAutoAttackSetting.Config.Farm.SingleTarget = this.UIAutoFight.AutoFarm.SingleTarget;
            KTAutoAttackSetting.Config.Farm.IgnoreBoss = this.UIAutoFight.AutoFarm.IgnoreBoss;
            KTAutoAttackSetting.Config.Farm.LowHPTargetPriority = this.UIAutoFight.AutoFarm.LowHPTargetPriority;
            KTAutoAttackSetting.Config.Farm.UseNewbieSkill = this.UIAutoFight.AutoFarm.UseNewbieSkill;
            KTAutoAttackSetting.Config.Farm.AutoDrinkWine = this.UIAutoFight.AutoFarm.AutoUseWine;
            KTAutoAttackSetting.Config.Farm.Skills = this.UIAutoFight.AutoFarm.Skills;
            KTAutoAttackSetting.SetSkillTrain(this.UIAutoFight.AutoFarm.Skills);

            /// Auto nhặt
            KTAutoAttackSetting.Config.PickAndSell.EnableAutoPickUp = this.UIAutoFight.AutoPickUpItem.AutoPickUpItem;
            KTAutoAttackSetting.Config.PickAndSell.ScanRadius = this.UIAutoFight.AutoPickUpItem.PickUpRange;
            KTAutoAttackSetting.Config.PickAndSell.PickUpCrystalStoneOnly = this.UIAutoFight.AutoPickUpItem.PickUpCrystalStone;
            KTAutoAttackSetting.Config.PickAndSell.PickUpCrystalStoneLevel = this.UIAutoFight.AutoPickUpItem.PickUpCrystalStoneLevel;
            KTAutoAttackSetting.Config.PickAndSell.PickUpEquip = this.UIAutoFight.AutoPickUpItem.PickUpEquip;
            KTAutoAttackSetting.Config.PickAndSell.PickUpEquipStar = this.UIAutoFight.AutoPickUpItem.PickUpEquipStar;
            KTAutoAttackSetting.Config.PickAndSell.PickUpItemByLinesCount = this.UIAutoFight.AutoPickUpItem.PickUpEquipLinesCount;
            KTAutoAttackSetting.Config.PickAndSell.AutoSortBag = this.UIAutoFight.AutoPickUpItem.AutoSortBag;
            KTAutoAttackSetting.Config.PickAndSell.AutoSellItems = this.UIAutoFight.AutoPickUpItem.AutoBackAndSellTrashes;
            KTAutoAttackSetting.Config.PickAndSell.SellEquipStar = this.UIAutoFight.AutoPickUpItem.AutoSellTrashEquipBelowStar;
            KTAutoAttackSetting.Config.PickAndSell.PickUpOtherItems = this.UIAutoFight.AutoPickUpItem.PickUpOtherItems;

            /// Auto PK
            KTAutoAttackSetting.Config.PK.AutoInviteToTeam = this.UIAutoFight.AutoPK.AutoInviteToTeam;
            KTAutoAttackSetting.Config.PK.AutoAccectJoinTeam = this.UIAutoFight.AutoPK.AutoAcceptInviteToTeam;
            KTAutoAttackSetting.Config.PK.AutoReflect = this.UIAutoFight.AutoPK.AutoReflectAttack;
            KTAutoAttackSetting.Config.PK.SeriesConquarePriority = this.UIAutoFight.AutoPK.SeriesConquePriority;
            KTAutoAttackSetting.Config.PK.LowHPTargetPriority = this.UIAutoFight.AutoPK.LowHPEnemyPriority;
            KTAutoAttackSetting.Config.PK.UseNewbieSkill = this.UIAutoFight.AutoPK.UseNewbieSkill;
            KTAutoAttackSetting.Config.PK.ChaseTarget = this.UIAutoFight.AutoPK.ChaseTarget;
            KTAutoAttackSetting.SetSkillPK(this.UIAutoFight.AutoPK.Skills);

            /// Lưu thiết lập vào hệ thống
            KTAutoAttackSetting.SaveSettings();

            KTGlobal.AddNotification("Lưu thiết lập tự đánh thành công!");

            /// Thực thi thiết lập hệ thống
            this.ExecuteAutoSettings();

            /// Đóng khung
            this.CloseUIAutoFight();

            /// Nếu đang tự đánh
            if (KTAutoFightManager.Instance.IsAutoFighting)
            {
                /// Tắt tự đánh
                KTAutoFightManager.Instance.StopAutoFight();
                /// Mở tự đánh
                KTAutoFightManager.Instance.StartAuto();
            }
        };
    }

    /// <summary>
    /// Đóng khung thiết lập tự đánh
    /// </summary>
    public void CloseUIAutoFight()
	{
        if (this.UIAutoFight != null)
        {
            CanvasManager canvas = Global.MainCanvas.GetComponent<CanvasManager>();
            canvas.RemoveUI(this.UIAutoFight);
            this.UIAutoFight = null;
        }
    }

    /// <summary>
    /// Thực thi thiết lập tự đánh
    /// </summary>
    public void ExecuteAutoSettings()
    {
		KTAutoAttackSetting.SaveSettings();
	}
    #endregion

    #region Thiết lập hệ thống
    /// <summary>
    /// Khung thiết lập hệ thống
    /// </summary>
    public UISystemSetting UISystemSetting { get; protected set; } = null;

    /// <summary>
    /// Hiển thị thiết lập hệ thống
    /// </summary>
    public void ShowSystemSetting()
    {
        /// Nếu khung đang hiển thị
        if (this.UISystemSetting != null)
        {
            return;
        }

        CanvasManager canvas = Global.MainCanvas.GetComponent<CanvasManager>();
        this.UISystemSetting = canvas.LoadUIPrefab<UISystemSetting>("MainGame/UISystemSetting");
        if (this.UISystemSetting == null)
        {
            return;
        }
        /// Thêm vào Canvas
        canvas.AddUI(this.UISystemSetting, true);

        /// Control thuộc tính của khung
        this.UISystemSetting.Close = () => {
            this.CloseSystemSetting();
        };
        this.UISystemSetting.SaveSetting = () => {
            KTSystemSetting.MusicVolume = (int) (this.UISystemSetting.MusicVolume * 100);
            KTSystemSetting.SkillVolume = (int) (this.UISystemSetting.EffectSoundVolume * 100);
            KTSystemSetting.FieldOfView = (int) this.UISystemSetting.FieldOfView;
            KTSystemSetting.HideOtherName = this.UISystemSetting.HideOtherName;
            KTSystemSetting.HideOtherHPBar = this.UISystemSetting.HideOtherHPBar;
            KTSystemSetting.HideRole = this.UISystemSetting.HideLeader;
            KTSystemSetting.HideOtherRole = this.UISystemSetting.HideOtherRole;
            KTSystemSetting.HideNPC = this.UISystemSetting.HideMonsterAndNPC;
            KTSystemSetting.HidePlayerChat = this.UISystemSetting.HidePlayerChat;
            KTSystemSetting.HideSkillCastEffect = this.UISystemSetting.HideSkillCastEffect;
            KTSystemSetting.HideSkillExplodeEffect = this.UISystemSetting.HideSkillExplodeEffect;
            KTSystemSetting.HideSkillBuffEffect = this.UISystemSetting.HideSkillBuffEffect;
            KTSystemSetting.HideSkillBullet = this.UISystemSetting.HideBullet;
            KTSystemSetting.DisableTrailEffect = this.UISystemSetting.DisableTrailEffect;
            KTSystemSetting.HideWeaponEnhanceEffect = this.UISystemSetting.HideWeaponEnhanceEffect;

            /// Lưu thiết lập vào hệ thống
            KTSystemSetting.SaveSettings();

            /// Thông báo
            KTGlobal.AddNotification("Lưu thiết lập hệ thống thành công!");

            /// Thực thi thiết lập hệ thống
            this.ExecuteSystemSettings();

            /// Đóng khung
            this.CloseSystemSetting();
        };
        this.UISystemSetting.QuitGame = () => {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            UnityEngine.Application.Quit();
#endif
        };
        this.UISystemSetting.BackToLogin = () =>
        {
            this.CloseSystemSetting();
            Super.BackToLoginFromGame();
        };
        this.UISystemSetting.MusicVolume = KTSystemSetting.MusicVolume / 100f;
        this.UISystemSetting.EffectSoundVolume = KTSystemSetting.SkillVolume / 100f;
        this.UISystemSetting.FieldOfView = KTSystemSetting.FieldOfView;
        this.UISystemSetting.HideOtherName = KTSystemSetting.HideOtherName;
        this.UISystemSetting.HideOtherHPBar = KTSystemSetting.HideOtherHPBar;
        this.UISystemSetting.HideLeader = KTSystemSetting.HideRole;
        this.UISystemSetting.HideOtherRole = KTSystemSetting.HideOtherRole;
        this.UISystemSetting.HideMonsterAndNPC = KTSystemSetting.HideNPC;
        this.UISystemSetting.HidePlayerChat = KTSystemSetting.HidePlayerChat;
        this.UISystemSetting.HideSkillCastEffect = KTSystemSetting.HideSkillCastEffect;
        this.UISystemSetting.HideSkillExplodeEffect = KTSystemSetting.HideSkillExplodeEffect;
        this.UISystemSetting.HideSkillBuffEffect = KTSystemSetting.HideSkillBuffEffect;
        this.UISystemSetting.HideBullet = KTSystemSetting.HideSkillBullet;
        this.UISystemSetting.DisableTrailEffect = KTSystemSetting.DisableTrailEffect;
        this.UISystemSetting.HideWeaponEnhanceEffect = KTSystemSetting.HideWeaponEnhanceEffect;
    }

    /// <summary>
    /// Đóng thiết lập hệ thống
    /// </summary>
    public void CloseSystemSetting()
    {
        if (this.UISystemSetting != null)
        {
            CanvasManager canvas = Global.MainCanvas.GetComponent<CanvasManager>();
            canvas.RemoveUI(this.UISystemSetting);
            this.UISystemSetting = null;
        }
    }

    /// <summary>
    /// Thực thi thiết lập hệ thống
    /// </summary>
    public void ExecuteSystemSettings()
    {
        KTSystemSetting.Apply();
    }
    #endregion
}
