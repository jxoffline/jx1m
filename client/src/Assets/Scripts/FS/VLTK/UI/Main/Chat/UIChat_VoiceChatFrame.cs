using FS.GameEngine.Logic;
using FS.VLTK.Utilities.UnityUI;
using System;
using UnityEngine;

namespace FS.VLTK.UI.Main.Chat
{
    /// <summary>
    /// Khung Voice Chat
    /// </summary>
    public class UIChat_VoiceChatFrame : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Button đóng khung
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Close;

        /// <summary>
        /// Button gửi tin
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Send;

        /// <summary>
        /// Toggle thu âm
        /// </summary>
        [SerializeField]
        private UIToggleSprite UIToggle_Record;
        
        /// <summary>
        /// Button phát lại
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Play;
        #endregion

        #region Properties
        /// <summary>
        /// Sự kiện đóng khung
        /// </summary>
        public Action Close { get; set; }

        /// <summary>
        /// Sự kiện gửi tin
        /// </summary>
        public Action<byte[]> Send { get; set; }

        /// <summary>
        /// Khung có đang hiển thị không
        /// </summary>
        public bool Visible
        {
            get
            {
                return this.gameObject.activeSelf;
            }
        }

        /// <summary>
        /// Dữ liệu thu âm
        /// </summary>
        public byte[] VoiceData { get; private set; }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.InitPrefabs();
        }

        /// <summary>
        /// Hàm này gọi liên tục mỗi Frame
        /// </summary>
        private void Update()
        {
            this.UIButton_Play.interactable = this.VoiceData != null;
            this.UIButton_Send.interactable = this.VoiceData != null;
        }
        #endregion

        #region Code UI
        /// <summary>
        /// Khởi tạo ban đầu
        /// </summary>
        private void InitPrefabs()
        {
            this.UIButton_Close.onClick.AddListener(this.ButtonClose_Clicked);
            this.UIButton_Send.onClick.AddListener(this.ButtonSend_Clicked);
            this.UIButton_Play.onClick.AddListener(this.ButtonPlay_Clicked);
            this.UIToggle_Record.OnSelected = this.ToggleRecord_Clicked;
        }

        /// <summary>
        /// Sự kiện khi Button đóng khung được ấn
        /// </summary>
        private void ButtonClose_Clicked()
        {
            if (this.VoiceData != null)
            {
                KTGlobal.ShowMessageBox("Tin nhắn vẫn chưa được gửi đi, xác nhận đóng khung?", () => {
                    this.Close?.Invoke();
                    /// Xóa dữ liệu cũ
                    Global.Recorder.Release();
                    this.Hide();
                }, true);
                return;
            }

            this.Close?.Invoke();
            /// Xóa dữ liệu cũ
            Global.Recorder.Release();
            this.Hide();
        }

        /// <summary>
        /// Sự kiện khi Button gửi tin được ấn
        /// </summary>
        private void ButtonSend_Clicked()
        {
            if (this.VoiceData == null)
            {
                KTGlobal.ShowMessageBox("Không có đoạn thu âm nào, không thể gửi đi!", true);
                return;
            }

            this.Send?.Invoke(Global.Recorder.ResultData);
            /// Xóa dữ liệu vừa thu âm
            Global.Recorder.Release();
            this.Hide();
        }

        /// <summary>
        /// Sự kiện khi Button phát lại được ấn
        /// </summary>
        private void ButtonPlay_Clicked()
        {
            Global.Recorder.Play();
        }

        /// <summary>
        /// Sự kiện khi Toggle thu âm được chọn
        /// </summary>
        /// <param name="isSelected"></param>
        private void ToggleRecord_Clicked(bool isSelected)
        {
            if (isSelected)
            {
                /// Xóa dữ liệu cũ
                Global.Recorder.Release();

                KTGlobal.AddNotification("Bắt đầu thu âm!");
                Global.Recorder.Record();
                this.VoiceData = null;
            }
            else
            {
                KTGlobal.AddNotification("Ngừng thu âm!");
                Global.Recorder.StopRecording();
                this.VoiceData = Global.Recorder.ResultData;
            }
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Hiện
        /// </summary>
        public void Show()
        {
            this.gameObject.SetActive(true);
            this.VoiceData = null;
        }

        /// <summary>
        /// Ẩn
        /// </summary>
        public void Hide()
        {
            this.gameObject.SetActive(false);
            /// Xóa dữ liệu cũ
            Global.Recorder.Release();
        }
        #endregion
    }
}
