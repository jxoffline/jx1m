using FS.GameEngine.Logic;
using FS.VLTK.Entities.Config;
using FS.VLTK.UI.Main.ItemBox;
using FS.VLTK.Utilities.UnityUI;
using Server.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using static FS.VLTK.Entities.Enum;

namespace FS.VLTK.UI.Main.Shop
{
    /// <summary>
    /// Ô vật phẩm trong Shop mua
    /// </summary>
    public class UIShop_BuyTab_Item : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Ô vật phẩm
        /// </summary>
        [SerializeField]
        private UIItemBox UIItemBox_Item;

        /// <summary>
        /// RectTransform giới hạn
        /// </summary>
        [SerializeField]
        private RectTransform UITransform_Limitation;

        /// <summary>
        /// Text loại giới hạn
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_LimitationType;

        /// <summary>
        /// Thời gian đếm lùi
        /// </summary>
        [SerializeField]
        private UIShop_BuyTab_Item_CountDown UI_Timer;

        /// <summary>
        /// Text tên vật phẩm
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_ItemName;

        /// <summary>
        /// Text giá mua
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Price;

        /// <summary>
        /// Image Icon tiền tệ
        /// </summary>
        [SerializeField]
        private SpriteFromAssetBundle UIImage_MoneyIcon;

        /// <summary>
        /// RectTransform giới hạn số lượng
        /// </summary>

        [SerializeField]
        private RectTransform UITransform_LimitCount;

        /// <summary>
        /// Text giới hạn số lượng có thể mua
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_LimitCount;

        /// <summary>
        /// Button mua
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Buy;
        #endregion

        #region Properties
        /// <summary>
        /// Sự kiện mua
        /// </summary>
        public Action Buy { get; set; }

        /// <summary>
        /// Loại tiền tệ
        /// </summary>
        public MoneyType MoneyType { get; set; }

        /// <summary>
        /// Dữ liệu vật phẩm bán
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
            this.Refresh();
        }
        #endregion

        #region Code UI
        /// <summary>
        /// Khởi tạo ban đầu
        /// </summary>
        private void InitPrefabs()
        {
            this.UIButton_Buy.onClick.AddListener(this.ButtonBuy_Clicked);
        }

        /// <summary>
        /// Sự kiện khi Button mua được ấn
        /// </summary>
        private void ButtonBuy_Clicked()
        {
            this.Buy?.Invoke();
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
            this.StartCoroutine(this.ExecuteSkipFrames(1, () => {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(layout);
            }));
        }

        /// <summary>
        /// Làm mới hiển thị
        /// </summary>
        private void Refresh()
        {
            /// Nếu không có dữ liệu
            if (this.Data == null)
            {
                this.Destroy();
                return;
            }
            /// Nếu vật phẩm không tồn tại
            if (!Loader.Loader.Items.TryGetValue(this.Data.ItemID, out ItemData itemData))
            {
                this.Destroy();
                return;
            }

            /// Đổ dữ liệu
            GoodsData itemGD = KTGlobal.CreateItemPreview(itemData);
            /// Thời gian sử dụng (phút)
            int usageMinutes = this.Data.Expiry;
            /// Tick
            DateTime dt = DateTime.Now.AddMinutes(usageMinutes);
            itemGD.Endtime = dt.ToString();
            itemGD.GCount = 1;
            itemGD.Binding = this.Data.Bind;
            itemGD.Series = this.Data.Series;
            this.UIItemBox_Item.Data = itemGD;
            this.UIItemBox_Item.Click = () => {
                KTGlobal.ShowItemInfo(itemGD, null, this.Data);
            };

            /// Giới hạn
            switch (this.Data.LimitType)
            {
                case (int) LimitType.NoLimit:
                {
                    this.UITransform_Limitation.gameObject.SetActive(false);
                    this.UITransform_LimitCount.gameObject.SetActive(false);
                    this.UI_Timer.Visible = false;
                    break;
                }
                case (int) LimitType.BuyCountPerDay:
                {
                    this.UITransform_Limitation.gameObject.SetActive(true);
                    this.UIText_LimitationType.text = "Ngày";
                    this.UITransform_LimitCount.gameObject.SetActive(true);
                    this.UIText_LimitCount.text = this.Data.LimitValue.ToString();
                    this.UI_Timer.Visible = false;
                    break;
                }
                case (int) LimitType.BuyCountPerWeek:
                {
                    this.UITransform_Limitation.gameObject.SetActive(true);
                    this.UIText_LimitationType.text = "Tuần";
                    this.UITransform_LimitCount.gameObject.SetActive(true);
                    this.UIText_LimitCount.text = this.Data.LimitValue.ToString();
                    this.UI_Timer.Visible = false;
                    break;
                }
                case (int) LimitType.LimitCount:
                {
                    this.UITransform_Limitation.gameObject.SetActive(true);
                    this.UIText_LimitationType.text = "Số lượng";
                    this.UITransform_LimitCount.gameObject.SetActive(true);
                    this.UIText_LimitCount.text = this.Data.LimitValue.ToString();
                    this.UI_Timer.Visible = false;
                    break;
                }
                case (int) LimitType.LimitTime:
                {
                    this.UITransform_Limitation.gameObject.SetActive(true);
                    this.UIText_LimitationType.text = "Thời gian";
                    this.UITransform_LimitCount.gameObject.SetActive(false);
                    this.UI_Timer.TickTime = this.Data.EndTime.Ticks / TimeSpan.TicksPerMillisecond;
                    this.UI_Timer.Visible = true;
                    this.UI_Timer.InvisibleWhenTimeout = false;
                    break;
                }
            }

            /// Tên
            this.UIText_ItemName.text = KTGlobal.GetItemName(itemGD);
            this.UIText_ItemName.color = KTGlobal.GetItemColor(itemGD);

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
                /// Nếu có giảm giá
                if (this.Data.IsDiscount)
                {
                    this.UIText_Price.text = string.Format("<color=#ffdf3d>{0}</color> <s><color=#ff0a0a>{1}</color></s>", KTGlobal.GetDisplayMoney(this.Data.Price), KTGlobal.GetDisplayMoney(this.Data.OldPrice));
                }
                /// Nếu không có giảm giá
                else
                {
                    this.UIText_Price.text = string.Format("<color=#ffdf3d>{0}</color>", KTGlobal.GetDisplayMoney(this.Data.Price));
                }
                KTGlobal.GetMoneyDisplayImage(this.MoneyType, out string bundleDir, out string atlasName, out string spriteName);
                this.UIImage_MoneyIcon.gameObject.SetActive(true);
                this.UIImage_MoneyIcon.BundleDir = bundleDir;
                this.UIImage_MoneyIcon.AtlasName = atlasName;
                this.UIImage_MoneyIcon.SpriteName = spriteName;
                this.UIImage_MoneyIcon.Load();
            }
            

            /// Xây lại giao diện
            this.RebuildLayout(this.UIText_Price.transform.parent.GetComponent<RectTransform>());
        }

        /// <summary>
        /// Xóa đối tượng
        /// </summary>
        private void Destroy()
        {
            GameObject.Destroy(this.gameObject);
        }
        #endregion

        #region Public methods

        #endregion
    }
}
