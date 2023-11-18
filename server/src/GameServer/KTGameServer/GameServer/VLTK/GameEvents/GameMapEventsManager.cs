using GameServer.KiemThe.GameEvents.Interface;
using GameServer.KiemThe.GameEvents.Model;
using GameServer.KiemThe.Logic;
using GameServer.Logic;
using Server.Tools;
using System;
using System.Collections.Concurrent;
using System.Linq;
using static GameServer.Logic.KTMapManager;

namespace GameServer.KiemThe.GameEvents
{
    /// <summary>
    /// Lớp quản lý các hoạt động trong Game
    /// </summary>
    public static partial class GameMapEventsManager
    {
        #region Define
        /// <summary>
        /// Danh sách các hoạt động
        /// </summary>
        private static readonly ConcurrentDictionary<int, GameMapEvent> GameMapEvents = new ConcurrentDictionary<int, GameMapEvent>();
        #endregion

        #region Core
        /// <summary>
        /// Khởi tạo
        /// </summary>
        public static void Init()
        {
            /// Bạch Hổ Đường
            BaiHuTang.BaiHuTang.Init();
            /// Võ lâm liên đấu
            TeamBattle.TeamBattle.Init();
            /// Đoán hoa đăng
            KnowledgeChallenge.KnowledgeChallenge.Init();
            /// Tần Lăng
            EmperorTomb.EmperorTomb.Init();
            /// Vận tiêu
            CargoCarriage.CargoCarriage.Init();
            /// Phong hỏa liên thành
            FengHuoLianCheng.FengHuoLianCheng.Init();
            /// Sự kiện đặc biệt
            SpecialEvent.SpecialEvent.Init();
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Thêm hoạt động tương ứng vào danh sách
        /// </summary>
        /// <param name="gameMapEvent"></param>
        public static void Add(GameMapEvent gameMapEvent)
		{
            GameMapEventsManager.GameMapEvents[gameMapEvent.Map.MapCode] = gameMapEvent;
		}

        /// <summary>
        /// Xóa hoạt động tương ứng khỏi danh sách
        /// </summary>
        /// <param name="gameMapEvent"></param>
        public static void Remove(GameMapEvent gameMapEvent)
		{
            GameMapEventsManager.GameMapEvents.TryRemove(gameMapEvent.Map.MapCode, out _);
		}

        /// <summary>
        /// Trả về Script điều khiển hoạt động tương ứng
        /// </summary>
        /// <param name="activityID"></param>
        /// <returns></returns>
        public static IActivityScript GetActivityScript(int activityID)
        {
            if (GameMapEventsManager.activityScripts.TryGetValue(activityID, out IActivityScript script))
            {
                return script;
            }
            return null;
        }

        /// <summary>
        /// Trả về Script điều khiển hoạt động tương ứng
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="activityID"></param>
        /// <returns></returns>
        public static T GetActivityScript<T>(int activityID) where T : IActivityScript
        {
            if (GameMapEventsManager.activityScripts.TryGetValue(activityID, out IActivityScript script))
            {
                if (script is T)
                {
                    return (T) script;
                }
                return default;
            }
            return default;
        }
		#endregion

		#region Events
        /// <summary>
        /// Sự kiện khi người chơi vào bản đồ hoạt động
        /// </summary>
        /// <param name="map"></param>
        /// <param name="player"></param>
        public static void OnPlayerEnter(KPlayer player)
		{
            /// Nếu tồn tại
            if (GameMapEventsManager.GameMapEvents.TryGetValue(player.CurrentMapCode, out GameMapEvent script))
			{
                try
                {
                    /// Thực thi sự kiện tương ứng
                    script.OnPlayerEnter(player);
                }
                catch (Exception ex)
                {
                    LogManager.WriteLog(LogTypes.Exception, ex.ToString());
                }
			}
			else
			{
                /// Kiểm tra các bản đồ sự kiện
                GameMapEventsManager.CheckSpecialActivityMap(KTMapManager.Find(player.CurrentMapCode), player);
			}
		}

        /// <summary>
        /// Sự kiện khi người chơi rời khỏi bản đồ hoạt động
        /// </summary>
        /// <param name="player"></param>
        /// <param name="toMap"></param>
        public static void OnPlayerLeave(KPlayer player, GameMap toMap)
		{
            /// Nếu tồn tại
            if (GameMapEventsManager.GameMapEvents.TryGetValue(player.CurrentMapCode, out GameMapEvent script))
			{
                try
                {
                    /// Thực thi sự kiện tương ứng
                    script.OnPlayerLeave(player, toMap);
                }
                catch (Exception ex)
                {
                    LogManager.WriteLog(LogTypes.Exception, ex.ToString());
                }
            }
		}

        /// <summary>
        /// Sự kiện khi người chơi giết đối tượng khác
        /// </summary>
        /// <param name="player"></param>
        /// <param name="obj"></param>
        public static void OnKillObject(KPlayer player, GameObject obj)
		{
            /// Nếu tồn tại
            if (GameMapEventsManager.GameMapEvents.TryGetValue(player.CurrentMapCode, out GameMapEvent script))
            {
                try
                {
                    /// Thực thi sự kiện tương ứng
                    script.OnKillObject(player, obj);
                }
                catch (Exception ex)
                {
                    LogManager.WriteLog(LogTypes.Exception, ex.ToString());
                }
            }
        }

        /// <summary>
        /// Sự kiện khi người chơi ấn nút về thành
        /// </summary>
        /// <param name="map"></param>
        /// <param name="player"></param>
        /// <returns>False thì sẽ lấy điểm về thành mặc định</returns>
        public static bool OnPlayerClickReliveButton(KPlayer player)
		{
            /// Nếu tồn tại
            if (GameMapEventsManager.GameMapEvents.TryGetValue(player.CurrentMapCode, out GameMapEvent script))
            {
                try
                {
                    /// Thực thi sự kiện tương ứng
                    return script.OnPlayerClickReliveButton(player);
                }
                catch (Exception ex)
                {
                    LogManager.WriteLog(LogTypes.Exception, ex.ToString());
                }
                return false;
            }
            /// Trả về False
            return false;
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Kiểm tra các bản đồ sự kiện đặc biệt
        /// </summary>
        /// <param name="map"></param>
        /// <param name="player"></param>
        private static void CheckSpecialActivityMap(GameMap map, KPlayer player)
		{
            #region Bạch Hổ Đường
            /// Nếu là bản đồ Bạch Hổ Đường 1
            if (BaiHuTang.BaiHuTang.Round1.Activities.Any(x => x.Value.Maps.Contains(map.MapCode)))
            {
                /// Nếu không phải thời gian hoạt động
                if (BaiHuTang.BaiHuTang.CurrentStage != 1)
                {
                    /// Thông báo
                    KTPlayerManager.ShowNotification(player, "Hiện không phải thời gian Bạch Hổ Đường!");
                    /// Đưa người chơi rời khỏi bản đồ hoạt động
                    KTPlayerManager.ChangeMap(player, BaiHuTang.BaiHuTang.Round1.OutMaps[map.MapCode].OutMapID, BaiHuTang.BaiHuTang.Round1.OutMaps[map.MapCode].OutPosX, BaiHuTang.BaiHuTang.Round1.OutMaps[map.MapCode].OutPosY);
                    return;
                }
            }
            /// Nếu là bản đồ Bạch Hổ Đường 2
            else if (BaiHuTang.BaiHuTang.Round2.Activities.Any(x => x.Value.Maps.Contains(map.MapCode)))
            {
                /// Nếu không phải thời gian hoạt động
                if (BaiHuTang.BaiHuTang.CurrentStage != 3)
                {
                    /// Thông báo
                    KTPlayerManager.ShowNotification(player, "Hiện không phải thời gian Bạch Hổ Đường!");
                    /// Đưa người chơi rời khỏi bản đồ hoạt động
                    KTPlayerManager.ChangeMap(player, BaiHuTang.BaiHuTang.Round2.OutMaps[map.MapCode].OutMapID, BaiHuTang.BaiHuTang.Round2.OutMaps[map.MapCode].OutPosX, BaiHuTang.BaiHuTang.Round2.OutMaps[map.MapCode].OutPosY);
                    return;
                }
            }
            /// Nếu là bản đồ Bạch Hổ Đường 3
            else if (BaiHuTang.BaiHuTang.Round3.Activities.Any(x => x.Value.Maps.Contains(map.MapCode)))
            {
                /// Nếu không phải thời gian hoạt động
                if (BaiHuTang.BaiHuTang.CurrentStage != 4)
                {
                    /// Thông báo
                    KTPlayerManager.ShowNotification(player, "Hiện không phải thời gian Bạch Hổ Đường!");
                    /// Đưa người chơi rời khỏi bản đồ hoạt động
                    KTPlayerManager.ChangeMap(player, BaiHuTang.BaiHuTang.Round3.OutMaps[map.MapCode].OutMapID, BaiHuTang.BaiHuTang.Round3.OutMaps[map.MapCode].OutPosX, BaiHuTang.BaiHuTang.Round3.OutMaps[map.MapCode].OutPosY);
                    return;
                }
            }
            #endregion

            #region Phong Hỏa Liên Thành
            /// Nếu là bản đồ Phong Hỏa Liên Thành
            if (map.MapCode == FengHuoLianCheng.FengHuoLianCheng.Data.Map.EnterMapID && !FengHuoLianCheng.FengHuoLianCheng.IsRegisterTime)
            {
                /// Thông báo
                KTPlayerManager.ShowNotification(player, "Hiện không phải thời gian sự kiện Phong Hỏa Liên Thành!");
                /// Đưa người chơi rời khỏi bản đồ hoạt động
                KTPlayerManager.ChangeMap(player, FengHuoLianCheng.FengHuoLianCheng.Data.Map.CityMapID, FengHuoLianCheng.FengHuoLianCheng.Data.Map.CityPosX, FengHuoLianCheng.FengHuoLianCheng.Data.Map.CityPosY);
            }
            #endregion
        }
        #endregion
    }
}
