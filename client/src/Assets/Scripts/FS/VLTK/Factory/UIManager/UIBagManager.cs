using FS.VLTK.Factory.DesignPatterns;
using FS.VLTK.UI.Main.Bag;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FS.VLTK.Factory.UIManager
{
    /// <summary>
    /// Quản lý danh sách các Prefab UIBagGrid
    /// </summary>
    public class UIBagManager : ChainOfResponsibility<UIBag_Grid>
    {
        #region Singleton - Instance
        /// <summary>
        /// Quản lý danh sách các Prefab UIBagGrid
        /// </summary>
        public static UIBagManager Instance { get; private set; }

        /// <summary>
        /// Quản lý danh sách các Prefab UIBagGrid
        /// </summary>
        private UIBagManager() { }
        #endregion

        #region Core
        /// <summary>
        /// Tạo một bản thể mới của đối tượng
        /// </summary>
        public static void NewInstance()
        {
            UIBagManager.Instance = new UIBagManager();
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Tải lại danh sách các vật phẩm trong túi đồ
        /// </summary>
        public void Reload()
        {
            this.NotifyAllElements((uiBagGrid) => {
                uiBagGrid.Reload();
            });
        }

        /// <summary>
        /// Làm mới vật phẩm tại vị trí tương ứng
        /// </summary>
        /// <param name="itemGD"></param>
        public void RefreshItem(GoodsData itemGD)
        {
            this.NotifyAllElements((uiBagGrid) => {
                uiBagGrid.RefreshItem(itemGD);
            });
        }

        /// <summary>
        /// Thêm vật phẩm mới vào túi
        /// </summary>
        /// <param name="itemGD"></param>
        public void AddItem(GoodsData itemGD)
        {
            this.NotifyAllElements((uiBagGrid) => {
                uiBagGrid.AddItem(itemGD);
            });
        }
        
        /// <summary>
        /// Mở ô tương ứng trong túi
        /// </summary>
        /// <param name="slot"></param>
        public void OpenSlot(int slot)
        {
            this.NotifyAllElements((uiBagGrid) => {
                uiBagGrid.OpenSlot(slot);
            });
        }
        #endregion

        
    }
}
