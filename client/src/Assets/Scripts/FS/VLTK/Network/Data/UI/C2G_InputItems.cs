using System;
using System.Collections.Generic;
using System.Linq;
using ProtoBuf;

namespace Server.Data
{
	/// <summary>
	/// Gói tin gửi từ Client lên Server thông báo danh sách vật phẩm đặt vào khung
	/// </summary>
	[ProtoContract]
	public class C2G_InputItems
	{
		/// <summary>
		/// Danh sách vật phẩm đặt lên
		/// </summary>
		[ProtoMember(1)]
		public List<GoodsData> Items { get; set; }

		/// <summary>
		/// Tag đính kèm
		/// </summary>
		[ProtoMember(2)]
		public string Tag { get; set; }
	}
}
