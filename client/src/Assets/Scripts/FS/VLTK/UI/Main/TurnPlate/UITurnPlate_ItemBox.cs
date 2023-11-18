using FS.VLTK.Entities.Config;
using FS.VLTK.Utilities.UnityUI;
using Server.Data;
using TMPro;
using UnityEngine;

namespace FS.VLTK.UI.Main.TurnPlate
{
    /// <summary>
    /// Ô vật phẩm trong khung vòng quay may mắn đặc biệt
    /// </summary>
    public class UITurnPlate_ItemBox : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Button
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton;

        /// <summary>
        /// Image Icon vật phẩm
        /// </summary>
        [SerializeField]
        private SpriteFromAssetBundle UIImage_Icon;

        /// <summary>
        /// Hiệu ứng kích hoạt
        /// </summary>
        [SerializeField]
        private UIAnimatedSprite UIAnimation;

        /// <summary>
        /// Text số lượng
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Quantity;

        /// <summary>
        /// Hiệu ứng kích hoạt ô
        /// </summary>
        [SerializeField]
        private RectTransform UIImage_Highlight;
        #endregion

        #region Properties
        private int _ItemID;
        /// <summary>
        /// ID vật phẩm
        /// </summary>
        public int ItemID
        {
            get
            {
                return this._ItemID;
            }
            set
            {
                this._ItemID = value;
                /// Thông tin vật phẩm
                if (!Loader.Loader.Items.TryGetValue(value, out ItemData itemData))
                {
                    /// Không tìm thấy thì bỏ qua
                    return;
                }

                /// Thiết lập
                this.UIImage_Icon.BundleDir = itemData.IconBundleDir;
                this.UIImage_Icon.AtlasName = itemData.IconAtlasName;
                this.UIImage_Icon.SpriteName = itemData.Icon;
                this.UIImage_Icon.Load();
            }
        }

        private int _Quantity;
        /// <summary>
        /// Số lượng
        /// </summary>
        public int Quantity
        {
            get
            {
                return this._Quantity;
            }
            set
            {
                this._Quantity = value;
                /// Thiết lập Text
                this.UIText_Quantity.text = string.Format("SL: {0}", value);
            }
        }

        /// <summary>
        /// Kích hoạt đối tượng
        /// </summary>
        public bool Active
        {
            get
            {
                return this.UIImage_Highlight.gameObject.activeSelf;
            }
            set
            {
                this.UIImage_Highlight.gameObject.SetActive(value);
            }
        }
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
            this.UIButton.onClick.AddListener(this.Button_Clicked);
            this.UIAnimation.OnStop = () =>
            {
                this.UIAnimation.gameObject.SetActive(false);
            };
        }

        /// <summary>
        /// Sự kiện khi Button được ấn
        /// </summary>
        private void Button_Clicked()
        {
            /// Thông tin vật phẩm
            if (!Loader.Loader.Items.TryGetValue(this._ItemID, out ItemData itemData))
            {
                /// Không tìm thấy thì bỏ qua
                return;
            }

            /// Xem trước vật phẩm
            GoodsData itemGD = KTGlobal.CreateItemPreview(itemData);
            /// Hiện Tooltip
            KTGlobal.ShowItemInfo(itemGD);
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Sáng đối tượng
        /// </summary>
        public void Highlight()
        {
            this.UIAnimation.gameObject.SetActive(true);
            this.UIAnimation.Play();
        }
        #endregion
    }
}
