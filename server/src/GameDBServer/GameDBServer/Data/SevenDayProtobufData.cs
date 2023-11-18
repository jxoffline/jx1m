using ProtoBuf;

namespace Server.Data
{
    [ProtoContract]
    public class SevenDayItemData
    {
        [ProtoMember(1)]
        public int AwardFlag;

        [ProtoMember(2)]
        public int Params1;

        [ProtoMember(3)]
        public int Params2;
    }
}