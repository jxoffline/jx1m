using FS.GameEngine.Logic;
using FS.GameEngine.Network;
using FS.VLTK;
using FS.VLTK.UI;
using FS.VLTK.UI.Main;
using UnityEngine;

/// <summary>
/// Quản lý các khung giao diện trong màn chơi
/// </summary>
public partial class PlayZone
{
    #region Giao dịch
    /// <summary>
    /// Khung giao dịch
    /// </summary>
    public UIExchange UIExchange { get; protected set; } = null;

    /// <summary>
    /// Mở khung giao dịch
    /// </summary>
    /// <param name="partnerID"></param>
    public void ShowUIExchange(int partnerID)
    {
        if (this.UIExchange != null)
        {
            return;
        }

        CanvasManager canvas = Global.MainCanvas.gameObject.GetComponent<CanvasManager>();
        this.UIExchange = canvas.LoadUIPrefab<UIExchange>("MainGame/UIExchange");
        canvas.AddUI(this.UIExchange);

        this.UIExchange.PartnerID = partnerID;
        this.UIExchange.Close = () => {
            /// Gửi gói tin thông báo người chơi hủy giao dịch
            GameInstance.Game.SpriteGoodsExchange(partnerID, (int) GoodsExchangeCmds.Cancel, Global.Data.ExchangeID);

            /// Đóng khung
            this.CloseUIExchange();
        };
        this.UIExchange.AddItemToExchangeField = (itemGD) => {
            /// Nếu vật phẩm không tồn tại
            if (itemGD == null || itemGD.GCount <= 0)
            {
                KTGlobal.AddNotification("Vật phẩm không tồn tại!");
                return;
            }
            /// Nếu vật phẩm đã bị khóa
            else if (itemGD.Binding == 1)
            {
                KTGlobal.AddNotification("Vật phẩm đã khóa, không thể đặt lên!");
                return;
            }

            /// Gửi gói tin thông báo thêm vật phẩm vào sàn giao dịch
            GameInstance.Game.SpriteGoodsExchange(partnerID, (int) GoodsExchangeCmds.AddGoods, itemGD.Id);

        };
        this.UIExchange.RemoveItemFromExchangeField = (itemGD) => {
            /// Nếu vật phẩm không tồn tại
            if (itemGD == null || itemGD.GCount <= 0)
            {
                KTGlobal.AddNotification("Vật phẩm không tồn tại!");
                return;
            }

            /// Gửi gói tin thông báo xóa vật phẩm vào sàn giao dịch
            GameInstance.Game.SpriteGoodsExchange(partnerID, (int) GoodsExchangeCmds.RemoveGoods, itemGD.Id);
        };
        this.UIExchange.AddMoneyToExchangeField = (money) => {
            /// Nếu số bạc mang theo không đủ
            if (Global.Data.RoleData.Money < money)
            {
                KTGlobal.AddNotification("Số bạc mang theo không đủ!");
                return;
            }

            /// Gửi gói tin thông báo đặt tiền vào sàn giao dịch
            GameInstance.Game.SpriteGoodsExchange(partnerID, (int) GoodsExchangeCmds.UpdateMoney, money);
        };
        this.UIExchange.Confirm = () => {
            /// Gửi gói tin thông báo xác nhận
            GameInstance.Game.SpriteGoodsExchange(partnerID, (int) GoodsExchangeCmds.Lock, Global.Data.ExchangeID);
        };
        this.UIExchange.Exchange = () => {
            /// Gửi gói tin giao dịch
            GameInstance.Game.SpriteGoodsExchange(partnerID, (int) GoodsExchangeCmds.Done, Global.Data.ExchangeID);
        };
        this.UIExchange.SelfAddPet = (petID) =>
        {
            /// Gửi gói thêm pet vào sàn giao dịch
            GameInstance.Game.SpriteGoodsExchange(partnerID, (int) GoodsExchangeCmds.AddPet, petID);
        };
        this.UIExchange.SelfRemovePet = (petID) =>
        {
            /// Gửi gói hủy pet khỏi sàn giao dịch
            GameInstance.Game.SpriteGoodsExchange(partnerID, (int) GoodsExchangeCmds.RemovePet, petID);
        };
    }

    /// <summary>
    /// Đóng khung giao dịch
    /// </summary>
    public void CloseUIExchange()
    {
        if (this.UIExchange != null)
        {
            GameObject.Destroy(this.UIExchange.gameObject);
            this.UIExchange = null;
        }
    }
    #endregion
}
