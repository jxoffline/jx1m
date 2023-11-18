using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace Tmsk.Contract.KuaFuData
{
    [Serializable]
    [ProtoContract]
    public class AllyData
    {
        [ProtoMember(1)]
        public int UnionID = 0;

        [ProtoMember(2)]
        public int UnionZoneID = 0;

        [ProtoMember(3)]
        public string UnionName = "";

        [ProtoMember(4)]
        public int UnionLevel = 0;

        [ProtoMember(5)]
        public int UnionNum = 0;

        [ProtoMember(6)]
        public int LeaderID = 0;

        [ProtoMember(7)]
        public int LeaderZoneID = 0;

        [ProtoMember(8)]
        public string LeaderName = "";

        [ProtoMember(9)]
        public DateTime LogTime = DateTime.MinValue;

        [ProtoMember(10)]
        public int LogState = 0;

        public void Copy(AllyData target)
        {
            this.UnionID = target.UnionID;
            this.UnionZoneID = target.UnionZoneID;
            this.UnionName = target.UnionName;
            this.UnionLevel = target.UnionLevel;
            this.UnionNum = target.UnionNum;
            this.LeaderID = target.LeaderID;
            this.LeaderZoneID = target.LeaderZoneID;
            this.LeaderName = target.LeaderName;
            this.LogTime = target.LogTime;
            this.LogState = target.LogState;
        }
    }

    [Serializable]
    public class KFAllyData : AllyData
    {
        public int ServerID = 0;
        public DateTime UpDateTime = DateTime.MinValue;

        public void UpdateLogtime()
        {
            UpDateTime = DateTime.Now;
        }

        public void Copy(KFAllyData target)
        {
            this.UnionID = target.UnionID;
            this.UnionZoneID = target.UnionZoneID;
            this.UnionName = target.UnionName;
            this.UnionLevel = target.UnionLevel;
            this.UnionNum = target.UnionNum;
            this.LeaderID = target.LeaderID;
            this.LeaderZoneID = target.LeaderZoneID;
            this.LeaderName = target.LeaderName;
            this.LogTime = target.LogTime;
            this.LogState = target.LogState;
            this.ServerID = target.ServerID;
            this.UpDateTime = target.UpDateTime;
        }
    }

    [Serializable]
    [ProtoContract]
    public class AllyLogData
    {
        [ProtoMember(1)]
        public int UnionID = 0;

        [ProtoMember(2)]
        public int UnionZoneID = 0;

        [ProtoMember(3)]
        public string UnionName = "";

        [ProtoMember(4)]
        public int MyUnionID = 0;

        [ProtoMember(5)]
        public DateTime LogTime = DateTime.MinValue;

        [ProtoMember(6)]
        public int LogState = 0;
    }


    public enum EAlly
    {
        EAddUnion = -18,            //需要添加战盟
        EFail = -17,                //中心操作失败
        EServer = -16,              //服务器错误

        //ENoAlly = -15,              //未结盟
        //ENoRequest = -14,           //未请求结盟
        ENoTargetUnion = -13,         //战盟id错误

        EAllyRequestMax = -12,      //超出上限——发起请求
        EAllyMax = -11,             //超出上限——结盟
        EMore = -10,                //已经发起结盟请求，不能重复请求
        EIsAlly = -9,               //已经结盟
        EMoney = -8,                //战盟资金不足
        ENotLeader = -7,            //不是战盟首领
        EIsSelf = -6,               //不能同自己战盟结盟
        EUnionLevel = -5,           //战盟等级不足，不能同其他战盟结盟
        EUnionJoin = -4,            //未加入战盟
        EName = -3,                 //战盟名称不存在（或者战盟等级未达到5级，不能结盟）
        EZoneID = -2,               //战盟服务器区号不存在    
        EAllyRequest = -1,          //结盟请求发送失败
        AllyRequestSucc = 1,        //结盟请求发送成功（——日志）

        EAlly = 10,                 //结盟操作失败     
        AllyRefuse = 11,            //拒绝结盟
        AllyAgree = 12,             //同意结盟

        AllyRefuseOther = 20,       //对方拒绝结盟（——日志）
        AllyAgreeOther = 21,        //对方同意结盟（——日志）

        EAllyCancel = 30,           //取消结盟请求失败
        AllyCancelSucc = 31,        //取消结盟请求成功

        EAllyRemove = 40,           //我解除结盟失败
        AllyRemoveSucc = 41,        //我解除结盟成功（——日志）
        AllyRemoveSuccOther = 42,   //对方解除结盟关系（——日志）

        Succ = 50,                  //成功

        Default = 0,                //默认状态
    }

    public enum EAllyDataType
    {
        Ally = 1,
        Accept = 2,
        Request = 3,
    }

    public enum EAllyOperate
    {
        Agree = 1,  //同意结盟
        Refuse = 2, //拒绝结盟
        Remove = 3, //解除结盟
        Cancel = 4, //取消请求
    }
}
