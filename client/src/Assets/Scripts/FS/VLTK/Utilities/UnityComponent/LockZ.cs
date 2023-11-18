using UnityEngine;

namespace FS.VLTK.Utilities.UnityComponent
{
    /// <summary>
    /// Cố định đối tượng theo trục Z
    /// </summary>
    public class LockZ : TTMonoBehaviour
    {
        /// <summary>
        /// Giá trị Z cố định
        /// </summary>
        public float FixedZ;

        /// <summary>
        /// Hàm này gọi liên tục mỗi Frame
        /// </summary>
        private void Update()
        {
            Vector3 pos3 = this.transform.localPosition;
            this.transform.localPosition = new Vector3(pos3.x, pos3.y, this.FixedZ);
        }
    }
}

