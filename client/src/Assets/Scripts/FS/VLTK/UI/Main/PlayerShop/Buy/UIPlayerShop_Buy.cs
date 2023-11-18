using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using FS.VLTK.UI.Main.Bag;
using FS.VLTK.UI.Main.PlayerShop;
using System.Collections;
using Server.Data;
using FS.GameEngine.Logic;

namespace FS.VLTK.UI.Main
{
    /// <summary>
    /// Khung bán hàng
    /// </summary>
    public class UIPlayerShop_Buy : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Button đóng khung
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Close;

        /// <summary>
        /// Text tiêu đề khung
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Title;

        /// <summary>
        /// Prefab vật phẩm được bán
        /// </summary>
        [SerializeField]
        private UIPlayerShop_Buy_Item UIItem_Prefab;

        /// <summary>
        /// Khung xác nhận mua vật phẩm
        /// </summary>
        [SerializeField]
        private UIPlayerShop_Buy_ConfirmBuyFrame UI_BuyItemConfirmFrame;
        #endregion

        #region Private fields
        /// <summary>
        /// RectTransform danh sách vật phẩm bán
        /// </summary>
        private RectTransform transformItemsList = null;
        #endregion

        #region Properties
        private StallData _Data;
        /// <summary>
        /// Dữ liệu
        /// </summary>
        public StallData Data
        {
            get
            {
                return this._Data;
            }
            set
            {
                this._Data = value;
                /// Làm mới hiển thị giao diện
                this.DoRefreshShop();
            }
        }

        /// <summary>
        /// Sự kiện đóng khung
        /// </summary>
        public Action Close { get; set; }

        /// <summary>
        /// Sự kiện mua vật phẩm
        /// </summary>
        public Action<GoodsData> Buy { get; set; }
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
        }

        /// <summary>
        /// Sự kiện khi vật phẩm trong cửa hàng được chọn
        /// </summary>
        /// <param name="uiBuyItem"></param>
        private void ButtonItem_Clicked(UIPlayerShop_Buy_Item uiBuyItem)
        {
            this.UI_BuyItemConfirmFrame.Data = uiBuyItem.Data;
            this.UI_BuyItemConfirmFrame.Price = uiBuyItem.Price;
            this.UI_BuyItemConfirmFrame.Buy = () => {
                this.Buy?.Invoke(uiBuyItem.Data);
                this.UI_BuyItemConfirmFrame.Hide();
            };
            this.UI_BuyItemConfirmFrame.Show();
        }
        #endregion

        #region Private fields
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
        /// Tìm ô vật phẩm tương ứng
        /// </summary>
        /// <param name="dbID"></param>
        /// <returns></returns>
        private UIPlayerShop_Buy_Item FindItem(int dbID)
        {
            /// Xóa toàn bộ vật phẩm
            foreach (Transform child in this.transformItemsList.transform)
            {
                if (child.gameObject != this.UIItem_Prefab.gameObject)
                {
                    /// UI tương ứng
                    UIPlayerShop_Buy_Item uiBuyItem = child.GetComponent<UIPlayerShop_Buy_Item>();
                    /// Thỏa mãn
                    if (uiBuyItem.Data.Id == dbID)
                    {
                        /// Trả về kết quả
                        return uiBuyItem;
                    }
                }
            }
            /// Không tìm thấy
            return null;
        }

        /// <summary>
        /// Làm mới giao diện cửa hàng
        /// </summary>
        private void DoRefreshShop()
        {
            /// Toác
            if (this._Data == null)
            {
                /// Đóng khung
                this.Close?.Invoke();
                /// Bỏ qua
                return;
            }

            /// Xóa danh sách vật phẩm cũ
            foreach (Transform child in this.transformItemsList.transform)
            {
                if (child.gameObject != this.UIItem_Prefab.gameObject)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }

            /// Thông tin người chơi
            if (Global.Data.OtherRoles.TryGetValue(this._Data.RoleID, out RoleData rd))
            {
                /// Title
                this.UIText_Title.text = string.Format("Sạp hàng của ", rd.RoleName);
            }
            /// Nếu không tồn tại
            else
            {
                /// Bot
                this.UIText_Title.text = string.Format("Sạp hàng ủy thác");
            }
            

            /// Nếu chưa tồn tại thì tạo mới
            if (this._Data.GoodsList == null)
            {
                this._Data.GoodsList = new List<GoodsData>();
            }
            if (this._Data.GoodsPriceDict == null)
            {
                this._Data.GoodsPriceDict = new Dictionary<int, int>();
            }

            /// Duyệt danh sách vật phẩm
            foreach (GoodsData itemGD in this._Data.GoodsList)
            {
                if (this._Data.GoodsPriceDict.TryGetValue(itemGD.Id, out int price))
                {
                    /// Tạo mới UI
                    UIPlayerShop_Buy_Item uiBuyItem = GameObject.Instantiate<UIPlayerShop_Buy_Item>(this.UIItem_Prefab);
                    uiBuyItem.transform.SetParent(this.transformItemsList, false);
                    uiBuyItem.gameObject.SetActive(true);
                    /// Thiết lập giá trị
                    uiBuyItem.Data = itemGD;
                    uiBuyItem.Price = price;
                    uiBuyItem.Buy = () => {
                        this.ButtonItem_Clicked(uiBuyItem);
                    };
                }
            }

            /// Xây lại giao diện
            this.StartCoroutine(this.ExecuteSkipFrames(1, () => {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.transformItemsList);
            }));
        }
        #endregion

        #region Public fields
        /// <summary>
        /// Xóa vật phẩm
        /// </summary>
        /// <param name="dbID"></param>
        public void RemoveItem(int dbID)
        {
            /// Ô vật phẩm cũ
            UIPlayerShop_Buy_Item uiSellItem = this.FindItem(dbID);
            /// Nếu tìm thấy
            if (uiSellItem != null)
            {
                /// Xóa ô cũ
                GameObject.Destroy(uiSellItem.gameObject);
                /// Xây lại giao diện
                this.StartCoroutine(this.ExecuteSkipFrames(1, () => {
                    UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.transformItemsList);
                }));
            }
        }
        #endregion
    }
}
