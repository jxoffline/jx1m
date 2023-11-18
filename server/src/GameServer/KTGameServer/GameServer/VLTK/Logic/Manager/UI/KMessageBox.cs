using GameServer.Logic;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Đối tượng bảng thông báo
    /// </summary>
    public class KMessageBox
    {
        /// <summary>
        /// ID tự tăng
        /// </summary>
        private static int AutoID = -1;

        /// <summary>
        /// ID bảng
        /// </summary>
        public int ID { get; private set; }

        /// <summary>
        /// Loại bảng thông báo
        /// <para>0: Bảng thông báo thường, Params = {}</para>
        /// <para>1: Bảng nhập số, Params = {Giá trị MIN, Giá trị MAX, Giá trị mặc định ban đầu}</para>
        /// </summary>
        public int MessageType { get; set; }

        /// <summary>
        /// Chủ nhân bảng thông báo
        /// </summary>
        public KPlayer Owner { get; set; }

        /// <summary>
        /// Tiêu đề bảng
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Nội dung bảng thông báo
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Danh sách các tham biến đi kèm
        /// </summary>
        public List<string> Parameters { get; set; }

        /// <summary>
        /// Tạo bảng thông báo
        /// </summary>
        public KMessageBox()
        {
            KMessageBox.AutoID = (KMessageBox.AutoID + 1) % 100000007;
            this.ID = KMessageBox.AutoID;
        }
    }

    /// <summary>
    /// Đối tượng bảng thông báo loại 0
    /// </summary>
    public class KTMessageBox : KMessageBox
    {
        /// <summary>
        /// Sự kiện OK
        /// </summary>
        public Action OK { get; set; }

        /// <summary>
        /// Sự kiện Cancel
        /// </summary>
        public Action Cancel { get; set; }

        /// <summary>
        /// Đối tượng bảng thông báo loại 0
        /// </summary>
        public KTMessageBox() : base()
        {

        }
    }

    /// <summary>
    /// Đối tượng bảng thông báo loại 1
    /// </summary>
    public class KTInputNumberBox : KMessageBox
    {
        /// <summary>
        /// Sự kiện OK
        /// </summary>
        public Action<int> OK { get; set; }

        /// <summary>
        /// Sự kiện Cancel
        /// </summary>
        public Action Cancel { get; set; }

        /// <summary>
        /// Bảng thông báo loại 1
        /// </summary>
        public KTInputNumberBox() : base()
        {

        }
    }

    /// <summary>
    /// Đối tượng bảng thông báo loại 2
    /// </summary>
    public class KTInputStringBox : KMessageBox
    {
        /// <summary>
        /// Sự kiện OK
        /// </summary>
        public Action<string> OK { get; set; }

        /// <summary>
        /// Sự kiện Cancel
        /// </summary>
        public Action Cancel { get; set; }

        /// <summary>
        /// Bảng thông báo loại 2
        /// </summary>
        public KTInputStringBox() : base()
        {

        }
    }
}
