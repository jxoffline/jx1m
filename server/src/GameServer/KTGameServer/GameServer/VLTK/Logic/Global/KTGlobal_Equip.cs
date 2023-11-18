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
        #region Trang bị

        /// <summary>
        /// Cấp độ cường hóa trang bị tối thiểu được tách Huyền Tinh
        /// </summary>
        public const int KD_MIN_ENHLEVEL_TO_SPLIT = 6;

        /// <summary>
        /// Hệ số giá trị Huyền Tinh có được so với gốc sau khi tách từ trang bị
        /// </summary>
        public const float KD_SPLIT_VALUE_COST = 0.7f;

        public const float KD_SPLITCRYTAL_VALUE_COST = 0.8f;

        public const int KT_MIN_LEVELMAGIC = 1;

        public const int KT_MAX_LEVELMAGIC = 7;

        #endregion Trang bị
    }
}
