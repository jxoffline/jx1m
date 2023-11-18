using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace FS.VLTK.UI.Main.Pet.Main
{
    /// <summary>
    /// Khung thông tin chú thích thêm của pet
    /// </summary>
    public class UIPet_MoreHintPopup : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Text chú thích
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Description;
        #endregion

        #region Properties
        /// <summary>
        /// Nội dung chú thích tương ứng
        /// </summary>
        public string Text
        {
            get
            {
                return this.UIText_Description.text;
            }
            set
            {
                this.UIText_Description.text = value;
            }
        }
        #endregion

        #region Private fields
        /// <summary>
        /// RectTransform của đối tượng
        /// </summary>
        private RectTransform transformBox;

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
            this.transformBox = this.UIText_Description.transform.parent.GetComponent<RectTransform>();
        }

        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            /// Đánh dấu đã chạy qua hàm Start
            this.isStarted = true;
            /// Xây lại giao diện
            this.Rebuild();
        }

        /// <summary>
        /// Hàm này gọi khi đối tượng được kích hoạt
        /// </summary>
        private void OnEnable()
        {
            /// Nếu chưa chạy qua hàm Start
            if (!this.isStarted)
            {
                /// Bỏ qua
                return;
            }
            /// Xây lại giao diện
            this.Rebuild();
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
        private void ExecuteSkipFrame(int skip, Action work)
        {
            this.StartCoroutine(this.DoExecuteSkipFrames(skip, work));
        }

        /// <summary>
        /// Xây lại giao diện
        /// </summary>
        private void Rebuild()
        {
            this.ExecuteSkipFrame(1, () =>
            {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(this.transformBox);
            });
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Hiện khung
        /// </summary>
        public void Show()
        {
            this.gameObject.SetActive(true);
        }

        /// <summary>
        /// Ẩn khung
        /// </summary>
        public void Hide()
        {
            this.gameObject.SetActive(false);
        }
        #endregion
    }
}
