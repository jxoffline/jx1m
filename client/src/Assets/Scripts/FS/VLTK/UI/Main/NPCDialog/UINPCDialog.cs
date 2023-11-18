using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using FS.VLTK.UI.Main.NPCDialog;
using System;
using FS.GameEngine.Network;
using FS.GameEngine.Logic;
using Server.Data;
using System.Xml.Linq;
using FS.GameFramework.Logic;
using Server.Tools;
using FS.VLTK.Entities.Config;

namespace FS.VLTK.UI.Main
{
    /// <summary>
    /// Khung thoại NPC
    /// </summary>
    public class UINPCDialog : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Text tiêu đề khung
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Title;

        /// <summary>
        /// Text nội dung khung
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Content;

        /// <summary>
        /// Button đồng ý
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Accept;

        /// <summary>
        /// Button thoát
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Close;

        /// <summary>
        /// Prefab selection
        /// </summary>
        [SerializeField]
        private UINPCDialog_Selection UISelection_Prefab;

        /// <summary>
        /// Prefab selection Item
        /// </summary>
        [SerializeField]
        private UINPCDialog_ItemSelection UISelection_ItemBoxPrefab;

        /// <summary>
        /// Text danh sách vật phẩm
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_ItemHeader;

        /// <summary>
        /// Text tiêu đề ô danh sách vật phẩm
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_ItemBoxTitle;

        /// <summary>
        /// RectTransform ô nội dung
        /// </summary>
        [SerializeField]
        private RectTransform UI_ContentBox;

        /// <summary>
        /// RectTransform ô danh sách vật phẩm chọn
        /// </summary>
        [SerializeField]
        private RectTransform UI_ItemSelections;

        /// <summary>
        /// RectTransform danh sách lựa chọn
        /// </summary>
        [SerializeField]
        private RectTransform UI_Selections;

        /// <summary>
        /// Chiều cao Text nội dung khi không có ItemSelection
        /// </summary>
        [SerializeField]
        private float UITextNoItemSelectionHeight;

        /// <summary>
        /// Chiều cao Text nội dung khi không có ItemSelection và Selection
        /// </summary>
        [SerializeField]
        private float UITextNoItemAndSelectionHeight;
        #endregion

        #region Private fields
        /// <summary>
        /// RectTransform nội dung
        /// </summary>
        private RectTransform transformContent = null;

        /// <summary>
        /// RectTransform danh sách vật phẩm
        /// </summary>
        private RectTransform transformItemList = null;

        /// <summary>
        /// RectTransform danh sách lựa chọn
        /// </summary>
        private RectTransform transformSelectionList = null;

        /// <summary>
        /// Vật phẩm được chọn
        /// </summary>
        private DialogItemSelectionInfo selectedItem = null;

        /// <summary>
        /// Chiều cao Text nội dung ban đầu
        /// </summary>
        private float UITextOriginHeight;
        #endregion

        #region Properties
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

        /// <summary>
        /// Nội dung
        /// </summary>
        public string Content
        {
            get
            {
                return this.UIText_Content.text;
            }
            set
            {
                this.UIText_Content.text = value;
                this.RebuildLayout(this.transformContent);
            }
        }

        private Dictionary<int, string> _Selections = new Dictionary<int, string>();
        /// <summary>
        /// Danh sách Selection
        /// </summary>
        public Dictionary<int, string> Selections
        {
            get
            {
                return this._Selections;
            }
            set
            {
                this._Selections = value;
                this.RefreshSelections();
            }
        }

        /// <summary>
        /// Chuỗi danh sách vật phẩm
        /// </summary>
        public string ItemHeaderString
        {
            get
            {
                return this.UIText_ItemHeader.text;
            }
            set
            {
                this.UIText_ItemHeader.text = value;
            }
        }

        private List<DialogItemSelectionInfo> _Items = new List<DialogItemSelectionInfo>();
        /// <summary>
        /// Danh sách vật phẩm
        /// </summary>
        public List<DialogItemSelectionInfo> Items
        {
            get
            {
                return this._Items;
            }
            set
            {
                this._Items = value;
                this.RefreshItemSelections();
            }
        }

        /// <summary>
        /// Item có thể chọn được không
        /// </summary>
        public bool ItemSelectable { get; set; }

        /// <summary>
        /// Sự kiện khi node trong Selection được Click
        /// </summary>
        public Action<int> SelectionClick { get; set; }

        /// <summary>
        /// Sự kiện khi Button đồng ý chọn vật phẩm được ấn
        /// </summary>
        public Action<DialogItemSelectionInfo> AcceptSelectItem { get; set; }

        /// <summary>
        /// Hiển thị Button đồng ý
        /// </summary>
        public bool ShowButtonAccept
        {
            get
            {
                return this.UIButton_Accept.gameObject.activeSelf;
            }
            set
            {
                this.UIButton_Accept.gameObject.SetActive(value);
            }
        }

        /// <summary>
        /// Hiển thị
        /// </summary>
        public bool Visible
        {
            get
            {
                return this.gameObject.activeSelf;
            }
            set
            {
                this.gameObject.SetActive(value);
            }
        }

        /// <summary>
        /// Sự kiện đóng khung
        /// </summary>
        public Action Close { get; set; }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            this.transformContent = this.UIText_Content.transform.parent.GetComponent<RectTransform>();
            this.transformItemList = this.UISelection_ItemBoxPrefab.transform.parent.GetComponent<RectTransform>();
            this.transformSelectionList = this.UISelection_Prefab.transform.parent.GetComponent<RectTransform>();
            this.UITextOriginHeight = this.UI_ContentBox.sizeDelta.y;
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
            this.UIButton_Accept.onClick.AddListener(this.ButtonAccept_Clicked);
            this.UIButton_Close.onClick.AddListener(this.ButtonClose_Clicked);
        }

        /// <summary>
        /// Sự kiện khi Button Accept được ấn
        /// </summary>
        private void ButtonAccept_Clicked()
        {
            /// Nếu vật phẩm không thể chọn
            if (!this.ItemSelectable)
            {
                KTGlobal.AddNotification("Không thể chọn vật phẩm!");
                return;
            }
            /// Nếu không có vật phẩm được chọn
            else if (this.selectedItem == null)
            {
                KTGlobal.AddNotification("Hãy chọn một vật phẩm trong danh sách!");
                return;
            }
            
            this.AcceptSelectItem?.Invoke(this.selectedItem);
        }

        /// <summary>
        /// Sự kiện khi Button thoát được ấn
        /// </summary>
        private void ButtonClose_Clicked()
        {
            /// Thực thi sự kiện đóng khung
            this.Close?.Invoke();
        }

        /// <summary>
        /// Sự kiện khi Toggle vật phẩm được ấn chọn
        /// </summary>
        /// <param name="indexInsideList"></param>
        private void ToggleItem_Selected(DialogItemSelectionInfo itemInfo)
        {
            /// Nếu vật phẩm không thể chọn thì thôi
            if (!this.ItemSelectable)
            {
                return;
            }

            /// Đánh dấu vật phẩm được chọn
            this.selectedItem = itemInfo;
            /// Kích hoạt Button nhận
            this.UIButton_Accept.interactable = true;
        }

        /// <summary>
        /// Sự kiện khi Selection được chọn
        /// </summary>
        /// <param name="selectionID"></param>
        private void Selection_Clicked(int selectionID)
        {
            this.SelectionClick?.Invoke(selectionID);
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
            for (int i = 1; i <= skip; i++)
            {
                yield return null;
            }
            work?.Invoke();
        }
        
        /// <summary>
        /// Xây lại giao diện
        /// </summary>
        /// <param name="rectTransform"></param>
        private void RebuildLayout(RectTransform rectTransform)
        {
            /// Nếu đối tượng không được kích hoạt
            if (!this.gameObject.activeSelf)
            {
                return;
            }
            /// Thực thi xây lại giao diện
            this.StartCoroutine(this.ExecuteSkipFrames(1, () => {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
            }));
        }

        #region Selection
        /// <summary>
        /// Làm rỗng danh sách Selection
        /// </summary>
        private void ClearSelections()
        {
            foreach (Transform child in this.transformSelectionList)
            {
                if (child.gameObject != this.UISelection_Prefab.gameObject)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }
        }

        /// <summary>
        /// Tạo mới một đối tượng Selection
        /// </summary>
        /// <param name="selectionID"></param>
        /// <param name="text"></param>
        /// <param name="onClick"></param>
        private void AddSelection(int selectionID, string text)
        {
            UINPCDialog_Selection selection = GameObject.Instantiate<UINPCDialog_Selection>(this.UISelection_Prefab);
            selection.gameObject.SetActive(true);
            selection.transform.SetParent(this.UISelection_Prefab.transform.parent, false);
            selection.SelectionID = selectionID;
            selection.Text = text;
            selection.Click = () => {
                this.Selection_Clicked(selectionID);
            };
        }

        /// <summary>
        /// Làm mới Selection
        /// </summary>
        private void RefreshSelections()
        {
            /// Làm rỗng danh sách lựa chọn
            this.ClearSelections();
            /// Ẩn khung Selection
            this.UI_Selections.gameObject.SetActive(false);

            /// Nếu có vật phẩm, và vật phẩm có thể chọn
            if (this._Items != null && this._Items.Count > 0 && this.ItemSelectable)
            {
                /// Bỏ qua Selection
                return;
            }

            /// Nếu có các lựa chọn
            if (this._Selections != null && this._Selections.Count > 0)
            {
                /// Nếu có thể chọn vật phẩm
                if (this.ItemSelectable)
                {
                    this.UIText_ItemBoxTitle.text = "Chọn một vật phẩm trong danh sách sau:";
                }
                else
                {
                    this.UIText_ItemBoxTitle.text = "Danh sách vật phẩm:";
                }

                /// Hiện khung Selection
                this.UI_Selections.gameObject.SetActive(true);

                /// Duyệt danh sách và thêm lựa chọn
                foreach (KeyValuePair<int, string> pair in this._Selections)
                {
                    this.AddSelection(pair.Key, pair.Value);
                }

                /// Nếu không có vật phẩm
                if (this._Items == null || this._Items.Count <= 0)
                {
                    this.UI_ContentBox.sizeDelta = new Vector2(this.UI_ContentBox.sizeDelta.x, this.UITextNoItemSelectionHeight);
                }
                else
                {
                    this.UI_ContentBox.sizeDelta = new Vector2(this.UI_ContentBox.sizeDelta.x, this.UITextOriginHeight);
                }
            }
            /// Nếu không có các lựa chọn
            else
            {
                /// Nếu không có cả vật phẩm
                if (this._Items == null || this._Items.Count <= 0)
                {
                    this.UI_ContentBox.sizeDelta = new Vector2(this.UI_ContentBox.sizeDelta.x, this.UITextNoItemAndSelectionHeight);
                }
                else
                {
                    this.UI_ContentBox.sizeDelta = new Vector2(this.UI_ContentBox.sizeDelta.x, this.UITextOriginHeight);
                }
            }

            /// Xây lại giao diện
            this.RebuildLayout(this.transformSelectionList);
        }
        #endregion

        #region Item selection
        /// <summary>
        /// Làm rỗng Item Selection
        /// </summary>
        private void ClearItemSelections()
        {
            foreach (Transform child in this.transformItemList)
            {
                if (child.gameObject != this.UISelection_ItemBoxPrefab.gameObject)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }
        }

        /// <summary>
        /// Xóa chọn toàn bộ vật phẩm
        /// </summary>
        private void RemoveSelectionOfAllItems()
        {
            foreach (Transform child in this.transformItemList)
            {
                if (child.gameObject != this.UISelection_ItemBoxPrefab.gameObject)
                {
                    UINPCDialog_ItemSelection uiItemBox = child.GetComponent<UINPCDialog_ItemSelection>();
                    uiItemBox.IsSelected = false;
                }
            }
        }

        /// <summary>
        /// Thêm vật phẩm vào danh sách lựa chọn
        /// </summary>
        /// <param name="itemGD"></param>
        /// <param name="indexInsideList"></param>
        private void AddItem(DialogItemSelectionInfo itemInfo)
        {
            /// Nếu vật phẩm không tồn tại
            if (!Loader.Loader.Items.TryGetValue(itemInfo.ItemID, out ItemData itemData))
            {
                return;
            }
            /// Tạo vật phẩm tương ứng
            GoodsData itemGD = KTGlobal.CreateItemPreview(itemData);
            itemGD.GCount = itemInfo.Quantity;
            itemGD.Binding = itemInfo.Binding;

            UINPCDialog_ItemSelection uiItemBox = GameObject.Instantiate<UINPCDialog_ItemSelection>(this.UISelection_ItemBoxPrefab);
            uiItemBox.transform.SetParent(this.transformItemList, false);
            uiItemBox.gameObject.SetActive(true);
            uiItemBox.Data = itemGD;
            uiItemBox.Interactable = this.ItemSelectable;
            uiItemBox.IsSelected = false;
            /// Nếu có thể chọn
            if (this.ItemSelectable)
            {
                uiItemBox.Select = () => {
                    /// Xóa chọn toàn bộ vật phẩm
                    this.RemoveSelectionOfAllItems();
                    /// Chọn vật phẩm
                    uiItemBox.IsSelected = true;
                    /// Thực thi sự kiện chọn vật phẩm
                    this.ToggleItem_Selected(itemInfo);
                };
            }
        }

        /// <summary>
        /// Làm mới Item Selection
        /// </summary>
        private void RefreshItemSelections()
        {
            /// Làm rỗng danh sách chọn vật phẩm
            this.ClearItemSelections();

            /// Hủy kích hoạt Button nhận
            this.UIButton_Accept.interactable = false;

            /// Nếu có vật phẩm
            if (this._Items != null && this._Items.Count > 0)
            {
                /// Hiện ô danh sách vật phẩm
                this.UI_ItemSelections.gameObject.SetActive(true);
                /// Nếu vật phẩm có thể chọn
                if (this.ItemSelectable)
                {
                    /// Xóa toàn bộ Selection
                    this.ClearSelections();
                    /// Xây lại giao diện Selection
                    this.RebuildLayout(this.transformSelectionList);
                    /// Ẩn Selection
                    this.UI_Selections.gameObject.SetActive(false);
                }

                /// Thứ tự trong danh sách
                int indexInsideList = 0;
                /// Duyệt danh sách vật phẩm
                foreach (DialogItemSelectionInfo itemInfo in this._Items)
                {
                    /// Tạo đối tượng tương ứng
                    this.AddItem(itemInfo);
                    /// Tăng thứ tự trong danh sách
                    indexInsideList++;
                }

                this.UI_ContentBox.sizeDelta = new Vector2(this.UI_ContentBox.sizeDelta.x, this.UITextOriginHeight);
            }
            /// Nếu không có vật phẩm
            else
            {
                /// Nếu không có cả Selection
                if (this._Selections == null || this._Selections.Count <= 0)
                {
                    this.UI_ContentBox.sizeDelta = new Vector2(this.UI_ContentBox.sizeDelta.x, this.UITextNoItemAndSelectionHeight);
                }
                else
                {
                    this.UI_ContentBox.sizeDelta = new Vector2(this.UI_ContentBox.sizeDelta.x, this.UITextNoItemSelectionHeight);
                }
                /// Ẩn ô danh sách vật phẩm
                this.UI_ItemSelections.gameObject.SetActive(false);
            }

            /// Xây lại giao diện
            this.RebuildLayout(this.transformItemList);
        }
        #endregion
        #endregion
    }
}