using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KF.Contract.Interface;
using ProtoBuf;

namespace KF.Contract.Data
{
    [Serializable]
    public class YongZheZhanChangGameData : IGameData
    {
        public int ZhanDouLi;
        public string RoleName;

        public object Clone()
        {
            return new YongZheZhanChangGameData()
                {
                    ZhanDouLi = ZhanDouLi,
                };
        }
    }

    /// <summary>
    /// 勇者战场角色信息
    /// </summary>
    [Serializable]
    public class YongZheZhanChangStatisticalData
    {
        /// <summary>
        /// 场次ID
        /// </summary>
        public int GameId;

        //报名统计	服务器统计每次战场的报名数量
        //胜利统计	服务器统计每次战场结束后获得胜利奖励的角色数量
        //失败统计	服务器统计每次战场结束后获得失败奖励的角色数量
        //连杀统计	服务器统计每次战场中连杀获取的积分占比
        //终结统计	服务器统计每次战场中终结获取的积分占比
        //采矿统计	服务器统计每次战场中采矿获取的积分占比
        //BOSS统计	服务器统计每次战场中BOSS获取的积分占比
        //PK统计	服务器统计每次战场中击杀敌对玩家获取的积分占比
        public int AllRoleCount;
        public int WinRoleCount;
        public int LoseRoleCount;
        public int LianShaScore;
        public int ZhongJieScore;
        public int CaiJiScore;
        public int BossScore;
        public int KillScore;
    }

    /// <summary>
    /// 王者战场角色信息
    /// </summary>
    [Serializable]
    public class KingOfBattleStatisticalData
    {
        /// <summary>
        /// 场次ID
        /// </summary>
        public int GameId;

        //报名统计	服务器统计每次战场的报名数量
        //胜利统计	服务器统计每次战场结束后获得胜利奖励的角色数量
        //失败统计	服务器统计每次战场结束后获得失败奖励的角色数量
        //连杀统计	服务器统计每次战场中连杀获取的积分占比
        //终结统计	服务器统计每次战场中终结获取的积分占比
        //采矿统计	服务器统计每次战场中采矿获取的积分占比
        //BOSS统计	服务器统计每次战场中BOSS获取的积分占比
        //PK统计	服务器统计每次战场中击杀敌对玩家获取的积分占比
        public int AllRoleCount;
        public int WinRoleCount;
        public int LoseRoleCount;
        public int LianShaScore;
        public int ZhongJieScore;
        public int CaiJiScore;
        public int BossScore;
        public int KillScore;
    }

    /// <summary>
    /// 跨服Boss每场信息
    /// </summary>
    [Serializable]
    public class KuaFuBossStatisticalData
    {
        /// <summary>
        /// 场次ID
        /// </summary>
        public int GameId;

        /// <summary>
        /// 怪物死亡统计
        /// </summary>
        public List<int> MonsterDieTimeList = new List<int>();
    }
}
