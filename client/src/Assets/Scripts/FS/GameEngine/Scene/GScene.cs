using System;
using UnityEngine;
using FS.GameEngine.TimerManager;

namespace FS.GameEngine.Scene
{
	/// <summary>
	/// Đối tượng bản đồ
	/// </summary>
	public partial class GScene
    {
        /// <summary>
        /// Đối tượng bản đồ
        /// </summary>
        public GScene()
        {
            /// Tải xuống bản đồ
            this.Scene_Loaded();
        }

        #region Tải xuống bản đồ

        /// <summary>
        /// Hoàn tất tải xuống bản đồ
        /// </summary>
        /// <param name="evt"></param>
        public void Scene_Loaded()
        {
            this.ClientHeartTimer = new DispatcherTimer("ClientHeartTimer");
            {
                this.ClientHeartTimer.Interval = TimeSpan.FromSeconds(10);
            }

            this.ClientHeartTimer.Tick = this.ClientHeartTimer_Tick;
            this.ClientHeartTimer.Start();

            this.LeaderMovingTimer = new DispatcherTimer("LeaderMovingTimer");
            {
                this.LeaderMovingTimer.Interval = TimeSpan.FromMilliseconds(800);
            }
            this.LeaderMovingTimer.Tick = this.LeaderMovingTick;
            this.LeaderMovingTimer.Start();


            this.PingTimer = new DispatcherTimer("PingTimer");
            {
                this.PingTimer.Interval = TimeSpan.FromMilliseconds(2000);
            }
            this.PingTimer.Tick = this.PingTimeTick;
            this.PingTimer.Start();
        }

        #endregion 

        #region Render

        /// <summary>
        /// Hàm này gọi liên tục mỗi Frame (tương tự hàm Update)
        /// </summary>
        /// <returns></returns>
        public void OnRenderScene()
        {
            /// Thực hiện sự kiện Update
            this.OnFrameEvents();

            if (Time.frameCount % 5 == 0)
            {
                /// Tải quái ở vị trí đầu tiên trong danh sách đang chờ
                this.AddListMonster();
                /// Tải người chơi ở vị trí đầu tiên trong sách đang chờ
                this.AddListRole();
                /// Tải điểm thu thập ở vị trí đầu tiên trong sách đang chờ 
                this.AddListGrowPoint();
                /// Tải vật phẩm rơi ở Map
                this.AddListGoodsPack();
                /// Tải khu vực động ở vị trí đầu tiên trong sách đang chờ 
                this.AddListDynamicArea();
                /// Tải BOT ở vị trí đầu tiên trong danh sách đang chờ
                this.AddListBot();
                /// Tải Pet ở vị trí đầu tiên trong danh sách đang chờ
                this.AddListPet();
                /// Tải xe tiêu ở vị trí đầu tiên trong danh sách đang chờ
                this.AddListTraderCarriage();
                /// Tải Bot bán hàng ở vị trí đầu tiên trong danh sách đang chờ
                this.AddListStallBot();
            }
        }

        #endregion



        #region Hủy đối tượng
        /// <summary>
        /// Hủy đối tượng
        /// </summary>
        public void Destroy()
        {
            /// Xóa các đối tượng trong Scene
            this.ClearScene();
            /// Hủy toàn bộ Timer
            this.ClientHeartTimer?.Dispose();
            this.LeaderMovingTimer?.Dispose();
            this.PingTimer?.Dispose();
            this.ChatVoiceForServerTimer?.Dispose();
        }
        #endregion
    }
}
