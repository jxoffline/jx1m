using System.Collections.Generic;
using ProtoBuf;

namespace Server.Data
{
	/// <summary>
	/// Thông tin gói tin gửi qua lại giữa Lua Client và hệ thống
	/// </summary>
	[ProtoContract]
	public class ClientLuaPacket
	{
		/// <summary>
		/// ID Packet
		/// </summary>
		[ProtoMember(1)]
		public int CmdID { get; set; }

		/// <summary>
		/// Danh sách các tham biến kiểu Int
		/// </summary>
		[ProtoMember(2)]
		public Dictionary<int, int> IntParams { get; set; }

		/// <summary>
		/// Danh sách các tham biến kiểu String
		/// </summary>
		[ProtoMember(3)]
		public Dictionary<int, string> StrParams { get; set; }

		/// <summary>
		/// Chuyển đối tượng về dạng string
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return string.Format("ID: {0}, Total Int-Params: {1}, Total Str-Params: {2}", this.CmdID, this.IntParams == null ? 0 : this.IntParams.Count, this.StrParams == null ? 0 : this.StrParams.Count);
		}
	}
}
