using GameServer.Server;
using Server.Protocol;
using Server.Tools;
using System;
using System.Collections.Generic;

namespace GameServer.Logic
{
    /// <summary>
    /// Quản lý ghi logs lại
    /// </summary>
    public class LogDBCmdManager
    {
        /// <summary>
        /// Khởi tạo 2000 hàng đợi
        /// </summary>
        private DBCmdPool _DBCmdPool = new DBCmdPool(2000);

        /// <summary>
        /// Khởi tạo sẵn 2000 Queue
        /// </summary>
        private Queue<DBCommand> _DBCmdQueue = new Queue<DBCommand>(2000);



        /// <summary>
        /// Ghi logs money
        /// </summary>
        /// <param name="RoleID"></param>
        /// <param name="AccountName"></param>
        /// <param name="RecoreType"> Loại thaot tác làm mất tiền</param>
        /// <param name="RecoreDesc"></param>
        /// <param name="Source"></param>
        /// <param name="Taget"></param>
        /// <param name="OptType"></param>
        /// <param name="ZoneID"></param>
        /// <param name="OtherPram_1"></param>
        /// <param name="OtherPram_2"></param>
        /// <param name="OtherPram_3"></param>
        /// <param name="OtherPram_4"></param>
        public void WriterLogs(int RoleID, string AccountName, string RecoreType, string RecoreDesc, string Source, string Taget, string OptType, int ZoneID, string OtherPram_1, string OtherPram_2, string OtherPram_3, string OtherPram_4, int ServerID)
        {
            String strLogInfo = String.Format("{0}:{1}:{2}:{3}:{4}:{5}:{6}:{7}:{8}:{9}:{10}:{11}", RoleID, AccountName, RecoreType, RecoreDesc, Source, Taget, OptType, ZoneID, OtherPram_1, OtherPram_2, OtherPram_3, OtherPram_4);

            AddDBCmd((int)TCPGameServerCmds.CMD_LOGDB_ADD_LOG, strLogInfo, null, ServerID);

        }



        /// <summary>
        /// Add DB CMD
        /// </summary>
        /// <param name="cmdID"></param>
        /// <param name="cmdText"></param>
        private void AddDBCmd(int cmdID, string cmdText, DBCommandEventHandler dbCommandEvent, int serverId)
        {
            DBCommand dbCmd = _DBCmdPool.Pop();
            if (null == dbCmd)
            {
                dbCmd = new DBCommand();
            }

            dbCmd.DBCommandID = cmdID;
            dbCmd.DBCommandText = cmdText;
            dbCmd.ServerId = serverId;
            if (null != dbCommandEvent)
            {
                dbCmd.DBCommandEvent += dbCommandEvent;
            }

            lock (_DBCmdQueue)
            {
                _DBCmdQueue.Enqueue(dbCmd);
            }
        }

        /// <summary>
        /// Đọc ra số lượng kết nối
        /// </summary>
        /// <returns></returns>
        public int GetDBCmdCount()
        {
            lock (_DBCmdQueue)
            {
                return _DBCmdQueue.Count;
            }
        }

        /// <summary>
        /// Do logs DB CMD
        /// </summary>
        /// <param name="tcpClientPool"></param>
        /// <param name="pool"></param>
        /// <param name="dbCmd"></param>
        /// <returns></returns>
        private TCPProcessCmdResults DoDBCmd(TCPClientPool tcpClientPool, TCPOutPacketPool pool, DBCommand dbCmd, out byte[] bytesData)
        {
            bytesData = Global.SendAndRecvData(dbCmd.DBCommandID, dbCmd.DBCommandText, dbCmd.ServerId, 1);
            if (null == bytesData || bytesData.Length <= 0)
            {
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            return TCPProcessCmdResults.RESULT_OK;
        }

        public void ExecuteDBCmd(TCPClientPool tcpClientPool, TCPOutPacketPool pool)
        {
            lock (_DBCmdQueue)
            {
                if (_DBCmdQueue.Count <= 0) return;
            }

            List<DBCommand> dbCmdList = new List<DBCommand>();
            lock (_DBCmdQueue)
            {
                while (_DBCmdQueue.Count > 0)
                {
                    dbCmdList.Add(_DBCmdQueue.Dequeue());
                }
            }

            byte[] bytesData = null;
            TCPProcessCmdResults result;

            for (int i = 0; i < dbCmdList.Count; i++)
            {
                result = DoDBCmd(tcpClientPool, pool, dbCmdList[i], out bytesData);
                if (result == TCPProcessCmdResults.RESULT_FAILED)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Có lỗi khi kết lối với LOGSDB, CMD={0}", (TCPGameServerCmds)dbCmdList[i].DBCommandID));
                }

                _DBCmdPool.Push(dbCmdList[i]);
            }
        }
    }
}