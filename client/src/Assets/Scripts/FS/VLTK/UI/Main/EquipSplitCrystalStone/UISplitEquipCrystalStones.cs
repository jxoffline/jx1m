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
    /// Khung tách Huyền Tinh của trang bị
    /// </summary>
    public class UISplitEquipCrystalStones : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Ô chứa trang bị
        /// </summary>
        [SerializeField]
        private UIItemBox UIItemBox_Equip;

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
        public int MinLevelToSplit { get; set; } = 10;

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
            this.UIItemBox_Equip.Click = this.ButtonEquip_Clicked;
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

            /// Nếu là trang bị
            if (KTGlobal.IsEquip(itemGD.GoodsID))
            {
                /// TODO kiểm tra trang bị có tách được không
                List<KeyValuePair<string, Action>> buttons = new List<KeyValuePair<string, Action>>();
                buttons.Add(new KeyValuePair<string, Action>("Đặt lên", () => {
                    /// Nếu vị trí đã có trang bị thì yêu cầu gỡ xuống
                    if (this.UIItemBox_Equip.Data != null)
                    {
                        KTGlobal.AddNotification("Hãy gỡ trang bị hiện tại xuống trước!");
                        return;
                    }

                    /// ItemID trang bị
                    if (!Loader.Loader.Items.TryGetValue(itemGD.GoodsID, out ItemData equipItemData))
                    {
                        KTGlobal.AddNotification("Trang bị không tồn tại!");
                        return;
                    }

                    /// Nếu trang bị không thể tách được
                    if (itemGD.Forge_level < this.MinLevelToSplit)
                    {
                        KTGlobal.AddNotification(string.Format("Chỉ có trang bị sau khi cường hóa cấp {0} trở lên mới có thể tách Huyền Tinh!", this.MinLevelToSplit));
                        return;
                    }

                    this.UIBag_Grid.RemoveItem(itemGD);
                    this.UIItemBox_Equip.Data = itemGD;
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
        private void ButtonEquip_Clicked()
        {
            if (this.UIItemBox_Equip.Data == null)
            {
                return;
            }

            List<KeyValuePair<string, Action>> buttons = new List<KeyValuePair<string, Action>>();
            buttons.Add(new KeyValuePair<string, Action>("Tháo xuống", () => {
                this.UIBag_Grid.AddItem(this.UIItemBox_Equip.Data);
                this.UIItemBox_Equip.Data = null;
                KTGlobal.CloseItemInfo();

                /// Tính toán số Huyền Tinh có được sau khi tách
                this.CalculateProduct();
            }));

            KTGlobal.ShowItemInfo(this.UIItemBox_Equip.Data, buttons);
        }

        /// <summary>
        /// Sự kiện khi Button tách được ấn
        /// </summary>
        private void ButtonSplit_Clicked()
        {
            /// Nếu không có trang bị
            if (this.UIItemBox_Equip.Data == null)
            {
                KTGlobal.AddNotification("Hãy đặt vào trang bị cần tách!");
                return;
            }

            /// ItemID trang bị
            if (!Loader.Loader.Items.TryGetValue(this.UIItemBox_Equip.Data.GoodsID, out ItemData equipItemData))
            {
                KTGlobal.AddNotification("Trang bị không tồn tại!");
                return;
            }

            /// Nếu cấp tách không đủ để tách
            if (this.UIItemBox_Equip.Data.Forge_level < this.MinLevelToSplit)
            {
                KTGlobal.AddNotification(string.Format("Chỉ có trang bị sau khi tách cấp {0} trở lên mới có thể tách Huyền Tinh!", this.MinLevelToSplit));
                return;
            }

            /// Thực hiện sự kiện tách Huyền Tinh khỏi trang bị
            this.Split?.Invoke(this.UIItemBox_Equip.Data);
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
            for (int i = 1; i <= UISplitEquipCrystalStones.CrystalStoneGridMaxItems; i++)
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
            if (this.UIItemBox_Equip.Data == null)
            {
                return;
            }

            /// Nếu cấp tách không đủ để tách
            if (this.UIItemBox_Equip.Data.Forge_level < this.MinLevelToSplit)
            {
                return;
            }

            /// ItemID trang bị
            if (!Loader.Loader.Items.TryGetValue(this.UIItemBox_Equip.Data.GoodsID, out ItemData equipItemData))
            {
                return;
            }

            /// Nếu cấp tách không đủ
            if (this.UIItemBox_Equip.Data.Forge_level < this.MinLevelToSplit)
            {
                return;
            }

            /// Làm rỗng lưới Huyền Tinh
            this.DoEmptyCrystalStoneGrid();

            /// Danh sách Huyền Tinh kết quả
            List<ItemData> listCrystalStones = KTGlobal.GetCrystalStonesBySplitingEquip(this.UIItemBox_Equip.Data, this.ProductRate);

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
            this.UIItemBox_Equip.Data = null;
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
        /// Cập nhật hiển thị trang bị
        /// </summary>
        public void RefreshEquip()
        {
            if (this.UIItemBox_Equip != null)
            {
                this.UIItemBox_Equip.Refresh();
            }
        }
        #endregion
    }
}
