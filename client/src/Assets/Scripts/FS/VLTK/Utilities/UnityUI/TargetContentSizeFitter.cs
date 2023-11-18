using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FS.VLTK.Utilities.UnityUI
{
    /// <summary>
    /// Mở rộng Content Size Fitter cho Fit theo kích thước đối tượng nhắm tới
    /// </summary>
    [ExecuteAlways]
    public class TargetContentSizeFitter : TTMonoBehaviour
    {
        #region Define
        /// <summary>
        /// Mục tiêu
        /// </summary>
        [SerializeField]
        private RectTransform _Target;

        /// <summary>
        /// Chiều rộng tối đa
        /// </summary>
        [SerializeField]
        private float _MaxWidth;

        /// <summary>
        /// Chiều cao tối đa
        /// </summary>
        [SerializeField]
        private float _MaxHeight;

        /// <summary>
        /// Fit chiều ngang
        /// </summary>
        [SerializeField]
        private bool _FitWidth;

        /// <summary>
        /// Fit chiều dọc
        /// </summary>
        [SerializeField]
        private bool _FitHeight;
        #endregion

        /// <summary>
        /// RectTransform của đối tượng
        /// </summary>
        private RectTransform rectTransform;

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            this.rectTransform = this.GetComponent<RectTransform>();
        }

        /// <summary>
        /// Hàm này gọi liên tục mỗi Frame
        /// </summary>
        private void Update()
        {
            if (this._Target == null)
            {
                return;
            }
            if (this.rectTransform == null)
            {
                this.rectTransform = this.GetComponent<RectTransform>();
            }

            if (this._FitWidth && (this._Target.sizeDelta.x < this._MaxWidth || this._MaxWidth <= 0))
            {
                this.rectTransform.sizeDelta = new Vector2(this._Target.sizeDelta.x, this.rectTransform.sizeDelta.y);
            }
            else if (this._FitWidth && this._MaxWidth > 0)
            {
                this.rectTransform.sizeDelta = new Vector2(this._MaxWidth, this.rectTransform.sizeDelta.y);
            }

            if (this._FitHeight && (this._Target.sizeDelta.y < this._MaxHeight || this._MaxHeight <= 0))
            {
                this.rectTransform.sizeDelta = new Vector2(this.rectTransform.sizeDelta.x, this._Target.sizeDelta.y);
            }
            else if (this._FitHeight && this._MaxHeight > 0)
            {
                this.rectTransform.sizeDelta = new Vector2(this.rectTransform.sizeDelta.x, this._MaxHeight);
            }
        }
        #endregion
    }
}
