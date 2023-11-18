namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Quản lý luồng Bot biểu diễn
    /// </summary>
    public partial class KTDecoBotTimerManager
    {
        #region Singleton - Instance
        /// <summary>
        /// Đối tượng quản lý Timer của đối tượng
        /// </summary>
        public static KTDecoBotTimerManager Instance { get; private set; }

        /// <summary>
        /// Private constructor
        /// </summary>
        private KTDecoBotTimerManager() : base()
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
            KTDecoBotTimerManager.Instance = new KTDecoBotTimerManager();
        }
        #endregion
    }
}