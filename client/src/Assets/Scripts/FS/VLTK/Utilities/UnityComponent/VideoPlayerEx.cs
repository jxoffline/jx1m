using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Collections;
using UnityEngine.Video;

namespace FS.VLTK.Utilities.UnityComponent
{
    /// <summary>
    /// Video player
    /// </summary>
    [RequireComponent(typeof(VideoPlayer), typeof(AudioSource))]
    public class VideoPlayerEx : TTMonoBehaviour
    {
        #region Define
        /// <summary>
        /// Tự phát
        /// </summary>
        [SerializeField]
        private bool _AutoPlay = true;
        #endregion

        #region Properties
        /// <summary>
        /// Tự phát
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
                this.videoPlayer.playOnAwake = value;
            }
        }

        /// <summary>
        /// Âm lượng âm thanh khi phát video
        /// </summary>
        public float Volume
        {
            get
            {
                return this.audioSource.volume;
            }
            set
            {
                this.audioSource.volume = value;
            }
        }

        /// <summary>
        /// Video
        /// </summary>
        public VideoClip Video
        {
            get
            {
                return this.videoPlayer.clip;
            }
            set
            {
                this.videoPlayer.clip = value;
            }
        }

        /// <summary>
        /// Phát lại không
        /// </summary>
        public bool Repeat
        {
            get
            {
                return this.videoPlayer.isLooping;
            }
            set
            {
                this.videoPlayer.isLooping = value;
                this.audioSource.loop = value;
            }
        }

        /// <summary>
        /// Render Texture
        /// </summary>
        public RenderTexture RenderTexture
        {
            get
            {
                return this.videoPlayer.targetTexture;
            }
            set
            {
                this.videoPlayer.targetTexture = value;
            }
        }

        /// <summary>
        /// Video có đang được phát không
        /// </summary>
        public bool IsPlaying
        {
            get
            {
                return this.videoPlayer.isPlaying;
            }
        }

        /// <summary>
        /// Video có đang tạm dừng không
        /// </summary>
        public bool IsPausing
        {
            get
            {
                return this.videoPlayer.isPaused;
            }
        }
        #endregion

        #region Private fields
        /// <summary>
        /// Video player
        /// </summary>
        private VideoPlayer videoPlayer;

        /// <summary>
        /// Audio source
        /// </summary>
        private AudioSource audioSource;

        /// <summary>
        /// Luồng thực hiện phát video
        /// </summary>
        private Coroutine playCoroutine;
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            this.videoPlayer = this.GetComponent<VideoPlayer>();
            this.audioSource = this.GetComponent<AudioSource>();
            this.videoPlayer.playOnAwake = this._AutoPlay;
            this.audioSource.playOnAwake = this._AutoPlay;
            this.audioSource.Pause();
            this.videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
            this.videoPlayer.errorReceived += (src, msg) => {
                KTGlobal.AddNotification("Thiết bị không hỗ trợ phát Video giới thiệu môn phái!");
            };
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Thực hiện phát video
        /// </summary>
        /// <returns></returns>
        private IEnumerator DoPlay()
        {
            /// Thiết lập âm thanh
            this.videoPlayer.EnableAudioTrack(0, true);
            this.videoPlayer.SetTargetAudioSource(0, this.audioSource);
            /// Tải xuống video
            this.videoPlayer.Prepare();

            /// Đợi đến khi video tải xuống hoàn tất
            while (!this.videoPlayer.isPrepared)
            {
                yield return null;
            }

            /// Phát video
            this.videoPlayer.Play();
            this.audioSource.Play();

            /// Hủy luồng
            this.playCoroutine = null;
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Phát video
        /// </summary>
        public void Play()
        {
            /// Nếu tồn tại luồng cũ
            if (this.playCoroutine != null)
            {
                /// Hủy luồng cũ
                this.StopCoroutine(this.playCoroutine);
            }
            /// Ngừng lại
            this.Stop();
            /// Bắt đầu phát
            this.playCoroutine = this.StartCoroutine(this.DoPlay());
        }

        /// <summary>
        /// Tạm dừng video
        /// </summary>
        public void Pause()
        {
            this.videoPlayer.Pause();
            this.audioSource.Pause();
        }

        /// <summary>
        /// Tiếp tục video
        /// </summary>
        public void Resume()
        {
            this.videoPlayer.Play();
            this.audioSource.Play();
        }

        /// <summary>
        /// Dừng video
        /// </summary>
        public void Stop()
        {
            this.videoPlayer.Stop();
            this.audioSource.Stop();
        }
        #endregion
    }
}
