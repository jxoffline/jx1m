using GameServer.KiemThe.Core.Item;
using GameServer.KiemThe.Logic;
using GameServer.Logic;
using System.Collections.Generic;
using System.Linq;
using static GameServer.Logic.KTMapManager;

namespace GameServer.KiemThe.CopySceneEvents.MiJingFuBen
{
	/// <summary>
	/// Script phụ bản Bí Cảnh
	/// </summary>
	public static class MiJing_EventScript
	{
		/// <summary>
		/// Kiểm tra điều kiện mở bí cảnh
		/// </summary>
		/// <param name="player"></param>
		public static string CheckCondition(KPlayer player)
		{
			/// Nếu không có nhóm
			if (player.TeamID == -1 || !KTTeamManager.IsTeamExist(player.TeamID))
			{
				return "Bạn cần tạo nhóm trước tiên!";
			}
			/// Nếu không phải đội trưởng
			else if (player.TeamLeader != null && player.TeamLeader != player)
			{
				return "Chỉ đội trưởng mới có thể thao tác!";
			}
			/// Nếu cấp độ không đủ
			else if (player.m_Level < MiJing.RequireLevel)
			{
				return string.Format("Yêu cầu cấp độ <color=green>{0}</color> trở lên mới có thể mở Bí Cảnh.", MiJing.RequireLevel);
			}
			/// Nếu không có bản đồ bí cảnh
			else if (ItemManager.GetItemCountInBag(player, MiJing.MapItem) <= 0)
			{
				return "Không có Bản Đồ Bí Cảnh, không thể tạo bí cảnh!";
			}
			/// Nếu đã tham gia trong ngày rồi thì thôi
			else if (CopySceneEventManager.GetCopySceneTotalEnterTimesToday(player, DailyRecord.MiJing) >= MiJing.LimitRoundPerDay)
			{
				return "Bạn đã tham gia đủ số lượt trong ngày, không thể tham gia nữa!";
			}
			/// Nếu số lượng Bí cảnh đã đạt tối đa
			else if (CopySceneEventManager.GetTotalCopyScene<MiJing_Script_Main>() >= MiJing.LimitCopyScenes)
            {
				return "Số lượng bí cảnh đang mở đã đạt giới hạn, hãy thử lại lúc khác!";
			}
			/// Nếu số lượng phụ bản đã đạt tối đa
			else if (CopySceneEventManager.CurrentCopyScenesCount >= CopySceneEventManager.LimitCopyScenes)
			{
				return "Số lượng phụ bản đã đạt giới hạn, hãy thử lại lúc khác!";
			}

			List<KPlayer> teammates = player.Teammates;

			/// Kiểm tra xem có thành viên không trong khu vực
			foreach (KPlayer teammate in teammates)
			{
				/// Nếu khác bản đồ
				if (teammate.MapCode != player.MapCode)
				{
					return "Hãy tập hợp đủ thành viên tới chỗ ta trước!";
				}
			}

			/// Danh sách đội viên không đủ cấp
			List<string> notEnoughLevelPlayers = new List<string>();
			/// Kiểm tra nhóm
			foreach (KPlayer teammate in teammates)
			{
				/// Nếu đã tham gia rồi
				if (teammate.m_Level < MiJing.RequireLevel)
				{
					notEnoughLevelPlayers.Add(string.Format("<color=#4dbeff>[{0}]</color>", teammate.RoleName));
				}
			}

			/// Nếu tồn tại danh sách đội viên không đủ cấp
			if (notEnoughLevelPlayers.Count > 0)
			{
				return string.Format("Trong tổ đội có {0} cấp độ không đủ <color=green>{1}</color>, không thể tham gia!", string.Join(", ", notEnoughLevelPlayers), MiJing.RequireLevel);
			}

			/// Danh sách đội viên đã tham gia quá số lượt trong ngày
			List<string> alreadyAttempMaxRoundTodayPlayers = new List<string>();
			/// Kiểm tra nhóm
			foreach (KPlayer teammate in teammates)
			{
				/// Nếu đã tham gia rồi
				if (CopySceneEventManager.GetCopySceneTotalEnterTimesToday(teammate, DailyRecord.MiJing) >= MiJing.LimitRoundPerDay)
				{
					alreadyAttempMaxRoundTodayPlayers.Add(string.Format("<color=#4dbeff>[{0}]</color>", teammate.RoleName));
				}
			}

			/// Nếu tồn tại danh sách đội viên đã tham gia quá số lượt trong ngày
			if (alreadyAttempMaxRoundTodayPlayers.Count > 0)
			{
				return string.Format("Trong tổ đội có {0} đã tham gia quá số lượt trong ngày!", string.Join(", ", alreadyAttempMaxRoundTodayPlayers));
			}

			/// Trả ra kết quả OK
			return "OK";
		}

		/// <summary>
		/// Bắt đầu mở bí cảnh
		/// </summary>
		/// <param name="player"></param>
		public static void Begin(KPlayer player)
		{
			/// Nếu lỗi gì đó
			if (player == null)
			{
				return;
			}

			/// Xóa vật phẩm bản đồ bí cảnh
			if (!ItemManager.RemoveItemFromBag(player, MiJing.MapItem, 1,-1,"Bí Cảnh"))
			{
				return;
			}

			List<KPlayer> teammates = player.Teammates;

			/// Duyệt danh sách thành viên, đánh dấu đã tham gia ngày hôm nay
			foreach (KPlayer teammate in teammates)
			{
				int totalEnterTimes = CopySceneEventManager.GetCopySceneTotalEnterTimesToday(teammate, DailyRecord.MiJing);
				totalEnterTimes++;
				CopySceneEventManager.SetCopySceneTotalEnterTimesToday(teammate, DailyRecord.MiJing, totalEnterTimes);
			}

			/// Cấp độ trung bình của các thành viên trong nhóm
			int totalLevel = 0;
			int totalMembers = 0;
			foreach (KPlayer teammate in teammates)
			{
				totalMembers++;
				totalLevel += teammate.m_Level;
			}
			/// Cấp độ trung bình
			int nLevel = totalLevel / totalMembers;

			/// Bản đồ tương ứng
			int mapID = MiJing.Map.ID;
			GameMap map = KTMapManager.Find(mapID);
			/// Tạo mới phụ bản
			KTCopyScene copyScene = new KTCopyScene(map, MiJing.Duration)
			{
				AllowReconnect = true,
				EnterPosX = MiJing.Map.EnterPosX,
				EnterPosY = MiJing.Map.EnterPosY,
				Level = nLevel,
				Name = "Bí cảnh",
				OutMapCode = player.CurrentMapCode,
				OutPosX = player.PosX,
				OutPosY = player.PosY,
				ReliveHPPercent = 20,
				ReliveMPPercent = 20,
				ReliveStaminaPercent = 20,
				ReliveMapCode = MiJing.Map.ID,
				RelivePosX = MiJing.Map.EnterPosX,
				RelivePosY = MiJing.Map.EnterPosY,
			};
			MiJing_Script_Main script = new MiJing_Script_Main(copyScene);
			script.Begin(teammates);
		}
	}
}
