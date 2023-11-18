using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FS.VLTK.UI.Main.SytemNotification
{
    /// <summary>
    /// Khung thông báo dòng chữ chạy kênh hệ thống
    /// </summary>
    public class UISystemNotification : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// UI gốc chứa đối tượng Text
        /// </summary>
        [SerializeField]
        private RectTransform UITextRoot;

        /// <summary>
        /// Text Nội dung
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText;

        /// <summary>
        /// Vận tốc chạy
        /// </summary>
        [SerializeField]
        private float m_Velocity = 200;

        /// <summary>
        /// Thời gian nghỉ 
        /// </summary>
        [SerializeField]
        private float m_DelayTime = 0;
        #endregion

        #region Properties
        /// <summary>
        /// Khung có đang hiển thị không
        /// </summary>
        public bool IsShown
        {
            get
            {
                return this.UITextRoot.gameObject.activeSelf;
            }
        }

        /// <summary>
        /// Nội dung chữ chạy
        /// </summary>
        public string Text
        {
            get
            {
                return this.UIText.text;
            }
        }

        /// <summary>
        /// Vận tốc chữ chạy
        /// </summary>
        public float Velocity
        {
            get
            {
                return this.m_Velocity;
            }
            set
            {
                this.m_Velocity = value;
            }
        }

        /// <summary>
        /// Thời gian chờ đến khi lặp lại hoặc thực hiện ẩn đối tượng sau khi chạy xong nếu đã hết thời gian
        /// </summary>
        public float DelayTime
        {
            get
            {
                return this.m_DelayTime;
            }
            set
            {
                this.m_DelayTime = value;
            }
        }
        #endregion

        /// <summary>
        /// RectTransform đối tượng Text
        /// </summary>
        private RectTransform uiTextTransform;

        /// <summary>
        /// Danh sách chữ đang chờ đến lượt chạy
        /// </summary>
        private readonly Queue<string> waitingMessages = new Queue<string>();

        /// <summary>
        /// Luồng thực thi hiệu ứng
        /// </summary>
        private Coroutine coroutine;

        /// <summary>
        /// Đang chạy chữ
        /// </summary>
        private bool isPlaying;

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.InitPrefabs();
            this.StartCoroutine(this.StartCheckingData());
        }
        #endregion

        #region Core UI
        /// <summary>
        /// Khởi tạo ban đầu
        /// </summary>
        private void InitPrefabs()
        {
            this.uiTextTransform = this.UIText.gameObject.GetComponent<RectTransform>();
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Luồng này sẽ kiểm tra chừng nào hàng đợi còn thì sẽ thực thi hiệu ứng liên tục
        /// </summary>
        private IEnumerator StartCheckingData()
        {
            if (this.coroutine != null)
            {
                this.StopCoroutine(this.coroutine);
                this.coroutine = null;
            }

            this.isPlaying = false;
            while (true)
            {
                if (this.waitingMessages.Count > 0 && this.coroutine == null)
                {
                    this.Show();
                    this.isPlaying = true;
                    this.UIText.text = this.waitingMessages.Dequeue();
                    this.coroutine = this.StartCoroutine(this.DoPlay(() => {
                        this.StopCoroutine(this.coroutine);
                        this.coroutine = null;
                        this.isPlaying = false;
                    }));
                }
                else if (this.waitingMessages.Count <= 0 && !this.isPlaying)
                {
                    this.Hide();
                }
                yield return null;
            }
        }

        /// <summary>
        /// Thực hiện hiệu ứng chữ chạy
        /// </summary>
        /// <returns></returns>
        private IEnumerator DoPlay(Action callback)
        {
            Vector2 originalPos = this.uiTextTransform.anchoredPosition;
            Vector2 targetPos = new Vector2(-this.UIText.preferredWidth, this.uiTextTransform.anchoredPosition.y);
            Vector2 directionVector = targetPos - originalPos;
            float totalTime = 0f;
            float runningTime = Vector2.Distance(originalPos, targetPos) / this.m_Velocity;

            while (true)
            {
                totalTime += Time.deltaTime;
                float percent = totalTime / runningTime;
                this.uiTextTransform.anchoredPosition = originalPos + directionVector * percent;
                if (this.uiTextTransform.anchoredPosition.x < targetPos.x)
                {
                    break;
                }
                else
                {
                    yield return null;
                }
            }

            this.uiTextTransform.anchoredPosition = originalPos;
            yield return new WaitForSeconds(this.m_DelayTime);
            callback?.Invoke();
        }

        /// <summary>
        /// Hiển thị khung
        /// </summary>
        private void Show()
        {
            this.UITextRoot.gameObject.SetActive(true);
        }

        /// <summary>
        /// Ẩn khung
        /// </summary>
        private void Hide()
        {
            this.UITextRoot.gameObject.SetActive(false);
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Thêm Message vào danh sách phát
        /// </summary>
        /// <param name="message"></param>
        public void AddMessage(string message)
        {
            this.waitingMessages.Enqueue(message);
        }
        #endregion
    }
}
