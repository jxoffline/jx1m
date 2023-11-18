using GameServer.Core.Executor;
using GameServer.KiemThe.Logic;
using GameServer.Server;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tmsk.Tools;

namespace GameServer.Logic
{
    /// <summary>
    /// Cpu And Memory Analysis
    /// </summary>
    public class ServerMonitorManager
    {
        private TimeSpan _PrevCpuTime = TimeSpan.Zero;
        private long _LastCalcCpuMs;
        private long _LastReportMs;
        private bool _BIsReporting = false;

        /// <summary>
        /// http://127.0.0.1:5757/server_analysis,5
        /// 上报的url，上报的间隔(单位：秒)
        /// </summary>
        public const string ReportUrlCfgKey = "server_monitor_report";

        private bool _BNeedReLoad = true;
        private string _ReportToUrl = string.Empty;
        private int _ReportIntervalSec = 5;

        public ServerMonitorManager()
        {
            _PrevCpuTime = Process.GetCurrentProcess().TotalProcessorTime;
            _LastCalcCpuMs = TimeUtil.NOW();
            _LastReportMs = TimeUtil.NOW();
        }

        /// <summary>
        /// 获取cpu和内存占用率
        /// </summary>
        /// <param name="cpuLoad">[0.0 --- 100.0] cpu占用率</param>
        /// <param name="memMb">内存占用，单位MB</param>
        /// <returns></returns>
        private bool GetCpuAndMem(out double cpuLoad, out double memMb)
        {
            cpuLoad = 0.0;
            memMb = 0.0;

            try
            {
                // cpu
                long nowMs = TimeUtil.NOW();
                double intervalMs = nowMs - _LastCalcCpuMs;
                TimeSpan curCpuTime = Process.GetCurrentProcess().TotalProcessorTime;
                if (intervalMs > 0.0)
                {
                    cpuLoad = (curCpuTime - _PrevCpuTime).TotalMilliseconds * 1.0 / intervalMs / Environment.ProcessorCount;
                    cpuLoad = Math.Min(cpuLoad, 1.0);
                   // cpuLoad *= 100;
                }

                // memory
                memMb = Process.GetCurrentProcess().WorkingSet64 / (1024.0 * 1024.0);

                _LastCalcCpuMs = nowMs;
                _PrevCpuTime = curCpuTime;
            }
            catch (Exception)
            {
                cpuLoad = 0.0;
                memMb = 0.0;
                return false;
            }

            return true;
        }

        /// <summary>
        /// 更新上传的url和间隔配置
        /// </summary>
        private void RefreshReportConfig()
        {
            if (_BNeedReLoad)
            {
                _BNeedReLoad = false;
                string report = GameManager.GameConfigMgr.GetGameConfigItemStr(ReportUrlCfgKey, "");
                string[] fields = report.Split(',');

                string tmpReportUrl = string.Empty;
                int tmpReportInterval = 3;
      
                if (fields.Length >= 1)
                {
                    tmpReportUrl = fields[0];
                }
                //tmpReportUrl = "http://192.168.2.31/server_monitor";
                if (fields.Length >= 2)
                {
                    if (!int.TryParse(fields[1], out tmpReportInterval))
                    {
                        tmpReportInterval = 5;
                    }
                }

                // 不能低于3秒的汇报间隔
                tmpReportInterval = Math.Max(3, tmpReportInterval);

                _ReportToUrl = tmpReportUrl;
                _ReportIntervalSec = tmpReportInterval;
            }       
        }

        public void SetNeedReload()
        {
            _BNeedReLoad = true;
        }

        /// <summary>
        /// 检测上报
        /// </summary>
        public void CheckReport()
        {
            try
            {
                //RefreshReportConfig();

                //if (string.IsNullOrEmpty(_ReportToUrl))
                //   return;

                long nowMs = TimeUtil.NOW();
                if (nowMs - _LastReportMs >= _ReportIntervalSec * 1000)
                {
                  

                    _LastReportMs = nowMs;

                    StringBuilder sb = new StringBuilder();
                    sb.Append(_ReportToUrl).Append("?");
                    sb.AppendFormat("serverid={0}&", GameCoreInterface.getinstance().GetLocalServerId());
                    sb.AppendFormat("platform={0}&", GameCoreInterface.getinstance().GetPlatformType().ToString());
                    double cpuLoad, memMb;
                    GetCpuAndMem(out cpuLoad, out memMb);
                    sb.AppendFormat("cpu={0}&", cpuLoad);
                    sb.AppendFormat("mem={0}&", memMb);
                    sb.AppendFormat("roleCount={0}&", KTPlayerManager.GetPlayersCount());
                    sb.AppendFormat("procCmdCount={0}&", TCPCmdHandler.TotalHandledCmdsNum);
                    sb.AppendFormat("cmdAvgProcMs={0}&", ProcessSessionTask.processCmdNum != 0 ? TimeUtil.TimeMS(ProcessSessionTask.processTotalTime / ProcessSessionTask.processCmdNum) : 0);
                    sb.AppendFormat("cmdMaxProcMs={0}&", TCPCmdHandler.MaxUsedTicksByCmdID);
                    sb.AppendFormat("dbConnCount={0}&", Global._TCPManager.tcpClientPool.GetPoolCount());
                    sb.AppendFormat("lastFlushMonsterToNow={0}", GameManager.LastFlushMonsterMs * 10000);


                    LogManager.WriteLog(LogTypes.Analysis, sb.ToString());

                  
                }
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Error, "ServerAnalysisManager.CheckReport() failed!", ex);
            }
        }
    }
}
