using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;

namespace Server.Data
{
    /// <summary>
    /// Gói tin gửi từ Server về Client thông báo đối tượng thay đổi động tác
    /// </summary>
    [ProtoContract]
    public class G2C_SpriteChangeAction
    {
        /// <summary>
        /// ID đối tượng
        /// </summary>
        [ProtoMember(1)]
        public int RoleID { get; set; }

        /// <summary>
        /// Hướng của đối tượng
        /// </summary>
        [ProtoMember(2)]
        public int Direction { get; set; }

        /// <summary>
        /// Tọa độ X
        /// </summary>
        [ProtoMember(3)]
        public int PosX { get; set; }

        /// <summary>
        /// Tọa độ Y
        /// </summary>
        [ProtoMember(4)]
        public int PosY { get; set; }

        /// <summary>
        /// ID động tác
        /// </summary>
        [ProtoMember(5)]
        public int ActionID { get; set; }

        /// <summary>
        /// Vị trí của đối tượng
        /// </summary>
        public UnityEngine.Vector2 Position
        {
            get
            {
                return new UnityEngine.Vector2(this.PosX, this.PosY);
            }
        }
    }
}
