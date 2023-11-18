using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KF.Contract.Data
{
    /// <summary>
    /// 情侣竞技数据
    /// </summary>
    [Serializable]
    public class CoupleArenaCoupleDataK : IComparable<CoupleArenaCoupleDataK>
    {
        /// <summary>
        /// db 自增couple id
        /// </summary>
        public int Db_CoupleId;

        /// <summary>
        /// 丈夫角色id
        /// </summary>
        public int ManRoleId;

        /// <summary>
        /// 丈夫zoneid
        /// </summary>
        public int ManZoneId;

        /// <summary>
        /// 丈夫形象
        /// </summary>
        public byte[] ManSelectorData;

        /// <summary>
        /// 妻子角色id
        /// </summary>
        public int WifeRoleId;

        /// <summary>
        /// 妻子zoneid
        /// </summary>
        public int WifeZoneId;

        /// <summary>
        /// 妻子形象
        /// </summary>
        public byte[] WifeSelectorData;

        /// <summary>
        /// 总战斗次数
        /// </summary>
        public int TotalFightTimes;

        /// <summary>
        /// 总胜利次数
        /// </summary>
        public int WinFightTimes;

        /// <summary>
        /// 连胜次数
        /// </summary>
        public int LianShengTimes;

        /// <summary>
        /// 段位type
        /// </summary>
        public int DuanWeiType;

        /// <summary>
        /// 段位等级
        /// </summary>
        public int DuanWeiLevel;

        /// <summary>
        /// 积分
        /// </summary>
        public int JiFen;

        /// <summary>
        /// 排名
        /// </summary>
        public int Rank;

        /// <summary>
        /// 是否已离婚, 经济结束后，离婚不下榜，但是离婚后不应获得钻戒
        /// </summary>
        public int IsDivorced;

        /// <summary>
        /// 默认比较函数
        /// 积分从小到大，积分一样比较丈夫角色id从小到大
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(CoupleArenaCoupleDataK other)
        {
            if (this.JiFen < other.JiFen) return 1;
            else if (this.JiFen > other.JiFen) return -1;
            else return this.ManRoleId - other.ManRoleId;
        }
    }

    /// <summary>
    /// 情侣竞技同步数据
    /// </summary>
    [Serializable]
    public class CoupleArenaSyncData
    {
        /// <summary>
        /// 排行榜列表, 该字段参与同步
        /// </summary>
        public List<CoupleArenaCoupleDataK> RankList = new List<CoupleArenaCoupleDataK>();

        /// <summary>
        /// key: roleid value:竞技数据
        /// 该字段不参与同步, 中心和GameServer由RankList构建
        /// </summary>
        public Dictionary<int, CoupleArenaCoupleDataK> RoleDict = new Dictionary<int, CoupleArenaCoupleDataK>();

        /// <summary>
        /// 排行榜最后一次修改的时间，用于无变化时，减少GameServer和中心的数据量
        /// </summary>
        public DateTime ModifyTime;

        /// <summary>
        /// 辅助函数，根据RandList构建RoleDict
        /// </summary>
        public void BuildRoleDict()
        {
            this.RoleDict.Clear();
            for (int i = 0; i < RankList.Count; i++)
            {
                this.RoleDict[RankList[i].ManRoleId] = RankList[i];
                this.RoleDict[RankList[i].WifeRoleId] = RankList[i];
            }
        }
    }

    [Serializable]
    public class CoupleArenaCanEnterData
    {
        public long GameId;
        public int KfServerId;
        public int RoleId1;
        public int RoleId2;
    }

    public class CoupleArenaJoinData
    {
        public uint JoinSeq;
        public int ServerId;
        public int RoleId1;
        public int RoleId2;
        public DateTime StartTime;

        public int DuanWeiType;
        public int DuanWeiLevel;

        public int ToKfServerId;
    }

    [Serializable]
    public class CoupleArenaPkResultReq
    {
        public long GameId;
        public int winSide;

        public int ManRole1;
        public int ManZoneId1;
        public byte[] ManSelector1;
        public int WifeRole1;
        public int WifeZoneId1;
        public byte[] WifeSelector1;

        public int ManRole2;
        public int ManZoneId2;
        public byte[] ManSelector2;
        public int WifeRole2;
        public int WifeZoneId2;
        public byte[] WifeSelector2;
    }

    [Serializable]
    public class CoupleArenaPkResultItem
    {
        public int Result;
        public int GetJiFen;
        public int OldDuanWeiType;
        public int OldDuanWeiLevel;
        public int NewDuanWeiType;
        public int NewDuanWeiLevel;
    }

    [Serializable]
    public class CoupleArenaPkResultRsp
    {
        public CoupleArenaPkResultItem Couple1RetData = new CoupleArenaPkResultItem();
        public CoupleArenaPkResultItem Couple2RetData = new CoupleArenaPkResultItem();
    }

    [Serializable]
    public class CoupleArenaFuBenData
    {
        public long GameId;
        public int KfServerId;
        public DateTime StartTime;
        public List<KuaFuFuBenRoleData> RoleList;
        public int FuBenSeq;
    }

    /// <summary>
    /// 情侣竞技战斗结果
    /// </summary>
    public enum ECoupleArenaPkResult
    {
        /// <summary>
        /// 对方夫妻无一人进入，超时后，本次pk无效
        /// </summary>
        Invalid = 0,
        Win = 1,
        Fail = 2,
    }
}
