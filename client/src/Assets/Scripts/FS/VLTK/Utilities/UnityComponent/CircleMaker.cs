using UnityEngine;

namespace FS.VLTK.Utilities.UnityComponent
{
    /// <summary>
    /// Tạo ra hình tròn ở vị trí chỉ định
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    [ExecuteAlways]
    public class CircleMaker : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Giá trị theta
        /// </summary>
        [Tooltip("Theta ảnh hưởng đến số điểm sẽ vẽ ra tạo thành đường tròn, theta càng nhỏ thì số điểm càng nhiều. Số điểm càng nhiều sẽ càng đẹp nhưng sẽ ảnh hưởng đến hiệu năng.")]
        [SerializeField]
        private float _ThetaScale = 0.01f;

        /// <summary>
        /// Bán kính vòng tròn
        /// </summary>
        [Tooltip("Bán kính vòng tròn")]
        [SerializeField]
        private float _Radius = 10f;

        /// <summary>
        /// Độ dày đường tròn
        /// </summary>
        [Tooltip("Độ dày đường tròn")]
        [SerializeField]
        private float _LineWidth = 1f;

#if UNITY_EDITOR
        /// <summary>
        /// Vẽ ngay lập tức
        /// </summary>
        [Tooltip("Vẽ ngay lập tức (chỉ có tác dụng trên Editor)")]
        [SerializeField]
        private bool _DrawNow;
#endif
        #endregion

        #region Constants
        /// <summary>
        /// Hằng số kích thước điểm (để tính toán ra số điểm cùng với Theta)
        /// </summary>
        private const float PointSize = 2f;
        #endregion

        #region Properties
        /// <summary>
        /// Giá trị theta
        /// </summary>
        public float ThetaScale
        {
            get
            {
                return this._ThetaScale;
            }
            set
            {
                this._ThetaScale = value;
            }
        }

        /// <summary>
        /// Bán kính vòng tròn
        /// </summary>
        public float Radius
        {
            get
            {
                return this._Radius;
            }
            set
            {
                this._Radius = value;
            }
        }

        /// <summary>
        /// Độ dày đường tròn
        /// </summary>
        public float LineWidth
        {
            get
            {
                return this._LineWidth;
            }
            set
            {
                this._LineWidth = value;
            }
        }
        #endregion

        #region Private fields
        /// <summary>
        /// Thành phần LineRenderer
        /// </summary>
        private LineRenderer lineRenderer;
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            this.lineRenderer = this.GetComponent<LineRenderer>();
        }

#if UNITY_EDITOR
        /// <summary>
        /// Hàm này gọi liên tục mỗi Frame
        /// </summary>
        private void Update()
        {
            /// Nếu có yêu cầu vẽ
            if (this._DrawNow)
            {
                /// Hủy yêu cầu
                this._DrawNow = false;
                /// Thực hiện vẽ
                this.Draw();
            }
        }
#endif
        #endregion

        #region Private methods
        /// <summary>
        /// Thực hiện vẽ đường tròn
        /// </summary>
        private void Draw()
        {
            /// Tổng số điểm
            int totalPoints = (int) ((CircleMaker.PointSize * Mathf.PI) / this._ThetaScale);
            /// Gắn vào
            this.lineRenderer.startWidth = this._LineWidth;
            this.lineRenderer.endWidth = this._LineWidth;
            this.lineRenderer.positionCount = totalPoints;

            /// Giá trị theta ban đầu
            float theta = 0;
            /// Duyệt tổng số điểm
            for (int i = 0; i < totalPoints; i++)
            {
                /// Giá trị theta
                theta += (CircleMaker.PointSize * Mathf.PI * this._ThetaScale);
                /// Vị trí X, Y
                float x = this._Radius * Mathf.Cos(theta);
                float y = this._Radius * Mathf.Sin(theta);
                /// Cộng thêm vị trí hiện tại
                x += this.transform.position.x;
                y += this.transform.position.y;
                /// Gắn vị trí vào
                this.lineRenderer.SetPosition(i, new Vector3(x, y, 0));
            }
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Thực hiện vẽ đường tròn (cần gọi đến khi có sự thay đổi vị trí)
        /// </summary>
        public void Render()
        {
            this.Draw();
        }
        #endregion
    }
}