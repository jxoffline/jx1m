using FS.VLTK.Entities.Config;
using FS.VLTK.UI.Main.ItemBox;
using FS.VLTK.Utilities.UnityUI;
using TMPro;
using UnityEngine;

namespace FS.VLTK.UI.Main.LuckyCircle
{
    /// <summary>
    /// Ô vật phẩm trong vòng quay may mắn
    /// </summary>
    public class UILuckyCircle_Cell : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Thông tin vật phẩm
        /// </summary>
        [SerializeField]
        private UIItemBox UIItem;

        /// <summary>
        /// Text số lượng
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Quantity;

        /// <summary>
        /// Hiệu ứng
        /// </summary>
        [SerializeField]
        private UIAnimatedSprite UIAnimation;

        /// <summary>
        /// Đánh dấu chọn ô
        /// </summary>
        [SerializeField]
        private RectTransform UI_Mark;

        /// <summary>
        /// Danh sách các loại hiệu ứng
        /// </summary>
        [SerializeField]
        private RectTransform[] UIEffects;
        #endregion

        #region Properties
        /// <summary>
        /// ID vật phẩm
        /// </summary>
        public int ItemID
        {
            get
            {
                /// Nếu chưa có
                if (this.UIItem.Data == null)
                {
                    return -1;
                }
                return this.UIItem.Data.GoodsID;
            }
            set
            {
                /// Hủy ô vật phẩm
                this.UIItem.Data = null;
                /// Nếu thông tin vật phẩm không tồn tại
                if (!Loader.Loader.Items.TryGetValue(value, out ItemData itemData))
                {
                    return;
                }
                /// Thông tin vật phẩm tương ứng
                this.UIItem.Data = KTGlobal.CreateItemPreview(itemData);
            }
        }

        private int _Quantity = 1;
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
                this.UIText_Quantity.text = string.Format("SL: ", value);
            }
        }

        /// <summary>
        /// Chọn đối tượng không
        /// </summary>
        public bool Selected
        {
            get
            {
                return this.UI_Mark.gameObject.activeSelf;
            }
            set
            {
                this.UI_Mark.gameObject.SetActive(value);
            }
        }

        private int _EffectType = -1;
        /// <summary>
        /// Loại hiệu ứng
        /// </summary>
        public int EffectType
        {
            get
            {
                return this._EffectType;
            }
            set
            {
                this._EffectType = value;

                /// Duyệt danh sách hiệu ứng và hủy
                foreach (RectTransform uiEffect in this.UIEffects)
                {
                    uiEffect.gameObject.SetActive(false);
                }
                /// Nếu tồn tại
                if (value >= 0 && value < this.UIEffects.Length)
                {
                    /// Kích hoạt hiệu ứng tương ứng
                    this.UIEffects[value].gameObject.SetActive(true);
                }
            }
        }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            /// Duyệt danh sách hiệu ứng và hủy
            foreach (RectTransform uiEffect in this.UIEffects)
            {
                uiEffect.gameObject.SetActive(false);
            }
            this.UIAnimation.gameObject.SetActive(false);
            this.UI_Mark.gameObject.SetActive(false);
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
            this.UIItem.Click = () => {
                /// Toác
                if (this.UIItem.Data == null)
                {
                    return;
                }
                /// Hiện thông tin vật phẩm
                KTGlobal.ShowItemInfo(this.UIItem.Data);
            };

            this.UIAnimation.OnStop = () =>
            {
                this.UIAnimation.gameObject.SetActive(false);
            };
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Thực thi hiệu ứng
        /// </summary>
        public void PlayAnimation()
        {
            this.UIAnimation.gameObject.SetActive(true);
            this.UIAnimation.Play();
        }
        #endregion
    }
}
