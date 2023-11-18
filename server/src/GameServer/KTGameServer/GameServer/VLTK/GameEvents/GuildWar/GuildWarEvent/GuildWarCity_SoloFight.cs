using GameServer.Core.Executor;
using GameServer.KiemThe;
using GameServer.KiemThe.CopySceneEvents.DynamicArena;
using GameServer.KiemThe.Logic;
using GameServer.Logic;
using GameServer.Server;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameServer.VLTK.Core.GuildManager
{
    public partial class GuildWarCity
    {
        /// <summary>
        /// Sơ đồ thi đấu lôi đài
        /// </summary>
        public List<ELIMINATION_INFO> ELIMINATION_TOTAL = new List<ELIMINATION_INFO>();

        /// <summary>
        /// Đây là trận thứ mấy
        /// </summary>
        public int GLOBALROUNDID { get; set; }

        /// <summary>
        /// Trạng thái hoạt động của lôi đài
        /// </summary>
        public SOLOFIGHT_STATE FIGHT_STATE { get; set; }

        /// <summary>
        /// Thời gian đăng chuẩn bị lên lôi đài giữa các hiệp
        /// </summary>
        public int PRE_FIGHT_DULATION { get; set; } = 300;

        /// <summary>
        /// 30 giây free dualtion
        /// </summary>
        public int PRE_FREE_DULATION { get; set; } = 30;

        /// <summary>
        /// Để dư ra tầm 60s để lôi đài trả về kết quả
        /// </summary>
        public int FIGHT_DULATION_TIME { get; set; } = 360;

        /// <summary>
        /// Danh sách các thành viên sẽ tham gia thi đấu
        /// </summary>
        public List<GuildTeamFightMember> FightTeams { get; set; }

        /// <summary>
        /// Thời gian tick gần đây nhất
        /// </summary>
        public long LastFightTick { get; set; }

        public long LastPushNotifyTick { get; set; }

        /// <summary>
        /// Bản đồ sẽ war hôm nay
        /// </summary>
        public static CityInfo ActiveWar = null;

        /// <summary>
        /// Tọa độ ra vào lôi đài
        /// </summary>
        public List<int> OutMapCode { get; set; }

        /// <summary>
        /// Hàm này được gọi sau mỗi round đấu để thằng nào online muộn có cơ hội được tham gia vào team
        /// </summary>
        public void ReloadPlayerMember()
        {
            foreach (GuildTeamFightMember _Team in FightTeams)
            {
                string[] TeamMeber = _Team.TeamList.Split('_');

                foreach (string Member in TeamMeber)
                {
                    string[] MemberPram = Member.Split('|');

                    int MemberID = Int32.Parse(MemberPram[0]);

                    var find = _Team.Members.Where(x => x.RoleID == MemberID).FirstOrDefault();
                    // Nếu như thằng này không tìm thấy thì thực hiện thêm mới vào
                    if (find == null)
                    {
                        KPlayer client = KTPlayerManager.Find(MemberID);
                        if (client != null)
                        {
                            _Team.Members.Add(client);
                        }
                    } // Nếu như thằng này đã có trong đội trước đó thì sẽ kiểm tra xem nó có online hay ko nếu ko thì xóa khỏi đội
                    else
                    {
                        KPlayer client = KTPlayerManager.Find(MemberID);
                        // nếu thằng này đéo có
                        if (client == null)
                        {
                            _Team.Members.Remove(find);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Đây có phải thời gian đăng ký thi đấu đội không
        /// </summary>
        /// <returns></returns>
        public bool IsTimeRegisterFight()
        {
            int Today = TimeUtil.GetWeekDay1To7(DateTime.Now);
            int Day = _WarConfig.Citys[0].TeamFightDay;
            if (Today < Day)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Hàm này sẽ kết nối vào DB để lây ra toàn bộ các team đã đăng ký lôi đài
        /// </summary>
        public void GetInfoTeamFight()
        {
            foreach (MiniGuildInfo _MiniGuild in GuildManager._TotalGuild.Values)
            {
                // Nếu bang này có thông tin
                if (_MiniGuild != null)
                {
                    // Lấy ra tất cả các bang đang đăng ký thi đấu lôi đài
                    if (_MiniGuild.IsMainCity == (int)GUILD_CITY_STATUS.REGISTERFIGHT)
                    {
                        // Nếu có thông tin về guildwar
                        if (_MiniGuild.GuildWar != null)
                        {
                            if (_MiniGuild.GuildWar.TeamList != null)
                            {
                                if (_MiniGuild.GuildWar.TeamList.Length > 0)
                                {
                                    // Tạo 1 team riêng tham chiến
                                    GuildTeamFightMember _FightTeam = new GuildTeamFightMember();
                                    _FightTeam.CityAttack = ActiveWar.CityID;
                                    _FightTeam.GuildID = _MiniGuild.GuildId;
                                    _FightTeam.GuildName = _MiniGuild.GuildName;
                                    _FightTeam.Members = new List<KPlayer>();

                                    _FightTeam.TeamList = _MiniGuild.GuildWar.TeamList;

                                    foreach (string _MemberID in _MiniGuild.GuildWar.TeamList.Split('_'))
                                    {
                                        string[] MEMBERINFO = _MemberID.Split('|');

                                        int MemberID = Int32.Parse(MEMBERINFO[0]);
                                        // Tìm người chơi theo roleID
                                        KPlayer client = KTPlayerManager.Find(MemberID);
                                        if (client != null)
                                        {
                                            _FightTeam.Members.Add(client);
                                        }
                                    }

                                    FightTeams.Add(_FightTeam);
                                }
                            }
                        }
                    }
                }
            }
        }

        #region SORTSOLOFIGHT

        public void SendMSGForAllTeamFight(string MSG)
        {
            foreach (GuildTeamFightMember _Team in FightTeams)
            {
                KTGlobal.SendGuildChat(_Team.GuildID, MSG, null, "");
            }
        }

        public void StartFight()
        {
            // Lấy ra toàn bộ các team chưa có thắng thua
            // Và trong hiệp lần này
            List<ELIMINATION_INFO> TotalTeamWillFuckThisRound = ELIMINATION_TOTAL.Where(x => x.ROUNDID == GLOBALROUNDID && x.WinThisRound == null).ToList();
            // Duyệt tất cả lại
            foreach (ELIMINATION_INFO _ARENA in TotalTeamWillFuckThisRound)
            {
                if (_ARENA.Team_1.Members == null || _ARENA.Team_1.Members?.Count == 0)
                {
                    string Notify = "Chiến đội đã dành chiến thằng trong vòng thi đấu thứ <b><color=red>" + GLOBALROUNDID + "</color></b>!Do chiến đội :" + _ARENA.Team_1.GuildName + " không có thành viên nào tham gia!";
                    KTGlobal.SendGuildChat(_ARENA.Team_2.GuildID, Notify, null, "");

                    string NOTIFYALL = "Chiến đội của bang  <b><color=red>" + _ARENA.Team_2.GuildName + "</color></b> đã dành chiến thắng trong vòng thi đấu thứ <b><color=red>" + GLOBALROUNDID + "</color></b>";
                    this.SendMSGForAllTeamFight(NOTIFYALL);

                    _ARENA._ROUNDSTATE = ROUNDSTATE.END;
                    // Đánh dấu team thắng là team 2
                    _ARENA.WinThisRound = _ARENA.Team_2;
                }
                else if (_ARENA.Team_2.Members == null || _ARENA.Team_2.Members?.Count == 0)
                {
                    string Notify = "Chiến đội đã dành chiến thằng trong vòng thi đấu thứ <color=red>" + GLOBALROUNDID + "</color>!!Do chiến đội :" + _ARENA.Team_2.GuildName + " không có thành viên nào tham gia!";
                    KTGlobal.SendGuildChat(_ARENA.Team_1.GuildID, Notify, null, "");

                    string NOTIFYALL = "Chiến đội của bang <b><color=red>" + _ARENA.Team_1.GuildName + "]</color></b> đã dành chiến thắng trong vòng thi đấu thứ <b><color=red>" + GLOBALROUNDID + "</color></b>";
                    this.SendMSGForAllTeamFight(NOTIFYALL);
                    _ARENA._ROUNDSTATE = ROUNDSTATE.END;
                    // Đánh dấu  Team 1 thắng
                    _ARENA.WinThisRound = _ARENA.Team_1;
                }
                else
                {
                    DynamicArena_EventScript.Begin("Thách Đấu Công Thành Chiến", _ARENA.Team_1.Members, _ARENA.Team_2.Members, 300000, OutMapCode[0], OutMapCode[1], OutMapCode[2], (winnerTeamPlayer) =>
                    {
                        if (winnerTeamPlayer != null)
                        {
                            // Nếu là team 1 thắng
                            if (_ARENA.Team_1.Members.Contains(winnerTeamPlayer))
                            {
                                string Notify = "Chiến đội đã dành chiến thằng trong vòng thi đấu thứ <color=red>" + GLOBALROUNDID + "</color>!Xin chúc mừng";
                                KTGlobal.SendGuildChat(_ARENA.Team_1.GuildID, Notify, null, "");

                                string NOTIFYALL = "Chiến đội của bang <b><color=red>" + _ARENA.Team_1.GuildName + "</color></b> đã dành chiến thắng trong vòng thi đấu thứ <color=red>" + GLOBALROUNDID + "</color>!";
                                this.SendMSGForAllTeamFight(NOTIFYALL);
                                _ARENA._ROUNDSTATE = ROUNDSTATE.END;
                                // Đánh dấu  Team 1 thắng
                                _ARENA.WinThisRound = _ARENA.Team_1;
                            }
                            else
                            {
                                string Notify = "Chiến đội đã dành chiến thằng trong vòng thi đấu thứ <color=red>" + GLOBALROUNDID + "</color>!Xin chúc mừng";
                                KTGlobal.SendGuildChat(_ARENA.Team_2.GuildID, Notify, null, "");

                                string NOTIFYALL = "Chiến đội của bang  <b><color=red>" + _ARENA.Team_2.GuildName + "</color></b> đã dành chiến thắng trong vòng thi đấu thứ <color=red>" + GLOBALROUNDID + "</color>!";
                                this.SendMSGForAllTeamFight(NOTIFYALL);

                                _ARENA._ROUNDSTATE = ROUNDSTATE.END;
                                // Đánh dấu team thắng là team 2
                                _ARENA.WinThisRound = _ARENA.Team_2;
                            }
                        }
                    });
                }
            }
        }

        /// <summary>
        /// Trả về nhà vô địch
        /// </summary>
        /// <returns></returns>
        public GuildTeamFightMember GetFinalTeamVictory()
        {
            //Lấy ra dánh sách lôi đài trong đợt thid dấu lần này
            List<ELIMINATION_INFO> FinalRound = ELIMINATION_TOTAL.Where(x => x.ROUNDID == GLOBALROUNDID).ToList();
            // Nếu đây là lôi đài duy nhất trong vòng đấu này
            if (FinalRound.Count() == 1)
            {
                ELIMINATION_INFO FinalArena = FinalRound[0];

                if (FinalArena.WinThisRound != null)
                {
                    return FinalArena.WinThisRound;
                }
            }
            // Nếu đéo tìm được ai thắng
            return null;
        }

        /// <summary>
        /// Tạo sơ đồ thi đấu
        /// </summary>
        /// <param name="BattleFactionRank"></param>
        public void CreateSoloEvent(List<GuildTeamFightMember> FightTeams)
        {
            // thực hiện trộn các đội lại với nhau
            KTGlobal.Shuffle<GuildTeamFightMember>(FightTeams, KTGlobal.GetRandom());

            int idx = 0;
            /// Cho từng cặp một vào
            while (idx + 1 < FightTeams.Count)
            {
                /// Thành viên đội 1
                GuildTeamFightMember firstPlayerTeam = FightTeams[idx];
                /// Thành viên đội 2
                GuildTeamFightMember secondPlayerTeam = FightTeams[idx + 1];

                ELIMINATION_INFO _INFO = new ELIMINATION_INFO();
                _INFO.Team_1 = firstPlayerTeam;
                _INFO.Team_2 = secondPlayerTeam;
                _INFO._ROUNDSTATE = ROUNDSTATE.NONE;
                _INFO.ROUNDID = GLOBALROUNDID;
                _INFO.WinThisRound = null;

                LogManager.WriteLog(LogTypes.WarCity, "CREATE TEAM FIGHT [" + GLOBALROUNDID + "] " + firstPlayerTeam.GuildName + " VS " + secondPlayerTeam.GuildName);

                // Add vào list để quản lý
                ELIMINATION_TOTAL.Add(_INFO);

                string Notify = "Vòng thi đấu thứ <color=red>" + GLOBALROUNDID + "</color> : <color=green>" + firstPlayerTeam.GuildName + "</color><color=red> VS </color><color=green>" + secondPlayerTeam.GuildName + "</color>";

                SendMSGForAllTeamFight(Notify);
                // Gửi chát cho cả 2 bang
                //KTGlobal.SendGuildChat(firstPlayerTeam.GuildID, Notify, null, "");
                //KTGlobal.SendGuildChat(secondPlayerTeam.GuildID, Notify, null, "");

                /// Tăng id
                idx += 2;
            }

            if (FightTeams.Count % 2 == 1)
            {
                /// Thành viên đội may mắn
                GuildTeamFightMember playerTeam = FightTeams[FightTeams.Count - 1];
                /// Thông tin nhóm
                ELIMINATION_INFO _INFO = new ELIMINATION_INFO();
                _INFO.Team_1 = playerTeam;
                _INFO.Team_2 = null;
                _INFO._ROUNDSTATE = ROUNDSTATE.END;
                // Đánh dấu là team này đã thắng đợt này
                _INFO.ROUNDID = GLOBALROUNDID;
                //Set luôn thằng này thắng
                _INFO.WinThisRound = playerTeam;
                // Add vào list để quản lý
                ELIMINATION_TOTAL.Add(_INFO);

                /// Thông báo
                string msg = "Chúc mừng chiến đội <color=green>" + playerTeam.GuildName + "</color> may mắn lẻ khỏi danh sách, tự động thắng trận!";

                SendMSGForAllTeamFight(msg);

                //KTGlobal.SendGuildChat(playerTeam.GuildID, msg, null, "");

                /// Ghi log
                LogManager.WriteLog(LogTypes.WarCity, string.Format("AUTO PICK TEAM {0} WIN BATTLE", playerTeam.GuildName));
            }
            // Call sự kiện start fight sau khi sắp xếp đội xong
            StartFight();
            //
        }

        #endregion SORTSOLOFIGHT

        #region GmCommand

        public bool WaitStartTeamFightByGmCommand = false;

        public void ForceStartTeamFight()
        {
            WaitStartTeamFightByGmCommand = true;
        }

        public void RegisterTeamFightByGmCommand(KPlayer client)
        {
            if (client.GuildID <= 0)
            {
                return;
            }

            string cmdData = client.GuildID + ":" + client.RoleID + ":" + client.RoleID + ":" + client.RoleID + ":" + client.RoleID + ":" + client.RoleID + ":" + client.RoleID;

            string[] fields = cmdData.Split(':');

            // 6 thằng thành viên gửi lên để đăng ký thi đấu
            if (fields.Length != 7)
            {
                KTPlayerManager.ShowMessageBox(client, "TOÁC", "THAM SỐ GỬI LÊN LỖI");
                return;
            }
            var Find = GuildManager.getInstance()._GetInfoGuildByGuildID(client.GuildID);
            if (Find != null)
            {
                if (Find.IsMainCity == (int)GUILD_CITY_STATUS.REGISTERFIGHT)
                {
                    KTPlayerManager.ShowNotification(client, "Bạn đã đăng ký thi đấu rồi không thể đăng ký tiếp");

                    return;
                }

                if (Find.IsMainCity == (int)GUILD_CITY_STATUS.HOSTCITY)
                {
                    KTPlayerManager.ShowNotification(client, "Chủ thành thì không thể đăng ký công thành");

                    return;
                }
            }
            else
            {
                KTPlayerManager.ShowNotification(client, "Có lỗi khi thi lấy dữ liệu bang hội");

                return;
            }

            if (!GuildWarCity.getInstance().IsTimeRegisterFight())
            {
                KTPlayerManager.ShowNotification(client, "Đây không phải thời gian đăng ký lôi đài");
                return;
            }

            byte[] ByteSendToDB = Encoding.ASCII.GetBytes(cmdData);
            byte[] bytesData = null;

            if (TCPProcessCmdResults.RESULT_FAILED == Global.ReadDataFromDb((int)TCPGameServerCmds.CMD_KT_GUILD_WAR_REGISTER, ByteSendToDB, ByteSendToDB.Length, out bytesData, client.ServerId))
            {
                KTPlayerManager.ShowNotification(client, "Không kết nối được với DBServer");

                return;
            }

            Int32 length = BitConverter.ToInt32(bytesData, 0);
            string strData = new UTF8Encoding().GetString(bytesData, 6, length - 2);

            string[] Pram = strData.Split(':');

            int Status = Int32.Parse(Pram[0]);
            // Nếu như status = 0 tức là insert thành công
            if (Status == 0)
            {
                int WEEKID = TimeUtil.GetIso8601WeekOfYear(DateTime.Now);
                // Set thằng này đã đăng ký thi đấu
                Find.IsMainCity = 2;
                if (Find.GuildWar == null)
                {
                    Find.GuildWar = new GuildWar();
                }
                Find.GuildWar.TeamList = Pram[2];
                Find.GuildWar.GuildName = Find.GuildName;

                Find.GuildWar.WeekID = WEEKID;
                Find.GuildWar.GuildID = Find.GuildId;

                KTPlayerManager.ShowMessageBox(client, "Thông báo", "Đăng ký thi đấu lôi đài cho bang hội thành công!");

                return;
            }
            else
            {
                KTPlayerManager.ShowMessageBox(client, "Thông báo", "Có lỗi khi đăng ký lôi đài công thành chiến");
                return;
            }
        }

        #endregion GmCommand

        /// <summary>
        /// Xử lý các sự kiện liên quan tới ngày lôi đàia
        /// </summary>
        public void ProseccFightEvent()
        {
            // Nếu đéo có bản đồ nào xảy ra war hôm nay thì thôi
            if (ActiveWar == null)
            {
                return;
            }

            if (FIGHT_STATE == SOLOFIGHT_STATE.FIGHT_NULL)
            {
                DateTime Now = DateTime.Now;

                // Nếu nay là ngày vã nhau
                if (Now.Hour == _WarConfig.OpenTime.Hours && Now.Minute == _WarConfig.OpenTime.Minute && FIGHT_STATE == SOLOFIGHT_STATE.FIGHT_NULL || (WaitStartTeamFightByGmCommand && FIGHT_STATE == SOLOFIGHT_STATE.FIGHT_NULL))
                {
                    WaitStartTeamFightByGmCommand = false;

                    if (FightTeams == null)
                    {
                        // Nếu đéo có team nào đăng ký
                        FIGHT_STATE = SOLOFIGHT_STATE.FIGHT_CLEAR;

                        LogManager.WriteLog(LogTypes.WarCity, "[" + ActiveWar.CityName + "] Tuần này không có bang nào đăng ký công thành " + _STATE.ToString());

                        var FindHost = GuildManager.getInstance().GetGuildWinHostCity();
                        if (FindHost != null)
                        {
                            //Thông báo tới toàn máy chủ không có sự kiện công thành xảy ra
                            KTGlobal.SendSystemEventNotification("Bang <color=red>" + FindHost.GuildName + "</color> tiếp tục là thành chủ của <color=green>" + ActiveWar.CityName + " trong tuần này!</color>");
                        }
                        else
                        {
                            KTGlobal.SendSystemEventNotification("Tuần này không có bang nào đăng ký công thành!Sự kiện công thành chiến sẽ được rời sang tuần tiếp theo!");
                        }
                    }
                    else
                    {
                        if (FightTeams.Count() == 0)
                        {
                            // Nếu đéo có team nào đăng ký
                            FIGHT_STATE = SOLOFIGHT_STATE.FIGHT_CLEAR;

                            LogManager.WriteLog(LogTypes.WarCity, "[" + ActiveWar.CityName + "] Tuần này không có bang nào đăng ký công thành " + _STATE.ToString());

                            var FindHost = GuildManager.getInstance().GetGuildWinHostCity();
                            if (FindHost != null)
                            {
                                //Thông báo tới toàn máy chủ không có sự kiện công thành xảy ra
                                KTGlobal.SendSystemEventNotification("Bang <color=red>" + FindHost.GuildName + "</color> tiếp tục là thành chủ của <color=green>" + ActiveWar.CityName + " trong tuần này!</color>");
                            }
                            else
                            {
                                KTGlobal.SendSystemEventNotification("Tuần này không có bang nào đăng ký công thành!Sự kiện công thành chiến sẽ được rời sang tuần tiếp theo!");
                            }
                        }
                        else if (FightTeams.Count() == 1) // Nếu chỉ có 1 bang đăng ký ==> nó nghiễm nhiên là thằng sẽ công vào ngày mai
                        {
                            int GuildID = FightTeams[0].GuildID;
                            // Set cho thằng này là bang chiến thắng
                            if (GuildManager.getInstance().UpdateCityStatus(GuildID, (int)GUILD_CITY_STATUS.ATTACKCITY))
                            {
                                var FindHost = GuildManager.getInstance().GetGuildWinHostCity();
                                if (FindHost != null)
                                {
                                    //Thông báo tới toàn máy chủ không có sự kiện công thành xảy ra
                                    KTGlobal.SendSystemEventNotification("Chỉ có một bang là <color=red>" + FightTeams[0].GuildName + "</color> đăng ký công thành.Nên bang <color=red>" + FightTeams[0].GuildName + "</color> nghiễm nhiên trở thành bang sẽ công thành vào ngày hôm sau!");

                                    KTGlobal.SendGuildChat(FightTeams[0].GuildID, "Bang của bạn sẽ là bang dành quyền tham gia công thành chiến vào ngày hôm sau!", null, "");
                                }
                                else
                                {
                                    KTGlobal.SendSystemEventNotification("Bang <color=red>" + FightTeams[0].GuildName + "</color> sẽ là bang đầu tiên khai mở Công Thành Chiến vào ngày hôm sau!");

                                    KTGlobal.SendGuildChat(FightTeams[0].GuildID, "Bang của bạn sẽ là bang đầu tiên khai mở công thành chiến vào ngày hôm sau!", null, "");
                                }

                                // Chuyển về trạng thái clear để dọn rác
                                FIGHT_STATE = SOLOFIGHT_STATE.FIGHT_CLEAR;
                            }
                            else
                            {
                                LogManager.WriteLog(LogTypes.WarCity, "[" + ActiveWar.CityName + "] BUG WHEN SET ATTACK STATUS ==> " + _STATE.ToString());
                            }
                        }
                        else
                        {
                            // thực hiện tick tiếp sự kiện
                            LastFightTick = TimeUtil.NOW();
                            //Chuyển sang chế độ chuẩn bị bắt đầu
                            FIGHT_STATE = SOLOFIGHT_STATE.FIGHT_PREPARSTART;

                            LogManager.WriteLog(LogTypes.WarCity, "[" + ActiveWar.CityName + "] SoloFight Change State ==> " + _STATE.ToString());

                            KTGlobal.SendSystemEventNotification("Hoạt động lôi đài tranh đấu công thành chuẩn bị bắt đầu,sự kiện sẽ bắt đầu sau 5 phút");
                        }
                    }
                }
            }
            else if (FIGHT_STATE == SOLOFIGHT_STATE.FIGHT_PREPARSTART)
            {
                // Cứ 5s lại push về client cái thông báo chuẩn bị chiến đấu
                if (TimeUtil.NOW() - LastPushNotifyTick >= 60 * 1000 && FIGHT_STATE == SOLOFIGHT_STATE.FIGHT_PREPARSTART)
                {
                    foreach (GuildTeamFightMember _Team in FightTeams)
                    {
                        string Notify = "";

                        long SEC = (LastFightTick + PRE_FIGHT_DULATION * 1000) - TimeUtil.NOW();

                        int FinalSec = (int)(SEC / 1000);

                        if (FinalSec > 60)
                        {
                            string TEAMMEMBER = "";

                            string[] TeamMeber = _Team.TeamList.Split('_');

                            foreach (string Member in TeamMeber)
                            {
                                string[] MemberPram = Member.Split('|');

                                TEAMMEMBER += "<b><color=yellow>" + MemberPram[1] + "</color></b><br>";
                            }

                            Notify = "Sự kiện <color=red>Thi Đấu Lôi Đài</color> sẽ bắt đầu sau <color=red>" + (FinalSec / 60) + " phút các thành viên có trong danh sách sau :<br>" + TEAMMEMBER + "Hãy chuẩn bị sẵn sàng thi đấu</color>";
                            KTGlobal.SendGuildChat(_Team.GuildID, Notify, null, "");
                        }
                    }
                    // Gửi thôn báo j đó cho kênh bang
                    LastPushNotifyTick = TimeUtil.NOW();
                }

                // Check để chuyển sang trạng thái bắt đầu
                if (TimeUtil.NOW() >= LastFightTick + this.PRE_FIGHT_DULATION * 1000 && FIGHT_STATE == SOLOFIGHT_STATE.FIGHT_PREPARSTART)
                {
                    LastFightTick = TimeUtil.NOW();

                    // CHUYỂN VÀO TRẬN
                    GLOBALROUNDID = 1;

                    // thực hiện reload các player có thể tham gia
                    ReloadPlayerMember();

                    // CHUYỂN SANG TRẠNG THÁI VÃ NHAU
                    FIGHT_STATE = SOLOFIGHT_STATE.FIGHT_ELIMINATION_ROUND;

                    //truyền tất cả vào tạo bảng thi đấu đầu tiên
                    CreateSoloEvent(FightTeams);

                    // Nếu tuần này không có team nào công thành
                    if (FightTeams.Count() == 0)
                    {
                        LogManager.WriteLog(LogTypes.WarCity, "[" + ActiveWar.CityName + "] Tuần này không có bang nào đăng ký công thành " + _STATE.ToString());
                    }

                    LogManager.WriteLog(LogTypes.WarCity, "[" + ActiveWar.CityName + "] SoloFight Change State ==> " + _STATE.ToString());
                }
            }
            else if (FIGHT_STATE == SOLOFIGHT_STATE.FIGHT_ELIMINATION_ROUND)
            {
                if (TimeUtil.NOW() - LastPushNotifyTick >= 60 * 1000 && FIGHT_STATE == SOLOFIGHT_STATE.FIGHT_ELIMINATION_ROUND)
                {
                    List<ELIMINATION_INFO> ALLTEAMFIGHTS = ELIMINATION_TOTAL.Where(x => x.ROUNDID == GLOBALROUNDID).ToList();

                    // Lấy ra toàn bộ team thắng cuộc
                    List<ELIMINATION_INFO> ALLWINCURRENTROUD = ELIMINATION_TOTAL.Where(x => x.ROUNDID == GLOBALROUNDID && x.WinThisRound != null).ToList();

                    if (ALLTEAMFIGHTS.Count == ALLWINCURRENTROUD.Count)
                    {
                        string Notify = "Vòng thì đấu <color=red>" + GLOBALROUNDID + "</color> đã hoàn thành!</color>";

                        SendMSGForAllTeamFight(Notify);

                        LastFightTick = TimeUtil.NOW();

                        // Chuyển sang trạng thái chờ
                        FIGHT_STATE = SOLOFIGHT_STATE.FREE_TIME;

                        LogManager.WriteLog(LogTypes.WarCity, "[" + ActiveWar.CityName + "] SoloFight Change State ==> " + _STATE.ToString());
                    }
                    else
                    {
                        foreach (GuildTeamFightMember _Team in FightTeams)
                        {
                            string Notify = "";

                            long SEC = (LastFightTick + FIGHT_DULATION_TIME * 1000) - TimeUtil.NOW();

                            int FinalSec = (int)(SEC / 1000);

                            if (FinalSec > 60)
                            {
                                Notify = "Vòng thì đấu <color=red>" + GLOBALROUNDID + "</color> sẽ kết thúc sau <color=red>" + (FinalSec / 60) + " phút!</color>";
                                KTGlobal.SendGuildChat(_Team.GuildID, Notify, null, "");
                            }
                        }
                        // Gửi thôn báo j đó cho kênh bang
                    }

                    LastPushNotifyTick = TimeUtil.NOW();
                }
                // Đợi cho các lôi đài thi đấu xong
                if (TimeUtil.NOW() >= LastFightTick + this.FIGHT_DULATION_TIME * 1000 && FIGHT_STATE == SOLOFIGHT_STATE.FIGHT_ELIMINATION_ROUND)
                {
                    LastFightTick = TimeUtil.NOW();

                    // Chuyển sang trạng thái chờ
                    FIGHT_STATE = SOLOFIGHT_STATE.FREE_TIME;

                    LogManager.WriteLog(LogTypes.WarCity, "[" + ActiveWar.CityName + "] SoloFight Change State ==> " + _STATE.ToString());
                }
            }
            else if (FIGHT_STATE == SOLOFIGHT_STATE.FREE_TIME)
            {
                if (TimeUtil.NOW() - LastPushNotifyTick >= 5 * 1000 && FIGHT_STATE == SOLOFIGHT_STATE.FREE_TIME)
                {
                    foreach (GuildTeamFightMember _Team in FightTeams)
                    {
                        string Notify = "";

                        long SEC = (LastFightTick + PRE_FREE_DULATION * 1000) - TimeUtil.NOW();

                        int FinalSec = (int)(SEC / 1000);

                        if (FinalSec > 1)
                        {
                            Notify = "Vòng thì đấu <color=red>" + GLOBALROUNDID + "</color> đã kết thúc! Vòng thi đấu tiếp theo sẽ bắt đầu sau : <color=red>" + FinalSec + " giây!</color>";
                            KTGlobal.SendGuildChat(_Team.GuildID, Notify, null, "");
                        }
                    }
                    // Gửi thôn báo j đó cho kênh bang
                    LastPushNotifyTick = TimeUtil.NOW();
                }
                // Đợi cho các lôi đài thi đấu xong
                if (TimeUtil.NOW() >= LastFightTick + this.PRE_FREE_DULATION * 1000 && FIGHT_STATE == SOLOFIGHT_STATE.FREE_TIME)
                {
                    // Set tick = tick
                    LastFightTick = TimeUtil.NOW();

                    GuildTeamFightMember _TeamGlobal = this.GetFinalTeamVictory();

                    if (_TeamGlobal != null)
                    {
                        int GuildID = _TeamGlobal.GuildID;
                        // Set cho thằng này là bang chiến thắng
                        if (GuildManager.getInstance().UpdateCityStatus(GuildID, (int)GUILD_CITY_STATUS.ATTACKCITY))
                        {
                            var FindHost = GuildManager.getInstance().GetGuildWinHostCity();
                            if (FindHost != null)
                            {
                                //Thông báo tới toàn máy chủ không có sự kiện công thành xảy ra
                                KTGlobal.SendSystemEventNotification("Trong sự kiện Lôi Đài để dành quyền Công Thành đợt này bang <color=red>" + _TeamGlobal.GuildName + "</color> đã dành chiến thắng.Bang <color=blue>" + FindHost.GuildName + "</color> sẽ là bang thủ thành.Bang <color=red>" + _TeamGlobal.GuildName + "</color> sẽ là bang công thành!");

                                KTGlobal.SendGuildChat(_TeamGlobal.GuildID, "Trong sự kiện Lôi Đài để dành quyền Công Thành đợt này bang <color=red>" + _TeamGlobal.GuildName + "</color> đã dành chiến thắng.Bang <color=blue>" + FindHost.GuildName + "</color> sẽ là bang thủ thành.Bang <color=red>" + _TeamGlobal.GuildName + "</color> sẽ là bang công thành!", null, "");
                            }
                            else
                            {
                                KTGlobal.SendSystemEventNotification("Trong sự kiện lôi đài để dành quyền Công Thành đợt này bang <color=red>" + _TeamGlobal.GuildName + "</color> đã dành chiến thắng");

                                KTGlobal.SendGuildChat(_TeamGlobal.GuildID, "Trong sự kiện lôi đài để dành quyền Công Thành đợt này bang <color=red>" + _TeamGlobal.GuildName + "</color> đã dành chiến thắng", null, "");
                            }

                            // Chuyển về trạng thái clear để dọn rác
                            FIGHT_STATE = SOLOFIGHT_STATE.FIGHT_CLEAR;
                        }
                        else
                        {
                            LogManager.WriteLog(LogTypes.WarCity, "[" + ActiveWar.CityName + "] BUG WHEN SET ATTACK STATUS ==> " + _STATE.ToString());
                        }
                    } // Nếu đây đéo phải round cuối cùng
                    else
                    {
                        // Reload lại người chơi cho những thằng online muộn có cớ hội tham gia lôi đaià
                        this.ReloadPlayerMember();

                        // Lấy ra toàn bộ team thắng cuộc
                        List<ELIMINATION_INFO> ALLWINCURRENTROUD = ELIMINATION_TOTAL.Where(x => x.ROUNDID == GLOBALROUNDID && x.WinThisRound != null).ToList();

                        if (ALLWINCURRENTROUD.Count > 0)
                        {
                            // Tạo ra cái list đút vào kia cho sắp đội tiếp
                            List<GuildTeamFightMember> ListWinThisRound = new List<GuildTeamFightMember>();

                            foreach (ELIMINATION_INFO _ELMENT in ALLWINCURRENTROUD)
                            {
                                // Add toàn bộ bọn thắng vào danh sách
                                ListWinThisRound.Add(_ELMENT.WinThisRound);
                            }

                            //Chuyển status sang thi đấu tiếp
                            FIGHT_STATE = SOLOFIGHT_STATE.FIGHT_ELIMINATION_ROUND;
                            // Sau khi đã có danh sách thắng
                            // Tăng hiệp này lên 1
                            GLOBALROUNDID++;

                            // Nếu như chỉ còn duy nhất 1 team chiến thắng vào thì sẽ để nó làm team thắng luôn
                            if (ListWinThisRound.Count == 1)
                            {
                                // SET NÓ LÀ THẰNG ĐẦU TIÊN
                                _TeamGlobal = ListWinThisRound[0];

                                int GuildID = _TeamGlobal.GuildID;
                                // Set cho thằng này là bang chiến thắng
                                if (GuildManager.getInstance().UpdateCityStatus(GuildID, (int)GUILD_CITY_STATUS.ATTACKCITY))
                                {
                                    var FindHost = GuildManager.getInstance().GetGuildWinHostCity();
                                    if (FindHost != null)
                                    {
                                        //Thông báo tới toàn máy chủ không có sự kiện công thành xảy ra
                                        KTGlobal.SendSystemEventNotification("Trong sự kiện Lôi Đài để dành quyền Công Thành đợt này bang <color=red>" + _TeamGlobal.GuildName + "</color> đã dành chiến thắng.Bang <color=blue>" + FindHost.GuildName + "</color> sẽ là bang thủ thành.Bang <color=red>" + _TeamGlobal.GuildName + "</color> sẽ là bang công thành!");

                                        KTGlobal.SendGuildChat(_TeamGlobal.GuildID, "Trong sự kiện Lôi Đài để dành quyền Công Thành đợt này bang <color=red>" + _TeamGlobal.GuildName + "</color> đã dành chiến thắng.Bang <color=blue>" + FindHost.GuildName + "</color> sẽ là bang thủ thành.Bang <color=red>" + _TeamGlobal.GuildName + "</color> sẽ là bang công thành!", null, "");
                                    }
                                    else
                                    {
                                        KTGlobal.SendSystemEventNotification("Trong sự kiện lôi đài để dành quyền Công Thành đợt này bang <color=red>" + _TeamGlobal.GuildName + "</color> đã dành chiến thắng");

                                        KTGlobal.SendGuildChat(_TeamGlobal.GuildID, "Trong sự kiện lôi đài để dành quyền Công Thành đợt này bang <color=red>" + _TeamGlobal.GuildName + "</color> đã dành chiến thắng", null, "");
                                    }

                                    // Chuyển về trạng thái clear để dọn rác
                                    FIGHT_STATE = SOLOFIGHT_STATE.FIGHT_CLEAR;
                                }
                                else
                                {
                                    LogManager.WriteLog(LogTypes.WarCity, "[" + ActiveWar.CityName + "] BUG WHEN SET ATTACK STATUS ==> " + _STATE.ToString());
                                }
                            }
                            else
                            {
                                // Thực hiện sự kiện cho nó vã nhau tiếp tới bao giờ chọn ra thằng thắng thì thôi
                                CreateSoloEvent(ListWinThisRound);
                            }
                        }
                        else
                        {
                            LogManager.WriteLog(LogTypes.WarCity, "[" + ActiveWar.CityName + "] BUG WTF......CHECK LÔI ĐÀI TẠI SAO KO TRẢ KẾT QUẢ ĐÚNG GIỜ ==> " + _STATE.ToString());
                        }
                    }

                    LogManager.WriteLog(LogTypes.WarCity, "[" + ActiveWar.CityName + "] SoloFight Change State ==> " + _STATE.ToString());
                }
            }
            // Nếu có bản đồ sẽ xảy ra solo day ngày hôm nay
        }
    }
}