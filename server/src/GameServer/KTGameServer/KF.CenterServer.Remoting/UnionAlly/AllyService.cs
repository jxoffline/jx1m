using KF.Contract;
using KF.Contract.Data;
using KF.Contract.Interface;
using KF.Remoting.Data;
using Server.Tools;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Lifetime;
using System.Text;
using System.Threading;
using Tmsk.Contract;
using Tmsk.Contract.Interface;
using Tmsk.Contract.KuaFuData;

namespace KF.Remoting
{
    public class AllyService : MarshalByRefObject, IAllyService
    {
        object _Mutex = new object();

        public readonly GameTypes _gameType = GameTypes.Ally;

        //战盟数据(战盟id，战盟数据)
        private ConcurrentDictionary<int, KFAllyData> _unionDic = new ConcurrentDictionary<int, KFAllyData>();

        //结盟数据(战盟id，盟友id)
        private ConcurrentDictionary<int, List<int>> _allyDic = new ConcurrentDictionary<int, List<int>>();

        //结盟请求(战盟id，被请求战盟数据)
        private ConcurrentDictionary<int, List<KFAllyData>> _requestDic = new ConcurrentDictionary<int, List<KFAllyData>>();

        private ConcurrentDictionary<int, List<KFAllyData>> _acceptDic = new ConcurrentDictionary<int, List<KFAllyData>>();
       
        #region 定时处理

        public static AllyService Instance = null;
        public AllyPersistence _Persistence = AllyPersistence.Instance;

        public Thread _BackgroundThread;

        private const double REQUEST_SECOND_CLEAR_SPAN = 30;

        private DateTime _clearTimeRequest = DateTime.MinValue;
        

        //生存期控制
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

        public AllyService()
        {
            _BackgroundThread = new Thread(ThreadProc);
            _BackgroundThread.IsBackground = true;
            _BackgroundThread.Start();
        }

        ~AllyService()
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

                    if (now > _clearTimeRequest)
                    {
                        _clearTimeRequest = now.AddSeconds(REQUEST_SECOND_CLEAR_SPAN);
                        ClearAcceptData();
                        ClearRequestData();
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

        private void ClearAcceptData()
        {
            lock (_Mutex)
            {
                if (_acceptDic == null || _acceptDic.Count <= 0) return;
                foreach (var r in _acceptDic)
                {
                    int unionID = r.Key;
                    List<KFAllyData> oldList = r.Value;

                    var temp = from info in oldList
                               where info.LogTime <= DateTime.Now.AddSeconds(-Consts.AllyRequestClearSecond)
                               select info;

                    if (temp.Any())
                    {
                        int state = 0;
                        KFAllyData myData = GetUnionData(unionID);
                        List<KFAllyData> list = temp.ToList<KFAllyData>();
                        foreach (KFAllyData targetData in list)
                        {
                            state = AllyOperate(myData.ServerID, myData.UnionID, targetData.UnionID, (int)EAllyOperate.Refuse);
                        }//foreach
                    }//if
                }//foreach
            }
        }
        
        private void ClearRequestData()
        {
            lock (_Mutex)
            {
                if (_requestDic == null || _requestDic.Count <= 0) return;
                foreach (var r in _requestDic)
                {
                    int unionID = r.Key;
                    List<KFAllyData> oldList = r.Value;

                    var temp = from info in oldList
                               where info.LogTime <= DateTime.Now.AddSeconds(-Consts.AllyRequestClearSecond)
                               select info;

                    if (temp.Any())
                    {
                        int state = 0;
                        KFAllyData myData = GetUnionData(unionID);
                        List<KFAllyData> list = temp.ToList<KFAllyData>();
                        foreach (KFAllyData targetData in list)
                        {
                            state = AllyOperate(targetData.ServerID, targetData.UnionID, myData.UnionID, (int)EAllyOperate.Refuse);
                        }//foreach
                    }//if
                }//foreach
            }
        }

        #endregion 


        #region 接口实现

        //初始化跨服客户端回调对象
        public int InitializeClient(IKuaFuClient callback, KuaFuClientContext clientInfo)
        {
            try
            {
                if (clientInfo.GameType == (int)_gameType && clientInfo.ServerId != 0)
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

        public AsyncDataItem[] GetClientCacheItems(int serverID)
        {
            return ClientAgentManager.Instance().PickAsyncEvent(serverID, _gameType);
        }

        public long UnionAllyVersion(int serverID)
        {
            if (!IsAgent(serverID)) return 0;
            return _Persistence.DataVersion;
        }

        public int UnionAllyInit(int serverID, int unionID, bool isKF)
        {
            if (!IsAgent(serverID)) return (int)EAlly.EServer;

            lock (_Mutex)
            {
                KFAllyData oldData;
                if (!_unionDic.TryGetValue(unionID, out oldData))
                {
                    oldData = AllyPersistence.Instance.DBUnionDataGet(unionID);
                    if (oldData == null) return (int)EAlly.EAddUnion;

                    _unionDic.TryAdd(unionID, oldData);
                }

                if (!isKF && oldData.ServerID != serverID)
                {
                    oldData.ServerID = serverID;
                    bool result = AllyPersistence.Instance.DBUnionDataUpdate(oldData);
                    if (!result) return (int)EAlly.EFail;
                }

                if (!isKF) CheckAllyLog(serverID, unionID);

                oldData.UpdateLogtime();
                return (int)EAlly.Succ;
            }
        }

        private void CheckAllyLog(int serverID, int unionID)
        {
            List<AllyLogData> list = AllyPersistence.Instance.DBAllyLogList(unionID);
            if (list == null || list.Count <= 0) return;

            ClientAgentManager.Instance().PostAsyncEvent(serverID, _gameType, new AsyncDataItem(KuaFuEventTypes.AllyLog, unionID, list));
        }

        public int UnionDataChange(int serverID, AllyData newData)
        {
            if (!IsAgent(serverID)) return (int)EAlly.EServer;

            lock (_Mutex)
            {
                //获取老的数据，更新状态，logtime
                KFAllyData kfData = new KFAllyData();
                kfData.Copy(newData);
                kfData.LogTime = DateTime.Now;
                kfData.ServerID = serverID;
                kfData.UpdateLogtime();

                bool result = AllyPersistence.Instance.DBUnionDataUpdate(kfData);
                if (!result) return (int)EAlly.EFail;

                KFAllyData oldData;
                if (_unionDic.TryGetValue(kfData.UnionID, out oldData))
                    _unionDic[kfData.UnionID] = kfData;
                else
                    _unionDic.TryAdd(kfData.UnionID, kfData);             

                //结盟数据
                List<int> oldAllyIDList;
                if (_allyDic.TryGetValue(kfData.UnionID, out oldAllyIDList))
                {
                    AllyData myData = new AllyData();
                    myData.Copy(kfData);
                    myData.LogState = (int)EAlly.AllyAgree;

                    foreach (int id in oldAllyIDList)
                    {
                        KFAllyData targetData = GetUnionData(id);
                        if (IsAgent(targetData.ServerID))
                            ClientAgentManager.Instance().PostAsyncEvent(targetData.ServerID, _gameType, new AsyncDataItem(KuaFuEventTypes.AllyUnionUpdate, targetData.UnionID, myData));
                    }
                }

                AllyData myData2 = new AllyData();
                myData2.Copy(kfData);
                myData2.LogState = (int)EAlly.AllyRequestSucc;
                //结盟请求
                List<KFAllyData> rList = null;
                if (_requestDic.TryGetValue(kfData.UnionID, out rList))
                {     
                    foreach (KFAllyData targetData in rList)
                    {
                        List<KFAllyData> raList = null;
                        if (IsAgent(targetData.ServerID) && _acceptDic.TryGetValue(targetData.UnionID, out raList))
                        {
                            KFAllyData oldAData = GetAcceptData(targetData.UnionID, kfData.UnionID);
                            if (oldAData != null)
                            {
                                raList.Remove(oldAData);
                                raList.Add(kfData);
                                ClientAgentManager.Instance().PostAsyncEvent(targetData.ServerID, _gameType, new AsyncDataItem(KuaFuEventTypes.AllyUnionUpdate, targetData.UnionID, myData2));
                            }
                        } 
                    }
                }

                //接受请求
                List<KFAllyData> aList = null;
                if (_acceptDic.TryGetValue(kfData.UnionID, out aList))
                {
                    foreach (KFAllyData targetData in aList)
                    {
                        List<KFAllyData> arList = null;
                        if (IsAgent(targetData.ServerID) && _requestDic.TryGetValue(targetData.UnionID, out arList))
                        {
                            KFAllyData oldRData = GetRequestData(targetData.UnionID, kfData.UnionID);
                            if (oldRData != null)
                            {
                                arList.Remove(oldRData);
                                arList.Add(kfData);
                                ClientAgentManager.Instance().PostAsyncEvent(targetData.ServerID, _gameType, new AsyncDataItem(KuaFuEventTypes.AllyUnionUpdate, targetData.UnionID, myData2));
                            }
                        }
                    }
                }

                return (int)EAlly.Succ;
            }
        }

        public int UnionDel(int serverID, int unionID)
        {
            if (!IsAgent(serverID)) return (int)EAlly.EServer;

            lock (_Mutex)
            {
                KFAllyData oldData;
                if (!_unionDic.TryGetValue(unionID, out oldData)) return (int)EAlly.EServer;

                int state = 0;
                //结盟数据
                List<int> oldAllyIDList;
                if (_allyDic.TryGetValue(unionID, out oldAllyIDList))
                {
                    foreach (int targetID in oldAllyIDList)
                    {
                        state = AllyOperate(serverID, unionID, targetID, (int)EAllyOperate.Remove, true);
                    }

                    _allyDic.TryRemove(unionID, out oldAllyIDList);
                }

                //结盟请求
                List<KFAllyData> rList = null;
                if (_requestDic.TryGetValue(unionID, out rList))
                {
                    foreach (KFAllyData targetData in rList)
                    {
                        state = AllyOperate(serverID, unionID, targetData.UnionID, (int)EAllyOperate.Cancel, true);
                    }

                    _requestDic.TryRemove(unionID, out rList);
                }

                //接受请求
                List<KFAllyData> aList = null;
                if (_acceptDic.TryGetValue(unionID, out aList))
                {
                    foreach (KFAllyData targetData in aList)
                    {
                        state = AllyOperate(serverID, unionID, targetData.UnionID, (int)EAllyOperate.Refuse, true);
                    }

                    _acceptDic.TryRemove(unionID, out aList);
                }

                _unionDic.TryRemove(unionID, out oldData);

                bool result = AllyPersistence.Instance.DBUnionDataDel(unionID);
                return result ? (int)EAlly.Succ : (int)EAlly.EFail;
            }
        }

        public List<AllyData> AllyDataList(int serverID, int unionID,int type)
        {
            List<AllyData> resultlist = new List<AllyData>();
            if (!IsAgent(serverID)) return resultlist;

            switch (type)
            {
                case (int)EAllyDataType.Ally:
                    resultlist = AllyList(unionID);
                    break;
                case (int)EAllyDataType.Request:
                    resultlist = AllyRequestList(unionID);
                    break;
                case (int)EAllyDataType.Accept:
                    resultlist = AllyAcceptList(unionID);
                    break;
            }

            return resultlist;
        }

        public List<int> InitAllyIDList(int unionID)
        {
            lock (_Mutex)
            {
                List<int> allyIDList = null;
                if (_allyDic.TryGetValue(unionID, out allyIDList)) return allyIDList;

                allyIDList = AllyPersistence.Instance.DBAllyIDList(unionID);
                _allyDic.TryAdd(unionID, allyIDList);

                return allyIDList;
            }
        }

        public List<AllyData> AllyList(int unionID)
        {
            lock (_Mutex)
            {
                List<int> allyIDList = InitAllyIDList(unionID);
                List<AllyData> list = new List<AllyData>();
                foreach (int id in allyIDList)
                {
                    KFAllyData data = GetUnionData(id);
                    if (data == null) continue;

                    AllyData result = new AllyData();
                    result.UnionID = data.UnionID;
                    result.UnionZoneID = data.UnionZoneID;
                    result.UnionName = data.UnionName;
                    result.UnionLevel = data.UnionLevel;
                    result.UnionNum = data.UnionNum;
                    result.LeaderID = data.LeaderID;
                    result.LeaderZoneID = data.LeaderZoneID;
                    result.LeaderName = data.LeaderName;
                    result.LogState = (int)EAlly.AllyAgree;

                    list.Add(result);
                }

                return list;
            }
        }

        private List<KFAllyData> InitAllyRequestList(int unionID)
        {
            lock (_Mutex)
            {
                List<KFAllyData> requestList = null;
                if (_requestDic.TryGetValue(unionID, out requestList)) return requestList;

                requestList = AllyPersistence.Instance.DBAllyRequestList(unionID);
                _requestDic.TryAdd(unionID, requestList);

                List<KFAllyData> list = new List<KFAllyData>();
                foreach (KFAllyData rData in requestList)
                {
                    KFAllyData data = GetUnionData(rData.UnionID);
                    if (data == null) continue;

                    rData.UnionZoneID = data.UnionZoneID;
                    rData.UnionName = data.UnionName;
                    rData.UnionLevel = data.UnionLevel;
                    rData.UnionNum = data.UnionNum;
                    rData.LeaderID = data.LeaderID;
                    rData.LeaderZoneID = data.LeaderZoneID;
                    rData.LeaderName = data.LeaderName;
                    rData.ServerID = data.ServerID;
                }

                return requestList;
            }
        }

        public List<AllyData> AllyRequestList(int unionID)
        {
            lock (_Mutex)
            {
                List<KFAllyData> requestList = InitAllyRequestList(unionID);

                List<AllyData> list = new List<AllyData>();
                foreach (KFAllyData rData in requestList)
                {
                    AllyData d = new AllyData();
                    d.Copy(rData);
                    list.Add(d);
                }

                return list;
            }
        }

        public void AllyRequestAdd(int unionID,KFAllyData item)
        {
            lock (_Mutex)
            {
                List<KFAllyData> list = InitAllyRequestList(unionID);
                list.Add(item);
            }
        }

        public List<KFAllyData> InitAllyAcceptList(int unionID)
        {
            lock (_Mutex)
            {
                List<KFAllyData> acceptList = null;
                if (_acceptDic.TryGetValue(unionID, out acceptList)) return acceptList;

                acceptList = AllyPersistence.Instance.DBAllyAcceptList(unionID);
                _acceptDic.TryAdd(unionID, acceptList);

                foreach (KFAllyData rData in acceptList)
                {
                    KFAllyData data = GetUnionData(rData.UnionID);
                    if (data == null) continue;

                    rData.UnionZoneID = data.UnionZoneID;
                    rData.UnionName = data.UnionName;
                    rData.UnionLevel = data.UnionLevel;
                    rData.UnionNum = data.UnionNum;
                    rData.LeaderID = data.LeaderID;
                    rData.LeaderZoneID = data.LeaderZoneID;
                    rData.LeaderName = data.LeaderName;
                    rData.ServerID = data.ServerID;
                }

                return acceptList;
            }
        }

        public List<AllyData> AllyAcceptList(int unionID)
        {
            lock (_Mutex)
            {
                List<KFAllyData> acceptList = InitAllyAcceptList(unionID);

                List<AllyData> list = new List<AllyData>();
                foreach (KFAllyData rData in acceptList)
                {
                    AllyData d = new AllyData();
                    d.Copy(rData);
                    list.Add(d);
                }

                return list;
            }
        }

        public void AllyAcceptAdd(int unionID, KFAllyData item)
        {
            lock (_Mutex)
            {
                List<KFAllyData> list = InitAllyAcceptList(unionID);
                list.Add(item);
            }
        }

        public AllyData AllyRequest(int serverID, int unionID, int zoneID, string unionName)
        {
            lock (_Mutex)
            {
                AllyData clientRequest = new AllyData();
                if (!IsAgent(serverID))
                {
                    clientRequest.LogState = (int)EAlly.EServer;
                    return clientRequest;
                }

                KFAllyData targetData = GetUnionData(zoneID, unionName);
                if (targetData == null)
                {
                    clientRequest.LogState = (int)EAlly.EName;
                    return clientRequest;
                }

                InitAllyIDList(targetData.UnionID);
                InitAllyRequestList(targetData.UnionID);
                InitAllyAcceptList(targetData.UnionID);

                if (UnionIsAlly(unionID, targetData.UnionID))
                {
                    clientRequest.LogState = (int)EAlly.EIsAlly;
                    return clientRequest;
                }

                if (UnionIsRequest(unionID, targetData.UnionID) || UnionIsAccept(targetData.UnionID, unionID))
                {
                    clientRequest.LogState = (int)EAlly.EMore;
                    return clientRequest;
                }

                int sum = _allyDic[unionID].Count + _requestDic[unionID].Count;
                if (sum >= Consts.AllyNumMax)
                {
                    clientRequest.LogState = (int)EAlly.EAllyMax;
                    return clientRequest;
                }

                DateTime logTime = DateTime.Now;
                int logState = (int)EAlly.AllyRequestSucc;
                bool isAdd = AllyPersistence.Instance.DBAllyRequestAdd(unionID, targetData.UnionID, logTime, logState);
                if (!isAdd)
                {
                    clientRequest.LogState = (int)EAlly.EAllyRequest;
                    return clientRequest;
                }
                //my
                KFAllyData myData = GetUnionData(unionID);

                AllyLogData logData = new AllyLogData();
                logData.UnionID = targetData.UnionID;
                logData.UnionZoneID = targetData.UnionZoneID;
                logData.UnionName = targetData.UnionName;
                logData.MyUnionID = unionID;
                logData.LogTime = logTime;
                logData.LogState = logState;
                ClientAgentManager.Instance().PostAsyncEvent(serverID, _gameType, new AsyncDataItem(KuaFuEventTypes.AllyLog, unionID, new List<AllyLogData>() { logData }));


                KFAllyData requestData = new KFAllyData();
                requestData.Copy(targetData);
                requestData.LogState = logState;
                requestData.LogTime = logTime;
                requestData.UpdateLogtime();

                AllyRequestAdd(unionID, requestData);

                clientRequest.Copy(requestData);
                // ClientAgentManager.Instance().PostAsyncEvent(serverID, _gameType, new AsyncDataItem(KuaFuEventTypes.AllyRequest, unionID, clientRequest));

                //target
                KFAllyData acceptData = new KFAllyData();
                acceptData.Copy(myData);
                acceptData.LogState = logState;
                acceptData.LogTime = logTime;
                AllyAcceptAdd(targetData.UnionID, acceptData);

                if (IsAgent(targetData.ServerID))
                {
                    AllyData clientAccept = new AllyData();
                    clientAccept.Copy(acceptData);
                    ClientAgentManager.Instance().PostAsyncEvent(targetData.ServerID, _gameType, new AsyncDataItem(KuaFuEventTypes.AllyAccept, targetData.UnionID, clientAccept));
                }

                return clientRequest;
            }
        }

        public int AllyOperate(int serverID, int unionID, int targetID, int operateType, bool isDel=false)
        {
            lock (_Mutex)
            {
                int result = (int)EAlly.EFail;

                KFAllyData targetData = GetUnionData(targetID);
                if (targetData == null) return (int)EAlly.ENoTargetUnion;

                InitAllyIDList(targetData.UnionID);
                InitAllyRequestList(targetData.UnionID);
                InitAllyAcceptList(targetData.UnionID);

                switch ((EAllyOperate)operateType)
                {
                    case EAllyOperate.Remove:
                        result = OperateRemove(serverID, unionID, targetID, isDel);
                        break;
                    case EAllyOperate.Cancel:
                        result = OperateCancel(serverID, unionID, targetID, isDel);
                        break;
                    case EAllyOperate.Agree:
                        result = OperateAgree(serverID, unionID, targetID);
                        break;
                    case EAllyOperate.Refuse:
                        result = OperateRefuse(serverID, unionID, targetID, isDel);
                        break;
                }

                return result;
            }
        }

        //Remove
        public int OperateRemove(int serverID, int unionID, int targetID, bool isDel = false)
        {
            lock (_Mutex)
            {
                KFAllyData targetData = GetUnionData(targetID);
                if (targetData == null) return (int)EAlly.ENoTargetUnion;

                if (!UnionIsAlly(unionID, targetData.UnionID)) return (int)EAlly.EFail;
                if (!UnionIsAlly(targetID, unionID)) return (int)EAlly.EFail;

                bool isDelResult = AllyPersistence.Instance.DBAllyDel(unionID, targetID);
                if (!isDelResult) return (int)EAlly.EFail;

                DateTime logTime = DateTime.Now;
                int logState = (int)EAlly.AllyRemoveSucc;

                //my
                if (!isDel) _allyDic[unionID].Remove(targetID);
                //ClientAgentManager.Instance().PostAsyncEvent(serverID, _gameType, new AsyncDataItem(KuaFuEventTypes.AllyRemove, unionID, targetID));

                AllyLogData logData = new AllyLogData();
                logData.UnionID = targetData.UnionID;
                logData.UnionZoneID = targetData.UnionZoneID;
                logData.UnionName = targetData.UnionName;
                logData.MyUnionID = unionID;
                logData.LogTime = logTime;
                logData.LogState = logState;
                ClientAgentManager.Instance().PostAsyncEvent(serverID, _gameType, new AsyncDataItem(KuaFuEventTypes.AllyLog, unionID, new List<AllyLogData>() { logData }));

                //target
                if (_allyDic.ContainsKey(targetData.UnionID)) _allyDic[targetData.UnionID].Remove(unionID);

                KFAllyData myData = GetUnionData(unionID);
                logData = new AllyLogData();
                logData.UnionID = myData.UnionID;
                logData.UnionZoneID = myData.UnionZoneID;
                logData.UnionName = myData.UnionName;
                logData.MyUnionID = targetID;
                logData.LogTime = logTime;
                logData.LogState = (int)EAlly.AllyRemoveSuccOther;

                if (IsAgent(targetData.ServerID))
                {
                    ClientAgentManager.Instance().PostAsyncEvent(targetData.ServerID, _gameType, new AsyncDataItem(KuaFuEventTypes.AllyRemove, targetID, unionID));
                    ClientAgentManager.Instance().PostAsyncEvent(targetData.ServerID, _gameType, new AsyncDataItem(KuaFuEventTypes.AllyLog, targetID, new List<AllyLogData>() { logData }));
                }
                else
                {
                    AllyPersistence.Instance.DBAllyLogAdd(logData);
                }

                ClientAgentManager.Instance().KFBroadCastAsyncEvent(_gameType, new AsyncDataItem(KuaFuEventTypes.KFAllyRemove, targetID, unionID));
                return (int)EAlly.AllyRemoveSucc;
            }
        }

        //Cancel
        public int OperateCancel(int serverID, int unionID, int targetID, bool isDel = false)
        {
            lock (_Mutex)
            {
                KFAllyData targetData = GetUnionData(targetID);
                if (targetData == null) return (int)EAlly.ENoTargetUnion;

                if (!UnionIsRequest(unionID, targetID)) return (int)EAlly.EFail;
                if (!UnionIsAccept(targetID, unionID)) return (int)EAlly.EFail;

                bool isDelRasult = AllyPersistence.Instance.DBAllyRequestDel(unionID, targetID);
                if (!isDelRasult) return (int)EAlly.EFail;

                DateTime logTime = DateTime.Now;
                int logState = (int)EAlly.AllyCancelSucc;

                //my
                KFAllyData rData = GetRequestData(unionID, targetID);
                if (!isDel) _requestDic[unionID].Remove(rData);
                ClientAgentManager.Instance().PostAsyncEvent(serverID, _gameType, new AsyncDataItem(KuaFuEventTypes.AllyRequestRemove, unionID, targetID));

                //target
                if (_acceptDic.ContainsKey(targetID))
                {
                    KFAllyData aData = GetAcceptData(targetID, unionID);
                    _acceptDic[targetID].Remove(aData);
                }

                if (IsAgent(targetData.ServerID))
                    ClientAgentManager.Instance().PostAsyncEvent(targetData.ServerID, _gameType, new AsyncDataItem(KuaFuEventTypes.AllyAcceptRemove, targetID, unionID));

                return (int)EAlly.AllyCancelSucc;
            }
        }

        //Agree
        public int OperateAgree(int serverID, int unionID, int targetID)
        {
            lock (_Mutex)
            {
                KFAllyData targetData = GetUnionData(targetID);
                if (targetData == null) return (int)EAlly.ENoTargetUnion;

                if (!UnionIsAccept(unionID, targetID)) return (int)EAlly.EFail;
                if (!UnionIsRequest(targetID, unionID)) return (int)EAlly.EFail;

                //num
                int sum = _allyDic[unionID].Count + _requestDic[unionID].Count;
                if (sum >= Consts.AllyNumMax) return (int)EAlly.EAllyMax;

                DateTime logTime = DateTime.Now;
                int logState = (int)EAlly.AllyAgree;

                bool isDB = AllyPersistence.Instance.DBAllyRequestDel(targetID, unionID);
                if (!isDB) return (int)EAlly.EFail;

                isDB = AllyPersistence.Instance.DBAllyAdd(unionID, targetID, logTime);
                if (!isDB) return (int)EAlly.EFail;

                //my
                KFAllyData aData = GetAcceptData(unionID, targetID);
                _acceptDic[unionID].Remove(aData);
                ClientAgentManager.Instance().PostAsyncEvent(serverID, _gameType, new AsyncDataItem(KuaFuEventTypes.AllyAcceptRemove, unionID, targetID));

                _allyDic[unionID].Add(targetID);
                aData.LogTime = logTime;
                aData.LogState = logState;

                AllyData clientMy = new AllyData();
                clientMy.Copy(aData);
                ClientAgentManager.Instance().PostAsyncEvent(serverID, _gameType, new AsyncDataItem(KuaFuEventTypes.Ally, unionID, clientMy, false));

                //target
                if (_requestDic.ContainsKey(targetID))
                {
                    KFAllyData rData = GetRequestData(targetID, unionID);
                    _requestDic[targetID].Remove(rData);
                }

                if (_allyDic.ContainsKey(targetID)) _allyDic[targetID].Add(unionID);

                KFAllyData myData = GetUnionData(unionID);
                myData.LogTime = logTime;
                myData.LogState = logState;

                AllyLogData logData = new AllyLogData();
                logData.UnionID = myData.UnionID;
                logData.UnionZoneID = myData.UnionZoneID;
                logData.UnionName = myData.UnionName;
                logData.MyUnionID = targetID;
                logData.LogTime = logTime;
                logData.LogState = (int)EAlly.AllyAgreeOther;

                AllyData clientTarget = new AllyData();
                clientTarget.Copy(myData);

                if (IsAgent(targetData.ServerID))
                {                             
                    ClientAgentManager.Instance().PostAsyncEvent(targetData.ServerID, _gameType, new AsyncDataItem(KuaFuEventTypes.Ally, targetID, clientTarget, true));
                    ClientAgentManager.Instance().PostAsyncEvent(targetData.ServerID, _gameType, new AsyncDataItem(KuaFuEventTypes.AllyRequestRemove, targetID, unionID));
                    ClientAgentManager.Instance().PostAsyncEvent(targetData.ServerID, _gameType, new AsyncDataItem(KuaFuEventTypes.AllyLog, targetID, new List<AllyLogData>() { logData }));
                }
                else
                {
                    AllyPersistence.Instance.DBAllyLogAdd(logData);
                }

                ClientAgentManager.Instance().KFBroadCastAsyncEvent(_gameType, new AsyncDataItem(KuaFuEventTypes.KFAlly, clientMy, clientTarget));
                return (int)EAlly.AllyAgree;
            }
        }

        //Refuse
        public int OperateRefuse(int serverID, int unionID, int targetID, bool isDel = false)
        {
            lock (_Mutex)
            {
                KFAllyData targetData = GetUnionData(targetID);
                if (targetData == null) return (int)EAlly.ENoTargetUnion;

                if (!UnionIsAccept(unionID, targetID)) return (int)EAlly.EFail;
                if (!UnionIsRequest(targetID, unionID)) return (int)EAlly.EFail;

                bool isDelResult = AllyPersistence.Instance.DBAllyRequestDel(targetID, unionID);
                if (!isDelResult) return (int)EAlly.EFail;

                DateTime logTime = DateTime.Now;
                int logState = (int)EAlly.AllyRefuse;

                //my
                KFAllyData rData = GetAcceptData(unionID, targetID);
                if (!isDel) _acceptDic[unionID].Remove(rData);
                ClientAgentManager.Instance().PostAsyncEvent(serverID, _gameType, new AsyncDataItem(KuaFuEventTypes.AllyAcceptRemove, unionID, targetID));

                //target
                if (_requestDic.ContainsKey(targetID))
                {
                    KFAllyData aData = GetRequestData(targetID, unionID);
                    _requestDic[targetID].Remove(aData);
                }

                KFAllyData myData = GetUnionData(unionID);
                AllyLogData logData = new AllyLogData();
                logData.UnionID = myData.UnionID;
                logData.UnionZoneID = myData.UnionZoneID;
                logData.UnionName = myData.UnionName;
                logData.MyUnionID = targetID;
                logData.LogTime = logTime;
                logData.LogState = (int)EAlly.AllyRefuseOther;

                if (IsAgent(targetData.ServerID))
                {
                    ClientAgentManager.Instance().PostAsyncEvent(targetData.ServerID, _gameType, new AsyncDataItem(KuaFuEventTypes.AllyRequestRemove, targetID, unionID));
                    ClientAgentManager.Instance().PostAsyncEvent(targetData.ServerID, _gameType, new AsyncDataItem(KuaFuEventTypes.AllyLog, targetID, new List<AllyLogData>() { logData }));
                }
                else
                {
                    AllyPersistence.Instance.DBAllyLogAdd(logData);
                }

                return (int)EAlly.AllyRefuse;
            }
        }

        #endregion


        #region 辅助函数

        private KFAllyData GetUnionData(int unionID)
        {
            lock (_Mutex)
            {
                KFAllyData data = null;
                if (!_unionDic.TryGetValue(unionID, out data))
                {
                    data = AllyPersistence.Instance.DBUnionDataGet(unionID);

                    if (data != null) _unionDic.TryAdd(unionID, data);
                }

                return data;
            }
        }

        private KFAllyData GetUnionData(int unionZoneID, string unionName)
        {
            lock (_Mutex)
            {
                KFAllyData resultData = null;

                var data = from item in _unionDic.Values
                           where item.UnionZoneID == unionZoneID && item.UnionName == unionName
                           select item;

                if (data.Any())
                {
                    resultData = data.First<KFAllyData>();
                }
                else
                {
                    resultData = AllyPersistence.Instance.DBUnionDataGet(unionZoneID, unionName);
                    if (resultData != null) _unionDic.TryAdd(resultData.UnionID, resultData);
                }

                return resultData;
            }
        }

        private bool UnionIsAlly(int unionID, int targetID)
        {
            lock (_Mutex)
            {
                List<AllyData> list = AllyList(unionID);
                if (list!=null && list.Count>0)
                {
                    AllyData resultData = list.Find(
                                     delegate(AllyData data) { return data.UnionID == targetID; });

                    if (resultData != null) return true;
                }

                return false;
            }
        }

        private bool UnionIsRequest(int unionID, int targetID)
        {
            lock (_Mutex)
            {
                KFAllyData resultData = GetRequestData(unionID, targetID);
                if (resultData != null) return true;

                return false;
            }
        }

        private KFAllyData GetRequestData(int unionID, int targetID)
        {
            lock (_Mutex)
            {
                List<KFAllyData> list = InitAllyRequestList(unionID);
                if (list != null && list.Count > 0)
                    return list.Find( delegate(KFAllyData data) { return data.UnionID == targetID; });

                return null;
            }
        }

        private bool UnionIsAccept(int unionID, int targetID)
        {
            lock (_Mutex)
            {
                KFAllyData resultData = GetAcceptData(unionID, targetID);
                if (resultData != null) return true;

                return false;
            }
        }

        private KFAllyData GetAcceptData(int unionID, int targetID)
        {
            lock (_Mutex)
            {
                List<KFAllyData> list = InitAllyAcceptList(unionID);
                if (list != null && list.Count > 0)
                    return list.Find(delegate(KFAllyData data) { return data.UnionID == targetID; });

                return null;
            }
        }

        public bool IsAgent(int serverID)
        {
            bool isAgent = ClientAgentManager.Instance().ExistAgent(serverID);
            if (!isAgent)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("UnionAlly时ServerId错误.ServerId:{0}", serverID));
            }

            return isAgent;
        }

        #endregion

    }
}
