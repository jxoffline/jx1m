namespace GameServer.KiemThe.Logic
{
	/// <summary>
	/// StoryBoard của quái
	/// </summary>
	public partial class KTMonsterStoryBoardEx
	{
		#region Singleton Instance
		/// <summary>
		/// Quản lý StoryBoard của quái
		/// </summary>
		public static KTMonsterStoryBoardEx Instance { get; private set; }
		#endregion

		#region Init
		/// <summary>
		/// Quản lý StoryBoard của người
		/// </summary>
		private KTMonsterStoryBoardEx()
		{
			this.StartTimer();
		}

		/// <summary>
		/// Khởi tạo dữ liệu
		/// </summary>
		public static void Init()
		{
			KTMonsterStoryBoardEx.Instance = new KTMonsterStoryBoardEx();
		}
		#endregion
	}
}
