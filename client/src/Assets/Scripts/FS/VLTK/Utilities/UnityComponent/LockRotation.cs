using UnityEngine;

namespace FS.VLTK.Utilities.UnityComponent
{
    /// <summary>
    /// Khóa xoay đối tượng
    /// </summary>
    public class LockRotation : TTMonoBehaviour
    {
        /// <summary>
        /// Độ quay cố định
        /// </summary>
        public Vector3 Degree = Vector3.zero;

        /// <summary>
        /// Chạy liên tục mỗi Frame để đồng bộ
        /// </summary>
        private void Update()
        {
            this.transform.localRotation = Quaternion.Euler(this.Degree);
        }
    }
}