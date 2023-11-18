using GameServer.KiemThe.Entities;
using GameServer.Logic;
using GameServer.Server;
using Server.Data;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Xml.Serialization;
using static GameServer.Logic.KTMapManager;

namespace GameServer.KiemThe.GameEvents.FactionBattle
{
    /// <summary>
    /// Quản lý thi đấu môn phái
    /// </summary>
    public class FactionBattleManager
    {
        public bool IsActive = false;

        /// <summary>
        /// Toàn bộ các trận thi đấu môn phái đang diễn ra
        /// </summary>
        public static ConcurrentDictionary<int, FactionBattle> TotalBattle = new ConcurrentDictionary<int, FactionBattle>();

        /// <summary>
        /// Config thi đấu môn phái
        /// </summary>
        public static string Battle_Config = "Config/KT_Battle/Faction_Battle.xml";

        /// <summary>
        /// Hàm này gọi trước khi người chơi chuyển sang bản đồ khác
        /// </summary>
        /// <param name="player"></param>
        public static void OnPlayerLeave(KPlayer client, GameMap map)
        {
            int FactionID = client.m_cPlayerFaction.GetFactionId();

            TotalBattle.TryGetValue(FactionID, out FactionBattle FindBattle);

            if (FindBattle != null)
            {
                int MAPCODE = FindBattle.GetMapCode;
                if (map.MapCode != MAPCODE)
                {
                    //XÓA STATE KHI RỜI KHỎI
                    PlayChangeState(client, 0);
                    client.PKMode = (int)PKMode.Peace;
                    client.ForbidUsingSkill = false;
                    client.Camp = -1;
                    client.StopAllActiveFights();
                }
            }
        }

        public static void PlayChangeState(KPlayer Player, int State)
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

        /// <summary>
        /// Xử lý sự kiện hồi sinh khi đang trong bản đồ thi đấu môn phái
        /// </summary>
        /// <param name="client"></param>
        public static void Revice(KPlayer client)
        {
            int FactionID = client.m_cPlayerFaction.GetFactionId();

            TotalBattle.TryGetValue(FactionID, out FactionBattle FindBattle);
            if (FindBattle != null)
            {
                FindBattle.Revice(client);
            }
        }

        /// <summary>
        /// Trả về client bảng xếp hạng khi người chơi ấn vào nút bảng xếp hạng
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public static FACTION_PVP_RANKING_INFO GetRanking(KPlayer client)
        {
            FACTION_PVP_RANKING_INFO _Ranking = new FACTION_PVP_RANKING_INFO();

            int FactionID = client.m_cPlayerFaction.GetFactionId();

            TotalBattle.TryGetValue(FactionID, out FactionBattle FindBattle);
            if (FindBattle != null)
            {
                _Ranking = FindBattle.RankingBuilder(client);
            }

            return _Ranking;
        }

        /// <summary>
        /// Kiểm tra xem người chơi có đang trong thi đấu môn phái ko
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static bool IsInFactionBattle(KPlayer player)
        {
            if ((player.MapCode >= 80 && player.MapCode <= 89) || player.MapCode == 71)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Lưu lại config
        /// </summary>
        public static FactionDef _BattleDef = new FactionDef();

        /// <summary>
        ///  Event xử lý sự kiện tham gia thi đấu môn phái khi click vào NPC
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static int FactionBattleJoin(KPlayer player)
        {
            if (player.m_cPlayerFaction.GetFactionId() == 0)
            {
                return -6;
            }
            else
            {
                int FactionID = player.m_cPlayerFaction.GetFactionId();

                TotalBattle.TryGetValue(FactionID, out FactionBattle FindBattle);
                if (FindBattle != null)
                {
                    return FindBattle.Register(player);
                }
                else
                {
                    return -1;
                }
            }
        }

        public static void FactionLogout(KPlayer player)
        {
            if (player.m_cPlayerFaction.GetFactionId() == 0)
            {
                return;
            }
            else
            {
                int FactionID = player.m_cPlayerFaction.GetFactionId();

                TotalBattle.TryGetValue(FactionID, out FactionBattle FindBattle);
                if (FindBattle != null)
                {
                    FindBattle.OnLogout(player);
                }
                else
                {
                    return;
                }
            }
        }

        public static void NpcClick(GameMap map, NPC npc, KPlayer client)
        {
            int FactionID = client.m_cPlayerFaction.GetFactionId();

            TotalBattle.TryGetValue(FactionID, out FactionBattle FindBattle);
            if (FindBattle != null)
            {
                FindBattle.NpcQuanQuan(map, npc, client);
            }
        }

        public static void DamgeRecore(GameObject HIT, GameObject BEHIT, int Dagme)
        {
            if (HIT is KPlayer && BEHIT is KPlayer)
            {
                int FactionID = ((KPlayer)HIT).m_cPlayerFaction.GetFactionId();

                TotalBattle.TryGetValue(FactionID, out FactionBattle FindBattle);

                ///Xử lý sự kiện khi nhân vật chết
                if (FindBattle != null)
                {
                    if (HIT.CurrentMapCode == FindBattle.GetMapCode && FindBattle.GetMapCode == BEHIT.CurrentMapCode)
                    {
                        FindBattle.OnHitTaget(HIT as KPlayer, Dagme);
                    }
                }
            }
        }

        /// <summary>
        /// Gọi hàm xử lý chết ở FACTNIONBATTE
        /// </summary>
        /// <param name="Kill"></param>
        /// <param name="BeKill"></param>
        public static void OnDie(GameObject Kill, GameObject BeKill)
        {
            if (Kill is KPlayer && BeKill is KPlayer)
            {
                int FactionID = ((KPlayer)Kill).m_cPlayerFaction.GetFactionId();

                TotalBattle.TryGetValue(FactionID, out FactionBattle FindBattle);

                ///Xử lý sự kiện khi nhân vật chết
                if (FindBattle != null)
                {
                    FindBattle.OnKillRole(Kill as KPlayer, BeKill as KPlayer);
                }
            }
        }

        public static void FinishCollectFlag(KPlayer player, int ResID)
        {
            int FactionID = player.m_cPlayerFaction.GetFactionId();

            TotalBattle.TryGetValue(FactionID, out FactionBattle FindBattle);
            if (FindBattle != null)
            {
                FindBattle.ClickFlag(player, ResID);
            }
        }

        /// <summary>
        ///  Sự kiện shutdown thi đấu môn phái khi thi đấu môn phái
        /// </summary>
        public static void ShutDown()
        {
            foreach (FactionBattle _Battle in TotalBattle.Values)
            {
                _Battle.showdown();
            }
        }

        #region GMCTR

        /// <summary>
        /// Lệnh start chiến trường = lệnh GM
        /// </summary>
        public static void ForceStartBattle()
        {
            foreach (FactionBattle _Faction in TotalBattle.Values)
            {
                _Faction.ForceStartBattle();
            }
        }

        /// <summary>
        /// Lệnh tắt chiến trường = lệnh GM
        /// </summary>
        public static void ForceEndBattle()
        {
            foreach (FactionBattle _Faction in TotalBattle.Values)
            {
                _Faction.ForceEndBattle();
            }
        }

        #endregion GMCTR

        /// <summary>
        /// Gọi sự kiện stảt chiến trường khi GS chạy
        /// </summary>
        public static void BattleStatup()
        {
            string Files = KTGlobal.GetDataPath(Battle_Config);

            using (var stream = System.IO.File.OpenRead(Files))
            {
                var serializer = new XmlSerializer(typeof(FactionDef));
                _BattleDef = serializer.Deserialize(stream) as FactionDef;
            }

            Start();
        }

        /// <summary>
        /// Start toàn bộ các bản đồ có sự kiện thi đấu môn phái
        /// </summary>
        public static void Start()
        {
            List<FactionMap> MapFactionList = _BattleDef.MapFactionList;

            foreach (FactionMap Map in MapFactionList)
            {
                FactionBattle _Battle = new FactionBattle(Map.FactionID, _BattleDef);

                _Battle.startup();

                TotalBattle.TryAdd(Map.FactionID, _Battle);
            }
        }
    }
}