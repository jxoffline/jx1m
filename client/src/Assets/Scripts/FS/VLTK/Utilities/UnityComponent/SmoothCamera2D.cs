using System.Collections;
using UnityEngine;

namespace FS.VLTK.Utilities.UnityComponent
{
    /// <summary>
    /// Đối tượng Camera theo sau mục tiêu
    /// </summary>
    public class SmoothCamera2D : TTMonoBehaviour
    {
        #region Define
        /// <summary>
        /// Mục tiêu
        /// </summary>
        public Transform Target;

        /// <summary>
        /// Thời gian chờ làm mượt
        /// </summary>
        [SerializeField]
        private float _DampTime = 0.05f;

        /// <summary>
        /// Vận tốc
        /// </summary>
        [SerializeField]
        private Vector3 _Velocity = Vector3.zero;

        /// <summary>
        /// Sử dụng làm mượt Camera
        /// </summary>
        [SerializeField]
        private bool _UseSmoothDampTime = true;
        #endregion

        /// <summary>
        /// Đối tượng Camera
        /// </summary>
        private Camera _Camera;

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            this._Camera = this.GetComponent<Camera>();
        }

        /// <summary>
        /// Hàm này gọi liên tục mỗi Frame trước khi Render
        /// </summary>
        private void OnPreRender()
        {
            if (this.Target != null)
            {
                if (!this._UseSmoothDampTime)
                {
                    this.gameObject.transform.localPosition = new Vector3(this.Target.localPosition.x, this.Target.localPosition.y, this.gameObject.transform.localPosition.z);
                }
                else
                {
                    Vector3 point = this._Camera.WorldToViewportPoint(this.Target.position);
                    Vector3 delta = this.Target.position - this._Camera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, point.z));
                    Vector3 destination = this.transform.position + delta;
                    transform.position = Vector3.SmoothDamp(transform.position, destination, ref this._Velocity, this._DampTime);
                }
            }
        }
        #endregion
    }
}
