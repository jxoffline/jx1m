using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using FS.VLTK.UI.Main.ItemBox;
using FS.VLTK.UI.Main.Bag;
using Server.Data;
using FS.GameEngine.Logic;
using FS.VLTK.Entities.Config;

namespace FS.VLTK.UI.Main
{
    /// <summary>
    /// Khung tách Huyền Tinh từ Huyền Tinh cấp cao
    /// </summary>
    public class UISplitCrystalStone : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Ô chứa Huyền Tinh gốc
        /// </summary>
        [SerializeField]
        private UIItemBox UIItemBox_SourceCrystalStone;

        /// <summary>
        /// Prefab ô đặt Huyền Tinh
        /// </summary>
        [SerializeField]
        private UIItemBox UIItem_CrystalStonePrefab;

        /// <summary>
        /// Prefab túi đồ
        /// </summary>
        [SerializeField]
        private UIBag_Grid UIBag_Grid;

        /// <summary>
        /// Button tách
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Split;

        /// <summary>
        /// Button đóng khung
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Close;
        #endregion

        #region Constants
        /// <summary>
        /// Kích thước túi huyền tinh tối đa
        /// </summary>
        private const int CrystalStoneGridMaxItems = 15;
        #endregion

        #region Private fields
        /// <summary>
        /// RectTransform lưới chứa danh sách Huyền Tinh đặt vào
        /// </summary>
        private RectTransform rectTransformCrystalStoneList = null;
        #endregion

        #region Properties
        /// <summary>
        /// Sự kiện đóng khung
        /// </summary>
        public Action Close { get; set; }

        /// <summary>
        /// Sự kiện tách Huyền Tinh khỏi trang bị
        /// </summary>
        public Action<GoodsData> Split { get; set; }

        /// <summary>
        /// Cấp độ tối thiểu cho tách
        /// </summary>
        public int MinLevelToSplit { get; set; } = 6;

        /// <summary>
        /// Giá trị Huyền Tinh sau khi tách so với gốc
        /// </summary>
        public float ProductRate { get; set; } = 1f;
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            this.rectTransformCrystalStoneList = this.UIItem_CrystalStonePrefab.transform.parent.GetComponent<RectTransform>();
        }

        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.InitPrefabs();
            this.InitializeCrystalStoneGrid();
            this.EmptyFrame();
        }
        #endregion

        #region Code UI
        /// <summary>
        /// Khởi tạo ban đầu
        /// </summary>
        private void InitPrefabs()
        {
            this.UIBag_Grid.BagItemClicked = this.ButtonBagItem_Clicked;
            this.UIItemBox_SourceCrystalStone.Click = this.ButtonInputCrystalStone_Clicked;
            this.UIButton_Split.onClick.AddListener(this.ButtonSplit_Clicked);
            this.UIButton_Close.onClick.AddListener(this.ButtonClose_Clicked);
        }

        /// <summary>
        /// Sự kiện khi Button trong lưới vật phẩm túi đồ được ấn
        /// </summary>
        /// <param name="itemGD"></param>
        private void ButtonBagItem_Clicked(GoodsData itemGD)
        {
            if (itemGD == null)
            {
                return;
            }

            /// Nếu là Huyền Tinh
            if (KTGlobal.ListCrystalStones.ContainsKey(itemGD.GoodsID))
            {
                /// TODO kiểm tra trang bị có tách được không
                List<KeyValuePair<string, Action>> buttons = new List<KeyValuePair<string, Action>>();
                buttons.Add(new KeyValuePair<string, Action>("Đặt lên", () => {
                    /// Nếu vị trí đã có Huyền Tinh rồi thì yêu cầu gỡ ra
                    if (this.UIItemBox_SourceCrystalStone.Data != null)
                    {
                        KTGlobal.AddNotification("Hãy gỡ Huyền Tinh hiện tại xuống trước!");
                        return;
                    }

                    /// ItemID Huyền Tinh
                    if (!Loader.Loader.Items.TryGetValue(itemGD.GoodsID, out ItemData equipItemData))
                    {
                        KTGlobal.AddNotification("Thông tin vật phẩm không tồn tại!");
                        return;
                    }

                    /// Nếu Huyền Tinh không đủ cấp
                    if (equipItemData.Level < this.MinLevelToSplit)
                    {
                        KTGlobal.AddNotification(string.Format("Chỉ có Huyền Tinh từ cấp {0} trở lên mới có thể tách!", this.MinLevelToSplit));
                        return;
                    }

                    this.UIBag_Grid.RemoveItem(itemGD);
                    this.UIItemBox_SourceCrystalStone.Data = itemGD;
                    KTGlobal.CloseItemInfo();

                    /// Tính toán số Huyền Tinh có được sau khi tách
                    this.CalculateProduct();
                }));

                KTGlobal.ShowItemInfo(itemGD, buttons);
            }
            else
            {
                KTGlobal.ShowItemInfo(itemGD);
            }
        }

        /// <summary>
        /// Sự kiện khi Button trang bị đang thao tác được ấn
        /// </summary>
        private void ButtonInputCrystalStone_Clicked()
        {
            if (this.UIItemBox_SourceCrystalStone.Data == null)
            {
                return;
            }

            List<KeyValuePair<string, Action>> buttons = new List<KeyValuePair<string, Action>>();
            buttons.Add(new KeyValuePair<string, Action>("Tháo xuống", () => {
                this.UIBag_Grid.AddItem(this.UIItemBox_SourceCrystalStone.Data);
                this.UIItemBox_SourceCrystalStone.Data = null;
                KTGlobal.CloseItemInfo();

                /// Tính toán số Huyền Tinh có được sau khi tách
                this.CalculateProduct();
            }));

            KTGlobal.ShowItemInfo(this.UIItemBox_SourceCrystalStone.Data, buttons);
        }

        /// <summary>
        /// Sự kiện khi Button tách được ấn
        /// </summary>
        private void ButtonSplit_Clicked()
        {
            /// Nếu không có Huyền Tinh
            if (this.UIItemBox_SourceCrystalStone.Data == null)
            {
                KTGlobal.AddNotification("Hãy đặt vào Huyền Tinh cần tách!");
                return;
            }

            /// ItemID Huyền Tinh
            if (!Loader.Loader.Items.TryGetValue(this.UIItemBox_SourceCrystalStone.Data.GoodsID, out ItemData equipItemData))
            {
                KTGlobal.AddNotification("Huyền Tinh không tồn tại!");
                return;
            }

            /// Nếu cấp tách không đủ để tách
            if (equipItemData.Level < this.MinLevelToSplit)
            {
                KTGlobal.AddNotification(string.Format("Chỉ có Huyền Tinh cấp {0} trở lên mới có thể tách!", this.MinLevelToSplit));
                return;
            }

            /// Thực hiện sự kiện tách Huyền Tinh cấp cao
            this.Split?.Invoke(this.UIItemBox_SourceCrystalStone.Data);
        }

        /// <summary>
        /// Sự kiện khi Button đóng khung được ấn
        /// </summary>
        private void ButtonClose_Clicked()
        {
            this.Close?.Invoke();
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Làm rỗng danh sách ô chứa Huyền Tinh
        /// </summary>
        private void DoEmptyCrystalStoneGrid()
        {
            foreach (Transform child in this.rectTransformCrystalStoneList.transform)
            {
                if (child.gameObject != this.UIItem_CrystalStonePrefab.gameObject)
                {
                    UIItemBox uiItemBox = child.GetComponent<UIItemBox>();
                    uiItemBox.Data = null;
                }
            }
        }

        /// <summary>
        /// Khởi tạo danh sách lưới Huyền Tinh
        /// </summary>
        private void InitializeCrystalStoneGrid()
        {
            for (int i = 1; i <= UISplitCrystalStone.CrystalStoneGridMaxItems; i++)
            {
                UIItemBox uiItemBox = GameObject.Instantiate<UIItemBox>(this.UIItem_CrystalStonePrefab);
                uiItemBox.transform.SetParent(this.rectTransformCrystalStoneList, false);
                uiItemBox.gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// Tìm một vị trí trống trong danh sách lưới Huyền Tinh
        /// </summary>
        /// <returns></returns>
        private UIItemBox FindEmptySlotInCrystalStoneGrid()
        {
            foreach (Transform child in this.rectTransformCrystalStoneList.transform)
            {
                if (child.gameObject != this.UIItem_CrystalStonePrefab.gameObject)
                {
                    UIItemBox uiItemBox = child.GetComponent<UIItemBox>();
                    if (uiItemBox.Data == null)
                    {
                        return uiItemBox;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Tìm vị trí chứa Huyền Tinh có DbID tương ứng
        /// </summary>
        /// <param name="itemDbID"></param>
        /// <returns></returns>
        private UIItemBox FindCrystalStoneSlot(int itemDbID)
        {
            foreach (Transform child in this.rectTransformCrystalStoneList.transform)
            {
                if (child.gameObject != this.UIItem_CrystalStonePrefab.gameObject)
                {
                    UIItemBox uiItemBox = child.GetComponent<UIItemBox>();
                    if (uiItemBox.Data != null && uiItemBox.Data.Id == itemDbID)
                    {
                        return uiItemBox;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Tính toán số lượng Huyền Tinh có sau khi tách
        /// </summary>
        private void CalculateProduct()
        {
            /// Nếu không có trang bị
            if (this.UIItemBox_SourceCrystalStone.Data == null)
            {
                return;
            }

            /// ItemID trang bị
            if (!Loader.Loader.Items.TryGetValue(this.UIItemBox_SourceCrystalStone.Data.GoodsID, out ItemData equipItemData))
            {
                return;
            }

            /// Nếu cấp tách không đủ
            if (equipItemData.Level < this.MinLevelToSplit)
            {
                return;
            }

            /// Làm rỗng lưới Huyền Tinh
            this.DoEmptyCrystalStoneGrid();

            /// Danh sách Huyền Tinh kết quả
            List<ItemData> listCrystalStones = KTGlobal.GetCrystalStonesBySplitingHighLevelCrystalStone(this.UIItemBox_SourceCrystalStone.Data, this.ProductRate);

            /// Mảng đánh dấu Huyền Tinh đã xét qua
            HashSet<int> checkedCrystalStones = new HashSet<int>();

            /// Duyệt danh sách Huyền Tinh kết quả
            foreach (ItemData itemData in listCrystalStones)
            {
                /// Nếu có trong danh sách đánh dấu
                if (checkedCrystalStones.Contains(itemData.ItemID))
                {
                    /// Bỏ qua
                    continue;
                }
                /// Thêm vào danh sách đánh dấu
                checkedCrystalStones.Add(itemData.ItemID);

                /// Tổng số lượng trong danh sách kết quả
                int count = listCrystalStones.Where(x => x.ItemID == itemData.ItemID).Count();

                /// Tạo Huyền Tinh tương ứng
                GoodsData itemGD = KTGlobal.CreateItemPreview(itemData);
                itemGD.Binding = 1;
                itemGD.GCount = count;

                /// Thêm Huyền Tinh vào danh sách
                UIItemBox uiItemBox = this.FindEmptySlotInCrystalStoneGrid();
                if (uiItemBox != null)
                {
                    uiItemBox.Data = itemGD;
                }
            }
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Làm rỗng khung
        /// </summary>
        public void EmptyFrame()
        {
            this.UIItemBox_SourceCrystalStone.Data = null;
            this.DoEmptyCrystalStoneGrid();
        }

        /// <summary>
        /// Làm rỗng danh sách Huyền Tinh
        /// </summary>
        public void EmptyCrystalStoneGrid()
        {
            this.DoEmptyCrystalStoneGrid();
        }

        /// <summary>
        /// Xóa Huyền Tinh đang thao tác
        /// </summary>
        public void RemoveSourceCrystalStone()
        {
            if (this.UIItemBox_SourceCrystalStone != null)
            {
                this.UIItemBox_SourceCrystalStone.Data = null;
            }
        }
        #endregion
    }
}