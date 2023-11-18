using ProtoBuf;

namespace Server.Data
{
    /// <summary>
    /// Gói tin gửi từ Server về Client thông báo đạn nổ
    /// </summary>
    [ProtoContract]
    public class G2C_BulletExplode
    {
        /// <summary>
        /// ID đạn
        /// </summary>
        [ProtoMember(1)]
        public int BulletID { get; set; }

        /// <summary>
        /// ID Res của đạn
        /// </summary>
        [ProtoMember(2)]
        public int ResID { get; set; }

        /// <summary>
        /// Vị trí nổ X
        /// </summary>
        [ProtoMember(3)]
        public int PosX { get; set; }

        /// <summary>
        /// Vị trí nổ Y
        /// </summary>
        [ProtoMember(4)]
        public int PosY { get; set; }

        /// <summary>
        /// Thời gian Delay trước khi nổ
        /// </summary>
        [ProtoMember(5)]
        public float Delay { get; set; }

        /// <summary>
        /// ID mục tiêu
        /// <para>Làm như này để biểu diễn ở Client nổ chính xác vào mục tiêu thì nhìn sẽ thật hơn</para>
        /// </summary>
        [ProtoMember(6)]
        public int TargetID { get; set; }

        /// <summary>
        /// Tọa độ vị trí nổ
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
