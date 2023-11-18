using GameServer.Logic;
using System.Collections.Generic;
using static GameServer.Logic.KTMapManager;

namespace GameServer.KiemThe.Logic.Manager.Battle
{
    /// <summary>
    /// Class quản lý toàn bộ tống kim
    /// </summary>
    public class Battel_SonJin_Manager
    {
        public static Dictionary<int, Battle_SongJin> _TotalBattle = new Dictionary<int, Battle_SongJin>();

        /// <summary>
        /// Khởi động battle
        /// </summary>
        public static void BattleStatup()
        {
            // Nếu là liên máy chủ
            if (GameManager.IsKuaFuServer)
            {
                Battle_SongJin _Battle_Hight = new Battle_SongJin();
                _Battle_Hight.initialize(3);
                // Call Statup()UpdatePreading
                _Battle_Hight.startup();
                _TotalBattle.Add(5, _Battle_Hight);

                // Khởi tọa chiến trường trung
                Battle_SongJin _Battle_Mid = new Battle_SongJin();
                _Battle_Mid.initialize(2);
                // Call Statup()
                _Battle_Mid.startup();
                _TotalBattle.Add(4, _Battle_Mid);
            }
            else
            {
                // NẾu không phải liên máy chủ
                Battle_SongJin _Battle_Low = new Battle_SongJin();
                _Battle_Low.initialize(1);
                // Call Statup()
                _Battle_Low.startup();
                _TotalBattle.Add(1, _Battle_Low);

                // Khởi tọa chiến trường trung
                Battle_SongJin _Battle_Mid = new Battle_SongJin();
                _Battle_Mid.initialize(2);
                // Call Statup()
                _Battle_Mid.startup();
                _TotalBattle.Add(2, _Battle_Mid);

                Battle_SongJin _Battle_Hight = new Battle_SongJin();
                _Battle_Hight.initialize(3);
                _Battle_Hight.startup();
                _TotalBattle.Add(3, _Battle_Hight);
            }
        }

        /// <summary>
        /// Hàm này gọi khi người chơi rời chiến trường
        /// </summary>
        /// <param name="player"></param>
        public static void OnPlayerLeave(KPlayer player)
        {
            /// Chuyển Camp về -1
            player.Camp = -1;
            /// Chuyển trạng thái PK về hòa bình
            player.PKMode = (int)KiemThe.Entities.PKMode.Peace;
        }

        /// <summary>
        /// Liên máy chủ
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public static bool IsInBattle(KPlayer client)
        {
            if (client.MapCode == 38 || client.MapCode == 72 || client.MapCode == 73 || client.MapCode == 74 || client.MapCode == 75)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static void ShutDown()
        {
            foreach (KeyValuePair<int, Battle_SongJin> entry in _TotalBattle)
            {
                Battle_SongJin PlayerBattle = entry.Value;
                PlayerBattle.showdown();
            }
        }

        /// <summary>
        /// Kiểm tra đăng ký
        /// </summary>
        /// <param name="player"></param>
        /// <param name="Camp"></param>
        /// <param name="Level"></param>
        /// <returns></returns>
        public static int BattleRegister(KPlayer player, int Camp, int Level)
        {
            int BattleLevel = GetBattleLevel(player.m_Level);

            if (BattleLevel != Level)
            {
                return -1000;
            }
            else
            {
                if (BattleLevel != -1)
                {
                    Battle_SongJin _Battle = _TotalBattle[BattleLevel];

                    return _Battle.Register(player, Camp);
                }
                else
                {
                    return -10;
                }
            }
        }
        public static string GetBatteNameCanJoin(int InputLevel)
        {
            foreach (KeyValuePair<int, Battle_SongJin> Battle in _TotalBattle)
            {
                if (Battle.Value.GetMinLevelJoin() <= InputLevel && InputLevel < Battle.Value.GetMaxLevelJoin())
                {

                    return "<color=green>" + Battle.Value.GetBattleName() + "</color>";
                }
            }

            return "<color=red>Không có chiến trường phù hợp</color>";
        }
        public static int GetBattleLevel(int InputLevel)
        {
            foreach (KeyValuePair<int, Battle_SongJin> Battle in _TotalBattle)
            {
                if (Battle.Value.GetMinLevelJoin() <= InputLevel && InputLevel < Battle.Value.GetMaxLevelJoin())
                {

                    return Battle.Key;
                }
            }

            return -1;
        }

        public static SongJinBattleRankingInfo GetRanking(KPlayer _Player)
        {
            SongJinBattleRankingInfo _Ranking = new SongJinBattleRankingInfo();

            int BattleLevel = GetBattleLevel(_Player.m_Level);

            if (BattleLevel != -1)
            {
                Battle_SongJin _Battle = _TotalBattle[BattleLevel];

                return _Battle.RankingBuilder(_Player);
            }

            return _Ranking;
        }

        /// <summary>
        ///  Thực hiện hồi sinh cho player
        /// </summary>
        /// <param name="client"></param>
        public static void Revice(KPlayer client)
        {
            int BattleLevel = GetBattleLevel(client.m_Level);

            if (BattleLevel != -1)
            {
                Battle_SongJin _Battle = _TotalBattle[BattleLevel];
                _Battle.Revice(client);
            }
        }

        #region BattleControler

        public static void ForceStartBattle(int BattleLevel)
        {
            if (BattleLevel != -1)
            {
                Battle_SongJin _Battle = _TotalBattle[BattleLevel];

                _Battle.ForceStartBattle();
            }
        }

        public static void ForceEndBattle(int BattleLevel)
        {
            if (BattleLevel != -1)
            {
                Battle_SongJin _Battle = _TotalBattle[BattleLevel];

                _Battle.ForceEndBattle();
            }
        }

        #endregion BattleControler

        public static string GetNameCamp(int Camp)
        {
            if (Camp == 10)
            {
                return "Tống";
            }
            else if (Camp == 20)
            {
                return "Kim";
            }
            return "";
        }

        public static bool CanUsingTeleport(KPlayer client)
        {
            int BattleLevel = GetBattleLevel(client.m_Level);

            if (BattleLevel != -1)
            {
                Battle_SongJin _Battle = _TotalBattle[BattleLevel];

                return _Battle.CanUsingTeleport();
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Random các vị trí dịch chuyển ra ngoài của Tống
        /// </summary>
        private static readonly List<UnityEngine.Vector2> RandomTeleportPos_Song = new List<UnityEngine.Vector2>()
        {
            new UnityEngine.Vector2(3938, 2260),
            new UnityEngine.Vector2(3378, 3378),
            new UnityEngine.Vector2(3219, 2622),
            new UnityEngine.Vector2(3990, 3990),
            new UnityEngine.Vector2(4054, 2713),
            new UnityEngine.Vector2(3643, 2431),
            new UnityEngine.Vector2(3717, 2812),
            new UnityEngine.Vector2(4263, 2465),
        };

        private static readonly List<UnityEngine.Vector2> RandomTeleportPos_Jin = new List<UnityEngine.Vector2>()
        {
            new UnityEngine.Vector2(10904, 6829),
            new UnityEngine.Vector2(11129, 6409),
            new UnityEngine.Vector2(10461, 6959),
            new UnityEngine.Vector2(10128, 6882),
            new UnityEngine.Vector2(10693, 6181),
            new UnityEngine.Vector2(9749, 6529),
            new UnityEngine.Vector2(9735, 6206),
            new UnityEngine.Vector2(10328, 6051),
        };

        /// <summary>
        /// Sử dụng cổng dịch chuyển
        /// </summary>
        /// <param name="player"></param>
        public static void UseTeleport(KPlayer player)
        {
            UnityEngine.Vector2 randomPos;

            // Nếu như ở bên kim
            if (player.Camp == 10)
            {
                randomPos = Battel_SonJin_Manager.RandomTeleportPos_Song[KTGlobal.GetRandomNumber(0, Battel_SonJin_Manager.RandomTeleportPos_Song.Count - 1)];
            }
            else // Nếu như ở bên tống
            {
                randomPos = Battel_SonJin_Manager.RandomTeleportPos_Jin[KTGlobal.GetRandomNumber(0, Battel_SonJin_Manager.RandomTeleportPos_Jin.Count - 1)];
            }

            /// Dịch chuyển người chơi đến các vị trí ngẫu nhiên
            KTPlayerManager.ChangePos(player, (int)randomPos.x, (int)randomPos.y);

            /// Bất tử
            player.SendChangeMapProtectionBuff();
        }

        public static BattelStatus GetBattleStatus(KPlayer client)
        {
            int BattleLevel = GetBattleLevel(client.m_Level);

            if (BattleLevel != -1)
            {
                Battle_SongJin _Battle = _TotalBattle[BattleLevel];

                return _Battle.GetBattelState();
            }
            else
            {
                return BattelStatus.STATUS_NULL;
            }
        }

        public static void NpcClick(GameMap map, NPC npc, KPlayer client)
        {
            int BattleLevel = GetBattleLevel(client.m_Level);

            if (BattleLevel != -1)
            {
                Battle_SongJin _Battle = _TotalBattle[BattleLevel];

                _Battle.CheckAward(map, npc, client);
            }
        }
    }
}