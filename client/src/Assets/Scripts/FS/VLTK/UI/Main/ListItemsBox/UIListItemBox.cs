using FS.VLTK.UI.Main.ItemBox;
using Server.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

namespace FS.VLTK.UI.Main.ItemSlotBox
{
    /// <summary>
    /// Khung danh sách vật phẩm
    /// </summary>
    public class UIListItemBox : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Button đóng khung
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Close;

        /// <summary>
        /// Text mô tả
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Description;

        /// <summary>
        /// Prefab ô vật phẩm
        /// </summary>
        [SerializeField]
        private UIItemBox UIItem_Prefab;
        #endregion

        #region Private fields
        /// <summary>
        /// RectTransform danh sách vật phẩm
        /// </summary>
        private RectTransform transformItemsList = null;
        #endregion

        #region Properties
        /// <summary>
        /// Sự kiện đóng khung
        /// </summary>
        public Action Close { get; set; }

        /// <summary>
        /// Sự kiện khi vật phẩm được ấn
        /// </summary>
        public Action<GoodsData> ItemClick { get; set; }

        /// <summary>
        /// Danh sách vật phẩm
        /// </summary>
        public List<GoodsData> Items { get; set; }

        /// <summary>
        /// Mô tả
        /// </summary>
        public string Description
        {
            get
            {
                return this.UIText_Description.text;
            }
            set
            {
                this.UIText_Description.text = value;
            }
        }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            this.transformItemsList = this.UIItem_Prefab.transform.parent.GetComponent<RectTransform>();
        }

        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.InitPrefabs();
            this.Refresh();
        }

        /// <summary>
        /// Hàm này gọi khi đối tượng được kích hoạt
        /// </summary>
        private void OnEnable()
        {

        }
        #endregion

        #region Code UI
        /// <summary>
        /// Khởi tạo ban đầu
        /// </summary>
        private void InitPrefabs()
        {
            this.UIButton_Close.onClick.AddListener(this.ButtonClose_Clicked);
        }

        /// <summary>
        /// Sự kiện khi Button đóng khung được ấn
        /// </summary>
        private void ButtonClose_Clicked()
        {
            this.Close?.Invoke();
            GameObject.Destroy(this.gameObject);
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Thực thi sự kiện bỏ qua một số Frame
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="work"></param>
        /// <returns></returns>
        private IEnumerator ExecuteSkipFrames(int skip, Action work)
        {
            for (int i = 1; i < skip; i++)
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
            /// Nếu không kích hoạt đối tượng
            if (!this.gameObject.activeSelf)
            {
                return;
            }
            /// Thực thi xây lại giao diện ở Frame tiếp theo
            this.StartCoroutine(this.ExecuteSkipFrames(1, () => {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.transformItemsList);
            }));
        }

        /// <summary>
        /// Làm rỗng danh sách ô vật phẩm
        /// </summary>
        private void ClearItems()
        {
            foreach (Transform child in this.transformItemsList.transform)
            {
                if (child.gameObject != this.UIItem_Prefab.gameObject)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }
        }

        /// <summary>
        /// Thêm vật phẩm vào danh sách
        /// </summary>
        /// <param name="itemGD"></param>
        private void AddItem(GoodsData itemGD)
        {
            UIItemBox uiItemBox = GameObject.Instantiate<UIItemBox>(this.UIItem_Prefab);
            uiItemBox.transform.SetParent(this.transformItemsList, false);
            uiItemBox.gameObject.SetActive(true);
            uiItemBox.Data = itemGD;
            if (this.ItemClick != null)
            {
                uiItemBox.Click = () => {
                    this.ItemClick?.Invoke(itemGD);
                };
            }
        }

        /// <summary>
        /// Làm mới giao diện
        /// </summary>
        private void Refresh()
        {
            /// Làm rỗng danh sách
            this.ClearItems();

            /// Duyệt danh sách vật phẩm
            foreach (GoodsData itemGD in this.Items)
            {
                /// Thêm vật phẩm tương ứng vào danh sách
                this.AddItem(itemGD);
            }

            /// Xây lại giao diện
            this.RebuildLayout();
        }
        #endregion
    }
}
