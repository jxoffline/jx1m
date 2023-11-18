using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameServer.Server;
using System.Windows;
using Tmsk.Contract;
using Server.Data;
using KF.Contract.Data;

namespace GameServer.Logic
{
    /// <summary>
    /// 勇者战场场景对象
    /// </summary>
    public class YongZheZhanChangScene
    {
        public int m_nMapCode = 0;

        public int FuBenSeqId = 0;

        public int CopyMapId = 0;

        /// <summary>
        /// 活动起始时间点
        /// </summary>
        public long StartTimeTicks = 0;

        /// <summary>
        /// 血色城堡场景开始时间
        /// </summary>
        public long m_lPrepareTime = 0;

        /// <summary>
        /// 血色城堡场景战斗开始时间
        /// </summary>
        public long m_lBeginTime = 0;

        /// <summary>
        /// 血色城堡场景战斗结束时间
        /// </summary>
        public long m_lEndTime = 0;

        /// <summary>
        /// 立场时间
        /// </summary>
        public long m_lLeaveTime = 0;

        /// <summary>
        /// 场景状态
        /// </summary>
        public GameSceneStatuses m_eStatus = GameSceneStatuses.STATUS_NULL;

        /// 玩家人数
        /// </summary>
        public int m_nPlarerCount = 0;

        /// <summary>
        /// 获胜方
        /// </summary>
        public int SuccessSide = 0;

        /// <summary>
        /// 结束标记
        /// </summary>
        public bool m_bEndFlag = false;

        /// <summary>
        /// 跨服
        /// </summary>
        public int GameId;



        /// <summary>
        /// 场景配置信息
        /// </summary>
        public YongZheZhanChangSceneInfo SceneInfo;

        /// <summary>
        /// 战斗统计信息,评估产品设计水平
        /// </summary>
        public YongZheZhanChangStatisticalData GameStatisticalData = new YongZheZhanChangStatisticalData();

        /// <summary>
        /// 阵营得分信息
        /// </summary>
        public YongZheZhanChangScoreData ScoreData = new YongZheZhanChangScoreData();

        /// <summary>
        /// 角色得分信息集合
        /// </summary>
        public Dictionary<int, YongZheZhanChangClientContextData> ClientContextDataDict = new Dictionary<int, YongZheZhanChangClientContextData>();

        /// <summary>
        /// 时间状态信息
        /// </summary>
        public GameSceneStateTimeData StateTimeData = new GameSceneStateTimeData();

        /// <summary>
        /// 怪物创建队列
        /// </summary>
        public SortedList<long, List<object>> CreateMonsterQueue = new SortedList<long, List<object>>();

        public int MapGridWidth = 100;

        public int MapGridHeight = 100;

        public void CleanAllInfo()
        {
            m_nMapCode = 0;
            m_lPrepareTime = 0;
            m_lBeginTime = 0;
            m_lEndTime = 0;
            m_eStatus = GameSceneStatuses.STATUS_NULL;
            m_nPlarerCount = 0;
            m_bEndFlag = false;
        }

    }
}
