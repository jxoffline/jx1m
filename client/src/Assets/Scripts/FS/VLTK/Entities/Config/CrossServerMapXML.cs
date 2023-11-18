using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace FS.VLTK.Entities.Config
{
	/// <summary>
	/// Bản đồ Liên máy chủ
	/// </summary>
	public class CrossServerMapXML
	{
		/// <summary>
		/// Thông tin bản đồ
		/// </summary>
		public class MapInfo
		{
			/// <summary>
			/// Tên bản đồ
			/// </summary>
			public string MapName { get; set; }

			/// <summary>
			/// Icon
			/// </summary>
			public string IconName { get; set; }
			
			/// <summary>
			/// Vị trí X của Icon
			/// </summary>
			public int IconPosX { get; set; }
			
			/// <summary>
			/// Vị trí Y của Icon
			/// </summary>
			public int IconPosY { get; set; }

			/// <summary>
			/// Loại bản đồ
			/// </summary>
			public PlaceType Type { get; set; }

			/// <summary>
			/// ID bản đồ hoặc nhóm bản đồ chứa NPC
			/// </summary>
			public int NPCMapCode { get; set; }

			/// <summary>
			/// ID NPC
			/// </summary>
			public int NPCID { get; set; }

			/// <summary>
			/// Chuyển đối tượng từ XMLNode
			/// </summary>
			/// <param name="xmlNode"></param>
			/// <returns></returns>
			public static MapInfo Parse(XElement xmlNode)
			{
				return new MapInfo()
				{
					MapName = xmlNode.Attribute("MapName").Value,
					IconName = xmlNode.Attribute("IconName").Value,
					IconPosX = int.Parse(xmlNode.Attribute("IconPosX").Value),
					IconPosY = int.Parse(xmlNode.Attribute("IconPosY").Value),
					Type = (PlaceType) int.Parse(xmlNode.Attribute("Type").Value),
					NPCMapCode = int.Parse(xmlNode.Attribute("NPCMapCode").Value),
					NPCID = int.Parse(xmlNode.Attribute("NPCID").Value),
				};
			}
		}

		/// <summary>
		/// Danh sách bản đồ
		/// </summary>
		public List<MapInfo> Maps { get; set; }

		/// <summary>
		/// Chuyển đối tượng từ XMLNode
		/// </summary>
		/// <param name="xmlNode"></param>
		/// <returns></returns>
		public static CrossServerMapXML Parse(XElement xmlNode)
		{
			CrossServerMapXML crossServer = new CrossServerMapXML()
			{
				Maps = new List<MapInfo>(),
			};

			foreach (XElement node in xmlNode.Elements("Map"))
			{
				crossServer.Maps.Add(MapInfo.Parse(node));
			}

			return crossServer;
		}
	}
}
