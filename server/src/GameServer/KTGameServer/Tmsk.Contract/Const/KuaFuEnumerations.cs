using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KF.Contract.Data
{
    public enum GameTypes
    {
        None = 0,
        HuanYingSiYuan = 1, // 幻影寺院
        TianTi = 2,         // 天梯
        MoRiJudge = 3,      // 末日审判
        ElementWar = 4,
        YongZheZhanChang = 5,
        KuaFuBoss = 6,
        KuaFuMap = 7,
        KuaFuCopy = 8,      // 跨服副本
        Spread = 9,
        LangHunLingYu = 10,
        CopyWolf = 11,
        ZhengBa = 12,       // 众神争霸
        CoupleArena = 13,   // 情侣竞技
        Ally = 14,          // 战盟结盟
        KingOfBattle = 15,  // 王者战场
    }

    public enum GameFuBenState
    {
        Wait, //等待加人
        WaitConfirm, //等待所有队友确认
        Start, //开战
        End,
    }

    public enum KuaFuRoleStates
    {
        None = 0,
        SignUp = 1,
        SignUpWaiting = 2,
        NotifyEnterGame = 3,
        EnterGame = 4,
        StartGame = 5,
        EndGame = 6,
        Offline = 7,
    }

    public enum KuaFuEventTypes
    {
        RoleSignUp,
        RoleStateChange,
        NotifyWaitingRoleCount,
        UpdateAndNotifyEnterGame,
        CopyCanceled, //副本取消
        NotifyRealEnterGame, //通知进入副本(队友全部确认)

        UpdateLhlyBhData, //圣域争霸帮会信息变更
        UpdateLhlyCityData, //圣域争霸城池信息变更
        UpdateLhlyOtherCityList, //圣域争霸4个展示城池ID列表        
        UpdateLhlyCityOwnerList, //圣域城主信息
        UpdateLhlyCityOwnerAdmire, //圣域城主膜拜信息更新

        KFCopyTeamCreate = 10000, // 跨服房间创建
        KFCopyTeamJoin, // 加入房间
        KFCopyTeamKickout, //踢人
        KFCopyTeamLeave, //退出房间
        KFCopyTeamSetReady, //准备
        KFCopyTeamStart, //开始游戏
        KFCopyTeamDestroty, //超时销毁？？？

        SpreadCount,    

        ZhengBaSupport, // 服争霸支持日志
        ZhengBaPkLog, // 众神争霸pk日志
        ZhengBaNtfEnter, // 众神争霸进入
        ZhengBaMirrorFight, // 众神争霸镜像出战
        ZhengBaButtetinJoin, // 众神争霸参与提示邮件

        CoupleArenaCanEnter, // 情侣竞技匹配ok

        AllyLog,
        //AllyCancel,
        Ally,
        AllyRemove,
        AllyAccept,
        AllyAcceptRemove,
        AllyRequest,
        AllyRequestRemove,
        AllyUnionUpdate,
        KFAlly,
        KFAllyRemove,
    }

    public enum KuaFuRoleExtendDataTypes
    {
        GameFuBenRoleCount, //所在排队队列中,匹配到的队伍人数
        TianTiRoleData, //天梯角色数据
        KuaFuRoleState, //角色跨服状态
    }

    public enum CopyCancelReason
    {
        Unknown = 0,
        TeamMemberQuit = 1, // 队友退出
    }


   
}
