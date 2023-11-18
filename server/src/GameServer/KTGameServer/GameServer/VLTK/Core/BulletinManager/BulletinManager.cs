using GameServer.Core.Executor;
using GameServer.Logic;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace GameServer.KiemThe.Core.BulletinManager
{
    /// <summary>
    /// Lớp quản lý thông báo của toàn máy chủ
    /// </summary>
    public class BulletinManager
    {
        public static int DayID = 0;

        public static string Bulletin_Path = "Config/KT_Chat/BulletinMsg.xml";

        public static List<BulletinMsg> _Total = new List<BulletinMsg>();

        public static void Setup()
        {
            DayID = DateTime.Now.DayOfYear;

            _Total.Clear();
            string Files = KTGlobal.GetDataPath(Bulletin_Path);
            using (var stream = System.IO.File.OpenRead(Files))
            {
                var serializer = new XmlSerializer(typeof(List<BulletinMsg>));
                _Total = serializer.Deserialize(stream) as List<BulletinMsg>;
            }

            ScheduleExecutor2.Instance.scheduleExecute(new NormalScheduleTask("NOTIFYCHECK", Proseccsing), 5 * 1000, 2000);
        }

        public static void Reset()
        {
            _Total.Clear();
            string Files = KTGlobal.GetDataPath(Bulletin_Path);
            using (var stream = System.IO.File.OpenRead(Files))
            {
                var serializer = new XmlSerializer(typeof(List<BulletinMsg>));
                _Total = serializer.Deserialize(stream) as List<BulletinMsg>;
            }
        }

        public static void Proseccsing(object sender, EventArgs e)
        {
            try
            {
                // Thực hiện reset lại thông báo khi qua ngày
                if (DayID != DateTime.Now.DayOfYear)
                {
                    DayID = DateTime.Now.DayOfYear;
                    Reset();
                }

                for (int i = 0; i < _Total.Count; i++)
                {
                    BulletinMsg _Msg = _Total[i];

                    List<int> DayOfWeek = _Msg.DayOfWeek;

                    int Today = TimeUtil.GetWeekDay1To7(DateTime.Now);

                    if (DayOfWeek.Contains(Today))
                    {
                        DateTime Now = DateTime.Now;

                        int NowHours = Now.Hour;

                        int NowMins = Now.Minute;

                        foreach (NotifyTime Time in _Msg.NotifyTimes)
                        {
                            if (Time.Hours == NowHours && Time.Minute == NowMins && Time.IsPushNotify == false)
                            {
                                Time.IsPushNotify = true;
                                KTGlobal.SendSystemEventNotification(_Msg.Messenger);
                            }
                        }
                    }
                }
            }
            catch
            {

            }
        }
    }
}