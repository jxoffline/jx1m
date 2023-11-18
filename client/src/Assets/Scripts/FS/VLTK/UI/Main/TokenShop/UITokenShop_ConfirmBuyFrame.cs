using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using FS.VLTK.UI.Main.ItemBox;
using FS.VLTK.Utilities.UnityUI;
using FS.VLTK.Entities.Config;
using static FS.VLTK.Entities.Enum;
using Server.Data;
using FS.GameEngine.Logic;
using System.Collections;
using FS.VLTK.UI.Main.MainUI.RadarMap;

namespace FS.VLTK.UI.Main.TokenShop
{
    /// <summary>
    /// Khung xác nhận mua vật phẩm ở Kỳ Trân Các
    /// </summary>
    public class UITokenShop_ConfirmBuyFrame : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Button đóng khung
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Close;

        /// <summary>
        /// Button mua
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Buy;

        /// <summary>
        /// ItemBox vật phẩm
        /// </summary>
        [SerializeField]
        private UIItemBox UIItemBox_Item;

        /// <summary>
        /// Text tên vật phẩm
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_ItemName;

        /// <summary>
        /// Text đơn giá bán
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Price;

        /// <summary>
        /// Image icon tiền tệ
        /// </summary>
        [SerializeField]
        private SpriteFromAssetBundle UIImage_MoneyIcon;

        /// <summary>
        /// Text tổng giá bán
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_TotalPrice;

        /// <summary>
        /// Image icon tổng số tiền tệ
        /// </summary>
        [SerializeField]
        private SpriteFromAssetBundle UIImage_TotalMoneyIcon;

        /// <summary>
        /// Button trừ
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Sub;

        /// <summary>
        /// Button cộng
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Add;

        /// <summary>
        /// Button số lượng mua
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_BuyCount;

        /// <summary>
        /// Text số lượng mua
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Quantity;

        /// <summary>
        /// Ô đặt phiếu giảm giá
        /// </summary>
        [SerializeField]
        private UIItemBox UIItem_DiscountCoupon;

        /// <summary>
        /// Khung chọn vật phẩm
        /// </summary>
        [SerializeField]
        private UISelectItem UISelectItem;
        #endregion

        #region Properties
        /// <summary>
        /// Sự kiện đóng khung
        /// </summary>
        public Action Close { get; set; }

        /// <summary>
        /// Sự kiện xác nhận mua
        /// </summary>
        public Action<ShopItem, int, int> ConfirmBuy { get; set; }

        /// <summary>
        /// Loại tiền tệ
        /// </summary>
        public MoneyType MoneyType { get; set; }

        /// <summary>
        /// Thông tin vật phẩm
        /// </summary>
        public ShopItem Data { get; set; }
        #endregion

        #region Core MonoBehavour
        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.InitPrefabs();
        }

        /// <summary>
        /// Hàm này gọi khi đối tượng được kích hoạt
        /// </summary>
        private void OnEnable()
        {
            /// Xây lại giao diện
            this.RebuildLayout(this.UIText_Price.transform.parent.GetComponent<RectTransform>());
            this.RebuildLayout(this.UIText_TotalPrice.transform.parent.GetComponent<RectTransform>());
        }
        #endregion

        #region Code UI
        /// <summary>
        /// Khởi tạo ban đầu
        /// </summary>
        private void InitPrefabs()
        {
            this.UIButton_Close.onClick.AddListener(this.ButtonClose_Clicked);
            this.UIButton_Buy.onClick.AddListener(this.ButtonBuy_Clicked);
            this.UIButton_Add.onClick.AddListener(this.ButtonAdd_Clicked);
            this.UIButton_Sub.onClick.AddListener(this.ButtonSub_Clicked);
            this.UIButton_BuyCount.onClick.AddListener(this.ButtonInputBuyCount_Clicked);
            this.UIItem_DiscountCoupon.Click = this.ButtonDiscountCoupon_Clicked;
        }

        /// <summary>
        /// Sự kiện khi Button đóng khung được ấn
        /// </summary>
        private void ButtonClose_Clicked()
        {
            this.Close?.Invoke();
            this.Hide();
        }

        /// <summary>
        /// Sự kiện khi Button nhập số lượng mua được ấn
        /// </summary>
        private void ButtonInputBuyCount_Clicked()
        {
            KTGlobal.ShowInputNumber("Nhập số lượng mua", 1, this.GetMaxCanBuy(), 1, (number) => {
                if (!this.IsCanBuy(number, out int amountCanBuy))
                {
                    KTGlobal.AddNotification("Số tiền mang theo không đủ!");
                    return;
                }

                /// Thiết lập số lượng
                this.UIText_Quantity.text = number.ToString();

                /// Cập nhật hiển thị tổng giá trị
                this.RefreshTotalPrice();
                /// Cập nhật hiển thị trạng thái của Button cộng trừ
                this.RefreshAddAndSubButtonState();
            });
        }

        /// <summary>
        /// Sự kiện khi Button xác nhận mua được ấn
        /// </summary>
        private void ButtonBuy_Clicked()
        {
            /// Nếu không tồn tại vật phẩm
            if (!Loader.Loader.Items.TryGetValue(this.Data.ItemID, out ItemData itemData))
            {
                KTGlobal.AddNotification("Vật phẩm không tồn tại trong hệ thống!");
                this.Hide();
                return;
            }
            /// Số lượng mua
            int count = int.Parse(this.UIText_Quantity.text);
            /// Thực hiện sự kiện mua
            this.ConfirmBuy?.Invoke(this.Data, count, this.UIItem_DiscountCoupon.Data == null ? -1 : this.UIItem_DiscountCoupon.Data.Id);
            /// Đóng khung
            this.ButtonClose_Clicked();
        }

        /// <summary>
        /// Sự kiện khi Button cộng được giữ
        /// </summary>
        private void ButtonAdd_Clicked()
        {
            /// Chuyển về số lượng cộng thêm
            int addCount = 1;
            /// Nếu có thể cộng số lượng tương ứng
            if (this.CanAdd(addCount, out int count))
            {
                this.UIText_Quantity.text = count.ToString();
            }
            /// Cập nhật hiển thị tổng giá trị
            this.RefreshTotalPrice();
            /// Cập nhật hiển thị trạng thái của Button cộng trừ
            this.RefreshAddAndSubButtonState();
        }

        /// <summary>
        /// Sự kiện khi Button trừ được giữ
        /// </summary>
        private void ButtonSub_Clicked()
        {
            /// Chuyển về số lượng trừ đi
            int subCount = 1;
            /// Nếu có thể cộng số lượng tương ứng
            if (this.CanSub(subCount, out int count))
            {
                this.UIText_Quantity.text = count.ToString();
            }
            /// Cập nhật hiển thị tổng giá trị
            this.RefreshTotalPrice();
            /// Cập nhật hiển thị trạng thái của Button cộng trừ
            this.RefreshAddAndSubButtonState();
        }

        /// <summary>
        /// Sự kiện khi Button phiếu giảm giá được ấn
        /// </summary>
        private void ButtonDiscountCoupon_Clicked()
		{
            /// Nếu không có dữ liệu
            if (this.UIItem_DiscountCoupon.Data == null)
			{
                List<GoodsData> coupons = Global.Data.RoleData.GoodsDataList?.Where(x => KTGlobal.IsDiscountCoupon(x.GoodsID)).ToList();
                /// Hiện khung chọn vật phẩm
                this.UISelectItem.Title = "Chọn phiếu giảm giá";
                this.UISelectItem.Items = coupons;
                this.UISelectItem.ItemSelected = (itemGD) => {
                    /// Thiết lập vật phẩm
                    this.UIItem_DiscountCoupon.Data = itemGD;
                    /// Đóng khung chọn
                    this.UISelectItem.Hide();

                    /// Đơn giá
                    int price = this.Data.Price;
                    /// Thông tin phiếu
                    Loader.Loader.Items.TryGetValue(this.UIItem_DiscountCoupon.Data.GoodsID, out ItemData couponItemData);
                    /// Tỷ lệ giảm
                    int discountPercent = couponItemData.ItemValue;
                    /// Đơn giá mới
                    price = (100 - discountPercent) * price / 100;
                    this.UIText_Price.text = string.Format("<color=#ffdf3d>{0}</color>", KTGlobal.GetDisplayMoney(price));

                    /// Thiết lập tổng giá
                    this.RefreshTotalPrice();

                    /// Xây lại giao diện
                    this.RebuildLayout(this.UIText_Price.transform.parent.GetComponent<RectTransform>());
                    this.RebuildLayout(this.UIText_TotalPrice.transform.parent.GetComponent<RectTransform>());
                };
                this.UISelectItem.Show();
            }
			/// Nếu đã tồn tại dữ liệu
			else
			{
                List<KeyValuePair<string, Action>> buttons = new List<KeyValuePair<string, Action>>();
                buttons.Add(new KeyValuePair<string, Action>("Gỡ xuống", () => {
                    /// Xóa vật phẩm
                    this.UIItem_DiscountCoupon.Data = null;
                    /// Đòng khung Tooltip
                    KTGlobal.CloseItemInfo();
                    /// Đóng khung chọn
                    this.UISelectItem.Hide();

                    /// Đơn giá
                    int price = this.Data.Price;
                    this.UIText_Price.text = string.Format("<color=#ffdf3d>{0}</color>", KTGlobal.GetDisplayMoney(price));

                    /// Thiết lập tổng giá
                    this.RefreshTotalPrice();

                    /// Xây lại giao diện
                    this.RebuildLayout(this.UIText_Price.transform.parent.GetComponent<RectTransform>());
                    this.RebuildLayout(this.UIText_TotalPrice.transform.parent.GetComponent<RectTransform>());
                }));
                KTGlobal.ShowItemInfo(this.UIItem_DiscountCoupon.Data, buttons, null);
			}
		}
        #endregion

        #region Private methods
        /// <summary>
        /// Thực thi sự kiện bỏ qua một số Frame
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        private IEnumerator ExecuteSkipFrames(int skip, Action callback)
        {
            for (int i = 1; i <= skip; i++)
            {
                yield return null;
            }
            callback?.Invoke();
        }

        /// <summary>
        /// Xây lại giao diện
        /// </summary>
        /// <param name="layout"></param>
        private void RebuildLayout(RectTransform layout)
        {
            if (!this.gameObject.activeSelf)
            {
                return;
            }

            this.StartCoroutine(this.ExecuteSkipFrames(1, () => {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(layout);
            }));
        }

        /// <summary>
        /// Làm mới hiển thị khung
        /// </summary>
        private void Refresh()
        {
            /// Nếu không có dữ liệu
            if (this.Data == null)
            {
                this.Hide();
                return;
            }

            /// ID vật phẩm
            int itemID = this.Data.ItemID;
            /// Nếu vật phẩm không tồn tại
            if (!Loader.Loader.Items.TryGetValue(itemID, out ItemData itemData))
            {
                KTGlobal.AddNotification("Vật phẩm không tồn tại trong hệ thống!");
                this.Hide();
                return;
            }

            /// Hủy phiếu
            this.UIItem_DiscountCoupon.Data = null;

            /// Đổ dữ liệu vào ItemBox
            GoodsData itemGD = KTGlobal.CreateItemPreview(itemData);
            /// Thời gian sử dụng (phút)
            int usageMinutes = this.Data.Expiry;
            /// Tick
            DateTime dt = DateTime.Now.AddMinutes(usageMinutes);
            itemGD.Endtime = dt.ToString();
            itemGD.Series = this.Data.Series;
            itemGD.Binding = this.MoneyType == MoneyType.Dong ? 0 : 1;
            if (this.Data.LimitType != (int) LimitType.NoLimit && this.Data.LimitType != (int) LimitType.LimitTime)
            {
                itemGD.GCount = Math.Min(1, this.Data.LimitValue);
            }
            else
            {
                itemGD.GCount = 1;
            }
            this.UIItemBox_Item.Data = itemGD;
            this.UIItemBox_Item.Click = () => {
                KTGlobal.ShowItemInfo(itemGD, null, this.Data);
            };
            /// Thiết lặp mặc định mua 1 vật phẩm
            this.UIText_Quantity.text = "1";
            /// Cập nhật thông tin tên vật phẩm
            this.UIText_ItemName.text = itemData.Name;

            /// Nếu bán bằng bang cống
            if (this.Data.TongFund > 0)
            {
                this.UIImage_MoneyIcon.gameObject.SetActive(false);
                this.UIText_Price.text = string.Format("{0} Bang cống", this.Data.TongFund);
            }
            /// Nếu bán bằng vật phẩm
            else if (this.Data.GoodsIndex > 0)
            {
                this.UIImage_MoneyIcon.gameObject.SetActive(false);
                if (Loader.Loader.Items.TryGetValue(this.Data.GoodsIndex, out ItemData _itemData))
                {
                    this.UIText_Price.text = string.Format("{0}", this.Data.GoodsPrice);
                    this.UIImage_MoneyIcon.gameObject.SetActive(true);
                    this.UIImage_MoneyIcon.BundleDir = _itemData.IconBundleDir;
                    this.UIImage_MoneyIcon.AtlasName = _itemData.IconAtlasName;
                    this.UIImage_MoneyIcon.SpriteName = _itemData.Icon;
                    this.UIImage_MoneyIcon.Load();
                }
            }
            /// Nếu bán bằng tiền tệ thường
            else
            {
                /// Hiển thị Icon và số lượng giá cả
                switch (this.MoneyType)
                {
                    case MoneyType.Bac:
                    case MoneyType.BacKhoa:
                    case MoneyType.Dong:
                    case MoneyType.DongKhoa:
                    {
                        /// Nếu có giảm giá
                        if (this.Data.IsDiscount)
                        {
                            int price = this.Data.Price;
                            int oldPrice = this.Data.OldPrice;
                            /// Nếu có phiếu giảm giá
                            if (this.UIItem_DiscountCoupon.Data != null)
                            {
                                /// Thông tin phiếu
                                Loader.Loader.Items.TryGetValue(this.UIItem_DiscountCoupon.Data.GoodsID, out ItemData couponItemData);
                                /// Tỷ lệ giảm
                                int discountPercent = couponItemData.ItemValue;
                                /// Đơn giá mới
                                price = (100 - discountPercent) * price / 100;
                                oldPrice = (100 - discountPercent) * oldPrice / 100;
                            }

                            this.UIText_Price.text = string.Format("<color=#ffdf3d>{0}</color> <s><color=#ff0a0a>{1}</color></s>", KTGlobal.GetDisplayMoney(price), KTGlobal.GetDisplayMoney(oldPrice));
                        }
                        /// Nếu không có giảm giá
                        else
                        {
                            int price = this.Data.Price;
                            /// Nếu có phiếu giảm giá
                            if (this.UIItem_DiscountCoupon.Data != null)
                            {
                                /// Thông tin phiếu
                                Loader.Loader.Items.TryGetValue(this.UIItem_DiscountCoupon.Data.GoodsID, out ItemData couponItemData);
                                /// Tỷ lệ giảm
                                int discountPercent = couponItemData.ItemValue;
                                /// Đơn giá mới
                                price = (100 - discountPercent) * price / 100;
                            }

                            this.UIText_Price.text = string.Format("<color=#ffdf3d>{0}</color>", KTGlobal.GetDisplayMoney(price));
                        }
                        KTGlobal.GetMoneyDisplayImage(this.MoneyType, out string bundleDir, out string atlasName, out string spriteName);
                        this.UIImage_MoneyIcon.gameObject.SetActive(true);
                        this.UIImage_MoneyIcon.BundleDir = bundleDir;
                        this.UIImage_MoneyIcon.AtlasName = atlasName;
                        this.UIImage_MoneyIcon.SpriteName = spriteName;
                        this.UIImage_MoneyIcon.Load();
                        break;
                    }
                    default:
                    {
                        /// Nếu có giảm giá
                        if (this.Data.IsDiscount)
                        {
                            this.UIText_Price.text = string.Format("<color=#ffdf3d>{0} điểm</color> <s><color=#ff0a0a>{1} điểm</color></s> {2}", KTGlobal.GetDisplayNumber(this.Data.Price), KTGlobal.GetDisplayNumber(this.Data.OldPrice), KTGlobal.GetMoneyName(this.MoneyType));
                        }
                        else
                        {
                            this.UIText_Price.text = string.Format("<color=#ffdf3d>{0} điểm</color> {1}", KTGlobal.GetDisplayNumber(this.Data.Price), KTGlobal.GetMoneyName(this.MoneyType));
                        }
                        this.UIImage_MoneyIcon.gameObject.SetActive(false);
                        break;
                    }
                }
            }

            /// Làm mới hiển thị tông giá
            this.RefreshTotalPrice();
            /// Cập nhật hiển thị Button
            this.RefreshButtonBuyState();
            this.RefreshAddAndSubButtonState();

            /// Xây lại giao diện
            this.RebuildLayout(this.UIText_Price.transform.parent.GetComponent<RectTransform>());
            this.RebuildLayout(this.UIText_TotalPrice.transform.parent.GetComponent<RectTransform>());
        }

        /// <summary>
        /// Kiểm tra có thể mua được số lượng tương ứng không
        /// </summary>
        /// <param name="count"></param>
        /// <param name="amountCanBuy"></param>
        /// <returns></returns>
        private bool IsCanBuy(int count, out int amountCanBuy)
        {
            /// Nếu số lượng dưới 0 thì không thể mua được
            if (count <= 0)
            {
                amountCanBuy = 0;
                return false;
            }

            /// Số lượng tối đa có thể mua
            amountCanBuy = this.GetMaxCanBuy();

            /// Nếu số lượng muốn mua <= số lượng có thể mua thì mua được
            if (amountCanBuy >= count)
            {
                return true;
            }
            else
            {
                amountCanBuy = 0;
                return false;
            }
        }

        /// <summary>
        /// Trả về số lượng lượng tối đa có thể mua
        /// </summary>
        /// <param name="nAdd">Lượng muốn thêm vào</param>
        /// <param name="count">Số lượng sau khi thêm</param>
        /// <returns></returns>
        private int GetMaxCanBuy()
        {
            /// Số lượng có thể mua
            int count = 0;
            /// Nếu bán bằng bang cống
            if (this.Data.TongFund > 0)
            {
                return 1000;
            }
            /// Nếu bán bằng vật phẩm
            else if (this.Data.GoodsIndex > 0)
            {
                return 1000;
            }
            else
            {
                /// Căn cứ theo tiền tính ra số lượng tối đa có thể mua
                switch (this.MoneyType)
                {
                    case MoneyType.Bac:
                    {
                        int currentMoney = Global.Data.RoleData.Money;
                        int price = this.Data.Price;
                        count = currentMoney / price;
                        break;
                    }
                    case MoneyType.BacKhoa:
                    {
                        int currentMoney = Global.Data.RoleData.BoundMoney;
                        int price = this.Data.Price;
                        count = currentMoney / price;
                        break;
                    }
                    case MoneyType.Dong:
                    {
                        int currentMoney = Global.Data.RoleData.Token;
                        int price = this.Data.Price;
                        count = currentMoney / price;
                        break;
                    }
                    case MoneyType.DongKhoa:
                    {
                        int currentMoney = Global.Data.RoleData.BoundToken;
                        int price = this.Data.Price;
                        count = currentMoney / price;
                        break;
                    }
                    /// ETC...
                }
                /// Nếu đây là loại giới hạn của hệ thống
                if (this.Data.LimitType != (int) LimitType.NoLimit && this.Data.LimitType != (int) LimitType.LimitTime)
                {
                    count = Math.Min(count, this.Data.LimitValue);
                }

                return count;
            }
        }

        /// <summary>
        /// Kiểm tra có thể cộng thêm số lượng vật phẩm nữa không
        /// </summary>
        /// <param name="nAdd">Lượng muốn thêm vào</param>
        /// <param name="count">Số lượng sau khi thêm</param>
        /// <returns></returns>
        private bool CanAdd(int nAdd, out int count)
        {
            /// Thiết lập số lượng mặc đinh có sau khi cộng
            count = 0;

            /// Nếu giá trị thêm vào âm thì toang
            if (nAdd <= 0)
            {
                return false;
            }

            try
            {
                /// Số lượng hiện có
                int currentCount = int.Parse(this.UIText_Quantity.text);
                /// Số lượng sẽ mua thêm
                count = currentCount + nAdd;
                /// Nếu có thể mua được
                if (this.IsCanBuy(count, out int amountCanBuy))
                {
                    /// Gắn lại số lượng
                    count = Math.Min(count, amountCanBuy);
                    /// Nếu không thể thêm số lượng
                    if (count == currentCount)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Kiểm tra có thể trừ số lượng vật phẩm nữa không
        /// </summary>
        /// <param name="nSub">Lượng muốn trừ đi</param>
        /// <param name="count">Lượng sau khi trừ</param>
        /// <returns></returns>
        private bool CanSub(int nSub, out int count)
        {
            /// Thiết lập số lượng mặc định có sau khi trừ
            count = 0;

            /// Nếu giá trị trừ đi âm thì toang
            if (nSub <= 0)
            {
                return false;
            }

            try
            {
                /// Số lượng hiện có
                int currentCount = int.Parse(this.UIText_Quantity.text);
                /// Số lượng sau khi trừ
                count = currentCount - nSub;
                /// Nếu số lượng sau khi trừ dưới 1, thì thiết lập bằng 1
                count = Math.Max(1, count);
                /// Nếu số lượng hiện có và số lượng sau khi trừ không thay đổi
                if (count == currentCount)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Làm mới tổng giá trị tiền tệ
        /// </summary>
        private void RefreshTotalPrice()
        {
            /// Số lượng mua
            int count = int.Parse(this.UIText_Quantity.text);

            /// Nếu không thể mua
            if (!this.IsCanBuy(count, out int amountCanBuy))
            {
                this.UIText_TotalPrice.text = string.Format("<color=#ff0f0f>Không đủ</color>");
                this.UIImage_TotalMoneyIcon.gameObject.SetActive(false);
            }
            /// Nếu có thể mua
            else
            {
                /// Nếu bán bằng bang cống
                if (this.Data.TongFund > 0)
                {
                    /// Đơn giá
                    int price = this.Data.TongFund;
                    /// Tổng giá
                    int totalPrice = price * count;

                    this.UIImage_TotalMoneyIcon.gameObject.SetActive(false);
                    this.UIText_TotalPrice.text = string.Format("{0} Bang cống", totalPrice);
                }
                /// Nếu bán bằng vật phẩm
                else if (this.Data.GoodsIndex > 0)
                {
                    /// Đơn giá
                    int price = this.Data.GoodsPrice;
                    /// Tổng giá
                    int totalPrice = price * count;

                    this.UIImage_TotalMoneyIcon.gameObject.SetActive(false);
                    if (Loader.Loader.Items.TryGetValue(this.Data.GoodsIndex, out ItemData _itemData))
                    {
                        this.UIText_TotalPrice.text = string.Format("{0}", totalPrice);
                        this.UIImage_TotalMoneyIcon.gameObject.SetActive(true);
                        this.UIImage_TotalMoneyIcon.BundleDir = _itemData.IconBundleDir;
                        this.UIImage_TotalMoneyIcon.AtlasName = _itemData.IconAtlasName;
                        this.UIImage_TotalMoneyIcon.SpriteName = _itemData.Icon;
                        this.UIImage_TotalMoneyIcon.Load();
                    }
                }
                /// Nếu bán bằng tiền tệ thường
                else
                {
                    /// Đơn giá
                    int price = this.Data.Price;
                    /// Nếu có phiếu giảm giá
                    if (this.UIItem_DiscountCoupon.Data != null)
					{
                        /// Thông tin phiếu
                        Loader.Loader.Items.TryGetValue(this.UIItem_DiscountCoupon.Data.GoodsID, out ItemData itemData);
                        /// Tỷ lệ giảm
                        int discountPercent = itemData.ItemValue;
                        /// Đơn giá mới
                        price = (100 - discountPercent) * price / 100;
                    }
                    /// Tổng giá
                    int totalPrice = price * count;

                    /// Hiển thị Icon và tổng giá
                    switch (this.MoneyType)
                    {
                        case MoneyType.Bac:
                        case MoneyType.BacKhoa:
                        case MoneyType.Dong:
                        case MoneyType.DongKhoa:
                        {
                            this.UIText_TotalPrice.text = string.Format("<color=#ffdf3d>{0}</color>", totalPrice);
                            KTGlobal.GetMoneyDisplayImage(this.MoneyType, out string bundleDir, out string atlasName, out string spriteName);
                            this.UIImage_TotalMoneyIcon.gameObject.SetActive(true);
                            this.UIImage_TotalMoneyIcon.BundleDir = bundleDir;
                            this.UIImage_TotalMoneyIcon.AtlasName = atlasName;
                            this.UIImage_TotalMoneyIcon.SpriteName = spriteName;
                            this.UIImage_TotalMoneyIcon.Load();
                            break;
                        }
                        default:
                        {
                            this.UIText_TotalPrice.text = string.Format("<color=#ffdf3d>{0} điểm</color> {1}", KTGlobal.GetDisplayNumber(this.Data.Price), KTGlobal.GetMoneyName(this.MoneyType));
                            this.UIImage_TotalMoneyIcon.gameObject.SetActive(false);
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Làm mới hiển thị Button mua
        /// </summary>
        private void RefreshButtonBuyState()
        {
            /// Số lượng mua
            int count = int.Parse(this.UIText_Quantity.text);

            /// Nếu không thể mua do không đủ tiền
            if (this.IsCanBuy(count, out _))
            {
                this.UIButton_Buy.interactable = true;
            }
            /// Nếu có thể mua
            else
            {
                this.UIButton_Buy.interactable = false;
            }
        }

        /// <summary>
        /// Làm mới hiển thị Button cộng và trừ
        /// </summary>
        private void RefreshAddAndSubButtonState()
        {
            /// Số lượng mua
            int count = int.Parse(this.UIText_Quantity.text);

            /// Nếu có thể mua thêm
            if (this.IsCanBuy(count, out int amountCanBuy))
            {
                this.UIButton_Add.interactable = count < amountCanBuy;
            }
            else
            {
                this.UIButton_Add.interactable = false;
            }

            /// Nếu có thể xóa bớt
            if (count > 1)
            {
                this.UIButton_Sub.interactable = true;
            }
            else
            {
                this.UIButton_Sub.interactable = false;
            }
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Hiện khung
        /// </summary>
        public void Show()
        {
            this.Refresh();
            this.gameObject.SetActive(true);
        }

        /// <summary>
        /// Đóng khung
        /// </summary>
        public void Hide()
        {
            this.Data = null;
            this.gameObject.SetActive(false);
        }
        #endregion
    }
}
