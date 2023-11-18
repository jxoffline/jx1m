using GameServer.Core.Executor;
using GameServer.KiemThe.Core;
using GameServer.KiemThe.Core.Item;
using GameServer.KiemThe.Core.Task;
using GameServer.KiemThe.Entities;
using GameServer.KiemThe.Logic;
using GameServer.KiemThe.Logic.Manager;
using GameServer.KiemThe.LuaSystem.Logic;
using GameServer.Logic;
using GameServer.Server;
using Server.Data;
using Server.Tools;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

using static GameServer.Logic.KTMapManager;

namespace GameServer.KiemThe.GameEvents.FactionBattle
{
    public class FactionBattle
    {
        /// <summary>
        /// Sự kiện tick gần nhất
        /// </summary>
        public long LastTick { get; set; }

        public long LastExpTick { get; set; }

        public long LastCreateFlag { get; set; }

        /// <summary>
        /// Thời gian notify gần đây nhất
        /// </summary>
        public long LastNofity { get; set; }

        /// <summary>
        /// Trạng thái của trận PVP 1vs1
        /// </summary>
        public ROUNDSTATE _ROUNDSTATE { get; set; }

        /// <summary>
        /// Bảng xếp hạng PVP hỗn chiến sử dụng để send về client
        /// </summary>
        public FACTION_PVP_RANKING_INFO _RANK = new FACTION_PVP_RANKING_INFO();

        /// <summary>
        /// Tổng số người đăng ký PVP
        /// </summary>
        public ConcurrentDictionary<int, FactionPlayer> PVPARENA = new ConcurrentDictionary<int, FactionPlayer>();

        /// <summary>
        /// Bảng xếp hạng hỗn chiến các người chơi với nhau
        /// </summary>
        public List<FactionPlayer> BattleFactionRank = new List<FactionPlayer>();

        /// <summary>
        /// Sơ đồ thi đấu lôi đài
        /// </summary>
        public List<ELIMINATION_INFO> ELIMINATION_TOTAL = new List<ELIMINATION_INFO>();

        public FactionState BATTLESTATE { get; set; }

        /// <summary>
        /// Môn phái nào
        /// </summary>
        public int FactionID { get; set; }

        /// <summary>
        /// Lấy ra tên phái
        /// </summary>
        public string GetFactionName
        {
            get
            {
                return KFaction.GetName(this.FactionID);
            }
        }

        public int GetMapCode
        {
            get
            {
                return _Config.MapFactionList.Where(x => x.FactionID == this.FactionID).FirstOrDefault().MapId;
            }
        }

        public FactionDef _Config { get; set; }

        public FactionBattle(int FactionID, FactionDef Def)
        {
            this.FactionID = FactionID;
            this._Config = Def;
        }

        #region ClearBattle

        public void ResetBattle()
        {
            LastTick = 0;
            LastExpTick = 0;
            LastCreateFlag = 0;
            LastNofity = 0;

            _RANK = new FACTION_PVP_RANKING_INFO();

            BATTLESTATE = FactionState.NOTHING;
            // Làm sạch danh sách đăng ký
            PVPARENA = new ConcurrentDictionary<int, FactionPlayer>();
            //  Làm sạch bảng xếp hạng
            BattleFactionRank = new List<FactionPlayer>();

            //Làm sạch Sơ đồ thi đấu
            ELIMINATION_TOTAL = new List<ELIMINATION_INFO>();

            _ROUNDSTATE = ROUNDSTATE.NONE;
        }

        #endregion ClearBattle

        public void Start()
        {
            ScheduleExecutor2.Instance.scheduleExecute(new NormalScheduleTask("FactionBattle_" + this.FactionID, ProsecBattle), 5 * 1000, 2000);
        }

        public bool showdown()
        {
            return true;
        }

        #region BattleGMCOMMAND

        public void ForceStartBattle()
        {
            ResetBattle();
            LastTick = TimeUtil.NOW();
            BATTLESTATE = FactionState.SIGN_UP;

            KTGlobal.SendSystemEventNotification("Thi đấu môn phái  [" + this.GetFactionName + "] đã mở, hãy đến gặp chưởng môn để vào ghi danh tham dự.");
        }

        public void ForceEndBattle()
        {
            // ResetBattle();
            LastTick = TimeUtil.NOW();
            BATTLESTATE = FactionState.CHAMPION_AWARD;

            KTGlobal.SendSystemEventNotification("Thi đấu môn phái đã kết thúc.");
        }

        #endregion BattleGMCOMMAND

        #region BattelTeleport

        public bool CheckExits(KPlayer Player)
        {
            if (this.GetMapCode == Player.MapCode)
            {
                return true;
            }

            if (PVPARENA.TryGetValue(Player.RoleID, out FactionPlayer Facetion))
            {
                if (BATTLESTATE != FactionState.NOTHING)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

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

        /// <summary>
        /// Revice hồi sinh
        /// </summary>
        /// <param name="Player"></param>
        public void Revice(KPlayer Player)
        {
            if (BATTLESTATE == FactionState.NOTHING || BATTLESTATE == FactionState.END)
            {
                KT_TCPHandler.GetLastMapInfo(Player, out int preMapCode, out int prePosX, out int prePosY);

                KTPlayerManager.ChangeMap(Player, preMapCode, prePosX, prePosY);
            }

            // Thực hiện hồi sinh trong PVP
            else if (BATTLESTATE == FactionState.MELEE_ROUND_1 || BATTLESTATE == FactionState.MELEE_ROUND_2)
            {
                int RandomSwapn = new Random().Next(0, _Config.PVPRandomRevice.Count);

                Postion _GetPostion = _Config.PVPRandomRevice[RandomSwapn];

                KTPlayerManager.Relive(Player, GetMapCode, _GetPostion.PosX, _GetPostion.PosY, 100, 100, 100);
            }
            // Nếu chết trong trận solo thì đưa ra điểm hồi sinh
            else /*if(BATTLESTATE == FactionState.ELIMINATION_ROUND_1 || BATTLESTATE == FactionState.ELIMINATION_ROUND_2 || BATTLESTATE == FactionState.ELIMINATION_ROUND_3 || BATTLESTATE == FactionState.ELIMINATION_ROUND_4)*/
            {
                PVPARENA.TryGetValue(Player.RoleID, out FactionPlayer _OUTPLAYER);
                if (_OUTPLAYER != null)
                {
                    // set camp cho thằng này = -1 và hòa bình
                    _OUTPLAYER.player.Camp = -1;
                    _OUTPLAYER.player.PKMode = (int)PKMode.Peace;
                }

                KTPlayerManager.Relive(Player, GetMapCode, _Config.GoIn.PosX, _Config.GoIn.PosY, 100, 100, 100);
            }
        }

        /// <summary>
        /// Chuyển người chơi vào đấu trường
        /// </summary>
        /// <param name="KPlayer"></param>
        public void MovePlayerToArena(KPlayer KPlayer)
        {
            if (KPlayer.TeamID != -1)
            {
                KPlayer.LeaveTeam();
            }

            // Nếu là thời gian đăng ký hoặc thời gian nghỉ
            if (BATTLESTATE == FactionState.SIGN_UP || BATTLESTATE == FactionState.FREE_MELEE || BATTLESTATE == FactionState.READY_ELIMINATION || BATTLESTATE == FactionState.FREE_ELIMINATION_ROUND_1 || BATTLESTATE == FactionState.FREE_ELIMINATION_ROUND_2 || BATTLESTATE == FactionState.FREE_ELIMINATION_ROUND_3 || BATTLESTATE == FactionState.CHAMPION_AWARD || BATTLESTATE == FactionState.ELIMINATION_ROUND_1 || BATTLESTATE == FactionState.ELIMINATION_ROUND_2 || BATTLESTATE == FactionState.ELIMINATION_ROUND_3 || BATTLESTATE == FactionState.ELIMINATION_ROUND_4)
            {
                KPlayer.PKMode = (int)PKMode.Peace;

                KPlayer.Camp = -1;
                //Cấm dùng skill toàn bộ người chơi
                KPlayer.ForbidUsingSkill = true;
                KPlayer.StopAllActiveFights();

                PlayChangeState(KPlayer, 1);
                // Ghi lại vị trí trước khi vào
                KT_TCPHandler.UpdateLastMapInfo(KPlayer, KPlayer.CurrentMapCode, KPlayer.PosX, KPlayer.PosY);
                // Chuyển người chơi vào map
                KTPlayerManager.ChangeMap(KPlayer, this.GetMapCode, _Config.GoIn.PosX, _Config.GoIn.PosY);
            } // Nếu đang là vòng hỗn chiến thì cho vào lại
            else if (BATTLESTATE == FactionState.MELEE_ROUND_1 || BATTLESTATE == FactionState.MELEE_ROUND_2)
            {
                KPlayer.PKMode = (int)PKMode.Custom;

                KPlayer.Camp = KPlayer.RoleID;
                //Cấm dùng skill toàn bộ người chơi
                KPlayer.ForbidUsingSkill = false;
                //  KPlayer.StopAllActiveFights();

                PlayChangeState(KPlayer, 1);

                List<Postion> RandomPost = _Config.PVPRandomRevice;

                int Random = new Random().Next(0, RandomPost.Count);

                Postion _Position = RandomPost[Random];

                KTPlayerManager.ChangeMap(KPlayer, this.GetMapCode, _Position.PosX, _Position.PosY);

                // GameManager.ClientMgr.NotifyOthersGoBack(Global._TCPManager.MySocketListener, Global._TCPManager.TcpOutPacketPool, KPlayer, _Position.X, _Position.Y, (int)KPlayer.CurrentDir);
            }
        }

        #endregion BattelTeleport

        public bool startup()
        {
            ResetBattle();

            this.Start();
            return true;
        }

        #region Register

        public int Register(KPlayer player)
        {
            /// Hiện tại không có trận thi đấu môn phái nào！
            if (BATTLESTATE == FactionState.NOTHING || BATTLESTATE == FactionState.END)
            {
                return -1;
            } // Thi đấu môn phái đã bắt đầu lúc 20:00h，hiện tại không thể tiếp nhận đăng ký
           
            if (player.m_cPlayerFaction.GetFactionId() != this.FactionID)
            {
                return -3;
            } //Cấp độ của bạn không đủ"..self.MIN_LEVEL.."Không thể tham gia thi đấu môn phái
            if (player.m_Level < _Config.MIN_LEVEL)
            {
                return -4;
            }

            if (player.TeamID != -1)
            {
                return -40;
            }
            //Số người đăng ký đã đến mức giới hạn 400 người，không thể tiếp nhận đăng ký，mời bạn tham gia các hoạt động khác tại đây");
            if (PVPARENA.Count >= _Config.MAX_ATTEND_PLAYER)
            {
                return -5;
            }

            if (PVPARENA.TryGetValue(player.RoleID, out FactionPlayer Player))
            {
                if (Player != null)
                {
                    // Nếu đang solo mà thằng này thoát ra
                    if (BATTLESTATE == FactionState.ELIMINATION_ROUND_1 || BATTLESTATE == FactionState.ELIMINATION_ROUND_2 || BATTLESTATE == FactionState.ELIMINATION_ROUND_3 || BATTLESTATE == FactionState.ELIMINATION_ROUND_4)
                    {
                        Player.IsReconnect = true;
                    }

                    // Gán lại player cho FACTIONBATTLE
                    Player.player = player;

                    LogManager.WriteLog(LogTypes.FactionBattle, "RELOGIN [" + player.RoleName + "][" + _ROUNDSTATE.ToString() + "]");

                    this.MovePlayerToArena(player);
                }
                return 1;
            }
            else
            {
                // Đăng ký thành công
                FactionPlayer _Player = new FactionPlayer();
                _Player.nArenaId = 100;
                _Player.nDeathCount = 0;
                _Player.nScore = 0;
                _Player.player = player;
                _Player.Rank = 0;
                _Player.TotalFlagCollect = 0;
                _Player.AreadyGetAward = false;
                _Player.ReviceTanNhanVuong = false;
                _Player.DamgeRecore = 0;
                // SET CHO WIN TRẬN NÀO = KO
                _Player.IsWinRound = false;

                PVPARENA.TryAdd(player.RoleID, _Player);

                this.MovePlayerToArena(player);

                return 0;
            }
        }

        /// <summary>
        /// Swith State của người chơi sang 1 cái bảng mới
        /// </summary>
        /// <param name="Player"></param>
        /// <param name="State"></param>
        public void PlayChangeState(KPlayer Player, int State)
        {
            G2C_EventState _State = new G2C_EventState();

            _State.EventID = 21;
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

        #endregion Register

        #region Notify Faction

        /// <summary>
        /// Update text về client
        /// </summary>
        /// <param name="IsNotiyText"></param>
        ///
        public void UpdatePreading()
        {
            long SEC = (LastTick + _Config.TimeAct.SIGN_UP_DULATION * 1000) - TimeUtil.NOW();

            int FinalSec = (int)(SEC / 1000);

            foreach (KeyValuePair<int, FactionPlayer> entry in PVPARENA)
            {
                FactionPlayer PlayerBattle = entry.Value;

                if (PlayerBattle.player.TeamID != -1)
                {
                    PlayerBattle.player.LeaveTeam();
                }

                if (PlayerBattle.player.CurrentMapCode == GetMapCode)
                {
                    // Gửi thông báo về thời gian đăng ký còn bao lâu cho bọn nó biết
                    this.SendPreadingNotify(PlayerBattle.player, FinalSec, PVPARENA.Count, true);
                }
            }
        }

        public void UpdatePreadingPvpRound2()
        {
            long SEC = (LastTick + _Config.TimeAct.MELEE_PVP_FREE_DUALTION * 1000) - TimeUtil.NOW();

            int FinalSec = (int)(SEC / 1000);

            foreach (KeyValuePair<int, FactionPlayer> entry in PVPARENA)
            {
                FactionPlayer PlayerBattle = entry.Value;

                if (PlayerBattle.player.CurrentMapCode == GetMapCode)
                {
                    // Gửi thông báo về thời gian đăng ký còn bao lâu cho bọn nó biết
                    this.SendPreadingNotify(PlayerBattle.player, FinalSec, PVPARENA.Count, false);
                }
            }
        }

        #region AWARD_REVICE

        public int GetRankFromTop(FactionPlayer _OutPlayer)
        {
            int Rank = -1;

            if (_OutPlayer.BestElimateRank == 1)
            {
                return 16;
            }
            else if (_OutPlayer.BestElimateRank == 2)
            {
                return 8;
            }
            else if (_OutPlayer.BestElimateRank == 3)
            {
                return 4;
            }
            else if (_OutPlayer.BestElimateRank == 4)
            {
                return 2;
            }

            ELIMINATION_INFO _FINDQUANQUAN = ELIMINATION_TOTAL.Where(x => x.ROUNDID == 4).FirstOrDefault();

            if (_OutPlayer.player.RoleID == _FINDQUANQUAN.WinThisRound.player.RoleID)
            {
                return 1;
            }

            return Rank;
        }

        public long GetMaxExpCanEarn(FactionPlayer _OutPlayer, int Rank)
        {
            int BasExp = KPlayerSetting.GetBaseExpLevel(_OutPlayer.player.m_Level);

            double BoundExpFromFlag = _OutPlayer.nScore * 1.0 / 1000;

            double BoudExp = BoundExpFromFlag * 12000 * 90;

            if (Rank == 16)
            {
                BoudExp += 90 * 12000;
            }
            else if (Rank == 8)
            {
                BoudExp += 120 * 12000;
            }
            else if (Rank == 4)
            {
                BoudExp += 180 * 12000;
            }
            else if (Rank == 2)
            {
                BoudExp += 180 * 12000;
            }
            else if (Rank == 1)
            {
                BoudExp += 240 * 12000;
            }

            return (long)BoudExp;
        }

        /// <summary>
        /// CHECK AWARD
        /// </summary>
        /// <param name="map"></param>
        /// <param name="npc"></param>
        /// <param name="client"></param>
        public void NpcQuanQuan(GameMap map, NPC npc, KPlayer client)
        {
            string Text = "";
            KNPCDialog _NpcDialog = new KNPCDialog();

            Dictionary<int, string> Selections = new Dictionary<int, string>();

            PVPARENA.TryGetValue(client.RoleID, out FactionPlayer _OutPlayer);

            if (_OutPlayer != null)
            {
                if (BATTLESTATE == FactionState.CHAMPION_AWARD)
                {
                    ELIMINATION_INFO _FINDQUANQUAN = ELIMINATION_TOTAL.Where(x => x.ROUNDID == 4).FirstOrDefault();

                    Text = "Xin chào <b><color=#00ff2a>" + client.RoleName + "</color></b>\n";

                    int RANKING = this.GetRankFromTop(_OutPlayer);
                    if (RANKING != -1)
                    {
                        Text += "Bạn có hạng <b><color=#00ff2a>" + RANKING + "</color></b> trong đợt Thi Đấu Môn Phái này\n";
                    }

                    Text += "Điểm hiện tại của bạn là: <color=#00ff2a>" + _OutPlayer.nScore + "</color>, Có thể đổi: <color=#00ff2a>" + GetMaxExpCanEarn(_OutPlayer, RANKING) + "</color> kinh nghiệm\n";

                    var FindAdward = _Config.FactionAward.Where(x => x.TopRank == RANKING).FirstOrDefault();

                    if (FindAdward != null)
                    {
                        Text += "Danh vọng môn phái :<color=green>" + FindAdward.Point + "</color>\n";

                        string ItemList = FindAdward.ItemList;

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

                        Text += "<color=green>" + ItemStr + "</color>";
                    }

                    // Nếu là tân nhân vương thì add thêm cho nó 1 dòng nhận thưởng chuyên cho tân nhân vương = bao gồm danh hiệu + tiền
                    if (client.RoleID == _FINDQUANQUAN.WinThisRound.player.RoleID)
                    {
                        if (!_OutPlayer.ReviceTanNhanVuong)
                        {
                            Selections.Add(-3, "Nhận thưởng <color=red>Tân Nhân Vương</color>");
                        }
                    }

                    Selections.Add(-4, "Nhận thưởng thi đấu môn phái");

                    Action<TaskCallBack> ActionWork = (x) => DoActionSelect(_OutPlayer, npc, x, RANKING);

                    _NpcDialog.OnSelect = ActionWork;
                }
                else
                {
                    Text = "Xin chào <b><color=#00ff2a>" + client.RoleName + "</color></b>\nChưa phải thời gian nhận thưởng";
                }
            }

            _NpcDialog.Selections = Selections;

            _NpcDialog.Text = Text;

            _NpcDialog.Show(npc, client);
        }

        /// <summary>
        /// Lấy ra title
        /// </summary>
        /// <param name="FactionID"></param>
        /// <returns></returns>
        public int GetTitleIDByFactionID(int FactionID)
        {
            var FINDFACTIONTITLE = _Config.TitleFaction.Where(x => x.FactionID == FactionID).FirstOrDefault();

            if (FINDFACTIONTITLE != null)
            {
                return FINDFACTIONTITLE.TitleID;
            }
            else
            {
                return -1;
            }
        }

        private void DoActionSelect(FactionPlayer outPlayer, NPC npc, TaskCallBack Select, int RANKING)
        {
            if (Select.SelectID == -3)
            {
                ELIMINATION_INFO _FINDQUANQUAN = ELIMINATION_TOTAL.Where(x => x.ROUNDID == 4).FirstOrDefault();

                if (outPlayer.player.RoleID == _FINDQUANQUAN.WinThisRound.player.RoleID)
                {
                    if (!outPlayer.ReviceTanNhanVuong)
                    {
                        outPlayer.player.RemoveSpecialTitle();

                        int TITLEID = GetTitleIDByFactionID(this.FactionID);

                        LogManager.WriteLog(LogTypes.FactionBattle, "[" + outPlayer.player.RoleID + "][" + outPlayer.player.RoleName + "]SET TITLEID [" + TITLEID + "]");

                        outPlayer.player.SetSpecialTitle(TITLEID);

                        outPlayer.ReviceTanNhanVuong = true;

                        // TẠO NỐT SỐ RƯƠNG CÒN LẠI
                        Box _BoxConfig = _Config.Box.Where(xx => xx.ArenaID == 0).FirstOrDefault();

                        // TẠO RANDOM từ 3 tới 6 rương
                        int Random = KTGlobal.GetRandomNumber(6, 12);

                        for (int i = 0; i < Random; i++)
                        {
                            int BoxCount = _BoxConfig.RandomBox.Count;

                            int RandomPos = KTGlobal.GetRandomNumber(0, BoxCount);
                            if (RandomPos > 0)
                            {
                                RandomPos = RandomPos - 1;
                            }
                            Postion _Postion = _BoxConfig.RandomBox[RandomPos];

                            GameMap Map = KTMapManager.Find(this.GetMapCode);

                            // TẠON BOX Ở VỊ TRÍ TƯƠNG ỨNG
                            CreateBox(Map, _Postion.PosX, _Postion.PosY);
                        }
                    }
                    else
                    {
                        KTLuaLib_Player.SendMSG("Bạn đã nhận thưởng tân nhân vương rồi", outPlayer.player, npc);
                    }
                }
                else
                {
                    KTLuaLib_Player.SendMSG("Bạn không phải tân nhân vương", outPlayer.player, npc);
                }
            }
            else if (Select.SelectID == -4)
            {
                if (!outPlayer.AreadyGetAward)
                {
                    //Đánh dấu là đã nhận thưởng rồi
                    outPlayer.AreadyGetAward = true;
                    //Fund reward của thằng này
                    var FindAdward = _Config.FactionAward.Where(x => x.TopRank == RANKING).FirstOrDefault();

                    if (FindAdward != null)
                    {
                        string ItemList = FindAdward.ItemList;
                        string ItemStr = "";

                        if (ItemList.Length > 0)
                        {
                            string[] ItemArray = ItemList.Split(';');

                            foreach (string StrItem in ItemArray)
                            {
                                string[] ItemPram = StrItem.Split('|');

                                int ItemID = Int32.Parse(ItemPram[0]);
                                int ItemNum = Int32.Parse(ItemPram[1]);

                                // Tạo vật phẩm cho thằng top
                                if (!ItemManager.CreateItem(Global._TCPManager.TcpOutPacketPool, outPlayer.player, ItemID, ItemNum, 0, "FACTIONBATTLE", false, 1, false, ItemManager.ConstGoodsEndTime))
                                {
                                    KTPlayerManager.ShowNotification(outPlayer.player, "Có lỗi khi nhận phần thưởng");
                                }
                            }
                        }

                        int CAMP = 300 + outPlayer.player.m_cPlayerFaction.GetFactionId();

                        KTGlobal.AddRepute(outPlayer.player, CAMP, FindAdward.Point);
                    }

                    // Xem thằng này có EXP không
                    long MaxExp = GetMaxExpCanEarn(outPlayer, RANKING);

                    if (MaxExp > 0)
                    {
                        // add Exp
                        KTPlayerManager.AddExp(outPlayer.player, MaxExp);
                    }

                    KTLuaLib_Player.SendMSG("Nhận thưởng thành công", outPlayer.player, npc);
                }
                else
                {
                    KTLuaLib_Player.SendMSG("Bạn đã nhận thưởng rồi", outPlayer.player, npc);
                }
            }
        }

        #endregion AWARD_REVICE

        /// <summary>
        /// Thông báo thời gian đăng ký còn bao lâu
        /// </summary>
        /// <param name="Player"></param>
        /// <param name="Sec"></param>
        /// <param name="TotalTong"></param>
        /// <param name="TotalKim"></param>
        /// <param name="IsNotiyText"></param>
        public void SendPreadingNotify(KPlayer Player, int Sec, int TotalTong, bool IsFistRound)
        {
            ///Chuyển trạng thái cái bảng về cái bảng notify

            this.PlayChangeState(Player, 1);

            Player.ForbidUsingSkill = true;
            Player.Camp = 0;

            ///Show cái trạng thái
            G2C_EventNotification _Notify = new G2C_EventNotification();

            _Notify.EventName = "Pk tự do bắt đầu sau :";

            _Notify.ShortDetail = "TIME|" + Sec;

            _Notify.TotalInfo = new List<string>();
            if (IsFistRound)
            {
                _Notify.TotalInfo.Add("Số người đã báo danh :" + TotalTong);
            }
            else
            {
                _Notify.TotalInfo.Add("Lần hỗn chiến thứ :" + ((int)BATTLESTATE - 1));
            }

            if (Player.IsOnline())
            {
                Player.SendPacket<G2C_EventNotification>((int)TCPGameServerCmds.CMD_KT_EVENT_NOTIFICATION, _Notify);
            }
            else
            {
                //Console.WriteLine("OFFLINE");
            }
        }

        #endregion Notify Faction

        #region BXH

        public void UpadateBXH()
        {
            lock (BattleFactionRank)
            {
                List<FactionPlayer> TEMP = new List<FactionPlayer>();

                foreach (FactionPlayer Player in PVPARENA.Values)
                {
                    TEMP.Add(Player);
                }

                //SOFT LẠI DANH SÁCH Theo danh sách giảm dần
                List<FactionPlayer> _Soft = TEMP.OrderByDescending(x => x.nScore).ToList();

                // GÁN LẠI SOFT = DANH SÁCH MỚI ĐÃ SẮP XẾP
                BattleFactionRank = _Soft;
            }
        }

        public int GetRankInBxh(int RoleID)
        {
            lock (BattleFactionRank)
            {
                var find = BattleFactionRank.FindIndex(x => x.player.RoleID == RoleID);

                return find;
            }
        }

        public FACTION_PVP_RANKING_INFO RankingBuilder(KPlayer Client)
        {
            FACTION_PVP_RANKING_INFO _RANK_BUILD = new FACTION_PVP_RANKING_INFO();

            if (BATTLESTATE < FactionState.READY_ELIMINATION)
            {
                _RANK_BUILD.State = 0;
                _RANK_BUILD.ELIMINATION_SCORE = new List<ELIMINATION_SCOREBOARD>();
                _RANK_BUILD.PlayerRanks = new List<FACTION_PVP_RANKING>();

                // LẤY RA TOP 50 THẰNG KHỎE NHẤT TRONG PVP HỖN CHIẾN
                List<FactionPlayer> TOP50 = new List<FactionPlayer>();
                if (BattleFactionRank.Count > 50)
                {
                    TOP50 = BattleFactionRank.GetRange(0, 49);
                }
                else
                {
                    TOP50 = BattleFactionRank;
                }

                for (int i = 0; i < TOP50.Count; i++)
                {
                    FactionPlayer _FACTION = TOP50[i];

                    FACTION_PVP_RANKING _Rank = new FACTION_PVP_RANKING();

                    _Rank.Faction = _FACTION.player.m_cPlayerFaction.GetFactionId();
                    _Rank.KillCount = _FACTION.nScore;
                    _Rank.Level = _FACTION.player.m_Level;
                    _Rank.MaxKillStreak = _FACTION.MaxKillStreak;
                    _Rank.PlayerName = _FACTION.player.RoleName;
                    _Rank.Rank = i + 1;
                    _Rank.Score = _FACTION.nScore;
                    _RANK_BUILD.PlayerRanks.Add(_Rank);
                }

                var findHost = BattleFactionRank.Where(x => x.player.RoleID == Client.RoleID).FirstOrDefault();
                if (findHost != null)
                {
                    FACTION_PVP_RANKING _Rank = new FACTION_PVP_RANKING();

                    _Rank.Faction = findHost.player.m_cPlayerFaction.GetFactionId();
                    _Rank.KillCount = findHost.nScore;
                    _Rank.Level = findHost.player.m_Level;
                    _Rank.MaxKillStreak = findHost.KillStreak;
                    _Rank.PlayerName = findHost.player.RoleName;
                    _Rank.Rank = GetRankInBxh(findHost.player.RoleID) + 1;
                    _Rank.Score = findHost.nScore;
                    _RANK_BUILD.PlayerRanks.Add(_Rank);
                }
            }
            else
            {
                _RANK_BUILD.State = 1;
                _RANK_BUILD.ELIMINATION_SCORE = new List<ELIMINATION_SCOREBOARD>();
                _RANK_BUILD.PlayerRanks = new List<FACTION_PVP_RANKING>();

                foreach (ELIMINATION_INFO _INFO in ELIMINATION_TOTAL)
                {
                    ELIMINATION_SCOREBOARD _RANK = new ELIMINATION_SCOREBOARD();

                    // Định nghĩa ranking

                    _RANK.ARENAID = _INFO.ARENAID;
                    _RANK.Player_1 = _INFO.Player_1.player.RoleName;
                    _RANK.Player_2 = _INFO.Player_2.player.RoleName;
                    _RANK.ROUNDID = _INFO.ROUNDID;
                    _RANK._ROUNDSTATE = _INFO._ROUNDSTATE;

                    _RANK_BUILD.ELIMINATION_SCORE.Add(_RANK);
                }
            }

            return _RANK_BUILD;
        }

        #endregion BXH

        /// <summary>
        /// Disable attack all
        /// </summary>
        private void DisableAttackAllRegister()
        {
            foreach (KeyValuePair<int, FactionPlayer> entry in PVPARENA)
            {
                FactionPlayer PlayerBattle = entry.Value;

                PlayerBattle.player.PKMode = (int)PKMode.Peace;
                //// Set camp cho người chơi
                PlayerBattle.player.Camp = -1;

                // mở khóa sử dụng skill
                entry.Value.player.ForbidUsingSkill = true;

                /// Ngừng toàn bộ tuyên chiến
                PlayerBattle.player.StopAllActiveFights();
            }
        }

        /// <summary>
        /// Cho mỗi thằng 1 camp
        /// </summary>
        public void EnableAttackAllRegister()
        {
            foreach (KeyValuePair<int, FactionPlayer> entry in PVPARENA)
            {
                FactionPlayer PlayerBattle = entry.Value;

                PlayerBattle.player.PKMode = (int)PKMode.Custom;
                //// Set camp cho người chơi
                PlayerBattle.player.Camp = PlayerBattle.player.RoleID;
                // mở khóa sử dụng skill
                entry.Value.player.ForbidUsingSkill = false;
                /// Ngừng toàn bộ tuyên chiến
                PlayerBattle.player.StopAllActiveFights();
            }
        }

        /// <summary>
        /// Xử lý các sự kiện sẽ xảy ra ở thi đấu môn phía
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ProsecBattle(object sender, EventArgs e)
        {
            try
            {
                // Nếu chưa báo danh
                if (BATTLESTATE == FactionState.NOTHING)
                {
                    List<int> DayOfWeek = _Config.OPEN_WEEK_DATE;

                    int Today = TimeUtil.GetWeekDay1To7(DateTime.Now);

                    if (DayOfWeek.Contains(Today))
                    {
                        DateTime Now = DateTime.Now;

                        if (Now.Hour == _Config.TimeBattle.Hours && Now.Minute == _Config.TimeBattle.Minute && Now.Second >= _Config.TimeBattle.Second && BATTLESTATE == FactionState.NOTHING)
                        {
                            LastTick = TimeUtil.NOW();

                            BATTLESTATE = FactionState.SIGN_UP;

                            LogManager.WriteLog(LogTypes.FactionBattle, "[" + GetFactionName + "] Battle Change State ==> " + BATTLESTATE.ToString());

                            KTGlobal.SendSystemEventNotification("Thi đấu môn phái đã mở, hãy đến gặp chưởng môn để vào ghi danh tham dự");
                        }
                    }
                }

                // Nếu đang trong trạng thái đăng ký
                else if (BATTLESTATE == FactionState.SIGN_UP)
                {
                    // Cứ 5s update notify register về toàn bộ bọn register 1 lần
                    if (TimeUtil.NOW() - LastNofity >= 5 * 1000 && BATTLESTATE == FactionState.SIGN_UP)
                    {
                        //DisableAttackAllRegister();

                        LastNofity = TimeUtil.NOW();

                        UpdatePreading();
                    }

                    if (TimeUtil.NOW() >= LastTick + _Config.TimeAct.SIGN_UP_DULATION * 1000 && BATTLESTATE == FactionState.SIGN_UP)
                    {
                        // Call bảng xếp hạng thử
                        UpadateBXH();

                        // Nếu số người tham gia chẵn đúng = số có thể xếm lôi đài thực hiện xếp lôi đài luôn
                        if (BattleFactionRank.Count == 2 || BattleFactionRank.Count == 4 || BattleFactionRank.Count == 8 || BattleFactionRank.Count == 16)
                        {
                            LastTick = TimeUtil.NOW();

                            this.DisableAttackAllRegister();

                            // Chuyển sang trạng thái nhặt cờ chuẩn bị thi đấu các thứu
                            BATTLESTATE = FactionState.READY_ELIMINATION;

                            // Chuyển toàn bộ người chơi ra ngoài đợi tới hiệp tiếp theo
                            this.MoveAllPlayerOutPvp();

                            LastNofity = TimeUtil.NOW();

                            // Thêm đoạn này tránh bug
                            if (BattleFactionRank.Count >= 2)
                            {
                                CreateSoloBoard(BattleFactionRank);
                            }

                            LogManager.WriteLog(LogTypes.FactionBattle, "[" + GetFactionName + "] Battle Change State ==> " + BATTLESTATE.ToString());
                        }
                        else
                        {
                            LastTick = TimeUtil.NOW();

                            BATTLESTATE = FactionState.MELEE_ROUND_1;

                            // Mở attack cho toàn bộ
                            EnableAttackAllRegister();

                            /// Chuyển toàn bộ người chơi vào ARENA
                            MoveAllPlayerToPvp();
                        }
                    }
                }
                else if (BATTLESTATE == FactionState.MELEE_ROUND_1)
                {
                    //Cứ mỗi 5s notify về kết quả trận đấu
                    if (TimeUtil.NOW() - LastNofity >= 5 * 1000 && (BATTLESTATE == FactionState.MELEE_ROUND_1))
                    {
                        LastNofity = TimeUtil.NOW();

                        UpdateBattleNotify();
                        UpadateBXH();
                    }

                    // Nếu kết thúc vòng 1
                    if (TimeUtil.NOW() >= LastTick + (_Config.TimeAct.MELEE_PVP_DULATION * 1000) && BATTLESTATE == FactionState.MELEE_ROUND_1)
                    {
                        LastTick = TimeUtil.NOW();

                        // Chuyển sang trạng thái nghỉ ngơi giữa trận hỗn chiến
                        BATTLESTATE = FactionState.FREE_MELEE;

                        // Gọi Update Bảng xếp hạng lần cuối
                        UpadateBXH();

                        DisableAttackAllRegister();

                        // Chuyển toàn bộ người chơi ra ngoài đợi tới hiệp tiếp theo
                        this.MoveAllPlayerOutPvp();

                        LastNofity = TimeUtil.NOW();

                        UpdatePreadingPvpRound2();

                        LogManager.WriteLog(LogTypes.FactionBattle, "[" + GetFactionName + "] Battle Change State ==> " + BATTLESTATE.ToString());
                    }
                }
                else if (BATTLESTATE == FactionState.FREE_MELEE)
                {
                    // Cứ 5s update notify register về toàn bộ bọn register 1 lần
                    if (TimeUtil.NOW() - LastNofity >= 5 * 1000 && BATTLESTATE == FactionState.FREE_MELEE)
                    {
                        //this.DisableAttackAllRegister();
                        LastNofity = TimeUtil.NOW();
                        UpdatePreadingPvpRound2();
                    }

                    if (TimeUtil.NOW() >= LastTick + _Config.TimeAct.MELEE_PVP_FREE_DUALTION * 1000 && BATTLESTATE == FactionState.FREE_MELEE)
                    {
                        LastTick = TimeUtil.NOW();

                        BATTLESTATE = FactionState.MELEE_ROUND_2;

                        /// Chuyển toàn bộ người chơi vào ARENA chod đấu lần 2
                        MoveAllPlayerToPvp();

                        EnableAttackAllRegister();
                    }
                }
                else if (BATTLESTATE == FactionState.MELEE_ROUND_2) // Nếu như đang trong trận PVP tự do
                {
                    //Cứ mỗi 5s notify về kết quả trận đấu
                    if (TimeUtil.NOW() - LastNofity >= 5 * 1000 && (BATTLESTATE == FactionState.MELEE_ROUND_2))
                    {
                        LastNofity = TimeUtil.NOW();

                        UpdateBattleNotify();

                        UpadateBXH();
                    }

                    // Nếu kết thúc vòng 2
                    if (TimeUtil.NOW() >= LastTick + (_Config.TimeAct.MELEE_PVP_DULATION * 1000) && BATTLESTATE == FactionState.MELEE_ROUND_2)
                    {
                        LastTick = TimeUtil.NOW();

                        // Gọi Update Bảng xếp hạng lần cuối
                        UpadateBXH();

                        this.DisableAttackAllRegister();

                        // Chuyển sang trạng thái nhặt cờ chuẩn bị thi đấu các thứu
                        BATTLESTATE = FactionState.READY_ELIMINATION;

                        // Chuyển toàn bộ người chơi ra ngoài đợi tới hiệp tiếp theo
                        this.MoveAllPlayerOutPvp();

                        LastNofity = TimeUtil.NOW();

                        // Thêm đoạn này tránh bug
                        if (BattleFactionRank.Count >= 2)
                        {
                            CreateSoloBoard(BattleFactionRank);
                        }
                        else
                        {
                            NotifyALLTOBATTLE("Không đủ số người tham gia đăng ký.TDMP sẽ không thể bắt đầu");
                            KTGlobal.SendFactionChat(this.FactionID, "Không đủ số người tham gia đăng ký.TDMP sẽ không thể bắt đầu");
                            BATTLESTATE = FactionState.END;
                            //CreateTop16ScoreBoardFake(BattleFactionRank);
                        }

                        // SEND BẢNG THI ĐẤU VỀ CLIENT

                        LogManager.WriteLog(LogTypes.FactionBattle, "[" + GetFactionName + "] Battle Change State ==> " + BATTLESTATE.ToString());
                    }
                }
                // Nếu là thời gian nghỉ thì cho nhặt cờ
                else if (BATTLESTATE == FactionState.READY_ELIMINATION || BATTLESTATE == FactionState.FREE_ELIMINATION_ROUND_1 || BATTLESTATE == FactionState.FREE_ELIMINATION_ROUND_2 || BATTLESTATE == FactionState.FREE_ELIMINATION_ROUND_3)
                {
                    //Cứ mỗi 5s NOTIFY VỀ KẾT QUẢ CỜ QUẠT ĐIỂM TÍCH LŨY CỜ
                    if (TimeUtil.NOW() - LastNofity >= 5 * 1000 && (BATTLESTATE == FactionState.READY_ELIMINATION || BATTLESTATE == FactionState.FREE_ELIMINATION_ROUND_1 || BATTLESTATE == FactionState.FREE_ELIMINATION_ROUND_2 || BATTLESTATE == FactionState.FREE_ELIMINATION_ROUND_3))
                    {
                        LastNofity = TimeUtil.NOW();

                        //this.DisableAttackAllRegister();
                        // Update tìm cờ
                        UpdateBattleFlagEvent();
                    }

                    //Cứ mỗi 10s tạo 20 cờ xuất hiện random trong bản đồ
                    if (TimeUtil.NOW() - LastCreateFlag >= 60 * 1000 && (BATTLESTATE == FactionState.READY_ELIMINATION || BATTLESTATE == FactionState.FREE_ELIMINATION_ROUND_1 || BATTLESTATE == FactionState.FREE_ELIMINATION_ROUND_2 || BATTLESTATE == FactionState.FREE_ELIMINATION_ROUND_3))
                    {
                        LastCreateFlag = TimeUtil.NOW();

                        NotifyALLTOBATTLE("Cờ tầm bảo đã xuất hiện, các đại hiệp hãy mau chóng đi tìm");

                        for (int i = 0; i < 20; i++)
                        {
                            GameMap Map = KTMapManager.Find(this.GetMapCode);

                            int RANDOM = KTGlobal.GetRandomNumber(0, _Config.RandomFlag.Count);
                            if (RANDOM > 0)
                            {
                                RANDOM = RANDOM - 1;
                            }
                            Postion _PointFlag = _Config.RandomFlag[RANDOM];

                            CreateFLAG(Map, _PointFlag.PosX, _PointFlag.PosY);
                        }
                    }

                    // Nết kết thục vòng tìm cờ
                    if (TimeUtil.NOW() >= LastTick + (_Config.TimeAct.FREE_ELIMINATION * 1000) && (BATTLESTATE == FactionState.READY_ELIMINATION || BATTLESTATE == FactionState.FREE_ELIMINATION_ROUND_1 || BATTLESTATE == FactionState.FREE_ELIMINATION_ROUND_2 || BATTLESTATE == FactionState.FREE_ELIMINATION_ROUND_3))
                    {
                        _ROUNDSTATE = ROUNDSTATE.PREADING;

                        int ROUNID = 0;

                        // Chuyển sang trạng thái nhặt cờ chuẩn bị thi đấu các thứu

                        if (BATTLESTATE == FactionState.READY_ELIMINATION)
                        {
                            ROUNID = 1;
                            BATTLESTATE = FactionState.ELIMINATION_ROUND_1;
                        }
                        else if (BATTLESTATE == FactionState.FREE_ELIMINATION_ROUND_1)
                        {
                            ROUNID = 2;
                            BATTLESTATE = FactionState.ELIMINATION_ROUND_2;
                        }
                        else if (BATTLESTATE == FactionState.FREE_ELIMINATION_ROUND_2)
                        {
                            ROUNID = 3;
                            BATTLESTATE = FactionState.ELIMINATION_ROUND_3;
                        }
                        else if (BATTLESTATE == FactionState.FREE_ELIMINATION_ROUND_3)
                        {
                            ROUNID = 4;
                            BATTLESTATE = FactionState.ELIMINATION_ROUND_4;
                        }

                        NotifyALLTOBATTLE("Vòng đấu loại PVP 1VS1 đã chính thức bắt đầu!");

                        //SET LẠI TICK CHO GAME
                        LastTick = TimeUtil.NOW();

                        // MOVE TOÀN BỘ NGƯỜI CHƠI VÀO ARENA

                        LogManager.WriteLog(LogTypes.FactionBattle, "[" + GetFactionName + "] Battle Change State ==> " + BATTLESTATE.ToString());

                        // DUYỆT RA TOÀN BỘ NGƯỜI CHƠI TRONG ROUND
                        List<ELIMINATION_INFO> ELIMINATION_TOTAL_ROUND = ELIMINATION_TOTAL.Where(x => x.ROUNDID == ROUNID).ToList();

                        foreach (ELIMINATION_INFO ARENA in ELIMINATION_TOTAL_ROUND)
                        {
                            ArenaPos _ARENAPOSTION = _Config.TotalArena.Where(x => x.ArenaID == ARENA.ARENAID).FirstOrDefault();

                            if (ARENA.Player_1.player != null && ARENA.Player_1.player.IsOnline() && ARENA.Player_1.player.MapCode == this.GetMapCode)
                            {
                                // SET Cho người chơi 1
                                ARENA.Player_1.player.PKMode = (int)PKMode.Peace;
                                ARENA.Player_1.player.Camp = -1;
                                ARENA.Player_1.player.ForbidUsingSkill = true;
                                //Send thông báo cho người chơi biết là trận đấu đã bắt đầu
                                KTPlayerManager.ShowNotification(ARENA.Player_1.player, "Thời gian chuẩn bị cho trận chiến là 60s");
                                // Gửi thông báo về cho thằng này thông tin trận đấu
                                SendBattleNotify1VS1(ARENA.Player_1, _ROUNDSTATE, 60, ARENA);

                                if (_ARENAPOSTION != null)
                                {
                                    this.ChangePos(ARENA.Player_1.player, _ARENAPOSTION.PosX, _ARENAPOSTION.PosY);
                                }
                            }

                            if (ARENA.Player_2.player != null && ARENA.Player_2.player.IsOnline() && ARENA.Player_2.player.MapCode == this.GetMapCode)
                            {
                                //// Set camp cho người chơi 2
                                ARENA.Player_2.player.PKMode = (int)PKMode.Peace;
                                ARENA.Player_2.player.Camp = -1;
                                ARENA.Player_2.player.ForbidUsingSkill = true;
                                //Send thông báo cho người chơi biết là trận đấu đã bắt đầu
                                KTPlayerManager.ShowNotification(ARENA.Player_2.player, "Thời gian chuẩn bị cho trận chiến là 60s");
                                // Gửi thông báo về cho thằng này thông tin trận đấu
                                SendBattleNotify1VS1(ARENA.Player_2, _ROUNDSTATE, 60, ARENA);
                                // set trạng thái đã bắt đầu chiến đấu

                                if (_ARENAPOSTION != null)
                                {
                                    this.ChangePos(ARENA.Player_2.player, _ARENAPOSTION.PosX, _ARENAPOSTION.PosY);
                                }
                            }

                            ARENA._ROUNDSTATE = ROUNDSTATE.PREADING;
                        }
                    }
                }
                else if (BATTLESTATE == FactionState.ELIMINATION_ROUND_1 || BATTLESTATE == FactionState.ELIMINATION_ROUND_2 || BATTLESTATE == FactionState.ELIMINATION_ROUND_3 || BATTLESTATE == FactionState.ELIMINATION_ROUND_4)
                {
                    if (TimeUtil.NOW() - LastNofity >= 3 * 1000 && (BATTLESTATE == FactionState.ELIMINATION_ROUND_1 || BATTLESTATE == FactionState.ELIMINATION_ROUND_2 || BATTLESTATE == FactionState.ELIMINATION_ROUND_3 || BATTLESTATE == FactionState.ELIMINATION_ROUND_4))
                    {
                        int ROUNID = 0;
                        // Chuyển sang trạng thái nhặt cờ chuẩn bị thi đấu các thứu

                        if (BATTLESTATE == FactionState.ELIMINATION_ROUND_1)
                        {
                            ROUNID = 1;
                        }
                        else if (BATTLESTATE == FactionState.ELIMINATION_ROUND_2)
                        {
                            ROUNID = 2;
                        }
                        else if (BATTLESTATE == FactionState.ELIMINATION_ROUND_3)
                        {
                            ROUNID = 3;
                        }
                        else if (BATTLESTATE == FactionState.ELIMINATION_ROUND_4)
                        {
                            ROUNID = 4;
                        }
                        LastNofity = TimeUtil.NOW();
                        // Notify tiến độ trận cho những người chơi khác
                        UpdatePVP1VS1TOOtherPlayer(ROUNID);
                    }

                    // NẾu đã qua 1 phút chuẩn bị
                    if (TimeUtil.NOW() - LastTick >= 60 * 1000 && _ROUNDSTATE == ROUNDSTATE.PREADING && (BATTLESTATE == FactionState.ELIMINATION_ROUND_1 || BATTLESTATE == FactionState.ELIMINATION_ROUND_2 || BATTLESTATE == FactionState.ELIMINATION_ROUND_3 || BATTLESTATE == FactionState.ELIMINATION_ROUND_4))
                    {
                        _ROUNDSTATE = ROUNDSTATE.START;

                        int ROUNID = 0;

                        // Chuyển sang trạng thái nhặt cờ chuẩn bị thi đấu các thứu

                        if (BATTLESTATE == FactionState.ELIMINATION_ROUND_1)
                        {
                            ROUNID = 1;
                        }
                        else if (BATTLESTATE == FactionState.ELIMINATION_ROUND_2)
                        {
                            ROUNID = 2;
                        }
                        else if (BATTLESTATE == FactionState.ELIMINATION_ROUND_3)
                        {
                            ROUNID = 3;
                        }
                        else if (BATTLESTATE == FactionState.ELIMINATION_ROUND_4)
                        {
                            ROUNID = 4;
                        }

                        // LẤY RA DANH SÁCH CÁC LÔI ĐÀI
                        List<ELIMINATION_INFO> ELIMINATION_TOTAL_ROUND = ELIMINATION_TOTAL.Where(x => x.ROUNDID == ROUNID).ToList();

                        long SEC = (LastTick + (_Config.TimeAct.ELIMINATION_ROUND_ROUND * 1000)) - TimeUtil.NOW();

                        // Hết 1 phút chuẩn bị chuyển toàn bộ người chơi sang trạng thái chiến đấu
                        foreach (ELIMINATION_INFO ARENA in ELIMINATION_TOTAL_ROUND)
                        {
                            // SET Cho người chơi 1
                            ARENA.Player_1.player.PKMode = (int)PKMode.Custom;
                            ARENA.Player_1.player.Camp = ARENA.Player_1.player.RoleID;
                            ARENA.Player_1.player.ForbidUsingSkill = false;
                            // Set lại là ko thằng nào thắng
                            ARENA.Player_1.IsWinRound = false;

                            // Gửi thông báo về cho thằng này thông tin trận đấu
                            SendBattleNotify1VS1(ARENA.Player_1, _ROUNDSTATE, (int)SEC, ARENA);

                            //Send thông báo cho người chơi biết là trận đấu đã bắt đầu
                            KTPlayerManager.ShowNotification(ARENA.Player_1.player, "PVP đã chính thức bắt đầu!");

                            //// Set camp cho người chơi 2
                            ARENA.Player_2.player.PKMode = (int)PKMode.Custom;
                            ARENA.Player_2.player.Camp = ARENA.Player_2.player.RoleID;
                            ARENA.Player_2.player.ForbidUsingSkill = false;
                            // Set lại là ko thằng nào thắng
                            ARENA.Player_2.IsWinRound = false;

                            // Gửi thông báo về cho thằng này thông tin trận đấu
                            SendBattleNotify1VS1(ARENA.Player_2, _ROUNDSTATE, (int)SEC, ARENA);

                            //Send thông báo cho người chơi biết là trận đấu đã bắt đầu
                            KTPlayerManager.ShowNotification(ARENA.Player_2.player, "PVP đã chính thức bắt đầu!");

                            // set trạng thái đã bắt đầu chiến đấu
                            ARENA._ROUNDSTATE = ROUNDSTATE.START;
                        }
                    }
                    // Nếu đã kết thức trận tính toán và tạo bảng xếp hạng mới
                    if (TimeUtil.NOW() >= LastTick + (_Config.TimeAct.ELIMINATION_ROUND_ROUND * 1000) && (BATTLESTATE == FactionState.ELIMINATION_ROUND_1 || BATTLESTATE == FactionState.ELIMINATION_ROUND_2 || BATTLESTATE == FactionState.ELIMINATION_ROUND_3 || BATTLESTATE == FactionState.ELIMINATION_ROUND_4))
                    {
                        int ROUNID = 0;

                        LastTick = TimeUtil.NOW();

                        if (BATTLESTATE == FactionState.ELIMINATION_ROUND_1)
                        {
                            ROUNID = 1;
                            BATTLESTATE = FactionState.FREE_ELIMINATION_ROUND_1;
                        }
                        else if (BATTLESTATE == FactionState.ELIMINATION_ROUND_2)
                        {
                            ROUNID = 2;
                            BATTLESTATE = FactionState.FREE_ELIMINATION_ROUND_2;
                        }
                        else if (BATTLESTATE == FactionState.ELIMINATION_ROUND_3)
                        {
                            ROUNID = 3;
                            BATTLESTATE = FactionState.FREE_ELIMINATION_ROUND_3;
                        }
                        else if (BATTLESTATE == FactionState.ELIMINATION_ROUND_4)
                        {
                            ROUNID = 4;
                            // NẾU ĐÃ KẾT THÚC TRẬN 4 thì là thời gian quân vương nhận thưởng
                            BATTLESTATE = FactionState.CHAMPION_AWARD;
                        }

                        _ROUNDSTATE = ROUNDSTATE.END;

                        LogManager.WriteLog(LogTypes.FactionBattle, "LIST RA TOÀN BỘ CÁC SÀN ĐẤU CHƯA PHÂN THẮNG BẠI CỦA ROUND :[" + ROUNID + "]!");
                        // Lấy ra toàn bộ các cặp đấu chưa phân thắng bại
                        List<ELIMINATION_INFO> ELIMINATION_TOTAL_ROUND = ELIMINATION_TOTAL.Where(x => x.ROUNDID == ROUNID && x.Player_1.IsWinRound == false && x.Player_2.IsWinRound == false && x.Player_1.IsReconnect == false && x.Player_2.IsReconnect == false && x.WinThisRound == null).ToList();

                        // Tính toán thắng thua cho FULL ARENA
                        foreach (ELIMINATION_INFO ARENA in ELIMINATION_TOTAL_ROUND)
                        {
                            LogManager.WriteLog(LogTypes.FactionBattle, "ARENAID :[" + ARENA.ARENAID + "] CHƯA PHÂN THĂNG BẠI");
                            long DAMGEPLAYER1 = ARENA.Player_1.DamgeRecore;
                            long DAMGEPLAYER2 = ARENA.Player_2.DamgeRecore;

                            if (DAMGEPLAYER1 > DAMGEPLAYER2)
                            {
                                KTPlayerManager.ShowMessageBox(ARENA.Player_1.player, "Thông báo kết quả", "Trong trận tỉ thí với " + ARENA.Player_2.player.RoleName + " bạn đã gây ra tổng " + KTGlobal.CreateStringByColor(DAMGEPLAYER1 + "", ColorType.Importal) + " sát thương và dành chiến thắng");

                                KTPlayerManager.ShowMessageBox(ARENA.Player_2.player, "Thông báo kết quả", "Trong trận tỉ thí với " + ARENA.Player_1.player.RoleName + " bạn đã thua vì chỉ gây ra [" + KTGlobal.CreateStringByColor(DAMGEPLAYER2 + "", ColorType.Importal) + "] tổng sát thương nhỏ hơn [" + KTGlobal.CreateStringByColor(DAMGEPLAYER1 + "", ColorType.Importal) + "] của đối thủ");

                                LogManager.WriteLog(LogTypes.FactionBattle, "SỐ DAMGE GÂY RA CỦA  :[" + ARENA.ARENAID + "][" + ARENA.Player_1.player.RoleName + "] là :" + DAMGEPLAYER1 + "| Nhiều hơn :" + ARENA.Player_2.player.RoleName + " là  :" + DAMGEPLAYER2 + "===> " + ARENA.Player_1.player.RoleName + " CHIẾN THẮNG");

                                ARENA.Player_2.BestElimateRank = ROUNID;
                                ARENA.WinThisRound = ARENA.Player_1;
                                ARENA.Player_1.IsWinRound = true;
                            }
                            else if (DAMGEPLAYER2 > DAMGEPLAYER1)
                            {
                                KTPlayerManager.ShowMessageBox(ARENA.Player_2.player, "Thông báo kết quả", "Trong trận tỉ thí với " + ARENA.Player_1.player.RoleName + " bạn đã gây ra tổng " + KTGlobal.CreateStringByColor(DAMGEPLAYER2 + "", ColorType.Importal) + " sát thương và dành chiến thắng");

                                KTPlayerManager.ShowMessageBox(ARENA.Player_1.player, "Thông báo kết quả", "Trong trận tỉ thí với " + ARENA.Player_2.player.RoleName + "  bạn đã thua vì chỉ gây ra [" + KTGlobal.CreateStringByColor(DAMGEPLAYER1 + "", ColorType.Importal) + "] tổng sát thương nhỏ hơn [" + KTGlobal.CreateStringByColor(DAMGEPLAYER2 + "", ColorType.Importal) + "] của đối thủ");

                                LogManager.WriteLog(LogTypes.FactionBattle, "SỐ DAMGE GÂY RA CỦA  :[" + ARENA.ARENAID + "][" + ARENA.Player_1.player.RoleName + "] là :" + DAMGEPLAYER1 + "| Ít hơn :" + ARENA.Player_2.player.RoleName + " là  :" + DAMGEPLAYER2 + "===> " + ARENA.Player_2.player.RoleName + " CHIẾN THẮNG");

                                ARENA.Player_1.BestElimateRank = ROUNID;

                                ARENA.WinThisRound = ARENA.Player_2;
                                ARENA.Player_2.IsWinRound = true;
                            }
                            else
                            {
                                // Nếu thằng  người chơi 1 bị diss ra thì thằng 2 thắng
                                if ((ARENA.Player_1.player.MapCode != this.GetMapCode || !ARENA.Player_1.player.IsOnline()) && (ARENA.Player_2.player.MapCode == this.GetMapCode && ARENA.Player_2.player.IsOnline()))
                                {
                                    LogManager.WriteLog(LogTypes.FactionBattle, "Thằng thứ 1  :[" + ARENA.Player_2.player.RoleName + "] BỊ diss ra  =====> PLAYER 2 THẮNG");

                                    ARENA.Player_1.BestElimateRank = ROUNID;

                                    ARENA.WinThisRound = ARENA.Player_2;
                                    ARENA.Player_2.IsWinRound = true;
                                }
                                else if ((ARENA.Player_1.player.MapCode == this.GetMapCode && ARENA.Player_1.player.IsOnline()) && (ARENA.Player_2.player.MapCode != this.GetMapCode || !ARENA.Player_2.player.IsOnline()))
                                {
                                    ARENA.Player_2.BestElimateRank = ROUNID;

                                    LogManager.WriteLog(LogTypes.FactionBattle, "Thằng thứ 2 :[" + ARENA.ARENAID + "][" + ARENA.Player_1.player.RoleName + "]Bị diss ra =====> PLAYER 1 THẮNG");
                                    ARENA.WinThisRound = ARENA.Player_1;
                                    ARENA.Player_1.IsWinRound = true;
                                }
                                else if (ARENA.Player_1.nScore > ARENA.Player_2.nScore)
                                {
                                    ARENA.Player_2.BestElimateRank = ROUNID;

                                    LogManager.WriteLog(LogTypes.FactionBattle, "ĐIỂM CỦA  :[" + ARENA.ARENAID + "][" + ARENA.Player_1.player.RoleName + "]NHIỀU HƠN =====> PLAYER 1 THẮNG");
                                    ARENA.WinThisRound = ARENA.Player_1;
                                    ARENA.Player_1.IsWinRound = true;
                                }
                                else
                                {
                                    ARENA.Player_1.BestElimateRank = ROUNID;
                                    LogManager.WriteLog(LogTypes.FactionBattle, "ĐIỂM CỦA  :[" + ARENA.Player_2.player.RoleName + "]NHIỀU HƠN =====> PLAYER 2 THẮNG");
                                    ARENA.WinThisRound = ARENA.Player_2;
                                    ARENA.Player_2.IsWinRound = true;
                                }
                            }
                        }

                        // MOVE TOÀN BỘ RA KHỎI TRẬN CHIẾN
                        ELIMINATION_TOTAL_ROUND = ELIMINATION_TOTAL.Where(x => x.ROUNDID == ROUNID).ToList();

                        foreach (ELIMINATION_INFO ARENA in ELIMINATION_TOTAL_ROUND)
                        {
                            // SET Cho người chơi 1
                            ARENA.Player_1.player.PKMode = (int)PKMode.Peace;
                            ARENA.Player_1.player.Camp = -1;
                            ARENA.Player_1.player.ForbidUsingSkill = true;
                            //Send thông báo cho người chơi biết là trận đấu đã bắt đầu
                            KTPlayerManager.ShowNotification(ARENA.Player_1.player, "Trận chiến đã kết thúc");


                            this.ChangePos(ARENA.Player_1.player, _Config.GoIn.PosX, _Config.GoIn.PosY);

                            //// Set camp cho người chơi 2
                            ARENA.Player_2.player.PKMode = (int)PKMode.Peace;
                            ARENA.Player_2.player.Camp = -1;
                            ARENA.Player_2.player.ForbidUsingSkill = true;
                            //Send thông báo cho người chơi biết là trận đấu đã bắt đầu
                            KTPlayerManager.ShowNotification(ARENA.Player_2.player, "Trận chiến đã kết thúc");


                            this.ChangePos(ARENA.Player_2.player, _Config.GoIn.PosX, _Config.GoIn.PosY);

                            ARENA._ROUNDSTATE = ROUNDSTATE.END;
                        }

                        if (ROUNID < 4)
                        {
                            // TẠO SCOREBARD
                            this.CreateTopScoreBard(ROUNID);
                        }
                        else
                        {
                            ELIMINATION_INFO _FINDQUANQUAN = ELIMINATION_TOTAL.Where(x => x.ROUNDID == 4).FirstOrDefault();
                            if (_FINDQUANQUAN != null)
                            {
                                // kết thúc trận cuối cùng |  Move 2 thằng cuối ra ngoài
                                this.ChangePos(_FINDQUANQUAN.Player_1.player, _Config.GoIn.PosX, _Config.GoIn.PosY, 100, 100, 100);
                                this.ChangePos(_FINDQUANQUAN.Player_2.player, _Config.GoIn.PosX, _Config.GoIn.PosY, 100, 100, 100);

                                if (_FINDQUANQUAN.WinThisRound != null)
                                {
                                    string NOTIFY = KTGlobal.CreateStringByColor(_FINDQUANQUAN.WinThisRound.player.RoleName, ColorType.Yellow) + " đã trở thành " + KTGlobal.CreateStringByColor("Tân Nhân Vương", ColorType.Importal) + " môn phái [" + this.GetFactionName + "]";

                                    KTGlobal.SendSystemEventNotification(NOTIFY);
                                    // KTGlobal.SendSystemEventNotification(NOTIFY);
                                }
                            }
                        }

                        //GỌI HÀM RESET SỐ CỜ ĐÃ TÌM ĐƯỢC
                        ResetFlag();
                    }
                }
                // Thời gian để thằng người quán quân nhận thưởng
                else if (BATTLESTATE == FactionState.CHAMPION_AWARD)
                {
                    //Cứ mỗi 5s tick cho toàn bộ người chơi
                    if (TimeUtil.NOW() - LastNofity >= 5 * 1000 && (BATTLESTATE == FactionState.CHAMPION_AWARD))
                    {
                        LastNofity = TimeUtil.NOW();
                        // Update tìm cờ
                        UpdateBattleBattleAward();
                    }

                    // Kết thúc thời gian nhận thưởng đẩy tất cả trở về chỗ vào
                    if (TimeUtil.NOW() >= LastTick + (_Config.TimeAct.REVICE_AWARD * 1000) && BATTLESTATE == FactionState.CHAMPION_AWARD)
                    {
                        LastTick = TimeUtil.NOW();
                        // SWTICH VỀ END
                        BATTLESTATE = FactionState.END;
                    }
                }
                // ĐẨY TOÀN BỘ NGƯỜI REGISTER RA KHỎI PVP
                else if (BATTLESTATE == FactionState.END)
                {
                    this.MoveAllPlayerQuitMap();

                    //Đợi thêm 1 giờ cho người chơi tới NPC nhận thưởng sau đó dọn lại chiến trường
                    if (TimeUtil.NOW() - LastNofity >= 60 * 60 * 1000 && (BATTLESTATE == FactionState.END))
                    {
                        // CHUYỂN VỀ NOTHING
                        BATTLESTATE = FactionState.NOTHING;
                        // Sau 1 giờ thi đấu môn phái tự xóa toàn bộ dữ liệu
                        ResetBattle();
                    }
                }

                // ADD EXP CHO TOÀN BỘ NGƯỜI CHƠI KHI THAM GIA THI ĐẤU
                // CỨ 30s giật 1 phát EXP cho toàn bộ người chơi trong đấu trường môn phái
                if (TimeUtil.NOW() - LastExpTick >= 30 * 1000 && (BATTLESTATE > FactionState.MELEE_ROUND_2 && BATTLESTATE < FactionState.CHAMPION_AWARD))
                {
                    LastExpTick = TimeUtil.NOW();
                    //Thực hiện add EXP cho người chơi
                    ExpProsecc();
                }

                // END ADD
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.FactionBattle, "BUG:" + ex.ToString());
            }
        }

        public void ResetFlag()
        {
            foreach (FactionPlayer Player in PVPARENA.Values)
            {
                Player.DamgeRecore = 0;
                Player.TotalFlagCollect = 0;
            }
        }

        private void CreateTopScoreBard(int ROUNID)
        {
            LogManager.WriteLog(LogTypes.FactionBattle, "[" + ROUNID + "] SẮP XẾP VÒNG ĐẤU!");

            // Lấy ra toàn bộ các ARENA của vòng đấu trước
            List<ELIMINATION_INFO> ELIMINATION_TOTAL_ROUND = ELIMINATION_TOTAL.Where(x => x.ROUNDID == ROUNID).ToList();

            List<FactionPlayer> TotalPlayWin = new List<FactionPlayer>();

            foreach (ELIMINATION_INFO ARENA in ELIMINATION_TOTAL_ROUND)
            {
                LogManager.WriteLog(LogTypes.FactionBattle, "[" + ROUNID + "] [" + ARENA.ARENAID + "] BẮT ĐẦU LẤY RA NGƯỜI THẮNG CỦA ARENA");

                if (ARENA.WinThisRound != null)
                {
                    LogManager.WriteLog(LogTypes.FactionBattle, "[" + ROUNID + "] [" + ARENA.ARENAID + "] BẮT ĐẦU LẤY RA NGƯỜI THẮNG CỦA ARENA===>" + ARENA.WinThisRound.player.RoleName);
                    TotalPlayWin.Add(ARENA.WinThisRound);
                }
            }

            LogManager.WriteLog(LogTypes.FactionBattle, "[" + ROUNID + "] TỔNG SỐ NGƯỜI THẮNG TRONG ROUND NÀY LÀ ===>" + TotalPlayWin.Count);

            // set vị trí lôi đài
            int LOIDAI = 1;

            //if (ROUNID == 3)
            //{
            //    LOIDAI = 0;
            //}
            /// Duyệt tất cả người chơi trong trận
            for (int i = 0; i < TotalPlayWin.Count; i++)
            {
                ELIMINATION_INFO _Info = new ELIMINATION_INFO();

                _Info.IsCreateBox = false;
                _Info.ARENAID = LOIDAI;
                // Đây là trận đầu tiên
                _Info.ROUNDID = ROUNID + 1;

                _Info.Player_1 = TotalPlayWin[i];
                _Info.Player_2 = TotalPlayWin[i + 1];

                _Info.Player_1.IsWinRound = false;
                _Info.Player_2.IsWinRound = false;

                _Info._ROUNDSTATE = ROUNDSTATE.NONE;

                ELIMINATION_TOTAL.Add(_Info);

                // Dịch 2 người 1
                i = i + 1;

                LogManager.WriteLog(LogTypes.FactionBattle, "[" + ROUNID + 1 + "]|[" + LOIDAI + "] ĐÃ SẮP XẾP XONG ĐỘI HÌNH CHO LÔI ĐÀI");

                LOIDAI++;
            }
        }

        /// <summary>
        /// Lấy ra cái lôi đài đang theo dõi
        /// </summary>
        /// <param name="InPut"></param>
        /// <returns></returns>
        public int GetWatchArenaID(FactionPlayer InPut)
        {
            Point CurentPostion = InPut.player.CurrentPos;

            int ArenaIDWatch = -1;

            float Min = 10000f;

            foreach (ArenaPos _Arena in _Config.TotalArena)
            {
                Point _ArenaPos = new Point(_Arena.PosX, _Arena.PosY);

                float Dis = KTGlobal.GetDistanceBetweenPoints(_ArenaPos, CurentPostion);

                if (Dis < Min)
                {
                    Min = Dis;
                    ArenaIDWatch = _Arena.ArenaID;
                }
            }

            return ArenaIDWatch;
        }

        public void ClickFlag(KPlayer client, int ResID)
        {
            // Nếu đong trong thời gian nghỉ thì tức là đang click vào cờ
            if (ResID == 2701)
            {
                if (client.TeamID != -1)
                {
                    List<KPlayer> TotalMember = client.Teammates;

                    // cộng tích lũy cho toàn bộ thành viên trong tổ đội
                    foreach (KPlayer member in TotalMember)
                    {
                        // nếu khác bản đồ thì thôi
                        if (member.MapCode != client.MapCode)
                        {
                            continue;
                        }
                        // nếu không trong bán kính thì thôi
                        if (KTGlobal.GetDistanceBetweenPoints(new Point(member.PosX, member.PosY), client.CurrentPos) > 2000)
                        {
                            continue;
                        }
                        // Nếu người chơi đã chết thì thôi
                        if (member.IsDead())
                        {
                            continue;
                        }

                        // tìm ra thằng người chơi này
                        var find = PVPARENA.Values.Where(x => x.player.RoleID == member.RoleID).FirstOrDefault();

                        // Tăng số điểm cờ thu thập được
                        if (find != null)
                        {
                            if (find.TotalFlagCollect < 5)
                            {
                                find.nScore = find.nScore + 20;
                                find.TotalFlagCollect = find.TotalFlagCollect + 1;

                                long SEC = (LastTick + (_Config.TimeAct.FREE_ELIMINATION * 1000)) - TimeUtil.NOW();

                                int FinalSec = (int)(SEC / 1000);

                                SendBattleNotifyFlag(find, FinalSec);
                            }
                        }
                    }
                }
                else // Nếu là giết quái solo một mình
                {
                    var find = PVPARENA.Values.Where(x => x.player.RoleID == client.RoleID).FirstOrDefault();

                    // Tăng số điểm cờ thu thập được
                    if (find != null)
                    {
                        if (find.TotalFlagCollect < 5)
                        {
                            find.nScore = find.nScore + 100;
                            find.TotalFlagCollect = find.TotalFlagCollect + 1;

                            long SEC = (LastTick + (_Config.TimeAct.FREE_ELIMINATION * 1000)) - TimeUtil.NOW();

                            int FinalSec = (int)(SEC / 1000);

                            SendBattleNotifyFlag(find, FinalSec);
                        }
                    }
                }
            } // nếu đang trong lúc quán quân nhận thưởng hoặc các vòng đấu lẻ
            else if (ResID == 2700)
            {
                // Add cái rương này vào
                if (!ItemManager.CreateItem(Global._TCPManager.TcpOutPacketPool, client, 335, 1, 0, "TDMP", false, 1, false, ItemManager.ConstGoodsEndTime))
                {
                    KTPlayerManager.ShowNotification(client, "Có lỗi khi nhận vật phẩm chế tạo");
                }
            }
        }

        public void CreateBox(GameMap gameMap, int XPos, int YPos)
        {
            LogManager.WriteLog(LogTypes.FactionBattle, "[" + _ROUNDSTATE.ToString() + "]TẠO RƯƠNG MÔN PHÁI TẠI TẠO ĐỘ XPos:" + XPos + "| YPos:" + YPos);
            GrowPointXML _Config = new GrowPointXML();
            _Config.CollectTick = 10000;
            _Config.Name = "Rương môn phái";
            _Config.ResID = 2700;
            _Config.RespawnTime = -100;
            _Config.InteruptIfTakeDamage = true;
            _Config.ScriptID = -1;

            // Tạo rương
            GrowPoint growPoint = new GrowPoint()
            {
                ID = KTGrowPointManager.AutoIndexManager.Take() + (int)ObjectBaseID.GrowPoint,
                Data = _Config,
                Name = _Config.Name,
                ObjectType = ObjectTypes.OT_GROWPOINT,
                MapCode = gameMap.MapCode,
                CurrentCopyMapID = -1,
                CurrentPos = new System.Windows.Point(XPos, YPos),
                CurrentGrid = new System.Windows.Point(XPos / gameMap.MapGridWidth, YPos / gameMap.MapGridHeight),
                RespawnTime = _Config.RespawnTime,
                ScriptID = _Config.ScriptID,
                LifeTime = 60000,
                Alive = true,
                GrowPointCollectCompleted = null,
            };
            /// Thực hiện tự động xóa
            growPoint.ProcessAutoRemoveTimeout();
            KTGrowPointManager.GrowPoints[growPoint.ID] = growPoint;
            /// Thêm điểm thu thập vào đối tượng quản lý map
            KTGrowPointManager.AddToMap(growPoint);
        }

        public void CreateFLAG(GameMap gameMap, int XPos, int YPos)
        {
            GrowPointXML _Config = new GrowPointXML();
            _Config.CollectTick = 10000;
            _Config.Name = "Cờ Tầm Bảo";
            _Config.ResID = 2701;
            _Config.RespawnTime = -100;
            _Config.InteruptIfTakeDamage = true;
            _Config.ScriptID = -1;

            // Tạo 1 cờ
            GrowPoint growPoint = new GrowPoint()
            {
                ID = KTGrowPointManager.AutoIndexManager.Take() + (int)ObjectBaseID.GrowPoint,
                Data = _Config,
                Name = _Config.Name,
                ObjectType = ObjectTypes.OT_GROWPOINT,
                MapCode = gameMap.MapCode,
                CurrentCopyMapID = -1,
                CurrentPos = new System.Windows.Point(XPos, YPos),
                CurrentGrid = new System.Windows.Point(XPos / gameMap.MapGridWidth, YPos / gameMap.MapGridHeight),
                RespawnTime = _Config.RespawnTime,
                ScriptID = _Config.ScriptID,
                LifeTime = 60000,
                Alive = true,
                ConditionCheck = (player) =>
                {
                    PVPARENA.TryGetValue(player.RoleID, out FactionPlayer _PLAYCOLLECT);

                    if (_PLAYCOLLECT != null)
                    {
                        if (_PLAYCOLLECT.TotalFlagCollect < 5)
                        {
                            return true;
                        }
                        else
                        {
                            KTPlayerManager.ShowNotification(player, "Bạn đã thu thập đủ cờ không thể thu thập thêm");
                            return false;
                        }
                    }

                    return false;
                },
                GrowPointCollectCompleted = null,
            };
            /// Thực hiện tự động xóa
            growPoint.ProcessAutoRemoveTimeout();
            KTGrowPointManager.GrowPoints[growPoint.ID] = growPoint;
            /// Thêm điểm thu thập vào đối tượng quản lý map
            KTGrowPointManager.AddToMap(growPoint);
        }

        /// <summary>
        /// Thực hiện update EXP
        /// </summary>
        public void ExpProsecc()
        {
            foreach (KeyValuePair<int, FactionPlayer> entry in PVPARENA)
            {
                FactionPlayer PlayerBattle = entry.Value;

                // Check xem nếu ở trong bản đồ mới nhận được exp
                if (PlayerBattle.player.MapCode == this.GetMapCode)
                {
                    KTPlayerManager.AddExp(PlayerBattle.player, 16000);
                }
            }
        }

        /// <summary>
        /// Lấy ra top 16 thằng khỏe nhất trong vòng loại PVP
        /// </summary>
        /// <param name="TOP16Player"></param>
        public void CreateSoloBoard(List<FactionPlayer> BattleFactionRank)
        {
            // HÀM NÀY VIẾT LẠI ĐỂ SẮP XẾP TRẠNG THÁI THI ĐẤU

            // Nếu số người lớn hơn 16 thì lấy ra 16 thằng có xếp hạng cao nhất
            if (BattleFactionRank.Count >= 16)
            {
                List<FactionPlayer> TOP16Player = BattleFactionRank.GetRange(0, 16);

                // Sắp xếp vào lôi đài 1
                ELIMINATION_INFO _Info = new ELIMINATION_INFO();
                _Info.ARENAID = 1;
                // Đây là trận đầu tiên
                _Info.IsCreateBox = false;
                _Info.ROUNDID = 1;
                _Info.Player_1 = TOP16Player[15];
                _Info.Player_2 = TOP16Player[0];
                _Info._ROUNDSTATE = ROUNDSTATE.NONE;

                ELIMINATION_TOTAL.Add(_Info);

                // Sắp xếp vào lôi đài 2
                _Info = new ELIMINATION_INFO();
                _Info.ARENAID = 2;
                _Info.IsCreateBox = false;
                // Đây là trận đầu tiên
                _Info.ROUNDID = 1;
                _Info.Player_1 = TOP16Player[7];
                _Info.Player_2 = TOP16Player[8];
                _Info._ROUNDSTATE = ROUNDSTATE.NONE;

                ELIMINATION_TOTAL.Add(_Info);

                // Sắp xếp vào lôi đài 3
                _Info = new ELIMINATION_INFO();
                _Info.ARENAID = 3;
                _Info.IsCreateBox = false;
                // Đây là trận đầu tiên
                _Info.ROUNDID = 1;
                _Info.Player_1 = TOP16Player[3];
                _Info.Player_2 = TOP16Player[12];
                _Info._ROUNDSTATE = ROUNDSTATE.NONE;

                ELIMINATION_TOTAL.Add(_Info);

                // Sắp xếp vào lôi đài 4
                _Info = new ELIMINATION_INFO();
                _Info.ARENAID = 4;
                _Info.IsCreateBox = false;
                // Đây là trận đầu tiên
                _Info.ROUNDID = 1;
                _Info.Player_1 = TOP16Player[5];
                _Info.Player_2 = TOP16Player[10];
                _Info._ROUNDSTATE = ROUNDSTATE.NONE;

                ELIMINATION_TOTAL.Add(_Info);

                // Sắp xếp vào lôi đài 5
                _Info = new ELIMINATION_INFO();
                _Info.ARENAID = 5;
                _Info.IsCreateBox = false;
                // Đây là trận đầu tiên
                _Info.ROUNDID = 1;
                _Info.Player_1 = TOP16Player[1];
                _Info.Player_2 = TOP16Player[14];
                _Info._ROUNDSTATE = ROUNDSTATE.NONE;

                ELIMINATION_TOTAL.Add(_Info);

                // Sắp xếp vào lôi đài 6
                _Info = new ELIMINATION_INFO();
                _Info.ARENAID = 6;
                _Info.IsCreateBox = false;
                // Đây là trận đầu tiên

                _Info.ROUNDID = 1;
                _Info.Player_1 = TOP16Player[6];
                _Info.Player_2 = TOP16Player[9];
                _Info._ROUNDSTATE = ROUNDSTATE.NONE;

                ELIMINATION_TOTAL.Add(_Info);

                // Sắp xếp vào lôi đài 7
                _Info = new ELIMINATION_INFO();
                _Info.ARENAID = 7;
                _Info.IsCreateBox = false;
                // Đây là trận đầu tiên
                _Info.ROUNDID = 1;
                _Info.Player_1 = TOP16Player[2];
                _Info.Player_2 = TOP16Player[13];
                _Info._ROUNDSTATE = ROUNDSTATE.NONE;

                ELIMINATION_TOTAL.Add(_Info);

                // Sắp xếp vào lôi đài 8
                _Info = new ELIMINATION_INFO();
                _Info.ARENAID = 8;
                _Info.IsCreateBox = false;
                // Đây là trận đầu tiên
                _Info.ROUNDID = 1;
                _Info.Player_1 = TOP16Player[4];
                _Info.Player_2 = TOP16Player[11];
                _Info._ROUNDSTATE = ROUNDSTATE.NONE;

                ELIMINATION_TOTAL.Add(_Info);

                //TODO SEND CÁI LIST NÀY VỀ CLIENT
            }
            else if (BattleFactionRank.Count < 16 && BattleFactionRank.Count >= 8) // Nếu số người lớn hơn 8 và nhỏ hơn 16
            {
                List<FactionPlayer> TOP8Player = BattleFactionRank.GetRange(0, 8);

                CreateTopScoreBard(1, TOP8Player);

                BATTLESTATE = FactionState.FREE_ELIMINATION_ROUND_1;
            }
            else if (BattleFactionRank.Count < 8 && BattleFactionRank.Count >= 4) // Nếu số người lớn hơn 8 và nhỏ hơn 16
            {
                List<FactionPlayer> TOP4Player = BattleFactionRank.GetRange(0, 4);

                CreateTopScoreBard(2, TOP4Player);

                BATTLESTATE = FactionState.FREE_ELIMINATION_ROUND_2;
            }
            else if (BattleFactionRank.Count < 4 && BattleFactionRank.Count >= 2) // Nếu số người lớn hơn 8 và nhỏ hơn 16
            {
                List<FactionPlayer> TOP4Player = BattleFactionRank.GetRange(0, 2);

                CreateTopScoreBard(3, TOP4Player);

                BATTLESTATE = FactionState.FREE_ELIMINATION_ROUND_3;
            }
        }

        private void CreateTopScoreBard(int ROUNID, List<FactionPlayer> TotalPlayWin)
        {
            int LOIDAI = 1;

            for (int i = 0; i < TotalPlayWin.Count; i++)
            {
                ELIMINATION_INFO _Info = new ELIMINATION_INFO();

                _Info.IsCreateBox = false;
                _Info.ARENAID = LOIDAI;
                // Đây là trận đầu tiên
                _Info.ROUNDID = ROUNID + 1;

                _Info.Player_1 = TotalPlayWin[i];
                _Info.Player_2 = TotalPlayWin[i + 1];

                _Info.Player_1.IsWinRound = false;
                _Info.Player_2.IsWinRound = false;

                _Info._ROUNDSTATE = ROUNDSTATE.NONE;

                ELIMINATION_TOTAL.Add(_Info);

                // Dịch 2 người 1
                i = i + 1;

                LogManager.WriteLog(LogTypes.FactionBattle, "[" + ROUNID + 1 + "]|[" + LOIDAI + "] ĐÃ SẮP XẾP XONG ĐỘI HÌNH CHO LÔI ĐÀI");

                LOIDAI++;
            }
        }

        public void UpdateBattleNotify()
        {
            foreach (KeyValuePair<int, FactionPlayer> entry in PVPARENA)
            {
                FactionPlayer PlayerBattle = entry.Value;

                if (PlayerBattle.player.CurrentMapCode == this.GetMapCode)
                {
                    long SEC = (LastTick + (_Config.TimeAct.MELEE_PVP_DULATION * 1000)) - TimeUtil.NOW();

                    int FinalSec = (int)(SEC / 1000);

                    SendBattleNotify(PlayerBattle, FinalSec);
                }
            }
        }

        /// <summary>
        /// Thông báo cờ
        /// </summary>
        ///
        public void SendPvpToOtherPlayer(FactionPlayer InputPlayer, int Sec, int STATUS, int ROUNID)
        {
            PlayChangeState(InputPlayer.player, 1);

            InputPlayer.player.StopAllActiveFights();

            InputPlayer.player.PKMode = (int)PKMode.Peace;
            //// Set camp cho người chơi
            InputPlayer.player.Camp = -1;

            // mở khóa sử dụng skill
            InputPlayer.player.ForbidUsingSkill = true;

            G2C_EventNotification _Notify = new G2C_EventNotification();

            _Notify.TotalInfo = new List<string>();

            if (Sec > 0)
            {
                if (STATUS == 1)
                {
                    _Notify.EventName = "Thời gian chuẩn bị thi đấu";
                    _Notify.ShortDetail = "TIME|" + Sec;

                    int WatchId = GetWatchArenaID(InputPlayer);
                    if (WatchId != -1)
                    {
                        ELIMINATION_INFO ELIMINATION_TOTAL_ROUND = ELIMINATION_TOTAL.Where(x => x.ROUNDID == ROUNID && x.ARENAID == WatchId).FirstOrDefault();

                        _Notify.TotalInfo.Add("Đang xem lôi đài :<color=red>" + WatchId + "</color>");

                        if (ELIMINATION_TOTAL_ROUND != null)
                        {
                            if (ELIMINATION_TOTAL_ROUND.Player_1.DamgeRecore > ELIMINATION_TOTAL_ROUND.Player_2.DamgeRecore)
                            {
                                _Notify.TotalInfo.Add("<color=green>" + ELIMINATION_TOTAL_ROUND.Player_1.player.RoleName + " | Sát thương :" + ELIMINATION_TOTAL_ROUND.Player_1.DamgeRecore + "</color>");
                                _Notify.TotalInfo.Add("<color=red>" + ELIMINATION_TOTAL_ROUND.Player_2.player.RoleName + " | Sát thương :" + ELIMINATION_TOTAL_ROUND.Player_2.DamgeRecore + "</color>");
                            }
                            else
                            {
                                _Notify.TotalInfo.Add("<color=red>" + ELIMINATION_TOTAL_ROUND.Player_1.player.RoleName + " | Sát thương :" + ELIMINATION_TOTAL_ROUND.Player_1.DamgeRecore + "</color>");
                                _Notify.TotalInfo.Add("<color=green>" + ELIMINATION_TOTAL_ROUND.Player_2.player.RoleName + " | Sát thương :" + ELIMINATION_TOTAL_ROUND.Player_2.DamgeRecore + "</color>");
                            }
                        }
                    }
                }
                else if (STATUS == 2)
                {
                    _Notify.EventName = "Thời gian đấu loại trực tiếp";
                    _Notify.ShortDetail = "TIME|" + Sec;

                    int WatchId = GetWatchArenaID(InputPlayer);

                    if (WatchId != -1)
                    {
                        ELIMINATION_INFO ELIMINATION_TOTAL_ROUND = ELIMINATION_TOTAL.Where(x => x.ROUNDID == ROUNID && x.ARENAID == WatchId).FirstOrDefault();

                        _Notify.TotalInfo.Add("Đang xem lôi đài :<color=red>" + WatchId + "</color>");

                        if (ELIMINATION_TOTAL_ROUND != null)
                        {
                            if (ELIMINATION_TOTAL_ROUND.Player_1.DamgeRecore > ELIMINATION_TOTAL_ROUND.Player_2.DamgeRecore)
                            {
                                _Notify.TotalInfo.Add("<color=green>" + ELIMINATION_TOTAL_ROUND.Player_1.player.RoleName + " | Sát thương :" + ELIMINATION_TOTAL_ROUND.Player_1.DamgeRecore + "</color>");
                                _Notify.TotalInfo.Add("<color=red>" + ELIMINATION_TOTAL_ROUND.Player_2.player.RoleName + " | Sát thương :" + ELIMINATION_TOTAL_ROUND.Player_2.DamgeRecore + "</color>");
                            }
                            else
                            {
                                _Notify.TotalInfo.Add("<color=red>" + ELIMINATION_TOTAL_ROUND.Player_1.player.RoleName + " | Sát thương :" + ELIMINATION_TOTAL_ROUND.Player_1.DamgeRecore + "</color>");
                                _Notify.TotalInfo.Add("<color=green>" + ELIMINATION_TOTAL_ROUND.Player_2.player.RoleName + " | Sát thương :" + ELIMINATION_TOTAL_ROUND.Player_2.DamgeRecore + "</color>");
                            }
                        }
                    }
                }
            }
            else
            {
                _Notify.EventName = "Thời gian tỉ thí đã kết thúc";
                _Notify.ShortDetail = "TIME|" + 0;
            }

            if (InputPlayer.player.IsOnline())
            {
                InputPlayer.player.SendPacket<G2C_EventNotification>((int)TCPGameServerCmds.CMD_KT_EVENT_NOTIFICATION, _Notify);
            }
        }

        /// <summary>
        /// Anti move lag
        /// </summary>
        /// <param name="CenterPoint"></param>
        /// <param name="Character"></param>
        public void AntiLagMove(Point CenterPoint, FactionPlayer Character)
        {
            // Nếu mà nhân vật bị rollback khổng sản
            //if (!KTGlobal.HasPath(Character.player.MapCode, CenterPoint, Character.player.CurrentPos))
            //{
            //    LogManager.WriteLog(LogTypes.FactionBattle, "[" + Character.player.RoleName + "] Toạch move vào chuồng cọp ==> Thực hiện move lại!");
            //    this.ChangePos(Character.player, (int)CenterPoint.X, (int)CenterPoint.Y);
            //}

            // Nếu người chơi đang ở khu antoanf thì lại giật lên
            if(Character.player.IsInsideSafeZone)
            {
                LogManager.WriteLog(LogTypes.FactionBattle, "[" + Character.player.RoleName + "] Toạch move vào chuồng cọp ==> Thực hiện move lại!");
                this.ChangePos(Character.player, (int)CenterPoint.X, (int)CenterPoint.Y);
            }
        }

        public void UpdatePVP1VS1TOOtherPlayer(int ROUNID)
        {
            List<int> FULLPLAYER = new List<int>();

            // LẤY RA DANH SÁCH CÁC LÔI ĐÀI
            List<ELIMINATION_INFO> ELIMINATION_TOTAL_ROUND = ELIMINATION_TOTAL.Where(x => x.ROUNDID == ROUNID).ToList();

            foreach (ELIMINATION_INFO ARENAINFO in ELIMINATION_TOTAL_ROUND)
            {
                FULLPLAYER.Add(ARENAINFO.Player_1.player.RoleID);
                FULLPLAYER.Add(ARENAINFO.Player_2.player.RoleID);

                if (_ROUNDSTATE == ROUNDSTATE.PREADING)
                {
                    long SEC = (LastTick + (60 * 1000)) - TimeUtil.NOW();

                    int FinalSec = (int)(SEC / 1000);

                    SendBattleNotify1VS1(ARENAINFO.Player_1, _ROUNDSTATE, FinalSec, ARENAINFO);
                    SendBattleNotify1VS1(ARENAINFO.Player_2, _ROUNDSTATE, FinalSec, ARENAINFO);
                }

                if (_ROUNDSTATE == ROUNDSTATE.START)
                {
                    ArenaPos _ARENAPOSTION = _Config.TotalArena.Where(x => x.ArenaID == ARENAINFO.ARENAID).FirstOrDefault();

                    if (ARENAINFO._ROUNDSTATE == ROUNDSTATE.PREADING || ARENAINFO.WinThisRound == null)
                    {
                        this.AntiLagMove(_ARENAPOSTION.ConvertToPoint, ARENAINFO.Player_1);
                        this.AntiLagMove(_ARENAPOSTION.ConvertToPoint, ARENAINFO.Player_2);
                    }
                    // Nếu battle đang bắt đầu
                    else if (ARENAINFO._ROUNDSTATE == ROUNDSTATE.START)
                    {
                        // nếu thằng 1 không recononect
                        if (!ARENAINFO.Player_1.IsReconnect || ARENAINFO.WinThisRound == null)
                        {
                            // Thực hiện spam camp set lại PK mode liên tục
                            ARENAINFO.Player_1.player.PKMode = (int)PKMode.Custom;
                            ARENAINFO.Player_1.player.Camp = ARENAINFO.Player_1.player.RoleID;
                            ARENAINFO.Player_1.player.ForbidUsingSkill = false;

                            this.AntiLagMove(_ARENAPOSTION.ConvertToPoint, ARENAINFO.Player_1);
                        }
                        // nếu thằng 2 không recononect
                        if (!ARENAINFO.Player_2.IsReconnect || ARENAINFO.WinThisRound == null)
                        {
                            ARENAINFO.Player_2.player.PKMode = (int)PKMode.Custom;
                            ARENAINFO.Player_2.player.Camp = ARENAINFO.Player_2.player.RoleID;
                            ARENAINFO.Player_2.player.ForbidUsingSkill = false;

                            this.AntiLagMove(_ARENAPOSTION.ConvertToPoint, ARENAINFO.Player_2);
                        }

                       
                    }
                    else if (ARENAINFO._ROUNDSTATE == ROUNDSTATE.END)
                    {
                        ARENAINFO.Player_1.player.StopAllActiveFights();
                        // Chuyển về hòa bình nếu đã đánh xong trận
                        ARENAINFO.Player_1.player.PKMode = (int)PKMode.Peace;
                        //// Set camp cho người chơi
                        ARENAINFO.Player_1.player.Camp = -1;
                        // mở khóa sử dụng skill
                        ARENAINFO.Player_1.player.ForbidUsingSkill = true;

                        ARENAINFO.Player_2.player.StopAllActiveFights();
                        // Chuyển về hòa bình nếu đã đánh xong trận
                        ARENAINFO.Player_2.player.PKMode = (int)PKMode.Peace;
                        //// Set camp cho người chơi
                        ARENAINFO.Player_2.player.Camp = -1;
                        // mở khóa sử dụng skill
                        ARENAINFO.Player_2.player.ForbidUsingSkill = true;
                    }

                    long SEC = (LastTick + (_Config.TimeAct.ELIMINATION_ROUND_ROUND * 1000)) - TimeUtil.NOW();
                    int FinalSec = (int)(SEC / 1000);
                    SendBattleNotify1VS1(ARENAINFO.Player_1, _ROUNDSTATE, FinalSec, ARENAINFO);
                    SendBattleNotify1VS1(ARENAINFO.Player_2, _ROUNDSTATE, FinalSec, ARENAINFO);
                }
            }

            ///Duyệt tất cả bọn nào đang ở trong khu vực đấu trường
            foreach (KeyValuePair<int, FactionPlayer> entry in PVPARENA)
            {
                FactionPlayer PlayerBattle = entry.Value;

                // Nếu thằng này đang thi đấu thì thôi
                if (FULLPLAYER.Contains(PlayerBattle.player.RoleID))
                {
                    continue;
                }

                // Max 100 phép toán trong này
                if (PlayerBattle.player.CurrentMapCode == this.GetMapCode)
                {
                    if (_ROUNDSTATE == ROUNDSTATE.PREADING)
                    {
                        long SEC = (LastTick + (60 * 1000)) - TimeUtil.NOW();

                        int FinalSec = (int)(SEC / 1000);

                        SendPvpToOtherPlayer(PlayerBattle, FinalSec, 1, ROUNID);
                    }

                    if (_ROUNDSTATE == ROUNDSTATE.START)
                    {
                        long SEC = (LastTick + (_Config.TimeAct.ELIMINATION_ROUND_ROUND * 1000)) - TimeUtil.NOW();
                        int FinalSec = (int)(SEC / 1000);

                        SendPvpToOtherPlayer(PlayerBattle, FinalSec, 2, ROUNID);
                    }
                }
            }
        }

        public void UpdateBattleFlagEvent()
        {
            foreach (KeyValuePair<int, FactionPlayer> entry in PVPARENA)
            {
                FactionPlayer PlayerBattle = entry.Value;

                if (PlayerBattle.player.CurrentMapCode == this.GetMapCode)
                {
                    long SEC = (LastTick + (_Config.TimeAct.FREE_ELIMINATION * 1000)) - TimeUtil.NOW();

                    int FinalSec = (int)(SEC / 1000);

                    SendBattleNotifyFlag(PlayerBattle, FinalSec);
                }
            }
        }

        public void UpdateBattleBattleAward()
        {
            foreach (KeyValuePair<int, FactionPlayer> entry in PVPARENA)
            {
                FactionPlayer PlayerBattle = entry.Value;

                if (PlayerBattle.player.CurrentMapCode == this.GetMapCode)
                {
                    long SEC = (LastTick + (_Config.TimeAct.REVICE_AWARD * 1000)) - TimeUtil.NOW();

                    int FinalSec = (int)(SEC / 1000);

                    SendBattleNotifyAward(PlayerBattle, FinalSec);
                }
            }
        }

        /// <summary>
        ///  Update thánh tích chiến đấu của người chơi trong vòng PK tự do
        /// </summary>
        /// <param name="InputPlayer"></param>
        /// <param name="Sec"></param>
        public void SendBattleNotifyAward(FactionPlayer InputPlayer, int Sec)
        {
            PlayChangeState(InputPlayer.player, 1);

            G2C_EventNotification _Notify = new G2C_EventNotification();

            if (Sec > 0)
            {
                _Notify.EventName = "Thời gian nhận thưởng:";
                _Notify.ShortDetail = "TIME|" + Sec;

                _Notify.TotalInfo = new List<string>();

                _Notify.TotalInfo.Add("Gặp Npc Cờ Quán Quân");
            }

            if (InputPlayer.player.IsOnline())
            {
                InputPlayer.player.SendPacket<G2C_EventNotification>((int)TCPGameServerCmds.CMD_KT_EVENT_NOTIFICATION, _Notify);
            }
        }

        public void NotifyALLTOBATTLE(string MSG)
        {
            foreach (KeyValuePair<int, FactionPlayer> entry in PVPARENA)
            {
                FactionPlayer PlayerBattle = entry.Value;

                KTPlayerManager.ShowNotification(PlayerBattle.player, MSG);
            }
        }

        /// <summary>
        ///  Update thánh tích chiến đấu của người chơi trong vòng PK tự do
        /// </summary>
        /// <param name="InputPlayer"></param>
        /// <param name="Sec"></param>
        public void SendBattleNotifyFlag(FactionPlayer InputPlayer, int Sec)
        {
            PlayChangeState(InputPlayer.player, 1);

            G2C_EventNotification _Notify = new G2C_EventNotification();

            if (Sec > 0)
            {
                _Notify.EventName = "Thời gian tìm cờ :";
                _Notify.ShortDetail = "TIME|" + Sec;
            }
            else
            {
                _Notify.EventName = "Thời gian tìm cờ đã hết";
                _Notify.ShortDetail = "TIME|" + 0;
            }

            _Notify.TotalInfo = new List<string>();

            _Notify.TotalInfo.Add("Tích Lũy : " + InputPlayer.nScore);

            _Notify.TotalInfo.Add("Số Cờ Đã Tìm : " + InputPlayer.TotalFlagCollect + "/" + 5);

            if (InputPlayer.player.IsOnline())
            {
                InputPlayer.player.SendPacket<G2C_EventNotification>((int)TCPGameServerCmds.CMD_KT_EVENT_NOTIFICATION, _Notify);
            }
        }

        /// <summary>
        ///  Update thánh tích chiến đấu của người chơi trong vòng PK tự do
        /// </summary>
        /// <param name="InputPlayer"></param>
        /// <param name="Sec"></param>
        public void SendBattleNotify1VS1(FactionPlayer InputPlayer, ROUNDSTATE _STATE, int Sec, ELIMINATION_INFO _INFO)
        {
            PlayChangeState(InputPlayer.player, 1);

            G2C_EventNotification _Notify = new G2C_EventNotification();

            if (_STATE == ROUNDSTATE.PREADING)
            {
                _Notify.EventName = "Tỉ thí sẽ bắt đầu sau:";
            }
            else if (_STATE == ROUNDSTATE.START)
            {
                _Notify.EventName = "Thời gian tỉ thí còn:";
            }
            else
            {
                _Notify.EventName = "Thời gian thi đấu đã kết thúc";
            }

            if (Sec > 0)
            {
                _Notify.ShortDetail = "TIME|" + Sec;
            }

            _Notify.TotalInfo = new List<string>();

            if (_INFO.Player_1.DamgeRecore > _INFO.Player_2.DamgeRecore)
            {
                _Notify.TotalInfo.Add(KTGlobal.CreateStringByColor(_INFO.Player_1.player.RoleName + " | Sát thương :" + _INFO.Player_1.DamgeRecore, ColorType.Green));

                _Notify.TotalInfo.Add(KTGlobal.CreateStringByColor("====VS====", ColorType.Importal));

                _Notify.TotalInfo.Add(KTGlobal.CreateStringByColor(_INFO.Player_2.player.RoleName + " | Sát thương :" + _INFO.Player_2.DamgeRecore, ColorType.Yellow));
            }
            else
            {
                _Notify.TotalInfo.Add(KTGlobal.CreateStringByColor(_INFO.Player_1.player.RoleName + " | Sát thương :" + _INFO.Player_1.DamgeRecore, ColorType.Yellow));

                _Notify.TotalInfo.Add(KTGlobal.CreateStringByColor("====VS====", ColorType.Importal));

                _Notify.TotalInfo.Add(KTGlobal.CreateStringByColor(_INFO.Player_2.player.RoleName + " | Sát thương :" + _INFO.Player_2.DamgeRecore, ColorType.Green));
            }

            if (InputPlayer.player.IsOnline())
            {
                InputPlayer.player.SendPacket<G2C_EventNotification>((int)TCPGameServerCmds.CMD_KT_EVENT_NOTIFICATION, _Notify);
            }
        }

        /// <summary>
        ///  Update thánh tích chiến đấu của người chơi trong vòng PK tự do
        /// </summary>
        /// <param name="InputPlayer"></param>
        /// <param name="Sec"></param>
        public void SendBattleNotify(FactionPlayer InputPlayer, int Sec)
        {
            PlayChangeState(InputPlayer.player, 1);

            G2C_EventNotification _Notify = new G2C_EventNotification();

            _Notify.EventName = "Vòng PK Hỗn Chiến";
            if (Sec > 0)
            {
                _Notify.ShortDetail = "TIME|" + Sec;
            }
            else
            {
                _Notify.ShortDetail = "Kết Thúc Hỗn Chiến";
            }

            _Notify.TotalInfo = new List<string>();

            _Notify.TotalInfo.Add("Tích Lũy : " + InputPlayer.nScore);

            _Notify.TotalInfo.Add("Bị Giết : " + InputPlayer.nDeathCount);

            _Notify.TotalInfo.Add("Hạng Hiện Tại :" + (GetRankInBxh(InputPlayer.player.RoleID) + 1));

            if (InputPlayer.player.IsOnline())
            {
                InputPlayer.player.SendPacket<G2C_EventNotification>((int)TCPGameServerCmds.CMD_KT_EVENT_NOTIFICATION, _Notify);
            }
        }

        #region MOVEPLAYER

        /// <summary>
        /// Move tất cả vào đấu trường PVP
        /// </summary>
        public void MoveAllPlayerToPvp()
        {
            // MOVE
            foreach (KeyValuePair<int, FactionPlayer> entry in PVPARENA)
            {
                FactionPlayer PlayerBattle = entry.Value;

                if (PlayerBattle.player.CurrentMapCode == GetMapCode)
                {
                    //Đánh đấu post
                    PlayerBattle.LastPosition = PlayerBattle.player.CurrentPos;

                    List<Postion> RandomPost = _Config.PVPRandomRevice;

                    int Random = new Random().Next(0, RandomPost.Count);

                    Postion _Position = RandomPost[Random];

                    this.ChangePos(PlayerBattle.player, _Position.PosX, _Position.PosY, 100, 100, 100);
                }
            }

            var _VerifyTele = VerifyGoIn();
        }

        public void MoveAllPlayerOutPvp()
        {
            // MOVE
            foreach (KeyValuePair<int, FactionPlayer> entry in PVPARENA)
            {
                FactionPlayer PlayerBattle = entry.Value;

                if (PlayerBattle.player.CurrentMapCode == GetMapCode)
                {
                    //Set lại vị trí cuối cùng
                    PlayerBattle.LastPosition = PlayerBattle.player.CurrentPos;

                    ////Nếu mà đang trong trạng thái khinh công
                    //if (PlayerBattle.player.m_eDoing == KE_NPC_DOING.do_jump)
                    //{
                    //    PlayerBattle.player.ForbidUsingSkill = true;

                    //    var Action = new Action(() => KTPlayerManager.Relive(PlayerBattle.player, this.GetMapCode, _Config.GoIn.X, _Config.GoIn.Y, 100, 100, 100));

                    //    var MoveValue = this.MoveDelayTask(Action, 2000);
                    //}

                    this.ChangePos(PlayerBattle.player, _Config.GoIn.PosX, _Config.GoIn.PosY, 100, 100, 100);
                }
            }
            // Call hàm verify tele để chắc chắn là người chơi đã được move ra
            var _VerifyTele = VerifyTelePortAsync();
        }

        public async System.Threading.Tasks.Task VerifyGoIn()
        {
            // LOOP 10 lần cho nó chắc
            for (int i = 0; i < 10; i++)
            {
                // Nếu trạng thái trận đấu là sẵn sàng cho solo 1 và nghỉ giữa trận thì chắc chắn rằng người chơi được chuyển ra khỏi khu vụ solo
                if ((BATTLESTATE == FactionState.MELEE_ROUND_1 || BATTLESTATE == FactionState.MELEE_ROUND_2) && (TimeUtil.NOW() - LastTick) < 120000)
                {
                    foreach (KeyValuePair<int, FactionPlayer> entry in PVPARENA)
                    {
                        FactionPlayer PlayerBattle = entry.Value;

                        if (PlayerBattle.player.CurrentMapCode == GetMapCode)
                        {
                            float Dist = KTGlobal.GetDistanceBetweenPoints(PlayerBattle.player.CurrentPos, PlayerBattle.LastPosition);

                            if (Dist < 1000)
                            {
                                LogManager.WriteLog(LogTypes.FactionBattle, "[" + PlayerBattle.player.RoleName + "] Toạch move vào đấu trường ==> Thực hiện move lại!");

                                List<Postion> RandomPost = _Config.PVPRandomRevice;

                                int Random = new Random().Next(0, RandomPost.Count);

                                Postion _Position = RandomPost[Random];

                                this.ChangePos(PlayerBattle.player, _Position.PosX, _Position.PosY);
                            }
                        }
                    }
                }

                // Nửa giây tick 1 phát
                await System.Threading.Tasks.Task.Delay(TimeSpan.FromMilliseconds(500));

                i++;
            }
        }

        public async System.Threading.Tasks.Task VerifyTelePortAsync()
        {
            // LOOP 10 lần cho nó chắc
            for (int i = 0; i < 10; i++)
            {
                // Nếu trạng thái trận đấu là sẵn sàng cho solo 1 và nghỉ giữa trận thì chắc chắn rằng người chơi được chuyển ra khỏi khu vụ solo
                if ((BATTLESTATE == FactionState.READY_ELIMINATION || BATTLESTATE == FactionState.FREE_MELEE) && (TimeUtil.NOW() - LastTick) < 120000)
                {
                    foreach (KeyValuePair<int, FactionPlayer> entry in PVPARENA)
                    {
                        FactionPlayer PlayerBattle = entry.Value;

                        if (PlayerBattle.player.CurrentMapCode == GetMapCode)
                        {
                            float Dist = KTGlobal.GetDistanceBetweenPoints(PlayerBattle.player.CurrentPos, PlayerBattle.LastPosition);

                            if (Dist < 1000)
                            {
                                LogManager.WriteLog(LogTypes.FactionBattle, "[" + PlayerBattle.player.RoleName + "] Toạch move khỏi đấu trường==> Thực hiện move lại!");

                                this.ChangePos(PlayerBattle.player, _Config.GoIn.PosX, _Config.GoIn.PosY, 100, 100, 100);
                            }
                        }
                    }
                }

                // Nửa giây tick 1 phát
                await System.Threading.Tasks.Task.Delay(TimeSpan.FromMilliseconds(500));

                i++;
            }
        }

        public async System.Threading.Tasks.Task MoveDelayTask(Action task, int delay)
        {
            await System.Threading.Tasks.Task.Delay(TimeSpan.FromMilliseconds(delay));

            task?.Invoke();
        }

        public void MoveAllPlayerQuitMap()
        {
            // MOVE
            foreach (KeyValuePair<int, FactionPlayer> entry in PVPARENA)
            {
                FactionPlayer PlayerBattle = entry.Value;

                //Xóa bỏ trạng thái cấm đánh
                PlayerBattle.player.ForbidUsingSkill = false;

                if (PlayerBattle.player.CurrentMapCode == GetMapCode)
                {
                    KT_TCPHandler.GetLastMapInfo(PlayerBattle.player, out int preMapCode, out int prePosX, out int prePosY);

                    KTPlayerManager.ChangeMap(PlayerBattle.player, preMapCode, prePosX, prePosY);
                }
            }
        }

        #endregion MOVEPLAYER

        public void SendKillStreak(KPlayer InputPlayer, int Count)
        {
            G2C_KillStreak _State = new G2C_KillStreak();

            _State.KillNumber = Count;

            if (InputPlayer.IsOnline())
            {
                InputPlayer.SendPacket<G2C_KillStreak>((int)TCPGameServerCmds.CMD_KT_KILLSTREAK, _State);
            }
        }

        public void NotifySocreForPlayer(FactionPlayer PlayerBattle, bool ShowKillStreak)
        {
            long SEC = (LastTick + (_Config.TimeAct.MELEE_PVP_DULATION * 1000)) - TimeUtil.NOW();

            int FinalSec = (int)(SEC / 1000);

            SendBattleNotify(PlayerBattle, FinalSec);
        }

        /// <summary>
        /// Nếu là on hit taget
        /// </summary>
        /// <param name="Hit"></param>
        /// <param name="BeHit"></param>
        /// <param name="DamgeRecore"></param>
        ///

        private Object lockDamage = new Object();

        public void OnHitTaget(KPlayer Hit, int DamgeRecore)
        {
            // Nếu là vòng tử chiến ghi lại số sát thương của thằng tấn công và thằng bị tấn công
            if (BATTLESTATE == FactionState.ELIMINATION_ROUND_1 || BATTLESTATE == FactionState.ELIMINATION_ROUND_2 || BATTLESTATE == FactionState.ELIMINATION_ROUND_3 || BATTLESTATE == FactionState.ELIMINATION_ROUND_4)
            {
                try
                {
                    PVPARENA.TryGetValue(Hit.RoleID, out FactionPlayer _HIT);

                    if (_HIT != null)
                    {
                        lock (lockDamage)
                        {
                            _HIT.DamgeRecore += DamgeRecore;
                            //LogManager.WriteLog(LogTypes.FactionBattle, "[" + _HIT.player.RoleName + "][" + BATTLESTATE.ToString() + "] Call Add Damge Value :" + _HIT.DamgeRecore);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogManager.WriteLog(LogTypes.FactionBattle, ex.ToString());
                }
            }
        }

        /// <summary>
        ///  Nếu mà đang thi đấu thằng này thoát ra
        /// </summary>
        /// <param name="Kill"></param>
        public void OnLogout(KPlayer LOGOUT)
        {
            if (BATTLESTATE == FactionState.ELIMINATION_ROUND_1 || BATTLESTATE == FactionState.ELIMINATION_ROUND_2 || BATTLESTATE == FactionState.ELIMINATION_ROUND_3 || BATTLESTATE == FactionState.ELIMINATION_ROUND_4)
            {
                PVPARENA.TryGetValue(LOGOUT.RoleID, out FactionPlayer _LOGOUT);

                if (_LOGOUT != null)
                {
                    int ROUNID = 0;

                    if (BATTLESTATE == FactionState.ELIMINATION_ROUND_1)
                    {
                        ROUNID = 1;
                    }
                    else if (BATTLESTATE == FactionState.ELIMINATION_ROUND_2)
                    {
                        ROUNID = 2;
                    }
                    else if (BATTLESTATE == FactionState.ELIMINATION_ROUND_3)
                    {
                        ROUNID = 3;
                    }
                    else if (BATTLESTATE == FactionState.ELIMINATION_ROUND_4)
                    {
                        ROUNID = 4;
                    }

                    foreach (ELIMINATION_INFO _ARENAFIND in ELIMINATION_TOTAL)
                    {
                        if (_ARENAFIND.ROUNDID == ROUNID && _ARENAFIND.WinThisRound == null)
                        {
                            if (_ARENAFIND.Player_1.player.RoleID == LOGOUT.RoleID || _ARENAFIND.Player_2.player.RoleID == LOGOUT.RoleID)
                            {
                                // Nếu mà thằng thoát là thằng đầu tiên
                                if (_ARENAFIND.Player_1.player.RoleID == LOGOUT.RoleID)
                                {
                                    lock (_ARENAFIND)
                                    {
                                        LogManager.WriteLog(LogTypes.FactionBattle, "[" + _ARENAFIND.Player_1.player.RoleName + "][" + BATTLESTATE.ToString() + "][" + ROUNID + "]  Đã thoát khỏi trận đấu! ====> Đối phương :" + _ARENAFIND.Player_2.player.RoleName + " dành thắng cuộc");

                                        // Nếu mà thằng đầu tiên
                                        if (_ARENAFIND.Player_1 != null)
                                        {
                                            _ARENAFIND.Player_1.BestElimateRank = ROUNID;
                                            // set camp cho thằng này = -1 và hòa bình
                                            _ARENAFIND.Player_1.player.Camp = -1;
                                            _ARENAFIND.Player_1.player.PKMode = (int)PKMode.Peace;
                                        }

                                        // SET PALYER 2 CAMP VỀ HÒA BÌNH
                                        if (_ARENAFIND.Player_2 != null)
                                        {
                                            _ARENAFIND.WinThisRound = _ARENAFIND.Player_2;

                                            _ARENAFIND.Player_2.IsWinRound = true;

                                            Action _Action = new Action(() => this.ChangePos(_ARENAFIND.Player_2.player, _Config.GoIn.PosX, _Config.GoIn.PosY));

                                            var MoveDelay = MoveDelayTask(_Action, 5000);

                                            // set camp cho thằng này = -1 và hòa bình
                                            _ARENAFIND.Player_2.player.Camp = -1;
                                            _ARENAFIND.Player_2.player.PKMode = (int)PKMode.Peace;
                                        }
                                    }

                                    string NOTIFY = KTGlobal.CreateStringByColor("Trong trận tỉ thí giữa ", ColorType.Yellow) + KTGlobal.CreateStringByColor(_ARENAFIND.Player_1.player.RoleName, GameServer.Logic.ColorType.Importal) + " VS " + KTGlobal.CreateStringByColor(_ARENAFIND.Player_2.player.RoleName, GameServer.Logic.ColorType.Importal) + " người chơi " + KTGlobal.CreateStringByColor(_ARENAFIND.Player_2.player.RoleName, GameServer.Logic.ColorType.Importal) + " đã dành chiến thắng";

                                    NotifyALLTOBATTLE(NOTIFY);

                                    KTPlayerManager.ShowMessageBox(_ARENAFIND.Player_2.player, "Thông báo thi đấu", "Trong trận tỉ thí với " + _ARENAFIND.Player_1.player.RoleName + " bạn đã dành chiến thắng");

                                    KTPlayerManager.ShowMessageBox(_ARENAFIND.Player_1.player, "Thông báo thi đấu", "Trong trận tỉ thí với " + _ARENAFIND.Player_2.player.RoleName + " bạn đã thua cuộc");
                                }
                                else if (_ARENAFIND.Player_2.player.RoleID == LOGOUT.RoleID)
                                {
                                    LogManager.WriteLog(LogTypes.FactionBattle, "[" + _ARENAFIND.Player_2.player.RoleName + "][" + BATTLESTATE.ToString() + "][" + ROUNID + "] Đã thoát khỏi trận đấu! ====> Đối phương :" + _ARENAFIND.Player_1.player.RoleName + " dành thắng cuộc");
                                    // SET PALYER 1 CAMP VỀ HÒA BÌNH
                                    lock (_ARENAFIND)
                                    {
                                        if (_ARENAFIND.Player_1 != null)
                                        {
                                            _ARENAFIND.WinThisRound = _ARENAFIND.Player_1;
                                            //Set cho thằng 1 thắng
                                            _ARENAFIND.Player_1.IsWinRound = true;

                                            Action _Action = new Action(() => this.ChangePos(_ARENAFIND.Player_1.player, _Config.GoIn.PosX, _Config.GoIn.PosY));

                                            var MoveDelay = MoveDelayTask(_Action, 5000);
                                            // set camp cho thằng này = -1 và hòa bình
                                            _ARENAFIND.Player_1.player.Camp = -1;
                                            _ARENAFIND.Player_1.player.PKMode = (int)PKMode.Peace;
                                        }

                                        // SET PALYER 2 CAMP VỀ HÒA BÌNH
                                        if (_ARENAFIND.Player_2 != null)
                                        {
                                            _ARENAFIND.Player_2.BestElimateRank = ROUNID;
                                            // set camp cho thằng này = -1 và hòa bình
                                            _ARENAFIND.Player_2.player.Camp = -1;
                                            _ARENAFIND.Player_2.player.PKMode = (int)PKMode.Peace;
                                        }
                                    }
                                    string NOTIFY = KTGlobal.CreateStringByColor("Trong trận tỉ thí giữa ", ColorType.Yellow) + KTGlobal.CreateStringByColor(_ARENAFIND.Player_1.player.RoleName, GameServer.Logic.ColorType.Importal) + " VS " + KTGlobal.CreateStringByColor(_ARENAFIND.Player_2.player.RoleName, GameServer.Logic.ColorType.Importal) + " người chơi " + KTGlobal.CreateStringByColor(_ARENAFIND.Player_1.player.RoleName, GameServer.Logic.ColorType.Importal) + " đã dành chiến thắng";

                                    NotifyALLTOBATTLE(NOTIFY);

                                    KTPlayerManager.ShowMessageBox(_ARENAFIND.Player_1.player, "Thông báo thi đấu", "Trong trận tỉ thí với " + _ARENAFIND.Player_2.player.RoleName + " bạn đã dành chiến thắng");

                                    KTPlayerManager.ShowMessageBox(_ARENAFIND.Player_2.player, "Thông báo thi đấu", "Trong trận tỉ thí với " + _ARENAFIND.Player_1.player.RoleName + " bạn đã thua cuộc");
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Event tính toán sự kiện A GIẾT B
        /// </summary>
        /// <param name="kPlayer1"></param>
        /// <param name="kPlayer2"></param>
        public void OnKillRole(KPlayer Kill, KPlayer Bekill)
        {
            if (BATTLESTATE == FactionState.MELEE_ROUND_1 || BATTLESTATE == FactionState.MELEE_ROUND_2)
            {
                PVPARENA.TryGetValue(Kill.RoleID, out FactionPlayer _KILL);

                if (_KILL != null && _KILL.player.MapCode == this.GetMapCode)
                {
                    _KILL.KillStreak = _KILL.KillStreak + 1;

                    if (_KILL.MaxKillStreak < _KILL.KillStreak)
                    {
                        _KILL.MaxKillStreak = _KILL.KillStreak;
                    }

                    if (_KILL.KillStreak > 0 && _KILL.KillStreak % 3 == 0)
                    {
                        SendKillStreak(Kill, _KILL.KillStreak);
                    }

                    _KILL.nScore = _KILL.nScore + 1;

                    NotifySocreForPlayer(_KILL, true);
                }

                PVPARENA.TryGetValue(Bekill.RoleID, out FactionPlayer _BEKILL);
                if (_BEKILL != null && _BEKILL.player.MapCode == this.GetMapCode)
                {
                    _BEKILL.KillStreak = 0;

                    _BEKILL.nDeathCount = _BEKILL.nDeathCount + 1;

                    NotifySocreForPlayer(_BEKILL, false);
                }
            }
            else if (BATTLESTATE == FactionState.ELIMINATION_ROUND_1 || BATTLESTATE == FactionState.ELIMINATION_ROUND_2 || BATTLESTATE == FactionState.ELIMINATION_ROUND_3 || BATTLESTATE == FactionState.ELIMINATION_ROUND_4)
            {
                PVPARENA.TryGetValue(Kill.RoleID, out FactionPlayer _KILL);
                PVPARENA.TryGetValue(Bekill.RoleID, out FactionPlayer _BEKILL);

                if (_KILL != null && _BEKILL != null)
                {
                    int ROUNID = 0;

                    if (BATTLESTATE == FactionState.ELIMINATION_ROUND_1)
                    {
                        ROUNID = 1;
                    }
                    else if (BATTLESTATE == FactionState.ELIMINATION_ROUND_2)
                    {
                        ROUNID = 2;
                    }
                    else if (BATTLESTATE == FactionState.ELIMINATION_ROUND_3)
                    {
                        ROUNID = 3;
                    }
                    else if (BATTLESTATE == FactionState.ELIMINATION_ROUND_4)
                    {
                        ROUNID = 4;
                    }

                    foreach (ELIMINATION_INFO _ARENAFIND in ELIMINATION_TOTAL)
                    {
                        if (_ARENAFIND.ROUNDID == ROUNID)
                        {
                            if (_ARENAFIND.Player_1.player != null)
                            {
                                if (_ARENAFIND.Player_1.player.RoleID == Kill.RoleID)
                                {
                                    lock (_ARENAFIND)
                                    {
                                        LogManager.WriteLog(LogTypes.FactionBattle, "PVP [" + _ARENAFIND.ARENAID + "]ROUND :" + ROUNID + " | KILL ROLE REPORT FORM EVENT KILL :" + Kill.RoleName + "| BEKILL :" + Bekill.RoleName);

                                        // SET PALYER 1 CAMP VỀ HÒA BÌNH
                                        _ARENAFIND.WinThisRound = _ARENAFIND.Player_1;
                                        _ARENAFIND.Player_1.IsWinRound = true;
                                        // set camp cho thằng này = -1 và hòa bình
                                        _ARENAFIND.Player_1.player.Camp = -1;
                                        _ARENAFIND.Player_1.player.PKMode = (int)PKMode.Peace;
                                        // SET PALYER 2 CAMP VỀ HÒA BÌNH
                                        _ARENAFIND.Player_2.BestElimateRank = ROUNID;
                                        // set camp cho thằng này = -1 và hòa bình
                                        _ARENAFIND.Player_2.player.Camp = -1;
                                        _ARENAFIND.Player_2.player.PKMode = (int)PKMode.Peace;

                                        Action _Action = new Action(() => this.ChangePos(_ARENAFIND.Player_1.player, _Config.GoIn.PosX, _Config.GoIn.PosY));
                                        var MoveDelay = MoveDelayTask(_Action, 5000);

                                        string NOTIFY = KTGlobal.CreateStringByColor("Trong trận tỉ thí giữa ", ColorType.Yellow) + KTGlobal.CreateStringByColor(_ARENAFIND.Player_1.player.RoleName, GameServer.Logic.ColorType.Importal) + " VS " + KTGlobal.CreateStringByColor(_ARENAFIND.Player_2.player.RoleName, GameServer.Logic.ColorType.Importal) + " người chơi " + KTGlobal.CreateStringByColor(_ARENAFIND.Player_1.player.RoleName, GameServer.Logic.ColorType.Importal) + " đã dành chiến thắng";

                                        NotifyALLTOBATTLE(NOTIFY);

                                        KTPlayerManager.ShowMessageBox(_ARENAFIND.Player_1.player, "Thông báo kết quả", "Trong trận tỉ thí với " + _ARENAFIND.Player_2.player.RoleName + " bạn đã dành chiến thắng");

                                        KTPlayerManager.ShowMessageBox(_ARENAFIND.Player_2.player, "Thông báo kết quả", "Trong trận tỉ thí với " + _ARENAFIND.Player_1.player.RoleName + " bạn đã thua cuộc");

                                        _ARENAFIND._ROUNDSTATE = ROUNDSTATE.END;
                                    }
                                }
                                // SET THẰNG THỨ 2 THẮNG
                                else if (_ARENAFIND.Player_2.player.RoleID == Kill.RoleID)
                                {
                                    lock (_ARENAFIND)
                                    {
                                        _ARENAFIND.Player_1.BestElimateRank = ROUNID;
                                        // set camp cho thằng này = -1 và hòa bình
                                        _ARENAFIND.Player_1.player.Camp = -1;
                                        _ARENAFIND.Player_1.player.PKMode = (int)PKMode.Peace;

                                        _ARENAFIND.WinThisRound = _ARENAFIND.Player_2;
                                        _ARENAFIND.Player_2.IsWinRound = true;
                                        // set camp cho thằng này = -1 và hòa bình
                                        _ARENAFIND.Player_2.player.Camp = -1;
                                        _ARENAFIND.Player_2.player.PKMode = (int)PKMode.Peace;
                                        Action _Action = new Action(() => this.ChangePos(_ARENAFIND.Player_2.player, _Config.GoIn.PosX, _Config.GoIn.PosY));
                                        var MoveDelay = MoveDelayTask(_Action, 5000);

                                        string NOTIFY = KTGlobal.CreateStringByColor("Trong trận tỉ thí giữa ", ColorType.Yellow) + KTGlobal.CreateStringByColor(_ARENAFIND.Player_1.player.RoleName, GameServer.Logic.ColorType.Importal) + " VS " + KTGlobal.CreateStringByColor(_ARENAFIND.Player_2.player.RoleName, GameServer.Logic.ColorType.Importal) + " người chơi " + KTGlobal.CreateStringByColor(_ARENAFIND.Player_2.player.RoleName, GameServer.Logic.ColorType.Importal) + " đã dành chiến thắng";

                                        NotifyALLTOBATTLE(NOTIFY);

                                        KTPlayerManager.ShowMessageBox(_ARENAFIND.Player_2.player, "Thông báo kết quả", "Trong trận tỉ thí với " + _ARENAFIND.Player_1.player.RoleName + " bạn đã dành chiến thắng");

                                        KTPlayerManager.ShowMessageBox(_ARENAFIND.Player_1.player, "Thông báo kết quả", "Trong trận tỉ thí với " + _ARENAFIND.Player_2.player.RoleName + " bạn đã thua cuộc");

                                        _ARENAFIND._ROUNDSTATE = ROUNDSTATE.END;
                                    }
                                }
                            }
                            else
                            {
                                LogManager.WriteLog(LogTypes.FactionBattle, "PVP [" + _ARENAFIND.ARENAID + "]ROUND :" + ROUNID + " | KILL ROLE REPORT FORM EVENT KILL :" + Kill.RoleName + "| BEKILL :" + Bekill.RoleName + "===============> NULL PLAYER");
                            }

                            if (_ARENAFIND.IsCreateBox == false)
                            {
                                // Gán là đã tọa box rồi để lần sau nó khỏi tạo tiếp
                                _ARENAFIND.IsCreateBox = true;

                                Box _BoxConfig = _Config.Box.Where(x => x.ArenaID == _ARENAFIND.ARENAID).FirstOrDefault();
                                // TẠO RANDOM từ 3 tới 6 rương
                                int Random = KTGlobal.GetRandomNumber(2, 6);

                                for (int i = 0; i < Random; i++)
                                {
                                    int BoxCount = _BoxConfig.RandomBox.Count;

                                    int RandomPos = KTGlobal.GetRandomNumber(0, BoxCount);
                                    if (RandomPos > 0)
                                    {
                                        RandomPos = RandomPos - 1;
                                    }
                                    Postion _Postion = _BoxConfig.RandomBox[RandomPos];

                                    GameMap Map = KTMapManager.Find(this.GetMapCode);

                                    // TẠON BOX Ở VỊ TRÍ TƯƠNG ỨNG
                                    CreateBox(Map, _Postion.PosX, _Postion.PosY);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}