using KF.Contract.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KF.Contract.Interface
{
    public interface ISpreadService
    {
        int InitializeClient(IKuaFuClient callback, KuaFuClientContext clientInfo); // 初始化游戏客户端

        int SpreadSign(int serverID, int zoneId, int roleId);
        int[] SpreadCount(int serverID, int zoneId, int roleId);

        int CheckVerifyCode(int cserverID,string cuserID, int czoneID, int croleID, int pzoneID, int proleID, int isVip, int isLevel);
        int TelCodeGet(int serverID, int czoneID, int croleID, string tel);
        int TelCodeVerify(int serverID, int czoneID, int croleID, int telCode);

        bool SpreadLevel(int pzoneID,int proleID, int czoneID, int croleID);
        bool SpreadVip(int pzoneID, int proleID, int czoneID, int croleID);

        //List<KuaFuServerInfo> GetKuaFuServerInfoData(int age);
        AsyncDataItem[] GetClientCacheItems(int serverId);// 游戏服务器从中心获取属于自己的消息(根据serverId)

    }
}
