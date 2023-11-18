using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using FS.VLTK.UI.Main.ItemBox;
using Server.Data;
using FS.GameEngine.Logic;
using System.Collections;

namespace FS.VLTK.UI.Main.PlayerShop
{
    /// <summary>
    /// Vật phẩm trong khung bán hàng của người chơi
    /// </summary>
    public class UIPlayerShop_Sell_Item : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Ô vật phẩm
        /// </summary>
        [SerializeField]
        private UIItemBox UIItemBox;

        /// <summary>
        /// Text tên vật phẩm
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_ItemName;

        /// <summary>
        /// Text giá vật phẩm
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_ItemPrice;

        /// <summary>
        /// Button gỡ xuống
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Remove;
        #endregion

        #region Private fields

        #endregion

        #region Properties
        /// <summary>
        /// Sự kiện Click vào vật phẩm
        /// </summary>
        public Action ItemClick { get; set; }

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
                this.UIText_ItemName.text = KTGlobal.GetItemName(value);
            }
        }

        private int _Price;
        /// <summary>
        /// Giá bán
        /// </summary>
        public int Price
        {
            get
            {
                return this._Price;
            }
            set
            {
                this._Price = value;

                this.UIText_ItemPrice.text = KTGlobal.GetDisplayMoney(value);

                this.RebuildLayout(this.UIText_ItemPrice.transform.parent.GetComponent<RectTransform>());
            }
        }

        /// <summary>
        /// Sự kiện gỡ vật phẩm xuống
        /// </summary>
        public Action Remove { get; set; }

        /// <summary>
        /// Kích hoạt thu hồi không
        /// </summary>
        public bool EnableRemove
        {
            get
            {
                return this.UIButton_Remove.interactable;
            }
            set
            {
                this.UIButton_Remove.interactable = value;
            }
        }
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
            this.UIItemBox.Click = this.ButtonItem_Clicked;
            this.UIButton_Remove.onClick.AddListener(this.ButtonRemoveItem_Clicked);
        }

        /// <summary>
        /// Sự kiện khi vật phẩm được ấn
        /// </summary>
        private void ButtonItem_Clicked()
        {
            this.ItemClick?.Invoke();
        }

        /// <summary>
        /// Sự kiện khi Button thu hồi vật phẩm được ấn
        /// </summary>
        private void ButtonRemoveItem_Clicked()
        {
            this.Remove?.Invoke();
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
    }
}
