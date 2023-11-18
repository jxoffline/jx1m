using ProtoBuf;

namespace Server.Data
{

    [ProtoContract]
    public class OldTaskData
    {

        [ProtoMember(1)]
        public int TaskID;


        [ProtoMember(2)]
        public int DoCount;

        [ProtoMember(3)]
        public int TaskClass;
    }
}