using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using System.Collections;

namespace FS.VLTK.UI.Main.MainUI
{
    /// <summary>
    /// Khung chat kênh đặc biệt
    /// </summary>
    public class UISpecialChatBox : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Text nội dung
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI UIText_Content;

        /// <summary>
        /// Thời gian giữ ở kênh
        /// </summary>
        [SerializeField]
        private float _KeepTime = 10f;
        #endregion

        #region Private fields
        /// <summary>
        /// Luồng tự thực thi hiệu ứng ẩn
        /// </summary>
        private Coroutine autoFadeCoroutine = null;
        #endregion

        #region Properties
        /// <summary>
        /// Hiển thị không
        /// </summary>
        public bool Visible
        {
            get
            {
                return this.gameObject.activeSelf;
            }
            set
            {
                this.gameObject.SetActive(value);

                /// Xóa luồng tự ẩn cũ
                if (this.autoFadeCoroutine != null)
                {
                    this.StopCoroutine(this.autoFadeCoroutine);
                    this.autoFadeCoroutine = null;
                }

                /// Nếu là hiện
                if (value)
                {
                    this.autoFadeCoroutine = this.StartCoroutine(this.AutoFade());
                }
            }
        }

        /// <summary>
        /// Nội dung
        /// </summary>
        public string Content
        {
            get
            {
                return this.UIText_Content.text;
            }
            set
            {
                this.UIText_Content.text = value;
            }
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Luồng tự ẩn sau một khoảng thời gian
        /// </summary>
        /// <returns></returns>
        private IEnumerator AutoFade()
        {
            /// Đợi cho hết thời gian giữ
            yield return new WaitForSeconds(this._KeepTime);

            /// Thực hiện ẩn
            this.Visible = false;
        }
        #endregion
    }
}
