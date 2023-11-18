using ProtoBuf;

namespace Server.Data
{

    [ProtoContract]
    public class SpecActInfoDB
    {

        [ProtoMember(1)]
        public int GroupID = 0;


        [ProtoMember(2)]
        public int ActID = 0;


        [ProtoMember(3)]
        public int PurNum = 0;


        [ProtoMember(4)]
        public int CountNum = 0;


        [ProtoMember(5)]
        public short Active = 0;
    }
}