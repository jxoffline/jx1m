using GameServer.KiemThe.Core;
using GameServer.KiemThe.Logic.Manager;
using Server.Tools;
using System;
using System.ComponentModel;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Các hàm cơ bản
    /// </summary>
    public partial class KTDynamicAreaTimerManager
    {
        #region Core
        /// <summary>
        /// Đánh dấu buộc xóa toàn bộ Timer của pet
        /// </summary>
        private bool forceClearAllTimers = false;

        /// <summary>
        /// Sự kiện khi Background Worker hoàn tất công việc
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Worker_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                LogManager.WriteLog(LogTypes.Exception, e.Error.ToString());
            }
        }

        /// <summary>
		/// Thực thi sự kiện gì đó
		/// </summary>
		/// <param name="work"></param>
		/// <param name="onError"></param>
		private void ExecuteAction(Action work, Action<Exception> onError)
        {
            try
            {
                work?.Invoke();
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Exception, ex.ToString());
                onError?.Invoke(ex);
            }
        }

        /// <summary>
        /// Xóa rỗng toàn bộ luồng quái
        /// </summary>
        public void ClearTimers()
        {
            this.forceClearAllTimers = true;
        }
        #endregion

        #region API
        /// <summary>
        /// Thêm đối tượng vào danh sách quản lý
        /// </summary>
        /// <param name="obj"></param>
        public void Add(KDynamicArea obj)
        {
            if (obj == null)
            {
                return;
            }

            /// Thời gian còn lại
            long totalTimes = KTGlobal.GetCurrentTimeMilis() - obj.StartTicks;

            /// Nếu đã hết thời gian
            if (obj.LifeTime != -1 && totalTimes >= obj.LifeTime)
            {
                /// Xóa khỏi luồng quản lý luôn
                KTDynamicAreaManager.Remove(obj.ID);
                return;
            }

            DynamicAreaTimer timer = new DynamicAreaTimer()
            {
                TickTime = obj.Tick,
                RefObject = obj,
                LastTick = 0,
                LifeTime = obj.LifeTime == -1 ? -1 : obj.LifeTime - totalTimes,
                InitTicks = KTGlobal.GetCurrentTimeMilis(),
                HasCompletedLastTick = true,
            };
            timer.Tick = () => {
                /// Nếu vi phạm điều kiện, cần xóa khỏi Timer
                if (this.NeedToBeRemoved(obj))
                {
                    this.Remove(obj);
                    return;
                }

                /// Thực thi sự kiện Tick
                this.ExecuteAction(obj.ProcessTick, null);

                /// Đánh dấu đã hoàn thành Tick lần trước
                timer.HasCompletedLastTick = true;
            };
            timer.Timeout = () =>
            {
                /// Thực thi sự kiện hết thời gian
                this.ExecuteAction(obj.OnTimeout, null);
                /// Xóa đối tượng
                this.Remove(obj);
                /// Xóa khỏi luồng quản lý luôn
                KTDynamicAreaManager.Remove(obj.ID);
            };

            /// Thêm vào danh sách cần tải
            this.waitingQueue.Enqueue(new QueueItem()
            {
                Type = 1,
                Data = timer,
            });
        }

        /// <summary>
        /// Dừng và xóa đối tượng khỏi luồng thực thi
        /// </summary>
        /// <param name="obj"></param>
        public void Remove(KDynamicArea obj)
        {
            if (obj == null)
            {
                return;
            }

            /// Thêm vào danh sách cần tải
            this.waitingQueue.Enqueue(new QueueItem()
            {
                Type = 0,
                Data = obj,
            });
        }
        #endregion
    }
}
