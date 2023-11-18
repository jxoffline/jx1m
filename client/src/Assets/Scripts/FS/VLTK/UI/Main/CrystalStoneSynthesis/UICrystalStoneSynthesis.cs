using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using FS.VLTK.UI.Main.ItemBox;
using Server.Data;
using FS.GameEngine.Logic;
using FS.VLTK.UI.Main.Bag;
using FS.VLTK.Utilities.UnityUI;
using static FS.VLTK.Entities.Config.Equip_Level;

namespace FS.VLTK.UI.Main
{
    /// <summary>
    /// Khung ghép Huyền Tinh
    /// </summary>
    public class UICrystalStoneSynthesis : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Ô chứa sản phẩm 1
        /// </summary>
        [SerializeField]
        private UIItemBox UIItemBox_Product1;

        /// <summary>
        /// Text tỷ lệ thành công sản phẩm 1
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_SuccessPercent1;

        /// <summary>
        /// Ô chứa sản phẩm 2
        /// </summary>
        [SerializeField]
        private UIItemBox UIItemBox_Product2;

        /// <summary>
        /// Text tỷ lệ thành công sản phẩm 2
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_SuccessPercent2;

        /// <summary>
        /// Toggle sử dụng bạc thường
        /// </summary>
        [SerializeField]
        private UIToggleSprite UIToggle_UseMoney;

        /// <summary>
        /// Toggle sử dụng bạc khóa
        /// </summary>
        [SerializeField]
        private UIToggleSprite UIToggle_UseBoundMoney;

        /// <summary>
        /// Text bạc cần
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_MoneyNeed;

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
        /// Button ghép Huyền Tinh
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Synthesis;

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
        /// Sự kiện ghép huyền tinh
        /// </summary>
        public Action<List<GoodsData>, bool> Synthesis { get; set; }
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
            this.UIButton_Synthesis.onClick.AddListener(this.ButtonSynthesis_Clicked);
            this.UIButton_Close.onClick.AddListener(this.ButtonClose_Clicked);
            this.UIToggle_UseMoney.OnSelected = this.ToggleUseMoney_Selected;
            this.UIToggle_UseBoundMoney.OnSelected = this.ToggleUseMoney_Selected;
        }

        /// <summary>
        /// Sự kiện khi Toggle dùng bạc khóa được ấn
        /// </summary>
        /// <param name="isSelected"></param>
        private void ToggleUseMoney_Selected(bool isSelected)
        {
            if (isSelected)
            {
                /// Làm mới thông tin sản phẩm
                this.RefreshCrystalStoneProductInfo();
            }
        }

        /// <summary>
        /// Sự kiện khi Button ô huyền tinh được ấn
        /// </summary>
        /// <param name="uiItemBox"></param>
        private void ButtonCrystalStone_Clicked(UIItemBox stoneSlot)
        {
            if (stoneSlot == null)
            {
                return;
            }

            List<KeyValuePair<string, Action>> buttons = new List<KeyValuePair<string, Action>>();
            buttons.Add(new KeyValuePair<string, Action>("Tháo xuống", () => {
                this.UIBag_Grid.AddItem(stoneSlot.Data);
                stoneSlot.Data = null;
                KTGlobal.CloseItemInfo();

                /// Làm mới thông tin sản phẩm
                this.RefreshCrystalStoneProductInfo();
            }));

            KTGlobal.ShowItemInfo(stoneSlot.Data, buttons);
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
                List<KeyValuePair<string, Action>> buttons = new List<KeyValuePair<string, Action>>();
                buttons.Add(new KeyValuePair<string, Action>("Đặt lên", () => {
                    this.UIBag_Grid.RemoveItem(itemGD);
                    UIItemBox crystalSlot = this.FindEmptySlotInCrystalStoneGrid();
                    if (crystalSlot == null)
                    {
                        KTGlobal.AddNotification("Túi Huyền Tinh đã đầy, không thể đặt thêm vào!");
                        return;
                    }
                    crystalSlot.Data = itemGD;
                    KTGlobal.CloseItemInfo();

                    /// Làm mới thông tin sản phẩm
                    this.RefreshCrystalStoneProductInfo();
                }));

                KTGlobal.ShowItemInfo(itemGD, buttons);
            }
            else
            {
                KTGlobal.ShowItemInfo(itemGD);
            }
        }

        /// <summary>
        /// Sự kiện khi Button ghép Huyền Tinh được ấn
        /// </summary>
        private void ButtonSynthesis_Clicked()
        {
            /// Danh sách Huyền Tinh
            List<GoodsData> crystalStones = new List<GoodsData>();
            foreach (Transform child in this.rectTransformCrystalStoneList.transform)
            {
                if (child.gameObject != this.UIItem_CrystalStonePrefab.gameObject)
                {
                    UIItemBox uiItemBox = child.GetComponent<UIItemBox>();
                    if (uiItemBox.Data != null)
                    {
                        crystalStones.Add(uiItemBox.Data);
                    }
                }
            }
            /// Nếu không có Huyền Tinh
            if (crystalStones.Count <= 0)
            {
                KTGlobal.AddNotification("Hãy đặt vào Huyền Tinh!");
                return;
            }

            /// Cập nhật thông tin sản phẩm ghép Huyền Tinh
            ComposeItem composeItem = KTGlobal.GetComposeCrystalStonesProduct(crystalStones, this.UIToggle_UseBoundMoney.Active);

            /// Nếu không thao tác được
            if (composeItem.nFee <= 0)
            {
                KTGlobal.AddNotification("Hãy đặt vào tối thiểu 2 viên Huyền Tinh để tiến hành ghép!");
                return;
            }

            /// Kiểm tra bạc
            if (this.UIToggle_UseBoundMoney.Active && Global.Data.RoleData.BoundMoney < composeItem.nFee)
            {
                KTGlobal.AddNotification("Bạc khóa mang theo không đủ để ghép Huyền Tinh!");
                return;
            }
            else if (!this.UIToggle_UseBoundMoney.Active && Global.Data.RoleData.Money < composeItem.nFee)
            {
                KTGlobal.AddNotification("Bạc mang theo không đủ để ghép Huyền Tinh!");
                return;
            }

            /// Thực hiện sự kiện cường hóa
            this.Synthesis?.Invoke(crystalStones, this.UIToggle_UseBoundMoney.Active);
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
        private void EmptyCrystalStoneGrid()
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
            for (int i = 1; i <= UICrystalStoneSynthesis.CrystalStoneGridMaxItems; i++)
            {
                UIItemBox uiItemBox = GameObject.Instantiate<UIItemBox>(this.UIItem_CrystalStonePrefab);
                uiItemBox.transform.SetParent(this.rectTransformCrystalStoneList, false);
                uiItemBox.gameObject.SetActive(true);
                uiItemBox.Click = () => {
                    this.ButtonCrystalStone_Clicked(uiItemBox);
                };
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
        /// Cập nhật thông tin sản phẩm ghép Huyền Tinh
        /// </summary>
        private void RefreshCrystalStoneProductInfo()
        {
            /// Danh sách Huyền Tinh
            List<GoodsData> crystalStones = new List<GoodsData>();
            foreach (Transform child in this.rectTransformCrystalStoneList.transform)
            {
                if (child.gameObject != this.UIItem_CrystalStonePrefab.gameObject)
                {
                    UIItemBox uiItemBox = child.GetComponent<UIItemBox>();
                    if (uiItemBox.Data != null)
                    {
                        crystalStones.Add(uiItemBox.Data);
                    }
                }
            }
            /// Nếu không có Huyền Tinh
            if (crystalStones.Count <= 0)
            {
                return;
            }

            /// Cập nhật thông tin sản phẩm ghép Huyền Tinh
            ComposeItem composeItem = KTGlobal.GetComposeCrystalStonesProduct(crystalStones, this.UIToggle_UseBoundMoney.Active);

            /// Nếu không thao tác được
            if (composeItem.nFee <= 0)
            {
                this.UIText_MoneyNeed.text = "0";
                this.UIItemBox_Product1.Data = null;
                this.UIText_SuccessPercent1.text = "";
                this.UIItemBox_Product2.Data = null;
                this.UIText_SuccessPercent2.text = "";
            }
            else
            {
                this.UIText_MoneyNeed.text = KTGlobal.GetDisplayMoney(composeItem.nFee);
                GoodsData product1 = KTGlobal.CreateItemPreview(composeItem.nItemMinLevel);
                product1.Binding = this.UIToggle_UseBoundMoney.Active ? 1 : 0;
                this.UIItemBox_Product1.Data = product1;
                this.UIText_SuccessPercent1.text = string.Format("{0}%", composeItem.nMinLevelRate);
                GoodsData product2 = KTGlobal.CreateItemPreview(composeItem.nItemMaxLevel);
                product2.Binding = this.UIToggle_UseBoundMoney.Active ? 1 : 0;
                this.UIItemBox_Product2.Data = product2;
                this.UIText_SuccessPercent2.text = string.Format("{0}%", composeItem.nMaxLevelRate);
            }
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Làm rỗng khung
        /// </summary>
        public void EmptyFrame()
        {
            this.UIText_MoneyNeed.text = "0";
            this.UIItemBox_Product1.Data = null;
            this.UIText_SuccessPercent1.text = "";
            this.UIItemBox_Product2.Data = null;
            this.UIText_SuccessPercent2.text = "";
            this.EmptyCrystalStoneGrid();
        }
        #endregion
    }
}
