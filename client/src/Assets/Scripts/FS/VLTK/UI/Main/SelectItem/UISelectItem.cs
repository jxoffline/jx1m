using FS.VLTK.Entities.Config;
using FS.VLTK.UI.Main.ItemBox;
using FS.VLTK.UI.Main.SelectItem;
using Server.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace FS.VLTK.UI.Main.MainUI.RadarMap
{
    /// <summary>
    /// Khung chọn vật phẩm trong danh sách
    /// </summary>
    public class UISelectItem : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Prefab vật phẩm tương ứng
        /// </summary>
        [SerializeField]
        private UISelectItem_ButtonItem UIButton_ItemPrefab;

        /// <summary>
        /// Text tiêu đề khung
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Title;

        /// <summary>
        /// Button đóng khung
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Close;
        #endregion

        #region Private fields
        /// <summary>
        /// RectTransform danh sách vật phẩm
        /// </summary>
        private RectTransform uiTransform_ListItems;
        #endregion

        #region Properties
        /// <summary>
        /// Sự kiện khi vật phẩm được ấn chọn
        /// </summary>
        public Action<GoodsData> ItemSelected { get; set; }

        /// <summary>
        /// Sự kiện đóng khung
        /// </summary>
        public Action Close { get; set; }

        /// <summary>
        /// Danh sách vật phẩm tương ứng
        /// </summary>
        public List<GoodsData> Items { get; set; }

        /// <summary>
        /// Tiêu đề khung
        /// </summary>
        public string Title
        {
            get
            {
                return this.UIText_Title.text;
            }
            set
            {
                this.UIText_Title.text = value;
            }
        }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            this.uiTransform_ListItems = this.UIButton_ItemPrefab.transform.parent.gameObject.GetComponent<RectTransform>();
        }

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
            this.UIButton_Close.onClick.AddListener(this.ButtonClose_Clicked);
        }

        /// <summary>
        /// Sự kiện khi Button đóng khung được ấn
        /// </summary>
        private void ButtonClose_Clicked()
        {
            this.Hide();
            this.Close?.Invoke();
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Thực thi sự kiện bỏ qua 1 số frame
        /// </summary>
        /// <param name="frame"></param>
        /// <param name="callBack"></param>
        /// <returns></returns>
        private IEnumerator ExecuteSkipFrames(int frame, Action callBack)
        {
            for (int i = 1; i <= frame; i++)
            {
                yield return null;
            }

            callBack?.Invoke();
        }

        /// <summary>
        /// Xây lại giao diện
        /// </summary>
        private void RebuildLayout()
        {
            this.StartCoroutine(this.ExecuteSkipFrames(1, () => {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.uiTransform_ListItems);
            }));
        }

        /// <summary>
        /// Làm rỗng danh sách
        /// </summary>
        private void ClearList()
        {
            foreach (Transform child in this.uiTransform_ListItems.transform)
            {
                if (child.gameObject != this.UIButton_ItemPrefab.gameObject)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }
            this.RebuildLayout();
        }

        /// <summary>
        /// Thêm vật phẩm vào danh sách
        /// </summary>
        /// <param name="itemGD"></param>
        private void AddItem(GoodsData itemGD)
        {
            /// Nếu vật phẩm không tồn tại
            if (!Loader.Loader.Items.TryGetValue(itemGD.GoodsID, out ItemData itemData))
            {
                return;
            }

            UISelectItem_ButtonItem uiButton = GameObject.Instantiate<UISelectItem_ButtonItem>(this.UIButton_ItemPrefab);
            uiButton.transform.SetParent(this.uiTransform_ListItems, false);
            uiButton.gameObject.SetActive(true);
            itemGD.GCount = 1;
            uiButton.Data = itemGD;
            uiButton.Click = () => {
                this.Hide();
                this.ItemSelected(itemGD);
            };
        }

        /// <summary>
        /// Làm mới hiển thị
        /// </summary>
        private void Refresh()
        {
            this.ClearList();
            /// Nếu không có danh sách vật phẩm
            if (this.Items == null)
            {
                return;
            }

            /// Duyệt danh sách và thêm vật phẩm vào
            foreach (GoodsData itemGD in this.Items)
            {
                this.AddItem(itemGD);
            }

            /// Xây lại giao diện
            this.RebuildLayout();
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Ẩn khung
        /// </summary>
        public void Hide()
        {
            this.gameObject.SetActive(false);
        }

        /// <summary>
        /// Hiện khung
        /// </summary>
        public void Show()
        {
            this.gameObject.SetActive(true);
            this.Refresh();
        }
        #endregion
    }
}
