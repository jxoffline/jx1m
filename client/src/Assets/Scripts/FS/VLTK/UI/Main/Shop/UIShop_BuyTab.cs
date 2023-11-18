using FS.VLTK.Entities.Config;
using FS.VLTK.UI.Main.Money;
using Server.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static FS.VLTK.Entities.Enum;

namespace FS.VLTK.UI.Main.Shop
{
    /// <summary>
    /// Tab mua
    /// </summary>
    public class UIShop_BuyTab : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Prefab vật phẩm
        /// </summary>
        [SerializeField]
        private UIShop_BuyTab_Item UIItem_Prefab;

        /// <summary>
        /// Ô tiền tệ hiện có
        /// </summary>
        [SerializeField]
        private UIMoneyBox UIMoneyBox;

        /// <summary>
        /// Khung xác nhận mua
        /// </summary>
        [SerializeField]
        private UIShop_BuyTab_ConfirmBuyFrame UIConfirmBuyFrame;
        #endregion

        #region Private fields
        /// <summary>
        /// RectTransform danh sách vật phẩm
        /// </summary>
        private RectTransform transformItemList;
        #endregion

        #region Properties
        /// <summary>
        /// Sự kiện mua vật phẩm
        /// </summary>
        public Action<ShopItem, int> Buy { get; set; }

        /// <summary>
        /// Loại tiền tệ
        /// </summary>
        public MoneyType MoneyType { get; set; }

        /// <summary>
        /// Danh sách vật phẩm được bán
        /// </summary>
        public List<ShopItem> Items { get; set; }
        #endregion

        #region Core MonoBehavour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            this.transformItemList = this.UIItem_Prefab.transform.parent.GetComponent<RectTransform>();
        }

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
            this.UIMoneyBox.Type = this.MoneyType;
            this.UIMoneyBox.Refresh();
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
        private void RebuildLayout()
        {
            this.StartCoroutine(this.ExecuteSkipFrames(1, () => {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.transformItemList);
            }));
        }

        /// <summary>
        /// Làm rỗng danh sách
        /// </summary>
        private void ClearList()
        {
            foreach (Transform child in this.transformItemList.transform)
            {
                if (child.gameObject != this.UIItem_Prefab.gameObject)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }
            this.RebuildLayout();
        }

        /// <summary>
        /// Mở bảng xác nhận mua
        /// </summary>
        /// <param name="sellItem"></param>
        private void OpenConfirmBuyFrame(ShopItem sellItem)
        {
            this.UIConfirmBuyFrame.Data = sellItem;
            this.UIConfirmBuyFrame.MoneyType = this.MoneyType;
            this.UIConfirmBuyFrame.ConfirmBuy = this.Buy;
            this.UIConfirmBuyFrame.Show();
        }

        /// <summary>
        /// Thêm vật phẩm tương ứng vào danh sách bán
        /// </summary>
        /// <param name="sellItem"></param>
        private void AddItem(ShopItem sellItem)
        {
            UIShop_BuyTab_Item uiItem = GameObject.Instantiate<UIShop_BuyTab_Item>(this.UIItem_Prefab);
            uiItem.gameObject.SetActive(true);
            uiItem.transform.SetParent(this.transformItemList, false);
            uiItem.Data = sellItem;
            uiItem.MoneyType = this.MoneyType;
            uiItem.Buy = () => {
                this.OpenConfirmBuyFrame(sellItem);
            };
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Làm mới danh sách vật phẩm được bán
        /// </summary>
        public void Refresh()
        {
            /// Làm rỗng danh sách
            this.ClearList();
            /// Nếu danh sách rỗng
            if (this.Items == null)
            {
                return;
            }
            /// Duyệt danh sách vật phẩm trong cửa hàng và tạo vật phẩm tương ứng lên khung
            foreach (ShopItem sellItem in this.Items)
            {
                this.AddItem(sellItem);
            }
            /// Xây lại giao diện
            this.RebuildLayout();
        }
        #endregion
    }
}
