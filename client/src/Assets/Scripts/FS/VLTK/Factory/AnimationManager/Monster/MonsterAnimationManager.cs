using System;
using System.Collections.Generic;
using System.Linq;

namespace FS.VLTK.Factory.Animation
{
    /// <summary>
    /// Quản lý Animation quái
    /// </summary>
    public partial class MonsterAnimationManager : TTMonoBehaviour
    {
        #region Singleton - Instance
        /// <summary>
        /// Đối tượng quản lý Animation quái
        /// </summary>
        public static MonsterAnimationManager Instance { get; private set; }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            MonsterAnimationManager.Instance = this;
        }
        #endregion
    }
}
