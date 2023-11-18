using FS.GameEngine.Logic;
using FS.VLTK.UI.Main.Bag;
using FS.VLTK.UI.Main.PortableBag;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace FS.VLTK.UI.Main
{
    /// <summary>
    /// Khung thương khố
    /// </summary>
    public class UIPortableBag : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Button đóng khung
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Close;

        /// <summary>
        /// Lưới danh sách đồ trong thương khố
        /// </summary>
        [SerializeField]
        private UIPortableBag_Grid UIPortableBag_Grid;

        /// <summary>
        /// Text số tiền gửi trong thương khố
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_StoreMoney;

        /// <summary>
        /// Button gửi bạc vào thương khố
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_AddMoney;

        /// <summary>
        /// Button rút bạc từ thương khố
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_WithdrawMoney;

        /// <summary>
        /// Button sắp xếp
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_SortBag;

        /// <summary>
        /// Lưới danh sách túi đồ
        /// </summary>
        [SerializeField]
        private UIBag_Grid UIBag_Grid;
        #endregion

        #region Private fields

        #endregion

        #region Properties
        /// <summary>
        /// Khung danh sách vật phẩm trong thương khố
        /// </summary>
        public UIPortableBag_Grid BagGrid
        {
            get
            {
                return this.UIPortableBag_Grid;
            }
        }

        /// <summary>
        /// Sự kiện đóng khung
        /// </summary>
        public Action Close { get; set; }

        /// <summary>
        /// Sự kiện sắp xếp túi đồ
        /// </summary>
        public Action Sort { get; set; }

        /// <summary>
        /// Sự kiện thêm vật phẩm vào thương khố
        /// </summary>
        public Action<GoodsData> AddItemToPortableBag { get; set; }

        /// <summary>
        /// Sự kiện lấy vật phẩm ra khỏi thương khố
        /// </summary>
        public Action<GoodsData> TakeoutItemFromPortableBag { get; set; }

        /// <summary>
        /// Thêm bạc vào thương khố
        /// </summary>
        public Action<int> AddStoreMoney { get; set; }

        /// <summary>
        /// Rút bạc từ thương khố
        /// </summary>
        public Action<int> WithdrawStoreMoney { get; set; }

        /// <summary>
        /// Quảng bá vật phẩm
        /// </summary>
        public Action<GoodsData> Advertise { get; set; }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.InitPrefabs();
            this.RefreshStoreMoney();
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
            this.UIButton_AddMoney.onClick.AddListener(this.ButtonAddMoney_Clicked);
            this.UIButton_WithdrawMoney.onClick.AddListener(this.ButtonWithdrawMoney_Clicked);
            this.UIBag_Grid.BagItemClicked = this.BagItem_Clicked;
            this.UIPortableBag_Grid.BagItemClicked = this.PortableBagItem_Clicked;

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
        /// Sự kiện khi Button thêm bạc vào thương khố được ấn
        /// </summary>
        private void ButtonAddMoney_Clicked()
        {
            /// Nếu trong túi không có tiền
            if (Global.Data.RoleData.Money <= 0)
            {
                KTGlobal.AddNotification("Không có bạc, không thể gửi vào thương khố!");
                return;
            }
            
            KTGlobal.ShowInputNumber("Nhập số tiền cần gửi vào thương khố.", 1, Global.Data.RoleData.Money, 1, (nMoney) => {
                this.AddStoreMoney?.Invoke(nMoney);
            });
        }

        /// <summary>
        /// Sự kiện khi Button rút bạc từ thương khố được ấn
        /// </summary>
        private void ButtonWithdrawMoney_Clicked()
        {
            /// Nếu thương khố không có tiền
            if (Global.Data.RoleData.StoreMoney <= 0)
            {
                KTGlobal.AddNotification("Không có bạc, không thể rút ra khỏi thương khố!");
                return;
            }

            KTGlobal.ShowInputNumber("Nhập số tiền muốn rút ra khỏi thương khố.", 1, Global.Data.RoleData.StoreMoney, 1, (nMoney) => {
                this.WithdrawStoreMoney?.Invoke(nMoney);
            });
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
            buttons.Add(new KeyValuePair<string, Action>("Cất vào", () => {
                this.AddItemToPortableBag?.Invoke(itemGD);
            }));
            buttons.Add(new KeyValuePair<string, Action>("Quảng bá", () => {
                this.Advertise?.Invoke(itemGD);
            }));
            KTGlobal.ShowItemInfo(itemGD, buttons);
        }

        /// <summary>
        /// Sự kiện khi vật phẩm trong thương khố được ấn
        /// </summary>
        /// <param name="itemGD"></param>
        private void PortableBagItem_Clicked(GoodsData itemGD)
        {
            /// Dữ liệu vật phẩm
            if (!Loader.Loader.Items.TryGetValue(itemGD.GoodsID, out Entities.Config.ItemData itemData))
            {
                return;
            }

            List<KeyValuePair<string, Action>> buttons = new List<KeyValuePair<string, Action>>();
            buttons.Add(new KeyValuePair<string, Action>("Lấy ra", () => {
                this.TakeoutItemFromPortableBag?.Invoke(itemGD);
            }));
            buttons.Add(new KeyValuePair<string, Action>("Quảng bá", () => {
                this.Advertise?.Invoke(itemGD);
            }));
            KTGlobal.ShowItemInfo(itemGD, buttons);
        }
        #endregion

        #region Private methods

        #endregion

        #region Public methods
        /// <summary>
        /// Làm mới số bạc trong kho
        /// </summary>
        public void RefreshStoreMoney()
        {
            this.UIText_StoreMoney.text = KTGlobal.GetDisplayMoney(Global.Data.RoleData.StoreMoney);
        }
        #endregion
    }
}
