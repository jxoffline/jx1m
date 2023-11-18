using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FS.VLTK.Utilities.UnityComponent
{
    /// <summary>
    /// Dịch chuyển đối tượng
    /// </summary>
    public class SpriteTransition : TTMonoBehaviour
    {
        #region Define
        /// <summary>
        /// Bắt đầu
        /// </summary>
        [SerializeField]
        private Vector2 _From;

        /// <summary>
        /// Kết thúc
        /// </summary>
        [SerializeField]
        private Vector2 _To;

        /// <summary>
        /// Gia tốc
        /// </summary>
        [SerializeField]
        private float _Acceleration;

        /// <summary>
        /// Vận tốc ban đầu
        /// </summary>
        [SerializeField]
        private float _Velocity;

        /// <summary>
        /// Tự thực thi
        /// </summary>
        [SerializeField]
        private bool _AutoPlay;

        /// <summary>
        /// Thực thi pha ngược không
        /// </summary>
        [SerializeField]
        private bool _Reverse;

        /// <summary>
        /// Lặp đi lặp lại không
        /// </summary>
        [SerializeField]
        private bool _Loop;
        #endregion

        #region Properties
        /// <summary>
        /// Sự kiện khi đối tượng hoàn tất di chuyển
        /// </summary>
        public Action Done { get; set; }

        /// <summary>
        /// Gia tốc
        /// </summary>
        public float Acceleration
        {
            get
            {
                return this._Acceleration;
            }
            set
            {
                this._Acceleration = value;
            }
        }

        /// <summary>
        /// Vận tốc ban đầu
        /// </summary>
        public float Velocity
        {
            get
            {
                return this._Velocity;
            }
            set
            {
                this._Velocity = value;
            }
        }

        /// <summary>
        /// Vị trí ban đầu
        /// </summary>
        public Vector2 FromPos
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
        /// Vị trí đích
        /// </summary>
        public Vector2 ToPos
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
        /// Luồng thực thi hiệu ứng
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
        /// Thực thi hiệu ứng di chuyển
        /// </summary>
        /// <param name="fromPos"></param>
        /// <param name="toPos"></param>
        /// <returns></returns>
        private IEnumerator DoTransition(Vector2 fromPos, Vector2 toPos)
        {
            /// Vector hướng
            Vector2 dirVector = toPos - fromPos;
            /// Quãng đường cần di chuyển
            float distance = Vector2.Distance(fromPos, toPos);
            /// Thời gian di chuyển
            float duration;
            /// Nếu có gia tốc
            if (this._Acceleration != 0)
            {
                /// t = (-v0 + Sqrt(v0 ^ 2 + as)) / a
                duration = (-this._Velocity + Mathf.Sqrt(this._Velocity * this._Velocity + 2 * this._Acceleration * distance)) / this._Acceleration;
            }
            else
            {
                /// t = s / v
                duration = distance / this._Velocity;
            }

            /// Thiết lập vị trí ban đầu
            this.transform.localPosition = fromPos;
            /// Bỏ qua Frame
            yield return null;

            /// Thời gian tồn tại
            float lifeTime = 0f;
            /// Lặp liên tục chừng nào còn tồn tại
            while (lifeTime < duration)
            {
                /// Tăng thời gian
                lifeTime += Time.deltaTime;
                /// Quãng đường mới (s = v0 * t + 1/2 * a * t^2)
                float newDistance = this._Velocity * lifeTime + 0.5f * this._Acceleration * lifeTime * lifeTime;
                /// Tọa độ mới
                Vector2 newPos = KTMath.FindPointInVectorWithDistance(fromPos, dirVector, newDistance);
                /// Cập nhật vị trí mới
                this.transform.localPosition = newPos;
                /// Bỏ qua Frame
                yield return null;
            }

            /// Thiết lập vị trí đích
            this.transform.localPosition = toPos;
            /// Bỏ qua Frame
            yield return null;
        }

        /// <summary>
        /// Thực thi hiệu ứng
        /// </summary>
        /// <returns></returns>
        private IEnumerator DoPlay()
        {
            /// Lặp liên tục
            while (true)
            {
                /// Thực thi pha thuận trước
                yield return this.DoTransition(this._From, this._To);
                /// Nếu có thực hiện pha nghịch
                if (this._Reverse)
                {
                    /// Thực thi pha nghịch
                    yield return this.DoTransition(this._To, this._From);
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

            /// Thực thi sự kiện di chuyển hoàn tất
            this.Done?.Invoke();
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
