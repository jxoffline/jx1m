using HSGameEngine.GameEngine.Network.Tools;

namespace FS.GameEngine.Network
{
	/// <summary>
	/// Quản lý Packet Ping qua lại với Server
	/// </summary>
	public class TCPPing
	{
		/// <summary>
		/// Thời điểm bắt đầu Tick di chuyển
		/// </summary>
		private static long MoveCmdStartTicks =  0;

		/// <summary>
		/// Thời điểm bắt đầu Tick vị trí
		/// </summary>
		private static long PosCmdStartTicks =  0;

		/// <summary>
		/// Thời điểm bắt đầu Tick sự kiện Heart
		/// </summary>
		private static long HeartCmdStartTicks =  0;

		/// <summary>
		/// Giá trị Ping trung bình
		/// </summary>
		private static long AvgPingTicks =  0;
		
		/// <summary>
		/// Trả về giá trị Ping trung bình
		/// </summary>
		/// <returns>
		/// The ping ticks.
		/// </returns>
		public static long GetPingTicks()
		{
			return TCPPing.AvgPingTicks;
		}
		
		/// <summary>
		/// Ghi lại thời gian gửi các gói tin Ping lên Server
		/// </summary>
		/// <param name='nID'>
		/// </param>
		public static void RecordSendCmd(int nID)
		{
			switch (nID)
			{
				case (int)(TCPGameServerCmds.CMD_SPR_MOVE):
                    TCPPing.MoveCmdStartTicks = TimeManager.GetCorrectLocalTime();
					break;
				case (int)(TCPGameServerCmds.CMD_SPR_POSITION):
                    TCPPing.PosCmdStartTicks = TimeManager.GetCorrectLocalTime();
					break;	
				case (int)(TCPGameServerCmds.CMD_SPR_CLIENTHEART):
                    TCPPing.HeartCmdStartTicks = TimeManager.GetCorrectLocalTime();
					break;
				default:
					break;
			}
		}
		
		/// <summary>
		/// Ghi lại thời gian nhận các gói tin Ping từ Server
		/// </summary>
		/// <param name='nID'>
		/// </param>
		public static void RecordRecCmd(int nID)
		{
			switch (nID)
			{
				case (int)(TCPGameServerCmds.CMD_SPR_MOVE):
					TCPPing.AvgPingTicks = (TimeManager.GetCorrectLocalTime() - MoveCmdStartTicks) / 2;
					break;
				case (int)(TCPGameServerCmds.CMD_SPR_POSITION):
                    TCPPing.AvgPingTicks = (TimeManager.GetCorrectLocalTime() - PosCmdStartTicks) / 2;
					break;	
				case (int)(TCPGameServerCmds.CMD_SPR_CLIENTHEART):
                    TCPPing.AvgPingTicks = (TimeManager.GetCorrectLocalTime() - HeartCmdStartTicks) / 2;
					break;
				default:
					break;
			}
		}
	}
}

