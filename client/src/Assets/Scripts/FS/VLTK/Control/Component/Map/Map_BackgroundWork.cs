using FS.VLTK.Logic.Settings;
using System;
using System.Collections;
using UnityEngine;

namespace FS.VLTK.Control.Component
{
    /// <summary>
    /// Quản lý luồng thực thi ngầm
    /// </summary>
    public partial class Map
    {
        /// <summary>
        /// Âm thanh lần trước
        /// </summary>
        private int lastVolume = -1;


        /// <summary>
        /// Thực thi thiết lập hệ thống
        /// </summary>
        private void ExecuteSetting()
        {
            /// Nếu không có MapMusic
            if (this.MapMusic == null)
            {
                return;
            }

            if (KTSystemSetting.MusicVolume != this.lastVolume)
            {
                this.MapMusic.Volume = KTSystemSetting.MusicVolume / 100f;
                this.lastVolume = KTSystemSetting.MusicVolume;
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
