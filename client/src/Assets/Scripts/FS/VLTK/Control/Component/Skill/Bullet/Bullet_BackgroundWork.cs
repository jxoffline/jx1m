using FS.GameEngine.Logic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FS.VLTK.Control.Component.Skill
{
    /// <summary>
    /// Quản lý luồng thực thi ngầm
    /// </summary>
    public partial class Bullet
    {
        /// <summary>
        /// Đánh dấu lần trước có ẩn đối tượng không
        /// </summary>
        private bool lastHideRole = false;

        /// <summary>
        /// Kiểm tra vị trí và thực hiện đổi màu nếu cần
        /// </summary>
        private void SynsLocation()
        {
            /// Nếu toác
            if (Global.CurrentMapData == null)
            {
                return;
            }

            /// Tọa độ lưới
            Vector2 gridPos = KTGlobal.WorldPositionToGridPosition(this.transform.localPosition);
            /// Nếu nằm trong khu vực mờ
            if (Global.CurrentMapData != null && Global.CurrentMapData.BlurPositions != null && Global.CurrentMapData.BlurPositions[(int) gridPos.x, (int) gridPos.y] % 2 == 1)
            {
                this.groupColor.Alpha = 0.5f;
            }
            /// Nếu không nằm trong khu vực mờ
            else
            {
                this.groupColor.Alpha = 1f;
            }
        }

        /// <summary>
        /// Thực thi thiết lập hệ thống
        /// </summary>
        private void ExecuteSetting()
        {
            if (this.lastHideRole)
            {
                this.ResumeActions();
                this.SetModelVisible(true);
                this.lastHideRole = false;
            }
        }

        /// <summary>
        /// Thực thi sự kiện ngầm
        /// </summary>
        /// <returns></returns>
        private IEnumerator ExecuteBackgroundWork()
        {
            while (true)
            {
                yield return new WaitForSeconds(0.2f);
                this.SynsLocation();
                this.SortingOrderHandler();
                this.ExecuteSetting();
            }
        }

        /// <summary>
        /// Thực thi sự kiện bỏ qua một số Frame
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="work"></param>
        /// <returns></returns>
        private IEnumerator ExecuteSkipFrames(int skip, Action work)
        {
            for (int i = 1; i <= skip; i++)
            {
                yield return null;
            }
            work?.Invoke();
        }

        /// <summary>
        /// Thực thi sự kiện sau khoảng thời gian tương ứng
        /// </summary>
        /// <param name="sec"></param>
        /// <param name="work"></param>
        /// <returns></returns>
        private IEnumerator DelayTask(float sec, Action work)
        {
            yield return new WaitForSeconds(sec);
            work?.Invoke();
        }
    }
}
