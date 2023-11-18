using System;
using System.Collections.Generic;
using System.Linq;
using ProtoBuf;

namespace Server.Data
{
	/// <summary>
	/// Gói tin gửi từ Client lên Server phản hồi trang bị đặt vào cùng danh sách nguyên liệu phía dưới
	/// </summary>
	[ProtoContract]
	public class C2G_InputEquipAndMaterials
	{
		/// <summary>
		/// Trang bị tương ứng
		/// </summary>
		[ProtoMember(1)]
		public GoodsData Equip { get; set; }

		/// <summary>
		/// Danh sách nguyên liệu
		/// </summary>
		[ProtoMember(2)]
		public List<GoodsData> Materials { get; set; }

		/// <summary>
		/// Tag
		/// </summary>
		[ProtoMember(3)]
		public string Tag { get; set; }
	}
}
