using ProtoBuf;

namespace Server.Data
{
    /// <summary>
    /// Gói tin gửi từ Client lên Server thông báo nhân vật di chuyển
    /// </summary>
    [ProtoContract]
    public class SpriteMoveData
    {
        /// <summary>
        /// ID nhân vật
        /// </summary>
        [ProtoMember(1)]
        public int RoleID { get; set; }

        /// <summary>
        /// Tọa độ bắt đầu X
        /// </summary>
        [ProtoMember(2)]
        public int FromX { get; set; }

        /// <summary>
        /// Tọa độ bắt đầu Y
        /// </summary>
        [ProtoMember(3)]
        public int FromY { get; set; }

        /// <summary>
        /// Tọa độ đích X
        /// </summary>
        [ProtoMember(4)]
        public int ToX { get; set; }

        /// <summary>
        /// Tọa độ đích Y
        /// </summary>
        [ProtoMember(5)]
        public int ToY { get; set; }

        /// <summary>
        /// Chuỗi mã hóa đoạn đường di chuyển
        /// </summary>
        [ProtoMember(6)]
        public string PathString = "";

        /// <summary>
        /// Vị trí xuất phát
        /// </summary>
        public UnityEngine.Vector2 FromPos
        {
            get
            {
                return new UnityEngine.Vector2(this.FromX, this.FromY);
            }
        }

        /// <summary>
        /// Vị trí đích
        /// </summary>
        public UnityEngine.Vector2 ToPos
        {
            get
            {
                return new UnityEngine.Vector2(this.ToX, this.ToY);
            }
        }

        /// <summary>
        /// Chuyển đối tượng về dạng string
        /// </summary>
        /// <returns></returns>
		public override string ToString()
		{
            return string.Format("RoleID = " + this.RoleID + " - Paths = " + this.PathString);
		}
	}
}