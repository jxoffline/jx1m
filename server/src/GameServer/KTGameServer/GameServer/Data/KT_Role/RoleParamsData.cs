using ProtoBuf;

namespace Server.Data
{
   
    [ProtoContract]
    public class RoleParamsData
    {
       
        [ProtoMember(1)]
        public string ParamName = "";

        [ProtoMember(2)]
        public string ParamValue = "";
    }
}