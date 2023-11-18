using ProtoBuf;
using System;
using System.Collections.Generic;
using Tmsk.Contract;

namespace Server.Data
{
    /// <summary>
    /// Gói tin thông tin vật phẩm thay đổi từ Server về Client
    /// </summary>
    [ProtoContract]
    public class SCModGoods : IProtoBuffData
    {
        /// <summary>
        /// Kết quả trả về
        /// </summary>
        [ProtoMember(1)]
        public int State { get; set; }

        /// <summary>
        /// Loại thay đổi
        /// </summary>
        [ProtoMember(2)]
        public int ModType { get; set; }

        /// <summary>
        /// Db ID
        /// </summary>
        [ProtoMember(3)]
        public int ID { get; set; }

        /// <summary>
        /// Vị trí trang bị trên người
        /// </summary>
        [ProtoMember(4)]
        public int IsUsing { get; set; }

        /// <summary>
        /// Vị trí túi
        /// </summary>
        [ProtoMember(5)]
        public int Site { get; set; }

        /// <summary>
        /// Tổng số vật phẩm tại ô tương ứng
        /// </summary>
        [ProtoMember(6)]
        public int Count { get; set; }

        /// <summary>
        /// Vị trí trong túi đồ
        /// </summary>
        [ProtoMember(7)]
        public int BagIndex { get; set; }

        /// <summary>
        /// Có thông báo không (1/0)
        /// </summary>
        [ProtoMember(8)]
        public int NewHint { get; set; }

        /// <summary>
        /// Chuyển đối tượng từ Byte Data
        /// </summary>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public int fromBytes(byte[] data, int offset, int count)
        {
            int pos = offset;
            int mycount = 0;

            for (; mycount < count;)
            {
                int fieldnumber = -1;
                int wt = -1;
                ProtoUtil.GetTag(data, ref pos, ref fieldnumber, ref wt, ref mycount);

                switch (fieldnumber)
                {
                    case 1:
                        this.State = ProtoUtil.IntMemberFromBytes(data, wt, ref pos, ref mycount);
                        break;
                    case 2:
                        this.ModType = ProtoUtil.IntMemberFromBytes(data, wt, ref pos, ref mycount);
                        break;
                    case 3:
                        this.ID = ProtoUtil.IntMemberFromBytes(data, wt, ref pos, ref mycount);
                        break;
                    case 4:
                        this.IsUsing = ProtoUtil.IntMemberFromBytes(data, wt, ref pos, ref mycount);
                        break;
                    case 5:
                        this.Site = ProtoUtil.IntMemberFromBytes(data, wt, ref pos, ref mycount);
                        break;
                    case 6:
                        this.Count = ProtoUtil.IntMemberFromBytes(data, wt, ref pos, ref mycount);
                        break;
                    case 7:
                        this.BagIndex = ProtoUtil.IntMemberFromBytes(data, wt, ref pos, ref mycount);
                        break;
                    case 8:
                        this.NewHint = ProtoUtil.IntMemberFromBytes(data, wt, ref pos, ref mycount);
                        break;
                    default:
                    {
                        throw new ArgumentException("error!!!");
                    }
                }
            }
            return pos;
        }

        public byte[] toBytes()
        {
            int total = 0;
            total += ProtoUtil.GetIntSize(State, true, 1);
            total += ProtoUtil.GetIntSize(ModType, true, 2);
            total += ProtoUtil.GetIntSize(ID, true, 3);
            total += ProtoUtil.GetIntSize(IsUsing, true, 4);
            total += ProtoUtil.GetIntSize(Site, true, 5);
            total += ProtoUtil.GetIntSize(Count, true, 6);
            total += ProtoUtil.GetIntSize(BagIndex, true, 7);
            total += ProtoUtil.GetIntSize(NewHint, true, 8);

            byte[] data = new byte[total];
            int offset = 0;

            ProtoUtil.IntMemberToBytes(data, 1, ref offset, State);
            ProtoUtil.IntMemberToBytes(data, 2, ref offset, ModType);
            ProtoUtil.IntMemberToBytes(data, 3, ref offset, ID);
            ProtoUtil.IntMemberToBytes(data, 4, ref offset, IsUsing);
            ProtoUtil.IntMemberToBytes(data, 5, ref offset, Site);
            ProtoUtil.IntMemberToBytes(data, 6, ref offset, Count);
            ProtoUtil.IntMemberToBytes(data, 7, ref offset, BagIndex);
            ProtoUtil.IntMemberToBytes(data, 8, ref offset, NewHint);

            return data;
        }
    }
}


