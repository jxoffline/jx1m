using MoonSharp.Interpreter;
using Server.Tools;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Xml.Linq;

namespace GameServer.KiemThe.Entities
{
    /// <summary>
    /// Đối tượng quản lý Script Lua của hệ thống
    /// </summary>
    public class KTLuaScript
    {
        #region Singleton - Instance

        /// <summary>
        /// Đối tượng quản lý Script Lua của hệ thống
        /// </summary>
        public static KTLuaScript Instance { get; private set; }

        private KTLuaScript()
        {
        }

        #endregion Singleton - Instance

        #region Constants
        /// <summary>
        /// Sử dụng đa luồng
        /// </summary>
        private const bool UseMultipleThreads = false;

        /// <summary>
        /// Số luồng tối đa thực thi mỗi lúc
        /// </summary>
        private const int MaxThreads = 5;
        #endregion

        /// <summary>
        /// Đối tượng Lua Script
        /// </summary>
        public class LuaScript
        {
            /// <summary>
            /// ID Script
            /// </summary>
            public int ID { get; set; }

            /// <summary>
            /// Đường dẫn File script (tính từ folder LuaScripts)
            /// </summary>
            public string FileDir { get; set; }

            /// <summary>
            /// Script thực thi
            /// </summary>
            public Table Script { get; set; }

            /// <summary>
            /// Đã tải xuống chưa
            /// </summary>
            public bool Loaded { get; set; }

            /// <summary>
            /// Chuyển đối tượng từ XMLNode
            /// </summary>
            /// <param name="xmlNode"></param>
            /// <returns></returns>
            public static LuaScript Parse(XElement xmlNode)
            {
                return new LuaScript()
                {
                    ID = int.Parse(xmlNode.Attribute("ID").Value),
                    FileDir = xmlNode.Attribute("Dir").Value,
                    Loaded = false,
                    Script = null,
                };
            }
        }

        /// <summary>
        /// Hàng đợi
        /// </summary>
        private class QueueItem
		{
            /// <summary>
            /// Script tương ứng
            /// </summary>
            public string Source { get; set; }
            
            /// <summary>
            /// Hàm thực thi công việc
            /// </summary>
            public Action Work { get; set; }
		}

        /// <summary>
        /// Danh sách Script trong hệ thống
        /// </summary>
        private readonly Dictionary<int, LuaScript> luaScripts = new Dictionary<int, LuaScript>();

        #region Background Worker
        /// <summary>
        /// Danh sách sự kiện đang chờ thực thi
        /// </summary>
        private readonly ConcurrentQueue<QueueItem> listWaitingRequest = new ConcurrentQueue<QueueItem>();

        /// <summary>
        /// Worker
        /// </summary>
        private BackgroundWorker worker;

        /// <summary>
        /// Sử dụng Semaphore
        /// </summary>
        private Semaphore limitation;

        /// <summary>
        /// Chạy luồng thực thi
        /// </summary>
        private void StartTimer()
        {
            /// Nếu sử dụng đa luồng
			if (KTLuaScript.UseMultipleThreads)
            {
                /// Khởi tạo Semaphore
                this.limitation = new Semaphore(KTLuaScript.MaxThreads, KTLuaScript.MaxThreads);
            }

            /// Khởi tạo worker
            this.worker = new BackgroundWorker();
            this.worker.DoWork += this.Worker_DoWork;
            this.worker.RunWorkerCompleted += this.Worker_Completed;

            /// Thời gian báo cáo trạng thái lần cuối
            long lastReportStateTick = KTGlobal.GetCurrentTimeMilis();

            /// Thời gian thực thi công việc lần cuối
            long lastWorkerBusyTick = 0;

            /// Tạo Timer riêng
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.AutoReset = true;
            timer.Interval = 100;
            timer.Elapsed += (o, e) => {
                /// Nếu có lệnh ngắt Server
                if (Program.NeedExitServer)
                {
                    LogManager.WriteLog(LogTypes.TimerReport, string.Format("Terminate => {0}", "KTLuaScript"));
                    /// Ngừng luồng
                    timer.Stop();
                    return;
                }

                if (KTGlobal.GetCurrentTimeMilis() - lastReportStateTick >= 5000)
                {
                    LogManager.WriteLog(LogTypes.TimerReport, string.Format("Tick alive => {0}", "KTLuaScript"));
                    lastReportStateTick = KTGlobal.GetCurrentTimeMilis();
                }

                if (this.worker.IsBusy)
                {
                    /// Quá thời gian thì Cancel
                    if (KTGlobal.GetCurrentTimeMilis() - lastWorkerBusyTick >= 2000)
                    {
                        /// Cập nhật thời gian thực thi công việc lần cuối
                        lastWorkerBusyTick = KTGlobal.GetCurrentTimeMilis();
                        LogManager.WriteLog(LogTypes.TimerReport, string.Format("Timeout => {0}", "KTLuaScript"));
                    }
                }
                else
                {
                    /// Cập nhật thời gian thực thi công việc lần cuối
                    lastWorkerBusyTick = KTGlobal.GetCurrentTimeMilis();
                    /// Thực thi công việc
                    this.worker.RunWorkerAsync();
                }
            };
            timer.Start();
        }


        /// <summary>
        /// Sự kiện khi Background Worker hoàn tất công việc
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Worker_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                LogManager.WriteLog(LogTypes.Exception, string.Format("KTLuaScript.this.Worker_DoWork", "KTLuaScript"));
            }
        }

        /// <summary>
        /// Thực thi công việc của Background Worker
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            long nowTick = KTGlobal.GetCurrentTimeMilis();

            try
            {
                /// Duyệt toàn bộ các yêu cầu đang chơ trong Queue và xử lý
                while (!this.listWaitingRequest.IsEmpty)
                {
                    if (this.listWaitingRequest.TryDequeue(out QueueItem action))
                    {
                        /// Thực thi sự kiện
                        this.ExecuteAction(action, null);
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, string.Format("Exception at {0}\n{1}", "KTLuaScript", ex.ToString()));
            }
        }

        /// <summary>
        /// Thêm công việc cần thực thi
        /// </summary>
        /// <param name="source"></param>
        /// <param name="work"></param>
        public void AddWork(string source, Action work)
        {
            if (work == null)
            {
                return;
            }

            /// Thêm vào danh sách chờ
            this.listWaitingRequest.Enqueue(new QueueItem()
            {
                Source = source,
                Work = work,
            });
        }

        /// <summary>
        /// Thực thi sự kiện gì đó
        /// </summary>
        /// <param name="work"></param>
        /// <param name="onError"></param>
        private void ExecuteAction(QueueItem work, Action<Exception> onError)
        {
            /// Nếu không sử dụng đa luồng
            if (!KTLuaScript.UseMultipleThreads)
            {
                try
                {
                    /// Thời điểm trước thực thi
                    long ticks = KTGlobal.GetCurrentTimeMilis();
                    /// Thực thi công việc
                    work.Work?.Invoke();
                    /// Nếu xử lý quá lâu
                    if (KTGlobal.GetCurrentTimeMilis() - ticks >= 100)
                    {
                        LogManager.WriteLog(LogTypes.Lua, string.Format("Script {0} took {1}ms to be processed", work.Source, (KTGlobal.GetCurrentTimeMilis() - ticks)));
                    }
                }
                catch (Exception ex)
                {
                    LogManager.WriteLog(LogTypes.Lua, ex.ToString());
                    onError?.Invoke(ex);
                }
            }
            /// Nếu sử dụng đa luồng
            else
            {
                /// Tạo luồng mới
                Thread thread = new Thread(() => {
                    try
                    {
                        /// Đợi đến khi có luồng Free
                        this.limitation.WaitOne();
                        /// Thời điểm trước thực thi
                        long ticks = KTGlobal.GetCurrentTimeMilis();
                        /// Thực thi công việc
                        work.Work?.Invoke();
                        /// Nếu xử lý quá lâu
                        if (KTGlobal.GetCurrentTimeMilis() - ticks >= 100)
						{
                            LogManager.WriteLog(LogTypes.Lua, string.Format("Script ID = {0} took {1}ms to be processed", work.Source, (KTGlobal.GetCurrentTimeMilis() - ticks)));
						}
                    }
                    catch (Exception ex)
                    {
                        LogManager.WriteLog(LogTypes.Lua, ex.ToString());
                        onError?.Invoke(ex);
                    }
                    finally
                    {
                        this.limitation.Release();
                    }
                });
                /// Hủy chế độ chạy ngầm
                thread.IsBackground = false;
                /// Bắt đầu luồng
                thread.Start();
            }
        }

        #endregion Background Worker

        /// <summary>
        /// Đọc dữ liệu danh sách LuaScript của hệ thống
        /// </summary>
        public static void Init()
        {
            KTLuaScript.Instance = new KTLuaScript();

            XElement xmlNode = XElement.Parse(File.ReadAllText("LuaScripts/ScriptIndex.xml"));
            foreach (XElement node in xmlNode.Elements("Script"))
            {
                LuaScript script = LuaScript.Parse(node);
                KTLuaScript.Instance.luaScripts[script.ID] = script;
            }

            /// Chạy Timer
            KTLuaScript.Instance.StartTimer();
        }

        /// <summary>
        /// Trả ra đối tượng Script theo ID, Null nếu không tìm thấy
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public LuaScript GetScriptByID(int id)
        {
            if (this.luaScripts.TryGetValue(id, out LuaScript script))
            {
                return script;
            }
            return null;
        }

        /// <summary>
        /// Tải mới Script từ File
        /// </summary>
        /// <param name="script"></param>
        private void LoadScript(Script luaEnv, LuaScript script)
        {
            if (!File.Exists(string.Format("LuaScripts/{0}", script.FileDir)))
            {
                string err = string.Format("[ERROR] Lua Script not found, ID = '{0}'", script.ID);
                LogManager.WriteLog(LogTypes.Lua, err);
                return;
            }
            else
            {
                try
                {
                    /// Thực hiện tạo đối tượng từ Base Class nếu là Script động
                    if (script.ID != -1)
                    {
                        string code = string.Format("Scripts[{0}] = BaseClass:New(); return Scripts[{0}]", script.ID);
                        script.Script = luaEnv.DoString(code).Table;
                    }

                    string fileContent = File.ReadAllText(string.Format("LuaScripts/{0}", script.FileDir));
                    luaEnv.DoString(fileContent);
                    script.Loaded = true;
                }
                catch (InterpreterException ex)
                {
                    string err = string.Format("[ERROR] Load Lua Script error, Dir = '{0}'\nLua Message: {1}\nException: {2}", script.FileDir, ex.DecoratedMessage, ex.ToString());
                    LogManager.WriteLog(LogTypes.Lua, err);
                }
            }
        }

        /// <summary>
        /// Thực thi Script có Function tương ứng
        /// </summary>
        /// <param name="luaEnv"></param>
        /// <param name="script"></param>
        /// <param name="functionName"></param>
        /// <param name="param"></param>
        private DynValue DoExecuteFunction(Script luaEnv, LuaScript script, string functionName, params object[] param)
        {
            string fullFunctionName = string.Format("Scripts[{0}]:{1}", script.ID, functionName);
            try
            {
                Table scriptTable = script.Script;
                if (scriptTable == null)
                {
                    throw new Exception();
                }
                Closure function = scriptTable[functionName] as Closure;
                if (function != null)
                {
                    object[] parameters = new object[param.Length + 2];
                    parameters[0] = (luaEnv.Globals["Scripts"] as Table)[script.ID];
                    for (int i = 0; i < param.Length; i++)
                    {
                        parameters[i + 1] = param[i];
                    }
                    return function?.Call(parameters);
                }
                else
                {
                    string err = string.Format("[ERROR] Execute Lua Script function got error, Dir = '{0}', Function = '{1}'\nException: {2}", script.FileDir, fullFunctionName, "Null Reference");
                    LogManager.WriteLog(LogTypes.Lua, err);
                    return null;
                }
            }
            catch (InterpreterException ex)
            {
                string err = string.Format("[ERROR] Execute Lua Script function got error, Dir = '{0}', Function = '{1}'\nLua Message: {2}\nException: {3}", script.FileDir, fullFunctionName, ex.DecoratedMessage, ex.ToString());
                LogManager.WriteLog(LogTypes.Lua, err);
                return null;
            }
            catch (ArgumentException ex)
            {
                string err = string.Format("[ERROR] Execute Lua Script function got error, Dir = '{0}', Function = '{1}'\nError Param: {2}", script.FileDir, fullFunctionName, ex.ToString());
                LogManager.WriteLog(LogTypes.Lua, err);
                return null;
            }
            catch (Exception ex)
            {
                string err = string.Format("[ERROR] Execute Lua Script function got error, Dir = '{0}', Function = '{1}'\nException: {2}", script.FileDir, fullFunctionName, ex.ToString());
                LogManager.WriteLog(LogTypes.Lua, err);
                return null;
            }
        }

        /// <summary>
        /// Thực thi hàm tương ứng
        /// </summary>
        /// <param name="sourceName"></param>
        /// <param name="func"></param>
        /// <param name="parameters"></param>
        /// <param name="callBack"></param>
        public void ExecuteFunctionAsync(string sourceName, Closure func, object[] parameters, Action<DynValue> callBack)
        {
            this.AddWork(sourceName, () =>
            {
                try
                {
                    DynValue dynV = null;
                    if (parameters == null)
                    {
                        dynV = func?.Call();
                    }
                    else
                    {
                        dynV = func?.Call(parameters);
                    }
                    callBack?.Invoke(dynV);
                }
                catch (InterpreterException ex)
                {
                    string err = string.Format("[ERROR] Execute internal Lua Script function got error, Dir = '{0}'\nLua Message: {1}\nException: {2}", sourceName, ex.DecoratedMessage, ex.ToString());
                    LogManager.WriteLog(LogTypes.Lua, err);
                }
                catch (ArgumentException ex)
                {
                    string err = string.Format("[ERROR] Execute internal Lua Script function got error, Dir = '{0}'\nError Param: {1}", sourceName, ex.ToString());
                    LogManager.WriteLog(LogTypes.Lua, err);
                }
                catch (Exception ex)
                {
                    string err = string.Format("[ERROR] Execute internal Lua Script function got error, Dir = '{0}'\nException: {1}", sourceName, ex.ToString());
                    LogManager.WriteLog(LogTypes.Lua, err);
                }
            });
        }

        /// <summary>
        /// Phương thức Async thực thi Script có Function tương ứng
        /// </summary>
        /// <param name="luaEnv"></param>
        /// <param name="scriptID"></param>
        /// <param name="functionName"></param>
        /// <param name="param"></param>
        /// <param name="callback"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public void ExecuteFunctionAsync(Script luaEnv, int scriptID, string functionName, object[] param, Action<DynValue> callback, Func<bool> predicate = null)
        {
            if (!this.luaScripts.ContainsKey(scriptID))
            {
                string err = string.Format("[ERROR] Lua Script not found, ID = '{0}'", scriptID);
                LogManager.WriteLog(LogTypes.Lua, err);
                return;
            }
            LuaScript script = this.luaScripts[scriptID];

            this.AddWork(string.Format("ID = {0}, Function = {1}", scriptID, functionName), () =>
            {
                if (!script.Loaded)
                {
                    this.LoadScript(luaEnv, script);
                }

                /// Nếu không thỏa mãn điều kiện
                if (predicate != null && !predicate.Invoke())
				{
                    return;
				}

                DynValue dynValue = this.DoExecuteFunction(luaEnv, script, functionName, param);
                if (dynValue != null)
                {
                    callback?.Invoke(dynValue);
                }
            });
        }

        /// <summary>
        /// Phương thức Async tải và thực thi Script với đường dẫn tương ứng
        /// </summary>
        /// <param name="luaEnv"></param>
        /// <param name="scriptName"></param>
        public void LoadScriptAsync(Script luaEnv, string scriptName, Action callback = null)
        {
            this.AddWork("LoadScript: " + scriptName, () =>
            {
                this.LoadScript(luaEnv, new LuaScript()
                {
                    ID = -1,
                    FileDir = scriptName,
                });

                callback?.Invoke();
            });
        }

        /// <summary>
        /// Tải lại Script ID tương ứng
        /// </summary>
        /// <param name="luaEnv"></param>
        /// <param name="scriptID"></param>
        public void ReloadScript(Script luaEnv, int scriptID)
        {
            if (!this.luaScripts.ContainsKey(scriptID))
            {
                string err = string.Format("[ERROR] Lua Script not found, ID = '{0}'", scriptID);
                LogManager.WriteLog(LogTypes.Lua, err);
                //Console.WriteLine(err);
                return;
            }
            LuaScript script = this.luaScripts[scriptID];

            this.AddWork("ReloadScript: " + scriptID, () =>
            {
                this.LoadScript(luaEnv, script);
            });
        }
    }
}