using GameServer.KiemThe;
using GameServer.KiemThe.Entities;
using ProtoBuf;
using System.Collections.Generic;

namespace Server.Data
{
    /// <summary>
    /// Vật phẩm hệ thống
    /// </summary>
    [ProtoContract]
    public class GoodsData
    {
        /// <summary>
        /// ID vật phẩm lưu ở DB
        /// </summary>
        [ProtoMember(1)]
        public int Id { get; set; }

        /// <summary>
        /// ID vật phẩm lưu ở File cấu hình
        /// </summary>
        [ProtoMember(2)]
        public int GoodsID { get; set; }

        /// <summary>
        /// Vị trí mặc trên người (-1 nếu ở trong túi đồ)
        /// </summary>
        [ProtoMember(3)]
        public int Using { get; set; }

        /// <summary>
        /// Cấp cường hóa
        /// </summary>
        [ProtoMember(4)]
        public int Forge_level { get; set; }

        /// <summary>
        /// Thời gian bắt đầu sử dụng
        /// </summary>
        [ProtoMember(5)]
        public string Starttime { get; set; }

        /// <summary>
        /// Thời gian hết hạn sử dụng
        /// </summary>
        [ProtoMember(6)]
        public string Endtime { get; set; }

        /// <summary>
        /// Vị trí đặt vật phẩm (0: túi đồ, 1: thương khố, ... các vị trí khác)
        /// </summary>
        [ProtoMember(7)]
        public int Site { get; set; }

        /// <summary>
        /// Thuộc tính của trang bị
        /// </summary>
        [ProtoMember(8)]
        public string Props { get; set; }

        /// <summary>
        /// Tổng số vật phẩm xếp chồng
        /// </summary>
        [ProtoMember(9)]
        public int GCount { get; set; }

        /// <summary>
        /// Cố định không (1/0)
        /// </summary>
        [ProtoMember(10)]
        public int Binding { get; set; }

        /// <summary>
        /// Vị trí trong túi đồ
        /// </summary>
        [ProtoMember(11)]
        public int BagIndex { get; set; }

        /// <summary>
        /// Độ bền
        /// </summary>
        [ProtoMember(12)]
        public int Strong { get; set; }

        /// <summary>
        /// Ngũ hành
        /// </summary>
        [ProtoMember(13)]
        public int Series { get; set; }

        /// <summary>
        /// Danh sách các thông số khác
        /// <para>Tiền tệ, etc....</para>
        /// </summary>
        [ProtoMember(14)]
        public Dictionary<ItemPramenter, string> OtherParams { get; set; } = null;

        /// <summary>
        /// Tác giả của vật phẩm
        /// </summary>
        public string Creator
        {
            get
            {
                if (this.OtherParams == null)
                {
                    return "";
                }
                if (this.OtherParams.TryGetValue(ItemPramenter.Creator, out string creator))
                {
                    return creator;
                }
                return "";
            }
        }

        public string ItemName
        {
            get
            {
                return KTGlobal.GetItemName(this.GoodsID);
            }
        }
        public override string ToString()
        {
            string itemName = KTGlobal.GetItemName(this);
            return string.Format("DbID: {0}, Name: {1}, BagIndex: {2}, EquipPos: {3}", this.Id, itemName, this.BagIndex, this.Using);
        }
    }
}
