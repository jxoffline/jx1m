using FS.VLTK.Control.Component;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FS.VLTK.Logic
{
    /// <summary>
    /// Quản lý tự làm nhiệm vụ dạng đánh quái số lượng tương ứng
    /// </summary>
    public partial class AutoQuest
    {
        /// <summary>
        /// Luồng thực thi tự đánh quái đến khi hoàn thành nhiệm vụ
        /// </summary>
        /// <returns></returns>
        private IEnumerator StartAutoQuest_AttackMonster()
        {
            /// Ngừng di chuyển
            KTLeaderMovingManager.StopMove();
            KTLeaderMovingManager.StopChasingTarget();

            /// Mở tự đánh
            KTAutoFightManager.Instance.StartAuto();

            /// Chừng nào chưa hoàn thành nhiệm vụ thì còn tiếp tục
            while (!KTGlobal.IsQuestCompleted(this.Task))
            {
                yield return null;
            }

            /// Ngừng tự đánh
            KTAutoFightManager.Instance.StopAutoFight();

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
