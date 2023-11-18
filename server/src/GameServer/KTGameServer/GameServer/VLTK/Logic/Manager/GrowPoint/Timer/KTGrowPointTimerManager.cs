namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Quản lý luồng thực thi điểm thu thập
    /// </summary>
    public partial class KTGrowPointTimerManager
    {
        #region Singleton - Instance
        /// <summary>
        /// Đối tượng quản lý Timer của đối tượng
        /// </summary>
        public static KTGrowPointTimerManager Instance { get; private set; }

        /// <summary>
        /// Private constructor
        /// </summary>
        private KTGrowPointTimerManager() : base()
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
            KTGrowPointTimerManager.Instance = new KTGrowPointTimerManager();
        }
        #endregion
    }
}