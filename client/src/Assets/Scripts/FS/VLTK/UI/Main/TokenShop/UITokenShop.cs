using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using FS.VLTK.Utilities.UnityUI;
using System.Collections;
using Server.Data;
using FS.VLTK.UI.Main.TokenShop;
using static FS.VLTK.Entities.Enum;
using FS.VLTK.Entities.Config;
using FS.VLTK.UI.Main.TokenShop.StoreProduct;


namespace FS.VLTK.UI.Main
{
    /// <summary>
    /// Khung Kỳ Trân Các
    /// </summary>
    public class UITokenShop : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Button đóng khung
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Close;

        /// <summary>
        /// Frame chứa danh sách món hàng bán trong cửa hàng
        /// </summary>
        [SerializeField]
        private RectTransform UI_MainTokenShop_Frame;

        /// <summary>
        /// Toggle tiệm KNB
        /// </summary>
        [SerializeField]
        private UIToggleSprite UIToggle_TokenShop;

        /// <summary>
        /// Toggle tiệm KNB khóa
        /// </summary>
        [SerializeField]
        private UIToggleSprite UIToggle_BoundTokenShop;

        /// <summary>
        /// Toggle mở khung mua hàng trên Store
        /// </summary>
        [SerializeField]
        private UIToggleSprite UIToggle_OpenBuyStoreProductFrame;

        /// <summary>
        /// Prefab Toggle tên cửa hàng
        /// </summary>
        [SerializeField]
        private UIToggleSprite UIToggle_ShopNamePrefab;

        /// <summary>
        /// Prefab vật phẩm được bán
        /// </summary>
        [SerializeField]
        private UITokenShop_Item UIItem_Prefab;

        /// <summary>
        /// Khung xác nhận mua vật phẩm
        /// </summary>
        [SerializeField]
        private UITokenShop_ConfirmBuyFrame UI_ConfirmBuyFrame;

        /// <summary>
        /// Button trang trước
        /// </summary>
        [SerializeField]
        private UIButtonSprite UIButton_PreviousPage;

        /// <summary>
        /// Button trang tiếp theo
        /// </summary>
        [SerializeField]
        private UIButtonSprite UIButton_NextPage;

        /// <summary>
        /// Text thứ tự trang hiện tại
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_CurrentPageNumber;

        /// <summary>
        /// Khung mua hàng trên Store
        /// </summary>
        [SerializeField]
        private UITokenShop_StoreProductBuy UI_StoreProductBuy;
        #endregion

        #region Constants
        /// <summary>
        /// Tổng số vật phẩm trong một trang
        /// </summary>
        private const int ItemPerPage = 9;
        #endregion

        #region Private fields
        /// <summary>
        /// RectTransform chứa danh sách cửa hàng trên đầu
        /// </summary>
        private RectTransform rectTransformTopShopList = null;

        /// <summary>
        /// RectTransform chứa danh sách vật phẩm
        /// </summary>
        private RectTransform rectTransformItemList = null;

        /// <summary>
        /// Thứ tự trang hiện tại
        /// </summary>
        private int currentPageIndex = -1;

        /// <summary>
        /// Tổng số trang
        /// </summary>
        private int maxPages;

        /// <summary>
        /// Cửa hàng hiện tại
        /// </summary>
        private ShopTab currentShop;
        #endregion

        #region Properties
        /// <summary>
        /// Dữ liệu Kỳ Trân Các
        /// </summary>
        public Server.Data.TokenShop Data { get; set; }

        /// <summary>
        /// Sự kiện mua vật phẩm
        /// </summary>
        public Action<ShopItem, int, int, int> Buy { get; set; }

        /// <summary>
        /// Sự kiện đóng khung
        /// </summary>
        public Action Close { get; set; }

        /// <summary>
        /// Sự kiện mua hàng trên Store
        /// </summary>
        public Action<TokenShopStoreProduct> StoreBuyItem { get; set; }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            this.rectTransformTopShopList = this.UIToggle_ShopNamePrefab.transform.parent.GetComponent<RectTransform>();
            this.rectTransformItemList = this.UIItem_Prefab.transform.parent.GetComponent<RectTransform>();
        }

        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.InitPrefabs();
            this.InitItemSlots();
            this.InitStoreProductBuy();
            this.ShowMainTokenShop(true);
        }
        #endregion

        #region Code UI
        /// <summary>
        /// Khởi tạo IAP
        /// </summary>
        /// <param name="done"></param>
        /// <returns></returns>
      

        /// <summary>
        /// Khởi tạo mua hàng trên Store
        /// </summary>
        private void InitStoreProductBuy()
        {
#if !UNITY_IOS
            this.UI_StoreProductBuy.Data = this.Data.StoreProducts;
            this.UI_StoreProductBuy.Click = (productData) => {
                /// Nếu IAP chưa được khởi tạo
               
            };
#else
            this.UIToggle_OpenBuyStoreProductFrame.gameObject.SetActive(false);
#endif
        }

        /// <summary>
        /// Hiện Main Token Shop
        /// </summary>
        /// <param name="isShow"></param>
        private void ShowMainTokenShop(bool isShow)
        {
            this.UI_MainTokenShop_Frame.gameObject.SetActive(isShow);
            this.UI_StoreProductBuy.gameObject.SetActive(!isShow);
        }

        /// <summary>
        /// Khởi tạo ban đầu
        /// </summary>
        private void InitPrefabs()
        {
            this.UIButton_Close.onClick.AddListener(this.ButtonClose_Clicked);
            this.UIToggle_TokenShop.OnSelected = this.ToggleTokenShop_Selected;
            this.UIToggle_BoundTokenShop.OnSelected = this.ToggleBoundTokenShop_Selected;
            this.UIToggle_OpenBuyStoreProductFrame.OnSelected = this.ToggleOpenBuyProductStoreFrame_Selected;
            this.UI_ConfirmBuyFrame.ConfirmBuy = (shopItemData, quantity, couponID) => {
                this.Buy?.Invoke(shopItemData, quantity, this.currentShop.ShopID, couponID);
            };
            this.UIButton_PreviousPage.Click = this.ButtonPreviousPage_Clicked;
            this.UIButton_NextPage.Click = this.ButtonNextPage_Clicked;

            /// Mặc định chọn Shop KNB
            this.ToggleTokenShop_Selected(true);
        }

        /// <summary>
        /// Sự kiện khi Toggle mở khung mua hàng trên Store được ấn
        /// </summary>
        /// <param name="isSelected"></param>
        private void ToggleOpenBuyProductStoreFrame_Selected(bool isSelected)
        {
            this.ShowMainTokenShop(false);
        }

        /// <summary>
        /// Sự kiện khi Toggle mở tiệm KNB được ấn
        /// </summary>
        /// <param name="isSelected"></param>
        private void ToggleTokenShop_Selected(bool isSelected)
        {
            if (!isSelected)
            {
                return;
            }

            this.ShowMainTokenShop(true);
            this.BuildShopList(this.Data.Token, true);
        }

        /// <summary>
        /// Sự kiện khi Toggle mở tiệm KNB khóa được ấn
        /// </summary>
        /// <param name="isSelected"></param>
        private void ToggleBoundTokenShop_Selected(bool isSelected)
        {
            if (!isSelected)
            {
                return;
            }

            this.ShowMainTokenShop(true);
            this.BuildShopList(this.Data.BoundToken, true);
        }

        /// <summary>
        /// Sự kiện khi Toggle cửa hàng được ấn
        /// </summary>
        /// <param name="shopTab"></param>
        private void ShopTab_Clicked(ShopTab shopTab)
        {
            /// Cập nhật dữ liệu cửa hàng hiện tại
            this.currentShop = shopTab;
            /// Chọn trang đầu tiên
            this.currentPageIndex = 1;

            /// Cập nhật tổng số trang
            this.maxPages = shopTab.Items.Count / UITokenShop.ItemPerPage + 1;
            if (shopTab.Items.Count % UITokenShop.ItemPerPage == 0)
            {
                this.maxPages--;
            }

            /// Xây dữ liệu trang
            this.SelectPage(this.currentPageIndex);
        }

        /// <summary>
        /// Sự kiện khi Button trang trước được ấn
        /// </summary>
        private void ButtonPreviousPage_Clicked()
        {
            /// Nếu không có trang trước
            if (this.currentPageIndex <= 1)
            {
                return;
            }

            /// Giảm trang hiện tại xuống
            this.currentPageIndex--;
            /// Xây dữ liệu trang
            this.SelectPage(this.currentPageIndex);
        }

        /// <summary>
        /// Sự kiện khi Button trang tiếp theo được ấn
        /// </summary>
        private void ButtonNextPage_Clicked()
        {
            /// Nếu không có trang sau
            if (this.currentPageIndex >= this.maxPages)
            {
                this.UIButton_NextPage.Enable = false;
            }

            /// Tăng trang hiện tại lên
            this.currentPageIndex++;
            /// Xây dữ liệu trang
            this.SelectPage(this.currentPageIndex);
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
        /// <summary>
        /// Thực thi sự kiện bỏ qua một số Frame
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="callBack"></param>
        /// <returns></returns>
        private IEnumerator ExecuteSkipFrames(int skip, Action callBack)
        {
            for (int i = 1; i <= skip; i++)
            {
                yield return null;
            }
            callBack?.Invoke();
        }

        /// <summary>
        /// Xây giao diện danh sách cửa hàng phía trên
        /// </summary>
        private void RebuildTopLayout()
        {
            this.StartCoroutine(this.ExecuteSkipFrames(1, () => {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.rectTransformTopShopList);
            }));
        }

        /// <summary>
        /// Làm rỗng danh sách cửa hàng phía trên
        /// </summary>
        private void ClearTopLayout()
        {
            foreach (Transform child in this.rectTransformTopShopList.transform)
            {
                if (child.gameObject != this.UIToggle_ShopNamePrefab.gameObject)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }
            this.RebuildTopLayout();
        }

        /// <summary>
        /// Xây danh sách cửa hàng
        /// </summary>
        /// <param name="shopList"></param>
        /// <param name="selectFirst"></param>
        private void BuildShopList(List<ShopTab> shopList, bool selectFirst = false)
        {
            /// Làm rỗng danh sách
            this.ClearTopLayout();

            /// Đánh dấu đã chọn vị trí đầu tiên chưa
            bool isFirstSelected = false;
            foreach (ShopTab shopTab in shopList)
            {
                UIToggleSprite uiShopName = GameObject.Instantiate<UIToggleSprite>(this.UIToggle_ShopNamePrefab);
                uiShopName.transform.SetParent(this.rectTransformTopShopList, false);
                uiShopName.gameObject.SetActive(true);
                uiShopName.Name = shopTab.ShopName;
                uiShopName.Active = false;
                uiShopName.OnSelected = (isSelected) => {
                    if (isSelected)
                    {
                        this.ShopTab_Clicked(shopTab);
                    }
                };
                /// Nếu chưa chọn vị trí đầu tiên
                if (selectFirst && !isFirstSelected)
                {
                    /// Đánh dấu đã chọn vị trí đầu tiên
                    isFirstSelected = true;
                    this.StartCoroutine(this.ExecuteSkipFrames(5, () => {
                        uiShopName.Active = true;
                        //this.ShopTab_Clicked(shopTab);
                    }));
                }
            }
            this.RebuildTopLayout();
        }

        /// <summary>
        /// Xây giao diện danh sách cửa hàng phía trên
        /// </summary>
        private void RebuildItemListLayout()
        {
            this.StartCoroutine(this.ExecuteSkipFrames(1, () => {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.rectTransformItemList);
            }));
        }

        /// <summary>
        /// Làm rỗng danh sách vật phẩm
        /// </summary>
        private void ClearItemList()
        {
            foreach (Transform child in this.rectTransformItemList.transform)
            {
                if (child.gameObject != this.UIItem_Prefab.gameObject)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }
            this.RebuildItemListLayout();
        }

        /// <summary>
        /// Khởi tạo danh sách vật phẩm được bán
        /// </summary>
        private void InitItemSlots()
        {
            this.ClearItemList();
            for (int i = 1; i <= UITokenShop.ItemPerPage; i++)
            {
                UITokenShop_Item uiItemBox = GameObject.Instantiate<UITokenShop_Item>(this.UIItem_Prefab);
                uiItemBox.transform.SetParent(this.rectTransformItemList, false);
                uiItemBox.gameObject.SetActive(true);
            }
            this.RebuildItemListLayout();
        }

        /// <summary>
        /// Đổ dữ liệu vào danh sách vật phẩm
        /// </summary>
        /// <param name="sellItems"></param>
        /// <param name="fromIdx"></param>
        /// <param name="toIdx"></param>
        /// <param name="moneyType"></param>
        private void AppendItemData(List<ShopItem> sellItems, int fromIdx, int toIdx, MoneyType moneyType)
        {
            int idx = fromIdx;
            foreach (Transform child in this.rectTransformItemList.transform)
            {
                if (child.gameObject != this.UIItem_Prefab.gameObject)
                {
                    UITokenShop_Item uiItemBox = child.GetComponent<UITokenShop_Item>();
                    if (idx <= toIdx)
                    {
                        ShopItem sellItem = sellItems[idx];
                        uiItemBox.Data = sellItem;
                        uiItemBox.MoneyType = moneyType;
                        uiItemBox.Buy = () => {
                            this.UI_ConfirmBuyFrame.Data = sellItem;
                            this.UI_ConfirmBuyFrame.MoneyType = moneyType;
                            this.UI_ConfirmBuyFrame.Show();
                        };
                        uiItemBox.Refresh();
                    }
                    else
                    {
                        uiItemBox.Data = null;
                        uiItemBox.Buy = null;
                        uiItemBox.Refresh();
                    }
                    idx++;
                }
            }
            this.RebuildItemListLayout();
        }

        /// <summary>
        /// Chọn trang tương ứng
        /// </summary>
        private void SelectPage(int pageIdx)
        {
            /// Thiết lập trang hiện tại
            this.currentPageIndex = pageIdx;

            /// Cập nhật hiển thị
            this.UIText_CurrentPageNumber.text = string.Format("{0}/{1}", this.currentPageIndex, this.maxPages);

            /// Nếu trang hiện tại <= 1
            if (this.currentPageIndex <= 1)
            {
                this.UIButton_PreviousPage.Enable = false;
            }
            else
            {
                this.UIButton_PreviousPage.Enable = true;
            }

            /// Nếu trang hiện tại >= tổng số trang
            if (this.currentPageIndex >= this.maxPages)
            {
                this.UIButton_NextPage.Enable = false;
            }
            else
            {
                this.UIButton_NextPage.Enable = true;
            }

            int fromIdx = (this.currentPageIndex - 1) * UITokenShop.ItemPerPage;
            int toIdx = Math.Min(fromIdx + UITokenShop.ItemPerPage, this.currentShop.Items.Count) - 1;
            this.AppendItemData(this.currentShop.Items, fromIdx, toIdx, (MoneyType) this.currentShop.MoneyType);
        }
        #endregion
    }
}
