using System;
using System.Collections.Generic;
using System.Linq;

namespace FS.VLTK.Logic
{
    /// <summary>
    /// Auto Pet
    /// </summary>
    public partial class KTAutoPetManager : TTMonoBehaviour
    {
        #region Singleton - Instance
        /// <summary>
        /// Luồng thực thi Auto pet
        /// </summary>
        public static KTAutoPetManager Instance { get; private set; }
        #endregion

        #region Private fields
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
            KTAutoPetManager.Instance = this;
        }

        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
		private void Start()
        {
            /// Đánh dấu đã chạy qua hàm Start
            this.isStarted = true;
            /// Khởi động luồng
            this.Restart();
        }
        #endregion
    }
}
