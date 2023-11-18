using FS.GameEngine.Logic;
using FS.VLTK.UI;
using FS.VLTK.UI.Main;
using Server.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FS.VLTK.Factory.UIManager
{
    /// <summary>
    /// Quản lý UI Hint Item
    /// </summary>
    public class UIHintItemManager : TTMonoBehaviour
    {
        #region Singleton - Instance
        /// <summary>
        /// Quản lý UI Hint Item
        /// </summary>
        public static UIHintItemManager Instance { get; private set; }
        #endregion

        #region Define
        /// <summary>
        /// Đối tượng thông tin
        /// </summary>
        private class HintItem
        {
            /// <summary>
            /// Vật phẩm tương ứng
            /// </summary>
            public GoodsData ItemGD { get; set; }

            /// <summary>
            /// Số lượng
            /// </summary>
            public int Quantity { get; set; }
        }
        #endregion

        #region Constants
        /// <summary>
        /// Thời gian Delay mỗi lần thực thi 1 phần tử
        /// </summary>
        private const float HintDelayEach = 0.5f;
        #endregion

        #region Private fields
        /// <summary>
        /// Danh sách các phần tử
        /// </summary>
        private readonly Queue<HintItem> elements = new Queue<HintItem>();

        /// <summary>
        /// Danh sách Hint theo ID vật phẩm
        /// </summary>
        private readonly Dictionary<int, HintItem> hintByItemID = new Dictionary<int, HintItem>();
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            UIHintItemManager.Instance = this;
        }

        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.StartCoroutine(this.DoLogic());
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Hiển thị thông tin vật phẩm mới tương ứng
        /// </summary>
        /// <param name="hintItem"></param>
        private void DoHintItem(HintItem hintItem)
        {
            CanvasManager canvas = Global.MainCanvas.GetComponent<CanvasManager>();
            UIHintItemText uiHintItemText = canvas.LoadUIPrefab<UIHintItemText>("MainGame/UIHintItemText");
            canvas.AddUI(uiHintItemText, true);
            uiHintItemText.Data = hintItem.ItemGD;
            uiHintItemText.ItemQuantity = hintItem.Quantity;
            uiHintItemText.Play();
        }

        /// <summary>
        /// Thực thi Logic
        /// </summary>
        /// <returns></returns>
        private IEnumerator DoLogic()
        {
            while (true)
            {
                /// Nếu tồn tại phần tử cần Hint
                if (this.elements.Count > 0)
                {
                    /// Lấy phần tử tương ứng ra khỏi danh sách
                    HintItem hintItem = this.elements.Dequeue();
                    /// Xóa phần tử khỏi từ điển
                    this.hintByItemID.Remove(hintItem.ItemGD.GoodsID);

                    /// Thực hiện biểu diễn
                    this.DoHintItem(hintItem);
                }
                yield return new WaitForSeconds(UIHintItemManager.HintDelayEach);
            }
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Làm rỗng danh sách
        /// </summary>
        public void Clear()
        {
            this.elements.Clear();
            this.hintByItemID.Clear();
        }

        /// <summary>
        /// Thêm thông báo
        /// </summary>
        /// <param name="itemGD"></param>
        /// <param name="count"></param>
        public void AddHint(GoodsData itemGD, int count)
        {
            /// Nếu đã tồn tại
            if (this.hintByItemID.TryGetValue(itemGD.GoodsID, out HintItem hintItem))
            {
                hintItem.Quantity += count;
            }
            /// Nếu chưa tồn tại
            else
            {
                /// Tạo mới đối tượng
                hintItem = new HintItem()
                {
                    ItemGD = itemGD,
                    Quantity = count,
                };
                /// Thêm vào từ điển
                this.hintByItemID[itemGD.GoodsID] = hintItem;
                /// Thêm vào danh sách chờ
                this.elements.Enqueue(hintItem);
            }
        }
        #endregion
    }
}
