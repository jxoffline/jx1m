using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProtoBuf;

namespace Server.Data
{
    /// <summary>
    /// Gói tin gửi từ Server về Client thông báo đối tượng sử dụng kỹ năng
    /// </summary>
    [ProtoContract]
    public class G2C_UseSkill
    {
        /// <summary>
        /// ID đối tượng
        /// </summary>
        [ProtoMember(1)]
        public int RoleID { get; set; }

        /// <summary>
        /// Hướng quay của đối tượng
        /// </summary>
        [ProtoMember(2)]
        public int Direction { get; set; }

        /// <summary>
        /// ID kỹ năng
        /// </summary>
        [ProtoMember(3)]
        public int SkillID { get; set; }

        /// <summary>
        /// Vị trí X
        /// </summary>
        [ProtoMember(4)]
        public int PosX { get; set; }

        /// <summary>
        /// Vị trí Y
        /// </summary>
        [ProtoMember(5)]
        public int PosY { get; set; }

        /// <summary>
        /// Có phải động tác đánh mặc định không
        /// </summary>
        [ProtoMember(6)]
        public bool IsSpecialAttack { get; set; }

        /// <summary>
        /// Vị trí ra chiêu
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
