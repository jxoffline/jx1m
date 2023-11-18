using FS.GameEngine.Logic;
using FS.GameEngine.Sprite;
using FS.VLTK.Factory;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FS.VLTK.Logic
{
    /// <summary>
    /// Tự động thu thập
    /// </summary>
    public partial class AutoQuest : TTMonoBehaviour
    {
        /// <summary>
        /// Tự động thực thi nhiệm vụ thu thập
        /// </summary>
        /// <returns></returns>
        private IEnumerator StartAutoQuest_CollectGrowPoint()
        {
            /// Ngừng di chuyển
            KTLeaderMovingManager.StopMove();
            KTLeaderMovingManager.StopChasingTarget();

            /// Chừng nào chưa hoàn thành nhiệm vụ
            while (!KTGlobal.IsQuestCompleted(this.Task))
            {
                /// Nếu đang thu thập
                if (KTGlobal.IsCollecting())
                {
                    yield return null;
                    continue;
                }
                yield return new WaitForSeconds(0.5f);

                /// Tìm danh sách điểm thu thập tương ứng
                List<GSprite> sprites = KTObjectsManager.Instance.FindObjects<GSprite>(x => x.ComponentMonster != null && x.ComponentMonster.StaticID == this.TaskData.TargetNPC).ToList();
                sprites?.Sort((o1, o2) =>
                {
                    /// Khoảng cách hiện tại đến mục tiêu
                    float distanceO1 = Vector2.Distance(o1.PositionInVector2, Global.Data.Leader.PositionInVector2);
                    float distanceO2 = Vector2.Distance(o2.PositionInVector2, Global.Data.Leader.PositionInVector2);

                    return (int) (distanceO1 - distanceO2);
                });

                /// Nếu tìm thấy danh sách điểm thu thập tương ứng
                if (sprites != null && sprites.Count > 0)
                {
                    Global.Data.GameScene.GrowPointClick(sprites[0]);
                }
                else
                {
                    KTGlobal.AddNotification("Không tìm thấy mục tiêu nào xung quanh!");
                }

                yield return new WaitForSeconds(1f);
            }

            /// Ngừng di chuyển
            KTLeaderMovingManager.StopMove();
            KTLeaderMovingManager.StopChasingTarget();

            /// Đợi 1s
            yield return new WaitForSeconds(1f);

            /// Thực thi hàm Callback khi hoàn tất nhiệm vụ
            this.Done?.Invoke();
            /// Ngừng Auto
            this.StopAutoQuest();
        }
    }
}
