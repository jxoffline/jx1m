using FS.VLTK.Utilities.Threading;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FS.GameEngine.Sprite
{
    /// <summary>
    /// Quản lý các luồng thực thi của đối tượng
    /// </summary>
    public partial class GSprite
    {
        /// <summary>
        /// Định nghĩa luồng thực thi của đối tượng
        /// </summary>
        private class SpriteTimer
        {
            /// <summary>
            /// Tên luồng
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Luồng thực thi
            /// </summary>
            public Coroutine Coroutine { get; set; }
        }

        /// <summary>
        /// Danh sách luồng đang thực thi
        /// </summary>
        private readonly Dictionary<string, SpriteTimer> SpriteTimers = new Dictionary<string, SpriteTimer>();

        /// <summary>
        /// Thực thi luồng
        /// </summary>
        /// <param name="delay"></param>
        /// <param name="work"></param>
        /// <returns></returns>
        private IEnumerator ExecuteSpriteTimer(float delay, Action work)
        {
            yield return new WaitForSeconds(delay);
            work?.Invoke();
        }

        /// <summary>
        /// Thực thi luồng
        /// </summary>
        /// <param name="duration"></param>
        /// <param name="periodTick"></param>
        /// <param name="period"></param>
        /// <param name="done"></param>
        /// <returns></returns>
        private IEnumerator ExecuteSpriteTimer(float duration, float periodTick, Action period, Action done)
        {
            float dTime = 0f;
            while (dTime < duration)
            {
                period?.Invoke();
                yield return new WaitForSeconds(periodTick);
                dTime += periodTick;
            }
            done?.Invoke();
        }

        /// <summary>
        /// Thiết lập luồng thực thi
        /// </summary>
        /// <param name="name"></param>
        /// <param name="delay"></param>
        /// <param name="work"></param>
        public void SetTimer(string name, float delay, Action work)
        {
            /// Nếu luồng thực thi cũ tồn tại thì ngừng thực thi
            this.StopTimer(name);
            SpriteTimer spriteTimer = new SpriteTimer()
            {
                Name = name,
                Coroutine = KTTimerManager.Instance.StartCoroutine(this.ExecuteSpriteTimer(delay, () => {
                    try
                    {
                        work?.Invoke();
                    }
                    catch (Exception) { }
                    /// Xóa luồng
                    this.SpriteTimers.Remove(name);
                })),
            };
            this.SpriteTimers[name] = spriteTimer;
        }

        /// <summary>
        /// Thiết lập luồng thực thi liên tục mỗi khoảng nhỏ duy trì trong thời gian tương ứng
        /// </summary>
        /// <param name="name"></param>
        /// <param name="duration"></param>
        /// <param name="periodTick"></param>
        /// <param name="period"></param>
        /// <param name="done"></param>
        public void SetTimer(string name, float duration, float periodTick, Action period, Action done)
        {
            /// Nếu luồng thực thi cũ tồn tại thì ngừng thực thi
            this.StopTimer(name);
            SpriteTimer spriteTimer = new SpriteTimer()
            {
                Name = name,
                Coroutine = KTTimerManager.Instance.StartCoroutine(this.ExecuteSpriteTimer(duration, periodTick, period, () => {
                    try
                    {
                        done?.Invoke();
                    }
                    catch (Exception) { }
                    /// Xóa luồng
                    this.SpriteTimers.Remove(name);
                })),
            };
            this.SpriteTimers[name] = spriteTimer;
        }

        /// <summary>
        /// Ngừng luồng thực thi tương ứng
        /// </summary>
        /// <param name="name"></param>
        public void StopTimer(string name)
        {
            /// Nếu luồng thực thi cũ tồn tại
            if (this.SpriteTimers.TryGetValue(name, out SpriteTimer oldTimer))
            {
                /// Ngừng luồng thực thi cũ
                KTTimerManager.Instance.StopCoroutine(oldTimer.Coroutine);
            }
        }
    }
}
