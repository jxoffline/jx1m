using FS.VLTK.Control.Component;
using FS.VLTK.Factory.DesignPatterns;
using UnityEngine;

namespace FS.VLTK.Factory.ObjectsManager
{
    /// <summary>
    /// Quản lý soi trước nhân vật
    /// </summary>
    public class KTRolePreviewManager : ChainOfResponsibility<CharacterPreview>
    {
        #region Singleton - Instance
        /// <summary>
        /// Quản lý soi trước nhân vật
        /// </summary>
        public static KTRolePreviewManager Instance { get; private set; }
        /// <summary>
        /// Tạo mới đối tượng quản lý soi trước nhân vật
        /// </summary>
        /// <returns></returns>
        public static KTRolePreviewManager NewInstance()
        {
            KTRolePreviewManager.Instance = new KTRolePreviewManager();
            return KTRolePreviewManager.Instance;
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Xóa toàn bộ các bản thể
        /// </summary>
        public void RemoveAllInstances()
        {
            /// Duyệt danh sách
            foreach (CharacterPreview rolePreview in this.elements)
            {
                /// Xóa toàn bộ các bản thể
                GameObject.Destroy(rolePreview.gameObject);
            }
            this.elements.Clear();
        }
        #endregion
    }
}
