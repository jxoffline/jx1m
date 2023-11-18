using System;
using System.Collections.Generic;
using System.Linq;

namespace GameServer.KiemThe.Logic
{
	/// <summary>
	/// StoryBoard của người
	/// </summary>
	public partial class KTPlayerStoryBoardEx
	{
		#region Singleton Instance
		/// <summary>
		/// Quản lý StoryBoard của người
		/// </summary>
		public static KTPlayerStoryBoardEx Instance { get; private set; }
		#endregion

		#region Init
		/// <summary>
		/// Quản lý StoryBoard của người
		/// </summary>
		private KTPlayerStoryBoardEx()
		{
			this.StartTimer();
		}

		/// <summary>
		/// Khởi tạo dữ liệu
		/// </summary>
		public static void Init()
		{
			KTPlayerStoryBoardEx.Instance = new KTPlayerStoryBoardEx();
		}
		#endregion
	}
}
