using GameServer.Core.Executor;
using KF.Contract.Data;
using KF.Remoting.Data;
using Maticsoft.DBUtility;
using Server.Tools;
using System.Collections.Generic;
using System.Threading;
using System.Xml.Linq;
using Tmsk.Tools.Tools;

namespace KF.Remoting
{
    public class HuanYingSiYuanPersistence
    {
        private HuanYingSiYuanPersistence()
        { }

        public static readonly HuanYingSiYuanPersistence Instance = new HuanYingSiYuanPersistence();

        /// <summary>
        /// 保护数据的互斥对象
        /// </summary>
        public object Mutex = new object();

        private int CurrGameId = Global.UninitGameId;

        /// <summary>
        /// 是否已初始化
        /// </summary>
        private bool Initialized = false;

        public int SignUpWaitSecs1 = 30;
        public int SignUpWaitSecs2 = 60;

        /// <summary>
        /// 数据库更新队列,异步写数据库
        /// </summary>
        private Queue<GameFuBenStateDbItem> GameFuBenStateDbItemQueue = new Queue<GameFuBenStateDbItem>();

        /// <summary>
        /// 初始化配置文件
        /// </summary>
        public void InitConfig()
        {
            try
            {
                XElement xmlFile = ConfigHelper.Load("config.xml");

                Consts.HuanYingSiYuanRoleCountTotal = (int)ConfigHelper.GetElementAttributeValueLong(xmlFile, "add", "key", "HuanYingSiYuanRoleCountTotal", "value", 16);
                Consts.HuanYingSiYuanRoleCountPerSide = (int)ConfigHelper.GetElementAttributeValueLong(xmlFile, "add", "key", "HuanYingSiYuanRoleCountPerSide", "value", 8);
                SignUpWaitSecs1 = (int)ConfigHelper.GetElementAttributeValueLong(xmlFile, "add", "key", "SignUpWaitSecs1", "value", 30);
                SignUpWaitSecs2 = (int)ConfigHelper.GetElementAttributeValueLong(xmlFile, "add", "key", "SignUpWaitSecs2", "value", 60);

                if (CurrGameId == Global.UninitGameId)
                {
                    CurrGameId = (int)((long)DbHelperMySQL.GetSingle("SELECT IFNULL(MAX(id),0) FROM t_hysy_0;"));
                }

                Initialized = true;
            }
            catch (System.Exception ex)
            {
                LogManager.WriteExceptionUseCache(ex.ToString());
            }
        }

        public void SaveCostTime(int ms)
        {
            try
            {
                if (ms > KuaFuServerManager.WritePerformanceLogMs)
                {
                    LogManager.WriteLog(LogTypes.Warning, "HuanYingSiYuan Time (ms):" + ms);
                }
            }
            catch
            {
            }
        }

        private void ExecuteSqlNoQuery(string sqlCmd)
        {
            //写数据库
            try
            {
                DbHelperMySQL.ExecuteSql(sqlCmd);
            }
            catch (System.Exception ex)
            {
                LogManager.WriteExceptionUseCache(ex.ToString());
            }
        }

        public int GetNextGameId()
        {
            return Interlocked.Add(ref this.CurrGameId, 1);
        }

        public void LogCreateHysyFuben(int gameId, int kfSrvId, int fubenSeqId, int roleNum)
        {
            string sql = string.Format("INSERT INTO t_hysy_0(`id`,`serverid`,`fubensid`,`createtime`,`rolenum`) VALUES({0},{1},{2},'{3}',{4});",
                gameId, kfSrvId, fubenSeqId, TimeUtil.NowDateTime().ToString("yyyy-MM-dd HH:mm:ss"), roleNum);

            ExecuteSqlNoQuery(sql);
        }
    }
}