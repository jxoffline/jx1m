using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBuf;
using Tmsk.Contract;

namespace KF.Contract.Data
{
    #region Enum & Consts
    /// <summary>
    /// 主要，策划后来又需求说，前几天的人数允许修改，不限制一定是100,64,32
    /// 所以 这个枚举的意义更像是第几天
    /// 但是16强之后的人数必须是16,8,4,2,1
    /// </summary>
    public enum EZhengBaGrade
    {
        Grade1 = 1, // 冠军
        Grade2 = 2, // 亚军
        Grade4 = 4, // 4强
        Grade8 = 8, // 8强
        Grade16 = 16, // 16强
        Grade32 = 32, // 32强
        Grade64 = 64, // 64强
        Grade100 = 100, // 100强

        GradeInvalid,
    }

    public enum EZhengBaPKResult
    {
        Invalid = 0, // 无效
        Win = 1, // 胜利
        Fail = 2, // 失败
    }

    public enum EZhengBaState
    {
        None = 0, // 无
        UpGrade = 1, // 晋级
        Failed = 2,  // 淘汰
    }

    public enum EZhengBaSupport
    {
        Invalid = 0, // 无效
        Support = 1,  // 支持
        Oppose = 2,  // 反对
        YaZhu = 3,   // 押注
    }

    /// <summary>
    /// 众神争霸pk规则
    /// </summary>
    public enum EZhengBaMatching
    {
        Random = 1, // 随机匹配
        Group = 2, // 按组匹配
    }

    public enum EZhengBaEnterType
    {
        Player = 1, // 立即进入
        Mirror = 2, // 镜像进入
    }
    
    public static class ZhengBaConsts
    {
        public const int NoneGroup = 0;
        public const int DefaultJoinRoleNum = 100;
        // public const int StartMonthDay = 10;
        public static int StartMonthDay { get { return ZhengBaConfig.StartDay; } }
        public const int ContinueDays = 7;
        public const int MaxGroupNum = 16;

        public const int MaxPkLogNum = 100;
        public const int MaxSupportLogNum = 30;

        public const string MatchConfigFile = @"Config\Match.xml";
        public const string MatchAwardConfigFile = @"Config\MatchAward.xml";
        public const string SupportConfigFile = @"Config\Sustain.xml";
        public const string BirthPointConfigFile = @"Config\MatchBirthPoint.xml";

        // 策划反馈说写死，一方进入副本之后，最多等待30秒，如果对方还不进入，那么直接胜利
        public const int CopyPrepareTimeOutSecs = 30;

        public const int DefaultAsyncMonth = 201111;
    }
    #endregion

    #region Utils
    public static class ZhengBaUtils
    {
        public static int MakeUnionGroup(int group1, int group2)
        {
            if (group1 > group2)
            {
                int tmp = group1;
                group1 = group2;
                group2 = tmp;
            }

            return group1 * 1000 + group2;
        }

        public static void SplitUnionGroup(int union, out int group1, out int group2)
        {
            group1 = union / 1000;
            group2 = union % 1000;
        }

        public static int MakeMonth(DateTime dt)
        {
            return dt.Year * 100 + dt.Month;
        }

        /// <summary>
        /// 检查两个group能否组成第day天pk的group
        /// </summary>
        /// <param name="group1"></param>
        /// <param name="group2"></param>
        /// <param name="day"></param>
        /// <returns></returns>
        public static bool IsValidPkGroup(int group1, int group2, int day)
        {
            if (group1 == group2) return false;
            if (day < 4 || day > ZhengBaConsts.ContinueDays) return false;

            for (int beginGroup = 1, step = (int)Math.Pow(2, day - 3);
                beginGroup < ZhengBaConsts.MaxGroupNum;
                beginGroup += step)
            {
                int endGroup = beginGroup + step;
                if ((beginGroup <= group1 && group1 < endGroup)
                    && (beginGroup <= group2 && group2 < endGroup))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 获取第day天结束后晋级到的等级
        /// </summary>
        /// <param name="day"></param>
        /// <returns></returns>
        public static EZhengBaGrade GetDayUpGrade(int day)
        {
            if (day < 1 || day > ZhengBaConsts.ContinueDays) return EZhengBaGrade.GradeInvalid;
            else if (day == 1) return EZhengBaGrade.Grade64;
            else if (day == 2) return EZhengBaGrade.Grade32;
            else if (day == 3) return EZhengBaGrade.Grade16;
            else if (day == 4) return EZhengBaGrade.Grade8;
            else if (day == 5) return EZhengBaGrade.Grade4;
            else if (day == 6) return EZhengBaGrade.Grade2;
            else return EZhengBaGrade.Grade1;
        }

        /// <summary>
        /// 获取第day天参赛的等级
        /// </summary>
        /// <param name="day"></param>
        /// <returns></returns>
        public static EZhengBaGrade GetDayJoinGrade(int day)
        {
            if (day < 1 || day > ZhengBaConsts.ContinueDays) return EZhengBaGrade.GradeInvalid;
            else if (day == 1) return EZhengBaGrade.Grade100;
            else if (day == 2) return EZhengBaGrade.Grade64;
            else if (day == 3) return EZhengBaGrade.Grade32;
            else if (day == 4) return EZhengBaGrade.Grade16;
            else if (day == 5) return EZhengBaGrade.Grade8;
            else if (day == 6) return EZhengBaGrade.Grade4;
            else return EZhengBaGrade.Grade2;
        }

        /// <summary>
        /// 根据grade获取是第几天活动的结束时的grade
        /// </summary>
        /// <param name="grade"></param>
        /// <returns></returns>
        public static int WhichDayResultByGrade(EZhengBaGrade grade)
        {
            if (grade == EZhengBaGrade.Grade100) return 0;
            else if (grade == EZhengBaGrade.Grade64) return 1;
            else if (grade == EZhengBaGrade.Grade32) return 2;
            else if (grade == EZhengBaGrade.Grade16) return 3;
            else if (grade == EZhengBaGrade.Grade8) return 4;
            else if (grade == EZhengBaGrade.Grade4) return 5;
            else if (grade == EZhengBaGrade.Grade2) return 6;
            else if (grade == EZhengBaGrade.Grade1) return 7;
            else return -1;
        }

        public static List<RangeKey> GetDayPkGroupRange(int day)
        {
            List<RangeKey> rangeList = new List<RangeKey>();
            if (day >= 4 && day <= ZhengBaConsts.ContinueDays)
            {
                // 第4天开始之后，才分组pk
                for (int begin = 1, step = (int)Math.Pow(2, day-3); begin < ZhengBaConsts.MaxGroupNum; begin += step)
                {
                    rangeList.Add(new RangeKey(begin, begin + step - 1));
                }
            }
            return rangeList;
        }
    }
    #endregion

    #region Protobuf & Serializable
    /// <summary>
    /// 众神争霸---赞、贬、押注统计，只用于前16强选手
    /// </summary>
    [ProtoContract]
    [Serializable]
    public class ZhengBaSupportAnalysisData
    {
        [ProtoMember(1)]
        public int UnionGroup;
        [ProtoMember(2)]
        public int Group;
        [ProtoMember(3)]
        public int TotalSupport;
        [ProtoMember(4)]
        public int TotalOppose;
        [ProtoMember(5)]
        public int TotalYaZhu;

        // 服务器需求
        [ProtoMember(6)]
        public int RankOfDay;
    }

    /// <summary>
    /// 众神争霸---支持日志 赞、贬、押注，只用于前16强选手
    /// </summary>
    [Serializable]
    [ProtoContract]
    public class ZhengBaSupportLogData
    {
        [ProtoMember(1)]
        public int FromRoleId;
        [ProtoMember(2)]
        public int FromZoneId;
        [ProtoMember(3)]
        public string FromRolename;
        /// <summary>
        /// 操作类型，赞、贬、押注，参考EZhengBaSupport
        /// </summary>
        [ProtoMember(4)]
        public int SupportType;
        [ProtoMember(5)]
        public int ToUnionGroup;
        [ProtoMember(6)]
        public int ToGroup;
        [ProtoMember(7)]
        public DateTime Time;

        // 以下字段为服务器需求
        [ProtoMember(8)]
        public int Month;
        [ProtoMember(9)]
        public int RankOfDay;
        [ProtoMember(10)]
        public int FromServerId;
    }

    /// <summary>
    /// 众神争霸 --- 通知客户端pk结果
    /// </summary>
    [Serializable]
    [ProtoContract]
    public class ZhengBaNtfPkResultData
    {
        [ProtoMember(1)]
        public int RoleID;
        /// <summary>
        /// [1---16]表示有效，如果有效，客户端出现假的“随机编号”的按钮
        /// </summary>
        [ProtoMember(2)]
        public int RandGroup;
        /// <summary>
        /// 仍然还需要胜利几场才能晋级
        /// </summary>
        [ProtoMember(3)]
        public int StillNeedWin;
        /// <summary>
        /// 剩余还有几个晋级名额
        /// </summary>
        [ProtoMember(4)]
        public int LeftUpGradeNum;
        /// <summary>
        /// 是否胜利
        /// </summary>
        [ProtoMember(5)]
        public bool IsWin;
        /// <summary>
        /// 是否晋级
        /// </summary>
        [ProtoMember(6)]
        public bool IsUpGrade;
        /// <summary>
        /// 如果晋级，那么新的EZhengBaGrade
        /// </summary>
        [ProtoMember(7)]
        public int NewGrade;
    }

    /// <summary>
    /// 众神争霸---pk日志
    /// 根据策划案文档，只记录有参赛方胜利或者晋级的pk日志
    /// </summary>
    [ProtoContract]
    [Serializable]
    public class ZhengBaPkLogData
    {
        /// <summary>
        /// 第几天活动的战斗日志，用于可以短根据Match.xml显示战斗名字
        /// Day="1" Name="100进64"
        /// Day="2" Name="64进32"
        /// ...
        /// </summary>
        [ProtoMember(1)]
        public int Day;
        [ProtoMember(2)]
        public int RoleID1;
        [ProtoMember(3)]
        public int ZoneID1;
        [ProtoMember(4)]
        public string RoleName1;
        [ProtoMember(5)]
        public int RoleID2;
        [ProtoMember(6)]
        public int ZoneID2;
        [ProtoMember(7)]
        public string RoleName2;

        /// <summary>
        /// 参考EZhengBaPKResult
        /// EZhengBaPKResult.Win ===> RoleID1 胜利
        /// EZhengBaPKResult.Fail ===> RoleID2 胜利
        /// EZhengBaPKResult.Invalid ===> pk异常，服务器专用
        /// </summary>
        [ProtoMember(8)]
        public int PkResult;
        /// <summary>
        /// 只有PkResult == Win，该字段才有效，表示胜方是否晋级
        /// </summary>
        [ProtoMember(9)]
        public bool UpGrade;

        // 以下字段为服务器需要
        [ProtoMember(10)]
        public int Month;
        [ProtoMember(11)]
        public bool IsMirror1;
        [ProtoMember(12)]
        public bool IsMirror2;
        [ProtoMember(13)]
        public DateTime StartTime;
        [ProtoMember(14)]
        public DateTime EndTime;
    }

    /// <summary>
    /// 争霸倒计时状态
    /// </summary>
    [ProtoContract]
    [Serializable]
    public class ZhengBaMiniStateData
    {
        /// <summary>
        /// 如果该值 > 0, 表示距离活动开始剩余秒数
        /// </summary>
        [ProtoMember(1)]
        public long PkStartWaitSec;
        /// <summary>
        /// 如果该值 > 0, 表示距离下轮开始剩余秒数
        /// </summary>
        [ProtoMember(2)]
        public long NextLoopWaitSec;
        /// <summary>
        /// 如果该值 > 0, 表示距离本轮结束剩余秒数
        /// </summary>
        [ProtoMember(3)]
        public long LoopEndWaitSec;

        /// <summary>
        /// 争霸赛是否开启
        /// </summary>
        [ProtoMember(4, IsRequired = true)]
        public bool IsZhengBaOpened;

        /// <summary>
        /// 本月是否举行
        /// </summary>
        [ProtoMember(5, IsRequired = true)]
        public bool IsThisMonthInActivity;
    }

    /// <summary>
    /// 由于t_tianti_roles中没有存储角色名字，众神争霸的全服广播战报需要用到名字
    /// 所以众神争霸版本在t_tianti_roles中增加rolename这一字段，但是该字段可能为空(历史数据)
    /// 所以生成争霸的100个参赛角色时，如果名字为空，那么从t_tianti_roles的data1字段中反序列化取出来名字
    /// 虽然不合理，但是也是个解决办法
    /// 下面这个结构参考 Server.Data.TianTiPaiHangRoleData
    /// 仅仅反序列化出来RoleName即可
    /// </summary>
    [ProtoContract]
    public class TianTiPaiHangRoleData_OnlyName
    {
        [ProtoMember(2)]
        public string RoleName;
    }

    #endregion

    #region Only Serializable
    [Serializable]
    public class ZhengBaRoleInfoData : TianTiRoleInfoData
    {
        /// <summary>
        /// 众神争霸成绩
        /// </summary>
        public int Grade;

        /// <summary>
        /// 众神争霸16强对战编号
        /// </summary>
        public int Group;

        /// <summary>
        /// 众神争霸当前状态
        /// </summary>
        public int State;
    }

    /// <summary>
    /// 跨服中心与GameServer同步
    /// </summary>
    [Serializable]
    public class ZhengBaSyncData
    {
        public int Month;
        public int RankResultOfDay;
        public int RealActDay;
        public DateTime RoleModTime;
        public List<ZhengBaRoleInfoData> RoleList;
        public DateTime SupportModTime;
        public List<ZhengBaSupportAnalysisData> SupportList;
        public bool TodayIsPking;
        public bool IsThisMonthInActivity;
        // 仅仅用于GameServer计算活动状态的时候做时间校正
        public DateTime CenterTime;
    }

    /// <summary>
    /// 匹配完成，通知GameServer邀请玩家进入
    /// </summary>
    [Serializable]
    public class ZhengBaNtfEnterData
    {
        public int RoleId1;
        public int RoleId2;
        public int GameId;
        public int ToServerId;
        public int Day;
        public int Loop;

        public string ToServerIp;
        public int ToServerPort;
    }

    [Serializable]
    public class ZhengBaMirrorFightData
    {
        public int RoleId;
        public int GameId;
        public int ToServerId;
    }

    [Serializable]
    public class ZhengBaBulletinJoinData
    {
        public enum ENtfType
        {
            None,
            BulletinServer,//选手确定后，全服公告
            MailJoinRole,//选手确定后，所有参与角色收到通知邮件
            MailUpgradeRole,//每天晋级的选手会收到当天的晋级通知邮件
            DayLoopEnd, // 每日结束后公告
        }

        public ENtfType NtfType = ENtfType.None;
        public int Args1 = 0;
    }
    #endregion
}
