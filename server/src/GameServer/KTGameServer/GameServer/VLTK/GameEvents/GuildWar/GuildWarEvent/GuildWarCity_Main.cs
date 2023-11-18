using GameServer.Core.Executor;
using GameServer.KiemThe;
using GameServer.KiemThe.Core.Item;
using GameServer.KiemThe.Core.Task;
using GameServer.KiemThe.Entities;
using GameServer.KiemThe.Logic;
using GameServer.KiemThe.LuaSystem.Logic;
using GameServer.Logic;
using GameServer.Server;
using Server.Data;
using Server.Tools;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using System.Xml.Serialization;

using static GameServer.Logic.KTMapManager;

namespace GameServer.VLTK.Core.GuildManager
{
    /// <summary>
    /// Class xử lý toàn bộ hệ thống công thành chiến
    /// </summary>
    public partial class GuildWarCity
    {
        /// <summary>
        /// Khởi tạo 1 instance
        /// </summary>
        private static GuildWarCity instance = new GuildWarCity();

        public static GuildWarCity getInstance()
        {
            return instance;
        }

        /// <summary>
        /// Cột trụ thuộc về bang nào
        /// </summary>
        public int BeLongGuild { get; set; } = -1;

        public string GuildTmpName { get; set; }

        /// <summary>
        /// Rate máu của trụ
        /// </summary>
        public double HPRate = 1;

        /// <summary>
        /// Toàn bộ cột trụ ở map
        /// </summary>
        public ConcurrentDictionary<int, Monster> AllBudding = new ConcurrentDictionary<int, Monster>();

        /// <summary>
        /// hồi sinh player bị giết
        /// </summary>
        /// <param name="client"></param>
        public static void Revice(KPlayer client)
        {
            _WarBattle.TryGetValue(client.GuildID, out GuildWarBattleReport _Report);
            if (_Report != null)
            {
                // Nếu là phe tấn công
                if (_Report.Flag == WARFLAG.ATTACK)
                {
                    var Find = _WarConfig.AttackPoint.FirstOrDefault();
                    if (Find != null)
                    {
                        KTPlayerManager.Relive(client, ActiveWar.FightMapID, Find.PosX, Find.PosY, 100, 100, 100);
                    }
                }
                else
                {
                    var Find = _WarConfig.DefPoint.FirstOrDefault();
                    if (Find != null)
                    {
                        KTPlayerManager.Relive(client, ActiveWar.FightMapID, Find.PosX, Find.PosY, 100, 100, 100);
                    }
                }
            }
        }

        public static bool IsInGuildWarCity(KPlayer client)
        {
            if (client.MapCode == ActiveWar.FightMapID)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Thời gian đăng ký
        /// </summary>
        public int SIGN_UP_DULATION { get; set; } = 30;

        /// <summary>
        /// Thời gian vã nhau
        /// </summary>
        public int FIGHT_DULATION { get; set; } = 2000;

        /// <summary>
        /// Thời gian để nhận thưởng
        /// </summary>
        public int GETGIFT_DULATION { get; set; } = 300;

        /// <summary>
        /// Nay có phải ngày lên lôi đày hay không
        /// </summary>
        public bool IsSoloDay { get; set; }

        /// <summary>
        /// Nay có phải ngày đánh thành hay không
        /// </summary>
        public bool IsFightDay { get; set; }

        public long LastProtectTick { get; set; }

        public long LastUpdateScore { get; set; }

        /// <summary>
        /// Gần tick gần đây nhất
        /// </summary>
        public long LastTick { get; set; }

        /// <summary>
        /// Lần gần đây nhất gửi thông báo cho client
        /// </summary>
        public long LastNofity { get; set; }

        /// <summary>
        /// Trạng thái của sự kiện công thành chiến
        /// </summary>
        public GUILD_WAR_STATE _STATE { get; set; }

        /// <summary>
        /// Xem ngày hôm này là ngày nào
        /// Xác định luôn là ngày teamfight hay công thành
        /// </summary>
        public int ThisDay { get; set; }

        /// <summary>
        /// Lấy ra thông tin bang thủ và bang công thành
        /// </summary>
        public void GetAttackGuildInfo()
        {
            foreach (MiniGuildInfo _MiniGuild in GuildManager._TotalGuild.Values)
            {
                // Nếu bang này có thông tin
                if (_MiniGuild != null)
                {
                    // Nếu mà bang này đang là chủ thành
                    if (_MiniGuild.IsMainCity == (int)GUILD_CITY_STATUS.HOSTCITY)
                    {
                        GuildWarBattleReport _DefReport = new GuildWarBattleReport();
                        _DefReport.Flag = WARFLAG.DEF;
                        _DefReport.GuildID = _MiniGuild.GuildId;
                        _DefReport.GuildName = _MiniGuild.GuildName;
                        _DefReport.TotalScore = 0;
                        _DefReport.Rank = 0;

                        // Khởi tạo dict Def
                        _WarBattle.TryAdd(_MiniGuild.GuildId, _DefReport);
                    }

                    if (_MiniGuild.IsMainCity == (int)GUILD_CITY_STATUS.ATTACKCITY)
                    {
                        GuildWarBattleReport _AttackReport = new GuildWarBattleReport();
                        _AttackReport.Flag = WARFLAG.ATTACK;
                        _AttackReport.GuildID = _MiniGuild.GuildId;
                        _AttackReport.TotalScore = 0;
                        _AttackReport.GuildName = _MiniGuild.GuildName;
                        _AttackReport.Rank = 0;

                        // Khởi tạo dict Attack
                        _WarBattle.TryAdd(_MiniGuild.GuildId, _AttackReport);
                    }
                }
            }
        }

        /// <summary>
        /// truyền vào 2 tham số của các bang
        /// </summary>
        /// <param name="AttackGuildID"></param>
        /// <param name="DefGuildID"></param>
        public void Setup()
        {
            // Call loading config
            this.LoadConfig();

            ScheduleExecutor2.Instance.scheduleExecute(new NormalScheduleTask("GuildWarCity", ProsecBattle), 5 * 1000, 2000);

            ThisDay = TimeUtil.GetWeekDay1To7(DateTime.Now);

            // Lấy ra toàn bộ bản đồ sẽ xảy ra sự kiện lôi đài ngày hôm nay
            // ActiveWar = _WarConfig.Citys.Where(x => x.TeamFightDay == Today).FirstOrDefault();

            //TEST
            ActiveWar = _WarConfig.Citys.FirstOrDefault();

            if (ActiveWar != null)
            {
                OutMapCode = new List<int>();

                OutMapCode.Add(1);
                OutMapCode.Add(5500);
                OutMapCode.Add(3000);

                if (ActiveWar.TeamFightDay == ThisDay)
                {
                    // Tạo mới danh sách thành viên đăng ký tham gia
                    FightTeams = new List<GuildTeamFightMember>();
                    // Lấy toàn bộ thông thin chiến đọi
                    this.GetInfoTeamFight();
                }

                if (ActiveWar.CityFightDay == ThisDay)
                {
                    // Lấy ra toàn bộ thông tin bang thủ và công
                    GetAttackGuildInfo();
                }
            }
        }

        /// <summary>
        /// List tạm thời lưu lại bảng xếp hạng của 1 bang công thành
        /// </summary>
        public static List<GuildWarPlayer> TMP_RANK_ATTACK = new List<GuildWarPlayer>();

        /// <summary>
        /// List tạm thời lưu lại bảng xếp hạng của 1 bang thủ thành
        /// </summary>
        public static List<GuildWarPlayer> TMP_RANK_DEF = new List<GuildWarPlayer>();

        /// <summary>
        /// Lưu lại tổng điểm của 2 bang vã nhau
        /// </summary>

        public static ConcurrentDictionary<int, GuildWarBattleReport> _WarBattle = new ConcurrentDictionary<int, GuildWarBattleReport>();

        /// <summary>
        /// Guild war config
        /// </summary>
        public static GuildWarCityConfig _WarConfig = new GuildWarCityConfig();

        /// <summary>
        /// Dict lưu lại bên tấn công
        /// </summary>
        public ConcurrentDictionary<int, GuildWarPlayer> AttackDict = new ConcurrentDictionary<int, GuildWarPlayer>();

        /// <summary>
        /// Dict lưu lại bên thủ
        /// </summary>
        public ConcurrentDictionary<int, GuildWarPlayer> DefDict = new ConcurrentDictionary<int, GuildWarPlayer>();

        /// <summary>
        /// Chứa thông tin bản đồ sẽ đánh chiếm
        /// Chứa thông tin phần thưởng của bang thủ và bang công
        /// Chứa thông tin cắm cọc
        /// Chứa thông tin quái sẽ mọc ra
        /// </summary>
        public string _GUILD_WAR_CITY_CONFIG = "Config/KT_Guild/GuildWarCityConfig.xml";

        /// <summary>
        /// LoadConfig GuildWar
        /// </summary>
        public void LoadConfig()
        {
            string Files = KTGlobal.GetDataPath(_GUILD_WAR_CITY_CONFIG);

            using (var stream = System.IO.File.OpenRead(Files))
            {
                var serializer = new XmlSerializer(typeof(GuildWarCityConfig));

                // Loading warconfig
                _WarConfig = serializer.Deserialize(stream) as GuildWarCityConfig;
            }
        }

        #region _BattleProsecc

        #region Notify

        public void UpdateMiniMapBuilding(KPlayer client)
        {
            List<LocalMapMonsterData> monsters = new List<LocalMapMonsterData>();

            foreach (Monster monster in AllBudding.Values.ToList())
            {
                monsters.Add(new LocalMapMonsterData()
                {
                    Name = monster.Title,
                    PosX = (int)monster.CurrentPos.X,
                    PosY = (int)monster.CurrentPos.Y,
                    IsBoss = true,
                });
            }

            /// Gửi lại gói tin về Client
            byte[] byteData = DataHelper.ObjectToBytes<List<LocalMapMonsterData>>(monsters);

            client.SendPacket((int)TCPGameServerCmds.CMD_KT_UPDATE_LOCALMAP_MONSTER, byteData);
        }

        /// <summary>
        /// Gửi về client cái thời gian đếm ngược chuẩn bị bắt đầu
        /// </summary>
        /// <param name="IsNotiyText"></param>
        public void UpdatePreading(bool IsNotiyText)
        {
            long SEC = (LastTick + SIGN_UP_DULATION * 1000) - TimeUtil.NOW();

            int FinalSec = (int)(SEC / 1000);

            string MSG = "Chiến trường sắp bắt đầu, thời gian còn: [" + FinalSec + "] giây.";

            // Gửi toàn bộ thoogn tin cho bên tấn công
            foreach (KeyValuePair<int, GuildWarPlayer> entry in AttackDict)
            {
                GuildWarPlayer PlayerBattle = entry.Value;

                if (PlayerBattle._Player.CurrentMapCode == ActiveWar.FightMapID)
                {
                    // Để nó hở hòa bình
                    PlayerBattle._Player.PKMode = (int)PKMode.Peace;
                    //Set camp thằng này chính bàng GUILDID của nó
                    PlayerBattle._Player.Camp = PlayerBattle._Player.GuildID;

                    KTPlayerManager.ShowNotification(PlayerBattle._Player, MSG);

                    SendPreadingNotify(PlayerBattle._Player, FinalSec, AttackDict.Count, DefDict.Count, IsNotiyText);
                }
            }

            // Load toàn bộ spam
            foreach (KeyValuePair<int, GuildWarPlayer> entry in DefDict)
            {
                GuildWarPlayer PlayerBattle = entry.Value;
                if (PlayerBattle._Player.CurrentMapCode == ActiveWar.FightMapID)
                {
                    // Để nó hở hòa bình
                    PlayerBattle._Player.PKMode = (int)PKMode.Peace;
                    PlayerBattle._Player.Camp = PlayerBattle._Player.GuildID;

                    KTPlayerManager.ShowNotification(PlayerBattle._Player, MSG);

                    SendPreadingNotify(PlayerBattle._Player, FinalSec, AttackDict.Count, DefDict.Count, IsNotiyText);
                }
            }
        }

        /// <summary>
        /// Sự kiện khi người chơi vào bản đồ này
        /// </summary>
        /// <param name="client"></param>
        public void OnEnterMap(KPlayer client)
        {
            if (IsInGuildWarCity(client))
            {
                PlayChangeState(client, 1);
            }
        }

        public void OnLeaverMap(KPlayer client, GameMap map)
        {
            if (map.MapCode != ActiveWar.FightMapID)
            {
                PlayChangeState(client, 0);
            }
        }

        public void UpdateStartBattle()
        {
            long SEC = (LastTick + FIGHT_DULATION * 1000) - TimeUtil.NOW();

            int FinalSec = (int)(SEC / 1000);

            foreach (KeyValuePair<int, GuildWarPlayer> entry in AttackDict)
            {
                GuildWarPlayer PlayerBattle = entry.Value;

                UpdateMiniMapBuilding(PlayerBattle._Player);

                if (PlayerBattle._Player.CurrentMapCode == ActiveWar.FightMapID)
                {
                    PlayerBattle._Player.PKMode = (int)PKMode.Custom;
                    //Set camp thằng này chính bàng GUILDID của nó
                    PlayerBattle._Player.Camp = PlayerBattle._Player.GuildID;
                }
                SendNotifyLoop(PlayerBattle);
            }

            // Load toàn bộ spam
            foreach (KeyValuePair<int, GuildWarPlayer> entry in DefDict)
            {
                GuildWarPlayer PlayerBattle = entry.Value;

                UpdateMiniMapBuilding(PlayerBattle._Player);
                if (PlayerBattle._Player.CurrentMapCode == ActiveWar.FightMapID)
                {
                    PlayerBattle._Player.PKMode = (int)PKMode.Custom;
                    PlayerBattle._Player.Camp = PlayerBattle._Player.GuildID;
                }
                SendNotifyLoop(PlayerBattle);
            }
            // Gửi toàn bộ thoogn tin cho bên tấn công
        }

        /// <summary>
        /// Lấy ra số điểm hiện tại của 2 bang
        /// </summary>
        /// <returns></returns>
        public List<string> GetGuildPoint(bool ForceUpdate = false)
        {
            List<string> _Info = new List<string>();

            foreach (GuildWarBattleReport _Report in _WarBattle.Values)
            {
                if (_Report.Flag == WARFLAG.ATTACK)
                {
                    _Info.Add("Bang Công <color=red>" + _Report.GuildName + "</color> : " + _Report.TotalScore);
                }
                else if (_Report.Flag == WARFLAG.DEF)
                {
                    _Info.Add("Bang Thủ <color=red>" + _Report.GuildName + "</color> : " + _Report.TotalScore);
                }
            }

            return _Info;
        }

        public void SendNotifyLoop(GuildWarPlayer player)
        {
            if (_STATE == GUILD_WAR_STATE.STATUS_START || _STATE == GUILD_WAR_STATE.STATUS_PREPAREEND)
            {
                long SEC = (LastTick + FIGHT_DULATION * 1000) - TimeUtil.NOW();

                int FinalSec = (int)(SEC / 1000);

                G2C_EventNotification _Notify = new G2C_EventNotification();

                _Notify.EventName = "Công thành đại chiến - <color=green>" + ActiveWar.CityName + "</color>";

                _Notify.ShortDetail = "TIME|" + FinalSec;

                _Notify.TotalInfo = new List<string>();

                _Notify.TotalInfo.Add("<color=yellow>Tích lũy cá nhân : </color><color=green>" + player.Score + "</color>");

                _Notify.TotalInfo.Add("<color=yellow>Phá long trụ : </color><color=green>" + player.DestroyCount + "</color>");

                if (player.CurentRank != -1)
                {
                    _Notify.TotalInfo.Add("<color=yellow>Hạng Hiện Tại : </color><color=green>" + player.CurentRank + "</color>");
                }
                else
                {
                    _Notify.TotalInfo.Add("Hạng Hiện Tại : Chưa xếp hạng");
                }

                _Notify.TotalInfo.AddRange(GetGuildPoint().ToArray());

                if (player._Player.IsOnline())
                {
                    player._Player.SendPacket<G2C_EventNotification>((int)TCPGameServerCmds.CMD_KT_EVENT_NOTIFICATION, _Notify);
                }
                else
                {
                    //Console.WriteLine("OFFLINE");
                }
            }
            else if (_STATE == GUILD_WAR_STATE.STATUS_END)
            {
                long SEC = (LastTick + GETGIFT_DULATION * 1000) - TimeUtil.NOW();

                int FinalSec = (int)(SEC / 1000);

                G2C_EventNotification _Notify = new G2C_EventNotification();

                _Notify.EventName = "Thời gian nhận thưởng";

                _Notify.ShortDetail = "TIME|" + FinalSec;

                _Notify.TotalInfo = new List<string>();

                _Notify.TotalInfo.Add("Nhận thưởng tại <color=green>Tiếp Đầu Công Thành Chiến</color>");

                _Notify.TotalInfo.AddRange(GetGuildPoint().ToArray());

                if (player._Player.IsOnline())
                {
                    player._Player.SendPacket<G2C_EventNotification>((int)TCPGameServerCmds.CMD_KT_EVENT_NOTIFICATION, _Notify);
                }
                else
                {
                    //Console.WriteLine("OFFLINE");
                }
            }
        }

        public void SendPreadingNotify(KPlayer Player, int Sec, int TotalAttack, int TotalDef, bool IsNotiyText)
        {
            if (IsNotiyText)
            {
                G2C_EventNotification _Notify = new G2C_EventNotification();

                _Notify.EventName = "Công thành sẽ diễn ra sau :";

                _Notify.ShortDetail = "TIME|" + Sec;

                _Notify.TotalInfo = new List<string>();

                _Notify.TotalInfo.Add("Phe công thành: " + TotalAttack);

                _Notify.TotalInfo.Add("Phe thủ thành: " + TotalDef);

                if (Player.IsOnline())
                {
                    Player.SendPacket<G2C_EventNotification>((int)TCPGameServerCmds.CMD_KT_EVENT_NOTIFICATION, _Notify);
                }
                else
                {
                    //Console.WriteLine("OFFLINE");
                }
            }
        }

        #endregion Notify

        #region UPDATEBXH

        public void UpdateBXH()
        {
            lock (TMP_RANK_ATTACK)
            {
                // Tmp attack
                List<GuildWarPlayer> AttackTMPDICT = AttackDict.Select(x => x.Value).ToList();

                // Attack tmp
                TMP_RANK_ATTACK.AddRange(AttackTMPDICT);

                // Tmp attack
                List<GuildWarPlayer> DefTMPDICT = DefDict.Select(x => x.Value).ToList();

                // Attack tmp
                TMP_RANK_DEF.AddRange(DefTMPDICT);
            }
        }

        public int GetPositionInBxh(GuildWarPlayer InputPlayer)
        {
            if (InputPlayer.Flag == WARFLAG.ATTACK)
            {
                lock (TMP_RANK_ATTACK)
                {
                    var find = TMP_RANK_ATTACK.FindIndex(x => x._Player.RoleID == InputPlayer._Player.RoleID);

                    return find;
                }
            }
            if (InputPlayer.Flag == WARFLAG.DEF)
            {
                lock (TMP_RANK_DEF)
                {
                    var find = TMP_RANK_DEF.FindIndex(x => x._Player.RoleID == InputPlayer._Player.RoleID);

                    return find;
                }
            }

            return -1;
        }

        #endregion UPDATEBXH

        /// <summary>
        /// Tạo long trụ hệ thống
        /// </summary>
        /// <param name="_Monster"></param>
        /// <param name="_BelongGuid"></param>
        /// <param name="_GuildName"></param>
        public void CreateMapObject(ObjectivePostion _Monster, int _BelongGuid, string _GuildName)
        {
            try
            {
                MonsterAIType aiType = MonsterAIType.Special_Boss;

                /// Ngũ hành
                KE_SERIES_TYPE series = KE_SERIES_TYPE.series_none;

                int RandomSeri = KTGlobal.GetRandomNumber(1, 5);
                series = (KE_SERIES_TYPE)RandomSeri;

                /// Hướng quay
                KiemThe.Entities.Direction dir = KiemThe.Entities.Direction.NONE;

                int RandomDir = KTGlobal.GetRandomNumber(0, 7);

                dir = (KiemThe.Entities.Direction)RandomDir;

                string TITLE = "";

                int CAMP = 1000;

                // Nếu không phỉa
                if (_BelongGuid == -1)
                {
                    // Nếu ko thì set ttitle là hệ thống
                    TITLE = "<b><color=#00ff2a>Long Trụ Hệ Thống</color></b>";
                }
                else
                {
                    //Nếu trụ này thuộc về bang nào đó
                    CAMP = _BelongGuid;
                    TITLE = "<b><color=#00ff2a>" + _GuildName + "</color></b>";
                }

                /// Tạo quái
                Monster monster = KTMonsterManager.Create(new KTMonsterManager.DynamicMonsterBuilder()
                {
                    MapCode = ActiveWar.FightMapID,
                    ResID = _Monster.ID,
                    PosX = _Monster.PosX,
                    PosY = _Monster.PosY,
                    Name = _Monster.Name,
                    Title = TITLE,
                    MaxHP = _Monster.Hp,
                    Level = 90,
                    MonsterType = aiType,
                    Camp = CAMP,
                });

                Console.WriteLine(monster);

                monster.OnDieCallback = (_GameObject) =>
                {
                    // Nếu nhưng đang đánh hoặc chuẩn bị kết thúc
                    if (_STATE == GUILD_WAR_STATE.STATUS_START || _STATE == GUILD_WAR_STATE.STATUS_PREPAREEND)
                    {
                        // Gọi sự kiện khi trụ này toác

                        // Nếu trụ này bị người chơi đập vỡ thì lại tạo 1 long trụ mới với CAMP là camp của bang này
                        if (_GameObject is KPlayer)
                        {
                            KPlayer _Client = (KPlayer)_GameObject;

                            //Tạo lại 1 cái long trụ khác
                            this.CreateMapObject(_Monster, _Client.GuildID, _Client.GuildName);

                            // Nếu như cái trụ này không phải của Hệ thống
                            if (monster.Camp != 1000)
                            {
                                // Nếu như cái trụ này bị 1 thằng bang khác đánh sập
                                if (_Client.GuildID != monster.Camp)
                                {
                                    string MSG = KTGlobal.CreateStringByColor("Long trụ đang bị bang [" + _Client.GuildName + "] chiếm mất , hãy mau chóng cử ngưỡi canh giữ!", ColorType.Importal);

                                    //Thông báo cho bang kia là đã bị đánh sập
                                    KTGlobal.SendGuildChat(monster.Camp, MSG, null, "");
                                }
                            }

                            // Xem thằng này có bang hội không
                            if (_Client.TeamID != -1)
                            {
                                KPlayer _Lead = KTTeamManager.GetTeamLeader(_Client.TeamID);

                                List<KPlayer> TotalMember = _Client.Teammates;

                                foreach (KPlayer member in TotalMember)
                                {
                                    if (KTGlobal.GetDistanceBetweenPoints(new Point(member.PosX, member.PosY), _Client.CurrentPos) > 1000)
                                    {
                                        continue;
                                    }
                                    // Cộng cho mỗi thằng 300 điểm
                                    this.UpdateScore(member, 300, 1, 0, false);
                                }

                                string MSG = "Tổ đội của " + KTGlobal.CreateStringByColor(_Lead.RoleName, ColorType.Yellow) + " đã có công phá vỡ Long Trụ của địch";

                                KTGlobal.SendGuildChat(_Lead.GuildID, MSG, null, "");
                            }
                            else
                            {
                                string MSG = "Người chơi " + KTGlobal.CreateStringByColor(_Client.RoleName, ColorType.Yellow) + " đã có công phá vỡ Long Trụ của địch";

                                KTGlobal.SendGuildChat(_Client.GuildID, MSG, null, "");

                                // Cộng cho thằng latst hit trụ 500 điểm

                                this.UpdateScore(_Client, 500, 1, 0, false);
                            }

                            this.UpdateGuildScore(_Client.GuildID, 100);
                        }
                        else if (_GameObject is Monster)
                        {
                            // Nếu là quái đập vỡ long trụ thì tạo 1 cái long trụ rỗng
                            Monster _Client = (Monster)_GameObject;

                            this.CreateMapObject(_Monster, -1, "");

                            string MSG = KTGlobal.CreateStringByColor("Long trụ đã bị quân Lưu vong nơi đó phản công, hãy mau quay về canh giữ!", ColorType.Importal);

                            KTGlobal.SendGuildChat(monster.Camp, MSG, null, "");
                        }
                    }
                };

                // Add cái này vào map
                int IDLOCALTION = _Monster.PosX * _Monster.PosY;

                if (AllBudding.ContainsKey(IDLOCALTION))
                {
                    AllBudding[IDLOCALTION] = monster;
                }
                else
                {
                    AllBudding.TryAdd(IDLOCALTION, monster);
                }

                monster.IsStatic = true;
                monster.m_IgnoreAllSeriesStates = true;
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.GuildWarManager, "BUG :" + ex.ToString());
            }
        }

        /// <summary>
        /// Update điểm cho bang giữ cột trụ mỗi 60 giây
        /// </summary>
        public void UpdateScoreForGuildEvery60SEC()
        {
            try
            {
                foreach (KeyValuePair<int, GuildWarBattleReport> entry in _WarBattle)
                {
                    GuildWarBattleReport MapEvent = entry.Value;

                    int COUNT = AllBudding.Values.Where(x => x.Camp == entry.Key).Count();

                    int FINALPOINT = COUNT * 5;

                    if (FINALPOINT > 0)
                    {
                        UpdateGuildScore(entry.Key, FINALPOINT);
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.GuildWarManager, "BUG :" + ex.ToString());
            }
        }

        public void OpenAllDor()
        {
            GameMap gameMap = KTMapManager.Find(ActiveWar.FightMapID);
            /// Nếu tồn tại
            if (gameMap != null)
            {
                gameMap.MyNodeGrid.CloseDynamicObsLabelTry(ActiveWar.FightMapID, 1);
                gameMap.MyNodeGrid.CloseDynamicObsLabelTry(ActiveWar.FightMapID, 2);
                gameMap.MyNodeGrid.CloseDynamicObsLabelTry(ActiveWar.FightMapID, 3);
            }
        }

        /// <summary>
        /// Xóa toàn bộ OBS
        /// </summary>
        public void ClearAllDor()
        {
            GameMap gameMap = KTMapManager.Find(ActiveWar.FightMapID);
            /// Nếu tồn tại
            if (gameMap != null)
            {
                gameMap.MyNodeGrid.OpenDynamicObsLabel(ActiveWar.FightMapID, 1);
                gameMap.MyNodeGrid.OpenDynamicObsLabel(ActiveWar.FightMapID, 2);
                gameMap.MyNodeGrid.OpenDynamicObsLabel(ActiveWar.FightMapID, 3);
            }
        }

        /// <summary>
        /// Thả quái trên bản đồ
        /// </summary>
        /// <param name="_Monster"></param>
        /// <param name="CAMP"></param>
        public void CreateMonster(ObjectivePostion _Monster, int CAMP)
        {
            try
            {
                MonsterAIType aiType = MonsterAIType.Special_Normal;

                /// Ngũ hành
                KE_SERIES_TYPE series = KE_SERIES_TYPE.series_none;

                int RandomSeri = KTGlobal.GetRandomNumber(1, 5);
                series = (KE_SERIES_TYPE)RandomSeri;

                /// Hướng quay
                KiemThe.Entities.Direction dir = KiemThe.Entities.Direction.NONE;

                int RandomDir = KTGlobal.GetRandomNumber(0, 7);

                dir = (KiemThe.Entities.Direction)RandomDir;

                string TITLE = "";

                int Random = new Random().Next(0, 100);

                // nếu đây là boss thì có tỉ lệ ra hoặc không ra
                if (_Monster.ID == 3408 || _Monster.ID == 3409 || _Monster.ID == 3410 || _Monster.ID == 3411 || _Monster.ID == 3412)
                {
                    // Nếu như tỉ lệ mà random mà nhỏ hơn 30 thì đéo ra boss
                    if (Random < 30)
                    {
                        return;
                    }
                    else
                    {
                        int RANODMTIMERESPAN = new Random().Next(600000, 1200000);

                        var Funtion = KTKTAsyncTask.Instance.ScheduleExecuteAsync(new DelayFuntionAsyncTask("CreateMonster", new Action(() => CreateMonsterByDelay(_Monster, CAMP))), RANODMTIMERESPAN);
                    }
                }
                else
                {
                    /// Tạo quái
                    Monster monster = KTMonsterManager.Create(new KTMonsterManager.DynamicMonsterBuilder()
                    {
                        MapCode = ActiveWar.FightMapID,
                        ResID = _Monster.ID,
                        PosX = _Monster.PosX,
                        PosY = _Monster.PosY,
                        Name = _Monster.Name,
                        Title = TITLE,
                        MaxHP = _Monster.Hp,
                        Level = 90,
                        MonsterType = aiType,
                        Camp = CAMP,
                        RespawnTick = 20000,
                    });

                    if (monster != null)
                    {
                        monster.DynamicRespawnPredicate = () =>
                        {
                            if (_STATE == GUILD_WAR_STATE.STATUS_START)
                            {
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        };
                        monster.OnDieCallback = (_GameObject) =>
                        {
                            if (_STATE == GUILD_WAR_STATE.STATUS_START || _STATE == GUILD_WAR_STATE.STATUS_PREPAREEND)
                            {
                                if (_GameObject is KPlayer)
                                {
                                    KPlayer _Player = (KPlayer)_GameObject;

                                    // Cứ mỗi lần giết con NPC sẽ được 37 điểm
                                    this.UpdateScore(_Player, 37, 0, 0, false);
                                }
                            }
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.GuildWarManager, "BUG :" + ex.ToString());
            }
        }

        /// <summary>
        /// Tạo monster delay time
        /// </summary>
        /// <param name="_Monster"></param>
        /// <param name="CAMP"></param>
        public void CreateMonsterByDelay(ObjectivePostion _Monster, int CAMP)
        {
            try
            {
                MonsterAIType aiType = MonsterAIType.Special_Boss;

                /// Ngũ hành
                KE_SERIES_TYPE series = KE_SERIES_TYPE.series_none;

                int RandomSeri = KTGlobal.GetRandomNumber(1, 5);

                series = (KE_SERIES_TYPE)RandomSeri;

                /// Hướng quay
                KiemThe.Entities.Direction dir = KiemThe.Entities.Direction.NONE;

                int RandomDir = KTGlobal.GetRandomNumber(0, 7);

                dir = (KiemThe.Entities.Direction)RandomDir;

                string TITLE = "";

                if (BeLongGuild == -1)
                {
                    //  TITLE = "<b><color=#00ff2a>Hệ Thống</color></b>";
                }
                else
                {
                    CAMP = BeLongGuild;
                    // TITLE = "<b><color=#00ff2a>" + this.BeLongGuildName + "</color></b>";
                }

                string NotifyProtect = "Boss công thành <b>[" + _Monster.Name + "]</b> đã xuất hiện tại <color=yellow>" + _Monster.Name + "</color> đã xuất hiện hãy mau chóng tiêu diệt!";

                // Thông báo cho toàn bộ các bang tham chiến biết thôn tin về boss lãnh thổ
                foreach (int GUILD in _WarBattle.Keys)
                {
                    KTGlobal.SendGuildChat(GUILD, NotifyProtect, null, "");
                }

                double BaseHP = 40385163 * HPRate * 10;

                /// Tạo quái
                Monster monster = KTMonsterManager.Create(new KTMonsterManager.DynamicMonsterBuilder()
                {
                    MapCode = ActiveWar.FightMapID,
                    ResID = _Monster.ID,
                    PosX = _Monster.PosX,
                    PosY = _Monster.PosY,
                    Name = _Monster.Name,
                    Title = TITLE,
                    MaxHP = (int)BaseHP,
                    Level = 90,
                    MonsterType = aiType,
                    Camp = CAMP,
                });
                monster.OnDieCallback = (_GameObject) =>
                {
                    if (_STATE == GUILD_WAR_STATE.STATUS_START || _STATE == GUILD_WAR_STATE.STATUS_PREPAREEND)
                    {
                        if (_GameObject is KPlayer)
                        {
                            KPlayer _Player = (KPlayer)_GameObject;

                            // Cứ mỗi lần giết con NPC sẽ được 37 điểm
                            this.UpdateScore(_Player, 37, 0, 0, false);
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.GuildWarManager, "BUG :" + ex.ToString());
            }
        }

        /// <summary>
        /// Đếm số lượng cột trụ
        /// </summary>
        /// <param name="GuildID"></param>
        /// <returns></returns>
        public int CountGuildObjective(int GuildID)
        {
            int COUNT = AllBudding.Values.Where(x => x.Camp == GuildID).Count();

            return COUNT;
        }

        /// <summary>
        /// Khởi tạo công trình khi sựu kiện bắt đầu
        /// </summary>
        public void CreateBuildingWhenStart()
        {
            LogManager.WriteLog(LogTypes.WarCity, "Create CreateBuildingWhenStart!");

            foreach (ObjectivePostion _Object in _WarConfig.ObjectPostion)
            {
                if (_Object.IsMonster == false)
                {
                    // Tìm Bang Thủ THành
                    var FindDef = GuildManager._TotalGuild.Values.Where(x => x.IsMainCity == 1).FirstOrDefault();

                    if (FindDef != null)
                    {
                        this.BeLongGuild = FindDef.GuildId;
                        this.GuildTmpName = FindDef.GuildName;
                    }

                    this.CreateMapObject(_Object, this.BeLongGuild, GuildTmpName);
                }
                else
                {
                    //Còn nếu là quái thì nó sẽ tấn công cả 2 phe
                    CreateMonster(_Object, 1000);
                }
            }
        }

        public bool WaitStartCityFight = false;

        public void ForceStartCityFight()
        {
            //this.CreateBuildingWhenStart();
            GetAttackGuildInfo();
            WaitStartCityFight = true;
        }

        /// <summary>
        /// Xử lý các sự kiện trong battle
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void ProsecBattle(object sender, EventArgs e)
        {
            try
            {
                // Nếu hôm này là ngày đấu lôi đài thì chỉ thực thi sự kiện lôi đài
                if ((ActiveWar.TeamFightDay == ThisDay && !WaitStartCityFight) || WaitStartTeamFightByGmCommand)
                {
                    // Xử lý csc sự kiện liên quan tới ngày lôi đài
                    this.ProseccFightEvent();
                    return;
                }

                // Nếu hôm này là ngày
                if (ActiveWar.CityFightDay == ThisDay || WaitStartCityFight)
                {
                    // Nếu như đang chả có cái state đéo j
                    if (_STATE == GUILD_WAR_STATE.STATUS_NULL)
                    {
                        DateTime Now = DateTime.Now;

                        // Nếu nay là ngày vã nhau
                        if ((Now.Hour == _WarConfig.OpenTime.Hours && Now.Minute == _WarConfig.OpenTime.Minute && _STATE == GUILD_WAR_STATE.STATUS_NULL) || (WaitStartCityFight && _STATE == GUILD_WAR_STATE.STATUS_NULL))
                        {
                            LastTick = TimeUtil.NOW();
                            //Chuyển sang chế độ chuẩn bị bắt đầu
                            _STATE = GUILD_WAR_STATE.STATUS_PREPARSTART;

                            // mở tất cả cổng ở bản đồ
                            OpenAllDor();

                            LogManager.WriteLog(LogTypes.WarCity, "[" + ActiveWar.CityName + "] Battle Change State ==> " + _STATE.ToString());

                            KTGlobal.SendSystemEventNotification("Hoạt động công thành chiến chuẩn bị bắt đầu.Xin mời bang công thành và bang thủ thành tới NPC <color=green>Tiếp Đầu Công Thành Chiến</color> ở các thành để tham gia");
                        }
                    }
                    // Nếu đang trong trạng thái chuẩn bị chiến đấu thì
                    else if (_STATE == GUILD_WAR_STATE.STATUS_PREPARSTART)
                    {
                        // Cứ 5s lại push về client cái thông báo chuẩn bị chiến đấu
                        if (TimeUtil.NOW() - LastNofity >= 5 * 1000 && _STATE == GUILD_WAR_STATE.STATUS_PREPARSTART)
                        {
                            LastNofity = TimeUtil.NOW();

                            UpdatePreading(true);
                        }

                        // Check để chuyển sang trạng thái bắt đầu
                        if (TimeUtil.NOW() >= LastTick + this.SIGN_UP_DULATION * 1000 && _STATE == GUILD_WAR_STATE.STATUS_PREPARSTART)
                        {
                            LastTick = TimeUtil.NOW();

                            // Chuyển sang trạng thái đã bắt đầu

                            _STATE = GUILD_WAR_STATE.STATUS_START;

                            //Khi sự kiện bắt đầu ta khởi tạo cột trụ
                            this.CreateBuildingWhenStart();

                            //TODO : Mở cổng thành cho bọn nó vào

                            ClearAllDor();

                            LogManager.WriteLog(LogTypes.WarCity, " Battle Change State ==> " + _STATE.ToString());
                        }
                    }
                    else if (_STATE == GUILD_WAR_STATE.STATUS_START || _STATE == GUILD_WAR_STATE.STATUS_PREPAREEND)
                    {
                        // Cứ 5s lại push về client cái thông báo chuẩn bị chiến đấu
                        if (TimeUtil.NOW() - LastNofity >= 5 * 1000 && _STATE == GUILD_WAR_STATE.STATUS_START || _STATE == GUILD_WAR_STATE.STATUS_PREPAREEND)
                        {
                            LastNofity = TimeUtil.NOW();

                            // Update liên tục bảng xếp hạng
                            UpdateBXH();

                            // Notitfy Battle report
                            UpdateStartBattle();
                        }

                        // Cứ 60s thì update điểm cho bang nào đang giữ được trụ
                        if (TimeUtil.NOW() - LastUpdateScore >= 60 * 1000 && (_STATE == GUILD_WAR_STATE.STATUS_START || _STATE == GUILD_WAR_STATE.STATUS_PREPAREEND))
                        {
                            LastUpdateScore = TimeUtil.NOW();

                            // Cứ mỗi 60s sẽ cộng điểm cho bang đó tùy vào số trụ chiểm được
                            UpdateScoreForGuildEvery60SEC();
                        }

                        if (TimeUtil.NOW() - LastProtectTick >= 30 * 1000 && (_STATE == GUILD_WAR_STATE.STATUS_START || _STATE == GUILD_WAR_STATE.STATUS_PREPAREEND))
                        {
                            LastProtectTick = TimeUtil.NOW();

                            // Cứ mỗi 30s sẽ cộng điểm bảo vệ cho người chơi và EXP
                            UpdateEXP();
                        }

                        if (TimeUtil.NOW() >= LastTick + ((FIGHT_DULATION * 1000) - (5 * 60 * 1000)) && _STATE == GUILD_WAR_STATE.STATUS_START)
                        {
                            KTGlobal.SendSystemEventNotification("Chỉ còn 5 phút nữa công thành chiến sẽ kết thúc!Các bang hãy mau chóng đẩy lùi quân địch");

                            //Chuyển sang trạng thái chuẩn bị kết thúc
                            _STATE = GUILD_WAR_STATE.STATUS_PREPAREEND;
                            LogManager.WriteLog(LogTypes.WarCity, " Battle Change State ==> " + _STATE.ToString());
                        }

                        if (TimeUtil.NOW() >= LastTick + (FIGHT_DULATION * 1000) && _STATE == GUILD_WAR_STATE.STATUS_PREPAREEND)
                        {
                            LastTick = TimeUtil.NOW();

                            KTGlobal.SendSystemEventNotification("Công thành chiến đã tới hồi kết thúc! Hãy tới NPC <color=yellow>Thống Soái</color> để nhận thưởng");

                            //Xóa toàn bộ cột trụ
                            ClearAllMonster();
                            // Tính toán bảng xếp hạng
                            // Gọi lệnh update bảng xếp hạng lần cuối
                            UpdateBXH();
                            //Set thành chủ cho bang chiến thắng
                            SetMainCityForGuild();
                            // Chuyển sang trạng thái end
                            _STATE = GUILD_WAR_STATE.STATUS_END;

                            LogManager.WriteLog(LogTypes.WarCity, " Battle Change State ==> " + _STATE.ToString());
                        }
                    }
                    else if (_STATE == GUILD_WAR_STATE.STATUS_END)
                    {
                        if (TimeUtil.NOW() - LastNofity >= 5 * 1000 && _STATE == GUILD_WAR_STATE.STATUS_END)
                        {
                            LastNofity = TimeUtil.NOW();

                            // Notitfy Battle report
                            UpdateStartBattle();
                        }
                        if (TimeUtil.NOW() >= LastTick + (GETGIFT_DULATION * 1000) && _STATE == GUILD_WAR_STATE.STATUS_END)
                        {
                            LastTick = TimeUtil.NOW();

                            _STATE = GUILD_WAR_STATE.STATUS_CLEAR;

                            LogManager.WriteLog(LogTypes.SongJinBattle, " Battle Change State ==> " + _STATE.ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.WarCity, "BUG:" + ex.ToString());
            }
        }

        /// <summary>
        /// Set thành chủ cho bọn nó
        /// </summary>
        public void SetMainCityForGuild()
        {
            //Sắp xếp theo thứ tự tăng dần
            var BestGuildSelect = _WarBattle.Values.OrderByDescending(x => x.TotalScore).FirstOrDefault();

            if (BestGuildSelect.Flag == WARFLAG.DEF)
            {
                string MSG = "Trong sự kiện Công Thành Chiến đợt này Bang <color=red>" + BestGuildSelect.GuildName + "</color> đã thành công bảo vệ thành lũy của mình!";
                KTGlobal.SendSystemEventNotification(MSG);
                KTGlobal.SendGuildChat(BestGuildSelect.GuildID, MSG, null, "");
            }
            else if (BestGuildSelect.Flag == WARFLAG.ATTACK)
            {
                int COUNT = AllBudding.Values.Where(x => x.Camp == BestGuildSelect.GuildID).Count();
                if (COUNT > 0)
                {
                    string MSG = "Trong sự kiện Công Thành Chiến đợt này Bang <color=red>" + BestGuildSelect.GuildName + "</color> đã chiếm được thành <color=green>" + ActiveWar.CityName + "</color>";

                    //Update vào DB chủ nhân mới của thành lũy
                    GuildManager.getInstance().UpdateCityStatus(BestGuildSelect.GuildID, (int)GUILD_CITY_STATUS.HOSTCITY);

                    MiniGuildInfo GetWinThisGame = GuildManager.getInstance()._GetInfoGuildByGuildID(BestGuildSelect.GuildID);

                    if (GetWinThisGame != null)
                    {
                        GetWinThisGame.IsMainCity = (int)GUILD_CITY_STATUS.HOSTCITY;
                    }

                    var FindGuildDef = _WarBattle.Values.Where(x => x.Flag == WARFLAG.DEF).FirstOrDefault();

                    if (FindGuildDef != null)
                    {
                        //Update 2 phế thằng chủ thành cũ
                        GuildManager.getInstance().UpdateCityStatus(FindGuildDef.GuildID, (int)GUILD_CITY_STATUS.NONE);

                        MiniGuildInfo GetLoseThisGame = GuildManager.getInstance()._GetInfoGuildByGuildID(FindGuildDef.GuildID);
                        if (GetLoseThisGame != null)
                        {
                            GetWinThisGame.IsMainCity = (int)GUILD_CITY_STATUS.NONE;
                        }
                    }
                    KTGlobal.SendSystemEventNotification(MSG);

                    KTGlobal.SendGuildChat(BestGuildSelect.GuildID, MSG, null, "");
                }
                else
                {
                    var FindGuildDef = _WarBattle.Values.Where(x => x.Flag == WARFLAG.DEF).FirstOrDefault();

                    if (FindGuildDef != null)
                    {
                        string MSG = "Trong sự kiện Công Thành Chiến đợt này Bang <color=red>" + FindGuildDef.GuildName + "</color> đã thành công bảo vệ thành lũy của mình!";
                        KTGlobal.SendSystemEventNotification(MSG);
                        KTGlobal.SendGuildChat(FindGuildDef.GuildID, MSG, null, "");

                        string MSG2 = "Thật đáng tiếc mặc dù có tích lũy cao hơn nhưng bang <color=red>" + BestGuildSelect.GuildName + "</color> không thể bảo vệ trụ tới giât phút cuối cùng,thành lũy vẫn thuộc về bang <color=red>" + FindGuildDef.GuildName + "</color>";
                        KTGlobal.SendSystemEventNotification(MSG2);
                        KTGlobal.SendGuildChat(BestGuildSelect.GuildID, MSG2, null, "");
                    }
                    else
                    {
                        string MSG2 = "Thật đáng tiếc mặc dù có tích lũy cao hơn nhưng bang <color=red>" + BestGuildSelect.GuildName + "</color> không thể bảo vệ trụ tới giât phút cuối cùng.Thành vẫn chưa được đánh chiếm!";
                        KTGlobal.SendSystemEventNotification(MSG2);
                        KTGlobal.SendGuildChat(BestGuildSelect.GuildID, MSG2, null, "");
                    }
                }
            }
        }

        /// <summary>
        /// Update exp từng cho thằng nào bảo vệ cột
        /// </summary>
        public void UpdateEXP()
        {
            try
            {
                foreach (Monster _Monster in AllBudding.Values.ToList())
                {
                    // Ông nào đứng gần trụ
                    List<KPlayer> friends = KTGlobal.GetNearByPeacePlayers(_Monster, 200);

                    foreach (KPlayer _Player in friends)
                    {
                        if (_Player.Camp == _Monster.Camp)
                        {
                            string NotifyProtect = "Ngươi có công bảo vệ long trụ, nhận được <color=yellow>5</color> điểm tích lũy và 1600 EXP";

                            KTPlayerManager.ShowNotification(_Player, NotifyProtect);

                            KTPlayerManager.AddExp(_Player, 16000);

                            this.UpdateScore(_Player, 5, 0, 0, false);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.GuildWarManager, "BUG :" + ex.ToString());
            }
        }

        /// <summary>
        /// Update score
        /// </summary>
        /// <param name="_Player"></param>
        /// <param name="Score"></param>
        /// <param name="DesotryCount"></param>
        /// <param name="KillCount"></param>
        /// <param name="IsRessetStreak"></param>
        public void UpdateScore(KPlayer _Player, int Score, int DesotryCount, int KillCount, bool IsRessetStreak)
        {
            int GuildID = _Player.GuildID;

            try
            {
              //  Console.WriteLine("Update SCORE :" + _Player.RoleName + "| POINT :" + Score);

                AttackDict.TryGetValue(_Player.RoleID, out GuildWarPlayer find);

                if (find == null)
                {
                    DefDict.TryGetValue(_Player.RoleID, out find);
                }

                if (find != null)
                {
                    if (find != null)
                    {
                        // Cộng điểm cho thằng người chơi này
                        find.Score = find.Score + Score;
                        // Nếu như có dấu hiệu phá hủy
                        if (DesotryCount > 0)
                        {
                            // Cộng tích lũy công trình
                            find.DestroyCount = find.DestroyCount + DesotryCount;
                        }
                        // Nếu như có ghi nhận giết người
                        if (KillCount > 0)
                        {
                            // Cập số người đã giết
                            find.KillCount = find.KillCount + KillCount;
                        }
                        // nếu bị reset Steak
                        if (IsRessetStreak)
                        {
                            // Thì thực hiện reset lại streak
                            find.CurentKillSteak = 0;
                        }
                        else
                        {
                            find.CurentKillSteak = find.CurentKillSteak + KillCount;

                            if (find.CurentKillSteak > find.MaxKillSteak)
                            {
                                find.MaxKillSteak = find.CurentKillSteak;
                            }

                            if (find.CurentKillSteak > 3)
                            {
                                SendKillStreak(_Player, find.CurentKillSteak);
                            }
                        }

                        // Lấy ra danh hiệu của người chơi có thể nhận được
                        string Title = GetRankTitleByPoint(find.Score);

                        if (_Player.TempTitle != Title)
                        {
                            //Sét lại danh hiệu cho người chơi
                            _Player.TempTitle = Title;
                        }

                        int Index = -1;
                        // Nếu thằng này ở phe tấn công
                        if (find.Flag == WARFLAG.ATTACK)
                        {
                            lock (TMP_RANK_ATTACK)
                            {
                                Index = TMP_RANK_ATTACK.FindIndex(x => x._Player.RoleID == find._Player.RoleID);
                            }
                        }
                        // nếu thằng này ở phe thủ
                        if (find.Flag == WARFLAG.DEF)
                        {
                            lock (TMP_RANK_DEF)
                            {
                                Index = TMP_RANK_DEF.FindIndex(x => x._Player.RoleID == find._Player.RoleID);
                            }
                        }
                        //  Tạo ra 1 cái TMP để lấy thứ tự

                        find.CurentRank = Index + 1;

                        // TODO : UPDATE TILE RANK FOR PLAYER

                        SendNotifyLoop(find);
                    }
                }

                //Console.WriteLine("Update SCORE DONE :" + _Player.RoleName + "| POINT :" + Score);
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.GuildWarManager, "UpdateScore TOÁC :" + ex.ToString());
            }

            //  Console.WriteLine("Update SCORE :" + _Player.RoleName + "| POINT :" + Score);
        }

        /// <summary>
        /// Reset Battle
        /// </summary>
        public void ResetBattle()
        {
            LastTick = 0;

            _STATE = GUILD_WAR_STATE.STATUS_NULL;
        }

        #endregion _BattleProsecc

        #region Unity

        /// <summary>
        /// Gửi về client liên trảm
        /// </summary>
        /// <param name="InputPlayer"></param>
        /// <param name="Count"></param>
        public void SendKillStreak(KPlayer InputPlayer, int Count)
        {
            G2C_KillStreak _State = new G2C_KillStreak();

            _State.KillNumber = Count;

            if (InputPlayer.IsOnline())
            {
                InputPlayer.SendPacket<G2C_KillStreak>((int)TCPGameServerCmds.CMD_KT_KILLSTREAK, _State);
            }
        }

        #endregion Unity

        #region PointDef

        /// <summary>
        /// Số điểm tối đa mà 1 người có thể nhận dựa trên tích lũy của thằng đối phương
        /// </summary>
        /// <param name="CurentScore"></param>
        /// <returns></returns>
        public int GetPointCanGet(int CurentScore)
        {
            int Point = 0;

            if (CurentScore >= 0 && CurentScore < 2000)
            {
                Point = 37;
            }
            else if (CurentScore >= 2000 && CurentScore < 6000)
            {
                Point = 75;
            }
            else if (CurentScore >= 6000 && CurentScore < 8000)
            {
                Point = 100;
            }
            else if (CurentScore >= 8000 && CurentScore < 10000)
            {
                Point = 150;
            }
            else if (CurentScore >= 10000)
            {
                Point = 300;
            }

            return Point;
        }

        /// <summary>
        /// Tham gia vào trận chiến
        /// </summary>
        /// <param name="client"></param>
        public void JoinBattle(KPlayer client, NPC npc)
        {
            if (ThisDay != ActiveWar.CityFightDay && !WaitStartCityFight)
            {
                KTLuaLib_Player.SendMSG("Lúc này chưa phải thời điểm công thành.Hãy quay lại sau", client, npc);
                return;
            }

            if (client.GuildID <= 0)
            {
                KTLuaLib_Player.SendMSG("Phải có bang hội mới có thể tham gia sự kiện này", client, npc);
                return;
            }

            if (!_WarBattle.ContainsKey(client.GuildID))
            {
                KTLuaLib_Player.SendMSG("Bang của bạn phải ở trong bang thủ hoặc bang công thành mới có thể tham gia sự kiện", client, npc);
                return;
            }

            if (_STATE == GUILD_WAR_STATE.STATUS_NULL || _STATE == GUILD_WAR_STATE.STATUS_END || _STATE == GUILD_WAR_STATE.STATUS_CLEAR)
            {
                KTLuaLib_Player.SendMSG("Sự kiện đã kết thúc!Hãy quay lại vào lần sau", client, npc);
                return;
            }
            ///Nếu có thì ta sẽ làm như sau
            GuildWarBattleReport _Report = _WarBattle[client.GuildID];

            if (_Report != null)
            {
                WARFLAG _Flag = _Report.Flag;

                int FightMapCode = ActiveWar.FightMapID;

                GuildWarPlayer guildWarPlayer = new GuildWarPlayer();
                guildWarPlayer.Flag = _Flag;
                guildWarPlayer.KillCount = 0;
                guildWarPlayer.MaxKillSteak = 0;
                guildWarPlayer.GuildID = client.GuildID;
                guildWarPlayer.DestroyCount = 0;
                guildWarPlayer.CurentKillSteak = 0;
                guildWarPlayer.KillCount = 0;
                guildWarPlayer.IsReviceReward = false;
                guildWarPlayer._Player = client;
                guildWarPlayer.Score = 0;
                guildWarPlayer.CurentRank = 0;

                // Nếu thằng này ở phe tấn công
                if (_Flag == WARFLAG.ATTACK)
                {
                    if (!AttackDict.TryGetValue(client.RoleID, out GuildWarPlayer _OutPlayer))
                    {
                        // nếu thằng này chưa tham gia thì ta sẽ cho nó vào dict
                        AttackDict.TryAdd(client.RoleID, guildWarPlayer);
                    }
                    else
                    {
                        // Nếu đã tham gia trước đó thì set lại cho thằng này
                        _OutPlayer._Player = client;
                    }
                    // Ghi lại vị trí trước khi vào
                    KT_TCPHandler.UpdateLastMapInfo(client, client.CurrentMapCode, (int)client.CurrentPos.X, (int)client.CurrentPos.Y);
                    // Chuyển người chơi vào map
                    // Thực hiện dịch thằng này vào bản đồ
                    // Lấy ra 1 điểm vào cố định
                    RespawnPoint Point = _WarConfig.AttackPoint[KTGlobal.GetRandomNumber(0, _WarConfig.AttackPoint.Count - 1)];
                    //Chuyển thằng này vào bản đồ này
                    KTPlayerManager.ChangeMap(client, FightMapCode, Point.PosX, Point.PosY);
                }
                else if (_Flag == WARFLAG.DEF)
                {
                    if (!DefDict.TryGetValue(client.RoleID, out GuildWarPlayer _OutPlayer))
                    {
                        // nếu thằng này chưa tham gia thì ta sẽ cho nó vào dict
                        DefDict.TryAdd(client.RoleID, guildWarPlayer);
                    }
                    else
                    {
                        // Nếu đã tham gia trước đó thì set lại cho thằng này
                        _OutPlayer._Player = client;
                    }
                    // Ghi lại vị trí trước khi vào
                    KT_TCPHandler.UpdateLastMapInfo(client, client.CurrentMapCode, (int)client.CurrentPos.X, (int)client.CurrentPos.Y);
                    // Chuyển người chơi vào map
                    // Thực hiện dịch thằng này vào bản đồ
                    // Lấy ra 1 điểm vào cố định
                    RespawnPoint Point = _WarConfig.DefPoint[KTGlobal.GetRandomNumber(0, _WarConfig.AttackPoint.Count - 1)];
                    //Chuyển thằng này vào bản đồ này
                    KTPlayerManager.ChangeMap(client, FightMapCode, Point.PosX, Point.PosY);
                }
            }
        }

        /// <summary>
        /// NPc của mỗi bên
        /// </summary>
        /// <param name="npc"></param>
        /// <param name="client"></param>
        public void NpcSupport(NPC npc, KPlayer client)
        {
            KNPCDialog _NpcDialog = new KNPCDialog();

            Dictionary<int, string> Selections = new Dictionary<int, string>();

            string STATUS = "";

            string ATTACK = "";

            string DEF = "";

            var FindAttack = GuildManager._TotalGuild.Values.Where(x => x.IsMainCity == 3).FirstOrDefault();

            var FindDef = GuildManager._TotalGuild.Values.Where(x => x.IsMainCity == 1).FirstOrDefault();

            if (FindAttack != null)
            {
                ATTACK = "BANG TẤN CÔNG : <color=yellow>" + FindAttack.GuildName + "</color></b>";
            }
            else
            {
                ATTACK = "BANG TẤN CÔNG : <color=yellow>CHƯA CÓ</color></b>";
            }

            if (FindDef != null)
            {
                DEF = "BANG THỦ : <color=yellow>" + FindDef.GuildName + "</color></b>";
            }
            else
            {
                DEF = "BANG THỦ : <color=yellow>HỆ THỐNG</color></b>";
            }
            // Nếu công thành chiến chưa bắt đầu
            if (_STATE == GUILD_WAR_STATE.STATUS_NULL)
            {
                STATUS = "TRẠNG THÁI : <b><color=red>CHƯA BẮT ĐẦU</color></b>";
            }
            else if (_STATE == GUILD_WAR_STATE.STATUS_PREPARSTART)
            {
                STATUS = "TRẠNG THÁI : <b><color=green>CHUẨN BỊ KHAI CHIẾN</color></b>";
            }
            else if (_STATE == GUILD_WAR_STATE.STATUS_START)
            {
                STATUS = "TRẠNG THÁI : <b><color=green>ĐANG DIỄN RA</color></b>";
            }
            else if (_STATE == GUILD_WAR_STATE.STATUS_PREPAREEND)
            {
                STATUS = "TRẠNG THÁI : <b><color=yellow>SẮP KẾT THÚC</color></b>";
            }

            _NpcDialog.Text = STATUS + "\n" + ATTACK + "\n" + DEF + "\n==========================\n<color=yellow>Thống Soái</color> Ta có thể giúp gì được cho người?Chiến trường nơi đây chỉ những người thật sự dũng cảm anh hùng mới có thể thống nhất được thiên hạ!";

            Selections.Add(1, "Rơi khỏi chiến trường!");

            if (npc.ResID == 20003)
            {
                Selections.Add(3, "Triệu hồi Thần Thú Bảo Vệ");
            }

            if (npc.ResID == 20004)
            {
                Selections.Add(4, "Triệu hồi Xe Công Thành");
            }
            // Nếu mà đã kết thúc sự kiện
            if (_STATE == GUILD_WAR_STATE.STATUS_END)
            {
                Selections.Add(2, "Nhận Thưởng");
            }

            // Nếu ở trạng thái chuẩn bị bắt đầu
            Selections.Add(1000, "Ta sẽ quay lại sau!");

            //Selections.Add(-2, "Ta muốn rời khỏi chiến trường");

            Action<TaskCallBack> ActionWork = (x) => DoActionSupport(client, npc, x);

            _NpcDialog.OnSelect = ActionWork;

            _NpcDialog.Selections = Selections;

            //_NpcDialog.Text = Text;

            _NpcDialog.Show(npc, client);
        }

        /// <summary>
        /// Call SupportMonster
        /// </summary>
        /// <param name="TMP"></param>
        public void CallSupportMonster(GuildWarPlayer TMP, NPC npc)
        {
            MiniGuildInfo GetWinThisGame = GuildManager.getInstance()._GetInfoGuildByGuildID(TMP.GuildID);

            if (TMP.Flag != WARFLAG.DEF)
            {
                KTLuaLib_Player.SendMSG("Chỉ có bang thủ thành mới có thể triệu hồi thần thú!", TMP._Player, npc);
                return;
            }

            if (TMP._Player.GuildRank != (int)GuildRank.Master)
            {
                KTLuaLib_Player.SendMSG("Chỉ có bang chủ mới có thể triệu hồi thần thú!", TMP._Player, npc);
                return;
            }
            if (GetWinThisGame == null)
            {
                KTLuaLib_Player.SendMSG("Không tìm thấy thông bang hội!", TMP._Player, npc);
                return;
            }


            if (GetWinThisGame != null)
            {
                if (GetWinThisGame.GuildMoney < _WarConfig.CostPrice.DefCost)
                {
                    KTLuaLib_Player.SendMSG("Bang cống không đủ để triệu hồi thần thú yêu cầu 1000 Vạn để triệu hồi!", TMP._Player, npc);
                    return;
                }
            }
            if (_STATE != GUILD_WAR_STATE.STATUS_START && _STATE != GUILD_WAR_STATE.STATUS_PREPAREEND)
            {
                KTLuaLib_Player.SendMSG("Không thể triệu hồi vào thời gian này!", TMP._Player, npc);
                return;
            }
            int PointLess = GetWinThisGame.GuildMoney - _WarConfig.CostPrice.DefCost;
            //Set Lại tiền cho bang
            GetWinThisGame.GuildMoney = PointLess;

            if (GuildManager.UpdateGuildResource(GetWinThisGame.GuildId, GUILD_RESOURCE.GUILD_MONEY, PointLess + ""))
            {
                try
                {
                    MonsterAIType aiType = MonsterAIType.Boss;

                    /// Ngũ hành
                    KE_SERIES_TYPE series = KE_SERIES_TYPE.series_none;

                    int RandomSeri = KTGlobal.GetRandomNumber(1, 5);
                    series = (KE_SERIES_TYPE)RandomSeri;

                    /// Hướng quay
                    KiemThe.Entities.Direction dir = KiemThe.Entities.Direction.NONE;

                    int RandomDir = KTGlobal.GetRandomNumber(0, 7);

                    dir = (KiemThe.Entities.Direction)RandomDir;

                    string TITLE = "Thần Thú Bảo Vệ";

                    int Random = new Random().Next(0, 100);

                    foreach (DefMonster _Object in _WarConfig.DefMonster)
                    {
                        Monster monster = KTMonsterManager.Create(new KTMonsterManager.DynamicMonsterBuilder()
                        {
                            MapCode = ActiveWar.FightMapID,
                            ResID = _Object.ID,
                            PosX = _Object.PosX,
                            PosY = _Object.PosY,
                            Name = _Object.Name,
                            Title = TITLE,
                            MaxHP = _Object.Hp,
                            Level = 90,
                            MonsterType = aiType,
                            Camp = TMP.GuildID,
                            RespawnTick = -1,
                            
                        });

                        if (monster != null)
                        {
                            monster.OnDieCallback = (_GameObject) =>
                            {
                                if (_STATE == GUILD_WAR_STATE.STATUS_START || _STATE == GUILD_WAR_STATE.STATUS_PREPAREEND)
                                {
                                    if (_GameObject is KPlayer)
                                    {
                                        KPlayer _Player = (KPlayer)_GameObject;

                                        // Cứ mỗi lần giết con NPC sẽ được 37 điểm
                                        this.UpdateScore(_Player, 100, 0, 0, false);

                                        string MSG = KTGlobal.CreateStringByColor("Thần thú bảo vệ đã bị <color=red>" + _Player.RoleName + "</color> tiêu diệt", ColorType.Importal);

                                        KTGlobal.SendGuildChat(monster.Camp, MSG, null, "");
                                        //Thông báo cho bang kia là đã bị đánh sập
                                        KTGlobal.SendGuildChat(_Player.GuildID, MSG, null, "");
                                    }
                                }
                            };
                        }
                    }

                    KTGlobal.SendGuildChat(TMP.GuildID, "<color=red>Thần thú</color> bảo vệ đã được Bang Hội triệu hồi thành công!", null, "");
                }
                catch (Exception ex)
                {
                    LogManager.WriteLog(LogTypes.WarCity, "BUG :" + ex.ToString());
                }
            }

        

            KT_TCPHandler.CloseDialog(TMP._Player);
        }


        public void CallAttackMonster(GuildWarPlayer TMP, NPC npc)
        {
            MiniGuildInfo GetWinThisGame = GuildManager.getInstance()._GetInfoGuildByGuildID(TMP.GuildID);

            if (TMP.Flag != WARFLAG.ATTACK)
            {
                KTLuaLib_Player.SendMSG("Chỉ có bang công thành mới có thể triệu hồi Xe Công Thành!", TMP._Player, npc);
                return;
            }

            if (TMP._Player.GuildRank != (int)GuildRank.Master)
            {
                KTLuaLib_Player.SendMSG("Chỉ có bang chủ mới có thể triệu hồi Xe Công Thành!", TMP._Player, npc);
                return;
            }
            if (GetWinThisGame == null)
            {
                KTLuaLib_Player.SendMSG("Không tìm thấy thông bang hội Xe Công Thành!", TMP._Player, npc);
                return;
            }

            if(_STATE!=GUILD_WAR_STATE.STATUS_START && _STATE != GUILD_WAR_STATE.STATUS_PREPAREEND)
            {
                KTLuaLib_Player.SendMSG("Không thể triệu hồi vào thời gian này!", TMP._Player, npc);
                return;
            }    

            if (GetWinThisGame != null)
            {
                if (GetWinThisGame.GuildMoney < _WarConfig.CostPrice.AttackCost)
                {
                    KTLuaLib_Player.SendMSG("Bang cống không đủ để triệu hồi Xe Công Thành yêu cầu 1000 Vạn để triệu hồi!", TMP._Player, npc);
                    return;
                }
            }

            int PointLess = GetWinThisGame.GuildMoney - _WarConfig.CostPrice.AttackCost;
            //Set Lại tiền cho bang
            GetWinThisGame.GuildMoney = PointLess;

            if (GuildManager.UpdateGuildResource(GetWinThisGame.GuildId, GUILD_RESOURCE.GUILD_MONEY, PointLess + ""))
            {
                try
                {
                    MonsterAIType aiType = MonsterAIType.Special_Boss;

                    /// Ngũ hành
                    KE_SERIES_TYPE series = KE_SERIES_TYPE.series_none;

                    int RandomSeri = KTGlobal.GetRandomNumber(1, 5);
                    series = (KE_SERIES_TYPE)RandomSeri;

                    /// Hướng quay
                    KiemThe.Entities.Direction dir = KiemThe.Entities.Direction.NONE;

                    int RandomDir = KTGlobal.GetRandomNumber(0, 7);

                    dir = (KiemThe.Entities.Direction)RandomDir;

                    string TITLE = "Xích Hỏa Diễm";

                    int Random = new Random().Next(0, 100);

                    foreach (AttackMonster _Object in _WarConfig.AttackMonster)
                    {
                        /// Tạo quái
                        Monster monster = KTMonsterManager.Create(new KTMonsterManager.DynamicMonsterBuilder()
                        {
                            MapCode = ActiveWar.FightMapID,
                            ResID = _Object.ID,
                            PosX = _Object.PosX,
                            PosY = _Object.PosY,
                            Name = _Object.Name,
                            Title = TITLE,
                            MaxHP = _Object.Hp,
                            Level = 90,
                            MonsterType = aiType,
                            Camp = TMP.GuildID,
                            RespawnTick = -1,
                           
                        });

                        if (monster != null)
                        {
                            monster.OnDieCallback = (_GameObject) =>
                            {
                                if (_STATE == GUILD_WAR_STATE.STATUS_START || _STATE == GUILD_WAR_STATE.STATUS_PREPAREEND)
                                {
                                    if (_GameObject is KPlayer)
                                    {
                                        KPlayer _Player = (KPlayer)_GameObject;

                                        // Cứ mỗi lần giết con NPC sẽ được 37 điểm
                                        this.UpdateScore(_Player, 100, 0, 0, false);

                                        string MSG = KTGlobal.CreateStringByColor("Xe công thành đã bị <color=red>" + _Player.RoleName + "</color> tiêu diệt", ColorType.Importal);

                                        KTGlobal.SendGuildChat(monster.Camp, MSG, null, "");
                                        //Thông báo cho bang kia là đã bị đánh sập
                                        KTGlobal.SendGuildChat(_Player.GuildID, MSG, null, "");
                                    }
                                }
                            };
                        }

                        /// Sử dụng thuật toán A* tìm đường
                        monster.UseAStarPathFinder = false;
                        /// Thực hiện di chuyển đến vị trí tương ứng
                        monster.MoveTo(new System.Windows.Point(_Object.TargetPosX, _Object.TargetPosY), true);

                        /// Đánh dấu đã đến đích chưa
                        bool destinationReached = false;
                        /// Sự kiện Tick
                        monster.OnTick = () =>
                        {
                            /// Nếu đã đến đích
                            if (destinationReached)
                            {
                                /// Bỏ qua
                                return;
                            }

                            /// Cập nhật vị trí đứng mới
                            monster.StartPos = monster.CurrentPos;
                            /// Nếu trong 2s chịu sát thương
                            if (KTGlobal.GetCurrentTimeMilis() - monster.LastBeHitTicks < 2000)
                            {
                                /// Bỏ qua
                                return;
                            }

                            /// Đã đến đích
                            if (KTGlobal.GetDistanceBetweenPoints(new System.Windows.Point(_Object.TargetPosX, _Object.TargetPosY), monster.CurrentPos) <= 10)
                            {
                                /// Đánh dấu đã đến đích
                                destinationReached = true;
                                /// Bỏ qua
                                return;
                            }

                            /// Nếu không có người chơi xung quanh và đang không di chuyển
                            if (!monster.IsChasingTarget && !monster.IsMoving)
                            {
                                /// Thực hiện di chuyển đến vị trí tương ứng
                                monster.MoveTo(new System.Windows.Point(_Object.TargetPosX, _Object.TargetPosY), true);
                            }
                        };
                    }

                    KTGlobal.SendGuildChat(TMP.GuildID, "<color=red>Xe Công Thành</color> đã được Bang Hội triệu hồi thành công!", null, "");
                }
                catch (Exception ex)
                {
                    LogManager.WriteLog(LogTypes.GuildWarManager, "BUG :" + ex.ToString());
                }
            }

            KT_TCPHandler.CloseDialog(TMP._Player);
        }

        /// <summary>
        /// Code supprt
        /// </summary>
        /// <param name="client"></param>
        /// <param name="npc"></param>
        /// <param name="x"></param>
        private void DoActionSupport(KPlayer client, NPC npc, TaskCallBack x)
        {
            if (x.SelectID == 1)
            {
                _WarBattle.TryGetValue(client.GuildID, out GuildWarBattleReport _Report);

                GuildWarPlayer TMP = null;

                if (_Report != null)
                {
                    if (_Report.Flag == WARFLAG.DEF)
                    {
                        DefDict.TryGetValue(client.RoleID, out TMP);
                    }
                    else if (_Report.Flag == WARFLAG.ATTACK)
                    {
                        AttackDict.TryGetValue(client.RoleID, out TMP);
                    }
                }

                KT_TCPHandler.CloseDialog(client);
                // Nếu có tích lũy

                if (TMP != null)
                {
                    if (TMP.Score > 0)
                    {
                        string MSG = "";
                        if (_STATE != GUILD_WAR_STATE.STATUS_END)
                        {
                            MSG = "Chiến trường chưa kết thúc nếu người rời đi điểm tích lũy sẽ không được tính!";
                        }
                        else
                        {
                            MSG = "Ngươi có chắc muốn rời đi?Ta khuyên ngươi hãy nhận thưởng trước!";
                        }

                        KTPlayerManager.ShowMessageBox(client, "Chú ý ", MSG, new Action(() => MoveOutConfirm(TMP)), true);
                    }
                    else // Nếu không có tích lũy thì chim cút luôn
                    {
                        MoveOutConfirm(TMP);
                    }
                }
            }
            else if (x.SelectID == 2)
            {
                this.ReviceGift(client, npc);
            }
            else if (x.SelectID == 3)
            {
                _WarBattle.TryGetValue(client.GuildID, out GuildWarBattleReport _Report);

                GuildWarPlayer TMP = null;

                if (_Report != null)
                {
                    if (_Report.Flag == WARFLAG.DEF)
                    {
                        DefDict.TryGetValue(client.RoleID, out TMP);
                    }
                    else if (_Report.Flag == WARFLAG.ATTACK)
                    {
                        AttackDict.TryGetValue(client.RoleID, out TMP);
                    }
                }
                if (TMP != null)
                {
                    KTPlayerManager.ShowMessageBox(client, "Chú ý ", "Triệu hồi thần thú sẽ tiêu tốn "+ _WarConfig.CostPrice.DefCost + " Bang Cống", new Action(() => CallSupportMonster(TMP, npc)), true);
                }
            }
            else if (x.SelectID == 4)
            {
                _WarBattle.TryGetValue(client.GuildID, out GuildWarBattleReport _Report);

                GuildWarPlayer TMP = null;

                if (_Report != null)
                {
                    if (_Report.Flag == WARFLAG.DEF)
                    {
                        DefDict.TryGetValue(client.RoleID, out TMP);
                    }
                    else if (_Report.Flag == WARFLAG.ATTACK)
                    {
                        AttackDict.TryGetValue(client.RoleID, out TMP);
                    }
                }
                if (TMP != null)
                {
                    KTPlayerManager.ShowMessageBox(client, "Chú ý ", "Triệu hồi Xe Công Thành sẽ tiêu tốn "+ _WarConfig.CostPrice.AttackCost + " Bang Cống", new Action(() => CallAttackMonster(TMP, npc)), true);
                }
            }
            else if (x.SelectID == 1000)
            {
                KT_TCPHandler.CloseDialog(client);
            }
        }

        /// <summary>
        /// Move thằng này ra khỏi chiến trường
        /// </summary>
        /// <param name="PlayerBattle"></param>
        public void MoveOutConfirm(GuildWarPlayer PlayerBattle)
        {
            KT_TCPHandler.GetLastMapInfo(PlayerBattle._Player, out int preMapCode, out int prePosX, out int prePosY);

            KTPlayerManager.ChangeMap(PlayerBattle._Player, preMapCode, prePosX, prePosY);
        }

        /// <summary>
        /// Npc quản lý việc tham gia sự kiện công thành chiến
        /// </summary>
        /// <param name="npc"></param>
        /// <param name="client"></param>
        public void NpcClickJoin(NPC npc, KPlayer client)
        {
            KNPCDialog _NpcDialog = new KNPCDialog();

            Dictionary<int, string> Selections = new Dictionary<int, string>();

            string STATUS = "";

            string ATTACK = "";

            string DEF = "";

            var FindAttack = GuildManager._TotalGuild.Values.Where(x => x.IsMainCity == 3).FirstOrDefault();

            var FindDef = GuildManager._TotalGuild.Values.Where(x => x.IsMainCity == 1).FirstOrDefault();

            if (FindAttack != null)
            {
                ATTACK = "BANG TẤN CÔNG : <color=yellow>" + FindAttack.GuildName + "</color></b>";
            }
            else
            {
                ATTACK = "BANG TẤN CÔNG : <color=yellow>CHƯA CÓ</color></b>";
            }

            if (FindDef != null)
            {
                DEF = "BANG THỦ : <color=yellow>" + FindDef.GuildName + "</color></b>";
            }
            else
            {
                DEF = "BANG THỦ : <color=yellow>HỆ THỐNG</color></b>";
            }
            // Nếu công thành chiến chưa bắt đầu
            if (_STATE == GUILD_WAR_STATE.STATUS_NULL)
            {
                STATUS = "TRẠNG THÁI : <b><color=red>CHƯA BẮT ĐẦU</color></b>";
            }
            else if (_STATE == GUILD_WAR_STATE.STATUS_PREPARSTART)
            {
                STATUS = "TRẠNG THÁI : <b><color=green>CHUẨN BỊ KHAI CHIẾN</color></b>";
            }
            else if (_STATE == GUILD_WAR_STATE.STATUS_START)
            {
                STATUS = "TRẠNG THÁI : <b><color=green>ĐANG DIỄN RA</color></b>";
            }
            else if (_STATE == GUILD_WAR_STATE.STATUS_PREPAREEND)
            {
                STATUS = "TRẠNG THÁI : <b><color=yellow>SẮP KẾT THÚC</color></b>";
            }

            _NpcDialog.Text = STATUS + "\n" + ATTACK + "\n" + DEF + "\n==========================\n<color=yellow>Công Thành Chiến</color> là tính năng được các bằng hữu mong chờ nhất của <color=green>Quy Vân Mobile</color> Sau đây là các thông tin và hướng dẫn liên quan đến tính năng này :\r\n<color=yellow>- Điều Kiện:</color> Bang hội có cấp 4 trở lên và có ít nhất 6 thành viên cấp 90 đã gia nhập môn phái.\r\n<color=yellow>- Cách Thức Tham Gia : </color> Đăng ký báo danh lôi đài trong mục C.T.Chiến trong giao diện bang hội,Chọn ra 6 người để lập chiến đội đăng ký tham gia\r\n<color=red>Thời gian đăng ký lôi đài:</color> Từ T2 đến 23H59 T6 hàng tuần.\r\n<color=red>Thời gian diễn ra lôi đài:</color> 20H00 T7 hàng tuần\r\n- Thi đấu theo thể thức loại trực tiếp 6 vs 6.\r\n- Đội quán quân trong vòng thi đấu Lôi Đài sẽ được quyền Công Thành.\r\n<color=red>Thời gian diễn ra công thành:</color> 20H00 Chủ Nhật hàng tuần\r\nCông Thành Tam Trụ được tính theo thể thức Tích Lũy và Chiếm trụ.\r\n<color=red>- Đối với bang công:</color> Cần chiếm đóng ít nhất một Long Trụ và có Tổng Tích Lũy cao hơn bên thủ sẽ công thành công .\r\n<color=red>- Đối với bang thủ:</color> Tổng Tích Lũy toàn trận cao hơn bang công sẽ thủ thành công.";

            Selections.Add(1, "Tham gia công thành!");

         

            // Nếu ở trạng thái chuẩn bị bắt đầu
            Selections.Add(1000, "Ta sẽ quay lại sau!");

            //Selections.Add(-2, "Ta muốn rời khỏi chiến trường");

            Action<TaskCallBack> ActionWork = (x) => DoActionSelect(client, npc, x);

            _NpcDialog.OnSelect = ActionWork;

            _NpcDialog.Selections = Selections;

            //_NpcDialog.Text = Text;

            _NpcDialog.Show(npc, client);
        }

        /// <summary>
        /// Xóa all monster
        /// </summary>
        public void ClearAllMonster()
        {
            try
            {
                foreach (Monster monster in AllBudding.Values.ToList())
                {
                    KTMonsterManager.Remove(monster);
                }
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.GuildWarManager, "BUG :" + ex.ToString());
            }
        }

        /// <summary>
        /// Swich STATE của người chơi sang
        /// </summary>
        /// <param name="Player"></param>
        /// <param name="State"></param>
        public static void PlayChangeState(KPlayer Player, int State)
        {
            G2C_EventState _State = new G2C_EventState();

            _State.EventID = 50;
            _State.State = State;
            if (Player.IsOnline())
            {
                Player.SendPacket<G2C_EventState>((int)TCPGameServerCmds.CMD_KT_EVENT_STATE, _State);
            }
            else
            {
                //Console.WriteLine("OFFLINE");
            }
        }

        /// <summary>
        /// Chọn cái j đó
        /// </summary>
        /// <param name="client"></param>
        /// <param name="npc"></param>
        /// <param name="x"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void DoActionSelect(KPlayer client, NPC npc, TaskCallBack x)
        {
            if (x.SelectID == 1)
            {
                this.JoinBattle(client, npc);
            }
            else if (x.SelectID == 2)
            {
                this.ReviceGift(client, npc);
            }
            else if (x.SelectID == 1000)
            {
                KT_TCPHandler.CloseDialog(client);
            }
        }

        /// <summary>
        /// Lấy ra exp nhận được
        /// </summary>
        /// <param name="nLevel"></param>
        /// <param name="nBouns"></param>
        /// <returns></returns>
        public int GetExpReward(double nLevel, double nBouns)
        {
            double Exp = 0;

            Exp = Math.Floor((700 + Math.Floor((nLevel - 40) / 5) * 100) * 60 * 7 / 3000) * nBouns;

            return (int)Exp;
        }

        /// <summary>
        /// Code nốt hàm nhận thưởng ở đây
        /// </summary>
        /// <param name="client"></param>
        /// <param name="npc"></param>
        public void ReviceGift(KPlayer client, NPC npc)
        {
            if (!_WarBattle.ContainsKey(client.GuildID))
            {
                KTLuaLib_Player.SendMSG("Người không nằm trong bang thủ thành hay bang công thành!", client, npc);
                return;
            }

            _WarBattle.TryGetValue(client.GuildID, out GuildWarBattleReport _Report);

            GuildWarPlayer TMP = null;

            if (_Report != null)
            {
                if (_Report.Flag == WARFLAG.DEF)
                {
                    DefDict.TryGetValue(client.RoleID, out TMP);
                }
                else if (_Report.Flag == WARFLAG.ATTACK)
                {
                    AttackDict.TryGetValue(client.RoleID, out TMP);
                }
            }
            else
            {
                KTLuaLib_Player.SendMSG("Bạn đã không tham sự kiện công thành chiến lần này!", client, npc);
                return;
            }

            if (TMP == null)
            {
                KTLuaLib_Player.SendMSG("Bạn đã không tham sự kiện công thành chiến lần này!", client, npc);
                return;
            }

            if (TMP.IsReviceReward)
            {
                KTLuaLib_Player.SendMSG("Bạn đã nhận thưởng rồi!", client, npc);
                return;
            }

            int Index = GetPositionInBxh(TMP);

            if (Index == -1)
            {
                KTLuaLib_Player.SendMSG("Xếp hạng của bạn không đủ để nhận thưởng!", client, npc);
                return;
            }
            else
            {
                Index = +1;
            }

            KNPCDialog _NpcDialog = new KNPCDialog();

            int TotalExpCanGet = GetExpReward(client.m_Level, TMP.Score);

            var FindIndex = _WarConfig.Award.Where(x => x.Rank == Index).FirstOrDefault();
            if (FindIndex == null)
            {
                FindIndex = _WarConfig.Award.Where(x => x.Rank == 100).FirstOrDefault();
            }

            string BUILD = "";

            int ItemID = Int32.Parse(FindIndex.BoxCount.Split('|')[0]);
            int Number = Int32.Parse(FindIndex.BoxCount.Split('|')[1]);

            ItemData Tmp = ItemManager.GetItemTemplate(ItemID);

            if (Tmp != null)
            {
                BUILD += "<color=green>Vật phẩm : </color><color=yellow>" + Tmp.Name + "x" + Number + "</color>\n";
            }

            BUILD += "<color=green>Điểm hoạt động tuần : " + FindIndex.WeekPoint + "</color>\n";

            BUILD += "<color=green>Điểm cống hiến cá nhân : " + FindIndex.GuildMoney + "</color>\n";

            BUILD += "<color=green>Kinh nghiệm : " + TotalExpCanGet + "</color>\n";

            _NpcDialog.Text = "Chào mừng <color=red>" + client.RoleName + "</color> hôm nay người đã rất cố gắng.Sau đây là thông tin phần thưởng của người\n<color=red>THÔNG TIN THÀNH TÍCH :</color>\n<color=green>Tổng tích lũy : </color>" + TMP.Score + "\n<color=green>Tiêu diệt địch : </color>" + TMP.KillCount + "\n<color=green>Phá hủy Long Trụ : </color>" + TMP.DestroyCount + "\n\n<color=yellow>THÔNG TIN PHẦN THƯỞNG:</color>\n" + BUILD;

            Dictionary<int, string> Selections = new Dictionary<int, string>();

            Selections.Add(1, "Ta muốn nhận thưởng");
            Selections.Add(1000, "Ta sẽ quay lại sau");

            _NpcDialog.Selections = Selections;

            Action<TaskCallBack> ActionWork = (x) => DoReviceGift(client, npc, x, TotalExpCanGet, FindIndex, TMP);

            _NpcDialog.OnSelect = ActionWork;

            _NpcDialog.Show(npc, client);
        }

        /// <summary>
        /// Xác nhận nhận thưởng
        /// </summary>
        /// <param name="client"></param>
        /// <param name="npc"></param>
        /// <param name="x"></param>
        private void DoReviceGift(KPlayer client, NPC npc, TaskCallBack x, int TotalExpCanGet, GuildWarCityAward _Award, GuildWarPlayer TMP)
        {
            if (x.SelectID == 1)
            {
                if (TMP.IsReviceReward)
                {
                    KTLuaLib_Player.SendMSG("Bạn đã nhận thưởng rồi!", client, npc);
                    return;
                }

                if (!KTGlobal.IsHaveSpace(10, client))
                {
                    KTLuaLib_Player.SendMSG("Cần ít nhất 10 ô trống để nhận thưởng!", client, npc);
                    return;
                }
                //Set thằng này đã nhận thưởng
                TMP.IsReviceReward = true;

                //Đút EXP vào mồm
                KTPlayerManager.AddExp(client, TotalExpCanGet);

                //Đút tiền bang hội vào mồm
                KTGlobal.AddMoney(client, _Award.GuildMoney, MoneyType.GuildMoney, "GUILDWARCITY");

                int WeekPointHave = GuildManager.getInstance().GetWeekPoint(client);

                if (WeekPointHave == -1)
                {
                    WeekPointHave = 0;
                }

                WeekPointHave += _Award.WeekPoint;
                //Đút hoạt động tuần vào mồm
                GuildManager.getInstance().SetWeekPoint(client, WeekPointHave);

                KTPlayerManager.ShowNotification(client, "Điểm hoạt động tuần của bạn tăng thêm :" + _Award.WeekPoint);

                string[] ItemPram = _Award.BoxCount.Split('|');

                int ItemID = Int32.Parse(ItemPram[0]);
                int ItemNum = Int32.Parse(ItemPram[1]);

                // Tạo vật phẩm cho thằng top
                if (!ItemManager.CreateItem(Global._TCPManager.TcpOutPacketPool, client, ItemID, ItemNum, 0, "GUILDWARCITY", false, 1, false, ItemManager.ConstGoodsEndTime))
                {
                    KTPlayerManager.ShowNotification(client, "Có lỗi khi nhận phần thưởng");
                }

                KT_TCPHandler.CloseDialog(client);
            }
            else if (x.SelectID == 1000)
            {
                KT_TCPHandler.CloseDialog(client);
            }
        }

        /// <summary>
        /// Giết bởi đồng đội mình đớp assis
        /// </summary>
        /// <param name="CurentScore"></param>
        /// <returns></returns>
        public int GetPointCanGetByTeam(int CurentScore)
        {
            int Point = 0;

            if (CurentScore >= 0 && CurentScore < 2000)
            {
                Point = 1;
            }
            else if (CurentScore >= 2000 && CurentScore < 6000)
            {
                Point = 2;
            }
            else if (CurentScore >= 6000 && CurentScore < 8000)
            {
                Point = 7;
            }
            else if (CurentScore >= 8000 && CurentScore < 10000)
            {
                Point = 15;
            }
            else if (CurentScore >= 10000)
            {
                Point = 30;
            }

            return Point;
        }

        /// <summary>
        /// Update điểm cho bang hội nào đó
        /// </summary>
        /// <param name="GuildID"></param>
        /// <param name="GuildName"></param>
        /// <param name="Score"></param>
        /// <param name="MapID"></param>
        /// <param name="MapName"></param>
        public void UpdateGuildScore(int GuildID, int Score)
        {
            // Nếu như vẫn đang trong trận đánh
            if (_STATE == GUILD_WAR_STATE.STATUS_START || _STATE == GUILD_WAR_STATE.STATUS_PREPAREEND)
            {
                // Nếu như có cái bản đồ này trong danh sách rồi thì
                if (_WarBattle.TryGetValue(GuildID, out GuildWarBattleReport _Report))
                {
                    // Cộng điểm vào này
                    _Report.TotalScore += Score;
                    // TODO THỰC HIỆN SOFT LẠI CHO NÓ NHẸ
                }
            }
        }

        /// <summary>
        /// Xử lý sự kiện khi người chơi ngóe loz
        /// </summary>
        /// <param name="Kill"></param>
        /// <param name="BeKill"></param>
        public void OnDie(GameObject Kill, GameObject BeKill)
        {
            if (_STATE == GUILD_WAR_STATE.STATUS_START || _STATE == GUILD_WAR_STATE.STATUS_PREPAREEND)
            {
                try
                {
                    // Nếu cả 2 thằng đều là người chơi
                    if (Kill is KPlayer && BeKill is KPlayer)
                    {
                        KPlayer kPlayer_Kill = (KPlayer)Kill;

                        KPlayer kPlayer_BeKill = (KPlayer)BeKill;

                        int CurentBeKilLScore = 0;

                        DefDict.TryGetValue(kPlayer_BeKill.RoleID, out GuildWarPlayer _BeKilL);

                        if (_BeKilL == null)
                        {
                            AttackDict.TryGetValue(kPlayer_BeKill.RoleID, out _BeKilL);
                        }

                        if (_BeKilL != null)
                        {
                            CurentBeKilLScore = _BeKilL.Score;
                        }

                        int MaxScoreCanGet = GetPointCanGet(CurentBeKilLScore);

                        UpdateScore(kPlayer_Kill, MaxScoreCanGet, 0, 1, false);
                        UpdateScore(kPlayer_BeKill, 0, 0, 0, true);

                        // Cho liếm ké nếu có PT
                        if (kPlayer_Kill.TeamID != -1)
                        {
                            int MaxScoreCanGetByTeam = GetPointCanGetByTeam(CurentBeKilLScore);

                            List<KPlayer> TotalMember = kPlayer_Kill.Teammates;

                            foreach (KPlayer member in TotalMember)
                            {
                                // Nếu như không nằm trong bán kính thì không cho ăn ké
                                if (KTGlobal.GetDistanceBetweenPoints(new Point(member.PosX, member.PosY), kPlayer_Kill.CurrentPos) > 1000)
                                {
                                    continue;
                                }

                                if (member.RoleID != kPlayer_BeKill.RoleID)
                                {
                                    //Cho ăn ké của đồng  đội
                                    UpdateScore(member, MaxScoreCanGetByTeam, 0, 1, false);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogManager.WriteLog(LogTypes.GuildWarManager, "OnDie TOÁC :" + ex.ToString());
                }
            }
        }

        /// <summary>
        /// Trả về rank của người chơi theo tích lũy cá nhân
        /// </summary>
        /// <param name="CurentScore"></param>
        /// <returns></returns>
        public string GetRankTitleByPoint(int CurentScore)
        {
            string Title = "";

            if (CurentScore >= 0 && CurentScore < 2000)
            {
                Title = "<b><color=#ffffff>Binh sĩ</color></b>";
            }
            else if (CurentScore >= 2000 && CurentScore < 6000)
            {
                Title = "<b><color=#109de8>Hiệu úy</color></b>";
            }
            else if (CurentScore >= 6000 && CurentScore < 8000)
            {
                Title = "<b><color=#b268c4>Thống lĩnh</color></b>";
            }
            else if (CurentScore >= 8000 && CurentScore < 10000)
            {
                Title = "<b><color=#fac241>Phó tướng</color></b>";
            }
            else if (CurentScore >= 10000)
            {
                Title = "<b><color=#f2fa00>Đại Tướng</color></b>";
            }

            return Title;
        }

        #endregion PointDef
    }
}