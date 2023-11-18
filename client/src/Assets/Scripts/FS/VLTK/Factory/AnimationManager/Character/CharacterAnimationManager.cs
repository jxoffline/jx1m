using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FS.VLTK.Factory.Animation
{
    /// <summary>
    /// Quản lý Animation người
    /// </summary>
    public partial class CharacterAnimationManager : TTMonoBehaviour
    {
        #region Singleton - Instance
        /// <summary>
        /// Đối tượng quản lý Animation người
        /// </summary>
        public static CharacterAnimationManager Instance { get; private set; }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            CharacterAnimationManager.Instance = this;
        }
        #endregion
    }
}
