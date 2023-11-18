namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Quản lý luồng thực thi Buff của nhân vật
    /// </summary>
    public partial class KTPlayerTimerManager
    {
        #region Singleton - Instance

        /// <summary>
        /// Đối tượng quản lý Timer của đối tượng
        /// </summary>
        public static KTPlayerTimerManager Instance { get; private set; }

        /// <summary>
        /// Private constructor
        /// </summary>
        private KTPlayerTimerManager() : base() { }

        #endregion Singleton - Instance

        #region Initialize

        /// <summary>
        /// Hàm này gọi đến khởi tạo đối tượng
        /// </summary>
        public static void Init()
        {
            KTPlayerTimerManager.Instance = new KTPlayerTimerManager();
            KTPlayerTimerManager.Instance.StartTimer_Base();
            KTPlayerTimerManager.Instance.StartTimer_UpdateVision();
            KTPlayerTimerManager.Instance.StartTimer_Local();
            KTPlayerTimerManager.Instance.StartTimer_GameDB();
        }

        #endregion Initialize
    }
}