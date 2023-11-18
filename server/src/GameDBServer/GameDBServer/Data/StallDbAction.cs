using ProtoBuf;
using Server.Data;
using System.Collections.Generic;

namespace GameDBServer.Data
{
    [ProtoContract]
    public class StallDbAction
    {
        /// <summary>
        /// Loại thao tác
        /// </summary>
        [ProtoMember(1)]
        public int Type { get; set; }

        /// <summary>
        /// Danh sách các trường
        /// </summary>
        [ProtoMember(2)]
        public List<int> Fields { get; set; }

        /// <summary>
        /// Thông tin mini sạp hàng nếu command phù hợp
        /// </summary>
        [ProtoMember(3)]
        public MiniStallData MiniData { get; set; }
    }

    public enum StallCommand
    {
        /// <summary>
        /// Thêm mới hoặc cập nhật sạp hàng
        /// </summary>
        UPDATE,
        /// <summary>
        /// Thêm mới 1 vật phẩm vào sạp
        /// </summary>
        INSERT_ITEM,
        /// <summary>
        /// Xóa 1 vật phẩm khỏi sạp
        /// </summary>
        REMOVE_ITEM,

        /// <summary>
        /// Xóa hẳn sạp hàng
        /// </summary>
        DELETE_STALL,


        
    }
}