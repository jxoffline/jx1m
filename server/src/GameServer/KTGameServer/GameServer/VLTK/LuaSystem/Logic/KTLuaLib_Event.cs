using GameServer.KiemThe.CopySceneEvents.MiJingFuBen;
using GameServer.KiemThe.GameEvents.TeamBattle;
using GameServer.KiemThe.CopySceneEvents.YouLongGe;
using GameServer.KiemThe.GameEvents.BaiHuTang;
using GameServer.KiemThe.Logic;
using GameServer.KiemThe.LuaSystem.Entities;
using GameServer.Server;
using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Text;
using GameServer.KiemThe.GameEvents.CargoCarriage;
using GameServer.KiemThe.CopySceneEvents.MilitaryCampFuBen;
using GameServer.KiemThe.GameEvents.FengHuoLianCheng;
using GameServer.KiemThe.GameEvents.EmperorTomb;
using GameServer.KiemThe.CopySceneEvents.ShenMiBaoKu;

namespace GameServer.KiemThe.LuaSystem.Logic
{
    /// <summary>
    /// Cung cấp thư viện dùng cho Lua, liên quan đến sự kiện, hoạt động
    /// </summary>
    [MoonSharpUserData]
	public static class KTLuaLib_Event
	{
		#region Sự kiện đặc biệt
		#region Bạch Hổ Đường
		/// <summary>
		/// Có phải thời gian báo danh Bạch Hổ Đường không
		/// </summary>
		/// <returns></returns>
		public static bool IsBaiHuTangRegisterTime()
		{
			return BaiHuTang.BeginRegistered;
		}

		/// <summary>
		/// Kiểm tra người chơi đã tham gia Bạch Hổ Đường hôm nay chưa
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
		public static bool BaiHuTang_HasCompletedToday(Lua_Player player)
		{
			return BaiHuTang_ActivityScript.BaiHuTang_HasCompletedToday(player.RefObject);
		}

		/// <summary>
		/// Thiết lập đánh dấu đã tham gia Bạch Hổ Đường ngày hôm nay
		/// </summary>
		/// <param name="player"></param>
		public static void BaiHuTang_SetEnteredToday(Lua_Player player)
		{
			BaiHuTang_ActivityScript.BaiHuTang_SetEnteredToday(player.RefObject);
		}
		#endregion

		#region Bí cảnh
		/// <summary>
		/// Kiểm tra điều kiện vào Bí Cảnh
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
		public static string MiJing_CheckCondition(Lua_Player player)
		{
			return MiJing_EventScript.CheckCondition(player.RefObject);
		}

		/// <summary>
		/// Bắt đầu bí cảnh
		/// </summary>
		/// <param name="player"></param>
		public static void MiJing_Begin(Lua_Player player)
		{
			MiJing_EventScript.Begin(player.RefObject);
		}
		#endregion

		#region Du Long Các
		/// <summary>
		/// Kiểm tra điều kiện vào Du Long Các
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
		public static string YouLong_CheckCondition(Lua_Player player)
		{
			return YouLong_EventScript.CheckCondition(player.RefObject);
		}

		/// <summary>
		/// Bắt đầu Du Long Các
		/// </summary>
		/// <param name="player"></param>
		public static void YouLong_Begin(Lua_Player player)
		{
			YouLong_EventScript.Begin(player.RefObject);
		}
		#endregion

		#region Thần Bí Bảo Khố
		/// <summary>
		/// Trả về danh sách thành viên bang có thể tham gia Thần Bí Bảo Khố
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
		public static string ShenMiBaoKu_GetNearByGuildMembersToJoinEventDescription(Lua_Player player)
		{
			return ShenMiBaoKu_EventScript.GetGuildMembersState(player.RefObject);
		}

		/// <summary>
		/// Kiểm tra điều kiện tham gia Thần Bí Bảo Khố
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
		public static string ShenMiBaoKu_CheckCondition(Lua_Player player)
		{
			return ShenMiBaoKu_EventScript.CheckCondition(player.RefObject);
		}

		/// <summary>
		/// Bắt đầu Thần Bí Bảo Khố
		/// </summary>
		/// <param name="player"></param>
		public static void ShenMiBaoKu_Begin(Lua_Player player)
		{
			ShenMiBaoKu_EventScript.Begin(player.RefObject);
		}

		/// <summary>
		/// Kiểm tra điều kiện sử dụng Câu Hồn Ngọc trong Thần Bí Bảo Khố
		/// </summary>
		/// <param name="player"></param>
		public static string ShenMiBaoKu_UseCallBossItem_CheckCondition(Lua_Player player)
		{
			return ShenMiBaoKu_EventScript.UseCallBossItem_CheckCondition(player.RefObject);
		}

		/// <summary>
		/// Kết thúc sử dụng Câu Hồn Ngọc trong Thần Bí Bảo Khố
		/// </summary>
		/// <param name="player"></param>
		/// <param name="bossID"></param>
		public static void ShenMiBaoKu_FinishUsingCallBossItem(Lua_Player player, int bossID)
		{
			ShenMiBaoKu_EventScript.UseCallBossItem(player.RefObject, bossID);
		}
		#endregion

		#region Võ lâm liên đấu
		/// <summary>
		/// Trả về loại Võ lâm liên đấu trong tháng này
		/// </summary>
		/// <returns></returns>
		public static string TeamBattle_GetCurrentMonthTeamBattleType()
        {
			return TeamBattle_ActivityScript.GetCurrentMonthTeamBattleType();
        }

		/// <summary>
		/// Kiểm tra có phải thời gian diễn ra Võ lâm liên đấu không
		/// </summary>
		/// <returns></returns>
		public static bool TeamBattle_IsRegisterTime()
        {
			return TeamBattle_ActivityScript.IsRegisterTime();
        }

		/// <summary>
		/// Kiểm tra người chơi đã có chiến đội đăng ký tham gia Võ lâm liên đấu chưa
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
		public static bool TeamBattle_IsRegistered(Lua_Player player)
        {
			return TeamBattle_ActivityScript.GetTeamInfo(player.RefObject) != null;
        }

		/// <summary>
		/// Trả về thông tin chiến đội bản thân
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
		public static string TeamBattle_GetMyTeamInfo(Lua_Player player)
        {
			StringBuilder builder = new StringBuilder();
			builder.AppendLine("Thông tin chiến đội bản thân:");
			TeamBattle.TeamBattleInfo teamInfo = TeamBattle_ActivityScript.GetTeamInfo(player.RefObject);
			/// Nếu tồn tại
			if (teamInfo != null)
            {
				/// Tên nhóm
				builder.AppendLine(string.Format("  - Chiến đội: <color=yellow><b>{0}</b></color>", teamInfo.Name));
				/// Danh sách thành viên
				builder.AppendLine(string.Format("  - Thành viên: <color=#52d4ff>{0}</color>", string.Join("<color=white>,</color> ", teamInfo.Members.Values)));
				/// Thời gian thành lập
				builder.AppendLine(string.Format("  - Thời gian thành lập: <color=green>{0}</color>", teamInfo.RegisterTime.ToString("HH:mm - dd/MM/yyyy")));
				/// Tổng số trận đấu đã tham dự
				builder.AppendLine(string.Format("  - Tổng số trận đã đấu: <color=#ff61c2>{0} trận</color>", teamInfo.TotalBattles));
				/// Tổng điểm đạt được
				builder.AppendLine(string.Format("  - Tổng điểm: <color=#ffb833>{0} điểm</color>", teamInfo.Point));
				/// Bậc thi đấu của chiến đội
				builder.AppendLine(string.Format("  - Bậc thi đấu: <color=#0afffb>{0}</color>", teamInfo.Stage));
				/// Thời gian thắng trận lần cuối
				if (teamInfo.LastWinTime != DateTime.MinValue)
                {
					builder.AppendLine(string.Format("  - Thắng trận cuối: <color=#0afffb>{0}</color>", teamInfo.LastWinTime.ToString("HH:mm - dd/MM/yyyy")));
				}
                else
                {
					builder.AppendLine(string.Format("  - Thắng trận cuối: <color=#0afffb>{0}</color>", "Chưa có"));
				}
				/// Nếu chưa cập nhật xếp hạng
				if (teamInfo.Rank == 0 || teamInfo.LastUpdateRankTime == DateTime.MinValue)
                {
					/// Xếp hạng
					builder.AppendLine(string.Format("  - Xếp hạng: <color=#c247ff>{0}</color>", "Chưa cập nhật"));
				}
                else
                {
					/// Xếp hạng
					builder.AppendLine(string.Format("  - Xếp hạng: <color=#c247ff>{0}</color>", teamInfo.Rank));
					/// Thời gian cập nhật xếp hạng
					builder.AppendLine(string.Format("  - Thời gian cập nhật: <color=#ffa31a>{0}</color>", teamInfo.LastUpdateRankTime.ToString("HH:mm - dd/MM/yyyy")));
				}
            }
			/// Chưa tồn tại
            else
            {
				builder.AppendLine("Chưa có thông tin.");
            }
			return builder.ToString();
        }

		/// <summary>
		/// Tạo nhóm đăng ký tham gia Võ lâm liên đấu
		/// </summary>
		/// <param name="player"></param>
		/// <param name="teamName"></param>
		/// <returns></returns>
		public static string TeamBattle_CreateTeam(Lua_Player player, string teamName)
        {
			return TeamBattle_ActivityScript.CreateTeam(player.RefObject, teamName);
        }

		/// <summary>
		/// Kiểm tra hôm nay có diễn ra Võ lâm liên đấu không
		/// </summary>
		/// <returns></returns>
		public static bool TeamBattle_IsBattleTimeToday()
        {
			return TeamBattle_ActivityScript.IsBattleTimeToday();
		}

		/// <summary>
		/// Chuyển người chơi đến bản đồ hội trường Võ lâm liên đấu
		/// </summary>
		/// <param name="player"></param>
		public static void TeamBattle_MoveToBattleHall(Lua_Player player)
        {
			TeamBattle_ActivityScript.MoveToBattleHall(player.RefObject);
        }

		/// <summary>
		/// Trả về bậc của trận đấu kế tiếp
		/// </summary>
		/// <returns></returns>
		public static int TeamBattle_GetNextBattleStage()
        {
			return TeamBattle_ActivityScript.GetNextBattleStage();
        }

		/// <summary>
		/// Trả về thời gian diễn ra Võ lâm liên đấu gần nhất tính từ hiện tại
		/// </summary>
		/// <returns></returns>
		public static string TeamBattle_GetNextBattleTime()
        {
			/// Mốc thời gian gần nhất
			DateTime? eventTime = TeamBattle_ActivityScript.GetNextBattleTime();
            /// Nếu không có kết quả
            if (eventTime == null)
            {
                return "FAILED";
            }
			/// Trả về kết quả
			return eventTime.Value.ToString("HH:mm - dd/MM/yyyy");
        }

		/// <summary>
		/// Đăng ký tham chiến trận đấu tiếp theo Võ lâm liên đấu
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
		public static string TeamBattle_RegisterForNextBattle(Lua_Player player)
        {
			return TeamBattle_ActivityScript.RegisterForBattle(player.RefObject);
        }

		/// <summary>
		/// Kiểm tra chiến đội đã đăng ký trận đấu tiếp theo Võ lâm liên đấu chưa
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
		public static bool TeamBattle_IsRegisteredForNextBattle(Lua_Player player)
        {
			return TeamBattle_ActivityScript.IsRegisteredForBattle(player.RefObject);
        }

		/// <summary>
		/// Trả về tổng số chiến đội trong Võ lâm liên đấu đã báo danh trận đấu kế tiếp
		/// </summary>
		/// <returns></returns>
		public static int TeamBattle_GetTotalRegisteredForNextBattleTeams()
        {
			return TeamBattle_ActivityScript.GetTotalRegisteredTeams();
		}

		/// <summary>
		/// Kiểm tra chiến đội bản thân trong Võ lâm liên đấu có phần thưởng để nhận không
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
		public static bool TeamBattle_IsHavingAwards(Lua_Player player)
        {
			return TeamBattle_ActivityScript.IsHavingAwards(player.RefObject, out _);
        }

		/// <summary>
		/// Nhận phần thưởng chiến đội bản thân trong Võ lâm liên đấu
		/// </summary>
		/// <param name="player"></param>
		public static string TeamBattle_GetAwards(Lua_Player player)
        {
			return TeamBattle_ActivityScript.GetAwards(player.RefObject);
        }

		/// <summary>
		/// Truy vấn thông tin Top chiến đội trong Võ lâm liên đấu
		/// </summary>
		/// <param name="player"></param>
		public static void TeamBattle_QueryTopTeam(Lua_Player player)
        {
			/// Nếu thao tác quá nhanh
			if (KTGlobal.GetCurrentTimeMilis() - player.RefObject.LastQueryTeamBattleTicks < 1000)
            {
				KTPlayerManager.ShowNotification(player.RefObject, "Thao tác quá nhanh, hãy thử lại sau giây lát!");
				return;
            }
			/// Đánh dấu thời điểm cập nhật
			player.RefObject.LastQueryTeamBattleTicks = KTGlobal.GetCurrentTimeMilis();

			/// Kết quả
			List<TeamBattle.TeamBattleInfo> teamBattles = TeamBattle_ActivityScript.GetTopTeams();
			/// Toác
			if (teamBattles == null)
            {
				KTPlayerManager.ShowNotification(player.RefObject, "Bảng xếp hạng chưa được cập nhật!");
				return;
			}

			/// Tạo gói tin gửi về Client
			player.RefObject.SendPacket<List<TeamBattle.TeamBattleInfo>>((int) TCPGameServerCmds.CMD_DB_TEAMBATTLE, teamBattles);
        }
		#endregion

		#region Tần Lăng
		/// <summary>
		/// Kiểm tra điều kiện tiến vào Tần lăng
		/// </summary>
		/// <returns></returns>
		public static string EmperorTomb_EnterMap_CheckCondition(Lua_Player player)
		{
			return EmperorTomb_ActivityScript.EnterMap_CheckCondition(player.RefObject);
		}

		/// <summary>
		/// Dịch chuyển người chơi vào Tần Lăng
		/// </summary>
		/// <param name="player"></param>
		public static void EmperorTomb_MoveToMap(Lua_Player player)
		{
			EmperorTomb_ActivityScript.MoveToEmperorTomb(player.RefObject);
		}
		#endregion

		#region Quân doanh
		/// <summary>
		/// Kiểm tra điều kiện tham gia Quân doanh
		/// </summary>
		/// <param name="player"></param>
		/// <param name="index"></param>
		/// <returns></returns>
		public static string MilitaryCamp_CheckCondition(Lua_Player player, int index)
        {
            return MilitaryCamp_EventScript.CheckCondition(player.RefObject, index);
        }

        /// <summary>
        /// Bắt đầu Quân doanh
        /// </summary>
        /// <param name="player"></param>
        /// <param name="index"></param>
        public static void MilitaryCamp_Begin(Lua_Player player, int index)
        {
            MilitaryCamp_EventScript.Begin(player.RefObject, index);
        }
        #endregion

        #region Vận tiêu
        /// <summary>
        /// Nhận nhiệm vụ vận tiêu tương ứng
        /// </summary>
        /// <param name="player"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string CargoCarriage_GiveTask(Lua_Player player, int type)
        {
			return CargoCarriage_ActivityScript.GiveTask(player.RefObject, type);
        }
        #endregion

        #region Phong Hỏa Liên Thành
		/// <summary>
		/// Kiểm tra người chơi tương ứng có phần quà sự kiện Phong Hỏa Liên Thành lần trước chưa nhận không
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
        public static bool FengHuoLianCheng_HasAward(Lua_Player player)
        {
			return FengHuoLianCheng_ActivityScript.HasAward(player.RefObject);
        }

		/// <summary>
		/// Nhận phần thưởng sự kiện Phong Hỏa Liên Thành lần trước
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
		public static string FengHuoLianCheng_GetAward(Lua_Player player)
        {
			return FengHuoLianCheng_ActivityScript.GetAward(player.RefObject);
		}

		/// <summary>
		/// Kiểm tra có phải thời điểm báo danh sự kiện Phong Hỏa Liên Thành không
		/// </summary>
		/// <returns></returns>
		public static bool FengHuoLianCheng_IsRegistrationTime()
        {
			return FengHuoLianCheng_ActivityScript.IsRegistrationTime();
        }

		/// <summary>
		/// Báo danh sự kiện Phong Hỏa Liên Thành
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
		public static string FengHuoLianCheng_Register(Lua_Player player)
        {
			return FengHuoLianCheng_ActivityScript.RegisterEvent(player.RefObject);
		}

		/// <summary>
		/// Đưa người chơi đến khu vực chuẩn bị chiến trường Phong Hỏa Liên Thành
		/// </summary>
		/// <param name="player"></param>
		public static void FengHuoLianCheng_BringToBattleOutpost(Lua_Player player)
        {
			FengHuoLianCheng_ActivityScript.BringToBattleOutpost(player.RefObject);
        }
        #endregion
        #endregion
    }
}
