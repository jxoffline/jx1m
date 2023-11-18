using System.Xml.Linq;

namespace FS.VLTK.Entities.Config
{
	/// <summary>
	/// Thông tin luyện hóa trang bị
	/// </summary>
	public class EquipRefineXML
	{
		/// <summary>
		/// ID công thức
		/// </summary>
		public int RecipeItemID { get; set; }

		/// <summary>
		/// ID vũ khí
		/// </summary>
		public int SourceEquipID { get; set; }

		/// <summary>
		/// ID sản phẩm
		/// </summary>
		public int ProductEquipID { get; set; }

		/// <summary>
		/// Phí
		/// </summary>
		public int Fee { get; set; }

		/// <summary>
		/// Chuyển đối tượng từ XMLNode
		/// </summary>
		/// <param name="xmlNode"></param>
		/// <returns></returns>
		public static EquipRefineXML Parse(XElement xmlNode)
		{
			return new EquipRefineXML()
			{
				RecipeItemID = int.Parse(xmlNode.Attribute("RefineId").Value),
				SourceEquipID = int.Parse(xmlNode.Attribute("SourceItem").Value),
				ProductEquipID = int.Parse(xmlNode.Attribute("ProduceItem").Value),
				Fee = int.Parse(xmlNode.Attribute("Fee").Value),
			};
		}
	}
}
