using GameServer.Logic;
using Server.Tools;
using System;
using System.ComponentModel;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Quản lý luồng Bot biểu diễn
    /// </summary>
    public partial class KTDecoBotTimerManager
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
        public void Add(KDecoBot obj)
        {
            if (obj == null)
            {
                return;
            }

            DecoBotTimer timer = new DecoBotTimer()
            {
                RefObject = obj,
                TickTime = 500,
                IsStarted = false,
                LastTick = 0,
                Start = () => {
                    /// Thực thi sự kiện
                    this.ProcessStart(obj);
                },
                HasCompletedLastTick = true,
            };
            timer.Tick = () => {
                /// Thực thi sự kiện
                this.ProcessTick(obj);

                /// Đánh dấu đã hoàn thành Tick lần trước
                timer.HasCompletedLastTick = true;
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
        public void Remove(KDecoBot obj)
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
