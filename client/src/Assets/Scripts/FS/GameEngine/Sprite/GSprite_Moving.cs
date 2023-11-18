using FS.GameEngine.Logic;
using FS.VLTK;
using FS.VLTK.Logic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static FS.VLTK.Entities.Enum;

namespace FS.GameEngine.Sprite
{
    /// <summary>
    /// Quản lý di chuyển
    /// </summary>
    public partial class GSprite
    {
        /// <summary>
        /// Danh sách đường đi
        /// </summary>
        private Queue<Vector2> MovePaths = new Queue<Vector2>();

        /// <summary>
        /// Luồng thực hiện di chuyển
        /// </summary>
        private Coroutine movingCoroutine = null;

        /// <summary>
        /// Sự kiện di chuyển hoàn tất
        /// </summary>
        private Action moveDone = null;

        /// <summary>
        /// Di chuyển từng bước
        /// </summary>
        /// <param name="toPos"></param>
        /// <param name="seconds"></param>
        /// <returns></returns>
        public IEnumerator StepMove(Vector2 toPos, float seconds)
        {
            /// Thời gian đã mô phỏng
            float elapsedTime = 0;
            /// Vị trí xuất phát
            Vector2 startPos = this.PositionInVector2;
            /// Chừng nào chưa hết thời gian
            while (elapsedTime < seconds)
            {
                /// Nếu chết rồi thì thôi
                if (this.IsDeath)
                {
                    /// Ngừng di chuyển
                    this.StopMove();
                    /// Bỏ qua
                    yield break;
                }

                /// % thời gian đã qua
                float percent = elapsedTime / seconds;
                /// Vị trí mới
                Vector2 newPos = Vector2.Lerp(startPos, toPos, percent);

                /// Tọa độ lưới
                Vector2 nextGrid = KTGlobal.WorldPositionToGridPosition(newPos);
                /// Nếu gặp phải điểm Obs động chưa được mở
                if (Global.Data.RoleData.RoleID == this.RoleID && Global.Data.GameScene.CurrentMapData.DynamicObstructions != null && Global.Data.GameScene.CurrentMapData.DynamicObstructions[(int) nextGrid.x, (int) nextGrid.y] > 0 && !Global.Data.GameScene.CurrentMapData.OpenedDynamicObsLabels.Contains(Global.Data.GameScene.CurrentMapData.DynamicObstructions[(int) nextGrid.x, (int) nextGrid.y]))
                {
                    /// Ngừng di chuyển
                    this.StopMove();
                    /// Bỏ qua
                    yield break;
                }

                /// Cập nhật vị trí
                this.Coordinate = new Drawing.Point((int) newPos.x, (int) newPos.y);

                /// Cập nhật thời gian cuối di chuyển nếu là Leader
                if (Global.Data.RoleData.RoleID == this.RoleID)
                {
                    KTLeaderMovingManager.LastMoveTick = KTGlobal.GetCurrentTimeMilis();
                }

                /// Tăng thời gian đã mô phỏng
                elapsedTime += Time.deltaTime;
                /// Bỏ qua Frame
                yield return null;
            }
            /// Thiết lập lại vị trí đích
            this.Coordinate = new Drawing.Point((int) toPos.x, (int) toPos.y);
        }

        /// <summary>
        /// Bắt đầu di chuyển
        /// </summary>
        /// <param name="moveDone"></param>
        /// <returns></returns>
        private IEnumerator StartMove()
        {
            /// Chừng nào vẫn còn các điểm cần đi
            while (this.MovePaths.Count > 0)
            {
                /// Nếu chết rồi thì thôi
                if (this.IsDeath)
                {
                    /// Ngừng di chuyển
                    this.StopMove();
                    /// Bỏ qua
                    break;
                }

                /// Nếu là Leader
                if (Global.Data.RoleData.RoleID == this.RoleID)
                {
                    /// Nếu thể lực dưới 5%
                    if (Global.Data.RoleData.CurrentStamina * 100 / Global.Data.RoleData.MaxStamina < 5)
                    {
                        /// Ép về trạng thái đi bộ
                        this.DoWalk();
                    }
                }

                /// Vị trí tiếp theo cần tới
                Vector2 nextPos = this.MovePaths.Peek();

                /// Vận tốc
                float velocity = KTGlobal.MoveSpeedToPixel(this.MoveSpeed) / (this.CurrentAction == KE_NPC_DOING.do_walk ? 2 : 1);
                /// Chừng nào vận tốc <= 0 thì bỏ qua
                while (velocity <= 0)
                {
                    yield return null;
                }

                /// Nếu không phải thực hiện động tác chạy hoặc đi bộ
                if (this.CurrentAction != KE_NPC_DOING.do_walk || this.CurrentAction != KE_NPC_DOING.do_run)
                {
                    /// Thực hiện động tác chạy
                    this.DoRun();
                }

                /// Khoảng cách từ vị trí hiện tại đến đích
                float distance = Vector2.Distance(this.PositionInVector2, nextPos);
                /// Thời gian cần để đến đích
                float time = distance / velocity;
                /// Nếu quá nhỏ
                if (time < 0.01f)
                {
                    /// Đến đích luôn
                    this.Coordinate = new Drawing.Point((int) nextPos.x, (int) nextPos.y);
                    /// Cập nhật thời gian cuối di chuyển nếu là Leader
                    if (Global.Data.RoleData.RoleID == this.RoleID)
                    {
                        KTLeaderMovingManager.LastMoveTick = KTGlobal.GetCurrentTimeMilis();
                    }
                    /// Xóa điểm đến khỏi hàng đợi
                    this.MovePaths.Dequeue();
                    /// Tiếp tục lặp
                    continue;
                }

                /// Nếu thời gian đủ lớn mới cập nhật hướng
                if (time > 0.025)
                {
                    /// Vector chỉ hướng di chuyển
                    Vector2 dirVector = nextPos - this.PositionInVector2;

                    /// Hướng quay của đối tượng
                    float rotationAngle = KTMath.GetAngle360WithXAxis(dirVector);
                    Direction newDir = KTMath.GetDirectionByAngle360(rotationAngle);

                    /// Thực hiện quay hướng
                    this.Direction = newDir;
                }

                /// Thực hiện di chuyển đến vị trí đích
                yield return this.StepMove(nextPos, time);

                /// Nếu hàng đợi rỗng
                if (this.MovePaths.Count <= 0)
                {
                    /// Thoát
                    break;
                }
                /// Xóa điểm đến khỏi hàng đợi
                this.MovePaths.Dequeue();
            }
            /// Thực hiện động tác đứng
            this.DoStand();
            /// Hủy đánh dấu đang di chuyển
            this.IsMoving = false;
            /// Thực thi sự kiện di chuyển hoàn tất
            this.moveDone?.Invoke();
        }

        /// <summary>
        /// Làm mới di chuyển (bao gồm tốc độ di chuyển hoặc trạng thái di chuyển thay đổi)
        /// </summary>
        public void RefreshMove(KE_NPC_DOING action)
        {
            /// Ngừng di chuyển
            this.StopMove(false);
            /// Đánh dấu đang di chuyển
            this.IsMoving = true;
            /// Vị trí ClickMove lần trước
            Vector2 lastClickMovePos = this.MovePaths.LastOrDefault();
            /// Đánh dấu đích đến
            KTLeaderMovingManager.LeaderMoveToPos = lastClickMovePos;
            /// Chuyển sang động tác tương ứng
            switch (action)
            {
                case KE_NPC_DOING.do_run:
                {
                    this.DoRun();
                    break;
                }
                case KE_NPC_DOING.do_walk:
                {
                    this.DoWalk();
                    break;
                }
            }

            /// Nếu là Leader, và có đường đi
            if (Global.Data.RoleData.RoleID == this.RoleID && this.MovePaths.Count > 0)
            {
                /// Nếu có vị trí ClickMove trước
                if (lastClickMovePos != Vector2.zero)
                {
                    Global.Data.GameScene.SetClickMovePos(lastClickMovePos);
                    /// Nếu đang mở khung bản đồ
                    if (PlayZone.Instance.UILocalMap != null && PlayZone.Instance.UILocalMap.gameObject.activeSelf)
                    {
                        PlayZone.Instance.UILocalMap.UpdateLocalMapFlagPos(lastClickMovePos);
                    }
                }
            }

            /// Bắt đầu di chuyển
            this.movingCoroutine = KTStoryBoard.Instance.StartCoroutine(this.StartMove());
        }

        /// <summary>
        /// Bắt đầu di chuyển
        /// </summary>
        /// <param name="paths"></param>
        /// <param name="moveDone"></param>
        public void StartMove(Queue<Vector2> paths, KE_NPC_DOING action, Action moveDone)
        {
            /// Nếu là Leader
            if (Global.Data.Leader == this)
            {
                /// Ngừng gọi pet
                KTAutoFightManager.Instance.IsCallingPet = false;
            }
            /// Ngừng di chuyển
            this.StopMove(true, false);
            /// Đánh dấu đoạn đường di chuyển
            this.MovePaths = paths;
            /// Đánh dấu sự kiện di chuyển hoàn tất
            this.moveDone = moveDone;
            /// Đánh dấu đang di chuyển
            this.IsMoving = true;
            /// Chuyển sang động tác tương ứng
            switch (action)
            {
                case KE_NPC_DOING.do_run:
                {
                    this.DoRun();
                    break;
                }
                case KE_NPC_DOING.do_walk:
                {
                    this.DoWalk();
                    break;
                }
            }
            /// Bắt đầu di chuyển
            this.movingCoroutine = KTStoryBoard.Instance.StartCoroutine(this.StartMove());
        }

        /// <summary>
        /// Ngừng di chuyển
        /// </summary>
        /// <param name="removePathsAndCallBack"></param>
        public void StopMove(bool removePathsAndCallBack = true, bool doStand = true)
        {
            /// Nếu có luồng di chuyển
            if (this.movingCoroutine != null)
            {
                /// Ngừng luồng mô phỏng
                KTStoryBoard.Instance.StopCoroutine(this.movingCoroutine);
                /// Hủy đối tượng
                this.movingCoroutine = null;
            }

            /// Hủy đánh dấu đang di chuyển
            this.IsMoving = false;
            /// Thực thi sự kiện di chuyển hoàn tất
            this.moveDone?.Invoke();

            /// Ngừng di chuyển
            if (doStand)
            {
                this.DoStand();
            }

            /// Nếu đánh dấu xóa thông tin di chuyển và sự kiện Callback
            if (removePathsAndCallBack)
            {
                /// Đánh dấu đoạn đường di chuyển
                this.MovePaths.Clear();
                /// Đánh dấu sự kiện di chuyển hoàn tất
                this.moveDone = null;
            }

            /// Nếu là Leader
            if (Global.Data.RoleData.RoleID == this.RoleID)
            {
                /// Hủy ký hiệu Click-Move
                Global.Data.GameScene.RemoveClickMovePos();
                /// Thực hiện di chuyển hoàn tất
                KTLeaderMovingManager.StopMove(false, false);
            }
        }
    }
}
