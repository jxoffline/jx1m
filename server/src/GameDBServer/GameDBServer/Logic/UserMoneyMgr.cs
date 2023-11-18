using GameDBServer.DB;
using GameDBServer.Server;
using Server.Protocol;
using Server.Tools;
using System;
using System.Text;

namespace GameDBServer.Logic
{
    /// <summary>
    /// Ghi lại logs khi nạp thẻ
    /// </summary>
    public class UserMoneyMgr
    {
        #region RechageLogs
        public static TCPProcessCmdResults CMT_KT_LOG_RECHAGE(DBManager dbMgr, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string cmdData = null;

            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Wrong socket data CMD={0}", (TCPGameServerCmds)nID));

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

                string UserID = fields[0];

                int Money = Int32.Parse(fields[1]);

                int RoleID = Int32.Parse(fields[2]);

                string RETURN = "";

                if (DBWriter.UpdateRechageData(dbMgr, UserID, Money) && DBWriter.UpdatetInputMoney(dbMgr, UserID, Money, GameDBManager.ZoneID, RoleID))
                {
                    RETURN = "0:1";
                }
                else
                {
                    RETURN = "-1:-1";
                }

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, RETURN, nID);

                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {

                DataHelper.WriteFormatExceptionLog(ex, "", false);

            }

            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
            return TCPProcessCmdResults.RESULT_DATA;
        }

        #endregion RechageLogs
    }
}