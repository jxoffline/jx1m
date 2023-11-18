using ProtoBuf;
using System;
using Tmsk.Contract;

namespace Server.Data
{
    /// <summary>
    /// Cộng điểm tiềm năng nhân vật
    /// </summary>
    [ProtoContract]
    public class CSPropAddPoint : IProtoBuffData
    {
        /// <summary>
        /// ID nhân vật
        /// </summary>
        [ProtoMember(1)]
        public int RoleID = 0;

        /// <summary>
        /// Sức
        /// </summary>
        [ProtoMember(2)]
        public int Strength = 0;

        /// <summary>
        /// Nội
        /// </summary>
        [ProtoMember(3)]
        public int Intelligence = 0;

        /// <summary>
        /// Thân
        /// </summary>
        [ProtoMember(4)]
        public int Dexterity = 0;

        /// <summary>
        /// Thể
        /// </summary>
        [ProtoMember(5)]
        public int Constitution = 0;

        /// <summary>
        /// Chuyển đối tượng từ Bytes Array
        /// </summary>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public int fromBytes(byte[] data, int offset, int count)
        {
            int pos = offset;
            int mycount = 0;

            for (; mycount < count; )
            {
                int fieldnumber = -1;
                int wt = -1;
                ProtoUtil.GetTag(data, ref pos, ref fieldnumber, ref wt, ref mycount);

                switch (fieldnumber)
                {
                    case 1: this.RoleID = ProtoUtil.IntMemberFromBytes(data, wt, ref pos, ref mycount); break;
                    case 2: this.Strength = ProtoUtil.IntMemberFromBytes(data, wt, ref pos, ref mycount); break;
                    case 3: this.Intelligence = ProtoUtil.IntMemberFromBytes(data, wt, ref pos, ref mycount); break;
                    case 4: this.Dexterity = ProtoUtil.IntMemberFromBytes(data, wt, ref pos, ref mycount); break;
                    case 5: this.Constitution = ProtoUtil.IntMemberFromBytes(data, wt, ref pos, ref mycount); break;
                    default:
                        {
                            throw new ArgumentException("error!!!");
                        }
                }
            }
            return pos;
        }

        /// <summary>
        /// Chuyển đối tượng thành Bytes Array
        /// </summary>
        /// <returns></returns>
        public byte[] toBytes()
        {
            int total = 0;
            total += ProtoUtil.GetIntSize(RoleID, true, 1);
            total += ProtoUtil.GetIntSize(Strength, true, 2);
            total += ProtoUtil.GetIntSize(Intelligence, true, 3);
            total += ProtoUtil.GetIntSize(Dexterity, true, 4);
            total += ProtoUtil.GetIntSize(Constitution, true, 5);

            byte[] data = new byte[total];
            int offset = 0;

            ProtoUtil.IntMemberToBytes(data, 1, ref offset, RoleID);
            ProtoUtil.IntMemberToBytes(data, 2, ref offset, Strength);
            ProtoUtil.IntMemberToBytes(data, 3, ref offset, Intelligence);
            ProtoUtil.IntMemberToBytes(data, 4, ref offset, Dexterity);
            ProtoUtil.IntMemberToBytes(data, 5, ref offset, Constitution);

            return data;
        }
    }
}


