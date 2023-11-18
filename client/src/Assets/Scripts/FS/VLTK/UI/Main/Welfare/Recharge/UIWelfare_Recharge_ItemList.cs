using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using FS.VLTK.UI.Main.ItemBox;
using Server.Data;
using System.Collections;

namespace FS.VLTK.UI.Main.Welfare.Recharge
{
    /// <summary>
    /// Danh sách vật phẩm trong sự kiện nạp thẻ
    /// </summary>
    public class UIWelfare_Recharge_ItemList : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Button nhận
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Get;

        /// <summary>
        /// Prefab ô vật phẩm
        /// </summary>
        [SerializeField]
        private UIItemBox UI_ItemPrefab;

        /// <summary>
        /// Text lượng nạp yêu cầu
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_RechargeAmount;

        /// <summary>
        /// Đánh dấu đã nhận chưa
        /// </summary>
        [SerializeField]
        private RectTransform UIImage_MarkAlreadyGotten;
        #endregion

        #region Private fields
        /// <summary>
        /// RectTransform danh sách vật phẩm
        /// </summary>
        private RectTransform transformItemList = null;
        #endregion

        #region Properties
        /// <summary>
        /// Sự kiện nhận
        /// </summary>
        public Action Get { get; set; }

        /// <summary>
        /// Cho phép nhận thưởng không
        /// </summary>
        public bool EnableGet
        {
            get
            {
                return this.UIButton_Get.interactable;
            }
            set
            {
                this.UIButton_Get.interactable = value;
            }
        }

        private List<GoodsData> _Items;
        /// <summary>
        /// Danh sách vật phẩm
        /// </summary>
        public List<GoodsData> Items
        {
            get
            {
                return this._Items;
            }
            set
            {
                this._Items = value;
            }
        }

        private int _RechargeAmount;
        /// <summary>
        /// Lượng nạp yêu cầu
        /// </summary>
        public int RechargeAmount
        {
            get
            {
                return this._RechargeAmount;
            }
            set
            {
                this._RechargeAmount = value;

                this.UIText_RechargeAmount.text = KTGlobal.GetDisplayMoney(this._RechargeAmount);
            }
        }

        /// <summary>
        /// Đã nhận chưa
        /// </summary>
        public bool AlreadyGotten
        {
            get
            {
                return this.UIImage_MarkAlreadyGotten.gameObject.activeSelf;
            }
            set
            {
                this.UIImage_MarkAlreadyGotten.gameObject.SetActive(value);
            }
        }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được kích hoạt
        /// </summary>
        private void Awake()
        {
            this.transformItemList = this.UI_ItemPrefab.transform.parent.GetComponent<RectTransform>();
        }

        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.InitPrefabs();
            this.Refresh();
        }
        #endregion

        #region Code UI
        /// <summary>
        /// Khởi tạo ban đầu
        /// </summary>
        private void InitPrefabs()
        {
            this.UIButton_Get.onClick.AddListener(this.ButtonGet_Clicked);
        }

        /// <summary>
        /// Sự kiện khi Button nhận thưởng được ấn
        /// </summary>
        private void ButtonGet_Clicked()
        {
            /// Nếu không cho nhận
            if (!this.EnableGet)
            {
                KTGlobal.AddNotification("Không có gì để nhận!");
                return;
            }

            /// Thực thi sự kiện nhận
            this.Get?.Invoke();
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
        private void RebuildLayout()
        {
            if (!this.gameObject.activeSelf)
            {
                return;
            }
            this.StartCoroutine(this.ExecuteSkipFrames(1, () => {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.transformItemList);
            }));
        }

        /// <summary>
        /// Làm rỗng danh sách vật phẩm
        /// </summary>
        private void ClearItems()
        {
            foreach (Transform child in this.transformItemList.transform)
            {
                if (child.gameObject != this.UI_ItemPrefab.gameObject)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }
        }

        /// <summary>
        /// Thêm vật phẩm tương ứng
        /// </summary>
        /// <param name="itemGD"></param>
        private void AddItem(GoodsData itemGD)
        {
            UIItemBox uiItemBox = GameObject.Instantiate<UIItemBox>(this.UI_ItemPrefab);
            uiItemBox.transform.SetParent(this.transformItemList, false);
            uiItemBox.gameObject.SetActive(true);
            uiItemBox.Data = itemGD;
        }

        /// <summary>
        /// Làm mới dữ liệu
        /// </summary>
        private void Refresh()
        {
            /// Làm rỗng danh sách vật phẩm
            this.ClearItems();

            /// Duyệt danh sách
            foreach (GoodsData itemGD in this._Items)
            {
                /// Thêm vật phẩm tương ứng vào danh sách
                this.AddItem(itemGD);
            }

            /// Xây lại giao diện
            this.RebuildLayout();
        }
        #endregion

        #region Public methods

        #endregion
    }
}
