using GameServer.KiemThe.CopySceneEvents.Model;
using GameServer.KiemThe.Entities;
using GameServer.KiemThe.Logic;
using GameServer.Logic;
using GameServer.VLTK.Core.GuildManager;
using System.Collections.Generic;
using System.Text;
using static GameServer.Logic.KTMapManager;

namespace GameServer.KiemThe.CopySceneEvents.ShenMiBaoKu
{
    /// <summary>
    /// Script phụ bản Thần Bí Bảo Khố
    /// </summary>
    public static class ShenMiBaoKu_EventScript
    {
        #region Core
        /// <summary>
        /// Trả về số lượt đã tham gia phụ bản trong tuần của người chơi tương ứng
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        private static int GetTotalParticipatedThisWeek(KPlayer player)
        {
            int value = player.GetValueOfWeekRecore((int) WeekRecord.ShenMiBaoKu_TotalParticipated);
            if (value == -1)
            {
                value = 0;
            }
            return value;
        }

        /// <summary>
        /// Thiết lập số lượt đã tham gia phụ bản trong tuần của người chơi tương ứng
        /// </summary>
        /// <param name="player"></param>
        /// <param name="value"></param>
        private static void SetTotalParticipatedThisWeek(KPlayer player, int value)
        {
            player.SetValueOfWeekRecore((int) WeekRecord.ShenMiBaoKu_TotalParticipated, value);
        }

        /// <summary>
        /// Trả về tổng số lượt đã mở phụ bản của bang hội tương ứng
        /// </summary>
        /// <param name="guildID"></param>
        /// <returns></returns>
        private static int GetGuildTotalCopySceneThisWeek(int guildID)
        {
            int value = GuildManager.GetTotal_Copy_Scenes_This_Week(guildID);
            if (value == -1)
            {
                value = 0;
            }
            return value;
        }

        /// <summary>
        /// Thiết lập tổng số lượt đã mở phụ bản của bang hội tương ứng
        /// </summary>
        /// <param name="guildID"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private static void SetGuildTotalCopySceneThisWeek(int guildID, int value)
        {
            GuildManager.SetTotal_Copy_Scenes_This_Week(guildID, value);
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Kiểm tra điều kiện để vào phụ bản
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static string CheckCondition(KPlayer player)
        {
            /// Nếu không có bang
            if (player.GuildID <= 0)
            {
                return "Ngươi không có bang hội, không thể mở <color=yellow>Thần Bí Bảo Khố</color>.";
            }
            /// Nếu không phải bang chủ hoặc phó bang chủ
            else if (player.GuildRank != (int) GuildRank.Master && player.GuildRank != (int) GuildRank.ViceMaster)
            {
                return "Chỉ có bang chủ hoặc phó bang chủ mới có quyền mở <color=yellow>Thần Bí Bảo Khố</color>.";
            }

            ///// Tổng số lượt đã tham gia trong tuần
            //int totalParticipated = ShenMiBaoKu_EventScript.GetTotalParticipatedThisWeek(player);
            ///// Nếu đã tham gia quá số lượt
            //if (totalParticipated >= ShenMiBaoKu.Data.Config.LimitRoundPerWeek)
            //{
            //    return string.Format("Ngươi đã tham gia quá <color=green>{0} lượt</color> tuần này. Tuần tới hãy quay lại", ShenMiBaoKu.Data.Config.LimitRoundPerWeek);
            //}

            /// Số lượt phụ bản đã mở của toàn bang
            int totalOpenedCopyScenes = ShenMiBaoKu_EventScript.GetGuildTotalCopySceneThisWeek(player.GuildID);
            /// Nếu đã quá số lượt
            if (totalOpenedCopyScenes >= ShenMiBaoKu.Data.Config.LimitRoundPerWeek)
            {
                return string.Format("Tuần này bang hội đã mở <color=green>{0} lượt</color> <color=yellow>Thần Bí Bảo Khố</color>. Tuần tới hãy quay lại.", ShenMiBaoKu.Data.Config.LimitRoundPerWeek);
            }

            /// Nếu cấp độ không đủ
            if (player.m_Level < ShenMiBaoKu.Data.Config.MinLevel)
            {
                return string.Format("Ngươi cần đạt cấp độ tối thiểu <color=green>{0} cấp</color> mới có thể mở <color=yellow>Thần Bí Bảo Khố</color>.", ShenMiBaoKu.Data.Config.MinLevel);
            }

            /// Nếu số lượng phụ bản đã đạt tối đa
            if (CopySceneEventManager.CurrentCopyScenesCount >= CopySceneEventManager.LimitCopyScenes)
            {
                return "Số lượng phụ bản đã đạt giới hạn, hãy thử lại lúc khác!";
            }

            /// OK
            return "OK";
        }

        /// <summary>
        /// Danh sách thông tin tham gia phụ bản của thành viên bang hội xung quanh
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static string GetGuildMembersState(KPlayer player)
        {
            /// Trả về danh sách người chơi đủ điều kiện tham gia phụ bản
            StringBuilder builder = new StringBuilder();

            /// Danh sách thành viên xung quanh
            List<KPlayer> players = KTGlobal.GetNearByPlayers(player, 1000);
            /// Duyệt danh sách
            foreach (KPlayer targetPlayer in players)
            {
                /// Nếu không phải thành viên bang hội
                if (targetPlayer.GuildID != player.GuildID)
                {
                    /// Bỏ qua
                    continue;
                }

                string roleNameStr = string.Format("<color=#24b6ff>[{0}]</color>", targetPlayer.RoleName);
                string guildRankStr = string.Format("<color=#c979ec>{0}</color>", KTGlobal.GetGuildRankName(player.GuildRank));
                string totalParticipatedStr;
                string levelStr;
                    
                /// Tổng số lượt đã tham gia trong tuần
                int totalParticipated = ShenMiBaoKu_EventScript.GetTotalParticipatedThisWeek(targetPlayer);
                /// Nếu đã tham gia quá số lượt
                if (totalParticipated >= ShenMiBaoKu.Data.Config.LimitRoundPerWeek)
                {
                    totalParticipatedStr = string.Format("<color=red>{0}</color>", totalParticipated);
                }
                /// Nếu chưa tham gia quá số lượt
                else
                {
                    totalParticipatedStr = string.Format("<color=#00f58f>{0}</color>", totalParticipated);
                }

                /// Nếu cấp độ không đủ
                if (player.m_Level < ShenMiBaoKu.Data.Config.MinLevel)
                {
                    levelStr = string.Format("<color=red>{0}</color>", player.m_Level);
                }
                /// Nếu chưa tham gia quá số lượt
                else
                {
                    levelStr = string.Format("<color=#00f58f>{0}</color>", player.m_Level);
                }

                /// Thêm Text
                builder.AppendLine(string.Format("{0} ({1}) - Cấp {2} - Số lượt đã tham gia {3}", roleNameStr, guildRankStr, totalParticipatedStr, levelStr));
            }

            /// Trả về kết quả
            return builder.ToString();
        }

        /// <summary>
        /// Bắt đầu Thần Bí Bảo Khố
        /// </summary>
        /// <param name="player"></param>
        public static void Begin(KPlayer player)
        {
            /// Nếu không có bang
            if (player.GuildID <= 0)
            {
                KTPlayerManager.ShowNotification(player, "Ngươi không có bang hội, không thể mở <color=yellow>Thần Bí Bảo Khố</color>.");
                return;
            }
            /// Nếu không phải bang chủ hoặc phó bang chủ
            else if (player.GuildRank != (int) GuildRank.Master && player.GuildRank != (int) GuildRank.ViceMaster)
            {
                KTPlayerManager.ShowNotification(player, "Chỉ có bang chủ hoặc phó bang chủ mới có quyền mở <color=yellow>Thần Bí Bảo Khố</color>.");
                return;
            }

            /// Tổng số lượt đã tham gia trong tuần
            int totalParticipated = ShenMiBaoKu_EventScript.GetTotalParticipatedThisWeek(player);
            /// Nếu đã tham gia quá số lượt
            if (totalParticipated >= ShenMiBaoKu.Data.Config.LimitRoundPerWeek)
            {
                KTPlayerManager.ShowNotification(player, string.Format("Ngươi đã tham gia quá <color=green>{0} lượt</color> tuần này. Tuần tới hãy quay lại", ShenMiBaoKu.Data.Config.LimitRoundPerWeek));
                return;
            }

            /// Số lượt phụ bản đã mở của toàn bang
            int totalOpenedCopyScenes = ShenMiBaoKu_EventScript.GetGuildTotalCopySceneThisWeek(player.GuildID);
            /// Nếu đã quá số lượt
            if (totalOpenedCopyScenes >= ShenMiBaoKu.Data.Config.LimitRoundPerWeek)
            {
                KTPlayerManager.ShowNotification(player, string.Format("Tuần này bang hội đã mở <color=green>{0} lượt</color> <color=yellow>Thần Bí Bảo Khố</color>. Tuần tới hãy quay lại.", ShenMiBaoKu.Data.Config.LimitRoundPerWeek));
                return;
            }

            /// Nếu cấp độ không đủ
            if (player.m_Level < ShenMiBaoKu.Data.Config.MinLevel)
            {
                KTPlayerManager.ShowNotification(player, string.Format("Ngươi cần đạt cấp độ tối thiểu <color=green>{0} cấp</color> mới có thể mở <color=yellow>Thần Bí Bảo Khố</color>.", ShenMiBaoKu.Data.Config.MinLevel));
                return;
            }

            /// Nếu số lượng phụ bản đã đạt tối đa
            if (CopySceneEventManager.CurrentCopyScenesCount >= CopySceneEventManager.LimitCopyScenes)
            {
                KTPlayerManager.ShowNotification(player, "Số lượng phụ bản đã đạt giới hạn, hãy thử lại lúc khác!");
                return;
            }

            /// Danh sách thành viên bang hội tham gia sự kiện
            List<KPlayer> guildMates = new List<KPlayer>();

            /// Danh sách thành viên xung quanh
            List<KPlayer> players = KTGlobal.GetNearByPlayers(player, 1000);
            /// Duyệt danh sách
            foreach (KPlayer targetPlayer in players)
            {
                /// Nếu không phải thành viên bang hội
                if (targetPlayer.GuildID != player.GuildID)
                {
                    /// Bỏ qua
                    continue;
                }

                /// Tổng số lượt đã tham gia trong tuần
                int _totalParticipated = ShenMiBaoKu_EventScript.GetTotalParticipatedThisWeek(targetPlayer);
                /// Nếu đã tham gia quá số lượt
                if (_totalParticipated >= ShenMiBaoKu.Data.Config.LimitRoundPerWeek)
                {
                    continue;
                }
                /// Nếu cấp độ không đủ
                else if (player.m_Level < ShenMiBaoKu.Data.Config.MinLevel)
                {
                    continue;
                }

                /// Thêm vào danh sách
                guildMates.Add(targetPlayer);
            }

            /// Nếu không có thành viên
			if (guildMates.Count <= 0)
            {
                KTPlayerManager.ShowNotification(player, "Không có thành viên, không thể mở <color=yellow>Thần Bí Bảo Khố</color>!");
                return;
            }

            /// Cấp độ phụ bản
            int copySceneLevel = 0;

            /// Tăng số lượt tham gia cho toàn thể thành viên
			foreach (KPlayer guildMate in guildMates)
            {
                /// Số lượt đã tham gia trong tuần
                int nTimes = ShenMiBaoKu_EventScript.GetTotalParticipatedThisWeek(guildMate);
                /// Tăng số lượt lên
                nTimes++;
                /// Lưu lại kết quả
                ShenMiBaoKu_EventScript.SetTotalParticipatedThisWeek(guildMate, nTimes);

                /// Tăng cấp phụ bản
                copySceneLevel += guildMate.m_Level;
            }

            /// Chia trung bình
            copySceneLevel /= guildMates.Count;
            /// Nếu dưới ngưỡng thì thiết lập lại
            if (copySceneLevel < ShenMiBaoKu.Data.Config.MinLevel)
            {
                copySceneLevel = ShenMiBaoKu.Data.Config.MinLevel;
            }

            /// Số lượt phụ bản đã mở tuần này
			int totalCopySceneThisWeek = ShenMiBaoKu_EventScript.GetGuildTotalCopySceneThisWeek(player.GuildID);
            /// Tăng số lượt
            totalCopySceneThisWeek++;
            /// Ghi lại kết quả
            ShenMiBaoKu_EventScript.SetGuildTotalCopySceneThisWeek(player.GuildID, totalCopySceneThisWeek);

            /// Bản đồ tương ứng
			int mapID = ShenMiBaoKu.Data.Map.MapID;
            GameMap map = KTMapManager.Find(mapID);
            /// Tạo mới phụ bản
            KTCopyScene copyScene = new KTCopyScene(map, ShenMiBaoKu.Data.Config.Duration)
            {
                AllowReconnect = true,
                EnterPosX = ShenMiBaoKu.Data.Map.EnterPosX,
                EnterPosY = ShenMiBaoKu.Data.Map.EnterPosY,
                Level = copySceneLevel,
                Name = "Thần Bí Bảo Khố",
                OutMapCode = player.CurrentMapCode,
                OutPosX = player.PosX,
                OutPosY = player.PosY,
                ReliveHPPercent = 100,
                ReliveMPPercent = 100,
                ReliveStaminaPercent = 100,
                ReliveMapCode = mapID,
                RelivePosX = ShenMiBaoKu.Data.Map.EnterPosX,
                RelivePosY = ShenMiBaoKu.Data.Map.EnterPosY,
            };
            /// Script điều khiển phụ bản
            ShenMiBaoKu_Script_Main script = new ShenMiBaoKu_Script_Main(copyScene);
            /// Bắt đầu phụ bản
            script.Begin(guildMates);
        }

        /// <summary>
		/// Kiểm tra điều kiện sử dụng Câu Hồn Ngọc
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
		public static string UseCallBossItem_CheckCondition(KPlayer player)
        {
            /// Script điều khiển phụ bản tương ứng
            CopySceneEvent script = CopySceneEventManager.GetCopySceneScript(player.CurrentCopyMapID, player.CurrentMapCode);
            /// Nếu không tồn tại
            if (script == null)
            {
                return "Đạo cụ này chỉ có thể sử dụng trong phụ bản Thần Bí Bảo Khố!";
            }
            /// Nếu không phải phụ bản Vượt ải gia tộc
            else if (!(script is ShenMiBaoKu_Script_Main))
            {
                return "Đạo cụ này chỉ có thể sử dụng trong phụ bản Thần Bí Bảo Khố!";
            }

            /// Script tương ứng
            ShenMiBaoKu_Script_Main smbkScript = (script as ShenMiBaoKu_Script_Main);
            /// Kiểm tra điều kiện
            return smbkScript.UseCallBossItem_CheckCondition(player);
        }

        /// <summary>
        /// Sử dụng Câu Hồn Ngọc
        /// </summary>
        /// <param name="player"></param>
        /// <param name="bossID"></param>
        public static void UseCallBossItem(KPlayer player, int bossID)
        {
            /// Script điều khiển phụ bản tương ứng
            CopySceneEvent script = CopySceneEventManager.GetCopySceneScript(player.CurrentCopyMapID, player.CurrentMapCode);
            /// Nếu không tồn tại
            if (script == null)
            {
                KTPlayerManager.ShowNotification(player, "Đạo cụ này chỉ có thể sử dụng trong phụ bản Thần Bí Bảo Khố!");
                return;
            }
            /// Nếu không phải phụ bản Vượt ải gia tộc
            else if (!(script is ShenMiBaoKu_Script_Main))
            {
                KTPlayerManager.ShowNotification(player, "Đạo cụ này chỉ có thể sử dụng trong phụ bản Thần Bí Bảo Khố!");
                return;
            }

            /// Script tương ứng
            ShenMiBaoKu_Script_Main smbkScript = (script as ShenMiBaoKu_Script_Main);
            /// Thực hiện triệu hồi
            smbkScript.UseCallBossItem(player, bossID);
        }
        #endregion
    }
}
