using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KF.Contract.Data
{
    /// <summary>
    /// 创建队伍请求  GameServer ---> Hosting
    /// </summary>
    [Serializable]
    public class KFCopyTeamCreateReq
    {
        public CopyTeamMemberData Member;
        public int CopyId;
        public int MinCombat;
        public int AutoStart; // 人满自动开
        public long TeamId; // 队伍id由gameserver创建，gameserver用区号和时间信息保证唯一
    }

    /// <summary>
    /// 创建队伍回应  Hosting ---> GameServer
    /// </summary>
    [Serializable]
    public class KFCopyTeamCreateRsp
    {
        public CopyTeamErrorCodes ErrorCode;
        public long TeamId;
        public CopyTeamCreateData Data;
    }

    /// <summary>
    /// 加入队伍请求
    /// </summary>
    [Serializable]
    public class KFCopyTeamJoinReq
    {
        public CopyTeamMemberData Member;
        public int CopyId;
        public long TeamId;
    }

    /// <summary>
    /// 加入队伍回应
    /// </summary>
    [Serializable]
    public class KFCopyTeamJoinRsp
    {
        public CopyTeamErrorCodes ErrorCode;
        public CopyTeamJoinData Data;
    }

    #region 准备状态
    [Serializable]
    public class KFCopyTeamSetReadyReq
    {
        public int RoleId;
        public long TeamId;
        public int Ready;
    }

    [Serializable]
    public class KFCopyTeamSetReadyRsp
    {
        public CopyTeamErrorCodes ErrorCode;
        public CopyTeamReadyData Data;
    }
    #endregion

    #region 离开房间
    [Serializable]
    public class KFCopyTeamLeaveReq
    {
        public int ReqServerId;
        public int RoleId;
        public long TeamId;
    }

    [Serializable]
    public class KFCopyTeamLeaveRsp
    {
        public CopyTeamErrorCodes ErrorCode;
        public CopyTeamLeaveData Data;
    }
    #endregion

    /// <summary>
    ///  踢出队伍的请求
    /// </summary>
    [Serializable]
    public class KFCopyTeamKickoutReq
    {
        public int FromRoleId;
        public int ToRoleId;
        public long TeamId;
    }

    /// <summary>
    ///  踢出队伍的回应
    /// </summary>
    [Serializable]
    public class KFCopyTeamKickoutRsp
    {
        public CopyTeamErrorCodes ErrorCode;
        public CopyTeamKickoutData Data;
    }

    /// <summary>
    /// 开始游戏请求
    /// </summary>
    [Serializable]
    public class KFCopyTeamStartReq
    {
        public int RoleId;
        public long TeamId;
        public int LastMs; // 副本持续毫秒，用于中心检测超时，需要关闭的副本
    }

    /// <summary>
    /// 开始游戏回应
    /// </summary>
    [Serializable]
    public class KFCopyTeamStartRsp
    {
        public CopyTeamErrorCodes ErrorCode;
        public CopyTeamStartData Data;
    }

    #region async kf copy event data
    /// <summary>
    /// 创建跨服副本队伍的数据 用于广播给各游戏服务器
    /// </summary>
    [Serializable]
    public class CopyTeamCreateData
    {
        public CopyTeamMemberData Member;
        public int MinCombat;
        public long TeamId;
        public int CopyId;
        public int AutoStart;
    }

    /// <summary>
    /// 加入房间的事件 用于广播给各游戏服务器
    /// </summary>
    [Serializable]
    public class CopyTeamJoinData
    {
        public CopyTeamMemberData Member;
        public long TeamId;
    }

    /// <summary>
    /// 踢出队伍的事件 用于广播给各游戏服务器
    /// </summary>
    [Serializable]
    public class CopyTeamKickoutData
    {
        public int FromRoleId;
        public int ToRoleId;
        public long TeamId;
    }

    /// <summary>
    /// 离开队伍的事件 用于广播给各游戏服务器
    /// </summary>
    [Serializable]
    public class CopyTeamLeaveData
    {
        public long TeamId;
        public int RoleId;
    }

    /// <summary>
    /// 更新准备状态的事件 用于广播给各游戏服务器
    /// </summary>
    [Serializable]
    public class CopyTeamReadyData
    {
        public int RoleId;
        public long TeamId;
        public int Ready;
    }

    /// <summary>
    /// 开始游戏房间的事件 用于广播给各游戏服务器
    /// </summary>
    [Serializable]
    public class CopyTeamStartData
    {
        public long TeamId;
        public long StartMs;
        public int FuBenSeqId;
        public int ToServerId;
    }

    /// <summary>
    /// 副本副本队伍销毁事件 用于广播给各游戏服务器
    /// </summary>
    [Serializable]
    public class CopyTeamDestroyData
    {
        public long TeamId;
    }
    #endregion
}