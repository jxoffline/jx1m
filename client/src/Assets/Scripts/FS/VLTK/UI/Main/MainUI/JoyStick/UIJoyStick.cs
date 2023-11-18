using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace FS.VLTK.UI.Main.MainUI
{
    /// <summary>
    /// Khung quản lý JoyStick
    /// </summary>
    public class UIJoyStick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        #region Define
        /// <summary>
        /// Background của JoyStick
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Image UIImage_JoyStickBackground;

        /// <summary>
        /// Background của Thumb
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Image UIImage_ThumbBackground;

        /// <summary>
        /// Alpha tối thiểu
        /// </summary>
        [SerializeField]
        private float _MinAlpha = 0.3f;

        /// <summary>
        /// Alpha tối đa
        /// </summary>
        [SerializeField]
        private float _MaxAlpha = 1f;
        #endregion

        /// <summary>
        /// Đối tượng đang được ấn
        /// </summary>
        private bool mouseDown = false;

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            this.mouseDown = false;

            Color color = this.UIImage_JoyStickBackground.color;
            color.a = this._MinAlpha;
            this.UIImage_JoyStickBackground.color = color;

            Color _color = this.UIImage_ThumbBackground.color;
            _color.a = this._MinAlpha;
            this.UIImage_ThumbBackground.color = _color;
        }
        #endregion

        #region Implements
        /// <summary>
        /// Bắt sự kiện Click vào đối tượng
        /// </summary>
        /// <param name="pointerEventData"></param>
        public void OnPointerDown(PointerEventData pointerEventData)
        {
            this.mouseDown = true;

            Color color = this.UIImage_JoyStickBackground.color;
            color.a = this._MaxAlpha;
            this.UIImage_JoyStickBackground.color = color;

            Color _color = this.UIImage_ThumbBackground.color;
            _color.a = this._MaxAlpha;
            this.UIImage_ThumbBackground.color = _color;
        }

        /// <summary>
        /// Bắt sự kiện khi ngừng Click đối tượng
        /// </summary>
        /// <param name="pointerEventData"></param>
        public void OnPointerUp(PointerEventData pointerEventData)
        {
            if (this.mouseDown)
            {
                this.mouseDown = false;

                Color color = this.UIImage_JoyStickBackground.color;
                color.a = this._MinAlpha;
                this.UIImage_JoyStickBackground.color = color;

                Color _color = this.UIImage_ThumbBackground.color;
                _color.a = this._MinAlpha;
                this.UIImage_ThumbBackground.color = _color;
            }
        }
        #endregion
    }
}

