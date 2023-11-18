using System;
using UnityEngine;
using TMPro;
using System.Collections;

namespace FS.VLTK.UI.Main.MainUI.EventBroadboardMini
{
    /// <summary>
    /// Nội dung trong khung hoạt động sự kiện phụ bản Mini
    /// </summary>
    public class UIEventBroadboardMini_Content : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Text nội dung
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Text;
        #endregion

        #region Properties
        /// <summary>
        /// Text nội dung
        /// </summary>
        public string Text
        {
            get
            {
                return this.UIText_Text.text;
            }
            set
            {
                this.UIText_Text.text = value;
            }
        }
        #endregion

        #region Private fields
        /// <summary>
        /// RectTransform text
        /// </summary>
        private RectTransform transformText;

        /// <summary>
        /// RectTransform nội dung
        /// </summary>
        private RectTransform transformContent;

        /// <summary>
        /// Đã chạy qua hàm Start chưa
        /// </summary>
        private bool isStarted = false;
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            this.transformText = this.UIText_Text.GetComponent<RectTransform>();
            this.transformContent = this.GetComponent<RectTransform>();
        }

        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.InitPrefabs();

            /// Thực hiện xây lại giao diện
            this.ExecuteSkipFrames(1, () =>
            {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.transformContent);
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.transformText);
            });

            /// Đánh dấu đã chạy qua hàm Start
            this.isStarted = true;
        }

        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void OnEnable()
        {
            /// Nếu chưa chạy qua hàm Start
            if (!this.isStarted)
            {
                /// Bỏ qua
                return;
            }

            /// Thực hiện xây lại giao diện
            this.ExecuteSkipFrames(1, () =>
            {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.transformContent);
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.transformText);
            });
        }
        #endregion

        #region Code UI
        /// <summary>
        /// Khởi tạo ban đầu
        /// </summary>
        private void InitPrefabs()
        {

        }
        #endregion

        #region Private methods
        /// <summary>
        /// Luồng thực thi sự kiện bỏ qua một số Frame
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="work"></param>
        /// <returns></returns>
        private IEnumerator DoExecuteSkipFrames(int skip, Action work)
        {
            for (int i = 1; i <= skip; i++)
            {
                yield return null;
            }
            work?.Invoke();
        }

        /// <summary>
        /// Thực thi sự kiện bỏ qua một số Frame
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="work"></param>
        private void ExecuteSkipFrames(int skip, Action work)
        {
            this.StartCoroutine(this.DoExecuteSkipFrames(skip, work));
        }
        #endregion
    }
}
