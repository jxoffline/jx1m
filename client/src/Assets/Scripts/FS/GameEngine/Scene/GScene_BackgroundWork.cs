using System;
using FS.GameEngine.Logic;
using FS.GameEngine.Network;

namespace FS.GameEngine.Scene
{
	/// <summary>
	/// Quản lý các công việc ngầm
	/// </summary>
	public partial class GScene
    {
        #region Ping đồng bộ với máy chủ

        private float LastProcessTicks = 0f;
        private long LastDateTimeTicks = 0;

        /// <summary>
        /// Ping đồng bộ với máy chủ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PingTimeTick(object sender, EventArgs e)
        {
            if (Global.Data == null || !Global.Data.PlayGame)
            {
                return;
            }

            int processSubTicks = LastProcessTicks > 0 ? (int)((UnityEngine.Time.realtimeSinceStartup - LastProcessTicks) * 1000.0f) : 0;
            int dateTimeSubTicks = LastDateTimeTicks > 0 ? (int)(DateTime.Now.Ticks / 10000 - LastDateTimeTicks) : 0;

            LastProcessTicks = UnityEngine.Time.realtimeSinceStartup;
            LastDateTimeTicks = DateTime.Now.Ticks / 10000;

            GameInstance.Game.SpriteCheck(processSubTicks, dateTimeSubTicks);

            if (!GameInstance.Game.ActiveDisconnect)
            {
                BasePlayZone.InWaitPingCount++;
                if (BasePlayZone.InWaitPingCount * 2000 > BasePlayZone.PING_TIMEOUT)
                {
                    KTDebug.Log("GameInstance.Game.PingTimeOut()");
                    GameInstance.Game.PingTimeOut();
                    return;
                }
            }
        }

        #endregion

        #region Đồng bộ vị trí lên GS
        /// <summary>
        /// đồng bộ vị trí hiện tại của Leader lên Server
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LeaderMovingTick(object sender, EventArgs e)
        {
            /// Nếu chưa tải map xong
            if (!this.MapLoadingCompleted)
            {
                return;
            }

            if (!Global.Data.PlayGame)
            {
                return;
            }

            if (null != this.Leader)
            {
                GameInstance.Game.SpritePosition();
            }
        }
        #endregion

        #region Sự kiện Heart
        /// <summary>
        /// Sự kiện Heart
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void ClientHeartTimer_Tick(object sender, EventArgs e)
        {
            this.SendClientHeart();
        }
        #endregion
    }
}
