using FS.GameEngine.Logic;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


namespace FS.VLTK.Utilities.UnityUI
{
    /// <summary>
    /// Tải ảnh từ AssetBundle, đặt vào đối tương UnityEngine.UI.Image hoặc UnityEngine.SpriteRenderer
    /// </summary>
    [ExecuteAlways]
    public class SpriteFromAssetBundle : MonoBehaviour
    {
        #region Defines
        /// <summary>
        /// Đường dẫn Bundle chứa Sprite
        /// </summary>
        [SerializeField]
        private string _BundleDir;

        /// <summary>
        /// Tên Atlas chứa Sprite trong Bundle
        /// </summary>
        [SerializeField]
        private string _AtlasName;

        /// <summary>
        /// Tên Sprite tương ứng
        /// </summary>
        [SerializeField]
        private string _SpriteName;

        /// <summary>
        /// Thiết lập kích thước của ảnh trùng với kích thước gốc
        /// </summary>
        [SerializeField]
        private bool _PixelPerfect;

        /// <summary>
        /// Thiết lập kích thước của ảnh trùng với kích thước gốc ở chế độ Play
        /// </summary>
        [SerializeField]
        private bool _PixelPerfectOnPlay;

#if UNITY_EDITOR
        /// <summary>
        /// Tải ảnh xuống ngay lập tức
        /// </summary>
        [SerializeField]
        private bool _LoadImmedatelly;
#endif

        /// <summary>
        /// Tỷ lệ phóng to
        /// </summary>
        [SerializeField]
        private float _Scale = 1f;

        /// <summary>
        /// Đường dẫn đến file Bundle đã thay đổi
        /// </summary>
        private string lastBundleDir = "";
        #endregion

        #region Properties
        /// <summary>
        /// Đường dẫn Bundle chứa Sprite
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
        /// Tên Atlas chứa Sprite trong Bundle
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
        /// Tên Sprite tương ứng
        /// </summary>
        public string SpriteName
        {
            get
            {
                return this._SpriteName;
            }
            set
            {
                this._SpriteName = value;
            }
        }

        /// <summary>
        /// Thiết lập kích thước của ảnh trùng với kích thước gốc
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
        /// Tỷ lệ phóng to
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
        #endregion

        #region Public methods
        /// <summary>
        /// Tải Sprites trong Bundle (chế độ play)
        /// </summary>
        public void Load()
        {
            /// Nếu không có thông tin
            if (string.IsNullOrEmpty(this._BundleDir) || string.IsNullOrEmpty(this._AtlasName) || string.IsNullOrEmpty(this._SpriteName))
			{
                return;
			}

            /// Đối tượng RectTransform tương ứng
            RectTransform transform = this.gameObject.GetComponent<UnityEngine.RectTransform>();
            /// Nếu tồn tại Bundle cũ
            if (!string.IsNullOrEmpty(this.lastBundleDir))
            {
                /// Đường dẫn Bundle cũ
                string lastBundleDir = (transform != null ? Consts.UI_DIR : Consts.RESOURCES_DIR) + "/" + this.lastBundleDir;
                /// Hủy tham chiếu Bundle cũ
                KTUIResourceManager.Instance.UnloadAssetBundle(lastBundleDir);
			}

            /// Đường dẫn Bundle mới
            string bundleDir = (transform != null ? Consts.UI_DIR : Consts.RESOURCES_DIR) + "/" + this._BundleDir;
            /// Tải Bundle mới
            KTUIResourceManager.Instance.LoadBundle(bundleDir);
            /// Gắn Bundle cũ thành Bundle hiện tại
            this.lastBundleDir = this._BundleDir;

            /// Nếu là UIImage
            if (transform != null)
            {
                UnityEngine.UI.Image image = this.gameObject.GetComponent<UnityEngine.UI.Image>();
                if (image != null)
                {
                    image.sprite = KTUIResourceManager.Instance.GetSprite(Consts.UI_DIR + "/" + this._BundleDir, this._AtlasName, this._SpriteName);
                    if (image.sprite == null)
                    {
                        KTDebug.LogError("Không có ảnh: " + this.gameObject.name);
                        return;
                    }

                    if (this._PixelPerfect)
                    {
                        transform.sizeDelta = image.sprite.rect.size * this._Scale;
                    }
                }
            }
            /// Nếu là SpriteRenderer
            else
            {
                UnityEngine.SpriteRenderer renderer = this.gameObject.GetComponent<UnityEngine.SpriteRenderer>();
                if (renderer != null)
                {
                    renderer.sprite = KTUIResourceManager.Instance.GetSprite(Consts.RESOURCES_DIR + "/" + this._BundleDir, this._AtlasName, this._SpriteName);
                    if (renderer.sprite == null)
                    {
                        KTDebug.LogError("Không có ảnh: " + this.gameObject.name);
                        return;
                    }

                    renderer.drawMode = SpriteDrawMode.Sliced;
                        
                    if (this._PixelPerfect)
                    {
                        renderer.size = renderer.sprite.rect.size * this._Scale;
                    }
                }
            }
        }

        /// <summary>
        /// Xóa ảnh
        /// </summary>
        public void ClearSprite()
        {
            /// Nếu là UIImage
            if (transform != null)
            {
                UnityEngine.UI.Image image = this.gameObject.GetComponent<UnityEngine.UI.Image>();
                if (image != null)
                {
                    image.sprite = null;
                }
            }
            /// Nếu là SpriteRenderer
            else
            {
                UnityEngine.SpriteRenderer renderer = this.gameObject.GetComponent<UnityEngine.SpriteRenderer>();
                if (renderer != null)
                {
                    renderer.sprite = null;
                }
            }
        }
        #endregion


        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng bị hủy kích hoạt
        /// </summary>
		private void OnDisable()
		{
            if (!string.IsNullOrEmpty(this.lastBundleDir) && KTUIResourceManager.Instance != null)
            {
                UnityEngine.RectTransform transform = this.gameObject.GetComponent<UnityEngine.RectTransform>();
                string bundleDir = (transform != null ? Consts.UI_DIR : Consts.RESOURCES_DIR) + "/" + this.lastBundleDir;

                KTUIResourceManager.Instance.UnloadAssetBundle(bundleDir);

				this.lastBundleDir = null;
            }
        }

        /// <summary>
        /// Hàm này gọi khi đối tượng được kích hoạt
        /// </summary>
		private void OnEnable()
		{
            if (Application.isPlaying)
            {
                this._PixelPerfect = this._PixelPerfectOnPlay;
                this.Load();
            }
#if UNITY_EDITOR
            else if (Application.isEditor)
            {
                this.Editor_Load();
            }
#endif
        }
        #endregion

#if UNITY_EDITOR
        /// <summary>
        /// Tải ảnh (ở Editor)
        /// </summary>
        private void Editor_Load()
        {
            UnityEngine.RectTransform transform = this.gameObject.GetComponent<UnityEngine.RectTransform>();
            if (!string.IsNullOrEmpty(this._BundleDir) && !string.IsNullOrEmpty(this._AtlasName) && !string.IsNullOrEmpty(this._SpriteName))
            {
                string url = Global.WebPath(string.Format("Data/{0}", (transform != null ? Consts.UI_DIR : Consts.RESOURCES_DIR) + "/" + this._BundleDir));

                UnityWebRequest www = UnityWebRequestAssetBundle.GetAssetBundle(url);
                www.SendWebRequest();

                while (!www.isDone) { }

                if (string.IsNullOrEmpty(www.error))
                {
                    AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(www);
                    if (bundle == null)
                    {
                        KTDebug.LogError("Không tìm thấy Bundle tại -> " + url);
                        return;
                    }

                    UnityEngine.Sprite[] sprites = bundle.LoadAssetWithSubAssets<UnityEngine.Sprite>(this._AtlasName);

                    foreach (UnityEngine.Sprite sprite in sprites)
                    {
                        if (sprite.name == this._SpriteName)
                        {
                            if (transform != null)
                            {
                                UnityEngine.UI.Image image = this.gameObject.GetComponent<UnityEngine.UI.Image>();
                                if (image != null && transform != null)
                                {
                                    image.sprite = sprite;
                                    if (image.sprite == null)
                                    {
                                        KTDebug.LogError("Không có ảnh: " + this.gameObject.name);
                                        continue;
                                    }

                                    if (this._PixelPerfectOnPlay)
                                    {
                                        image.GetComponent<RectTransform>().sizeDelta = sprite.rect.size;
                                    }
                                }
                            }
                            else
                            {
                                UnityEngine.SpriteRenderer renderer = this.gameObject.GetComponent<UnityEngine.SpriteRenderer>();
                                if (renderer != null)
                                {
                                    renderer.sprite = sprite;
                                    if (renderer.sprite == null)
                                    {
                                        KTDebug.LogError("Không có ảnh: " + this.gameObject.name);
                                        continue;
                                    }

                                    renderer.drawMode = SpriteDrawMode.Sliced;
                                    if (this._PixelPerfectOnPlay)
                                    {
                                        renderer.size = sprite.rect.size;
                                    }
                                }
                            }
                        }
                    }

                    bundle.Unload(false);
                }
                else
                {
                    KTDebug.LogError("Lỗi tải Bundle -> " + www.error);
                }
            }
        }

        /// <summary>
        /// Hàm này gọi liên tục mỗi frame
        /// </summary>
        private void Update()
        {
            if (!Application.isPlaying)
            {
                if (this._LoadImmedatelly)
                {
                    this.Editor_Load();

                    this._LoadImmedatelly = false;
                }


                if (this._PixelPerfect)
                {
                    UnityEngine.RectTransform transform = this.gameObject.GetComponent<UnityEngine.RectTransform>();
                    if (transform != null)
                    {
                        UnityEngine.UI.Image image = this.gameObject.GetComponent<UnityEngine.UI.Image>();
                        if (image != null)
                        {
                            transform.sizeDelta = image.sprite == null ? Vector2.zero : image.sprite.rect.size * this._Scale;
                        }
                    }
                    else
                    {
                        UnityEngine.SpriteRenderer renderer = this.gameObject.GetComponent<UnityEngine.SpriteRenderer>();
                        if (renderer != null)
                        {
                            renderer.size = renderer.sprite.rect.size * this._Scale;
                        }
                    }

                    this._PixelPerfect = false;
                }
            }
        }
#endif
    }
}