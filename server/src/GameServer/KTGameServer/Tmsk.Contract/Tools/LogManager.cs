using GameServer.Core.Executor;
using System;
using System.Collections.Generic;
using System.IO;

namespace Server.Tools
{
    /// <summary>
    /// Loại Log
    /// </summary>
    public enum LogTypes
    {
        Ignore = -1, //忽略
        Info = 0,
        Warning = 1,
        Error = 2,
        SQL = 3,
        Robot = 4,
        Analysis = 5,

        /// <summary>
        /// Trang bị
        /// </summary>
        EquipBody = 6,
        
        /// <summary>
        /// Dữ liệu
        /// </summary>
        Data = 7,
        
        /// <summary>
        /// Mua vật phẩm
        /// </summary>
        BuyNpc = 8,

        /// <summary>
        /// Ngoại lệ
        /// </summary>
        Exception = 9,

        /// <summary>
        /// Kỹ năng
        /// </summary>
        Skill = 10,

        /// <summary>
        /// Thông báo Timer
        /// </summary>
        Alert = 11,

        /// <summary>
        /// Sạp hàng cá nhân
        /// </summary>
        Stall = 12,

        /// <summary>
        /// Vật phẩm
        /// </summary>
        Item = 13,

        /// <summary>
        /// Nhiệm vụ
        /// </summary>
        Task = 14,

        /// <summary>
        /// Giao dịch
        /// </summary>
        Trade = 15,


        /// <summary>
        /// Log vị trí
        /// </summary>
        RolePosition = 16,

        /// <summary>
        /// Nhiệm vụ
        /// </summary>
        Quest = 17,

        /// <summary>
        /// Bách Bảo Rương
        /// </summary>
        SeashellCircle = 18,

        /// <summary>
        /// Tống kim
        /// </summary>
        SongJinBattle = 19,

        /// <summary>
        /// Lua
        /// </summary>
        Lua = 20,

        /// <summary>
        /// Sự kiện
        /// </summary>
        GameMapEvents = 21,

        /// <summary>
        /// Phụ bản
        /// </summary>
        CopyScene = 22,


        /// <summary>
        /// Thi đấu môn phái
        /// </summary>
        FactionBattle = 23,

        /// <summary>
        /// Report Timer
        /// </summary>
        TimerReport = 24,

        /// <summary>
        /// Bang hội
        /// </summary>
        Guild = 25,

        /// <summary>
        /// Nạp thẻ
        /// </summary>
        Rechage = 26,

        /// <summary>
        /// Ngũ hành hồn thạch
        /// </summary>
        NHHT = 27,

        /// <summary>
        /// Chúc phúc
        /// </summary>
        PlayerPray = 28,

        /// <summary>
        /// Cường hóa
        /// </summary>
        Enhance = 29,

        /// <summary>
        /// Du Long Các
        /// </summary>
        YouLongGe = 30,

        /// <summary>
        /// Đăng xuất
        /// </summary>
        Logout = 31,



        /// <summary>
        /// Kỹ năng sống
        /// </summary>
        LifeSkill = 32,

        /// <summary>
        /// Chat
        /// </summary>
        Chat = 33,

        /// <summary>
        /// Tranh đoạt lãnh thổ
        /// </summary>
        GuildWarManager = 34,

        /// <summary>
        /// Võ lâm liên đấu
        /// </summary>
        TeamBattle = 35,

        /// <summary>
        /// Thông tin thiết bị đăng nhập
        /// </summary>
        DeviceInfo = 36,

        /// <summary>
        /// Tần Lăng
        /// </summary>
        EmperorTomb = 37,

        /// <summary>
        /// Vòng quay may mắn
        /// </summary>
        LuckyCircle = 38,

        /// <summary>
        /// Sự kiện đặc biệt
        /// </summary>
        SpecialEvent = 39,


        /// <summary>
        ///  Toàn bộ phúc lợi
        /// </summary>
        Welfare = 40,

        /// <summary>
        /// Pet
        /// </summary>
        Pet = 41,


        WarCity = 42,


        GrowTree = 43,

        /// <summary>
        /// Lôi đài di động
        /// </summary>
        DynamicArena = 44,


        PickUpNotify = 45,


        ShopGuild = 46,

        /// <summary>
        /// Vòng quay may mắn - đặc biệt
        /// </summary>
        TurnPlate = 47,


        CargoCarriage = 48,

        Fatal = 1000,

    }

    public class LogFilePoolItem
    {
        public FileStream _FileStream;
        public StreamWriter _StreamWriter;
        public long OpenTimeOnHours; //打开的时间
        public long OpenTimeOnDayOfYear; //打开的时间

        ~LogFilePoolItem()
        {
            if (null != _StreamWriter)
            {
                try
                {
                    _StreamWriter.Close();
                }
                catch { }
                _StreamWriter = null;
            }
        }
    }

    /// <summary>
    /// 日志管理类
    /// </summary>
    public static class LogManager
    {
        /// <summary>
        /// 异常日志字典,防止相同异常重复写过多日志
        /// </summary>
        private static Dictionary<string, int> ExceptionCacheDict = new Dictionary<string, int>();

        /// <summary>
        /// 上次清理日志缓存的时间,定时清理是为了保证手动删除所有日志文件后,一段时间后还可以再次生成
        /// </summary>
        private static DateTime LastClearCacheTime;

        /// <summary>
        /// 日志文件按类型缓存对象字典
        /// </summary>
        public static Dictionary<LogTypes, LogFilePoolItem> LogType2FileDict { get; set; } = new Dictionary<LogTypes, LogFilePoolItem>();

        /// <summary>
        /// 允许实际写的日志级别
        /// </summary>
        public static LogTypes LogTypeToWrite
        {
            get;
            set;
        }

        /// <summary>
        /// 是否允许输出到dbgView窗口
        /// </summary>
        public static bool EnableDbgView = false;

        /// <summary>
        /// 日志输出目录
        /// </summary>
        private static string _LogPath = string.Empty;

        /// <summary>
        /// 日志输出目录
        /// </summary>
        public static string LogPath
        {
            get
            {
                lock (mutex)
                {
                    if (_LogPath == string.Empty)
                    {
                        _LogPath = DataHelper2.CurrentDirectory + @"log/";
                        if (!System.IO.Directory.Exists(_LogPath))
                        {
                            System.IO.Directory.CreateDirectory(_LogPath);
                        }
                    }
                }

                return _LogPath;
            }
            set
            {
                lock (mutex)
                {
                    _LogPath = value;
                }

                if (!System.IO.Directory.Exists(_LogPath))
                {
                    System.IO.Directory.CreateDirectory(_LogPath);
                }
            }
        }

        /// <summary>
        /// 异常输出目录
        /// </summary>
        private static string _ExceptionPath = string.Empty;

        /// <summary>
        /// 异常个数
        /// </summary>
        private static int _ExceptionCount;

        /// <summary>
        /// 异常输出目录
        /// </summary>
        public static string ExceptionPath
        {
            get
            {
                lock (mutex)
                {
                    if (_ExceptionPath == string.Empty)
                    {
                        _ExceptionPath = DataHelper2.CurrentDirectory + @"Exception/";
                        if (!System.IO.Directory.Exists(_ExceptionPath))
                        {
                            System.IO.Directory.CreateDirectory(_ExceptionPath);
                        }
                    }
                }

                return _ExceptionPath;
            }
            set
            {
                lock (mutex)
                {
                    _ExceptionPath = value;
                }

                if (!System.IO.Directory.Exists(_ExceptionPath))
                {
                    System.IO.Directory.CreateDirectory(_ExceptionPath);
                }
            }
        }

        /// <summary>
        /// 将日志写入指定的文件
        /// </summary>
        private static void WriteLogEx(LogTypes logType, string logMsg)
        {
            try
            {
                StreamWriter sw = GetStreamWriter(logType);

                // 分析日志有特殊的日志格式
                //string text = (logType == LogTypes.Analysis) ? logMsg : (TimeUtil.NowDateTime().ToString("yyyy-MM-dd HH:mm:ss: ") + logMsg);
                string text = TimeUtil.NowDateTime().ToString("yyyy-MM-dd HH:mm:ss: ") + logMsg;
                if (EnableDbgView)
                {
                    System.Diagnostics.Debug.WriteLine(text);
                }

                sw.WriteLine(text);
            }
            catch (Exception ex)
            {
                string exstr = ex.ToString();
            }
        }

        /// <summary>
        /// 将异常写入指定的文件
        /// </summary>
        private static void _WriteException(string exceptionMsg)
        {
            WriteLog(LogTypes.Exception, "##exception##\r\n" + exceptionMsg);
        }

        /// <summary>
        /// 写日志的锁对象
        /// </summary>
        private static object mutex = new object();

        /// <summary>
        /// 写日志
        /// </summary>
        /// <param name="logType">日志类型</param>
        /// <param name="logMsg">日志信息</param>

        public static void WriteLog(LogTypes logType, string logMsg, Exception ex = null, bool bConsole = true)
        {
            if ((int)logType < (int)LogTypeToWrite) //不必记录
            {
                return;
            }

            lock (mutex)
            {
                WriteLogEx(logType, logMsg);
            }

            if (logType >= LogTypes.Fatal && bConsole)
            {
                ConsoleColor color = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                SysConOut.WriteLine(logMsg);
                Console.ForegroundColor = color;
            }

            if (null != ex)
            {
                WriteException(logMsg + ex.ToString());
            }
        }

        /// <summary>
        /// 写异常
        /// </summary>
        /// <param name="exceptionMsg">异常信息</param>
        public static void WriteException(string exceptionMsg)
        {
            _WriteException(exceptionMsg);
        }

        private static StreamWriter GetStreamWriter(LogTypes logType)
        {
            LogFilePoolItem item;
            DateTime now = TimeUtil.NowDateTime();
            lock (mutex)
            {
                if (!LogType2FileDict.TryGetValue(logType, out item))
                {
                    item = new LogFilePoolItem();
                    LogType2FileDict.Add(logType, item);
                }
                if (now.Hour != item.OpenTimeOnHours || now.DayOfYear != item.OpenTimeOnDayOfYear || item._StreamWriter == null)
                {
                    if (null != item._StreamWriter)
                    {
                        item._StreamWriter.Close();
                        item._StreamWriter = null;
                    }
                    item._StreamWriter = File.AppendText(LogPath + logType.ToString() + "_" + now.ToString("yyyyMMdd") + ".log");
                    item.OpenTimeOnHours = now.Hour;
                    item.OpenTimeOnDayOfYear = now.DayOfYear;
                    item._StreamWriter.AutoFlush = true;
                }
                return item._StreamWriter;
            }
        }

        /// <summary>
        /// 写入异常信息,使用减少不可预知的原因导致有过多重复的异常日志
        /// </summary>
        /// <param name="exStr"></param>
        public static void WriteExceptionUseCache(string exStr)
        {
            try
            {
                DateTime now = TimeUtil.NowDateTime();
                lock (ExceptionCacheDict)
                {
                    if (now.Hour != LastClearCacheTime.Hour || ExceptionCacheDict.Count > 10000)
                    {
                        ExceptionCacheDict.Clear();
                        LastClearCacheTime = now;
                    }

                    int count = 0;
                    if (!ExceptionCacheDict.TryGetValue(exStr, out count))
                    {
                        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(2, true);
                        WriteException(exStr + stackTrace.ToString());
                    }

                    ExceptionCacheDict[exStr] = ++count;
                }
            }
            catch (Exception ex)
            {
                WriteException(ex.ToString());
            }
        }
    }
}
