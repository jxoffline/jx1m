using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameServer.Logic;
using Server.Tools;
using Server.Data;
using GameServer.Logic.JingJiChang;

namespace GameServer.Server.CmdProcesser
{
    /// <summary>
    /// 竞技场排行榜
    /// </summary>
    public class JingJiRankingListCmdProcessor : ICmdProcessor
    {
        private static JingJiRankingListCmdProcessor instance = new JingJiRankingListCmdProcessor();

        private JingJiRankingListCmdProcessor() { }

        public static JingJiRankingListCmdProcessor getInstance()
        {
            return instance;
        }

        public void processCmd(Logic.KPlayer client, string[] cmdParams)
        {
            //战斗时不允许请求
            if (!JingJiChangManager.getInstance().checkAction(client))
            {
                return;
            }

            //死亡时不允许请求
            if (client.m_CurrentLife <= 0 || client.CurrentAction == (int)GActions.Death)
            {
                return;
            }

            int pageIndex = Convert.ToInt32(cmdParams[1]);

            List<PlayerJingJiRankingData> rankingDataList = Global.sendToDB<List<PlayerJingJiRankingData>>((int)TCPGameServerCmds.CMD_DB_JINGJICHANG_GET_RANKINGLIST_DATA, DataHelper.ObjectToBytes<int>(pageIndex));

            client.sendCmd<List<PlayerJingJiRankingData>>((int)TCPGameServerCmds.CMD_SPR_JINGJI_RANKINGLIST, rankingDataList);
        }
    }
}
