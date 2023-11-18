using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace FS.VLTK.Utilities.UnityComponent
{
    /// <summary>
    /// Thực thi hiệu ứng Animation
    /// </summary>
    [ExecuteAlways]
    public class AnimatedSprite : TTMonoBehaviour
    {
        #region Define
        /// <summary>
        /// Danh sách Sprite tạo thành hiệu ứng
        /// </summary>
        [SerializeField]
        private Sprite[] _Sprites;

        /// <summary>
        /// Thời gian thực thi hiệu ứng
        /// </summary>
        [SerializeField]
        private float _Duration;

        /// <summary>
        /// Ảnh khớp kích thước Sprite gốc
        /// </summary>
        [SerializeField]
        private bool _PixelPerfect;

#if UNITY_EDITOR
        /// <summary>
        /// Thực hiện hiệu ứng ngay (Chế độ Editor)
        /// </summary>
        [SerializeField]
        private bool _PlayNow;
#endif
#endregion

        #region Private fields
        /// <summary>
        /// Đối tượng SpriteRenderer
        /// </summary>
        private SpriteRenderer spriteRenderer = null;

        /// <summary>
        /// Đối tượng UnityEngine.UI.Image
        /// </summary>
        private UnityEngine.UI.Image uiImage = null;

        /// <summary>
        /// Đối tượng RectTransform
        /// </summary>
        private RectTransform uiRectTransform = null;

        /// <summary>
        /// ID Frame hiện tại
        /// </summary>
        private int frameIndex = -1;
        #endregion

        #region Properties
        /// <summary>
        /// Tạm dừng
        /// </summary>
        public bool IsPausing { get; private set; } = false;

        /// <summary>
        /// Danh sách ảnh tạo nên hiệu ứng
        /// </summary>
        public Sprite[] Sprites
        {
            get
            {
                return this._Sprites;
            }
            set
            {
                this._Sprites = value;
            }
        }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            this.spriteRenderer = this.gameObject.GetComponent<SpriteRenderer>();
            this.uiImage = this.gameObject.GetComponent<UnityEngine.UI.Image>();
            this.uiRectTransform = this.gameObject.GetComponent<RectTransform>();
        }

        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            
        }

        /// <summary>
        /// Hàm này gọi liên tục mỗi Frame
        /// </summary>
        private void Update()
        {
#if UNITY_EDITOR
            if (this._Sprites == null || this._Sprites.Length <= 0)
            {
                this.IsPausing = true;
                this.frameIndex = -1;
            }

            if (this._PlayNow)
            {
                this.StopAllCoroutines();
                this.StartCoroutine(this.DoPlay());
                this._PlayNow = false;
            }
#endif
        }

        /// <summary>
        /// Hàm này gọi khi đối tượng được kích hoạt
        /// </summary>
        private void OnEnable()
        {
            this.StartCoroutine(this.DoPlay());
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
        /// Thực hiện hiệu ứng
        /// </summary>
        /// <returns></returns>
        private IEnumerator DoPlay()
        {
            float durationEach = this._Duration / this._Sprites.Length;
            while (true)
            {
                /// Nếu đang tạm dừng
                if (this.IsPausing)
                {
                    yield return null;
                    continue;
                }

                /// Tăng ID frame lên
                this.frameIndex++;
                /// Nếu ID vượt quá kích thước danh sách ảnh
                if (this.frameIndex >= this._Sprites.Length)
                {
                    this.frameIndex = -1;
                    /// Nếu không có Sprite
                    if (this._Sprites.Length <= 0)
                    {
                        yield return null;
                    }
                    continue;
                }

                /// Sprite tương ứng
                Sprite sprite = this._Sprites[this.frameIndex];
                /// Gắn ảnh vào Component tương ứng
                if (this.spriteRenderer != null)
                {
                    this.spriteRenderer.sprite = sprite;
                    this.spriteRenderer.drawMode = SpriteDrawMode.Sliced;
                    /// Nếu có gắn cờ Pixel Perfect
                    if (this._PixelPerfect)
                    {
                        this.spriteRenderer.size = sprite.rect.size;
                    }
                }
                else
                {
                    this.uiImage.sprite = sprite;
                    /// Nếu có gắn cờ Pixel Perfect
                    if (this._PixelPerfect)
                    {
                        this.uiRectTransform.sizeDelta = sprite.rect.size;
                    }
                }

                yield return new WaitForSeconds(durationEach);
            }
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Tạm dừng hiệu ứng
        /// </summary>
        public void Pause()
        {
            this.IsPausing = true;
        }

        /// <summary>
        /// Tiếp tục hiệu ứng
        /// </summary>
        public void Resume()
        {
            this.IsPausing = false;
        }
        #endregion
    }
}
