using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FS.VLTK.UI.Main.SeashellCircle
{
    /// <summary>
    /// Ô item trong khung Bách Bảo Rương
    /// </summary>
    public class UISeashellCircle_Item : MonoBehaviour
    {
        #region Define
        /// <summary>
        /// Hiệu ứng
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Image UIImage_Animation;

        /// <summary>
        /// Ảnh chọn đối tượng
        /// </summary>
        [SerializeField]
        private UnityEngine.UI.Image UIImage_Select;

        /// <summary>
        /// Thời gian thực thi hiệu ứng
        /// </summary>
        [SerializeField]
        private float AnimationDuration = 0.5f;
        #endregion

        #region Private fields
        /// <summary>
        /// Luồng thực thi hiệu ứng
        /// </summary>
        private Coroutine animationCoroutine = null;
        #endregion

        #region Properties
        /// <summary>
        /// Ô có được chọn không
        /// </summary>
        public bool Selected
        {
            get
            {
                return this.UIImage_Select.gameObject.activeSelf;
            }
            set
            {
                this.UIImage_Select.gameObject.SetActive(value);
            }
        }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            /// Thiết lập màu về trạng thái trong suốt
            this.ChangeAlpha(0f);
            /// Đánh dấu ô không được chọn
            this.Selected = false;
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Thay đổi Alpha tương ứng
        /// </summary>
        /// <param name="alpha"></param>
        private void ChangeAlpha(float alpha)
        {
            Color color = this.UIImage_Animation.color;
            color.a = alpha;
            this.UIImage_Animation.color = color;
        }

        /// <summary>
        /// Thực hiện ẩn
        /// </summary>
        /// <returns></returns>
        private IEnumerator FadeOut()
        {
            /// Thiết lập màu ban đầu
            this.ChangeAlpha(1f);
            /// Bỏ qua Frame
            yield return null;
            /// Thời gian tồn tại
            float lifeTime = 0f;
            /// Chừng nào chưa hết thời gian
            while (lifeTime < this.AnimationDuration)
            {
                /// Tăng thời gian tồn tại
                lifeTime += Time.deltaTime;
                /// % thời gian đã qua
                float percent = lifeTime / this.AnimationDuration;
                /// Đổi màu tương ứng
                this.ChangeAlpha(1f - percent);
                /// Bỏ qua Frame
                yield return null;
            }
            /// Thiết lập màu đích
            this.ChangeAlpha(0f);
            /// Hủy tham chiếu luồng thực thi
            this.animationCoroutine = null;
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Thực thi hiệu ứng
        /// </summary>
        public void PlayAnimation()
        {
            /// Ngừng luồng đang thực thi
            this.StopAnimation();
            /// Thực thi hiệu ứng
            this.animationCoroutine = this.StartCoroutine(this.FadeOut());
        }

        /// <summary>
        /// Ngừng thực thi hiệu ứng
        /// </summary>
        public void StopAnimation()
        {
            /// Nếu luồng thực thi hiệu ứng tồn tại
            if (this.animationCoroutine != null)
            {
                this.StopCoroutine(this.animationCoroutine);
                this.animationCoroutine = null;
            }
            /// Thiết lập màu về trạng thái trong suốt
            this.ChangeAlpha(0f);
        }
        #endregion
    }
}
