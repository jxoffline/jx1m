using UnityEngine;

namespace FS.VLTK.UI.UICore
{
    /// <summary>
    /// Thanh máu của đối tượng trên bản đồ
    /// </summary>
    public class UISpriteHPBar : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Background
        /// </summary>
        [SerializeField]
        private SpriteRenderer UIImage_Background;

        /// <summary>
        /// Thumb
        /// </summary>
        [SerializeField]
        private SpriteRenderer UIImage_Thumb;

        /// <summary>
        /// Kích thước Thumb
        /// </summary>
        [SerializeField]
        private Vector2 _ThumbSize;

        /// <summary>
        /// Kích thước Background
        /// </summary>
        [SerializeField]
        private Vector2 _BackgroundSize;
        #endregion

        #region Properties
        /// <summary>
        /// Màu
        /// </summary>
        public Color Color
        {
            get
            {
                return this.UIImage_Thumb.color;
            }
            set
            {
                this.UIImage_Thumb.color = value;
            }
        }

        private float _Value;
        /// <summary>
        /// Giá trị (0-1)
        /// </summary>
        public float Value
        {
            get
            {
                return this._Value;
            }
            set
            {
                this._Value = value;
                
                /// Nếu đối tượng đang hiển thị
                if (this.gameObject.activeSelf)
                {
                    /// Cập nhật kích thước
                    this.UpdateThumbSize();
                }
            }
        }

        /// <summary>
        /// Kích thước Background
        /// </summary>
        public Vector2 BackgroundSize
        {
            get
            {
                return this._BackgroundSize;
            }
            set
            {
                this._BackgroundSize = value;

                /// Nếu đối tượng đang hiển thị
                if (this.gameObject.activeSelf)
                {
                    /// Cập nhật kích thước
                    this.UpdateBackgroundSize();
                }
            }
        }

        /// <summary>
        /// Kích thước Thumb
        /// </summary>
        public Vector2 ThumbSize
        {
            get
            {
                return this._ThumbSize;
            }
            set
            {
                this._ThumbSize = value;

                /// Nếu đối tượng đang hiển thị
                if (this.gameObject.activeSelf)
                {
                    /// Cập nhật kích thước
                    this.UpdateThumbSize();
                }
            }
        }
        #endregion

        #region Private fields
        /// <summary>
        /// Đã chạy qua hàm Start
        /// </summary>
        private bool isStarted = false;
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            /// Cập nhật kích thước
            this.UpdateThumbSize();
            this.UpdateBackgroundSize();
            /// Đánh dấu đã chạy qua hàm Start
            this.isStarted = true;
        }

        /// <summary>
        /// Hàm này gọi khi đối tượng được kích hoạt
        /// </summary>
        private void OnEnable()
        {
            /// Nếu chưa chạy qua hàm Start
            if (!this.isStarted)
            {
                /// Bỏ qua
                return;
            }
            /// Cập nhật kích thước
            this.UpdateThumbSize();
            this.UpdateBackgroundSize();
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Cập nhật kích thước
        /// </summary>
        private void UpdateThumbSize()
        {
            /// Cập nhật lại theo tỷ lệ
            this.UIImage_Thumb.size = new Vector2(this._ThumbSize.x * this._Value, this._ThumbSize.y);
        }

        /// <summary>
        /// Cập nhật kích thước Background
        /// </summary>
        private void UpdateBackgroundSize()
        {
            this.UIImage_Background.size = this._BackgroundSize;
        }
        #endregion
    }
}
