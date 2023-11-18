using GameServer.Core.Executor;
using GameServer.Core.GameEvent;
using GameServer.Core.GameEvent.EventOjectImpl;
using GameServer.KiemThe.CopySceneEvents;
using GameServer.KiemThe.Core.Activity.CardMonth;
using GameServer.KiemThe.Core.Repute;
using GameServer.KiemThe.Entities;
using GameServer.KiemThe.GameDbController;
using GameServer.KiemThe.GameEvents.FactionBattle;
using GameServer.KiemThe.GameEvents.TeamBattle;
using GameServer.KiemThe.Logic;
using GameServer.Logic;
using GameServer.Server;
using GameServer.VLTK.Core.GuildManager;
using Server.Data;
using Server.Protocol;
using Server.TCP;
using Server.Tools;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace GameServer.KiemThe
{
    /// <summary>
    /// Các phương thức và đối tượng toàn cục của Kiếm Thế
    /// </summary>
    public static partial class KTGlobal
    {
        #region RoleLoginPram

        /// <summary>
        /// LOAD RA CHỈ SỐ KHI NHÂN VẬT LOGIN GAME
        /// </summary>
        /// <param name="client"></param>
        public static void InitRoleLoginPrams(KPlayer client)
        {
            string LifeSkill = Global.GetRoleParamsStringWithDB(client, RoleParamName.LifeSkill);

            if (LifeSkill != null)
            {
                if (LifeSkill.Length > 0)
                {
                    try
                    {
                        byte[] Base64Decode = Convert.FromBase64String(LifeSkill);

                        Dictionary<int, LifeSkillPram> _LifeSkillData = DataHelper.BytesToObject<Dictionary<int, LifeSkillPram>>(Base64Decode, 0, Base64Decode.Length);

                        client.SetLifeSkills(_LifeSkillData);
                    }
                    catch (Exception ex)
                    {
                        LogManager.WriteLog(LogTypes.Exception, "Có lỗi khi khởi tạo lại LIFESKILL :" + ex.ToString());
                        Dictionary<int, LifeSkillPram> _LifeSkillData = new Dictionary<int, LifeSkillPram>();

                        for (int i = 1; i < 12; i++)
                        {
                            LifeSkillPram _LifeSkill = new LifeSkillPram();
                            _LifeSkill.LifeSkillID = i;
                            _LifeSkill.LifeSkillLevel = 1;
                            _LifeSkill.LifeSkillExp = 0;

                            _LifeSkillData.Add(i, _LifeSkill);
                        }

                        client.SetLifeSkills(_LifeSkillData);
                    }
                }
            }
            else
            {
                Dictionary<int, LifeSkillPram> _LifeSkillData = new Dictionary<int, LifeSkillPram>();

                for (int i = 1; i < 12; i++)
                {
                    LifeSkillPram _LifeSkill = new LifeSkillPram();
                    _LifeSkill.LifeSkillID = i;
                    _LifeSkill.LifeSkillLevel = 1;
                    _LifeSkill.LifeSkillExp = 0;

                    _LifeSkillData.Add(i, _LifeSkill);
                }

                client.SetLifeSkills(_LifeSkillData);
            }

            string ReputeInfoStr = Global.GetRoleParamsStringWithDB(client, RoleParamName.ReputeInfo);

            if (ReputeInfoStr != null)
            {
                if (ReputeInfoStr.Length > 0)
                {
                    try
                    {
                        byte[] Base64Decode = Convert.FromBase64String(ReputeInfoStr);

                        List<ReputeInfo> _ReputeInfo = DataHelper.BytesToObject<List<ReputeInfo>>(Base64Decode, 0, Base64Decode.Length);

                        // Thêm code này để fix những camp chưa có của nhân vật
                        foreach (Camp _Camp in ReputeManager._ReputeConfig.Camp)
                        {
                            foreach (Class _Class in _Camp.Class)
                            {
                                int DBID = _Camp.Id * 100 + _Class.Id;

                                var FinExist = _ReputeInfo.Where(x => x.DBID == DBID).FirstOrDefault();
                                if (FinExist == null)
                                {
                                    int Level = 1;
                                    int Exp = 0;

                                    ReputeInfo _Info = new ReputeInfo();

                                    _Info.DBID = DBID;
                                    _Info.Level = Level;
                                    _Info.Exp = Exp;

                                    _ReputeInfo.Add(_Info);
                                }
                            }
                        }

                        client.SetReputeInfo(_ReputeInfo);
                    }
                    catch (Exception ex)
                    {
                        LogManager.WriteLog(LogTypes.Exception, "Có lỗi khi khởi tạo lại ReputeInfoStr :" + ex.ToString());

                        List<ReputeInfo> _ReputeInfo = new List<ReputeInfo>();

                        foreach (Camp _Camp in ReputeManager._ReputeConfig.Camp)
                        {
                            foreach (Class _Class in _Camp.Class)
                            {
                                int DBID = _Camp.Id * 100 + _Class.Id;
                                int Level = 1;
                                int Exp = 0;

                                ReputeInfo _Info = new ReputeInfo();

                                _Info.DBID = DBID;
                                _Info.Level = Level;
                                _Info.Exp = Exp;

                                _ReputeInfo.Add(_Info);
                            }
                        }

                        client.SetReputeInfo(_ReputeInfo);
                    }
                }
            }
            else
            {
                List<ReputeInfo> _ReputeInfo = new List<ReputeInfo>();

                foreach (Camp _Camp in ReputeManager._ReputeConfig.Camp)
                {
                    foreach (Class _Class in _Camp.Class)
                    {
                        int DBID = _Camp.Id * 100 + _Class.Id;
                        int Level = 1;
                        int Exp = 0;

                        ReputeInfo _Info = new ReputeInfo();

                        _Info.DBID = DBID;
                        _Info.Level = Level;
                        _Info.Exp = Exp;

                        _ReputeInfo.Add(_Info);
                    }
                }

                client.SetReputeInfo(_ReputeInfo);
            }

            /// Chuỗi mã hóa danh hiệu
            string roleTitlesInfo = Global.GetRoleParamsStringWithDB(client, RoleParamName.RoleTitles);
            try
            {
                /// Nếu toác
                if (roleTitlesInfo == null)
                {
                    throw new Exception();
                }

                string[] fields = roleTitlesInfo.Split('|');
                int currentTitleID = int.Parse(fields[0]);
                ConcurrentDictionary<int, int> titles = new ConcurrentDictionary<int, int>();
                for (int i = 1; i < fields.Length; i++)
                {
                    string[] data = fields[i].Split('_');
                    int titleID = int.Parse(data[0]);
                    int startTime = int.Parse(data[1]);
                    titles[titleID] = startTime;
                }
                /// ID danh hiệu hiện tại
                client.CurrentRoleTitleID = currentTitleID;
                /// Danh sách danh hiệu hiện tại
                client.RoleTitles = titles;
            }
            catch (Exception ex)
            {
                client.RoleTitles = new ConcurrentDictionary<int, int>();
                client.CurrentRoleTitleID = -1;
            }

            //SET LẠI CẤP ĐỘ CHO THỰC THỂ
            client.m_Level = client.GetRoleData().Level;

            //SET Lại Exp Hiện tại Cho CLient
            client.m_Experience = client.GetRoleData().Experience;

            // Đánh dấu lại thời gian trừ pK
            client.LastSiteSubPKPointTicks = KTGlobal.GetCurrentTimeMilis();

            lock (client.PropPointMutex)
            {
                /// Load base của nhân vật trước tiên
                KTAttributesModifier.LoadRoleBaseAttributes(client);

                /// Tổng điểm tiềm năng được cộng thêm từ các loại bánh đã ăn
                int additionRemainPoint = Global.GetRoleParamsInt32FromDB(client, RoleParamName.TotalPropPoint);
                client.SetBonusRemainPotentialPoints(additionRemainPoint);

                /// Yổng điểm kỹ năng được cộng thêm từ các loại bánh đã ăn
                int additionSkillPoint = Global.GetRoleParamsInt32FromDB(client, RoleParamName.TotalSkillPoint);
                client.SetBonusSkillPoint(additionSkillPoint, false);

                /// Tổng điểm tiềm năng có từ Base
                int baseRemainPoint = client.GetBaseRemainPotentialPoints();

                // Console.WriteLine("Bonus Remain = " + additionRemainPoint + " - " + "Base Remain = " + baseRemainPoint + " - " + "Base SkillPoint = " + client.GetBaseSkillPoint());

                /// Thuộc tính từng chỉ số đã cộng
                int STR = Global.GetRoleParamsInt32FromDB(client, RoleParamName.sPropStrength);
                int ENER = Global.GetRoleParamsInt32FromDB(client, RoleParamName.sPropIntelligence);
                int DEX = Global.GetRoleParamsInt32FromDB(client, RoleParamName.sPropDexterity);
                int VIT = Global.GetRoleParamsInt32FromDB(client, RoleParamName.sPropConstitution);

                int totalRemainPoint = baseRemainPoint + additionRemainPoint - STR - ENER - DEX - VIT;

                /// Kiểm tra nếu có BUG thì tiến hành reset toàn bộ
                if (totalRemainPoint < 0)
                {
                    totalRemainPoint = baseRemainPoint + additionRemainPoint;
                    STR = 0;
                    ENER = 0;
                    DEX = 0;
                    VIT = 0;
                }

                // Kiểm tra xem điểm phân phối có đúng theo quyy tắc không

                if (!client.CheckAssignPotential(ref STR, ref DEX, ref VIT, ref ENER))
                {
                    client.UnAssignPotential();
                }

                client.ChangeStrength(STR);
                client.ChangeEnergy(ENER);
                client.ChangeDexterity(DEX);
                client.ChangeVitality(VIT);

                // TINH LỰC HOẠT LỰC
                int GatherPoint = Global.GetRoleParamsInt32FromDB(client, RoleParamName.GatherPoint);
                int MakePoint = Global.GetRoleParamsInt32FromDB(client, RoleParamName.MakePoint);

                // SET GIÁ TRỊ CHO TINH LỰC HOẠT LỰC
                if (GatherPoint > 0)
                {
                    client.ChangeCurGatherPoint(GatherPoint);
                }

                if (MakePoint > 0)
                {
                    client.ChangeCurMakePoint(MakePoint);
                }

                int nVerifyBuffProp = Global.GetRoleParamsInt32FromDB(client, RoleParamName.VerifyBuffProp);

                /// ĐOẠN NÀY LÀ ĐOẠN TẠM THỜI BYPASS ĐỂ CHO VÀO GAME
                //client.m_CurrentLife = client.m_CurrentLifeMax;
                //client.m_CurrentMana = client.m_CurrentManaMax;

                //client.m_CurrentStamina = client.m_CurrentStaminaMax;

                /// Trạng thái có cưỡi ngựa hay không
                int horseRidingParam = Global.GetRoleParamsInt32FromDB(client, RoleParamName.HorseToggleOn);
                client.IsRiding = horseRidingParam == 1;

                // Read Ra thông tin nhiệm vụ tuần hoàn

                if (client.OldTasks != null)
                {
                    var findmaxMain = client.OldTasks.Where(x => x.TaskClass == 0).ToList();

                    if (findmaxMain.Count > 0)
                    {
                        List<int> TaskInput = findmaxMain.Select(x => x.TaskID).ToList();

                        int LastMainTask = findmaxMain.Last().TaskID;

                        QuestInfo MainTask = new QuestInfo();
                        MainTask.TaskClass = 0;
                        MainTask.CurTaskIndex = LastMainTask;
                        client.AddQuestInfo(MainTask);
                    }
                    else
                    {
                        QuestInfo MainTask = new QuestInfo();
                        MainTask.TaskClass = 0;
                        MainTask.CurTaskIndex = -1;
                        client.AddQuestInfo(MainTask);
                    }
                }
                else
                {
                    QuestInfo MainTask = new QuestInfo();
                    MainTask.TaskClass = 0;
                    MainTask.CurTaskIndex = -1;
                    client.AddQuestInfo(MainTask);

                    QuestInfo SUBTASK = new QuestInfo();
                    SUBTASK.TaskClass = 5;
                    SUBTASK.CurTaskIndex = -1;
                    client.AddQuestInfo(SUBTASK);
                }

                // Đọc ra các ghi chép nhật ký theo ngày
                string DailyRecore = Global.GetRoleParamsStringWithDB(client, RoleParamName.DailyRecore);
                int DayID = DateTime.Now.DayOfYear;

                if (DailyRecore != null)
                {
                    if (DailyRecore.Length > 0)
                    {
                        try
                        {
                            byte[] Base64Decode = Convert.FromBase64String(DailyRecore);

                            DailyDataRecore _DailyDataRecore = DataHelper.BytesToObject<DailyDataRecore>(Base64Decode, 0, Base64Decode.Length);

                            client._DailyDataRecore = _DailyDataRecore;

                            if (_DailyDataRecore.DayID != DayID)
                            {
                                // Nếu như khác ngày thì reset hết dữ liệu luôn
                                DailyDataRecore _NewDailyDataRecore = new DailyDataRecore();
                                _NewDailyDataRecore.DayID = DayID;
                                _NewDailyDataRecore.EventRecoding = new Dictionary<int, int>();

                                client._DailyDataRecore = _NewDailyDataRecore;
                            }
                        }
                        catch (Exception ex)
                        {
                            DailyDataRecore _DailyDataRecore = new DailyDataRecore();
                            _DailyDataRecore.DayID = DayID;
                            _DailyDataRecore.EventRecoding = new Dictionary<int, int>();
                            client._DailyDataRecore = _DailyDataRecore;
                        }
                    }
                }
                else
                {
                    DailyDataRecore _DailyDataRecore = new DailyDataRecore();
                    _DailyDataRecore.DayID = DayID;
                    _DailyDataRecore.EventRecoding = new Dictionary<int, int>();
                    client._DailyDataRecore = _DailyDataRecore;
                }

                // Đọc ra các ghi chép nhật ký theo tuần
                string WeekRecore = Global.GetRoleParamsStringWithDB(client, RoleParamName.WeekRecore);
                int WeekID = TimeUtil.GetIso8601WeekOfYear(DateTime.Now);

                if (WeekRecore != null)
                {
                    if (WeekRecore.Length > 0)
                    {
                        try
                        {
                            byte[] Base64Decode = Convert.FromBase64String(WeekRecore);

                            WeekDataRecore _WeekDataRecore = DataHelper.BytesToObject<WeekDataRecore>(Base64Decode, 0, Base64Decode.Length);

                            client._WeekDataRecore = _WeekDataRecore;

                            if (_WeekDataRecore.WeekID != WeekID)
                            {
                                WeekDataRecore _NewWeekDataRecore = new WeekDataRecore();
                                _NewWeekDataRecore.WeekID = WeekID;
                                _NewWeekDataRecore.EventRecoding = new Dictionary<int, int>();

                                client._WeekDataRecore = _NewWeekDataRecore;
                            }
                        }
                        catch (Exception ex)
                        {
                            WeekDataRecore _WeekDataRecore = new WeekDataRecore();
                            _WeekDataRecore.WeekID = WeekID;
                            _WeekDataRecore.EventRecoding = new Dictionary<int, int>();
                            client._WeekDataRecore = _WeekDataRecore;
                        }
                    }
                }
                else
                {
                    WeekDataRecore _WeekDataRecore = new WeekDataRecore();
                    _WeekDataRecore.WeekID = WeekID;
                    _WeekDataRecore.EventRecoding = new Dictionary<int, int>();
                    client._WeekDataRecore = _WeekDataRecore;
                }

                //int ReviceFistLoginReward = client.GetValueOfWeekRecore(111111);
                //if (ReviceFistLoginReward == -1)
                //{
                //    SubRep _REP = KTGlobal.AddMoney(client, 1000000, MoneyType.Dong, "NEWBIE");
                //    if (_REP.IsOK)
                //    {
                //        client.SetValueOfWeekRecore(111111, 1);
                //    }
                //}

                ///Bản ghi vĩnh viễn
                string ForeverRecore = Global.GetRoleParamsStringWithDB(client, RoleParamName.ForeverRecore);
                if (ForeverRecore != null)
                {
                    if (ForeverRecore.Length > 0)
                    {
                        try
                        {
                            byte[] Base64Decode = Convert.FromBase64String(ForeverRecore);

                            ForeverRecore _ForeverRecore = DataHelper.BytesToObject<ForeverRecore>(Base64Decode, 0, Base64Decode.Length);

                            client._ForeverRecore = _ForeverRecore;
                        }
                        catch (Exception ex)
                        {
                            ForeverRecore _ForeverRecore = new ForeverRecore();

                            _ForeverRecore.EventRecoding = new Dictionary<int, int>();
                            client._ForeverRecore = _ForeverRecore;
                        }
                    }
                    else
                    {
                        ForeverRecore _ForeverRecore = new ForeverRecore();
                        _ForeverRecore.EventRecoding = new Dictionary<int, int>();
                        client._ForeverRecore = _ForeverRecore;
                    }
                }
                else
                {
                    ForeverRecore _ForeverRecore = new ForeverRecore();
                    _ForeverRecore.EventRecoding = new Dictionary<int, int>();
                    client._ForeverRecore = _ForeverRecore;
                }

                // Lấy ra thời gian ủy thác bạch cầu hoàn
                client.baijuwan = Global.GetRoleParamsInt32FromDB(client, RoleParamName.MeditateTime);
                client.baijuwanpro = Global.GetRoleParamsInt32FromDB(client, RoleParamName.NotSafeMeditateTime);

                /// Nhận thưởng tải game hay chưa
                client.ReviceBounsDownload = Global.GetRoleParamsInt32FromDB(client, RoleParamName.TreasureJiFen);

                long LastOffLineTime = client.LastOfflineTime;

                long TimeNow = TimeUtil.NOW();

                long TotalOfflineTime = TimeNow - LastOffLineTime;

                int SECOFFLINE = (int)TotalOfflineTime / 1000;

                if (client.baijuwan > 0 || client.baijuwanpro > 0)
                {
                    if (SECOFFLINE < 0)
                    {
                        SECOFFLINE = 0;
                    }

                    if (SECOFFLINE > 0)
                    {
                        int TOTALOFFFMIN = 0;

                        // QUY ĐỔI RA PHÚT
                        int MIN = SECOFFLINE / 60;
                        TOTALOFFFMIN = MIN;
                        int TIMEPRO = 0;
                        int TIMENORMAL = 0;
                        if (client.baijuwanpro > 0)
                        {
                            // Nếu thời gian rời mạng > lớn hơn thời gian đại bạch cầu hoàn có

                            if (MIN >= client.baijuwanpro)
                            {
                                // Thời gian ủy thác đại bạch cầu hoàn = max số phút hiện có
                                TIMEPRO = client.baijuwanpro;
                                // Set thời gian ủy thác đại bạch cầu hoàn về 0
                                client.baijuwanpro = 0;

                                // Số phút còn lại để tính cho bạch cầu hoàn
                                MIN = MIN - TIMEPRO;
                            }
                            else
                            {
                                // Nếu số thời gian đại bạch cầu hoàn còn dư sức thì

                                TIMEPRO = MIN;

                                client.baijuwanpro = client.baijuwanpro - MIN;
                                // Số phút ủy thác về 0
                                MIN = 0;
                            }
                        }
                        // Nếu số phút của bạch cầu hoàn còn lớn hơn 0
                        if (client.baijuwan > 0 && MIN > 0)
                        {
                            if (MIN >= client.baijuwan)
                            {
                                // Thời gian ủy thác  bạch cầu hoàn = max số phút hiện có
                                TIMENORMAL = client.baijuwan;
                                // Set thời gian ủy thác  bạch cầu hoàn về 0
                                client.baijuwan = 0;
                            }
                            else
                            {
                                // Nếu số thời gian  bạch cầu hoàn còn dư sức thì

                                TIMENORMAL = MIN;
                                client.baijuwan = client.baijuwan - MIN;
                            }
                        }

                        int BASEEXPERN = KPlayerSetting.GetBaseExpLevel(client.m_Level);

                        double EXPPERSECON = (BASEEXPERN / 60);

                        double EXPGIAN = 0;

                        double EXPBCH = 0;

                        double EXPDAIBCH = 0;

                        if (TIMEPRO > 0)
                        {
                            EXPGIAN = (TIMEPRO * 60) * EXPPERSECON * 1.3;

                            EXPDAIBCH = EXPGIAN;
                        }

                        if (TIMENORMAL > 0)
                        {
                            EXPBCH = (TIMENORMAL * 60) * EXPPERSECON;
                            EXPGIAN += (TIMENORMAL * 60) * EXPPERSECON;
                        }

                        var ActivExpLoginMap = KTKTAsyncTask.Instance.ScheduleExecuteAsync(new DelayFuntionAsyncTask("ExpCallAddDelay", new Action(() => KTPlayerManager.NotifyBCH(client, EXPGIAN, EXPBCH, EXPDAIBCH, TIMEPRO, TIMENORMAL, TOTALOFFFMIN))), 10 * 1000);
                    }
                }

                /// Ghi lại thời điểm Online để tick ở BGWork
                client.LastUpdateOnlineTimeTicks = KTGlobal.GetCurrentTimeMilis();

                // Đăng nhập liên tục => cái này cần cho maketing phân tích sau này
                client.SeriesLoginNum = Global.GetRoleParamsInt32FromDB(client, RoleParamName.SeriesLoginCount);

                //DỮ LIỆU THẺ THÁNG
                client.YKDetail.ParseFrom(Global.GetRoleParamByName(client, RoleParamName.YueKaInfo));
            }

            /// ID pet trước đó
            int lastPetID = client.GetValueOfForeverRecore(ForeverRecord.CurrentPetID);
            /// Nếu tồn tại
            if (lastPetID != -1)
            {
                if (client.PetList != null)
                {
                    /// Thông tin pet
                    PetData petData = client.PetList.Where(x => x.ID == lastPetID).FirstOrDefault();
                    /// Nếu tồn tại
                    if (petData != null)
                    {
                        /// Gọi xuất chiến
                        client.CallPetImmediately(petData);
                    }
                }
            }

            var ExpCallAddDelay = KTKTAsyncTask.Instance.ScheduleExecuteAsync(new DelayFuntionAsyncTask("ExpCallAddDelay", new Action(() =>
            {
               // KTGlobal.SendSystemChatForPlayer(client, "<color=red>Trang chủ :</color> <color=green>https://vokiem.mobi/</color>\n<color=red>Trang quản lý tài khoản</color>: <color=green>https://id.vokiem.mobi/</color>\n<color=red>Nhóm Facebook :</color><color=green>https://www.facebook.com/groups/vokiem.mobi/</color>\n<color=red>Nhóm Telegram:</color><color=green> https://t.me/+vTo3eBLuoMkwOTI9</color>\n<color=red>Cảnh báo lừa đảo:Các hội nhóm nằm ngoài các đường dẫn kể trên đều không phải của nhà phát hành VÕ Kiếm,Hãy cẩn trọng trước khi truy cập tránh bị lừa đảo!\nTrong mọi trường hợp Admin sẽ không yêu cầu bạn cung cấp mật khẩu,hoặc mã OPT</color>");

                KT_TCPHandler.NotifySelfExperience(client);
                client.LastClickDialog = KTGlobal.GetCurrentTimeMilis();
                /// Thực hiện xếp lại túi đồ khi login
                client.GoodsData.SortBag(0);
            })), 10 * 1000);
            //GHI CHÉP LẠI THỜI GIAN LINE

            Global.ExecuteDBCmd((int)TCPGameServerCmds.CMD_DB_ROLE_ONLINE,
                   string.Format("{0}:{1}:{2}:{3}",
                   client.RoleID,
                   GameManager.ServerLineID,
                   0,
                   Global.GetSocketRemoteIP(client)), client.ServerId);


            if (client.GetValueOfForeverRecore(ForeverRecord.ResetTurnPlateMark) ==-1)
            {
                //Đánh dấu là đã reset rồi
                client.SetValueOfForeverRecore(ForeverRecord.ResetTurnPlateMark, 1);

                //Reset vòng quay may mắn
                client.SetValueOfForeverRecore(ForeverRecord.LuckyCircle_TotalTurn, 0);

                //Reset vòng quay đặc biệt
               // client.SetValueOfForeverRecore(ForeverRecord.TurnPlate_TotalTurn, 0);
            }    

        }

        #endregion RoleLoginPram

        #region RoleLogoutAction

        /// <summary>
        /// Thực hiện toàn bộ LOGIC xử lý khi nhân vật LOGOUT
        /// </summary>
        /// <param name="sl"></param>
        /// <param name="pool"></param>
        /// <param name="client"></param>
        public static void Logout(SocketListener sl, TCPOutPacketPool pool, KPlayer client)
        {
            try
            {
                /// Xóa khỏi danh sách theo địa chỉ IP
                GameManager.OnlineUserSession.RemoveClientFromIPAddressList(client);

                /// Ngừng StoryBoard tương ứng
                KTPlayerStoryBoardEx.Instance.Remove(client);

                /// Nếu đang ở trong bản đồ Liên đấu hội trường
                if (TeamBattle.IsInTeamBattleMap(client))
                {
                    /// Thực thi sự kiện rời bản đồ hội trường
                    TeamBattle_ActivityScript.OnPlayerLeave(client, null);
                }

                /// Rời nhóm hiện tại
                if (client.TeamID != -1 && KTTeamManager.IsTeamExist(client.TeamID))
                {
                    client.LeaveTeam();
                }

                //Call Global Event LOGOUT cho các classs khác biết nhân vật đang LOGOUT
                GlobalEventSource.getInstance().fireEvent(new PlayerLogoutEventObject(client));

                //FOCE GỬI LỆNH BẮT DB PHẢI LƯU TRỮ CÁC THÔNG TIN  | TIỀN | EXP |
                GameDb.ProcessDBCmdByTicks(client, true);

                LogManager.WriteLog(LogTypes.Logout, "[" + client.RoleID + "][" + client.RoleName + "][" + client.MapCode + "|" + client.PosX + "|" + client.PosY + "]  HOẠT LỰC :" + client.GetGatherPoint() + " | TINH LỰC :" + client.GetMakePoint() + "| BẠC :" + client.Money + "| BẠC KHÓA :" + client.BoundMoney + "| ĐỒNG  :" + client.Token + "| ĐỒNG KHÓA :" + client.BoundToken);

                /// Gọi hàm tương ứng
                client.OnQuitGame();

                /// Nếu đang trong phụ bản
                if (client.CopyMapID != -1 && CopySceneEventManager.IsCopySceneExist(client.CopyMapID, client.CurrentMapCode))
                {
                    /// Phụ bản tương ứng
                    KTCopyScene copyScene = CopySceneEventManager.GetCopyScene(client.CopyMapID, client.CurrentMapCode);
                    /// Thực thi sự kiện Disconnected
                    CopySceneEventManager.OnPlayerDisconnected(copyScene, client);
                }

                //LƯU LẠI ROLE DATA BY PRAMENTER
                GameDb.ProcessDBRoleParamCmdByTicks(client, true);

                //Thực Hiện Lệnh LƯU LẠI ĐỘ BỀN CỦA TOÀN BỘ TRANG BỊ
                GameDb.ProcessDBEquipStrongCmdByTicks(client, true);

                //Lưu dữ liệu các hoạt động trong ngày
                Global.WriterWelfare(client);

                // Gỡ item khỏi giỏ bán hàng
                if (!client.ClientSocket.IsKuaFuLogin && client.SaleGoodsDataList.Count > 0)
                {
                    SaleRoleManager.RemoveSaleRoleItem(client.RoleID);
                    SaleGoodsManager.RemoveSaleGoodsItems(client);
                }

                /// Nếu có pet đang tham chiến
                if (client.CurrentPet != null)
                {
                    /// Xóa pet
                    KTPetManager.RemovePet(client.CurrentPet);
                }

                /// Nếu đang có xe tiêu
                if (client.CurrentTraderCarriage != null)
                {
                    /// Xóa xe tiêu
                    KTTraderCarriageManager.RemoveTraderCarriage(client.CurrentTraderCarriage);
                }

                /// Cập nhật thời gian online

                Global.ExecuteDBCmd((int)TCPGameServerCmds.CMD_DB_UPDATEONLINETIME, string.Format("{0}:{1}:{2}", client.RoleID, client.TotalOnlineSecs, 0), client.ServerId);

                /// Cập nhật trị PK
                KT_TCPHandler.UpdatePKValueToDB(client);

                /// Xóa khỏi danh sách quản lý
                KTPlayerManager.Remove(client);

                //OnLogout
                FactionBattleManager.FactionLogout(client);

                //Xử lý giao dịch khi bị ngắt kết nối
                ProcessExchangeData(sl, pool, client);

                //Nếu mà máu hiện tại là chết ==> đưa về thành
                if (client.m_CurrentLife <= 0)
                {
                    /// Thông tin điểm hồi sinh
                    KT_TCPHandler.GetPlayerDefaultRelivePos(client, out int mapCode, out int posX, out int posY);
                    client.MapCode = mapCode;
                    client.PosX = posX;
                    client.PosY = posY;
                }

                //Ghi lại thông tin vị trí đang đứng

                Global.ExecuteDBCmd((int)TCPGameServerCmds.CMD_DB_UPDATE_POS,
                    string.Format("{0}:{1}:{2}:{3}:{4}", client.RoleID, client.MapCode,
                    client.RoleDirection, client.PosX, client.PosY),
                     client.ServerId);

                //Cập nhật trạng thái OFFLINE ở DB

                Global.ExecuteDBCmd((int)TCPGameServerCmds.CMD_DB_ROLE_OFFLINE,
                    string.Format("{0}:{1}:{2}:{3}:{4}",
                    client.RoleID,
                    GameManager.ServerLineID,
                    Global.GetSocketRemoteIP(client),
                    0,
                    TimeUtil.NOW()),
                    client.ServerId);

                //Ghi lại event logout ra files logs
                KTGlobal.AddRoleLogoutEvent(client);

                // Update các thuộc tính cho nhân vật
                UpdateRoleParamsInfo(client);

                try
                {
                    string ip = KTGlobal.GetIPAddress(client);

                    string analysisLog = string.Format("[LOGOUT] {0} (ID: {1}), IP: {2}, Device: {3} - {4} - {5}, Version: {6}", client.RoleName, client.RoleID, ip, client.DeviceType, client.DeviceModel, client.DeviceGeneration, client.ClientVersion);
                    LogManager.WriteLog(LogTypes.Analysis, analysisLog);
                }
                catch { }

                /// Đánh dấu đã rời mạng
                client.LogoutState = true;
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
                DataHelper.WriteFormatExceptionLog(ex, Global.GetDebugHelperInfo(client.ClientSocket), false);
            }
        }

        #endregion RoleLogoutAction

        #region UpdateRolePrams

        /// <summary>
        /// Lưu lại các PRAM TRƯỚC KHI THOÁT
        /// </summary>
        /// <param name="client"></param>
        public static void UpdateRoleParamsInfo(KPlayer client)
        {
            // Lưu lại giá trị máu và MP hiện tại

            if (client.m_CurrentLife < 0)
            {
                client.m_CurrentLife = client.m_CurrentLifeMax;
            }
            if (client.m_CurrentMana < 0)
            {
                client.m_CurrentMana = client.m_CurrentManaMax;
            }
            if (client.m_CurrentStamina < 0)
            {
                client.m_CurrentStamina = client.m_CurrentStaminaMax;
            }

            double HpBeforeExits = (double)client.m_CurrentLife / (double)client.m_CurrentLifeMax * 100;
            double mpBeforeDetach = (double)client.m_CurrentMana / (double)client.m_CurrentManaMax * 100;
            double staminaBeforeDetach = (double)client.m_CurrentStamina / (double)client.m_CurrentStaminaMax * 100;
            // Lưu lại máu hiện tại của nhân vật
            Global.SaveRoleParamsInt32ValueToDB(client, RoleParamName.CurHP, (int)HpBeforeExits, true);
            // Lưu lại mana của nhân vật
            Global.SaveRoleParamsInt32ValueToDB(client, RoleParamName.CurMP, (int)mpBeforeDetach, true);
            // Lưu lại thể lực của nhân vật
            Global.SaveRoleParamsInt32ValueToDB(client, RoleParamName.CurStamina, (int)staminaBeforeDetach, true);

            lock (client.PropPointMutex)
            {
                /// Điểm tiềm năng được cộng thêm từ bánh
                Global.SaveRoleParamsInt32ValueToDB(client, RoleParamName.TotalPropPoint, client.GetBonusRemainPotentialPoints(), true);

                // LƯU VÀO DB 4 chỉ số
                // SỨC , TRÍ , NHANH, THỂ
                Global.SaveRoleParamsInt32ValueToDB(client, RoleParamName.sPropStrength, client.GetStrength(), true);
                Global.SaveRoleParamsInt32ValueToDB(client, RoleParamName.sPropIntelligence, client.GetEnergy(), true);
                Global.SaveRoleParamsInt32ValueToDB(client, RoleParamName.sPropDexterity, client.GetDexterity(), true);
                Global.SaveRoleParamsInt32ValueToDB(client, RoleParamName.sPropConstitution, client.GetVitality(), true);

                // Lưu lại tinh hoạt lực cho nhân vật
                Global.SaveRoleParamsInt32ValueToDB(client, RoleParamName.GatherPoint, client.GetGatherPoint(), true);
                Global.SaveRoleParamsInt32ValueToDB(client, RoleParamName.MakePoint, client.GetMakePoint(), true);

                // LƯU LẠI ĐÃ NHẬN QUÀ TẢI CHƯA
                Global.SaveRoleParamsInt32ValueToDB(client, RoleParamName.TreasureJiFen, client.ReviceBounsDownload, true);

                // LƯU LẠI THÔNG IN KỸ NĂNG SỐNG
                Global.SaveRoleParamsStringToDB(client, RoleParamName.LifeSkill, client.LifeSkillToString, true);

                // LƯU LẠI VINH DỰ VÕ LÂM
                Global.SaveRoleParamsStringToDB(client, RoleParamName.ReputeInfo, client.ReputeInfoToString, true);

                /// Lưu lại danh sách danh hiệu
                Global.SaveRoleParamsStringToDB(client, RoleParamName.RoleTitles, client.RoleTitlesInfoString, true);

                // Lưu lại các bản ghi hàng ngày
                Global.SaveRoleParamsStringToDB(client, RoleParamName.DailyRecore, client.DailyRecoreString, true);
                //Lưu lại các bản ghi tạm thời trong tuần
                Global.SaveRoleParamsStringToDB(client, RoleParamName.WeekRecore, client.WeekRecoreString, true);
                // Lưu lại các bản ghi vĩnh viễn
                Global.SaveRoleParamsStringToDB(client, RoleParamName.ForeverRecore, client.ForeverRecoreString, true);
                // LƯU LẠI THỜI GIAN ỦY THÁC BẠCH CẦU HOÀN
                Global.SaveRoleParamsInt32ValueToDB(client, RoleParamName.MeditateTime, client.baijuwan, true);

                Global.SaveRoleParamsInt32ValueToDB(client, RoleParamName.NotSafeMeditateTime, client.baijuwanpro, true);
            }

            // Lưu lại thời gian đăng nhập
            Global.SaveRoleParamsInt32ValueToDB(client, RoleParamName.DayOnlineSecond, client.DayOnlineSecond, true);

            // Lưu lại đăng nhập liên tục
            Global.SaveRoleParamsInt32ValueToDB(client, RoleParamName.SeriesLoginCount, client.SeriesLoginNum, true);

            Global.SaveRoleParamsInt32ValueToDB(client, RoleParamName.OpenPortableGridTick, client.OpenPortableGridTime, true);

            /// Lưu lại số điểm Kinh nghiệm Tu Luyện Châu còn lại
            Global.SaveRoleParamsInt32ValueToDB(client, RoleParamName.XiuLianZhu, client.XiuLianZhu_Exp, true);

            Global.SaveRoleParamsInt32ValueToDB(client, RoleParamName.XiuLianZhu_TotalTime, client.XiuLianZhu_TotalTime, true);
        }

        #endregion UpdateRolePrams

        #region Thay đổi ngày đăng nhập

        /// <summary>
        /// Xử lý các sự kiện khi qua 1 ngày
        /// </summary>
        /// <param name="client"></param>
        public static void ChangeDayLoginNum(KPlayer client)
        {
            int dayID = TimeUtil.NowDateTime().DayOfYear;

            if (dayID == client.LoginDayID)
            {
                return;
            }

            client.LoginDayID = dayID;

            UpdateEventNextDay(client);
        }

        /// <summary>
        /// Xử lý sự kiện khi next ngày
        /// </summary>
        /// <param name="client"></param>
        public static void UpdateEventNextDay(KPlayer client)
        {
            //Reload task bang hội cho ngày mới
            GuildManager.getInstance().ReloadAllTaskNextDay();

            // Cập nhật thẻ tháng cho ngày mới
            CardMonthManager.UpdateNewDay(client);

            // Ghi lại nhật ký đăng nhập
            Global.UpdateRoleLoginRecord(client);

            client.SendPacket((int)TCPGameServerCmds.CMD_SYNC_CHANGE_DAY_SERVER, string.Format("{0}", TimeUtil.NOW() * 10000));
        }

        #endregion Thay đổi ngày đăng nhập

        #region Xử lý giao dịch khi thoát game

        /// <summary>
        /// Xử lý giao dịch khi ngắt kết nối
        /// </summary>
        /// <param name="sl"></param>
        /// <param name="pool"></param>
        /// <param name="client"></param>
        public static void ProcessExchangeData(SocketListener sl, TCPOutPacketPool pool, KPlayer client)
        {
            if (client.ExchangeID > 0)
            {
                ExchangeData ed = KTTradeManager.Find(client.ExchangeID);
                if (null != ed)
                {
                    int otherRoleID = (ed.RequestRoleID == client.RoleID) ? ed.AgreeRoleID : ed.RequestRoleID;

                    // TÌm thằng đối phương đang giao dịch cùng

                    KPlayer otherClient = KTPlayerManager.Find(otherRoleID);
                    if (null != otherClient) //Nếu thằng này vẫn đang online
                    {
                        if (otherClient.ExchangeID > 0 && otherClient.ExchangeID == client.ExchangeID)
                        {
                            KTTradeManager.Remove(client.ExchangeID);

                            KTTradeManager.RestoreExchangeData(otherClient, ed);

                            otherClient.ExchangeID = 0;
                            otherClient.ExchangeTicks = 0;

                            //Thông báo về là hủy giao dịch
                            KT_TCPHandler.NotifyGoodsExchangeCmd(client.RoleID, otherRoleID, null, otherClient, client.ExchangeID, (int)GoodsExchangeCmds.Cancel);
                        }
                    }
                }
            }
        }

        #endregion Xử lý giao dịch khi thoát game
    }
}