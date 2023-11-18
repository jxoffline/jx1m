using GameServer.Logic;
using Server.Tools;
using System;
using System.ComponentModel;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Quản lý luồng vật phẩm rơi
    /// </summary>
    public partial class KTGoodsPackTimerManager
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
		/// <param name="enableMultipleThreads"></param>
		private void ExecuteAction(Action work, Action<Exception> onError, bool enableMultipleThreads)
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
        public void Add(KGoodsPack obj)
        {
            if (obj == null)
            {
                return;
            }

            GoodsPackTimer timer = new GoodsPackTimer()
            {
                InitTicks = KTGlobal.GetCurrentTimeMilis(),
                RefObject = obj,
                LifeTime = KGoodsPack.GoodsPackKeepTimes,
                TickTime = 500,
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
        public void Remove(KGoodsPack obj)
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
