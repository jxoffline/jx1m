using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FS.VLTK.Utilities.UnityComponent
{
    /// <summary>
    /// Đối tượng tính giá trị FPS
    /// </summary>
    public class FPSCounter : TTMonoBehaviour
    {
        #region Private fields
        /// <summary>
        /// Tổng số Frame lần trước
        /// </summary>
        private int lastFrameCount = 0;

        /// <summary>
        /// Số Frame trong suốt khoảng tương ứng
        /// </summary>
        private float lastTouchTimer = 0;
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {

        }

        /// <summary>
        /// Hàm này gọi liên tục mỗi Frame
        /// </summary>
        private void Update()
        {
            this.Record();
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Cập nhật số FPS
        /// </summary>
        private void Record()
        {
            float t = Time.time;
            int fc = Time.frameCount;

            /// Tính số FPS
            MainGame.Instance.FPS = (fc - this.lastFrameCount) / (t - this.lastTouchTimer);

            this.lastFrameCount = fc;
            this.lastTouchTimer = t;
        }
        #endregion

        #region Public methods
        #endregion
    }
}
