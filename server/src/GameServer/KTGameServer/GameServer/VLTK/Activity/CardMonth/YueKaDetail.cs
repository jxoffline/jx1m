using Server.Data;
using System;

namespace GameServer.KiemThe.Core.Activity.CardMonth
{
    /// <summary>
    /// Class xử lý thông tin thẻ tháng từ DB parse ra
    /// </summary>
    public class YueKaDetail
    {
        public int HasYueKa = 0;

        public int BegOffsetDay = 0;

        public int EndOffsetDay = 0;

        public int CurOffsetDay = 0;

        public string AwardInfo = "";

        public YueKaDetail()
        {
            HasYueKa = 0;
            BegOffsetDay = 0;
            EndOffsetDay = 0;
            CurOffsetDay = 0;
            AwardInfo = "";
        }

        public YueKaData ToYueKaData()
        {
            YueKaData ykData = new YueKaData();
            ykData.HasYueKa = HasYueKa == 1 ? true : false;
            ykData.CurrDay = CurDayOfPerYueKa();
            ykData.AwardInfo = AwardInfo;
            ykData.RemainDay = RemainDayOfYueKa();
            return ykData;
        }

        public void ParseFrom(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return;
            }

            string[] fields = str.Split(',');
            if (fields.Length == 5)
            {
                HasYueKa = Convert.ToInt32(fields[0]);
                BegOffsetDay = Convert.ToInt32(fields[1]);
                EndOffsetDay = Convert.ToInt32(fields[2]);
                CurOffsetDay = Convert.ToInt32(fields[3]);
                AwardInfo = fields[4];
            }
        }

        public string SerializeToString()
        {
            if (HasYueKa == 0)
            {
                return "0,0,0,0,0";
            }
            else
            {
                return string.Format("{0},{1},{2},{3},{4}", 1,
                    BegOffsetDay, EndOffsetDay, CurOffsetDay, AwardInfo);
            }
        }

        public int CurDayOfPerYueKa()
        {
            if (HasYueKa == 0)
            {
                return 0;
            }
            else
            {
                return ((CurOffsetDay - BegOffsetDay) % CardMonthManager.DAYS_PER_YUE_KA) + 1;
            }
        }

        public int RemainDayOfYueKa()
        {
            if (HasYueKa == 0)
            {
                return 0;
            }
            else
            {
                return EndOffsetDay - CurOffsetDay;
            }
        }
    }
}