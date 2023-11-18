using ProtoBuf;

namespace Server.Data
{

    [ProtoContract]
    public class DailyTaskData
    {

        [ProtoMember(1)]
        public int HuanID = 0;


        [ProtoMember(2)]
        public string RecTime = "";

        [ProtoMember(3)]
        public int RecNum = 0;


        [ProtoMember(4)]
        public int TaskClass = 0;

        [ProtoMember(5)]
        public int ExtDayID = 0;


        [ProtoMember(6)]
        public int ExtNum = 0;
    }
}