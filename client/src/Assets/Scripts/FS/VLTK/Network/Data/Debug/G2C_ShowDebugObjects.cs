using System;
using ProtoBuf;

namespace Server.Data
{
    /// <summary>
    /// Gói tin gửi từ Server về Client yêu cầu hiện Debug Object tại vị trí tương ứng
    /// </summary>
    [ProtoContract]
    public class G2C_ShowDebugObject
    {
        /// <summary>
        /// Tọa độ X
        /// </summary>
        [ProtoMember(1)]
        public int PosX { get; set; }

        /// <summary>
        /// Tọa độ Y
        /// </summary>
        [ProtoMember(2)]
        public int PosY { get; set; }

        /// <summary>
        /// Kích thước Debug Object
        /// </summary>
        [ProtoMember(3)]
        public int Size { get; set; }

        /// <summary>
        /// Thời gian tồn tại
        /// </summary>
        [ProtoMember(4)]
        public float LifeTime { get; set; }

        /// <summary>
        /// Vị trí đối tượng
        /// </summary>
        public UnityEngine.Vector2 Pos
        {
            get
            {
                return new UnityEngine.Vector2(this.PosX, this.PosY);
            }
        }
    }
}
