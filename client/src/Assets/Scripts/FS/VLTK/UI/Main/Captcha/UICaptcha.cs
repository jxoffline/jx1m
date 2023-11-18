using FS.VLTK.UI.Main.Captcha;
using FS.VLTK.Utilities.UnityUI;
using Server.Data;
using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace FS.VLTK.UI.Main
{
    /// <summary>
    /// Khung Captcha
    /// </summary>
    public class UICaptcha : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Text thời gian đếm lùi
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Countdown;

        /// <summary>
        /// Image Captcha
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.RawImage UIImage_Captcha;

        /// <summary>
        /// Prefab câu trả lời
        /// </summary>
        [SerializeField]
        private UICaptcha_ToggleAnswer UI_AnswerPrefab;

        /// <summary>
        /// Button xác nhận
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Button UIButton_Submit;
        #endregion

        #region Constants
        /// <summary>
        /// Thời gian trả lời (giây)
        /// </summary>
        private const float Duration = 30f;
        #endregion

        #region Private fields
        /// <summary>
        /// RectTransform danh sách câu trả lời
        /// </summary>
        private RectTransform uiTransformAnswersList = null;

        /// <summary>
        /// Đáp án được chọn
        /// </summary>
        private UICaptcha_ToggleAnswer selectedAnswer = null;

        /// <summary>
        /// Luồng thực hiện bấm giờ
        /// </summary>
        private Coroutine countdownTimer = null;
        #endregion

        #region Properties
        /// <summary>
        /// Dữ liệu
        /// </summary>
        public G2C_Captcha Data { get; set; }

        /// <summary>
        /// Sự kiện trả lời
        /// </summary>
        public Action<string> Submit { get; set; }

        /// <summary>
        /// Sự kiện đóng khung
        /// </summary>
        public Action Close { get; set; }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            this.uiTransformAnswersList = this.UI_AnswerPrefab.transform.parent.GetComponent<RectTransform>();
        }

        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.InitPrefabs();
            this.Refresh();
        }
        #endregion

        #region Code UI
        /// <summary>
        /// Khởi tạo ban đầu
        /// </summary>
        private void InitPrefabs()
        {
            this.UIButton_Submit.onClick.AddListener(this.ButtonSubmit_Clicked);
        }

        /// <summary>
        /// Sự kiện khi Button xác nhận được ấn
        /// </summary>
        private void ButtonSubmit_Clicked()
        {
            /// Nếu không có đáp án được chọn
            if (this.selectedAnswer == null)
            {
                KTGlobal.AddNotification("Hãy chọn một đáp án!");
                return;
            }
            /// Thực thi sự kiện
            this.Submit?.Invoke(this.selectedAnswer.Text);
            /// Đóng khung
            this.Close?.Invoke();
        }

        /// <summary>
        /// Sự kiện khi Toggle trả lời được ấn
        /// </summary>
        /// <param name="toggle"></param>
        private void ToggleAnswer_Selected(UICaptcha_ToggleAnswer toggleAnswer)
        {
            /// Đánh dấu đáp án được chọn
            this.selectedAnswer = toggleAnswer;
            /// Hiện Button
            this.UIButton_Submit.interactable = true;
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Thực thi sự kiện bỏ qua một số Frame
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="work"></param>
        /// <returns></returns>
        private IEnumerator ExecuteSkipFrames(int skip, Action work)
        {
            for (int i = 1; i <= skip; i++)
            {
                yield return null;
            }
            work?.Invoke();
        }

        /// <summary>
        /// Xây lại giao diện danh sách đáp án
        /// </summary>
        private void RebuildAnswerLayout()
        {
            /// Nếu đối tượng không kích hoạt
            if (!this.gameObject.activeSelf)
            {
                return;
            }
            /// Xây lại giao diện ở Frame tiếp theo
            this.StartCoroutine(this.ExecuteSkipFrames(1, () => {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.uiTransformAnswersList);
            }));
        }

        /// <summary>
        /// Làm rỗng danh sách câu trả lời
        /// </summary>
        private void ClearAnswersList()
        {
            foreach (Transform child in this.uiTransformAnswersList.transform)
            {
                if (child.gameObject != this.UI_AnswerPrefab.gameObject)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }
        }

        /// <summary>
        /// Thêm câu trả lời tương ứng
        /// </summary>
        /// <param name="answerText"></param>
        private UICaptcha_ToggleAnswer AddAnswer(string answerText)
        {
            UICaptcha_ToggleAnswer uiAnswerToggle = GameObject.Instantiate<UICaptcha_ToggleAnswer>(this.UI_AnswerPrefab);
            uiAnswerToggle.transform.SetParent(this.uiTransformAnswersList, false);
            uiAnswerToggle.gameObject.SetActive(true);
            uiAnswerToggle.Text = answerText;
            uiAnswerToggle.Selected = () => {
                this.ToggleAnswer_Selected(uiAnswerToggle);
            };
            return uiAnswerToggle;
        }

        /// <summary>
        /// Xóa ảnh Captcha
        /// </summary>
        private void ClearCaptchaImage()
        {
            this.UIImage_Captcha.gameObject.SetActive(false);
        }

        /// <summary>
        /// Thiết lập ảnh Captcha
        /// </summary>
        /// <param name="captchaData"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        private void SetCaptchaImage(byte[] captchaData, short width, short height)
        {
            try
            {
                /// Load Texture
                Texture2D texture2D = new Texture2D(width, height);
                texture2D.LoadImage(captchaData);
                texture2D.Apply();
                /// Gắn vào ảnh
                this.UIImage_Captcha.texture = texture2D;
                this.UIImage_Captcha.gameObject.SetActive(true);
            }
            catch (Exception ex)
            {
                KTDebug.LogException(ex);
            }
        }

        /// <summary>
        /// Xóa KNB hồ bấm giờ
        /// </summary>
        private void ClearCountdownTimer()
        {
            this.UIText_Countdown.text = "";
            /// Nếu tồn tại luồng
            if (this.countdownTimer != null)
            {
                /// Ngừng luồng
                this.StopCoroutine(this.countdownTimer);
                /// Hủy
                this.countdownTimer = null;
            }
        }

        /// <summary>
        /// Bắt đầu chạy KNB hồ bấm giờ
        /// </summary>
        private void StartCountdownTimer()
        {
            /// Luồng thực thi công việc
            IEnumerator DoWork()
            {
                /// Thời gian còn lại
                float totalSecLeft = UICaptcha.Duration;
                /// Đợi 1s
                WaitForSeconds wait = new WaitForSeconds(1f);
                /// Lặp liên tục
                while (true)
                {
                    /// Giảm thời gian
                    totalSecLeft--;
                    /// Hiển thị thời gian
                    this.UIText_Countdown.text = KTGlobal.DisplayTime(totalSecLeft);
                    /// Đợi
                    yield return wait;
                    /// Nếu đã quá thời gian
                    if (totalSecLeft == 0)
                    {
                        /// Thoát lặp
                        break;
                    }
                }
                /// Xóa luồng đếm thời gian
                this.ClearCountdownTimer();
                /// Đóng khung
                this.Close?.Invoke();
            }
            /// Bắt đầu luồng
            this.countdownTimer = this.StartCoroutine(DoWork());
        }

        /// <summary>
        /// Làm mới dữ liệu
        /// </summary>
        private void Refresh()
        {
            /// Xóa danh sách câu trả lời
            this.ClearAnswersList();
            /// Xóa ảnh Captcha
            this.ClearCaptchaImage();
            /// Ẩn Button
            this.UIButton_Submit.interactable = false;
            /// Xóa KNB hồ bấm giờ
            this.ClearCountdownTimer();

            /// Nếu không tồn tại dữ liệu
            if (this.Data == null)
            {
                return;
            }

            /// Đánh dấu đã chọn đáp án đầu tiên chưa
            bool isSelectedFirst = false;
            /// Duyệt danh sách đáp án
            foreach (string answerText in this.Data.Answers)
            {
                /// Thêm đáp án tương ứng
                UICaptcha_ToggleAnswer uiAnswerToggle = this.AddAnswer(answerText);
                /// Nếu chưa chọn đáp án đầu tiên
                if (!isSelectedFirst)
                {
                    /// Thực hiện chọn đáp án đầu tiên ở Frame tiếp theo
                    this.StartCoroutine(this.ExecuteSkipFrames(1, () => {
                        /// Chọn đáp án đầu tiên
                        uiAnswerToggle.Select();
                        this.ToggleAnswer_Selected(uiAnswerToggle);
                    }));
                    /// Đánh dấu đã chọn đáp án đầu tiên
                    isSelectedFirst = true;
                }
            }

            /// Tạo ảnh Captcha
            this.SetCaptchaImage(this.Data.Data, this.Data.Width, this.Data.Height);

            /// Chạy KNB hồ bấm giờ
            this.StartCountdownTimer();

            /// Xây lại giao diện
            this.RebuildAnswerLayout();
        }
        #endregion
    }
}
