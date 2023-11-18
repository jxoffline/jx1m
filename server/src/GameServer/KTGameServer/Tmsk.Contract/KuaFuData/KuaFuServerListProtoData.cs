using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace KF.Contract.Data
{
    [ProtoContract]
    public class GetKuaFuServerListRequestData
    {
        [ProtoMember(1)]
        public int GameType;

        [ProtoMember(2)]
        public int ServerListAge;

        [ProtoMember(3)]
        public int ServerGameConfigAge;

        [ProtoMember(4)]
        public int GameConfigAge;
    }

    [ProtoContract]
    public class GetKuaFuServerListResponseData
    {
        [ProtoMember(1)]
        public int GameType;

        [ProtoMember(2)]
        public int ServerListAge;

        [ProtoMember(3)]
        public int ServerGameConfigAge;

        [ProtoMember(4)]
        public int GameConfigAge;

        [ProtoMember(5)]
        public List<KuaFuServerInfoProtoData> ServerList;

        [ProtoMember(6)]
        public List<KuaFuServerGameConfigProtoData> ServerGameConfigList;
    }

    [ProtoContract]
    public class KuaFuServerGameConfigProtoData
    {
        [ProtoMember(1)]
        public int ServerId;

        [ProtoMember(2)]
        public int GameType;

        [ProtoMember(3)]
        public int Capacity;
    }

    [ProtoContract]
    public class KuaFuServerInfoProtoData
    {
        [ProtoMember(1)]
        public int ServerId;

        [ProtoMember(2)]
        public string Ip;

        [ProtoMember(3)]
        public int Port;

        [ProtoMember(4)]
        public int DbPort;

        [ProtoMember(5)]
        public int LogDbPort;

        [ProtoMember(6)]
        public int Flags;
    }
}
