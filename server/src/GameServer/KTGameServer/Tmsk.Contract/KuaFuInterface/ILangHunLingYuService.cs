using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using KF.Contract.Data;

namespace KF.Contract.Interface
{
    /// <summary>
    /// 跨服副本 RPC IDL
    /// </summary>
    public interface ILangHunLingYuService
    {
        /// <summary>
        /// 初始化游戏客户端
        /// </summary>
        int InitializeClient(IKuaFuClient callback, KuaFuClientContext clientInfo);

        /// <summary>
        /// 把活动服务器的副本流水号(db generate)告知中心服务器
        /// </summary>
        int PushFuBenSeqId(int serverId, List<int> list);

        /// <summary>
        /// 创建房间
        /// </summary>
        KFCopyTeamCreateRsp CreateTeam(KFCopyTeamCreateReq req);

        /// <summary>
        /// 加入房间
        /// </summary>
        KFCopyTeamJoinRsp JoinTeam(KFCopyTeamJoinReq req);

        /// <summary>
        /// 踢出房间
        /// </summary>
        KFCopyTeamKickoutRsp KickoutTeam(KFCopyTeamKickoutReq req);

        /// <summary>
        /// 离开房间
        /// </summary>
        KFCopyTeamLeaveRsp LeaveTeam(KFCopyTeamLeaveReq req);

        /// <summary>
        /// 开始游戏
        /// </summary>
        KFCopyTeamStartRsp StartGame(KFCopyTeamStartReq req);

        /// <summary>
        /// 设置准备状态
        /// </summary>
        KFCopyTeamSetReadyRsp TeamSetReady(KFCopyTeamSetReadyReq req);

        /// <summary>
        /// 游戏服务器从中心获取属于自己的消息(根据serverId)
        /// </summary>
        AsyncDataItem[] GetClientCacheItems(int serverId);

        /// <summary>
        /// 游戏服务器从中心获取各游戏服务器信息
        /// 用途：
        /// 1：GS根据分配给副本的ServerID取出来K-GS的ip和port
        /// 2：K-GS根据登入的client的ServerID取出来源服务器的DB和LogDB的ip和port
        /// </summary>
        List<KuaFuServerInfo> GetKuaFuServerInfoData(int age);

        /// <summary>
        /// 跨服活动服务器接到分给自己了一个副本的消息时，从中心取一下最新的队伍信息
        /// 并且向中心发回确认，我已经准备好了，请玩家切过来吧
        /// </summary>
        CopyTeamData GetTeamData(long teamid);

        /// <summary>
        /// 跨服副本结束的时候调用
        /// </summary>
        /// <param name="teamId"></param>
        void RemoveTeam(long teamId);

        /// <summary>
        /// 获取平台种植王排行榜
        /// </summary>
        /// <returns></returns>
        object GetPlatChargeKing();

        void UpdateStatisticalData(object data);
        int GameFuBenChangeState(int gameId, GameFuBenState state, DateTime time);
        KuaFuRoleData GetKuaFuRoleData(int serverId, int roleId);
        IKuaFuFuBenData GetFuBenData(int gameId);
        object GetRoleExtendData(int serverId, int roleId, int dataType);

        LangHunLingYuCityDataEx SignUp(int serverId, string userId, int zoneId, int roleId, int gameType, int groupIndex, IGameData gameData);
        int RoleChangeState(int serverId, int roleId, int state);
        int GameFuBenRoleChangeState(int serverId, int roleId, int gameId, int state);
    }
}
