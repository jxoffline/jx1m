using System;
using System.Collections.Generic;
using System.Linq;

namespace FS.VLTK.Factory.Animation
{
    /// <summary>
    /// Quản lý Animation hiệu ứng
    /// </summary>
    public partial class EffectAnimationManager : TTMonoBehaviour
    {
        #region Singleton - Instance
        /// <summary>
        /// Đối tượng quản lý Animation hiệu ứng
        /// </summary>
        public static EffectAnimationManager Instance { get; private set; }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            EffectAnimationManager.Instance = this;
        }
        #endregion
    }
}
