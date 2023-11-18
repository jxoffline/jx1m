using FS.GameEngine.Logic;
using FS.VLTK.UI.Main;
using FS.VLTK.UI;
using System;
using System.Collections.Generic;
using UnityEngine;
using Server.Data;
using FS.VLTK.Network;
using FS.VLTK.UI.Main.Pet;
using FS.VLTK.UI.Main.MainUI;
using FS.GameEngine.Network;
using FS.VLTK.Logic;

/// <summary>
/// Quản lý các khung giao diện trong màn chơi
/// </summary>
public partial class PlayZone
{
    #region Quảng bá pet
    /// <summary>
    /// Khung quảng bá pet
    /// </summary>
    public UIAdvertisePet UIAdvertisePet { get; protected set; }

    /// <summary>
    /// Hiển thị khung quảng bá pet
    /// </summary>
    public void ShowUIAdvertisePet(PetData petData)
    {
        if (this.UIAdvertisePet == null)
        {
            CanvasManager canvas = Global.MainCanvas.GetComponent<CanvasManager>();
            this.UIAdvertisePet = canvas.LoadUIPrefab<UIAdvertisePet>("MainGame/Pet/UIAdvertisePet");
            canvas.AddUI(this.UIAdvertisePet);
        }

        this.UIAdvertisePet.ItemGD = petData;
        this.UIAdvertisePet.Close = this.CloseUIAdvertisePet;
        this.UIAdvertisePet.Send = (channel, toRoleName, content) => {
            GameInstance.Game.SpriteSendChat(Global.Data.RoleData.RoleName, toRoleName, content, channel, null, new List<PetData>() { petData });
        };
    }

    /// <summary>
    /// Đóng khung quảng bá pet
    /// </summary>
    public void CloseUIAdvertisePet()
    {
        if (this.UIAdvertisePet != null)
        {
            GameObject.Destroy(this.UIAdvertisePet.gameObject);
            this.UIAdvertisePet = null;
        }
    }
    #endregion

    #region Thông tin pet của người chơi
    /// <summary>
    /// Khung thông tin pet của người chơi
    /// </summary>
    public UIOtherRolePet UIOtherRolePet { get; protected set; }

    /// <summary>
    /// Hiển thị khung thông tin pet của người chơi
    /// </summary>
    public void ShowUIOtherRolePet(OtherRolePetData data)
    {
        if (this.UIOtherRolePet != null)
        {
            return;
        }

        CanvasManager canvas = Global.MainCanvas.GetComponent<CanvasManager>();
        this.UIOtherRolePet = canvas.LoadUIPrefab<UIOtherRolePet>("MainGame/Pet/UIOtherRolePet");
        canvas.AddUI(this.UIOtherRolePet);

        this.UIOtherRolePet.Data = data;
        this.UIOtherRolePet.Close = this.CloseUIOtherRolePet;
    }

    /// <summary>
    /// Đóng khung quảng bá pet
    /// </summary>
    public void CloseUIOtherRolePet()
    {
        if (this.UIOtherRolePet != null)
        {
            GameObject.Destroy(this.UIOtherRolePet.gameObject);
            this.UIOtherRolePet = null;
        }
    }
    #endregion

    #region Thông tin chỉ số pet
    /// <summary>
    /// Khung thông tin chỉ số pet ở MainUI
    /// </summary>
    public UIPetPart UIPetPart { get; protected set; }

    /// <summary>
    /// Cập nhật UI thông tin chỉ số nhân vật
    /// </summary>
    protected void InitPlayerPetInfo()
    {
        if (this.UIPetPart != null)
        {
            return;
        }

        CanvasManager canvas = Global.MainCanvas.GetComponent<CanvasManager>();
        this.UIPetPart = canvas.LoadUIPrefab<UIPetPart>("MainGame/MainUI/UIPetPart");
        canvas.AddMainUI(this.UIPetPart);
    }
    #endregion

    #region Khung pet
    /// <summary>
    /// Khung pet
    /// </summary>
    public UIPet UIPet { get; protected set; } = null;

    /// <summary>
    /// Mở khung pet
    /// </summary>
    /// <param name="petList"></param>
    public void ShowUIPet(List<PetData> petList)
    {
        if (this.UIPet != null)
        {
            return;
        }

        CanvasManager canvas = Global.MainCanvas.gameObject.GetComponent<CanvasManager>();
        this.UIPet = canvas.LoadUIPrefab<UIPet>("MainGame/Pet/UIPet");
        canvas.AddUI(this.UIPet);

        this.UIPet.Close = this.CloseUIPet;
        this.UIPet.Data = petList;
        this.UIPet.ChangeName = (petID, newName) =>
        {
            KT_TCPHandler.SendChangePetName(petID, newName);
        };
        this.UIPet.GiftItems = (petID, items) =>
        {
            KT_TCPHandler.SendGiftPetItems(petID, items);
        };
        this.UIPet.FeedJoy = (petID) =>
        {
            KT_TCPHandler.SendFeedPet(petID, 0);
        };
        this.UIPet.FeedLife = (petID) =>
        {
            KT_TCPHandler.SendFeedPet(petID, 1);
        };
        this.UIPet.Advertise = (petData) =>
        {
            this.ShowUIAdvertisePet(petData);
        };
        this.UIPet.Call = (petID) =>
        {
            /// Ngừng di chuyển
            KTLeaderMovingManager.StopMove();
            /// Gửi yêu cầu
            KT_TCPHandler.SendDoPetCommand(petID, 1);
        };
        this.UIPet.CallBack = (petID) =>
        {
            KT_TCPHandler.SendDoPetCommand(petID, 0);
        };
        this.UIPet.Release = (petID) =>
        {
            KT_TCPHandler.SendReleasePet(petID);
        };
        this.UIPet.OpenStudySkill = (petData) =>
        {
            this.CloseUIPet();
            this.ShowUIPetStudySkill(petData);
        };
        this.UIPet.AssignAttributes = (petID, str, dex, sta, ene) =>
        {
            KT_TCPHandler.SendAssignPetAttributes(petID, str, dex, sta, ene);
        };
        this.UIPet.ResetAttributes = (petID) =>
        {
            KT_TCPHandler.SendResetPetAttributes(petID);
        };
        this.UIPet.Unequip = (itemGD) =>
        {
            GameInstance.Game.SpriteModGoods((int) ModGoodsTypes.EquipUnload, itemGD.Id, itemGD.GoodsID, -1, itemGD.Site, 1, -1);
        };
    }

    /// <summary>
    /// Đóng khung pet
    /// </summary>
    public void CloseUIPet()
    {
        if (this.UIPet != null)
        {
            GameObject.Destroy(this.UIPet.gameObject);
            this.UIPet = null;
        }
    }
    #endregion

    #region Học kỹ năng pet
    /// <summary>
    /// Khung học kỹ năng pet
    /// </summary>
    public UIPet_StudySkill UIPetStudySkill { get; protected set; } = null;

    /// <summary>
    /// Mở khung học kỹ năng pet
    /// </summary>
    /// <param name="petData"></param>
    public void ShowUIPetStudySkill(PetData petData)
    {
        if (this.UIPetStudySkill != null)
        {
            return;
        }

        CanvasManager canvas = Global.MainCanvas.gameObject.GetComponent<CanvasManager>();
        this.UIPetStudySkill = canvas.LoadUIPrefab<UIPet_StudySkill>("MainGame/Pet/UIPet_StudySkill");
        canvas.AddUI(this.UIPetStudySkill);

        this.UIPetStudySkill.Close = () =>
        {
            /// Đóng khung
            this.CloseUIPetStudySkill();
            /// Gửi yêu cầu mở khung pet
            KT_TCPHandler.SendGetPetList();
        };
        this.UIPetStudySkill.Data = petData;
        this.UIPetStudySkill.Study = (scrollDbID, medicineDbID) =>
        {
            /// Gửi yêu cầu học kỹ năng pet
            KT_TCPHandler.SendPetStudySkill(petData.ID, scrollDbID, medicineDbID);
        };
    }

    /// <summary>
    /// Đóng khung học kỹ năng pet
    /// </summary>
    public void CloseUIPetStudySkill()
    {
        if (this.UIPetStudySkill != null)
        {
            GameObject.Destroy(this.UIPetStudySkill.gameObject);
            this.UIPetStudySkill = null;
        }
    }
    #endregion

    #region Thông tin pet
    /// <summary>
    /// Khung thông tin pet
    /// </summary>
    public UIPetInfo UIPetInfo { get; protected set; } = null;

    /// <summary>
    /// Mở khung thông tin pet
    /// </summary>
    /// <param name="petData"></param>
    public void ShowUIPetInfo(PetData petData)
    {
        if (this.UIPet != null)
        {
            return;
        }

        CanvasManager canvas = Global.MainCanvas.gameObject.GetComponent<CanvasManager>();
        this.UIPetInfo = canvas.LoadUIPrefab<UIPetInfo>("MainGame/Pet/UIPetInfo");
        canvas.AddUI(this.UIPetInfo);

        this.UIPetInfo.Close = this.CloseUIPetInfo;
        this.UIPetInfo.Data = petData;
    }

    /// <summary>
    /// Đóng khung pet
    /// </summary>
    public void CloseUIPetInfo()
    {
        if (this.UIPetInfo != null)
        {
            GameObject.Destroy(this.UIPetInfo.gameObject);
            this.UIPetInfo = null;
        }
    }
    #endregion
}
