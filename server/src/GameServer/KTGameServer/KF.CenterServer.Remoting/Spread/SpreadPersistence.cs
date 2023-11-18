using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KF.Contract.Data;
using System.Configuration;
using Maticsoft.DBUtility;
using KF.Remoting.Data;
using MySql.Data.MySqlClient;
using Tmsk.Tools;
using System.Collections.Concurrent;
using Tmsk.Contract.Const;
using Tmsk.Contract;
using Server.Tools;
using GameServer.Core.Executor;
using Tmsk.Tools.Tools;
using System.Xml.Linq;
using Tmsk.DbHelper;

namespace KF.Remoting
{
    public class SpreadPersistence
    {

        private SpreadPersistence() { }

        public static readonly SpreadPersistence Instance = new SpreadPersistence();


        /// <summary>
        /// 保护数据的互斥对象
        /// </summary>
        public object Mutex = new object();

        /// <summary>
        /// 是否已初始化
        /// </summary>
        private bool Initialized = false;

        /// <summary>
        /// 初始化配置文件
        /// </summary>
        public void InitConfig()
        {
            try
            {

                XElement xmlFile = ConfigHelper.Load("config.xml");

                Consts.TelMaxCount = (int)ConfigHelper.GetElementAttributeValueLong(xmlFile, "add", "key", "SpreadTelMaxCount", "value", 5);
                Consts.TelTimeLimit = (int)ConfigHelper.GetElementAttributeValueLong(xmlFile, "add", "key", "SpreadTelTimeLimit", "value", 20) * 60;
                Consts.TelTimeStop = (int)ConfigHelper.GetElementAttributeValueLong(xmlFile, "add", "key", "SpreadTelTimeStop", "value", 120) * 60;

                Consts.VerifyRoleMaxCount = (int)ConfigHelper.GetElementAttributeValueLong(xmlFile, "add", "key", "SpreadVerifyRoleMaxCount", "value", 5);
                Consts.VerifyRoleTimeLimit = (int)ConfigHelper.GetElementAttributeValueLong(xmlFile, "add", "key", "SpreadVerifyRoleTimeLimit", "value", 5) * 60;
                Consts.VerifyRoleTimeStop = (int)ConfigHelper.GetElementAttributeValueLong(xmlFile, "add", "key", "SpreadVerifyRoleTimeStop", "value", 30) * 60;

                Consts.IsTest = (int)ConfigHelper.GetElementAttributeValueLong(xmlFile, "add", "key", "IsTest", "value", 0);

                Initialized = true;
            }
            catch (System.Exception ex)
            {
                LogManager.WriteExceptionUseCache(ex.ToString());
            }
        }

        private int ExecuteSqlNoQuery2(string sqlCmd)
        {
            //写数据库
            int i = 0;
            try
            {
                i = DbHelperMySQL.ExecuteSql(sqlCmd);
            }
            catch (System.Exception ex)
            {
                LogManager.WriteExceptionUseCache(ex.ToString());
            }

            return i;
        }

        #region ----------推广

        public bool DBSpreadSign(int pzoneID, int proleID)
        {
            string sql = string.Format("REPLACE INTO t_spread(logTime, zoneID, roleID) VALUES('{0}','{1}','{2}')",
                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), pzoneID, proleID);
            int i = ExecuteSqlNoQuery2(sql);
            return i > 0;
        }

        public bool DBSpreadSignCheck(int pzoneID, int proleID)
        {
            try
            {
                object ageObj = DbHelperMySQL.GetSingle(
                    string.Format("select IFNULL(id,0) d from t_spread where zoneID = {0} and roleID={1}", pzoneID, proleID));

                if (null != ageObj)
                {
                    int age = Convert.ToInt32(ageObj);
                    if (age > 0) return true;
                }
            }
            catch (Exception ex)
            {
                LogManager.WriteExceptionUseCache(ex.ToString());
            }

            return false;
        }

        public bool DBSpreadVeruftCheck(int czoneID, int croleID, string cuserID)
        {
            try
            {
                object ageObj = DbHelperMySQL.GetSingle(
                    string.Format("select IFNULL(id,0) d from t_spread_role where (czoneID = {0} and croleID={1}) or (cuserID='{2}') limit 1", czoneID, croleID, cuserID));

                if (null != ageObj)
                {
                    int age = Convert.ToInt32(ageObj);
                    if (age > 0) return true;
                }
            }
            catch (Exception ex)
            {
                LogManager.WriteExceptionUseCache(ex.ToString());
            }

            return false;
        }

        public int DBSpreadCountAll(int pzoneID, int proleID)
        {
            try
            {
                object ageObj = DbHelperMySQL.GetSingle(
                    string.Format("select count(*) c from t_spread_role where pzoneID={0} and proleID={1}", pzoneID, proleID));

                if (null != ageObj)  return Convert.ToInt32(ageObj);
            }
            catch (Exception ex)
            {
                LogManager.WriteExceptionUseCache(ex.ToString());
            }

            return 0;
        }

        public int DBSpreadCountVip(int pzoneID, int proleID)
        {
            try
            {
                object ageObj = DbHelperMySQL.GetSingle(
                     string.Format("select count(*) c from t_spread_role where isVip>0 and pzoneID={0} and proleID={1}", pzoneID, proleID));

                if (null != ageObj) return Convert.ToInt32(ageObj);
            }
            catch (Exception ex)
            {
                LogManager.WriteExceptionUseCache(ex.ToString());
            }

            return 0;
        }

        public int DBSpreadCountLevel(int pzoneID, int proleID)
        {
            try
            {
                object ageObj = DbHelperMySQL.GetSingle(
                    string.Format("select count(*) c from t_spread_role where isLevel>0 and pzoneID={0} and proleID={1}", pzoneID, proleID));

                if (null != ageObj) return Convert.ToInt32(ageObj);
            }
            catch (Exception ex)
            {
                LogManager.WriteExceptionUseCache(ex.ToString());
            }

            return 0;
        }

        public bool DBSpreadIsVip(int pzoneID,int proleID,int czoneID, int croleID)
        {
            string sql = string.Format("UPDATE t_spread_role set isVip=1 where pzoneID={0} and proleID={1} and czoneID={2} and croleID={3};",
                 pzoneID,proleID,czoneID, croleID);
            int i = ExecuteSqlNoQuery2(sql);
            return i > 0;
        }

        public bool DBSpreadIsLevel(int pzoneID, int proleID, int czoneID, int croleID)
        {
            string sql = string.Format("UPDATE t_spread_role set isLevel=1 where  pzoneID={0} and proleID={1} and czoneID={2} and croleID={3};",
                pzoneID, proleID, czoneID, croleID);
            int i = ExecuteSqlNoQuery2(sql);
            return i > 0;
        }

        public bool DBSpreadRoleAdd(int pzoneID, int proleID, string cuserID, int czoneID, int croleID, string tel, int isVip, int isLevel)
        {
            string sql = string.Format("INSERT INTO t_spread_role(pzoneID,proleID,cuserID,czoneID,croleID,tel,isVip,isLevel,logTime) VALUES('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}')",
               pzoneID, proleID, cuserID, czoneID, croleID, tel, isVip, isLevel, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            int i = ExecuteSqlNoQuery2(sql);
            return i > 0;
        }

        public bool DBSpreadTelCodeAdd(int pzoneID, int proleID, int czoneID, int croleID, string tel, int telCode)
        {
            string sql = string.Format("INSERT INTO t_spread_tel(pzoneID, proleID, czoneID,croleID,tel,telCode,logTime) VALUES('{0}','{1}','{2}','{3}','{4}','{5}','{6}')",
               pzoneID, proleID, czoneID, croleID, tel, telCode, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            int i = ExecuteSqlNoQuery2(sql);
            return i > 0;
        }

        public bool DBSpreadTelBind(string tel)
        {
            try
            {
                object ageObj = DbHelperMySQL.GetSingle(
                    string.Format("select IFNULL(id,0) d from t_spread_role where tel='{0}'", tel));

                if (null != ageObj)
                {
                    int age = Convert.ToInt32(ageObj);
                    if (age > 0) return true;
                }
            }
            catch (Exception ex)
            {
                LogManager.WriteExceptionUseCache(ex.ToString());
            }

            return false;
        }

        #endregion
    }
}

