using System;
using System.Collections.Generic;
using KF.Contract.Data;

namespace KF.Contract.Interface
{
    public interface IKuaFuFuBenData
    {

    }

    public interface IKuaFuClient
    {
        #region 因为线程锁问题这组函数尚未安全实现,不支持远程调用

        void EventCallBackHandler(int eventType, params object[] args);
        object GetDataFromClientServer(int dataType, params object[] args);
        int GetNewFuBenSeqId();
        int UpdateRoleData(KuaFuRoleData kuaFuRoleData, int roleId = 0);
        int OnRoleChangeState(int roleId, int state, int age);

        #endregion 因为线程锁问题这组函数尚未安全实现,不支持远程调用
    }

    public interface IKuaFuService
    {
        int InitializeClient(IKuaFuClient callback, KuaFuClientContext clientInfo);
        int RoleSignUp(int serverId, string userId, int zoneId, int roleId, int gameType, int groupIndex, IGameData gameData);
        int RoleChangeState(int serverId, int roleId, int state);
        int GameFuBenRoleChangeState(int serverId, int roleId, int gameId, int state);
        int GameFuBenChangeState(int gameId, GameFuBenState state, DateTime time);
        KuaFuRoleData GetKuaFuRoleData(int serverId, int roleId);
        IKuaFuFuBenData GetFuBenData(int gameId);
        object GetRoleExtendData(int serverId, int roleId, int dataType);
        AsyncDataItem[] GetClientCacheItems(int serverId);
        List<KuaFuServerInfo> GetKuaFuServerInfoData(int age);
    }
}
