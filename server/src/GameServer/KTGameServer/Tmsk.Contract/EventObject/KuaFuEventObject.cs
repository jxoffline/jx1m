using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tmsk.Contract;

using KF.Contract.Data;

namespace KF.Client
{
    public class KuaFuFuBenRoleCountEvent : EventObjectEx
    {
        public int RoleId;
        public int RoleCount;

        public KuaFuFuBenRoleCountEvent(int roleId, int count)
            : base((int)GlobalEventTypes.KuaFuRoleCountChange)
        {
            RoleId = roleId;
            RoleCount = count;
        }
    }

    public class NotifyLhlyBangHuiDataGameEvent : EventObjectEx
    {
        public object Arg;
        public NotifyLhlyBangHuiDataGameEvent(object arg)
            : base((int)GlobalEventTypes.NotifyLhlyBangHuiData)
        {
            Arg = arg;
        }
    }

    public class NotifyLhlyCityDataGameEvent : EventObjectEx
    {
        public object Arg;
        public NotifyLhlyCityDataGameEvent(object arg)
            : base((int)GlobalEventTypes.NotifyLhlyCityData)
        {
            Arg = arg;
        }
    }

    public class NotifyLhlyOtherCityListGameEvent : EventObjectEx
    {
        public Dictionary<int, List<int>> Arg;
        public NotifyLhlyOtherCityListGameEvent(Dictionary<int, List<int>> arg)
            : base((int)GlobalEventTypes.NotifyLhlyOtherCityList)
        {
            Arg = arg;
        }
    }

    public class NotifyLhlyCityOwnerHistGameEvent : EventObjectEx
    {
        public List<LangHunLingYuKingHist> Arg;
        public NotifyLhlyCityOwnerHistGameEvent(List<LangHunLingYuKingHist> arg)
            : base((int)GlobalEventTypes.NotifyLhlyCityOwnerHist)
        {
            Arg = arg;
        }
    }

    public class NotifyLhlyCityOwnerAdmireGameEvent : EventObjectEx
    {
        public int RoleID;
        public int AdmireCount;
        public NotifyLhlyCityOwnerAdmireGameEvent(int rid, int admirecount)
            : base((int)GlobalEventTypes.NotifyLhlyCityOwnerAdmire)
        {
            RoleID = rid;
            AdmireCount = admirecount;
        }
    }

    public class KuaFuNotifyEnterGameEvent : EventObjectEx
    {
        public object Arg;
        public int TeamCombatAvg; // 队伍平均战斗力
        public KuaFuNotifyEnterGameEvent(object arg)
            : base((int)GlobalEventTypes.KuaFuNotifyEnterGame)
        {
            Arg = arg;
        }
    }

    public class KuaFuNotifyCopyCancelEvent : EventObjectEx
    {
        public int RoleId;
        public int GameId;
        public int Reason;
        public KuaFuNotifyCopyCancelEvent(int roleId, int gameId, int reason)
            : base((int)GlobalEventTypes.KuaFuCopyCanceled)
        {
            this.RoleId = roleId;
            this.GameId = gameId;
            this.Reason = reason;
        }
    }

    public class KuaFuNotifyRealEnterGameEvent : EventObjectEx
    {
        public int RoleId;
        public int GameId;
        public KuaFuNotifyRealEnterGameEvent(int roleId, int gameId)
            : base((int)GlobalEventTypes.KuaFuNotifyRealEnterGame)
        {
            this.RoleId = roleId;
            this.GameId = gameId;
        }
    }


    /// <summary>
    /// 包装跨服创建房间的事件，继承EventObjectEx，方便投递给事件监听
    /// </summary>
    public class KFCopyRoomCreateEvent : EventObjectEx
    {
        public CopyTeamCreateData Data;
        public KFCopyRoomCreateEvent(CopyTeamCreateData data)
            : base((int)GlobalEventTypes.KFCopyTeamCreate)
        {
            this.Data = data;
        }
    }

    /// <summary>
    /// 包装跨服加入房间的事件，继承EventObjectEx，方便投递给事件监听
    /// </summary>
    public class KFCopyRoomJoinEvent : EventObjectEx
    {
        public CopyTeamJoinData Data;
        public KFCopyRoomJoinEvent(CopyTeamJoinData data)
            : base((int)GlobalEventTypes.KFCopyTeamJoin)
        {
            this.Data = data;
        }
    }

    /// <summary>
    /// 包装跨服踢出房间的事件，继承EventObjectEx，方便投递给事件监听
    /// </summary>
    public class KFCopyRoomKickoutEvent : EventObjectEx
    {
        public CopyTeamKickoutData Data;
        public KFCopyRoomKickoutEvent(CopyTeamKickoutData data)
            : base((int)GlobalEventTypes.KFCopyTeamKickout)
        {
            this.Data = data;
        }
    }

    /// <summary>
    /// 包装跨服离开房间的事件，继承EventObjectEx，方便投递给事件监听
    /// </summary>
    public class KFCopyRoomLeaveEvent : EventObjectEx
    {
        public CopyTeamLeaveData Data;
        public KFCopyRoomLeaveEvent(CopyTeamLeaveData data)
            : base((int)GlobalEventTypes.KFCopyTeamLeave)
        {
            this.Data = data;
        }
    }

    /// <summary>
    /// 包装跨服房间的准备事件，继承EventObjectEx，方便投递给事件监听
    /// </summary>
    public class KFCopyRoomReadyEvent : EventObjectEx
    {
        public CopyTeamReadyData Data;
        public KFCopyRoomReadyEvent(CopyTeamReadyData data)
            : base((int)GlobalEventTypes.KFCopyTeamReady)
        {
            this.Data = data;
        }
    }

    /// <summary>
    /// 包装跨服离开房间的事件，继承EventObjectEx，方便投递给事件监听
    /// </summary>
    public class KFCopyRoomStartEvent : EventObjectEx
    {
        public CopyTeamStartData Data;
        public KFCopyRoomStartEvent(CopyTeamStartData data)
            : base((int)GlobalEventTypes.KFCopyTeamStart)
        {
            this.Data = data;
        }
    }

    /// <summary>
    /// 跨服副本队伍销毁的事件
    /// </summary>
    public class KFCopyTeamDestroyEvent : EventObjectEx
    {
        public CopyTeamDestroyData Data;
        public KFCopyTeamDestroyEvent(CopyTeamDestroyData data)
            : base((int)GlobalEventTypes.KFCopyTeamDestroy)
        {
            this.Data = data;
        }
    }

    public class KFNotifySpreadCountGameEvent : EventObjectEx
    {
        public int PZoneID;
        public int PRoleID;
        public int CountRole;
        public int CountVip;
        public int CountLevel;

        public KFNotifySpreadCountGameEvent(int pzoneID, int proleID, int countRole, int countVip, int countLevel)
            : base((int)GlobalEventTypes.KFSpreadCount)
        {
            this.PZoneID = pzoneID;
            this.PRoleID = proleID;
            this.CountRole = countRole;
            this.CountVip = countVip;
            this.CountLevel = countLevel;

        }
    }

    public class KFZhengBaSupportEvent : EventObjectEx
    {
        public ZhengBaSupportLogData Data;
        public KFZhengBaSupportEvent(ZhengBaSupportLogData data)
            : base((int)GlobalEventTypes.KFZhengBaSupportLog)
        {
            this.Data = data;
        }
    }

    public class KFZhengBaPkLogEvent : EventObjectEx
    {
        public ZhengBaPkLogData Log;
        public KFZhengBaPkLogEvent(ZhengBaPkLogData log)
            : base((int)GlobalEventTypes.KFZhengBaPkLog)
        {
            this.Log = log;
        }
    }

    public class KFZhengBaNtfEnterEvent : EventObjectEx
    {
        public ZhengBaNtfEnterData Data;
        public KFZhengBaNtfEnterEvent(ZhengBaNtfEnterData data)
            : base((int)GlobalEventTypes.KFZhengBaNtfEnter)
        {
            this.Data = data;
        }
    }

    public class KFZhengBaMirrorFightEvent : EventObjectEx
    {
        public ZhengBaMirrorFightData Data;
        public KFZhengBaMirrorFightEvent(ZhengBaMirrorFightData data)
            : base((int)GlobalEventTypes.KFZhengBaMirrorFight)
        {
            this.Data = data;
        }
    }

    public class KFZhengBaBulletinJoinEvent : EventObjectEx
    {
         public ZhengBaBulletinJoinData Data;
         public KFZhengBaBulletinJoinEvent(ZhengBaBulletinJoinData data)
            : base((int)GlobalEventTypes.KFZhengBaBulletinJoin)
        {
            this.Data = data;
        }
    }

    public class CoupleArenaCanEnterEvent : EventObjectEx
    {
        public CoupleArenaCanEnterData Data;
        public CoupleArenaCanEnterEvent(CoupleArenaCanEnterData data)
            : base((int)GlobalEventTypes.CoupleArenaCanEnter)
        {
            this.Data = data;
        }
    }

    #region ally

    public class KFNotifyAllyStartGameEvent : EventObjectEx
    {
        public KFNotifyAllyStartGameEvent()
            : base((int)GlobalEventTypes.KFAllyStart)
        {
        }
    }

    public class KFNotifyAllyGameEvent : EventObjectEx
    {
        public int UnionID;

        public KFNotifyAllyGameEvent(int unionID)
            : base((int)GlobalEventTypes.Ally)
        {
            this.UnionID = unionID;
        }
    }

    public class KFNotifyAllyLogGameEvent : EventObjectEx
    {
        public object LogList;

        public KFNotifyAllyLogGameEvent(object logList)
            : base((int)GlobalEventTypes.AllyLog)
        {
            this.LogList = logList;
        }
    }

    public class KFNotifyAllyTipGameEvent : EventObjectEx
    {
        public int UnionID;
        public int TipID;

        public KFNotifyAllyTipGameEvent(int unionID, int tipID)
            : base((int)GlobalEventTypes.AllyTip)
        {
            this.UnionID = unionID;
            this.TipID = tipID;
        }
    }

    #endregion
}
