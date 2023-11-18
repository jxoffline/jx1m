namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Quản lý luồng thực thi Buff của nhân vật
    /// </summary>
    public partial class KTPetTimerManager
    {
        #region Singleton - Instance
        /// <summary>
        /// Đối tượng quản lý Timer của đối tượng
        /// </summary>
        public static KTPetTimerManager Instance { get; private set; }

        /// <summary>
        /// Private constructor
        /// </summary>
        private KTPetTimerManager() : base()
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
            KTPetTimerManager.Instance = new KTPetTimerManager();
        }
        #endregion
    }
}