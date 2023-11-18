using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace FS.VLTK.Entities.Config
{
	/// <summary>
	/// Thông tin bản đồ lãnh thổ
	/// </summary>
	public class ColonyMapXML
	{
		/// <summary>
		/// ID bản đồ
		/// </summary>
		public int MapID { get; set; }

		/// <summary>
		/// Loại bản đồ
		/// </summary>
		public string Type { get; set; }

		/// <summary>
		/// ID bản đồ tranh đoạt
		/// </summary>
		public int MapFightID { get; set; }

		/// <summary>
		/// Vị trí X
		/// </summary>
		public int PosX { get; set; }

		/// <summary>
		/// Vị trí Y
		/// </summary>
		public int PosY { get; set; }

		/// <summary>
		/// Số sao
		/// </summary>
		public int Star { get; set; }

		/// <summary>
		/// Chuyển đối tượng từ XMLNode
		/// </summary>
		/// <param name="xmlNode"></param>
		/// <returns></returns>
		public static ColonyMapXML Parse(XElement xmlNode)
		{
			return new ColonyMapXML()
			{
				MapID = int.Parse(xmlNode.Attribute("MapID").Value),
				Type = xmlNode.Attribute("Type").Value,
				MapFightID = int.Parse(xmlNode.Attribute("MapFightID").Value),
				PosX = int.Parse(xmlNode.Attribute("PosX").Value),
				PosY = int.Parse(xmlNode.Attribute("PosY").Value),
				Star = int.Parse(xmlNode.Attribute("Star").Value),
			};
		}
	}
}
