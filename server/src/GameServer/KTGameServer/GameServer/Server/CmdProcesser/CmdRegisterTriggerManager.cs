using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameServer.Logic;

namespace GameServer.Server.CmdProcesser
{
    /// <summary>
    /// 通用的注册指令管理器
    /// </summary>
    public class CmdRegisterTriggerManager : IManager
    {
        private static CmdRegisterTriggerManager instance = new CmdRegisterTriggerManager();

        private CmdRegisterTriggerManager() { }

        public static CmdRegisterTriggerManager getInstance()
        {
            return instance;
        }

        /// <summary>
        /// 指令处理队列
        /// </summary>
        private List<ICmdProcessor> CmdProcessorList = new List<ICmdProcessor>();

        /// <summary>
        /// 开始出发指令处理流程
        /// </summary>
        /// <param name="icp"></param>
        private void TriggerProcessor(ICmdProcessor icp)
        {
            ;//不做任何处理
        }

        public bool initialize()
        {
          
         
            TriggerProcessor(GetSweepWanMoTaAwardCmdProcessor.getInstance());
            TriggerProcessor(GetWanMoTaDetailCmdProcessor.getInstance());
            TriggerProcessor(SweepWanMoTaCmdProcessor.getInstance());

            return true;
        }

        public bool startup()
        {
            return true;
        }

        public bool showdown()
        {
            return true;
        }

        public bool destroy()
        {
            return true;
        }
    }
}
