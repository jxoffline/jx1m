using ProtoBuf;
using Tmsk.Contract;

namespace Server.Data
{
    /// <summary>
    /// 移动结束
    /// CMD_SPR_MOVEEND
    /// 双向
    /// </summary>
    [ProtoContract]
    public class SCMoveEnd : IProtoBuffData
    {
        [ProtoMember(1)]
        public int RoleID = 0;

        [ProtoMember(2)]
        public int Action = 0;

        [ProtoMember(3)]
        public int MapCode = 0;

        [ProtoMember(4)]
        public int ToMapX = 0;

        [ProtoMember(5)]
        public int ToMapY = 0;

        [ProtoMember(6)]
        public int ToDiection = 0;

        [ProtoMember(7)]
        public int TryRun = 0;

        [ProtoMember(8)]
        public long clientTicks = 0;

        public SCMoveEnd()
        {
        }

        public SCMoveEnd(int roleID, int mapCode, int action, int toNewMapX, int toNewMapY, int toNewDiection, int tryRun, long clientTicks = 0)
        {
            this.RoleID = roleID;
            this.Action = action;
            this.MapCode = mapCode;
            this.ToMapX = toNewMapX;
            this.ToMapY = toNewMapY;
            this.ToDiection = toNewDiection;
            this.TryRun = tryRun;
            this.clientTicks = clientTicks;
        }

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
                    case 1: this.RoleID = ProtoUtil.IntMemberFromBytes(data, wt, ref pos, ref mycount); break;
                    case 2: this.Action = ProtoUtil.IntMemberFromBytes(data, wt, ref pos, ref mycount); break;
                    case 3: this.MapCode = ProtoUtil.IntMemberFromBytes(data, wt, ref pos, ref mycount); break;
                    case 4: this.ToMapX = ProtoUtil.IntMemberFromBytes(data, wt, ref pos, ref mycount); break;
                    case 5: this.ToMapY = ProtoUtil.IntMemberFromBytes(data, wt, ref pos, ref mycount); break;
                    case 6: this.ToDiection = ProtoUtil.IntMemberFromBytes(data, wt, ref pos, ref mycount); break;
                    case 7: this.TryRun = ProtoUtil.IntMemberFromBytes(data, wt, ref pos, ref mycount); break;
                    case 8: this.clientTicks = ProtoUtil.LongMemberFromBytes(data, wt, ref pos, ref mycount); break;
                        //default:
                        //    {
                        //        throw new ArgumentException("error!!!");
                        //    }
                }
            }
            return pos;
        }

        public byte[] toBytes()
        {
            int total = 0;
            total += ProtoUtil.GetIntSize(RoleID, true, 1);
            total += ProtoUtil.GetIntSize(Action, true, 2);
            total += ProtoUtil.GetIntSize(MapCode, true, 3);
            total += ProtoUtil.GetIntSize(ToMapX, true, 4);
            total += ProtoUtil.GetIntSize(ToMapY, true, 5);
            total += ProtoUtil.GetIntSize(ToDiection, true, 6);
            total += ProtoUtil.GetIntSize(TryRun, true, 7);
            total += ProtoUtil.GetLongSize(clientTicks, true, 8);

            byte[] data = new byte[total];
            int offset = 0;

            ProtoUtil.IntMemberToBytes(data, 1, ref offset, RoleID);
            ProtoUtil.IntMemberToBytes(data, 2, ref offset, Action);
            ProtoUtil.IntMemberToBytes(data, 3, ref offset, MapCode);
            ProtoUtil.IntMemberToBytes(data, 4, ref offset, ToMapX);
            ProtoUtil.IntMemberToBytes(data, 5, ref offset, ToMapY);
            ProtoUtil.IntMemberToBytes(data, 6, ref offset, ToDiection);
            ProtoUtil.IntMemberToBytes(data, 7, ref offset, TryRun);
            ProtoUtil.LongMemberToBytes(data, 8, ref offset, clientTicks);

            return data;
        }
    }
}