using ProtoBuf;

namespace Server.Data
{

    [ProtoContract]
    public class KTCoinRequest
    {
        [ProtoMember(1)]
        public int Type { get; set; }

        [ProtoMember(2)]
        public int UserID { get; set; }

        [ProtoMember(3)]
        public int Value { get; set; }

        [ProtoMember(4)]
        public int RoleID { get; set; }

        [ProtoMember(5)]
        public string RoleName { get; set; }

        [ProtoMember(6)]
        public int SeverID { get; set; }
    }

    [ProtoContract]
    public class KTCoinResponse
    {
        [ProtoMember(1)]
        public int Status { get; set; }

        [ProtoMember(2)]
        public string Msg { get; set; }

        [ProtoMember(3)]
        public int Value { get; set; }
    }

    [ProtoContract]
    public class GiftCodeRep
    {
        [ProtoMember(1)]
        public int Status { get; set; }

        [ProtoMember(2)]
        public string Msg { get; set; }

        [ProtoMember(3)]
        public string GiftItem { get; set; }
    }

    [ProtoContract]
    public class GiftCodeRequest
    {
        [ProtoMember(1)]
        public int RoleActive { get; set; }

        [ProtoMember(2)]
        public string CodeActive { get; set; }

        [ProtoMember(3)]
        public int ServerID { get; set; }
    }
}
