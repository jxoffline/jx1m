using ProtoBuf;

namespace Server.Data
{

    [ProtoContract]
    public class TaskData
    {

        [ProtoMember(1)]
        public int DbID;

        [ProtoMember(2)]
        public int DoingTaskID;

        [ProtoMember(3)]
        public int DoingTaskVal1;


        [ProtoMember(4)]
        public int DoingTaskVal2;

        [ProtoMember(5)]
        public int DoingTaskFocus;

        [ProtoMember(6)]
        public long AddDateTime;


        [ProtoMember(7)]
        public TaskAwardsData TaskAwards = null;


        [ProtoMember(8)]
        public int DoneCount = 0;


        [ProtoMember(9)]
        public int TaskType = 0;
    }
}