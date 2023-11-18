using GameServer.KiemThe.Entities;
using GameServer.KiemThe.GameEvents.Interface;
using GameServer.KiemThe.Logic;
using GameServer.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using static GameServer.Logic.KTMapManager;

namespace GameServer.KiemThe.GameEvents.BaiHuTang
{
    /// <summary>
    /// Hoạt động Bạch Hổ Đường
    /// </summary>
    public class BaiHuTang_Script_Prepare : GameMapEvent
    {
        #region Constants
        /// <summary>
        /// Thời gian mỗi lần cập nhật trạng thái PK của người chơi
        /// </summary>
        private const int ChangePlayersPKModeEveryTick = 5000;
        #endregion

        #region Private fields
        /// <summary>
        /// Thời gian lần trước chuyển trạng thái PK của tất cả người chơi
        /// </summary>
        private long LastChangePKModeTick;
        #endregion

        #region Core GameMapEvent
        /// <summary>
        /// Hoạt động Bạch Hổ Đường
        /// </summary>
        /// <param name="gameMap"></param>
        /// <param name="activity"></param>
        public BaiHuTang_Script_Prepare(GameMap gameMap, KTActivity activity) : base(gameMap, activity)
		{

		}

        /// <summary>
        /// Sự kiện bắt đầu
        /// </summary>
        protected override void OnStart()
        {
            /// Đánh dấu đợi Bạch Hổ Đường 1
            BaiHuTang.CurrentStage = 1;
        }

        /// <summary>
        /// Sự kiện Tick
        /// </summary>
        protected override void OnTick()
        {
            /// Nếu đã đến thời gian chuyển PKMode
            if (KTGlobal.GetCurrentTimeMilis() - this.LastChangePKModeTick >= BaiHuTang_Script_Prepare.ChangePlayersPKModeEveryTick)
            {
                /// Đánh dấu thời gian chuyển PKMode
                this.LastChangePKModeTick = KTGlobal.GetCurrentTimeMilis();
                /// Chuyển PKMode tương ứng
                this.ChangePlayersPKMode();
            }
        }

        /// <summary>
        /// Sự kiện kết thúc
        /// </summary>
        protected override void OnClose()
        {
            /// Kết thúc báo danh Bạch Hổ Đường
            BaiHuTang.BeginRegistered = false;
        }

        /// <summary>
        /// Sự kiện người chơi vào bản đồ hoạt động
        /// </summary>
        /// <param name="player"></param>
        public override void OnPlayerEnter(KPlayer player)
        {
            base.OnPlayerEnter(player);

            this.OpenEventBroadboard(player, (int) GameEvent.BaiHuTang);
            /// Cập nhật thông báo sự kiện
            this.UpdateEventDetailsToPlayers("Bạch Hổ Đường", this.DurationTicks - this.LifeTimeTicks, "Bạch Hổ Đường chuẩn bị khai mở.");
        }

        /// <summary>
        /// Sự kiện người chơi rời bản đồ hoạt động
        /// </summary>
        /// <param name="player"></param>
        /// <param name="toMap"></param>
        public override void OnPlayerLeave(KPlayer player, GameMap toMap)
        {
            base.OnPlayerLeave(player, toMap);

            this.CloseEventBroadboard(player, (int) GameEvent.BaiHuTang);
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Cập nhật toàn bộ trạng thái PK của người chơi
        /// </summary>
        private void ChangePlayersPKMode()
        {
            foreach (KPlayer player in this.GetPlayers())
            {
                /// Chuyển trạng thái PK
                player.PKMode = (int) PKMode.Peace;
                /// Chuyển Camp về -1
                player.Camp = -1;
            }
        }
        #endregion
    }
}
