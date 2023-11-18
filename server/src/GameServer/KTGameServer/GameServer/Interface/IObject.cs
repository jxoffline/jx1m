using GameServer.Logic;
using System.Windows;

namespace GameServer.Interface
{
    /// <summary>
    /// Interface cho toàn bộ đối tượng di động
    /// </summary>
    public interface IObject
    {
        /// <summary>
        /// Loại đối tượng
        /// </summary>
        ObjectTypes ObjectType
        {
            get;
        }

        /// <summary>
        /// Tọa độ lưới hiện tại
        /// </summary>
        Point CurrentGrid { get; set; }

        /// <summary>
        /// Tọa độ thực hiện tại
        /// </summary>
        Point CurrentPos { get; set; }

        /// <summary>
        /// ID bản đồ hiện tại
        /// </summary>
        int CurrentMapCode { get; }

        /// <summary>
        /// ID phụ bản hiện tại
        /// </summary>
        int CurrentCopyMapID { get; }

        /// <summary>
        /// Hướng quay hiện tại
        /// </summary>
        KiemThe.Entities.Direction CurrentDir { get; set; }
    }
}