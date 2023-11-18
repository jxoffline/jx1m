using Server.Data;
using System.Collections.Generic;
using System.Linq;

namespace GameDBServer.Logic.GuildLogic
{
    public partial class GuildManager
    {
        /// <summary>
        /// Trả về dánh ách Guild
        /// </summary>
        /// <param name="GuildId"></param>
        /// <param name="PageIndex"></param>
        /// <returns></returns>
        public List<GuildShare> GetListGuildShare(int GuildId, int PageIndex, out int totalPage)
        {
            List<GuildShare> _Member = new List<GuildShare>();

            TotalGuild.TryGetValue(GuildId, out Guild _OutGuild);

            totalPage = 0;
            if (_OutGuild != null)
            {
                int END = PageIndex * PageDisplay;

                int START = END - PageDisplay;

                List<GuildShare> TotalMember = _OutGuild.GuildShare.OrderByDescending(x => x.Share).ToList();
                totalPage = _OutGuild.GuildShare.Count / PageDisplay + 1;
                if (_OutGuild.GuildShare.Count % PageDisplay == 0)
                {
                    totalPage--;
                }

                if (TotalMember.Count < START)
                {
                    _Member = TotalMember;
                }

                if (TotalMember.Count > START && TotalMember.Count < END)
                {
                    int RANGER = TotalMember.Count - START;
                    _Member = TotalMember.GetRange(START, RANGER);
                }
                else
                {
                    _Member = TotalMember.GetRange(START, PageDisplay);
                }
            }

            return _Member;
        }
    }
}