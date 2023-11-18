using FS.VLTK.Control.Component;
using FS.VLTK.Factory.DesignPatterns;
using UnityEngine;

namespace FS.VLTK.Factory.ObjectsManager
{
    /// <summary>
    /// Quản lý soi trước quái
    /// </summary>
    public class KTMonsterPreviewManager : ChainOfResponsibility<MonsterPreview>
    {
        #region Singleton - Instance
        /// <summary>
        /// Quản lý soi trước nhân vật
        /// </summary>
        public static KTMonsterPreviewManager Instance { get; private set; }
        /// <summary>
        /// Tạo mới đối tượng quản lý soi trước nhân vật
        /// </summary>
        /// <returns></returns>
        public static KTMonsterPreviewManager NewInstance()
        {
            KTMonsterPreviewManager.Instance = new KTMonsterPreviewManager();
            return KTMonsterPreviewManager.Instance;
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Xóa toàn bộ các bản thể
        /// </summary>
        public void RemoveAllInstances()
        {
            /// Duyệt danh sách
            foreach (MonsterPreview monsterPreview in this.elements)
            {
                /// Xóa toàn bộ các bản thể
                GameObject.Destroy(monsterPreview.gameObject);
            }
            this.elements.Clear();
        }
        #endregion
    }
}
