namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Quản lý luồng thực thi khu vực động
    /// </summary>
    public partial class KTDynamicAreaTimerManager
    {
        #region Singleton - Instance
        /// <summary>
        /// Đối tượng quản lý Timer của đối tượng
        /// </summary>
        public static KTDynamicAreaTimerManager Instance { get; private set; }

        /// <summary>
        /// Private constructor
        /// </summary>
        private KTDynamicAreaTimerManager() : base()
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
            KTDynamicAreaTimerManager.Instance = new KTDynamicAreaTimerManager();
        }
        #endregion
    }
}