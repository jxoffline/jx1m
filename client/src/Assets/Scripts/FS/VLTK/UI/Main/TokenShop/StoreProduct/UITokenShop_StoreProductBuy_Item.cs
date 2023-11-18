using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using FS.VLTK.Utilities.UnityUI;
using Server.Data;


namespace FS.VLTK.UI.Main.TokenShop.StoreProduct
{
    /// <summary>
    /// Thông tin gói hàng được bán trên Store
    /// </summary>
    public class UITokenShop_StoreProductBuy_Item : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Button
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton;

        /// <summary>
        /// Mark tiến cử
        /// </summary>
        [SerializeField]
        private RectTransform RecommendMark;

        /// <summary>
        /// Tên gói hàng
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_ProductName;

        /// <summary>
        /// Icon gói hàng
        /// </summary>
        [SerializeField]
        private SpriteFromAssetBundle UIImage_ProductIcon;

        /// <summary>
        /// Hint
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Hint;

        /// <summary>
        /// Text giá gói hàng
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Price;
        #endregion

        #region Properties
        private TokenShopStoreProduct _Data;
        /// <summary>
        /// Dữ liệu gói hàng
        /// </summary>
        public TokenShopStoreProduct Data
        {
            get
            {
                return this._Data;
            }
            set
            {
                this._Data = value;
                this.UIText_ProductName.text = value.Name;
                this.UIText_Hint.text = value.Hint;
                this.UIImage_ProductIcon.SpriteName = value.Icon;
                this.UIImage_ProductIcon.Load();
                //this.UIText_Price.text = IAPManager.Instance.GetProductPriceOnStore(value.ID);
                this.UIText_Price.text = string.Format("{0} đ", KTGlobal.GetDisplayNumber(value.Price));
                this.RecommendMark.gameObject.SetActive(value.Recommend);
            }
        }

        /// <summary>
        /// Sự kiện Click
        /// </summary>
        public Action Click { get; set; }
        #endregion

        #region Core MonoBehaviour
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
            this.UIButton.onClick.AddListener(this.Button_Clicked);
        }

        /// <summary>
        /// Sự kiện khi Button được ấn
        /// </summary>
        private void Button_Clicked()
        {
            this.Click?.Invoke();
        }
        #endregion
    }
}