using System.Collections.Generic;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace FS.VLTK.Entities.Config
{
	/// <summary>
	/// Thông tin danh vọng theo cấp
	/// </summary>
	public class ReputeLevel
	{
		/// <summary>
		/// ID
		/// </summary>
		[XmlAttribute(AttributeName = "id")]
		public int Id { get; set; }

		/// <summary>
		/// Tên
		/// </summary>
		[XmlAttribute(AttributeName = "name")]
		public string Name { get; set; }

		/// <summary>
		/// Điểm cần để thăng cấp
		/// </summary>
		[XmlAttribute(AttributeName = "level_up")]
		public int LevelUp { get; set; }

		/// <summary>
		/// Yêu cầu cấp
		/// </summary>
		[XmlAttribute(AttributeName = "require_level")]
		public int RequireLevel { get; set; }

		/// <summary>
		/// Chuyển đối tượng từ XMLNode
		/// </summary>
		/// <param name="xmlNode"></param>
		/// <returns></returns>
		public static ReputeLevel Parse(XElement xmlNode)
        {
			return new ReputeLevel()
			{
				Id = int.Parse(xmlNode.Attribute("id").Value),
				Name = xmlNode.Attribute("name").Value,
				LevelUp = int.Parse(xmlNode.Attribute("level_up").Value),
				RequireLevel = xmlNode.Attribute("require_level") != null ? int.Parse(xmlNode.Attribute("require_level").Value) : 0,
			};
        }
	}

	/// <summary>
	/// Thông tin lớp danh vọng
	/// </summary>
	[XmlRoot(ElementName = "class")]
	public class ReputeClass
	{
		/// <summary>
		/// Danh sách cấp độ
		/// </summary>
		[XmlElement(ElementName = "level")]
		public List<ReputeLevel> Level { get; set; }

		/// <summary>
		/// ID lớp
		/// </summary>
		[XmlAttribute(AttributeName = "id")]
		public int Id { get; set; }

		/// <summary>
		/// Tên lớp
		/// </summary>
		[XmlAttribute(AttributeName = "name")]
		public string Name { get; set; }

		/// <summary>
		/// Cấp độ mặc định
		/// </summary>
		[XmlAttribute(AttributeName = "default_level")]
		public int DefaultLevel { get; set; }

		/// <summary>
		/// Giới hạn tối đa mỗi ngày cắn được bao nhiêu điểm
		/// </summary>
		[XmlAttribute(AttributeName = "daily_limit")]
		public int DailyLimit { get; set; }

		/// <summary>
		/// Chưa dùng
		/// </summary>
		[XmlAttribute(AttributeName = "tips")]
		public string Tips { get; set; }

		/// <summary>
		/// Chuyển đối tượng từ XMLNode
		/// </summary>
		/// <param name="xmlNode"></param>
		/// <returns></returns>
		public static ReputeClass Parse(XElement xmlNode)
		{
			ReputeClass reputeClass = new ReputeClass()
			{
				Id = int.Parse(xmlNode.Attribute("id").Value),
				Name = xmlNode.Attribute("name").Value,
				DefaultLevel = int.Parse(xmlNode.Attribute("default_level").Value),
				DailyLimit = int.Parse(xmlNode.Attribute("daily_limit").Value),
				Tips = xmlNode.Attribute("tips") != null ? xmlNode.Attribute("tips").Value : "",
				Level = new List<ReputeLevel>(),
			};

			foreach (XElement node in xmlNode.Elements("level"))
            {
				reputeClass.Level.Add(ReputeLevel.Parse(node));
			}

			return reputeClass;
		}
	}

	/// <summary>
	/// Danh sách loại danh vọng
	/// </summary>
	[XmlRoot(ElementName = "camp")]
	public class ReputeCamp
	{
		/// <summary>
		/// Danh sách lớp tương ứng
		/// </summary>
		[XmlElement(ElementName = "class")]
		public List<ReputeClass> Class { get; set; }

		/// <summary>
		/// ID loại
		/// </summary>
		[XmlAttribute(AttributeName = "id")]
		public int Id { get; set; }

		/// <summary>
		/// Tên loại
		/// </summary>
		[XmlAttribute(AttributeName = "name")]
		public string Name { get; set; }

		/// <summary>
		/// Chuyển đối tượng từ XMLNode
		/// </summary>
		/// <param name="xmlNode"></param>
		/// <returns></returns>
		public static ReputeCamp Parse(XElement xmlNode)
		{
			ReputeCamp reputeCamp = new ReputeCamp()
			{
				Id = int.Parse(xmlNode.Attribute("id").Value),
				Name = xmlNode.Attribute("name").Value,
				Class = new List<ReputeClass>(),
			};

			foreach (XElement node in xmlNode.Elements("class"))
			{
				reputeCamp.Class.Add(ReputeClass.Parse(node));
			}

			return reputeCamp;
		}
	}

	/// <summary>
	/// Lớp mô tả hệ thống danh vọng
	/// </summary>
	[XmlRoot(ElementName = "repute")]
	public class Repute
	{
		/// <summary>
		/// Danh sách danh vọng
		/// </summary>
		[XmlElement(ElementName = "camp")]
		public List<ReputeCamp> Camp { get; set; }

		/// <summary>
		/// Chuyển đối tượng từ XMLNode
		/// </summary>
		/// <param name="xmlNode"></param>
		/// <returns></returns>
		public static Repute Parse(XElement xmlNode)
		{
			Repute repute = new Repute()
			{
				Camp = new List<ReputeCamp>(),
			};

			foreach (XElement node in xmlNode.Elements("camp"))
			{
				repute.Camp.Add(ReputeCamp.Parse(node));
			}

			return repute;
		}
	}
}
