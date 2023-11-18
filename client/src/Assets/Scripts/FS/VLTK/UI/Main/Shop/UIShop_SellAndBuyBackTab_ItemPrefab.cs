using FS.GameEngine.Logic;
using FS.VLTK.Entities.Config;
using FS.VLTK.UI.Main.ItemBox;
using FS.VLTK.Utilities.UnityUI;
using Server.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using static FS.VLTK.Entities.Enum;

namespace FS.VLTK.UI.Main.Shop
{
    /// <summary>
    /// Ô vật phẩm trong khung bán và mua lại
    /// </summary>
    public class UIShop_SellAndBuyBackTab_ItemPrefab : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Ô vật phẩm
        /// </summary>
        [SerializeField]
        private UIItemBox UIItemBox_Item;

        /// <summary>
        /// Text tên vật phẩm
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_ItemName;

        /// <summary>
        /// Text giá mua
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Price;

        /// <summary>
        /// Image Icon tiền tệ
        /// </summary>
        [SerializeField]
        private SpriteFromAssetBundle UIImage_MoneyIcon;

        /// <summary>
        /// Button mua
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Buy;
        #endregion

        #region Properties
        /// <summary>
        /// Sự kiện mua
        /// </summary>
        public Action Buy { get; set; }

        /// <summary>
        /// Loại tiền tệ
        /// </summary>
        public MoneyType MoneyType { get; set; }

        /// <summary>
        /// Dữ liệu vật phẩm bán
        /// </summary>
        public GoodsData Data { get; set; }
        #endregion

        #region Core MonoBehavour
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
            this.UIButton_Buy.onClick.AddListener(this.ButtonBuy_Clicked);
        }

        /// <summary>
        /// Sự kiện khi Button mua được ấn
        /// </summary>
        private void ButtonBuy_Clicked()
        {
            this.Buy?.Invoke();
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Thực thi sự kiện bỏ qua một số Frame
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        private IEnumerator ExecuteSkipFrames(int skip, Action callback)
        {
            for (int i = 1; i <= skip; i++)
            {
                yield return null;
            }
            callback?.Invoke();
        }

        /// <summary>
        /// Xây lại giao diện
        /// </summary>
        /// <param name="layout"></param>
        private void RebuildLayout(RectTransform layout)
        {
            this.StartCoroutine(this.ExecuteSkipFrames(1, () => {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(layout);
            }));
        }

        /// <summary>
        /// Làm mới hiển thị
        /// </summary>
        private void Refresh()
        {
            /// Nếu không có dữ liệu
            if (this.Data == null)
            {
                this.Destroy();
                return;
            }
            /// Nếu vật phẩm không tồn tại
            if (!Loader.Loader.Items.TryGetValue(this.Data.GoodsID, out ItemData itemData))
            {
                this.Destroy();
                return;
            }

            /// Đổ dữ liệu
            GoodsData itemGD = this.Data;
            this.UIItemBox_Item.Data = itemGD;

            /// Tên
            this.UIText_ItemName.text = KTGlobal.GetItemName(itemGD);
            this.UIText_ItemName.color = KTGlobal.GetItemColor(itemGD);

            /// Giá mua lại ở Shop là giá gốc của vật phẩm
            this.UIText_Price.text = string.Format("<color=#ffdf3d>{0}</color>", KTGlobal.GetDisplayMoney(Math.Max(1, itemData.Price)));
            KTGlobal.GetMoneyDisplayImage(this.MoneyType, out string bundleDir, out string atlasName, out string spriteName);
            this.UIImage_MoneyIcon.gameObject.SetActive(true);
            this.UIImage_MoneyIcon.BundleDir = bundleDir;
            this.UIImage_MoneyIcon.AtlasName = atlasName;
            this.UIImage_MoneyIcon.SpriteName = spriteName;
            this.UIImage_MoneyIcon.Load();

            this.RebuildLayout(this.UIText_Price.transform.parent.GetComponent<RectTransform>());
        }

        /// <summary>
        /// Xóa đối tượng
        /// </summary>
        private void Destroy()
        {
            GameObject.Destroy(this.gameObject);
        }
        #endregion

        #region Public methods

        #endregion
    }
}
