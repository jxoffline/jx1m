using FS.GameEngine.Logic;
using FS.GameEngine.Network;
using FS.VLTK.Network;
using FS.VLTK.UI;
using FS.VLTK.UI.Main;
using Server.Data;
using System.Collections.Generic;
using UnityEngine;
using static FS.VLTK.Entities.Enum;

/// <summary>
/// Quản lý các khung giao diện trong màn chơi
/// </summary>
public partial class PlayZone
{
    #region Quảng bá vật phẩm
    /// <summary>
    /// Khung Chat
    /// </summary>
    public UIAdvertiseItem UIAdvertiseItem { get; protected set; }

    /// <summary>
    /// Hiển thị khung Chat
    /// </summary>
    public void ShowUIAdvertiseItem(GoodsData itemGD)
    {
        if (this.UIAdvertiseItem == null)
        {
            CanvasManager canvas = Global.MainCanvas.GetComponent<CanvasManager>();
            this.UIAdvertiseItem = canvas.LoadUIPrefab<UIAdvertiseItem>("MainGame/UIAdvertiseItem");
            canvas.AddUI(this.UIAdvertiseItem);
        }

        this.UIAdvertiseItem.ItemGD = itemGD;
        this.UIAdvertiseItem.Close = this.CloseUIAdvertiseItem;
        this.UIAdvertiseItem.Send = (channel, toRoleName, content) => {
            GameInstance.Game.SpriteSendChat(Global.Data.RoleData.RoleName, toRoleName, content, channel, new List<GoodsData>() { itemGD }, null);
        };
    }

    /// <summary>
    /// Đóng khung Chat
    /// </summary>
    public void CloseUIAdvertiseItem()
    {
        if (this.UIAdvertiseItem != null)
        {
            GameObject.Destroy(this.UIAdvertiseItem.gameObject);
            this.UIAdvertiseItem = null;
        }
    }
    #endregion

    #region Khung cường hóa Ngũ hành ấn
    /// <summary>
    /// Khung cường hóa Ngũ hành ấn
    /// </summary>
    public UISignetEnhance UISignetEnhance { get; protected set; }

    /// <summary>
    /// Mở khung cường hóa Ngũ hành ấn
    /// </summary>
    public void OpenUISignetEnhance()
    {
        if (this.UISignetEnhance != null)
        {
            return;
        }
        this.UISignetEnhance = CanvasManager.Instance.LoadUIPrefab<UISignetEnhance>("MainGame/UISignetEnhance");
        CanvasManager.Instance.AddUI(this.UISignetEnhance);
        this.UISignetEnhance.Close = this.CloseUISignetEnhance;
        this.UISignetEnhance.Enhance = (signet, fsList, type) => {
            KT_TCPHandler.SendSignetEnhance(signet, fsList, type);
        };
    }

    /// <summary>
    /// Đóng khung cường hóa Ngũ hành ấn
    /// </summary>
    public void CloseUISignetEnhance()
    {
        if (this.UISignetEnhance != null)
        {
            GameObject.Destroy(this.UISignetEnhance.gameObject);
            this.UISignetEnhance = null;
        }
    }
    #endregion

    #region Khung Cường hóa
    /// <summary>
    /// Khung cường hóa
    /// </summary>
    public UIEnhance UIEnhance { get; protected set; }

    /// <summary>
    /// Mở khung cường hóa trang bị
    /// </summary>
    public void OpenUIEnhance()
    {
        if (this.UIEnhance != null)
        {
            return;
        }
        this.UIEnhance = CanvasManager.Instance.LoadUIPrefab<UIEnhance>("MainGame/UIEnhance");
        CanvasManager.Instance.AddUI(this.UIEnhance);
        this.UIEnhance.Close = this.CloseUIEnhance;
        this.UIEnhance.Enhance = (equip, crystalList, usingBoundMoney) => {
            KT_TCPHandler.SendEquipEnhance(equip, crystalList, usingBoundMoney ? MoneyType.BacKhoa : MoneyType.Bac);
        };
    }

    /// <summary>
    /// Đóng khung cường hóa trang bị
    /// </summary>
    public void CloseUIEnhance()
    {
        if (this.UIEnhance != null)
        {
            GameObject.Destroy(this.UIEnhance.gameObject);
            this.UIEnhance = null;
        }
    }
    #endregion

    #region Khung ghép Huyền Tinh
    /// <summary>
    /// Khung ghép Huyền Tinh
    /// </summary>
    public UICrystalStoneSynthesis UICrystalStoneSynthesis { get; protected set; }

    /// <summary>
    /// Mở khung ghép Huyền Tinh
    /// </summary>
    public void OpenUICrystalStoneSynthesis()
    {
        if (this.UIEnhance != null)
        {
            return;
        }
        this.UICrystalStoneSynthesis = CanvasManager.Instance.LoadUIPrefab<UICrystalStoneSynthesis>("MainGame/UICrystalStoneSynthesis");
        CanvasManager.Instance.AddUI(this.UICrystalStoneSynthesis);
        this.UICrystalStoneSynthesis.Close = this.CloseUICrystalStoneSynthesis;
        this.UICrystalStoneSynthesis.Synthesis = (crystalList, usingBoundMoney) => {
            KT_TCPHandler.SendComposeCrystalStones(crystalList, usingBoundMoney ? MoneyType.BacKhoa : MoneyType.Bac);
        };
    }

    /// <summary>
    /// Đóng khung ghép Huyền Tinh
    /// </summary>
    public void CloseUICrystalStoneSynthesis()
    {
        if (this.UICrystalStoneSynthesis != null)
        {
            GameObject.Destroy(this.UICrystalStoneSynthesis.gameObject);
            this.UICrystalStoneSynthesis = null;
        }
    }
    #endregion

    #region Khung tách Huyền Tinh khỏi trang bị
    /// <summary>
    /// Khung tách Huyền Tinh khỏi trang bị
    /// </summary>
    public UISplitEquipCrystalStones UISplitEquipCrystalStones { get; protected set; }

    /// <summary>
    /// Mở khung tách Huyền Tinh khỏi trang bị
    /// </summary>
    /// <param name="minEnhanceLevelToSplit"></param>
    /// <param name="nRate"></param>
    public void OpenUISplitEquipCrystalStones(int minEnhanceLevelToSplit, float nRate)
    {
        if (this.UIEnhance != null)
        {
            return;
        }
        this.UISplitEquipCrystalStones = CanvasManager.Instance.LoadUIPrefab<UISplitEquipCrystalStones>("MainGame/UISplitEquipCrystalStones");
        CanvasManager.Instance.AddUI(this.UISplitEquipCrystalStones);
        this.UISplitEquipCrystalStones.ProductRate = nRate;
        this.UISplitEquipCrystalStones.MinLevelToSplit = minEnhanceLevelToSplit;
        this.UISplitEquipCrystalStones.Close = this.CloseUISplitEquipCrystalStones;
        this.UISplitEquipCrystalStones.Split = (equipGD) => {
            KT_TCPHandler.SendSplitCrystalStones(equipGD, -1);
        };
    }

    /// <summary>
    /// Đóng khung tách Huyền Tinh khỏi trang bị
    /// </summary>
    public void CloseUISplitEquipCrystalStones()
    {
        if (this.UISplitEquipCrystalStones != null)
        {
            GameObject.Destroy(this.UISplitEquipCrystalStones.gameObject);
            this.UISplitEquipCrystalStones = null;
        }
    }
    #endregion

    #region Khung tách Huyền Tinh từ Huyền Tinh cấp cao
    /// <summary>
    /// Khung tách Huyền Tinh khỏi từ Huyền Tinh cấp cao
    /// </summary>
    public UISplitCrystalStone UISplitCrystalStone { get; protected set; }

    /// <summary>
    /// Mở khung tách Huyền Tinh khỏi từ Huyền Tinh cấp cao
    /// </summary>
    /// <param name="minLevelToSplit"></param>
    /// <param name="nRate"></param>
    public void OpenUISplitCrystalStone(int minLevelToSplit, float nRate)
    {
        if (this.UISplitCrystalStone != null)
        {
            return;
        }
        this.UISplitCrystalStone = CanvasManager.Instance.LoadUIPrefab<UISplitCrystalStone>("MainGame/UISplitCrystalStone");
        CanvasManager.Instance.AddUI(this.UISplitCrystalStone);
        this.UISplitCrystalStone.ProductRate = nRate;
        this.UISplitCrystalStone.MinLevelToSplit = minLevelToSplit;
        this.UISplitCrystalStone.Close = this.CloseUISplitCrystalStone;
        this.UISplitCrystalStone.Split = (crystalStoneGD) => {
            KT_TCPHandler.SendSplitCrystalStones(crystalStoneGD, -2);
        };
    }

    /// <summary>
    /// Đóng khung tách Huyền Tinh khỏi từ Huyền Tinh cấp cao
    /// </summary>
    public void CloseUISplitCrystalStone()
    {
        if (this.UISplitCrystalStone != null)
        {
            GameObject.Destroy(this.UISplitCrystalStone.gameObject);
            this.UISplitCrystalStone = null;
        }
    }
	#endregion

	#region Khung luyện hóa trang bị
    /// <summary>
    /// Khung luyện hóa trang bị
    /// </summary>
    public UIEquipLevelUp UIEquipLevelUp { get; protected set; }

    /// <summary>
    /// Mở khung luyện hóa trang bị
    /// </summary>
    public void OpenUIEquipLevelUp()
	{
        if (this.UIEquipLevelUp != null)
        {
            return;
        }
        this.UIEquipLevelUp = CanvasManager.Instance.LoadUIPrefab<UIEquipLevelUp>("MainGame/UIEquipLevelUp");
        CanvasManager.Instance.AddUI(this.UIEquipLevelUp);

        this.UIEquipLevelUp.Close = this.CloseUIEquipLevelUp;
        this.UIEquipLevelUp.EquipLevelUp = (equipGD, recipeGD, crystalStones, productGoodsID) => {
            KT_TCPHandler.SendEquipRefine(equipGD, recipeGD, crystalStones, productGoodsID);
        };
    }

    /// <summary>
    /// Đóng khung luyện hóa trang bị
    /// </summary>
    public void CloseUIEquipLevelUp()
	{
        if (this.UIEquipLevelUp != null)
        {
            GameObject.Destroy(this.UIEquipLevelUp.gameObject);
            this.UIEquipLevelUp = null;
        }
    }
    #endregion

    #region Tách trang bị chế thành Ngũ Hành Hồn Thạch
    /// <summary>
    /// Khung tách trang bị chế thành Ngũ Hành Hồn Thạch
    /// </summary>
    public UIEquipRefineToFS UIEquipRefineToFS { get; protected set; }

    /// <summary>
    /// Mở khung tách trang bị chế thành Ngũ Hành Hồn Thạch
    /// </summary>
    public void OpenUIEquipRefineToFS()
	{
        if (this.UIEquipRefineToFS != null)
        {
            return;
        }
        this.UIEquipRefineToFS = CanvasManager.Instance.LoadUIPrefab<UIEquipRefineToFS>("MainGame/UIEquipRefineToFS");
        CanvasManager.Instance.AddUI(this.UIEquipRefineToFS);

        this.UIEquipRefineToFS.Close = this.CloseUIEquipRefineToFS;
        this.UIEquipRefineToFS.Refine = (equipGD) => {
            KT_TCPHandler.SendEquipRefineIntoFS(equipGD);
        };
    }

    /// <summary>
    /// Đóng khung tách trang bị chế thành Ngũ Hành Hồn Thạch
    /// </summary>
    public void CloseUIEquipRefineToFS()
	{
        if (this.UIEquipRefineToFS != null)
        {
            GameObject.Destroy(this.UIEquipRefineToFS.gameObject);
            this.UIEquipRefineToFS = null;
        }
    }
	#endregion
}
