using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Server.Data;
using KF.Contract.Data;
using Tmsk.Contract;

namespace GameServer.Logic
{
    public class YongZheZhanChangSceneInfo
    {
        public int Id;
        public int MapCode;

        public int MinZhuanSheng = 1;
        public int MinLevel = 1;
        public int MaxZhuanSheng = 1;
        public int MaxLevel = 1;

        /// <summary>
        /// 活动时间区间列表,依次为:开始时间1,结束时间1,开始时间2,结束时间2...
        /// </summary>
        public List<TimeSpan> TimePoints = new List<TimeSpan>();
        // 每个时间相对于一天经过的秒数
        public List<double> SecondsOfDay = new List<double>();

        /// <summary>
        /// 准备进入时间
        /// </summary>
        public int WaitingEnterSecs;

        /// <summary>
        /// 准备时间
        /// </summary>
        public int PrepareSecs;

        /// <summary>
        /// 战斗时间
        /// </summary>
        public int FightingSecs;

        /// <summary>
        /// 清场时间
        /// </summary>
        public int ClearRolesSecs;

        /// <summary>
        /// 总时间(额外60秒作为副本删除时间)
        /// </summary>
        public int TotalSecs { get { return WaitingEnterSecs + PrepareSecs + FightingSecs + ClearRolesSecs + 120; } }

        /// <summary>
        /// 开始报名的提前时间(秒)
        /// </summary>
        public int SignUpStartSecs;

        /// <summary>
        /// 停止报名时间
        /// </summary>
        public int SignUpEndSecs;

        #region 奖励

        public long Exp;
        public int BandJinBi;
   

        #endregion 奖励
    }

    public class YongZheZhanChangBirthPoint
    {
        /// <summary>
        /// ID
        /// </summary>
        public int ID;

        /// <summary>
        /// X坐标
        /// </summary>
        public int PosX;

        /// <summary>
        /// Y坐标
        /// </summary>
        public int PosY;

        /// <summary>
        /// 随机半径（米）
        /// </summary>
        public int BirthRadius; 
    }

 

    // 勇者战场水晶采集物
    public class BattleCrystalMonsterItem
    {
        public int Id; // 流水号，据说MonsterID以后可能配成重复的
        public int MonsterID;
        public int GatherTime;
        public int BattleJiFen;
        public int PosX;
        public int PosY;
        public int FuHuoTime; //毫秒
        public int BuffGoodsID;
        public int BuffTime;
    }

    public class YongZheZhanChangRoleMiniData
    {
        public int RoleId;
        public int ZoneId;
        public string RoleName;
        public int BattleWitchSide;
    }

    /// <summary>
    /// 配置信息和运行时数据
    /// </summary>
    public class YongZheZhanChangData
    {
        /// <summary>
        /// 保证数据完整性,敏感数据操作需加锁
        /// </summary>
        public object Mutex = new object();

        #region 活动配置

        /// <summary>
        /// 复活和地图传送位置
        /// </summary>
        public Dictionary<int, YongZheZhanChangBirthPoint> MapBirthPointDict = new Dictionary<int, YongZheZhanChangBirthPoint>();

        /// <summary>
        /// 段位排行奖励配置
        /// </summary>
        public Dictionary<RangeKey, YongZheZhanChangSceneInfo> LevelRangeSceneIdDict = new Dictionary<RangeKey, YongZheZhanChangSceneInfo>(RangeKey.Comparer);

        /// <summary>
        /// 活动配置字典
        /// </summary>
        public Dictionary<int, YongZheZhanChangSceneInfo> SceneDataDict = new Dictionary<int, YongZheZhanChangSceneInfo>();

        #endregion 活动配置

        #region 奖励配置

        /// <summary>
        /// 采集得分信息
        /// </summary>
        public Dictionary<int, BattleCrystalMonsterItem> BattleCrystalMonsterDict = new Dictionary<int, BattleCrystalMonsterItem>();

        /// <summary>
        /// 本次跨服活动角色的跨服登录Token缓存集合
        /// </summary>
        public Dictionary<int, KuaFuServerLoginData> RoleIdKuaFuLoginDataDict = new Dictionary<int, KuaFuServerLoginData>();

        public Dictionary<int, KuaFuServerLoginData> NotifyRoleEnterDict = new Dictionary<int, KuaFuServerLoginData>();

        // 原服务器报名的角色到报名分组的映射
        public ConcurrentDictionary<int, int> RoleId2JoinGroup = new ConcurrentDictionary<int, int>();


        /// <summary>
        /// 攻击Boss得分配置,伤害百分比
        /// </summary>
        public double WarriorBattleBossAttackPercent = 0.001;

        /// <summary>
        /// 攻击Boss得分配置,得分
        /// </summary>
        public int WarriorBattleBossAttackScore = 20;

        /// <summary>
        /// 击杀Boss得分配置
        /// </summary>
        public int WarriorBattleBOssLastAttack = 20;

        /// <summary>
        /// 杀人得分(公式合并入连杀积分,这个配置无)
        /// </summary>
        public int WarriorBattlePk = 8;

        /// <summary>
        /// 给予奖励的最低积分要求
        /// </summary>
        public int WarriorBattleLowestJiFen = 5;

        /// <summary>
        /// Mu勇士战场被杀获得积分，积分值
        /// </summary>
        public int WarriorBattleDie = 5;

        /// <summary>
        /// Mu勇士战场连杀积分，格式：基础值,系数,最小值,最大值
        /// </summary>
        public int WarriorBattleUltraKillParam1 = 27;
        public int WarriorBattleUltraKillParam2 = 3;
        public int WarriorBattleUltraKillParam3 = 30;
        public int WarriorBattleUltraKillParam4 = 75;

        /// <summary>
        /// Mu勇士战场连杀积分，格式：基础值,系数,最小值,最大值
        /// </summary>
        public int WarriorBattleShutDownParam1 = -10;
        public int WarriorBattleShutDownParam2 = 5;
        public int WarriorBattleShutDownParam3 = 0;
        public int WarriorBattleShutDownParam4 = 100;

        /// <summary>
        /// 代表已报名状态的数子
        /// </summary>
        public int SighUpStateMagicNum = 100000000;

        //public string RoleParamsAwardsDefaultString = "0,0,0,0,0";
        public string RoleParamsAwardsDefaultString = "";

        /// <summary>
        /// NotifyCenerPrepareGame
        /// </summary>
        public bool PrepareGame;

        #endregion 奖励配置

        #region 运行时数据

        public Dictionary<int, YongZheZhanChangFuBenData> FuBenItemData = new Dictionary<int, YongZheZhanChangFuBenData>();

        #endregion 运行时数据
    }
}
