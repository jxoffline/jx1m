using ProtoBuf;

namespace Server.Data
{
    /// <summary>
    /// Gói tin gửi từ Server về Client thông báo đối tượng tái sinh
    /// </summary>
    [ProtoContract]
    internal class MonsterRealiveData
    {
        /// <summary>
        /// ID
        /// </summary>
        [ProtoMember(1)]
        public int RoleID { get; set; }

        /// <summary>
        /// Tọa độ X
        /// </summary>
        [ProtoMember(2)]
        public int PosX { get; set; }

        /// <summary>
        /// Tọa độ Y
        /// </summary>
        [ProtoMember(3)]
        public int PosY { get; set; }

        /// <summary>
        /// Hướng quay
        /// </summary>
        [ProtoMember(4)]
        public int Direction { get; set; }

        /// <summary>
        /// Ngũ hành
        /// </summary>
        [ProtoMember(5)]
        public int Series { get; set; }

        /// <summary>
        /// Sinh lực hiện tại
        /// </summary>
        [ProtoMember(6)]
        public int CurrentHP { get; set; }

        /// <summary>
        /// Nội lực hiện tại
        /// </summary>
        [ProtoMember(7)]
        public int CurrentMP { get; set; }

        /// <summary>
        /// Thể lực hiện tại
        /// </summary>
        [ProtoMember(8)]
        public int CurrentStamina { get; set; }

        /// <summary>
        /// Vị trí tái sinh
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