using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using FS.VLTK.UI.Main.ItemBox;
using FS.VLTK.Utilities.UnityUI;
using FS.VLTK.UI.Main.Bag;
using Server.Data;
using FS.GameEngine.Logic;
using static FS.VLTK.Entities.Config.Equip_Level;
using FS.VLTK.Entities.Config;

namespace FS.VLTK.UI.Main
{
    /// <summary>
    /// Khung cường hóa trang bị
    /// </summary>
    public class UIEnhance : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Ô chứa trang bị
        /// </summary>
        [SerializeField]
        private UIItemBox UIItemBox_Equip;

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
        /// Text tỷ lệ thành công
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_SuccessPercent;

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
        /// Button cường hóa
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Enhance;

        /// <summary>
        /// Button đóng khung
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Close;

        /// <summary>
        /// Hiệu ứng cường hóa trang bị thành công
        /// </summary>
        [SerializeField]
        private UIAnimatedImage UIImage_EnhanceSuccessEffect;
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
        /// Sự kiện cường hóa trang bị
        /// </summary>
        public Action<GoodsData, List<GoodsData>, bool> Enhance { get; set; }
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
            this.UIButton_Enhance.onClick.AddListener(this.ButtonEnhance_Clicked);
            this.UIButton_Close.onClick.AddListener(this.ButtonClose_Clicked);
            this.UIImage_EnhanceSuccessEffect.gameObject.SetActive(false);
            this.UIImage_EnhanceSuccessEffect.Finish = () => {
                this.UIImage_EnhanceSuccessEffect.gameObject.SetActive(false);
            };
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

                /// Tính toán tỷ lệ cường hóa và số bạc cần
                this.CalculateEnhanceRateAndMoneyDemand();
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

                    /// Tính toán tỷ lệ cường hóa và số bạc cần
                    this.CalculateEnhanceRateAndMoneyDemand();
                }));

                KTGlobal.ShowItemInfo(itemGD, buttons);
            }
            /// Nếu là trang bị
            else if (KTGlobal.IsEquip(itemGD.GoodsID) || KTGlobal.IsPetEquipItem(itemGD.GoodsID))
            {
                /// TODO kiểm tra trang bị có cường hóa được không
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

                    /// Nếu trang bị không thể cường hóa được
                    if (!KTGlobal.CanEquipEnhance(equipItemData))
                    {
                        KTGlobal.AddNotification("Trang bị này không thể cường hóa!");
                        return;
                    }
                    /// Nếu cấp cường hóa đã đạt tối đa
                    else if (itemGD.Forge_level >= KTGlobal.CalculateEquipMaxEnhanceLevel(equipItemData))
                    {
                        KTGlobal.AddNotification("Trang bị này đã cường hóa tới cấp độ tối đa!");
                        return;
                    }

                    this.UIBag_Grid.RemoveItem(itemGD);
                    this.UIItemBox_Equip.Data = itemGD;
                    KTGlobal.CloseItemInfo();

                    /// Tính toán tỷ lệ cường hóa và số bạc cần
                    this.CalculateEnhanceRateAndMoneyDemand();
                }));

                KTGlobal.ShowItemInfo(itemGD, buttons);
            }
            else
            {
                KTGlobal.ShowItemInfo(itemGD);
            } 
        }

        /// <summary>
        /// Sự kiện khi Button trang bị đang cường hóa được ấn
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

                /// Tính toán tỷ lệ cường hóa và số bạc cần
                this.CalculateEnhanceRateAndMoneyDemand();
            }));

            KTGlobal.ShowItemInfo(this.UIItemBox_Equip.Data, buttons);
        }

        /// <summary>
        /// Sự kiện khi Button cường hóa được ấn
        /// </summary>
        private void ButtonEnhance_Clicked()
        {
            /// Nếu không có trang bị
            if (this.UIItemBox_Equip.Data == null)
            {
                KTGlobal.AddNotification("Hãy đặt vào trang bị cần cường hóa!");
                return;
            }

            /// ItemID trang bị
            if (!Loader.Loader.Items.TryGetValue(this.UIItemBox_Equip.Data.GoodsID, out ItemData equipItemData))
            {
                KTGlobal.AddNotification("Trang bị không tồn tại!");
                return;
            }

            /// Nếu trang bị không thể cường hóa được
            if (!KTGlobal.CanEquipEnhance(equipItemData))
            {
                KTGlobal.AddNotification("Trang bị này không thể cường hóa!");
                return;
            }
            /// Nếu cấp cường hóa đã đạt tối đa
            else if (this.UIItemBox_Equip.Data.Forge_level >= KTGlobal.CalculateEquipMaxEnhanceLevel(equipItemData))
            {
                KTGlobal.AddNotification("Trang bị này đã cường hóa tới cấp độ tối đa!");
                return;
            }

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

            /// Tỷ lệ cường hóa và số bạc cần
            CalcProb calcProb = KTGlobal.GetEquipEnhanceRequirement(crystalStones, this.UIItemBox_Equip.Data);

            /// Nếu tỷ lệ cường hóa quá thấp
            if (calcProb.nTrueProb < 10)
            {
                KTGlobal.AddNotification("Tỷ lệ cường hóa quá thấp!");
                return;
            }
            else if (calcProb.nTrueProb >= 120)
            {
                KTGlobal.AddNotification("Tỷ lệ cường hóa quá cao, xin đừng lãng phí Huyền Tinh!");
                return;
            }

            /// Kiểm tra số bạc
            if (this.UIToggle_UseBoundMoney.Active && Global.Data.RoleData.BoundMoney < calcProb.nMoney)
            {
                KTGlobal.AddNotification("Số bạc khóa mang theo không đủ để cường hóa!");
                return;
            }
            else if (!this.UIToggle_UseBoundMoney.Active && Global.Data.RoleData.Money < calcProb.nMoney)
            {
                KTGlobal.AddNotification("Số bạc mang theo không đủ để cường hóa!");
                return;
            }

            /// Thực hiện sự kiện cường hóa
            this.Enhance?.Invoke(this.UIItemBox_Equip.Data, crystalStones, this.UIToggle_UseBoundMoney.Active);
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
            for (int i = 1; i <= UIEnhance.CrystalStoneGridMaxItems; i++)
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
        /// Tính toán tỷ lệ cường hóa cũng như số bạc cần
        /// </summary>
        private void CalculateEnhanceRateAndMoneyDemand()
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

            /// Nếu không có trang bị hoặc danh sách Huyền Tinh rỗng thì hiện mặc định
            if (this.UIItemBox_Equip.Data == null || crystalStones.Count <= 0)
            {
                this.UIText_MoneyNeed.text = "0";
                this.UIText_SuccessPercent.text = "0%";
                return;
            }

            /// ItemID trang bị
            if (!Loader.Loader.Items.TryGetValue(this.UIItemBox_Equip.Data.GoodsID, out ItemData equipItemData))
            {
                this.UIText_MoneyNeed.text = "0";
                this.UIText_SuccessPercent.text = "0%";
                return;
            }
            
            /// Nếu trang bị không thể cường hóa được
            if (!KTGlobal.CanEquipEnhance(equipItemData))
            {
                //KTGlobal.AddNotification("Trang bị này không thể cường hóa!");
                this.UIText_MoneyNeed.text = "0";
                this.UIText_SuccessPercent.text = "0%";
                return;
            }
            /// Nếu cấp cường hóa đã đạt tối đa
            else if (this.UIItemBox_Equip.Data.Forge_level >= KTGlobal.CalculateEquipMaxEnhanceLevel(equipItemData))
            {
                //KTGlobal.AddNotification("Trang bị này đã cường hóa tới cấp độ tối đa!");
                this.UIText_MoneyNeed.text = "0";
                this.UIText_SuccessPercent.text = "0%";
                return;
            }

            /// Tỷ lệ cường hóa và số bạc cần
            CalcProb calcProb = KTGlobal.GetEquipEnhanceRequirement(crystalStones, this.UIItemBox_Equip.Data);

            /// Đổ dữ liệu vào UI
            this.UIText_MoneyNeed.text = KTGlobal.GetDisplayMoney(calcProb.nMoney);
            this.UIText_SuccessPercent.text = string.Format("{0}%", calcProb.nTrueProb);
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Làm rỗng khung
        /// </summary>
        public void EmptyFrame()
        {
            this.UIText_MoneyNeed.text = "0";
            this.UIText_SuccessPercent.text = "0%";
            this.UIItemBox_Equip.Data = null;
            this.DoEmptyCrystalStoneGrid();
        }

        /// <summary>
        /// Làm rỗng danh sách Huyền Tinh
        /// </summary>
        public void EmptyCrystalStoneGrid()
        {
            this.DoEmptyCrystalStoneGrid();
            this.UIText_MoneyNeed.text = "0";
            this.UIText_SuccessPercent.text = "0%";
        }

        /// <summary>
        /// Thực thi hiệu ứng cường hóa trang bị thành công
        /// </summary>
        public void PlayEquipEnhanceSuccessEffect()
        {
            this.UIImage_EnhanceSuccessEffect.gameObject.SetActive(true);
            this.UIImage_EnhanceSuccessEffect.Play();
        }

        /// <summary>
        /// Cập nhật hiển thị trang bị cường hóa
        /// </summary>
        public void RefreshEnhanceEquip()
        {
            if (this.UIItemBox_Equip != null)
            {
                this.UIItemBox_Equip.Refresh();
            }
        }
        #endregion
    }
}
