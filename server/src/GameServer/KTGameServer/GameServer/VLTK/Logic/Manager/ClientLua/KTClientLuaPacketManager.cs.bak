using GameServer.Logic;
using GameServer.Server;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameServer.KiemThe.Logic.Manager.ClientLua
{
	/// <summary>
	/// Làm việc với Packet Lua gửi từ Client
	/// </summary>
	public static partial class KTClientLuaPacketManager
	{
		#region Define
		/// <summary>
		/// Danh sách Packet Lua theo ID tương ứng
		/// </summary>
		public enum TCPLuaPacketCmd
		{
			/// <summary>
			/// Test
			/// </summary>
			TEST = 10000000,
			/// <summary>
			/// Test 1
			/// </summary>
			TEST_1 = 10000001,
		}
		#endregion

		#region Core
		/// <summary>
		/// Xử lý gói tin từ Client Lua gửi về
		/// </summary>
		/// <param name="player"></param>
		/// <param name="packet"></param>
		/// <returns></returns>
		public static TCPProcessCmdResults Handle(KPlayer player, ClientLuaPacket packet)
		{
			/// Xem ID Packet là gì
			switch (packet.CmdID)
			{
				/// Nếu là Packet Test
				case (int) TCPLuaPacketCmd.TEST:
				{
					KTClientLuaPacketManager.ResolveTestPacket(player, packet);
					break;
				}
				/// Nếu là Packet Test
				case (int) TCPLuaPacketCmd.TEST_1:
				{
					KTClientLuaPacketManager.ResolveTest1Packet(player, packet);
					break;
				}
			}
			/// Không vào đám trên tức chưa được Handle
			return TCPProcessCmdResults.RESULT_OK;
		}

		/// <summary>
		/// Gửi gói tin về Client Lua
		/// </summary>
		/// <param name="player"></param>
		/// <param name="packet"></param>
		public static void Send(KPlayer player, ClientLuaPacket packet)
		{
			KT_TCPHandler.SendClientLuaPacket(player, packet);
		}
		#endregion

		/// <summary>
		/// Xử lý Packet Test gửi từ Lua Client lên
		/// </summary>
		/// <param name="packet"></param>
		private static void ResolveTestPacket(KPlayer player, ClientLuaPacket packet)
		{
			Console.WriteLine(packet);
			KTClientLuaPacketManager.Send(player, packet);
		}

		/// <summary>
		/// Xử lý Packet Test gửi từ Lua Client lên
		/// </summary>
		/// <param name="packet"></param>
		private static void ResolveTest1Packet(KPlayer player, ClientLuaPacket packet)
		{
			Console.WriteLine(packet);
		}
	}
}
