using System;
using System.Collections.Generic;
using System.Linq;

namespace FS.VLTK.Factory.AnimationManager
{
	/// <summary>
	/// Quản lý hiệu ứng đạn
	/// </summary>
	public partial class BulletAnimationManager : TTMonoBehaviour
	{
        #region Singleton - Instance
        /// <summary>
        /// Đối tượng quản lý Animation đạn
        /// </summary>
        public static BulletAnimationManager Instance { get; private set; }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            BulletAnimationManager.Instance = this;
        }
        #endregion
    }
}
