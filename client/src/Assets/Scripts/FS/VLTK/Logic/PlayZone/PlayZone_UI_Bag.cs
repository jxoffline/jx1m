using FS.GameEngine.Logic;
using FS.GameEngine.Network;
using FS.VLTK;
using FS.VLTK.Entities.Config;
using FS.VLTK.Loader;
using FS.VLTK.Network;
using FS.VLTK.UI;
using FS.VLTK.UI.Main;
using UnityEngine;

/// <summary>
/// Quản lý các khung giao diện trong màn chơi
/// </summary>
public partial class PlayZone
{
    #region UI Bag
    /// <summary>
    /// Khung túi đồ
    /// </summary>
    public UIBag UIBag { get; protected set; }

    /// <summary>
    /// Hiện khung túi đồ
    /// </summary>
    public void ShowUIBag()
    {
        if (this.UIBag != null)
        {
            return;
        }

        CanvasManager canvas = Global.MainCanvas.GetComponent<CanvasManager>();
        this.UIBag = canvas.LoadUIPrefab<UIBag>("MainGame/UIBag");
        canvas.AddUI(this.UIBag);

        this.UIBag.Close = this.HideUIBag;
        this.UIBag.Sort = () => {
            /// Nếu túi đồ rỗng
            if (Global.Data.RoleData.GoodsDataList == null || Global.Data.RoleData.GoodsDataList.Count <= 0)
            {
                return;
            }

            /// Gửi gói tin yêu cầu sắp xếp túi đồ
            GameInstance.Game.SpriteSortBag();
        };
        this.UIBag.OpenMyselfShop = () => {
            /// Nếu đang cưỡi thì thông báo
            if (Global.Data.RoleData.IsRiding)
            {
                KTGlobal.AddNotification("Trong trạng thái cưỡi không thể mở sạp hàng!");
                return;
            }

            /// Đóng khung túi đồ
            this.HideUIBag();
            /// Gửi gói tin về GS yêu cầu mở gian hàng
            KT_TCPHandler.SendOpenStall(Global.Data.RoleData.RoleID);
        };
        this.UIBag.Use = (itemGD) => {
            /// Nếu đang trong trạng thái bị khống chế
            if (!Global.Data.Leader.CanDoLogic)
            {
                KTGlobal.AddNotification("Trong trạng thái bị khống chế không thể sử dụng vật phẩm!");
                return;
            }

            /// Cấu hình vật phẩm
            ItemData itemData = null;
            if (!Loader.Items.TryGetValue(itemGD.GoodsID, out itemData))
            {
                KTGlobal.AddNotification("Vật phẩm bị lỗi, hãy thông báo với hỗ trợ để được xử lý!");
                return;
            }

            /// Nếu không thể sử dụng, và không phải thuốc
            if (!KTGlobal.IsItemCanUse(itemGD.GoodsID))
            {
                KTGlobal.AddNotification("Vật phẩm này không thể sử dụng được!");
                return;
            }

            /// Sử dụng
            GameInstance.Game.SpriteUseGoods(itemGD.Id);

            /// Đóng khung Tooltip vật phẩm
            KTGlobal.CloseItemInfo();
        };
        this.UIBag.Equip = (itemGD) => {
            /// Nếu đang trong trạng thái bị khống chế
            if (!Global.Data.Leader.CanDoLogic)
            {
                KTGlobal.AddNotification("Trong trạng thái bị khống chế không thể thay đổi trang bị!");
                return;
            }

            /// Cấu hình vật phẩm
            ItemData itemData = null;
            if (!Loader.Items.TryGetValue(itemGD.GoodsID, out itemData))
            {
                KTGlobal.AddNotification("Vật phẩm bị lỗi, hãy thông báo với hỗ trợ để được xử lý!");
                return;
            }

            /// Nếu là trang bị
            if (itemData.IsEquip)
            {
                /// Vị trí muốn mặc
                int pos = Loader.g_anEquipPos[itemData.DetailType];
                GameInstance.Game.SpriteModGoods((int) ModGoodsTypes.EquipLoad, itemGD.Id, itemGD.GoodsID, Global.Data.ShowReserveEquip ? 100 + pos : pos, itemGD.Site, 1, itemGD.BagIndex);
            }
            /// Nếu là trang bị pet
            else if (KTGlobal.IsPetEquip(itemData.Genre))
            {
                /// Vị trí muốn mặc
                int pos = Loader.g_anEquipPetPos[itemData.DetailType];
                GameInstance.Game.SpriteModGoods((int) ModGoodsTypes.EquipLoad, itemGD.Id, itemGD.GoodsID, pos, itemGD.Site, 1, itemGD.BagIndex);
            }

            /// Đóng khung Tooltip vật phẩm
            KTGlobal.CloseItemInfo();
        };
        this.UIBag.ThrowAway = (itemGD) => {
            /// Nếu đang trong trạng thái bị khống chế
            if (!Global.Data.Leader.CanDoLogic)
            {
                KTGlobal.AddNotification("Trong trạng thái bị khống chế không thể vứt bỏ vật phẩm!");
                return;
            }

            /// Cấu hình vật phẩm
            ItemData itemData = null;
            if (!Loader.Items.TryGetValue(itemGD.GoodsID, out itemData))
            {
                KTGlobal.AddNotification("Vật phẩm bị lỗi, hãy thông báo với hỗ trợ để được xử lý!");
                return;
            }

            /// Nếu vật phẩm đã khóa thì không thể vứt bỏ
            if (itemGD.Binding == 1)
            {
                KTGlobal.AddNotification("Vật phẩm đã khóa, không thể vứt bỏ!");
                return;
            }

            KTGlobal.ShowMessageBox("Vứt bỏ", string.Format("Xác nhận vứt bỏ vật phẩm <color=yellow>{0}</color>?", itemData.Name), () => {
                GameInstance.Game.SpriteModGoods((int) ModGoodsTypes.Abandon, itemGD.Id, itemGD.GoodsID, -1, itemGD.Site, itemGD.GCount, itemGD.BagIndex);
                KTGlobal.CloseItemInfo();
            }, true);
        };
        this.UIBag.Advertise = (itemGD) => {
            this.ShowUIAdvertiseItem(itemGD);
            KTGlobal.CloseItemInfo();
        };
        this.UIBag.Split = (itemGD, number) => {
            GameInstance.Game.SpriteModGoods((int) ModGoodsTypes.SplitItem, itemGD.Id, itemGD.GoodsID, -1, itemGD.Site, itemGD.GCount - number, itemGD.BagIndex, number.ToString());
            KTGlobal.CloseItemInfo();
        };
    }

    /// <summary>
    /// Đóng khung túi đồ
    /// </summary>
    public void HideUIBag()
    {
        if (this.UIBag != null)
        {
            GameObject.Destroy(this.UIBag.gameObject);
            this.UIBag = null;
        }
    }
    #endregion

    #region UI Portable Bag
    /// <summary>
    /// Khung thương khố
    /// </summary>
    public UIPortableBag UIPortableBag { get; protected set; }

    /// <summary>
    /// Hiện khung thương khố
    /// </summary>
    public void OpenUIPortableBag()
    {
        if (this.UIPortableBag != null)
        {
            return;
        }

        CanvasManager canvas = Global.MainCanvas.GetComponent<CanvasManager>();
        this.UIPortableBag = canvas.LoadUIPrefab<UIPortableBag>("MainGame/UIPortableBag");
        canvas.AddUI(this.UIPortableBag);

        this.UIPortableBag.Close = this.CloseUIPortableBag;
        this.UIPortableBag.Sort = () => {
            /// Nếu túi đồ rỗng
            if (Global.Data.RoleData.GoodsDataList == null || Global.Data.RoleData.GoodsDataList.Count <= 0)
            {
                return;
            }

            /// Gửi gói tin yêu cầu sắp xếp túi đồ
            GameInstance.Game.SpriteSortPortableBag();
        };
        this.UIPortableBag.AddStoreMoney = (value) => {
            /// Nếu số bạc thêm vào vượt quá số bạc đang có
            if (value > Global.Data.RoleData.Money)
            {
                KTGlobal.AddNotification("Số bạc trong túi không đủ!");
                return;
            }

            /// Gửi gói tin thêm bạc vào kho
            GameInstance.Game.ModifyStoreMoney(1, value);
        };
        this.UIPortableBag.WithdrawStoreMoney = (value) => {
            /// Nếu số bạc trong kho vượt quá số bạc rút
            if (value > Global.Data.RoleData.StoreMoney)
            {
                KTGlobal.AddNotification("Số bạc trong thương khố không đủ!");
                return;
            }

            /// Gửi gói tin rút bạc từ kho
            GameInstance.Game.ModifyStoreMoney(-1, value);
        };
        this.UIPortableBag.AddItemToPortableBag = (itemGD) => {
            /// Nếu đang trong trạng thái bị khống chế
            if (!Global.Data.Leader.CanDoLogic)
            {
                KTGlobal.AddNotification("Trong trạng thái bị khống chế không thể sử dụng vật phẩm!");
                return;
            }

            /// Cấu hình vật phẩm
            ItemData itemData = null;
            if (!Loader.Items.TryGetValue(itemGD.GoodsID, out itemData))
            {
                KTGlobal.AddNotification("Vật phẩm bị lỗi, hãy thông báo với hỗ trợ để được xử lý!");
                return;
            }

            /// Nếu vật phẩm không nằm trong túi đồ
            if (itemGD.Site != 0)
            {
                KTGlobal.AddNotification("Vật phẩm không tồn tại!");
                return;
            }

            /// Gửi gói tin thêm vật phẩm vào thương khố
            GameInstance.Game.SpriteModGoods((int) ModGoodsTypes.ModValue, itemGD.Id, itemGD.GoodsID, -1, 1, itemGD.GCount, -1);

            /// Đóng khung Tooltip vật phẩm
            KTGlobal.CloseItemInfo();
        };
        this.UIPortableBag.TakeoutItemFromPortableBag = (itemGD) => {
            /// Nếu đang trong trạng thái bị khống chế
            if (!Global.Data.Leader.CanDoLogic)
            {
                KTGlobal.AddNotification("Trong trạng thái bị khống chế không thể sử dụng vật phẩm!");
                return;
            }

            /// Cấu hình vật phẩm
            ItemData itemData = null;
            if (!Loader.Items.TryGetValue(itemGD.GoodsID, out itemData))
            {
                KTGlobal.AddNotification("Vật phẩm bị lỗi, hãy thông báo với hỗ trợ để được xử lý!");
                return;
            }

            /// Nếu vật phẩm không nằm trong thương khố
            if (itemGD.Site != 1)
            {
                KTGlobal.AddNotification("Vật phẩm không tồn tại!");
                return;
            }

            /// Gửi gói tin thêm vật phẩm vào thương khố
            GameInstance.Game.SpriteModGoods((int) ModGoodsTypes.ModValue, itemGD.Id, itemGD.GoodsID, -1, 0, itemGD.GCount, -1);


            /// Đóng khung Tooltip vật phẩm
            KTGlobal.CloseItemInfo();
        };
        this.UIPortableBag.Advertise = (itemGD) => {
            this.ShowUIAdvertiseItem(itemGD);
            KTGlobal.CloseItemInfo();
        };
    }

    /// <summary>
    /// Đóng khung thương khố
    /// </summary>
    public void CloseUIPortableBag()
    {
        if (this.UIPortableBag != null)
        {
            GameObject.Destroy(this.UIPortableBag.gameObject);
            this.UIPortableBag = null;
        }
    }
    #endregion
}
