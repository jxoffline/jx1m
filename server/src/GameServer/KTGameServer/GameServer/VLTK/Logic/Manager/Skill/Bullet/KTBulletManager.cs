namespace GameServer.KiemThe.Logic
{
	/// <summary>
	/// Quản lý đạn bay
	/// </summary>
	public partial class KTBulletManager
    {
        #region Singleton - Instance
        /// <summary>
        /// Quản lý đạn bay
        /// </summary>
        public static KTBulletManager Instance { get; private set; }

        /// <summary>
        /// Private constructor
        /// </summary>
        private KTBulletManager() { }
        #endregion

		#region Initialize
		/// <summary>
		/// Khởi tạo luồng quản lý đạn bay
		/// </summary>
		public static void Init()
        {
            KTBulletManager.Instance = new KTBulletManager();
            KTBulletManager.Instance.StartTimer();
            KTBulletManager.Instance.StartDelayTaskTimer();
        }

        /// <summary>
        /// Buộc chương trình làm rỗng luồng Bullet ngay lập tức
        /// </summary>
        public static void ForceResetBulletTimer()
        {
            KTBulletManager.Instance.ClearBulletTimers();
            KTBulletManager.Instance.ClearBulletDelayTasks();
        }
        #endregion
    }
}
