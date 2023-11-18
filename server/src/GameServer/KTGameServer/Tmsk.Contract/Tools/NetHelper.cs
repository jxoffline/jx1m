//#define  _D_WCF
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
#if _D_WCF
using System.ServiceModel;
using System.ServiceModel.Channels;
#endif

namespace System.Net
{
    public static class NetHelper
    {
#if _D_WCF
        public static string GetRemoteIp()
        {
            try
            {
                OperationContext context = OperationContext.Current;
                MessageProperties properties = context.IncomingMessageProperties;
                RemoteEndpointMessageProperty endpoint = properties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;
                return endpoint.Address;
            }
            catch
            {
                return null;
            }
        }
#endif
        public static string GetLocalAddressIPs()
        {
            string addressIP = "";
            //获取本地的IP地址
            try
            {
                foreach (IPAddress _IPAddress in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
                {
                    if (_IPAddress.AddressFamily.ToString() == "InterNetwork")
                    {
                        if (addressIP == "")
                        {
                            addressIP = _IPAddress.ToString();
                        }
                        else
                        {
                            addressIP += "_" + _IPAddress.ToString();
                        }
                    }
                }
            }
            catch
            {

            }
            return addressIP;
        }

        public static bool InAuthIp(string authIp, string remoteIp = null)
        {
            if (string.IsNullOrWhiteSpace(authIp))
            {
                return true;
            }

            string[] ipArray = authIp.Split(',');

            if (null == remoteIp)
            {
                //获取本地IP
                IPAddress[] ips = Dns.GetHostAddresses(Dns.GetHostName());
                foreach (var ip in ips)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        foreach (var ip0 in ipArray)
                        {
                            if (ip.ToString().StartsWith(ip0))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            else
            {
                foreach (var ip0 in ipArray)
                {
                    if (remoteIp.StartsWith(ip0))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
