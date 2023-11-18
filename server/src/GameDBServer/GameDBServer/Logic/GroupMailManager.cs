using GameDBServer.DB;
using GameDBServer.Server;
using Server.Data;
using Server.Protocol;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameDBServer.Logic
{
    internal class GroupMailManager
    {


        private static Dictionary<int, GroupMailData> GroupMailDataDict = new Dictionary<int, GroupMailData>();

        private static List<GroupMailData> GetGroupMailList(int beginID)
        {
            List<GroupMailData> GroupMailList = null;

            foreach (var item in GroupMailDataDict)
            {
                if (item.Value.GMailID <= beginID)
                {
                    continue;
                }

                if (null == GroupMailList)
                {
                    GroupMailList = new List<GroupMailData>();
                }

                GroupMailList.Add(item.Value);
            }

            return GroupMailList;
        }

        public static TCPProcessCmdResults RequestNewGroupMailList(DBManager dbMgr, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;

            string cmdData = null;

            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit, CMD={0}", (TCPGameServerCmds)nID));

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                return TCPProcessCmdResults.RESULT_DATA;
            }

            try
            {
                string[] fields = cmdData.Split(':');
                if (fields.Length != 1)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}",
                        (TCPGameServerCmds)nID, fields.Length, cmdData));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                int beginID = Convert.ToInt32(fields[0]);

                List<GroupMailData> GroupMailList = GetGroupMailList(beginID);
                byte[] retBytes = DataHelper.ObjectToBytes<List<GroupMailData>>(GroupMailList);
                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, retBytes, 0, retBytes.Length, nID);
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, "", false);
            }
            return TCPProcessCmdResults.RESULT_DATA;
        }

        /// <summary>
        /// Sửa lại mail ID
        /// </summary>
        public static TCPProcessCmdResults ModifyGMailRecord(DBManager dbMgr, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;

            string cmdData = null;

            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit, CMD={0}", (TCPGameServerCmds)nID));

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                return TCPProcessCmdResults.RESULT_DATA;
            }

            try
            {
                string[] fields = cmdData.Split(':');
                if (fields.Length != 3)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}",
                        (TCPGameServerCmds)nID, fields.Length, cmdData));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                int roleID = Convert.ToInt32(fields[0]);
                int gmailID = Convert.ToInt32(fields[1]);
                int mailID = Convert.ToInt32(fields[2]);

                DBRoleInfo dbRoleInfo = dbMgr.GetDBRoleInfo(roleID);
                if (null == dbRoleInfo)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("发起请求的角色不存在，CMD={0}, RoleID={1}",
                        (TCPGameServerCmds)nID, roleID));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                int result = DBWriter.ModifyGMailRecord(dbMgr, roleID, gmailID, mailID);

                if (result > 0)
                {
                    lock (dbRoleInfo)
                    {
                        if (null == dbRoleInfo.GroupMailRecordList)
                        {
                            dbRoleInfo.GroupMailRecordList = new List<int>();
                        }

                        if (dbRoleInfo.GroupMailRecordList.IndexOf(gmailID) < 0)
                        {
                            dbRoleInfo.GroupMailRecordList.Add(gmailID);
                        }
                    }
                }

                string strcmd = string.Format("{0}", result);
                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, "", false);
            }
            return TCPProcessCmdResults.RESULT_DATA;
        }
    }
}