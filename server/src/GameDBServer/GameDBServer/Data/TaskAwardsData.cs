using ProtoBuf;
using System.Collections.Generic;

namespace Server.Data
{
    [ProtoContract]
    public class TaskAwardsData
    {
        [ProtoMember(1)]
        public List<AwardsItemData> TaskawardList = null;

        [ProtoMember(2)]
        public List<AwardsItemData> OtherTaskawardList = null;

        [ProtoMember(3)]
        public int Moneyaward = 0;

        [ProtoMember(4)]
        public int Experienceaward = 0;

        [ProtoMember(5)]
        public int YinLiangaward = 0;

        [ProtoMember(6)]
        public int LingLiaward = 0;

        [ProtoMember(7)]
        public int BindYuanBaoaward = 0;

        [ProtoMember(8)]
        public int ZhenQiaward = 0;

        [ProtoMember(9)]
        public int LieShaaward = 0;

        [ProtoMember(10)]
        public int WuXingaward = 0;

        [ProtoMember(11)]
        public int NeedYuanBao = 0;

        [ProtoMember(12)]
        public int JunGongaward = 0;

        [ProtoMember(13)]
        public int RongYuaward = 0;

        [ProtoMember(14)]
        public int AddExperienceForDailyCircleTask = 0;

        [ProtoMember(15)]
        public int AddBindYuanBaoForDailyCircleTask = 0;

        [ProtoMember(16)]
        public int AddGoodsForDailyCircleTask = 0;
    }
}