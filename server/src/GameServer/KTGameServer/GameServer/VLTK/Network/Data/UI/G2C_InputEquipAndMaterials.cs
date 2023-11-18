using System;
using System.Collections.Generic;
using System.Linq;
using ProtoBuf;

namespace Server.Data
{
	/// <summary>
	/// Gói tin gửi từ Server về Client yêu cầu mở khung đặt vào trang bị cùng danh sách vật phẩm
	/// </summary>
	[ProtoContract]
	public class G2C_InputEquipAndMaterials
	{
		/// <summary>
		/// Tiêu đề khung
		/// </summary>
		[ProtoMember(1)]
		public string Title { get; set; }

		/// <summary>
		/// Mô tả khung
		/// </summary>
		[ProtoMember(2)]
		public string Description { get; set; }

		/// <summary>
		/// Mô tả khác
		/// </summary>
		[ProtoMember(3)]
		public string OtherDetail { get; set; }

		/// <summary>
		/// Buộc phải bao gồm cả nguyên liệu phía dưới không
		/// </summary>
		[ProtoMember(4)]
		public bool MustIncludeMaterials { get; set; }

		/// <summary>
		/// Tag
		/// </summary>
		[ProtoMember(5)]
		public string Tag { get; set; }
	}
}
