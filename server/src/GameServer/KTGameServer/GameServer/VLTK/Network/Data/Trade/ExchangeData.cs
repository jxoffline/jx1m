using ProtoBuf;
using System.Collections.Generic;

namespace Server.Data
{
    /// <summary>
    /// Cấu trúc của 1 session giao dịch
    /// </summary>
    [ProtoContract]
    public class ExchangeData
    {
        /// <summary>
        /// ID phiên giao dịch
        /// </summary>
        [ProtoMember(1)]
        public int ExchangeID { get; set; }

        /// <summary>
        /// Người yêu cầu giao dịch
        /// </summary>
        [ProtoMember(2)]
        public int RequestRoleID { get; set; }

        /// <summary>
        /// Người đồng ý giao dịch
        /// </summary>
        [ProtoMember(3)]
        public int AgreeRoleID { get; set; }

        /// <summary>
        /// Danh sách vật phẩm giao dịch
        /// </summary>
        [ProtoMember(4)]
        public Dictionary<int, List<GoodsData>> GoodsDict { get; set; }

        /// <summary>
        /// Tiền giao dịch
        /// </summary>
        [ProtoMember(5)]
        public Dictionary<int, int> MoneyDict { get; set; }

        /// <summary>
        /// Danh sách đã lock
        /// </summary>
        [ProtoMember(6)]
        public Dictionary<int, int> LockDict { get; set; }

        /// <summary>
        /// Danh sách sách hoàn tất giao dịch
        /// </summary>
        [ProtoMember(7)]
        public Dictionary<int, int> DoneDict { get; set; }

        /// <summary>
        /// Thời gian add vào
        /// </summary>
        [ProtoMember(8)]
        public long AddDateTime { get; set; }

        /// <summary>
        /// Có hoàn thành không
        /// </summary>
        [ProtoMember(9)]
        public int Done { get; set; }

        /// <summary>
        /// KNB
        /// </summary>
        [ProtoMember(10)]
        public Dictionary<int, int> YuanBaoDict { get; set; }

        /// <summary>
        /// Dict Pet giao dịch
        /// Pet Dict
        /// </summary>
        [ProtoMember(11)]
        public Dictionary<int, List<PetData>> PetDict { get; set; }
    }
}