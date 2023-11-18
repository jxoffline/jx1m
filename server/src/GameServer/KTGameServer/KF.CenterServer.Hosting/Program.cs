using KF.Remoting;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;
using System.Text;
using System.Threading;

namespace KF.Hosting.HuanYingSiYuan
{
    internal class Program
    {
        [DllImport("User32.dll", EntryPoint = "FindWindow")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", EntryPoint = "FindWindowEx")]   //找子窗体
        private static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        [DllImport("User32.dll", EntryPoint = "SendMessage")]   //用于发送信息给窗体
        private static extern int SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, string lParam);

        [DllImport("User32.dll", EntryPoint = "ShowWindow")]   //
        private static extern bool ShowWindow(IntPtr hWnd, int type);

        public static void SetWindowMin()
        {
            Console.Title = "KF.Server.Hosting";
            IntPtr ParenthWnd = new IntPtr(0);
            IntPtr et = new IntPtr(0);
            ParenthWnd = FindWindow(null, "KF.Server.Hosting");

            ShowWindow(ParenthWnd, 2);//隐藏本dos窗体, 0: 后台执行；1:正常启动；2:最小化到任务栏；3:最大化
        }

        #region 控制台关闭控制 windows

        public delegate bool ControlCtrlDelegate(int CtrlType);

        [DllImport("kernel32.dll")]
        private static extern bool SetConsoleCtrlHandler(ControlCtrlDelegate HandlerRoutine, bool Add);

        private static ControlCtrlDelegate newDelegate = new ControlCtrlDelegate(HandlerRoutine);

        public static bool HandlerRoutine(int CtrlType)
        {
            switch (CtrlType)
            {
                case 0:
                    //Console.WriteLine("工具被强制关闭(ctrl + c)"); //Ctrl+C关闭
                    break;

                case 2:
                    //Console.WriteLine("工具被强制关闭(界面关闭按钮)");//按控制台关闭按钮关闭
                    break;
            }

            //关闭事件被捕获之后，不需要进行关闭处理
            return true;
        }

        //[DllImport("user32.dll", EntryPoint = "FindWindow")]
        //extern static IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll", EntryPoint = "GetSystemMenu")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, IntPtr bRevert);

        [DllImport("user32.dll", EntryPoint = "RemoveMenu")]
        private static extern IntPtr RemoveMenu(IntPtr hMenu, uint uPosition, uint uFlags);

        /// <summary>
        /// 禁用关闭按钮
        /// </summary>
        private static void HideCloseBtn()
        {
            IntPtr windowHandle = FindWindow(null, Console.Title);
            IntPtr closeMenu = GetSystemMenu(windowHandle, IntPtr.Zero);
            uint SC_CLOSE = 0xF060;
            RemoveMenu(closeMenu, SC_CLOSE, 0x0);
        }

        #endregion 控制台关闭控制 windows

        private static bool NeedExitServer = false;

        private static CmdHandlerDict CmdDict = new CmdHandlerDict();

        private static void Main(string[] args)
        {
            try
            {
                FileStream fs = File.Open("Pid.txt", FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
                if (fs != null)
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(Process.GetCurrentProcess().Id.ToString());
                    fs.Write(bytes, 0, bytes.Length);
                    fs.Flush();
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("This program has started a process, press any key to exit!");
                Console.ReadKey();
                return;
            }

            #region 控制台关闭控制

            HideCloseBtn();

            SetConsoleCtrlHandler(newDelegate, true);

            if (Console.WindowWidth < 88)
            {
                Console.BufferWidth = 88;
                Console.WindowWidth = 88;
            }

            #endregion 控制台关闭控制

            Console.WriteLine("Cross-service center server start!");

            LogManager.WriteLog(LogTypes.Info, "Cross-service center server start!");

            SetWindowMin();

            if (!KuaFuServerManager.CheckConfig())
            {
                Console.WriteLine("Server can't start!");
            }

            if (!KuaFuServerManager.LoadConfig())
            {
                Console.ReadLine();
                return;
            }

            KuaFuServerManager.StartServerConfigThread();

            RemotingConfiguration.Configure(Process.GetCurrentProcess().MainModule.FileName + ".config", false);
            InitCmdDict();

            do
            {
                try
                {
                    ShowCmdHelp();
                    string cmd = Console.ReadLine();
                    if (null != cmd)
                    {
                        CmdDict.ExcuteCmd(cmd);
                    }

                    if (NeedExitServer)
                    {
                        KuaFuServerManager.OnStopServer();

                        Console.WriteLine("Press any key to Stop!");
                        Console.ReadKey();
                        break;
                    }
                }
                catch (System.Exception ex)
                {
                    LogManager.WriteException(ex.ToString());
                }
            } while (true);
        }

        /// <summary>
        /// 初始化命令行命令处理器
        /// </summary>
        private static void InitCmdDict()
        {
            CmdDict.AddCmdHandler("exit", ExitCmdHandler);
            CmdDict.AddCmdHandler("reload", ReloadCmdHandler);
            CmdDict.AddCmdHandler("clear", ClearCmdHandler);
            CmdDict.AddCmdHandler("load", ReloadCmdHandler);
        }

        private static void ShowCmdHelp()
        {
            Console.WriteLine("COMMAND SUPPPORT:");
            Console.WriteLine("exit : EXIT CENTER SV");
            Console.WriteLine("reload : RELOAD SERVER");
            Console.WriteLine("clear : CLEAR");
        }

        public static void ExitCmdHandler(object obj)
        {
            Console.WriteLine("Press Y To  Exit");
            if (Console.ReadLine() == "y")
            {
                NeedExitServer = true;
                Console.WriteLine("Exit SV!");
            }
        }

        public static void ReloadCmdHandler(object obj)
        {
            try
            {
                string[] args = obj as string[];
                if (args.Length == 1 && args[0] == "reload")
                {
                    KuaFuServerManager.LoadConfig();
                }
                else
                {
                    TianTiService.Instance.ExecCommand(args);
                }
            }
            catch
            {
            }

            Console.WriteLine("Successfully reloaded the configuration!");
        }

        public static void ReloadPaiHangCmdHandler(object obj)
        {
            try
            {
                TianTiService.Instance.ExecCommand(new string[] { "reload", "paihang" });
            }
            catch
            {
            }

            Console.WriteLine("Successfully reloaded the configuration!");
        }

        public static void ClearCmdHandler(object obj)
        {
            Console.Clear();
        }
    }

    public class CmdHandlerDict
    {
        private Dictionary<String, ParameterizedThreadStart> CmdDict = new Dictionary<string, ParameterizedThreadStart>();

        public void AddCmdHandler(string cmd, ParameterizedThreadStart handler)
        {
            CmdDict.Add(cmd, handler);
        }

        public string[] ParseConsoleCmd(string cmd)
        {
            List<string> argsList = new List<string>();
            string arg = "";
            Stack<char> quoteStack = new Stack<char>();
            foreach (var c in cmd)
            {
                if (char.IsWhiteSpace(c))
                {
                    if (quoteStack.Count == 0)
                    {
                        if (arg != "")
                        {
                            argsList.Add(arg);
                            arg = "";
                        }

                        continue;
                    }
                }
                else if (c == '\"')
                {
                    if (quoteStack.Count > 0 && quoteStack.Peek() == '\"')
                    {
                        quoteStack.Pop();
                    }
                    else
                    {
                        quoteStack.Push(c);
                    }
                }
                else if (c == '\'')
                {
                    if (quoteStack.Count > 0 && quoteStack.Peek() == '\'')
                    {
                        quoteStack.Pop();
                    }
                    else
                    {
                        quoteStack.Push(c);
                    }
                }

                arg += c;
            }

            if (arg != "")
            {
                argsList.Add(arg);
            }

            return argsList.ToArray();
        }

        public void ExcuteCmd(string cmd)
        {
            if (!string.IsNullOrEmpty(cmd))
            {
                string[] args = ParseConsoleCmd(cmd);
                if (args == null || args.Length == 0)
                {
                    return;
                }

                ParameterizedThreadStart proc;
                if (CmdDict.TryGetValue(args[0].ToLower(), out proc))
                {
                    proc(args);
                }
            }
        }
    }
}