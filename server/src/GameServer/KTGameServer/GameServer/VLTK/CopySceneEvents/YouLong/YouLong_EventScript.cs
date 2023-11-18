using GameServer.KiemThe.Core.Item;
using GameServer.KiemThe.Logic;
using GameServer.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using static GameServer.Logic.KTMapManager;

namespace GameServer.KiemThe.CopySceneEvents.YouLongGe
{
	/// <summary>
	/// Script phụ bản Du Long Các
	/// </summary>
	public static class YouLong_EventScript
	{
		/// <summary>
		/// Kiểm tra điều kiện mở phụ bản Du Long Các
		/// </summary>
		/// <param name="player"></param>
		public static string CheckCondition(KPlayer player)
		{
			/// Nếu cấp độ không đủ
			if (player.m_Level < YouLong.Data.RequireLevel)
			{
				return string.Format("Yêu cầu cấp độ <color=green>{0}</color> trở lên mới có thể mở Du Long Các.", YouLong.Data.RequireLevel);
			}
			/// Nếu không có Chiến Thư
			else if (ItemManager.GetItemCountInBag(player, YouLong.Data.RequireItem) <= 0)
			{
				return "Không có Chiến Thư, không thể vào Du Long Các!";
			}
			/// Nếu đã tham gia trong ngày rồi thì thôi
			else if (CopySceneEventManager.GetCopySceneTotalEnterTimesToday(player, DailyRecord.YouLong) >= YouLong.Data.LimitRoundPerDay)
			{
				return "Ngươi đã tham gia đủ số lượt trong ngày, không thể tham gia nữa!";
			}
			/// Nếu số lượng phụ bản đã đạt tối đa
			else if (CopySceneEventManager.CurrentCopyScenesCount >= CopySceneEventManager.LimitCopyScenes)
			{
				return "Số lượng phụ bản đã đạt giới hạn, hãy thử lại lúc khác!";
			}

			/// Trả ra kết quả OK
			return "OK";
		}

		/// <summary>
		/// Bắt đầu phụ bản Du Long Các
		/// </summary>
		/// <param name="player"></param>
		public static void Begin(KPlayer player)
		{
			/// Nếu lỗi gì đó
			if (player == null)
			{
				return;
			}

			/// Xóa vật phẩm Chiến Thư Du Long
			if (!ItemManager.RemoveItemFromBag(player, YouLong.Data.RequireItem, 1,-1,"YouLong"))
			{
				return;
			}

			/// Cấp độ phụ bản
			int level = player.m_Level;

			/// Bản đồ tương ứng
			int mapID = YouLong.Data.Map.ID;
			GameMap map = KTMapManager.Find(mapID);
			/// Tạo mới phụ bản
			KTCopyScene copyScene = new KTCopyScene(map, YouLong.Data.Duration)
			{
				AllowReconnect = false,
				EnterPosX = YouLong.Data.Map.EnterPosX,
				EnterPosY = YouLong.Data.Map.EnterPosY,
				Level = level,
				Name = "Du Long Các",
				OutMapCode = player.CurrentMapCode,
				OutPosX = player.PosX,
				OutPosY = player.PosY,
				ReliveHPPercent = 100,
				ReliveMPPercent = 100,
				ReliveStaminaPercent = 100,
				ReliveMapCode = player.CurrentMapCode,
				RelivePosX = player.PosX,
				RelivePosY = player.PosY,
			};
			/// Bắt đầu phụ bản
			YouLong_Script_Main script = new YouLong_Script_Main(copyScene);
			script.Begin(new List<KPlayer>() { player });
		}
	}
}
