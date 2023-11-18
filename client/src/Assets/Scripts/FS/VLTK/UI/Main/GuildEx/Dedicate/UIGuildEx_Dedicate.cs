using FS.GameEngine.Logic;
using FS.VLTK.UI.Main.GuildEx.Dedicate;
using Server.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace FS.VLTK.UI.Main.GuildEx
{
    /// <summary>
    /// Khung cống hiến bang hội
    /// </summary>
    public class UIGuildEx_Dedicate : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Button đóng khung
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Close;

        /// <summary>
        /// Text tổng số bang cống
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_TotalMoney;

        /// <summary>
        /// Prefab vật phẩm cống hiến bang
        /// </summary>
        [SerializeField]
        private UIGuildEx_Dedicate_Item UI_ItemPrefab;

        /// <summary>
        /// Text tổng cống hiến cá nhân
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_SelfTotalDedications;

        /// <summary>
        /// Text cống hiến tuần cá nhân
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_SelfWeekDedications;

        /// <summary>
        /// Text quy đổi cống hiến bạc sang cống hiến cá nhân
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_DedicationPerMoneyHint;

        /// <summary>
        /// Button cống hiến bạc
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_DedicateMoney;

        /// <summary>
        /// Prefab tỷ lệ quy đổi vật phẩm sang điểm cống hiến cá nhân
        /// </summary>
        [SerializeField]
        private UIGuildEx_Dedicate_DedicationPerItem UI_DedicationPerItemPrefab;

        /// <summary>
        /// Button cống hiến vật phẩm
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_DedicateItems;
        #endregion

        #region Properties
        private GuildInfo _Data;
        /// <summary>
        /// Thông tin bang hội
        /// </summary>
        public GuildInfo Data
        {
            get
            {
                return this._Data;
            }
            set
            {
                this._Data = value;
                /// Làm mới dữ liệu
                this.RefreshData();
            }
        }

        /// <summary>
        /// Sự kiện đóng khung
        /// </summary>
        public Action Close { get; set; }

        /// <summary>
        /// Sự kiện cống hiến bạc
        /// </summary>
        public Action<int> DedicateMoney { get; set; }

        /// <summary>
        /// Sự kiện cống hiến vật phẩm
        /// </summary>
        public Action<Dictionary<int, int>> DedicateItems { get; set; }
        #endregion

        #region Private fields
        /// <summary>
        /// RectTransform danh sách vật phẩm
        /// </summary>
        private RectTransform itemListRectTransform;

        /// <summary>
        /// RectTransform tỷ lệ quy đổi vật phẩm sang cống hiến bang hội
        /// </summary>
        private RectTransform dedicationPerItemListRectTransform;
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            this.itemListRectTransform = this.UI_ItemPrefab.transform.parent.GetComponent<RectTransform>();
            this.dedicationPerItemListRectTransform = this.UI_DedicationPerItemPrefab.transform.parent.GetComponent<RectTransform>();
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
            this.UIButton_DedicateMoney.onClick.AddListener(this.ButtonDedicateMoney_Clicked);
            this.UIButton_DedicateItems.onClick.AddListener(this.ButtonDedicateItems_Clicked);
        }

        /// <summary>
        /// Sự kiện khi Button đóng khung được ấn
        /// </summary>
        private void ButtonClose_Clicked()
        {
            this.Close?.Invoke();
        }

        /// <summary>
        /// Sự kiện khi Button cống hiến bạc được ấn
        /// </summary>
        private void ButtonDedicateMoney_Clicked()
        {
            /// Nếu bản thân không có bang hội
            if (Global.Data.RoleData.GuildID == -1)
            {
                KTGlobal.AddNotification("Bạn chưa gia nhập bang hội nào, không thể thực hiện cống hiến!");
                return;
            }

            /// Hiện khung nhập số lượng
            KTGlobal.ShowInputNumber("Nhập số bạc muốn cống hiến vào bang hội.", (totalMoney) =>
            {
                /// Hiện thông báo xác nhận
                KTGlobal.ShowMessageBox("Cống hiến bang hội", string.Format("Xác nhận cống hiến <color=yellow>{0} Bạc</color> vào ngân quỹ của bang hội?", totalMoney), () =>
                {
                    /// Thực thi sự kiện cống hiến bạc
                    this.DedicateMoney?.Invoke(totalMoney);
                });
            });
        }

        /// <summary>
        /// Sự kiện khi Button cống hiến vật phẩm được ấn
        /// </summary>
        private void ButtonDedicateItems_Clicked()
        {
            /// Nếu bản thân không có bang hội
            if (Global.Data.RoleData.GuildID == -1)
            {
                KTGlobal.AddNotification("Bạn chưa gia nhập bang hội nào, không thể thực hiện cống hiến!");
                return;
            }

            /// Tên vật phẩm được đặt vào
            List<string> itemNames = Loader.Loader.GuildConfig.Dedication.PointForItems.Select(x => string.Format("<color=#51f089>[{0}]</color>", KTGlobal.GetItemName(x.ItemID))).ToList();

            KTGlobal.ShowInputItems("Cống hiến bang hội", string.Format("Chọn các vật phẩm {0} để cống hiến vào ngân quỹ của bang hội.", string.Join(", ", itemNames)), "", (itemGD) =>
            {
                return Loader.Loader.GuildConfig.Dedication.PointForItems.Any(x => x.ItemID == itemGD.GoodsID);
            }, (listItems) =>
            {
                /// Danh sách vật phẩm cống hiến với số lượng tương ứng
                Dictionary<int, int> items = new Dictionary<int, int>();
                /// Duyệt danh sách vật phẩm truyền vào
                foreach (GoodsData itemGD in listItems)
                {
                    /// Nếu chưa tồn tại
                    if (!items.ContainsKey(itemGD.GoodsID))
                    {
                        /// Thêm mới
                        items[itemGD.GoodsID] = 0;
                    }
                    /// Tăng số lượng
                    items[itemGD.GoodsID] += itemGD.GCount;
                }
                List<string> itemInfos = items.Select(x => string.Format("<color=#51f089>[{0}]</color> <color=yellow> - SL: {1}</color>", KTGlobal.GetItemName(x.Key), x.Value)).ToList();

                /// Hiện thông báo xác nhận
                KTGlobal.ShowMessageBox("Cống hiến bang hội", string.Format("Xác nhận cống hiến các vật phẩm {0} vào ngân quỹ của bang hội?", string.Join(", ", itemInfos)), () =>
                {
                    /// Thực thi sự kiện cống hiến vật phẩm
                    this.DedicateItems?.Invoke(items);
                }, true);
            });
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Luồng thực thi sự kiện bỏ qua một số Frame
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="work"></param>
        /// <returns></returns>
        private IEnumerator DoExecuteSkipFrames(int skip, Action work)
        {
            for (int i = 1; i <= skip; i++)
            {
                yield return null;
            }
            work?.Invoke();
        }

        /// <summary>
        /// Thực thi sự kiện bỏ qua một số Frame
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="work"></param>
        private void ExecuteSkipFrames(int skip, Action work)
        {
            this.StartCoroutine(this.DoExecuteSkipFrames(skip, work));
        }

        /// <summary>
        /// Làm mới dữ liệu
        /// </summary>
        private void RefreshData()
        {
            /// Toác
            if (this._Data == null)
            {
                /// Đóng khung
                this.Close?.Invoke();
                return;
            }
            /// Nếu bản thân không có bang hội
            else if (Global.Data.RoleData.GuildID == -1)
            {
                KTGlobal.AddNotification("Bạn chưa gia nhập bang hội nào, không thể thực hiện thao tác!");
                /// Đóng khung
                this.Close?.Invoke();
                return;
            }

            /// Ngân quỹ bang hội
            this.UIText_TotalMoney.text = KTGlobal.GetDisplayMoney(this._Data.Data.GuildMoney);

            /// Làm rỗng danh sách vật phẩm
            foreach (Transform child in this.itemListRectTransform.transform)
            {
                if (child.gameObject != this.UI_ItemPrefab.gameObject)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }
            /// Danh sách vật phẩm đã tích lũy
            Dictionary<int, int> storeItems = new Dictionary<int, int>();
            /// Nếu có dữ liệu
            if (!string.IsNullOrEmpty(this._Data.Data.ItemStore))
            {
                /// Chuỗi mã hóa danh sách vật phẩm đã tích lũy
                string[] storeItemsString = this._Data.Data.ItemStore.Split('|');
                /// Duyệt danh sách vật phẩm đã tích lũy
                foreach (string storeItemString in storeItemsString)
                {
                    /// Tách thành các trường
                    string[] fields = storeItemString.Split('_');
                    /// ID vật phẩm
                    int itemID = int.Parse(fields[0]);
                    /// Số lượng
                    int quantity = int.Parse(fields[1]);
                    /// Thêm vào danh sách
                    storeItems[itemID] = quantity;
                }
            }
            /// Duyệt danh sách vật phẩm cống hiến
            foreach (DedicationItem dedicationItem in Loader.Loader.GuildConfig.Dedication.PointForItems)
            {
                /// ID vật phẩm
                int itemID = dedicationItem.ItemID;
                /// Số lượng
                int quantity = storeItems.ContainsKey(itemID) ? storeItems[itemID] : 0;
                /// Tạo ô vật phẩm tích lũy
                UIGuildEx_Dedicate_Item uiItem = GameObject.Instantiate<UIGuildEx_Dedicate_Item>(this.UI_ItemPrefab);
                uiItem.transform.SetParent(this.itemListRectTransform, false);
                uiItem.gameObject.SetActive(true);
                uiItem.ItemID = itemID;
                uiItem.Quantity = quantity;
            }
            /// Xây lại giao diện
            this.ExecuteSkipFrames(1, () =>
            {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.itemListRectTransform);
            });

            /// Cống hiến cá nhân
            this.UIText_SelfTotalDedications.text = this._Data.TotalDedications.ToString();
            this.UIText_SelfWeekDedications.text = this._Data.WeekDedications.ToString();

            /// Tỷ lệ quy đổi bạc ra cống hiến
            this.UIText_DedicationPerMoneyHint.text = string.Format("Tỷ lệ quy đổi: <color=#51f089>1 Bạc</color> tương đương <color=#51f089>{0} điểm cống hiến</color>.", Loader.Loader.GuildConfig.Dedication.PointPerGold);

            /// Làm rỗng danh sách vật phẩm quy đổi
            foreach (Transform child in this.dedicationPerItemListRectTransform.transform)
            {
                if (child.gameObject != this.UI_DedicationPerItemPrefab.gameObject)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }
            /// Duyệt danh sách tỷ lệ quy đổi vật phẩm sang cống hiến bang
            foreach (DedicationItem dedicationItem in Loader.Loader.GuildConfig.Dedication.PointForItems)
            {
                /// Tạo ô vật phẩm tích lũy
                UIGuildEx_Dedicate_DedicationPerItem uiDedicateItem = GameObject.Instantiate<UIGuildEx_Dedicate_DedicationPerItem>(this.UI_DedicationPerItemPrefab);
                uiDedicateItem.transform.SetParent(this.dedicationPerItemListRectTransform, false);
                uiDedicateItem.gameObject.SetActive(true);
                uiDedicateItem.ItemID = dedicationItem.ItemID;
                uiDedicateItem.DedicationRate = dedicationItem.Point;
            }
            /// Xây lại giao diện
            this.ExecuteSkipFrames(1, () =>
            {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.dedicationPerItemListRectTransform);
            });
        }
        #endregion
    }
}
