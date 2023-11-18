using FS.GameEngine.Logic;
using FS.VLTK.UI;
using FS.VLTK.UI.CoreUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FS.VLTK.Factory.UIManager
{
    /// <summary>
    /// Quản lý UIBottomText
    /// </summary>
    public class UIBottomTextManager : TTMonoBehaviour
    {
        #region Define
        /// <summary>
        /// Dữ liệu Text
        /// </summary>
        public class TextData
        {
            /// <summary>
            /// Text tương ứng
            /// </summary>
            public string Text { get; set; }

            /// <summary>
            /// Màu tương ứng
            /// </summary>
            public Color Color { get; set; }

            /// <summary>
            /// Vị trí bắt đầu
            /// </summary>
            public Vector2 Offset { get; set; }

            /// <summary>
            /// Thời gian thực hiện hiệu ứng bay
            /// </summary>
            public float Duration { get; set; }
        }
        #endregion

        #region Singleton - Instance
        /// <summary>
        /// Quản lý UIBottomText
        /// </summary>
        public static UIBottomTextManager Instance { get; private set; }
        #endregion

        #region Constants
        /// <summary>
        /// Thời gian Delay mỗi lần thực thi 1 phần tử
        /// </summary>
        private const float ShowDelayEach = 0.75f;
        #endregion

        #region Private fields
        /// <summary>
        /// Danh sách các phần tử
        /// </summary>
        private readonly Queue<TextData> elements = new Queue<TextData>();
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            UIBottomTextManager.Instance = this;
        }

        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.StartCoroutine(this.DoLogic());
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Thực thi Logic
        /// </summary>
        /// <returns></returns>
        private IEnumerator DoLogic()
        {
            while (true)
            {
                /// Nếu tồn tại phần tử cần Hint
                if (this.elements.Count > 0)
                {
                    /// Lấy phần tử tương ứng ra khỏi danh sách
                    TextData textData = this.elements.Dequeue();

                    /// Tạo UI tương ứng
                    CanvasManager canvas = Global.MainCanvas.GetComponent<CanvasManager>();
                    UICanvasFlyingText uiText = KTUIElementPoolManager.Instance.Instantiate<UICanvasFlyingText>("UIBottomText");
                    /// Toác cái đéo gì đó
                    if (uiText == null)
                    {
                        /// Bỏ qua
                        goto CONTINUE;
                    }

                    /// Kích hoạt
                    uiText.gameObject.SetActive(true);
                    /// Thêm vào Canvas
                    canvas.AddUI(uiText, true);

                    /// Đổ dữ liệu
                    uiText.Text = textData.Text;
                    uiText.Color = textData.Color;
                    uiText.Offset = textData.Offset;
                    uiText.Duration = textData.Duration;

                    /// Thực hiện biểu diễn
                    uiText.Play();
                }

                CONTINUE:
                yield return new WaitForSeconds(UIBottomTextManager.ShowDelayEach);
            }
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Làm rỗng danh sách
        /// </summary>
        public void Clear()
        {
            this.elements.Clear();
        }

        /// <summary>
        /// Thêm thông báo
        /// </summary>
        /// <param name="text"></param>
        public void AddText(TextData text)
        {
            /// Thêm vào danh sách chờ
            this.elements.Enqueue(text);
        }
        #endregion
    }
}
