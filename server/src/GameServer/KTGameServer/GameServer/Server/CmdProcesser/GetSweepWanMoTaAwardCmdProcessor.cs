using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameServer.Logic;
using Server.Data;

namespace GameServer.Server.CmdProcesser
{
    /// <summary>
    /// 领取扫荡奖励指令
    /// </summary>
    public class GetSweepWanMoTaAwardCmdProcessor : ICmdProcessor
    {
        private static GetSweepWanMoTaAwardCmdProcessor instance = new GetSweepWanMoTaAwardCmdProcessor();

        private GetSweepWanMoTaAwardCmdProcessor() 
        {
            TCPCmdDispatcher.getInstance().registerProcessor((int)TCPGameServerCmds.CMD_SPR_GET_SWEEP_REWARD, 1, this);
        }

        public static GetSweepWanMoTaAwardCmdProcessor getInstance()
        {
            return instance;
        }        

        /// <summary>
        /// 给扫荡奖励
        /// </summary>
        private int GiveSweepReward(KPlayer client)
        {
            return 0;
        }

        public bool processCmd(KPlayer client, string[] cmdParams)
        {
            return false;
        }
    }
}
