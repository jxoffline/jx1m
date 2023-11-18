using System;
using System.IO;

namespace Server.Tools
{
    /// <summary>
    /// Quản lý toàn bộ logs type
    /// </summary>
    public enum LogTypes
    {
        Ignore = -1, // Bỏ qua đéo ghi gì cả
        Info = 0,
        Warning = 1,
        Error = 2,
        SQL = 3,
        Exception = 4,
        Trace = 5,
        Family = 6,
        Guild = 7,
        TeamBattle = 8,
        Cache = 9,
        Pet = 10,
        TotalUserMoney = 100,
    }

    /// <summary>
    /// Class xử lý logs
    /// </summary>
    public class LogManager
    {
        public LogManager()
        {
        }


        public static LogTypes LogTypeToWrite
        {
            get;
            set;
        }

        public static bool EnableDbgView = false;


        private static string _LogPath = string.Empty;


        public static string LogPath
        {
            get
            {
                lock (mutex)
                {
                    if (_LogPath == string.Empty)
                    {
                        _LogPath = AppDomain.CurrentDomain.BaseDirectory + @"log/";
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


        private static string _ExceptionPath = string.Empty;

        public static string ExceptionPath
        {
            get
            {
                lock (mutex)
                {
                    if (_ExceptionPath == string.Empty)
                    {
                        _ExceptionPath = AppDomain.CurrentDomain.BaseDirectory + @"Exception/";
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
        /// Xử lý ghi logs ra files
        /// </summary>
        private static void WriteLog(string logFile, string logMsg)
        {
            try
            {
                StreamWriter sw = File.AppendText(
                    LogPath + logFile + "_" + DateTime.Now.ToString("yyyyMMdd") + ".log");

                string text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss\t") + logMsg;
                if (EnableDbgView)
                {
                    System.Diagnostics.Debug.WriteLine(text);
                }

                sw.WriteLine(text);
                sw.Close();
                sw = null;
            }
            catch
            {
            }
        }


        private static object mutex = new object();


        public static void WriteLog(LogTypes logType, string logMsg)
        {
            if ((int)logType < (int)LogTypeToWrite)
            {
                return;
            }

            // Lock lại để đản bảo 1 lúc chỉ có 1 session ghi vào files
            lock (mutex)
            {
                WriteLog(logType.ToString(), logMsg);
            }
        }


        public static void WriteException(string exceptionMsg)
        {
            WriteLog(LogTypes.Exception, "##exception##\r\n" + exceptionMsg);
        }
    }
}