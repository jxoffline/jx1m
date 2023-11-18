using GameDBServer.Logic;
using Server.Tools;
using System;
using System.Collections.Generic;

namespace GameDBServer.Server
{
    public class TCPCmdDispatcher
    {
        private static readonly TCPCmdDispatcher instance = new TCPCmdDispatcher();


        private Dictionary<int, ICmdProcessor> cmdProcesserMapping = new Dictionary<int, ICmdProcessor>();

        private TCPCmdDispatcher()
        { }

        public static TCPCmdDispatcher getInstance()
        {
            return instance;
        }

        public void initialize()
        {
        }

        public void registerProcessor(int cmdId, /*short paramNum,*/ ICmdProcessor processor)
        {
            //cmdParamNumMapping.Add(cmdId, paramNum);
            cmdProcesserMapping.Add(cmdId, processor);
        }

        public TCPProcessCmdResults dispathProcessor(GameServerClient client, int nID, byte[] data, int count)
        {


            try
            {

                ICmdProcessor cmdProcessor = null;
                if (!cmdProcesserMapping.TryGetValue(nID, out cmdProcessor))
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Unhandled packet, CMD={0}, Client={1}",
                        (TCPGameServerCmds)nID, Global.GetSocketRemoteEndPoint(client.CurrentSocket)));

                    client.sendCmd((int)TCPGameServerCmds.CMD_DB_ERR_RETURN, "0");
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                cmdProcessor.processCmd(client, nID, data, count);

                return TCPProcessCmdResults.RESULT_OK;
            }
            catch (Exception ex)
            {

                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(client.CurrentSocket), false);
            }

            client.sendCmd((int)TCPGameServerCmds.CMD_DB_ERR_RETURN, "0");
            return TCPProcessCmdResults.RESULT_DATA;
        }
    }
}