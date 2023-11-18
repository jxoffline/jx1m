namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Quản lý luồng vật phẩm rơi
    /// </summary>
    public partial class KTStallBotTimerManager
    {
        #region Singleton - Instance
        /// <summary>
        /// Đối tượng quản lý Timer của đối tượng
        /// </summary>
        public static KTStallBotTimerManager Instance { get; private set; }

        /// <summary>
        /// Private constructor
        /// </summary>
        private KTStallBotTimerManager() : base()
        {
            this.StartTimer();
        }
        #endregion

        #region Initialize
        /// <summary>
        /// Hàm này gọi đến khởi tạo đối tượng
        /// </summary>
        public static void Init()
        {
            KTStallBotTimerManager.Instance = new KTStallBotTimerManager();
        }
        #endregion
    }
}