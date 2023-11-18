using GameServer.Logic;
using Server.Tools;
using System;
using static GameServer.Logic.KTMapManager;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Logic StoryBoard
    /// </summary>
    public partial class KTTraderCarriageStoryBoardEx
    {
        /// <summary>
        /// Bắt đầu thực hiện StoryBoard
        /// </summary>
        /// <param name="storyBoard"></param>
        private void StartStoryBoard(TraderCarriageStoryBoard storyBoard)
        {
            try
            {
                /// Nếu dính trạng thái không thể di chuyển
                if (!storyBoard.Owner.IsCanMove())
                {
                    this.Remove(storyBoard.Owner);
                    return;
                }

                /// Cập nhật động tác tương ứng
                storyBoard.Owner.m_eDoing = storyBoard.Action;

                /// Vị trí hiện tại
                UnityEngine.Vector2 currentPos = new UnityEngine.Vector2((int) storyBoard.Owner.CurrentPos.X, (int) storyBoard.Owner.CurrentPos.Y);
                /// Nếu vị trí hiện tại trùng với vị trí đầu tiên trong hàng đợi thì bỏ qua vị trí đầu
                storyBoard.Paths.TryPeek(out UnityEngine.Vector2 nextPos);
                if (currentPos == nextPos)
                {
                    storyBoard.Paths.TryDequeue(out _);
                }
            }
            catch (Exception ex)
            {
                /// Xóa StoryBoard vì lỗi
                this.Remove(storyBoard.Owner);
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }

        /// <summary>
        /// Tick thực hiện StoryBoard
        /// </summary>
        /// <param name="storyBoard"></param>
        private void TickStoryBoard(TraderCarriageStoryBoard storyBoard, long additionTick = 0, bool autoRemoveUnusedStoryBoard = true)
        {
            try
            {
                /// Nếu StoryBoard Tick cũ chưa hoàn thành
                if (!storyBoard.HasCompletedLastMove)
                {
                    return;
                }

                /// Nếu dính trạng thái không thể di chuyển
                if (autoRemoveUnusedStoryBoard && !storyBoard.Owner.IsCanMove())
                {
                    this.Remove(storyBoard.Owner);
                    return;
                }

                /// Đánh dấu chưa hoàn thành
                storyBoard.HasCompletedLastMove = false;

                /// Thời gian Tick hiện tại
                long currentTick = KTGlobal.GetCurrentTimeMilis();
                /// Thời gian Tick lần trước
                long lastTick = storyBoard.LastTick;
                /// Khoảng thời gian cần mô phỏng (giây)
                float elapseTime = (currentTick - lastTick + additionTick) / 1000f;
                /// Cập nhật thời gian Tick hiện tại
                storyBoard.LastTick = currentTick;

                /// Vị trí trước đó
                UnityEngine.Vector2 previousPos = new UnityEngine.Vector2((int) storyBoard.Owner.CurrentPos.X, (int) storyBoard.Owner.CurrentPos.Y);

                /// Thực hiện di chuyển đối tượng trong khoảng thời gian cần mô phỏng
                bool needToRemove = KTStoryBoardLogic.StepMove(storyBoard.Owner, storyBoard.Paths, elapseTime, storyBoard.Action, () =>
                {
                    return storyBoard.IsDisposed;
                });

                GameMap gameMap = KTMapManager.Find(storyBoard.Owner.CurrentMapCode);
                /// Vị trí mới sau khi di chuyển
                UnityEngine.Vector2 newPos = new UnityEngine.Vector2((int) storyBoard.Owner.CurrentPos.X, (int) storyBoard.Owner.CurrentPos.Y);
                /// Tọa độ lưới
                UnityEngine.Vector2 newGridPos = KTGlobal.WorldPositionToGridPosition(gameMap, newPos);

                /// Nếu đã bị hủy
                if (storyBoard.IsDisposed)
                {
                    return;
                }

                /// Nếu tọa độ lưới cũ khác tọa độ lưới mới
                if (newGridPos != storyBoard.LastGridPos)
                {
                    /// Cập nhật vị trí đối tượng vào Map
                    gameMap.Grid.MoveObject((int) storyBoard.Owner.CurrentPos.X, (int) storyBoard.Owner.CurrentPos.Y, storyBoard.Owner);

                    /// Cập nhật tọa độ lưới cũ
                    storyBoard.LastGridPos = newGridPos;
                }

                /// Đánh dấu đã hoàn thành
                storyBoard.HasCompletedLastMove = true;

                /// Nếu cần thiết phải xóa StoryBoard
                if (autoRemoveUnusedStoryBoard && needToRemove)
                {
                    this.Remove(storyBoard.Owner);
                    return;
                }
            }
            catch (Exception ex)
            {
                /// Xóa StoryBoard vì lỗi
                this.Remove(storyBoard.Owner);
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }

        /// <summary>
        /// Hoàn thành StoryBoard
        /// </summary>
        /// <param name="timer"></param>
        private void StopStoryBoard(TraderCarriageStoryBoard storyBoard)
        {
            try
            {
                /// Nếu không có StoryBoard cũ
                if (!this.HasStoryBoard(storyBoard.Owner))
                {
                    storyBoard.Owner.m_eDoing = Entities.KE_NPC_DOING.do_stand;
                }
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
            }
        }
    }
}
