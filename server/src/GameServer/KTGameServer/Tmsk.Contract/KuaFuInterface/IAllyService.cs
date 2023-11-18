using KF.Contract;
using KF.Contract.Data;
using KF.Contract.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tmsk.Contract.KuaFuData;

namespace Tmsk.Contract.Interface
{
    public interface IAllyService
    {
        int InitializeClient(IKuaFuClient callback, KuaFuClientContext clientInfo); // 初始化游戏客户端
        AsyncDataItem[] GetClientCacheItems(int serverId);// 游戏服务器从中心获取属于自己的消息(根据serverId)

        long UnionAllyVersion(int serverID);

        int UnionAllyInit(int serverID, int unionID, bool isKF);
        int UnionDel(int serverID, int unionID);
        int UnionDataChange(int serverID, AllyData unionData);

        List<AllyData> AllyDataList(int serverID, int unionID, int type);

        AllyData AllyRequest(int serverID, int unionID, int zoneID, string unionName);
        int AllyOperate(int serverID, int unionID, int targetID, int operateType, bool isDel=false);

    }
}
