using FS.GameEngine.Logic;
using FS.VLTK.Factory;
using UnityEngine;

namespace FS.VLTK.Utilities.UnityComponent
{
    /// <summary>
    /// Audio Clip từ AssetBundle
    /// </summary>
    [RequireComponent(typeof(AudioSource), typeof(AudioPlayer))]
    public class AudioClipFromAssetBundle : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Đường dẫn File AssetBundle chứa nhạc
        /// </summary>
        [SerializeField]
        private string _BundleDir;

        /// <summary>
        /// Tên nhạc
        /// </summary>
        [SerializeField]
        private string _SoundName;

        /// <summary>
        /// Tự phát khi khởi động không
        /// </summary>
        [SerializeField]
        private bool _AutoPlay = false;
        #endregion

        #region Properties
        /// <summary>
        /// Đường dẫn File Bundle chứa nhạc
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
        /// Tên nhạc
        /// </summary>
        public string SoundName
        {
            get
            {
                return this._SoundName;
            }
            set
            {
                this._SoundName = value;
            }
        }

        /// <summary>
        /// Tự phát khi khởi động
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
        /// Trình phát nhạc
        /// </summary>
        public AudioPlayer Player
        {
            get
            {
                return this.player;
            }
        }
        #endregion

        #region Private fields
        /// <summary>
        /// Trình phát nhạc
        /// </summary>
        private AudioPlayer player;

        /// <summary>
        /// Đã chạy qua hàm Start chưa
        /// </summary>
        private bool isStarted = false;

        /// <summary>
        /// Đường dẫn Bundle trước đó
        /// </summary>
        private string lastBundleDir = "";
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            this.player = this.GetComponent<AudioPlayer>();
        }

        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            /// Tải xuống Bundle
            this.Load();

            /// Nếu tự phát
            if (this._AutoPlay)
            {
                this.player.Play();
            }

            /// Đánh dấu đã chạy qua hàm Start
            this.isStarted = true;
        }

        /// <summary>
        /// Hàm này gọi khi đối tượng được kích hoạt
        /// </summary>
        private void OnEnable()
        {
            /// Nếu chưa chạy qua hàm Start
            if (!this.isStarted)
            {
                /// Bỏ qua
                return;
            }

            /// Tải xuống Bundle
            this.Load();

            /// Nếu tự phát
            if (this._AutoPlay)
            {
                this.player.Play();
            }
        }

        /// <summary>
        /// Hàm này gọi khi đối tượng bị hủy kích hoạt
        /// </summary>
        private void OnDisable()
        {
            /// Nếu chưa chạy qua hàm Start
            if (!this.isStarted)
            {
                /// Bỏ qua
                return;
            }

            /// Hủy bundle
            this.Unload();
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Tải xuống Bundle
        /// </summary>
        private void Load()
        {
            /// Nếu tồn tại Bundle cũ
            if (!string.IsNullOrEmpty(this.lastBundleDir))
            {
                /// Đường dẫn Bundle cũ
                string lastBundleDir = string.Format("{0}/{1}", Consts.SOUND_DIR, this.lastBundleDir);
                /// Hủy tham chiếu Bundle cũ
                KTResourceManager.Instance.ReleaseBundle(lastBundleDir);
            }

            /// Lưu lại Bundle trước đó
            this.lastBundleDir = this._BundleDir;

            /// Đường dẫn Bundle
            string bundleDir = string.Format("{0}/{1}", Consts.SOUND_DIR, this._BundleDir);
            /// Tải xuống Bundle
            KTResourceManager.Instance.LoadAssetBundle(bundleDir);
            /// Tải xuống Atlas
            if (KTResourceManager.Instance.LoadAsset<AudioClip>(bundleDir, this._SoundName, false))
            {
                /// AudioClip tương ứng
                AudioClip sound = KTResourceManager.Instance.GetAsset<AudioClip>(bundleDir, this._SoundName);
                /// Thiết lập
                this.player.Sound = sound;
            }
        }

        /// <summary>
        /// Hủy tham chiếu tới Bundle tương ứng
        /// </summary>
        private void Unload()
        {
            /// Hủy Bundle trước đó
            this.lastBundleDir = "";
            /// Đường dẫn Bundle
            string bundleDir = string.Format("{0}/{1}", Consts.SOUND_DIR, this._BundleDir);
            /// Hủy Bundle
            KTResourceManager.Instance.ReleaseBundle(bundleDir);
        }
        #endregion
    }
}
