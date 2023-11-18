using FS.VLTK.Entities.Config;
using FS.VLTK.Utilities.UnityUI;
using System;
using System.Collections;
using UnityEngine;

namespace FS.VLTK.Utilities.UnityComponent
{
    /// <summary>
    /// Quản lý hiệu ứng rơi vật phẩm xuống đất
    /// </summary>
    public class DropItemAnimation2D : TTMonoBehaviour
    {
        #region Define
        #region Reference Object
        /// <summary>
        /// Thân vật phẩm
        /// </summary>
        public SpriteFromAssetBundle Body { get; set; }
        #endregion

        private ItemData _Data = null;
        /// <summary>
        /// ID vật phẩm
        /// </summary>
        public ItemData Data
        {
            get
            {
                return this._Data;
            }
            set
            {
                this._Data = value;

                this.OnDataChanged();
            }
        }

        /// <summary>
        /// Sự kiện khi bắt đầu thực hiện động tác
        /// </summary>
        public Action OnStart { get; set; }

        /// <summary>
        /// Độ thu phóng
        /// </summary>
        public float Scale { get; set; } = 1f;
        #endregion

        #region Private methods
        /// <summary>
        /// Sự kiện khi dữ liệu vật phẩm thay đổi
        /// </summary>
        private void OnDataChanged()
        {

        }

        /// <summary>
        /// Thực hiện quay đối tượng
        /// </summary>
        /// <param name="fromAngle"></param>
        /// <param name="toAngle"></param>
        /// <param name="duration"></param>
        /// <param name="frameCount"></param>
        /// <returns></returns>
        private IEnumerator Rotate(int fromAngle, int toAngle, float duration, int frameCount)
        {
            this.Body.transform.localRotation = Quaternion.Euler(0, 0, fromAngle);
            float lifeTime = 0f;
            float tick = frameCount <= 0 ? -1 : duration / frameCount;

            if (tick == -1)
            {
                yield return null;
            }
            else
            {
                yield return new WaitForSeconds(tick);
            }

            while (true)
            {
                lifeTime += tick == -1 ? Time.deltaTime : tick;
                if (lifeTime >= duration)
                {
                    break;
                }

                float percent = lifeTime / duration;

                float nextAngle = fromAngle + (toAngle - fromAngle) * percent;
                this.Body.transform.localRotation = Quaternion.Euler(0, 0, nextAngle);

                if (tick == -1)
                {
                    yield return null;
                }
                else
                {
                    yield return new WaitForSeconds(tick);
                }
            }
            this.Body.transform.localRotation = Quaternion.Euler(0, 0, toAngle);
        }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Đánh dấu vừa thực hiện hàm Awake
        /// </summary>
        private bool justAwaken = false;

        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            this.justAwaken = true;
        }

        /// <summary>
        /// Hàm này gọi đến khi bắt đầu frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.justAwaken = false;
        }

        /// <summary>
        /// Hàm này gọi liên tục mỗi Frame
        /// </summary>
        private void Update()
        {

        }

        /// <summary>
        /// Hàm này gọi đến khi đối tượng bị ẩn
        /// </summary>
        private void OnDisable()
        {
            if (this.justAwaken)
            {
                this.justAwaken = false;
                return;
            }

            this.Body.ClearSprite();
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Thực hiện động tác
        /// </summary>
        /// <returns></returns>
        public void DoAction()
        {
            if (!this.gameObject.activeSelf)
            {
                return;
            }

            if (!this.gameObject || !this.gameObject.activeSelf)
            {
                return;
            }

            this.Body.BundleDir = "UI/" + this._Data.MapSpriteBundleDir;
            this.Body.AtlasName = this._Data.MapSpriteAtlasName;
            this.Body.SpriteName = this._Data.Image;
            this.Body.PixelPerfect = false;
            //this.Body.Scale = 0.4f;

            try
            {
                this.Body.Load();
            }
            catch (Exception)
            {
                /// Load icon mặc định
                this.Body.SpriteName = KTGlobal.UnknownItemIcon;
                this.Body.BundleDir = "UI/" + KTGlobal.UnknowItemBundleDir;
                this.Body.AtlasName = KTGlobal.UnknownItemAtlas;
                this.Body.Load();
            }

            /// Renderer
            SpriteRenderer renderer = this.Body.GetComponent<SpriteRenderer>();
            /// Kích thước ảnh gốc
            Vector2 originSize = renderer.sprite.rect.size;
            /// Chiều ngang
            float width = originSize.x;
            /// Chiều dọc
            float height = originSize.y;
            /// Kích thước tối đa
            float maxSize = Math.Max(width, height);
            /// Tỷ lệ chia
            float ratio = 1f;
            /// Nếu kích thước tối đa vượt quá 35
            if (maxSize > 25)
            {
                /// Cập nhật tỷ lệ
                ratio = 25 / maxSize;
            }

            /// Nhân kích thước mới với tỷ lệ
            originSize *= ratio;
            /// Thiết lập lại
            renderer.size = originSize;
        }

        /// <summary>
        /// Thực hiện quay đối tượng
        /// </summary>
        /// <param name="fromAngle"></param>
        /// <param name="toAngle"></param>
        /// <param name="duration"></param>
        /// <param name="frameCount"></param>
        /// <returns></returns>
        public IEnumerator DoRotateAsync(int fromAngle, int toAngle, float duration, int frameCount = -1)
        {
            if (!this.gameObject || !this.gameObject.activeSelf)
            {
                yield break;
            }

            yield return this.Rotate(fromAngle, toAngle, duration, frameCount);
        }
        #endregion
    }
}
