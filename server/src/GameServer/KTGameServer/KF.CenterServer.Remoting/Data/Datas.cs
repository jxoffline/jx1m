using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KF.Contract.Data;
using Tmsk.Contract;

namespace KF.Remoting.Data
{
    public class GameTypeStaticsData
    {
        public int ServerAlived;
        public int FuBenAlived;
        public int SingUpRoleCount;
        public int StartGameRoleCount;
    }

    public class ServerLoadContext
    {
        public int AlivedServerCount;
        public int AlivedGameFuBenCount;

        public int ServerLoadAvg;
        public int RealServerLoadAvg;

        public int SignUpRoleCount;
        public int StartGameRoleCount;

        public bool AssginGameFuBenComplete;

        /// <summary>
        /// 当前活跃的服务器队列,仅单线程使用
        /// </summary>
        public LinkedList<KuaFuServerGameConfig> IdelActiveServerQueue = new LinkedList<KuaFuServerGameConfig>();
        
        public void CalcServerLoadAvg()
        {
            if (AlivedServerCount > 0 && AlivedGameFuBenCount > 0)
            {
                RealServerLoadAvg = AlivedGameFuBenCount / AlivedServerCount;
            }
            else
            {
                RealServerLoadAvg = 0;
            }

            ServerLoadAvg = RealServerLoadAvg + 5;
        }
    }

    public class GameFuBenStateDbItem
    {
        public int GameId;
        public int State;
        public string CreateTime;
        public string StartTime;
        public string EndTime;
    }

    public static class AsyncTypes
    {
        /// <summary>
        /// t_server_info
        /// </summary>
        public const int ServerListAge = 1;
        /// <summary>
        /// t_game_config [已废弃]
        /// </summary>
        public const int GameConfigAge = 2;
        /// <summary>
        /// t_server_game_config [已废弃]
        /// </summary>
        public const int ServerGameConfigAge = 3;
        /// <summary>
        /// 天梯排行
        /// </summary>
        public const int TianTiPaiHangModifyOffsetDay = 4;
        /// <summary>
        /// 夫妻竞技场当前第几周排行
        /// </summary>
        public const int CoupleAreanCurrPaiHangWeek = 5;
        /// <summary>
        /// 夫妻竞技场当前第几周排行
        /// </summary>
        public const int CoupleWishCurrPaiHangWeek = 6;
        /// <summary>
        /// 争霸赛数据月份记录
        /// 例如记录 201102则表示为2011年1月份的天体排行榜，也就是为2011年2月份的众神争霸
        /// </summary>
        public const int ZhengBaCurrMonth = 30;
    }

    public class GameLogItem
    {
        public int GameType;
        public int ServerCount;
        public int FubenCount;
        public int SignUpCount;
        public int EnterCount;
    }
}
