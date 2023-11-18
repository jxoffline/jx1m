namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Quản lý luồng thực thi Buff của nhân vật
    /// </summary>
    public partial class KTMonsterTimerManager
    {
        #region Singleton - Instance
        /// <summary>
        /// Đối tượng quản lý Timer của đối tượng
        /// </summary>
        public static KTMonsterTimerManager Instance { get; private set; }

        /// <summary>
        /// Private constructor
        /// </summary>
        private KTMonsterTimerManager() : base()
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
            KTMonsterTimerManager.Instance = new KTMonsterTimerManager();
        }
        #endregion

        #region Core
        /// <summary>
        /// Bắt đầu chạy Timer
        /// </summary>
        private void StartTimer()
        {
            /// Khởi tạo luồng quái thường
			this.StartNormalMonsterTimer();
            /// Khởi tạo luồng Boss
			this.StartNormalBossTimer();
            /// Khởi tạo luồng quái đặc biệt
            this.StartSpecialMonsterTimer();
            /// Khởi tạo luồng Boss đặc biệt
            this.StartSpecialBossTimer();
            /// Khởi tạo luồng quản lý quái
			this.StartSystemMonsterManagerTimer();
            /// Khởi tạo luồng quản lý quái tùy chọn
            this.StartDynamicNPCTimer();
        }
        #endregion
    }
}