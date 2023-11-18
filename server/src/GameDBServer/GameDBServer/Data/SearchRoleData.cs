using ProtoBuf;

namespace Server.Data
{

    [ProtoContract]
    public class SearchRoleData
    {

        [ProtoMember(1)]
        public int RoleID = 0;


        [ProtoMember(2)]
        public string RoleName;


        [ProtoMember(3)]
        public int RoleSex = 0;

        [ProtoMember(4)]
        public int Level = 0;

        [ProtoMember(5)]
        public int Occupation = 0;


        [ProtoMember(6)]
        public int MapCode = 0;

        [ProtoMember(7)]
        public int PosX = 0;


        [ProtoMember(8)]
        public int PosY = 0;

        [ProtoMember(9)]
        public int Faction = 0;


        [ProtoMember(10)]
        public string BHName = "";
    }
}