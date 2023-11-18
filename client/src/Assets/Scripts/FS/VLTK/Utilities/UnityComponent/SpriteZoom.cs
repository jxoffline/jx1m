using System.Collections;
using UnityEngine;

namespace FS.VLTK.Utilities.UnityComponent
{
    /// <summary>
    /// Hiệu ứng thu phóng
    /// </summary>
    public class SpriteZoom : TTMonoBehaviour
    {
        #region Define
        /// <summary>
        /// Bắt đầu
        /// </summary>
        [SerializeField]
        private Vector2 _From = new Vector2(1, 1);

        /// <summary>
        /// Kết thúc
        /// </summary>
        [SerializeField]
        private Vector2 _To = new Vector2(2, 2);

        /// <summary>
        /// Thời gian thực hiện hiệu ứng
        /// </summary>
        [SerializeField]
        private float _Duration = 1f;

        /// <summary>
        /// Tự thực thi
        /// </summary>
        [SerializeField]
        private bool _AutoPlay = true;

        /// <summary>
        /// Có thực hiện pha ngược không
        /// </summary>
        [SerializeField]
        private bool _Reverse = true;

        /// <summary>
        /// Lặp lại không
        /// </summary>
        [SerializeField]
        private bool _Loop = true;
        #endregion

        #region Private fields
        /// <summary>
        /// Luồng thực thi hiệu ứng
        /// </summary>
        private Coroutine animationCoroutine;
        #endregion

        #region Properties
        /// <summary>
        /// Vector vị trí bắt đầu
        /// </summary>
        public Vector3 From
        {
            get
            {
                return this._From;
            }
            set
            {
                this._From = value;
            }
        }

        /// <summary>
        /// Vị trí kết thúc
        /// </summary>
        public Vector3 To
        {
            get
            {
                return this._To;
            }
            set
            {
                this._To = value;
            }
        }

        /// <summary>
        /// Thời gian thực thi
        /// </summary>
        public float Duration
        {
            get
            {
                return this._Duration;
            }
            set
            {
                this._Duration = value;
            }
        }

        /// <summary>
        /// Có thực hiện pha ngược không
        /// </summary>
        public bool Reverse
        {
            get
            {
                return this._Reverse;
            }
            set
            {
                this._Reverse = value;
            }
        }

        /// <summary>
        /// Lặp đi lặp lại không
        /// </summary>
        public bool Loop
        {
            get
            {
                return this._Loop;
            }
            set
            {
                this._Loop = value;
            }
        }

        /// <summary>
        /// Tự phát không
        /// </summary>
        public bool AutoPlay
        {
            get
            {
                return this._AutoPlay;
            }
            set
            {
                this._AutoPlay = value;
            }
        }

        /// <summary>
        /// Có đang thực thi hiệu ứng không
        /// </summary>
        public bool IsPlaying
        {
            get
            {
                return this.playAnimationCoroutine != null;
            }
        }
        #endregion

        #region Private fields
        /// <summary>
        /// Luồng thực thi Animation
        /// </summary>
        private Coroutine playAnimationCoroutine;

        /// <summary>
        /// Đã chạy qua hàm Start chưa
        /// </summary>
        private bool isStarted = false;
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            /// Nếu đánh dấu tự phát
            if (this._AutoPlay)
            {
                /// Bắt đầu hiệu ứng
                this.Play();
            }

            /// Đánh dấu đã chạy qua hàm Start
            this.isStarted = true;
        }

        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void OnEnable()
        {
            /// Nếu chưa chạy qua hàm Start
            if (!this.isStarted)
            {
                /// Bỏ qua
                return;
            }

            /// Nếu đánh dấu tự phát
            if (this._AutoPlay)
            {
                /// Bắt đầu hiệu ứng
                this.Play();
            }
        }

        /// <summary>
        /// Hàm này gọi khi đối tượng bị hủy
        /// </summary>
        private void OnDisable()
        {
            /// Ngừng hiệu ứng
            this.Stop();
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Thực thi hiệu ứng thu phóng
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        private IEnumerator DoPlayAnimation(Vector2 from, Vector2 to)
        {
            /// Vector hướng
            Vector2 dirVector = to - from;
            /// Thiết lập vị trí ban đầu
            this.transform.localScale = from;
            /// Bỏ qua Frame
            yield return null;

            /// Thời gian tồn tại
            float lifeTime = 0f;
            /// Lặp liên tục chừng nào còn tồn tại
            while (lifeTime < this._Duration)
            {
                /// Cập nhật % tiến độ
                float percent = lifeTime / this._Duration;
                /// Vị trí mới
                Vector2 newPos = from + dirVector * percent;
                /// Cập nhật vị trí mới
                this.transform.localScale = newPos;
                /// Bỏ qua Frame
                yield return null;
                /// Tăng thời gian
                lifeTime += Time.deltaTime;
                /// Nếu đã quá thời gian thì thoát
                if (lifeTime > this._Duration)
                {
                    break;
                }
            }

            /// Thiết lập vị trí đích
            this.transform.localScale = to;
            /// Bỏ qua Frame
            yield return null;
        }

        /// <summary>
        /// Luồng thực thi hiệu ứng
        /// </summary>
        /// <returns></returns>
        private IEnumerator DoPlay()
        {
            /// Lặp liên tục
            while (true)
            {
                /// Thực thi pha thuận trước
                yield return this.DoPlayAnimation(this._From, this._To);
                /// Nếu có thực hiện pha nghịch
                if (this._Reverse)
                {
                    /// Thực thi pha nghịch
                    yield return this.DoPlayAnimation(this._To, this._From);
                }

                /// Nếu không lặp lại
                if (!this._Loop)
                {
                    /// Thoát
                    break;
                }
            }
            /// Hủy luồng
            this.playAnimationCoroutine = null;
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Thực thi hiệu ứng
        /// </summary>
        public void Play()
        {
            /// Ngừng hiệu ứng trước
            this.Stop();
            /// Bắt đầu thực hiện hiệu ứng
            this.playAnimationCoroutine = this.StartCoroutine(this.DoPlay());
        }

        /// <summary>
        /// Ngừng hiệu ứng
        /// </summary>
        public void Stop()
        {
            /// Nếu đang có luồng thực thi hiệu ứng
            if (this.playAnimationCoroutine != null)
            {
                /// Ngừng luồng
                this.StopCoroutine(this.playAnimationCoroutine);
                /// Hủy tham chiếu
                this.playAnimationCoroutine = null;
            }
        }
        #endregion
    }
}
