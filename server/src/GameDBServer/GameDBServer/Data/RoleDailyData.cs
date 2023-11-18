using ProtoBuf;

namespace Server.Data
{

    [ProtoContract]
    public class RoleDailyData
    {

        [ProtoMember(1)]
        public int ExpDayID = 0;


        [ProtoMember(2)]
        public int TodayExp = 0;

        [ProtoMember(3)]
        public int LingLiDayID = 0;


        [ProtoMember(4)]
        public int TodayLingLi = 0;


        [ProtoMember(5)]
        public int KillBossDayID = 0;

        [ProtoMember(6)]
        public int TodayKillBoss = 0;


        [ProtoMember(7)]
        public int FuBenDayID = 0;

        [ProtoMember(8)]
        public int TodayFuBenNum = 0;


        [ProtoMember(9)]
        public int WuXingDayID = 0;

        [ProtoMember(10)]
        public int WuXingNum = 0;
    }
}