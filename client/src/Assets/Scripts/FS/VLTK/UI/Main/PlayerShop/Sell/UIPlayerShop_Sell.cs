using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using FS.VLTK.UI.Main.Bag;
using FS.VLTK.UI.Main.PlayerShop;
using System.Collections;
using Server.Data;
using FS.GameEngine.Logic;

namespace FS.VLTK.UI.Main
{
    /// <summary>
    /// Khung bán hàng
    /// </summary>
    public class UIPlayerShop_Sell : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Button đóng khung
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Close;

        /// <summary>
        /// Input tên cửa hàng
        /// </summary>
        [SerializeField]
        private TMP_InputField UIInput_ShopName;

        /// <summary>
        /// Túi đồ
        /// </summary>
        [SerializeField]
        private UIBag_Grid UIBagGrid;

        /// <summary>
        /// Prefab vật phẩm được bán
        /// </summary>
        [SerializeField]
        private UIPlayerShop_Sell_Item UIItem_Prefab;

        /// <summary>
        /// Khung thêm vật phẩm bán
        /// </summary>
        [SerializeField]
        private UIPlayerShop_Sell_AddItemToSellFrame UI_AddItemToSellFrame;

        /// <summary>
        /// RectTransform khung
        /// </summary>
        [SerializeField]
        private RectTransform UI_Frame;

        /// <summary>
        /// Button bắt đầu bán vật phẩm
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Start;

        /// <summary>
        /// Button ủy thác bán hàng
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_StartCommission;

        /// <summary>
        /// Button hủy bán hàng
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Cancel;

        /// <summary>
        /// Text trạng thái cửa hàng
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_ShopStatus;
        #endregion

        #region Private fields
        /// <summary>
        /// RectTransform danh sách vật phẩm bán
        /// </summary>
        private RectTransform transformItemsList = null;
        #endregion

        #region Properties
        /// <summary>
        /// Sự kiện đóng khung
        /// </summary>
        public Action Close { get; set; }

        /// <summary>
        /// Sự kiện thêm vật phẩm vào cửa hàng để bán
        /// </summary>
        public Action<GoodsData, int> AddItemToSell { get; set; }

        /// <summary>
        /// Xóa vật phẩm khỏi danh sách bán
        /// </summary>
        public Action<GoodsData> RemoveItemFromSell { get; set; }

        /// <summary>
        /// Sự kiện bắt đầu bán hàng
        /// </summary>
        public Action<string> StartSelling { get; set; }

        /// <summary>
        /// Sự kiện bắt đầu ủy thác bán hàng
        /// </summary>
        public Action<string> StartCommissionSelling { get; set; }

        /// <summary>
        /// Sự kiện hủy bán hàng
        /// </summary>
        public Action CancelSelling { get; set; }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            this.transformItemsList = this.UIItem_Prefab.transform.parent.GetComponent<RectTransform>();
        }

        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.InitPrefabs();
            /// Làm mới dữ liệu
            this.DoRefreshShop();
        }
        #endregion

        #region Code UI
        /// <summary>
        /// Khởi tạo ban đầu
        /// </summary>
        private void InitPrefabs()
        {
            this.UIButton_Close.onClick.AddListener(this.ButtonClose_Clicked);
            this.UIBagGrid.BagItemClicked = this.BagGridItem_Clicked;
            this.UIButton_Start.onClick.AddListener(this.ButtonStart_Clicked);
            this.UIButton_StartCommission.onClick.AddListener(this.ButtonStartCommission_Clicked);
            this.UIButton_Cancel.onClick.AddListener(this.ButtonCancel_Clicked);
        }

        /// <summary>
        /// Sự kiện khi Button đóng khung được ấn
        /// </summary>
        private void ButtonClose_Clicked()
        {
            /// Đóng khung
            this.Close?.Invoke();
        }

        /// <summary>
        /// Sự kiện khi vật phẩm trong túi đồ được ấn
        /// </summary>
        /// <param name="itemGD"></param>
        private void BagGridItem_Clicked(GoodsData itemGD)
        {
            /// Dữ liệu vật phẩm
            if (!Loader.Loader.Items.TryGetValue(itemGD.GoodsID, out Entities.Config.ItemData itemData))
            {
                return;
            }

            /// Nếu đã bán rồi
            if (Global.Data.StallDataItem.Start == 1)
            {
                KTGlobal.ShowItemInfo(itemGD);
            }
            /// Nếu vật phẩm đã khóa
            else if (itemGD.Binding == 1)
            {
                KTGlobal.ShowItemInfo(itemGD);
            }
            else
            {
                List<KeyValuePair<string, Action>> buttons = new List<KeyValuePair<string, Action>>();
                buttons.Add(new KeyValuePair<string, Action>("Đặt lên", () => {
                    this.UI_AddItemToSellFrame.Data = itemGD;
                    this.UI_AddItemToSellFrame.Sell = () => {
                        this.AddItemToSell?.Invoke(itemGD, this.UI_AddItemToSellFrame.Price);
                        this.UI_AddItemToSellFrame.Hide();
                    };

                    this.UI_AddItemToSellFrame.Show();
                    KTGlobal.CloseItemInfo();
                }));
                KTGlobal.ShowItemInfo(itemGD, buttons);
            }
        }

        /// <summary>
        /// Sự kiện khi vật phẩm trong danh sách bán được ấn
        /// </summary>
        /// <param name="uiSellItem"></param>
        private void SellItem_Clicked(UIPlayerShop_Sell_Item uiSellItem)
        {
            /// Dữ liệu vật phẩm
            if (!Loader.Loader.Items.TryGetValue(uiSellItem.Data.GoodsID, out Entities.Config.ItemData itemData))
            {
                return;
            }

            /// Nếu đã bán rồi
            if (Global.Data.StallDataItem.Start == 1)
            {
                KTGlobal.ShowItemInfo(uiSellItem.Data);
            }
            /// Nếu chưa bán
            else
            {
                List<KeyValuePair<string, Action>> buttons = new List<KeyValuePair<string, Action>>();
                buttons.Add(new KeyValuePair<string, Action>("Thu hồi", () => {
                    /// Nếu đã bán rồi
                    if (Global.Data.StallDataItem.Start == 1)
                    {
                        /// Thông báo
                        KTGlobal.AddNotification("Đã bày bán, không thể thu hồi!");
                        /// Bỏ qua
                        return;
                    }

                    KTGlobal.ShowMessageBox("Gỡ vật phẩm", "Xác nhận gỡ vật phẩm khỏi cửa hàng?", () => {
                        this.RemoveItemFromSell?.Invoke(uiSellItem.Data);
                        KTGlobal.CloseItemInfo();
                    }, true);
                }));
                KTGlobal.ShowItemInfo(uiSellItem.Data, buttons);
            }
        }

        /// <summary>
        /// Sự kiện khi Button bắt đầu bán hàng được ấn
        /// </summary>
        private void ButtonStart_Clicked()
        {
            /// Nếu đang bán rồi
            if (Global.Data.StallDataItem.Start == 1)
            {
                /// Thông báo
                KTGlobal.AddNotification("Đang bán hàng, không thể thao tác!");
                /// Bỏ qua
                return;
            }

            /// Tên sạp
            string shopName = this.UIInput_ShopName.text;
            /// Nếu chưa nhập tên cửa hàng
            if (string.IsNullOrEmpty(shopName))
            {
                /// Thông báo
                KTGlobal.AddNotification("Hãy nhập tên cửa hàng!");
                /// Bỏ qua
                return;
            }
            /// Toác
            else if (!KTFormValidation.IsValidString(shopName, false, true, true, false))
            {
                /// Thông báo
                KTGlobal.AddNotification("Tên cửa hàng không thể chứa ký tự đặc biệt!");
                /// Bỏ qua
                return;
            }

            /// Nếu danh sách vật phẩm rỗng
            if (Global.Data.StallDataItem.GoodsList == null || Global.Data.StallDataItem.GoodsList.Count <= 0)
            {
                /// Thông báo
                KTGlobal.AddNotification("Hãy đặt các vật phẩm muốn bán lên sạp!");
                /// Bỏ qua
                return;
            }

            /// Thực thi sự kiện
            this.StartSelling?.Invoke(shopName);
        }

        /// <summary>
        /// Sự kiện khi Button ủy thác được ấn
        /// </summary>
        private void ButtonStartCommission_Clicked()
        {
            /// Nếu đang bán rồi
            if (Global.Data.StallDataItem.Start == 1)
            {
                /// Thông báo
                KTGlobal.AddNotification("Đang bán hàng, không thể thao tác!");
                /// Bỏ qua
                return;
            }

            /// Tên sạp
            string shopName = this.UIInput_ShopName.text;
            /// Nếu chưa nhập tên cửa hàng
            if (string.IsNullOrEmpty(shopName))
            {
                /// Thông báo
                KTGlobal.AddNotification("Hãy nhập tên cửa hàng!");
                /// Bỏ qua
                return;
            }
            /// Toác
            else if (!KTFormValidation.IsValidString(shopName, false, true, true, false))
            {
                /// Thông báo
                KTGlobal.AddNotification("Tên cửa hàng không thể chứa ký tự đặc biệt!");
                /// Bỏ qua
                return;
            }

            /// Nếu danh sách vật phẩm rỗng
            if (Global.Data.StallDataItem.GoodsList == null || Global.Data.StallDataItem.GoodsList.Count <= 0)
            {
                /// Thông báo
                KTGlobal.AddNotification("Hãy đặt các vật phẩm muốn bán lên sạp!");
                /// Bỏ qua
                return;
            }

            /// Thực thi sự kiện
            this.StartCommissionSelling?.Invoke(shopName);
        }

        /// <summary>
        /// Sự kiện khi Button hủy bán hàng được ấn
        /// </summary>
        private void ButtonCancel_Clicked()
        {
            /// Nếu chưa bán rồi
            if (Global.Data.StallDataItem.Start != 1)
            {
                /// Bỏ qua
                return;
            }

            /// Hiện thông báo
            KTGlobal.ShowMessageBox("Xác nhận hủy bán hàng?", () =>
            {
                this.CancelSelling?.Invoke();
            }, true);
        }
        #endregion

        #region Private fields
        /// <summary>
        /// Thực thi sự kiện bỏ qua một số Frame
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="work"></param>
        /// <returns></returns>
        private IEnumerator ExecuteSkipFrames(int skip, Action work)
        {
            for (int i = 1; i <= skip; i++)
            {
                yield return null;
            }
            work?.Invoke();
        }

        /// <summary>
        /// Tìm ô vật phẩm tương ứng
        /// </summary>
        /// <param name="dbID"></param>
        /// <returns></returns>
        private UIPlayerShop_Sell_Item FindItem(int dbID)
        {
            /// Xóa toàn bộ vật phẩm
            foreach (Transform child in this.transformItemsList.transform)
            {
                if (child.gameObject != this.UIItem_Prefab.gameObject)
                {
                    /// UI tương ứng
                    UIPlayerShop_Sell_Item uiSellItem = child.GetComponent<UIPlayerShop_Sell_Item>();
                    /// Thỏa mãn
                    if (uiSellItem.Data.Id == dbID)
                    {
                        /// Trả về kết quả
                        return uiSellItem;
                    }
                }
            }
            /// Không tìm thấy
            return null;
        }

        /// <summary>
        /// Thêm vật phẩm tương ứng
        /// </summary>
        /// <param name="itemGD"></param>
        /// <param name="price"></param>
        private void DoAddItem(GoodsData itemGD, int price)
        {
            /// Tạo mới UI
            UIPlayerShop_Sell_Item uiSellItem = GameObject.Instantiate<UIPlayerShop_Sell_Item>(this.UIItem_Prefab);
            uiSellItem.transform.SetParent(this.transformItemsList, false);
            uiSellItem.gameObject.SetActive(true);
            /// Thiết lập giá trị
            uiSellItem.Data = itemGD;
            uiSellItem.Price = price;
            uiSellItem.ItemClick = () => {
                this.SellItem_Clicked(uiSellItem);
            };
            uiSellItem.Remove = () =>
            {
                this.RemoveItemFromSell?.Invoke(itemGD);
            };
            uiSellItem.EnableRemove = Global.Data.StallDataItem.Start != 1;
        }

        /// <summary>
        /// Làm mới dữ liệu cửa hàng
        /// </summary>
        private void DoRefreshShop()
        {
            /// Nếu không có dữ liệu
            if (Global.Data.StallDataItem == null)
            {
                /// Thông báo
                KTGlobal.AddNotification("Không có dữ liệu sạp hàng!");
                /// Đóng khung
                this.Close?.Invoke();
                /// Bỏ qua
                return;
            }

            /// Xóa toàn bộ vật phẩm
            foreach (Transform child in this.transformItemsList.transform)
            {
                if (child.gameObject != this.UIItem_Prefab.gameObject)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }

            /// Tên sạp
            this.UIInput_ShopName.text = Global.Data.StallDataItem.StallName;

            /// Trạng thái
            string status = "";
            /// Nếu chưa mở
            if (Global.Data.StallDataItem.Start != 1)
            {
                status = "<color=red>Chưa hoạt động</color>";
            }
            /// Nếu đã mở
            else
            {
                /// Nếu là ủy thác
                if (Global.Data.StallDataItem.IsBot)
                {
                    status = "<color=green>Đang ủy thác</color>";
                }
                /// Nếu là bán thường
                else
                {
                    status = "<color=green>Đang bày bán</color>";
                }
            }
            this.UIText_ShopStatus.text = status;

            /// Rỗng thì tạo mới
            if (Global.Data.StallDataItem.GoodsList == null)
            {
                Global.Data.StallDataItem.GoodsList = new List<GoodsData>();
            }
            if (Global.Data.StallDataItem.GoodsPriceDict == null)
            {
                Global.Data.StallDataItem.GoodsPriceDict = new Dictionary<int, int>();
            }

            /// Duyệt danh sách vật phẩm và thêm vào
            if (Global.Data.StallDataItem.GoodsList != null && Global.Data.StallDataItem.GoodsPriceDict != null)
            {
                foreach (GoodsData itemGD in Global.Data.StallDataItem.GoodsList)
                {
                    if (Global.Data.StallDataItem.GoodsPriceDict.TryGetValue(itemGD.Id, out int price))
                    {
                        this.DoAddItem(itemGD, price);
                    }
                }
            }

            /// Xây lại giao diện
            this.StartCoroutine(this.ExecuteSkipFrames(1, () => {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.transformItemsList);
            }));

            /// Các Button chức năng
            this.UIButton_Start.interactable = Global.Data.StallDataItem.Start != 1;
            this.UIButton_StartCommission.interactable = Global.Data.StallDataItem.Start != 1;
            this.UIButton_Cancel.interactable = Global.Data.StallDataItem.Start == 1;
            this.UIInput_ShopName.interactable = Global.Data.StallDataItem.Start != 1;
        }
        #endregion

        #region Public fields
        /// <summary>
        /// Làm mới dữ liệu
        /// </summary>
        public void RefreshShop()
        {
            this.DoRefreshShop();
        }

        /// <summary>
        /// Thêm vật phẩm
        /// </summary>
        /// <param name="itemGD"></param>
        /// <param name="price"></param>
        public void AddItem(GoodsData itemGD, int price)
        {
            /// Ô vật phẩm cũ
            UIPlayerShop_Sell_Item uiSellItem = this.FindItem(itemGD.Id);
            /// Nếu tìm thấy
            if (uiSellItem != null)
            {
                /// Xóa ô cũ
                GameObject.Destroy(uiSellItem.gameObject);
            }

            /// Thêm mới
            this.DoAddItem(itemGD, price);

            /// Xây lại giao diện
            this.StartCoroutine(this.ExecuteSkipFrames(1, () => {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.transformItemsList);
            }));
        }

        /// <summary>
        /// Xóa vật phẩm
        /// </summary>
        /// <param name="dbID"></param>
        public void RemoveItem(int dbID)
        {
            /// Ô vật phẩm cũ
            UIPlayerShop_Sell_Item uiSellItem = this.FindItem(dbID);
            /// Nếu tìm thấy
            if (uiSellItem != null)
            {
                /// Xóa ô cũ
                GameObject.Destroy(uiSellItem.gameObject);
                /// Xây lại giao diện
                this.StartCoroutine(this.ExecuteSkipFrames(1, () => {
                    UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.transformItemsList);
                }));
            }
        }
        #endregion
    }
}
