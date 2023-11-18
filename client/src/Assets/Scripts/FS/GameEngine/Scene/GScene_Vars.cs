using FS.GameEngine.Data;
using FS.GameEngine.Sprite;
using FS.GameEngine.TimerManager;

namespace FS.GameEngine.Scene
{
	/// <summary>
	/// Quản lý các tham biến
	/// </summary>
	public partial class GScene
    {
        #region Đinh nghĩa tham số bản đồ

        /// <summary>
        /// ID bản đồ
        /// </summary>
        public int MapCode { get; private set; }

        /// <summary>
        /// Bản đồ đã được tải xuống hoàn tất chưa
        /// </summary>
        public bool MapLoadingCompleted { get; private set; } = false;

        /// <summary>
        /// Dữ liệu bản đồ hiện tại
        /// </summary>
        public GMapData CurrentMapData { get; set; } = null;

        #endregion

        #region Người chơi

        /// <summary>
        /// Đối tượng người chơi
        /// </summary>
        private GSprite Leader = null;

        #endregion

        #region Timer

        /// <summary>
        /// Sự kiện Tick cập nhật tọa độ nhân vật
        /// </summary>
        protected DispatcherTimer LeaderMovingTimer { get; set; } = null;
        /// <summary>
        /// Sự kiện Tick Ping
        /// </summary>
        protected DispatcherTimer PingTimer { get; set; } = null;
        /// <summary>
        /// Sự kiện Heart
        /// </summary>
        protected DispatcherTimer ClientHeartTimer { get; set; } = null;
        /// <summary>
        /// Sự kiện ChatVoice
        /// </summary>
        protected DispatcherTimer ChatVoiceForServerTimer { get; set; } = null;

        #endregion
    }
}
