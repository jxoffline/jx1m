using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FS.VLTK.Utilities.UnityComponent
{
    /// <summary>
    /// Hiệu ứng đung đưa
    /// </summary>
    public class SwingSprite : TTMonoBehaviour
    {
        #region Define
        /// <summary>
        /// Vector vị trí ban đầu
        /// </summary>
        [SerializeField]
        private Vector3 _From;

        /// <summary>
        /// Vector vị trí đích
        /// </summary>
        [SerializeField]
        private Vector3 _To;

        /// <summary>
        /// Thời gian thực thi
        /// </summary>
        [SerializeField]
        private float _Duration;

        /// <summary>
        /// Tự thực thi
        /// </summary>
        [SerializeField]
        private bool _AutoPlay;
        #endregion

        #region Private fields
        /// <summary>
        /// Luồng thực thi hiệu ứng
        /// </summary>
        private Coroutine animationCoroutine;
        #endregion

        #region Properties
        /// <summary>
        /// Vector vị trí bắt đầu
        /// </summary>
        public Vector3 From
        {
            get
            {
                return this._From;
            }
            set
            {
                this._From = value;
            }
        }

        /// <summary>
        /// Vị trí kết thúc
        /// </summary>
        public Vector3 To
        {
            get
            {
                return this._To;
            }
            set
            {
                this._To = value;
            }
        }

        /// <summary>
        /// Thời gian thực thi
        /// </summary>
        public float Duration
        {
            get
            {
                return this._Duration;
            }
            set
            {
                this._Duration = value;
            }
        }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            if (this._AutoPlay)
            {
                this.Play();
            }
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Thực thi hiệu ứng đung đưa
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        private IEnumerator DoSwing(Vector3 from, Vector3 to)
        {
            /// Vector hướng
            Vector3 dirVector = to - from;
            /// Thiết lập vị trí ban đầu
            this.transform.localRotation = Quaternion.Euler(from);
            /// Bỏ qua Frame
            yield return null;

            /// Thời gian tồn tại
            float lifeTime = 0f;
            /// Lặp liên tục chừng nào còn tồn tại
            while (lifeTime < (this._Duration / 2))
            {
                /// Tăng thời gian
                lifeTime += Time.deltaTime;
                /// Cập nhật % tiến độ
                float percent = lifeTime / (this._Duration / 2);
                /// Vị trí mới
                Vector3 newPos = from + dirVector * percent;
                /// Cập nhật vị trí mới
                this.transform.localRotation = Quaternion.Euler(newPos);
                /// Bỏ qua Frame
                yield return null;
            }

            /// Thiết lập vị trí đích
            this.transform.localRotation = Quaternion.Euler(to);
            /// Bỏ qua Frame
            yield return null;
        }

        /// <summary>
        /// Thực thi hiệu ứng
        /// </summary>
        /// <returns></returns>
        private IEnumerator DoAnimation()
        {
            while (true)
            {
                yield return this.DoSwing(this._From, this._To);
                yield return this.DoSwing(this._To, this._From);
            }
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Ngừng thực thi hiệu ứng
        /// </summary>
        public void Stop()
        {
            if (this.animationCoroutine != null)
            {
                this.StopCoroutine(this.animationCoroutine);
                this.animationCoroutine = null;
            }
        }

        /// <summary>
        /// Thực thi hiệu ứng
        /// </summary>
        public void Play()
        {
            /// Nếu đối tượng không được kích hoạt
            if (!this.gameObject)
            {
                return;
            }
            /// Ngừng thực thi hiệu ứng cũ
            this.Stop();
            /// Thực thi hiệu ứng
            this.animationCoroutine = this.StartCoroutine(this.DoAnimation());
        }
        #endregion
    }
}
