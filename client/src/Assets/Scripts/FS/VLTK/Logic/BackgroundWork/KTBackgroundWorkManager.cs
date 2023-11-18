using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FS.VLTK.Logic.BackgroundWork
{
    /// <summary>
    /// Thực thi các sự kiện ngầm
    /// </summary>
    public partial class KTBackgroundWorkManager : MonoBehaviour
    {
        #region Singleton Instance
        /// <summary>
        /// Thực thi các sự kiện ngầm
        /// </summary>
        public static KTBackgroundWorkManager Instance { get; private set; }
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            KTBackgroundWorkManager.Instance = this;
        }
        #endregion
    }
}
