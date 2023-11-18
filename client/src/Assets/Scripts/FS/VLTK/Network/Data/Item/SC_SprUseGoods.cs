using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;
using Tmsk.Contract;
using System.IO;

namespace Server.Data
{
    [ProtoContract]
    public class SC_SprUseGoods : IProtoBuffData
    {
        // 角色id
        [ProtoMember(1)]
        public int Error = 0;

        // 物品dbId
        [ProtoMember(2)]
        public int DbId = 0;

        // 物品类型Id
        [ProtoMember(3)]
        public int Cnt = 0;

        public SC_SprUseGoods()
        { 
        }

        public SC_SprUseGoods(int error, int dbId, int cnt)
        {
            this.Error = error;
            this.DbId = dbId;
            this.Cnt = cnt;
        }

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
                    case 1: this.Error = ProtoUtil.IntMemberFromBytes(data, wt, ref pos, ref mycount); break;
                    case 2: this.DbId = ProtoUtil.IntMemberFromBytes(data, wt, ref pos, ref mycount); break;
                    case 3: this.Cnt = ProtoUtil.IntMemberFromBytes(data, wt, ref pos, ref mycount); break;
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
            total += ProtoUtil.GetIntSize(this.Error, true, 1);
            total += ProtoUtil.GetIntSize(this.DbId, true, 2);
            total += ProtoUtil.GetIntSize(this.Cnt, true, 3);

            byte[] data = new byte[total];
            int offset = 0;

            ProtoUtil.IntMemberToBytes(data, 1, ref offset, this.Error);
            ProtoUtil.IntMemberToBytes(data, 2, ref offset, this.DbId);
            ProtoUtil.IntMemberToBytes(data, 3, ref offset, this.Cnt);

            /*
            MemoryStream ms = new MemoryStream();
            
            Serializer.Serialize<SC_SprUseGoods>(ms,this);
            ms.Position = 0;
            byte[] buff = new byte[ ms.Length ];
            ms.Read(buff, 0, (int)ms.Length);*/

            return data;
        }
    }
}
