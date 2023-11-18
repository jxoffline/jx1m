using GameDBServer.DB;
using GameDBServer.Server;
using Server.Protocol;
using Server.Tools;
using System;
using System.Text;

namespace GameDBServer.Logic
{
    internal class RechargeRepayActiveMgr
    {
        private static bool GetCmdDataField(int nID, byte[] data, int count, out string[] fields)
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
            return true;
        }

        public static TCPProcessCmdResults ProcessQueryActiveInfo(DBManager dbMgr, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string[] fields = null;
            string strcmd;
            string extData = "";
            try
            {
                if (!GetCmdDataField(nID, data, count, out fields))
                {
                    return TCPProcessCmdResults.RESULT_FAILED;
                }

                int roleID = Convert.ToInt32(fields[0]);
                int activeid = Global.SafeConvertToInt32(fields[1]);
                DBRoleInfo roleInfo = dbMgr.GetDBRoleInfo(roleID);

                if (null == roleInfo)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("发起请求的角色不存在，CMD={0}, RoleID={1}",
                        (TCPGameServerCmds)nID, roleID));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }


                switch (activeid)
                {
                    // Lấy ra tổng nạp ngày của người chơi đó
                    case (int)(ActivityTypes.MeiRiChongZhiHaoLi):
                        {
                            DateTime startTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
                            DateTime endTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 23, 59, 59);

                            int money = DBQuery.GetUserInputMoney(dbMgr, roleInfo.UserID, roleID, roleInfo.ZoneID, startTime.ToString("yyyy-MM-dd HH:mm:ss"), endTime.ToString("yyyy-MM-dd HH:mm:ss"));

                            extData = "" + money;
                        }
                        break;

                    case (int)(ActivityTypes.TotalCharge):
                        int realmoney = 0;
                        int usermoney = 0;
                        DBQuery.QueryTotalMoneyRechage(dbMgr, roleInfo.UserID, roleID, roleInfo.ZoneID, out usermoney, out realmoney);
                        realmoney = Global.TransMoneyToYuanBao(realmoney);
                        extData = "" + realmoney;
                        break;

                    case (int)(ActivityTypes.TotalConsume):
                        {

                            // Tạm thời lấy ra toàn bộ thời gian
                            extData = DBQuery.GetUserUsedMoney(dbMgr, roleID).ToString();
                        }
                        break;
                }

                lock (roleInfo)
                {

                    strcmd = string.Format("{0}:{1}", roleID, extData);
                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                }
                return TCPProcessCmdResults.RESULT_DATA;
            }
            catch (System.Exception ex)
            {
                LogManager.WriteException(ex.ToString());
            }

            strcmd = string.Format("{0}:{1}:{2}", 0, "", "");
            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
            return TCPProcessCmdResults.RESULT_DATA;
        }

        public static TCPProcessCmdResults ProcessGetActiveAwards(DBManager dbMgr, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
        {
            tcpOutPacket = null;
            string[] fields = null;

            if (!GetCmdDataField(nID, data, count, out fields))
            {
                return TCPProcessCmdResults.RESULT_FAILED;
            }

            int roleID = Convert.ToInt32(fields[0]);
            int activeid = Global.SafeConvertToInt32(fields[1]);
            int hasgettimes = Global.SafeConvertToInt32(fields[2]);
            long hasgettimesLong = Global.SafeConvertToInt64(fields[2]);
            DBRoleInfo roleInfo = dbMgr.GetDBRoleInfo(roleID);

            if (null == roleInfo)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("发起请求的角色不存在，CMD={0}, RoleID={1}",
                    (TCPGameServerCmds)nID, roleID));

                tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                return TCPProcessCmdResults.RESULT_DATA;
            }

            string huoDongKeyStr = "not_limit";
            string extData = "";
            string strcmd = "";
            switch (activeid)
            {
                case (int)ActivityTypes.MeiRiChongZhiHaoLi:
                    {
                        DateTime startTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
                        DateTime endTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 23, 59, 59);
                        huoDongKeyStr = Global.GetHuoDongKeyString(startTime.ToString("yyyy-MM-dd HH:mm:ss"), endTime.ToString("yyyy-MM-dd HH:mm:ss"));
                    }
                    break;

                case (int)(ActivityTypes.TotalCharge):
                case (int)(ActivityTypes.TotalConsume):
                    {
                        lock (roleInfo)
                        {
                            //int ret = DBWriter.UpdateHongDongAwardRecordForUser(dbMgr, roleInfo.UserID, activeid, huoDongKeyStr, hasgettimesLong, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                            //if (ret < 0)
                            //    ret = DBWriter.AddHongDongAwardRecordForUser(dbMgr, roleInfo.UserID, activeid, huoDongKeyStr, hasgettimesLong, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                            //if (ret < 0)
                            //{
                            //    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                            //    return TCPProcessCmdResults.RESULT_FAILED;
                            //}
                        }
                        strcmd = string.Format("{0}:{1}:{2}", 1, activeid, hasgettimesLong);
                        tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
                        return TCPProcessCmdResults.RESULT_DATA;
                    }
            }

            lock (roleInfo)
            {
                //int ret = DBWriter.UpdateHongDongAwardRecordForUser(dbMgr, roleInfo.UserID, activeid, huoDongKeyStr, hasgettimes, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                //if (ret < 0)
                //    ret = DBWriter.AddHongDongAwardRecordForUser(dbMgr, roleInfo.UserID, activeid, huoDongKeyStr, hasgettimes, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                //if (ret < 0)
                //{
                //    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                //    return TCPProcessCmdResults.RESULT_FAILED;
                //}
            }
            strcmd = string.Format("{0}:{1}:{2}", 1, activeid, hasgettimes);
            tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, strcmd, nID);
            return TCPProcessCmdResults.RESULT_DATA;
        }
    }
}