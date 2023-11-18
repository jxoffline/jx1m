using GameServer.Core.Executor;
using GameServer.Core.GameEvent;
using GameServer.Core.GameEvent.EventOjectImpl;
using GameServer.KiemThe.Core.Item;
using GameServer.KiemThe.Core.Task;
using GameServer.KiemThe.Entities;
using GameServer.KiemThe.LuaSystem.Logic;
using GameServer.Logic;
using GameServer.Server;
using Server.Data;
using Server.Tools;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using static GameServer.Logic.KTMapManager;

namespace GameServer.KiemThe.Logic.Manager.Battle
{
    /// <summary>
    /// Điều khiển toàn bộ tống kim
    /// </summary>
    public class Battle_SongJin : IEventListener
    {
        #region BattelDEF

        /// <summary>
        /// Trạng thái hoạt động
        /// </summary>
        public BattelStatus BATTLESTATE = BattelStatus.STATUS_NULL;

        /// <summary>
        /// Config hoạt động
        /// </summary>
        ///

        public int BattleLevel { get; set; }

        public int TotalSongScore { get; set; }

        public int TotalJinScore { get; set; }

        public long LastTick { get; set; }

        public bool IsBossReswpan { get; set; }
        public long LastNofity { get; set; }

        public bool showdown()
        {
            GlobalEventSource.getInstance().removeListener((int)EventTypes.MonsterDead, this);

            GlobalEventSource.getInstance().removeListener((int)EventTypes.PlayerDead, this);

            GlobalEventSource.getInstance().removeListener((int)EventTypes.PlayEnterMap, this);

            return true;
        }

        public bool startup()
        {
            // Đăng ký event khi quái chết với hệ thống
            GlobalEventSource.getInstance().registerListener((int)EventTypes.MonsterDead, this);
            // Đăng ký event khi có thằng nào chết với hệ thống
            GlobalEventSource.getInstance().registerListener((int)EventTypes.PlayerDead, this);

            GlobalEventSource.getInstance().registerListener((int)EventTypes.PlayEnterMap, this);

            BATTLESTATE = BattelStatus.STATUS_NULL;

            return true;
        }

        /// <summary>
        /// Config hoạt động
        /// </summary>

        private BattleConfig _BattleConfig = new BattleConfig();

        /// <summary>
        /// Bảng xếp hạng send về client nếu có người request
        /// </summary>
        public SongJinBattleRankingInfo CurRanking = new SongJinBattleRankingInfo();

        /// <summary>
        ///  Bảng ranking update động suốt thời gian có kill
        /// </summary>

        public List<BattlePlayer> SongJinRankking = new List<BattlePlayer>();
        /// <summary>
        /// Total Register Tống
        /// </summary>

        public ConcurrentDictionary<int, BattlePlayer> SongCampRegister = new ConcurrentDictionary<int, BattlePlayer>();

        /// <summary>
        /// Total Register Kim
        /// </summary>
        public ConcurrentDictionary<int, BattlePlayer> JinCampRegister = new ConcurrentDictionary<int, BattlePlayer>();

        #endregion BattelDEF

        public void Revice(KPlayer Player)
        {
            int Camp = Player.Camp;

            Region _Find = _BattleConfig.Region.Where(x => x.RegionCamp == Camp).FirstOrDefault();
            if (_Find != null)
            {
                KTPlayerManager.Relive(Player, _BattleConfig.MapID, _Find.PosX, _Find.PosY, 100, 100, 100);
            }
        }

        #region Battel Register

        /// <summary>
        /// Đăng ký vào tống kim
        /// </summary>
        /// <param name="KPlayer"></param>
        /// <param name="Camp"></param>
        public int Register(KPlayer KPlayer, int Camp)
        {
            if (BATTLESTATE == BattelStatus.STATUS_PREPARE)
            {
                // Nếu Camp = 10 thì là đăng ký bên tống
                if (Camp == 10)
                {
                    /// Nếu số lượng lệch quá 3 người thì thông báo là ko được
                    if (SongCampRegister.Count - JinCampRegister.Count > 3)
                    {
                        return -100;
                    }
                    else
                    {
                        if (!SongCampRegister.ContainsKey(KPlayer.RoleID))
                        {
                            BattlePlayer _Battle = new BattlePlayer(KPlayer, 0, 0, 1, 0, 0, 10, 0);

                            SongCampRegister.TryAdd(KPlayer.RoleID, _Battle);

                            this.MovePlayerToArena(KPlayer, Camp);

                            return 0;
                        }
                        else
                        {
                            if (SongCampRegister.TryGetValue(KPlayer.RoleID, out BattlePlayer _Battle))
                            {
                                _Battle.Player = KPlayer;

                                this.MovePlayerToArena(KPlayer, Camp);

                                return 0;
                            }
                        }
                    }
                }
                else if (Camp == 20)
                {
                    if (JinCampRegister.Count - SongCampRegister.Count > 3)
                    {
                        return -200;
                    }
                    else
                    {
                        if (!JinCampRegister.ContainsKey(KPlayer.RoleID))
                        {
                            BattlePlayer _Battle = new BattlePlayer(KPlayer, 0, 0, 1, 0, 0, 20, 0);

                            JinCampRegister.TryAdd(KPlayer.RoleID, _Battle);

                            this.MovePlayerToArena(KPlayer, Camp);

                            return 0;
                        }
                        else
                        {
                            if (JinCampRegister.TryGetValue(KPlayer.RoleID, out BattlePlayer _Battle))
                            {
                                _Battle.Player = KPlayer;

                                this.MovePlayerToArena(KPlayer, Camp);

                                return 0;
                            }
                        }
                    }
                }
                else
                {
                    return -3;
                }
            } // Nếu chiến trường đang diễn ra
            else if (BATTLESTATE == BattelStatus.STATUS_START)
            {
                if (Camp == 10)
                {
                    if (!SongCampRegister.ContainsKey(KPlayer.RoleID))
                    {
                        return -4;
                    }
                    if (SongCampRegister.TryGetValue(KPlayer.RoleID, out BattlePlayer _Battle))
                    {
                        _Battle.Player = KPlayer;

                        this.MovePlayerToArena(KPlayer, Camp);

                        return 0;
                    }
                }
                else if (Camp == 20)
                {
                    if (!JinCampRegister.ContainsKey(KPlayer.RoleID))
                    {
                        return -4;
                    }

                    if (JinCampRegister.TryGetValue(KPlayer.RoleID, out BattlePlayer _Battle))
                    {
                        _Battle.Player = KPlayer;

                        this.MovePlayerToArena(KPlayer, Camp);

                        return 0;
                    }
                }
                else
                {
                    return -3;
                }
            }
            else
            {
                return -4;
            }

            return -4;
        }

        #endregion Battel Register

        #region ClearBattle()

        public void ResetBattle()
        {
            TotalSongScore = 0;
            TotalJinScore = 0;
            ClearMonster();
            BATTLESTATE = BattelStatus.STATUS_NULL;
            LastTick = 0;
            CurRanking = new SongJinBattleRankingInfo();
            SongCampRegister.Clear();
            SongJinRankking = new List<BattlePlayer>();
            JinCampRegister.Clear();
        }

        #endregion ClearBattle()

        /// <summary>
        /// Chuyển người chơi tới tống kim
        /// </summary>
        /// <param name="KPlayer"></param>
        /// <param name="Camp"></param>
        public void MovePlayerToArena(KPlayer KPlayer, int Camp)
        {
            KPlayer.PKMode = (int)PKMode.Custom;
            KPlayer.Camp = Camp;

            KT_TCPHandler.UpdateLastMapInfo(KPlayer, KPlayer.CurrentMapCode, (int)KPlayer.CurrentPos.X, (int)KPlayer.CurrentPos.Y);

            Region _Find = _BattleConfig.Region.Where(x => x.RegionCamp == Camp).FirstOrDefault();

            if (_Find != null)
            {
                KTPlayerManager.ChangeMap(KPlayer, _BattleConfig.MapID, _Find.PosX, _Find.PosY);
            }
        }

        #region MonsterControler

        public void ClearMonster()
        {
            List<Monster> objs = KTMonsterManager.GetMonstersAtMap(_BattleConfig.MapID);
            if (objs != null)
            {
                /// Duyệt danh sách
                foreach (Monster obj in objs)
                {
                    KTMonsterManager.Remove(obj as Monster);
                }
            }
        }

        public void CreateMonster()
        {
            foreach (MonsterBattle _Monster in _BattleConfig.BattelMonster)
            {
                string Tag = "";

                // Nếu là boss thì delay xuất hiện
                if (_Monster.IsBoss)
                {
                    Tag = "Boss";

                    var Funtion = KTKTAsyncTask.Instance.ScheduleExecuteAsync(new DelayFuntionAsyncTask("CreateMonster", new Action(() => CreateMonster(_Monster, true))), _Monster.RebornTime);
                }
                else // Nếu không phải thì mọc luôn
                {
                    CreateMonster(_Monster);
                }
            }
        }

        public void CreateMonster(MonsterBattle _Monster, bool IsBoss = false)
        {
            MonsterAIType aiType = MonsterAIType.Hater;
            if (IsBoss)
            {
                if (_Monster.Camp == 10)
                {
                    string ProtectNotify = _Monster.Name + " đã xuất hiện tại [" + _Monster.PosX + "|" + _Monster.PosY + "] hãy mau tới bảo vệ";
                    NotifyAllSongCampRegister(ProtectNotify);
                }
                else if (_Monster.Camp == 20)
                {
                    string ProtectNotify = _Monster.Name + " đã xuất hiện tại [" + _Monster.PosX + "|" + _Monster.PosY + "] hãy mau tới bảo vệ";
                    NotifyAllJinCampRegister(ProtectNotify);
                }

                aiType = MonsterAIType.Boss;

                this.IsBossReswpan = true;
            }
            /// Ngũ hành
            KE_SERIES_TYPE series = KE_SERIES_TYPE.series_none;

            int RandomSeri = KTGlobal.GetRandomNumber(1, 5);
            series = (KE_SERIES_TYPE)RandomSeri;

            /// Hướng quay
            KiemThe.Entities.Direction dir = KiemThe.Entities.Direction.NONE;

            int RandomDir = KTGlobal.GetRandomNumber(0, 7);

            dir = (KiemThe.Entities.Direction)RandomDir;

            int Level = KTGlobal.GetRandomNumber(_BattleConfig.MinLevel, _BattleConfig.MaxLevel);

            /// AIType

            Monster monster = KTMonsterManager.Create(new KTMonsterManager.DynamicMonsterBuilder()
            {
                MapCode = _BattleConfig.MapID,
                ResID = _Monster.ID,
                PosX = _Monster.PosX,
                PosY = _Monster.PosY,
                Name = _Monster.Name,
                MaxHP = _Monster.HP,
                Level = Level,
                MonsterType = aiType,
                Camp = _Monster.Camp,
                RespawnTick = _Monster.RespawnTick,
            });
            monster.DynamicRespawnPredicate = () =>
            {
                return BATTLESTATE == BattelStatus.STATUS_START || BATTLESTATE == BattelStatus.STATUS_PREPAREEND;
            };
        }

        public void ProtectBossBounds()
        {
            if (this.IsBossReswpan && (BATTLESTATE == BattelStatus.STATUS_START || BATTLESTATE == BattelStatus.STATUS_PREPAREEND))
            {
                List<Monster> _TotalMonster = this.GetAllBossLive();

                foreach (Monster _Monster in _TotalMonster)
                {
                    List<KPlayer> friends = KTGlobal.GetNearByPeacePlayers(_Monster, 500);

                    foreach (KPlayer _Player in friends)
                    {
                        string NotifyProtect = "Ngươi có công bảo vệ " + _Monster.MonsterInfo.Name + ", nhận được <color=yellow>15</color> điểm tích lũy";

                        KTPlayerManager.ShowNotification(_Player, NotifyProtect);

                        BattlePlayer _FindKill = null;

                        if (_Player.Camp == 10)
                        {
                            SongCampRegister.TryGetValue(_Player.RoleID, out _FindKill);
                        }
                        else if (_Player.Camp == 20)
                        {
                            JinCampRegister.TryGetValue(_Player.RoleID, out _FindKill);
                        }

                        if (_FindKill != null)
                        {
                            int KillCurenScore = _FindKill.Score;

                            int ScoreGet = 15;

                            int TotalScore = KillCurenScore + ScoreGet;

                            _FindKill.Score = TotalScore;

                            if (_Player.Camp == 10)
                            {
                                TotalSongScore += ScoreGet;
                            }
                            else if (_Player.Camp == 20)
                            {
                                TotalJinScore += ScoreGet;
                            }
                            UpdateTitleRank(_FindKill.Player, _FindKill.Rank);

                            // Gửi notify cho thằng giết
                            NotifySocreForPlayer(_FindKill, true);
                        }
                    }
                }
            }
        }

        public List<Monster> GetAllBossLive()
        {
            List<Monster> objs = KTMonsterManager.GetMonstersAtMap(_BattleConfig.MapID);

            List<Monster> MonsterOut = new List<Monster>();

            List<MonsterBattle> AllBoss = _BattleConfig.BattelMonster.GroupBy(x => x.ID).Select(y => y.FirstOrDefault()).ToList();
            /// Duyệt danh sách
            foreach (Monster obj in objs)
            {
                foreach (MonsterBattle _Monster in AllBoss)
                {
                    if (_Monster.IsBoss)
                    {
                        if ((obj).MonsterInfo.Code == _Monster.ID)
                        {
                            MonsterOut.Add((obj));
                        }
                    }
                }
            }

            return MonsterOut;
        }

        #endregion MonsterControler

        public void intBattle(int Level)
        {
            string Battle_Config = "";

            if (Level == 1)
            {
                Battle_Config = "Config/KT_Battle/Battle_SongJin_Low.xml";
            }
            else if (Level == 2)
            {
                Battle_Config = "Config/KT_Battle/Battle_SongJin_Mid.xml";
            }
            else if (Level == 3)
            {
                Battle_Config = "Config/KT_Battle/Battle_SongJin_High.xml";
            }

            string Files = KTGlobal.GetDataPath(Battle_Config);

            using (var stream = System.IO.File.OpenRead(Files))
            {
                var serializer = new XmlSerializer(typeof(BattleConfig));
                _BattleConfig = serializer.Deserialize(stream) as BattleConfig;
            }
        }

        public bool initialize(int Level)
        {
            this.BattleLevel = Level;

            string TimerName = "Battle_SongJin_" + Level;

            intBattle(Level);

            //        Thread t = new Thread(() => {
            //            while (true)
            //{
            //                this.ProsecBattle(null, null);
            //                Thread.Sleep(2000);
            //            }
            //        });
            //        t.IsBackground = false;
            //        t.Start();

            ScheduleExecutor2.Instance.scheduleExecute(new NormalScheduleTask(TimerName, ProsecBattle), 5 * 1000, 2000);

            LogManager.WriteLog(LogTypes.SongJinBattle, "[" + _BattleConfig.BattleName + "] Stating Battle OK!");

            return true;
        }

        public bool CanUsingTeleport()
        {
            return BATTLESTATE == BattelStatus.STATUS_START || BATTLESTATE == BattelStatus.STATUS_PREPAREEND;
        }

        public BattelStatus GetBattelState()
        {
            return BATTLESTATE;
        }

        #region SENDPREADINGNOTIFY

        public void SendPreadingNotify(KPlayer Player, int Sec, int TotalTong, int TotalKim, bool IsNotiyText)
        {
            this.PlayChangeState(Player, 1);

            if (IsNotiyText)
            {
                G2C_EventNotification _Notify = new G2C_EventNotification();

                _Notify.EventName = _BattleConfig.BattleName;

                _Notify.ShortDetail = "TIME|" + Sec;

                _Notify.TotalInfo = new List<string>();

                _Notify.TotalInfo.Add("Báo Danh Tống : " + TotalTong);

                _Notify.TotalInfo.Add("Báo Danh Kim : " + TotalKim);

                _Notify.TotalInfo.Add("");

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

        /// <summary>
        /// Cứ 5s spam camp 1 lần
        /// </summary>
        public void SpamCamp()
        {
            foreach (KeyValuePair<int, BattlePlayer> entry in SongCampRegister)
            {
                BattlePlayer PlayerBattle = entry.Value;

                if (PlayerBattle.Player.CurrentMapCode == _BattleConfig.MapID)
                {
                    PlayerBattle.Player.PKMode = (int)PKMode.Custom;
                    PlayerBattle.Player.Camp = 10;
                }
            }

            foreach (KeyValuePair<int, BattlePlayer> entry in JinCampRegister)
            {
                BattlePlayer PlayerBattle = entry.Value;

                if (PlayerBattle.Player.CurrentMapCode == _BattleConfig.MapID)
                {
                    PlayerBattle.Player.PKMode = (int)PKMode.Custom;
                    PlayerBattle.Player.Camp = 20;
                }
            }
        }

        public void UpdatePreading(bool IsNotiyText)
        {
            long SEC = (LastTick + _BattleConfig.RegisterDualtion * 1000) - TimeUtil.NOW();

            int FinalSec = (int)(SEC / 1000);

            string MSG = "Chiến trường sắp bắt đầu, thời gian còn: [" + FinalSec + "] giây.";

            foreach (KeyValuePair<int, BattlePlayer> entry in SongCampRegister)
            {
                BattlePlayer PlayerBattle = entry.Value;
                if (PlayerBattle.Player.CurrentMapCode == _BattleConfig.MapID)
                {
                    PlayerBattle.Player.PKMode = (int)PKMode.Custom;
                    PlayerBattle.Player.Camp = PlayerBattle.Camp;

                    KTPlayerManager.ShowNotification(PlayerBattle.Player, MSG);

                    SendPreadingNotify(PlayerBattle.Player, FinalSec, SongCampRegister.Count, JinCampRegister.Count, IsNotiyText);
                }
            }

            foreach (KeyValuePair<int, BattlePlayer> entry in JinCampRegister)
            {
                BattlePlayer PlayerBattle = entry.Value;
                if (PlayerBattle.Player.CurrentMapCode == _BattleConfig.MapID)
                {
                    PlayerBattle.Player.PKMode = (int)PKMode.Custom;
                    PlayerBattle.Player.Camp = PlayerBattle.Camp;

                    KTPlayerManager.ShowNotification(PlayerBattle.Player, MSG);

                    SendPreadingNotify(PlayerBattle.Player, FinalSec, SongCampRegister.Count, JinCampRegister.Count, IsNotiyText);
                }
            }
        }

        #endregion SENDPREADINGNOTIFY

        #region NotifyBattleInfo

        public void SendBattleNotify(BattlePlayer InputPlayer, int Sec)
        {
            G2C_EventNotification _Notify = new G2C_EventNotification();

            _Notify.EventName = _BattleConfig.BattleName;
            if (Sec > 0)
            {
                _Notify.ShortDetail = "TIME|" + Sec;
            }
            else
            {
                _Notify.ShortDetail = "Chiến trường đã kết thúc!";
            }

            _Notify.TotalInfo = new List<string>();

            _Notify.TotalInfo.Add("Giết Địch : " + InputPlayer.Kill);

            _Notify.TotalInfo.Add("Bị Giết : " + InputPlayer.BeKill);

            _Notify.TotalInfo.Add("Tích Lũy : " + InputPlayer.Score);

            _Notify.TotalInfo.Add("Hạng Hiện Tại :" + (GetRankInBxh(InputPlayer.Player.RoleID) + 1));

            _Notify.TotalInfo.Add("");

            if (InputPlayer.Player.IsOnline())
            {
                InputPlayer.Player.SendPacket<G2C_EventNotification>((int)TCPGameServerCmds.CMD_KT_EVENT_NOTIFICATION, _Notify);
            }
        }

        public void SendBattleNotifyEnd(BattlePlayer InputPlayer, int Sec)
        {
            G2C_EventNotification _Notify = new G2C_EventNotification();

            _Notify.EventName = "Thời gian nhận thưởng còn :";
            if (Sec > 0)
            {
                _Notify.ShortDetail = "TIME|" + Sec;
            }

            _Notify.TotalInfo = new List<string>();

            _Notify.TotalInfo.Add("");

            if (InputPlayer.Player.IsOnline())
            {
                InputPlayer.Player.SendPacket<G2C_EventNotification>((int)TCPGameServerCmds.CMD_KT_EVENT_NOTIFICATION, _Notify);
            }
        }

        public void SendKillStreak(KPlayer InputPlayer, int Count)
        {
            G2C_KillStreak _State = new G2C_KillStreak();

            _State.KillNumber = Count;

            if (InputPlayer.IsOnline())
            {
                InputPlayer.SendPacket<G2C_KillStreak>((int)TCPGameServerCmds.CMD_KT_KILLSTREAK, _State);
            }
        }

        public void UpdateBattleNotify()
        {
            foreach (KeyValuePair<int, BattlePlayer> entry in SongCampRegister)
            {
                BattlePlayer PlayerBattle = entry.Value;

                if (PlayerBattle.Player.CurrentMapCode == _BattleConfig.MapID)
                {
                    long SEC = (LastTick + (_BattleConfig.BattleDualtion * 1000)) - TimeUtil.NOW();

                    int FinalSec = (int)(SEC / 1000);

                    SendBattleNotify(PlayerBattle, FinalSec);
                }
            }

            foreach (KeyValuePair<int, BattlePlayer> entry in JinCampRegister)
            {
                BattlePlayer PlayerBattle = entry.Value;
                if (PlayerBattle.Player.CurrentMapCode == _BattleConfig.MapID)
                {
                    long SEC = (LastTick + (_BattleConfig.BattleDualtion * 1000)) - TimeUtil.NOW();

                    int FinalSec = (int)(SEC / 1000);

                    SendBattleNotify(PlayerBattle, FinalSec);
                }
            }
        }

        public void UpdateBattleNotifyAdward()
        {
            foreach (KeyValuePair<int, BattlePlayer> entry in SongCampRegister)
            {
                BattlePlayer PlayerBattle = entry.Value;

                if (PlayerBattle.Player.CurrentMapCode == _BattleConfig.MapID)
                {
                    long SEC = (LastTick + (_BattleConfig.ReviceAwardDualtion * 1000)) - TimeUtil.NOW();

                    int FinalSec = (int)(SEC / 1000);

                    SendBattleNotifyEnd(PlayerBattle, FinalSec);
                }
            }

            foreach (KeyValuePair<int, BattlePlayer> entry in JinCampRegister)
            {
                BattlePlayer PlayerBattle = entry.Value;
                if (PlayerBattle.Player.CurrentMapCode == _BattleConfig.MapID)
                {
                    long SEC = (LastTick + (_BattleConfig.ReviceAwardDualtion * 1000)) - TimeUtil.NOW();

                    int FinalSec = (int)(SEC / 1000);

                    SendBattleNotifyEnd(PlayerBattle, FinalSec);
                }
            }
        }

        /// <summary>
        /// Gửi thành tích hiện tại cho player
        /// </summary>
        /// <param name="Player"></param>
        public void NotifySocreForPlayer(BattlePlayer PlayerBattle, bool ShowKillStreak)
        {
            long SEC = (LastTick + (_BattleConfig.BattleDualtion * 1000)) - TimeUtil.NOW();

            int FinalSec = (int)(SEC / 1000);

            SendBattleNotify(PlayerBattle, FinalSec);

            if (ShowKillStreak)
            {
                if (PlayerBattle.KillStreak > 2)
                {
                    this.SendKillStreak(PlayerBattle.Player, PlayerBattle.KillStreak);

                    if (PlayerBattle.KillStreak % 6 == 0 && PlayerBattle.KillStreak > 0)
                    {
                        string Notify = KTGlobal.CreateStringByColor("Quân [" + Battel_SonJin_Manager.GetNameCamp(PlayerBattle.Camp) + "] - [" + GetRankTitile(PlayerBattle.Player, PlayerBattle.Rank) + " - " + PlayerBattle.Player.RoleName + "] liên tiếp đẩy lùi " + PlayerBattle.KillStreak + " quân địch, nhận thưởng Liên Trảm 150 điểm tích lũy", ColorType.Yellow);

                        NotifyAllBattle(Notify);
                    }
                }
            }
        }

        public string GetRankTitile(KPlayer Player, int Rank)
        {
            var find = _BattleConfig.Rank.Where(x => x.RankID == Rank).FirstOrDefault();

            if (find != null)
            {
                return find.RankTitle;
            }
            else
            {
                return "Binh sĩ";
            }
        }

        public void NotifyAllSongCampRegister(string MSG)
        {
            foreach (KeyValuePair<int, BattlePlayer> entry in SongCampRegister)
            {
                BattlePlayer PlayerBattle = entry.Value;

                if (PlayerBattle.Player.CurrentMapCode == _BattleConfig.MapID)
                {
                    KTPlayerManager.ShowNotification(PlayerBattle.Player, MSG);
                }
            }
        }

        public void NotifyAllJinCampRegister(string MSG)
        {
            foreach (KeyValuePair<int, BattlePlayer> entry in JinCampRegister)
            {
                BattlePlayer PlayerBattle = entry.Value;
                if (PlayerBattle.Player.CurrentMapCode == _BattleConfig.MapID)
                {
                    KTPlayerManager.ShowNotification(PlayerBattle.Player, MSG);
                }
            }
        }

        /// <summary>
        ///  Cấp độ thấp nhất
        /// </summary>
        /// <returns></returns>
        public int GetMinLevelJoin()
        {
            return _BattleConfig.MinLevel;
        }

        /// <summary>
        /// Lấy ra tên của chiến trường
        /// </summary>
        /// <returns></returns>
        public string GetBattleName()
        {
            return _BattleConfig.BattleName;
        }

        /// <summary>
        /// Cấp độ cao nhất
        /// </summary>
        /// <returns></returns>
        public int GetMaxLevelJoin()
        {
            return _BattleConfig.MaxLevel;
        }

        public void NotifyAllBattle(string MSG)
        {
            foreach (KeyValuePair<int, BattlePlayer> entry in SongCampRegister)
            {
                BattlePlayer PlayerBattle = entry.Value;

                if (PlayerBattle.Player.CurrentMapCode == _BattleConfig.MapID)
                {
                    KTPlayerManager.ShowNotification(PlayerBattle.Player, MSG);
                }
            }

            foreach (KeyValuePair<int, BattlePlayer> entry in JinCampRegister)
            {
                BattlePlayer PlayerBattle = entry.Value;
                if (PlayerBattle.Player.CurrentMapCode == _BattleConfig.MapID)
                {
                    KTPlayerManager.ShowNotification(PlayerBattle.Player, MSG);
                }
            }
        }

        #endregion NotifyBattleInfo

        #region UpdateBXH

        public SongJinBattleRankingInfo RankingBuilder(KPlayer Client)
        {
            SongJinBattleRankingInfo _Rank = new SongJinBattleRankingInfo();
            _Rank.JinTotalScore = TotalJinScore + "";
            _Rank.SongTotalScore = TotalSongScore + "";
            _Rank.PlayerRanks = new List<SongJinRanking>();

            lock (SongJinRankking)
            {
                // Chỉ lấy ra 10 người
                List<BattlePlayer> _Soft = SongJinRankking.Take(10).ToList();

                for (int i = 0; i < _Soft.Count; i++)
                {
                    BattlePlayer _Player = _Soft[i];

                    SongJinRanking _PlayerRank = new SongJinRanking();
                    if (_Player.Camp == 10)
                    {
                        _PlayerRank.Camp = "Tống";
                    }
                    else
                    {
                        _PlayerRank.Camp = "Kim";
                    }

                    _PlayerRank.Faction = _Player.Player.m_cPlayerFaction.GetFactionId();
                    _PlayerRank.KillCount = _Player.Kill;
                    _PlayerRank.Level = _Player.Player.m_Level;
                    _PlayerRank.Rank = i + 1;
                    _PlayerRank.MaxKillStreak = _Player.MaxKillStreak;
                    _PlayerRank.PlayerName = _Player.Player.RoleName;
                    _PlayerRank.Score = _Player.Score;

                    _Rank.PlayerRanks.Add(_PlayerRank);
                }

                var findHost = SongJinRankking.Where(x => x.Player.RoleID == Client.RoleID).FirstOrDefault();
                if (findHost != null)
                {
                    SongJinRanking _PlayerRank = new SongJinRanking();
                    if (findHost.Camp == 10)
                    {
                        _PlayerRank.Camp = "Tống";
                    }
                    else
                    {
                        _PlayerRank.Camp = "Kim";
                    }

                    _PlayerRank.Faction = findHost.Player.m_cPlayerFaction.GetFactionId();
                    _PlayerRank.KillCount = findHost.Kill;
                    _PlayerRank.Level = findHost.Player.m_Level;
                    _PlayerRank.Rank = GetRankInBxh(Client.RoleID) + 1;
                    _PlayerRank.MaxKillStreak = findHost.MaxKillStreak;
                    _PlayerRank.PlayerName = findHost.Player.RoleName;
                    _PlayerRank.Score = findHost.Score;

                    _Rank.PlayerRanks.Add(_PlayerRank);
                }
            }

            return _Rank;
        }

        public void UpadateBXH()
        {
            lock (SongJinRankking)
            {
                SongJinRankking.Clear();

                List<BattlePlayer> _SongCampRegister = SongCampRegister.Select(x => x.Value).ToList();

                SongJinRankking.AddRange(_SongCampRegister);

                List<BattlePlayer> _JinCampRegister = JinCampRegister.Select(x => x.Value).ToList();

                SongJinRankking.AddRange(_JinCampRegister);

                //SOFT LẠI DANH SÁCH Theo danh sách giảm dần
                List<BattlePlayer> _Soft = SongJinRankking.OrderByDescending(x => x.Score).ToList();

                // GÁN LẠI SOFT = DANH SÁCH MỚI ĐÃ SẮP XẾP
                SongJinRankking = _Soft;
            }
        }

        #endregion UpdateBXH

        #region BattleState

        public void OnChangeBattleState(KPlayer KPlayer, int Min)
        {
            this.PlayChangeState(KPlayer, 1);

            this.SendPreadingNotify(KPlayer, Min, SongCampRegister.Count, JinCampRegister.Count, true);

            UpdateTitleRank(KPlayer, 1);
        }

        public void PlayChangeState(KPlayer Player, int State)
        {
            G2C_EventState _State = new G2C_EventState();

            _State.EventID = 20;
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

        public void ForceStartBattle()
        {
            ResetBattle();
            LastTick = TimeUtil.NOW();
            BATTLESTATE = BattelStatus.STATUS_PREPARE;

            KTGlobal.SendSystemEventNotification("Đại chiến Tống Kim sắp bắt đầu, hiện đang tiến hành báo danh, người muốn tham chiến hãy nhanh chóng tìm Quan Mộ Binh (chiến trường) hoặc dùng Chiêu Thư Tống Kim, thời gian báo danh còn: 10 phút.");
        }

        public bool IsInBattleSongJin(int PlayerMapCode)
        {
            if (_BattleConfig.MapID == PlayerMapCode)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion BattleState

        #region Phần thưởng

        /// <summary>
        /// Lấy ra rank hiện tại trong bản xếp hạng
        /// </summary>
        /// <param name="RoleID"></param>
        /// <returns></returns>
        public int GetRankInBxh(int RoleID)
        {
            lock (SongJinRankking)
            {
                var find = SongJinRankking.FindIndex(x => x.Player.RoleID == RoleID);

                return find;
            }
        }

        /// <summary>
        /// Kiểm tra phần thưởng
        /// </summary>
        /// <param name="map"></param>
        /// <param name="npc"></param>
        /// <param name="client"></param>
        public void CheckAward(GameMap map, NPC npc, KPlayer client)
        {
            BattlePlayer _FindPlayer = null;

            if (client.Camp == 10)
            {
                SongCampRegister.TryGetValue(client.RoleID, out _FindPlayer);
            }
            else if (client.Camp == 20)
            {
                JinCampRegister.TryGetValue(client.RoleID, out _FindPlayer);
            }

            if (_FindPlayer == null)
            {
                KTPlayerManager.ShowNotification(client, "[" + client.RoleName + "][" + client.Camp + "] Không tìm thấy người chơi!");
                return;
            }
            string Text = "";

            KNPCDialog _NpcDialog = new KNPCDialog();

            Dictionary<int, string> Selections = new Dictionary<int, string>();

            // Nếu mà chiến trường đã kết thúc thì add thêm cái nhận thưởng
            if (BATTLESTATE == BattelStatus.STATUS_END)
            {
                if (_FindPlayer.Score == 0)
                {
                    Text = "Chiến trường đã kết thúc.Hôm nay người làm rất tốt.";
                }
                else
                {
                    string MoreAward = "";
                    string MoreAwardExtras = "";

                    BattelAward _Find = _BattleConfig.BattelAward.OrderByDescending(x => x.Score).Where(x => x.Score <= _FindPlayer.Score).FirstOrDefault();

                    int PlayRank = GetRankInBxh(client.RoleID) + 1;

                    BattelAwardExtras _FindExtrass = _BattleConfig.BattelAwardExtras.Where(x => x.Rank == PlayRank).FirstOrDefault();

                    if (_FindExtrass != null)
                    {
                        string ItemList = _FindExtrass.ItemList;

                        string ItemStr = "";

                        if (ItemList.Length > 0)
                        {
                            string[] ItemArray = ItemList.Split(';');

                            foreach (string StrItem in ItemArray)
                            {
                                string[] ItemPram = StrItem.Split('|');

                                int ItemID = Int32.Parse(ItemPram[0]);
                                int ItemNum = Int32.Parse(ItemPram[1]);

                                ItemData _ItemFind = ItemManager.GetItemTemplate(ItemID);
                                if (_ItemFind != null)
                                {
                                    ItemStr += _ItemFind.Name + "X" + ItemNum + "<br>";
                                }
                            }
                        }

                        MoreAwardExtras = "Thưởng Top :<color=green>" + PlayRank + "</color><br>" + "Danh vọng Tống Kim:<color=green>" + _FindExtrass.Point + "</color><br>" + ItemStr;
                    }

                    //Quà tặng theo tích lũy
                    if (_Find != null)
                    {
                        MoreAward = "Bạc Khóa :<color=green>" + _Find.Money + "</color><br>" + "Danh Vọng Chiến Trường:<color=green>" + _Find.Point + "</color>";
                    }

                    // Nếu thằng này đủ tích lũy
                    if (_FindPlayer.Score >= 1000 || _FindPlayer.IsReviceAward == false)
                    {
                        Text = "Điểm tích lũy của người là :<color=red>" + _FindPlayer.Score + "</color><br>Đã giết :<color=red>" + _FindPlayer.Kill + "</color> quân địch!<br>Xếp hạng của ngươi :<color=red>" + GetRankInBxh(_FindPlayer.Player.RoleID) + "</color><br>Phần thưởng :<br>Exp :<color=green>" + GetExpReward(_FindPlayer.Player.m_Level, _FindPlayer.Score) + "</color><br>" + MoreAward + "<br>" + MoreAwardExtras;

                        if (!_FindPlayer.IsReviceAward)
                        {
                            // Nếu có Điểm tích lũy thì cho nhận thưởng
                            Selections.Add(-1, "Nhận Thưởng");
                        }
                    }
                    else
                    {
                        Text = "Điểm tích lũy của người là :<color=red>" + _FindPlayer.Score + "</color><br>Đã giết :<color=red>" + _FindPlayer.Kill + "</color> quân địch!<br>Xếp hạng của ngươi :<color=red>" + GetRankInBxh(_FindPlayer.Player.RoleID) + "</color><br>Phần thưởng :<br>" + KTGlobal.CreateStringByColor("Không đủ điều kiện nhận thưởng", ColorType.Importal);
                    }
                }
            }
            else
            {
                Text = "Chiến trường đang diễn ra, hãy đợi kết thúc để đổi lấy phần thưởng.";

              //  Selections.Add(-3, "Ta muốn nhận quân rương quân nhu");
            }

            // Menu này lúc nào cũng có

            Selections.Add(-2, "Ta muốn rời khỏi chiến trường");

            Action<TaskCallBack> ActionWork = (x) => DoActionSelect(_FindPlayer, npc, x);

            _NpcDialog.OnSelect = ActionWork;

            _NpcDialog.Selections = Selections;

            _NpcDialog.Text = Text;

            _NpcDialog.Show(npc, client);
        }

        /// <summary>
        /// Click chọn acc
        /// </summary>
        /// <param name="findPlayer"></param>
        /// <param name="npc"></param>
        /// <param name="x"></param>
        private void DoActionSelect(BattlePlayer findPlayer, NPC npc, TaskCallBack x)
        {
            if (x.SelectID == -1)
            {
                // Thực hiện cho nó nhận xong thì đóng dialog
                ReviceAward(findPlayer, npc, x.SelectID);

                KT_TCPHandler.CloseDialog(findPlayer.Player);
            }
            else if (x.SelectID == -2)
            {
                // Thực hiện đóng khung
                KT_TCPHandler.CloseDialog(findPlayer.Player);
                // Nếu có tích lũy
                if (findPlayer.Score > 0)
                {
                    string MSG = "";
                    if (BATTLESTATE != BattelStatus.STATUS_END)
                    {
                        MSG = "Chiến trường chưa kết thúc nếu người rời đi điểm tích lũy sẽ không được tính!";
                    }
                    else
                    {
                        MSG = "Ngươi có chắc muốn rời đi?Ta khuyên ngươi hãy nhận thưởng trước!";
                    }

                    KTPlayerManager.ShowMessageBox(findPlayer.Player, "Chú ý ", MSG, new Action(() => MoveOutConfirm(findPlayer)));
                }
                else // Nếu không có tích lũy thì chim cút luôn
                {
                    MoveOutConfirm(findPlayer);
                }
            }
            else if (x.SelectID == -3)
            {
                // Nếu đã nhận đủ 4 rương mỗi ngày
                if (findPlayer.Player.ReviceMedicineOfDay > 1)
                {
                    KTLuaLib_Player.SendMSG("Hôm nay người đã nhận hết quân nhu rồi", findPlayer.Player, npc);
                }
                else
                {
                    if (!KTGlobal.IsHaveSpace(1, findPlayer.Player))
                    {
                        KTLuaLib_Player.SendMSG("Túi đồ không đủ chỗ trống.Vui lòng thu dọn hành trang", findPlayer.Player, npc);
                        return;
                    }

                    // Update số lần nhận quân nhu
                    findPlayer.Player.ReviceMedicineOfDay = findPlayer.Player.ReviceMedicineOfDay + 1;

                    KT_TCPHandler.CloseDialog(findPlayer.Player);
                }
            }
        }

        public void MoveOutConfirm(BattlePlayer PlayerBattle)
        {
            if (PlayerBattle.Player.Camp == 10)
            {
                if (SongCampRegister.TryRemove(PlayerBattle.Player.RoleID, out BattlePlayer Remove))
                {
                    KT_TCPHandler.GetLastMapInfo(PlayerBattle.Player, out int preMapCode, out int prePosX, out int prePosY);

                    KTPlayerManager.ChangeMap(PlayerBattle.Player, preMapCode, prePosX, prePosY);
                    // Set lại camp cho người chơi về -1
                    PlayerBattle.Player.Camp = -1;
                    PlayerBattle.Player.TempTitle = "";
                }
            }
            else if (PlayerBattle.Player.Camp == 20)
            {
                if (JinCampRegister.TryRemove(PlayerBattle.Player.RoleID, out BattlePlayer Remove))
                {
                    KT_TCPHandler.GetLastMapInfo(PlayerBattle.Player, out int preMapCode, out int prePosX, out int prePosY);

                    KTPlayerManager.ChangeMap(PlayerBattle.Player, preMapCode, prePosX, prePosY);
                    // Set lại camp cho người chơi về -1
                    PlayerBattle.Player.Camp = -1;
                    PlayerBattle.Player.TempTitle = "";
                }
            }
        }

        /// <summary>
        /// Nhận Exp
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
        /// Người chơi nhận thưởng
        /// </summary>
        /// <param name="_Player"></param>
        // Thưởng theo tích lũy
        public void ReviceAward(BattlePlayer _Player, NPC npc, int X)
        {
            if (!_Player.IsReviceAward)
            {
                int Score = _Player.Score;

                BattelAward _Find = _BattleConfig.BattelAward.OrderByDescending(x => x.Score).Where(x => x.Score <= Score).FirstOrDefault();

                if (_Find != null)
                {
                    // Nếu không đủ chỗ trống
                    if (!KTGlobal.IsHaveSpace(3, _Player.Player))
                    {
                        KTPlayerManager.ShowNotification(_Player.Player, "Cần ít nhất 3 ô trống để nhận thưởng");
                        return;
                    }

                    int Money = _Find.Money;

                    int Point = _Find.Point;

                    int PointType = _Find.PointType;

                    int PlayRank = GetRankInBxh(_Player.Player.RoleID) + 1;

                    // Nhận thêm phần quà thưởng thêm
                    BattelAwardExtras _FindExtrass = _BattleConfig.BattelAwardExtras.Where(x => x.Rank == PlayRank).FirstOrDefault();

                    if (_FindExtrass != null)
                    {
                        string ItemList = _FindExtrass.ItemList;

                        if (ItemList.Length > 0)
                        {
                            string[] ItemArray = ItemList.Split(';');

                            foreach (string StrItem in ItemArray)
                            {
                                string[] ItemPram = StrItem.Split('|');

                                int ItemID = Int32.Parse(ItemPram[0]);
                                int ItemNum = Int32.Parse(ItemPram[1]);

                                // Tạo vật phẩm cho thằng top
                                if (!ItemManager.CreateItem(Global._TCPManager.TcpOutPacketPool, _Player.Player, ItemID, ItemNum, 0, "SONGJINBATTLE", false, 1, false, ItemManager.ConstGoodsEndTime))
                                {
                                    KTPlayerManager.ShowNotification(_Player.Player, "Có lỗi khi nhận phần thưởng");
                                }
                            }
                        }

                        Point = Point + _FindExtrass.Point;
                    }

                    // add EXO cho người chơi
                    long ExpRecvice = (long)GetExpReward(_Player.Player.m_Level, Score);

                    if (ExpRecvice > 0)
                    {
                        KTPlayerManager.AddExp(_Player.Player, ExpRecvice);
                    }

                    KTGlobal.AddRepute(_Player.Player, PointType, Point);

                    // ADd bạc khóa
                    KTGlobal.AddMoney(_Player.Player, Money, MoneyType.BacKhoa, "BATTLE_SONG_JIN");

                    int MoneyPresinal = 0;
                    int MoneyGuild = 0;

                    // Tính toán thêm tiền bang hội cho mấy thằng TOP
                    if (PlayRank == 1)
                    {
                        MoneyPresinal = 1000;

                        MoneyGuild = 150;

                        // Danh vọng cá nhân
                        _Player.Player.Prestige = _Player.Player.Prestige + 10;
                    }
                    else if (PlayRank >= 2 && PlayRank <= 10)
                    {
                        MoneyPresinal = 700;

                        MoneyGuild = 150;

                        // Danh vọng cá nhân
                        _Player.Player.Prestige = _Player.Player.Prestige + 8;
                    }
                    else if (PlayRank >= 11 && PlayRank <= 20)
                    {
                        MoneyPresinal = 500;

                        MoneyGuild = 150;
                        // Danh vọng cá nhân
                        _Player.Player.Prestige = _Player.Player.Prestige + 6;
                    }
                    else if (_Player.Score > 800 && _Player.Score <= 1200)
                    { // Danh vọng cá nhân
                        _Player.Player.Prestige = _Player.Player.Prestige + 1;
                    }
                    else if (_Player.Score > 1200 && _Player.Score <= 1800)
                    {
                        MoneyPresinal = 200;

                        MoneyGuild = 50;
                        // Danh vọng cá nhân
                        _Player.Player.Prestige = _Player.Player.Prestige + 3;
                    }
                    else if (_Player.Score > 1800 && _Player.Score <= 3000)
                    { // Danh vọng cá nhân
                        MoneyPresinal = 300;
                        MoneyGuild = 50;
                        _Player.Player.Prestige = _Player.Player.Prestige + 4;
                    }
                    else if (_Player.Score > 3000 && _Player.Score <= 4500)
                    { // Danh vọng cá nhân
                        MoneyPresinal = 500;
                        MoneyGuild = 150;
                        _Player.Player.Prestige = _Player.Player.Prestige + 5;
                    }
                    else if (_Player.Score > 4500)
                    {
                        MoneyPresinal = 500;
                        MoneyGuild = 150;
                    }

                    // Cộng tài sản cá nhân
                    if (MoneyPresinal > 0 && _Player.Player.GuildID > 0)
                    {
                        //Update tài sản bang hội
                        KTGlobal.AddMoney(_Player.Player, MoneyPresinal, MoneyType.GuildMoney, "TONGKIM");
                    }
                    // Cộng tài sản bang
                    if (MoneyGuild > 0 && _Player.Player.GuildID > 0)
                    {
                        //Update tài sản bang hội
                        KTGlobal.UpdateGuildMoney(MoneyGuild, _Player.Player.GuildID, _Player.Player);
                    }

                    // Nhận xong set lại điểm cho thằng này về 0
                    _Player.IsReviceAward = true;
                }
                else
                {
                    KTPlayerManager.ShowMessageBox(_Player.Player, "Thông báo", "KHông đủ điều kiện để nhận thưởng");
                }
            }
        }

        #endregion Phần thưởng

        /// <summary>
        /// Thực hiện chuyển vị trí
        /// </summary>
        /// <param name="player"></param>
        /// <param name="posX"></param>
        /// <param name="posY"></param>
        /// <param name="hpPercent"></param>
        /// <param name="mpPercent"></param>
        /// <param name="staminaPercent"></param>
        private void ChangePos(KPlayer player, int posX, int posY, int hpPercent = -1, int mpPercent = -1, int staminaPercent = -1)
        {
            /// Cập nhật máu cho nó
            if (hpPercent != -1)
            {
                player.m_CurrentLife = player.m_CurrentLifeMax * hpPercent / 100;
            }
            if (mpPercent != -1)
            {
                player.m_CurrentMana = player.m_CurrentManaMax * mpPercent / 100;
            }
            if (staminaPercent != -1)
            {
                player.m_CurrentStamina = player.m_CurrentStaminaMax * staminaPercent / 100;
            }
            /// Dịch chuyển
            KTPlayerManager.ChangePos(player, posX, posY);
        }

        public void MovePlayerToSafeZone()
        {
            try
            {
                Region _FindSongCampRegister = _BattleConfig.Region.Where(x => x.RegionCamp == 10).FirstOrDefault();
                Region _FindJinCampRegister = _BattleConfig.Region.Where(x => x.RegionCamp == 20).FirstOrDefault();

                foreach (int Key in SongCampRegister.Keys)
                {
                    SongCampRegister.TryGetValue(Key, out BattlePlayer PlayerBattle);
                    if (PlayerBattle != null)
                    {
                        PlayerBattle.LastPoint = PlayerBattle.Player.CurrentPos;
                        if (_FindSongCampRegister != null)
                        {
                            LogManager.WriteLog(LogTypes.SongJinBattle, "[" + PlayerBattle.Player.RoleID + "][" + PlayerBattle.Player.RoleName + "] Move Play TO SafeZone!");
                            this.ChangePos(PlayerBattle.Player, _FindSongCampRegister.PosX, _FindSongCampRegister.PosY, 100, 100, 100);
                        }
                    }
                }

                foreach (int Key in JinCampRegister.Keys)
                {
                    JinCampRegister.TryGetValue(Key, out BattlePlayer PlayerBattle);
                    if (PlayerBattle != null)
                    {
                        if (PlayerBattle.Player.CurrentMapCode == _BattleConfig.MapID)
                        {
                            PlayerBattle.LastPoint = PlayerBattle.Player.CurrentPos;
                            if (_FindJinCampRegister != null)
                            {
                                LogManager.WriteLog(LogTypes.SongJinBattle, "[" + PlayerBattle.Player.RoleID + "][" + PlayerBattle.Player.RoleName + "] Move Play TO SafeZone!");
                                this.ChangePos(PlayerBattle.Player, _FindJinCampRegister.PosX, _FindJinCampRegister.PosY, 100, 100, 100);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.SongJinBattle, "[BUG WHEN MOVE EXP]:" + ex.ToString());
            }

          
        }

        /// <summary>
        /// Call verify lại nếu nhân vật chưa ra
        /// </summary>
        public void VerifyTelePortAsync()
        {
            // LOOP 10 lần cho nó chắc
            if (BATTLESTATE == BattelStatus.STATUS_END && TimeUtil.NOW() - LastTick < 120000)
            {
                foreach (KeyValuePair<int, BattlePlayer> entry in SongCampRegister)
                {
                    BattlePlayer PlayerBattle = entry.Value;

                    Region _Find = _BattleConfig.Region.Where(x => x.RegionCamp == PlayerBattle.Camp).FirstOrDefault();

                    if (PlayerBattle.Player.CurrentMapCode == _BattleConfig.MapID)
                    {
                        float Dist = KTGlobal.GetDistanceBetweenPoints(PlayerBattle.Player.CurrentPos, PlayerBattle.LastPoint);

                        if (Dist < 1000)
                        {
                            this.ChangePos(PlayerBattle.Player, _Find.PosX, _Find.PosY, 100, 100, 100);
                        }
                    }
                }

                foreach (KeyValuePair<int, BattlePlayer> entry in JinCampRegister)
                {
                    BattlePlayer PlayerBattle = entry.Value;

                    Region _Find = _BattleConfig.Region.Where(x => x.RegionCamp == PlayerBattle.Camp).FirstOrDefault();

                    if (PlayerBattle.Player.CurrentMapCode == _BattleConfig.MapID)
                    {
                        float Dist = KTGlobal.GetDistanceBetweenPoints(PlayerBattle.Player.CurrentPos, PlayerBattle.LastPoint);

                        if (Dist < 1000)
                        {
                            this.ChangePos(PlayerBattle.Player, _Find.PosX, _Find.PosY, 100, 100, 100);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Đưa tất cả người chơi ra ngoài
        /// </summary>
        public void MovePlayerOut()
        {
            foreach (KeyValuePair<int, BattlePlayer> entry in SongCampRegister)
            {
                BattlePlayer PlayerBattle = entry.Value;

                // Nếu người chơi còn ở trong bản đồ
                if (PlayerBattle.Player.CurrentMapCode == _BattleConfig.MapID)
                {
                    this.PlayChangeState(PlayerBattle.Player, 0);

                    KT_TCPHandler.GetLastMapInfo(PlayerBattle.Player, out int preMapCode, out int prePosX, out int prePosY);

                    KTPlayerManager.ChangeMap(PlayerBattle.Player, preMapCode, prePosX, prePosY);

                    // Set lại camp cho người chơi về -1
                    PlayerBattle.Player.Camp = -1;
                    PlayerBattle.Player.TempTitle = "";
                }
            }

            foreach (KeyValuePair<int, BattlePlayer> entry in JinCampRegister)
            {
                BattlePlayer PlayerBattle = entry.Value;

                // Nếu người chơi còn ở trong bản đồ
                if (PlayerBattle.Player.CurrentMapCode == _BattleConfig.MapID)
                {
                    this.PlayChangeState(PlayerBattle.Player, 0);

                    KT_TCPHandler.GetLastMapInfo(PlayerBattle.Player, out int preMapCode, out int prePosX, out int prePosY);

                    KTPlayerManager.ChangeMap(PlayerBattle.Player, preMapCode, prePosX, prePosY);

                    PlayerBattle.Player.Camp = -1;
                    PlayerBattle.Player.TempTitle = "";
                }
            }
        }

        public void MoveAgianIfMising()
        {
            List<KPlayer> objs = KTPlayerManager.FindAll(_BattleConfig.MapID);
            if (objs != null)
            {
                /// Duyệt danh sách
                foreach (KPlayer obj in objs)
                {
                    this.PlayChangeState(obj, 0);

                    KT_TCPHandler.GetLastMapInfo(obj, out int preMapCode, out int prePosX, out int prePosY);
                    KTPlayerManager.ChangeMap(obj, preMapCode, prePosX, prePosY);

                    obj.Camp = -1;
                    obj.TempTitle = "";
                }
            }
        }

        private void ProsecBattle(object sender, EventArgs e)
        {
            try
            {
                // Nếu chiến trường chưa bắt đầu
                if (BATTLESTATE == BattelStatus.STATUS_NULL)
                {
                    List<int> DayOfWeek = _BattleConfig.DayOfWeek;

                    int Today = TimeUtil.GetWeekDay1To7(DateTime.Now);

                    if (DayOfWeek.Contains(Today))
                    {
                        DateTime Now = DateTime.Now;

                        int NowHours = Now.Hour;

                        var FindNext = _BattleConfig.OpenTime.Where(x => x.Hours > NowHours).FirstOrDefault();

                        if (FindNext != null)
                        {
                            int NextHour = FindNext.Hours;
                            int NextMin = FindNext.Minute;

                            DateTime _RegisterDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, NextHour, NextMin, 0).AddSeconds(_BattleConfig.RegisterDualtion * -1);

                            if (Now.Hour == _RegisterDate.Hour && Now.Minute == _RegisterDate.Minute && Now.Second >= _RegisterDate.Second && BATTLESTATE == BattelStatus.STATUS_NULL)
                            {
                                LastTick = TimeUtil.NOW();

                                BATTLESTATE = BattelStatus.STATUS_PREPARE;

                                LogManager.WriteLog(LogTypes.SongJinBattle, "[" + _BattleConfig.BattleName + "] Battle Change State ==> " + BATTLESTATE.ToString());

                                KTGlobal.SendSystemEventNotification("Đại chiến Tống Kim sắp bắt đầu, hiện đang tiến hành báo danh, người muốn tham chiến hãy nhanh chóng tìm Quan Mộ Binh (chiến trường) hoặc dùng Chiêu Thư Tống Kim, thời gian báo danh còn: 10 phút.");
                            }
                        }
                    }
                }
                // Nếu là trạng thái chuẩn bị
                else if (BATTLESTATE == BattelStatus.STATUS_PREPARE)
                {
                    // Cứ 5s update thông tin 1 lần
                    if (TimeUtil.NOW() - LastNofity >= 5 * 1000 && BATTLESTATE == BattelStatus.STATUS_PREPARE)
                    {
                        LastNofity = TimeUtil.NOW();
                        UpdatePreading(true);
                    }

                    if (TimeUtil.NOW() >= LastTick + _BattleConfig.RegisterDualtion * 1000 && BATTLESTATE == BattelStatus.STATUS_PREPARE)
                    {
                        LastTick = TimeUtil.NOW();

                        if (JinCampRegister.Count >= 0 && SongCampRegister.Count >= 0)
                        {
                            BATTLESTATE = BattelStatus.STATUS_START;

                            KTGlobal.SendSystemEventNotification("Chiến trường Tống Kim [" + _BattleConfig.BattleName + "] đã chính thức bắt đầu.");

                            LastNofity = TimeUtil.NOW();

                            UpdateBattleNotify();

                            CreateMonster();
                        }
                        else
                        {
                            BATTLESTATE = BattelStatus.STATUS_NULL;

                            LogManager.WriteLog(LogTypes.SongJinBattle, "[" + _BattleConfig.BattleName + "] Battle Change State ==> " + BATTLESTATE.ToString());

                            NotifyAllBattle("Thời gian chuẩn bị đã hết nhưng 2 bên Tống và Kim không tập hợp đủ số lượng quân nên chiến trường tạm hoãn giao chiến");
                            MovePlayerOut();
                            ResetBattle();
                        }
                    }
                }
                else if (BATTLESTATE == BattelStatus.STATUS_START || BATTLESTATE == BattelStatus.STATUS_PREPAREEND)
                {
                    //NOtify về mỗi 5s
                    if (TimeUtil.NOW() - LastNofity >= 5 * 1000 && (BATTLESTATE == BattelStatus.STATUS_START || BATTLESTATE == BattelStatus.STATUS_PREPAREEND))
                    {
                        LastNofity = TimeUtil.NOW();

                        SpamCamp();
                        UpdateBattleNotify();
                        UpadateBXH();
                        ProtectBossBounds();
                    }

                    //  THông báo còn 5p nữa kết thúc chiến trường và chuyển trạng thái về chuẩn bị kết thúc

                    if (TimeUtil.NOW() >= LastTick + ((_BattleConfig.BattleDualtion * 1000) - (5 * 60 * 1000)) && BATTLESTATE == BattelStatus.STATUS_START)
                    {
                        KTGlobal.SendSystemEventNotification("Chỉ còn 5 phút nữa chiến trường Tống Kim sẽ kết thúc,Các anh hũng hào kiệt hãy mau đẩy lui quân địch!");

                        BATTLESTATE = BattelStatus.STATUS_PREPAREEND;

                        LogManager.WriteLog(LogTypes.SongJinBattle, "[" + _BattleConfig.BattleName + "] Battle Change State ==> " + BATTLESTATE.ToString());
                    }

                    // Nếu mà chiến trường fax kết thúc
                    if (TimeUtil.NOW() >= LastTick + (_BattleConfig.BattleDualtion * 1000) && BATTLESTATE == BattelStatus.STATUS_PREPAREEND)
                    {
                        LastTick = TimeUtil.NOW();

                        // Gọi Update Bảng xếp hạng lần cuối
                        UpadateBXH();

                        if (TotalSongScore > TotalJinScore)
                        {
                            KTGlobal.SendSystemEventNotification("Chiến trường [" + KTGlobal.CreateStringByColor(_BattleConfig.BattleName, ColorType.Done) + "] đã kết thúc.Quân Tống thế như chẻ tre, thắng lợi tuyệt đối!");
                        }
                        else if (TotalJinScore > TotalSongScore)
                        {
                            KTGlobal.SendSystemEventNotification("Chiến trường [" + KTGlobal.CreateStringByColor(_BattleConfig.BattleName, ColorType.Done) + "] đã kết thúc.Quân Kim thế như chẻ tre, thắng lợi tuyệt đối!.");
                        }
                        else
                        {
                            KTGlobal.SendSystemEventNotification("Chiến trường [" + KTGlobal.CreateStringByColor(_BattleConfig.BattleName, ColorType.Done) + "] Hai bên bất phân thắng bại, chọn ngày đấu tiếp!");
                        }

                        // Chuyển sang trạng thái end
                        BATTLESTATE = BattelStatus.STATUS_END;

                        // Chuyển người chơi về khu an toàn
                        this.MovePlayerToSafeZone();

                        // Xóa hết monster trên map
                        this.ClearMonster();

                        LastNofity = TimeUtil.NOW();

                        // Tông báo nhận thưởng tới người chơi
                        UpdateBattleNotifyAdward();

                        LogManager.WriteLog(LogTypes.SongJinBattle, "[" + _BattleConfig.BattleName + "] Battle Change State ==> " + BATTLESTATE.ToString());
                    }
                }

                // Cho 1 phút để nhận thưởng làm ABC trong chiến trường
                else if (BATTLESTATE == BattelStatus.STATUS_END)
                {
                    if (TimeUtil.NOW() - LastNofity >= 5 * 1000 && BATTLESTATE == BattelStatus.STATUS_END)
                    {
                        LastNofity = TimeUtil.NOW();

                        // Call verify teleport
                        VerifyTelePortAsync();
                    }
                    // Cho người chơi 5p để nhận thưởng

                    if (TimeUtil.NOW() >= LastTick + (_BattleConfig.ReviceAwardDualtion * 1000) && BATTLESTATE == BattelStatus.STATUS_END)
                    {
                        LastTick = TimeUtil.NOW();

                        //Hết 5 phút thì chuyển người chơi ra khỏi map
                        MovePlayerOut();

                        // Chuyển sang trạng thái chờ clear
                        BATTLESTATE = BattelStatus.STATUS_CLEAR;

                        LogManager.WriteLog(LogTypes.SongJinBattle, "[" + _BattleConfig.BattleName + "] Battle Change State ==> " + BATTLESTATE.ToString());
                    }
                }
                else if (BATTLESTATE == BattelStatus.STATUS_CLEAR)
                {
                    // Đợi tiếp 60s nữa và dọn lại chiến trường chuẩn bị cho lần sau
                    if (TimeUtil.NOW() >= LastTick + (60 * 1000) && BATTLESTATE == BattelStatus.STATUS_CLEAR)
                    {
                        MoveAgianIfMising();
                        ResetBattle();
                        BATTLESTATE = BattelStatus.STATUS_NULL;

                        LogManager.WriteLog(LogTypes.SongJinBattle, "[" + _BattleConfig.BattleName + "] Battle Change State ==> " + BATTLESTATE.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.SongJinBattle, "[" + _BattleConfig.BattleName + "]==> " + ex.ToString());
            }
        }

        public void ForceEndBattle()
        {
            BATTLESTATE = BattelStatus.STATUS_END;
            LastTick = TimeUtil.NOW();
        }

        #region Score

        /// <summary>
        /// Lấy ra điểm khi A Kill B
        /// </summary>
        /// <param name="KillRank"></param>
        /// <param name="BeKillRank"></param>
        /// <returns></returns>
        public int GetKillScore(int KillRank, int BeKillRank, bool IsMonster)
        {
            int SCORE = 75 + (BeKillRank - KillRank) * 15;

            if (IsMonster)
            {
                SCORE = SCORE / 2;
            }
            return SCORE;
        }

        public void UpdateTitleRank(KPlayer Player, int Rank)
        {
            var find = _BattleConfig.Rank.Where(x => x.RankID == Rank).FirstOrDefault();

            if (find != null)
            {
                if (Player.Camp == 10)
                {
                    Player.TempTitle = "<color=#" + find.Color + ">Tống " + find.RankTitle + "</color>";
                }
                else if (Player.Camp == 20)
                {
                    Player.TempTitle = "<color=#" + find.Color + ">Kim " + find.RankTitle + "</color>";
                }
            }
        }

        public int GetRank(int Score)
        {
            var find = _BattleConfig.Rank.OrderByDescending(x => x.Score).Where(x => x.Score <= Score).FirstOrDefault();

            if (find != null)
            {
                return find.RankID;
            }
            else
            {
                return 1;
            }
        }

        public int GetMonsterRank(int MonsterID)
        {
            var find = _BattleConfig.MonsterRank.Where(x => x.MonsterID == MonsterID).FirstOrDefault();

            if (find != null)
            {
                return find.RankID;
            }
            else
            {
                return 1;
            }
        }

        #endregion Score

        #region KillEventProsecc

        public void processEvent(EventObject eventObject)
        {
            if (eventObject.getEventType() == (int)EventTypes.MonsterDead)
            {
                MonsterDeadEventObject obj = eventObject as MonsterDeadEventObject;
                Monster monster = obj.getMonster();
                KPlayer client = obj.getAttacker();
                PlayerKillMonster(client, monster);
            }
            else if (eventObject.getEventType() == (int)EventTypes.PlayerDead)
            {
                PlayerDeadEventObject playerDeadEvent = eventObject as PlayerDeadEventObject;
                if (null != playerDeadEvent)
                {
                    if (playerDeadEvent.Type == PlayerDeadEventTypes.ByRole)
                    {
                        OnKillRole(playerDeadEvent.getAttackerRole(), playerDeadEvent.getPlayer());
                    }
                }
            }
            else if (eventObject.getEventType() == (int)EventTypes.PlayEnterMap)
            {
                PlayerEnterMap PlayEvent = eventObject as PlayerEnterMap;
                if (null != PlayEvent)
                {
                    if (PlayEvent.Client.Camp == 10 || PlayEvent.Client.Camp == 20)
                    {
                        long SEC = (LastTick + _BattleConfig.RegisterDualtion * 1000) - TimeUtil.NOW();

                        int FinalSec = (int)(SEC / 1000);

                        OnChangeBattleState(PlayEvent.Client, FinalSec);
                    }
                }
            }
        }

        /// <summary>
        /// Xử lý sự kiện khi người này giết người chơi kia
        /// </summary>
        /// <param name="kPlayer1"></param>
        /// <param name="kPlayer2"></param>
        private void OnKillRole(KPlayer Kill, KPlayer Bekill)
        {
            // Nếu chiến trường đã bắt đầu
            if (BATTLESTATE == BattelStatus.STATUS_START || BATTLESTATE == BattelStatus.STATUS_PREPAREEND)
            {
                // Check xem có đang trong chiến trường không
                if ((Kill.Camp == 10 || Kill.Camp == 20) && (Bekill.Camp == 10 || Bekill.Camp == 20))
                {
                    BattlePlayer _FindKill = null;

                    if (Kill.m_Level >= _BattleConfig.MinLevel && Kill.m_Level < _BattleConfig.MaxLevel)
                    {
                        if (Kill.Camp == 10)
                        {
                            SongCampRegister.TryGetValue(Kill.RoleID, out _FindKill);
                        }
                        else if (Kill.Camp == 20)
                        {
                            JinCampRegister.TryGetValue(Kill.RoleID, out _FindKill);
                        }

                        BattlePlayer _FinBeKill = null;

                        if (Bekill.Camp == 10)
                        {
                            SongCampRegister.TryGetValue(Bekill.RoleID, out _FinBeKill);
                        }
                        else if (Bekill.Camp == 20)
                        {
                            JinCampRegister.TryGetValue(Bekill.RoleID, out _FinBeKill);
                        }

                        if (_FindKill != null && _FinBeKill != null)
                        {
                            int KillCurenScore = _FindKill.Score;

                            int KillCurRank = _FindKill.Rank;

                            int BeKillCurRank = _FinBeKill.Rank;

                            int ScoreGet = GetKillScore(KillCurRank, BeKillCurRank, false);

                            int TotalScore = KillCurenScore + ScoreGet;

                            // Nếu giết liên tục 3 người
                            if (_FindKill.KillStreak + 1 > 3)
                            {
                                TotalScore = TotalScore + 150;
                            }

                            _FindKill.Kill = _FindKill.Kill + 1;

                            _FindKill.KillStreak = _FindKill.KillStreak + 1;
                            if (_FindKill.MaxKillStreak < _FindKill.KillStreak)
                            {
                                _FindKill.MaxKillStreak = _FindKill.KillStreak;
                            }
                            _FindKill.Score = TotalScore;
                            _FindKill.Rank = GetRank(TotalScore);

                            if (Kill.Camp == 10)
                            {
                                TotalSongScore += ScoreGet;
                            }
                            else if (Kill.Camp == 20)
                            {
                                TotalJinScore += ScoreGet;
                            }

                            // Reset lại KillStreak
                            _FinBeKill.KillStreak = 0;
                            _FinBeKill.BeKill = _FinBeKill.BeKill + 1;

                            UpdateTitleRank(_FindKill.Player, _FindKill.Rank);

                            // Gửi notify cho thằng giết
                            NotifySocreForPlayer(_FindKill, true);

                            // Gửi notify cho thằng bị giết
                            NotifySocreForPlayer(_FinBeKill, false);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Update điểm khi người chơi giết monster
        /// </summary>
        /// <param name="client"></param>
        /// <param name="monster"></param>
        private void PlayerKillMonster(KPlayer client, Monster monster)
        {
            // Nếu chiến trường đã bắt đầu
            if (BATTLESTATE == BattelStatus.STATUS_START || BATTLESTATE == BattelStatus.STATUS_PREPAREEND)
            {
                // Check xem có đang trong chiến trường không
                if ((client.Camp == 10 || client.Camp == 20) && (monster.Camp == 10 || monster.Camp == 20))
                {
                    BattlePlayer _Find = null;

                    // Nếu mà đúng là chiến trường đang diễn ra
                    if (client.m_Level >= _BattleConfig.MinLevel && client.m_Level < _BattleConfig.MaxLevel)
                    {
                        if (client.Camp == 10)
                        {
                            SongCampRegister.TryGetValue(client.RoleID, out _Find);
                        }
                        else if (client.Camp == 20)
                        {
                            JinCampRegister.TryGetValue(client.RoleID, out _Find);
                        }

                        int CurRank = _Find.Rank;

                        int MonsterID = monster.MonsterInfo.Code;

                        var RankOfMon = GetMonsterRank(MonsterID);

                        int ScoreGet = GetKillScore(CurRank, RankOfMon, true);

                        _Find.Score = _Find.Score + ScoreGet;

                        _Find.Rank = GetRank(_Find.Score);

                        UpdateTitleRank(_Find.Player, _Find.Rank);

                        if (client.Camp == 10)
                        {
                            TotalSongScore += ScoreGet;
                        }
                        else if (client.Camp == 20)
                        {
                            TotalJinScore += ScoreGet;
                        }

                        NotifySocreForPlayer(_Find, false);

                        if (monster.MonsterInfo.Code == 644 || monster.MonsterInfo.Code == 638)
                        {
                            string Notify = KTGlobal.CreateStringByColor("Cấp Báo", ColorType.Importal) + " phe " + KTGlobal.CreateStringByColor(Battel_SonJin_Manager.GetNameCamp(_Find.Camp), ColorType.Accpect) + " dũng cảm đánh bại <color=yellow>" + monster.MonsterInfo.Name + "</color>, toàn quân " + KTGlobal.CreateStringByColor(Battel_SonJin_Manager.GetNameCamp(_Find.Camp), ColorType.Accpect) + " nhận được 10.000 điểm tích lũy.";

                            NotifyAllBattle(Notify);

                            if (client.Camp == 10)
                            {
                                TotalSongScore = TotalSongScore + 10000;
                            }
                            else if (client.Camp == 20)
                            {
                                TotalJinScore = TotalJinScore + 10000;
                            }
                        }
                    }
                }
            }
        }

        #endregion KillEventProsecc
    }
}