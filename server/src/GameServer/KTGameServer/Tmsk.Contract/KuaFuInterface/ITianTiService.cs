using System;
using System.Collections.Generic;
using KF.Contract.Data;

using Tmsk.Contract;

namespace KF.Contract.Interface
{
    /// <summary>
    /// 跨服众神争霸
    /// 剥离出单独的接口，但是并不作为一个真正的Remote Service Object
    /// 只是寄宿在TianTiService中
    /// </summary>
    public interface IZhengBaService
    {
        ZhengBaSyncData SyncZhengBaData(ZhengBaSyncData lastSyncData);
        int ZhengBaSupport(ZhengBaSupportLogData log);
        int ZhengBaRequestEnter(int roleId, int gameId, EZhengBaEnterType enter);
        int ZhengBaKuaFuLogin(int roleid, int gameId);
        List<ZhengBaNtfPkResultData> ZhengBaPkResult(int gameId, int winner, int FirstLeaveRoleId);
    }

    /// <summary>
    /// 跨服天梯
    /// </summary>
    public interface ITianTiService : IZhengBaService, ICoupleArenaService, ICoupleWishService
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
        TianTiRankData GetRankingData(DateTime modifyTime);
        void UpdateRoleInfoData(TianTiRoleInfoData data);
    }
}
