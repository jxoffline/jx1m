using FS.Drawing;
using UnityEngine;
using FS.GameEngine.Logic;
using static FS.VLTK.Entities.Enum;

namespace FS.GameEngine.Interface
{
    /// <summary>
    /// Đối tượng trong bản đồ
    /// </summary>
	public interface IObject
    {
        /// <summary>
        /// BaseID đối tượng, để phân biệt với các đối tượng khác
        /// </summary>
        int BaseID { get; set; }

        /// <summary>
        /// Tên đối tượng
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Đã khởi tạo chưa
        /// </summary>
        bool InitStatus { get; }

        /// <summary>
        /// Script điều khiển nhân vật 2D
        /// </summary>
        GameObject Role2D { get; set; }

        /// <summary>
        /// Tọa độ
        /// </summary>
        Point Coordinate { set; get; }

        /// <summary>
        /// Tọa độ thực X
        /// </summary>
        int PosX { get; set; }

        /// <summary>
        /// Tọa độ thực Y
        /// </summary>
        int PosY { get; set; }

        /// <summary>
        /// Khởi tạo đối tượng
        /// </summary>
        void Start();

        /// <summary>
        /// Xóa đối tượng
        /// </summary>
        void Destroy();

        /// <summary>
        /// Hàm này gọi liên tục tương tự hàm Update
        /// </summary>
        /// <param name="time"></param>
        void OnFrameRender();

        /// <summary>
        /// Loại đối tượng
        /// </summary>
        GSpriteTypes SpriteType { get; set; }

        /// <summary>
        /// Động tác hiện tại
        /// </summary>
        KE_NPC_DOING CurrentAction { get; }

        /// <summary>
        /// Hướng quay hiện tại
        /// </summary>
        Direction Direction { get; set; }
    }
}
