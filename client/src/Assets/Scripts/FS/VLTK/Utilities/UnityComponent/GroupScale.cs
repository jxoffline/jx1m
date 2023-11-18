using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FS.VLTK.Utilities.UnityComponent
{
    /// <summary>
    /// Nhóm các đối tượng thay đổi độ co giãn
    /// </summary>
    public class GroupScale : TTMonoBehaviour
    {
        #region Define
        /// <summary>
        /// Các phần tử trong nhóm
        /// </summary>
        [SerializeField]
        private Transform[] _Members;

        /// <summary>
        /// Độ co giãn
        /// </summary>
        [SerializeField]
        private Vector3 _Scale;
        #endregion

        #region Properties
        /// <summary>
        /// Độ co giãn
        /// </summary>
        public Vector3 Scale
        {
            get
            {
                return this._Scale;
            }
            set
            {
                this._Scale = value;

                /// Duyệt danh sách và thay đổi độ co giãn của từng thành viên nhóm
                foreach (RectTransform member in this._Members)
                {
                    member.localScale = value;
                }
            }
        }
        #endregion
    }
}
