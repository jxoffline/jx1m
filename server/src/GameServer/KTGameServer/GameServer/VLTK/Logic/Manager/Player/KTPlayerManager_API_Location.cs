using GameServer.Logic;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.Windows;
using static GameServer.Logic.KTMapManager;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Quản lý người chơi
    /// </summary>
    public static partial class KTPlayerManager
    {
        #region Vị trí
        /// <summary>
        /// Thay đổi vị trí trong cùng bản đồ
        /// </summary>
        /// <param name="player"></param>
        /// <param name="posX"></param>
        /// <param name="posY"></param>
        public static void ChangePos(KPlayer player, int posX, int posY, bool notifyOthers = true)
        {
            /// Toác
            if (player == null)
            {
                /// Bỏ qua
                return;
            }

            try
            {
                /// Cập nhật thời điểm thay đổi vị trí lần trước
                player.LastChangePositionTicks = KTGlobal.GetCurrentTimeMilis();
                /// Ghi lại vị trí đích đến
                player.LastChangedPosition = new Point(posX, posY);
                /// Ghi lại vị trí hợp lệ
                player.LastValidPos = new Point(posX, posY);

                /// Ngừng StoryBoard
                KTPlayerStoryBoardEx.Instance.Remove(player);
                /// Ngừng Blink
                player.StopBlink();

                /// Bản đồ tương ứng
                GameMap gameMap = KTMapManager.Find(player.CurrentMapCode);
                /// Toác
                if (gameMap == null)
                {
                    KTPlayerManager.ShowNotification(player, "Bản đồ này chưa được mở!");
                    /// Bỏ qua
                    return;
                }

                /// Bọn xung quanh
                List<KPlayer> playersAround = KTRadarMapManager.GetPlayersAround(player);
                /// Nếu có bọn xung quanh
                if (playersAround != null)
                {
                    /// Nếu có thông báo đến bọn xung quanh
                    if (notifyOthers)
                    {
                        /// Duyệt danh sách bọn xung quanh
                        foreach (KPlayer playerAround in playersAround)
                        {
                            /// Xóa khỏi tầm nhìn của thằng này
                            playerAround.VisibleGrid9Objects.TryRemove(player, out _);
                        }
                    }
                    /// Duyệt danh sách bọn xung quanh
                    foreach (KPlayer playerAround in playersAround)
                    {
                        /// Xóa thằng này khỏi tầm nhìn của bọn xung quanh
                        player.VisibleGrid9Objects.TryRemove(playerAround, out _);
                    }
                }

                /// Thông báo bản thân thay đổi vị trí
                KT_TCPHandler.NotifyMyselfChangePosition(player, posX, posY, false);

                /// Cập nhật tọa độ
                player.PosX = posX;
                player.PosY = posY;

                /// Cập nhật vị trí vào MapGrid
                gameMap.Grid.MoveObject(posX, posY, player);
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }
        #endregion

        #region Bản đồ
        /// <summary>
        /// 
        /// </summary>
        /// <param name="player"></param>
        /// <param name="mapCode"></param>
        /// <param name="posX"></param>
        /// <param name="posY"></param>
        /// <param name="toEnterCopyScene"></param>
        public static void ChangeMap(KPlayer player, int mapCode, int posX, int posY, bool toEnterCopyScene = false)
        {
            /// Toác
            if (player == null)
            {
                /// Bỏ qua
                return;
            }

            /// Bản đồ tương ứng
            GameMap gameMap = KTMapManager.Find(mapCode);
            /// Nếu bản đồ không tồn tại
            if (gameMap == null)
            {
                KTPlayerManager.ShowNotification(player, "Bản đồ này chưa được mở!");
                return;
            }
            /// Nếu là phụ bản
            if (gameMap != null && gameMap.IsCopyScene && !toEnterCopyScene)
            {
                KTPlayerManager.ShowNotification(player, "Không thể tiến vào bản đồ phụ bản!");
                return;
            }

            /// Thực thi sự kiện trước khi chuyển bản đồ
            player.OnPreChangeMap(gameMap);

            /// Đánh dấu đang đợi chuyển Map
            player.WaitingForChangeMap = true;

            /// Xóa đối tượng khỏi vị trí hiện tại
            KTPlayerManager.PlayerContainer.Remove(player);

            /// Đánh dấu vị trí đích cần dịch đến
            player.WaitingChangeMapCode = mapCode;
            player.WaitingChangeMapPosX = posX;
            player.WaitingChangeMapPosY = posY;

            /// Gửi gói tin
            KT_TCPHandler.NotifyChangeMap(player, mapCode, posX, posY);
        }
        #endregion
    }
}
