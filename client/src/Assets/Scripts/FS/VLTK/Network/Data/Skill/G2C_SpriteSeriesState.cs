using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;

namespace Server.Data
{
    /// <summary>
    /// Gói tin gửi từ Server về Client thông báo trạng thái ngũ hành của đối tượng
    /// </summary>
    [ProtoContract]
    public class G2C_SpriteSeriesState
    {
        /// <summary>
        /// ID đối tượng
        /// </summary>
        [ProtoMember(1)]
        public int RoleID { get; set; }

        /// <summary>
        /// ID trạng thái
        /// </summary>
        [ProtoMember(2)]
        public int SeriesID { get; set; }

        /// <summary>
        /// Thời gian duy trì (s)
        /// </summary>
        [ProtoMember(3)]
        public float Time { get; set; }

        /// <summary>
        /// Loại thông báo
        /// <para>0: Xóa</para>
        /// <para>1: Thêm</para>
        /// </summary>
        [ProtoMember(4)]
        public int Type { get; set; }

        /// <summary>
        /// Tọa độ bị giật X
        /// </summary>
        [ProtoMember(5)]
        public int DragPosX { get; set; }

        /// <summary>
        /// Tọa độ bị giật Y
        /// </summary>
        [ProtoMember(6)]
        public int DragPosY { get; set; }
    }
}
