using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tmsk.Contract
{
    /// <summary>
    /// 分配区间10000-19999
    /// </summary>
    public enum GlobalEventTypes
    {
        KuaFuRoleCountChange = 10000,
        KuaFuNotifyEnterGame,//队伍匹配成功，成员为确认，通知玩家可以进入副本
        PlayerCaiJi,  //玩家采集
        KuaFuCopyCanceled,
        KuaFuNotifyRealEnterGame,//所有成员确认，通知玩家可以进入副本

        // 跨服队伍相关的事件
        KFCopyTeamCreate,
        KFCopyTeamJoin,
        KFCopyTeamReady,
        KFCopyTeamKickout,
        KFCopyTeamLeave,
        KFCopyTeamDestroy,
        KFCopyTeamStart,

        KFSpreadCount,

        NotifyLhlyBangHuiData,
        NotifyLhlyCityData,
        NotifyLhlyOtherCityList,
        NotifyLhlyCityOwnerHist,
        NotifyLhlyCityOwnerAdmire,

        KFZhengBaSupportLog,
        KFZhengBaPkLog,
        KFZhengBaNtfEnter,
        KFZhengBaMirrorFight,
        KFZhengBaBulletinJoin,

        CoupleArenaCanEnter,

        Ally,
        AllyLog,
        AllyTip,
        KFAllyStart,
    }
}
