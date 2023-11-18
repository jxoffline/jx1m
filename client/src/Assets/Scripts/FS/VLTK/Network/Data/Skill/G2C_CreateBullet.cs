using ProtoBuf;

namespace Server.Data
{
    /// <summary>
    /// Gói tin gửi về Client từ Server thông báo tạo đạn tương ứng
    /// </summary>
    [ProtoContract]
    public class G2C_CreateBullet
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
        /// Tọa độ xuất phát X
        /// </summary>
        [ProtoMember(3)]
        public int FromX { get; set; }

        /// <summary>
        /// Tọa độ xuất phát Y
        /// </summary>
        [ProtoMember(4)]
        public int FromY { get; set; }

        /// <summary>
        /// Tọa độ xuất phát
        /// </summary>
        public UnityEngine.Vector2 FromPos
        {
            get
            {
                return new UnityEngine.Vector2(this.FromX, this.FromY);
            }
        }

        /// <summary>
        /// Tọa độ đích X
        /// <para>Nếu là bẫy hoặc đạn đuổi mục tiêu thì không có tác dụng</para>
        /// </summary>
        [ProtoMember(5)]
        public int ToX { get; set; }

        /// <summary>
        /// Tọa độ đích Y
        /// <para>Nếu là bẫy hoặc đạn đuổi mục tiêu thì không có tác dụng</para>
        /// </summary>
        [ProtoMember(6)]
        public int ToY { get; set; }

        /// <summary>
        /// Tọa độ đích
        /// <para>Nếu là bẫy hoặc đạn đuổi mục tiêu thì không có tác dụng</para>
        /// </summary>
        public UnityEngine.Vector2 ToPos
        {
            get
            {
                return new UnityEngine.Vector2(this.ToX, this.ToY);
            }
        }

        /// <summary>
        /// ID mục tiêu
        /// <para>Nếu là đạn đuổi thì giá trị này mới tồn tại</para>
        /// <para>Nếu mục tiêu tồn tại thì ToPos vô nghĩa</para>
        /// <para>Nếu là bẫy thì giá trị này không có tác dụng</para>
        /// </summary>
        [ProtoMember(7)]
        public int TargetID { get; set; }

        /// <summary>
        /// Vận tốc bay
        /// <para></para>
        /// </summary>
        [ProtoMember(8)]
        public int Velocity { get; set; }

        /// <summary>
        /// Thời gian tồn tại (giây)
        /// </summary>
        [ProtoMember(9)]
        public float LifeTime { get; set; }

        /// <summary>
        /// Lặp lại thời gian hiệu ứng
        /// </summary>
        [ProtoMember(10)]
        public bool LoopAnimation { get; set; }

        /// <summary>
        /// Thời gian Delay trước khi ra đạn
        /// </summary>
        [ProtoMember(11)]
        public float Delay { get; set; }

        /// <summary>
        /// Đối tượng xuất chiêu
        /// </summary>
        [ProtoMember(12)]
        public int CasterID { get; set; }

        /// <summary>
        /// Theo đối tượng ra chiêu
        /// </summary>
        [ProtoMember(13)]
        public bool FollowCaster { get; set; }

        /// <summary>
        /// Bán kính di chuyển theo đường tròn
        /// </summary>
        [ProtoMember(14)]
        public float CircleRadius { get; set; }

        /// <summary>
        /// Tạo độ X Vector chỉ hướng di chuyển theo đường tròn
        /// </summary>
        [ProtoMember(15)]
        public int CircleDirVectorX { get; set; }

        /// <summary>
        /// Tạo độ Y Vector chỉ hướng di chuyển theo đường tròn
        /// </summary>
        [ProtoMember(16)]
        public int CircleDirVectorY { get; set; }

        /// <summary>
        /// Vector chỉ hướng di chuyển theo đường tròn
        /// </summary>
        public UnityEngine.Vector2 DirVector
        {
            get
            {
                return new UnityEngine.Vector2(this.CircleDirVectorX, this.CircleDirVectorY);
            }
            set
            {
                this.CircleDirVectorX = (int) value.x;
                this.CircleDirVectorY = (int) value.y;
            }
        }

        /// <summary>
        /// Quay trở lại vị trí ban đầu không
        /// </summary>
        [ProtoMember(17)]
        public bool Comeback { get; set; }
    }
}
