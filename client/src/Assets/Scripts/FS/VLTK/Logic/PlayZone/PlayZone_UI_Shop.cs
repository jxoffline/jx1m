using FS.GameEngine.Logic;
using FS.GameEngine.Network;
using FS.VLTK;
using FS.VLTK.Network;

using FS.VLTK.UI;
using FS.VLTK.UI.Main;
using Server.Data;
using UnityEngine;

/// <summary>
/// Quản lý các khung giao diện trong màn chơi
/// </summary>
public partial class PlayZone
{
    #region UI Shop
    /// <summary>
    /// Của hàng từ NPC
    /// </summary>
    protected UIShop UIShop { get; set; } = null;

    /// <summary>
    /// Hiển thị khung cửa hàng từ NPC
    /// </summary>
    /// <param name="shopTab"></param>
    public void OpenUIShop(ShopTab shopTab)
    {
        /// Nếu đã tồn tại khung thì thôi
        if (this.UIShop != null)
        {
            return;
        }

        CanvasManager canvas = Global.MainCanvas.GetComponent<CanvasManager>();
        this.UIShop = canvas.LoadUIPrefab<UIShop>("MainGame/UIShop");
        canvas.AddUI(this.UIShop);

        this.UIShop.Data = shopTab;
        this.UIShop.Buy = (shopItemData, quantity) => {
            GameInstance.Game.SpriteBuyGoods(shopItemData.ID, quantity, shopTab.ShopID, -1);
        };
        this.UIShop.BuyBack = (itemGD) => {
            GameInstance.Game.SpriteBuyGoods(itemGD.Id, itemGD.GCount, 9999, -1);
        };
        this.UIShop.Sell = (itemGD) => {
            GameInstance.Game.SpriteBuyOutGoods(itemGD.Id);
            KTGlobal.CloseItemInfo();
        };
        this.UIShop.Close = this.CloseUIShop;
    }

    /// <summary>
    /// Đóng khung cửa hàng từ NPC
    /// </summary>
    public void CloseUIShop()
    {
        /// Nếu khung tồn tại
        if (this.UIShop != null)
        {
            GameObject.Destroy(this.UIShop.gameObject);
            this.UIShop = null;
        }
    }
    #endregion

    #region Kỳ Trân Các
    /// <summary>
    /// Khung Kỳ Trân Các
    /// </summary>
    public UITokenShop UITokenShop { get; protected set; }

    /// <summary>
    /// Mở khung Kỳ Trân Các
    /// </summary>
    /// <param name="data"></param>
    public void OpenTokenShop(TokenShop data)
    {
        /// Nếu đã tồn tại khung thì thôi
        if (this.UITokenShop != null)
        {
            return;
        }

        CanvasManager canvas = Global.MainCanvas.GetComponent<CanvasManager>();
        this.UITokenShop = canvas.LoadUIPrefab<UITokenShop>("MainGame/UITokenShop");
        canvas.AddUI(this.UITokenShop);

        /// Tham chiếu shop tương ứng từ vật phẩm
        foreach (ShopTab shopTab in data.Token)
        {
            foreach (ShopItem shopItem in shopTab.Items)
            {
                shopItem.ShopTab = shopTab;
            }
        }
        foreach (ShopTab shopTab in data.BoundToken)
        {
            foreach (ShopItem shopItem in shopTab.Items)
            {
                shopItem.ShopTab = shopTab;
            }
        }

        this.UITokenShop.Data = data;
        this.UITokenShop.Buy = (shopItemData, quantity, shopID, couponID) => {
            GameInstance.Game.SpriteBuyGoods(shopItemData.ID, quantity, shopID, couponID);
        };
        this.UITokenShop.StoreBuyItem = (storeItemData) => {

        };
        this.UITokenShop.Close = this.CloseTokenShop;
    }

    /// <summary>
    /// Đóng khung Kỳ Trân Các
    /// </summary>
    public void CloseTokenShop()
    {
        /// Nếu khung tồn tại
        if (this.UITokenShop != null)
        {
            GameObject.Destroy(this.UITokenShop.gameObject);
            this.UITokenShop = null;
        }
    }
    #endregion

    #region Cửa hàng của người chơi
    #region Bán
    /// <summary>
    /// Khung cửa hàng của người bán
    /// </summary>
    public UIPlayerShop_Sell UIPlayerShop_Sell { get; protected set; } = null;

    /// <summary>
    /// Hiển thị khung cửa hàng của người bán
    /// </summary>
    public void ShowUIPlayerShop_Sell()
    {
        if (this.UIPlayerShop_Sell != null)
        {
            return;
        }

        CanvasManager canvas = Global.MainCanvas.gameObject.GetComponent<CanvasManager>();
        this.UIPlayerShop_Sell = canvas.LoadUIPrefab<UIPlayerShop_Sell>("MainGame/PlayerShop/UIPlayerShop_Sell");
        canvas.AddUI(this.UIPlayerShop_Sell);

        this.UIPlayerShop_Sell.Close = this.CloseUIPlayerShop_Sell;
        this.UIPlayerShop_Sell.AddItemToSell = (itemGD, price) => {
            KT_TCPHandler.SendAddItemToStall(itemGD.Id, price);
        };
        this.UIPlayerShop_Sell.RemoveItemFromSell = (itemGD) => {
            KT_TCPHandler.SendRemoveItemFromStall(itemGD.Id);
        };
        this.UIPlayerShop_Sell.StartSelling = (shopName) =>
        {
            KT_TCPHandler.SendStartStall(shopName, false);
        };
        this.UIPlayerShop_Sell.StartCommissionSelling = (shopName) =>
        {
            KT_TCPHandler.SendStartStall(shopName, true);
        };
        this.UIPlayerShop_Sell.CancelSelling = () =>
        {
            KT_TCPHandler.SendStopStall();
        };
    }

    /// <summary>
    /// Đóng khung cửa hàng của người bán
    /// </summary>
    public void CloseUIPlayerShop_Sell()
    {
        if (this.UIPlayerShop_Sell != null)
        {
            GameObject.Destroy(this.UIPlayerShop_Sell.gameObject);
            this.UIPlayerShop_Sell = null;
        }
    }
    #endregion

    #region Mua
    /// <summary>
    /// Khung cửa hàng của người bán
    /// </summary>
    public UIPlayerShop_Buy UIPlayerShop_Buy { get; protected set; } = null;

    /// <summary>
    /// Hiển thị khung cửa hàng của người bán
    /// </summary>
    /// <param name="data"></param>
    public void ShowUIPlayerShop_Buy(StallData data)
    {
        if (this.UIPlayerShop_Buy != null)
        {
            return;
        }

        CanvasManager canvas = Global.MainCanvas.gameObject.GetComponent<CanvasManager>();
        this.UIPlayerShop_Buy = canvas.LoadUIPrefab<UIPlayerShop_Buy>("MainGame/PlayerShop/UIPlayerShop_Buy");
        canvas.AddUI(this.UIPlayerShop_Buy);

        this.UIPlayerShop_Buy.Close = this.CloseUIPlayerShop_Buy;
        this.UIPlayerShop_Buy.Data = data;
        this.UIPlayerShop_Buy.Buy = (itemGD) => {
            KT_TCPHandler.SendBuyItemFromStall(data.RoleID, itemGD.Id);
        };
    }

    /// <summary>
    /// Đóng khung cửa hàng của người bán
    /// </summary>
    public void CloseUIPlayerShop_Buy()
    {
        if (this.UIPlayerShop_Buy != null)
        {
            GameObject.Destroy(this.UIPlayerShop_Buy.gameObject);
            this.UIPlayerShop_Buy = null;
        }
    }
    #endregion
    #endregion
}
