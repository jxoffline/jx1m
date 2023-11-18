using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using KF.Contract.Data;
using GameServer.Core.Executor;
using Server.Tools;
using MySql.Data.MySqlClient;
using Maticsoft.DBUtility;

namespace KF.Remoting
{
    /// <summary>
    /// 祝福记录管理
    /// </summary>
    class CoupleWishRecordManager
    {
        private CoupleWishPersistence Persistence = CoupleWishPersistence.getInstance();

        private object Mutex = new object();
        private Dictionary<int, Queue<CoupleWishWishRecordData>> RoleWishRecords = new Dictionary<int, Queue<CoupleWishWishRecordData>>();
        private Dictionary<int, long> RoleLastReadMs = new Dictionary<int, long>();
        private int ThisWeek = 0;

        public CoupleWishRecordManager(int week)
        {
            UpdateWeek(week);
        }

        /// <summary>
        /// 增加祝福记录
        /// </summary>
        /// <param name="req"></param>
        public void AddWishRecord(KuaFuRoleMiniData from, int wishType, string wishTxt, int toDbCoupleId, KuaFuRoleMiniData toMan, KuaFuRoleMiniData toWife)
        {
            lock (Mutex)
            {
                if (!Persistence.SaveWishRecord(ThisWeek, from, wishType, wishTxt,toDbCoupleId, toMan, toWife))
                    return;

                AddCachedWishOther(from, wishType, wishTxt, toMan, toWife);
                AddCachedBeWished(toMan, toWife, wishType, wishTxt, from);
                AddCachedBeWished(toWife, toMan, wishType, wishTxt, from);
            }      
        }

        /// <summary>
        /// 查询祝福记录
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public List<CoupleWishWishRecordData> GetWishRecord(int roleId)
        {
            List<CoupleWishWishRecordData> result = null;
            MySqlDataReader sdr = null;
            try
            {
                lock (Mutex)
                {
                    Queue<CoupleWishWishRecordData> wishQ = null;
                    if (RoleWishRecords.TryGetValue(roleId, out wishQ))
                        result = wishQ.ToList();

                    if (result == null)
                    {
                        string sql = string.Format(
                            "SELECT `from_rid`,`from_zoneid`,`from_rname`,`to_man_rid`,`to_man_zoneid`,`to_man_rname`,`to_wife_rid`,`to_wife_zoneid`,`to_wife_rname`,`wish_type`,`wish_txt` "
                            + " FROM t_couple_wish_wish_log WHERE `week`={0} AND (`from_rid`={1} OR `to_man_rid`={1} OR `to_wife_rid`={1}) ORDER BY `time` LIMIT {2};", 
                            this.ThisWeek, roleId, CoupleWishConsts.MaxWishRecordNum);

                        sdr = DbHelperMySQL.ExecuteReader(sql);
                        RoleWishRecords[roleId] = wishQ = new Queue<CoupleWishWishRecordData>();

                        while (sdr != null && sdr.Read())
                        {
                            CoupleWishWishRoleReq req = new CoupleWishWishRoleReq();
                            req.From.RoleId = Convert.ToInt32(sdr["from_rid"]);
                            req.From.ZoneId = Convert.ToInt32(sdr["from_zoneid"]);
                            req.From.RoleName = sdr["from_rname"].ToString();

                            req.ToMan.RoleId = Convert.ToInt32(sdr["to_man_rid"]);
                            req.ToMan.ZoneId = Convert.ToInt32(sdr["to_man_zoneid"]);
                            req.ToMan.RoleName = sdr["to_man_rname"].ToString();

                            req.ToWife.RoleId = Convert.ToInt32(sdr["to_wife_rid"]);
                            req.ToWife.ZoneId = Convert.ToInt32(sdr["to_wife_zoneid"]);
                            req.ToWife.RoleName = sdr["to_wife_rname"].ToString();

                            req.WishType = Convert.ToInt32(sdr["wish_type"]);
                            req.WishTxt = sdr["wish_txt"].ToString();

                            if (req.From.RoleId == roleId) AddCachedWishOther(req.From, req.WishType, req.WishTxt, req.ToMan, req.ToWife);
                            if (req.ToMan.RoleId == roleId) AddCachedBeWished(req.ToMan, req.ToWife, req.WishType, req.WishTxt, req.From);
                            if (req.ToWife.RoleId == roleId) AddCachedBeWished(req.ToWife, req.ToMan, req.WishType, req.WishTxt, req.From);
                        }
                        result = wishQ.ToList();
                    }

                    RoleLastReadMs[roleId] = TimeUtil.NOW();
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
        /// 重置数据
        /// </summary>
        public void UpdateWeek(int week)
        {
            lock (Mutex)
            {
                if (this.ThisWeek != week)
                {
                    this.RoleWishRecords.Clear();
                    this.RoleLastReadMs.Clear();
                    this.ThisWeek = week;
                }
            }
        }

        /// <summary>
        /// 清除长时间处于未激活状态的数据
        /// </summary>
        public void ClearUnActiveRecord()
        {
            lock (Mutex)
            {
                long nowMs = TimeUtil.NOW();
                int timeOutMs = 30 * 60 * 1000; // 30分钟
                List<int> rmList = RoleLastReadMs.Keys.ToList().FindAll(_r => RoleLastReadMs.ContainsKey(_r) && nowMs - RoleLastReadMs[_r] >= timeOutMs);
                if (rmList == null) return;

                foreach (var r in rmList)
                {
                    RoleLastReadMs.Remove(r);
                    RoleWishRecords.Remove(r);
                }
            }   
        }

        /// <summary>
        /// 增加缓存的祝福记录
        /// </summary>
        /// <param name="from"></param>
        /// <param name="wishType"></param>
        /// <param name="wishTxt"></param>
        /// <param name="toMan"></param>
        /// <param name="toWife"></param>
        private void AddCachedWishOther(KuaFuRoleMiniData from, int wishType, string wishTxt, KuaFuRoleMiniData toMan, KuaFuRoleMiniData toWife)
        {
            lock (Mutex)
            {
                Queue<CoupleWishWishRecordData> fromRecords = null;
                if (RoleWishRecords.TryGetValue(from.RoleId, out fromRecords))
                {
                    var rec = new CoupleWishWishRecordData();
                    rec.FromRole = from;
                    rec.TargetRoles = new List<KuaFuRoleMiniData>();
                    if (toMan.RoleId != from.RoleId) rec.TargetRoles.Add(toMan);
                    if (toWife.RoleId != from.RoleId) rec.TargetRoles.Add(toWife);
                    rec.WishType = wishType;
                    rec.WishTxt = wishTxt;

                    fromRecords.Enqueue(rec);
                    while (fromRecords.Count > CoupleWishConsts.MaxWishRecordNum)
                        fromRecords.Dequeue();
                }
            }       
        }

        /// <summary>
        /// 增加缓存的被祝福记录
        /// </summary>
        /// <param name="from"></param>
        /// <param name="wishType"></param>
        /// <param name="wishTxt"></param>
        /// <param name="toMan"></param>
        /// <param name="toWife"></param>
        private void AddCachedBeWished(KuaFuRoleMiniData to, KuaFuRoleMiniData toSpouse, int wishType, string wishTxt, KuaFuRoleMiniData from)
        {
            lock (Mutex)
            {
                if (to.RoleId == from.RoleId) return;

                Queue<CoupleWishWishRecordData> toRecords = null;
                if (RoleWishRecords.TryGetValue(to.RoleId, out toRecords))
                {
                    var rec = new CoupleWishWishRecordData();
                    rec.FromRole = from;
                    rec.TargetRoles = new List<KuaFuRoleMiniData>();
                    rec.TargetRoles.Add(to);
                    if (toSpouse.RoleId != from.RoleId) rec.TargetRoles.Add(toSpouse);
                    rec.WishType = wishType;
                    rec.WishTxt = wishTxt;

                    toRecords.Enqueue(rec);
                    while (toRecords.Count > CoupleWishConsts.MaxWishRecordNum)
                        toRecords.Dequeue();
                }
            }        
        }
    }
}
