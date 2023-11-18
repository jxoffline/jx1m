using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Maticsoft.DBUtility;
using KF.Remoting.Data;
using Server.Tools;
using KF.Contract.Data;
using MySql.Data.MySqlClient;
using GameServer.Core.Executor;

namespace KF.Remoting
{
    /// <summary>
    /// 情侣竞技持久化管理
    /// </summary>
    class CoupleArenaPersistence
    {
        #region Singleton
        private static CoupleArenaPersistence _Instance = new CoupleArenaPersistence();
        private CoupleArenaPersistence() { }
        public static CoupleArenaPersistence getInstance() { return _Instance; }
        #endregion

        private int CurrDbCoupleId = Global.UninitGameId;
        private long CurrGameId = Global.UninitGameId;

        /// <summary>
        /// 获取数据库当前自增的db_couple_id
        /// </summary>
        /// <returns></returns>
        public int GetNextDbCoupleId()
        {
            if (CurrDbCoupleId == Global.UninitGameId)
            {
                string sql = "SELECT IFNULL(MAX(couple_id),0) couple_id  FROM(" +
                "SELECT MAX(couple_id) couple_id FROM t_couple_arena_group UNION " +
                "SELECT MAX(couple_id) couple_id FROM t_couple_arena_rank_history) couple_ids;";

                CurrDbCoupleId = (int)((long)DbHelperMySQL.GetSingle(sql));
            }

            CurrDbCoupleId++;
            return CurrDbCoupleId;
        }

        public long GetNextGameId()
        {
            if (CurrGameId == Global.UninitGameId)
            {
                string sql = "SELECT IFNULL(MAX(id),0) id FROM t_couple_arena_pk_log;";
                CurrGameId = ((long)DbHelperMySQL.GetSingle(sql));
            }

            CurrGameId++;
            return CurrGameId;
        }

        /// <summary>
        /// 检测是否重置本周排行榜
        /// </summary>
        /// <param name="currWeek"></param>
        /// <returns></returns>
        public bool CheckClearRank(int currWeek)
        {
            try
            {
                DbHelperMySQL.ExecuteSql(
                    string.Format("INSERT IGNORE INTO t_async(`id`,`value`) VALUES({0},{1});",
                        AsyncTypes.CoupleAreanCurrPaiHangWeek, 20151230)
                    );

                int oldWeek = (int)DbHelperMySQL.GetSingle("select value from t_async where id = " + AsyncTypes.CoupleAreanCurrPaiHangWeek);
                if (oldWeek == currWeek)
                {
                    return false;
                }

                string rankBakSql = "INSERT INTO t_couple_arena_rank_history(`couple_id`,`man_rid`,`man_zoneid`,`wife_rid`,`wife_zoneid`,`total_pk_times`,`total_win_times`,`liansheng`,`jifen`,`duanwei_type`,`duanwei_level`,`rank`,`is_divorced`) " +
                                "(SELECT `couple_id`,`man_rid`,`man_zoneid`,`wife_rid`,`wife_zoneid`,`total_pk_times`,`total_win_times`,`liansheng`,`jifen`,`duanwei_type`,`duanwei_level`,`rank`,`is_divorced` FROM t_couple_arena_group);" +
                                string.Format("UPDATE t_couple_arena_rank_history SET `week`={0} WHERE `week`=0;", oldWeek);
                string clearSql = "DELETE FROM t_couple_arena_group;";
                string setFlagSql = string.Format("REPLACE INTO t_async(`id`,`value`) VALUES({0},{1});", AsyncTypes.CoupleAreanCurrPaiHangWeek, currWeek);

                DbHelperMySQL.ExecuteSql(rankBakSql + clearSql + setFlagSql);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// 刷新排行榜到数据库
        /// </summary>
        /// <param name="list"></param>
        public void FlushRandList2Db(List<CoupleArenaCoupleDataK> list)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < list.Count; i++)
            {
                sb.AppendFormat("UPDATE t_couple_arena_group SET `total_pk_times`={0},`total_win_times`={1},`liansheng`={2},`jifen`={3},`duanwei_type`={4},`duanwei_level`={5},`rank`={6},`is_divorced`={7} WHERE `couple_id`={8};",
                    list[i].TotalFightTimes, list[i].WinFightTimes, list[i].LianShengTimes, list[i].JiFen, list[i].DuanWeiType, list[i].DuanWeiLevel, list[i].Rank, list[i].IsDivorced, list[i].Db_CoupleId);
                sb.AppendLine();

                if ((i % 50 == 0 || i == list.Count - 1) && sb.Length > 0)
                {
                    try
                    {
                        DbHelperMySQL.ExecuteSql(sb.ToString());
                    }
                    catch (Exception ex)
                    {
                        LogManager.WriteException( "CoupleArenaPersistence.FlushRandList2Db failed! " + ex.Message);
                    }

                    sb.Clear();
                }
            }
        }

        /// <summary>
        /// 从数据库加载排行榜
        /// </summary>
        /// <returns></returns>
        public List<CoupleArenaCoupleDataK> LoadRankFromDb()
        {
            MySqlDataReader sdr = null;
            List<CoupleArenaCoupleDataK> result = new List<CoupleArenaCoupleDataK>();
            try
            {
                string sql = "SELECT `couple_id`,`man_rid`,`man_zoneid`,`man_data1`,`wife_rid`,`wife_zoneid`,`wife_data1`,`total_pk_times`,`total_win_times`,`liansheng`,`jifen`,`duanwei_type`,`duanwei_level`,`rank`,`is_divorced` FROM t_couple_arena_group ORDER BY `rank`;";
                sdr = DbHelperMySQL.ExecuteReader(sql);
                while (sdr != null && sdr.Read())
                {
                    CoupleArenaCoupleDataK data = new CoupleArenaCoupleDataK();
                    data.Db_CoupleId = Convert.ToInt32(sdr["couple_id"]);
                    data.ManRoleId = Convert.ToInt32(sdr["man_rid"]);
                    data.ManZoneId = Convert.ToInt32(sdr["man_zoneid"]);
                    if (!sdr.IsDBNull(sdr.GetOrdinal("man_data1")))
                        data.ManSelectorData = (byte[])(sdr["man_data1"]);

                    data.WifeRoleId = Convert.ToInt32(sdr["wife_rid"]);
                    data.WifeZoneId = Convert.ToInt32(sdr["wife_zoneid"]);
                    if (!sdr.IsDBNull(sdr.GetOrdinal("wife_data1")))
                        data.WifeSelectorData = (byte[])(sdr["wife_data1"]);

                    data.TotalFightTimes = Convert.ToInt32(sdr["total_pk_times"]);
                    data.WinFightTimes = Convert.ToInt32(sdr["total_win_times"]);
                    data.LianShengTimes = Convert.ToInt32(sdr["liansheng"]);
                    data.DuanWeiType = Convert.ToInt32(sdr["duanwei_type"]);
                    data.DuanWeiLevel = Convert.ToInt32(sdr["duanwei_level"]);
                    data.JiFen = Convert.ToInt32(sdr["jifen"]);
                    data.Rank = Convert.ToInt32(sdr["rank"]);
                    data.IsDivorced = Convert.ToInt32(sdr["is_divorced"]);

                    result.Add(data);
                }
            }
            catch (Exception ex)
            {
                LogManager.WriteExceptionUseCache(ex.Message);
            }
            finally
            {
                if (sdr != null)
                {
                    sdr.Close();
                }
            }

            return result;
        }
        
        /// <summary>
        /// 清理情侣竞技数据
        /// </summary>
        /// <param name="db_coupleid"></param>
        /// <returns></returns>
        public bool ClearCoupleData(int db_coupleid)
        {
            try
            {
                string sql = string.Format("DELETE FROM t_couple_arena_group WHERE `couple_id`={0}", db_coupleid);
                DbHelperMySQL.ExecuteSql(sql);
                return true;
            }
            catch (Exception ex)
            {
                LogManager.WriteException(ex.Message);
                return false;
            }
        }

        public void AddPkLog(long gameId, DateTime startTime, DateTime endTime, int man1, int wife1, int result1, int man2, int wife2, int result2)
        {
            try
            {
                string sql = string.Format("INSERT INTO t_couple_arena_pk_log(`id`,`man_rid1`,`wife_rid1`,`result1`,`man_rid2`,`wife_rid2`,`result2`,`day`,`starttime`,`endtime`) "+
                    "VALUES({0},{1},{2},{3},{4},{5},{6},{7},'{8}','{9}');", gameId, man1, wife1, result1, man2,wife2, result2,
                    TimeUtil.MakeYearMonthDay(startTime), startTime.ToString("yyyy-MM-dd HH:mm:ss"), endTime.ToString("yyyy-MM-dd HH:mm:ss"));
                DbHelperMySQL.ExecuteSql(sql);
            }
            catch (Exception ex)
            {
                LogManager.WriteException(ex.Message);
            }
        }

        /// <summary>
        /// 写入竞技数据
        /// </summary>
        /// <param name="coupleData"></param>
        public void WriteCoupleData(CoupleArenaCoupleDataK coupleData)
        {
            if (coupleData == null) return;
            try
            {
                string sql = string.Format(
                            "INSERT INTO t_couple_arena_group(`couple_id`,`man_rid`,`man_zoneid`,`wife_rid`,`wife_zoneid`,`total_pk_times`,`total_win_times`,`liansheng`,`jifen`,`duanwei_type`,`duanwei_level`,`rank`,`is_divorced`,`man_data1`,`wife_data1`) "
                            + " VALUES({0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},@man_data1,@wife_data1) "
                            + " ON DUPLICATE KEY UPDATE `total_pk_times`={5},`total_win_times`={6},`liansheng`={7},`jifen`={8},`duanwei_type`={9},`duanwei_level`={10},`rank`={11},`is_divorced`={12},`man_data1`=@man_data1,`wife_data1`=@wife_data1;",
                            coupleData.Db_CoupleId, coupleData.ManRoleId, coupleData.ManZoneId, coupleData.WifeRoleId, coupleData.WifeZoneId, coupleData.TotalFightTimes, coupleData.WinFightTimes,
                            coupleData.LianShengTimes, coupleData.JiFen, coupleData.DuanWeiType, coupleData.DuanWeiLevel, coupleData.Rank, coupleData.IsDivorced);

                DbHelperMySQL.ExecuteSqlInsertImg(sql,
                           new List<Tuple<string, byte[]>>{
                                new Tuple<string, byte[]>("man_data1", coupleData.ManSelectorData),
                                new Tuple<string, byte[]>("wife_data1", coupleData.WifeSelectorData)
                            });
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Error,
                    string.Format("FlushRandList2Db failed, couple_id={0},man={1},wife={2}", coupleData.Db_CoupleId, coupleData.ManRoleId, coupleData.WifeRoleId),
                    ex);
            }
        }
    }
}
