using FS.VLTK.UI.Main.Bag;
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
    /// Tab bán và mua lại
    /// </summary>
    public class UIShop_SellAndBuyBackTab : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Prefab vật phẩm
        /// </summary>
        [SerializeField]
        private UIShop_SellAndBuyBackTab_ItemPrefab UIItem_Prefab;

        /// <summary>
        /// Lưới túi đồ
        /// </summary>
        [SerializeField]
        private UIBag_Grid UIBag_Grid;
        #endregion

        #region Private fields
        /// <summary>
        /// RectTransform danh sách vật phẩm
        /// </summary>
        private RectTransform transformItemList;
        #endregion

        #region Properties
        /// <summary>
        /// Sự kiện mua lại
        /// </summary>
        public Action<GoodsData> BuyBack { get; set; }

        /// <summary>
        /// Sự kiện bán vật phẩm trong túi
        /// </summary>
        public Action<GoodsData> Sell { get; set; }

        /// <summary>
        /// Loại tiền tệ
        /// </summary>
        public MoneyType MoneyType { get; set; }

        /// <summary>
        /// Danh sách vật phẩm đã bán
        /// </summary>
        public List<GoodsData> Items { get; set; }
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
            this.UIBag_Grid.BagItemClicked = this.BagItem_Clicked;
        }

        /// <summary>
        /// Sự kiện khi click vào vật phẩm trong túi đồ
        /// </summary>
        /// <param name="itemGD"></param>
        private void BagItem_Clicked(GoodsData itemGD)
        {
            /// Dữ liệu vật phẩm
            if (!Loader.Loader.Items.TryGetValue(itemGD.GoodsID, out Entities.Config.ItemData itemData))
            {
                return;
            }

            List<KeyValuePair<string, Action>> buttons = new List<KeyValuePair<string, Action>>();
            /// Nếu vật phẩm có thể bán
            if (KTGlobal.IsCanBeSold(itemGD))
            {
                buttons.Add(new KeyValuePair<string, Action>("Bán", () => {
                    this.Sell?.Invoke(itemGD);
                }));
            }
            /// Hiện Tooltip vật phẩm
            KTGlobal.ShowItemInfo(itemGD, buttons);
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
        /// Thêm vật phẩm tương ứng vào danh sách bán
        /// </summary>
        /// <param name="itemGD"></param>
        private void DoAddItem(GoodsData itemGD)
        {
            UIShop_SellAndBuyBackTab_ItemPrefab uiItem = GameObject.Instantiate<UIShop_SellAndBuyBackTab_ItemPrefab>(this.UIItem_Prefab);
            uiItem.gameObject.SetActive(true);
            uiItem.transform.SetParent(this.transformItemList, false);
            uiItem.Data = itemGD;
            uiItem.MoneyType = itemGD.Binding == 1 ? MoneyType.BacKhoa : MoneyType.Bac;
            uiItem.Buy = () => {
                this.BuyBack?.Invoke(itemGD);
            };
        }

        /// <summary>
        /// Tìm UI chứa thông tin vật phẩm có DbID tương ứng
        /// </summary>
        /// <param name="itemDBID"></param>
        /// <returns></returns>
        private UIShop_SellAndBuyBackTab_ItemPrefab FindOldItemNode(int itemDBID)
        {
            foreach (Transform child in this.transformItemList.transform)
            {
                if (child.gameObject != this.UIItem_Prefab.gameObject)
                {
                    UIShop_SellAndBuyBackTab_ItemPrefab uiItem = child.GetComponent<UIShop_SellAndBuyBackTab_ItemPrefab>();
                    if (uiItem != null && uiItem.Data != null && uiItem.Data.Id == itemDBID)
                    {
                        return uiItem;
                    }
                }
            }
            return null;
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
            foreach (GoodsData itemGD in this.Items)
            {
                this.DoAddItem(itemGD);
            }
            /// Xây lại giao diện
            this.RebuildLayout();
        }

        /// <summary>
        /// Thêm vật phẩm vừa bán vào danh sách
        /// </summary>
        /// <param name="itemGD"></param>
        public void AddItem(GoodsData itemGD)
        {
            /// UI chứa vật phẩm cũ
            UIShop_SellAndBuyBackTab_ItemPrefab uiItem = this.FindOldItemNode(itemGD.Id);
            /// Nếu đã tồn tại
            if (uiItem != null)
            {
                return;
            }
            /// Nếu danh sách vật phẩm rỗng
            if (this.Items == null)
            {
                this.Items = new List<GoodsData>();
            }
            /// Thêm vào danh sách cũ
            this.Items.Add(itemGD);
            /// Thêm mới
            this.DoAddItem(itemGD);
            /// Xây lại giao diện
            this.RebuildLayout();
        }

        /// <summary>
        /// Xóa vật phẩm vừa bán khỏi danh sách
        /// </summary>
        /// <param name="itemDbID"></param>
        public void RemoveItem(int itemDbID)
        {
            /// UI chứa vật phẩm cũ
            UIShop_SellAndBuyBackTab_ItemPrefab uiItem = this.FindOldItemNode(itemDbID);
            /// Nếu không tồn tại
            if (uiItem == null)
            {
                return;
            }
            /// Nếu danh sách vật phẩm rỗng
            if (this.Items == null)
            {
                this.Items = new List<GoodsData>();
            }
            /// Thông tin vật phẩm tương ứng trong danh sách
            GoodsData itemGD = this.Items.Where(x => x.Id == itemDbID).FirstOrDefault();
            if (itemGD != null)
            {
                /// Xóa khỏi danh sách cũ
                this.Items.Remove(itemGD);
            }
            /// Xóa vật phẩm tương ứng khỏi danh sách
            GameObject.Destroy(uiItem.gameObject);
            /// Xây lại giao diện
            this.RebuildLayout();
        }
        #endregion
    }
}
