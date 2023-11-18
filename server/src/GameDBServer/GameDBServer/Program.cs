#define ENABLE_AUTO_RELEASE_MEMORY

using GameDBServer.Core;
using GameDBServer.DB;
using GameDBServer.Logic;
using GameDBServer.Logic.GuildLogic;
using GameDBServer.Logic.KT_ItemManager;
using GameDBServer.Logic.Pet;
using GameDBServer.Logic.StallManager;
using GameDBServer.Logic.SystemParameters;
using GameDBServer.Logic.TeamBattle;
using GameDBServer.Server;
using MySQLDriverCS;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Xml.Linq;

namespace GameDBServer
{
    public class Program
    {
        /// <summary>
        /// Thông tin phiên bản
        /// </summary>
        public static FileVersionInfo AssemblyFileVersion;

#if Windows

        public delegate bool ControlCtrlDelegate(int CtrlType);

        [DllImport("kernel32.dll")]
        private static extern bool SetConsoleCtrlHandler(ControlCtrlDelegate HandlerRoutine, bool Add);

        private static ControlCtrlDelegate newDelegate = new ControlCtrlDelegate(HandlerRoutine);

        /// <summary>
        /// Xử lý sự kiện nút bấm
        /// </summary>
        /// <param name="CtrlType"></param>
        /// <returns></returns>
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
        /// Xử lý khi có sự kiện đóng ứng dụng bằng nút X
        /// </summary>
        private static void HideCloseBtn()
        {
            Console.Title = "VLTK - Game Database Server";
            IntPtr windowHandle = FindWindow(null, Console.Title);
            IntPtr closeMenu = GetSystemMenu(windowHandle, IntPtr.Zero);
            uint SC_CLOSE = 0xF060;
            RemoveMenu(closeMenu, SC_CLOSE, 0x0);
        }

#endif

        /// <summary>
        /// Đối tượng
        /// </summary>
        public static Program ServerConsole = new Program();

        /// <summary>
        /// Callback khi có Packet
        /// </summary>
        /// <param name="cmd"></param>
        public delegate void CmdCallback(String cmd);

        /// <summary>
        /// Danh sách Packet
        /// </summary>
        private static Dictionary<string, CmdCallback> CmdDict = new Dictionary<string, CmdCallback>();

        /// <summary>
        /// Bool thể hiện việc việc chuẩn bị dữ liệu lên cache đã sẵn sàng chưa
        /// </summary>
        public static Boolean IsDataReady = false;

        /// <summary>
        /// Đánh dấu cần thoát không
        /// </summary>
        private static Boolean NeedExitServer = false;

        #region Exception Handler

        /// <summary>
        /// Gọi khi có ngoại lệ chưa được Handle
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                Exception exception = e.ExceptionObject as Exception;
                DataHelper.WriteFormatExceptionLog(exception, "CurrentDomain_UnhandledException", UnhandedException.ShowErrMsgBox, true);
            }
            catch
            {
            }
        }

        /// <summary>
        /// Gọi khi có ngoại lệ
        /// </summary>
        private static void ExceptionHook()
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
        }

        #endregion Exception Handler

        #region Xử lý File

        /// <summary>
        /// Xóa File
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
        /// Ghi lại ID Process vào File
        /// </summary>
        public static void WritePIDToFile(String strFile)
        {
            String strFileName = System.IO.Directory.GetCurrentDirectory() + "\\" + strFile;

            Process processes = Process.GetCurrentProcess();
            int nPID = processes.Id;
            File.WriteAllText(strFileName, "" + nPID);
        }

        /// <summary>
        /// Lấy ID Process từ File
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

        #endregion Xử lý File

        #region Main

        private static string[] cmdLineARGS = null;

        /// <summary>
        /// Hàm Main
        /// </summary>
        /// <param name="args"></param>
        private static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                cmdLineARGS = args;
            }

            /// Xóa các File tương ứng
            DeleteFile("Start.txt");
            DeleteFile("Stop.txt");
            DeleteFile("GameServerStop.txt");

#if Windows
            HideCloseBtn();

            SetConsoleCtrlHandler(newDelegate, true);

            try
            {
                if (Console.WindowWidth < 88)
                {
                    Console.BufferWidth = 88;
                    Console.WindowWidth = 88;
                }
            }
            catch
            {
            }
#endif
            /// Thiết lập Exception Handler
            ExceptionHook();

            InitCommonCmd();

            /// Thực hiện hàm khởi động hệ thống
            OnStartServer();

            ShowCmdHelpInfo();

            /// Ghi lại ID Process ra File
            WritePIDToFile("Start.txt");

            /// Tạo luồng tự ngắt
            Thread thread = new Thread(ConsoleInputThread);
            thread.IsBackground = true;
            thread.Start();
            while (!NeedExitServer || !ServerConsole.MustCloseNow || ServerConsole.MainDispatcherWorker.IsBusy)
            {
                Thread.Sleep(1000);
            }
            thread.Abort();
        }

        /// <summary>
        /// Console IO
        /// </summary>
        /// <param name="obj"></param>
        public static void ConsoleInputThread(object obj)
        {
            String cmd = System.Console.ReadLine();

            /// Chừng nào còn chạy
            while (!NeedExitServer)
            {
                if (null == cmd || 0 == cmd.CompareTo("exit"))
                {
                    if (ServerConsole.CanExit())
                    {
                        System.Console.WriteLine("Are you sure want to exit? Press y to exit immediatelly!");
                        cmd = System.Console.ReadLine();
                        if (0 == cmd.CompareTo("y"))
                        {
                            break;
                        }
                    }
                }
                else if (null == cmd || 0 == cmd.CompareTo("DoGiftProsecc"))
                {
                    System.Console.WriteLine("Sure DoGiftProsecc?");
                    cmd = System.Console.ReadLine();
                    if (0 == cmd.CompareTo("y"))
                    {
                        GuildManager.getInstance().DoGiftProsecc();
                        // break;
                    }
                }
                else if (null == cmd || 0 == cmd.CompareTo("UpdateShare"))
                {
                    System.Console.WriteLine("Sure UpdateShare?");
                    cmd = System.Console.ReadLine();
                    if (0 == cmd.CompareTo("y"))
                    {
                        // GuildManager.getInstance().UpdateGuildShareAutoMatic();
                        // break;
                    }
                }
                else if (null == cmd || 0 == cmd.CompareTo("ReloadItem"))
                {
                    System.Console.WriteLine("Sure ReloadItem?");
                    cmd = System.Console.ReadLine();
                    if (0 == cmd.CompareTo("y"))
                    {
                        //ItemManager.getInstance().ResetItemUpdate();
                        // break;
                    }
                }
                else
                {
                    ParseInputCmd(cmd);
                }

                cmd = System.Console.ReadLine();
            }

            OnExitServer();
        }

        /// <summary>
        /// Xử lý InputCMD
        /// </summary>
        /// <param name="cmd"></param>
        private static void ParseInputCmd(String cmd)
        {
            CmdCallback cb = null;

            if (CmdDict.TryGetValue(cmd, out cb) && null != cb)
            {
                cb(cmd);
            }
            else
            {
                System.Console.WriteLine("Unknow command, type 'help' to get more details.");
            }
        }

        /// <summary>
        /// Hàm này gọi khi khởi động hệ thống
        /// </summary>
        private static void OnStartServer()
        {
            ServerConsole.InitServer();

            Process processes = Process.GetCurrentProcess();
            int nPID = processes.Id;

            Console.Title = string.Format("GameDBServer ZoneID = {0}, Version = @{1} | ProseccID : " + nPID, GameDBManager.ZoneID, GetVersionDateTime());
        }

        /// <summary>
        /// Hàm này gọi khi thoát
        /// </summary>
        private static void OnExitServer()
        {
            ServerConsole.ExitServer();
        }

        #endregion Main

        #region Logic CMD

        /// <summary>
        /// Khởi tạo CMD
        /// </summary>
        private static void InitCommonCmd()
        {
            CmdDict.Add("help", ShowCmdHelpInfo);
            CmdDict.Add("gc", GarbageCollect);
            CmdDict.Add("show baseinfo", ShowServerBaseInfo);
            CmdDict.Add("show tcpinfo", ServerConsole.ShowServerTCPInfo);
        }

        /// <summary>
        /// Hiện thông tin CMD
        /// </summary>
        private static void ShowCmdHelpInfo(String cmd = null)
        {
            System.Console.WriteLine(string.Format("GameDB Server"));
            System.Console.WriteLine("Type 'help' to get more details.");
            System.Console.WriteLine("Type 'exit' then press Y to exit.");
            System.Console.WriteLine("Type 'gc' to start GarbageCollector.");
            System.Console.WriteLine("Type 'show baseinfo' to get basic informations.");
            System.Console.WriteLine("Type 'show tcpinfo' to get information about TCPSocket.");
        }

        /// <summary>
        /// Dọn rác
        /// </summary>
        private static void GarbageCollect(String cmd = null)
        {
            try
            {
                GC.Collect();
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, "GarbageCollect()", false);
            }
        }

        /// <summary>
        /// Hiện thông tin cơ bản của hệ thống
        /// </summary>
        private static void ShowServerBaseInfo(String cmd = null)
        {
            string info = string.Format("Number of connected Servers: {0}", ServerConsole.TotalConnections);

            System.Console.WriteLine(info);

            info = string.Format("Number of DBManagers: {0}", ServerConsole._DBManger.GetMaxConnsCount());

            System.Console.WriteLine(info);

            info = string.Format("User info count: {0}, Role info count: {1}",
                ServerConsole._DBManger.dbUserMgr.GetUserInfoCount(),
                ServerConsole._DBManger.DBRoleMgr.GetRoleInfoCount());

            System.Console.WriteLine(info);
        }

        /// <summary>
        /// Hiện thông tin TCP
        /// </summary>
        private void ShowServerTCPInfo(String cmd = null)
        {
            bool clear = cmd.Contains("/c");
            bool detail = cmd.Contains("/d");

            string info = string.Format("Total received bytes: {0:0.00} MB", _TCPManager.MySocketListener.TotalBytesReadSize / (1024.0 * 1024.0));
            System.Console.WriteLine(info);
            info = string.Format("Total sent bytes: {0:0.00} MB", _TCPManager.MySocketListener.TotalBytesWriteSize / (1024.0 * 1024.0));
            System.Console.WriteLine(info);

            //////////////////////////////////////////
            info = string.Format("Average process time (ms): {0}", TCPManager.processCmdNum != 0 ? TimeUtil.TimeMS(TCPManager.processTotalTime / TCPManager.processCmdNum) : 0);
            System.Console.WriteLine(info);
            info = string.Format("Details of packet which cost much time.");
            System.Console.WriteLine(info);

            int count = 0;
            lock (TCPManager.cmdMoniter)
            {
                foreach (PorcessCmdMoniter m in TCPManager.cmdMoniter.Values)
                {
                    Console.ForegroundColor = (ConsoleColor)(count % 5 + ConsoleColor.Green);
                    if (detail)
                    {
                        if (count++ == 0)
                        {
                            info = string.Format("{0, -48}, {1, 6}, {2, 7}, {3, 7}, {4, 7}, {5, 7}", "Info", "Process time", "Average process time", "Use time", "Max process time", "Max wait time");
                            System.Console.WriteLine(info);
                        }
                        info = string.Format("{0, -50}, {1, 11}, {2, 13:0.##}, {3, 13:0.##}, {4, 13:0.##}, {5, 13:0.##}", (TCPGameServerCmds)m.cmd, m.processNum, TimeUtil.TimeMS(m.avgProcessTime()), TimeUtil.TimeMS(m.processTotalTime), TimeUtil.TimeMS(m.processMaxTime), TimeUtil.TimeMS(m.maxWaitProcessTime));
                        System.Console.WriteLine(info);
                    }
                    else
                    {
                        if (count++ == 0)
                        {
                            info = string.Format("{0, -48}, {1, 6}, {2, 7}, {3, 7}", "Info", "Process time", "Average process time", "Use time");
                            System.Console.WriteLine(info);
                        }
                        info = string.Format("{0, -50}, {1, 11}, {2, 13:0.##}, {3, 13:0.##}", (TCPGameServerCmds)m.cmd, m.processNum, TimeUtil.TimeMS(m.avgProcessTime()), TimeUtil.TimeMS(m.processTotalTime));
                        System.Console.WriteLine(info);
                    }
                    if (clear)
                    {
                        m.maxWaitProcessTime = 0;
                        m.processMaxTime = 0;
                        m.processNum = 0;
                        m.processTotalTime = 0;
                    }
                }
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        #endregion Logic CMD

        #region Kết nối

        /// <summary>
        /// Tổng số kết nối
        /// </summary>
        private int _TotalConnections = 0;

        /// <summary>
        /// Tổng số kết nối
        /// </summary>
        /// <param name="index"></param>
        /// <param name="info"></param>
        public int TotalConnections
        {
            set { _TotalConnections = value; }
            get { return _TotalConnections; }
        }

        #endregion Kết nối

        #region GameDB

        /// <summary>
        /// Quản lý DB
        /// </summary>
        private DBManager _DBManger = DBManager.getInstance();

        /// <summary>
        /// Quản lý TCP
        /// </summary>
        private TCPManager _TCPManager = null;

        /// <summary>
        /// Đánh dấu có cần phải ngắt kết nối không
        /// </summary>
        private bool MustCloseNow = false;

        /// <summary>
        /// Đánh dấu vào trạng thái thoát
        /// </summary>
        private bool EnterClosingMode = false;

        /// <summary>
        /// Timer tick mỗi 60s 1 lần
        /// </summary>
        private int ClosingCounter = 30 * 200;

        public static long LastRunningDbtime = TimeUtil.NOW();

        /// <summary>
        /// Thời điểm lần trước ghi LogDB
        /// </summary>
        private long LastWriteDBLogTicks = DateTime.Now.Ticks / 10000;

#if ENABLE_AUTO_RELEASE_MEMORY

        /// <summary>
        /// Thời gian tự giải phóng dữ liệu
        /// </summary>
        private long NextReleaseIdleDataTicks = DateTime.Now.Ticks / 10000;

#endif

        /// <summary>
        /// Xử lý tiến trình thoát ứng dụng
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void closingTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                string title = "";

                ClosingCounter -= 200;

                if (ClosingCounter <= 0)
                {
                    MustCloseNow = true;
                }
                else
                {
                    int counter = (ClosingCounter / 200);
                    title = string.Format("GameDBServer is shutting down at ZoneID = {0}, Time left: {1}", GameDBManager.ZoneID, counter);
                }

                Console.Title = title;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, "closingTimer_Tick", false);
            }
        }

        public static string CreateMysqlConnectString(string ServerAddress, string DatabaseName, string UserName, string Password)
        {
            string STR = "Server=" + ServerAddress + "; Port=3306; Database=" + DatabaseName + "; Uid=" + UserName + ";Pwd = " + Password + "; ";

            return STR;
        }

        /// <summary>
        /// Background Main Worker
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainDispatcherWorker_DoWork(object sender, EventArgs e)
        {
            long startTicks = DateTime.Now.Ticks / 10000;
            long endTicks = DateTime.Now.Ticks / 10000;

            int maxSleepMs = 1000;
            int sleepMs = 1000;

            while (true)
            {
                try
                {
                    startTicks = DateTime.Now.Ticks / 10000;

                    /// Thực hiện các công việc ngầm
                    ExecuteBackgroundWorkers(null, EventArgs.Empty);

                    if (NeedExitServer)
                    {
                        maxSleepMs = 200;
                        sleepMs = 200;

                        /// Thực hiện công việc đóng ứng dụng
                        closingTimer_Tick(null, null);

                        if (MustCloseNow)
                        {
                            break;
                        }
                    }

                    endTicks = DateTime.Now.Ticks / 10000;

                    sleepMs = (int)Math.Max(1, maxSleepMs - (endTicks - startTicks));

                    Thread.Sleep(sleepMs);

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

            System.Console.WriteLine("Main loop exit, press enter to quit!");
            if (0 != GetServerPIDFromFile())
            {
                WritePIDToFile("Stop.txt");
            }
        }

        /// <summary>
        /// Tên ứng dụng
        /// </summary>
        private static string ProgramExtName = "";

        /// <summary>
        /// Khởi tạo tên ứng dụng
        /// </summary>
        /// <returns></returns>
        private static void InitProgramExtName()
        {
            ProgramExtName = AppDomain.CurrentDomain.BaseDirectory;
        }

        /// <summary>
        /// Khởi tạo hệ thống
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void InitServer()
        {
            InitProgramExtName();

            System.Console.WriteLine("Init language dictionary.");

            XElement xml = null;

            System.Console.WriteLine("Init App Config");

            try
            {
                xml = XElement.Load(@"AppConfig.xml");
            }
            catch (Exception)
            {
                throw new Exception(string.Format("Read XML file '{0}' faild.", @"AppConfig.xml"));
            }

            /// Thiết lập level sẽ ghi lại logs
            LogManager.LogTypeToWrite = (LogTypes)(int)Global.GetSafeAttributeLong(xml, "Server", "LogType");

            /// Thiết lập level sẽ ghi lại event
            GameDBManager.SystemServerSQLEvents.EventLevel = (EventLevels)(int)Global.GetSafeAttributeLong(xml, "Server", "EventLevel");

            int dbLog = Math.Max(0, (int)Global.GetSafeAttributeLong(xml, "DBLog", "DBLogEnable"));

            GameDBManager.HttpPort = (int)Global.GetSafeAttributeLong(xml, "Socket", "HttpPort");

            GameDBManager.ZoneID = (int)Global.GetSafeAttributeLong(xml, "Zone", "ID");

            string uname = StringEncrypt.Decrypt(Global.GetSafeAttributeStr(xml, "Database", "uname"), "eabcix675u49,/", "3&3i4x4^+-0");
            string upasswd = StringEncrypt.Decrypt(Global.GetSafeAttributeStr(xml, "Database", "upasswd"), "eabcix675u49,/", "3&3i4x4^+-0");

            System.Console.WriteLine("Number of max connections: {0}", (int)Global.GetSafeAttributeLong(xml, "Database", "maxConns"));
            System.Console.WriteLine("IP address: {0}", Global.GetSafeAttributeStr(xml, "Database", "ip"));
            System.Console.WriteLine("DB name: {0}", Global.GetSafeAttributeStr(xml, "Database", "dname"));
            System.Console.WriteLine("Char set: {0}", Global.GetSafeAttributeStr(xml, "Database", "names"));

            DBConnections.dbNames = Global.GetSafeAttributeStr(xml, "Database", "names");

            System.Console.WriteLine("Establishing connection to Database...");

            _DBManger.LoadDatabase(new MySQLConnectionString(
                    Global.GetSafeAttributeStr(xml, "Database", "ip"),
                    Global.GetSafeAttributeStr(xml, "Database", "dname"),
                    uname,
                    upasswd),
                (int)Global.GetSafeAttributeLong(xml, "Database", "maxConns"),
                (int)Global.GetSafeAttributeLong(xml, "Database", "codePage"));

            /// Chắc chắn là đã khai báo đúng ZONEID
            // ValidateZoneID();

            GameDBManager.DBName = Global.GetSafeAttributeStr(xml, "Database", "dname");
            DBWriter.ValidateDatabase(_DBManger, GameDBManager.DBName);

            /// Tự tăng giá trị
            if (!Global.InitDBAutoIncrementValues(_DBManger))
            {
                System.Console.WriteLine("A dangerous error was occurred, press Y to exit.");
                return;
            }

            LineManager.LoadConfig();

            System.Console.WriteLine("Init network...");

            _TCPManager = TCPManager.getInstance();
            _TCPManager.initialize((int)Global.GetSafeAttributeLong(xml, "Socket", "capacity"));

            /// Khởi tạo DBManager
            _TCPManager.DBMgr = _DBManger;
            _TCPManager.RootWindow = this;
            _TCPManager.Start(Global.GetSafeAttributeStr(xml, "Socket", "ip"),
                (int)Global.GetSafeAttributeLong(xml, "Socket", "port"));

            System.Console.WriteLine("Init background works...");

            /// Luồng thực hiện ghi thông tin tiền
            //updateMoneyWorker = new BackgroundWorker();
            //updateMoneyWorker.DoWork += updateMoneyWorker_DoWork;

#if ENABLE_AUTO_RELEASE_MEMORY
            /// Luồng thực hiện giải phóng bộ nhớ
            releaseMemoryWorker = new BackgroundWorker();
            releaseMemoryWorker.DoWork += releaseMemoryWorker_DoWork;
#endif

            /// Luồng thực hiện đọc ghi bảng xếp hạng
            updatePaiHangWorker = new BackgroundWorker();
            updatePaiHangWorker.DoWork += updatePaiHangWorker_DoWork;

            UpdateItem = new BackgroundWorker();
            UpdateItem.DoWork += UpdateItemWorker_DoWork;

            UpdateGuild = new BackgroundWorker();
            UpdateGuild.DoWork += UpdateGuildgWorker_DoWork;

            /// Luồng thực hiện ghi vào DB
            dbWriterWorker = new BackgroundWorker();
            dbWriterWorker.DoWork += dbWriterWorker_DoWork;

            /// Luồng thực hiện kiểm tra Mail
            updateLastMailWorker = new BackgroundWorker();
            updateLastMailWorker.DoWork += updateLastMail_DoWork;

            /// Luồng chính
            MainDispatcherWorker = new BackgroundWorker();
            MainDispatcherWorker.DoWork += MainDispatcherWorker_DoWork;

            UnhandedException.ShowErrMsgBox = false;

            if (!MainDispatcherWorker.IsBusy)
            {
                MainDispatcherWorker.RunWorkerAsync();
            }

            Console.WriteLine("Loading Pet...");
            KTPetManager.Init(_DBManger);

            Console.WriteLine("Loading TeamBattle Data...");
            KTTeamBattleManager.Init(_DBManger);

            Console.WriteLine("Loading Guild Data....");
            GuildManager.getInstance().Setup(_DBManger);

            Console.WriteLine("Loading All StallData....");
            StallManager.getInstance().Setup(_DBManger);

            IsDataReady = true;

            /// Luồng tải thông tin WebAPI
            System.Console.WriteLine("BackgroundWorker WEBAPI Start");
            WebAPIService = new BackgroundWorker();
            WebAPIService.DoWork += LoadWebApi_DoWork;

            if (!WebAPIService.IsBusy)
            {
                WebAPIService.RunWorkerAsync();
            }

            GameDBManager.GameConfigMgr.UpdateGameConfigItem("gamedb_version", GetVersionDateTime());
            DBWriter.UpdateGameConfig(_DBManger, "gamedb_version", GetVersionDateTime());

            System.Console.WriteLine("GameDBServer started successfully!");
        }

        /// <summary>
        /// Update các công việc liên quan tới bang hội
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateGuildgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                if (Program.IsDataReady)
                {
                    /// Cập nhật các tham biến hệ thống
                    SystemGlobalParametersManager.UpdateSystemGlobalParameters(this._DBManger);
                    GuildManager.getInstance().UpdateGuildProsecc();
                }
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.SQL, "TOAC UpdateGuildgWorker_DoWork:" + ex.ToString());
            }
        }

        /// <summary>
        /// Update các công việc liên quan tới vật phẩm
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateItemWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                DelayUpdateManager.getInstance().DoUpdateSql();
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.SQL, "Bug UpdateItemWorker_DoWork:" + ex.ToString());
            }
        }

        /// <summary>
        /// Trả về địa chỉ IP hiện tại của ứng dụng
        /// </summary>
        /// <returns></returns>
        public static string GetLocalIPAddress()
        {
            if (GameDBManager.ZoneID == 9)
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

        /// <summary>
        /// Công việc thực hiện tải thông tin WebAPI
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LoadWebApi_DoWork(object sender, EventArgs e)
        {
            HttpListener web = new HttpListener();

            Console.WriteLine("IP LA:" + GetLocalIPAddress());
            web.Prefixes.Add("http://" + GetLocalIPAddress() + ":"+ GameDBManager.HttpPort + "/");
            string msg = "OK";

            web.Start();

            System.Console.WriteLine("HTTP Service Bring Port :"+ GameDBManager.HttpPort + "");
            while (true)
            {
                try
                {
                    HttpListenerContext context = web.GetContext();

                    if (context.Request.QueryString["FixPostion"] != null && context.Request.QueryString["KeyStore"] != null)
                    {
                        string MD5Key = HttpService.MakeMD5Hash(HttpService.WebKey);

                        string KeyStore = context.Request.QueryString["KeyStore"].ToString().Trim();

                        if (KeyStore == MD5Key)
                        {
                            string RoleID = context.Request.QueryString["RoleID"].ToString().Trim();

                            int RID = Int32.Parse(RoleID);

                            HttpService.FixPostion(RID, _DBManger);
                        }
                    }

                    if (context.Request.QueryString["FixZoneID"] != null && context.Request.QueryString["KeyStore"] != null)
                    {
                        string MD5Key = HttpService.MakeMD5Hash(HttpService.WebKey);

                        string KeyStore = context.Request.QueryString["KeyStore"].ToString().Trim();

                        if (KeyStore == MD5Key)
                        {
                            string RoleID = context.Request.QueryString["RoleID"].ToString().Trim();

                            string FixZoneID = context.Request.QueryString["FixZoneID"].ToString().Trim();

                            int RID = Int32.Parse(RoleID);
                            int ZonID = Int32.Parse(FixZoneID);

                            HttpService.FixZoneID(RID, ZonID, _DBManger);
                        }
                    }

                    if (context.Request.QueryString["ShutDownServer"] != null && context.Request.QueryString["KeyStore"] != null)
                    {
                        string MD5Key = HttpService.MakeMD5Hash(HttpService.WebKey);

                        string KeyStore = context.Request.QueryString["KeyStore"].ToString().Trim();

                        if (KeyStore == MD5Key)
                        {
                            Thread _Thread = new Thread(() => OnExitServer());
                            _Thread.Start();
                        }
                    }

                    HttpListenerResponse response = context.Response;

                    byte[] buffer = Encoding.UTF8.GetBytes(msg);
                    response.ContentLength64 = buffer.Length;
                    Stream st = response.OutputStream;
                    st.Write(buffer, 0, buffer.Length);
                    context.Response.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }

        /// <summary>
        /// Kiểm tra ID khu vực có hợp lý không
        /// </summary>
        public static void ValidateZoneID()
        {
            System.Console.Write("Enter ZoneID: ");

            String readLine = "";

            if (cmdLineARGS != null && cmdLineARGS.Length > 0)
            {
                readLine = cmdLineARGS[0];
            }
            else
            {
                readLine = System.Console.ReadLine();
            }

            while (true)
            {
                try
                {
                    int inputZone = int.Parse(readLine);
                    if (inputZone == GameDBManager.ZoneID)
                    {
                        System.Console.WriteLine("Zone validated successfully!!");
                        break;
                    }
                    else
                    {
                        System.Console.WriteLine("Zone is invalid!!");
                    }
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine(ex.Message);
                }

                System.Console.Write("Enter ZoneID: ");
                readLine = System.Console.ReadLine();
            }
        }

        /// <summary>
        /// Thực hiện thoát ứng dụng
        /// </summary>
        public void ExitServer()
        {
            if (NeedExitServer)
            {
                return;
            }

            _TCPManager.Stop();

            Window_Closing();

            System.Console.WriteLine("GameDBServer is going to shutdown. Press Enter to quit.");

            if (0 == GetServerPIDFromFile())
            {
                //  String cmd = System.Console.ReadLine();

                while (true)
                {
                    if (MainDispatcherWorker.IsBusy)
                    {
                        System.Console.WriteLine("Shutting down...");
                        Thread.Sleep(1000);
                        continue;
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        #region

        /// <summary>
        /// Luồng cập nhật Đồng
        /// </summary>
        // private BackgroundWorker updateMoneyWorker;

#if ENABLE_AUTO_RELEASE_MEMORY

        /// <summary>
        /// Luồng giải phóng bộ nhớ
        /// </summary>
        private BackgroundWorker releaseMemoryWorker;

#endif

        /// <summary>
        /// Luồng cập nhật bảng xếp hạng
        /// </summary>
        private BackgroundWorker updatePaiHangWorker;

        private BackgroundWorker UpdateGuild;

        private BackgroundWorker UpdateItem;

        /// <summary>
        /// Luồng ghi DB
        /// </summary>
        private BackgroundWorker dbWriterWorker;

        /// <summary>
        /// Luồng Service WebAPI
        /// </summary>
        private BackgroundWorker WebAPIService;

        /// <summary>
        /// Luồng kiểm tra Mail
        /// </summary>
        private BackgroundWorker updateLastMailWorker;

        /// <summary>
        /// Luồng chính
        /// </summary>
        private BackgroundWorker MainDispatcherWorker;

        /// <summary>
        /// Thời điểm lần trước cập nhật giải phóng bộ nhớ
        /// </summary>
        private long LastReleaseMemoryTicks = DateTime.Now.Ticks / 10000;

        /// <summary>
        /// Thực thi công việc của các Background Worker
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExecuteBackgroundWorkers(object sender, EventArgs e)
        {
            //LogManager.WriteLog(LogTypes.SQL, "DO update PaiHangWorker!");
            if (!updatePaiHangWorker.IsBusy)
            {
                updatePaiHangWorker.RunWorkerAsync();
            }
            else
            {
                LogManager.WriteLog(LogTypes.SQL, "Update Ranking BUSSYYYYYYYYYYYYYYYYYYYYYYYY!");
            }

            if (!UpdateItem.IsBusy)
            {
                UpdateItem.RunWorkerAsync();
            }
            else
            {
                LogManager.WriteLog(LogTypes.SQL, "UpdatItem BUSSYYYYYYYYYYYYYYYYYYYYYYYY!");
            }

            if (!UpdateGuild.IsBusy)
            {
                UpdateGuild.RunWorkerAsync();
            }
            else
            {
                LogManager.WriteLog(LogTypes.SQL, "UpdateGuild BUSSYYYYYYYYYYYYYYYYYYYYYYYY!");
            }

            /// Ghi dữ liệu vào DB
            if (!dbWriterWorker.IsBusy)
            {
                dbWriterWorker.RunWorkerAsync();
            }
            else
            {
                LogManager.WriteLog(LogTypes.SQL, "DBManager  BUSSYYYYYYYYYYYYYYYYYYYYYYYY!");
            }

            // Call trong trường hợp webservice chết thì tự chạy lại
            //if (!WebAPIService.IsBusy)
            //{
            //    WebAPIService.RunWorkerAsync();
            //}

            long nowTicks = DateTime.Now.Ticks / 10000;
            if (nowTicks - LastReleaseMemoryTicks >= (1 * 60 * 1000))
            {
                LastReleaseMemoryTicks = DateTime.Now.Ticks / 10000;
                if (!releaseMemoryWorker.IsBusy) { releaseMemoryWorker.RunWorkerAsync(); }
            }

            ThreadPool.GetAvailableThreads(out int workerThreads, out int completionPortThreads);

            Console.WriteLine("TOTAL CONNECT AVALIBLE :" + ServerConsole._DBManger.GetMaxConnsCount() + " | WorkerThreads: " + workerThreads + ", CompletionPortThreads: " + completionPortThreads);

            LogManager.WriteLog(LogTypes.Trace, "WorkerThreads: " + workerThreads + ", CompletionPortThreads: " + completionPortThreads);
        }

        ///// <summary>
        ///// Xử lý tiền tệ
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void updateMoneyWorker_DoWork(object sender, DoWorkEventArgs e)
        //{
        //    try
        //    {
        //        /// Truy vấn tổng số Đồng trong hệ thống
        //        UserMoneyMgr.QueryTotalUserMoney();

        //        /// Cập nhật các tham biến hệ thống
        //        SystemGlobalParametersManager.UpdateSystemGlobalParameters(this._DBManger);
        //    }
        //    catch (Exception ex)
        //    {
        //        DataHelper.WriteFormatExceptionLog(ex, "updateMoneyWorker_DoWork", false);
        //    }
        //}

#if ENABLE_AUTO_RELEASE_MEMORY

        /// <summary>
        /// Giải phóng bộ nhớ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void releaseMemoryWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                long ticks = TimeUtil.NOW();
                if (ticks < NextReleaseIdleDataTicks)
                {
                    return;
                }

                NextReleaseIdleDataTicks = ticks + 300 * 1000;

                int ticksSlot = (30 * 60 * 1000);

                LogManager.WriteLog(LogTypes.SQL, "CLEAR ROLE LastReleaseMemoryTicks");
                /// Tự xóa User khỏi hệ thống khi không sử dụng trong 30p
                _DBManger.dbUserMgr.ReleaseIdleDBUserInfos(ticksSlot);
                _DBManger.DBRoleMgr.ReleaseIdleDBRoleInfos(ticksSlot);

                LogManager.WriteLog(LogTypes.SQL, "CLEAR ROLE LastReleaseMemoryTicks==> DONE");
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, "releaseMemoryWorker_DoWork", false);
            }
        }

#endif

        /// <summary>
        /// Cập nhật bảng xếp hạng
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void updatePaiHangWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                if (Program.IsDataReady)
                {
                    /// Thực hiện update rank mỗi lần chạy lại gameDB
                    RankingManager.getInstance().UpdateRank(_DBManger);
                }
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.SQL, "updatePaiHangWorker_DoWork TOAC :" + ex.ToString());
            }
        }

        /// <summary>
        /// Luồng ghi DB
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dbWriterWorker_DoWork(object sender, EventArgs e)
        {
            try
            {
                DBManager.getInstance().DBConns.SupplyConnections();

                long ticks = DateTime.Now.Ticks / 10000;
                if (ticks - LastWriteDBLogTicks < (30 * 1000))
                {
                    return;
                }

                LastWriteDBLogTicks = ticks;
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, "dbWriterWorker_DoWork", false);
            }
        }

        /// <summary>
        /// Luồng cập nhật Mail
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void updateLastMail_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                /// Kiểm tra Mail
                UserMailManager.ScanLastMails(_DBManger);

                /// Xóa các Mail hết hạn
                UserMailManager.ClearOverdueMails(_DBManger);
            }
            catch (Exception ex)
            {
                DataHelper.WriteFormatExceptionLog(ex, "updateLastMail_DoWork", false);
            }
        }

        #endregion GameDB

        /// <summary>
        /// Kiểm tra có thể thoát ứng dụng không
        /// </summary>
        /// <returns></returns>
        public bool CanExit()
        {
            if (_TCPManager.MySocketListener.ConnectedSocketsCount > 0)
            {
                ///LineId cục bộ của máy chủ thông thường là 1 và LineID cục bộ của máy chủ Liên Server = ZoneId. Nếu đang online, để tránh mất dữ liệu, không thể bị buộc đóng
                if (Global.IsGameServerClientOnline(1) || Global.IsGameServerClientOnline(GameDBManager.ZoneID))
                {
                    System.Console.WriteLine(string.Format("Can not terminate the connection to {0}!", Console.Title));
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Hàm này gọi đến khi đóng ứng dụng
        /// </summary>
        private void Window_Closing()
        {
            if (MustCloseNow)
            {
                return;
            }

            if (EnterClosingMode)
            {
                return;
            }

            EnterClosingMode = true;
            LastWriteDBLogTicks = 0;
            NeedExitServer = true;
        }

        #endregion

        #region Version

        /// <summary>
        /// Thông tin phiên bản
        /// </summary>
        /// <returns></returns>
        public static string GetVersionDateTime()
        {
            AssemblyFileVersion = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
            int revsion = Assembly.GetExecutingAssembly().GetName().Version.Revision;
            int build = Assembly.GetExecutingAssembly().GetName().Version.Build;
            DateTime dtbase = new DateTime(2000, 1, 1, 0, 0, 0);
            TimeSpan tsbase = new TimeSpan(dtbase.Ticks);
            TimeSpan tsv = new TimeSpan(tsbase.Days + build, 0, 0, revsion * 2);
            DateTime dtv = new DateTime(tsv.Ticks);
            return dtv.ToString("yyyy-MM-dd HH") + string.Format(" {0}", AssemblyFileVersion.FilePrivatePart);
        }

        #endregion
    }
}