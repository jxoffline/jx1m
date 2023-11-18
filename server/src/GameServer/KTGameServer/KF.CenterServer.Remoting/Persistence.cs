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

namespace KF.Remoting
{
    public class Persistence
    {
        private Persistence() { }

        public static readonly Persistence Instance = new Persistence();

        /// <summary>
        /// 保护数据的互斥对象
        /// </summary>
        public object Mutex = new object();

        /// <summary>
        /// 是否已初始化
        /// </summary>
        private bool Initialized = false;

        /// <summary>
        /// 是否从服务器(远程)加载服务器列表
        /// </summary>
        private bool LoadConfigFromServer = false;

        /// <summary>
        /// 数据同步信息
        /// </summary>
        public int ServerListAge;

        public int ServerGameConfigAge;

        public int GameDataAge;

        public int WritePerformanceLogMs = 1;

        public int SignUpWaitSecs1 = 30;
        public int SignUpWaitSecs2 = 60;

        /// <summary>
        /// 从远程获取服务器列表是的请求信息(扩展方法,暂未启用)
        /// </summary>
        GetKuaFuServerListRequestData KuaFuServerListRequestData = new GetKuaFuServerListRequestData();

        /// <summary>
        /// 服务器列表获取接口地址
        /// </summary>
        private string ServerListUrl;

        /// <summary>
        /// 跨服活动服务器列表获取接口地址
        /// </summary>
        public string KuaFuServerListUrl;

        /// <summary>
        /// 幻影寺院活动配置数据
        /// </summary>
        public GameConfigData HuanYingSiYuanGameConfigData = new GameConfigData();

        /// <summary>
        /// 服务器信息
        /// </summary>
        public ConcurrentDictionary<int, KuaFuServerInfo> ServerIdServerInfoDict = new ConcurrentDictionary<int, KuaFuServerInfo>();

        /// <summary>
        /// 服务器活动配置信息
        /// </summary>
        public ConcurrentDictionary<int, KuaFuServerGameConfig> ServerIdGameConfigDict = new ConcurrentDictionary<int, KuaFuServerGameConfig>();

        /// <summary>
        /// 服务器活动配置信息
        /// </summary>
        public ConcurrentDictionary<int, KuaFuServerGameConfig> KuaFuServerIdGameConfigDict = new ConcurrentDictionary<int, KuaFuServerGameConfig>();

        /// <summary>
        /// 数据库更新队列,异步写数据库
        /// </summary>
        private Queue<GameFuBenStateDbItem> GameFuBenStateDbItemQueue = new Queue<GameFuBenStateDbItem>();

        /// <summary>
        /// 服务器压力信息
        /// </summary>
        public ServerLoadContext ServerLoadContext = new ServerLoadContext();

        /// <summary>
        /// 初始化配置文件
        /// </summary>
        public void InitConfig()
        {
            try
            {
                int v;
                ConfigurationManager.RefreshSection("appSettings");
                string huanYingSiYuanRoleCountTotal = ConfigurationManager.AppSettings.Get("HuanYingSiYuanRoleCountTotal");
                if (!string.IsNullOrEmpty(huanYingSiYuanRoleCountTotal) && int.TryParse(huanYingSiYuanRoleCountTotal, out v))
                {
                    Consts.HuanYingSiYuanRoleCountTotal = v;
                }

                string huanYingSiYuanRoleCountPerSide = ConfigurationManager.AppSettings.Get("HuanYingSiYuanRoleCountPerSide");
                if (!string.IsNullOrEmpty(huanYingSiYuanRoleCountPerSide) && int.TryParse(huanYingSiYuanRoleCountPerSide, out v))
                {
                    Consts.HuanYingSiYuanRoleCountPerSide = v;
                }

                ServerListUrl = ConfigurationManager.AppSettings.Get("ServerListUrl");
                KuaFuServerListUrl = ConfigurationManager.AppSettings.Get("KuaFuServerListUrl");
                ConstData.HTTP_MD5_KEY = ConfigurationManager.AppSettings.Get("KuaFuServerListUrl");

                string loadConfigFromDataBaseStr = ConfigurationManager.AppSettings.Get("LoadConfigFromDataBase");
                if (string.IsNullOrEmpty(loadConfigFromDataBaseStr) || !bool.TryParse(loadConfigFromDataBaseStr, out LoadConfigFromServer))
                {
                    LoadConfigFromServer = false;
                }

                string writePerformanceLogMs = ConfigurationManager.AppSettings.Get("WritePerformanceLogMs");
                if (string.IsNullOrEmpty(writePerformanceLogMs) || !int.TryParse(writePerformanceLogMs, out WritePerformanceLogMs))
                {
                    WritePerformanceLogMs = 1;
                }

                string signUpWaitSecs1 = ConfigurationManager.AppSettings.Get("SignUpWaitSecs1");
                if (string.IsNullOrEmpty(signUpWaitSecs1) || !int.TryParse(signUpWaitSecs1, out SignUpWaitSecs1))
                {
                    SignUpWaitSecs1 = 30;
                }

                string signUpWaitSecs2 = ConfigurationManager.AppSettings.Get("SignUpWaitSecs2");
                if (string.IsNullOrEmpty(signUpWaitSecs2) || !int.TryParse(signUpWaitSecs2, out SignUpWaitSecs2))
                {
                    SignUpWaitSecs2 = 60;
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
                if (ms > WritePerformanceLogMs)
                {
                    LogManager.WriteLog(LogTypes.Info, "执行时间(ms):" + ms);
                }
            }
            catch
            {
            	
            }
        }

        /// <summary>
        /// 保存服务器负载信息
        /// </summary>
        public void SaveServerLoadData()
        {
            try
            {
                string sql = string.Format("INSERT INTO t_server_load(gametype, server_alived, fuben_alived, role_signup_count, role_start_game_count) VALUES({0}, {1}, {2}, {3}, {4}) ON DUPLICATE KEY UPDATE server_alived={1},fuben_alived={2},role_signup_count={3},role_start_game_count={4}",
                                                (int)GameTypes.HuanYingSiYuan, ServerLoadContext.AlivedServerCount, ServerLoadContext.AlivedGameFuBenCount, ServerLoadContext.SignUpRoleCount, ServerLoadContext.StartGameRoleCount);
                DbHelperMySQL.ExecuteSql(sql);
            }
            catch (System.Exception ex)
            {
                LogManager.WriteExceptionUseCache(ex.ToString());
            }
        }

        /// <summary>
        /// 更新服务器服务器配置和服务器列表
        /// </summary>
        public void UpdateServerConfig()
        {
            if (Initialized)
            {
                AsyncFromDataBase();
                if (!LoadConfigFromServer)
                {
                    UpdateDataFromServer2();
                }
            }
        }

        /// <summary>
        /// 从数据库同步配置信息
        /// </summary>
        public void AsyncFromDataBase()
        {
            try
            {
                //刷新服务器列表
                object ageObj = DbHelperMySQL.GetSingle("select value from t_async where id = 1");
                if (null != ageObj)
                {
                    int age = (int)ageObj;
                    if (age > ServerListAge)
                    {
                        LoadServerInfoFromDataBase(age);
                    }
                }


                //刷新服务器活动配置
                ageObj = DbHelperMySQL.GetSingle("select value from t_async where id = 2");
                if (null != ageObj)
                {
                    int age = (int)ageObj;
                    if (age > ServerGameConfigAge)
                    {
                        LoadServerGameConfigFromDataBase(age);
                    }
                }

                //刷新服务器活动配置
                ageObj = DbHelperMySQL.GetSingle("select value from t_async where id = 3");
                if (null != ageObj)
                {
                    int age = (int)ageObj;
                    if (age > GameDataAge)
                    {
                        LoadGameConfigFromDataBase(age);
                    }
                }

            }
            catch (Exception ex)
            {
                LogManager.WriteExceptionUseCache(ex.ToString());
            }
        }

        /// <summary>
        /// 从数据库t_server_game_config加载数据
        /// </summary>
        /// <param name="dbAge"></param>
        public void LoadServerGameConfigFromDataBase(int dbAge)
        {
            try
            {
                HashSet<int> existIds = new HashSet<int>();
                MySqlDataReader sdr = DbHelperMySQL.ExecuteReader("select * from t_server_game_config where gametype=" + (int)GameTypes.HuanYingSiYuan);
                while (sdr.Read())
                {
                    KuaFuServerGameConfig serverGame = new KuaFuServerGameConfig()
                    {
                        ServerId = (int)Convert.ToInt32(sdr["serverid"]),
                        GameType = (int)Convert.ToInt32(sdr["gametype"]),
                        Capacity = (int)Convert.ToInt32(sdr["capacity"]),
                    };

                    ServerIdGameConfigDict[serverGame.ServerId] = serverGame;
                    KuaFuServerIdGameConfigDict[serverGame.ServerId] = serverGame;
                    existIds.Add(serverGame.ServerId);
                }

                lock (Mutex)
                {
                    foreach (var id in ServerIdGameConfigDict.Keys.ToList())
                    {
                        if (!existIds.Contains(id))
                        {
                            KuaFuServerGameConfig tmp;
                            ServerIdGameConfigDict.TryRemove(id, out tmp);
                            KuaFuServerIdGameConfigDict.TryRemove(id, out tmp);
                        }
                    }

                    ServerGameConfigAge = dbAge;
                }
            }
            catch (System.Exception ex)
            {
                LogManager.WriteExceptionUseCache(ex.ToString());
                return;
            }
        }

        /// <summary>
        /// 从数据库t_server_info加载数据
        /// </summary>
        /// <param name="dbAge"></param>
        public void LoadServerInfoFromDataBase(int dbAge)
        {
            HashSet<int> existIds = new HashSet<int>();

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
                        Age = dbAge,
                    };

                    ServerIdServerInfoDict[serverInfo.ServerId] = serverInfo;
                    existIds.Add(serverInfo.ServerId);
                }
                catch (System.Exception ex)
                {
                    LogManager.WriteExceptionUseCache(ex.ToString());
                }
            }

            lock (Mutex)
            {
                foreach (var id in ServerIdServerInfoDict.Keys.ToList())
                {
                    if (!existIds.Contains(id))
                    {
                        KuaFuServerInfo tmp;
                        ServerIdServerInfoDict.TryRemove(id, out tmp);
                    }
                }

                ServerListAge = dbAge;
            }
        }

        /// <summary>
        /// 从数据库t_game_config加载数据
        /// </summary>
        /// <param name="dbAge"></param>
        public void LoadGameConfigFromDataBase(int dbAge)
        {
            MySqlDataReader sdr = DbHelperMySQL.ExecuteReader(string.Format("select * from t_game_config where gametype={0}", (int)GameTypes.HuanYingSiYuan));
            if (sdr.Read())
            {
                HuanYingSiYuanGameConfigData.GameType = (int)Convert.ToInt32(sdr["gametype"]);
                HuanYingSiYuanGameConfigData.MaxCount = (int)Convert.ToInt32(sdr["maxcount"]);
                HuanYingSiYuanGameConfigData.DayCount = (int)Convert.ToInt32(sdr["daycount"]);
                HuanYingSiYuanGameConfigData.Date = (DateTime)Convert.ToDateTime(sdr["date"]);
            }

            DateTime date = DateTime.Now.Date;
            if (HuanYingSiYuanGameConfigData.Date != date)
            {
                dbAge++;
                HuanYingSiYuanGameConfigData.Date = date;
                HuanYingSiYuanGameConfigData.DayCount = 0;
                DbHelperMySQL.ExecuteSql(string.Format("update t_game_config set daycount=0,date='{1}' where gametype={0}", (int)GameTypes.HuanYingSiYuan, date.ToString("yyyy-MM-dd")));
                DbHelperMySQL.ExecuteSql("update t_async set value=value+1 where id = 3");
            }

            GameDataAge = dbAge;
        }

        /// <summary>
        /// 从中心后台检查更新配置
        /// </summary>
        /// <returns></returns>
        public bool UpdateDataFromServer()
        {
            try
            {
                byte[] bytes = DataHelper.ObjectToBytes<GetKuaFuServerListRequestData>(KuaFuServerListRequestData);
                byte[] responseData = WebHelper.RequestByPost(ServerListUrl, bytes, 2000, 30000);
                if (responseData == null)
                {
                    return false;
                }

                GetKuaFuServerListResponseData kuaFuServerListResponseData = DataHelper.BytesToObject<GetKuaFuServerListResponseData>(responseData, 0, responseData.Length);
                if (null != kuaFuServerListResponseData)
                {
                    lock (Mutex)
                    {
                        bool serverInfoChanged = false;
                        bool serverGameConfigChanged = false;

                        KuaFuServerListRequestData.ServerListAge = kuaFuServerListResponseData.ServerListAge;
                        KuaFuServerListRequestData.GameConfigAge = kuaFuServerListResponseData.GameConfigAge;
                        KuaFuServerListRequestData.ServerGameConfigAge = kuaFuServerListResponseData.ServerGameConfigAge;

                        HashSet<int> existIds = new HashSet<int>();
                        if (null != kuaFuServerListResponseData.ServerList)
                        {
                            foreach (var item in kuaFuServerListResponseData.ServerList)
                            {
                                KuaFuServerInfo data;
                                if (!ServerIdServerInfoDict.TryGetValue(item.ServerId, out data))
                                {
                                    serverInfoChanged = true;
                                    data = new KuaFuServerInfo()
                                    {
                                        ServerId = item.ServerId,
                                    };
                                }
                                else if (data.Ip != item.Ip || data.Port != item.Port || data.DbPort != item.DbPort || data.LogDbPort != item.LogDbPort || data.Flags != item.Flags)
                                {
                                    data.Ip = item.Ip;
                                    data.Port = item.Port;
                                    data.DbPort = item.DbPort;
                                    data.LogDbPort = item.LogDbPort;
                                    data.Flags = item.Flags;
                                    data.Age = ServerListAge;
                                    UpdateServerInfo(item.ServerId, item.Ip, item.Port, item.DbPort, item.LogDbPort, item.Flags);
                                    serverInfoChanged = true;
                                }

                                existIds.Add(item.ServerId);
                            }

                            foreach (var id in ServerIdServerInfoDict.Keys)
                            {
                                if (!existIds.Contains(id))
                                {
                                    KuaFuServerInfo data;
                                    ServerIdServerInfoDict.TryRemove(id, out data);
                                    RemoveServerInfo(id);
                                    serverInfoChanged = true;
                                }
                            }

                            if (serverInfoChanged)
                            {
                                ServerListAge++;
                            }

                            existIds.Clear();
                        }

                        if (true)
                        {
                            //默认所有跨服服务器都允许分配幻影寺院活动
                            foreach (var item in ServerIdServerInfoDict.Values)
                            {
                                if ((item.Flags & ServerFlags.KuaFuServer) > 0)
                                {
                                    UpdateServerGameConfig(item.ServerId, (int)GameTypes.HuanYingSiYuan, 250, ref serverGameConfigChanged);
                                    existIds.Add(item.ServerId);
                                }
                            }
                        }
                        else
                        {
                            //添加明确的分配配置
                            if (null != kuaFuServerListResponseData.ServerGameConfigList)
                            {
                                foreach (var item in kuaFuServerListResponseData.ServerGameConfigList)
                                {
                                    UpdateServerGameConfig(item.ServerId, item.GameType, item.Capacity, ref serverGameConfigChanged);
                                    existIds.Add(item.ServerId);
                                }
                            }
                        }

                        //从列表移除已不存在的服务器
                        foreach (var id in ServerIdGameConfigDict.Keys)
                        {
                            if (!existIds.Contains(id))
                            {
                                RemoveServerGameConfig(id, ref serverGameConfigChanged);
                                RemoveServerGameConfigToDb(id, (int)GameTypes.HuanYingSiYuan);
                            }
                        }

                        if (serverGameConfigChanged)
                        {
                            ServerGameConfigAge++;
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                LogManager.WriteExceptionUseCache(ex.ToString());
                return false;
            }

            return true;
        }

        /// <summary>
        /// 从中心后台检查更新配置
        /// </summary>
        /// <returns></returns>
        public bool UpdateDataFromServer2()
        {
            try
            {
                //确认是否配置了获取接口地址
                if (string.IsNullOrEmpty(KuaFuServerListUrl) || string.IsNullOrEmpty(ServerListUrl))
                {
                    return false;
                }

                //首先获取普通服务器列表
                //1其次获取跨服活动服务器列表
                ClientServerListData clientListData = new ClientServerListData();
                clientListData.lTime = TimeUtil.NOW();
                clientListData.strMD5 = MD5Helper.get_md5_string(ConstData.HTTP_MD5_KEY + clientListData.lTime.ToString());
                byte[] clientBytes = DataHelper.ObjectToBytes<ClientServerListData>(clientListData);
                byte[] responseData = WebHelper.RequestByPost(ServerListUrl, clientBytes, 2000, 30000);
                if (responseData == null)
                {
                    return false;
                }

                HashSet<int> existIds = new HashSet<int>();
                BuffServerListData kuaFuServerListResponseData = DataHelper.BytesToObject<BuffServerListData>(responseData, 0, responseData.Length);
                if (null != kuaFuServerListResponseData && null != kuaFuServerListResponseData.listServerData)
                {
                    lock (Mutex)
                    {
                        bool serverInfoChanged = false;
                        foreach (var item in kuaFuServerListResponseData.listServerData)
                        {
                            KuaFuServerInfo data;
                            if (!ServerIdServerInfoDict.TryGetValue(item.nServerID, out data))
                            {
                                serverInfoChanged = true;
                                data = new KuaFuServerInfo()
                                {
                                    ServerId = item.nServerID,
                                };

                                ServerIdServerInfoDict[item.nServerID] = data;
                            }

                            if (data.Ip != item.strURL || data.Port != item.nServerPort)
                            {
                                data.Ip = data.DbIp = data.LogDbIp = item.strURL;
                                data.Port = item.nServerPort;
                                data.DbPort = item.nServerPort + 10000;
                                data.LogDbPort = item.nServerPort + 20000;
                                data.Flags = ServerFlags.NormalServerOnly;
                                data.Age = ServerListAge;
                                UpdateServerInfo(data.ServerId, data.Ip, data.Port, data.DbPort, data.LogDbPort, data.Flags);
                                serverInfoChanged = true;
                            }

                            existIds.Add(item.nServerID);
                        }
                    }
                }

                //其次获取跨服活动服务器列表
                clientListData = new ClientServerListData();
                clientListData.lTime = TimeUtil.NOW();
                clientListData.strMD5 = MD5Helper.get_md5_string(ConstData.HTTP_MD5_KEY + clientListData.lTime.ToString());
                clientBytes = DataHelper.ObjectToBytes<ClientServerListData>(clientListData);
                responseData = WebHelper.RequestByPost(KuaFuServerListUrl, clientBytes, 2000, 30000);
                if (responseData == null)
                {
                    return false;
                }

                kuaFuServerListResponseData = DataHelper.BytesToObject<BuffServerListData>(responseData, 0, responseData.Length);
                if (null != kuaFuServerListResponseData && null != kuaFuServerListResponseData.listServerData)
                {
                    lock (Mutex)
                    {
                        bool serverInfoChanged = false;
                        bool serverGameConfigChanged = false;
                        foreach (var item in kuaFuServerListResponseData.listServerData)
                        {
                            KuaFuServerInfo data;
                            if (!ServerIdServerInfoDict.TryGetValue(item.nServerID, out data))
                            {
                                serverInfoChanged = true;
                                data = new KuaFuServerInfo()
                                {
                                    ServerId = item.nServerID,
                                };

                                ServerIdServerInfoDict[item.nServerID] = data;
                            }

                            if (data.Ip != item.strURL || data.Port != item.nServerPort)
                            {
                                data.Ip = data.DbIp = data.LogDbIp = item.strURL;
                                data.Port = item.nServerPort;
                                data.DbPort = item.nServerPort + 10000;
                                data.LogDbPort = item.nServerPort + 20000;
                                data.Flags = ServerFlags.KuaFuServer;
                                data.Age = ServerListAge;
                                UpdateServerInfo(data.ServerId, data.Ip, data.Port, data.DbPort, data.LogDbPort, data.Flags);
                                serverInfoChanged = true;
                            }

                            existIds.Add(item.nServerID);
                        }

                        foreach (var id in ServerIdServerInfoDict.Keys)
                        {
                            if (!existIds.Contains(id))
                            {
                                KuaFuServerInfo data;
                                ServerIdServerInfoDict.TryRemove(id, out data);
                                RemoveServerInfo(id);
                                serverInfoChanged = true;
                            }
                        }

                        if (serverInfoChanged)
                        {
                            ServerListAge++;
                        }

                        existIds.Clear();

                        //默认所有跨服服务器都允许分配幻影寺院活动
                        foreach (var item in ServerIdServerInfoDict.Values)
                        {
                            if ((item.Flags & ServerFlags.KuaFuServer) > 0)
                            {
                                UpdateServerGameConfig(item.ServerId, (int)GameTypes.HuanYingSiYuan, HuanYingSiYuanGameConfigData.MaxCount, ref serverGameConfigChanged);
                                existIds.Add(item.ServerId);
                            }

                            item.Age = ServerListAge;
                        }

                        //从列表移除已不存在的服务器
                        foreach (var id in ServerIdGameConfigDict.Keys)
                        {
                            if (!existIds.Contains(id))
                            {
                                RemoveServerGameConfig(id, ref serverGameConfigChanged);
                                RemoveServerGameConfigToDb(id, (int)GameTypes.HuanYingSiYuan);
                            }
                        }

                        if (serverGameConfigChanged)
                        {
                            ServerGameConfigAge++;
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                LogManager.WriteExceptionUseCache(ex.ToString());
                return false;
            }

            return true;
        }

        private void UpdateServerGameConfig(int serverId, int gameType, int capacity, ref bool serverGameConfigChanged)
        {
            KuaFuServerGameConfig data;
            if (!ServerIdGameConfigDict.TryGetValue(serverId, out data))
            {
                data = new KuaFuServerGameConfig()
                {
                    ServerId = serverId,
                    GameType = gameType,
                };

                ServerIdGameConfigDict[serverId] = data;
                KuaFuServerIdGameConfigDict[serverId] = data;
                serverGameConfigChanged = true;
            }
            else if (data.Capacity != capacity)
            {
                data.Capacity = capacity;
                UpdateServerGameConfigToDb(serverId, gameType, capacity);
                serverGameConfigChanged = true;
            }
        }

        private void RemoveServerGameConfig(int serverId, ref bool serverGameConfigChanged)
        {
            KuaFuServerGameConfig data;
            ServerIdGameConfigDict.TryRemove(serverId, out data);
            KuaFuServerIdGameConfigDict.TryRemove(serverId, out data);
            RemoveServerGameConfigToDb(serverId, (int)GameTypes.HuanYingSiYuan);
            serverGameConfigChanged = true;
        }

        /// <summary>
        /// 获取服务器列表信息
        /// </summary>
        /// <param name="age"></param>
        /// <returns></returns>
        public List<KuaFuServerInfo> GetKuaFuServerInfoData(int age)
        {
            List<KuaFuServerInfo> result = null;
            lock (Mutex)
            {
                if (age != ServerListAge)
                {
                    result = ServerIdServerInfoDict.Values.ToList();
                }
            }

            return result;
        }

        /// <summary>
        /// 更新服务器负载和状态
        /// </summary>
        /// <param name="serverId"></param>
        /// <param name="load"></param>
        /// <param name="state"></param>
        public void UpdateServerLoadState(int serverId, int load, int state)
        {
            bool update = false;
            lock (Mutex)
            {
                KuaFuServerInfo kuaFuServerInfo;
                if (ServerIdServerInfoDict.TryGetValue(serverId, out kuaFuServerInfo))
                {
                    if (kuaFuServerInfo.Load != load)
                    {
                        kuaFuServerInfo.Load = load;
                        update = true;
                    }

                    if (kuaFuServerInfo.State != state)
                    {
                        kuaFuServerInfo.State = state;
                        update = true;
                    }
                }

                if (update)
                {
                    //写数据库
                    try
                    {
                        DbHelperMySQL.ExecuteSql(string.Format("update ignore t_server_info set `load`={0},`state`={1} where `serverid`={2}", load, state, serverId));
                    }
                    catch (System.Exception ex)
                    {
                        LogManager.WriteExceptionUseCache(ex.ToString());
                    }
                }
            }
        }

        private void UpdateServerGameConfigToDb(int serverId, int gameType, int capacity)
        {
            ExecuteSqlNoQuery(string.Format("INSERT INTO t_server_game_config(serverid, gametype, capacity) VALUES({0},{1},{2}) ON DUPLICATE KEY UPDATE `capacity`={2}", serverId, gameType, capacity));
        }

        private void RemoveServerGameConfigToDb(int serverId, int gameType)
        {
            ExecuteSqlNoQuery(string.Format("delete from t_server_game_config where `serverid`={0} and `gametype`={1}", serverId, gameType));
        }

        private void UpdateServerInfo(int serverId, string ip, int port, int dbPort, int logdbPort, int flags)
        {
            ExecuteSqlNoQuery(string.Format("INSERT INTO t_server_info(serverid,ip,port,dbip,dbport,logdbip,logdbport,state,age,flags) " + 
                                            "VALUES({0},'{1}',{2},'{1}',{3},'{1}',{4},0,0,{5}) " +
                                            "ON DUPLICATE KEY UPDATE `ip`='{1}',port={2},dbip='{1}',dbport={3},logdbip='{1}',logdbport={4},flags={5}",
                                            serverId, ip, port, dbPort, logdbPort, flags));
        }

        private void RemoveServerInfo(int serverId)
        {
            ExecuteSqlNoQuery(string.Format("delete from t_server_info where `serverid`={0}", serverId));
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

        /// <summary>
        /// 创建一个幻影寺院副本
        /// </summary>
        /// <param name="huanYingSiYuanAgent"></param>
        /// <param name="groupIndex"></param>
        /// <param name="roleNum"></param>
        /// <returns></returns>
        public HuanYingSiYuanFuBenData CreateHysyGameFuBen(HuanYingSiYuanAgent huanYingSiYuanAgent, int groupIndex, int roleNum)
        {
            int fuBenSeqId = huanYingSiYuanAgent.PopFuBenSeqId();
            if (fuBenSeqId <= 0)
            {
                return null;
            }

            int gameId;
            int serverId = huanYingSiYuanAgent.ClientInfo.ServerId;

            try
            {
                gameId = (int)DbHelperMySQL.GetSingle(string.Format("select CreateHysyGameFuBen({0},{1},{2})", serverId, fuBenSeqId, roleNum));
                //id = (int)DbHelperMySQL.GetSingle(string.Format("select CreateHysyGameFuBen('{0}',{1},{2},{3})", DateTime.Now.ToString("yyMMdd"), serverId, fuBenSeqId, roleNum));
            }
            catch (System.Exception ex)
            {
                LogManager.WriteExceptionUseCache(ex.ToString());
                gameId = -1;
            }

            HuanYingSiYuanFuBenData huanYingSiYuanFuBenData = null;
            if (gameId > 0)
            {
                huanYingSiYuanFuBenData = new HuanYingSiYuanFuBenData()
                {
                    ServerId = serverId,
                    GroupIndex = groupIndex,
                    SequenceId = fuBenSeqId,
                    GameId = gameId,
                    EndTime = Global.NowTime.AddMinutes(Consts.HuanYingSiYuanGameFuBenMaxExistMinutes),
                };
            }

            return huanYingSiYuanFuBenData;
        }

        /// <summary>
        /// 获取一个唯一ClientId;
        /// </summary>
        /// <param name="serverId"></param>
        /// <returns></returns>
        public int GetUniqueClientId(int serverId)
        {
            int id;
            try
            {
                id = (int)DbHelperMySQL.GetSingle("SELECT `GetUniqueClientId`()");
            }
            catch (System.Exception ex)
            {
                LogManager.WriteExceptionUseCache(ex.ToString());
                id = serverId;
            }

            return id;
        }
    }
}
