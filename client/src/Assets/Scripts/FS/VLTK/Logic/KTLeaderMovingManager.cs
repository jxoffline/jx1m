using FS.Drawing;
using FS.GameEngine.Logic;
using FS.GameEngine.Network;
using FS.GameEngine.Sprite;
using FS.VLTK.Utilities.Threading;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static FS.VLTK.Entities.Enum;

namespace FS.VLTK.Logic
{
    /// <summary>
    /// Lớp quản lý di chuyển của Leader
    /// </summary>
    public static class KTLeaderMovingManager
    {
        /// <summary>
        /// Sự kiện khi quá trình di chuyển hoàn tất
        /// </summary>
        public static Action Done { get; set; }

        /// <summary>
        /// Tọa độ đích của Leader khi dịch tới
        /// </summary>
        public static Vector2 LeaderMoveToPos { get; set; }

        /// <summary>
        /// Tọa độ đích của Leader dịch tới được thiết lập bởi người dùng khi tương tác với JoyStick hoặc Click bản đồ để dịch chuyển
        /// </summary>
        public static Vector2 LeaderPositiveMoveToPos { get; set; }

        /// <summary>
        /// Thời gian lần cuối thực hiện di chuyển
        /// </summary>
        public static long LastMoveTick { get; set; } = 0;

        /// <summary>
        /// Luồng thực hiện đuổi theo mục tiêu
        /// </summary>
        private static Coroutine ChaseTargetCoroutine = null;

        /// <summary>
        /// Thời điểm lần trước GS buộc Client đổi vị trí
        /// </summary>
        public static long LastForceChangePosTick { get; set; }

        /// <summary>
        /// Luồng thực hiện đuỏi theo mục tiêu
        /// </summary>
        /// <param name="target"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        private static IEnumerator DoFollowTarget(GSprite target, float distance, Action done)
        {
            /// Bản thân
            GSprite leader = Global.Data.Leader;
            /// Thời gian nghỉ
            float deltaTime = 0.2f;
            /// Nghỉ đợi
            WaitForSeconds wait = new WaitForSeconds(deltaTime);
            /// Lặp liên tục
            do
            {
                /// Vị trí của bản thân
                Vector2 leaderPos = leader.PositionInVector2;
                /// Vị trí của Target
                Vector2 targetPos = target.PositionInVector2;

                /// Khoảng cách đến mục tiêu
                float distanceToTarget = Vector2.Distance(leaderPos, targetPos);
                /// Nếu đủ gần
                if (distanceToTarget <= distance)
                {
                    /// Thực thi sự kiện đuổi hoàn tất
                    done?.Invoke();
                    /// Thoát lặp
                    break;
                }

                /// Vector hướng di chuyển
                Vector2 dirVector = targetPos - leaderPos;
                /// Vận tốc di chuyển của bản thân
                float leaderVelocity = KTGlobal.MoveSpeedToPixel(leader.MoveSpeed) / (leader.CurrentAction == KE_NPC_DOING.do_walk ? 2 : 1);
                /// Khoảng dịch được
                Vector2 nextPos = KTMath.FindPointInVectorWithDistance(leaderPos, dirVector, leaderVelocity * deltaTime);

                /// Biến đánh dấu tạm ngưng
                bool pausing = false;
                /// Thực hiện tìm đường đến chỗ mục tiêu
                KTLeaderMovingManager.AutoFindRoad(new Point((int) nextPos.x, (int) nextPos.y), () => {
                    pausing = true;
                });

                /// Chừng nào đang tạm ngưng thì thôi
                while (pausing)
                {
                    /// Bỏ qua Frame
                    yield return null;
                    /// Tiếp tục đợi
                    continue;
                }

                /// Nghỉ
                yield return wait;
            }
            while (true);

            /// Ngừng luồng
            KTLeaderMovingManager.StopChasingTarget();
        }

        /// <summary>
        /// Đuổi theo mục tiêu
        /// </summary>
        /// <param name="target"></param>
        public static void ChaseTarget(GSprite target, float distance, Action done)
        {
            KTLeaderMovingManager.StopChasingTarget();
            KTLeaderMovingManager.ChaseTargetCoroutine = KTTimerManager.Instance.StartCoroutine(KTLeaderMovingManager.DoFollowTarget(target, distance, done));
        }

        /// <summary>
        /// Ngừng đuổi mục tiêu
        /// </summary>
        public static void StopChasingTarget()
        {
            if (KTLeaderMovingManager.ChaseTargetCoroutine != null)
            {
                KTTimerManager.Instance.StopCoroutine(KTLeaderMovingManager.ChaseTargetCoroutine);
                KTLeaderMovingManager.ChaseTargetCoroutine = null;
            }
        }

        /// <summary>
        /// Tự tìm đường đến vị trí chỉ định
        /// </summary>
        /// <param name="toPos">Vị trí đích</param>
        public static void AutoFindRoad(Point toPos)
        {
            KTLeaderMovingManager.AutoFindRoad(toPos, null);
        }

        /// <summary>
        /// Tự tìm đường đến vị trí chỉ định
        /// </summary>
        /// <param name="toPos">Vị trí đích</param>
        /// <param name="moveDone">Sự kiện khi quá trình hoàn tất</param>
        public static void AutoFindRoad(Point toPos, Action moveDone)
        {
            /// Đối tượng Leader
            GSprite leader = Global.Data.Leader;
            if (leader == null)
            {
                return;
            }

            /// Nếu đang khinh công
            if (leader.CurrentAction == Entities.Enum.KE_NPC_DOING.do_jump)
            {
                return;
            }

            /// Hủy thanh Progress Bar
            PlayZone.Instance.HideProgressBar();

            /// Nếu bị kẹt
            if (!Global.Data.GameScene.CanMoveByWorldPos(leader.Coordinate))
            {
                /// Lấy vị trí thỏa mãn không có vật cản xung quanh
                Vector2 gridPos = KTGlobal.WorldPositionToGridPosition(leader.PositionInVector2);
                Point noObsPos = KTGlobal.GetRandomNoObsPointAroundPos(new Point((int) gridPos.x, (int) gridPos.y));
                /// Chuyển sang tọa độ thực
                Vector2 worldPos = KTGlobal.GridPositionToWorldPosition(new Vector2(noObsPos.X, noObsPos.Y));
                /// Tìm vị trí gần nhất không có vật cản xung quanh
                leader.Coordinate = new Point((int) worldPos.x, (int) worldPos.y);
            }

            /// Vị trí đầu cuối
            Vector2 fromVector = leader.PositionInVector2;
            Vector2 toVector = new Vector2(toPos.X, toPos.Y);

            /// Nếu không có đường đi
			if (!KTGlobal.HasPath(fromVector, toVector))
            {
                // Nếu đang bật auto cảnh bảo tới auto luôn là lỗi vị trí để AUTO đổi taget khác
                if (KTAutoFightManager.Instance.IsAutoFighting)
                {
                    KTAutoFightManager.Instance.BugPostion = 1000;
                }
                KTGlobal.AddNotification("Vị trí đích không thể đến được!");
                return;
            }

            /// Thực hiện tìm đường
            List<Vector2> paths = Global.Data.GameScene.FindPath(leader, ref fromVector, ref toVector);

            /// Nếu không có đường đi thì bỏ qua
            if (paths == null || paths.Count < 1)
            {
                // Nếu đang bật auto cảnh bảo tới auto luôn là lỗi vị trí để AUTO đổi taget khác
                if (KTAutoFightManager.Instance.IsAutoFighting)
                {
                    KTAutoFightManager.Instance.BugPostion = 1000;
                }
                KTGlobal.AddNotification("Vị trí đích không thể đến được!");
                return;
            }

            /// Hàng đợi danh sách các điểm trên đường đi
            Queue<Vector2> queue = new Queue<Vector2>();
            /// Nạp tất cả các điểm vừa tìm được vào hàng đợi
            foreach (Vector2 pos in paths)
            {
                queue.Enqueue(pos);
            }

            /// Chuyển về dạng string
            string pathString = "";
            while (queue.Count > 0)
            {
                Vector2 node = queue.Dequeue();
                pathString += "|" + string.Format("{0}_{1}", (int) node.x, (int) node.y);
            }
            pathString = pathString.Substring(1);

            /// Nếu thể lực dưới 5%
            if (Global.Data.RoleData.CurrentStamina * 100 / Global.Data.RoleData.MaxStamina <= 5)
            {
                /// Đi bộ
                leader.DoWalk();
            }
            else
            {
                /// Chạy
                leader.DoRun();
            }

            /// Gửi gói tin về Server thông báo Leader di chuyển theo Path đã tìm sẵn
            GameInstance.Game.KTLeaderMoveTo(new Point((int) fromVector.x, (int) fromVector.y), new Point((int) toVector.x, (int) toVector.y), pathString);

            /// Thực thi StoryBoard của Leader
            KTStoryBoard.Instance.AddOrUpdate(leader, pathString, KTLeaderMovingManager.MoveDone);

            /// Cập nhật vị trí đích cho Leader
            KTLeaderMovingManager.LeaderMoveToPos = new Vector2(toPos.X, toPos.Y);

            /// Sự kiện thực thi khi quá trình di chuyển hoàn tất
            KTLeaderMovingManager.Done = moveDone;
        }

        /// <summary>
        /// Di chuyển tới vị trí phía trước theo hướng hiện tại của JoyStick
        /// </summary>
        /// <param name="dirVector"></param>
        /// <param name="distance"></param>
        /// <param name="moveDone"></param>
        public static void MoveByDirVector(Vector2 dirVector, float distance, Action moveDone)
        {
            /// Nếu JoyStick Pos về 0 thì đã dừng thao tác
            if (dirVector == Vector2.zero)
            {
                return;
            }

            /// Đối tượng Leader
            GSprite leader = Global.Data.Leader;
            if (leader == null)
            {
                return;
            }

            /// Nếu đang khinh công
            if (leader.CurrentAction == Entities.Enum.KE_NPC_DOING.do_jump)
            {
                return;
            }

            /// Hủy thanh Progress Bar
            PlayZone.Instance.HideProgressBar();

            /// Nếu bị kẹt
            if (!Global.Data.GameScene.CanMoveByWorldPos(leader.Coordinate))
            {
                /// Lấy vị trí thỏa mãn không có vật cản xung quanh
                Vector2 gridPos = KTGlobal.WorldPositionToGridPosition(leader.PositionInVector2);
                Point noObsPos = KTGlobal.GetRandomNoObsPointAroundPos(new Point((int) gridPos.x, (int) gridPos.y));
                /// Chuyển sang tọa độ thực
                Vector2 worldPos = KTGlobal.GridPositionToWorldPosition(new Vector2(noObsPos.X, noObsPos.Y));
                /// Tìm vị trí gần nhất không có vật cản xung quanh
                leader.Coordinate = new Point((int) worldPos.x, (int) worldPos.y);
            }

            /// Vị trí bắt đầu
            Vector2 fromPos = leader.PositionInVector2;
            /// Vị trí đích đến
            Vector2 toPos = KTMath.FindPointInVectorWithDistance(fromPos, dirVector, distance);

            /// Danh sách các điểm trên đường đi
            List<Vector2> paths;
            
            /// Nếu không có đường đi
            if (!KTGlobal.HasPath(fromPos, toPos) || !Global.Data.GameScene.CanMoveByWorldPos(toPos))
            {
                /// Tìm vị trí trên đường thẳng mà không có vật cản
                toPos = KTGlobal.FindLinearNoObsPoint(fromPos, toPos);
                paths = new List<Vector2>()
                {
                    new Vector2(fromPos.x, fromPos.y),
                    new Vector2(toPos.x, toPos.y),
                };
            }
            /// Nếu có đường đi
            else
            {
                /// Thực hiện tìm đường
                paths = Global.Data.GameScene.FindPath(leader, ref fromPos, ref toPos);
            }

            /// Nếu không có đường đi thì bỏ qua
            if (paths == null || paths.Count < 1)
            {
                return;
            }

            /// Hàng đợi danh sách các điểm trên đường đi
            Queue<Vector2> queue = new Queue<Vector2>();
            /// Nạp tất cả các điểm vừa tìm được vào hàng đợi
            foreach (Vector2 pos in paths)
            {
                queue.Enqueue(pos);
            }

            /// Chuyển về dạng string
            string pathString = "";
            while (queue.Count > 0)
            {
                Vector2 node = queue.Dequeue();
                pathString += "|" + string.Format("{0}_{1}", (int) node.x, (int) node.y);
            }
            pathString = pathString.Substring(1);

            /// Nếu thể lực dưới 5%
            if (Global.Data.RoleData.CurrentStamina * 100 / Global.Data.RoleData.MaxStamina <= 5)
            {
                /// Đi bộ
                leader.DoWalk();
            }
            else
            {
                /// Chạy
                leader.DoRun();
            }

            /// Gửi gói tin về Server thông báo Leader di chuyển theo Path đã tìm sẵn
            GameInstance.Game.KTLeaderMoveTo(new Point((int) fromPos.x, (int) fromPos.y), new Point((int) toPos.x, (int) toPos.y), pathString);

            /// Thực thi StoryBoard của Leader
            KTStoryBoard.Instance.AddOrUpdate(leader, pathString, KTLeaderMovingManager.MoveDone);

            /// Cập nhật vị trí đích cho Leader
            KTLeaderMovingManager.LeaderMoveToPos = toPos;

            /// Cập nhật vị trí dịch bằng JoyStick
            KTLeaderMovingManager.LeaderPositiveMoveToPos = toPos;

            /// Sự kiện thực thi khi quá trình di chuyển hoàn tất
            KTLeaderMovingManager.Done = moveDone;
        }

        /// <summary>
        /// Sự kiện khi quá trình dịch chuyển hoàn tất
        /// </summary>
        public static void MoveDone()
        {
            /// Sự kiện MoveDone hiện tại
            Action done = KTLeaderMovingManager.Done;
            /// Xóa sự kiện MoveDone (chống StackOverFlow)
            KTLeaderMovingManager.Done = null;
            /// Thực thi sự kiện MoveDone
            done?.Invoke();
            KTLeaderMovingManager.LeaderMoveToPos = default;
            /// Hủy chữ tự tìm đường
            PlayZone.Instance.HideTextAutoFindPath();

            /// Xóa ký hiệu Click-Move
            Global.Data.GameScene.RemoveClickMovePos();

        }

        /// <summary>
        /// Dừng tự tìm đường
        /// </summary>
        /// <param name="sendToGS"></param>
        /// <param name="sendToGS"></param>
        public static void StopMove(bool removeFromStoryBoard = true, bool sendToGS = true)
        {
            /// Đối tượng Leader
            GSprite leader = Global.Data.Leader;
            if (leader == null)
            {
                return;
            }

            /// Nếu không có trạng thái tìm đường
            if (!leader.IsMoving)
            {
                return;
            }

            KTLeaderMovingManager.LeaderMoveToPos = default;
            KTLeaderMovingManager.LeaderPositiveMoveToPos = default;

            if (removeFromStoryBoard)
            {
                /// Xóa StoryBoard đã có sẵn
                leader.StopMove();
            }

            /// Hủy chữ tự tìm đường
            PlayZone.Instance?.HideTextAutoFindPath();

            /// Xóa ký hiệu Click-Move
            Global.Data.GameScene.RemoveClickMovePos();

            /// Gửi gói tin về Server thông báo đối tượng ngừng tự tìm đường
            if (sendToGS)
            {
                GameInstance.Game.SpriteStopMove();
            }
        }

        /// <summary>
        /// Dừng tự tìm đường
        /// </summary>
        /// <param name="sendToGS"></param>
        public static void StopMoveImmediately(bool removeFromStoryBoard = true, bool sendToGS = true)
        {
            /// Đối tượng Leader
            GSprite leader = Global.Data.Leader;
            if (leader == null)
            {
                return;
            }

            /// Nếu không có trạng thái tìm đường
            if (!leader.IsMoving)
            {
                return;
            }

            KTLeaderMovingManager.LeaderMoveToPos = default;
            KTLeaderMovingManager.LeaderPositiveMoveToPos = default;

            if (removeFromStoryBoard)
            {
                /// Xóa StoryBoard đã có sẵn
                leader.StopMove();
            }

            /// Hủy chữ tự tìm đường
            PlayZone.Instance.HideTextAutoFindPath();

            /// Xóa ký hiệu Click-Move
            Global.Data.GameScene.RemoveClickMovePos();

            /// Gửi gói tin về Server thông báo đối tượng ngừng tự tìm đường
            if (sendToGS)
            {
                GameInstance.Game.SpriteStopMove();
            }
        }
    }
}
