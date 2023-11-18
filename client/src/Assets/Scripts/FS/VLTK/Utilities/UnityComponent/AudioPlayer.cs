using FS.VLTK.Factory;
using System;
using System.Collections;
using UnityEngine;

namespace FS.VLTK.Utilities.UnityComponent
{
    /// <summary>
    /// Đối tượng âm thanh
    /// </summary>
    public class AudioPlayer : TTMonoBehaviour
    {
        #region Define
        /// <summary>
        /// Âm thanh chạy lúc xuất hiệu ứng
        /// </summary>
        public AudioClip Sound = null;

        /// <summary>
        /// Thời gian delay phát ra âm thanh lúc xuất hiệu ứng
        /// </summary>
        public float ActivateAfter = 0f;

        /// <summary>
        /// Lặp âm thanh
        /// </summary>
        public bool IsRepeat = false;

        /// <summary>
        /// Thời gian giãn cách lặp lại hiệu ứng
        /// </summary>
        public float RepeatTimer = 0f;

        /// <summary>
        /// Âm lượng
        /// </summary>
        public float Volume = 1f;

        /// <summary>
        /// Hàm thực thi khi phương thức Play được gọi
        /// </summary>
        public Action OnPlay { get; set; } = null;

        /// <summary>
        /// Hàm thực thi khi phương thức Pause được gọi
        /// </summary>
        public Action OnPause { get; set; } = null;

        /// <summary>
        /// Hàm thực thi khi phương thức Resume được gọi
        /// </summary>
        public Action OnResume { get; set; } = null;

        /// <summary>
        /// Hàm thực thi khi phương thức Stop được gọi
        /// </summary>
        public Action OnStop { get; set; } = null;

        /// <summary>
        /// Sự kiện khi đối tượng bị xóa
        /// </summary>
        public Action Destroyed { get; set; }

        /// <summary>
        /// Nhạc đang tạm dừng
        /// </summary>
        public bool IsPause { get; private set; }
        #endregion

        /// <summary>
        /// Đối tượng AudioSource
        /// </summary>
        private AudioSource audioSource = null;

        /// <summary>
        /// Âm thanh lần trước
        /// </summary>
        private float lastVolume = -1;

        #region Public methods
        /// <summary>
        /// Phát âm thanh
        /// </summary>
        public void Play()
        {
            this.StopAllCoroutines();
            this.StartCoroutine(this.DoPlaySound());

            this.OnPlay?.Invoke();
        }

        /// <summary>
        /// Dừng phát âm thanh
        /// </summary>
        public void Stop()
        {
            this.StopAllCoroutines();
            AudioSource audio = this.audioSource;
            if (audio == null)
            {
                return;
            }
            audio.Stop();

            this.OnStop?.Invoke();
        }

        /// <summary>
        /// Tiếp tục phát âm thanh
        /// </summary>
        public void Resume()
        {
            AudioSource audio = this.audioSource;
            if (audio == null)
            {
                return;
            }
            audio.UnPause();

            if (this.IsPause)
            {
                this.IsPause = false;
            }

            this.OnResume?.Invoke();
        }

        /// <summary>
        /// Tạm dừng phát âm thanh
        /// </summary>
        public void Pause()
        {
            AudioSource audio = this.audioSource;
            if (audio == null)
            {
                return;
            }
            audio.Pause();
            this.IsPause = true;

            this.OnPause?.Invoke();
        }
        #endregion

        #region Private methods
        private float currentPlayTime;

        /// <summary>
        /// Chạy âm thanh
        /// </summary>
        /// <returns></returns>
        private IEnumerator DoPlaySound()
        {
            if (this.ActivateAfter > 0)
            {
                yield return new WaitForSeconds(this.ActivateAfter);
            }

            AudioSource audio = this.audioSource;
            this.currentPlayTime = 0;
            /// Đánh dấu lần đầu phát
            bool isFirstTime = true;

            /// Nếu tồn tại dữ liệu
            if (audio != null && this.Sound != null)
            {
                audio.clip = this.Sound;
                audio.volume = this.Volume;
                if (!this.IsRepeat)
                {
                    audio.loop = false;
                    audio.Play();
                    yield return new WaitForSeconds(audio.clip.length);
                    this.Stop();
                }
                else
                {
                    if (this.IsRepeat && this.RepeatTimer <= 0)
                    {
                        audio.loop = true;
                        audio.Play();
                    }
                    else
                    {
                        audio.loop = false;
                        while (true)
                        {
                            if (!this.IsPause)
                            {
                                if (!audio.isPlaying)
                                {
                                    this.currentPlayTime += Mathf.Min(this.RepeatTimer, 1f);
                                    /// Nếu là lần đầu phát hoặc đã đợi khoảng thời gian tương ứng
                                    if (isFirstTime || this.currentPlayTime >= this.RepeatTimer)
                                    {
                                        /// Hủy đánh dấu lần đầu phát
                                        isFirstTime = false;
                                        this.currentPlayTime = 0;
                                        audio.Play();
                                    }
                                }
                            }
                            yield return new WaitForSeconds(Mathf.Min(this.RepeatTimer, 1f));
                        }
                    }
                }
            }
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Xóa đối tượng
        /// </summary>
        public void Destroy()
        {
            this.StopAllCoroutines();

            this.Sound = null;
            this.ActivateAfter = 0;
            this.IsRepeat = false;
            this.RepeatTimer = 0;
            this.Volume = 0;
            this.OnPlay = null;
            this.OnPause = null;
            this.OnResume = null;
            this.OnStop = null;
            this.IsPause = false;
            this.Destroyed?.Invoke();
            this.Destroyed = null;
            KTObjectPoolManager.Instance.ReturnToPool(this.gameObject);
        }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            this.audioSource = this.GetComponent<AudioSource>();
        }

        /// <summary>
        /// Hàm này gọi liên tục mỗi Frame
        /// </summary>
        private void Update()
        {
            if (this.Volume != this.lastVolume)
            {
                this.audioSource.volume = this.Volume;
                this.lastVolume = this.Volume;
            }
        }

        /// <summary>
        /// Hàm này gọi đến khi đối tượng bị hủy kích hoạt
        /// </summary>
        private void OnDisable()
        {

        }
        #endregion
    }
}