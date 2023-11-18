using UnityEngine;

namespace FS.VLTK.Utilities.UnityUI
{
    /// <summary>
    /// Thành phần này sẽ chuyển đối tượng lên thành con ở vị trí cha gần nhất
    /// </summary>
    public class UIFirstChild : MonoBehaviour
    {
        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi khi đối tượng được tạo ra
        /// </summary>
        private void Awake()
        {
            this.transform.SetAsFirstSibling();
        }
        #endregion
    }
}
