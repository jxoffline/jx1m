using FS.VLTK.UI.Main.ItemBox;
using FS.VLTK.Utilities.UnityUI;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FS.VLTK.UI.Main
{
    /// <summary>
    /// Ô Slot vật phẩm, có thể quay để chọn ra vật phẩm tại vị trí tương ứng
    /// </summary>
    [RequireComponent(typeof(UISlotSimulation))]
    public class UIItemSlotBox : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Prefab ô vật phẩm
        /// </summary>
        [SerializeField]
        private UIItemBox UIItem_Prefab;
        #endregion

        #region Private fields
        /// <summary>
        /// Khung mô phỏng ItemSlot
        /// </summary>
        private UISlotSimulation uiSlotSimulation;

        /// <summary>
        /// RectTransform đối tượng
        /// </summary>
        private RectTransform rectTransform = null;
        #endregion

        #region Properties
        /// <summary>
        /// Sự kiện Click
        /// </summary>
        public Action Click { get; set; }

        /// <summary>
        /// Danh sách vật phẩm
        /// </summary>
        public List<GoodsData> Items { get; set; }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            this.uiSlotSimulation = this.GetComponent<UISlotSimulation>();
            this.rectTransform = this.GetComponent<RectTransform>();
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
            this.uiSlotSimulation.Click = this.ButtonSlot_Clicked;
        }

        /// <summary>
        /// Sự kiện khi Button SlotItem được ấn
        /// </summary>
        private void ButtonSlot_Clicked()
        {
            /// Nếu có sự kiện Click
            if (this.Click != null)
            {
                this.Click?.Invoke();
            }
            else
            {
                /// Hiện khung danh sách vật phẩm
                KTGlobal.ShowItemListBox("Danh sách vật phẩm ngẫu nhiên nhận được.", this.Items);
            }
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Làm mới danh sách
        /// </summary>
        private void Refresh()
        {
            /// Nếu danh sách vật phẩm rỗng
            if (this.Items == null)
            {
                return;
            }

            /// Danh sách ô vật phẩm
            List<RectTransform> itemsList = new List<RectTransform>();
            /// Duyệt danh sách vật phẩm và tạo mới
            foreach (GoodsData itemGD in this.Items)
            {
                UIItemBox uiItemBox = GameObject.Instantiate<UIItemBox>(this.UIItem_Prefab);
                uiItemBox.gameObject.SetActive(true);
                RectTransform rectTrans = uiItemBox.GetComponent<RectTransform>();
                rectTrans.sizeDelta = this.rectTransform.sizeDelta;
                uiItemBox.Data = itemGD;
                uiItemBox.Click = null;
                itemsList.Add(rectTrans);
            }
            this.uiSlotSimulation.Items = itemsList.ToArray();
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Thực thi hiệu ứng
        /// </summary>
        /// <param name="stopAt"></param>
        /// <param name="rearrangeItems"></param>
        public void Play(GoodsData stopAt, bool rearrangeItems = false)
        {
            /// Nếu không có vị trí dừng
            if (stopAt == null)
            {
                KTDebug.LogError("Must declare item inside list to stop");
                return;
            }

            /// Vị trí dừng
            int stopIndex = this.uiSlotSimulation.FindNodeIndex((rectTransform) => {
                UIItemBox uiItemBox = rectTransform.GetComponent<UIItemBox>();
                if (uiItemBox != null)
                {
                    return uiItemBox.Data == stopAt;
                }
                return false;
            });

            /// Nếu không tìm thấy vị trí tương ứng
            if (stopIndex == -1)
            {
                KTDebug.LogError("Can not find item inside slot => " + stopAt.GoodsID);
                return;
            }

            /// Đánh dấu vị trí dừng
            this.uiSlotSimulation.StopIndex = stopIndex;
            /// Nếu cần xếp lại vị trí ban đầu
            if (rearrangeItems)
            {
                this.uiSlotSimulation.RearrangeItems();
            }
            /// Thực thi
            this.uiSlotSimulation.Play();
        }

        /// <summary>
        /// Ngừng thực thi hiệu ứng
        /// </summary>
        public void Stop()
        {
            /// Ngừng thực thi
            this.uiSlotSimulation.Stop();
        }
        #endregion
    }
}
