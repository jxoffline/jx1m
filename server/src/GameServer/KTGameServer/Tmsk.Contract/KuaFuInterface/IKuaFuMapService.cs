using System;
using System.Collections.Generic;
using KF.Contract.Data;

namespace KF.Contract.Interface
{
    public interface IKuaFuMapService
    {
        int InitializeClient(IKuaFuClient callback, KuaFuClientContext clientInfo);
        int PushFuBenSeqId(int serverId, List<int> list);
        int RoleSignUp(int serverId, string userId, int zoneId, int roleId, int gameType, int groupIndex, IGameData gameData);
        int RoleChangeState(int serverId, int roleId, int state);
        int GameFuBenRoleChangeState(int serverId, int roleId, int gameId, int state);
        int GameFuBenChangeState(int gameId, GameFuBenState state, DateTime time);
        KuaFuRoleData GetKuaFuRoleData(int serverId, int roleId);
        IKuaFuFuBenData GetFuBenData(int gameId);
        object GetRoleExtendData(int serverId, int roleId, int dataType);
        AsyncDataItem[] GetClientCacheItems(int serverId);
        List<KuaFuServerInfo> GetKuaFuServerInfoData(int age);
        int ExecuteCommand(string cmd);
        void UpdateStatisticalData(object data);
    }
}
