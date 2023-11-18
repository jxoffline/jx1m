using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KF.Contract.Data
{
    /// <summary>
    /// 跨服多人副本错误码
    /// </summary>
    public enum CopyTeamErrorCodes
    {
        Success = 1, //成功
        NoTeam = -1, //你已经不在队伍中(没有队伍)
        TeamIsDestoryed = -2, //队伍已解散
        AllreadyHasTeam = -3, //已有队伍
        NotTeamLeader = -4, //你不是队长(队长才能执行)
        TeamIsFull = -5, //队伍已满
        ZhanLiLow = -6, //未达到队伍的最低战力要求
        NoAcceptableTeam = -7, //没有合适的队伍

        LeaveTeam = -11, //离开队伍
        BeRemovedFromTeam = -12, //被请出队伍

        ServerException = -13, // 服务器异常
        CenterServerFailed = -14,
        TeamAlreadyStart = -15, //队伍已经开始游戏
        NotInMyTeam = -16, // 不在我的队伍中
        MemeberNotReady = -17,
        KFServerIsBusy = -18, // 服务器繁忙
    }
}