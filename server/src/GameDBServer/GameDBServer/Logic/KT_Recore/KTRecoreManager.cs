using GameDBServer.DB;
using GameDBServer.Server;
using MySQLDriverCS;
using Server.Protocol;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameDBServer.Logic.KT_Recore
{
    public class KTRecoreManager
    {
        /// <summary>
        /// Lấy ra top rank theo thứ tự
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="Input"></param>
        /// <param name="RoleID"></param>
        /// <param name="RankType"></param>
        /// <param name="LimitCount"></param>
        /// <param name="StartTime"></param>
        /// <param name="EndTime"></param>
        /// <returns></returns>
        public static List<RecoreRanking> GetRecoreByRank(DBManager dbMgr, int RankType, int LimitCount, string StartTime, string EndTime)
        {
            List<RecoreRanking> _TotalRank = new List<RecoreRanking>();

            MySQLConnection conn = null;

            try
            {
                conn = dbMgr.DBConns.PopDBConnection();

                string cmdText = string.Format("Select RoleID,RoleName,SUM(ValueRecore) as TotalValue where DateRecore >= '" + StartTime + "' and DateRecore<= '" + EndTime + "' and RecoryType = " + RankType + " GROUP by RoleID ORDER BY TotalValue ASC LIMIT " + LimitCount);

                MySQLCommand cmd = new MySQLCommand(cmdText, conn);
                MySQLDataReader reader = cmd.ExecuteReaderEx();

                int count = 0;

                while (reader.Read())
                {
                    RecoreRanking paiHangItemData = new RecoreRanking()
                    {
                        Index = count,
                        RoleID = Convert.ToInt32(reader["RoleID"].ToString()),
                        RoleName = DataHelper.Base64Decode(reader["RoleName"].ToString()),
                        RankType = RankType,
                        TotalValue = Convert.ToInt32(reader["TotalValue"].ToString()),
                    };

                    _TotalRank.Add(paiHangItemData);
                    count++;
                }

                GameDBManager.SystemServerSQLEvents.AddEvent(string.Format("+SQL: {0}", cmdText), EventLevels.Important);

                cmd.Dispose();
                cmd = null;
            }
            finally
            {
                if (null != conn)
                {
                    dbMgr.DBConns.PushDBConnection(conn);
                }
            }

            return _TotalRank;
        }

        /// <summary>
        /// Update đánh giấu giá trị trong 1 khoảng thời gian kèm theo key
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <param name="tcpOutPacket"></param>
        /// <returns></returns>
        public static TCPProcessCmdResults CMD_KT_UPDATEMARKVALUE(DBManager dbMgr, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
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
                string[] fields = cmdData.Split('#');
                if (fields.Length != 4)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}",
                        (TCPGameServerCmds)nID, fields.Length, cmdData));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                int RoleID = Convert.ToInt32(fields[0]);
                string TimerRanger = fields[1];

                int MarkValue = Convert.ToInt32(fields[2]);
                int MarkType = Convert.ToInt32(fields[3]);

                string OUTDATA = RoleID + ":";

                if (DBWriter.UpdateMarkData(dbMgr, RoleID, TimerRanger, MarkValue, MarkType))
                {
                    OUTDATA = OUTDATA + 1;
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, OUTDATA, nID);
                }
                else
                {
                    OUTDATA = OUTDATA + -1;
                }

                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, "", false);
            }

            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
            return TCPProcessCmdResults.RESULT_DATA;
        }

        public static TCPProcessCmdResults CMD_KT_GET_RECORE_BYTYPE(DBManager dbMgr, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
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
                string[] fields = cmdData.Split('|');
                if (fields.Length != 4)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}",
                        (TCPGameServerCmds)nID, fields.Length, cmdData));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                int RoleID = Convert.ToInt32(fields[0]);
                int RecoreType = Convert.ToInt32(fields[1]);
                string StartTime = fields[2];
                string EndTime = fields[3];

                int Value = DBQuery.GetSumRecoreValue(dbMgr, RoleID, RecoreType, StartTime, EndTime, GameDBManager.ZoneID);

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, RoleID + ":" + Value, nID);

                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, "", false);
            }

            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
            return TCPProcessCmdResults.RESULT_DATA;
        }

        public static TCPProcessCmdResults CMD_KT_ADD_RECORE_BYTYPE(DBManager dbMgr, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string cmdData = null;

            try
            {
                cmdData = new UTF8Encoding().GetString(data, 0, count);
            }
            catch (Exception)
            {
                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                return TCPProcessCmdResults.RESULT_DATA;
            }

            try
            {
                string[] fields = cmdData.Split('|');
                if (fields.Length != 4)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}",
                        (TCPGameServerCmds)nID, fields.Length, cmdData));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                int RoleID = Convert.ToInt32(fields[0]);
                string RoleName = DataHelper.Base64Encode(fields[1]);
                int RecoreType = Convert.ToInt32(fields[2]);
                int ValueRecore = Convert.ToInt32(fields[3]);

                string OUTDATA = RoleID + ":";

                if (DBWriter.AddRecore(dbMgr, RoleID, RoleName, ValueRecore, RecoreType))
                {
                    OUTDATA = OUTDATA + 1;
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, OUTDATA, nID);
                }
                else
                {
                    OUTDATA = OUTDATA + -1;
                }

                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, "", false);
            }

            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
            return TCPProcessCmdResults.RESULT_DATA;
        }

        /// <summary>
        /// Lấy ra gái trị MARK VALUE
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <param name="tcpOutPacket"></param>
        /// <returns></returns>
        public static TCPProcessCmdResults CMD_KT_GETMARKVALUE(DBManager dbMgr, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
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
                string[] fields = cmdData.Split('#');
                if (fields.Length != 3)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}",
                        (TCPGameServerCmds)nID, fields.Length, cmdData));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                int RoleID = Convert.ToInt32(fields[0]);
                string TimerRanger = fields[1];

                int MarkType = Convert.ToInt32(fields[2]);

                int MARKVALUE = DBQuery.GetTMark(dbMgr, RoleID, TimerRanger, MarkType);

                string OUTDATA = RoleID + ":" + MARKVALUE;

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, OUTDATA, nID);

                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, "", false);
            }

            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
            return TCPProcessCmdResults.RESULT_DATA;
        }

        /// <summary>
        /// GetRecoreRankList
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <param name="tcpOutPacket"></param>
        /// <returns></returns>
        public static TCPProcessCmdResults CMD_KT_GETRANK_RECORE_BYTYPE(DBManager dbMgr, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
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
                string[] fields = cmdData.Split('#');
                if (fields.Length != 4)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}",
                        (TCPGameServerCmds)nID, fields.Length, cmdData));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                int RankType = Convert.ToInt32(fields[0]);
                int LimtCount = Convert.ToInt32(fields[1]);
                string StartTime = fields[2];
                string EndTime = fields[3];

                List<RecoreRanking> _RankGet = KTRecoreManager.GetRecoreByRank(dbMgr, RankType, LimtCount, StartTime, EndTime);

                tcpOutPacket = DataHelper.ObjectToTCPOutPacket<List<RecoreRanking>>(_RankGet, pool, nID);

                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, "", false);
            }

            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
            return TCPProcessCmdResults.RESULT_DATA;
        }
    }
}