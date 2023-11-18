using ProtoBuf;
using System.Collections.Generic;

namespace Server.Data
{
    /// <summary>
    /// Dữ liệu thao tác gian hàng
    /// </summary>
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

    /// <summary>
    /// Dữ liệu cửa hàng của người chơi bày bán
    /// </summary>
    [ProtoContract]
    public class StallData
    {
        /// <summary>
        /// ID cửa hàng
        /// </summary>
        [ProtoMember(1)]
        public int StallID;

        /// <summary>
        /// ID người chơi bán hàng
        /// </summary>
        [ProtoMember(2)]
        public int RoleID;

        /// <summary>
        /// Tên cửa hàng
        /// </summary>
        [ProtoMember(3)]
        public string StallName;

        /// <summary>
        /// Có phải Bot bán hàng không
        /// </summary>
        [ProtoMember(4)]
        public bool IsBot;

        /// <summary>
        /// Danh sách vật phẩm
        /// </summary>
        [ProtoMember(5)]
        public List<GoodsData> GoodsList;

        /// <summary>
        /// Danh sách giá vật phẩm theo GoodsDbID
        /// </summary>
        [ProtoMember(6)]
        public Dictionary<int, int> GoodsPriceDict;

        /// <summary>
        /// Thời điểm thêm vào
        /// </summary>
        [ProtoMember(7)]
        public long AddDateTime;

        /// <summary>
        /// Bắt đầu mở bán
        /// </summary>
        [ProtoMember(8)]
        public int Start;

        /// <summary>
        /// Hình ảnh cái shop bán trông như thế nào
        /// </summary>
        [ProtoMember(9)]
        public List<int> ListResID;

        /// <summary>
        /// Map mà thằng này ngồi bán hàng
        /// </summary>
        [ProtoMember(10)]
        public int MapCode;

        /// <summary>
        /// Địa điểm bày bán X
        /// </summary>
        [ProtoMember(11)]
        public int PosX;

        /// <summary>
        /// Vị trí bày bán Y
        /// </summary>
        [ProtoMember(12)]
        public int PosY;
    }
}