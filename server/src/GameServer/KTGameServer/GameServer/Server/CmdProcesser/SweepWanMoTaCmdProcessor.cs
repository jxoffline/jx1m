using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameServer.Logic;

namespace GameServer.Server.CmdProcesser
{
    /// <summary>
    /// 翅膀进阶指令
    /// </summary>
    public class SweepWanMoTaCmdProcessor : ICmdProcessor
    {
        private static SweepWanMoTaCmdProcessor instance = new SweepWanMoTaCmdProcessor();

        private SweepWanMoTaCmdProcessor() 
        {
            TCPCmdDispatcher.getInstance().registerProcessor((int)TCPGameServerCmds.CMD_SPR_SWEEP_WANMOTA, 2, this);
        }

        public static SweepWanMoTaCmdProcessor getInstance()
        {
            return instance;
        }

        public bool processCmd(Logic.KPlayer client, string[] cmdParams)
        {
            return false;
        }
    }
}
