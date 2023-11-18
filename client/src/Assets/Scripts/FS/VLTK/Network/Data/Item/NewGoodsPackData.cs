using ProtoBuf;

namespace Server.Data
{
    /// <summary>
    /// Đối tượng vật phẩm rơi ở Map
    /// </summary>
    [ProtoContract]
    public class NewGoodsPackData
    {
        /// <summary>
        /// ID tự động
        /// </summary>
        [ProtoMember(1)]
        public int AutoID { get; set; }

        /// <summary>
        /// Vị trí X
        /// </summary>
        [ProtoMember(2)]
        public int PosX { get; set; }

        /// <summary>
        /// Vị trí Y
        /// </summary>
        [ProtoMember(3)]
        public int PosY { get; set; }

        /// <summary>
        /// ID vật phẩm
        /// </summary>
        [ProtoMember(4)]
        public int GoodsID { get; set; }

        /// <summary>
        /// Thời gian tồn tại còn lại (mili giây)
        /// </summary>
        [ProtoMember(5)]
        public long LifeTime { get; set; }

        /// <summary>
        /// Mã màu HTML
        /// </summary>
        [ProtoMember(6)]
        public string HTMLColor { get; set; }

        /// <summary>
        /// Tổng số
        /// </summary>
        [ProtoMember(7)]
        public int GoodCount { get; set; }

        /// <summary>
        /// Tổng số sao
        /// </summary>
        [ProtoMember(8)]
        public int Star { get; set; }

        /// <summary>
        /// Tổng số sao
        /// </summary>
        [ProtoMember(9)]
        public int EnhanceLevel { get; set; }

        /// <summary>
        /// Số dòng
        /// </summary>
        [ProtoMember(10)]
        public int LinesCount { get; set; }

    }
}