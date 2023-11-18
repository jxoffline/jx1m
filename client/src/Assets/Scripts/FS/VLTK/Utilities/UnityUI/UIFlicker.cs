using UnityEngine;
using TMPro;
using System.Collections;

namespace FS.VLTK.Utilities.UnityUI
{
    /// <summary>
    /// Đối tượng nhấp nháy
    /// </summary>
    public class UIFlicker : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Thời gian thực thi hiệu ứng
        /// </summary>
        [SerializeField]
        private float _Duration = 2f;

        /// <summary>
        /// Tự thực thi khi đối tượng được kích hoạt
        /// </summary>
        [SerializeField]
        private bool _AutoPlay = true;

        /// <summary>
        /// Giá trị Alpha min
        /// </summary>
        [SerializeField]
        private float _MinAlpha = 0f;

        /// <summary>
        /// Giá trị Alpha max
        /// </summary>
        [SerializeField]
        private float _MaxAlpha = 1f;
        #endregion

        #region Private fields
        /// <summary>
        /// Thành phần Text
        /// </summary>
        private TextMeshProUGUI UIText = null;

        /// <summary>
        /// Thành phần Image
        /// </summary>
        private UnityEngine.UI.Image UIImage = null;

        /// <summary>
        /// Thành phần Sprite Renderer
        /// </summary>
        private SpriteRenderer SpriteRenderer = null;

        /// <summary>
        /// Luồng thực thi hiệu ứng
        /// </summary>
        private Coroutine animationCoroutine = null;
        #endregion

        #region Properties
        /// <summary>
        /// Thời gian thực thi hiệu ứng
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
        /// Tự thực thi khi đối tượng được kích hoạt
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
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            this.UIText = this.GetComponent<TextMeshProUGUI>();
            this.UIImage = this.GetComponent<UnityEngine.UI.Image>();
            this.SpriteRenderer = this.GetComponent<SpriteRenderer>();
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
            if (this._AutoPlay)
            {
                this.Play();
            }
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Đổi alpha của thành phần tương ứng
        /// </summary>
        /// <param name="alpha"></param>
        private void ChangeAlpha(float alpha)
        {
            /// Nếu tồn tại thành phần Text
            if (this.UIText != null)
            {
                Color color = this.UIText.color;
                color.a = alpha;
                this.UIText.color = color;
            }

            /// Nếu tồn tại thành phần Image
            if (this.UIImage != null)
            {
                Color color = this.UIImage.color;
                color.a = alpha;
                this.UIImage.color = color;
            }

            /// Nếu tồn tại thành phần SpriteRenderer
            if (this.SpriteRenderer != null)
            {
                Color color = this.SpriteRenderer.color;
                color.a = alpha;
                this.SpriteRenderer.color = color;
            }
        }

        /// <summary>
        /// Thực thi bước nhỏ hiệu ứng từ fromAlpha đến toAlpha
        /// </summary>
        /// <param name="fromAlpha"></param>
        /// <param name="toAlpha"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        private IEnumerator DoAnimationStep(float fromAlpha, float toAlpha, float duration)
        {
            /// Thời gian tồn tại
            float lifeTime = 0f;
            /// Đổi về fromAlpha
            this.ChangeAlpha(fromAlpha);
            /// Bỏ qua 1 Frame
            yield return null;
            /// Lặp liên tục đến hết thời gian thực thi
            while (lifeTime < duration)
            {
                /// Tăng thời gian tồn tại lên
                lifeTime += Time.deltaTime;
                /// Tỷ lệ % tương ứng thời gian đã qua
                float percent = lifeTime / duration;
                /// Giá trị Alpha đích
                float newAlpha = fromAlpha + (toAlpha - fromAlpha) * percent;
                /// Đổi Alpha
                this.ChangeAlpha(newAlpha);
                /// Bỏ qua 1 Frame
                yield return null;
            }
            /// Đổi về toAlpha
            this.ChangeAlpha(toAlpha);
            /// Bỏ qua 1 Frame
            yield return null;
        }

        /// <summary>
        /// Thực thi hiệu ứng
        /// </summary>
        /// <returns></returns>
        private IEnumerator DoAnimation()
        {
            while (true)
            {
                /// Thực hiện Fade-In
                yield return this.DoAnimationStep(this._MinAlpha, this._MaxAlpha, this._Duration / 2);
                /// Thực hiện Fade-Out
                yield return this.DoAnimationStep(this._MaxAlpha, this._MinAlpha, this._Duration / 2);
            }
            /// Hủy luồng thực thi
            this.animationCoroutine = null;
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Bắt đầu thực thi hiệu ứng
        /// </summary>
        public void Play()
        {
            /// Nếu đang thực thi hiệu ứng
            if (this.animationCoroutine != null)
            {
                this.StopCoroutine(this.animationCoroutine);
            }
            this.animationCoroutine = this.StartCoroutine(this.DoAnimation());
        }

        /// <summary>
        /// Ngừng hiệu ứng
        /// </summary>
        public void Stop()
        {
            /// Nếu đang thực thi hiệu ứng
            if (this.animationCoroutine != null)
            {
                this.StopCoroutine(this.animationCoroutine);
            }
        }

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
        #endregion
    }
}
