using GameDBServer.Data;
using GameDBServer.Logic;
using MySQLDriverCS;
using Server.Data;
using Server.Tools;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace GameDBServer.DB
{
    /// <summary>
    /// Lớp xử lý truy vấn tới csdl
    /// </summary>
    public class DBQuery
    {
        /// <summary>
        /// Lấy ra config của trò chơi trong DB như rate nạp thẻ,rate exp vv
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <returns></returns>
        public static Dictionary<string, string> QueryGameConfigDict(DBManager dbMgr)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            MySQLConnection conn = null;

            try
            {
                conn = dbMgr.DBConns.PopDBConnection();
                string cmdText = "SELECT paramname, paramvalue FROM t_config";
                MySQLCommand cmd = new MySQLCommand(cmdText, conn);
                MySQLDataReader reader = cmd.ExecuteReaderEx();

                while (reader.Read())
                {
                    string paramName = reader["paramname"].ToString();
                    string paramVal = reader["paramvalue"].ToString();
                    dict[paramName] = paramVal;
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

            return dict;
        }

        /// <summary>
        /// Truy vấn tiền của người chơi thông qua userID
        /// </summary>
        /// <param name="sex"></param>
        /// <param name="roleName"></param>
        public static void QueryUserMoneyByUserID(DBManager dbMgr, string userID, out int userMoney, out int realMoney)
        {
            userMoney = 0;
            realMoney = 0;

            MySQLConnection conn = null;

            try
            {
                conn = dbMgr.DBConns.PopDBConnection();

                string cmdText = string.Format("SELECT money, realmoney FROM t_money WHERE userid='{0}'", userID);
                MySQLCommand cmd = new MySQLCommand(cmdText, conn);
                MySQLDataReader reader = cmd.ExecuteReaderEx();

                if (reader.Read())
                {
                    userMoney = Convert.ToInt32(reader["money"].ToString());
                    realMoney = Convert.ToInt32(reader["realmoney"].ToString());
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
        }

        /// <summary>
        /// Truy vấn số tiền nạp trong ngày của người chơi
        /// </summary>
        /// <param name="sex"></param>
        /// <param name="roleName"></param>
        public static void QueryTodayUserMoneyByUserID(DBManager dbMgr, string userID, int RoleID, int zoneID, out int userMoney, out int realMoney)
        {
            userMoney = 0;
            realMoney = 0;

            DateTime now = DateTime.Now;
            string todayStart = string.Format("{0:0000}-{1:00}-{2:00} 00:00:00", now.Year, now.Month, now.Day);
            string todayEnd = string.Format("{0:0000}-{1:00}-{2:00} 23:59:59", now.Year, now.Month, now.Day);

            MySQLConnection conn = null;

            try
            {
                conn = dbMgr.DBConns.PopDBConnection();

                string cmdText = string.Format("SELECT SUM(amount) AS totalmoney FROM t_inputlog WHERE u='{0}' AND zoneID={1} AND role_action = " + RoleID + " AND inputtime>='{2}' AND inputtime<='{3}'", userID, zoneID, todayStart, todayEnd);
                MySQLCommand cmd = new MySQLCommand(cmdText, conn);
                MySQLDataReader reader = cmd.ExecuteReaderEx();

                if (reader.Read())
                {
                    try
                    {
                        userMoney = Convert.ToInt32(reader["totalmoney"].ToString());
                        realMoney = userMoney;
                    }
                    catch (Exception)
                    {
                    }
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
        }

        /// <summary>
        /// Truy vấn số tiền nạp trong ngày của người chơi
        /// </summary>
        /// <param name="sex"></param>
        /// <param name="roleName"></param>
        public static void QueryTotalMoneyRechage(DBManager dbMgr, string userID, int roleiD, int zoneID, out int userMoney, out int realMoney)
        {
            userMoney = 0;
            realMoney = 0;

            MySQLConnection conn = null;

            try
            {
                conn = dbMgr.DBConns.PopDBConnection();

                string cmdText = "";
                if (roleiD != -1)
                {
                    cmdText = "SELECT SUM(amount) AS totalmoney FROM t_inputlog WHERE u= '" + userID + "' AND zoneID= " + zoneID + " AND role_action = " + roleiD + " ";
                }
                else
                {
                    cmdText = "SELECT SUM(amount) AS totalmoney FROM t_inputlog WHERE u= '" + userID + "' AND zoneID= " + zoneID + "";
                }
                MySQLCommand cmd = new MySQLCommand(cmdText, conn);
                MySQLDataReader reader = cmd.ExecuteReaderEx();

                if (reader.Read())
                {
                    try
                    {
                        userMoney = Convert.ToInt32(reader["totalmoney"].ToString());
                        realMoney = userMoney;
                    }
                    catch (Exception)
                    {
                    }
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
        }

        /// <summary>
        /// Truy vấn tổng số Đồng hiện có trong Server
        /// </summary>
        /// <param name="sex"></param>
        /// <param name="roleName"></param>
        public static long QueryServerTotalUserMoney()
        {
            using (MySqlUnity conn = new MySqlUnity())
            {
                return conn.GetSingleLong("SELECT IFNULL(SUM(money),0) as money FROM t_money");
            }
        }

        /// <summary>
        /// Thực hiện truy vấn mysql lấy ra tổng lượng đã nạp trong ngày
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public static int GetUserInputMoney(DBManager dbMgr, string userid, int roleid, int zoneid, string startTime, string endTime)
        {
            int totalmoney = 0;

            MySQLConnection conn = null;
            try
            {
                conn = dbMgr.DBConns.PopDBConnection();

                string cmdText = string.Format("SELECT u, sum(amount) as totalmoney, max(time) as time from t_inputlog where inputtime>='{0}' and inputtime<='{1}' and u='{2}' and role_action = " + roleid + " and zoneid={3} and result='success' GROUP by u ", startTime, endTime, userid, zoneid);

                MySQLCommand cmd = new MySQLCommand(cmdText, conn);
                MySQLDataReader reader = cmd.ExecuteReaderEx();

                while (reader.Read())
                {
                    totalmoney += Convert.ToInt32(reader["totalmoney"].ToString());
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

            return totalmoney;
        }

        /// <summary>
        /// Lấy ra tổng giá trị đã đánh dấu trong tháng
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="RoleID"></param>
        /// <param name="RecoreType"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="ZoneID"></param>
        /// <returns></returns>
        public static int GetSumRecoreValue(DBManager dbMgr, int RoleID, int RecoreType, string startTime, string endTime, int ZoneID)
        {
            int TotalValue = 0;

            MySQLConnection conn = null;
            try
            {
                conn = dbMgr.DBConns.PopDBConnection();

                string cmdText = "SELECT RoleID, SUM(ValueRecore) as TotalValue from t_recore where DateRecore>='" + startTime + "' and DateRecore<='" + endTime + "' and RoleID= " + RoleID + " and ZoneID= " + ZoneID + " and RecoryType = " + RecoreType + " GROUP by RoleID";

                MySQLCommand cmd = new MySQLCommand(cmdText, conn);
                MySQLDataReader reader = cmd.ExecuteReaderEx();

                while (reader.Read())
                {
                    TotalValue += Convert.ToInt32(reader["TotalValue"].ToString());
                }

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

            return TotalValue;
        }

        /// <summary>
        /// Lấy ra giá trị trước đó đã nhận hay chưa
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="RoleID"></param>
        /// <param name="TimeRanger"></param>
        /// <param name="RecoreType"></param>
        /// <returns></returns>
        public static int GetTMark(DBManager dbMgr, int RoleID, string TimeRanger, int RecoreType)
        {
            int T_MarkValue = 0;

            MySQLConnection conn = null;
            try
            {
                conn = dbMgr.DBConns.PopDBConnection();

                string cmdText = "SELECT MarkValue from t_mark where RoleID = " + RoleID + " and TimeRanger = '" + TimeRanger + "' and MarkType = " + RecoreType + "";

                MySQLCommand cmd = new MySQLCommand(cmdText, conn);
                MySQLDataReader reader = cmd.ExecuteReaderEx();

                while (reader.Read())
                {
                    T_MarkValue += Convert.ToInt32(reader["MarkValue"].ToString());
                }

                // GameDBManager.SystemServerSQLEvents.AddEvent(string.Format("+SQL: {0}", cmdText), EventLevels.Important);

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

            return T_MarkValue;
        }

        /// <summary>
        /// Lấy ra danh sách các vật phẩm sử dụng limit
        /// </summary>
        /// <param name="huodongID"></param>
        /// <returns></returns>
        public static int QueryLimitGoodsUsedNumByRoleID(DBManager dbMgr, int roleID, int goodsID, out int dayID, out int usedNum)
        {
            dayID = 0;
            usedNum = 0;

            int ret = -1;
            MySQLConnection conn = null;

            try
            {
                conn = dbMgr.DBConns.PopDBConnection();

                string cmdText = string.Format("SELECT dayid, usednum FROM t_limitgoodsbuy WHERE rid={0} AND goodsid={1}", roleID, goodsID);

                MySQLCommand cmd = new MySQLCommand(cmdText, conn);
                MySQLDataReader reader = cmd.ExecuteReaderEx();

                if (reader.Read())
                {
                    dayID = Convert.ToInt32(reader["dayid"].ToString());
                    usedNum = Convert.ToInt32(reader["usednum"].ToString());
                }

                GameDBManager.SystemServerSQLEvents.AddEvent(string.Format("+SQL: {0}", cmdText), EventLevels.Important);

                cmd.Dispose();
                cmd = null;

                ret = 0;
            }
            finally
            {
                if (null != conn)
                {
                    dbMgr.DBConns.PushDBConnection(conn);
                }
            }

            return ret;
        }

        /// <summary>
        /// Trả về danh sách Mail thu gọn của người chơi
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <returns></returns>
        public static List<MailData> GetMailItemDataList(DBManager dbMgr, int rid)
        {
            List<MailData> list = new List<MailData>();
            MySQLConnection conn = null;

            try
            {
                conn = dbMgr.DBConns.PopDBConnection();

                string cmdText = string.Format("SELECT mailid,senderrid,senderrname,sendtime,receiverrid,reveiverrname,readtime,isread," +
                                                " mailtype,hasfetchattachment,subject,content,bound_money,bound_token" +
                                                " from t_mail where receiverrid={0} ORDER by sendtime desc limit 100", rid);

                MySQLCommand cmd = new MySQLCommand(cmdText, conn);
                MySQLDataReader reader = cmd.ExecuteReaderEx();

                while (reader.Read())
                {
                    MailData mailItemData = new MailData()
                    {
                        MailID = Convert.ToInt32(reader["mailid"].ToString()),
                        SenderRID = Convert.ToInt32(reader["senderrid"].ToString()),
                        SenderRName = DataHelper.Base64Decode(reader["senderrname"].ToString()),
                        SendTime = reader["sendtime"].ToString(),
                        ReceiverRID = Convert.ToInt32(reader["receiverrid"].ToString()),
                        ReveiverRName = DataHelper.Base64Decode(reader["reveiverrname"].ToString()),
                        ReadTime = reader["readtime"].ToString(),
                        IsRead = Convert.ToInt32(reader["isread"].ToString()),
                        MailType = Convert.ToInt32(reader["mailtype"].ToString()),
                        HasFetchAttachment = Convert.ToInt32(reader["hasfetchattachment"].ToString()),
                        Subject = DataHelper.Base64Decode(reader["subject"].ToString()),
                        Content = "",
                        Money = Convert.ToInt32(reader["bound_money"].ToString()),
                        Token = Convert.ToInt32(reader["bound_token"].ToString()),
                    };

                    list.Add(mailItemData);
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

            return list;
        }

        /// <summary>
        /// Lấy tổng số vật phẩm đính kèm trong Mail
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="rid"></param>
        /// <param name="excludeIsRead">
        /// <returns></returns>
        public static int GetMailItemDataCount(DBManager dbMgr, int rid, int excludeReadState = 0, int limitCount = 1)
        {
            MySQLConnection conn = null;
            int count = 0;
            try
            {
                conn = dbMgr.DBConns.PopDBConnection();

                string cmdText = string.Format("SELECT mailid from t_mail where receiverrid={0} and isread<>{1} LIMIT 0,{2}", rid, excludeReadState, limitCount);

                MySQLCommand cmd = new MySQLCommand(cmdText, conn);
                MySQLDataReader reader = cmd.ExecuteReaderEx();

                while (reader.Read())
                {
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

            return count;
        }

        /// <summary>
        /// Lấy thông tin đầy đủ của Mail có ID tương ứng
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <returns></returns>
        public static MailData GetMailItemData(DBManager dbMgr, int rid, int mailID)
        {
            MailData mailItemData = null;
            MySQLConnection conn = null;

            try
            {
                conn = dbMgr.DBConns.PopDBConnection();

                string cmdText = string.Format("SELECT mailid,senderrid,senderrname,sendtime,receiverrid,reveiverrname,readtime,isread," +
                                                " mailtype,hasfetchattachment,subject,content,bound_money,bound_token" +
                                                " from t_mail where receiverrid={0} and mailid={1} ORDER by sendtime desc", rid, mailID);

                MySQLCommand cmd = new MySQLCommand(cmdText, conn);
                MySQLDataReader reader = cmd.ExecuteReaderEx();

                //有且仅有一封
                if (reader.Read())
                {
                    mailItemData = new MailData()
                    {
                        MailID = Convert.ToInt32(reader["mailid"].ToString()),
                        SenderRID = Convert.ToInt32(reader["senderrid"].ToString()),
                        SenderRName = DataHelper.Base64Decode(reader["senderrname"].ToString()),
                        SendTime = reader["sendtime"].ToString(),
                        ReceiverRID = Convert.ToInt32(reader["receiverrid"].ToString()),
                        ReveiverRName = DataHelper.Base64Decode(reader["reveiverrname"].ToString()),
                        ReadTime = reader["readtime"].ToString(),
                        IsRead = Convert.ToInt32(reader["isread"].ToString()),
                        MailType = Convert.ToInt32(reader["mailtype"].ToString()),
                        HasFetchAttachment = Convert.ToInt32(reader["hasfetchattachment"].ToString()),
                        Subject = DataHelper.Base64Decode(reader["subject"].ToString()),
                        Content = DataHelper.Base64Decode(reader["content"].ToString()),
                        Money = Convert.ToInt32(reader["bound_money"].ToString()),
                        Token = Convert.ToInt32(reader["bound_token"].ToString()),
                    };
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

            if (null != mailItemData)
            {
                mailItemData.GoodsList = GetMailGoodsDataList(dbMgr, mailID);
            }

            return mailItemData;
        }

        /// <summary>
        /// Trả về thông tin vật phẩm đính kèm trong thư
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="mailID"></param>
        /// <returns></returns>
        public static List<GoodsData> GetMailGoodsDataList(DBManager dbMgr, int mailID)
        {
            List<GoodsData> list = new List<GoodsData>();
            MySQLConnection conn = null;

            try
            {
                conn = dbMgr.DBConns.PopDBConnection();

                string cmdText = string.Format(@"SELECT Id, mailid, goodsid, forge_level, Props, gcount, binding, series, otherpramer, strong from t_mailgoods where mailid={0}", mailID);

                MySQLCommand cmd = new MySQLCommand(cmdText, conn);
                MySQLDataReader reader = cmd.ExecuteReaderEx();

                while (reader.Read())
                {
                    GoodsData mailItemData = new GoodsData()
                    {
                        Id = Convert.ToInt32(reader["id"].ToString()),
                        GoodsID = Convert.ToInt32(reader["goodsid"].ToString()),
                        Forge_level = Convert.ToInt32(reader["forge_level"].ToString()),
                        Props = reader["Props"].ToString(),
                        GCount = Convert.ToInt32(reader["gcount"].ToString()),
                        Binding = Convert.ToInt32(reader["binding"].ToString()),
                        Series = Convert.ToInt32(reader["series"].ToString()),
                        Strong = Convert.ToInt32(reader["strong"].ToString()),
                    };
                    byte[] Base64Decode = Convert.FromBase64String(reader["otherpramer"].ToString());
                    mailItemData.OtherParams = DataHelper.BytesToObject<Dictionary<ItemPramenter, string>>(Base64Decode, 0, Base64Decode.Length);

                    list.Add(mailItemData);
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

            return list;
        }

        /// <summary>
        /// Lấy ra last MailID
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <returns></returns>
        public static Dictionary<int, int> ScanLastMailIDListFromTable(DBManager dbMgr)
        {
            Dictionary<int, int> lastMailDct = new Dictionary<int, int>();

            MySQLConnection conn = null;
            try
            {
                conn = dbMgr.DBConns.PopDBConnection();

                string cmdText = string.Format("SELECT MAX(mailid) as mailid, receiverrid from t_mailtemp  GROUP by mailid,receiverrid ORDER by receiverrid asc limit 0, 20");

                MySQLCommand cmd = new MySQLCommand(cmdText, conn);
                MySQLDataReader reader = cmd.ExecuteReaderEx();

                while (reader.Read())
                {
                    int receiverrid = Convert.ToInt32(reader["receiverrid"].ToString());
                    if (!lastMailDct.ContainsKey(receiverrid))
                    {
                        lastMailDct.Add(receiverrid, Convert.ToInt32(reader["mailid"].ToString()));
                    }
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

            return lastMailDct;
        }

        /// <summary>
        /// Lấy ra ROLEID bởi role NAME
        /// </summary>
        /// <param name="sex"></param>
        /// <param name="roleName"></param>
        public static int GetRoleIDByRoleName(DBManager dbMgr, string roleName, int zoneid)
        {
            int rid = -1;

            if (string.IsNullOrWhiteSpace(roleName))
            {
                return rid;
            }

            MySQLConnection conn = null;
            try
            {
                conn = dbMgr.DBConns.PopDBConnection();

                string cmdText = string.Format("SELECT rid from t_roles WHERE rname='{0}' and zoneid={1}", roleName, zoneid);
                MySQLCommand cmd = new MySQLCommand(cmdText, conn);
                MySQLDataReader reader = cmd.ExecuteReaderEx();

                if (reader.Read())
                {
                    rid = Convert.ToInt32(reader["rid"].ToString());
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

            return rid;
        }

        /// <summary>
        /// Get maxMail ID
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <returns></returns>
        public static int GetMaxMailID(DBManager dbMgr)
        {
            int maxValue = -1;

            MySQLConnection conn = null;
            try
            {
                conn = dbMgr.DBConns.PopDBConnection();

                string cmdText = string.Format("SELECT MAX(mailid) as mymaxvalue from t_mail");
                MySQLCommand cmd = new MySQLCommand(cmdText, conn);
                MySQLDataReader reader = cmd.ExecuteReaderEx();

                if (reader.Read())
                {
                    maxValue = Global.SafeConvertToInt32(reader["mymaxvalue"].ToString());
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

            return maxValue;
        }

        /// <summary>
        /// Get ra max roleID
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <returns></returns>
        public static int GetMaxRoleID(DBManager dbMgr)
        {
            int maxValue = -1;

            MySQLConnection conn = null;
            try
            {
                conn = dbMgr.DBConns.PopDBConnection();

                string cmdText = string.Format("SELECT MAX(rid) as mymaxvalue from t_roles");
                MySQLCommand cmd = new MySQLCommand(cmdText, conn);
                MySQLDataReader reader = cmd.ExecuteReaderEx();

                if (reader.Read())
                {
                    maxValue = Global.SafeConvertToInt32(reader["mymaxvalue"].ToString());
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

            return maxValue;
        }

        /// <summary>
        /// Lấy ra max FAMILY
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <returns></returns>
        public static int GetMaxFamilyID(DBManager dbMgr)
        {
            int maxValue = -1;

            MySQLConnection conn = null;
            try
            {
                conn = dbMgr.DBConns.PopDBConnection();

                string cmdText = string.Format("SELECT MAX(FamilyID) as mymaxvalue from t_family");
                MySQLCommand cmd = new MySQLCommand(cmdText, conn);
                MySQLDataReader reader = cmd.ExecuteReaderEx();

                if (reader.Read())
                {
                    maxValue = Global.SafeConvertToInt32(reader["mymaxvalue"].ToString());
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

            return maxValue;
        }

        /// <summary>
        /// Lấy ra max GUILID
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <returns></returns>
        public static int GetMaxGuildID(DBManager dbMgr)
        {
            int maxValue = -1;

            MySQLConnection conn = null;
            try
            {
                conn = dbMgr.DBConns.PopDBConnection();

                string cmdText = string.Format("SELECT MAX(GuildID) as mymaxvalue from t_guild");
                MySQLCommand cmd = new MySQLCommand(cmdText, conn);
                MySQLDataReader reader = cmd.ExecuteReaderEx();

                if (reader.Read())
                {
                    maxValue = Global.SafeConvertToInt32(reader["mymaxvalue"].ToString());
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

            return maxValue;
        }

        /// <summary>
        /// Lấy ra tổng số lần nạp
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        public static int GetFirstChongZhiDaLiNum(DBManager dbMgr, string userID)
        {
            int totalNum = 0;

            MySQLConnection conn = null;
            try
            {
                conn = dbMgr.DBConns.PopDBConnection();

                string cmdText = string.Format("SELECT COUNT(rid) AS totalnum from t_roles WHERE userid='{0}' and cztaskid>0", userID);
                MySQLCommand cmd = new MySQLCommand(cmdText, conn);
                MySQLDataReader reader = cmd.ExecuteReaderEx();

                try
                {
                    if (reader.Read())
                    {
                        totalNum = Convert.ToInt32(reader["totalnum"].ToString());
                    }
                }
                catch (Exception)
                {
                    totalNum = 0;
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

            return totalNum;
        }

        /// <summary>
        /// Trả về danh sách các tham biến hệ thống
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="index"></param>
        public static ConcurrentDictionary<int, string> GetSystemGlobalValues(DBManager dbMgr)
        {
            ConcurrentDictionary<int, string> parameters = new ConcurrentDictionary<int, string>();
            MySQLConnection conn = null;
            try
            {
                conn = dbMgr.DBConns.PopDBConnection();
                string cmdText = string.Format("SELECT id, value FROM t_systemglobalvalue");
                MySQLCommand cmd = new MySQLCommand(cmdText, conn);
                MySQLDataReader reader = cmd.ExecuteReaderEx();

                while (reader.Read())
                {
                    int id = Convert.ToInt32(reader["id"].ToString());
                    string value = reader["value"].ToString();
                    parameters[id] = value;
                }
            }
            finally
            {
                if (null != conn)
                {
                    dbMgr.DBConns.PushDBConnection(conn);
                }
            }
            return parameters;
        }

        /// <summary>
        /// Lưu tham biến hệ thống
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="index"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string SaveSystemGlobalValue(DBManager dbMgr, int index, string value)
        {
            MySQLConnection conn = null;
            try
            {
                conn = dbMgr.DBConns.PopDBConnection();
                /// Kiểm tra tồn tại chưa
                //bool isExist = false;
                //{
                //    string cmdText = string.Format("SELECT value FROM t_systemglobalvalue WHERE id = {0}", index);
                //    MySQLCommand cmd = new MySQLCommand(cmdText, conn);
                //    MySQLDataReader reader = cmd.ExecuteReaderEx();
                //    /// Nếu có dữ liệu nghĩa là đã tồn tại
                //    isExist = reader.Read();
                //    cmd.Dispose();
                //    /// Giải phóng
                //    cmd = null;
                //}

                ///// Nếu đã tồn tại thì Update
                //if (isExist)
                //{
                //    string cmdText = string.Format("UPDATE t_systemglobalvalue SET value = {0} WHERE id = {1}", value, index);
                //    MySQLCommand cmd = new MySQLCommand(cmdText, conn);
                //    cmd.ExecuteNonQuery();
                //    cmd.Dispose();
                //    /// Giải phóng
                //    cmd = null;
                //}
                ///// Nếu chưa tồn tại thì Insert
                //else
                //{
                //    string cmdText = string.Format("INSERT INTO t_systemglobalvalue(id, value) VALUES({0}, {1}) ON DUPLICATE KEY UPDATE value = '{1}'", index, value);
                //    MySQLCommand cmd = new MySQLCommand(cmdText, conn);
                //    cmd.ExecuteNonQuery();
                //    cmd.Dispose();
                //    /// Giải phóng
                //    cmd = null;
                //}

                string cmdText = string.Format("INSERT INTO t_systemglobalvalue(id, value) VALUES({0}, '{1}') ON DUPLICATE KEY UPDATE value = '{1}'", index, value);
                MySQLCommand cmd = new MySQLCommand(cmdText, conn);
                cmd.ExecuteNonQuery();
                cmd.Dispose();
                /// Giải phóng
                cmd = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                if (null != conn)
                {
                    dbMgr.DBConns.PushDBConnection(conn);
                }
            }
            return "";
        }

        /// <summary>
        /// Lấy ra số tiền mà tài khoản đã sử dụng
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="rid"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public static int GetUserUsedMoney(DBManager dbMgr, int rid, string startTime, string endTime)
        {
            int totalmoney = 0;

            MySQLConnection conn = null;
            try
            {
                conn = dbMgr.DBConns.PopDBConnection();

                string cmdText = string.Format("SELECT rid, sum(amount) as totalmoney  from t_consumelog where cdate>='{0}' and cdate<='{1}' and rid={2} GROUP by rid "
                                 , startTime, endTime, rid);

                MySQLCommand cmd = new MySQLCommand(cmdText, conn);
                MySQLDataReader reader = cmd.ExecuteReaderEx();

                while (reader.Read())
                {
                    totalmoney = (int)Convert.ToDouble(reader["totalmoney"].ToString());
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

            return totalmoney;
        }

        /// <summary>
        /// Lấy ra số ĐỒng đã tiêu trong toàn bộ thời gian
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="rid"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public static int GetUserUsedMoney(DBManager dbMgr, int rid)
        {
            int totalmoney = 0;

            MySQLConnection conn = null;
            try
            {
                conn = dbMgr.DBConns.PopDBConnection();

                string cmdText = string.Format("SELECT rid, sum(amount) as totalmoney  from t_consumelog where rid={0} GROUP by rid"
                                 , rid);

                MySQLCommand cmd = new MySQLCommand(cmdText, conn);
                MySQLDataReader reader = cmd.ExecuteReaderEx();

                while (reader.Read())
                {
                    totalmoney = (int)Convert.ToDouble(reader["totalmoney"].ToString());
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

            return totalmoney;
        }

        /// <summary>
        /// Scan ra 1 nhóm email mới trong bảng
        /// </summary>
        /// <param name="sex"></param>
        /// <param name="roleName"></param>
        public static List<GroupMailData> ScanNewGroupMailFromTable(DBManager dbMgr, int beginID)
        {
            List<GroupMailData> GroupMailList = null;

            MySQLConnection conn = null;
            try
            {
                conn = dbMgr.DBConns.PopDBConnection();

                string today = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                string cmdText = string.Format("SELECT * from t_groupmail where gmailid > {0} and endtime > '{1}'", beginID, today);

                MySQLCommand cmd = new MySQLCommand(cmdText, conn);
                MySQLDataReader reader = cmd.ExecuteReaderEx();

                while (reader.Read())
                {
                    GroupMailData gmailData = new GroupMailData();
                    gmailData.GMailID = Convert.ToInt32(reader["gmailid"].ToString());
                    gmailData.Subject = reader["subject"].ToString();
                    gmailData.Content = reader["content"].ToString();//Encoding.Default.GetString((byte[])reader["content"]);
                    gmailData.Conditions = reader["conditions"].ToString();
                    gmailData.InputTime = DateTime.Parse(reader["inputtime"].ToString()).Ticks;
                    gmailData.EndTime = DateTime.Parse(reader["endtime"].ToString()).Ticks;
                    gmailData.Yinliang = Convert.ToInt32(reader["yinliang"].ToString());
                    gmailData.Tongqian = Convert.ToInt32(reader["tongqian"].ToString());
                    gmailData.YuanBao = Convert.ToInt32(reader["yuanbao"].ToString());
                    gmailData.GoodsList = Global.ParseGoodsDataList(reader["goodlist"].ToString());

                    if (null == GroupMailList)
                    {
                        GroupMailList = new List<GroupMailData>();
                    }

                    GroupMailList.Add(gmailData);
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

            return GroupMailList;
        }

        public static int GetIntValue(string Sql, string ReadPram)
        {
            int Value = -1;
            using (MySqlUnity conn = new MySqlUnity())
            {
                string cmdText = Sql;
                MySQLDataReader reader = conn.ExecuteReader(cmdText);
                if (reader.Read())
                {
                    Value = Convert.ToInt32(reader[ReadPram].ToString());
                }
            }

            return Value;
        }

        /// <summary>
        /// Lấy ra role mini data
        /// </summary>
        /// <param name="rid"></param>
        /// <returns></returns>
        public static RoleMiniInfo QueryRoleMiniInfo(long rid)
        {
            RoleMiniInfo roleMiniInfo = null;
            using (MySqlUnity conn = new MySqlUnity())
            {
                string cmdText = string.Format("SELECT zoneid,userid from t_roles where rid={0};", rid);
                MySQLDataReader reader = conn.ExecuteReader(cmdText);
                if (reader.Read())
                {
                    roleMiniInfo = new RoleMiniInfo();
                    roleMiniInfo.roleId = rid;
                    roleMiniInfo.zoneId = Convert.ToInt32(reader["zoneid"].ToString());
                    roleMiniInfo.userId = reader["userid"].ToString();
                }
            }

            return roleMiniInfo;
        }

        /// <summary>
        /// Lấy ra số lần đăng nhập của người chơi
        /// </summary>
        /// <param name="sex"></param>
        /// <param name="roleName"></param>
        public static AccountActiveData GetAccountActiveInfo(DBManager dbMgr, string strAccountID)
        {
            using (MySqlUnity conn = new MySqlUnity())
            {
                string sql = string.Format("select * from t_user_active_info where Account = '{0}';", strAccountID);

                return conn.ReadObject<AccountActiveData>(sql);
            }
        }

        /// <summary>
        /// Lưu lại số lần đăng nhập của người chơi
        /// </summary>
        /// <param name="sex"></param>
        /// <param name="roleName"></param>
        public static bool UpdateAccountActiveInfo(DBManager dbMgr, string strAccountID)
        {
            bool ret = false;
            string today = DateTime.Now.ToString("yyyy-MM-dd");

            AccountActiveData dataActive = GetAccountActiveInfo(dbMgr, strAccountID);
            if (null == dataActive)
            {
                using (MySqlUnity conn = new MySqlUnity())
                {
                    string cmdText = string.Format("INSERT INTO t_user_active_info(Account, createTime, seriesLoginCount, lastSeriesLoginTime) VALUES('{0}', '{1}', {2}, '{3}')",
                                strAccountID, today, 1, today);
                    ret = conn.ExecuteNonQueryBool(cmdText);
                }
            }
            else
            {
                DateTime datePreDay = DateTime.Now.AddDays(-1);
                DateTime dateLastLogin = DateTime.Parse(dataActive.strLastSeriesLoginTime + " 00:00:00");

                if (datePreDay.DayOfYear == dateLastLogin.DayOfYear)
                {
                    using (MySqlUnity conn = new MySqlUnity())
                    {
                        string cmdText = string.Format("UPDATE t_user_active_info SET seriesLoginCount={0}, lastSeriesLoginTime='{1}' WHERE Account='{2}'",
                                    dataActive.nSeriesLoginCount + 1, today, dataActive.strAccount);
                        ret = conn.ExecuteNonQueryBool(cmdText);
                    }
                }
            }

            return ret;
        }
    }
}