using GameDBServer.Core;
using GameDBServer.DB;
using GameDBServer.Server;
using MySQLDriverCS;
using Server.Data;
using Server.Protocol;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameDBServer.Logic
{
    /// <summary>
    /// Bảng xếp hạng
    /// </summary>
    public class RankingManager
    {
        private static RankingManager instance = new RankingManager();

        /// <summary>
        /// Nếu đang xử lý dữ liệu
        /// </summary>
        public bool IsRankProseccsing = false;

        /// <summary>
        ///  10 phút bú 1 lần
        /// </summary>
        private const long MaxDBRoleParamCmdSlot = (60 * 10 * 1 * 1000);

        public long LastUpdateRanking = 0;

        public static RankingManager getInstance()
        {
            return instance;
        }

        /// <summary>
        /// Tổng số bản ghi sẽ lấy ra 1 trang
        /// </summary>
        public int TotalRankNumber = 10;

        /// <summary>
        /// Rank
        /// </summary>
        public static Dictionary<RankMode, List<PlayerRanking>> RankServer = new Dictionary<RankMode, List<PlayerRanking>>();

        public static Dictionary<RankMode, List<PlayerRanking>> CacheTopServerWhenEndEvent = new Dictionary<RankMode, List<PlayerRanking>>();

        /// <summary>
        /// Lấy ra xếp hạng rank của 1 người chơi trong bảng xếp hạng
        /// </summary>
        /// <param name="RoleID"></param>
        /// <param name="_RankInput"></param>
        /// <returns></returns>
        public int GetRankOfPlayer(int RoleID, RankMode _RankInput)
        {
            if (IsRankProseccsing)
            {
                return -1;
            }
            else
            {
                RankServer.TryGetValue(_RankInput, out List<PlayerRanking> _Rank);

                if (_Rank != null)
                {
                    var find = _Rank.Where(x => x.RoleID == RoleID).FirstOrDefault();
                    if (find != null)
                    {
                        return find.ID;
                    }
                    else
                    {
                        return -100;
                    }
                }
                else
                {
                    return -1;
                }
            }
        }

        public int Count(RankMode _ModeIN)
        {
            if (IsRankProseccsing)
            {
                return 0;
            }
            if (RankServer.ContainsKey(_ModeIN))
            {
                int count = 0;

                count = RankServer[_ModeIN].Count;

                return count;
            }
            return 0;
        }

        public void UpdateRank(DBManager _DbInput)
        {
            long Now = TimeUtil.NOW();

            if (Now - LastUpdateRanking > MaxDBRoleParamCmdSlot)
            {
                IsRankProseccsing = true;

                RankServer = new Dictionary<RankMode, List<PlayerRanking>>();
                LastUpdateRanking = Now;

                LogManager.WriteLog(LogTypes.SQL, "DO EXECUTE UPDATE RANK DATA!");

                RankServer[RankMode.CapDo] = GetTop100RankLevel(_DbInput);
                RankServer[RankMode.TaiPhu] = GetTop100RankTaiPhu(_DbInput);

                CacheTopServerWhenEndEvent[RankMode.CapDo] = GetTop10LevelRanking(_DbInput);
                CacheTopServerWhenEndEvent[RankMode.TaiPhu] = GetTop10RankingTaiPhu(_DbInput);

                IsRankProseccsing = false;

                LogManager.WriteLog(LogTypes.SQL, "END  EXECUTE UPDATE RANK DATA!");
                // THỰC HIỆN CÁC TRUY VẤN Ở ĐÂY ĐỂ FILL RA BẢNG
            }
        }

        #region GetRank Cấp độ

        public List<PlayerRanking> GetTop10LevelRanking(DBManager dbMgr)
        {
            List<PlayerRanking> TotalRank = new List<PlayerRanking>();

            MySQLConnection conn = null;

            try
            {
                conn = dbMgr.DBConns.PopDBConnection();

                string cmdText = string.Format("Select rid,rname,occupation,sub_id,experience,level,RankingEventType0Value,RankingEventType0Status,CurentRank0Value from t_ranking WHERE RankingEventType0Value!=-1 order by RankingEventType0Value  LIMIT 10");

                MySQLCommand cmd = new MySQLCommand(cmdText, conn);
                MySQLDataReader reader = cmd.ExecuteReaderEx();

                int count = 0;
                while (reader.Read())
                {
                    PlayerRanking paiHangItemData = new PlayerRanking()
                    {
                        ID = count,
                        RoleID = Convert.ToInt32(reader["rid"].ToString()),
                        RoleName = reader["rname"].ToString(),
                        Type = (int)RankMode.CapDo,
                        FactionID = Convert.ToInt32(reader["occupation"].ToString()),
                        RouteID = Convert.ToInt32(reader["sub_id"].ToString()),
                        Level = Convert.ToInt32(reader["CurentRank0Value"].ToString()),
                        Value = 0,
                        Status = Convert.ToInt32(reader["RankingEventType0Status"].ToString()),
                        LastIndex = Convert.ToInt32(reader["RankingEventType0Value"].ToString()),
                    };

                    TotalRank.Add(paiHangItemData);
                    count++;
                }

                //  GameDBManager.SystemServerSQLEvents.AddEvent(string.Format("+SQL: {0}", cmdText), EventLevels.Important);

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

            return TotalRank;
        }

        public List<PlayerRanking> GetTop100RankLevel(DBManager dbMgr)
        {
            List<PlayerRanking> TotalRank = new List<PlayerRanking>();

            MySQLConnection conn = null;

            try
            {
                conn = dbMgr.DBConns.PopDBConnection();

                string cmdText = string.Format("Select rid,rname,occupation,sub_id,experience,level,RankingEventType0Value,RankingEventType0Status from t_ranking order by level desc,experience desc  LIMIT 100");

                MySQLCommand cmd = new MySQLCommand(cmdText, conn);
                MySQLDataReader reader = cmd.ExecuteReaderEx();

                int count = 0;
                while (reader.Read() && count < 100)
                {
                  

                    PlayerRanking paiHangItemData = new PlayerRanking()
                    {

                       
                        ID = count,
                        RoleID = Convert.ToInt32(reader["rid"].ToString()),
                        RoleName = reader["rname"].ToString(),
                        Type = (int)RankMode.CapDo,
                        FactionID = Convert.ToInt32(reader["occupation"].ToString()),
                        RouteID = Convert.ToInt32(reader["sub_id"].ToString()),
                        Level = Convert.ToInt32(reader["level"].ToString()),
                        Value = (int)(Convert.ToInt64(reader["experience"].ToString())/100),
                        Status = Convert.ToInt32(reader["RankingEventType0Status"].ToString()),
                        LastIndex = Convert.ToInt32(reader["RankingEventType0Value"].ToString()),
                    };

                    TotalRank.Add(paiHangItemData);
                    count++;
                }

                //  GameDBManager.SystemServerSQLEvents.AddEvent(string.Format("+SQL: {0}", cmdText), EventLevels.Important);

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

            return TotalRank;
        }

        #endregion GetRank Cấp độ

        #region RANKTAIPHU

        public List<PlayerRanking> GetTop10RankingTaiPhu(DBManager dbMgr)
        {
            List<PlayerRanking> TotalRank = new List<PlayerRanking>();

            MySQLConnection conn = null;

            try
            {
                conn = dbMgr.DBConns.PopDBConnection();

                string cmdText = string.Format("Select rid,rname,occupation,sub_id,taiphu,level,RankingEventType1Status,RankingEventType1Value,CurentRank1Value from t_ranking where RankingEventType1Value!=-1 order by RankingEventType1Value LIMIT 10");

                MySQLCommand cmd = new MySQLCommand(cmdText, conn);
                MySQLDataReader reader = cmd.ExecuteReaderEx();

                int count = 0;
                while (reader.Read())
                {
                    PlayerRanking paiHangItemData = new PlayerRanking()
                    {
                        ID = count,
                        RoleID = Convert.ToInt32(reader["rid"].ToString()),
                        RoleName = reader["rname"].ToString(),
                        Type = (int)RankMode.TaiPhu,
                        FactionID = Convert.ToInt32(reader["occupation"].ToString()),
                        RouteID = Convert.ToInt32(reader["sub_id"].ToString()),
                        Level = Convert.ToInt32(reader["level"].ToString()),
                        Value = Convert.ToInt32(reader["CurentRank1Value"].ToString()),
                        Status = Convert.ToInt32(reader["RankingEventType1Status"].ToString()),
                        LastIndex = Convert.ToInt32(reader["RankingEventType1Value"].ToString()),
                    };

                    TotalRank.Add(paiHangItemData);

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

            return TotalRank;
        }

        public List<PlayerRanking> GetTop100RankTaiPhu(DBManager dbMgr)
        {
            List<PlayerRanking> TotalRank = new List<PlayerRanking>();

            MySQLConnection conn = null;

            try
            {
                conn = dbMgr.DBConns.PopDBConnection();

                string cmdText = string.Format("Select rid,rname,occupation,sub_id,taiphu,level,RankingEventType1Status,RankingEventType1Value from t_ranking order by taiphu desc LIMIT 500");

                MySQLCommand cmd = new MySQLCommand(cmdText, conn);
                MySQLDataReader reader = cmd.ExecuteReaderEx();

                int count = 0;
                while (reader.Read() && count < 500)
                {
                    PlayerRanking paiHangItemData = new PlayerRanking()
                    {
                        ID = count,
                        RoleID = Convert.ToInt32(reader["rid"].ToString()),
                        RoleName = reader["rname"].ToString(),
                        Type = (int)RankMode.TaiPhu,
                        FactionID = Convert.ToInt32(reader["occupation"].ToString()),
                        RouteID = Convert.ToInt32(reader["sub_id"].ToString()),
                        Level = Convert.ToInt32(reader["level"].ToString()),
                        Value = Convert.ToInt32(reader["taiphu"].ToString()),
                        Status = Convert.ToInt32(reader["RankingEventType1Status"].ToString()),
                        LastIndex = Convert.ToInt32(reader["RankingEventType1Value"].ToString()),
                    };

                    TotalRank.Add(paiHangItemData);

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

            return TotalRank;
        }

        #endregion RANKTAIPHU

        #region RANKTAIPHU

        public List<PlayerRanking> GetTop100RankVoLam(DBManager dbMgr)
        {
            List<PlayerRanking> TotalRank = new List<PlayerRanking>();

            MySQLConnection conn = null;

            try
            {
                conn = dbMgr.DBConns.PopDBConnection();

                string cmdText = string.Format("Select rid,rname,occupation,sub_id,volam,level from t_ranking order by volam desc,level desc LIMIT 100");

                MySQLCommand cmd = new MySQLCommand(cmdText, conn);
                MySQLDataReader reader = cmd.ExecuteReaderEx();

                int count = 0;
                while (reader.Read() && count < 100)
                {
                    PlayerRanking paiHangItemData = new PlayerRanking()
                    {
                        ID = count,
                        RoleID = Convert.ToInt32(reader["rid"].ToString()),
                        RoleName = DataHelper.Base64Decode(reader["rname"].ToString()),
                        Type = (int)RankMode.TaiPhu,
                        FactionID = Convert.ToInt32(reader["occupation"].ToString()),
                        RouteID = Convert.ToInt32(reader["sub_id"].ToString()),
                        Level = Convert.ToInt32(reader["level"].ToString()),
                        Value = Convert.ToInt32(reader["volam"].ToString()),
                    };

                    TotalRank.Add(paiHangItemData);
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

            return TotalRank;
        }

        #endregion RANKTAIPHU

        #region RANKTAIPHU

        public List<PlayerRanking> GetTop100RankLienDau(DBManager dbMgr)
        {
            List<PlayerRanking> TotalRank = new List<PlayerRanking>();

            MySQLConnection conn = null;

            try
            {
                conn = dbMgr.DBConns.PopDBConnection();

                string cmdText = string.Format("Select rid,rname,occupation,sub_id,liendau,level from t_ranking order by liendau desc,level desc LIMIT 100");

                MySQLCommand cmd = new MySQLCommand(cmdText, conn);
                MySQLDataReader reader = cmd.ExecuteReaderEx();

                int count = 0;
                while (reader.Read() && count < 100)
                {
                    PlayerRanking paiHangItemData = new PlayerRanking()
                    {
                        ID = count,
                        RoleID = Convert.ToInt32(reader["rid"].ToString()),
                        RoleName = DataHelper.Base64Decode(reader["rname"].ToString()),
                        Type = (int)RankMode.TaiPhu,
                        FactionID = Convert.ToInt32(reader["occupation"].ToString()),
                        RouteID = Convert.ToInt32(reader["sub_id"].ToString()),
                        Level = Convert.ToInt32(reader["level"].ToString()),
                        Value = Convert.ToInt32(reader["liendau"].ToString()),
                    };

                    TotalRank.Add(paiHangItemData);
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

            return TotalRank;
        }

        #endregion RANKTAIPHU

        #region RANKTAIPHU

        public List<PlayerRanking> GetTop100RankUyDanh(DBManager dbMgr)
        {
            List<PlayerRanking> TotalRank = new List<PlayerRanking>();

            MySQLConnection conn = null;

            try
            {
                conn = dbMgr.DBConns.PopDBConnection();

                string cmdText = string.Format("Select rid,rname,occupation,sub_id,uydanh,level from t_ranking order by uydanh desc,level desc LIMIT 100");

                MySQLCommand cmd = new MySQLCommand(cmdText, conn);
                MySQLDataReader reader = cmd.ExecuteReaderEx();

                int count = 0;
                while (reader.Read() && count < 100)
                {
                    PlayerRanking paiHangItemData = new PlayerRanking()
                    {
                        ID = count,
                        RoleID = Convert.ToInt32(reader["rid"].ToString()),
                        RoleName = DataHelper.Base64Decode(reader["rname"].ToString()),
                        Type = (int)RankMode.TaiPhu,
                        FactionID = Convert.ToInt32(reader["occupation"].ToString()),
                        RouteID = Convert.ToInt32(reader["sub_id"].ToString()),
                        Level = Convert.ToInt32(reader["level"].ToString()),
                        Value = Convert.ToInt32(reader["uydanh"].ToString()),
                    };

                    TotalRank.Add(paiHangItemData);
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

            return TotalRank;
        }

        #endregion RANKTAIPHU

        #region RANKTAIPHU

        public List<PlayerRanking> GetTop100Faction(DBManager dbMgr, int FactionID)
        {
            List<PlayerRanking> TotalRank = new List<PlayerRanking>();

            MySQLConnection conn = null;

            try
            {
                conn = dbMgr.DBConns.PopDBConnection();

                string cmdText = string.Format("Select rid,rname,occupation,sub_id,monphai,level from t_ranking where occupation = " + FactionID + " order by monphai desc,level desc LIMIT 100");

                MySQLCommand cmd = new MySQLCommand(cmdText, conn);
                MySQLDataReader reader = cmd.ExecuteReaderEx();

                int count = 0;
                while (reader.Read() && count < 100)
                {
                    PlayerRanking paiHangItemData = new PlayerRanking()
                    {
                        ID = count,
                        RoleID = Convert.ToInt32(reader["rid"].ToString()),
                        RoleName = DataHelper.Base64Decode(reader["rname"].ToString()),
                        Type = (int)RankMode.TaiPhu,
                        FactionID = Convert.ToInt32(reader["occupation"].ToString()),
                        RouteID = Convert.ToInt32(reader["sub_id"].ToString()),
                        Level = Convert.ToInt32(reader["level"].ToString()),
                        Value = Convert.ToInt32(reader["monphai"].ToString()),
                    };

                    TotalRank.Add(paiHangItemData);
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

            return TotalRank;
        }

        #endregion RANKTAIPHU

        /// <summary>
        /// Lấy ra rank chỉ định
        /// </summary>
        /// <param name="Input"></param>
        /// <returns></returns>
        public List<PlayerRanking> GetRank(RankMode Input, int RoleID, int PageNumber, int State = 0)
        {
            List<PlayerRanking> _TotalRank = new List<PlayerRanking>();

            if (IsRankProseccsing)
            {
                return _TotalRank;
            }

            try
            {   // Nếu là rank thường
                if (State == 0)
                {
                    if (RankServer.ContainsKey(Input))
                    {
                        int END = PageNumber * TotalRankNumber;

                        int START = END - TotalRankNumber;

                        // Nếu mà số lượng trong bản còn thấp hơn cả start của page thì trả về trống
                        if (RankServer[Input].Count <= START)
                        {
                            return _TotalRank;
                        }

                        if (RankServer[Input].Count > START && RankServer[Input].Count < END)
                        {
                            int RANGER = RankServer[Input].Count - START;
                            _TotalRank = RankServer[Input].GetRange(START, RANGER);
                        }
                        else
                        {
                            _TotalRank = RankServer[Input].GetRange(START, TotalRankNumber);
                        }
                        // Lấy ra thứ hạng bản thân

                        var find = RankServer[Input].Where(x => x.RoleID == RoleID).FirstOrDefault();
                        if (find != null)
                        {
                            _TotalRank.Add(find);
                        }
                        else
                        {
                            PlayerRanking _Rank = new PlayerRanking();
                            _Rank.RoleID = RoleID;
                            _Rank.ID = -1000;

                            _TotalRank.Add(_Rank);
                        }
                    }
                }
                else if (State == 2)
                {
                    if (RankServer.ContainsKey(Input))
                    {
                        int END = PageNumber * TotalRankNumber;

                        int START = END - TotalRankNumber;

                        // Nếu mà số lượng trong bản còn thấp hơn cả start của page thì trả về trống
                        if (RankServer[Input].Count <= START)
                        {
                            return _TotalRank;
                        }

                        if (RankServer[Input].Count > START && RankServer[Input].Count < END)
                        {
                            int RANGER = RankServer[Input].Count - START;
                            _TotalRank = RankServer[Input].GetRange(START, RANGER);
                        }
                        else
                        {
                            _TotalRank = RankServer[Input].GetRange(START, TotalRankNumber);
                        }
                        // Lấy ra thứ hạng bản thân

                        var find = RankServer[Input].Where(x => x.RoleID == RoleID).FirstOrDefault();
                        if (find != null)
                        {
                            _TotalRank.Add(find);
                        }
                    }
                }
                else if (State == 1)
                {
                    if (RankServer.ContainsKey(Input))
                    {
                        // Tạo ra 1 list mới coppy cái list cũ
                        List<PlayerRanking> TotalPlayBeforeSoft = new List<PlayerRanking>(CacheTopServerWhenEndEvent[Input]);
                        // Thực hiện soft lại theo thứ tự

                        // sort lại theo thứ tự đã cache
                        List<PlayerRanking> AfterSoft = TotalPlayBeforeSoft.OrderBy(x => x.LastIndex).ToList();

                        if (AfterSoft.Count > 10)
                        {
                            _TotalRank = AfterSoft.Take(10).ToList();
                        }
                        else
                        {
                            _TotalRank = AfterSoft;
                        }

                        //Set lại Index cho ID để về client hiển thị lại
                        _TotalRank.ForEach(x => x.ID = x.LastIndex);

                        //RankServer.TryGetValue(Input, out TotalPlayBeforeSoft);
                    }
                }
                else if (State == 3)
                {
                    if (RankServer.ContainsKey(Input))
                    {
                        //Thực hiện update mới nhất top 100 thằng
                        if (Input == RankMode.CapDo)
                        {
                            IsRankProseccsing = true;
                            RankServer[RankMode.CapDo] = GetTop100RankLevel(DBManager.getInstance());
                            IsRankProseccsing = false;
                        }

                        //Update mới nhất 100 thằng
                        if (Input == RankMode.TaiPhu)
                        {
                            IsRankProseccsing = true;
                            // Set lại rank cho tài phú
                            RankServer[RankMode.TaiPhu] = GetTop100RankTaiPhu(DBManager.getInstance());

                            IsRankProseccsing = false;
                        }

                        // Tạo ra 1 list mới coppy cái list cũ
                        List<PlayerRanking> TotalPlayBeforeSoft = RankServer[Input];
                        // Thực hiện soft lại theo thứ tự

                        // Chỉ lấy ra 10 thằng có thứ hạng cao nhất
                        if (TotalPlayBeforeSoft.Count > 10)
                        {
                            // Chỉ lấy ra 10 ông thôi
                            _TotalRank = TotalPlayBeforeSoft.Take(10).ToList();
                        }
                        else
                        {
                            _TotalRank = TotalPlayBeforeSoft;
                        }

                        // Thực hiện set lại LAST ID cho nó
                        foreach (PlayerRanking _Rank in _TotalRank)
                        {
                            // Đánh dấu lại vào list cache đã khai sinh
                            var FindUpdate = RankServer[Input].Where(x => x.RoleID == _Rank.RoleID).FirstOrDefault();
                            if (FindUpdate != null)
                            {
                                int VALUECURENT = -1;
                                if (Input == RankMode.CapDo)
                                {
                                    VALUECURENT = _Rank.Level;
                                }
                                else if (Input == RankMode.TaiPhu)
                                {
                                    VALUECURENT = _Rank.Value;
                                }
                                // Set lại lastIndex cho ông này
                                FindUpdate.LastIndex = _Rank.ID;
                                // Set 10 ông này vào game DB để sau này nếu reload lại DB thì vãn ngon
                                string UpdateRanking = "Update t_ranking set RankingEventType" + (int)Input + "Value = " + _Rank.ID + ",CurentRank" + (int)Input + "Value = " + VALUECURENT + " where rid = " + _Rank.RoleID + "";
                                // Thực hiện ghi lại vào DB
                                DBWriter.ExecuteSqlScript(UpdateRanking);
                            }
                        }

                        // Cache lại TOP 10 
                        if (Input == RankMode.CapDo)
                        {
                            IsRankProseccsing = true;
                            CacheTopServerWhenEndEvent[RankMode.CapDo] = GetTop10LevelRanking(DBManager.getInstance());
                            IsRankProseccsing = false;
                        }

                        // Cache lại TOP 10 
                        if (Input == RankMode.TaiPhu)
                        {
                            IsRankProseccsing = true;
                            CacheTopServerWhenEndEvent[RankMode.TaiPhu] = GetTop10RankingTaiPhu(DBManager.getInstance());
                            IsRankProseccsing = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception,ex.ToString());
            }

            return _TotalRank;
        }

        /// <summary>
        /// Lấy ra topranking của 1 loại rank nào đó sử dụng trong event đua top
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <param name="tcpOutPacket"></param>
        /// <returns></returns>
        public static TCPProcessCmdResults CMD_KT_TOPRANKING(DBManager dbMgr, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
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
                if (fields.Length != 2)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}",
                        (TCPGameServerCmds)nID, fields.Length, cmdData));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                int _RankMode = Int32.Parse(fields[0]);

                int State = Int32.Parse(fields[1]);

                RankMode _Mode = (RankMode)_RankMode;

                List<PlayerRanking> TotalRanking = new List<PlayerRanking>();
                // Nếu như sự kiện chưa kết thúc
                TotalRanking = RankingManager.getInstance().GetRank(_Mode, -1, 1, State);

                // trả về thông tin ngắn gọn của bang hội
                //TODO : F9 xem thông tin bang nhận được ở GS đã đúng hay chưa
                tcpOutPacket = DataHelper.ObjectToTCPOutPacket<List<PlayerRanking>>(TotalRanking, pool, nID);

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
        /// Lấy ra thứ hạng của 1 người chơi với loại rank chỉ định
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <param name="tcpOutPacket"></param>
        /// <returns></returns>
        public static TCPProcessCmdResults CMD_KT_RANKING_CHECKING(DBManager dbMgr, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
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
                if (fields.Length != 2)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}",
                        (TCPGameServerCmds)nID, fields.Length, cmdData));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                int _RankMode = Int32.Parse(fields[0]);

                int RoleID = Int32.Parse(fields[1]);

                RankMode _Mode = (RankMode)_RankMode;

                int RankingGet = RankingManager.getInstance().GetRankOfPlayer(RoleID, _Mode);

                string OUTDATA = RoleID + ":" + RankingGet;

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
        /// Ghi lại trạng thái đã nhận quà đua top cho thằng này
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="pool"></param>
        /// <param name="nID"></param>
        /// <param name="data"></param>
        /// <param name="count"></param>
        /// <param name="tcpOutPacket"></param>
        /// <returns></returns>
        public static TCPProcessCmdResults CMD_KT_UPDATE_REVICE_STATUS(DBManager dbMgr, TCPOutPacketPool pool, int nID, byte[] data, int count, out TCPOutPacket tcpOutPacket)
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
                if (fields.Length != 2)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("Error Socket params count not fit CMD={0}, Recv={1}, CmdData={2}",
                        (TCPGameServerCmds)nID, fields.Length, cmdData));

                    tcpOutPacket = TCPOutPacket.MakeTCPOutPacket(pool, "0", (int)TCPGameServerCmds.CMD_DB_ERR_RETURN);
                    return TCPProcessCmdResults.RESULT_DATA;
                }

                int RoleID = Int32.Parse(fields[0]);

                int RankType = Int32.Parse(fields[1]);

                int Status = -1;

                string SqlQuery = "Update t_ranking set RankingEventType" + RankType + "Status = 1 where rid = " + RoleID + "";

                if (DBWriter.ExecuteSqlScript(SqlQuery))
                {
                    RankMode _Mode = (RankMode)RankType;

                    List<PlayerRanking> RankList = RankServer[_Mode];

                    var find = RankList.Where(x => x.RoleID == RoleID).FirstOrDefault();
                    if (find != null)
                    {
                        find.Status = 1;
                    }
                    Status = 0;
                }

                string OUTDATA = RoleID + ":" + Status;

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
    }
}