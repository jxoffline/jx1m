using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using Server.Data;
using System.Collections;
using FS.GameEngine.Logic;
using FS.VLTK.UI.Main.Bag;
using FS.VLTK.UI.Main.Money;

namespace FS.VLTK.UI.Main
{
    /// <summary>
    /// Khung túi đồ
    /// </summary>
    public class UIBag : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Button đóng khung
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Close;

        /// <summary>
        /// Lưới danh sách đồ
        /// </summary>
        [SerializeField]
        private UIBag_Grid UIBag_Grid;

        /// <summary>
        /// Ô số lượng KNB
        /// </summary>
        [SerializeField]
        private UIMoneyBox UIMoneyBox_TokenAmount;

        /// <summary>
        /// Ô số lượng KNB khóa
        /// </summary>
        [SerializeField]
        private UIMoneyBox UIMoneyBox_BoundTokenAmount;

        /// <summary>
        /// Ô số lượng bạc
        /// </summary>
        [SerializeField]
        private UIMoneyBox UIMoneyBox_MoneyAmount;

        /// <summary>
        /// Ô số lượng bạc khóa
        /// </summary>
        [SerializeField]
        private UIMoneyBox UIMoneyBox_BoundMoneyAmount;

        /// <summary>
        /// Button sắp xếp túi đồ
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_SortBag;

        /// <summary>
        /// Button bày bán
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_StartMyselfShop;
        #endregion

        #region Properties
        /// <summary>
        /// Sự kiện đóng khung
        /// </summary>
        public Action Close { get; set; }

        /// <summary>
        /// Sự kiện sắp xếp túi đồ
        /// </summary>
        public Action Sort { get; set; }

        /// <summary>
        /// Sự kiện bày bán
        /// </summary>
        public Action OpenMyselfShop { get; set; }

        /// <summary>
        /// Sự kiện sử dụng vật phẩm
        /// </summary>
        public Action<GoodsData> Use { get; set; }

        /// <summary>
        /// Sự kiện trang bị lên người
        /// </summary>
        public Action<GoodsData> Equip { get; set; }

        /// <summary>
        /// Sự kiện vứt bỏ vật phẩm
        /// </summary>
        public Action<GoodsData> ThrowAway { get; set; }

        /// <summary>
        /// Sự kiện quảng cáo vật phẩm
        /// </summary>
        public Action<GoodsData> Advertise { get; set; }

        /// <summary>
        /// Sự kiện tách vật phẩm
        /// </summary>
        public Action<GoodsData, int> Split { get; set; }
        #endregion

        #region Core MonoBehaviour
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
            this.UIButton_SortBag.onClick.AddListener(this.ButtonSort_Clicked);
            this.UIButton_StartMyselfShop.onClick.AddListener(this.ButtonOpenMyselfShop_Clicked);
            this.UIBag_Grid.BagItemClicked = this.BagItem_Clicked;

        }

        /// <summary>
        /// Sự kiện khi Button đóng khung được ấn
        /// </summary>
        private void ButtonClose_Clicked()
        {
            this.Close?.Invoke();
        }

        /// <summary>
        /// Sự kiện khi Button sắp xếp túi được ấn
        /// </summary>
        private void ButtonSort_Clicked()
        {
            this.Sort?.Invoke();
        }

        /// <summary>
        /// Sự kiện khi Button bày bán được ấn
        /// </summary>
        private void ButtonOpenMyselfShop_Clicked()
        {
            this.OpenMyselfShop?.Invoke();
        }

        /// <summary>
        /// Sự kiện khi vật phẩm trong túi được ấn
        /// </summary>
        /// <param name="itemGD"></param>
        private void BagItem_Clicked(GoodsData itemGD)
        {
            /// Dữ liệu vật phẩm
            if (!Loader.Loader.Items.TryGetValue(itemGD.GoodsID, out Entities.Config.ItemData itemData))
            {
                return;
            }

            List<KeyValuePair<string, Action>> buttons = new List<KeyValuePair<string, Action>>();
            if (itemData.IsEquip || KTGlobal.IsPetEquip(itemData.Genre))
            {
                buttons.Add(new KeyValuePair<string, Action>("Trang bị", () => {
                    /// Nếu trang bị chưa khóa
                    if (itemGD.Binding == 0)
					{
                        KTGlobal.ShowMessageBox("Thông báo", "Trang bị hiện chưa khóa, sau khi trang bị sẽ khóa. Xác nhận mặc trang bị lên?", () => {
                            this.Equip?.Invoke(itemGD);
                        }, true);
					}
					/// Nếu đã khóa thì cho mặc luôn
					else
					{
                        this.Equip?.Invoke(itemGD);
                    }
                }));
            }
            else if (KTGlobal.IsItemCanUse(itemGD.GoodsID))
            {
                buttons.Add(new KeyValuePair<string, Action>("Sử dụng", () => {
                    this.Use?.Invoke(itemGD);
                }));
            }

            /// Nếu vật phẩm có thể vứt bỏ
            if (itemGD.Binding == 0)
            {
                buttons.Add(new KeyValuePair<string, Action>("Vứt bỏ", () => {
                    this.ThrowAway?.Invoke(itemGD);
                }));
            }

            /// Nếu vật phẩm có xếp chồng
            if (itemGD.GCount > 1)
			{
                buttons.Add(new KeyValuePair<string, Action>("Tách", () => {
                    KTGlobal.ShowInputNumber("Nhập số lượng muốn tách", 1, itemGD.GCount - 1, 1, (number) => {
                        this.Split?.Invoke(itemGD, number);
                    });
                }));
            }

            buttons.Add(new KeyValuePair<string, Action>("Quảng bá", () => {
                this.Advertise?.Invoke(itemGD);
            }));

            KTGlobal.ShowItemInfo(itemGD, buttons);
        }
        #endregion
    }
}
