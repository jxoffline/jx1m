using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ProtoBuf;

namespace Server.Data
{
    [ProtoContract]
    public class StallAction
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
        public List<string> Fields { get; set; }

        /// <summary>
        /// Thông tin vật phẩm nếu có
        /// </summary>
        [ProtoMember(3)]
        public GoodsData GoodsData { get; set; }
    }
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
        public List<int> Fields = new List<int>();

        /// <summary>
        /// Mini Stall thứ 3
        /// </summary>
        [ProtoMember(3)]
        public MiniStallData MiniData { get; set; }


    }

    /// <summary>
    /// Các câu lệnh liên quan tới gamedb
    /// </summary>
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
