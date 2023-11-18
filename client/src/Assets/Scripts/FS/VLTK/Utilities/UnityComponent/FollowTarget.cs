using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FS.VLTK.Utilities.UnityComponent
{
    /// <summary>
    /// Theo sau đối tượng
    /// </summary>
    public class FollowTarget : TTMonoBehaviour
    {
        #region Defines
        [SerializeField]
        private Transform _Target;
        #endregion

        /// <summary>
        /// Mục tiêu đi theo
        /// </summary>
        public Transform Target
        {
            get
            {
                return this._Target;
            }
            set
            {
                this._Target = value;
            }
        }

        /// <summary>
        /// Hàm này chạy liên tục mỗi frame
        /// </summary>
        private void Update()
        {
            if (this._Target != null)
            {
                Vector3 currentPos = this.transform.localPosition;
                this.transform.localPosition = new Vector3(this._Target.localPosition.x, this._Target.localPosition.y, currentPos.z);
            }
        }
    }
}
