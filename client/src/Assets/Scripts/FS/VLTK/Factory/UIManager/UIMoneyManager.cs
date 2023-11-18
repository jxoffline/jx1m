using FS.VLTK.Factory.DesignPatterns;
using FS.VLTK.UI.Main.Money;
using System;
using System.Collections.Generic;
using System.Linq;
using static FS.VLTK.Entities.Enum;

namespace FS.VLTK.Factory.UIManager
{
    /// <summary>
    /// Quản lý danh sách hiển thị tiền
    /// </summary>
    public class UIMoneyManager : ChainOfResponsibility<UIMoneyBox>
    {
        #region Singleton - Instance
        /// <summary>
        /// Quản lý danh sách hiển thị tiền
        /// </summary>
        public static UIMoneyManager Instance { get; private set; }

        /// <summary>
        /// Quản lý danh sách hiển thị tiền
        /// </summary>
        private UIMoneyManager()
        {

        }

        /// <summary>
        /// Tạo bản thể mới của đối tượng
        /// </summary>
        public static void NewInstance()
        {
            UIMoneyManager.Instance = new UIMoneyManager();
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Cập nhật giá trị tiền tệ
        /// </summary>
        /// <param name="type"></param>
        public void UpdateValue(MoneyType type)
        {
            this.NotifyAllElements((uiMoneyBox) => {
                if (uiMoneyBox.Type == type)
                {
                    uiMoneyBox.Refresh();
                }
            });
        }
        #endregion
    }
}
