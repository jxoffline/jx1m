namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// StoryBoard của các đối tượng khác
    /// </summary>
    public partial class KTBotStoryBoardEx
    {
        #region Singleton Instance
        /// <summary>
        /// Quản lý StoryBoard của các đối tượng khác
        /// </summary>
        public static KTBotStoryBoardEx Instance { get; private set; }
        #endregion

        #region Init
        /// <summary>
        /// Quản lý StoryBoard của xe tiêu
        /// </summary>
        private KTBotStoryBoardEx()
        {
            this.StartTimer();
        }

        /// <summary>
        /// Khởi tạo dữ liệu
        /// </summary>
        public static void Init()
        {
            KTBotStoryBoardEx.Instance = new KTBotStoryBoardEx();
        }
        #endregion
    }
}
