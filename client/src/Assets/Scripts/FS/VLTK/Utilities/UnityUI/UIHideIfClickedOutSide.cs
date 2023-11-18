using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FS.VLTK.Utilities.UnityUI
{
    /// <summary>
    /// Khung tự động ẩn khi Click bên ngoài
    /// </summary>
    public class UIHideIfClickedOutSide : MonoBehaviour
    {
        #region Private fields
        /// <summary>
        /// RectTransform
        /// </summary>
        private RectTransform rectTransform = null;
        #endregion

        #region Properties
        /// <summary>
        /// Sự kiện ẩn khung
        /// </summary>
        public Action Hidden { get; set; }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            this.rectTransform = this.GetComponent<RectTransform>();
        }

        /// <summary>
        /// Hàm này gọi khi đối tượng được kích hoạt
        /// </summary>
        private void OnEnable()
        {
            this.StartCoroutine(this.CheckEveryFrame());
        }

        /// <summary>
        /// Hàm này gọi khi đối tượng bị hủy kích hoạt
        /// </summary>
        private void OnDisable()
        {
            this.StopAllCoroutines();
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Thực thi liên tục mỗi Frame
        /// </summary>
        /// <returns></returns>
        private IEnumerator CheckEveryFrame()
        {
            /// Bỏ qua Frame đầu
            yield return null;
            /// Lặp liên tục
            while (true)
            {
                /// Kiểm tra nếu Click bên ngoài thì ẩn
                yield return this.HideIfClickedOutside();
            }
        }

        /// <summary>
        /// Kiểm tra và tự ẩn nếu Click bên ngoài
        /// </summary>
        private IEnumerator HideIfClickedOutside()
        {
            if (Application.isEditor || Application.platform == RuntimePlatform.WindowsPlayer)
            {
                /// Nếu không Click
                if (!Input.GetMouseButtonDown(0))
                {
                    yield break;
                }
            }
            else
            {
                /// Nếu không chạm
                if (Input.touchCount <= 0)
                {
                    yield break;
                }
                /// Nếu có chạm
                else
                {
                    /// Thông tin chạm
                    Touch touch = Input.GetTouch(0);
                    /// Nếu là giữ hoặc ngắt chạm
                    if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Stationary)
                    {
                        /// Bỏ qua
                        yield break;
                    }
                }
            }

            /// Nếu không Click vào đối tượng
            if (!Utils.IsClickOnGUI(this.rectTransform))
            {
                /// Đợi
                yield return new WaitForSeconds(0.1f);
                /// Hủy kích hoạt đối tượng
                this.gameObject.SetActive(false);
                /// Thực thi sự kiện khi khung bị hủy
                this.Hidden?.Invoke();
                /// Thoát
                yield break;
            }

            /// Đợi đến khi kết thúc Frame
            yield return null;
        }
        #endregion
    }
}
