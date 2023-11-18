using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KF.Contract.Data;
using KF.Contract.Interface;
using System.Threading;
using Tmsk.Contract;
using System.Runtime.Remoting.Lifetime;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using KF.Contract;
using Maticsoft.DBUtility;
using MySql.Data.MySqlClient;
using Tmsk.Tools;
using System.Configuration;
using KF.Remoting.Data;
using System.Collections.Concurrent;
using System.Collections;
using Tmsk.Contract.Const;
using Server.Tools;
using Tmsk.Tools.Tools;

namespace KF.Remoting
{
    public class SpreadService : MarshalByRefObject, ISpreadService
    {
        #region  字段
        public static SpreadService Instance = null;

        public SpreadPersistence _Persistence = SpreadPersistence.Instance;

        /// <summary>
        /// lock对象
        /// </summary>
        private object _Mutex = new object();

        public readonly GameTypes GameType = GameTypes.Spread;

        /// <summary>
        /// 清理间隔——推广数据
        /// </summary>
        private const double CLEAR_INTERVAL_SPREAD = 24 * 3600;

        /// <summary>
        /// 清理间隔——验证，手机，角色
        /// </summary>
        private const double CLEAR_INTERVAL_VERIFY = 3600;
    
        /// <summary>
        /// 清理时间——推广数据
        /// </summary>
        private DateTime _clearTimeSpread = DateTime.MinValue;

        /// <summary>
        /// 清理时间——验证，手机，角色
        /// </summary>
        private DateTime _clearTimeVerify = DateTime.MinValue; 

        /// <summary>
        /// 推广员数据
        /// </summary>
        private ConcurrentDictionary<KFSpreadKey, KFSpreadData> _spreadDataDic = new ConcurrentDictionary<KFSpreadKey, KFSpreadData>();

        /// <summary>
        /// 手机验证数据
        /// </summary>
        private ConcurrentDictionary<KFSpreadKey, KFSpreadVerifyData> _spreadVerifyDataDic = new ConcurrentDictionary<KFSpreadKey, KFSpreadVerifyData>();

        /// <summary>
        /// 手机统计
        /// </summary>
        private ConcurrentDictionary<string, KFSpreadTelTotal> _telTotalDic = new ConcurrentDictionary<string, KFSpreadTelTotal>();

        /// <summary>
        /// 角色统计
        /// </summary>
        private ConcurrentDictionary<KFSpreadKey, KFSpreadRoleTotal> _roleTotalDic = new ConcurrentDictionary<KFSpreadKey, KFSpreadRoleTotal>();
      
        public Thread _BackgroundThread;

        //private const int LIMIT_TEL_COUNT_MAX = 5;//相同tel，1小时内发送上限
        //private const int LIMIT_ROLE_COUNT_MAX = 5;//相同角色，1小时内发送上限
        //private const int LIMIT_TIME_VERIFY_MAX = 3600;
        //private const int STOP_TIME_MAX = 24 * 3600;

        private const int TEL_CODE_OUT_TIME = 90;

        #endregion

        #region 定时处理

        /// <summary>
        /// 生存期控制
        /// </summary>
        /// <returns></returns>
        public override object InitializeLifetimeService()
        {
            Instance = this;

            ILease lease = (ILease)base.InitializeLifetimeService();
            if (lease.CurrentState == LeaseState.Initial)
            {
                lease.InitialLeaseTime = TimeSpan.FromDays(2000);
            }

            return lease;
        }

        public SpreadService()
        {
            _BackgroundThread = new Thread(ThreadProc);
            _BackgroundThread.IsBackground = true;
            _BackgroundThread.Start();
        }

        ~SpreadService()
        {
            _BackgroundThread.Abort();
        }

        public void ThreadProc(object state)
        {
            _Persistence.InitConfig();

            do 
            {
                try
                {
                    DateTime now = DateTime.Now;
                    Global.UpdateNowTime(now);

                    //清理：推广数据
                    if (now > _clearTimeSpread)
                    {
                        _clearTimeSpread = now.AddSeconds(CLEAR_INTERVAL_SPREAD);
                        ClearSpreadData();
                    }

                     //清理：验证
                    if (now > _clearTimeVerify)
                    {
                        _clearTimeVerify = now.AddSeconds(CLEAR_INTERVAL_VERIFY);
                        ClearVerifyData();
                        ClearTelData();
                        ClearRoleData();
                    }

                    int sleepMS = (int)((DateTime.Now - now).TotalMilliseconds);
                    sleepMS = 1600 - sleepMS; //最大睡眠1600ms,最少睡眠50ms
                    if (sleepMS < 50)
                    {
                        sleepMS = 50;
                    }

                    Thread.Sleep(sleepMS);
                }
                catch (System.Exception ex)
                {
                    LogManager.WriteExceptionUseCache(ex.ToString());
                }
            } while (true);
        }

        //清理：推广数据（1天前）
        private void ClearSpreadData()
        {
            if (_spreadDataDic == null || _spreadDataDic.Count <= 0) return;

            List<KFSpreadKey> list = (from info in _spreadDataDic.Values
                                      where info.LogTime <= DateTime.Now.AddSeconds(-CLEAR_INTERVAL_SPREAD)
                                      select KFSpreadKey.Get(info.ZoneID, info.RoleID)).ToList<KFSpreadKey>();

            foreach (var t in list)
            {
                KFSpreadData d;
                _spreadDataDic.TryRemove(t, out d);
            }
        }

        //清理：手机验证数据（1小时前）
        private void ClearVerifyData()
        {
            if (_spreadVerifyDataDic == null || _spreadVerifyDataDic.Count <= 0) return;

            List<KFSpreadKey> list = (from info in _spreadVerifyDataDic.Values
                                      where info.LogTime <= DateTime.Now.AddHours(-CLEAR_INTERVAL_VERIFY)
                                      select KFSpreadKey.Get(info.CZoneID, info.CRoleID)).ToList<KFSpreadKey>();

            foreach (var t in list)
            {
                KFSpreadVerifyData d;
                _spreadVerifyDataDic.TryRemove(t, out d);
            }
        }

        //清理：手机统计数据（1小时前）
        private void ClearTelData()
        {
            if (_telTotalDic == null || _telTotalDic.Count <= 0) return;

            List<string> list = (from info in _telTotalDic.Values
                                      where info.LogTime <= DateTime.Now.AddHours(-CLEAR_INTERVAL_VERIFY) && info.IsStop==false
                                 select info.Tel).ToList<string>();

            foreach (var t in list)
            {
                KFSpreadTelTotal d;
                _telTotalDic.TryRemove(t, out d);
            }
        }

        //清理：角色统计数据
        private void ClearRoleData()
        {
            if (_roleTotalDic == null || _roleTotalDic.Count <= 0) return;

            List<KFSpreadKey> list = (from info in _roleTotalDic.Values
                                      where info.LogTime <= DateTime.Now.AddHours(-CLEAR_INTERVAL_VERIFY) && info.IsStop == false
                                      select KFSpreadKey.Get(info.CZoneID, info.CRoleID)).ToList<KFSpreadKey>();

            foreach (var t in list)
            {
                KFSpreadRoleTotal d;
                _roleTotalDic.TryRemove(t, out d);
            }
        }

        #endregion 

        #region 接口实现

        /// <summary>
        /// 初始化跨服客户端回调对象
        /// </summary>
        public int InitializeClient(IKuaFuClient callback, KuaFuClientContext clientInfo)
        {
            try
            {
                if (clientInfo.GameType == (int)GameTypes.Spread && clientInfo.ServerId != 0)
                {
                    return ClientAgentManager.Instance().InitializeClient(callback, clientInfo);
                }
                else
                {
                    LogManager.WriteLog(LogTypes.Warning, string.Format("InitializeClient时GameType错误,禁止连接.ServerId:{0},GameType:{1}", clientInfo.ServerId, clientInfo.GameType));
                    return StdErrorCode.Error_Invalid_GameType;
                }
            }
            catch (System.Exception ex)
            {
                LogManager.WriteExceptionUseCache(string.Format("InitializeClient服务器ID重复,禁止连接.ServerId:{0},ClientId:{1}", clientInfo.ServerId, clientInfo.ClientId));
                return StdErrorCode.Error_Server_Internal_Error;
            }
        }

        public int SpreadSign(int serverID, int zoneID, int roleID)
        {
            if (!IsAgent(serverID)) return (int)ESpreadState.EServer;

            KFSpreadKey key = KFSpreadKey.Get(zoneID, roleID);
            KFSpreadData oldData;
            if (_spreadDataDic.TryGetValue(key, out oldData))
            {
                oldData.UpdateLogtime();
                return (int)ESpreadState.ESpreadIsSign;
            }

            bool result = SpreadPersistence.Instance.DBSpreadSign(zoneID, roleID);
            if (!result) return (int)ESpreadState.Fail;

            //添加或更新
            oldData = new KFSpreadData()
            {
                ServerID = serverID,
                ZoneID = zoneID,
                RoleID = roleID,
            };

            _spreadDataDic.TryAdd(key, oldData);
            return (int)ESpreadState.Success;
        }

        public int[] SpreadCount(int serverID, int zoneID, int roleID)
        {
            int[] result = new int[] { 0, 0, 0 };

            if (!IsAgent(serverID)) return result;

            KFSpreadKey key = KFSpreadKey.Get(zoneID, roleID);
            KFSpreadData oldData;
            if (_spreadDataDic.TryGetValue(key, out oldData))
            {
                result[0] = oldData.CountRole;
                result[1] = oldData.CountVip;
                result[2] = oldData.CountLevel;

                oldData.UpdateLogtime();
                return result;
            }

            result[0] = SpreadPersistence.Instance.DBSpreadCountAll(zoneID, roleID);
            result[1] = SpreadPersistence.Instance.DBSpreadCountVip(zoneID, roleID);
            result[2] = SpreadPersistence.Instance.DBSpreadCountLevel(zoneID, roleID);

            //添加或更新
            oldData = new KFSpreadData()
            {
                ServerID = serverID,
                ZoneID = zoneID,
                RoleID = roleID,
                CountRole = result[0],
                CountVip = result[1],
                CountLevel = result[2],
            };

            _spreadDataDic.TryAdd(key, oldData);
            return result;
        }

        public int CheckVerifyCode(int cserverID,string cuserID, int czoneID, int croleID, int pzoneID, int proleID, int isVip, int isLevel)
        {
            if (!IsAgent(cserverID)) return (int)ESpreadState.EServer;

            //推广员
            KFSpreadData pData = GetSpreadData(pzoneID, proleID);
            if(pData == null)return (int)ESpreadState.EVerifyCodeWrong;

            //已验证
            bool isVerify = SpreadPersistence.Instance.DBSpreadVeruftCheck(czoneID, croleID, cuserID);
            if (isVerify) return (int)ESpreadState.EVerifyCodeHave;

            //角色验证统计
            KFSpreadRoleTotal roleTotalData = GetRoleTotalData(cserverID, czoneID, croleID);
            if (roleTotalData.IsStop) return (int)ESpreadState.EVerifyMore;

            //验证信息           
            KFSpreadKey ckey = KFSpreadKey.Get(czoneID, croleID);
            KFSpreadVerifyData verifyData =null;
            _spreadVerifyDataDic.TryRemove(ckey, out verifyData);
  
            verifyData = new KFSpreadVerifyData()
            {
                CUserID = cuserID,
                CServerID = cserverID,
                CZoneID = czoneID,
                CRoleID = croleID,

                PZoneID = pzoneID,
                PRoleID = proleID,
                IsVip = isVip,
                IsLevel = isLevel,
            };

            _spreadVerifyDataDic.TryAdd(ckey, verifyData);

            return (int)ESpreadState.Success;
        }

        public int TelCodeGet(int cserverID, int czoneID, int croleID, string tel)
        {
            if (!IsAgent(cserverID)) return (int)ESpreadState.EServer;

            KFSpreadKey ckey = KFSpreadKey.Get(czoneID, croleID);
            KFSpreadVerifyData verifyData = null;
            if (!_spreadVerifyDataDic.TryGetValue(ckey, out verifyData)) return (int)ESpreadState.EVerifyNo;

            //手机号绑定
            bool isTelBind = SpreadPersistence.Instance.DBSpreadTelBind(tel);
            if (isTelBind) return (int)ESpreadState.ETelBind;

            //手机验证统计
            KFSpreadTelTotal totalData = GetTelTotalData(tel, true);
            if (totalData.IsStop) return (int)ESpreadState.ETelMore;

            //角色验证统计
            KFSpreadRoleTotal roleTotalData = GetRoleTotalData(cserverID, czoneID, croleID, true);
            if (roleTotalData.IsStop) return (int)ESpreadState.EVerifyMore;

            verifyData.Tel = tel;
            verifyData.TelCode = GetTelCodeRandom();
            verifyData.TelTime = DateTime.Now;

            bool result = SpreadPersistence.Instance.DBSpreadTelCodeAdd(verifyData.PZoneID,verifyData.PRoleID,czoneID, croleID, tel, verifyData.TelCode);
            if (!result) return (int)ESpreadState.ETelCodeGet;

            return (int)ESpreadState.Success;
        }

        public int TelCodeVerify(int serverID, int czoneID, int croleID, int telCode)
        {
            if (!IsAgent(serverID)) return (int)ESpreadState.EServer;

            KFSpreadKey ckey = KFSpreadKey.Get(czoneID, croleID);
            KFSpreadVerifyData verifyData = null;
            if (!_spreadVerifyDataDic.TryGetValue(ckey, out verifyData)) return (int)ESpreadState.EVerifyNo;

            //推广员
            KFSpreadData pData = GetSpreadData(verifyData.PZoneID, verifyData.PRoleID);
            if (pData == null) return (int)ESpreadState.EVerifyCodeWrong;
            pData.UpdateLogtime();

            if (verifyData.TelCode != telCode) return (int)ESpreadState.ETelCodeWrong;

            if (DateTime.Now.AddSeconds(-TEL_CODE_OUT_TIME) > verifyData.TelTime) return (int)ESpreadState.ETelCodeOutTime;

            //验证成功加数据库
            bool result = SpreadPersistence.Instance.DBSpreadRoleAdd(
                verifyData.PZoneID, verifyData.PRoleID, verifyData.CUserID, verifyData.CZoneID, verifyData.CRoleID, verifyData.Tel, verifyData.IsVip, verifyData.IsLevel);
            if (!result) return (int)ESpreadState.Fail;

            //推广员
            lock (pData)
            {
                pData.CountLevel += verifyData.IsLevel;
                pData.CountVip += verifyData.IsVip;
                pData.CountRole += 1;

                if (pData.ServerID > 0) NotifySpreadData(pData);
            }

            _spreadVerifyDataDic.TryRemove(ckey, out verifyData);
            return (int)ESpreadState.Success;
        }

        public bool SpreadLevel(int pzoneID, int proleID, int czoneID, int croleID)
        {
            //推广员
            KFSpreadData pData = GetSpreadData(pzoneID, proleID);
            if (pData == null) return false;
            pData.UpdateLogtime();

            //推广员
            lock (pData)
            {
                bool result = SpreadPersistence.Instance.DBSpreadIsLevel(pzoneID, proleID, czoneID, croleID);
                if (result)
                {
                    pData.CountLevel += 1;
                    if (pData.ServerID > 0) NotifySpreadData(pData);

                    return true;
                }  
            }

            return false;
        }

        public bool SpreadVip(int pzoneID, int proleID, int czoneID, int croleID)
        {
            //推广员
            KFSpreadData pData = GetSpreadData(pzoneID, proleID);
            if (pData == null) return false;
            pData.UpdateLogtime();

            //推广员
            lock (pData)
            {
                bool result = SpreadPersistence.Instance.DBSpreadIsVip(pzoneID, proleID, czoneID, croleID);
                if (result)
                {
                    pData.CountVip += 1;
                    if (pData.ServerID > 0) NotifySpreadData(pData);

                    return true;
                }
            }

            return false;
        }

        private void NotifySpreadData(KFSpreadData data)
        {
            ClientAgentManager.Instance().PostAsyncEvent(data.ServerID, GameType,
                new AsyncDataItem(KuaFuEventTypes.SpreadCount, data.ZoneID, data.RoleID, data.CountRole, data.CountVip, data.CountLevel));
        }

        private int GetTelCodeRandom()
        {
            if (Consts.IsTest > 0) return 123456;
            return RandomHelper.GetRandomNumber(100000, 999999);
        }

        private KFSpreadData GetSpreadData(int pzoneID, int proleID)
        {
            KFSpreadKey pkey = KFSpreadKey.Get(pzoneID, proleID);
            KFSpreadData pData = null;
            if (!_spreadDataDic.TryGetValue(pkey, out pData))
            {
                bool isSign = SpreadPersistence.Instance.DBSpreadSignCheck(pzoneID, proleID);
                if (!isSign) return null;

                pData = new KFSpreadData()
                {
                    ServerID = 0,
                    ZoneID = pzoneID,
                    RoleID = proleID,
                    CountRole = SpreadPersistence.Instance.DBSpreadCountAll(pzoneID, proleID),
                    CountVip = SpreadPersistence.Instance.DBSpreadCountVip(pzoneID, proleID),
                    CountLevel = SpreadPersistence.Instance.DBSpreadCountLevel(pzoneID, proleID),
                };

                _spreadDataDic.TryAdd(pkey, pData);
            }

            if (pData != null) pData.UpdateLogtime();
            return pData;
        }

        private KFSpreadRoleTotal GetRoleTotalData(int cserverID,int czoneId,int croleID, bool isAddCount = false)
        {
            KFSpreadKey key = KFSpreadKey.Get(czoneId, croleID);
            KFSpreadRoleTotal data = null;
            if (!_roleTotalDic.TryGetValue(key, out data))
            {
                data = new KFSpreadRoleTotal()
                {
                    CServerID = cserverID,
                    CZoneID = czoneId,
                    CRoleID = croleID,
                };

                _roleTotalDic.TryAdd(key, data);
            }

            int spanSecond = TimeSpanSecond(data.LogTime, DateTime.Now);
            if (data.IsStop)
            {
                if (spanSecond > Consts.VerifyRoleTimeStop)
                {
                    data.LogTime = DateTime.Now;
                    data.Count = 0;
                    data.IsStop = false;
                }

                return data;
            }

            if (spanSecond > Consts.VerifyRoleTimeLimit)
            {
                data.LogTime = DateTime.Now;
                data.Count = 0;
            }

            if (isAddCount) data.AddCount();

            if (data.Count > Consts.VerifyRoleMaxCount) data.IsStop = true;

            return data;
        }

        private KFSpreadTelTotal GetTelTotalData(string tel, bool isAddCount = false)
        {
            KFSpreadTelTotal data = null;
            if (!_telTotalDic.TryGetValue(tel, out data))
            {
                data = new KFSpreadTelTotal()
                {
                    Tel = tel,
                };

                _telTotalDic.TryAdd(tel, data);
            }

            int spanSecond = TimeSpanSecond(data.LogTime, DateTime.Now);
            if (data.IsStop)
            {
                if (spanSecond > Consts.TelTimeStop)
                {
                    data.LogTime = DateTime.Now;
                    data.Count = 0;
                    data.IsStop = false;
                }

                return data;
            }

            if (spanSecond > Consts.TelTimeLimit)
            {
                data.LogTime = DateTime.Now;
                data.Count = 0;
            }

            if (isAddCount) data.AddCount();

            if (data.Count > Consts.TelMaxCount) data.IsStop = true;

            return data;
        }

        private int TimeSpanSecond(DateTime begin, DateTime end)
        {
            TimeSpan tb = new TimeSpan(begin.Ticks);
            TimeSpan te = new TimeSpan(end.Ticks);
            TimeSpan sp = te.Subtract(tb).Duration();

            return sp.Seconds;
        }

        #endregion 

        #region 辅助函数
        public bool IsAgent(int serverID)
        {
            bool isAgent = ClientAgentManager.Instance().ExistAgent(serverID);
            if (!isAgent)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("SpreadSign时ServerId错误.ServerId:{0}", serverID));
            }

            return isAgent;
        }

        public AsyncDataItem[] GetClientCacheItems(int serverID)
        {
            return ClientAgentManager.Instance().PickAsyncEvent(serverID, GameType);
        }



        

        //private void NotifyFuBenRoleCount(ElementWarFuBenData fuBenData)
        //{
        //    try
        //    {
        //        lock(fuBenData)
        //        {
        //            int roleCount = fuBenData.RoleDict.Count;
        //            foreach (var role in fuBenData.RoleDict.Values)
        //            {
        //                KuaFuRoleData kuaFuRoleData;
        //                if (_RoleIdKuaFuRoleDataDict.TryGetValue(KuaFuRoleKey.Get(role.ServerId, role.RoleId), out kuaFuRoleData))
        //                {
        //                    ElementWarAgent agent;
        //                    if (_ServerId2KuaFuClientAgent.TryGetValue(kuaFuRoleData.ServerId, out agent))
        //                    {
        //                        agent.NotifyFuBenRoleCount(kuaFuRoleData, roleCount);
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch
        //    {
        //    }
        //}


        #endregion 
    }
}
