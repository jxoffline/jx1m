using ProtoBuf;

namespace Server.Data
{

    [ProtoContract]
    public class GoodsLimitData
    {

        [ProtoMember(1)]
        public int GoodsID;


        [ProtoMember(2)]
        public int DayID;


        [ProtoMember(3)]
        public int UsedNum;
    }
}