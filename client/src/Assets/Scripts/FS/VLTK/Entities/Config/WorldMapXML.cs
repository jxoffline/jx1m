using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace FS.VLTK.Entities.Config
{
	/// <summary>
	/// Loại khu vực
	/// </summary>
	public enum PlaceType
	{
		/// <summary>
		/// Tân thủ thôn
		/// </summary>
		Village,
		/// <summary>
		/// Thành thị
		/// </summary>
		City,
		/// <summary>
		/// Môn phái
		/// </summary>
		Faction,
		/// <summary>
		/// Dã ngoại
		/// </summary>
		OutMap,
	}

	/// <summary>
	/// Bản đồ thế giới
	/// </summary>
	public class WorldMapXML
	{
		/// <summary>
		/// Thế giới
		/// </summary>
		public class World
		{
			/// <summary>
			/// Khu vực
			/// </summary>
			public class Area
			{
				/// <summary>
				/// ID khu vực
				/// </summary>
				public int ID { get; set; }

				/// <summary>
				/// Tên khu vực
				/// </summary>
				public string Name { get; set; }

				/// <summary>
				/// Vị trí đặt X
				/// </summary>
				public int PosX { get; set; }

				/// <summary>
				/// Vị trí đặt Y
				/// </summary>
				public int PosY { get; set; }

				/// <summary>
				/// Chuyển đối tượng từ XMLNode
				/// </summary>
				/// <param name="xmlNode"></param>
				/// <returns></returns>
				public static Area Parse(XElement xmlNode)
				{
					return new Area()
					{
						ID = int.Parse(xmlNode.Attribute("ID").Value),
						Name = xmlNode.Attribute("Name").Value,
						PosX = int.Parse(xmlNode.Attribute("PosX").Value),
						PosY = int.Parse(xmlNode.Attribute("PosY").Value),
					};
				}
			}

			/// <summary>
			/// Tên ảnh
			/// </summary>
			public string ImageFileName { get; set; }

			/// <summary>
			/// Danh sách khu vực
			/// </summary>
			public List<Area> Areas { get; set; }

			/// <summary>
			/// Danh sách địa danh
			/// </summary>
			public List<Place> Places { get; set; }

			/// <summary>
			/// Chuyển đối tượng từ XMLNode
			/// </summary>
			/// <param name="xmlNode"></param>
			/// <returns></returns>
			public static World Parse(XElement xmlNode)
			{
				World world = new World()
				{
					ImageFileName = xmlNode.Attribute("ImageFileName").Value,
					Areas = new List<Area>(),
					Places = new List<Place>(),
				};

				foreach (XElement node in xmlNode.Elements("Area"))
				{
					world.Areas.Add(Area.Parse(node));
				}

				foreach (XElement node in xmlNode.Elements("Place"))
				{
					world.Places.Add(Place.Parse(node));
				}

				return world;
			}
		}

		/// <summary>
		/// Địa danh
		/// </summary>
		public class Place
		{
			/// <summary>
			/// ID bản đồ
			/// </summary>
			public int MapCode { get; set; }

			/// <summary>
			/// Tên Icon
			/// </summary>
			public string IconName { get; set; }

			/// <summary>
			/// Vị trí X
			/// </summary>
			public int IconPosX { get; set; }

			/// <summary>
			/// Vị trí Y
			/// </summary>
			public int IconPosY { get; set; }

			/// <summary>
			/// Loại địa danh
			/// </summary>
			public PlaceType Type { get; set; }

			/// <summary>
			/// Chuyển đối tượng từ XMLNode
			/// </summary>
			/// <param name="xmlNode"></param>
			/// <returns></returns>
			public static Place Parse(XElement xmlNode)
			{
				return new Place()
				{
					MapCode = int.Parse(xmlNode.Attribute("MapCode").Value),
					IconName = xmlNode.Attribute("IconName").Value,
					IconPosX = int.Parse(xmlNode.Attribute("IconPosX").Value),
					IconPosY = int.Parse(xmlNode.Attribute("IconPosY").Value),
					Type = (PlaceType) int.Parse(xmlNode.Attribute("Type").Value),
				};
			}
		}

		/// <summary>
		/// Chỉ đường
		/// </summary>
		public class WayPort
		{
			/// <summary>
			/// ID bản đồ
			/// </summary>
			public int MapCode { get; set; }

			/// <summary>
			/// Vị trí X
			/// </summary>
			public int IconPosX { get; set; }

			/// <summary>
			/// Vị trí Y
			/// </summary>
			public int IconPosY { get; set; }

			/// <summary>
			/// Chuyển đối tượng từ XMLNode
			/// </summary>
			/// <param name="xmlNode"></param>
			/// <returns></returns>
			public static WayPort Parse(XElement xmlNode)
			{
				return new WayPort()
				{
					MapCode = int.Parse(xmlNode.Attribute("MapCode").Value),
					IconPosX = int.Parse(xmlNode.Attribute("IconPosX").Value),
					IconPosY = int.Parse(xmlNode.Attribute("IconPosY").Value),
				};
			}
		}

		/// <summary>
		/// Khu vực
		/// </summary>
		public class Area
		{
			/// <summary>
			/// ID khu vực
			/// </summary>
			public int ID { get; set; }

			/// <summary>
			/// Tên Res bản đồ khu vực
			/// </summary>
			public string ImageFileName { get; set; }

			/// <summary>
			/// Danh sách địa danh
			/// </summary>
			public List<Place> Places { get; set; }

			/// <summary>
			/// Danh sách chỉ đường
			/// </summary>
			public List<WayPort> WayPorts { get; set; }

			/// <summary>
			/// Chuyển đối tượng từ XMLNode
			/// </summary>
			/// <param name="xmlNode"></param>
			/// <returns></returns>
			public static Area Parse(XElement xmlNode)
			{
				Area area = new Area()
				{
					ID = int.Parse(xmlNode.Attribute("ID").Value),
					ImageFileName = xmlNode.Attribute("ImageFileName").Value,
					Places = new List<Place>(),
					WayPorts = new List<WayPort>(),
				};

				foreach (XElement node in xmlNode.Elements("WayPort"))
				{
					area.WayPorts.Add(WayPort.Parse(node));
				}

				foreach (XElement node in xmlNode.Elements("Place"))
				{
					area.Places.Add(Place.Parse(node));
				}

				return area;
			}
		}

		/// <summary>
		/// Thông tin bản đồ thế giới
		/// </summary>
		public World WorldMap { get; set; }

		/// <summary>
		/// Danh sách khu vực
		/// </summary>
		public List<Area> Areas { get; set; }

		/// <summary>
		/// Chuyển đối tượng từ XMLNode
		/// </summary>
		/// <param name="xmlNode"></param>
		/// <returns></returns>
		public static WorldMapXML Parse(XElement xmlNode)
		{
			WorldMapXML worldMap = new WorldMapXML()
			{
				WorldMap = World.Parse(xmlNode.Element("World")),
				Areas = new List<Area>(),
			};

			foreach (XElement node in xmlNode.Elements("Area"))
			{
				worldMap.Areas.Add(Area.Parse(node));
			}

			return worldMap;
		}
	}
}
