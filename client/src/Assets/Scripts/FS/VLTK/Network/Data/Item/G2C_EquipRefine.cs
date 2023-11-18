using ProtoBuf;
using System.Collections.Generic;

namespace Server.Data
{
	/// <summary>
	/// Luyện hóa trang bị
	/// </summary>
	[ProtoContract]
	public class G2C_EquipRefine
	{
		/// <summary>
		/// DbID trang bị
		/// </summary>
		[ProtoMember(1)]
		public int EquipDbID { get; set; }

		/// <summary>
		/// DbID công thức
		/// </summary>
		[ProtoMember(2)]
		public int RecipeDbID { get; set; }

		/// <summary>
		/// Danh sách Huyền Tinh
		/// </summary>
		[ProtoMember(3)]
		public List<int> CrystalStoneDbIDs { get; set; }

		/// <summary>
		/// ID sản phẩm được chọn
		/// </summary>
		[ProtoMember(4)]
		public int ProductGoodsID { get; set; }
	}
}
