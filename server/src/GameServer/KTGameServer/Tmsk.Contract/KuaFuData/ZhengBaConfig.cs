using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Diagnostics;

using Tmsk.Contract;
using Server.Tools;
using Tmsk.Tools.Tools;

namespace KF.Contract.Data
{
    public class ZhengBaMatchConfig
    {
        public int Day;
        public string Name;
        public int MapCode;
        public int MinRank;
        public long DayBeginTick;
        public long DayEndTick;
        public int WaitSeconds;
        public int FightSeconds;
        public int ClearSeconds;
        public int IntervalSeconds;
        public EZhengBaMatching Mathching;
        public int NeedWinTimes;
        public EZhengBaGrade WillUpGrade;
        public int MaxUpGradeNum;
    }

    public class ZhengBaBirthPoint
    {
        public int X;
        public int Y;
        public int Radius;
    }

    public class ZhengBaSupportConfig
    {
        public class TimeConfig
        {
            public int RealDay;
            public long DayBeginTicks;
            public long DayEndTicks;
        }

        public int RankOfDay;   // 活动第几天结束时的排行榜
        public int CostJinBi; // bind fist, then unbind
        public int MaxTimes; // 最大支持次数，赞、贬无限制
        public int WinPoint; 
        public int FailPoint;
        public List<TimeConfig> TimeList;
        public int MinChangeLife;
        public int MinLevel;

        public object WinAwardTag;
        public object FailAwardTag;
    }

    public class ZhengBaConfig
    {
        public readonly List<ZhengBaMatchConfig> MatchConfigList = new List<ZhengBaMatchConfig>();
        public readonly List<ZhengBaSupportConfig> SupportConfigList = new List<ZhengBaSupportConfig>();
        public readonly List<ZhengBaBirthPoint> BirthPointList = new List<ZhengBaBirthPoint>();

        private static int _StartDay = 10;
        public static int StartDay
        {
            get { return _StartDay; }
            private set { _StartDay = value; }
        }
        public ZhengBaConfig()
        {

        }

        public bool Load(string matchFile, string supportFile, string birthFile)
        {
            return LoadMatch(matchFile) && LoadSupport(supportFile) && LoadBirth(birthFile);
        }

        private bool LoadBirth(string birthFile)
        {
            try
            {
                BirthPointList.Clear();

                foreach (var xmlItem in XElement.Load(birthFile).Elements())
                {
                    ZhengBaBirthPoint point = new ZhengBaBirthPoint();
                    point.X = Convert.ToInt32(xmlItem.Attribute("PosX").Value.ToString());
                    point.Y = Convert.ToInt32(xmlItem.Attribute("PosY").Value.ToString());
                    point.Radius = Convert.ToInt32(xmlItem.Attribute("BirthRadius").Value.ToString());

                    BirthPointList.Add(point);
                }

                Debug.Assert(BirthPointList.Count == 2);
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Error, "ZhengBaConfig.LoadMatch failed", ex);
                return false;
            }

            return true;
        }

        private bool LoadMatch(string matchFile)
        {
            try
            {
                MatchConfigList.Clear();

                XElement xml = XElement.Load(matchFile);
                ZhengBaConfig.StartDay = Convert.ToInt32(xml.Attribute("MuLeiTaiDay").Value.ToString());
                foreach (var xmlItem in xml.Elements())
                {
                    ZhengBaMatchConfig matchConfig = new ZhengBaMatchConfig();
                    matchConfig.Day = Convert.ToInt32(xmlItem.Attribute("Day").Value.ToString());
                    Debug.Assert(matchConfig.Day >= 1 && matchConfig.Day <= ZhengBaConsts.ContinueDays);
                    matchConfig.Name = xmlItem.Attribute("Name").Value.ToString();
                    matchConfig.MapCode = Convert.ToInt32(xmlItem.Attribute("MapCode").Value.ToString());
                    matchConfig.MinRank = Convert.ToInt32(xmlItem.Attribute("MinLevelRank").Value.ToString());

                    string szDayBeginTime = xmlItem.Attribute("BeginTime").Value.ToString();
                    string szDayEndTime = xmlItem.Attribute("EndTime").Value.ToString();
                    DateTime dayBegin = DateTime.MinValue, dayEnd = DateTime.MinValue;
                    Debug.Assert(DateTime.TryParse(szDayBeginTime, out dayBegin) && DateTime.TryParse(szDayEndTime, out dayEnd));
                    matchConfig.DayBeginTick = dayBegin.TimeOfDay.Ticks;
                    matchConfig.DayEndTick = dayEnd.TimeOfDay.Ticks;
                    Debug.Assert(matchConfig.DayEndTick > matchConfig.DayBeginTick);

                    matchConfig.WaitSeconds = Convert.ToInt32(xmlItem.Attribute("WaitTime").Value.ToString());
                    matchConfig.WaitSeconds += 3;// 当前的gameserver和kf.hosting的通信机制有明显的延迟，服务器要多等待几秒，不然客户端倒计时最后几秒钟，服务器无法通过
                    matchConfig.FightSeconds = Convert.ToInt32(xmlItem.Attribute("FightTime").Value.ToString());
                    matchConfig.ClearSeconds = Convert.ToInt32(xmlItem.Attribute("ClearTime").Value.ToString());
                    matchConfig.IntervalSeconds = Convert.ToInt32(xmlItem.Attribute("IntervalTime").Value.ToString());
                    matchConfig.Mathching = (EZhengBaMatching)Convert.ToInt32(xmlItem.Attribute("MatchingType").Value.ToString());
                    Debug.Assert(Enum.IsDefined(typeof(EZhengBaMatching), matchConfig.Mathching));
                    Debug.Assert((matchConfig.Mathching == EZhengBaMatching.Random && matchConfig.Day <= 3)
                        || (matchConfig.Mathching == EZhengBaMatching.Group && matchConfig.Day > 3));

                    matchConfig.NeedWinTimes = Convert.ToInt32(xmlItem.Attribute("NeedWinNum").Value.ToString());
                    matchConfig.MaxUpGradeNum = Convert.ToInt32(xmlItem.Attribute("RankNum").Value.ToString());
                    matchConfig.WillUpGrade = ZhengBaUtils.GetDayUpGrade(matchConfig.Day);
                    if ((matchConfig.Day == 3 && matchConfig.MaxUpGradeNum != (int)EZhengBaGrade.Grade16)
                        || (matchConfig.Day == 4 && matchConfig.MaxUpGradeNum != (int)EZhengBaGrade.Grade8)
                        || (matchConfig.Day == 5 && matchConfig.MaxUpGradeNum != (int)EZhengBaGrade.Grade4)
                        || (matchConfig.Day == 6 && matchConfig.MaxUpGradeNum != (int)EZhengBaGrade.Grade2)
                        || (matchConfig.Day == 7 && matchConfig.MaxUpGradeNum != (int)EZhengBaGrade.Grade1))
                    {
                        throw new Exception("第3---7天的晋级人数不可修改");
                    }

                    MatchConfigList.Add(matchConfig);
                }

                for (int i = 1; i <= ZhengBaConsts.ContinueDays; i++)
                {
                    Debug.Assert(MatchConfigList.Exists(_m => _m.Day == i));
                }
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Error, "ZhengBaConfig.LoadMatch failed", ex);
                return false;
            }

            return true;
        }

        private bool LoadSupport(string supportFile)
        {
            try
            {
                SupportConfigList.Clear();

                foreach (var xmlItem in XElement.Load(supportFile).Elements())
                {
                    ZhengBaSupportConfig supportConfig = new ZhengBaSupportConfig();
                    supportConfig.RankOfDay = Convert.ToInt32(xmlItem.Attribute("Day").Value.ToString());
                    Debug.Assert(supportConfig.RankOfDay >= 3 && supportConfig.RankOfDay <= 6);
                    supportConfig.CostJinBi = Convert.ToInt32(xmlItem.Attribute("CostZhiChi").Value.ToString());
                    supportConfig.MaxTimes = Convert.ToInt32(xmlItem.Attribute("MaxNum").Value.ToString());
                    supportConfig.WinPoint = 0; // GameServer根据WinMiniGoodsList解析，中心不使用
                    supportConfig.FailPoint = 0; // GameServer根据FailMiniGoodsList解析, 中心不使用
                    supportConfig.WinAwardTag = xmlItem.Attribute("WinAward").Value.ToString();
                    supportConfig.FailAwardTag = xmlItem.Attribute("FaillAward").Value.ToString();

                    supportConfig.TimeList = new List<ZhengBaSupportConfig.TimeConfig>();
                    string szDayBeginTime = xmlItem.Attribute("BeginTime").Value.ToString();
                    string szDayEndTime = xmlItem.Attribute("EndTime").Value.ToString();
                    DateTime dayBegin = DateTime.MinValue, dayEnd = DateTime.MinValue;
                    Debug.Assert(DateTime.TryParse(szDayBeginTime, out dayBegin) && DateTime.TryParse(szDayEndTime, out dayEnd));

                    ZhengBaSupportConfig.TimeConfig today = new ZhengBaSupportConfig.TimeConfig();
                    today.RealDay = supportConfig.RankOfDay;
                    today.DayBeginTicks = dayBegin.TimeOfDay.Ticks;
                    today.DayEndTicks = TimeSpan.TicksPerDay;

                    ZhengBaSupportConfig.TimeConfig tomorrow = new ZhengBaSupportConfig.TimeConfig();
                    tomorrow.RealDay = today.RealDay + 1;
                    tomorrow.DayBeginTicks = 0;
                    tomorrow.DayEndTicks = dayEnd.TimeOfDay.Ticks;

                    supportConfig.TimeList.Add(today);
                    supportConfig.TimeList.Add(tomorrow);

                    string[] szLevel = xmlItem.Attribute("MinLevel").Value.ToString().Split(',');
                    supportConfig.MinChangeLife = Convert.ToInt32(szLevel[0]);
                    supportConfig.MinLevel = Convert.ToInt32(szLevel[1]);

                    SupportConfigList.Add(supportConfig);
                }

                // magic number, only 3rd - 6rd day can support!
                for (int i = 3; i <= 6; i++)
                {
                    Debug.Assert(SupportConfigList.Exists(_s => _s.RankOfDay == i));
                }
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Error, "ZhengBaConfig.LoadSupport failed", ex);
                return false;
            }

            return true;
        }
    }
}
