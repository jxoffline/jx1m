using FS.VLTK.Factory.DesignPatterns;
using FS.VLTK.UI.Main;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FS.VLTK.Factory.UIManager
{
    /// <summary>
    /// Quản lý UIRoleAvarta
    /// </summary>
    public class UIRoleAvartaManager : ChainOfResponsibility<UIRoleAvarta>
    {
        #region Singleton - Instance
        /// <summary>
        /// Quản lý danh sách hiển thị tiền
        /// </summary>
        public static UIRoleAvartaManager Instance { get; private set; }

        /// <summary>
        /// Quản lý danh sách hiển thị tiền
        /// </summary>
        private UIRoleAvartaManager()
        {

        }

        /// <summary>
        /// Tạo bản thể mới của đối tượng
        /// </summary>
        public static void NewInstance()
        {
            UIRoleAvartaManager.Instance = new UIRoleAvartaManager();
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Cập nhật thay đổi Avarta nhân vật
        /// </summary>
        /// <param name="type"></param>
        public void UpdateAvarta(int roleID, int avartaID)
        {
            this.NotifyAllElements((uiRoleAvarta) => {
                if (uiRoleAvarta.RoleID == roleID)
                {
                    uiRoleAvarta.AvartaID = avartaID;
                }
            });
        }
        #endregion
    }
}
