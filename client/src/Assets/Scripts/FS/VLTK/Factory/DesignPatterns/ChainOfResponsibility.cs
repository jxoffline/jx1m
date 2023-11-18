using System;
using System.Collections.Generic;
using System.Linq;

namespace FS.VLTK.Factory.DesignPatterns
{
    /// <summary>
    /// Lớp mô tả Pattern [Chain of Responsibility]
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ChainOfResponsibility<T>
    {
        #region Private fields
        /// <summary>
        /// Danh sách các phần tử quản lý
        /// </summary>
        protected readonly List<T> elements = new List<T>();
        #endregion

        #region Core
        /// <summary>
        /// Cập nhật thay đổi tới tất cả các phần tử thành viên
        /// </summary>
        /// <param name="callback"></param>
        protected void NotifyAllElements(Action<T> callback)
        {
            foreach (T element in this.elements)
            {
                callback?.Invoke(element);
            }
        }

        /// <summary>
        /// Thêm phần tử vào danh sách quản lý
        /// </summary>
        /// <param name="element"></param>
        public void AddElement(T element)
        {
            this.elements.Add(element);
        }

        /// <summary>
        /// Xóa phần tử khỏi danh sách quản lý
        /// </summary>
        /// <param name="element"></param>
        public void RemoveElement(T element)
        {
            this.elements.Remove(element);
        }
        #endregion
    }
}
