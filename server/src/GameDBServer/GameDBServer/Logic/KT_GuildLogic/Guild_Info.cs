using Server.Data;
using Server.Tools;
using System;

namespace GameDBServer.Logic.GuildLogic
{
    public partial class GuildManager
    {
        #region CMD_KT_GUILD_GETMEMBERLIST

        /// <summary>
        /// Lấy ra danh sách thành viên bang hội
        /// </summary>
        /// <param name="RoleID"></param>
        /// <param name="GuildID"></param>
        /// <param name="PageIndex"></param>
        /// <returns></returns>
        public GuildMemberData GetGuidMemberData(int RoleID, int GuildID, int PageIndex)
        {
            GuildMemberData _memberdata = new GuildMemberData();
            try
            {
                //List<FamilyObj> TotalFamilyMemeber = new List<FamilyObj>();

                //TotalGuild.TryGetValue(GuildID, out Guild _OutGuild);

                //if (_OutGuild != null)
                //{
                //    int END = PageIndex * PageDisplay;

                //    int START = END - PageDisplay;

                //    List<GuildMember> TotalMember = _OutGuild.GuildMember.Values.OrderByDescending(x => x.OnlienStatus).ToList();
                //    int totalPage = _OutGuild.GuildMember.Count / PageDisplay + 1;
                //    if (_OutGuild.GuildMember.Count % PageDisplay == 0)
                //    {
                //        totalPage--;
                //    }

                //    if (TotalMember.Count < START)
                //    {
                //        _memberdata.TotalGuildMember = TotalMember;
                //    }

                //    if (TotalMember.Count > START && TotalMember.Count < END)
                //    {
                //        int RANGER = TotalMember.Count - START;
                //        _memberdata.TotalGuildMember = TotalMember.GetRange(START, RANGER);
                //    }
                //    else
                //    {
                //        _memberdata.TotalGuildMember = TotalMember.GetRange(START, PageDisplay);
                //    }

                //    string[] FamilyStr = _OutGuild.Familys.Split('|');

                //    foreach (string FamilyID in FamilyStr)
                //    {
                //        int FamilyConvert = Int32.Parse(FamilyID);

                //        Family Find = FamilyManager.getInstance().GetFamily(FamilyConvert);
                //        if (Find != null)
                //        {
                //            FamilyObj _obj = new FamilyObj();
                //            _obj.FamilyID = Find.FamilyID;
                //            _obj.FamilyName = Find.FamilyName;
                //            _obj.MemberCount = Find.Members.Count;
                //            _obj.TotalpPrestige = Find.Members.Sum(x => x.Prestige);

                //            TotalFamilyMemeber.Add(_obj);
                //        }
                //    }

                //    _memberdata.TotalFamilyMemeber = TotalFamilyMemeber;
                //    _memberdata.PageIndex = PageIndex;
                //    _memberdata.TotalPage = totalPage;
                //}
            }
            catch (Exception exx)
            {
                LogManager.WriteLog(LogTypes.Guild, "BUG :" + exx.ToString());
            }
            return _memberdata;
        }

        #endregion CMD_KT_GUILD_GETMEMBERLIST
    }
}