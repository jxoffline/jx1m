using ProtoBuf;

namespace Server.Data
{

    [ProtoContract]
    public class RoleKuaFuDayLogData
    {

        [ProtoMember(1)]
        public int RoleID = 0;


        [ProtoMember(2)]
        public string Day = "2000-1-1";


        [ProtoMember(3)]
        public int ZoneId = 0;


        [ProtoMember(4)]
        public int SignupCount = 0;


        [ProtoMember(5)]
        public int StartGameCount = 0;


        [ProtoMember(6)]
        public int SuccessCount = 0;


        [ProtoMember(7)]
        public int FaildCount = 0;


        [ProtoMember(8)]
        public int GameType = 0;
    }
}