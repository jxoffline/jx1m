using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

namespace FS.VLTK.UI
{
    /// <summary>
    /// Đối tượng Debug Console
    /// </summary>
    public class UIDebugConsole : MonoBehaviour
    {
        #region Singleton Instance
        /// <summary>
        /// Đối tượng Debug Console
        /// </summary>
        public static UIDebugConsole Instance { get; private set; }
        #endregion

        #region Define
        /// <summary>
        /// Prefab Text
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_ContentPrefab;

        /// <summary>
        /// Đối tượng chứa Text
        /// </summary>
        [SerializeField]
        private RectTransform UITransform_Console;

        /// <summary>
        /// Button xóa
        /// </summary>
        [SerializeField]
        private Button UIButton_Clear;

        /// <summary>
        /// Button đóng khung
        /// </summary>
        [SerializeField]
        private Button UIButton_Close;

        /// <summary>
        /// Button phóng to/thu nhỏ
        /// </summary>
        [SerializeField]
        private Button UIButton_Zoom;

        /// <summary>
        /// Text Button phóng to/thu nhỏ
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_ButtonZoom;

        /// <summary>
        /// Kích thước khi phóng to
        /// </summary>
        [SerializeField]
        private Vector2 MaxSize;
        #endregion

        #region Properties
        /// <summary>
        /// Tổng số dòng tối đa
        /// </summary>
        public int MaxCapacity { get; set; } = 50;

        /// <summary>
        /// Đối tượng đang đạt kích thước cực đại
        /// </summary>
        public bool Maximized { get; private set; }
        #endregion

        #region Private fields
        /// <summary>
        /// Kích thước ban đầu
        /// </summary>
        private Vector2 originSize;

        /// <summary>
        /// RectTransform của khung
        /// </summary>
        private RectTransform rectTransform;

        /// <summary>
        /// Vị trí ban đầu
        /// </summary>
        private Vector2 originPos;
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            UIDebugConsole.Instance = this;
        }

        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.InitPrefabs();
        }
        #endregion

        #region Code UI
        /// <summary>
        /// Khởi tạo ban đầu
        /// </summary>
        private void InitPrefabs()
        {
            this.UIText_ButtonZoom.text = "Phóng to";
            this.Maximized = false;
            this.rectTransform = this.GetComponent<RectTransform>();
            this.originSize = this.rectTransform.sizeDelta;
            this.originPos = this.rectTransform.anchoredPosition;
            this.UIButton_Clear.onClick.AddListener(this.ButtonClear_Clicked);
            this.UIButton_Close.onClick.AddListener(this.ButtonClose_Clicked);
            this.UIButton_Zoom.onClick.AddListener(this.ButtonZoom_Clicked);
        }

        /// <summary>
        /// Sự kiện khi Button đóng khung được ấn
        /// </summary>
        private void ButtonClose_Clicked()
        {
            GameObject.Destroy(this.gameObject);
            UIDebugConsole.Instance = null;
        }

        /// <summary>
        /// Sự kiện khi Button xóa được ấn
        /// </summary>
        private void ButtonClear_Clicked()
        {
            this.ClearContent();
        }

        /// <summary>
        /// Sự kiện khi Button phóng to/thu nhỏ được ấn
        /// </summary>
        private void ButtonZoom_Clicked()
        {
            /// Nếu đang đạt kích thước cực đại
            if (this.Maximized)
            {
                this.UIText_ButtonZoom.text = "Phóng to";
                this.rectTransform.sizeDelta = this.originSize;
                this.rectTransform.anchoredPosition = this.originPos;
                this.ExecuteNextFrame(() => {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(this.UITransform_Console);
                });
                this.Maximized = false;
            }
            /// Nếu đang đạt kích thước cực tiểu
            else
            {
                this.UIText_ButtonZoom.text = "Thu nhỏ";
                this.rectTransform.sizeDelta = this.MaxSize;
                this.rectTransform.anchoredPosition = this.originPos;
                this.ExecuteNextFrame(() => {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(this.UITransform_Console);
                });
                this.Maximized = true;
            }
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Thực thi sự kiện ở Frame tiếp theo
        /// </summary>
        /// <param name="action"></param>
        private void ExecuteNextFrame(Action action)
        {
            IEnumerator DoExecute()
            {
                yield return null;
                action?.Invoke();
            }
            this.StartCoroutine(DoExecute());
        }

        /// <summary>
        /// Làm rỗng Debug Console
        /// </summary>
        private void ClearContent()
        {
            foreach (Transform child in this.UITransform_Console.transform)
            {
                if (child.gameObject != this.UIText_ContentPrefab.gameObject)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }
        }

        /// <summary>
        /// Thêm Text vào Debug Console
        /// </summary>
        /// <param name="text"></param>
        private void DoAddText(string text)
        {
            /// Nếu số dòng lớn hơn giới hạn
            if (this.UITransform_Console.transform.childCount - 1 >= this.MaxCapacity)
            {
                GameObject.Destroy(this.UITransform_Console.transform.GetChild(1).gameObject);
            }

            TextMeshProUGUI uiText = GameObject.Instantiate<TextMeshProUGUI>(this.UIText_ContentPrefab);
            uiText.transform.SetParent(this.UITransform_Console.transform, false);
            uiText.gameObject.SetActive(true);
            uiText.text = text;
            this.ExecuteNextFrame(() => {
                LayoutRebuilder.ForceRebuildLayoutImmediate(this.UITransform_Console);
            });
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Thêm text với tiền tố [LOG] ở đầu vào Debug Console
        /// </summary>
        /// <param name="text"></param>
        public void WriteLog(string text)
        {
            string content = string.Format("<b><color=#dedede>[LOG]</color></b> {0}", text);
            this.DoAddText(content);
        }

        /// <summary>
        /// Thêm text với tiền tố [SUCCESS] ở đầu vào Debug Console
        /// </summary>
        /// <param name="text"></param>
        public void WriteLogSuccess(string text)
        {
            string content = string.Format("<b><color=#7eff2e>[SUCCESS]</color></b> {0}", text);
            this.DoAddText(content);
        }

        /// <summary>
        /// Thêm text với tiền tố [WARNING] ở đầu vào Debug Console
        /// </summary>
        /// <param name="text"></param>
        public void WriteLogWarning(string text)
        {
            string content = string.Format("<b><color=#ff1f1f>[ERROR]</color></b> {0}", text);
            this.DoAddText(content);
        }

        /// <summary>
        /// Thêm text với tiền tố [ERROR] ở đầu vào Debug Console
        /// </summary>
        /// <param name="text"></param>
        public void WriteLogError(string text)
        {
            string content = string.Format("<b><color=#ffea70>[ERROR]</color></b> {0}", text);
            this.DoAddText(content);
        }

        /// <summary>
        /// Thêm text với tiền tố [INFO] ở đầu vào Debug Console
        /// </summary>
        /// <param name="text"></param>
        public void WriteLogInfo(string text)
        {
            string content = string.Format("<b><color=#70c1ff>[INFO]</color></b> {0}", text);
            this.DoAddText(content);
        }

        /// <summary>
        /// Thêm text với tiền tố [EXCEPTION] ở đầu vào Debug Console
        /// </summary>
        /// <param name="text"></param>
        public void WriteLogException(string text)
        {
            string content = string.Format("<b><color=#ffb92e>[EXCEPTION]</color></b> {0}", text);
            this.DoAddText(content);
        }
        #endregion
    }
}
