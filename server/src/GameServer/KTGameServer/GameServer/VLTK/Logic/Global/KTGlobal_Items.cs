using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.KiemThe
{
    /// <summary>
    /// Các phương thức và đối tượng toàn cục của Kiếm Thế
    /// </summary>
    public static partial class KTGlobal
    {
        #region Thẻ đổi tên

        /// <summary>
        /// ID vật phẩm Thẻ đổi tên
        /// </summary>
        public const int ChangeNameCardItemID = 2167;

        #endregion

        #region Quan Thiên Quyển

        /// <summary>
        /// Vật phẩm Quan Thiên Quyển dùng để kiểm tra vị trí người chơi
        /// </summary>
        public static List<int> CheckPositionScroll { get; } = new List<int>()
        {
            205,
        };

        #endregion Quan Thiên Quyển

        #region Loa chat

        /// <summary>
        /// Vật phẩm dùng trong kênh Chat đặc biệt
        /// </summary>
        public static List<int> SpecialChatMaterial { get; } = new List<int>()
        {
            382,
        };

        /// <summary>
        /// Vật phẩm dùng trong kênh Chat liên máy chủ
        /// </summary>
        public static List<int> CrossServerChatMaterial { get; } = new List<int>()
        {
            383,
        };

        #endregion Loa chat

        #region Thuốc hồi sinh
        /// <summary>
        /// Vật phẩm Cửu Chuyển Tục Mệnh Hoàn
        /// </summary>
        public static List<int> ReviveMedicine { get; } = new List<int>()
        {
            
        };
        #endregion
    }
}
