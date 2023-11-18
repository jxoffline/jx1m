using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using FS.VLTK.UI.Main.ItemBox;
using System.Collections;
using Server.Data;
using FS.GameEngine.Logic;

namespace FS.VLTK.UI.Main.Exchange
{
    /// <summary>
    /// Khung giao dịch
    /// </summary>
    public class UIExchange_ExchangeBagGrid : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Prefab ô vật phẩm
        /// </summary>
        [SerializeField]
        private UIItemBox UI_ItemPrefab;
        #endregion

        #region Constants
        /// <summary>
        /// Kích thước túi
        /// </summary>
        private const int BagCapacity = 9;
        #endregion

        #region Private fields
        /// <summary>
        /// RectTransform lưới đựng
        /// </summary>
        private RectTransform gridTransform;
        #endregion

        #region Properties
        /// <summary>
        /// Sự kiện ấn vào vật phẩm trong danh sách
        /// </summary>
        public Action<GoodsData> ItemClick { get; set; }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            this.gridTransform = this.GetComponent<RectTransform>();
        }

        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.InitPrefabs();
            this.ClearBag();
            this.InitItemBoxes();
        }
        #endregion

        #region Code UI
        /// <summary>
        /// Khởi tạo ban đầu
        /// </summary>
        private void InitPrefabs()
        {

        }

        /// <summary>
        /// Sự kiện ô vật phẩm được ấn
        /// </summary>
        /// <param name="itemBox"></param>
        private void ButtonItem_Clicked(UIItemBox itemBox)
        {
            this.ItemClick?.Invoke(itemBox.Data);
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Thực thi bỏ qua 1 số frame
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
        /// Xây lại giao diện
        /// </summary>
        private void RebuildLayout()
        {
            this.StartCoroutine(this.ExecuteSkipFrames(1, () => {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.gridTransform);
            }));
        }

        /// <summary>
        /// Làm rỗng danh sách
        /// </summary>
        private void ClearBag()
        {
            foreach (Transform child in this.gridTransform.transform)
            {
                if (child.gameObject != this.UI_ItemPrefab.gameObject)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }
            this.RebuildLayout();
        }

        /// <summary>
        /// Khởi tạo danh sách vật phẩm
        /// </summary>
        private void InitItemBoxes()
        {
            /// Sinh số ô tương ứng
            for (int i = 1; i <= UIExchange_ExchangeBagGrid.BagCapacity; i++)
            {
                UIItemBox uiItemBox = GameObject.Instantiate<UIItemBox>(this.UI_ItemPrefab);
                uiItemBox.transform.SetParent(this.gridTransform, false);
                uiItemBox.gameObject.SetActive(true);

                uiItemBox.Data = null;
                uiItemBox.Click = () => {
                    this.ButtonItem_Clicked(uiItemBox);
                };
            }
            this.RebuildLayout();
        }

        /// <summary>
        /// Trả về vị trí trống chưa có vật phẩm
        /// </summary>
        /// <returns></returns>
        private UIItemBox FindEmptySlot()
        {
            foreach (Transform child in this.gridTransform.transform)
            {
                if (child.gameObject != this.UI_ItemPrefab.gameObject)
                {
                    UIItemBox uiItemBox = child.GetComponent<UIItemBox>();
                    /// Nếu vị trí trống
                    if (uiItemBox.Data == null)
                    {
                        return uiItemBox;
                    }
                }
            }
            /// Không tìm thấy
            return null;
        }

        /// <summary>
        /// Trả về vị trí chứa vật phẩm tương ứng trong túi
        /// </summary>
        /// <param name="itemGD"></param>
        /// <returns></returns>
        private UIItemBox FindItemSlot(GoodsData itemGD)
        {
            foreach (Transform child in this.gridTransform.transform)
            {
                if (child.gameObject != this.UI_ItemPrefab.gameObject)
                {
                    UIItemBox uiItemBox = child.GetComponent<UIItemBox>();
                    /// Nếu tìm thấy
                    if (uiItemBox.Data == itemGD)
                    {
                        return uiItemBox;
                    }
                }
            }
            /// Không tìm thấy
            return null;
        }

        /// <summary>
        /// Làm rỗng túi
        /// </summary>
        private void EmptyBag()
        {
            foreach (Transform child in this.gridTransform.transform)
            {
                if (child.gameObject != this.UI_ItemPrefab.gameObject)
                {
                    UIItemBox uiItemBox = child.GetComponent<UIItemBox>();
                    /// Làm rỗng dữ liệu
                    uiItemBox.Data = null;
                }
            }
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Thêm vật phẩm vào danh sách
        /// </summary>
        /// <param name="itemGD"></param>
        public void AddItem(GoodsData itemGD)
        {
            /// Nếu không có vật phẩm
            if (itemGD == null)
            {
                return;
            }

            /// Vị trí trống trong túi
            UIItemBox uiItemBox = this.FindEmptySlot();
            /// Nếu không tìm thấy tức túi đã đầy
            if (uiItemBox == null)
            {
                KTGlobal.AddNotification("Danh sách vật phẩm giao dịch đã đầy!");
                return;
            }

            /// Thêm vật phẩm vào ô tương ứng
            uiItemBox.Data = itemGD;
        }

        /// <summary>
        /// Xóa vật phẩm khỏi danh sách
        /// </summary>
        /// <param name="itemGD"></param>
        public void RemoveItem(GoodsData itemGD)
        {
            /// Nếu không có vật phẩm
            if (itemGD == null)
            {
                return;
            }

            /// Vị trí tương ứng trong túi
            UIItemBox uiItemBox = this.FindItemSlot(itemGD);
            /// Nếu không tìm thấy vật phẩm tương ứng
            if (uiItemBox == null)
            {
                return;
            }

            /// Xóa vật phẩm khỏi ô tương ứng
            uiItemBox.Data = null;
        }

        /// <summary>
        /// Làm rỗng túi
        /// </summary>
        public void Clear()
        {
            this.EmptyBag();
        }
        #endregion
    }
}
