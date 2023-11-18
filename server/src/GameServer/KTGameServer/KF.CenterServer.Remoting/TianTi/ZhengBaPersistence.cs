using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using KF.Remoting.Data;
using Maticsoft.DBUtility;
using KF.Contract.Data;
using Server.Tools;
using GameServer.Core.Executor;

namespace KF.Remoting
{
    public class ZhengBaPersistence
    {
        #region Singleton
        private ZhengBaPersistence() { }
        public static readonly ZhengBaPersistence Instance = new ZhengBaPersistence();
        #endregion

        public delegate void FirstCreateDbRank(int selectRoleIfNewCreate);
        public FirstCreateDbRank MonthRankFirstCreate = null;

        public ZhengBaSyncData LoadZhengBaSyncData(DateTime now, int selectRoleIfNewCreate, long dayBeginTicks)
        {
            ZhengBaSyncData syncData = new ZhengBaSyncData();
            syncData.Month = ZhengBaUtils.MakeMonth(now);
            syncData.IsThisMonthInActivity = CheckThisMonthInActivity(now, dayBeginTicks);
    
            bool bMonthFirst = false;
            if (syncData.IsThisMonthInActivity)
            {
                bMonthFirst = CheckBuildZhengBaRank(selectRoleIfNewCreate, syncData.Month);
                syncData.RoleList = LoadZhengBaRankData(syncData.Month);
                syncData.SupportList = LoadZhengBaSupportData(syncData.Month);
            }
            else
            {
                bMonthFirst = false;
                syncData.RoleList = new List<ZhengBaRoleInfoData>();
                syncData.SupportList = new List<ZhengBaSupportAnalysisData>();
            }
                
            syncData.RoleModTime = now;
            syncData.SupportModTime = now;

            // 本月的参赛人数第一次创建到db中，那么广播公告和邮件通知
            if (bMonthFirst && MonthRankFirstCreate != null)
            {
                MonthRankFirstCreate(selectRoleIfNewCreate);
            }

            return syncData;
        }

        public bool SaveSupportLog(ZhengBaSupportLogData data)
        {
            if (data == null) return false;

            try
            {
                string sql = string.Format("INSERT INTO t_zhengba_support_log(month,from_rid,from_zoneid,from_rolename,support_type,to_union_group,to_group,`time`,rank_of_day,from_serverid) "
                    + "VALUES({0},{1},{2},'{3}',{4},{5},{6},'{7}',{8},{9});",
                    ZhengBaUtils.MakeMonth(data.Time) , data.FromRoleId, data.FromZoneId, data.FromRolename, 
                    (int)data.SupportType, data.ToUnionGroup, data.ToGroup, data.Time.ToString("yyyy-MM-dd HH:mm:ss"),
                    data.RankOfDay, data.FromServerId);

                if (DbHelperMySQL.ExecuteSql(sql) <= 0)
                    return false;
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Error, "SaveSupportLog failed!", ex);
                return false;
            }

            return true;
        }

        public bool SavePkLog(ZhengBaPkLogData log)
        {
            if (log == null) return false;

            try
            {
                string sql = string.Format("INSERT INTO t_zhengba_pk_log(month,day,rid1,zoneid1,rname1,ismirror1,rid2,zoneid2,rname2,ismirror2,result,upgrade,starttime,endtime) "
                  + "VALUES({0},{1},{2},{3},'{4}',{5},{6},{7},'{8}',{9},{10},{11},'{12}','{13}');",
                  log.Month, log.Day,
                  log.RoleID1, log.ZoneID1, log.RoleName1, log.IsMirror1 ? 1 : 0, 
                  log.RoleID2, log.ZoneID2, log.RoleName2, log.IsMirror2 ? 1 : 0,
                  (int)log.PkResult, log.UpGrade ? 1 : 0,
                  log.StartTime.ToString("yyyy-MM-dd HH:mm:ss"), log.EndTime.ToString("yyyy-MM-dd HH:mm:ss"));

                if (DbHelperMySQL.ExecuteSql(sql) <= 0)
                    return false;
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Error, "SavePkLog failed!", ex);
                return false;
            }

            return true;
        }

        public bool UpdateRole(int month, int rid, int grade, int state, int group)
        {
            try
            {
                string sql = string.Format("UPDATE t_zhengba_roles SET grade={0},`group`={1},state={2} WHERE month={3} AND rid={4}",
                    grade, group, state, month, rid);

                if (DbHelperMySQL.ExecuteSql(sql) <= 0)
                    return false;
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Error, "UpdateRole failed!", ex);
                return false;
            }

            return true;
        }

        /// <summary>
        /// 检测是否本月是否可以举行活动，是为了考虑到该版本的更新问题
        /// 例如活动每月10号开启，但是这个版本是15号更新上去的，那么虽然有天数推迟功能，那么这个活动举行么？
        /// 策划的答复： 1-9号, 以及10号15:30分之前的版本更新 会正常开启众神争霸
        /// </summary>
        /// <param name="now"></param>
        /// <param name="dayBeginTicks"></param>
        /// <returns></returns>
        private bool CheckThisMonthInActivity(DateTime now, long dayBeginTicks)
        {
            try
            {
                // 这个字段记录当前存储的争霸信息是哪个月的，默认插入201111(2011年11月)
                DbHelperMySQL.ExecuteSql(
                    string.Format("INSERT IGNORE INTO t_async(`id`,`value`) VALUES({0},{1});",
                        AsyncTypes.ZhengBaCurrMonth, ZhengBaConsts.DefaultAsyncMonth)
                );

                // 取出来记录的月份
                int oldMonth = (int)DbHelperMySQL.GetSingle("select value from t_async where id = " + AsyncTypes.ZhengBaCurrMonth);     
                if (oldMonth == ZhengBaConsts.DefaultAsyncMonth)
                {
                    // 如果当前记录的月份是默认值201111，那么说明还没有举行过一次众神争霸，有可能是第一次升级该版本，所以检测本月是否能够开启活动

                    if (now.Day > ZhengBaConsts.StartMonthDay) return false; // 第一次启动是在10号之后, 那么本月不举行
                    else if (now.Day < ZhengBaConsts.StartMonthDay) return true; // 第一次启动是在10号之前, 那么本月举行
                    else return now.TimeOfDay.Ticks < dayBeginTicks; // 第一次启动是10号，那么检测当前时间是否在15:30之前
                }
                else
                {
                    // 不是默认值，说明已经举行过了
                    return true;
                }         
            }
            catch (Exception ex)
            {
                LogManager.WriteExceptionUseCache(ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// 检测生成众生争霸排行榜
        /// </summary>
        /// <param name="now"></param>
        private bool CheckBuildZhengBaRank(int selectRoleIfNewCreate, int nowMonth)
        {
            bool bMonthFirst = false;
            try
            {
                DbHelperMySQL.ExecuteSql(
                    string.Format("INSERT IGNORE INTO t_async(`id`,`value`) VALUES({0},{1});",
                        AsyncTypes.ZhengBaCurrMonth, ZhengBaConsts.DefaultAsyncMonth)
                    );

                // 防止人为的重新reload月排行榜导致众神争霸战斗信息丢失
                /* 这里有一些细节要注意：
                 * 1：每月1号凌晨3点更新月排行榜，然后触发更新争霸角色
                 * 2: 服务器启动的时候，尝试生成争霸角色
                 * 3：要防止当月的争霸角色重复生成
                 * 4：必须保证天梯排行角色已经生成，才能生成争霸角色 -->例如，1号凌晨两点启动，必须保证等到3点的时候，天梯月排行榜生成之后，能够生成本月的争霸角色
                 */
                int oldMonth = (int)DbHelperMySQL.GetSingle("select value from t_async where id = " + AsyncTypes.ZhengBaCurrMonth);
                object ageObj_tiantiMonth = DbHelperMySQL.GetSingle("select value from t_async where id = " + AsyncTypes.TianTiPaiHangModifyOffsetDay);
                if (oldMonth != nowMonth 
                    && ageObj_tiantiMonth != null 
                    && ZhengBaUtils.MakeMonth(DataHelper2.GetRealDate((int)ageObj_tiantiMonth)) == nowMonth)
                {
                    string strSql = string.Format("SELECT rid,rname,zoneid,duanweiid,duanweijifen,duanweirank,zhanli,data1,data2 FROM t_tianti_month_paihang ORDER BY duanweirank ASC LIMIT {0};", selectRoleIfNewCreate);
                    var sdr = DbHelperMySQL.ExecuteReader(strSql);
                    while (sdr != null && sdr.Read())
                    {
                        ZhengBaRoleInfoData roleData = new ZhengBaRoleInfoData();
                        roleData.RoleId = (int)Convert.ToInt32(sdr["rid"]);
                        roleData.ZoneId = (int)Convert.ToInt32(sdr["zoneid"]);
                        roleData.DuanWeiId = (int)Convert.ToInt32(sdr["duanweiid"]);
                        roleData.DuanWeiJiFen = (int)Convert.ToInt32(sdr["duanweijifen"]);
                        roleData.DuanWeiRank = (int)Convert.ToInt32(sdr["duanweirank"]);
                        roleData.ZhanLi = (int)Convert.ToInt32(sdr["zhanli"]);
                        roleData.RoleName = sdr["rname"].ToString();
                        if (!sdr.IsDBNull(sdr.GetOrdinal("data1"))) roleData.TianTiPaiHangRoleData = (byte[])(sdr["data1"]);
                        if (!sdr.IsDBNull(sdr.GetOrdinal("data2"))) roleData.PlayerJingJiMirrorData = (byte[])(sdr["data2"]);
                        if (string.IsNullOrEmpty(roleData.RoleName) && roleData.TianTiPaiHangRoleData != null)
                        {
                            var onlyName = DataHelper2.BytesToObject<TianTiPaiHangRoleData_OnlyName>(
                                roleData.TianTiPaiHangRoleData, 0, roleData.TianTiPaiHangRoleData.Length);
                            if (onlyName != null)
                            {
                                roleData.RoleName = onlyName.RoleName;
                            }
                        }

                        string repSql = string.Format(
                            "REPLACE INTO t_zhengba_roles(`month`,rid,zoneid,duanweiid,duanweijifen,duanweirank,zhanli,`grade`,`group`,state,rname,data1,data2) "
                            + "VALUES({0},{1},{2},{3},{4},{5},{6},{7},{8},{9},'{10}',@content,@mirror)",
                            nowMonth, roleData.RoleId, roleData.ZoneId, roleData.DuanWeiId,
                            roleData.DuanWeiJiFen, roleData.DuanWeiRank, roleData.ZhanLi,
                            (int)EZhengBaGrade.Grade100, (int)ZhengBaConsts.NoneGroup, (int)EZhengBaState.None, roleData.RoleName);

                        DbHelperMySQL.ExecuteSqlInsertImg(repSql,
                            new List<Tuple<string, byte[]>>{
                                new Tuple<string, byte[]>("content", roleData.TianTiPaiHangRoleData),
                                new Tuple<string, byte[]>("mirror", roleData.PlayerJingJiMirrorData)
                            });
                    }
                    if (sdr != null)
                    {
                        sdr.Close();
                    }

                    DbHelperMySQL.ExecuteSql(
                        string.Format("REPLACE INTO t_async(`id`,`value`) VALUES({0},{1});",
                            AsyncTypes.ZhengBaCurrMonth, nowMonth)
                        );

                    bMonthFirst = true;
                }
            }
            catch (Exception ex)
            {
                LogManager.WriteExceptionUseCache(ex.ToString());
            }

            return bMonthFirst;
        }

        private List<ZhengBaRoleInfoData> LoadZhengBaRankData(int nowMonth)
        {
            List<ZhengBaRoleInfoData> roleList = new List<ZhengBaRoleInfoData>();
            try
            {
                string strSql = string.Format("SELECT rid,rname,zoneid,duanweiid,duanweijifen,duanweirank,zhanli,grade,`group`,state,data1,data2 FROM t_zhengba_roles where `month`={0} ORDER BY duanweirank ASC;",
                    nowMonth);
                var sdr = DbHelperMySQL.ExecuteReader(strSql);
                while (sdr != null && sdr.Read())
                {
                    ZhengBaRoleInfoData roleData = new ZhengBaRoleInfoData();
                    roleData.RoleId = (int)Convert.ToInt32(sdr["rid"]);
                    roleData.ZoneId = (int)Convert.ToInt32(sdr["zoneid"]);
                    roleData.DuanWeiId = (int)Convert.ToInt32(sdr["duanweiid"]);
                    roleData.DuanWeiJiFen = (int)Convert.ToInt32(sdr["duanweijifen"]);
                    roleData.DuanWeiRank = (int)Convert.ToInt32(sdr["duanweirank"]);
                    roleData.ZhanLi = (int)Convert.ToInt32(sdr["zhanli"]);
                    if (!sdr.IsDBNull(sdr.GetOrdinal("data1"))) roleData.TianTiPaiHangRoleData = (byte[])(sdr["data1"]);
                    if (!sdr.IsDBNull(sdr.GetOrdinal("data2"))) roleData.PlayerJingJiMirrorData = (byte[])(sdr["data2"]);

                    roleData.Grade = (int)Convert.ToInt32(sdr["grade"]);
                    roleData.Group = (int)Convert.ToInt32(sdr["group"]);
                    roleData.State = (int)Convert.ToInt32(sdr["state"]);
                    roleData.RoleName = sdr["rname"].ToString();

                    roleList.Add(roleData);
                }

                if (sdr != null)
                {
                    sdr.Close();
                }
            }
            catch (Exception ex)
            {
                LogManager.WriteExceptionUseCache(ex.ToString());
            }

            return roleList;
        }

        private List<ZhengBaSupportAnalysisData> LoadZhengBaSupportData(int nowMonth)
        {
            List<ZhengBaSupportAnalysisData> supportList = new List<ZhengBaSupportAnalysisData>();
            try
            {
                string strSql = string.Format("SELECT support_type,rank_of_day,to_union_group,to_group FROM t_zhengba_support_log where `month`={0}", nowMonth);
                var sdr = DbHelperMySQL.ExecuteReader(strSql);
                while (sdr != null && sdr.Read())
                {
                    int supportType = (int)Convert.ToInt32(sdr["support_type"]);
                    int toUnionGroup = (int)Convert.ToInt32(sdr["to_union_group"]);
                    int toGroup = (int)Convert.ToInt32(sdr["to_group"]);
                    int rankOfDay = (int)Convert.ToInt32(sdr["rank_of_day"]);
                    ZhengBaSupportAnalysisData support = null;
                    if ((support = supportList.Find(_s => _s.UnionGroup == toUnionGroup && _s.Group == toGroup)) == null)
                    {
                        support = new ZhengBaSupportAnalysisData() {UnionGroup = toUnionGroup, Group = toGroup, RankOfDay = rankOfDay };
                        supportList.Add(support);
                    }

                    if (supportType == (int)EZhengBaSupport.Support) support.TotalSupport++;
                    else if (supportType == (int)EZhengBaSupport.Oppose) support.TotalOppose++;
                    else if (supportType == (int)EZhengBaSupport.YaZhu) support.TotalYaZhu++;
                }

                if (sdr != null)
                {
                    sdr.Close();
                }
            }
            catch (Exception ex)
            {
                LogManager.WriteExceptionUseCache(ex.ToString());
            }

            return supportList;
        }
    }
}
