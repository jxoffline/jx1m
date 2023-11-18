using GameDBServer.Server;
using Server.Tools;
using System;
using System.Text;

namespace GameDBServer.Tools
{
    public class CheckHelper
    {
        /// <summary>
        /// TCPCmdHandler消息验证
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <param name="fields"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static bool CheckTCPCmdFields(int nID, byte[] data, int count, out string[] fields, int length)
        {
            string cmdData = null;
            fields = null;

            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit, CMD={0}", (TCPGameServerCmds)nID));
                return false;
            }

            fields = cmdData.Split(':');
            if (fields.Length != length)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit CMD={0},  Recv={1}, CmdData={2}",
                    (TCPGameServerCmds)nID, fields.Length, cmdData));
                return false;
            }

            return true;
        }

        public static bool CheckTCPCmdFields2(int nID, byte[] data, int count, out string[] fields, int length, char span = '|')
        {
            string cmdData = null;
            fields = null;

            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit, CMD={0}", (TCPGameServerCmds)nID));
                return false;
            }

            fields = cmdData.Split(span);
            if (fields.Length != length)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit CMD={0},  Recv={1}, CmdData={2}",
                    (TCPGameServerCmds)nID, fields.Length, cmdData));
                return false;
            }

            return true;
        }
    }
}