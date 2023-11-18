using GameServer.Core.Executor;
using GameServer.KiemThe;
using GameServer.KiemThe.Logic;
using GameServer.Logic;
using Server.Tools;
using System;
using System.Net;
using System.Threading;
using System.Xml.Serialization;

namespace GameServer.VLTK.Core.Activity.X2ExpEvent
{
    public class ExpMutipleEvent
    {
        private static ExpMutipleModel _ExpMutiple = new ExpMutipleModel();

        public static string EXP_CONFIG_FILE = "Config/KT_Activity/ExpMutipleModel.xml";

        public static EXPSTATE eXPSTATE = EXPSTATE.NOT_OPEN;

        public static bool IsToday = false;

        public static long LastTick { get; set; }


        public static long LastUpdateStatus { get; set; } = 0;

        public static bool IsOpenEvent = false;

        /// <summary>
        /// Setup Exp Time
        /// </summary>
        public static void Setup()
        {
            ///Sự kiện X2 Exp toàn server
            string Files = KTGlobal.GetDataPath(EXP_CONFIG_FILE);

            using (var stream = System.IO.File.OpenRead(Files))
            {
                var serializer = new XmlSerializer(typeof(ExpMutipleModel));
                _ExpMutiple = serializer.Deserialize(stream) as ExpMutipleModel;
            }

            int TODAY = TimeUtil.GetWeekDay1To7(DateTime.Now);

            if (_ExpMutiple.DayOfWeek.Contains(TODAY))
            {
                if (_ExpMutiple.DayOfWeek.Contains(TODAY))
                {
                    IsToday = true;
                    // SET TRẠNG THÁI VỀ NOT OPEN
                    eXPSTATE = EXPSTATE.NOT_OPEN;
                }
            }

            ScheduleExecutor2.Instance.scheduleExecute(new NormalScheduleTask("ExpMutipleEvent", ProsecEvent), 5 * 1000, 2000);
        }

        /// <summary>
        /// Xử lý sự kiện 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void ProsecEvent(object sender, EventArgs e)
        {
            DateTime TimeOpen = DateTime.Now;

            if (IsToday)
            {
                if (eXPSTATE == EXPSTATE.NOT_OPEN)
                {
                    if (TimeOpen.Hour == _ExpMutiple.StartTime.HOUR && eXPSTATE == EXPSTATE.NOT_OPEN && TimeOpen.Minute >= _ExpMutiple.StartTime.MINUTE && TimeOpen.Second >= _ExpMutiple.StartTime.SECOND)
                    {
                        eXPSTATE = EXPSTATE.OPEN;

                        IsOpenEvent = true;

                        LastTick = TimeUtil.NOW();

                        KTGlobal.SendSystemEventNotification("Sự kiện X2 Kinh Nghiệm toàn bộ máy chủ đã bắt đầu.Sự kiện sẽ diễn ra trong vòng " + (_ExpMutiple.StartTime.DUALTION / 60) + " phút!");
                    }
                }
                else if (eXPSTATE == EXPSTATE.OPEN && TimeUtil.NOW() >= LastTick + _ExpMutiple.StartTime.DUALTION * 1000)
                {
                    LastTick = TimeUtil.NOW();

                    eXPSTATE = EXPSTATE.CLOSE;

                    IsOpenEvent = false;

                    KTGlobal.SendSystemEventNotification("Sự kiện X2 Kinh Nghiệm toàn bộ máy chủ đã kết thúc!");
                }
            }
            //10s thực hiện update trạng thái của gs lên SDK
            if (TimeUtil.NOW() - LastUpdateStatus >= 60 * 1000)
            {
                LastUpdateStatus = TimeUtil.NOW();
                DoUpdateServerStatus();
            }
        }

        public static int GetServerStatus()
        {
            // Đang bảo trì
            if (Program.NeedExitServer)
            {
                return 1;
            }

            int COUNT = KTPlayerManager.GetPlayersCount();

            // Nếu số CCU còn nhỏ hơn 500 thì còn vào tốt
            if (COUNT >= 0 && COUNT < 500)
            {
                return 4;
            }
            // Gần đầy
            if (COUNT >= 500 && COUNT < 800)
            {
                return 3;
            }
            //Đầy
            if (COUNT >= 800)
            {
                return 2;
            }

            return 4;
        }

        public static string GetStatus()
        {
            if (IsOpen())
            {
                return "<color=#7bff4f>Kinh nghiệm:X" + _ExpMutiple.ExpRate + "</color>|<color=#d53e07>Tiền:X" + _ExpMutiple.MoneyRate + "";
            }
            return "";
        }


        public static void DoUpdateServerStatus()
        {
            Thread _Thread = new Thread(() => UpdateServerStatus());
            _Thread.Start();
        }
        public static void UpdateServerStatus()
        {
            UpdateServerModel _UpdateModel = new UpdateServerModel();
            // Server này là server nào
            _UpdateModel.SeverID = GameManager.ServerLineID;
            _UpdateModel.Status = GetServerStatus();
            _UpdateModel.NotifyUpdate = GetStatus();

            byte[] SendDATA = DataHelper.ObjectToBytes<UpdateServerModel>(_UpdateModel);

            WebClient wwc = new WebClient();

            try
            {
                byte[] VL = wwc.UploadData("http://207.148.73.199:88/UpdateServerStatus.aspx", SendDATA);
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Error, "Update server Status Bug :" + ex.ToString());
            }
        }

        public static bool IsOpen()
        {
            return IsOpenEvent;
        }

        public static double GetRate()
        {
            return _ExpMutiple.ExpRate;
        }

        public static double GetMoneyRate()
        {
            if (ExpMutipleEvent.IsOpen())
            {
                return _ExpMutiple.MoneyRate;

            }
            else
            {
                return 1.0;
            }    
               
        }
    }
}