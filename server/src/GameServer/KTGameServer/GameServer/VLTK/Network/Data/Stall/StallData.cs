using ProtoBuf;
using System.Collections.Generic;

namespace Server.Data
{
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
        /// Có phải bot bán hàng ko
        /// </summary>
        [ProtoMember(4)]
        public bool IsBot;

        /// <summary>
        /// Danh sách vật phẩm
        /// </summary>
        [ProtoMember(5)]
        public List<GoodsData> GoodsList = new List<GoodsData>();

        /// <summary>
        /// Danh sách giá vật phẩm theo GoodsDbID
        /// </summary>
        [ProtoMember(6)]
        public Dictionary<int, int> GoodsPriceDict = new Dictionary<int, int>();

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

        /// <summary>
        /// Tên của thằng bán hàng là thằng nào
        /// </summary>
        [ProtoMember(13)]
        public string RoleName;
    }

    /// <summary>
    /// Sử dụng để trao đổi dữ liệu với gameDB
    /// </summary>
    [ProtoContract]
    public class MiniStallData
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
        /// Có phải bot bán hàng ko
        /// </summary>
        [ProtoMember(4)]
        public bool IsBot;

        /// <summary>
        /// Danh sách giá vật phẩm theo GoodsDbID
        /// </summary>
        [ProtoMember(5)]
        public Dictionary<int, int> GoodsPriceDict;

        /// <summary>
        /// Thời điểm thêm vào
        /// </summary>
        [ProtoMember(6)]
        public long AddDateTime;

        /// <summary>
        /// Bắt đầu mở bán
        /// </summary>
        [ProtoMember(7)]
        public int Start;

        /// <summary>
        /// Hình ảnh cái shop bán trông như thế nào
        /// </summary>
        [ProtoMember(8)]
        public List<int> ListResID;

        /// <summary>
        /// Map mà thằng này ngồi bán hàng
        /// </summary>
        [ProtoMember(9)]
        public int MapCode;

        /// <summary>
        /// Địa điểm bày bán X
        /// </summary>
        [ProtoMember(10)]
        public int PosX;

        /// <summary>
        /// Vị trí bày bán Y
        /// </summary>
        [ProtoMember(11)]
        public int PosY;

        /// <summary>
        /// Tên của thằng bán hàng là thằng nào
        /// </summary>
        [ProtoMember(12)]
        public string RoleName;
    }
}