using UnityEngine;

namespace FS.VLTK.Utilities.UnityComponent
{
    /// <summary>
    /// Thu âm
    /// </summary>
    public class AudioRecorder : TTMonoBehaviour
    {
        #region Private fields
        /// <summary>
        /// Dữ liệu âm thanh được ghi lại
        /// </summary>
        private AudioClip recording;

        /// <summary>
        /// Thời gian bắt đầu thu
        /// </summary>
        private float startRecordingTime;
        #endregion

        #region Properties
        /// <summary>
        /// Kết quả thu âm
        /// </summary>
        public AudioClip Result
        {
            get
            {
                return this.recording;
            }
        }
        #endregion

        /// <summary>
        /// Bắt đầu thu âm
        /// </summary>
        public void StartRecording()
        {
            //Get the max frequency of a microphone, if it's less than 44100 record at the max frequency, else record at 44100
            int minFreq;
            int maxFreq;
            int freq = 44100;
            Microphone.GetDeviceCaps("", out minFreq, out maxFreq);
            if (maxFreq < 44100)
                freq = maxFreq;

            //Start the recording, the length of 10 gives it a cap of 5 sec
            this.recording = Microphone.Start("", false, 10, 44100);
            this.startRecordingTime = Time.time;
        }

        /// <summary>
        /// Ngừng thu âm
        /// </summary>
        public void StopRecording()
        {
            //End the recording when the mouse comes back up, then play it
            Microphone.End("");

            /// Nếu không có thiết bị thu âm
            if (this.recording == null)
            {
                return;
            }

            //Trim the audioclip by the length of the recording
            AudioClip recordingNew = AudioClip.Create(this.recording.name, (int) ((Time.time - this.startRecordingTime) * this.recording.frequency), this.recording.channels, this.recording.frequency, false);
            float[] data = new float[(int) ((Time.time - this.startRecordingTime) * this.recording.frequency)];
            this.recording.GetData(data, 0);
            recordingNew.SetData(data, 0);
            this.recording = recordingNew;
        }

        /// <summary>
        /// Giải phóng bộ nhớ
        /// </summary>
        public void Release()
		{
            if (this.recording != null)
			{
                GameObject.Destroy(this.recording);
                this.recording = null;
            }
        }
    }
}
