using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameServer.Logic;
using Server.Data;
using Server.Tools;

namespace GameServer.Server.CmdProcesser
{
    /// <summary>
    /// 翅膀进阶指令
    /// </summary>
    public class GetWanMoTaDetailCmdProcessor : ICmdProcessor
    {
        private static GetWanMoTaDetailCmdProcessor instance = new GetWanMoTaDetailCmdProcessor();
        /*
        /// <summary>
        /// 挑战成功的万魔塔最高层编号
        /// </summary>
        private int _WanMoTaTopLayer = 0;

        /// <summary>
        /// 挑战成功的万魔塔最高层编号
        /// </summary>
        public int WanMoTaTopLayer
        {
            get { lock (this) return _WanMoTaTopLayer; }
            set { lock (this) _WanMoTaTopLayer = value; }
        }*/

        private GetWanMoTaDetailCmdProcessor() 
        {
            TCPCmdDispatcher.getInstance().registerProcessor((int)TCPGameServerCmds.CMD_SPR_GET_WANMOTA_DETAIL, 1, this);
        }

        public static GetWanMoTaDetailCmdProcessor getInstance()
        {
            return instance;
        }

        public bool processCmd(Logic.KPlayer client, string[] cmdParams)
        {
            return false;
        }
    }
}
