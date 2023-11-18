using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;

namespace KF.Contract.Data
{
    /// <summary>
    /// 组队数据
    /// </summary>
    [Serializable]
    [ProtoContract]
    public class CopyTeamData
    {
        public CopyTeamData SimpleClone()
        {
            CopyTeamData simple = new CopyTeamData();
            simple.TeamID = TeamID;
            simple.LeaderRoleID = LeaderRoleID;
            simple.StartTime = StartTime;
            simple.GetThingOpt = GetThingOpt;
            simple.FuBenId = FuBenId;
            simple.FuBenSeqID = FuBenSeqID;
            simple.MinZhanLi = MinZhanLi;
            simple.AutoStart = AutoStart;
            simple.TeamRoles = null;
            simple.MemberCount = MemberCount;
            simple.TeamName = TeamName;
            simple.KFServerId = KFServerId;
            return simple;
        }

        /// <summary>
        /// 队伍流水ID
        /// </summary>
        [ProtoMember(1)]
        public long TeamID = 0;

        /// <summary>
        /// 队伍队长
        /// 根据代码的意思，发给客户端的==-1表示，队伍没了
        /// </summary>
        [ProtoMember(2)]
        public int LeaderRoleID = 0;

        /// <summary>
        /// 组队的成员列表
        /// </summary>
        [ProtoMember(3)]
        public List<CopyTeamMemberData> TeamRoles = new List<CopyTeamMemberData>();

        /// <summary>
        /// 在组队副本中表示开始时间,0表示未开始
        /// </summary>
        [ProtoMember(4)]
        public long StartTime = 0;

        /// <summary>
        /// 自由拾取选项
        /// </summary>
        [ProtoMember(5)]
        public int GetThingOpt = 0;

        /// <summary>
        /// 副本ID
        /// </summary>
        [ProtoMember(6)]
        public int FuBenId = 0;

        /// <summary>
        /// 运行时场景ID, 对于副本,是FuBenSeqID
        /// </summary>
        [ProtoMember(7)]
        public int FuBenSeqID = 0;

        /// <summary>
        /// 队长设定的最小战力要求
        /// </summary>
        [ProtoMember(8)]
        public int MinZhanLi = 0;

        /// <summary>
        /// 是否自动开始
        /// </summary>
        [ProtoMember(9)]
        public bool AutoStart = false;

        /// <summary>
        /// 成员个数
        /// </summary>
        [ProtoMember(10)]
        public int MemberCount = 0;

        /// <summary>
        /// 队伍名称(队长名称)
        /// </summary>
        [ProtoMember(11)]
        public string TeamName = string.Empty;

        /// <summary>
        /// 记录分配到哪个跨服活动服务器了
        /// </summary>
        [ProtoMember(12)]
        public int KFServerId;
    }

    /// <summary>
    /// 组队成员数据
    /// </summary>
    [Serializable]
    [ProtoContract]
    public class CopyTeamMemberData
    {
        /// <summary>
        /// 成员角色ID
        /// </summary>
        [ProtoMember(1)]
        public int RoleID = 0;

        /// <summary>
        /// 成员角色名称
        /// </summary>
        [ProtoMember(2)]
        public string RoleName;

        /// <summary>
        /// 角色的性别
        /// </summary>
        [ProtoMember(3)]
        public int RoleSex = 0;

        /// <summary>
        /// 成员的等级
        /// </summary>
        [ProtoMember(4)]
        public int Level = 0;

        /// <summary>
        /// 成员的职业
        /// </summary>
        [ProtoMember(5)]
        public int Occupation = 0;

        /// <summary>
        /// 当前的头像
        /// </summary>
        [ProtoMember(6)]
        public int RolePic = 0;

        /// <summary>
        /// 所在的地图的编号
        /// </summary>
        [ProtoMember(7)]
        public int MapCode = 0;

        /// <summary>
        /// 成员的在线状态
        /// </summary>
        [ProtoMember(8)]
        public int OnlineState = 0;

        /// <summary>
        /// 成员的最大血量
        /// </summary>
        [ProtoMember(9)]
        public int MaxLifeV = 0;

        /// <summary>
        /// 成员的当前血量
        /// </summary>
        [ProtoMember(10)]
        public int CurrentLifeV = 0;

        /// <summary>
        /// 成员的最大魔量
        /// </summary>
        [ProtoMember(11)]
        public int MaxMagicV = 0;

        /// <summary>
        /// 成员的当前魔量
        /// </summary>
        [ProtoMember(12)]
        public int CurrentMagicV = 0;

        /// <summary>
        /// 成员的当前X坐标
        /// </summary>
        [ProtoMember(13)]
        public int PosX = 0;

        /// <summary>
        /// 成员的当前Y坐标
        /// </summary>
        [ProtoMember(14)]
        public int PosY = 0;

        /// <summary>
        /// 成员战力
        /// </summary>
        [ProtoMember(15)]
        public int CombatForce = 0;

        /// <summary>
        /// 成员转生级别  MU新增 [1/10/2014 LiaoWei]
        /// </summary>
        [ProtoMember(16)]
        public int ChangeLifeLev = 0;

        /// <summary>
        /// 是否准备
        /// </summary>
        [ProtoMember(17)]
        public bool IsReady = false;

        /// <summary>
        /// 服务器ID
        /// </summary>
        [ProtoMember(18)]
        public int ServerId;

        /// <summary>
        /// 服务器区ID
        /// </summary>
        [ProtoMember(19)]
        public int ZoneId;
    }

    /// <summary>
    /// 搜索队伍的数据
    /// </summary>
    [ProtoContract]
    public class CopySearchTeamData
    {
        public CopySearchTeamData SimpleClone()
        {
            CopySearchTeamData simple = new CopySearchTeamData();
            simple.PageTeamsCount = PageTeamsCount;
            simple.StartIndex = StartIndex;
            simple.TotalTeamsCount = TotalTeamsCount;

            if (null != TeamDataList)
            {
                simple.TeamDataList = new List<CopyTeamData>();
                foreach (var item in TeamDataList)
                {
                    simple.TeamDataList.Add(item.SimpleClone());
                }
            }

            return simple;
        }

        /// <summary>
        /// 搜索的开始索引
        /// </summary>
        [ProtoMember(1)]
        public int StartIndex = 0;

        /// <summary>
        /// 当前总的条数
        /// </summary>
        [ProtoMember(2)]
        public int TotalTeamsCount = 0;

        /// <summary>
        /// 每页总的条数
        /// </summary>
        [ProtoMember(3)]
        public int PageTeamsCount = 0;

        /// <summary>
        /// 返回的队伍的列表
        /// </summary>
        [ProtoMember(4)]
        public List<CopyTeamData> TeamDataList = null;
    }
}