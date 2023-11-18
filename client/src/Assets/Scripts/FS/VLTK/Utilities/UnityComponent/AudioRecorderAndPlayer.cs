using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FS.VLTK.Utilities.UnityComponent
{
    /// <summary>
    /// Đối tượng quản lý thu âm và phát lại
    /// </summary>
    public class AudioRecorderAndPlayer : TTMonoBehaviour
    {
        #region Define
        /// <summary>
        /// Đối tượng thu âm
        /// </summary>
        [SerializeField]
        private AudioRecorder Recorder;

        /// <summary>
        /// Đối tượng phát âm
        /// </summary>
        [SerializeField]
        private AudioPlayer Player;
        #endregion

        #region Properties
        /// <summary>
        /// Kết quả thu âm
        /// </summary>
        public AudioClip Result
        {
            get
            {
                return this.Recorder.Result;
            }
        }

        /// <summary>
        /// Kết quả thu âm dưới dạng byte array
        /// </summary>
        public byte[] ResultData
        {
            get
            {
                if (this.Recorder == null)
                {
                    return null;
                }
                return KTGlobal.AudioClipToBytes(this.Result);
            }
        }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Bắt đầu thu âm
        /// </summary>
        public void Record()
        {
            this.Player.Stop();
            this.Recorder.StartRecording();
        }

        /// <summary>
        /// Ngừng thu âm
        /// </summary>
        public void StopRecording()
        {
            this.Recorder.StopRecording();
        }

        /// <summary>
        /// Phát đoạn âm vừa thu
        /// </summary>
        public void Play()
        {
            /// Nếu không có kết quả
            if (this.Recorder.Result == null)
            {
                return;
            }
            this.Player.Stop();
            this.Player.Sound = this.Recorder.Result;
            this.Player.Play();
        }

        /// <summary>
        /// Phát đoạn âm thanh truyền vào dưới dạng Byte array
        /// </summary>
        /// <param name="soundData"></param>
        public void Play(byte[] soundData)
        {
            if (soundData == null)
            {
                return;
            }
            /// Xóa dữ liệu cũ
            this.Release();

            AudioClip sound = KTGlobal.BytesToAudioClip(soundData, 1, 44100, false, 1f);
            this.Player.Sound = sound;
            this.Player.Play();
        }

        /// <summary>
        /// Xóa dữ liệu thu âm đẻ giải phóng bộ nhớ
        /// </summary>
        public void Release()
		{
            this.Recorder.Release();
            this.Player.Stop();
            if (this.Player.Sound != null)
			{
                GameObject.Destroy(this.Player.Sound);
                this.Player.Sound = null;
            }
        }
        #endregion
    }
}
