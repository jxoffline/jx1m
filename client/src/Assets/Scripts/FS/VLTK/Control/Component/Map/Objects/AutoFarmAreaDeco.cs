using FS.VLTK.Utilities.UnityComponent;
using FS.VLTK.Utilities.UnityUI;
using UnityEngine;

namespace FS.VLTK.Control.Component
{
    /// <summary>
    /// Quản lý biểu diễn vị trí đặt Auto
    /// </summary>
    public class AutoFarmAreaDeco : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Đối tượng vẽ hình tròn
        /// </summary>
        [SerializeField]
        private CircleMaker _Maker;

        /// <summary>
        /// Ảnh iện tích vùng quét
        /// </summary>
        [SerializeField]
        private SpriteRenderer _Square;
        #endregion

        #region Properties
        /// <summary>
        /// Bán kính
        /// </summary>
        public float Radius
        {
            get
            {
                return this._Maker.Radius;
            }
            set
            {
                this._Maker.Radius = value;
            }
        }

        /// <summary>
        /// Vị trí
        /// </summary>
        public Vector2 Position
        {
            get
            {
                return this.transform.localPosition;
            }
            set
            {
                this.transform.localPosition = value;
            }
        }
        #endregion

        #region Private fields
        /// <summary>
        /// Đánh dấu đã chạy qua hàm Start chưa
        /// </summary>
        private bool isStarted = false;
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            /// Làm mới đối tượng
            this.Refresh();
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
            /// Làm mới đối tượng
            this.Refresh();
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Hiện đối tượng
        /// </summary>
        public void Show()
        {
            this.gameObject.SetActive(true);
            
        }

        /// <summary>
        /// Ẩn đối tượng
        /// </summary>
        public void Hide()
        {
            this.gameObject.SetActive(false);
        }

        /// <summary>
        /// Làm mới đối tượng
        /// </summary>
        public void Refresh()
        {
            /// Vẽ đường tròn
            this._Maker.Render();
            /// Cập nhật ảnh diện tích
            this._Square.drawMode = SpriteDrawMode.Sliced;
            this._Square.size = new Vector2(this.Radius * 2, this.Radius * 2);
        }
        #endregion
    }
}
