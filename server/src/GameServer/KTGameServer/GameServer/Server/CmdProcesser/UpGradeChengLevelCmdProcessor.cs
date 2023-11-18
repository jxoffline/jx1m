using System;
using GameServer.Logic;
using Server.Data;
using System.Collections.Generic;

namespace GameServer.Server.CmdProcesser
{
    /// <summary>
    /// 炼制系统
    /// </summary>
    public class UpGradeChengLevelCmdProcessor : ICmdProcessor
    {
        #region 通用接口

        private TCPGameServerCmds CmdID = TCPGameServerCmds.CMD_SPR_UPGRADE_CHENGJIU;

        public UpGradeChengLevelCmdProcessor(TCPGameServerCmds cmdID)
        {
            CmdID = cmdID;
        }

        public static UpGradeChengLevelCmdProcessor getInstance(TCPGameServerCmds cmdID)
        {
            return new UpGradeChengLevelCmdProcessor(cmdID);
        }

        /// <summary>
        /// 炼制系统命令处理
        /// </summary>
        public bool processCmd(KPlayer client, string[] cmdParams)
        {
            int nID = (int)CmdID;

            if (CmdID == TCPGameServerCmds.CMD_SPR_UPGRADE_CHENGJIU)
            {
                int nRoleID = Global.SafeConvertToInt32(cmdParams[0]);          // 角色ID
                int nChengJiuLevel = Global.SafeConvertToInt32(cmdParams[1]);   // 角色成就等级

                return true;
            }

            return false;
        }

        #endregion 通用接口
    }
}
