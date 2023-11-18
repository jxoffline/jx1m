using System;
using System.Collections;
using UnityEngine;

namespace FS.VLTK.Utilities.UnityComponent
{
    /// <summary>
    /// Hiệu ứng Glow của Sprite
    /// </summary>
    [RequireComponent(typeof(SpriteGlow.SpriteGlowEffect))]
    public class SpriteGlowAnimation : TTMonoBehaviour
    {
        #region Define
        /// <summary>
        /// Alpha tối thiểu
        /// </summary>
        [SerializeField]
        private float _MinAlpha = 0f;

        /// <summary>
        /// Alpha tối đa
        /// </summary>
        [SerializeField]
        private float _MaxAlpha = 1f;

        /// <summary>
        /// Tốc độ play hiệu ứng
        /// </summary>
        [SerializeField]
        private float _AnimationSpeed = 1f;

        /// <summary>
        /// Có pha ngược lại không
        /// </summary>
        [SerializeField]
        private bool _Reverse = true;

        /// <summary>
        /// Lặp lại không
        /// </summary>
        [SerializeField]
        private bool _Loop = true;

        /// <summary>
        /// Tự phát không
        /// </summary>
        [SerializeField]
        private bool _AutoPlay = true;
        #endregion

        #region Properties
        /// <summary>
        /// Alpha tối thiểu
        /// </summary>
        public float MinAlpha
        {
            get
            {
                return this._MinAlpha;
            }
            set
            {
                this._MinAlpha = value;
            }
        }

        /// <summary>
        /// Alpha tối đa
        /// </summary>
        public float MaxAlpha
        {
            get
            {
                return this._MaxAlpha;
            }
            set
            {
                this._MaxAlpha = value;
            }
        }

        /// <summary>
        /// Tốc độ phát hiệu ứng
        /// </summary>
        public float AnimationSpeed
        {
            get
            {
                return this._AnimationSpeed;
            }
            set
            {
                this._AnimationSpeed = value;
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
        /// Component Sprite Glow
        /// </summary>
        private SpriteGlow.SpriteGlowEffect SpriteGlowEffect;

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
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            this.SpriteGlowEffect = this.GetComponent<SpriteGlow.SpriteGlowEffect>();
        }

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
        /// Luồng thực thi hiệu ứng
        /// </summary>
        /// <param name="fromAlpha"></param>
        /// <param name="toAlpha"></param>
        /// <returns></returns>
        private IEnumerator DoPlayAnimation(float fromAlpha, float toAlpha)
        {
            /// Thời gian tồn tại
            float lifeTime = 0;

            /// Lặp liên tục
            while (true)
            {
                /// % tương ứng
                float percent = lifeTime / this._AnimationSpeed;
                /// Alpha mới
                float newAlpha = fromAlpha + (toAlpha - fromAlpha) * percent;
                /// Thiết lập Alpha
                Color color = this.SpriteGlowEffect.GlowColor;
                color.a = newAlpha;
                this.SpriteGlowEffect.GlowColor = color;

                /// Bỏ qua Frame
                yield return null;
                /// Tăng thời gian tồn tại
                lifeTime += Time.deltaTime;
                /// Nếu đã quá thời gian thì thoát
                if (lifeTime > this._AnimationSpeed)
                {
                    break;
                }
            }

            /// Thiết lập Alpha
            Color _color = this.SpriteGlowEffect.GlowColor;
            _color.a = toAlpha;
            this.SpriteGlowEffect.GlowColor = _color;
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
                yield return this.DoPlayAnimation(this._MinAlpha, this._MaxAlpha);
                /// Nếu có thực hiện pha nghịch
                if (this._Reverse)
                {
                    /// Thực thi pha nghịch
                    yield return this.DoPlayAnimation(this._MaxAlpha, this._MinAlpha);
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
