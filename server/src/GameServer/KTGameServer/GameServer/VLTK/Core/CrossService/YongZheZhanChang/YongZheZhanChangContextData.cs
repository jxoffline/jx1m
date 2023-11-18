using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameServer.Server;
using System.Windows;

namespace GameServer.Logic
{
    /// <summary>
    /// 幻影寺院圣杯上下文对象
    /// </summary>
    public class YongZheZhanChangClientContextData
    {
        public int RoleId;
        public int ServerId;
        public int BattleWhichSide;

        /// <summary>
        /// 总得分,包括采集和击杀获得
        /// </summary>
        public int TotalScore;

        /// <summary>
        /// 连续击杀数
        /// </summary>
        public int KillNum;

        /// <summary>
        /// 伤害Boss未计积分的部分
        /// </summary>
        public double InjureBossDelta;
    }

    /// <summary>
    /// 勇者战场副本上下文对象
    /// </summary>
    public class YongZheZhanChangContextData
    {
        public int TotalSignUpCount;
        public int SuccessRoleCount;
        public int FaildRoleCount;
        public int ScoreFromCaiJi;
        public int ScoreFromKill;
        public int ScoreFromContinueKill;
        public int ScoreFromBreakContinueKill;
        public int ScoreFromBoss;
    }
}
