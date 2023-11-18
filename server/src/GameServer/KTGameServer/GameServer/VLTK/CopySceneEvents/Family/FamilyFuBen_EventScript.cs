using GameServer.KiemThe.CopySceneEvents.Model;
using GameServer.KiemThe.Entities;
using GameServer.KiemThe.Logic;
using GameServer.Logic;
using System.Collections.Generic;
using System.Text;

namespace GameServer.KiemThe.CopySceneEvents.Family
{
    /// <summary>
    /// Script phụ bản Vượt ải gia tộc
    /// </summary>
    public static class FamilyFuBen_EventScript
    {
        /// <summary>
        /// Đã mở phụ bản Vượt ải gia tộc chưa
        /// </summary>
        public const bool FamilyFuBen_Open = true;

        /// <summary>
        /// Trả về tổng số lượt đã tham gia vượt ải trong tuần của người chơi tương ứng
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        private static int GetTotalParticipatedTimesThisWeek(KPlayer player)
        {
            int nTimes = player.GetValueOfWeekRecore((int)WeeklyActivityRecord.FamilyFuBen_TotalParticipatedTimes);
            if (nTimes < 0)
            {
                nTimes = 0;
            }
            return nTimes;
        }

        /// <summary>
        /// Trả về số lượt đã mở vượt ải gia tộc tuần này của tộc ID tương ứng
        /// </summary>
        /// <param name="familyID"></param>
        /// <returns></returns>
        private static int GetTotalOpenedFuBenThisWeek(int familyID)
        {
            return 0;
        }

        /// <summary>
        /// Thiết lập số lượt đã mở vượt ải gia tộc tuần này của tộc ID tương ứng
        /// </summary>
        /// <param name="familyID"></param>
        /// <param name="nTimes"></param>
        private static void SetTotalOpenedFuBenThisWeek(int familyID, int nTimes)
        {

        }

        /// <summary>
        /// Trả về thông tin các thành viên trong tộc ở bản đồ hiện tại đủ điêu kiện tham gia Vượt ải gia tộc
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static string GetNearByFamilyMembersToJoinEventDescription(KPlayer player)
        {
            /// Kết quả
            StringBuilder result = new StringBuilder();
            /// Danh sách thành viên trong tộc xung quanh
            List<KPlayer> familyMembers = KTLogic.GetNearByPlayers(player, 1500);
            /// Duyệt danh sách thành viên
            foreach (KPlayer nearByPlayer in familyMembers)
            {
                /// Nếu không cùng gia tộc
                if (player.FamilyID != nearByPlayer.FamilyID)
                {
                    continue;
                }
                /// Tổng số lượt đã tham gia
                int nTimes = FamilyFuBen_EventScript.GetTotalParticipatedTimesThisWeek(nearByPlayer);
                /// Nếu thỏa mãn điều kiện tham gia đủ số lượt và đủ cấp độ
                if (nTimes < FamilyFuBen.Config.MaxParticipatedTimesPerWeek && nearByPlayer.m_Level >= FamilyFuBen.Config.MinLevel)
                {
                    /// Thông tin tương ứng
                    result.AppendLine(string.Format("{0} (Cấp: {1}, Đã tham gia trong tuần: <color=green>{2} lượt</color>)", nearByPlayer.RoleName, nearByPlayer.m_Level, nTimes));
                }
                /// Nếu không thỏa mãn điều kiện
                else
                {
                    /// Thông tin tương ứng
                    result.AppendLine(string.Format("<s>{0}</s> <color=red>(Cấp: {1}, Đã tham gia trong tuần: {2} lượt)</color>", nearByPlayer.RoleName, nearByPlayer.m_Level, nTimes));
                }
            }
            /// Trả về kết quả
            return result.ToString();
        }

        /// <summary>
        /// Kiểm tra có đủ điều kiện tham gia Vượt ải gia tộc không
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static string CheckCondition(KPlayer player)
        {
            /// Nếu chưa mở phụ bản
            if (!FamilyFuBen_EventScript.FamilyFuBen_Open)
            {
                return "Phụ bản Vượt ải gia tộc hiện chưa mở, hãy quay lại sau!";
            }

            /// Nếu không có tộc
            if (player.FamilyID <= 0)
            {
                return "Ngươi chưa gia nhập gia tộc, không thể tham gia vượt ải!";
            }
            /// Nếu không phải tộc trưởng
            else if (player.FamilyRank != (int)FamilyRank.Master)
            {
                return "Chỉ có tộc trưởng mới có thể khai mở vượt ải gia tộc!";
            }
            /// Nếu cấp độ không đủ
            else if (player.m_Level < FamilyFuBen.Config.MinLevel)
            {
                return string.Format("Vượt ải gia tộc yêu cầu cấp độ thành viên tối thiểu cấp {0} mới được tham gia!", FamilyFuBen.Config.MinLevel);
            }
            /// Nếu đã tham gia quá số lượt
            else if (FamilyFuBen_EventScript.GetTotalParticipatedTimesThisWeek(player) >= FamilyFuBen.Config.MaxParticipatedTimesPerWeek)
            {
                return "Ngươi đã tham gia vượt ải gia tộc quá số lượt cho phép trong tuần, hãy quay lại vào tuần tới!";
            }
            /// Nếu tộc đã mở quá số lượng phụ bản trong tuần
            else if (FamilyFuBen_EventScript.GetTotalOpenedFuBenThisWeek(player.FamilyID) >= FamilyFuBen.Config.MaxParticipatedTimesPerWeek)
            {
                return "Tộc này đã mở quá số lượt phụ bản cho phép trong tuần, hãy quay lại vào tuần tới!";
            }
            /// Nếu số lượng phụ bản đã đạt tối đa
            else if (CopySceneEventManager.CurrentCopyScenesCount >= CopySceneEventManager.LimitCopyScenes)
            {
                return "Số lượng phụ bản đã đạt giới hạn, hãy thử lại lúc khác!";
            }

            /// Danh sách thành viên thỏa mãn điều kiện được tham gia
            List<KPlayer> familyMembers = new List<KPlayer>();
            /// Duyệt danh sách thành viên
            foreach (KPlayer nearByPlayer in KTLogic.GetNearByPlayers(player, 1500))
            {
                /// Nếu không cùng gia tộc
                if (player.FamilyID != nearByPlayer.FamilyID)
                {
                    continue;
                }

                /// Tổng số lượt đã tham gia
                int nTimes = FamilyFuBen_EventScript.GetTotalParticipatedTimesThisWeek(nearByPlayer);
                /// Nếu thỏa mãn điều kiện tham gia đủ số lượt và đủ cấp độ
                if (nTimes < FamilyFuBen.Config.MaxParticipatedTimesPerWeek && nearByPlayer.m_Level >= FamilyFuBen.Config.MinLevel)
                {
                    /// Thêm vào danh sách
                    familyMembers.Add(nearByPlayer);
                }
            }

            /// Nếu không có thành viên
            if (familyMembers.Count < 5)
            {
                return "Cần có tối thiểu 5 thành viên thỏa mãn điều kiện đứng quanh đây mới có thể tham gia vượt ải gia tộc!";
            }

            /// Trả về kết quả có thể tham gia
            return "OK";
        }

        /// <summary>
        /// Bắt đầu phụ bản Vượt ải gia tộc
        /// </summary>
        /// <param name="player"></param>
        public static void Begin(KPlayer player)
        {
            /// Danh sách thành viên thỏa mãn điều kiện được tham gia
            List<KPlayer> familyMembers = new List<KPlayer>();
            /// Duyệt danh sách thành viên
            foreach (KPlayer nearByPlayer in KTLogic.GetNearByPlayers(player, 1500))
            {
                /// Nếu không cùng gia tộc
                if (player.FamilyID != nearByPlayer.FamilyID)
                {
                    continue;
                }

                /// Tổng số lượt đã tham gia
                int nTimes = FamilyFuBen_EventScript.GetTotalParticipatedTimesThisWeek(nearByPlayer);
                /// Nếu thỏa mãn điều kiện tham gia đủ số lượt và đủ cấp độ
                if (nTimes < FamilyFuBen.Config.MaxParticipatedTimesPerWeek && nearByPlayer.m_Level >= FamilyFuBen.Config.MinLevel)
                {
                    /// Thêm vào danh sách
                    familyMembers.Add(nearByPlayer);
                }
            }

            /// Nếu không có thành viên
            if (familyMembers.Count <= 0)
            {
                PlayerManager.ShowNotification(player, "Không có thành viên, không thể tham gia Vượt ải gia tộc!");
                return;
            }

            /// Tăng số lượt tham gia Vượt ải gia tộc cho toàn thể thành viên
            foreach (KPlayer familyMember in familyMembers)
            {
                /// Số lượt đã tham gia trong tuần
                int nTimes = FamilyFuBen_EventScript.GetTotalParticipatedTimesThisWeek(familyMember);
                /// Tăng số lượt lên
                nTimes++;
                /// Lưu lại kết quả
                familyMember.SetValueOfWeekRecore((int)WeeklyActivityRecord.FamilyFuBen_TotalParticipatedTimes, nTimes);
            }

            /// Số lượt tộc đã mở vượt ải tuần này
            int currentWeekFubenCount = FamilyFuBen_EventScript.GetTotalOpenedFuBenThisWeek(player.FamilyID);
            /// Tăng số lượt
            currentWeekFubenCount++;
            /// Ghi lại kết quả
            FamilyFuBen_EventScript.SetTotalOpenedFuBenThisWeek(player.FamilyID, currentWeekFubenCount);

            /// Bản đồ tương ứng
            int mapID = FamilyFuBen.Map.MapID;
            GameMap map = GameManager.MapMgr.GetGameMap(mapID);
            /// Tạo mới phụ bản
            KTCopyScene copyScene = new KTCopyScene(map, FamilyFuBen.Config.Duration + FamilyFuBen.Config.PrepareTime + FamilyFuBen.Config.FinishWaitTime)
            {
                AllowReconnect = true,
                EnterPosX = FamilyFuBen.Map.EnterPosX,
                EnterPosY = FamilyFuBen.Map.EnterPosY,
                Level = 80,
                Name = "Vượt ải gia tộc",
                OutMapCode = player.CurrentMapCode,
                OutPosX = player.PosX,
                OutPosY = player.PosY,
                ReliveHPPercent = 100,
                ReliveMPPercent = 100,
                ReliveStaminaPercent = 100,
                ReliveMapCode = mapID,
                RelivePosX = FamilyFuBen.Map.EnterPosX,
                RelivePosY = FamilyFuBen.Map.EnterPosY,
            };
            /// Script điều khiển phụ bản
            FamilyFuBen_Script_Main script = new FamilyFuBen_Script_Main(copyScene);
            /// Bắt đầu phụ bản
            script.Begin(familyMembers);
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
                return "Đạo cụ này chỉ có thể sử dụng trong phụ bản Vượt ải gia tộc!";
            }
            /// Nếu không phải phụ bản Vượt ải gia tộc
            else if (!(script is FamilyFuBen_Script_Main))
            {
                return "Đạo cụ này chỉ có thể sử dụng trong phụ bản Vượt ải gia tộc!";
            }

            /// Script tương ứng
            FamilyFuBen_Script_Main familyFuBenScript = (script as FamilyFuBen_Script_Main);
            /// Kiểm tra điều kiện
            return familyFuBenScript.UseCallBossItem_CheckCondition(player);
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
                PlayerManager.ShowNotification(player, "Đạo cụ này chỉ có thể sử dụng trong phụ bản Vượt ải gia tộc!");
                return;
            }
            /// Nếu không phải phụ bản Vượt ải gia tộc
            else if (!(script is FamilyFuBen_Script_Main))
            {
                PlayerManager.ShowNotification(player, "Đạo cụ này chỉ có thể sử dụng trong phụ bản Vượt ải gia tộc!");
                return;
            }

            /// Script tương ứng
            FamilyFuBen_Script_Main familyFuBenScript = (script as FamilyFuBen_Script_Main);
            /// Thực hiện triệu hồi
            familyFuBenScript.UseCallBossItem(player, bossID);
        }
    }
}
