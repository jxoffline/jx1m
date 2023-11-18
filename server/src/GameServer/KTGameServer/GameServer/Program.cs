using GameServer.Core.Executor;
using GameServer.KiemThe;
using GameServer.KiemThe.Logic;
using GameServer.KiemThe.Logic.Manager;
using GameServer.Logic;
using GameServer.Server;
using GameServer.VLTK;
using GameServer.VLTK.Core.Activity.X2ExpEvent;
using Server.Protocol;
using Server.TCP;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Threading;
using System.Xml.Linq;
using Tmsk.Contract;

namespace GameServer
{
    public class Program : IConnectInfoContainer
    {
        public static FileVersionInfo AssemblyFileVersion;

#if Windows

        #region LOCK BUTTION CLOSE windows

        public delegate bool ControlCtrlDelegate(int CtrlType);

        [DllImport("kernel32.dll")]
        private static extern bool SetConsoleCtrlHandler(ControlCtrlDelegate HandlerRoutine, bool Add);

        private static ControlCtrlDelegate newDelegate = new ControlCtrlDelegate(HandlerRoutine);

        public static bool HandlerRoutine(int CtrlType)
        {
            switch (CtrlType)
            {
                case 0:

                    break;

                case 2:

                    break;
            }

            return true;
        }

        [DllImport("user32.dll", EntryPoint = "FindWindow")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", EntryPoint = "GetSystemMenu")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, IntPtr bRevert);

        [DllImport("user32.dll", EntryPoint = "RemoveMenu")]
        private static extern IntPtr RemoveMenu(IntPtr hMenu, uint uPosition, uint uFlags);

        /// <summary>
        /// Khóa nút Close
        /// </summary>
        private static void HideCloseBtn()
        {
            Console.Title = "Server_" + KTGlobal.GetRandomNumber(0, 100000);
            IntPtr windowHandle = FindWindow(null, Console.Title);
            IntPtr closeMenu = GetSystemMenu(windowHandle, IntPtr.Zero);
            uint SC_CLOSE = 0xF060;
            RemoveMenu(closeMenu, SC_CLOSE, 0x0);
        }

        #endregion LOCK BUTTION CLOSE windows

#endif

        /// <summary>
        /// Global Console Install
        /// </summary>
        public static Program ServerConsole = new Program();

        /// <summary>
        /// Xử lý callback CMD
        /// </summary>
        /// <param name="cmd"></param>
        public delegate void CmdCallback(String cmd);

        /// <summary>
        /// Tòa bộ thư viện lệnh
        /// </summary>
        private static Dictionary<String, CmdCallback> CmdDict = new Dictionary<string, CmdCallback>();

        public static Boolean NeedExitServer = false;

        #region Xử lý khi server Dumps

        /// <summary>
        /// Chặn ngoại lệ khi xảy ra crash
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception exception = e.ExceptionObject as Exception;
            LogManager.WriteLog(LogTypes.Exception, exception.ToString());
        }

        /// <summary>
        /// Xử lý ngoại lệ khi chết Threading
        /// </summary>
        private static void ExceptionHook()
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
        }

        #endregion Xử lý khi server Dumps

        #region Khi đóng trương trình

        /// <summary>
        /// 删除某个指定的文件
        /// </summary>
        public static void DeleteFile(String strFileName)
        {
            String strFullFileName = System.IO.Directory.GetCurrentDirectory() + "\\" + strFileName;
            if (File.Exists(strFullFileName))
            {
                FileInfo fi = new FileInfo(strFullFileName);
                if (fi.Attributes.ToString().IndexOf("ReadOnly") != -1)
                {
                    fi.Attributes = FileAttributes.Normal;
                }

                File.Delete(strFullFileName);
            }
        }

        /// <summary>
        /// Ghi lại thông tin prosecc
        /// </summary>
        public static void WritePIDToFile(String strFile)
        {
            String strFileName = System.IO.Directory.GetCurrentDirectory() + "\\" + strFile;

            Process processes = Process.GetCurrentProcess();
            int nPID = processes.Id;
            File.WriteAllText(strFileName, "" + nPID);
        }

        /// <summary>
        /// Ghi lại logs khi server stop
        /// </summary>
        public static int GetServerPIDFromFile()
        {
            String strFileName = System.IO.Directory.GetCurrentDirectory() + "\\GameServerStop.txt";
            if (File.Exists(strFileName))
            {
                string str = File.ReadAllText(strFileName);
                return int.Parse(str);
            }

            return 0;
        }

        #endregion Khi đóng trương trình

        #region Main Console

        private static void Main(string[] args)
        {

          
            // Thực hiện xóa files trước khi start server
            DeleteFile("Start.txt");
            DeleteFile("Stop.txt");
            DeleteFile("GameServerStop.txt");

            if (!GCSettings.IsServerGC && Environment.ProcessorCount > 2)
            {
                SysConOut.WriteLine(string.Format("Server Running Model  :{0}, {1}", GCSettings.IsServerGC ? "GameCenter" : "GameServer", GCSettings.LatencyMode));

                Console.WriteLine("Settings GameCenter ! ");

                string configFile = Process.GetCurrentProcess().MainModule.FileName + ".config";
                XElement xml = XElement.Load(configFile);
                XElement xml1 = xml.Element("runtime");
                if (null == xml1)
                {
                    xml.SetElementValue("runtime", "");
                    xml1 = xml.Element("runtime");
                }
                xml1.SetElementValue("gcServer", "");
                xml1.Element("gcServer").SetAttributeValue("enabled", "true");
                xml.Save(configFile);

                Console.WriteLine("Settings Server OK! Please Restart!");
                Console.Read();
                return;
            }
            else
            {
                SysConOut.WriteLine(string.Format("Server Running Model :{0}, {1}", GCSettings.IsServerGC ? "GameCenter" : "GameServer", GCSettings.LatencyMode));
            }
#if Windows

            #region Prosecc Console windows

            HideCloseBtn();

            SetConsoleCtrlHandler(newDelegate, true);

            if (Console.WindowWidth < 88)
            {
                Console.BufferWidth = 88;
                Console.WindowWidth = 88;
            }

            #endregion Prosecc Console windows

#endif
            ///Xử lý ngoại lệ
            ExceptionHook();

            ///Khởi tạo biến thời gian Tick Toàn bộ các sự kiện theo hiệu năng của máy chủ |
            // Máy chủ khỏe sẽ tick nhanh | Máy yếu sẽ tick chậm
            TimeUtil.Init();

            InitCommonCmd();

            //Khởi động máy chủ
            OnStartServer();

            WritePIDToFile("Start.txt");

            Thread thread = new Thread(ConsoleInputThread);
            thread.IsBackground = true;
            thread.Start();
            while (!NeedExitServer || !ServerConsole.MustCloseNow || ServerConsole.MainDispatcherWorker.IsBusy)
            {
                Thread.Sleep(1000);
            }
            thread.Abort();
            Process.GetCurrentProcess().Kill();
        }

        public static void ConsoleInputThread(object obj)
        {
            String cmd = null;
            while (!NeedExitServer)
            {
                cmd = System.Console.ReadLine();
                if (!string.IsNullOrEmpty(cmd))
                {
                    //ctrl + c
                    if (null != cmd && 0 == cmd.CompareTo("exit"))
                    {
                        SysConOut.WriteLine("Press Y to Exit Server?");
                        cmd = System.Console.ReadLine();
                        if (0 == cmd.CompareTo("y"))
                        {
                            break;
                        }
                    }

                    ParseInputCmd(cmd);
                }
            }

            // Exitding Server
            OnExitServer();
        }

        /// <summary>
        /// Xử lý lệnh khi gõ CMD
        /// </summary>
        /// <param name="cmd"></param>
        private static void ParseInputCmd(String cmd)
        {
            CmdCallback cb = null;
            int index = cmd.IndexOf('/');
            string cmd0 = cmd;
            if (index > 0)
            {
                cmd0 = cmd.Substring(0, index - 1).TrimEnd();
            }
            if (CmdDict.TryGetValue(cmd0, out cb) && null != cb)
            {
                cb(cmd);
            }
            else
            {
                SysConOut.WriteLine("Unknow command, input 'help' to get the data.");
            }
        }

        /// <summary>
        /// Start Servers
        /// </summary>
        private static void OnStartServer()
        {
            ServerConsole.InitServer();

            Console.Title = string.Format("KIEMTHE SERVER {0} Version : {1} : {2}", GameManager.ServerLineID, GetVersionDateTime(), ProgramExtName);
        }

        /// <summary>
        /// 进程退出
        /// </summary>
        public static void OnExitServer()
        {
            ExpMutipleEvent.UpdateServerStatus();
            ServerConsole.ExitServer();
        }

        public static void Exit()
        {
            NeedExitServer = true;
            //主线程处于接收输入状态，如何唤醒呢？
        }

        #endregion Main Console

        #region Console command

        private static void InitCommonCmd()
        {
            CmdDict.Add("resetbullettimer", ResetBulletTimers);
            CmdDict.Add("resetbufftimer", ResetBuffTimers);
            CmdDict.Add("resetmonstertimer", ResetMonsterTimers);
            CmdDict.Add("clearscheduletask", ClearScheduleTasks);
            CmdDict.Add("resetpettimer", ResetPetTimers);
            CmdDict.Add("resettradercarriagetimer", ResetTraderCarriageTimers);

            CmdDict.Add("clear", (x) => { Console.Clear(); });
        }

        private static void ResetBulletTimers(string cmdID)
        {
            KTBulletManager.ForceResetBulletTimer();
            SysConOut.WriteLine("Reset bullet timer ok!!!");
        }

        private static void ClearScheduleTasks(string cmdID)
        {
            KTScheduleTaskManager.Instance.ClearScheduleTasks();
            SysConOut.WriteLine("Clear ScheduleTask timer ok!!!");
        }

        private static void ResetBuffTimers(string cmdID)
        {
            KTBuffManager.Instance.ClearAllBuffTimers();
            SysConOut.WriteLine("Reset buff timer ok!!!");
        }

        private static void ResetMonsterTimers(string cmdID)
        {
            KTMonsterTimerManager.Instance.ClearMonsterTimers();
            SysConOut.WriteLine("Reset monster timer ok!!!");
        }

        private static void ResetPetTimers(string cmdID)
        {
            KTPetTimerManager.Instance.ClearTimers();
            SysConOut.WriteLine("Reset pet timer ok!!!");
        }

        private static void ResetTraderCarriageTimers(string cmdID)
        {
            KTTraderCarriageTimerManager.Instance.ClearTimers();
            SysConOut.WriteLine("Reset trader carriage timer ok!!!");
        }

        public static void LoadIPList(string strCmd)
        {
            try
            {
                if (String.IsNullOrEmpty(strCmd))
                {
                    strCmd = GameManager.GameConfigMgr.GetGameConfigItemStr("whiteiplist", "");
                }

                bool enabeld = true;
                string[] ipList = strCmd.Split(',');
                List<string> resultList = Global._TCPManager.MySocketListener.InitIPWhiteList(ipList, enabeld);

                if (resultList.Count > 0)
                {
                    Console.WriteLine("Ip Band List :");
                    foreach (var ip in resultList)
                    {
                        Console.WriteLine(ip);
                    }
                }
                else
                {
                    Console.WriteLine("No Band Ip List Found!");
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("Proplem when loading Band Ip List:\n" + ex.ToString());
            }
        }

        #endregion Console command

        #region 外部调用接口

        /// <summary>
        /// 与dbserver的链接信息词典
        /// </summary>
        public Dictionary<int, String> DBServerConnectDict = new Dictionary<int, string>();

        /// <summary>
        /// 增加数据服务器链接信息
        /// </summary>
        /// <param name="index"></param>
        /// <param name="info"></param>
        public void AddDBConnectInfo(int index, String info)
        {
            lock (DBServerConnectDict)
            {
                if (DBServerConnectDict.ContainsKey(index))
                {
                    DBServerConnectDict[index] = info;
                }
                else
                {
                    DBServerConnectDict.Add(index, info);
                }
            }
        }

        /// <summary>
        /// 与dbserver的链接信息词典
        /// </summary>
        public Dictionary<int, String> LogDBServerConnectDict = new Dictionary<int, string>();

        /// <summary>
        /// 增加数据服务器链接信息
        /// </summary>
        /// <param name="index"></param>
        /// <param name="info"></param>
        public void AddLogDBConnectInfo(int index, String info)
        {
            lock (LogDBServerConnectDict)
            {
                if (LogDBServerConnectDict.ContainsKey(index))
                {
                    LogDBServerConnectDict[index] = info;
                }
                else
                {
                    LogDBServerConnectDict.Add(index, info);
                }
            }
        }

        #endregion 外部调用接口

        #region 游戏服务器具体功能部分

        /// <summary>
        /// 程序额外的名称
        /// </summary>
        private static string ProgramExtName = "";

        /// <summary>
        /// 初始化应用程序名称
        /// </summary>
        /// <returns></returns>
        private static void InitProgramExtName()
        {
            ProgramExtName = DataHelper.CurrentDirectory;
        }

        /// <summary>
        /// Call khởi chạy máy chủ
        ///  Window_Loaded(object sender, RoutedEventArgs e)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void InitServer()
        {
            InitProgramExtName();

            ThreadPool.SetMaxThreads(1000, 1000);

            if (!File.Exists(@"Policy.xml"))
            {
                throw new Exception(string.Format("Can't find files: {0}", @"Policy.xml"));
            }

            TCPPolicy.LoadPolicyServerFile(@"Policy.xml");

            SysConOut.WriteLine("Load Game Server Settings!");

            //Read Config XMl
            XElement xml = XElement.Load(@"AppConfig.xml");

            try
            {
                SysConOut.WriteLine("Connect to Database Server...");
                this.InitTCPManager(xml, true);

                SysConOut.WriteLine("Loading all game objects data Npcs, Monsters, Items...");
                this.InitGameManager(xml);

                SysConOut.WriteLine("Initializing KTData...");
              
                KTMain.InitFirst();

                /// Khởi tạo bản đồ và quái
                SysConOut.WriteLine("Loading Maps and Monsters...");
                KTMain.LoadMaps();
                //this.InitGameMapsAndMonsters();

              
                KTMain.InitAfterLoadingMap();

                GlobalServiceManager.Initialize();

                GlobalServiceManager.Startup();

                // Start Http Serivce
                HttpService Service = new HttpService();
                Service.Start();
            }
            catch (System.Exception ex)
            {
                SysConOut.WriteLine("Exception was detected when Starting the Server... Kill Process after 5s!");
                Thread.Sleep(5000);
                LogManager.WriteException(ex.ToString());
                Process.GetCurrentProcess().Kill();
            }

            SysConOut.WriteLine("Starting all BackgroundWorkers...");

            SysConOut.WriteLine("BackgroundWorker LogsCommand Start");
            logDBCommandWorker = new BackgroundWorker();
            logDBCommandWorker.DoWork += logDBCommandWorker_DoWork;

            SysConOut.WriteLine("BackgroundWorker othersWorker Start");
            othersWorker = new BackgroundWorker();
            othersWorker.DoWork += othersWorker_DoWork;

            SysConOut.WriteLine("BackgroundWorker dbWriterWorker Start");
            dbWriterWorker = new BackgroundWorker();
            dbWriterWorker.DoWork += dbWriterWorker_DoWork;

            SysConOut.WriteLine("Restore SocketBufferData for Client");
            SocketSendCacheDataWorker = new BackgroundWorker();
            SocketSendCacheDataWorker.DoWork += SocketSendCacheDataWorker_DoWork;

            SysConOut.WriteLine("BackgroundWorker MainDispatcherWorker Start");
            MainDispatcherWorker = new BackgroundWorker();
            MainDispatcherWorker.DoWork += MainDispatcherWorker_DoWork;

            SysConOut.WriteLine("BackgroundWorker socketCheckWorker Start");
            socketCheckWorker = new BackgroundWorker();
            socketCheckWorker.DoWork += SocketCheckWorker_DoWork;

            SysConOut.WriteLine("BackgroundWorker chat Start");
            chatMsgWorker = new BackgroundWorker();
            chatMsgWorker.DoWork += chatMsgWorker_DoWork;

            Global._TCPManager.MySocketListener.DontAccept = false;

            if (!MainDispatcherWorker.IsBusy)
            {
                MainDispatcherWorker.RunWorkerAsync();
            }

            StartThreadPoolDriverTimer();

            GameManager.GameConfigMgr.SetGameConfigItem("gameserver_version", GetVersionDateTime());

            Global.UpdateDBGameConfigg("gameserver_version", GetVersionDateTime());

            SysConOut.WriteLine("Bring Tcp Protocal...");

            Thread.Sleep(3000);
            InitTCPManager(xml, false);


            Utils.SendTelegram("[" + GameManager.ServerLineID + "] Server is started!");

            GameManager.ServerStarting = false;
        }

        public static string GetLocalIPAddress()
        {
            if (GameManager.ServerLineID == 9)
            {
                return "66.42.60.246";
            }
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("Local IP Address Not Found!");
        }

        //关闭服务器
        public void ExitServer()
        {
            if (NeedExitServer)
            {
                return;
            }

            GlobalServiceManager.Showdown();

            GlobalServiceManager.Destroy();

            Global._TCPManager.Stop(); //停止TCP的侦听，否则mono无法正常退出

            Window_Closing();

            SysConOut.WriteLine("Server is shutting down...");

            if (0 == GetServerPIDFromFile())
            {
                String cmd = System.Console.ReadLine();

                while (true)
                {
                    if (MainDispatcherWorker.IsBusy)
                    {
                        SysConOut.WriteLine("Trying to shut down Server...");
                        cmd = System.Console.ReadLine();
                        continue;
                    }
                    else
                    {
                        break;
                    }
                }

                StopThreadPoolDriverTimer();
            }
        }

        #region 初始化部分
        /// <summary>
        /// 缓存信息初始化
        /// </summary>
        private void InitCache(XElement xml)
        {
            //Quản lý lỗi bộ nhớ đệm
            Global._FullBufferManager = new FullBufferManager();
            //Dánh ách bộ nhớ đệm
            Global._SendBufferManager = new SendBufferManager();
            //Tiếu thiểu 50 minis
            SendBuffer.SendDataIntervalTicks = Math.Max(20, Math.Min(500, (int)Global.GetSafeAttributeLong(xml, "SendDataParam", "SendDataIntervalTicks")));
            //这个值必须小于10240,大于等于1500
            SendBuffer.MaxSingleSocketSendBufferSize = Math.Max(18000, Math.Min(256000, (int)Global.GetSafeAttributeLong(xml, "SendDataParam", "MaxSingleSocketSendBufferSize")));
            //发送超时判读 单位毫秒
            SendBuffer.SendDataTimeOutTicks = Math.Max(3000, Math.Min(20000, (int)Global.GetSafeAttributeLong(xml, "SendDataParam", "SendDataTimeOutTicks")));
            //
            SendBuffer.MaxBufferSizeForLargePackge = SendBuffer.MaxSingleSocketSendBufferSize * 2 / 3;

            //内存管理器
            Global._MemoryManager = new MemoryManager();

            string cacheMemoryBlocks = Global.GetSafeAttributeStr(xml, "CacheMemoryParam", "CacheMemoryBlocks");
            if (string.IsNullOrWhiteSpace(cacheMemoryBlocks))
            {
                Global._MemoryManager.AddBatchBlock(100, 1500);
                Global._MemoryManager.AddBatchBlock(600, 400);
                Global._MemoryManager.AddBatchBlock(600, 50);
                Global._MemoryManager.AddBatchBlock(600, 100);
            }
            else
            {
                string[] items = cacheMemoryBlocks.Split('|');
                foreach (var item in items)
                {
                    string[] pair = item.Split(',');
                    int blockSize = int.Parse(pair[0]);
                    int blockNum = int.Parse(pair[1]);
                    blockNum = Math.Max(blockNum, 80); //缓存数不少于80
                    if (blockSize > 0 && blockNum > 0)
                    {
                        Global._MemoryManager.AddBatchBlock(blockNum, blockSize);
                        GameManager.MemoryPoolConfigDict[blockSize] = blockNum;
                    }
                }
            }
        }

        /// <summary>
        /// Khởi tạo đối tượng quản lý giao tiếp
        /// </summary>
        private void InitTCPManager(XElement xml, bool bConnectDB)
        {
            if (bConnectDB)
            {
                GameManager.HttpServiceCode = (int)Global.GetSafeAttributeLong(xml, "APIWebServer", "Port");

                // Khởi tạo server LINE
                GameManager.ServerLineID = (int)Global.GetSafeAttributeLong(xml, "Server", "LineID");

                if (GameManager.ServerLineID > 9000)
                {
                    GameManager.IsKuaFuServer = true;
                }
                else
                {
                    GameManager.IsKuaFuServer = false;
                }

                // Logs ra xem có phải đây là KUNGFU server không
                Console.WriteLine("Server Line : " + GameManager.ServerLineID + "| IsKuaFuServer :" + GameManager.IsKuaFuServer.ToString());

                GameManager.ActiveGiftCodeUrl = Global.GetSafeAttributeStr(xml, "GiftCode", "Url");

                Console.WriteLine("ActiveGiftCode Service : " + GameManager.ActiveGiftCodeUrl);

                // Logs Level Lưu
                LogManager.LogTypeToWrite = (LogTypes)(int)Global.GetSafeAttributeLong(xml, "Server", "LogType");

                Console.WriteLine("Logs level Set : " + LogManager.LogTypeToWrite);

                
                GameManager.SystemServerEvents.EventLevel = (EventLevels)(int)Global.GetSafeAttributeLong(xml, "Server", "EventLevel");
                GameManager.SystemRoleLoginEvents.EventLevel = GameManager.SystemServerEvents.EventLevel;
                GameManager.SystemRoleLogoutEvents.EventLevel = GameManager.SystemServerEvents.EventLevel;
                GameManager.SystemRoleTaskEvents.EventLevel = GameManager.SystemServerEvents.EventLevel;
                GameManager.SystemRoleDeathEvents.EventLevel = GameManager.SystemServerEvents.EventLevel;
                GameManager.SystemRoleBuyWithTongQianEvents.EventLevel = GameManager.SystemServerEvents.EventLevel;
                GameManager.SystemRoleBuyWithBoundTokenEvents.EventLevel = GameManager.SystemServerEvents.EventLevel;
                GameManager.SystemRoleBuyWithYinPiaoEvents.EventLevel = GameManager.SystemServerEvents.EventLevel;
                GameManager.SystemRoleBuyWithYuanBaoEvents.EventLevel = GameManager.SystemServerEvents.EventLevel;
                GameManager.SystemRoleSaleEvents.EventLevel = GameManager.SystemServerEvents.EventLevel;
                GameManager.SystemRoleExchangeEvents1.EventLevel = GameManager.SystemServerEvents.EventLevel;
                GameManager.SystemRoleExchangeEvents2.EventLevel = GameManager.SystemServerEvents.EventLevel;
                GameManager.SystemRoleExchangeEvents3.EventLevel = GameManager.SystemServerEvents.EventLevel;
                GameManager.SystemRoleUpgradeEvents.EventLevel = GameManager.SystemServerEvents.EventLevel;
                GameManager.SystemRoleGoodsEvents.EventLevel = GameManager.SystemServerEvents.EventLevel;
                GameManager.SystemRoleFallGoodsEvents.EventLevel = GameManager.SystemServerEvents.EventLevel;
                GameManager.SystemRoleBoundTokenEvents.EventLevel = GameManager.SystemServerEvents.EventLevel;
                GameManager.SystemRoleHorseEvents.EventLevel = GameManager.SystemServerEvents.EventLevel;
                GameManager.SystemRoleBangGongEvents.EventLevel = GameManager.SystemServerEvents.EventLevel;
                GameManager.SystemRoleJingMaiEvents.EventLevel = GameManager.SystemServerEvents.EventLevel;
                GameManager.SystemRoleRefreshQiZhenGeEvents.EventLevel = GameManager.SystemServerEvents.EventLevel;
                GameManager.SystemRoleWaBaoEvents.EventLevel = GameManager.SystemServerEvents.EventLevel;
                GameManager.SystemRoleMapEvents.EventLevel = GameManager.SystemServerEvents.EventLevel;
                GameManager.SystemRoleFuBenAwardEvents.EventLevel = GameManager.SystemServerEvents.EventLevel;
                GameManager.SystemRoleWuXingAwardEvents.EventLevel = GameManager.SystemServerEvents.EventLevel;
                GameManager.SystemRolePaoHuanOkEvents.EventLevel = GameManager.SystemServerEvents.EventLevel;
                GameManager.SystemRoleYaBiaoEvents.EventLevel = GameManager.SystemServerEvents.EventLevel;
                GameManager.SystemRoleLianZhanEvents.EventLevel = GameManager.SystemServerEvents.EventLevel;
                GameManager.SystemRoleHuoDongMonsterEvents.EventLevel = GameManager.SystemServerEvents.EventLevel;
                GameManager.SystemRoleDigTreasureWithYaoShiEvents.EventLevel = GameManager.SystemServerEvents.EventLevel;
                GameManager.SystemRoleAutoSubYuanBaoEvents.EventLevel = GameManager.SystemServerEvents.EventLevel;
                GameManager.SystemRoleAutoSubBoundMoneyEvents.EventLevel = GameManager.SystemServerEvents.EventLevel;
                GameManager.SystemRoleAutoSubEvents.EventLevel = GameManager.SystemServerEvents.EventLevel;
                GameManager.SystemRoleBuyWithTianDiJingYuanEvents.EventLevel = GameManager.SystemServerEvents.EventLevel;
                GameManager.SystemRoleFetchVipAwardEvents.EventLevel = GameManager.SystemServerEvents.EventLevel;

                GameManager.SystemRoleFetchMailMoneyEvents.EventLevel = GameManager.SystemServerEvents.EventLevel;

                GameManager.SystemRoleBuyWithBoundMoneyEvents.EventLevel = GameManager.SystemServerEvents.EventLevel;
                GameManager.SystemRoleBoundMoneyEvents.EventLevel = GameManager.SystemServerEvents.EventLevel;

                int dbLog = Math.Max(0, (int)Global.GetSafeAttributeLong(xml, "DBLog", "DBLogEnable"));

                //Khởi tạo Xml sẽ cache
                InitCache(xml);

                //try
                //{
                //    Global.Flag_NameServer = true;
                //    NameServerNamager.Init(xml);
                //}
                //catch (System.Exception ex)
                //{
                //    Global.Flag_NameServer = false;
                //    Console.WriteLine(ex.ToString());
                //    //throw ex;
                //}

                int nCapacity = (int)Global.GetSafeAttributeLong(xml, "Socket", "capacity") * 3;

                TCPManager.getInstance().initialize(nCapacity);

                Global._TCPManager = TCPManager.getInstance();

                Global._TCPManager.tcpClientPool.RootWindow = this;
                Global._TCPManager.tcpClientPool.Init(
                    (int)Global.GetSafeAttributeLong(xml, "DBServer", "pool"),
                    Global.GetSafeAttributeStr(xml, "DBServer", "ip"),
                    (int)Global.GetSafeAttributeLong(xml, "DBServer", "port"),
                    "DBServer");

                Global._TCPManager.tcpLogClientPool.RootWindow = this;
                Global._TCPManager.tcpLogClientPool.Init(
                    (int)Global.GetSafeAttributeLong(xml, "LogDBServer", "pool"),
                    Global.GetSafeAttributeStr(xml, "LogDBServer", "ip"),
                    (int)Global.GetSafeAttributeLong(xml, "LogDBServer", "port"),
                    "LogDBServer");
            }
            else
            {
                KT_TCPHandler.KeySHA1 = Global.GetSafeAttributeStr(xml, "Token", "sha1");
                KT_TCPHandler.KeyData = Global.GetSafeAttributeStr(xml, "Token", "data");
                KT_TCPHandler.WebKey = Global.GetSafeAttributeStr(xml, "Token", "webkey");
                KT_TCPHandler.WebKeyLocal = KT_TCPHandler.WebKey;
                string loginWebKey = GameManager.GameConfigMgr.GetGameConfigItemStr("loginwebkey", KT_TCPHandler.WebKey);
                if (!string.IsNullOrEmpty(loginWebKey) && loginWebKey.Length >= 5)
                {
                    KT_TCPHandler.WebKey = loginWebKey;
                }

                Global._TCPManager.tcpRandKey.Init(
                (int)Global.GetSafeAttributeLong(xml, "Token", "count"),
                (int)Global.GetSafeAttributeLong(xml, "Token", "randseed"));

                //启动通讯管理对象
                Global._TCPManager.RootWindow = this;
                Global._TCPManager.Start(Global.GetSafeAttributeStr(xml, "Socket", "ip"),
                    (int)Global.GetSafeAttributeLong(xml, "Socket", "port"));
            }
        }

        /// <summary>
        /// Khởi tạo game
        /// </summary>
        private void InitGameManager(XElement xml)
        {
            //从数据库中获取配置参数
            GameManager.GameConfigMgr.LoadGameConfigFromDBServer();

            // 装入白名单
            LoadIPList("");

            //初始化和数据库存储的配置相关的数据
            InitGameConfigWithDB();
        }

        /// <summary>
        /// 初始化游戏配置(数据库相关)
        /// </summary>
        private void InitGameConfigWithDB()
        {
            //初始化服务器ID(服务器区号)
            GameManager.ServerId = Global.SendToDB<int, string>((int)TCPGameServerCmds.CMD_DB_GET_SERVERID, "", GameManager.LocalServerId);

            GameManager.Flag_OptimizationBagReset = GameManager.GameConfigMgr.GetGameConfigItemInt("optimization_bag_reset", 1) > 0;
            GameManager.SetLogFlags(GameManager.GameConfigMgr.GetGameConfigItemInt("logflags", 0x7fffffff));

            //以下是平台相关的
            string platformType = GameManager.GameConfigMgr.GetGameConfigItemStr("platformtype", "app");
            for (PlatformTypes i = PlatformTypes.Tmsk; i < PlatformTypes.Max; i++)
            {
                if (0 == string.Compare(platformType, i.ToString(), true))
                {
                    GameManager.PlatformType = i;
                    return;
                }
            }

            //处理拼写不规范的配置
            if (platformType == "andrid")
            {
                GameManager.PlatformType = PlatformTypes.Android;
            }
            else
            {
                GameManager.PlatformType = PlatformTypes.APP;
            }

            GameManager.LoadGameConfigFlags();
        }

        /// <summary>
        /// 初始化怪物管理对象
        /// </summary>
        private void InitMonsterManager()
        {
            //GameManager.MonsterMgr.CycleExecute += ExecuteBackgroundWorkers;
        }

        /// <summary>
        private static Timer ThreadPoolDriverTimer = null;

        /// <summary>
        /// 日志线程池驱动定时器
        /// </summary>
        private static Timer LogThreadPoolDriverTimer = null;

        /// <summary>
        /// 初始化线程池驱动定时器
        /// </summary>
        protected static void StartThreadPoolDriverTimer()
        {
            ThreadPoolDriverTimer = new Timer(ThreadPoolDriverTimer_Tick, null, 1000, 1000);
            LogThreadPoolDriverTimer = new Timer(LogThreadPoolDriverTimer_Tick, null, 500, 500);
        }

        /// <summary>
        /// 停止定时器
        /// </summary>
        protected static void StopThreadPoolDriverTimer()
        {
            ThreadPoolDriverTimer.Change(Timeout.Infinite, Timeout.Infinite);
            LogThreadPoolDriverTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        protected static void ThreadPoolDriverTimer_Tick(Object sender)
        {
            try
            {
                //驱动后台线程池
                ServerConsole.ExecuteBackgroundWorkers(null, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                //System.Windows.Application.Current.Dispatcher.Invoke((MethodInvoker)delegate
                {
                    // 格式化异常错误信息
                    DataHelper.WriteFormatExceptionLog(ex, "ThreadPoolDriverTimer_Tick", false);
                    //throw ex;
                }//);
            }
        }

        public static void LogThreadPoolDriverTimer_Tick(Object sender)
        {
            try
            {
                ServerConsole.ExecuteBackgroundLogWorkers(null, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, "LogThreadPoolDriverTimer_Tick", false);
            }
        }

        #endregion 初始化部分

        #region 线程部分

        /// 数据库命令执行线程
        /// </summary>
       // private BackgroundWorker dbCommandWorker;

        /// <summary>
        /// 日志数据库命令执行线程
        /// </summary>
        private BackgroundWorker logDBCommandWorker;

        /// <summary>
        /// 后台处理线程
        /// </summary>
        private BackgroundWorker othersWorker;

        /// </summary>
        private BackgroundWorker dbWriterWorker;

        private BackgroundWorker chatMsgWorker;

        /// <summary>
        /// 套接字缓冲数据发送线程
        /// </summary>
        private BackgroundWorker SocketSendCacheDataWorker;

        /// <summary>
        /// 主调度线程,这个线程一直处于循环状态，不断的处理各种逻辑判断,相当于原来的主界面线程
        /// </summary>
        private BackgroundWorker MainDispatcherWorker;

        /// <summary>
        /// socket检查线程
        /// </summary>
        private BackgroundWorker socketCheckWorker;

        // Danh sách các BG update tầm nhìn cho client
        private BackgroundWorker[] Gird9UpdateWorkers;

        /// <summary>
        /// 是否是要立刻关闭
        /// </summary>
        private bool MustCloseNow = false;

        /// <summary>
        /// 是否进入了关闭模式
        /// </summary>
        private bool EnterClosingMode = false;

        /// <summary>
        /// 60秒钟的倒计时器
        /// </summary>
        private int ClosingCounter = 30 * 200;

        /// <summary>
        /// 最近一次写数据库日志的时间
        /// </summary>
        private long LastWriteDBLogTicks = TimeUtil.NOW();

        /// <summary>
        /// 执行日志后台线程对象
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExecuteBackgroundLogWorkers(object sender, EventArgs e)
        {
            try
            {
                if (!logDBCommandWorker.IsBusy) { logDBCommandWorker.RunWorkerAsync(); }
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, "logDBCommandWorker", false);
            }
        }

        /// <summary>
        /// 执行后台线程对象
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExecuteBackgroundWorkers(object sender, EventArgs e)
        {
            
            try
            {
                if (!othersWorker.IsBusy) { othersWorker.RunWorkerAsync(); }
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, "othersWorker BUG :" + ex.ToString());
            }

            try
            {
                if (!dbWriterWorker.IsBusy) { dbWriterWorker.RunWorkerAsync(); }
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, "dbWriterWorker BUG :" + ex.ToString());
            }
            try
            {
                if (!SocketSendCacheDataWorker.IsBusy) { SocketSendCacheDataWorker.RunWorkerAsync(); }
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, "SocketSendCacheDataWorker BUG :" + ex.ToString());
            }

            try
            {
                if (!socketCheckWorker.IsBusy) { socketCheckWorker.RunWorkerAsync(); }
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, "socketCheckWorker BUG :" + ex.ToString());
            }

            try
            {
                if (!chatMsgWorker.IsBusy) { chatMsgWorker.RunWorkerAsync(); }
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, "chatMsgWorker", false);
            }

            ThreadPool.GetAvailableThreads(out int workerThreads, out int completionPortThreads);

            LogManager.WriteLog(LogTypes.Alert, "WorkerThreads: " + workerThreads + ", CompletionPortThreads: " + completionPortThreads);

            long ticks = TimeUtil.NOW();

            if (completionPortThreads < 950)
            {
              

                if (ticks - LastTelegramSpam > 5000)
                {
                    LastTelegramSpam = ticks;

                    Utils.SendTelegram("[S" + GameManager.ServerLineID + "] CompletionPortThreads: " + completionPortThreads);
                }

            }


            if (ticks - LastCheckCCu > 300000)
            {
                LastCheckCCu = ticks;

                Utils.SendTelegram("[S" + GameManager.ServerLineID + "] CCU ONLINE : " + KTPlayerManager.GetPlayersCount());
            }
        }

        private long LastTelegramSpam = TimeUtil.NOW();

        private long LastCheckCCu = TimeUtil.NOW();

        /// <summary>
        /// 原来的 closingTimer_Tick(object sender, EventArgs e)
        /// 显示关闭信息的计时器
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void closingTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                string title = "";

                //关闭角色
                KPlayer client = KTPlayerManager.Find(x => true);
                if (null != client)
                {
                    /**/
                    title = string.Format("GameServer {0} is closing, total {1} clients left.", GameManager.ServerLineID, KTPlayerManager.GetPlayersCount());
                    Global.ForceCloseClient(client, "GameServer force close", true);
                }
                else
                {
                    //关闭倒计时
                    ClosingCounter -= 200;

                    //判断DB的命令队列是否已经执行完毕?
                    if (ClosingCounter <= 0)
                    {
                        //不再发送数据
                        Global._SendBufferManager.Exit = true;

                        //是否立刻关闭
                        MustCloseNow = true;

                        //程序主窗口
                        //GameManager.AppMainWnd.Close();
                        //Window_Closing();//没必要调用
                    }
                }

                //设置标题
                Console.Title = title;
            }
            catch (Exception ex)
            {
                //System.Windows.Application.Current.Dispatcher.Invoke((MethodInvoker)delegate
                {
                    // 格式化异常错误信息
                    DataHelper.WriteFormatExceptionLog(ex, "closingTimer_Tick", false);
                    //throw ex;
                }//);
            }
        }

        private long LastAuxiliaryTicks = TimeUtil.NOW();

        /// <summary>
        /// 怪物Ai攻击索引
        /// </summary>
        //private int IndexOfMonsterAiAttack = 0;

        /// <summary>
        /// 计时器函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void auxiliaryTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                long ticks = TimeUtil.NOW();

                if (ticks - LastAuxiliaryTicks > 1000)
                {
                    DoLog(String.Format("\r\nAuxiliaryTimer took much times to process: {0}ms", ticks - LastAuxiliaryTicks));
                }

                LastAuxiliaryTicks = ticks;

                ticks = TimeUtil.NOW();
                Global._TCPManager.tcpClientPool.Supply();
                if (TimeUtil.NOW() - ticks >= 500)
                {
                    LogManager.WriteLog(LogTypes.Error, "AuxiliaryTimer - Tick 1 took too much times: " + (TimeUtil.NOW() - ticks));
                }

                ticks = TimeUtil.NOW();
                Global._TCPManager.tcpLogClientPool.Supply();
                if (TimeUtil.NOW() - ticks >= 500)
                {
                    LogManager.WriteLog(LogTypes.Error, "AuxiliaryTimer - Tick 2 took too much times: " + (TimeUtil.NOW() - ticks));
                }
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }

        /// <summary>
        /// 记录日志，控制台打印或者记录到文件
        /// </summary>
        /// <param name="warning"></param>
        private void DoLog(String warning)
        {
            LogManager.WriteLog(LogTypes.Error, warning);
        }

        /// <summary>
        /// 后台主调度线程
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        ///
        public long LastReport = 0;

        private void MainDispatcherWorker_DoWork(object sender, EventArgs e)
        {
            long lastTicks = TimeUtil.NOW();
            long startTicks = TimeUtil.NOW();
            long endTicks = TimeUtil.NOW();

            //睡眠时间
            int maxSleepMs = 100;
            int sleepMs = 100;

            int nTimes = 0;

            while (true)
            {
                try
                {
                    startTicks = TimeUtil.NOW();

                    //if (nTimes % 5 == 0)
                    if (startTicks - lastTicks >= 500)
                    {
                        GameManager.GM_NoCheckTokenTimeRemainMS -= startTicks - lastTicks;
                        lastTicks = startTicks;

                        //辅助调度--->500毫秒执行一次
                        auxiliaryTimer_Tick(null, null);
                    }

                    if (startTicks - LastReport >= 5000)
                    {
                        LastReport = startTicks;
                        LogManager.WriteLog(LogTypes.TimerReport, "MainDispatcherWorker_DoWork ===> LIVE");
                    }

                    if (NeedExitServer)
                    {
                        //调度关闭操作--->原来200毫秒执行一次
                        closingTimer_Tick(null, null);

                        //关闭完毕，自己也该退出了
                        if (MustCloseNow)
                        {
                            break;
                        }
                    }

                    endTicks = TimeUtil.NOW();

                    //最多睡眠100毫秒，最少睡眠1毫秒
                    sleepMs = (int)Math.Max(5, maxSleepMs - (endTicks - startTicks));

                    Thread.Sleep(sleepMs);

                    nTimes++;

                    if (nTimes >= 100000)
                    {
                        nTimes = 0;
                    }
                    if (0 != GetServerPIDFromFile())
                    {
                        OnExitServer();
                    }
                }
                catch (Exception ex)
                {
                    DataHelper.WriteFormatExceptionLog(ex, "MainDispatcherWorker_DoWork", false);
                }
            }

            if (0 != GetServerPIDFromFile())
            {
                // 结束时将进程ID写入文件
                WritePIDToFile("Stop.txt");

                StopThreadPoolDriverTimer();
            }
        }

        /// <summary>
        /// Xử lý đóng các client bị delay packet
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //private void dbCommandWorker_DoWork(object sender, DoWorkEventArgs e)
        //{
        //    try
        //    {
        //        GameManager.DBCmdMgr.ExecuteDBCmd(Global._TCPManager.tcpClientPool, Global._TCPManager.TcpOutPacketPool);
        //    }
        //    catch (Exception ex)
        //    {
        //        DataHelper.WriteFormatExceptionLog(ex, "dbCommandWorker_DoWork", false);
        //    }
        //}

        /// <summary>
        /// Xử lý lệnh với LOGS server
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void logDBCommandWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                GameManager.logDBCmdMgr.ExecuteDBCmd(Global._TCPManager.tcpLogClientPool, Global._TCPManager.TcpOutPacketPool);
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, "logDBCommandWorker_DoWork", false);
            }
        }


        /// <summary>
        /// Other Work quản lý toàn bộ vật phẩm | Item rơi trên map,bản tin ,Fake Role,Tiêu vv....| quản lý toàn bộ các sự kiện theo thởi gian
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void othersWorker_DoWork(object sender, EventArgs e)
        {
            // Tự điều chỉnh hiệu năng server
            TimeUtil.RecordTimeAnchor();
        }

        private void chatMsgWorker_DoWork(object sender, EventArgs e)
        {
            try
            {
                long ticksA = TimeUtil.NOW();

                KT_TCPHandler.HandleTransferChatMsg();

                long ticksB = TimeUtil.NOW();

                if (ticksB > ticksA + 1000)
                {
                    DoLog(String.Format("chatMsgWorker_DoWork time processed: {0}s", ticksB - ticksA));
                }
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, "chatMsgWorker_DoWork", false);
            }
        }

        /// <summary>
        /// Thực hiện ghi vào DB
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dbWriterWorker_DoWork(object sender, EventArgs e)
        {
            try
            {
                long ticks = TimeUtil.NOW();
                if (ticks - LastWriteDBLogTicks < (30 * 1000))
                {
                    return;
                }

                LastWriteDBLogTicks = ticks;

                Global._TCPManager.MySocketListener.ClearTimeoutSocket();
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, "dbWriterWorker_DoWork", false);
            }
        }

        /// <summary>
        /// 客户端套接字发送数据线程后台工作函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SocketSendCacheDataWorker_DoWork(object sender, EventArgs e)
        {
            try
            {
                Global._SendBufferManager.TrySendAll();
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, "SocketFlushBuffer_DoWork", false);
            }
        }

        private long LastSocketCheckTicks = TimeUtil.NOW();

        /// <summary>
        /// socket检查
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SocketCheckWorker_DoWork(object sender, EventArgs e)
        {
            try
            {
                long now = TimeUtil.NOW();
                //if (now - LastSocketCheckTicks < (1 * 60 * 1000))
                if (now - LastSocketCheckTicks < (5 * 60 * 1000))
                    return;

                LastSocketCheckTicks = now;

                //int timeCount = 1 * 60 * 1000;
                int timeCount = 15 * 60 * 1000;
                List<TMSKSocket> socketList = GameManager.OnlineUserSession.GetSocketList();
                foreach (TMSKSocket socket in socketList)
                {
                    long nowSocket = TimeUtil.NOW();
                    long spanSocket = nowSocket - socket.session.SocketTime[0];
                    if (socket.session.SocketState < 4 && spanSocket > timeCount)
                    {
                        KPlayer otherClient = KTPlayerManager.Find(socket);
                        if (null == otherClient)
                            Global.ForceCloseSocket(socket, "被GM踢了, 但是这个socket上没有对应的client");
                    }
                }
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, "SocketCheckWorker_DoWork", false);
            }
        }

        #endregion 线程部分

        /// <summary>
        /// 退出程序
        /// 原来的Window_Closing(object sender, CancelEventArgs e)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing()
        {
            //是否立刻关闭
            if (MustCloseNow)
            {
                return;
            }

            //已经进入了关闭模式
            if (EnterClosingMode)
            {
                return;
            }

            //是否进入了关闭模式
            EnterClosingMode = true;

            //设置不再接受新的请求，就是接受到后，立刻关闭
            //是否不再接受新的用户
            Global._TCPManager.MySocketListener.DontAccept = true;

            LastWriteDBLogTicks = 0; //强迫写缓存

            //设置退出标志
            NeedExitServer = true;
        }

        #endregion 游戏服务器具体功能部分

        #region 获取编译日期

        /// <summary>
        /// 获取程序的编译日期
        /// </summary>
        /// <returns></returns>
        public static string GetVersionDateTime()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            //AssemblyFileVersion = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
            int revsion = assembly.GetName().Version.Revision;//获取修订号
            int build = assembly.GetName().Version.Build;//获取内部版本号
            DateTime dtbase = new DateTime(2000, 1, 1, 0, 0, 0);//微软编译基准时间
            TimeSpan tsbase = new TimeSpan(dtbase.Ticks);
            TimeSpan tsv = new TimeSpan(tsbase.Days + build, 0, 0, revsion * 2);//编译时间，注意修订号要*2
            DateTime dtv = new DateTime(tsv.Ticks);//转换成编译时间
                                                   //return dtv.ToString("yyyy-MM-dd HH") + string.Format(" {0}", AssemblyFileVersion.FilePrivatePart);

            string version = "0.0";
            return dtv.ToString("yyyy-MM-dd_HH") + string.Format("_{0}", version);
        }

        #endregion 获取编译日期
    }
}