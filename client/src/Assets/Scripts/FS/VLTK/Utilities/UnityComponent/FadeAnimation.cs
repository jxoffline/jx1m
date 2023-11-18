using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FS.VLTK.Utilities.UnityComponent
{
    /// <summary>
    /// Thực hiện hiệu ứng ẩn / hiện đối tượng
    /// </summary>
    public class FadeAnimation : MonoBehaviour
    {
        /// <summary>
        /// Loại hiệu ứng
        /// </summary>
        public enum FadeType
        {
            /// <summary>
            /// Hiện
            /// </summary>
            FadeIn = 0,
            /// <summary>
            /// Ẩn
            /// </summary>
            FadeOut = 1,
            /// <summary>
            /// Hiện rồi ẩn
            /// </summary>
            FadeInAndFadeOut = 2,
        }

        #region Define
        /// <summary>
        /// Thời gian thực thi hiệu ứng
        /// </summary>
        [SerializeField]
        private float _Duration;

        /// <summary>
        /// Có lặp lại liên tục không
        /// </summary>
        [SerializeField]
        private bool _Repeat = false;

        /// <summary>
        /// Loại hiệu ứng
        /// </summary>
        [SerializeField]
        private FadeType _Type;
        #endregion

        #region Private fields
        /// <summary>
        /// Đối tượng Sprite Renderer
        /// </summary>
        private SpriteRenderer spriteRenderer = null;

        /// <summary>
        /// Đối tượng UnityEngine.UI.Image
        /// </summary>
        private UnityEngine.UI.Image uiImage = null;
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            this.spriteRenderer = this.GetComponent<SpriteRenderer>();
            this.uiImage = this.GetComponent<UnityEngine.UI.Image>();
        }

        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            
        }

        /// <summary>
        /// Hàm này gọi khi đối tượng được kích hoạt
        /// </summary>
        private void OnEnable()
        {
            /// Loại hiệu ứng
            switch (this._Type)
            {
                /// Chỉ có hiện
                case FadeType.FadeIn:
                {
                    this.FadeIn();
                    break;
                }
                /// Chỉ có ẩn
                case FadeType.FadeOut:
                {
                    this.FadeOut();
                    break;
                }
                /// Nếu là cả ẩn và hiện
                case FadeType.FadeInAndFadeOut:
                {
                    this.FadeInAndFadeOut();
                    break;
                }
            }
        }

        /// <summary>
        /// Hàm này gọi khi đối tượng bị hủy kích hoạt
        /// </summary>
        private void OnDisable()
        {
            this.StopAllCoroutines();
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Thực hiện hiệu ứng Fade
        /// </summary>
        /// <param name="fromAlpha"></param>
        /// <param name="toAlpha"></param>
        /// <param name="repeat"></param>
        private IEnumerator DoFade(float fromAlpha, float toAlpha, bool repeat)
        {
            /// Nhãn BEGIN
            BEGIN:
            {
                float lifeTime = 0f;
                /// Thiết lập Alpha ban đầu = fromAlpha
                if (this.spriteRenderer != null)
                {
                    Color color = this.spriteRenderer.color;
                    color.a = fromAlpha;
                    this.spriteRenderer.color = color;
                }
                else if (this.uiImage != null)
                {
                    Color color = this.uiImage.color;
                    color.a = fromAlpha;
                    this.uiImage.color = color;
                }
                yield return null;

                /// Lặp liên tục để thực hiện tăng hoặc giảm Alpha tương ứng theo Vector đầu vào
                while (true)
                {
                    lifeTime += Time.deltaTime;
                    if (lifeTime >= this._Duration)
                    {
                        break;
                    }

                    float percent = lifeTime / this._Duration;

                    /// Tính Alpha mới dựa theo công thức dịch Vector
                    float newAlpha = fromAlpha + (toAlpha - fromAlpha) * percent;

                    /// Thiết lập Alpha cho đối tượng tương ứng
                    if (this.spriteRenderer != null)
                    {
                        Color color = this.spriteRenderer.color;
                        color.a = newAlpha;
                        this.spriteRenderer.color = color;
                    }
                    else if (this.uiImage != null)
                    {
                        Color color = this.uiImage.color;
                        color.a = newAlpha;
                        this.uiImage.color = color;
                    }

                    yield return null;
                }

                /// Thiết lập Alpha cuối = toAlpha
                if (this.spriteRenderer != null)
                {
                    Color color = this.spriteRenderer.color;
                    color.a = toAlpha;
                    this.spriteRenderer.color = color;
                }
                else if (this.uiImage != null)
                {
                    Color color = this.uiImage.color;
                    color.a = toAlpha;
                    this.uiImage.color = color;
                }
                yield return null;
            }

            /// Nếu lặp
            if (repeat)
            {
                /// Quay về đoạn code có nhãn BEGIN
                goto BEGIN;
            }
        }

        /// <summary>
        /// Luồng thực thi hiệu ứng ẩn và hiện liên tục
        /// </summary>
        /// <param name="repeat"></param>
        /// <returns></returns>
        private IEnumerator DoFadeInAndFadeOut(bool repeat)
        {
            /// Nhãn LOOP
            LOOP:
            {
                yield return this.DoFade(0f, 1f, false);
                yield return this.DoFade(1f, 0f, false);
            }

            /// Nếu lặp
            if (repeat)
            {
                /// Quay về đoạn code có nhãn BEGIN
                goto LOOP;
            }
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Thực hiện hiệu ứng hiện lên
        /// </summary>
        public void FadeIn()
        {
            this.StopAllCoroutines();
            this.StartCoroutine(this.DoFade(0f, 1f, this._Repeat));
        }

        /// <summary>
        /// Thực hiện hiệu ứng ẩn đi
        /// </summary>
        public void FadeOut()
        {
            this.StopAllCoroutines();
            this.StartCoroutine(this.DoFade(1f, 0f, this._Repeat));
        }

        /// <summary>
        /// Thực hiện hiệu ứng ẩn và hiện liên tục
        /// </summary>
        public void FadeInAndFadeOut()
        {
            this.StopAllCoroutines();
            this.StartCoroutine(this.DoFadeInAndFadeOut(this._Repeat));
        }
        #endregion
    }
}
