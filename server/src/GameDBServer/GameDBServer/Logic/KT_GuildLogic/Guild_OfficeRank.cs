using Server.Data;
using System.Collections.Generic;
using System.Linq;

namespace GameDBServer.Logic.GuildLogic
{
    // QUẢN LÝ RANK
    public partial class GuildManager
    {
        public string GetRankTile(int RankOffice)
        {
            switch (RankOffice)
            {
                case 1:
                    return "Trí Sự";

                case 2:
                    return "Tư Mã";

                case 3:
                    return "Thái Thú";

                case 4:
                    return "Thiếu Khanh";

                case 5:
                    return "Thượng Khanh";

                case 6:
                    return "Quốc Công";
            }

            return "Chưa có hạng";
        }

        public int GetMaxRankCanGet(int RankInput, int Index)
        {
            if (Index == 1)
            {
                return RankInput;
            }
            else if (Index > 1 && Index < 5)
            {
                return RankInput - 1;
            }
            else if (Index >= 5 && Index < 7)
            {
                return RankInput - 2;
            }
            else if (Index >= 7 && Index < 10)
            {
                return RankInput - 3;
            }

            return -1;
        }

        public int GetRankOfGuild(int TotalTerory)
        {
            if (TotalTerory >= 42)
            {
                return 6;
            }

            if (TotalTerory >= 18)
            {
                return 5;
            }

            if (TotalTerory >= 10)
            {
                return 4;
            }

            if (TotalTerory >= 6)
            {
                return 3;
            }

            if (TotalTerory >= 3)
            {
                return 2;
            }

            if (TotalTerory >= 1)
            {
                return 1;
            }

            return 0;
        }

        public int GetMemberRank(int GuildID, int ROLEID)
        {
            OfficeRankInfo _RankInfo = this.GetOfficeRankInfoOfGuild(GuildID);

            var find = _RankInfo.OffcieRankMember?.Where(x => x != null && x.RoleID == ROLEID).FirstOrDefault();

            if (find != null)
            {
                return find.RankNum;
            }
            else
            {
                return 0;
            }
        }

        public OfficeRankInfo GetOfficeRankInfoOfGuild(int GuildID)
        {
            OfficeRankInfo _Rank = new OfficeRankInfo();

            _Rank.OffcieRankMember = new List<OfficeRankMember>();

            TotalGuild.TryGetValue(GuildID, out Guild _OutGuild);

            if (_OutGuild != null)
            {
                _Rank.GuildRank = GetRankOfGuild(_OutGuild.Territorys.Count);

                if (_OutGuild != null)
                {
                    if (_OutGuild.CacheGuildShare != null)
                    {
                        if (_OutGuild.CacheGuildShare.Count > 0)
                        {
                            // Đoạn này viết lại lấy từ DB ra là ổn
                            List<GuildShare> TotalMember = _OutGuild.CacheGuildShare?.OrderBy(x => x.Rank).ToList();

                            int i = 1;

                            foreach (GuildShare _Member in TotalMember)
                            {
                                OfficeRankMember _member = new OfficeRankMember();

                                _member.ID = i;
                                _member.RoleName = _Member.RoleName;
                                _member.RoleID = _Member.RoleID;

                                if (i == 1)
                                {
                                    _member.RankTile = "Tinh Anh";

                                    _member.OfficeRankTitle = GetRankTile(_Rank.GuildRank);

                                    int MaxRankCanGet = GetMaxRankCanGet(_Rank.GuildRank, i);

                                    _member.RankNum = MaxRankCanGet;
                                }
                                else
                                {
                                    _member.RankTile = "Quan Cấp " + (i - 1);

                                    int MaxRankCanGet = GetMaxRankCanGet(_Rank.GuildRank, i);

                                    _member.OfficeRankTitle = GetRankTile(MaxRankCanGet);

                                    _member.RankNum = MaxRankCanGet;
                                }

                                _Rank.OffcieRankMember.Add(_member);

                                i++;
                            }
                        }
                    }
                }
            }

            if (_Rank.GuildRank <= 0)
            {
                _Rank.GuildRank = -1;
            }

            return _Rank;
        }
    }
}