using GameServer.Core.Executor;
using KF.Contract.Data;
using KF.Remoting.Data;
using Maticsoft.DBUtility;
using MySql.Data.MySqlClient;
using Server.Tools;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using Tmsk.Contract;
using Tmsk.Contract.Const;
using Tmsk.Tools;
using Tmsk.Tools.Tools;

namespace KF.Remoting
{
    public delegate void UpdateServerInfoToDbProc(int serverId, string ip, int port, int dbPort, int logdbPort, int flags);

    public delegate void RemoveServerInfoToDbProc(int serverId);

    public delegate void UpdateServerGameConfigToDbProc(int serverId, int gameType, int capacity);

    public delegate void RemoveServerGameConfigToDbProc(int serverId, int gameType);

    public class KuaFuServerManagerData
    {
        /// <summary>
        /// Server information
        /// </summary>
        public ConcurrentDictionary<int, KuaFuServerInfo> ServerIdServerInfoDict = new ConcurrentDictionary<int, KuaFuServerInfo>();

        /// <summary>
        /// Server activity configuration information
        /// </summary>
        public ConcurrentDictionary<int, KuaFuServerGameConfig> ServerIdGameConfigDict = new ConcurrentDictionary<int, KuaFuServerGameConfig>();

        /// <summary>
        ///  Server activity configuration information
        /// </summary>
        public ConcurrentDictionary<int, KuaFuServerGameConfig> KuaFuServerIdGameConfigDict = new ConcurrentDictionary<int, KuaFuServerGameConfig>();
    }

    public static class KuaFuServerManager
    {
        #region ConfigInfo

        /// <summary>
        /// Mutex def
        /// </summary>
        public static object Mutex = new object();

        private static object MutexServerList = new object();

        /// <summary>
        /// Load Server List Form Onlinline
        /// </summary>
        public static bool LoadConfigFromServer = false;

        public static int WritePerformanceLogMs = 1;

        public static int MapMaxOnlineCount = 500;

        /// <summary>
        /// Server list get interface address
        /// </summary>
        private static string ServerListUrl;

        /// <summary>
        /// Get the interface address from the cross-server active server list
        /// </summary>
        private static string KuaFuServerListUrl;

        /// <summary>
        /// Platform recharge king
        /// </summary>
        public static string GetPlatChargeKingUrl;

        private static int _ServerListAge;

        private static Thread UpdateServerConfigThread;
        private static Thread CheckServerLoadThread;

        /// <summary>
        /// Generate a unique customer ID lock object
        /// </summary>
        private static object UniqueClientIdMutex = new object();

        /// <summary>
        /// Generate unique customer ID
        /// </summary>
        private static int UniqueClientId = 0;

        public static string ResourcePath;

        public static Dictionary<int, int> KuaFuMapLineDict = new Dictionary<int, int>();

        public static PlatformTypes platformType = PlatformTypes.Max;

        public static bool LoadConfigSuccess = true;

        #endregion ConfigInfo

        /// <summary>
        /// Line status information dictionary of the cross-server mainline map
        /// </summary>
        public static ConcurrentDictionary<IntPairKey, KuaFuLineData> LineMap2KuaFuLineDataDict = new ConcurrentDictionary<IntPairKey, KuaFuLineData>();

        /// <summary>
        /// Line status information dictionary of the cross-server mainline map | trả về trạng thái sẵn sàng hay ko
        /// </summary>
        public static ConcurrentDictionary<IntPairKey, KuaFuLineData> ServerMap2KuaFuLineDataDict = new ConcurrentDictionary<IntPairKey, KuaFuLineData>();

        /// <summary>
        /// Cross-server mainline map data corresponding to the cross-server server ID
        /// </summary>
        public static ConcurrentDictionary<int, List<KuaFuLineData>> KuaFuMapServerIdDict = new ConcurrentDictionary<int, List<KuaFuLineData>>();

        /// <summary>
        /// Cross-server mainline map data corresponding to the map number
        /// </summary>
        public static ConcurrentDictionary<int, List<KuaFuLineData>> MapCode2KuaFuLineDataDict = new ConcurrentDictionary<int, List<KuaFuLineData>>();

        #region Configuration file

        static KuaFuServerManager()
        {
            LoadConfig();
        }

        private static int ConfigPathStructType = 1;

        public static bool CheckConfig()
        {
            string filePath = GetResourcePath("KT_Task/SystemTasks.xml", ResourcePathTypes.Isolate);
            if (!File.Exists(filePath))
            {
                Console.WriteLine("Task not Found {0}", filePath);
                return false;
            }

            return true;
        }

        public static bool LoadConfig()
        {
            lock (Mutex)
            {
                try
                {
                    LoadConfigSuccess = true;

                    int v;
                    string str;

                    #region ReadConfigFile

                    ConfigurationManager.RefreshSection("appSettings");
                    ServerListUrl = ConfigurationManager.AppSettings.Get("ServerListUrl");
                    KuaFuServerListUrl = ConfigurationManager.AppSettings.Get("KuaFuServerListUrl");
                    GetPlatChargeKingUrl = ConfigurationManager.AppSettings.Get("PlatChargeKingUrl");
                    ResourcePath = ConfigurationManager.AppSettings.Get("ResourcePath");
                    ConstData.HTTP_MD5_KEY = ConfigurationManager.AppSettings.Get("MD5Key");

                    Console.WriteLine("ServerListUrl :" + ServerListUrl);
                    Console.WriteLine("KuaFuServerListUrl :" + KuaFuServerListUrl);
                    Console.WriteLine("GetPlatChargeKingUrl :" + GetPlatChargeKingUrl);

                    Console.WriteLine("ResourcePath:" + ResourcePath);

                    Console.WriteLine("Server Key :" + ConstData.HTTP_MD5_KEY);

                    string platformStr = ConfigurationManager.AppSettings.Get("Platform");

                    for (PlatformTypes i = PlatformTypes.Tmsk; i < PlatformTypes.Max; i++)
                    {
                        if (0 == string.Compare(platformStr, i.ToString(), true))
                        {
                            platformType = i;
                            break;
                        }
                    }

                    if (platformType == PlatformTypes.Max)
                    {
                        LogManager.WriteLog(LogTypes.Fatal, "Platform Not Found: Platform");
                        LoadConfigSuccess = false;
                        return false;
                    }

                    string kuaFuMapLineStr = ConfigurationManager.AppSettings.Get("KuaFuMapLine");
                    if (!string.IsNullOrEmpty(kuaFuMapLineStr))
                    {
                        KuaFuMapLineDict.Clear();

                        string[] mapLineStrs = kuaFuMapLineStr.Split('|');
                        foreach (var mapLineStr in mapLineStrs)
                        {
                            KuaFuLineData kuaFuLineData = new KuaFuLineData();
                            string[] mapLineParams = mapLineStr.Split(',');
                            int line;
                            int serverId;
                            if (mapLineParams.Length == 2 && int.TryParse(mapLineParams[0], out line) && int.TryParse(mapLineParams[1], out serverId))
                            {
                                KuaFuMapLineDict[line] = serverId;
                            }
                        }
                    }

                    #endregion ReadConfigFile

                    #region READMAPLINE

                    Console.WriteLine("READ MAP LINES");

                    string fullFileName = GetResourcePath("KT_Map/MapLine.xml", ResourcePathTypes.GameRes);
                    XElement xmlFile = ConfigHelper.Load(fullFileName);

                    str = ConfigHelper.GetElementAttributeValue(xmlFile, "add", "key", "LoadConfigFromServer", "value", "true");
                    if (!bool.TryParse(str, out LoadConfigFromServer))
                    {
                        LoadConfigFromServer = false;
                    }

                    WritePerformanceLogMs = (int)ConfigHelper.GetElementAttributeValueLong(xmlFile, "add", "key", "WritePerformanceLogMs", "value", 10);

                    LineMap2KuaFuLineDataDict.Clear();
                    ServerMap2KuaFuLineDataDict.Clear();
                    KuaFuMapServerIdDict.Clear();
                    MapCode2KuaFuLineDataDict.Clear();
                    IEnumerable<XElement> xmls = ConfigHelper.GetXElements(xmlFile, "MapLine");
                    if (xmls == null)
                    {
                        return false;
                    }
                    foreach (var node in xmls)
                    {
                        MapMaxOnlineCount = (int)ConfigHelper.GetElementAttributeValueLong(node, "MaxNum", 500);
                        str = ConfigHelper.GetElementAttributeValue(node, "Line", "");
                        if (!string.IsNullOrEmpty(str))
                        {
                            string[] mapLineStrs = str.Split('|');
                            foreach (var mapLineStr in mapLineStrs)
                            {
                                KuaFuLineData kuaFuLineData = new KuaFuLineData();
                                string[] mapLineParams = mapLineStr.Split(',');
                                kuaFuLineData.Line = int.Parse(mapLineParams[0]);
                                kuaFuLineData.MapCode = int.Parse(mapLineParams[1]);
                                KuaFuMapLineDict.TryGetValue(kuaFuLineData.Line, out kuaFuLineData.ServerId);

                                kuaFuLineData.MaxOnlineCount = MapMaxOnlineCount;
                                LineMap2KuaFuLineDataDict.TryAdd(new IntPairKey(kuaFuLineData.Line, kuaFuLineData.MapCode), kuaFuLineData);
                                if (kuaFuLineData.ServerId > 0)
                                {
                                    if (ServerMap2KuaFuLineDataDict.TryAdd(new IntPairKey(kuaFuLineData.ServerId, kuaFuLineData.MapCode), kuaFuLineData))
                                    {
                                        List<KuaFuLineData> list = null;
                                        if (!KuaFuMapServerIdDict.TryGetValue(kuaFuLineData.ServerId, out list))
                                        {
                                            list = new List<KuaFuLineData>();
                                            KuaFuMapServerIdDict.TryAdd(kuaFuLineData.ServerId, list);
                                        }

                                        list.Add(kuaFuLineData);

                                        if (!MapCode2KuaFuLineDataDict.TryGetValue(kuaFuLineData.MapCode, out list))
                                        {
                                            list = new List<KuaFuLineData>();
                                            MapCode2KuaFuLineDataDict.TryAdd(kuaFuLineData.MapCode, list);
                                        }

                                        list.Add(kuaFuLineData);
                                    }
                                }
                            }
                        }
                    }

                    #endregion READMAPLINE
                }
                catch (System.Exception ex)
                {
                    LogManager.WriteException(ex.ToString());
                }
            }

            return LoadConfigSuccess;
        }

        public static void StartServerConfigThread()
        {
            if (UpdateServerConfigThread == null)
            {
                UpdateServerConfigThread = new Thread(() =>
                {
                    for (; ; )
                    {
                        try
                        {
                            AsyncFromDataBase();
                            UpdateDataFromServer();

                            Thread.Sleep(20000);
                        }
                        catch (System.Exception ex)
                        {
                            LogManager.WriteExceptionUseCache(ex.ToString());
                        }
                    }
                });
                UpdateServerConfigThread.IsBackground = true;
                UpdateServerConfigThread.Start();
            }

            if (CheckServerLoadThread == null)
            {
                CheckServerLoadThread = new Thread(() =>
                {
                    for (; ; )
                    {
                        try
                        {
                            UpdateServerLoad();

                            Thread.Sleep(8000);
                        }
                        catch (System.Exception ex)
                        {
                            LogManager.WriteExceptionUseCache(ex.ToString());
                        }
                    }
                });
                CheckServerLoadThread.IsBackground = true;
                CheckServerLoadThread.Start();
            }
        }

        private static void UpdateServerLoad()
        {
            lock (Mutex)
            {
                foreach (var srv in _ServerIdServerInfoDict.Values)
                {
                    int load = 0, state = 0;
                    ClientAgentManager.Instance().GetServerState(srv.ServerId, out state, out load);
                    if (load != srv.Load || state != srv.State)
                    {
                        try
                        {
                            DbHelperMySQL.ExecuteSql(string.Format("update ignore t_server_info set `load`={0},`state`={1} where `serverid`={2}", load, state, srv.ServerId));
                            srv.Load = load;
                            srv.State = state;
                        }
                        catch (System.Exception ex)
                        {
                            LogManager.WriteExceptionUseCache(ex.ToString());
                        }
                    }
                }

                Dictionary<int, GameTypeStaticsData> statics = ClientAgentManager.Instance().GetGameTypeStatics();
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("DELETE FROM t_server_load;");
                if (statics != null)
                {
                    foreach (var kvp in statics)
                    {
                        sb.AppendFormat("INSERT INTO t_server_load(gametype, server_alived, fuben_alived, role_signup_count, role_start_game_count,tip) VALUES({0}, {1}, {2}, {3}, {4},'{5}');",
                            (int)kvp.Key, kvp.Value.ServerAlived, kvp.Value.FuBenAlived, kvp.Value.SingUpRoleCount, kvp.Value.StartGameRoleCount, ((GameTypes)kvp.Key).ToString());
                        sb.AppendLine();
                    }
                }

                try
                {
                    DbHelperMySQL.ExecuteSql(sb.ToString());
                }
                catch (System.Exception ex)
                {
                    LogManager.WriteExceptionUseCache(ex.ToString());
                }
            }
        }

        private static void AsyncFromDataBase()
        {
            try
            {
                object ageObj = DbHelperMySQL.GetSingle("select value from t_async where id = 1");
                if (null != ageObj)
                {
                    int age = (int)ageObj;
                    if (age > _ServerListAge)
                    {
                        HashSet<int> existAllIds = new HashSet<int>();
                        HashSet<int> existKfIds = new HashSet<int>();

                        MySqlDataReader sdr = DbHelperMySQL.ExecuteReader("select * from t_server_info");
                        while (sdr.Read())
                        {
                            try
                            {
                                KuaFuServerInfo serverInfo = new KuaFuServerInfo()
                                {
                                    ServerId = (int)Convert.ToInt32(sdr["serverid"]),
                                    Ip = sdr["ip"].ToString(),
                                    Port = (int)Convert.ToInt32(sdr["port"]),
                                    DbIp = sdr["dbip"].ToString(),
                                    DbPort = (int)Convert.ToInt32(sdr["dbport"]),
                                    LogDbIp = sdr["logdbip"].ToString(),
                                    LogDbPort = (int)Convert.ToInt32(sdr["logdbport"]),
                                    State = (int)Convert.ToInt32(sdr["state"]),
                                    //Load = (int)Convert.ToInt32(sdr["load"]),
                                    Flags = (int)Convert.ToInt32(sdr["flags"]),
                                    Age = age,
                                };

                                LogManager.WriteLog(LogTypes.Error, "Loading Server :" + serverInfo.ServerId + "| Ip :" + serverInfo.Ip + "| DbIp :" + serverInfo.DbPort);
                                _ServerIdServerInfoDict[serverInfo.ServerId] = serverInfo;
                                existAllIds.Add(serverInfo.ServerId);

                                if ((serverInfo.Flags & ServerFlags.KuaFuServer) != 0)
                                {
                                    existKfIds.Add(serverInfo.ServerId);
                                }
                            }
                            catch (System.Exception ex)
                            {
                                LogManager.WriteExceptionUseCache(ex.ToString());
                            }
                        }

                        sdr.Close();

                        lock (Mutex)
                        {
                            foreach (var id in _ServerIdServerInfoDict.Keys.ToList())
                            {
                                if (!existAllIds.Contains(id))
                                {
                                    KuaFuServerInfo tmp;
                                    _ServerIdServerInfoDict.TryRemove(id, out tmp);
                                }
                            }

                            _ServerListAge = age;
                        }

                        ClientAgentManager.Instance().SetAllKfServerId(existKfIds);
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.WriteExceptionUseCache(ex.ToString());
            }
        }

        public enum ResourcePathTypes
        {
            Application,
            GameRes,
            Isolate,
            Map,
            MapConfig,
        }

        public static string GetResourcePath(string fileName, ResourcePathTypes resType)
        {
            return ResourcePath + "\\" + fileName;
        }

        #endregion Configuration file

        #region 统一的服务器列表管理方式(保留将来使用)

        private static GetKuaFuServerListRequestData KuaFuServerListRequestData = new GetKuaFuServerListRequestData();

        private static ConcurrentDictionary<int, KuaFuServerInfo> _ServerIdServerInfoDict = new ConcurrentDictionary<int, KuaFuServerInfo>();

        #endregion 统一的服务器列表管理方式(保留将来使用)

        #region 服务器列表管理

        public static List<KuaFuServerInfo> GetKuaFuServerInfoData(int age)
        {
            List<KuaFuServerInfo> result = null;
            lock (Mutex)
            {
                if (age != _ServerListAge)
                {
                    result = _ServerIdServerInfoDict.Values.ToList();
                }
            }

            return result;
        }

        public static bool UpdateDataFromServer()
        {
            if (Monitor.TryEnter(MutexServerList))
            {
                try
                {
                    int nextServerListAge = DataHelper2.UnixSecondsNow();
                    int nextServerGameConfigAge = DataHelper2.UnixSecondsNow();
                    if (Math.Abs(nextServerListAge - _ServerListAge) < 3)
                    {
                        return false;
                    }

                    if (!LoadConfigFromServer)
                    {
                        return false;
                    }

                    if (string.IsNullOrEmpty(KuaFuServerListUrl) || string.IsNullOrEmpty(ServerListUrl))
                    {
                        return false;
                    }

                    ClientServerListData clientListData = new ClientServerListData();
                    clientListData.lTime = TimeUtil.NOW();
                    clientListData.strMD5 = MD5Helper.get_md5_string(ConstData.HTTP_MD5_KEY + clientListData.lTime.ToString());
                    byte[] clientBytes = DataHelper2.ObjectToBytes<ClientServerListData>(clientListData);
                    byte[] responseData = WebHelper.RequestByPost(ServerListUrl, clientBytes, 2000, 30000);
                    if (responseData == null)
                    {
                        return false;
                    }

                    bool serverInfoChanged = false;
                    HashSet<int> existAllIds = new HashSet<int>();
                    HashSet<int> existKfIds = new HashSet<int>();
                    BuffServerListData serverListResponseData;
                    BuffServerListData kuaFuServerListResponseData;
                    serverListResponseData = DataHelper2.BytesToObject<BuffServerListData>(responseData, 0, responseData.Length);
                    if (null == serverListResponseData || null == serverListResponseData.listServerData || serverListResponseData.listServerData.Count == 0)
                    {
                        return false;
                    }

                    clientListData = new ClientServerListData();
                    clientListData.lTime = TimeUtil.NOW();
                    clientListData.strMD5 = MD5Helper.get_md5_string(ConstData.HTTP_MD5_KEY + clientListData.lTime.ToString());
                    clientBytes = DataHelper2.ObjectToBytes<ClientServerListData>(clientListData);
                    responseData = WebHelper.RequestByPost(KuaFuServerListUrl, clientBytes, 2000, 30000);
                    if (responseData == null)
                    {
                        return false;
                    }

                    kuaFuServerListResponseData = DataHelper2.BytesToObject<BuffServerListData>(responseData, 0, responseData.Length);
                    if (null == kuaFuServerListResponseData || null == kuaFuServerListResponseData.listServerData || kuaFuServerListResponseData.listServerData.Count == 0)
                    {
                        return false;
                    }

                    if (null != serverListResponseData && null != serverListResponseData.listServerData)
                    {
                        lock (Mutex)
                        {
                            foreach (var item in serverListResponseData.listServerData)
                            {
                                existAllIds.Add(item.nServerID);
                                KuaFuServerInfo data;
                                if (UpdateServerInfo(item, _ServerListAge, ServerFlags.NormalServer, out data, _ServerIdServerInfoDict))
                                {
                                    serverInfoChanged = true;
                                }
                            }
                        }
                    }

                    if (null != kuaFuServerListResponseData && null != kuaFuServerListResponseData.listServerData)
                    {
                        lock (Mutex)
                        {
                            foreach (var item in kuaFuServerListResponseData.listServerData)
                            {
                                existAllIds.Add(item.nServerID);
                                KuaFuServerInfo data;
                                if (UpdateServerInfo(item, _ServerListAge, ServerFlags.KuaFuServer, out data, _ServerIdServerInfoDict))
                                {
                                    serverInfoChanged = true;
                                }
                            }

                            foreach (var id in _ServerIdServerInfoDict.Keys)
                            {
                                if (!existAllIds.Contains(id))
                                {
                                    RemoveServerInfo(id, _ServerIdServerInfoDict);
                                    serverInfoChanged = true;
                                }
                            }

                            if (serverInfoChanged)
                            {
                                _ServerListAge = nextServerListAge;
                            }

                            foreach (var item in _ServerIdServerInfoDict.Values)
                            {
                                if ((item.Flags & ServerFlags.KuaFuServer) > 0)
                                {
                                    existKfIds.Add(item.ServerId);
                                }

                                item.Age = _ServerListAge;
                            }
                        }
                    }

                    ClientAgentManager.Instance().SetAllKfServerId(existKfIds);
                }
                catch (System.Exception ex)
                {
                    LogManager.WriteExceptionUseCache(ex.ToString());
                    return false;
                }
                finally
                {
                    Monitor.Exit(MutexServerList);
                }
            }

            return true;
        }

        public static bool UpdateServerInfo(BuffServerInfo item, int ServerListAge, int serverFlags, out KuaFuServerInfo data, ConcurrentDictionary<int, KuaFuServerInfo> ServerIdServerInfoDict)
        {
            bool serverInfoChanged = false;
            if (!ServerIdServerInfoDict.TryGetValue(item.nServerID, out data))
            {
                data = new KuaFuServerInfo()
                {
                    ServerId = item.nServerID,
                    Age = ServerListAge,
                    Flags = serverFlags,
                };

                ServerIdServerInfoDict[item.nServerID] = data;
                serverInfoChanged = true;
            }

            if (data.Ip != item.strURL || data.Port != item.nServerPort)
            {
                data.Ip = data.DbIp = data.LogDbIp = item.strURL;
                data.Port = item.nServerPort;
                data.DbPort = item.nServerPort + 10000;
                data.LogDbPort = item.nServerPort + 20000;
                data.Flags = serverFlags;
                data.Age = ServerListAge;
                serverInfoChanged = true;
            }

            if (serverInfoChanged)
            {
                try
                {
                    DbHelperMySQL.ExecuteSql(string.Format("INSERT INTO t_server_info(serverid,ip,port,dbip,dbport,logdbip,logdbport,state,age,flags) " +
                            "VALUES({0},'{1}',{2},'{1}',{3},'{1}',{4},0,0,{5}) " +
                            "ON DUPLICATE KEY UPDATE `ip`='{1}',port={2},dbip='{1}',dbport={3},logdbip='{1}',logdbport={4},flags={5}",
                            data.ServerId, data.Ip, data.Port, data.DbPort, data.LogDbPort, data.Flags));
                }
                catch (Exception ex)
                {
                    LogManager.WriteException(ex.Message);
                }
            }

            return serverInfoChanged;
        }

        public static void RemoveServerInfo(int serverId, ConcurrentDictionary<int, KuaFuServerInfo> ServerIdServerInfoDict)
        {
            KuaFuServerInfo data;
            ServerIdServerInfoDict.TryRemove(serverId, out data);
            try
            {
                DbHelperMySQL.ExecuteSql(string.Format("delete from t_server_info where `serverid`={0}", serverId));
            }
            catch (Exception ex)
            {
                LogManager.WriteException(ex.Message);
            }
        }

        public static bool UpdateServerGameConfig(int serverId, int gameType, int capacity, ConcurrentDictionary<int, KuaFuServerGameConfig> KuaFuServerIdGameConfigDict)
        {
            bool serverGameConfigChanged = false;
            KuaFuServerGameConfig data;
            if (!KuaFuServerIdGameConfigDict.TryGetValue(serverId, out data))
            {
                data = new KuaFuServerGameConfig()
                {
                    ServerId = serverId,
                    GameType = gameType,
                };

                KuaFuServerIdGameConfigDict[serverId] = data;
                serverGameConfigChanged = true;
            }
            else if (data.Capacity != capacity)
            {
                data.Capacity = capacity;
                serverGameConfigChanged = true;
            }

            return serverGameConfigChanged;
        }

        public static int GetUniqueClientId()
        {
            int uniqueClientId = DataHelper2.UnixSecondsNow();
            lock (UniqueClientIdMutex)
            {
                UniqueClientId++;
                if (UniqueClientId < uniqueClientId)
                {
                    UniqueClientId = uniqueClientId;
                }

                uniqueClientId = UniqueClientId;
            }

            return uniqueClientId;
        }

        #endregion 服务器列表管理

        #region 跨服地图

        public static int EnterKuaFuMapLine(int line, int mapCode)
        {
            lock (Mutex)
            {
                KuaFuLineData kuaFuLineData;
                if (LineMap2KuaFuLineDataDict.TryGetValue(new IntPairKey(line, mapCode), out kuaFuLineData))
                {
                    if (kuaFuLineData.OnlineCount < kuaFuLineData.MaxOnlineCount)
                    {
                        kuaFuLineData.OnlineCount++;
                        return kuaFuLineData.ServerId;
                    }
                }
            }

            return 0;
        }

        public static void UpdateKuaFuLineData(int serverId, Dictionary<int, int> mapClientCountDict)
        {
            if (null != mapClientCountDict)
            {
                lock (Mutex)
                {
                    KuaFuLineData kuaFuLineData;
                    foreach (var kv in mapClientCountDict)
                    {
                        if (ServerMap2KuaFuLineDataDict.TryGetValue(new IntPairKey(serverId, kv.Key), out kuaFuLineData))
                        {
                            kuaFuLineData.OnlineCount = kv.Value;
                            kuaFuLineData.State = 1;
                        }
                    }
                }
            }
        }

        public static void UpdateKuaFuMapLineState(int serverId, int state)
        {
            List<KuaFuLineData> list = null;
            lock (Mutex)
            {
                if (KuaFuMapServerIdDict.TryGetValue(serverId, out list))
                {
                    foreach (var kuaFuLineData in list)
                    {
                        kuaFuLineData.State = state;
                    }
                }
            }
        }

        public static List<KuaFuLineData> GetKuaFuLineDataList(int mapCode)
        {
            List<KuaFuLineData> list;
            lock (Mutex)
            {
                if (MapCode2KuaFuLineDataDict.TryGetValue(mapCode, out list))
                {
                    return list;
                }
            }

            return null;
        }

        #endregion 跨服地图

        public static void OnStopServer()
        {
            try
            {
                CoupleArenaService.getInstance().OnStopServer();
                CoupleWishService.getInstance().OnStopServer();
            }
            catch (Exception ex)
            {
                LogManager.WriteException(ex.Message);
            }
        }
    }
}