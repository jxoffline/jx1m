using GameServer.KiemThe.Logic;
using GameServer.Logic;
using System;
using System.Collections.Generic;
using static GameServer.Logic.KTMapManager;

namespace GameServer.KiemThe.CopySceneEvents.DynamicArena
{
    /// <summary>
    /// Script phụ bản lôi đài di động
    /// </summary>
    public static class DynamicArena_EventScript
    {
		/// <summary>
		/// Bắt đầu lôi đài di động
		/// </summary>
		/// <param name="arenaName">Tên lôi đài</param>
		/// <param name="firstTeamPlayers">Danh sách người chơi nhóm 1</param>
		/// <param name="lastTeamPlayers">Danh sách người chơi nhóm 2</param>
		/// <param name="durationTicks">Thời gian tồn tại</param>
		/// <param name="outMapCode">ID bản đồ ra</param>
		/// <param name="outPosX">Vị trí X bản đồ ra</param>
		/// <param name="outPosY">Vị trí Y bản đồ ra</param>
		/// <param name="processBattleResult">Kết quả trận đấu, nếu có nhóm nào thắng thì người chơi bất kỳ trong nhóm đó còn sống trong lôi đài sẽ được truyền vào, nếu hòa thì sẽ là NULL</param>
		public static void Begin(string arenaName, List<KPlayer> firstTeamPlayers, List<KPlayer> lastTeamPlayers, int durationTicks, int outMapCode, int outPosX, int outPosY, Action<KPlayer> processBattleResult)
        {
			/// Danh sách người chơi tham gia
			List<KPlayer> participants = new List<KPlayer>();
			/// Thêm đội 1 vào
			participants.AddRange(firstTeamPlayers);
			/// Thêm đội 2 vào
			participants.AddRange(lastTeamPlayers);

			/// Bản đồ tương ứng
			int mapID = DynamicArena.Data.Map.ID;
			GameMap map = KTMapManager.Find(mapID);
			/// Tạo mới phụ bản
			KTCopyScene copyScene = new KTCopyScene(map, durationTicks)
			{
				AllowReconnect = false,
				EnterPosX = DynamicArena.Data.Map.PosX,
				EnterPosY = DynamicArena.Data.Map.PosY,
				Level = 1,
				Name = arenaName,
				OutMapCode = outMapCode,
				OutPosX = outPosX,
				OutPosY = outPosY,
				ReliveHPPercent = 100,
				ReliveMPPercent = 100,
				ReliveStaminaPercent = 100,
			};
			DynamicArena_Script_Main script = new DynamicArena_Script_Main(copyScene, firstTeamPlayers, lastTeamPlayers)
			{
				ProcessBattleResult = processBattleResult,
			};
			script.Begin(participants);
		}

		/// <summary>
		/// Bắt đầu lôi đài di động
		/// </summary>
		/// <param name="arenaName">Tên lôi đài</param>
		/// <param name="firstTeamPlayers">Danh sách người chơi nhóm 1</param>
		/// <param name="lastTeamPlayers">Danh sách người chơi nhóm 2</param>
		/// <param name="durationTicks">Thời gian tồn tại</param>
		/// <param name="processBattleResult">Kết quả trận đấu, nếu có nhóm nào thắng thì người chơi bất kỳ trong nhóm đó còn sống trong lôi đài sẽ được truyền vào, nếu hòa thì sẽ là NULL</param>
		public static void Begin(string arenaName, List<KPlayer> firstTeamPlayers, List<KPlayer> lastTeamPlayers, int durationTicks, Action<KPlayer> processBattleResult)
        {
			DynamicArena_EventScript.Begin(arenaName, firstTeamPlayers, lastTeamPlayers, durationTicks, -1, -1, -1, processBattleResult);

		}
    }
}
