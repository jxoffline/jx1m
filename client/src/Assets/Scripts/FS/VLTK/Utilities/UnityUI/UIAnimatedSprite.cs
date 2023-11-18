using FS.VLTK.Utilities.UnityUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FS.VLTK.Utilities.UnityUI
{
    /// <summary>
    /// Hiệu ứng động chung Bundle và Atlas
    /// </summary>
    public class UIAnimatedSprite : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Đối tượng Image
        /// </summary>
        [SerializeField]
        private SpriteFromAssetBundle UIImage;

        /// <summary>
        /// Tốc độ thực thi
        /// </summary>
        [SerializeField]
        private float _Duration = 1f;

        /// <summary>
        /// Tỷ lệ thu phóng
        /// </summary>
        [SerializeField]
        private float _Scale = 1f;

        /// <summary>
        /// Khớp kích thước gốc
        /// </summary>
        [SerializeField]
        private bool _PixelPerfect = true;

        /// <summary>
        /// Đường dẫn Bundle chứa ảnh
        /// </summary>
        [SerializeField]
        private string _BundleDir;

        /// <summary>
        /// Tên Atlas chứa ảnh
        /// </summary>
        [SerializeField]
        private string _AtlasName;

        /// <summary>
        /// Danh sách Sprite
        /// </summary>
        [SerializeField]
        private string[] _SpriteNames;

        /// <summary>
        /// Tự thực thi
        /// </summary>
        [SerializeField]
        private bool _AutoPlay = true;

        /// <summary>
        /// Có lặp lại không
        /// </summary>
        [SerializeField]
        private bool _Loop = true;
        #endregion

        #region Private fields
        /// <summary>
        /// Luồng thực thi hiệu ứng
        /// </summary>
        private Coroutine animationCoroutine = null;

        /// <summary>
        /// Đối tượng UnityEngine.UI.Image
        /// </summary>
        private UnityEngine.UI.Image uiImage;

        /// <summary>
        /// Đối tượng SpriteRenderer
        /// </summary>
        private SpriteRenderer spriteRenderer;
        #endregion

        #region Properties
        /// <summary>
        /// Tốc độ thực thi
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
        /// Độ thu phóng
        /// </summary>
        public float Scale
        {
            get
            {
                return this._Scale;
            }
            set
            {
                this._Scale = value;
            }
        }

        /// <summary>
        /// Khớp kích thước ảnh gốc
        /// </summary>
        public bool PixelPerfect
        {
            get
            {
                return this._PixelPerfect;
            }
            set
            {
                this._PixelPerfect = value;
            }
        }

        /// <summary>
        /// Đường dẫn file Bundle chứa ảnh
        /// </summary>
        public string BundleDir
        {
            get
            {
                return this._BundleDir;
            }
            set
            {
                this._BundleDir = value;
            }
        }

        /// <summary>
        /// Tên Atlas chứa ảnh
        /// </summary>
        public string AtlasName
        {
            get
            {
                return this._AtlasName;
            }
            set
            {
                this._AtlasName = value;
            }
        }

        /// <summary>
        /// Danh sách Sprite
        /// </summary>
        public string[] SpriteNames
        {
            get
            {
                return this._SpriteNames;
            }
            set
            {
                this._SpriteNames = value;
            }
        }

        /// <summary>
        /// Tự thực thi
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
        /// Đánh dấu có đang thực thi không
        /// </summary>
        public bool IsPlaying { get; private set; } = false;

        /// <summary>
        /// Đối tượng có hiển thị không
        /// </summary>
        public bool Visible
        {
            get
            {
                return this.gameObject.activeSelf;
            }
            set
            {
                this.gameObject.SetActive(value);
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
        /// Độ trong suốt
        /// </summary>
        private float Alpha
        {
            set
            {
                /// Nếu là UI.Image
                if (this.uiImage != null)
                {
                    Color color = this.uiImage.color;
                    color.a = value;
                    this.uiImage.color = color;
                }
                /// Nếu là SpriteRenderer
                else if (this.spriteRenderer != null)
                {
                    Color color = this.spriteRenderer.color;
                    color.a = value;
                    this.spriteRenderer.color = color;
                }
            }
        }

        /// <summary>
        /// Sự kiện khi dừng
        /// </summary>
        public Action OnStop { get; set; }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            this.uiImage = this.UIImage.GetComponent<UnityEngine.UI.Image>();
            this.spriteRenderer = this.UIImage.GetComponent<SpriteRenderer>();
        }

        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.InitPrefabs();
            if (this._AutoPlay)
            {
                this.Play();
            }
            else
            {
                /// Thiết lập độ trong suốt
                this.Alpha = 0f;
            }
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

        /// <summary>
        /// Hàm này gọi khi đối tượng bị hủy kích hoạt
        /// </summary>
        private void OnDisable()
        {
            this.StopAllCoroutines();
            this.animationCoroutine = null;
            this.IsPlaying = false;
        }
        #endregion

        #region Code UI
        /// <summary>
        /// Khởi tạo ban đầu
        /// </summary>
        private void InitPrefabs()
        {

        }
        #endregion

        #region Private methods
        /// <summary>
        /// Thực thi hiệu ứng
        /// </summary>
        /// <returns></returns>
        private IEnumerator DoPlay()
        {
            /// Nếu thời gian tồn tại quá ngắn
            if (this._Duration <= 0)
            {
                yield break;
            }
            /// Nếu không có Sprite
            if (this._SpriteNames == null || this._SpriteNames.Length <= 0)
            {
                yield break;
            }

            /// Đánh dấu đang thực thi
            this.IsPlaying = true;

            /// Vị trí hiện tại
            int currentIndex = -1;
            /// Thời gian mỗi lần chạy 1 ảnh
            float durationEach = this._Duration / this._SpriteNames.Length;

            /// Thiết lập độ trong suốt
            this.Alpha = 1f;

            /// Lặp liên tục
            while (true)
            {
                /// Nếu Sprite rỗng
                if (this._SpriteNames == null || this._SpriteNames.Length <= 0)
                {
                    /// Bỏ qua Frame
                    yield return null;
                    /// Tiếp tục lặp
                    continue;
                }
                /// Tăng vị trí hiện tại
                currentIndex++;
                /// Nếu đã thực thi đến Frame cuối
                if (currentIndex >= this._SpriteNames.Length)
                {
                    /// Nếu không lặp lại
                    if (!this._Loop)
                    {
                        break;
                    }
                    /// Quay về Frame đầu tiên
                    currentIndex = 0;
                }

                /// Thực thi hiệu ứng
                this.UIImage.BundleDir = this._BundleDir;
                this.UIImage.AtlasName = this._AtlasName;
                this.UIImage.SpriteName = this._SpriteNames[currentIndex];
                this.UIImage.PixelPerfect = this._PixelPerfect;
                this.UIImage.Scale = this._Scale;
                this.UIImage.Load();

                /// Nghỉ thời gian tương ứng
                yield return new WaitForSeconds(durationEach);
            }

            /// Thực thi sự kiện
            this.OnStop?.Invoke();
            /// Dừng luồng
            this.Stop();
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Thực thi hiệu ứng
        /// </summary>
        public void Play()
        {
            if (!this.gameObject.activeSelf)
            {
                return;
            }

            this.Stop();
            this.animationCoroutine = this.StartCoroutine(this.DoPlay());
        }

        /// <summary>
        /// Ngừng thực thi hiệu ứng
        /// </summary>
        public void Stop()
        {
            if (!this.gameObject.activeSelf)
            {
                return;
            }

            if (this.animationCoroutine != null)
            {
                /// Bỏ dấu đang thực thi
                this.IsPlaying = false;
                this.StopCoroutine(this.animationCoroutine);
                this.animationCoroutine = null;
            }
            
            /// Thiết lập độ trong suốt
            this.Alpha = 0f;
        }
        #endregion
    }
}
