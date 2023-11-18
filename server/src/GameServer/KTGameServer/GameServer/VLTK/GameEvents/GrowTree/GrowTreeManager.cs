using GameServer.Core.Executor;
using GameServer.KiemThe;
using GameServer.KiemThe.Core;
using GameServer.KiemThe.Core.Item;
using GameServer.KiemThe.Core.Task;
using GameServer.KiemThe.Entities;
using GameServer.KiemThe.Logic;
using GameServer.KiemThe.Logic.Manager;
using GameServer.Logic;
using GameServer.Server;
using GameServer.VLTK.Core.GuildManager;
using Server.Data;
using Server.Tools;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Xml.Serialization;
using static GameServer.Logic.KTMapManager;

namespace GameServer.VLTK.GameEvents.GrowTree
{
    //TODO : XỬ lý sự kiện người chơi giết người chơi | Ghi lại tích lũy khi thu thập hạt | Xử lý sự kiện khi kết thúc sự kiện
    /// <summary>
    /// Class quản lý hạt hoàng kim
    /// </summary>
    public class GrowTreeManager
    {
        public static GROWTREE_STATE _STATE = GROWTREE_STATE.NOT_OPEN;

        public static bool IsToday = false;

        public static long LastNofity { get; set; }
        public static long LastTick { get; set; }

        public static long LastUpdateScore { get; set; }

        public static int RegisterDualtion = 120;

        /// <summary>
        /// Files grow tree
        /// </summary>
        public static string GROW_CONFIG_FILE = "Config/KT_Activity/KTGrowTreeConfig.xml";

        public static GrowTreeConfig _GrowTreeConfig = new GrowTreeConfig();

        /// <summary>
        /// Toàn bộ người chơi tham gia hạt hoàng kim
        /// </summary>
        public static ConcurrentDictionary<int, GrowTreePlayer> GrowTreePlayers = new ConcurrentDictionary<int, GrowTreePlayer>();

        /// <summary>
        /// Tổng điểm tích lũy của các bang
        /// </summary>
        public static ConcurrentDictionary<int, int> GuildPoint = new ConcurrentDictionary<int, int>();


        public static void ReloadConfig()
        {
            string Files = KTGlobal.GetDataPath(GROW_CONFIG_FILE);

            using (var stream = System.IO.File.OpenRead(Files))
            {
                var serializer = new XmlSerializer(typeof(GrowTreeConfig));
                _GrowTreeConfig = serializer.Deserialize(stream) as GrowTreeConfig;
            }

            if (_GrowTreeConfig != null)
            {
                int TODAY = TimeUtil.GetWeekDay1To7(DateTime.Now);

                if (_GrowTreeConfig.DayOfWeek.Contains(TODAY))
                {
                    IsToday = true;
                }
            }
        }
        public static void Setup()
        {
            ReloadConfig();

            ScheduleExecutor2.Instance.scheduleExecute(new NormalScheduleTask("GrowTreeEvent", ProsecEvent), 5 * 1000, 2000);
        }

        public static bool IsInEvent(KPlayer client)
        {
            if (client.MapCode == _GrowTreeConfig.MapID)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Khi nào làm xong NPC thì gắn scippt vào đây
        /// </summary>
        /// <param name="npc"></param>
        /// <param name="client"></param>
        public static void NpcClickJoin(NPC npc, KPlayer client)
        {
            string DESC = "<color=red>Sự kiện :</color> <b><color=yellow>Hạt Hoàng Kim</color></b>\n<color=red>Thời gian :</color> Diễn ra vào 19h-20h thứ 2 và thứ 5 hàng tuần\n<color=red>Yêu cầu :</color> Cấp [" + _GrowTreeConfig.MinLevelCanJoin + "] trở lên!\n\nNgười chơi thu thập hạt hoàng kim sẽ nhận được nhiều phần thưởng hấp dẫn\nNgoài ra Top 3 bang xếp hạng cao nhất sẽ nhận được Exp bang hội dựa trên tổng tích lũy của bang";

            KNPCDialog _NpcDialog = new KNPCDialog();

            Dictionary<int, string> Selections = new Dictionary<int, string>();

            // Nếu công thành chiến chưa bắt đầu
            if (_STATE == GROWTREE_STATE.NOT_OPEN)
            {
                _NpcDialog.Text = DESC + "\n\n\n<color=red><b>Sự kiện chưa bắt đầu.\nHãy quay lại sau!</b></color>";
            }
            // Nếu mà đang chuẩn bị tham chiến hoặc là đã bắt đầu
            // Đã bắt đầu cho vào để cứu những thằng bị diss ra mà muốn vào lại
            else if (_STATE == GROWTREE_STATE.PREDING_OPEN || _STATE == GROWTREE_STATE.OPEN)
            {
                if (client.m_Level < _GrowTreeConfig.MinLevelCanJoin)
                {
                    _NpcDialog.Text = DESC + "<b>Cấp độ của bạn không đủ để tham gia sự kiện\nCấp độ tối thiểu để có thể tham gia sự kiện là :" + _GrowTreeConfig.MinLevelCanJoin + "!</b>";
                }
                else
                {
                    _NpcDialog.Text = DESC + "\n\n\n<color=green><b>Sự kiện đã bắt đầu.\nHãy mau tới thu thập hạt!</b></color>";
                    Selections.Add(1, "Hãy đưa ta tới điểm thu thập!");
                }
            }
            else
            {
                _NpcDialog.Text = DESC + "<b>Sự kiện đã kết thúc.\nHãy quay lại vào ngày hôm sau!</b>";
            }

            // Nếu ở trạng thái chuẩn bị bắt đầu
            Selections.Add(1000, "Ta hiểu rồi!");

            //Selections.Add(-2, "Ta muốn rời khỏi chiến trường");

            Action<TaskCallBack> ActionWork = (x) => DoActionSelect(client, npc, x);

            _NpcDialog.OnSelect = ActionWork;

            _NpcDialog.Selections = Selections;

            //_NpcDialog.Text = Text;

            _NpcDialog.Show(npc, client);
        }

        private static void DoActionSelect(KPlayer client, NPC npc, TaskCallBack x)
        {
            if (x.SelectID == 1)
            {
                JoinEvent(client);
            }
            else if (x.SelectID == 1000)
            {
                KT_TCPHandler.CloseDialog(client);
            }
        }

        /// <summary>
        /// Thực thi sự kiện cho thằng này tham gia event
        /// </summary>
        /// <param name="client"></param>
        private static void JoinEvent(KPlayer client)
        {
            GrowTreePlayer _GrowTreePlayer = new GrowTreePlayer();
            _GrowTreePlayer.client = client;
            _GrowTreePlayer.KillCount = 0;
            _GrowTreePlayer.MaxKillSteak = 0;
            _GrowTreePlayer.GuildID = client.GuildID;
            _GrowTreePlayer.CollectTotal = 0;

            _GrowTreePlayer.Point = 0;

            GrowTreePlayers.TryGetValue(client.RoleID, out GrowTreePlayer _Player);
            if (_Player != null)
            {
                // set lại client cho thằng reconnect
                GrowTreePlayers[client.RoleID].client = client;
            }
            else
            {
                // add mới vào dánh sách
                GrowTreePlayers.TryAdd(client.RoleID, _GrowTreePlayer);
            }

            // Lưu lại địa điểm trước khi vào
            KT_TCPHandler.UpdateLastMapInfo(client, client.CurrentMapCode, (int)client.CurrentPos.X, (int)client.CurrentPos.Y);

            RespwanPoint _Point = _GrowTreeConfig.RespwanPoints[KTGlobal.GetRandomNumber(0, _GrowTreeConfig.RespwanPoints.Count - 1)];

            // Chuyển thằng này vào trong bản đồ chỉ định======> mút
            KTPlayerManager.ChangeMap(client, _GrowTreeConfig.MapID, _Point.PosX, _Point.PosY);
        }

        #region Notify

        /// <summary>
        /// Force khởi động sự kiện = lệnh Gm
        /// </summary>
        public static void GMFORCESTARTEVENT()
        {
            _STATE = GROWTREE_STATE.PREDING_OPEN;

            LastTick = TimeUtil.NOW();

            LogManager.WriteLog(LogTypes.GrowTree, "Event Change State ==> " + _STATE.ToString());

            KTGlobal.SendSystemEventNotification("Sự kiện Hạt Hoàng Kim chuẩn bị bắt đầu hãy tới NPC Long Ngũ để tham gia!");
        }

        public static void GMSTOPEVENT()
        {
            LastTick = TimeUtil.NOW();

            ClearAllGrowPoint();

            GuildExp();
            // Đánh dấu sự kiện đã diễn ra
            _STATE = GROWTREE_STATE.CLOSE;

            KTGlobal.SendSystemEventNotification("Sự kiện Hạt Hoàng Kim đã kết thúc!");

            SendNotifyAllMap("Sự kiện đã kết thúc.Rời khỏi phụ bản sau 10s");
        }

        /// <summary>
        ///  Chuyển sang event state
        /// </summary>
        /// <param name="Player"></param>
        /// <param name="State"></param>
        public static void PlayChangeState(KPlayer Player, int State)
        {
            G2C_EventState _State = new G2C_EventState();

            _State.EventID = 10;
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

        public static void UpdatePreading()
        {
            long SEC = (LastTick + RegisterDualtion * 1000) - TimeUtil.NOW();

            int FinalSec = (int)(SEC / 1000);

            foreach (KeyValuePair<int, GrowTreePlayer> entry in GrowTreePlayers)
            {
                GrowTreePlayer PlayerBattle = entry.Value;

                // Nếu thằng này đang còn trong bản đồ này
                if (PlayerBattle.client.CurrentMapCode == _GrowTreeConfig.MapID)
                {
                    // Chuyển sang chế độ pk bang hội
                    PlayerBattle.client.PKMode = (int)PKMode.Guild;

                    // chuyển thằng này cho nó hiện cái bảng
                    PlayChangeState(PlayerBattle.client, 1);

                    SendPreadingNotify(PlayerBattle.client, FinalSec, GrowTreePlayers.Count);
                }
            }
        }

        public static void SendNotifyAllMap(string Notyfi)
        {
            foreach (KeyValuePair<int, GrowTreePlayer> entry in GrowTreePlayers)
            {
                GrowTreePlayer PlayerBattle = entry.Value;

                // Nếu thằng này đang còn trong bản đồ này
                if (PlayerBattle.client.CurrentMapCode == _GrowTreeConfig.MapID)
                {
                    KTPlayerManager.ShowNotification(PlayerBattle.client, Notyfi);
                }
            }
        }

        public static void UpdateEventNotify()
        {
            foreach (KeyValuePair<int, GrowTreePlayer> entry in GrowTreePlayers)
            {
                GrowTreePlayer PlayerBattle = entry.Value;

                // Nếu thằng này đang còn trong bản đồ này
                if (PlayerBattle.client.CurrentMapCode == _GrowTreeConfig.MapID)
                {
                    PlayChangeState(PlayerBattle.client, 1);
                    // Chuyển sang chế độ pk bang hội
                    PlayerBattle.client.PKMode = (int)PKMode.Guild;

                    SendNotify(PlayerBattle);
                }
            }
        }

        public static void SendNotify(GrowTreePlayer Player)
        {
            // Xem còn bao nhiêu thơi gian
            long SEC = (LastTick + (_GrowTreeConfig.Dualtion * 1000)) - TimeUtil.NOW();

            int FinalSec = (int)(SEC / 1000);

            G2C_EventNotification _Notify = new G2C_EventNotification();

            _Notify.EventName = "Hạt Hoàng Kim";
            if (FinalSec > 0)
            {
                _Notify.ShortDetail = "TIME|" + FinalSec;
            }
            else
            {
                _Notify.ShortDetail = "Hạt Hoàng Kim Đã Kết Thúc!";
            }

            _Notify.TotalInfo = new List<string>();

            _Notify.TotalInfo.Add("Giết Địch : " + Player.KillCount);

            _Notify.TotalInfo.Add("Hạt Thu Thập: " + Player.CollectTotal);

            int POINT = 0;

            if (Player.client.GuildID > 0)
            {
                GuildPoint.TryGetValue(Player.client.GuildID, out POINT);

                _Notify.TotalInfo.Add("Tổng Tích Lũy Bang: " + POINT);
            }

            if (Player.client.IsOnline())
            {
                Player.client.SendPacket<G2C_EventNotification>((int)TCPGameServerCmds.CMD_KT_EVENT_NOTIFICATION, _Notify);
            }
        }

        public static void SendPreadingNotify(KPlayer Player, int Sec, int TotalTong)
        {
            G2C_EventNotification _Notify = new G2C_EventNotification();

            _Notify.EventName = "Hạt hoàng kim sẽ xuất hiện sau:";

            _Notify.ShortDetail = "TIME|" + Sec;

            _Notify.TotalInfo = new List<string>();

            _Notify.TotalInfo.Add("Số người tham gia : " + TotalTong);

            if (Player.IsOnline())
            {
                Player.SendPacket<G2C_EventNotification>((int)TCPGameServerCmds.CMD_KT_EVENT_NOTIFICATION, _Notify);
            }
            else
            {
                //Console.WriteLine("OFFLINE");
            }
        }

        #endregion Notify
        
        /// <summary>
        /// Hàm clear gọi khi ngày mới trôi qua
        /// </summary>
        public static void Clear()
        {
            ReloadConfig();
            //Set lại state = not open
            _STATE = GROWTREE_STATE.NOT_OPEN;
        }
        /// <summary>
        /// Ontick xử lý các sự kiện liên quan tới hạt hoàng kim
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void ProsecEvent(object sender, EventArgs e)
        {
            if (_STATE == GROWTREE_STATE.NOT_OPEN)
            {
                // nếu như là ngày hôm này
                if (IsToday)
                {
                    DateTime now = DateTime.Now;
                    if (now.Hour == _GrowTreeConfig.TimeStart[0] && now.Minute >= _GrowTreeConfig.TimeStart[1] && now.Second >= _GrowTreeConfig.TimeStart[2])
                    {
                        _STATE = GROWTREE_STATE.PREDING_OPEN;

                        LastTick = TimeUtil.NOW();

                        LogManager.WriteLog(LogTypes.GrowTree, "Event Change State ==> " + _STATE.ToString());

                        KTGlobal.SendSystemEventNotification("Sự kiện Hạt Hoàng Kim chuẩn bị bắt đầu hãy tới Npc <color=red>Sứ Giả Hoạt Động</color> tại các Thành Thị để tham gia!");
                    }
                }
            }
            else if (_STATE == GROWTREE_STATE.PREDING_OPEN)
            {
                // Cứ 5s spam notitfy về client 1 phát
                if (TimeUtil.NOW() - LastNofity >= 5 * 1000 && _STATE == GROWTREE_STATE.PREDING_OPEN)
                {
                    LastNofity = TimeUtil.NOW();
                    //spam nào
                    UpdatePreading();
                }

                if (TimeUtil.NOW() >= LastTick + RegisterDualtion * 1000 && _STATE == GROWTREE_STATE.PREDING_OPEN)
                {
                    LastTick = TimeUtil.NOW();

                    // Đánh dấu sự kiện đã diễn ra
                    _STATE = GROWTREE_STATE.OPEN;

                    // UpdateBattleNotify();
                    KTGlobal.SendSystemEventNotification("Sự kiện Hạt Hoàng Kim đã chính thức bắt đầu,hãy mau chóng thu thập quả lớn!");
                    // Tạo ra danh sách hạt hoàng kim
                    CreateTree();
                }
            }
            // Đánh dấu sự kiện đã bắt đầu
            else if (_STATE == GROWTREE_STATE.OPEN)
            {
                //Cứ 5s notify update kết quả về cho tất cả ngưởi chơi
                if (TimeUtil.NOW() - LastNofity >= 5 * 1000 && _STATE == GROWTREE_STATE.OPEN)
                {
                    LastNofity = TimeUtil.NOW();
                    //spam nào
                    UpdateEventNotify();
                }

                if (TimeUtil.NOW() >= LastTick + _GrowTreeConfig.Dualtion * 1000 && _STATE == GROWTREE_STATE.OPEN)
                {
                    LastTick = TimeUtil.NOW();

                    ClearAllGrowPoint();

                    GuildExp();
                    // Đánh dấu sự kiện đã diễn ra
                    _STATE = GROWTREE_STATE.CLOSE;

                    KTGlobal.SendSystemEventNotification("Sự kiện Hạt Hoàng Kim đã kết thúc!");

                    SendNotifyAllMap("Sự kiện đã kết thúc.Rời khỏi phụ bản sau 10s");
                }
            }
            else if (_STATE == GROWTREE_STATE.CLOSE)
            {
                //10s sau thì thì kick tất cả bọn nó ra
                if (TimeUtil.NOW() >= LastTick + 10 * 1000 && _STATE == GROWTREE_STATE.CLOSE)
                {
                    LastTick = TimeUtil.NOW();

                    // Chim cút tất cả người chơi
                    MovePlayerOut();

                    _STATE = GROWTREE_STATE.CLEAR;

                    KTGlobal.SendSystemEventNotification("Sự kiện Hạt Hoàng Kim đã kết thúc!");
                }
            }
        }

        /// <summary>
        /// Prosec GuidExp
        /// </summary>
        public static void GuildExp()
        {
            int Position = 1;

            foreach (KeyValuePair<int, int> entry in GuildPoint.OrderByDescending(x => x.Value))
            {
                int GuildID = entry.Key;

                MiniGuildInfo Info = GuildManager.getInstance()._GetInfoGuildByGuildID(GuildID);

                int CurentExp = Info.GuildExp;

                int EXPGUILDCANEAR = (entry.Value / 10);

                CurentExp += EXPGUILDCANEAR;

                string NOTIFY = "Kinh nghiệm bang hội gia tăng :" + EXPGUILDCANEAR + " từ tích lũy hạt hoàng kim";

                // Gửi thông báo cho bang hội
                KTGlobal.SendGuildChat(GuildID, NOTIFY, null, "");

                if (Position == 1)
                {
                    KTGlobal.SendSystemEventNotification("Bang hội [" + Info.GuildName + "] đã là bang xuất sắc trong đợt khai thác Hạt đợt này!");
                }
                Position++;
            }
        }

        /// <summary>
        /// Di chuyển tất cả bọn này ra khỏi bản đồ
        /// </summary>
        public static void MovePlayerOut()
        {
            foreach (KeyValuePair<int, GrowTreePlayer> entry in GrowTreePlayers)
            {
                GrowTreePlayer PlayerBattle = entry.Value;

                // Nếu người chơi còn ở trong bản đồ
                if (PlayerBattle.client.CurrentMapCode == _GrowTreeConfig.MapID)
                {
                    PlayChangeState(PlayerBattle.client, 0);

                    KT_TCPHandler.GetLastMapInfo(PlayerBattle.client, out int preMapCode, out int prePosX, out int prePosY);

                    KTPlayerManager.ChangeMap(PlayerBattle.client, preMapCode, prePosX, prePosY);

                    // Set lại cho camp người chơi về hòa bình
                    PlayerBattle.client.Camp = (int)PKMode.Peace;
                    PlayerBattle.client.TempTitle = "";
                }
            }
        }

        public static void ClearAllGrowPoint()
        {
            KTGrowPointManager.RemoveMapGrowPoints(_GrowTreeConfig.MapID);
        }

        /// <summary>
        /// Tạo hạt hoàng kim ở đây sau mỗi thời gian nó sẽ ngẫu nhiên mọc lại
        /// </summary>
        private static void CreateTree()
        {
            //throw new NotImplementedException();
            List<GrowTreeModel> GrowTrees = _GrowTreeConfig.GrowTrees;
            if (GrowTrees != null)
            {
                foreach (GrowTreeModel _Grow in GrowTrees)
                {
                    // Gọi ra hàm để tạo gro
                    CreateTreePoint(_Grow);
                }
            }
        }

        /// <summary>
        /// Thực hiện hồi sinh
        /// </summary>
        /// <param name="client"></param>
        public static void Revice(KPlayer client)
        {
            // lấy ra 1 điểm rồi cho hồi sinh
            RespwanPoint _Point = _GrowTreeConfig.RespwanPoints[KTGlobal.GetRandomNumber(0, _GrowTreeConfig.RespwanPoints.Count - 1)];
            // Cho thằng này hồi sinh nếu toác
            KTPlayerManager.Relive(client, _GrowTreeConfig.MapID, _Point.PosX, _Point.PosY, 100, 100, 100);
        }

        public static int GetPointCanGetByQuality(int Quality)
        {
            switch (Quality)
            {
                case 0:
                    return 50;

                case 1:
                    return 60;

                case 2:
                    return 70;

                case 3:
                    return 80;

                case 4:
                    return 100;
            }

            return 0;
        }

        /// <summary>
        /// Sau khi thu thập xong thì sẽ nhận được cái gì
        /// </summary>
        /// <param name="Quality"></param>
        public static void CollectComplete(int Quality, KPlayer client)
        {
            int MAXPOINT = GetPointCanGetByQuality(Quality);

            GrowTreePlayers.TryGetValue(client.RoleID, out GrowTreePlayer _OUT);

            // Cộng tích lũy cho thằng này
            if (_OUT != null)
            {
                _OUT.CollectTotal += 1;
                _OUT.Point += MAXPOINT;
            }

            // Lấy ra tổng vấn đề
            List<GrowAward> TotalGrow = _GrowTreeConfig.GrowAwards.Where(x => x.Quality == Quality).ToList();

            int TotalRate = TotalGrow.Sum(x => x.Rate);

            int RanndomValue = KTGlobal.GetRandomNumber(0, TotalRate);
            int Add = 0;

            GrowAward _SelectItem = null;

            foreach (GrowAward _Item in TotalGrow)
            {
                Add = Add + _Item.Rate;

                if (Add >= RanndomValue)
                {
                    _SelectItem = _Item;
                    break;
                }
            }
            if (_SelectItem != null)
            {
                if (_SelectItem.IsExp)
                {
                    KTPlayerManager.AddExp(client, _SelectItem.ItemNum);
                }
                else if (_SelectItem.IsMoney)
                {
                    KTGlobal.AddMoney(client, _SelectItem.ItemNum, MoneyType.BacKhoa, "GROWTREE");
                }
                else
                {
                    // Nếu là vật phẩm thì add cho nó
                    if (!ItemManager.CreateItem(Global._TCPManager.TcpOutPacketPool, client, _SelectItem.ItemID, _SelectItem.ItemNum, 0, "GROWTREE", false, 1, false, ItemManager.ConstGoodsEndTime))
                    {
                        KTPlayerManager.ShowNotification(client, "Có lỗi khi nhận phần thưởng");
                    }
                }
            }
        }

        /// <summary>
        /// Sự kiện người chơi giết người chơi
        /// </summary>
        /// <param name="Kill"></param>
        /// <param name="BeKill"></param>
        public static void OnDie(GameObject Kill, GameObject BeKill)
        {
            if (_STATE == GROWTREE_STATE.PREDING_OPEN || _STATE == GROWTREE_STATE.OPEN)
            {
                try
                {
                    // Nếu cả 2 thằng đều là người chơi
                    if (Kill is KPlayer && BeKill is KPlayer)
                    {
                        KPlayer kPlayer_Kill = (KPlayer)Kill;

                        KPlayer kPlayer_BeKill = (KPlayer)BeKill;

                        int CurentBeKilLScore = 0;

                        // Lấy ra thằng bị giết
                        GrowTreePlayers.TryGetValue(kPlayer_BeKill.RoleID, out GrowTreePlayer _BeKilL);

                        if (_BeKilL != null)
                        {
                            CurentBeKilLScore = _BeKilL.Point;
                        }

                        int MaxScoreCanGet = GetPointCanGet(CurentBeKilLScore);

                        UpdateScore(kPlayer_Kill, MaxScoreCanGet, 1, false);
                        UpdateScore(kPlayer_BeKill, 0, 0, true);

                        // Cộng điểm cho bang cho thằng này

                        if (kPlayer_Kill.GuildID > 0)
                        {
                            GuildPoint.TryGetValue(kPlayer_Kill.GuildID, out int PointGuild);

                            PointGuild += MaxScoreCanGet / 10;
                        }

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
                                    UpdateScore(member, MaxScoreCanGetByTeam, 1, false);
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
        /// Lấy ra số điểm ăn ké lớn nhất
        /// </summary>
        /// <param name="CurentScore"></param>
        /// <returns></returns>
        public static int GetPointCanGetByTeam(int CurentScore)
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
        /// Lấy ra số điểm lớn nhất thằng này có thể nhận được
        /// </summary>
        /// <param name="CurentScore"></param>
        /// <returns></returns>
        public static int GetPointCanGet(int CurentScore)
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

        public static void UpdateScore(KPlayer _Player, int Score, int KillCount, bool IsRessetStreak)
        {
            int GuildID = _Player.GuildID;

            try
            {
                Console.WriteLine("Update SCORE :" + _Player.RoleName + "| POINT :" + Score);

                GrowTreePlayers.TryGetValue(_Player.RoleID, out GrowTreePlayer find);

                if (find != null)
                {
                    if (find != null)
                    {
                        // Cộng điểm cho thằng người chơi này
                        find.Point = find.Point + Score;

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
                            // Nếu mà số lần đặt kill steak chia hết cho 10 thì thông báo lên kênh bang hội
                            if (find.CurentKillSteak % 10 == 0)
                            {
                                string NOTIFY = "Thành viên bang hội [" + _Player.RoleName + "] đang liên tục lập chiến công tại sự kiện Hạt Hoàng Kim";

                                if (GuildID > 0)
                                {
                                    // Gửi thông báo cho bang hội
                                    KTGlobal.SendGuildChat(GuildID, NOTIFY, null, "");
                                }
                            }
                        }

                        // Lấy ra danh hiệu của người chơi có thể nhận được
                        string Title = GetRankTitleByPoint(find.Point);

                        if (_Player.TempTitle != Title)
                        {
                            //Sét lại danh hiệu cho người chơi
                            _Player.TempTitle = Title;
                        }

                        int Index = -1;

                        //Send lại tích lũy cho thằng này
                        SendNotify(find);
                    }
                }

                Console.WriteLine("Update SCORE DONE :" + _Player.RoleName + "| POINT :" + Score);
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.GuildWarManager, "UpdateScore TOÁC :" + ex.ToString());
            }

            //  Console.WriteLine("Update SCORE :" + _Player.RoleName + "| POINT :" + Score);
        }

        public static string GetRankTitleByPoint(int CurentScore)
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

        /// <summary>
        /// Gửi về kill streak
        /// </summary>
        /// <param name="InputPlayer"></param>
        /// <param name="Count"></param>
        public static void SendKillStreak(KPlayer InputPlayer, int Count)
        {
            G2C_KillStreak _State = new G2C_KillStreak();

            _State.KillNumber = Count;

            if (InputPlayer.IsOnline())
            {
                InputPlayer.SendPacket<G2C_KillStreak>((int)TCPGameServerCmds.CMD_KT_KILLSTREAK, _State);
            }
        }

        public static void CreateTreePoint(GrowTreeModel _Tree)
        {
            GameMap gameMap = KTMapManager.Find(_GrowTreeConfig.MapID);

            GrowPointXML _Config = new GrowPointXML();
            _Config.CollectTick = _Tree.CollectTick;
            _Config.Name = _Tree.Name;
            _Config.ResID = _Tree.ResID;
            _Config.RespawnTime = KTGlobal.GetRandomNumber(_Tree.RespwanTime, _Tree.RespwanTime * 2);
            _Config.InteruptIfTakeDamage = true;
            _Config.ScriptID = -1;

            // Khi thằng này chết thì ta sẽ làm sao
            Action<KPlayer> ActionWork = (x) => CollectComplete(_Tree.Quality, x);

            /// Tạo 1 lửa trại
            GrowPoint growPoint = new GrowPoint()
            {
                ID = KTGrowPointManager.AutoIndexManager.Take() + (int)ObjectBaseID.GrowPoint,
                Data = _Config,
                Name = _Config.Name,
                ObjectType = ObjectTypes.OT_GROWPOINT,
                MapCode = gameMap.MapCode,
                CurrentCopyMapID = -1,
                // Pos này là pos nào
                CurrentPos = new System.Windows.Point(_Tree.PosX, _Tree.PosY),
                // Lấy ra cái girl
                CurrentGrid = new System.Windows.Point(_Tree.PosX / gameMap.MapGridWidth, _Tree.PosY / gameMap.MapGridHeight),
                RespawnTime = _Config.RespawnTime,
                ScriptID = _Config.ScriptID,
                // Thời gian sống 5 phút
                LifeTime = 300000,
                Alive = true,
                ConditionCheck = (player) =>
                {
                    // Cứ tạm retrurn true nếu mà sau có làm giới hạn collect thì làm
                    return true;
                },

                GrowPointCollectCompleted = ActionWork,
            };
            /// Thực hiện tự động xóa
            growPoint.ProcessAutoRemoveTimeout();

            KTGrowPointManager.GrowPoints[growPoint.ID] = growPoint;
            /// Thêm điểm thu thập vào đối tượng quản lý map
            KTGrowPointManager.AddToMap(growPoint);
        }
    }
}