namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// StoryBoard của xe tiêu
    /// </summary>
    public partial class KTTraderCarriageStoryBoardEx
    {
        #region Singleton Instance
        /// <summary>
        /// Quản lý StoryBoard của xe tiêu
        /// </summary>
        public static KTTraderCarriageStoryBoardEx Instance { get; private set; }
        #endregion

        #region Init
        /// <summary>
        /// Quản lý StoryBoard của xe tiêu
        /// </summary>
        private KTTraderCarriageStoryBoardEx()
        {
            this.StartTimer();
        }

        /// <summary>
        /// Khởi tạo dữ liệu
        /// </summary>
        public static void Init()
        {
            KTTraderCarriageStoryBoardEx.Instance = new KTTraderCarriageStoryBoardEx();
        }
        #endregion
    }
}
