using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Channels;
using System.Net;
using System.Security.Principal;

namespace KF.Contract
{
    [Serializable]
    public class KuaFuClientContext// : ILogicalThreadAffinative
    {
        public int ServerId;
        public int ClientId;
        public int GameType;
        public string Token;
        public Dictionary<int, int> MapClientCountDict;

        public KuaFuClientContext()
        {
            Token = NetHelper.GetLocalAddressIPs();
        }

        public static bool CheckConnectClient()
        {
            KuaFuClientContext checkContext = CallContext.GetData("KuaFuClientContext") as KuaFuClientContext;
            if (checkContext == null)
            {
                return false;
            }

            return true;
        }
    }

    public class AuthorizationContext : IAuthorizeRemotingConnection
    {
        bool IAuthorizeRemotingConnection.IsConnectingEndPointAuthorized(EndPoint endPoint)
        {
            Console.WriteLine("新客户IP : " + endPoint); //连接的IP
            return true;
        }

        bool IAuthorizeRemotingConnection.IsConnectingIdentityAuthorized(IIdentity identity)
        {
            Console.WriteLine("新客户用户名 : " + identity.Name); //连接用户计算机名
            return true;
        }
    }
}
