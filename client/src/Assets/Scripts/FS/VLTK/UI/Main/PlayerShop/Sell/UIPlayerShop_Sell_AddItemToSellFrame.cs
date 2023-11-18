using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using Server.Data;
using FS.VLTK.UI.Main.ItemBox;
using FS.GameEngine.Logic;
using FS.VLTK.Entities.Config;
using System.Collections;

namespace FS.VLTK.UI.Main.PlayerShop
{
    /// <summary>
    /// Khung thêm vật phẩm vào danh sách bán trong cửa hàng
    /// </summary>
    public class UIPlayerShop_Sell_AddItemToSellFrame : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Button đóng khung
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Close;

        /// <summary>
        /// ItemBox
        /// </summary>
        [SerializeField]
        private UIItemBox UIItemBox;

        /// <summary>
        /// Text tên vật phẩm
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_ItemName;

        /// <summary>
        /// Text số lượng
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Quantity;

        /// <summary>
        /// Button thiết lập giá bán
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_SetPrice;

        /// <summary>
        /// Text giá bán
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Price;

        /// <summary>
        /// Text thuế
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Tax;

        /// <summary>
        /// Text tiền còn lại
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_TrueMoney;

        /// <summary>
        /// Button bán
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Sell;
        #endregion

        #region Constants
        /// <summary>
        /// Giá bán tối đa
        /// </summary>
        private const int MaxMoney = 10000000;
        #endregion

        #region Private fields

        #endregion

        #region Properties
        /// <summary>
        /// Sự kiện xác nhận bán
        /// </summary>
        public Action Sell { get; set; }

        /// <summary>
        /// Thông tin vật phẩm
        /// </summary>
        public GoodsData Data
        {
            get
            {
                return this.UIItemBox.Data;
            }
            set
            {
                this.UIItemBox.Data = value;
                this.UIItemBox.SetShowQuantity(false);

                this.UIText_ItemName.text = KTGlobal.GetItemName(value);
                this.UIText_ItemName.color = KTGlobal.GetItemColor(value);

                this.UIText_Quantity.text = value.GCount.ToString();

                this.Price = 0;

                this.UIText_Price.text = KTGlobal.GetDisplayMoney(this.Price);
                this.UIText_Tax.text = KTGlobal.GetDisplayMoney(this.Price * 10 / 100);
                this.UIText_TrueMoney.text = KTGlobal.GetDisplayMoney(this.Price * 90 / 100);
            }
        }

        /// <summary>
        /// Giá bán
        /// </summary>
        public int Price { get; private set; }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.InitPrefabs();

            this.RebuildLayout(this.UIText_Price.transform.parent.GetComponent<RectTransform>());
            this.RebuildLayout(this.UIText_Tax.transform.parent.GetComponent<RectTransform>());
            this.RebuildLayout(this.UIText_TrueMoney.transform.parent.GetComponent<RectTransform>());
        }
        #endregion

        #region Code UI
        /// <summary>
        /// Khởi tạo ban đầu
        /// </summary>
        private void InitPrefabs()
        {
            this.UIButton_Close.onClick.AddListener(this.ButtonClose_Clicked);
            this.UIButton_Sell.onClick.AddListener(this.ButtonSell_Clicked);
            this.UIButton_SetPrice.onClick.AddListener(this.ButtonSetItemPrice_Clicked);
        }

        /// <summary>
        /// Sự kiện khi Button đóng khung được ấn
        /// </summary>
        private void ButtonClose_Clicked()
        {
            this.Hide();
        }

        /// <summary>
        /// Sự kiện xác nhận bán vật phẩm
        /// </summary>
        private void ButtonSell_Clicked()
        {
            this.Sell?.Invoke();
        }

        /// <summary>
        /// Sự kiện khi Button thiết lập giá bán được ấn
        /// </summary>
        private void ButtonSetItemPrice_Clicked()
        {
            KTGlobal.ShowInputNumber("Nhập giá bán vật phẩm.", 1, UIPlayerShop_Sell_AddItemToSellFrame.MaxMoney, 1, (value) => {
                this.Price = value;

                this.UIText_Price.text = KTGlobal.GetDisplayMoney(this.Price);
                this.UIText_Tax.text = KTGlobal.GetDisplayMoney(this.Price * 10 / 100);
                this.UIText_TrueMoney.text = KTGlobal.GetDisplayMoney(this.Price * 90 / 100);

                this.RebuildLayout(this.UIText_Price.transform.parent.GetComponent<RectTransform>());
                this.RebuildLayout(this.UIText_Tax.transform.parent.GetComponent<RectTransform>());
                this.RebuildLayout(this.UIText_TrueMoney.transform.parent.GetComponent<RectTransform>());

                this.UIButton_Sell.interactable = true;
            });
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
        /// Xây lại giao diện tương ứng
        /// </summary>
        /// <param name="rectTransform"></param>
        private void RebuildLayout(RectTransform rectTransform)
        {
            this.StartCoroutine(this.ExecuteSkipFrames(1, () => {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
            }));
        }
        #endregion

        #region Public fields
        /// <summary>
        /// Hiển thị khung
        /// </summary>
        public void Show()
        {
            this.gameObject.SetActive(true);
            this.UIButton_Sell.interactable = false;
        }

        /// <summary>
        /// Đóng khung
        /// </summary>
        public void Hide()
        {
            this.gameObject.SetActive(false);
        }
        #endregion
    }
}
