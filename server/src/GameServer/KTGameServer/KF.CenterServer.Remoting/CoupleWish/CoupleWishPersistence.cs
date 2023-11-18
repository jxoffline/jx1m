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
    class CoupleWishPersistence
    {
        #region Singleton
        private static CoupleWishPersistence _Instance = new CoupleWishPersistence();
        private CoupleWishPersistence() { }
        public static CoupleWishPersistence getInstance() { return _Instance; }
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
                string sql = "SELECT IFNULL(MAX(couple_id),0) couple_id FROM t_couple_wish_group;";
                CurrDbCoupleId = (int)((long)DbHelperMySQL.GetSingle(sql));
            }

            CurrDbCoupleId++;
            return CurrDbCoupleId;
        }

        /// <summary>
        /// 只写入基本的排行，祝福数量到数据库
        /// </summary>
        /// <param name="list"></param>
        public void UpdateRand2Db(List<CoupleWishCoupleDataK> list)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < list.Count; i++)
            {
                sb.AppendFormat("UPDATE t_couple_wish_group SET `be_wish_num`={0},`rank`={1} WHERE `couple_id`={2};", list[i].BeWishedNum, list[i].Rank, list[i].DbCoupleId);
                sb.AppendLine();

                if ((i % 50 == 0 || i == list.Count - 1) && sb.Length > 0)
                {
                    try
                    {
                        string sql = sb.ToString();
                        sb.Clear();
                        DbHelperMySQL.ExecuteSql(sql);
                    }
                    catch (Exception ex)
                    {
                        LogManager.WriteException("CoupleWishPersistence.UpdateRank2Db " + ex.Message);
                    }
                }
            }
        }

        /// <summary>
        /// 从数据库加载排行榜
        /// </summary>
        /// <returns></returns>
        public List<CoupleWishCoupleDataK> LoadRankFromDb(int week)
        {
            MySqlDataReader sdr = null;
            List<CoupleWishCoupleDataK> result = new List<CoupleWishCoupleDataK>();
            try
            {
                string sql = string.Format("SELECT `couple_id`,`man_rid`,`man_zoneid`,`man_rname`,`man_selector`,`wife_rid`,`wife_zoneid`,`wife_rname`,`wife_selector`,`be_wish_num`,`rank` " +
                    "FROM t_couple_wish_group WHERE `week`={0} ORDER BY `rank`;", week);
                sdr = DbHelperMySQL.ExecuteReader(sql);
                while (sdr != null && sdr.Read())
                {
                    CoupleWishCoupleDataK data = new CoupleWishCoupleDataK();
                    data.DbCoupleId = Convert.ToInt32(sdr["couple_id"]);

                    data.Man = new KuaFuRoleMiniData();
                    data.Man.RoleId = Convert.ToInt32(sdr["man_rid"]);
                    data.Man.ZoneId = Convert.ToInt32(sdr["man_zoneid"]);
                    data.Man.RoleName = sdr["man_rname"].ToString();
                    if (!sdr.IsDBNull(sdr.GetOrdinal("man_selector")))
                        data.ManSelector = (byte[])(sdr["man_selector"]);

                    data.Wife = new KuaFuRoleMiniData();
                    data.Wife.RoleId = Convert.ToInt32(sdr["wife_rid"]);
                    data.Wife.ZoneId = Convert.ToInt32(sdr["wife_zoneid"]);
                    data.Wife.RoleName = sdr["wife_rname"].ToString();
                    if (!sdr.IsDBNull(sdr.GetOrdinal("wife_selector")))
                        data.WifeSelector = (byte[])(sdr["wife_selector"]);

                    data.BeWishedNum = Convert.ToInt32(sdr["be_wish_num"]);
                    data.Rank = Convert.ToInt32(sdr["rank"]);

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
                string sql = string.Format("DELETE FROM t_couple_wish_group WHERE `couple_id`={0}", db_coupleid);
                DbHelperMySQL.ExecuteSql(sql);
                return true;
            }
            catch (Exception ex)
            {
                LogManager.WriteException(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 存储祝福记录
        /// </summary>
        /// <param name="wishWeek"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        public bool SaveWishRecord(int wishWeek, KuaFuRoleMiniData from, int wishType, string wishTxt,int toDbCoupleId, KuaFuRoleMiniData toMan, KuaFuRoleMiniData toWife)
        {
            try
            {
                string sql = string.Format("INSERT INTO t_couple_wish_wish_log(`week`,`from_rid`,`from_zoneid`,`from_rname`,`to_couple_id`,`to_man_rid`,`to_man_zoneid`,`to_man_rname`,`to_wife_rid`,`to_wife_zoneid`,`to_wife_rname`,`time`,`wish_txt`,`wish_type`) "+
                    "VALUES({0},{1},{2},'{3}',{4},{5},{6},'{7}',{8},{9},'{10}','{11}','{12}',{13});", wishWeek, from.RoleId, from.ZoneId, from.RoleName,  toDbCoupleId, 
                    toMan.RoleId, toMan.ZoneId, toMan.RoleName,toWife.RoleId, toWife.ZoneId, toWife.RoleName, TimeUtil.NowDateTime().ToString("yyyy-MM-dd HH:mm:ss"), wishTxt,wishType);
                DbHelperMySQL.ExecuteSql(sql);

                return true;
            }
            catch (Exception ex)
            {
                LogManager.WriteException(ex.Message);

                return false;
            }
        }

        /// <summary>
        /// 写入coupple data的详细信息
        /// </summary>
        /// <param name="week"></param>
        /// <param name="coupleData"></param>
        /// <returns></returns>
        public bool WriteCoupleData(int week, CoupleWishCoupleDataK coupleData)
        {
            try
            {
                string sql = string.Format(
                            "INSERT INTO t_couple_wish_group(`couple_id`,`man_rid`,`man_zoneid`,`man_rname`,`wife_rid`,`wife_zoneid`,`wife_rname`,`be_wish_num`,`rank`,`week`,`man_selector`,`wife_selector`) "
                            + " VALUES({0},{1},{2},'{3}',{4},{5},'{6}',{7},{8},{9},@man_selector,@wife_selector) "
                            + " ON DUPLICATE KEY UPDATE `man_rname`='{3}',`wife_rname`='{6}',`be_wish_num`={7},`rank`={8},`man_selector`=@man_selector,`wife_selector`=@wife_selector;",
                            coupleData.DbCoupleId, coupleData.Man.RoleId, coupleData.Man.ZoneId, coupleData.Man.RoleName,
                            coupleData.Wife.RoleId, coupleData.Wife.ZoneId, coupleData.Wife.RoleName, coupleData.BeWishedNum, coupleData.Rank, week);

                DbHelperMySQL.ExecuteSqlInsertImg(sql,
                           new List<Tuple<string, byte[]>>{
                                new Tuple<string, byte[]>("man_selector", coupleData.ManSelector),
                                new Tuple<string, byte[]>("wife_selector", coupleData.WifeSelector),
                            });

                return true;
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Error,
                    string.Format("CoupleWish FlushRandList2Db failed, couple_id={0},man={1},wife={2}", coupleData.DbCoupleId, coupleData.Man.RoleId, coupleData.Wife.RoleId),
                    ex);
                return false;
            }
        }

        /// <summary>
        /// 增加膜拜记录
        /// </summary>
        /// <param name="fromRole"></param>
        /// <param name="fromZone"></param>
        /// <param name="admireType"></param>
        /// <param name="toCoupleId"></param>
        public void AddAdmireLog(int fromRole, int fromZone, int admireType, int toCoupleId, int week)
        {
            try
            {
                string sql = string.Format("INSERT INTO t_couple_wish_admire_log(`from_rid`,`from_zoneid`,`admire_type`,`to_couple_id`,`week`,`time`) " +
                    "VALUES({0},{1},{2},{3},{4},'{5}');", fromRole, fromZone, admireType, toCoupleId, week, TimeUtil.NowDateTime().ToString("yyyy-MM-dd HH:mm:ss"));
                DbHelperMySQL.ExecuteSql(sql);
            }
            catch (Exception ex)
            {
                LogManager.WriteException(ex.Message);
            }
        }

        /// <summary>
        /// 加载雕像数据
        /// </summary>
        /// <param name="week"></param>
        /// <returns></returns>
        public CoupleWishSyncStatueData LoadCoupleStatue(int week)
        {
            CoupleWishSyncStatueData data = new CoupleWishSyncStatueData();
            data.ModifyTime = TimeUtil.NowDateTime();
            MySqlDataReader sdr = null;

            try
            {
                string sql = string.Format("SELECT `couple_id`,`man_rid`,`man_zoneid`,`man_rname`,`man_statue`,`wife_rid`,`wife_zoneid`,`wife_rname`,`wife_statue`,`admire_cnt`,`is_divorced`,`yanhui_join_num` " +
                    "FROM t_couple_wish_statue WHERE `week`={0};", week);
                sdr = DbHelperMySQL.ExecuteReader(sql);
                while (sdr != null && sdr.Read())
                {
                    data.DbCoupleId = Convert.ToInt32(sdr["couple_id"]);

                    data.Man = new KuaFuRoleMiniData();
                    data.Man.RoleId = Convert.ToInt32(sdr["man_rid"]);
                    data.Man.ZoneId = Convert.ToInt32(sdr["man_zoneid"]);
                    data.Man.RoleName = sdr["man_rname"].ToString();
                    if (!sdr.IsDBNull(sdr.GetOrdinal("man_statue")))
                        data.ManRoleDataEx = (byte[])(sdr["man_statue"]);

                    data.Wife = new KuaFuRoleMiniData();
                    data.Wife.RoleId = Convert.ToInt32(sdr["wife_rid"]);
                    data.Wife.ZoneId = Convert.ToInt32(sdr["wife_zoneid"]);
                    data.Wife.RoleName = sdr["wife_rname"].ToString();
                    if (!sdr.IsDBNull(sdr.GetOrdinal("wife_statue")))
                        data.WifeRoleDataEx = (byte[])(sdr["wife_statue"]);

                    data.BeAdmireCount = Convert.ToInt32(sdr["admire_cnt"]);
                    data.IsDivorced = Convert.ToInt32(sdr["is_divorced"]);
                    data.YanHuiJoinNum = Convert.ToInt32(sdr["yanhui_join_num"]);
                    data.Week = week;

                    break;
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

            return data;
        }

        /// <summary>
        /// 写入雕像数据
        /// </summary>
        /// <param name="statue"></param>
        /// <returns></returns>
        public bool WriteStatueData(CoupleWishSyncStatueData statue)
        {
            try
            {
                string sql = string.Format(
                            "INSERT INTO t_couple_wish_statue(`couple_id`,`man_rid`,`man_zoneid`,`man_rname`,`wife_rid`,`wife_zoneid`,`wife_rname`,`admire_cnt`,`is_divorced`,`week`,`yanhui_join_num`,`man_statue`,`wife_statue`) "
                            + " VALUES({0},{1},{2},'{3}',{4},{5},'{6}',{7},{8},{9},{10},@man_statue,@wife_statue) "
                            + " ON DUPLICATE KEY UPDATE `admire_cnt`={7},`is_divorced`={8},`yanhui_join_num`={10},`man_statue`=@man_statue,`wife_statue`=@wife_statue;",
                            statue.DbCoupleId, statue.Man.RoleId, statue.Man.ZoneId, statue.Man.RoleName, statue.Wife.RoleId, statue.Wife.ZoneId, statue.Wife.RoleName,
                            statue.BeAdmireCount, statue.IsDivorced, statue.Week, statue.YanHuiJoinNum);

                DbHelperMySQL.ExecuteSqlInsertImg(sql,
                           new List<Tuple<string, byte[]>>{
                                new Tuple<string, byte[]>("man_statue", statue.ManRoleDataEx),
                                new Tuple<string, byte[]>("wife_statue", statue.WifeRoleDataEx),
                            });

                return true;
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Error,
                    string.Format("CoupleWish WriteStatueData failed, couple_id={0},man={1},wife={2}", statue.DbCoupleId, statue.Man.RoleId, statue.Wife.RoleId),
                    ex);
                return false;
            }
        }

        /// <summary>
        /// 增加宴会参加记录
        /// </summary>
        /// <param name="fromRole"></param>
        /// <param name="fromZone"></param>
        /// <param name="toCoupleId"></param>
        public void AddYanHuiJoinLog(int fromRole, int fromZone, int toCoupleId, int week)
        {
            try
            {
                string sql = string.Format("INSERT INTO t_couple_wish_join_yanhui_log(`from_rid`,`from_zoneid`,`to_couple_id`,`week`,`time`) " +
                    "VALUES({0},{1},{2},{3},'{4}');", fromRole, fromZone, toCoupleId, week, TimeUtil.NowDateTime().ToString("yyyy-MM-dd HH:mm:ss"));
                DbHelperMySQL.ExecuteSql(sql);
            }
            catch (Exception ex)
            {
                LogManager.WriteException(ex.Message);
            }
        }
    }
}
