using FS.GameEngine.Logic;
using FS.VLTK.Logic.Settings;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FS.VLTK.Control.Component
{
    /// <summary>
    /// Quản lý luồng thực thi ngầm
    /// </summary>
    public partial class Item
    {
        /// <summary>
        /// Âm thanh lần trước
        /// </summary>
        private int lastVolume = -1;


        /// <summary>
        /// Kiểm tra vị trí và thực hiện đổi màu nếu cần
        /// </summary>
        private void SynsLocation()
        {
            Drawing.Point point = new Drawing.Point((int) this.transform.localPosition.x / Global.CurrentMapData.GridSizeX, (int) this.transform.localPosition.y / Global.CurrentMapData.GridSizeY);
            if (Global.CurrentMapData.BlurPositions[point.X, point.Y] == 1)
            {
                this.groupColor.Alpha = this.MaxAlpha / 2;
            }
            else
            {
                this.groupColor.Alpha = this.MaxAlpha;
            }
        }

        /// <summary>
        /// Thực thi thiết lập hệ thống
        /// </summary>
        private void ExecuteSetting()
        {
            if (KTSystemSetting.SkillVolume != this.lastVolume)
            {
                this.audioPlayer.Volume = KTSystemSetting.SkillVolume / 100f;
                this.lastVolume = KTSystemSetting.SkillVolume;
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
