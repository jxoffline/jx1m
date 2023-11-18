using GameDBServer.Core;
using GameDBServer.Data;
using GameDBServer.DB;
using GameDBServer.Logic.SystemParameters;
using MySQLDriverCS;
using Server.Data;
using Server.Tools;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace GameDBServer.Logic.GuildLogic
{
    public partial class GuildManager
    {
        private static GuildManager instance = new GuildManager();

        public int PageDisplay = 9;

        public DBManager _Database = null;

        public int ThisWeek = 0;

        public long LastUpdateStatus = 0;

        /// <summary>
        /// 10s Reload lại trang thái của bang
        /// </summary>
        public long RefreshOnlineStatus = 30000;

        /// <summary>
        ///  Xem số lượng thành viên của bang
        /// </summary>
        /// <param name="GuildLevel"></param>
        /// <returns></returns>
        public int GetMaxMember(int GuildLevel)
        {
            return 20 + (GuildLevel * 10);
        }

        /// <summary>
        ///  Danh sách thành viên
        /// </summary>

        public ConcurrentDictionary<int, Guild> TotalGuild = new ConcurrentDictionary<int, Guild>();

        public ConcurrentDictionary<int, MiniGuildInfo> _MiniGuildInfo = new ConcurrentDictionary<int, MiniGuildInfo>();

        public MiniGuildInfo GetMiniGuildInfo(int GuildID)
        {
            _MiniGuildInfo.TryGetValue(GuildID, out MiniGuildInfo find);

            //var find = _MiniGuildInfo.Where(x => x.GuildId == GuildID).FirstOrDefault();
            if (find != null)
            {
                return find;
            }

            return null;
        }

        /// <summary>
        /// trả về bang hội theo ID
        /// </summary>
        /// <param name="GuildID"></param>
        /// <returns></returns>
        public Guild GetGuildByID(int GuildID)
        {
            TotalGuild.TryGetValue(GuildID, out Guild guild);
            return guild;
        }

        /// <summary>
        /// Instance class
        /// </summary>
        /// <returns></returns>
        public static GuildManager getInstance()
        {
            return instance;
        }

        /// <summary>
        /// Load ra toàn bộ bang hội
        /// </summary>
        /// <param name="_Db"></param>
        public void Setup(DBManager _Db)
        {
            this._Database = _Db;
            this.LoadAllGuild();
            this.LoadAllMiniGuildInfo();
            // Lấy ra tuần hiện tại
            this.ThisWeek = TimeUtil.GetIso8601WeekOfYear(DateTime.Now);
        }

        #region LoadingRequestJoin()

        public RequestJoinInfo GetTotalRequestInfo(int GuildID, int PageIndex)
        {
            RequestJoinInfo _Request = new RequestJoinInfo();

            TotalGuild.TryGetValue(GuildID, out Guild _OutGuild);

            int END = PageIndex * PageDisplay;

            int START = END - PageDisplay;

            _Request.AutoAccept = _OutGuild.AutoAccept;
            _Request.AutoAcceptRule = _OutGuild.RuleAccept;

            List<RequestJoin> _TotalList = new List<RequestJoin>(_OutGuild.TotalRequestJoin.Values);
            // Cast 1 ConcurrentDictionary về List

            int totalPage = _TotalList.Count / PageDisplay + 1;

            if (_TotalList.Count % PageDisplay == 0)
            {
                totalPage--;
            }

            _Request.PageIndex = PageIndex;
            _Request.TotalPage = totalPage;

            if (_TotalList.Count == 0)
            {
                _Request.TotalRequestJoin = new List<RequestJoin>();
                return _Request;
            }

            if (_TotalList.Count < START)
            {
                _Request.TotalRequestJoin = _TotalList;
            }

            if (_TotalList.Count > START && _TotalList.Count < END)
            {
                int RANGER = _TotalList.Count - START;

                _Request.TotalRequestJoin = _TotalList.GetRange(START, RANGER);
            }
            else
            {
                _Request.TotalRequestJoin = _TotalList.GetRange(START, PageDisplay);
            }

            return _Request;
        }

        public ConcurrentDictionary<int, RequestJoin> LoadingRequestJoin(int GuildID)
        {
            ConcurrentDictionary<int, RequestJoin> _Dict = new ConcurrentDictionary<int, RequestJoin>();

            MySQLConnection conn = null;

            try
            {
                conn = _Database.DBConns.PopDBConnection();

                string cmdText = "Select ID,RoleID,RoleName,RoleFactionID,RoleValue,RoleLevel,TimeRequest,GuildID from t_guild_request_join where GuildID = " + GuildID + "";

                MySQLCommand cmd = new MySQLCommand(cmdText, conn);
                MySQLDataReader reader = cmd.ExecuteReaderEx();

                while (reader.Read())
                {
                    RequestJoin _member = new RequestJoin();

                    _member.ID = Convert.ToInt32(reader["ID"].ToString());
                    _member.RoleID = Convert.ToInt32(reader["RoleID"].ToString());
                    _member.RoleName = reader["RoleName"].ToString();
                    _member.RoleFactionID = Convert.ToInt32(reader["RoleFactionID"].ToString());
                    // Read ra tài phú template tại thời điểm nó xin vào
                    _member.RoleValue = Convert.ToInt64(reader["RoleValue"].ToString());

                    _member.GuildID = Convert.ToInt32(reader["GuildID"].ToString());
                    // Thời gian gia nhập
                    _member.TimeRequest = DateTime.Parse(reader["TimeRequest"].ToString());
                    // Cấp độ của người chơi
                    _member.RoleLevel = Convert.ToInt32(reader["RoleLevel"].ToString());
                    _Dict.TryAdd(_member.RoleID, _member);
                }
                cmd.Dispose();
            }
            finally
            {
                if (null != conn)
                {
                    this._Database.DBConns.PushDBConnection(conn);
                }
            }

            return _Dict;
        }

        #endregion LoadingRequestJoin()

        #region Loading AllGuildMember()

        /// <summary>
        /// Trả về thành danh sách thành viên trong bang hội
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
                TotalGuild.TryGetValue(GuildID, out Guild _OutGuild);

                if (_OutGuild != null)
                {
                    int END = PageIndex * PageDisplay;

                    int START = END - PageDisplay;

                    // lấy ra toàn bộ guild
                    List<GuildMember> TotalMember = _OutGuild.GuildMember.Values.OrderByDescending(x => x.OnlineStatus).ToList();

                    int totalPage = _OutGuild.GuildMember.Count / PageDisplay + 1;
                    if (_OutGuild.GuildMember.Count % PageDisplay == 0)
                    {
                        totalPage--;
                    }

                    if (TotalMember.Count < START)
                    {
                        _memberdata.TotalGuildMember = TotalMember;
                    }

                    if (TotalMember.Count > START && TotalMember.Count < END)
                    {
                        int RANGER = TotalMember.Count - START;
                        _memberdata.TotalGuildMember = TotalMember.GetRange(START, RANGER);
                    }
                    else
                    {
                        _memberdata.TotalGuildMember = TotalMember.GetRange(START, PageDisplay);
                    }

                    _memberdata.PageIndex = PageIndex;
                    _memberdata.TotalPage = totalPage;
                }
            }
            catch (Exception exx)
            {
                LogManager.WriteLog(LogTypes.Guild, "BUG :" + exx.ToString());
            }
            return _memberdata;
        }

        /// <summary>
        /// Đọc ra bảng ghi
        /// </summary>
        /// <param name="GuildID"></param>
        /// <returns></returns>
        public GuildWar LoadAllGuildWar(int GuildID)
        {
            GuildWar _guildward = new GuildWar();
            MySQLConnection conn = null;

            try
            {
                int WEEKID = TimeUtil.GetIso8601WeekOfYear(DateTime.Now);

                conn = _Database.DBConns.PopDBConnection();

                string cmdText = "Select GuildID,GuildName,TeamList,WeekID from t_guildwar where GuildID = " + GuildID + " and WeekID = " + WEEKID + "";

                MySQLCommand cmd = new MySQLCommand(cmdText, conn);
                MySQLDataReader reader = cmd.ExecuteReaderEx();

                while (reader.Read())
                {
                    _guildward.GuildID = Convert.ToInt32(reader["GuildID"].ToString());

                    _guildward.GuildName = reader["GuildName"].ToString();

                    _guildward.TeamList = reader["TeamList"].ToString();

                    _guildward.WeekID = Convert.ToInt32(reader["WeekID"].ToString());
                }
                cmd.Dispose();
            }
            finally
            {
                if (null != conn)
                {
                    this._Database.DBConns.PushDBConnection(conn);
                }
            }

            return _guildward;
        }

        /// <summary>
        /// Lấy ra dánh ách thành viên trong bang
        /// </summary>
        /// <param name="GuildID"></param>
        /// <returns></returns>
        public ConcurrentDictionary<int, GuildMember> LoadingGuildMember(int GuildID)
        {

            int WEEKID = TimeUtil.GetIso8601WeekOfYear(DateTime.Now);

            ConcurrentDictionary<int, GuildMember> _MemberDict = new ConcurrentDictionary<int, GuildMember>();
            MySQLConnection conn = null;

            try
            {
                conn = _Database.DBConns.PopDBConnection();

                string cmdText = "Select rid,rname,occupation,level,zoneid,guildid,guildrank,roleprestige,guildmoney from t_roles where guildid = " + GuildID + "";

                MySQLCommand cmd = new MySQLCommand(cmdText, conn);
                MySQLDataReader reader = cmd.ExecuteReaderEx();

                while (reader.Read())
                {
                    GuildMember _member = new GuildMember();

                    _member.RoleID = Convert.ToInt32(reader["rid"].ToString());
                    _member.RoleName = reader["rname"].ToString();
                    _member.FactionID = Convert.ToInt32(reader["occupation"].ToString());
                    _member.GuildID = Convert.ToInt32(reader["guildid"].ToString());
                    _member.Level = Convert.ToInt32(reader["level"].ToString());
                    _member.Rank = Convert.ToInt32(reader["guildrank"].ToString());
                    _member.GuildMoney = Convert.ToInt32(reader["guildmoney"].ToString());
                    _member.ZoneID = Convert.ToInt32(reader["zoneid"].ToString());
                    _member.TotalValue = Convert.ToInt32(reader["roleprestige"].ToString());
                    _member.WeekPoint = 0;

                    DBRoleInfo otherDbRoleInfo = this._Database.GetDBRoleInfo(_member.RoleID);
                    if (otherDbRoleInfo != null)
                    {
                        otherDbRoleInfo.RoleParamsDict.TryGetValue("WeekRecore", out RoleParamsData _Data);

                        if (_Data != null)
                        {
                            try
                            {
                                string WeekRecore = _Data.ParamValue;

                                byte[] Base64Decode = Convert.FromBase64String(WeekRecore);

                                WeekDataRecore _WeekDataRecore = DataHelper.BytesToObject<WeekDataRecore>(Base64Decode, 0, Base64Decode.Length);

                                // nếu như khác tuần thì nó =0 luôn
                                if (_WeekDataRecore.WeekID != WEEKID)
                                {
                                    _member.WeekPoint = 0;
                                }
                                else
                                {
                                    _WeekDataRecore.EventRecoding.TryGetValue(100001, out int POINT);
                                    if (POINT > 0)
                                    {
                                        _member.WeekPoint = POINT;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                _member.WeekPoint = 0;
                            }
                        }
                    }

                    _MemberDict.TryAdd(_member.RoleID, _member);
                }
                cmd.Dispose();
            }
            finally
            {
                if (null != conn)
                {
                    this._Database.DBConns.PushDBConnection(conn);
                }
            }

            return _MemberDict;
        }

        #endregion Loading AllGuildMember()

        #region LoadingAllGuilData

        /// <summary>
        /// Load toàn bộ thông tin bang hội
        /// </summary>
        public void LoadAllGuild()
        {
            MySQLConnection conn = null;

            try
            {
                conn = _Database.DBConns.PopDBConnection();

                string cmdText = "Select GuildID,GuildName,MoneyBound,MoneyStore,ZoneID,Notify,IsMainCity,MaxWithDraw,Leader,DateCreate,GuildLevel,GuildExp,AutoAccept,RuleAccept,ItemStore,CurQuestID,SkillNote,PreDelete,DeleteStartDay,Total_Copy_Scenes_This_Week from t_guild order by GuildID desc";

                MySQLCommand cmd = new MySQLCommand(cmdText, conn);
                MySQLDataReader reader = cmd.ExecuteReaderEx();

                while (reader.Read())
                {
                    Guild _Guild = new Guild();

                    _Guild.GuildID = Convert.ToInt32(reader["GuildID"].ToString());
                    _Guild.GuildName = reader["GuildName"].ToString();
                    _Guild.MoneyBound = Convert.ToInt32(reader["MoneyBound"].ToString());
                    _Guild.MoneyStore = Convert.ToInt32(reader["MoneyStore"].ToString());
                    _Guild.ZoneID = Convert.ToInt32(reader["ZoneID"].ToString());
                    _Guild.Notify = "Quỹ Tô Thuế Đã Thu :" + _Guild.MoneyBound + "\n" + DataHelper.Base64Decode(reader["Notify"].ToString());
                    _Guild.IsMainCity = Convert.ToInt32(reader["IsMainCity"].ToString());
                    _Guild.MaxWithDraw = Convert.ToInt32(reader["MaxWithDraw"].ToString());
                    _Guild.Leader = Convert.ToInt32(reader["Leader"].ToString());
                    _Guild.DateCreate = DateTime.Parse(reader["DateCreate"].ToString());
                    _Guild.GuildLevel = Convert.ToInt32(reader["GuildLevel"].ToString());
                    _Guild.GuildExp = Convert.ToInt32(reader["GuildExp"].ToString());
                    _Guild.AutoAccept = Convert.ToInt32(reader["AutoAccept"].ToString());
                    _Guild.RuleAccept = reader["RuleAccept"].ToString();
                    _Guild.ItemStore = reader["ItemStore"].ToString();

                    _Guild.SkillNote = reader["SkillNote"].ToString();
                    _Guild.PreDelete = Convert.ToInt32(reader["PreDelete"].ToString());

                    string Total_Copy_Scenes_This_Week = reader["Total_Copy_Scenes_This_Week"].ToString();
                    if (Total_Copy_Scenes_This_Week.Length > 0)
                    {
                        _Guild.Total_Copy_Scenes_This_Week = Convert.ToInt32(reader["Total_Copy_Scenes_This_Week"].ToString());
                    }
                    else
                    {
                        _Guild.Total_Copy_Scenes_This_Week = 0;
                    }
                    // Thêm đoạn này để reset dữ liệu bang hội
                    if (DateTime.Now.DayOfWeek == DayOfWeek.Sunday)
                    {
                        string MARK = SystemGlobalParametersManager.GetSystemGlobalParameter(111);
                        if (MARK == "")
                        {
                            SystemGlobalParametersManager.SetSystemGlobalParameter(111, DateTime.Now.DayOfYear + "");
                            _Guild.Total_Copy_Scenes_This_Week = 0;
                            // Thực hiện ghi vào DB
                            UpdateResource(_Guild.GuildID, 5, "0");
                        }
                        else
                        {
                            if (Int32.Parse(MARK) != DateTime.Now.DayOfYear)
                            {
                                SystemGlobalParametersManager.SetSystemGlobalParameter(111, DateTime.Now.DayOfYear + "");
                                _Guild.Total_Copy_Scenes_This_Week = 0;
                                // Thực hiện ghi vào DB
                                UpdateResource(_Guild.GuildID, 5, "0");
                            }
                        }
                    }

                    _Guild.DeleteStartDay = DateTime.Parse(reader["DeleteStartDay"].ToString());
                    _Guild.TotalRequestJoin = LoadingRequestJoin(_Guild.GuildID);
                    // Lấy ra toàn bộ nhiệm vụ của bang
                    _Guild.Task = GetGuildTask(_Guild.GuildID);
                    //Lấy ra toàn bộ danh sách member
                    _Guild.GuildMember = LoadingGuildMember(_Guild.GuildID);

                    _Guild.GuildWar = LoadAllGuildWar(_Guild.GuildID);
                    // Thực hiện add vào danh sách bang hội
                    TotalGuild.TryAdd(_Guild.GuildID, _Guild);
                }

                GameDBManager.SystemServerSQLEvents.AddEvent(string.Format("+SQL: {0}", cmdText), EventLevels.Important);

                cmd.Dispose();
                cmd = null;
            }
            finally
            {
                if (null != conn)
                {
                    this._Database.DBConns.PushDBConnection(conn);
                }
            }
        }

        #endregion LoadingAllGuilData

        /// <summary>
        /// Lấy ra toàn bộ thông tin bang cơ bản để show ra tiền sảnh
        /// Chỗ này sẽ gọi lại mỗi khi chạy lại hoặc theo thời gian
        /// </summary>
        public void LoadAllMiniGuildInfo()
        {
            _MiniGuildInfo.Clear();
            // Duyệt toàn bộ bang hội
            foreach (var VARIABLE in TotalGuild)
            {
                Guild _Value = VARIABLE.Value;
                // Tìm xem bang có bagn chủ hay không
                var FindMaster = _Value.GuildMember.Where(x => x.Value.Rank == 1).FirstOrDefault();
                // Nếu mà tìm thấy bang chủ
                if (FindMaster.Value != null)
                {
                    MiniGuildInfo mini_info = new MiniGuildInfo();
                    mini_info.GuilLevel = _Value.GuildLevel;
                    mini_info.GuildId = _Value.GuildID;
                    mini_info.GuildName = _Value.GuildName;
                    mini_info.HostName = FindMaster.Value.RoleName;
                    mini_info.TotalMember = _Value.GuildMember.Count;
                    mini_info.SkillNote = _Value.SkillNote;
                    mini_info.GuildMoney = _Value.MoneyStore;
                    mini_info.GuildExp = _Value.GuildExp;
                    mini_info.ItemStore = _Value.ItemStore;
                    mini_info.GuildNotify = _Value.Notify;
                    mini_info.Task = _Value.Task;
                    mini_info.IsMainCity = _Value.IsMainCity;
                    mini_info.GuildWar = _Value.GuildWar;
                    mini_info.Total_Copy_Scenes_This_Week = _Value.Total_Copy_Scenes_This_Week;
                    mini_info.MoneyBound = _Value.MoneyBound;
                    _MiniGuildInfo.TryAdd(_Value.GuildID, mini_info);
                }
            }
        }

        #region DoGiftProsec

        #region Check Resource

        /// <summary>
        /// Update thuần từ tầng TCP
        /// </summary>
        /// <param name="Money"></param>
        /// <param name="GuildID"></param>
        /// <param name="RoleID"></param>
        /// <returns></returns>
        public bool UpdateRoleGuildMoneyFromTCP(int Money, int GuildID, int RoleID)
        {
            TotalGuild.TryGetValue(GuildID, out Guild _OutGuild);
            if (_OutGuild != null)
            {
                _OutGuild.GuildMember.TryGetValue(RoleID, out GuildMember _OutMember);
                if (_OutMember != null)
                {
                    // Cộng tiền vào tài khoản
                    _OutMember.GuildMoney += Money;

                    string cmdText = "Update t_roles set guildmoney = guildmoney + " + Money + " where rid = " + RoleID + " and guildid = " + GuildID + "";

                    return DBWriter.ExecuteSqlScript(cmdText, true);
                }
            }

            return false;
        }

        /// <summary>
        /// Cần làm read time bởi vì dữ liệu ở gs chưa chắc đã là mới nhất vì sysn thông tin không phải là 2 chiều
        /// </summary>
        /// <param name="GuildID"></param>
        /// <param name="ResourceType"></param>
        /// <returns></returns>
        public string CheckResource(int GuildID, int ResourceType)
        {
            TotalGuild.TryGetValue(GuildID, out Guild _OUTGUILD);

            if (_OUTGUILD != null)
            {
                // Nếu kiểu là 0 tức là đang kiểm trang ngân quỹ bang hội cần có
                if (ResourceType == 0)
                {
                    return _OUTGUILD.MoneyStore + ":" + GuildID;
                } // Nếu là 1 tức là kiểm tra vật phẩm hiện đang có
                else if (ResourceType == 1)
                {
                    return _OUTGUILD.ItemStore + ":" + GuildID;
                } // Kiểm tra danh sách kỹ năng đã kích hoạt
                else if (ResourceType == 2)
                {
                    return _OUTGUILD.SkillNote + ":" + GuildID;
                }
                else if (ResourceType == 7)
                {
                    return _OUTGUILD.MoneyBound + ":" + GuildID;
                }
            }
            else
            {
                return "-1:-1";
            }

            return "-2:-2";
        }

        //TODO : UPDATE RESOURCE VÀO MYSQL
        /// <summary>
        /// Cập nhật tài nguyên bang
        /// </summary>
        /// <param name="GuildID"></param>
        /// <param name="ResourceType"></param>
        /// <param name="ResourceValue"></param>
        /// <returns></returns>
        public string UpdateResource(int GuildID, int ResourceType, string ResourceValue)
        {
            TotalGuild.TryGetValue(GuildID, out Guild _OUTGUILD);

            if (_OUTGUILD != null)
            {
                // Nếu kiểu là 0 tức là đang kiểm trang ngân quỹ bang hội cần có
                if (ResourceType == 0)
                {
                    string Update = "Update t_guild set MoneyStore = " + Int32.Parse(ResourceValue) + " where GuildID = " + GuildID + "";
                    if (DBWriter.ExecuteSqlScript(Update))
                    {
                        _OUTGUILD.MoneyStore = Int32.Parse(ResourceValue);

                        _MiniGuildInfo.TryGetValue(GuildID, out MiniGuildInfo _Update);

                        if (_Update != null)
                        {
                            _Update.GuildMoney = Int32.Parse(ResourceValue);
                        }

                        return _OUTGUILD.MoneyStore + ":" + GuildID;
                    }
                    else
                    {
                        return "-1:-1:-1";
                    }
                } // Nếu là 1 tức là kiểm tra vật phẩm hiện đang có
                else if (ResourceType == 1)
                {
                    string Update = "Update t_guild set ItemStore = '" + ResourceValue + "' where GuildID = " + GuildID + "";
                    if (DBWriter.ExecuteSqlScript(Update))
                    {
                        _OUTGUILD.ItemStore = ResourceValue;

                        _MiniGuildInfo.TryGetValue(GuildID, out MiniGuildInfo _Update);

                        if (_Update != null)
                        {
                            _Update.ItemStore = ResourceValue;
                        }

                        return _OUTGUILD.ItemStore + ":" + GuildID;
                    }
                    else
                    {
                        return "-1:-1:-1";
                    }
                }
                else if (ResourceType == 2)
                { // Set danh sách kỹ năng
                    string Update = "Update t_guild set SkillNote = '" + ResourceValue + "' where GuildID = " + GuildID + "";

                    if (DBWriter.ExecuteSqlScript(Update))
                    {
                        _OUTGUILD.SkillNote = ResourceValue;

                        _MiniGuildInfo.TryGetValue(GuildID, out MiniGuildInfo _Update);

                        if (_Update != null)
                        {
                            _Update.SkillNote = ResourceValue;
                        }

                        return _OUTGUILD.SkillNote + ":" + GuildID;
                    }
                    else
                    {
                        return "-1:-1:-1";
                    }
                }
                else if (ResourceType == 3)
                { // Set danh sách kỹ năng
                    string Update = "Update t_guild set GuildExp = " + Int32.Parse(ResourceValue) + " where GuildID = " + GuildID + "";

                    if (DBWriter.ExecuteSqlScript(Update))
                    {
                        _OUTGUILD.GuildExp = Int32.Parse(ResourceValue);

                        _MiniGuildInfo.TryGetValue(GuildID, out MiniGuildInfo _Update);

                        if (_Update != null)
                        {
                            _Update.GuildExp = Int32.Parse(ResourceValue); ;
                        }

                        return _OUTGUILD.GuildExp + ":" + GuildID;
                    }
                    else
                    {
                        return "-1:-1:-1";
                    }
                }
                else if (ResourceType == 4)
                { // Set danh sách kỹ năng
                    string Update = "Update t_guild set GuildLevel = " + Int32.Parse(ResourceValue) + " where GuildID = " + GuildID + "";

                    if (DBWriter.ExecuteSqlScript(Update))
                    {
                        _OUTGUILD.GuildLevel = Int32.Parse(ResourceValue);

                        _MiniGuildInfo.TryGetValue(GuildID, out MiniGuildInfo _Update);

                        if (_Update != null)
                        {
                            _Update.GuilLevel = Int32.Parse(ResourceValue);
                        }

                        return _OUTGUILD.GuildLevel + ":" + GuildID;
                    }
                    else
                    {
                        return "-1:-1:-1";
                    }
                }
                else if (ResourceType == 5)
                { // Set danh sách kỹ năng
                    string Update = "Update t_guild set Total_Copy_Scenes_This_Week = " + Int32.Parse(ResourceValue) + " where GuildID = " + GuildID + "";

                    if (DBWriter.ExecuteSqlScript(Update))
                    {
                        _OUTGUILD.Total_Copy_Scenes_This_Week = Int32.Parse(ResourceValue);

                        _MiniGuildInfo.TryGetValue(GuildID, out MiniGuildInfo _Update);

                        if (_Update != null)
                        {
                            _Update.Total_Copy_Scenes_This_Week = Int32.Parse(ResourceValue);
                        }

                        return _OUTGUILD.Total_Copy_Scenes_This_Week + ":" + GuildID;
                    }
                    else
                    {
                        return "-1:-1:-1";
                    }
                }
                // Nếu là update thành chủ
                else if (ResourceType == 6)
                { // Set danh sách kỹ năng
                    string Update = "Update t_guild set IsMainCity = " + Int32.Parse(ResourceValue) + " where GuildID = " + GuildID + "";

                    if (DBWriter.ExecuteSqlScript(Update))
                    {
                        _OUTGUILD.IsMainCity = Int32.Parse(ResourceValue);

                        _MiniGuildInfo.TryGetValue(GuildID, out MiniGuildInfo _Update);

                        if (_Update != null)
                        {
                            _Update.IsMainCity = Int32.Parse(ResourceValue);
                        }

                        return _OUTGUILD.IsMainCity + ":" + GuildID;
                    }
                    else
                    {
                        return "-1:-1:-1";
                    }
                }
                else if (ResourceType == 7)
                {
                    string Update = "Update t_guild set MoneyBound = " + Int32.Parse(ResourceValue) + " where GuildID = " + GuildID + "";

                    if (DBWriter.ExecuteSqlScript(Update))
                    {
                        _OUTGUILD.MoneyBound = Int32.Parse(ResourceValue);

                        _MiniGuildInfo.TryGetValue(GuildID, out MiniGuildInfo _Update);

                        if (_Update != null)
                        {
                            _Update.MoneyBound = Int32.Parse(ResourceValue);
                        }

                        return _OUTGUILD.MoneyBound + ":" + GuildID;
                    }
                    else
                    {
                        return "-1:-1:-1";
                    }
                }
            }
            else
            {
                return "-1:-1";
            }
            return "-2:-2";
        }

        #endregion Check Resource

        /// <summary>
        /// Thực hiện gửi quà tặng cho các thành viên tích cực
        /// </summary>
        /// 


        public void ReloadGuildWeeklyPointMember(List<GuildMember> TotalMember)
        {
            foreach(GuildMember _member in TotalMember)
            {
                DBRoleInfo otherDbRoleInfo = this._Database.GetDBRoleInfo(_member.RoleID);
                if (otherDbRoleInfo != null)
                {
                    otherDbRoleInfo.RoleParamsDict.TryGetValue("WeekRecore", out RoleParamsData _Data);

                    if (_Data != null)
                    {
                        try
                        {
                            string WeekRecore = _Data.ParamValue;

                            byte[] Base64Decode = Convert.FromBase64String(WeekRecore);

                            WeekDataRecore _WeekDataRecore = DataHelper.BytesToObject<WeekDataRecore>(Base64Decode, 0, Base64Decode.Length);


                            _WeekDataRecore.EventRecoding.TryGetValue(100001, out int POINT);
                            if (POINT > 0)
                            {
                                _member.WeekPoint = POINT;
                            }
                        }
                        catch (Exception ex)
                        {
                            _member.WeekPoint = 0;
                        }
                    }
                }
            }
           
        }
        public void DoGiftProsecc()
        {
            foreach (Guild _Guild in TotalGuild.Values)
            {
                try
                {

                   // ReloadGuildWeeklyPointMember(_Guild.GuildMember.Values.ToList());

                    // Lấy ra toàn bộ các thành viên có điểm hoạt động tuần lớn hơn 0
                    List<GuildMember> _TotalMember = _Guild.GuildMember.Values.Where(x=>x.WeekPoint>0).OrderByDescending(x=>x.WeekPoint).ToList();

                    // lấy ra số tiền thưởng của bang hiện có
                    int _TotalMoneyBound = _Guild.MoneyBound;

                    // Lấy ra tổng số điểm hoạt động tuần của cả bang
                    long TOTALPOINT = _TotalMember.Sum(x => x.WeekPoint);

                    if (_TotalMoneyBound > _TotalMember.Count)
                    {
                        int COUNT = 1;
                       
                        foreach (GuildMember _Member in _TotalMember)
                        {

                            int ThisMemberPoint = _Member.WeekPoint;

                            // Xem thằng này chiếm tỉ trọng bao nhiêu trong tổng tích lũy
                            double PERCENT = ((double)ThisMemberPoint / (double)TOTALPOINT) *100;

                            double MoneyGet = (PERCENT * (double)_TotalMoneyBound) / 100;

                            int MoneyFix = (int)MoneyGet;

                            if (MoneyFix > 1)
                            {

                                PacketSendToGs _Packet = new PacketSendToGs();
                                _Packet.chatType = 0;
                                _Packet.extTag1 = _Guild.GuildID;
                                _Packet.index = ChatChannel.Guild;
                                _Packet.Msg = "ADDBACKHOA|" + _Member.RoleID + "|" + _Member.RoleName + "|" + MoneyFix + "|GUILD";
                                _Packet.RoleID = _Member.RoleID;
                                _Packet.roleName = _Member.RoleName;
                                _Packet.serverLineID = -1;
                                _Packet.status = 0;
                                _Packet.toRoleName = _Member.RoleName;

                                // gửi lệnh phát thưởng cho CLIENT
                                this.SendMsgToGameServer(_Packet);


                                this.PushGuildMsg("Thành viên <color=red>" + _Member.RoleName + "</color> tuần vừa qua với điểm hoạt động <color=green>"+ ThisMemberPoint + "</color> đã xếp thứ <b>"+ COUNT + "</b> trong bang nhận được "+ MoneyFix + " bạc khóa lợi tức xin chúc mừng!", _Guild.GuildID, 0, "");
                            }

                            LogManager.WriteLog(LogTypes.Guild, "[" + _Guild.GuildID + "][" + _Guild.GuildName + "][ADDTHUONGUUTU] : " + _Member.RoleID + "==>" + MoneyFix);

                            COUNT++;
                        }

                        string EXECUTE = string.Format("Update t_guild set MoneyBound = 0 where guildid = " + _Guild.GuildID + "");
                        DBWriter.ExecuteSqlScript(EXECUTE);

                        //Set lại tiền cho bang hội
                        _Guild.MoneyBound =0;
                    }
                    else
                    {
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    LogManager.WriteLog(LogTypes.Guild, "[BUG]["+ _Guild.GuildName+ "] : " + ex.ToString() + "===> Thực hiện tiếp cho bang khác");
                    continue;
                    
                }
            }
        }

        #endregion DoGiftProsec

        /// <summary>
        /// Cập nhật tiền cho bang hội
        /// </summary>
        /// <param name="MoneyAdd"></param>
        /// <param name="GuildId"></param>
        /// <returns></returns>
        public bool UpdateGuildMoneyBound(int MoneyAdd, int GuildId)
        {
            TotalGuild.TryGetValue(GuildId, out Guild _OutGuild);

            if (_OutGuild != null)
            {
                _OutGuild.MoneyStore = _OutGuild.MoneyStore + MoneyAdd;

                _MiniGuildInfo.TryGetValue(GuildId, out MiniGuildInfo _MiniInfo);
                if (_MiniInfo != null)
                {
                    _MiniInfo.GuildMoney = _OutGuild.MoneyStore;
                }

                MySQLConnection conn = null;

                try
                {
                    conn = this._Database.DBConns.PopDBConnection();

                    string cmdText = string.Format("Update t_guild set MoneyStore =  MoneyStore + " + MoneyAdd + " where guildid = " + GuildId + "");

                    MySQLCommand cmd = new MySQLCommand(cmdText, conn);

                    cmd.ExecuteNonQuery();

                    GameDBManager.SystemServerSQLEvents.AddEvent(string.Format("+SQL: {0}", cmdText), EventLevels.Important);

                    cmd.Dispose();
                    cmd = null;

                    return true;
                }
                catch (Exception ex)
                {
                    LogManager.WriteLog(LogTypes.Guild, "BUG :" + ex.ToString());
                }
                finally
                {
                    if (null != conn)
                    {
                        this._Database.DBConns.PushDBConnection(conn);
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Đổi thông báo của bang
        /// </summary>
        /// <param name="GuildID"></param>
        /// <param name="NotifyIn"></param>
        /// <returns></returns>
        public bool ChangeNotifyGuild(int GuildID, string NotifyIn)
        {
            if (NotifyIn.Length > 1000)
            {
                return false;
            }

            string NotifyCode = DataHelper.Base64Encode(NotifyIn);

            if (this.UpdateGuildNofity(GuildID, NotifyCode))
            {
                TotalGuild.TryGetValue(GuildID, out Guild _OutGuild);

                if (_OutGuild != null)
                {
                    _OutGuild.Notify = NotifyIn;
                }
                this.PushGuildMsg("Tôn chỉ bang đã thay đổi :" + NotifyIn, GuildID, 0, "");

                return true;
            }

            return false;
        }

        /// <summary>
        /// Cập nhật database thông báo của bang
        /// </summary>
        /// <param name="GuildId"></param>
        /// <param name="Notify"></param>
        /// <returns></returns>
        public bool UpdateGuildNofity(int GuildId, string Notify)
        {
            MySQLConnection conn = null;

            try
            {
                conn = this._Database.DBConns.PopDBConnection();
                string cmdText = string.Format("Update t_guild set Notify = '" + Notify + "' where GuildID = " + GuildId + "");
                MySQLCommand cmd = new MySQLCommand(cmdText, conn);
                cmd.ExecuteNonQuery();
                GameDBManager.SystemServerSQLEvents.AddEvent(string.Format("+SQL: {0}", cmdText), EventLevels.Important);

                cmd.Dispose();
                return true;
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Guild, "BUG :" + ex.ToString());
            }
            finally
            {
                if (null != conn)
                {
                    this._Database.DBConns.PushDBConnection(conn);
                }
            }

            return false;
        }

        /// <summary>
        /// Cập nhật bang hội
        /// </summary>
        public void UpdateGuildProsecc()
        {
            try
            {
                int WeekID = TimeUtil.GetIso8601WeekOfYear(DateTime.Now);
                // Nếu sang tuần mới thì thực hiện phát thưởng
                if (this.ThisWeek != WeekID)
                {
                    this.ThisWeek = WeekID;

                    LogManager.WriteLog(LogTypes.SQL, "Thực hiện phát lợi tức cho toàn bộ người chơi!");
                    //Thực hiện add thưởng ưu tú cho tất cả bang hội
                    this.DoGiftProsecc();

                    LogManager.WriteLog(LogTypes.SQL, "Hoàn tất việc phát lợi tức!");
                }

                long Now = TimeUtil.NOW();

                if (Now - LastUpdateStatus > RefreshOnlineStatus)
                {
                    LogManager.WriteLog(LogTypes.SQL, "[GUILD]Update Online Status");

                    LastUpdateStatus = Now;

                    foreach (Guild _Guild in TotalGuild.Values)
                    {
                        // Loop toàn bộ ngươi chơi trong bang
                        foreach (GuildMember _Member in _Guild.GuildMember.Values)
                        {
                            DBRoleInfo otherDbRoleInfo = this._Database.GetDBRoleInfo(_Member.RoleID);

                            _Member.OnlineStatus = Global.GetRoleOnlineState(otherDbRoleInfo);
                        }

                        ConcurrentDictionary<int, GuildMember> concurrentDictionary = new ConcurrentDictionary<int, GuildMember>(_Guild.GuildMember.OrderByDescending(x => x.Value.OnlineStatus).ThenByDescending(x => x.Value.Rank));
                    }
                }

                // Viết thêm 1 hàm tính toán lại rank cho bọn nó ở đây gọi là cache cổ tức
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Guild, "BUG ::" + ex.ToString());
            }
        }

        // Yêu cầu tham gia bang
        public int RequestJoinGuild(int RoleID, int GuildID)
        {
            if (RoleExitsGuild(RoleID))
            {
                // Đã có guild rồi thì thôi chim cút
                return -1;
            }

            // Nếu có cái bang này
            TotalGuild.TryGetValue(GuildID, out Guild _OutGuild);

            if (_OutGuild != null)
            {
                // Lấy ra số thành viên tối đa mà bang có thể chứa được
                int MaxMeber = GetMaxMember(_OutGuild.GuildLevel);

                if (_OutGuild.GuildMember.Count + 1 > MaxMeber)
                {
                    // Đã hết slot thành viên vui lòng nâng cấp bang hội
                    return -2;
                }

                int AutoAccecpt = _OutGuild.AutoAccept;
                // Nếu như bang có chế độ tự động đồng ý
                // Kiểm tra xem có thỏa mãn điều kiện hay không
                if (AutoAccecpt == 1)
                {
                    string AutoAccectRule = _OutGuild.RuleAccept;
                    // Lấy ra các pram
                    string[] Pram = AutoAccectRule.Split('|');

                    int LevelRequest = Int32.Parse(Pram[0]);

                    int Faction = Int32.Parse(Pram[1]);

                    long Value = Int64.Parse(Pram[2]);

                    DBRoleInfo _RoleInfo = this._Database.GetDBRoleInfo(RoleID);

                    if (LevelRequest > 0)
                    {
                        int CurLevel = _RoleInfo.Level;
                        if (CurLevel < LevelRequest)
                        {
                            return -300;
                        }
                    }

                    if (Faction != 0)
                    {
                        int CurFaction = _RoleInfo.Occupation;
                        if (CurFaction != Faction)
                        {
                            return -300;
                        }
                    }

                    if (Value > 0)
                    {
                        if (_RoleInfo.Prestige < Value)
                        {
                            return -300;
                        }
                    }

                    // Lập tức cho thằng này vào bang
                    if (this.UpdateRoleJoinGuild(RoleID, _OutGuild.GuildName, _OutGuild.GuildID, (int)GuildRank.Member))
                    {
                        //UPDATE VÀO ROLE HIỆN TẠI
                        DBRoleInfo roleInfo = _Database.GetDBRoleInfo(RoleID);

                        // Nếu tìm thấy người chơi này
                        if (roleInfo != null)
                        {
                            // Lock lại cho chắc chắn
                            lock (roleInfo)
                            {
                                // set lại thông tin cho role info
                                roleInfo.GuildID = GuildID;
                                roleInfo.GuildName = _OutGuild.GuildName;
                                roleInfo.GuildRank = (int)GuildRank.Member;
                            }

                            // Tạo mới 1 member đẩy vào DICT thành viên
                            GuildMember __GuidMember = new GuildMember();

                            if (roleInfo != null)
                            {
                                // Thực hiện xóa toàn bộ yêu cầu xin vào bang của thằng này ở các bang khác
                                this.ClearRequestJoin(roleInfo.RoleID);

                                __GuidMember.FactionID = roleInfo.Occupation;

                                __GuidMember.GuildID = roleInfo.GuildID;
                                // Cống hiến cá nhân chắc chắn =0;
                                __GuidMember.GuildMoney = 0;
                                __GuidMember.Level = roleInfo.Level;
                                __GuidMember.OnlineStatus = 1;
                                // Tài phú của thằng này để hiện trong bang cho nó tiện
                                __GuidMember.TotalValue = roleInfo.Prestige;
                                __GuidMember.Rank = (int)GuildRank.Member;
                                __GuidMember.RoleID = roleInfo.RoleID;
                                __GuidMember.RoleName = roleInfo.RoleName;

                                __GuidMember.ZoneID = roleInfo.ZoneID;

                                //Điểm hoạt động tuần chắc chắn là 0;
                                __GuidMember.WeekPoint = 0;

                                if (_OutGuild.GuildMember.TryAdd(__GuidMember.RoleID, __GuidMember))
                                {
                                    // Thông báo tới toàn bang có thằng vào
                                    this.PushGuildMsg("Thành viên [<b>" + roleInfo.RoleName + "</b>] đã gia nhập bang hội!", _OutGuild.GuildID, 0, "");
                                }

                                return 10;
                            }
                        }
                    }
                }
                else if (AutoAccecpt == 0) // nếu như bang này không thiết lập tự động duyệt
                {
                    // Nếu thằng này đã xin vào bang này rồi
                    if (_OutGuild.TotalRequestJoin.TryGetValue(RoleID, out RequestJoin _Request))
                    {
                        return -100;
                    }

                    DBRoleInfo roleInfo = _Database.GetDBRoleInfo(RoleID);
                    if (roleInfo != null)
                    {
                        // Nếu ko thì cho nó vào
                        RequestJoin _RequestJoin = new RequestJoin();
                        _RequestJoin.RoleID = RoleID;
                        _RequestJoin.GuildID = GuildID;
                        _RequestJoin.RoleValue = roleInfo.Prestige;
                        _RequestJoin.TimeRequest = DateTime.Now;
                        _RequestJoin.RoleFactionID = roleInfo.Occupation;
                        _RequestJoin.RoleName = roleInfo.RoleName;
                        _RequestJoin.RoleLevel = roleInfo.Level;
                        _OutGuild.TotalRequestJoin.TryAdd(RoleID, _RequestJoin);
                    }

                    string InsertSql =
                        "Insert into t_guild_request_join(RoleID,RoleName,RoleFactionID,RoleValue,TimeRequest,GuildID,RoleLevel) VALUES (" +
                        RoleID + ",'" + roleInfo.RoleName + "'," + roleInfo.Occupation + "," + roleInfo.Prestige +
                        ",now()," + GuildID + "," + roleInfo.Level + ")";

                    DBWriter.ExecuteSqlScript(InsertSql);

                    // Trả về kết quả là xin gia nhập thành công
                    return 0;
                }
            }

            return -1;
        }

        /// <summary>
        /// Xóa yêu cầu gia nhập ở các bang khác
        /// </summary>
        /// <param name="RoleID"></param>
        /// <returns></returns>
        public bool ClearRequestJoin(int RoleID, int GuildID = -1)
        {
            try
            {
                if (GuildID == -1)
                {
                    // Duyệt tất cả bang hội hiện có ở máy chủ
                    foreach (var guild in TotalGuild.Values)
                    {
                        // Nếu có thằng nào xin vào
                        if (guild.TotalRequestJoin.Count > 0)
                        {
                            // Thử xóa luôn nếu có thằng này tồn tại trong list request
                            guild.TotalRequestJoin.TryRemove(RoleID, out RequestJoin _Request);
                        }
                    }

                    //Thực thi xóa
                    DBWriter.ExecuteSqlScript("Delete from t_guild_request_join where RoleID = " + RoleID + "");
                }
                else
                {
                    TotalGuild.TryGetValue(GuildID, out Guild _Guild);
                    if (_Guild != null)
                    {
                        if (_Guild.TotalRequestJoin.TryRemove(RoleID, out RequestJoin _Request))
                        {
                            DBWriter.ExecuteSqlScript("Delete from t_guild_request_join where RoleID = " + RoleID + " and GuildID = " + GuildID + "");
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Family, "BUG :" + ex.ToString());

                return false;
            }
        }

        /// <summary>
        /// Khi bang chủ hoặc phó bang chủ duyệt thành viên nó sẽ nhảy vào đây
        /// </summary>
        /// <param name="RoleID"></param>
        /// <param name="GuildID"></param>
        /// <param name="Rank"></param>
        /// <returns></returns>
        public int AccecptJoinGuild(int RoleID, int GuildID)
        {
            if (RoleExitsGuild(RoleID))
            {
                // Đã có guild rồi
                return -1;
            }

            TotalGuild.TryGetValue(GuildID, out Guild _OutGuild);

            if (_OutGuild != null)
            {
                // Lấy ra số thành viên tối đa mà bang có thể chứa được
                int MaxMeber = GetMaxMember(_OutGuild.GuildLevel);

                if (_OutGuild.GuildMember.Count + 1 > MaxMeber)
                {
                    // Xóa yêu cầu của thằng kia luôn
                    //this.ClearRequestJoin(RoleID, GuildID);
                    // Đã hết slot thành viên vui lòng nâng cấp bang hội
                    return -2;
                }

                // Update cho thằng này vào guild
                if (this.UpdateRoleJoinGuild(RoleID, _OutGuild.GuildName, _OutGuild.GuildID, (int)GuildRank.Member))
                {
                    //UPDATE VÀO ROLE HIỆN TẠI
                    DBRoleInfo roleInfo = _Database.GetDBRoleInfo(RoleID);

                    // Nếu tìm thấy người chơi này
                    if (roleInfo != null)
                    {
                        // Lock lại cho chắc chắn
                        lock (roleInfo)
                        {
                            // set lại thông tin cho role info
                            roleInfo.GuildID = GuildID;
                            roleInfo.GuildName = _OutGuild.GuildName;
                            roleInfo.GuildRank = (int)GuildRank.Member;
                        }

                        // Tạo mới 1 member đẩy vào DICT thành viên
                        GuildMember __GuidMember = new GuildMember();

                        if (roleInfo != null)
                        {
                            // Thực hiện xóa toàn bộ yêu cầu xin vào bang của thằng này ở các bang khác
                            this.ClearRequestJoin(roleInfo.RoleID);

                            __GuidMember.FactionID = roleInfo.Occupation;

                            __GuidMember.GuildID = roleInfo.GuildID;
                            // Cống hiến cá nhân chắc chắn =0;
                            __GuidMember.GuildMoney = 0;
                            __GuidMember.Level = roleInfo.Level;
                            __GuidMember.OnlineStatus = 1;
                            // Tài phú của thằng này để hiện trong bang cho nó tiện
                            __GuidMember.TotalValue = roleInfo.Prestige;
                            __GuidMember.Rank = (int)GuildRank.Member;
                            __GuidMember.RoleID = roleInfo.RoleID;
                            __GuidMember.RoleName = roleInfo.RoleName;

                            __GuidMember.ZoneID = roleInfo.ZoneID;

                            //Điểm hoạt động tuần chắc chắn là 0;
                            __GuidMember.WeekPoint = 0;

                            if (_OutGuild.GuildMember.TryAdd(__GuidMember.RoleID, __GuidMember))
                            {
                                // Thông báo tới toàn bang có thằng vào
                                this.PushGuildMsg("Thành viên [<b>" + roleInfo.RoleName + "</b>] đã gia nhập bang hội!", _OutGuild.GuildID, 0, "");
                            }
                        }
                    }
                }
            }
            else
            {
                // Bang không tồn tại
                return -3;
            }

            // Nếu mút thì return 0
            return 0;
        }

        /// <summary>
        /// Từ chối cho thằng này gia nhập bang hội
        /// </summary>
        /// <param name="RoleID"></param>
        /// <param name="GuildID"></param>
        /// <returns></returns>
        public int RejectJoinRequest(int RoleID, int GuildID)
        {
            if (this.ClearRequestJoin(RoleID, GuildID))
            {
                return 10;
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// Send MSG TO GS
        /// </summary>
        /// <param name="serverLineID"></param>
        /// <param name="gmCmd"></param>
        public void SendMsgToGameServer(PacketSendToGs SendToGs)
        {
            string Build = string.Format("{0}:{1}:{2}:{3}:{4}:{5}:{6}:{7}:{8}", SendToGs.RoleID, SendToGs.roleName, SendToGs.status, SendToGs.toRoleName, (int)(SendToGs.index), SendToGs.Msg, SendToGs.chatType, SendToGs.extTag1, SendToGs.serverLineID);

            List<LineItem> itemList = LineManager.GetLineItemList();

            if (null != itemList)
            {
                for (int i = 0; i < itemList.Count; i++)
                {
                    if (itemList[i].LineID < GameDBManager.KuaFuServerIdStartValue || itemList[i].LineID == GameDBManager.ZoneID)
                    {
                        ChatMsgManager.AddChatMsg(itemList[i].LineID, Build);
                    }
                }
            }
        }

        #region CreateGuild

        /// <summary>
        /// Tạo bang
        /// </summary>
        /// <param name="RoleID"></param>
        /// <param name="ZoneID"></param>
        /// <param name="GuildName"></param>
        /// <returns></returns>
        public string CreateGuild(int RoleID, string GuildName, int ZoneID)
        {
            // Nếu thằng này có bang rồi thì thôi
            if (this.RoleExitsGuild(RoleID))
            {
                return "-4:ERROR";
            }

            // Nếu bang này đã tồn tại
            if (this.IsGuildExist(this._Database, GuildName))
            {
                return "-3:ERROR";
            }

            string RequestPram = "0|0|0";

            string Base64Encode = DataHelper.Base64Encode("Chào mừng các bạn đã gia nhập bang [" + GuildName + "]");

            //Thêm thông tin bang này vào DB
            string cmdText = "Insert into t_guild(GuildName,MoneyBound,MoneyStore,ZoneID,Notify,IsMainCity,MaxWithDraw,Leader,DateCreate,GuildLevel,GuildExp,AutoAccept,RuleAccept,ItemStore,CurQuestID,SkillNote,PreDelete,DeleteStartDay) VALUES " +
                             "('" + GuildName + "',0,1000000," + ZoneID + ",'" + Base64Encode + "',0,50," + RoleID + ",now(),1,0,0,'" + RequestPram + "','',-1,'',0,now())";

            // Thực hiện ghi bang này vào database
            if (DBWriter.ExecuteSqlScript(cmdText, true))
            {
                string GetGuildName = "SELECT GuildID from t_guild where GuildName = '" + GuildName + "'";

                // Lấy ra thông tin ID BANG
                int GuildID = DBQuery.GetIntValue(GetGuildName, "GuildID");

                // Cập nhật thông tin cho thằng tạo bang
                if (this.UpdateRoleJoinGuild(RoleID, GuildName, GuildID, (int)GuildRank.Master))
                {
                    // Change info cache của thằng bang chủ

                    DBRoleInfo roleInfo = _Database.GetDBRoleInfo(RoleID);
                    if (roleInfo != null)
                    {
                        lock (roleInfo)
                        {
                            roleInfo.GuildID = GuildID;
                            roleInfo.GuildName = GuildName;
                            roleInfo.GuildRank = (int)GuildRank.Master;
                        }
                    }

                    //Tạo mới 1 cái bang đưa vào dict
                    Guild _Guild = new Guild();

                    _Guild.DateCreate = DateTime.Now;

                    _Guild.GuildID = GuildID;

                    _Guild.GuildName = GuildName;

                    _Guild.Leader = RoleID;

                    _Guild.Notify = "Chào mừng các bạn đã gia nhập bang [" + GuildName + "]";

                    _Guild.IsMainCity = 0;

                    _Guild.GuildLevel = 1;

                    _Guild.GuildExp = 0;

                    _Guild.AutoAccept = 0;

                    _Guild.RuleAccept = RequestPram;

                    _Guild.ItemStore = "";

                    _Guild.Total_Copy_Scenes_This_Week = 0;

                    _Guild.SkillNote = "";

                    _Guild.PreDelete = 0;

                    _Guild.DeleteStartDay = DateTime.Now;

                    _Guild.GuildMember = new ConcurrentDictionary<int, GuildMember>();

                    _Guild.TotalRequestJoin = new ConcurrentDictionary<int, RequestJoin>();

                    _Guild.MaxWithDraw = 50;

                    _Guild.MoneyBound = 0;

                    _Guild.MoneyStore = 1000000;

                    _Guild.Notify = "Thông báo bang hội";

                    _Guild.ZoneID = ZoneID;

                    //Tạo mới 1 guild meber
                    GuildMember __GuidMember = new GuildMember();

                    __GuidMember.FactionID = roleInfo.Occupation;

                    __GuidMember.GuildID = roleInfo.GuildID;

                    // Điểm cống hiến chắc chắn là = 0;

                    __GuidMember.GuildMoney = 0;
                    __GuidMember.Level = roleInfo.Level;
                    __GuidMember.OnlineStatus = 1;

                    __GuidMember.TotalValue = roleInfo.Prestige;
                    __GuidMember.Rank = (int)GuildRank.Master;
                    __GuidMember.RoleID = roleInfo.RoleID;
                    __GuidMember.RoleName = roleInfo.RoleName;

                    __GuidMember.ZoneID = roleInfo.ZoneID;

                    //Điểm hoạt động tuần chắc chắn là 0;
                    __GuidMember.WeekPoint = 0;

                    // Add thành viên này vào bang
                    _Guild.GuildMember.TryAdd(roleInfo.RoleID, __GuidMember);

                    //Add bang này vào cache
                    TotalGuild.TryAdd(GuildID, _Guild);

                    //Setup lại guild mini info sau khi tạo bang xong
                    this.LoadAllMiniGuildInfo();

                    return "0:" + _Guild.GuildID + ":" + _Guild.GuildName;
                }
            }
            else
            {
                return "-6:ERROR";
            }

            return "-5:ERROR";
        }

        #endregion CreateGuild

        public bool UpdateRoleJoinGuild(int RoleID, string GuildName, int GuildID, int Rank)
        {
            string cmdText = string.Format("Update t_roles set guildid = " + GuildID + ",guildname ='" + GuildName + "',guildrank = " + Rank + " where rid = " + RoleID + "");

            return DBWriter.ExecuteSqlScript(cmdText, false);
        }

        /// <summary>
        /// Kích 1 thằng khỏi bang hội
        /// </summary>
        /// <param name="RoleID"></param>
        /// <param name="GuildID"></param>
        public int RoleLeaverGuild(int RoleID, int GuildID)
        {
            TotalGuild.TryGetValue(GuildID, out Guild _outGuild);

            if (_outGuild != null)
            {
                if (_outGuild.GuildMember.TryRemove(RoleID, out GuildMember _Out))
                {
                    string RemoveGuild = string.Format("Update t_roles set guildid = 0,guildname = '',guildrank = 0,guildmoney = 0 where rid = " + RoleID + "");
                    if (DBWriter.ExecuteSqlScript(RemoveGuild))
                    {
                        DBRoleInfo _RoleTargetFind = this._Database.GetDBRoleInfo(RoleID);
                        // Cho thằng này chim cút
                        lock (_RoleTargetFind)
                        {
                            _RoleTargetFind.GuildID = 0;
                            _RoleTargetFind.GuildRank = 0;
                            _RoleTargetFind.GuildName = "";
                            _RoleTargetFind.RoleGuildMoney = 0;
                        }

                        return 0;
                    }
                }
            }

            return -1;
        }

        /// <summary>
        /// Kiểm tra xem 1 thành viên đã có bang chưa
        /// </summary>
        /// <param name="RoleID"></param>
        /// <returns></returns>
        public bool RoleExitsGuild(int RoleID)
        {
            DBRoleInfo roleInfo = _Database.GetDBRoleInfo(RoleID);

            if (roleInfo != null)
            {
                if (roleInfo.GuildID > 0)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Check xem Guild đã tồn tại chưa
        /// </summary>
        /// <param name="dbMgr"></param>
        /// <param name="GuildName"></param>
        /// <returns></returns>
        public bool IsGuildExist(DBManager dbMgr, string GuildName)
        {
            using (MySqlUnity conn = new MySqlUnity())
            {
                string cmdText = string.Format("SELECT count(*) from t_guild where GuildName='" + GuildName + "'");
                return conn.GetSingleInt(cmdText) > 0;
            }
        }

        /// <summary>
        /// Lấy ra danh hiệu của bang
        /// </summary>
        /// <param name="_Rank"></param>
        /// <returns></returns>
        public string GetGuildTitile(int _Rank)
        {
            if (_Rank <= 0)
            {
                return "";
            }

            /// Danh hiệu theo chức vụ
            string guildRankName = "";

            if (_Rank <= (int)GuildRank.Member)
            {
                guildRankName = "Bang chúng";
            }
            else if (_Rank == (int)GuildRank.Master)
            {
                guildRankName = "Bang chủ";
            }
            else if (_Rank == (int)GuildRank.ViceMaster)
            {
                guildRankName = "Phó bang chủ";
            }
            else if (_Rank == (int)GuildRank.ViceAmbassador)
            {
                guildRankName = "Đường chủ";
            }
            else if (_Rank == (int)GuildRank.Ambassador)
            {
                guildRankName = "Trưởng lão";
            }
            else if (_Rank == (int)GuildRank.Elite)
            {
                guildRankName = "Tinh anh";
            }

            /// Trả về kết quả
            return guildRankName;
        }

        public bool CheckExitViceMaster(int GuildID)
        {
            TotalGuild.TryGetValue(GuildID, out Guild _GuildOut);

            if (_GuildOut != null)
            {
                var findrole = _GuildOut.GuildMember.Values.Where(x => x.Rank == (int)GuildRank.ViceMaster).FirstOrDefault();
                if (findrole != null)
                {
                    return true;
                }
            }
            return false;
        }

        public void ChangeGuildMemberName(int GuildID, int RoleID, string NewName)
        {
            TotalGuild.TryGetValue(GuildID, out Guild _GuildOut);

            if (_GuildOut != null)
            {
                var findrole = _GuildOut.GuildMember.Values.Where(x => x.RoleID == RoleID).FirstOrDefault();
                if (findrole != null)
                {
                    findrole.RoleName = NewName;
                }
            }
        }

        /// <summary>
        /// Gửi tin nhắn về GAMESERVER QUA TOÀN BỘ KÊNH BANG
        /// </summary>
        /// <param name="MSG"></param>
        /// <param name="GuildID"></param>
        /// <param name="RoleID"></param>
        /// <param name="RoleName"></param>
        public void PushGuildMsg(string MSG, int GuildID, int RoleID, string RoleName)
        {
            PacketSendToGs _Packet = new PacketSendToGs();
            _Packet.chatType = 0;
            _Packet.extTag1 = GuildID;
            _Packet.index = ChatChannel.Guild;
            _Packet.Msg = MSG;
            _Packet.RoleID = RoleID;
            _Packet.roleName = RoleName;
            _Packet.serverLineID = -1;
            _Packet.status = 0;
            _Packet.toRoleName = RoleName;

            // gửi lệnh phát thưởng cho CLIENT
            this.SendMsgToGameServer(_Packet);
        }

        /// <summary>
        /// Trả về số thành viên tối đa cho 1 chức vụ
        /// </summary>
        /// <param name="_Rank"></param>
        /// <returns></returns>
        public int GetMaxCountMemberByRank(GuildRank _Rank)
        {
            switch (_Rank)
            {
                case GuildRank.Member:
                    return 9999;

                case GuildRank.Ambassador:
                    return 5;

                case GuildRank.Master:
                    return 1;

                case GuildRank.ViceMaster:
                    return 2;

                case GuildRank.Elite:
                    return 42;

                case GuildRank.ViceAmbassador:
                    return 10;

                default:
                    return 0;
            }
        }

        /// <summary>
        /// Check xem thằng này có quyền hạn thay đổi chức vụ thành viên hay không
        /// </summary>
        /// <param name="InputRank"></param>
        /// <param name="TargetRank"></param>
        /// <returns></returns>
        public bool CheckPermission(GuildRank InputRank, GuildRank TargetRank)
        {
            // Nếu là bang chủ thì ok hết
            if (InputRank == GuildRank.Master)
            {
                return true;
            }
            else if (InputRank == GuildRank.ViceMaster)
            {
                if (TargetRank == GuildRank.Ambassador || TargetRank == GuildRank.ViceAmbassador ||
                    TargetRank == GuildRank.Elite || TargetRank == GuildRank.Member)
                {
                    return true;
                }
            }
            else if (InputRank == GuildRank.Ambassador)
            {
                if (TargetRank == GuildRank.ViceAmbassador ||
                    TargetRank == GuildRank.Elite || TargetRank == GuildRank.Member)
                {
                    return true;
                }
            }
            else if (InputRank == GuildRank.ViceAmbassador)
            {
                if (TargetRank == GuildRank.Elite || TargetRank == GuildRank.Member)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Update rank in mysql
        /// </summary>
        /// <param name="RoleID"></param>
        /// <param name="Rank"></param>
        /// <returns></returns>
        public bool UpdateGuildRank(int RoleID, int Rank, bool UpdateLead = false, int GuildID = -1)
        {
            string cmdText = string.Format("Update t_roles set guildrank = " + Rank + " where rid = " + RoleID + "");

            if (DBWriter.ExecuteSqlScript(cmdText, false))
            {
                if (UpdateLead)
                {
                    string _UpdateLead = "Update t_guild set Leader = " + RoleID + " where GuildID = " + GuildID + "";
                    DBWriter.ExecuteSqlScript(_UpdateLead, false);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Thăng hạng cho 1 thằng nào đấy
        /// </summary>
        /// <param name="GuildID"></param>
        /// <param name="HostRoleID"></param>
        /// <param name="RoleIDSelect"></param>
        /// <param name="RankTarget"></param>
        /// <returns></returns>
        public int ChangeRankForMember(int GuildID, int HostRoleID, int RoleIDSelect, int RankTarget)
        {
            TotalGuild.TryGetValue(GuildID, out Guild _OutGuild);

            // Nếu thật sự bang này tồn tại
            if (_OutGuild != null)
            {
                GuildMember Host = _OutGuild.GuildMember.Values.Where(x => x.RoleID == HostRoleID).FirstOrDefault();

                GuildMember RoleTarget = _OutGuild.GuildMember.Values.Where(x => x.RoleID == RoleIDSelect).FirstOrDefault();

                //Target chức vụ của thằng muốn đề cử
                GuildRank _TargetRank = (GuildRank)RankTarget;

                if (Host != null && RoleTarget != null)
                {
                    GuildRank _HostRank = (GuildRank)Host.Rank;

                    if (!CheckPermission(_HostRank, _TargetRank))
                    {
                        // Không đủ quyền hạn để thay đổi chức vụ
                        return -2;
                    }

                    // Chỉ cần set đúng 1 trường hợp bổ nhiệm bang chủ khác
                    if (_TargetRank == GuildRank.Master)
                    {
                        //Set rank cho thằng được chọn làm bang chủ
                        RoleTarget.Rank = (int)GuildRank.Master;

                        // Set cho thằng bang chủ hiện thời về làm dân thường
                        Host.Rank = (int)GuildRank.Member;

                        // Tìm ra db của thằng target
                        DBRoleInfo _RoleTargetFind = this._Database.GetDBRoleInfo(RoleTarget.RoleID);
                        if (_RoleTargetFind != null)
                        {
                            //Update luôn leader
                            _RoleTargetFind.GuildRank = (int)GuildRank.Master;
                            this.UpdateGuildRank(RoleTarget.RoleID, (int)GuildRank.Master, true, _RoleTargetFind.GuildID);

                            _OutGuild.Leader = _RoleTargetFind.RoleID;
                        }

                        DBRoleInfo _RoleHost = this._Database.GetDBRoleInfo(Host.RoleID);
                        if (_RoleHost != null)
                        {
                            // Set thằng host về dân thường
                            _RoleHost.GuildRank = (int)GuildRank.Member;
                            this.UpdateGuildRank(_RoleHost.RoleID, (int)GuildRank.Member);
                        }

                        // Thông báo tới toàn bang có thằng vào
                        this.PushGuildMsg("Thành viên [<b>" + _RoleTargetFind.RoleName + "</b>] đã được bổ nhiệm làm bang chủ!", _OutGuild.GuildID, 0, "");

                        return 10;
                    } // Các trường hợp khác thì chỉ cần đủ quyền lực là có thể thay đổi chức vụ cho thằng thành viên đó
                    else
                    {
                        int MaxCount = GetMaxCountMemberByRank(_TargetRank);

                        int Count = _OutGuild.GuildMember.Values.Count(x => x.Rank == (int)_TargetRank);

                        if (Count + 1 > MaxCount)
                        {
                            return -5;
                        }

                        DBRoleInfo _RoleHost = this._Database.GetDBRoleInfo(Host.RoleID);

                        // Set rank cho thằng target
                        RoleTarget.Rank = (int)_TargetRank;

                        // Update cho thằng Target
                        DBRoleInfo _RoleTargetFind = this._Database.GetDBRoleInfo(RoleTarget.RoleID);
                        if (_RoleTargetFind != null)
                        {
                            _RoleTargetFind.GuildRank = (int)_TargetRank;
                            this.UpdateGuildRank(RoleTarget.RoleID, (int)_TargetRank);
                        }

                        if (RankTarget != (int)GuildRank.Member)
                        {
                            this.PushGuildMsg("Thành viên [<b>" + _RoleTargetFind.RoleName + "</b>] đã được bổ nhiệm làm " + GetGuildTitile(RankTarget) + " bởi [" + _RoleHost.RoleName + "]", _OutGuild.GuildID, 0, "");
                        }
                        else
                        {
                            this.PushGuildMsg("Thành viên [<b>" + _RoleTargetFind.RoleName + "</b>] đã bị hạ cấp xuống " + GetGuildTitile(RankTarget) + " bởi [" + _RoleHost.RoleName + "]", _OutGuild.GuildID, 0, "");
                        }

                        return 0;
                    }
                }
                else
                {
                    return -100;
                }
            }

            return -1;
        }

        /// <summary>
        /// Thực hiện ghi vào DB danh sách đăng ký tham gia công thành chiến
        /// </summary>
        /// <param name="GuildID"></param>
        /// <param name="GuildName"></param>
        /// <param name="TeamList"></param>
        /// <param name="CityAttack"></param>
        /// <param name="IsWiner"></param>
        /// <returns></returns>
        public int InsertGuildWar(int GuildID, string GuildName, string TeamList, int WeekID)
        {
            string Insert = "Insert into t_guildwar(GuildID,GuildName,TeamList,WeekID) VALUES (" + GuildID + ",'" + GuildName + "','" + TeamList + "'," + WeekID + ") ON DUPLICATE KEY UPDATE TeamList = '" + TeamList + "',WeekID= " + WeekID + "";

            if (DBWriter.ExecuteSqlScript(Insert))
            {
                return 0;
            }

            return -1;
        }

        /// <summary>
        /// Change Rule tự động duyệt bang
        /// </summary>
        /// <param name="GuildID"></param>
        /// <param name="OnOff"></param>
        /// <param name="Level"></param>
        /// <param name="Faction"></param>
        /// <param name="Value"></param>
        /// <returns></returns>
        public int ChangeRule(int GuildID, int OnOff, int Level, int Faction, int Value)
        {
            TotalGuild.TryGetValue(GuildID, out Guild _GuildOut);

            if (_GuildOut != null)
            {
                _GuildOut.AutoAccept = OnOff;
                _GuildOut.RuleAccept = Level + "|" + Faction + "|" + Value;

                string UpdateDB = "Update t_guild set AutoAccept = " + OnOff + ",RuleAccept= '" + _GuildOut.RuleAccept + "' WHERE GuildID = " + GuildID + "";

                DBWriter.ExecuteSqlScript(UpdateDB, false);
                return 0;
            }
            return -1;
        }
    }
}