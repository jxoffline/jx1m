using FS.GameEngine.Logic;
using FS.GameEngine.Sprite;
using FS.GameFramework.Logic;
using FS.VLTK.Entities.Config;
using FS.VLTK.Loader;
using FS.VLTK.Network;
using FS.VLTK.UI;
using FS.VLTK.UI.Main;
using FS.VLTK.UI.Main.MainUI;
using Server.Data;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Quản lý các khung giao diện trong màn chơi
/// </summary>
public partial class PlayZone
{
    #region ShortcutNPCTalk
    /// <summary>
    /// Khung đối thoại nhanh với NPC
    /// </summary>
    public UIShortcutNPCTalk UIShortcutNPCTalk { get; protected set; }

    /// <summary>
    /// Khởi tạo khung đối thoại nhanh với NPC
    /// </summary>
    protected void InitUIShortcutNPCTalk()
    {
        CanvasManager canvas = Global.MainCanvas.GetComponent<CanvasManager>();
        this.UIShortcutNPCTalk = canvas.LoadUIPrefab<UIShortcutNPCTalk>("MainGame/MainUI/UIShortcutNPCTalk");
        canvas.AddMainUI(this.UIShortcutNPCTalk);
    }
    #endregion

    #region NPC Dialog
    /// <summary>
    /// Khung thoại NPC
    /// </summary>
    public UINPCDialog UINPCDialog { get; private set; }

    /// <summary>
    /// Hiển thị hội thoại NPC gọi từ Lua
    /// </summary>
    /// <param name="npcID"></param>
    /// <param name="dialogID"></param>
    /// <param name="content"></param>
    /// <param name="selections"></param>
    /// <param name="items"></param>
    /// <param name="itemSelectable"></param>
    /// <param name="itemHeaderString"></param>
    public void ShowNPCLuaDialog(int npcID, int dialogID, string content, Dictionary<int, string> selections, List<DialogItemSelectionInfo> items, bool itemSelectable, string itemHeaderString, Dictionary<int, string> otherParams)
    {
        GSprite npc = null;
        if (npcID > 0)
        {
            npc = scene.FindSprite(npcID);
        }

        if (null != this.UINPCDialog)
        {
            this.UINPCDialog.Title = npc != null ? npc.RoleName : "";
            this.UINPCDialog.Content = content;
            this.UINPCDialog.Selections = selections;
            this.UINPCDialog.ItemSelectable = itemSelectable;
            this.UINPCDialog.ShowButtonAccept = items != null && items.Count > 0 && itemSelectable;
            this.UINPCDialog.Items = items;
            this.UINPCDialog.ItemHeaderString = itemHeaderString;
            this.UINPCDialog.SelectionClick = (selectionID) => {
                KT_TCPHandler.SendNPCSelection(npcID, dialogID, selectionID, null, otherParams);
            };
            this.UINPCDialog.AcceptSelectItem = (itemInfo) => {
                KT_TCPHandler.SendNPCSelection(npcID, dialogID, 0, itemInfo, otherParams);
            };
            return;
        }

        CanvasManager canvas = Global.MainCanvas.GetComponent<CanvasManager>();
        this.UINPCDialog = canvas.LoadUIPrefab<UINPCDialog>("MainGame/UINPCDialog");
        canvas.AddUI(this.UINPCDialog);
        this.UINPCDialog.Close = this.CloseNPCDialog;
        this.UINPCDialog.Title = npc != null ? npc.RoleName : "";
        this.UINPCDialog.Content = content;
        this.UINPCDialog.Selections = selections;
        this.UINPCDialog.ItemSelectable = itemSelectable;
        this.UINPCDialog.ShowButtonAccept = items != null && items.Count > 0 && itemSelectable;
        this.UINPCDialog.Items = items;
        this.UINPCDialog.ItemHeaderString = itemHeaderString;
        this.UINPCDialog.SelectionClick = (selectionID) => {
            KT_TCPHandler.SendNPCSelection(npcID, dialogID, selectionID, null, otherParams);
        };
        this.UINPCDialog.AcceptSelectItem = (itemInfo) => {
            KT_TCPHandler.SendNPCSelection(npcID, dialogID, 0, itemInfo, otherParams);
        };
    }

    /// <summary>
    /// Hiển thị hội thoại ItemDialog gọi từ Lua
    /// </summary>
    /// <param name="itemID"></param>
    /// <param name="itemDbID"></param>
    /// <param name="content"></param>
    /// <param name="selections"></param>
    /// <param name="items"></param>
    /// <param name="itemSelectable"></param>
    /// <param name="itemHeaderString"></param>
    /// <param name="otherParams"></param>
    public void ShowNPCLuaDialog(int itemID, int itemDbID, int dialogID, string content, Dictionary<int, string> selections, List<DialogItemSelectionInfo> items, bool itemSelectable, string itemHeaderString, Dictionary<int, string> otherParams)
    {
        if (!Loader.Items.TryGetValue(itemID, out ItemData itemData))
        {
            return;
        }

        if (null != this.UINPCDialog)
        {
            this.UINPCDialog.Title = itemData.Name;
            this.UINPCDialog.Content = content;
            this.UINPCDialog.Selections = selections;
            this.UINPCDialog.ItemSelectable = itemSelectable;
            this.UINPCDialog.ShowButtonAccept = items != null && items.Count > 0 && itemSelectable;
            this.UINPCDialog.Items = items;
            this.UINPCDialog.ItemHeaderString = itemHeaderString;
            this.UINPCDialog.SelectionClick = (selectionID) => {
                KT_TCPHandler.SendItemSelection(itemDbID, itemID, dialogID, selectionID, null, otherParams);
            };
            this.UINPCDialog.AcceptSelectItem = (itemInfo) => {
                KT_TCPHandler.SendItemSelection(itemDbID, itemID, dialogID, 0, itemInfo, otherParams);
            };
            return;
        }

        CanvasManager canvas = Global.MainCanvas.GetComponent<CanvasManager>();
        this.UINPCDialog = canvas.LoadUIPrefab<UINPCDialog>("MainGame/UINPCDialog");
        canvas.AddUI(this.UINPCDialog);
        this.UINPCDialog.Close = this.CloseNPCDialog;
        this.UINPCDialog.Title = itemData.Name;
        this.UINPCDialog.Content = content;
        this.UINPCDialog.Selections = selections;
        this.UINPCDialog.ItemSelectable = itemSelectable;
        this.UINPCDialog.ShowButtonAccept = items != null && items.Count > 0 && itemSelectable;
        this.UINPCDialog.Items = items;
        this.UINPCDialog.ItemHeaderString = itemHeaderString;
        this.UINPCDialog.SelectionClick = (selectionID) => {
            KT_TCPHandler.SendItemSelection(itemDbID, itemID, dialogID, selectionID, null, otherParams);
        };
        this.UINPCDialog.AcceptSelectItem = (itemInfo) => {
            KT_TCPHandler.SendItemSelection(itemDbID, itemID, dialogID, 0, itemInfo, otherParams);
        };
    }

    /// <summary>
    /// Đóng khung thoại NPC
    /// </summary>
    public void CloseNPCDialog()
    {
        if (this.UINPCDialog != null)
        {
            GameObject.Destroy(this.UINPCDialog.gameObject);
            this.UINPCDialog = null;
        }
    }
    #endregion
}
