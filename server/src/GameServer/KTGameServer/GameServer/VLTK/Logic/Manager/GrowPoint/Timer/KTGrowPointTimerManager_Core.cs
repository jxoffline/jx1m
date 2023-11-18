using GameServer.KiemThe.Core;
using GameServer.KiemThe.Logic.Manager.Skill.PoisonTimer;
using GameServer.Logic;
using Server.Tools;
using System;
using System.ComponentModel;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Các hàm cơ bản
    /// </summary>
    public partial class KTGrowPointTimerManager
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
        /// <param name="duration"></param>
        /// <param name="timeout"></param>
        public void Add(GrowPoint obj, int duration, Action timeout)
        {
            if (obj == null)
            {
                return;
            }

            GrowPointTimer timer = new GrowPointTimer()
            {
                RefObject = obj,
                LifeTime = duration,
                Timeout = timeout,
                InitTicks = KTGlobal.GetCurrentTimeMilis(),
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
        public void Remove(GrowPoint obj)
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
