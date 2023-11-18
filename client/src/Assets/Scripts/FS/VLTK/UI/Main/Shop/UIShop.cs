using FS.GameEngine.Logic;
using FS.VLTK.Entities.Config;
using FS.VLTK.UI.Main.Shop;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace FS.VLTK.UI.Main
{
    /// <summary>
    /// Khung cửa hàng NPC
    /// </summary>
    public class UIShop : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Button đóng khung
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Close;

        /// <summary>
        /// Tab Header mua lại
        /// </summary>
        [SerializeField]
        private RectTransform UITabHeader_SellAndBuyBackTab;

        /// <summary>
        /// Khung mua vật phẩm
        /// </summary>
        [SerializeField]
        private UIShop_BuyTab UI_BuyTab;

        /// <summary>
        /// Khung bán và mua lại vật phẩm đã bán
        /// </summary>
        [SerializeField]
        private UIShop_SellAndBuyBackTab UI_SellAndBuyBackTab;

        /// <summary>
        /// Text tên cửa hàng
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Title;
        #endregion

        #region Properties
        /// <summary>
        /// Sự kiện mua vật phẩm từ cửa hàng
        /// </summary>
        public Action<ShopItem, int> Buy { get; set; }

        /// <summary>
        /// Sự kiện mua lại
        /// </summary>
        public Action<GoodsData> BuyBack { get; set; }

        /// <summary>
        /// Sự kiện bán vật phẩm trong túi
        /// </summary>
        public Action<GoodsData> Sell { get; set; }

        /// <summary>
        /// Sự kiện đóng khung
        /// </summary>
        public Action Close { get; set; }

        private ShopTab _Data;
        /// <summary>
        /// Dữ liệu cửa hàng
        /// </summary>
        public ShopTab Data
        {
            get
            {
                return this._Data;
            }
            set
            {
                this._Data = value;

                if (this._Data == null)
                {
                    KTGlobal.AddNotification("Dữ liệu truyền về bị lỗi, hãy thử lại sau!");
                    return;
                }

                /// Tên cửa hàng
                this.UIText_Title.text = value.ShopName;

                /// Đổ dữ liệu cho danh sách vật phẩm bán
                foreach (ShopItem shopItem in value.Items)
                {
                    /// Đổ dữ liệu tham chiếu shop
                    shopItem.ShopTab = value;
                    /// Nếu có Limit theo thời gian
                    if (!string.IsNullOrEmpty(value.TimeSaleEnd))
                    {
                        shopItem.LimitType = (int) LimitType.LimitTime;
                        DateTime dt = DateTime.Parse(value.TimeSaleEnd);
                        shopItem.EndTime = dt - DateTime.Now;
                    }
                }

                /// Nếu là cửa hàng bang hội
                if (value.ShopID == 226)
                {
                    /// Ẩn Tab mua lại
                    this.UITabHeader_SellAndBuyBackTab.gameObject.SetActive(false);
                }
                /// Nếu không phải cửa hàng bang hội
                else
                {
                    /// Hiện Tab mua lại
                    this.UITabHeader_SellAndBuyBackTab.gameObject.SetActive(true);
                }

                /// Dữ liệu Tab mua
                this.UI_BuyTab.Items = value.Items;
                this.UI_BuyTab.MoneyType = (Entities.Enum.MoneyType) value.MoneyType;
                /// Dữ liệu Tab bán và mua lại
                this.UI_SellAndBuyBackTab.Items = value.TotalSellItem;
                this.UI_SellAndBuyBackTab.MoneyType = (Entities.Enum.MoneyType) value.MoneyType;
            }
        }
        #endregion

        #region Core MonoBehavour
        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.InitPrefabs();
        }
        #endregion

        #region Code UI
        /// <summary>
        /// Khởi tạo ban đầu
        /// </summary>
        private void InitPrefabs()
        {
            this.UIButton_Close.onClick.AddListener(this.ButtonClose_Clicked);
            this.UI_BuyTab.Buy = this.Buy;
            this.UI_SellAndBuyBackTab.BuyBack = this.BuyBack;
            this.UI_SellAndBuyBackTab.Sell = this.Sell;
        }

        /// <summary>
        /// Sự kiện khi Button đóng khung được ấn
        /// </summary>
        private void ButtonClose_Clicked()
        {
            this.Close?.Invoke();
        }
        #endregion

        #region Private methods

        #endregion

        #region Public methods
        /// <summary>
        /// Thêm vật phẩm vào danh sách có thể mua lại
        /// </summary>
        /// <param name="itemGD"></param>
        public void AddItemToBuyBack(GoodsData itemGD)
        {
            this.UI_SellAndBuyBackTab.AddItem(itemGD);
        }

        /// <summary>
        /// Xóa vật phẩm tương ứng khỏi danh sách có thể mua lại
        /// </summary>
        /// <param name="itemDbID"></param>
        public void RemoveItemFromBuyBackList(int itemDbID)
        {
            this.UI_SellAndBuyBackTab.RemoveItem(itemDbID);
        }
        #endregion
    }
}
