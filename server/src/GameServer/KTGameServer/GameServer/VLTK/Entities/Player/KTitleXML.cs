using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace GameServer.KiemThe.Entities.Player
{
	/// <summary>
	/// Định nghĩa danh hiệu của người chơi
	/// </summary>
	public class KTitleXML
	{
		/// <summary>
		/// ID danh hiệu
		/// </summary>
		public int ID { get; set; }

		/// <summary>
		/// Tên danh hiệu
		/// </summary>
		public string Text { get; set; }

		/// <summary>
		/// Mô tả danh hiệu
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		/// Thời gian tồn tại (giờ)
		/// </summary>
		public int Duration { get; set; }

		/// <summary>
		/// Chuyển đối tượng từ XMLNode
		/// </summary>
		/// <param name="xmlNode"></param>
		/// <returns></returns>
		public static KTitleXML Parse(XElement xmlNode)
		{
			return new KTitleXML()
			{
				ID = int.Parse(xmlNode.Attribute("ID").Value),
				Text = xmlNode.Attribute("Text").Value,
				Description = xmlNode.Attribute("Description").Value,
				Duration = int.Parse(xmlNode.Attribute("Duration").Value),
			};
		}
	}

	/// <summary>
	/// Định nghĩa danh hiệu đặc biệt
	/// </summary>
	public class KSpecialTitleXML
    {
		/// <summary>
		/// ID danh hiệu
		/// </summary>
		public int ID { get; set; }

		/// <summary>
		/// Thời gian tồn tại (mili-giây)
		/// </summary>
		public long Duration { get; set; }

		/// <summary>
		/// ID hiệu ứng
		/// </summary>
		public int EffectID { get; set; }

		/// <summary>
		/// Đường dẫn File AssetBundle chứa ảnh
		/// </summary>
		public string BundleDir { get; set; }

		/// <summary>
		/// Tên Atlas chứa ảnh
		/// </summary>
		public string AtlasName { get; set; }

		/// <summary>
		/// Thời gian thực thi hiệu ứng (giây)
		/// </summary>
		public int AnimationSpeed { get; set; }

		/// <summary>
		/// Danh sách tên Sprite tạo nên hiệu ứng
		/// </summary>
		public List<string> SpriteNames { get; set; }

		/// <summary>
		/// Chuyển đối tượng từ XMLNode
		/// </summary>
		/// <param name="xmlNode"></param>
		/// <returns></returns>
		public static KSpecialTitleXML Parse(XElement xmlNode)
        {
			/// Tạo mới
			KSpecialTitleXML data = new KSpecialTitleXML()
			{
				ID = int.Parse(xmlNode.Attribute("ID").Value),
				Duration = long.Parse(xmlNode.Attribute("Duration").Value),
				EffectID = int.Parse(xmlNode.Attribute("EffectID").Value),
				BundleDir = xmlNode.Attribute("BundleDir").Value,
				AtlasName = xmlNode.Attribute("AtlasName").Value,
				AnimationSpeed = int.Parse(xmlNode.Attribute("AnimationSpeed").Value),
				SpriteNames = new List<string>(),
			};

			/// Duyệt danh sách ảnh
			foreach (XElement node in xmlNode.Elements("Sprite"))
            {
				/// Thêm vào danh sách
				data.SpriteNames.Add(node.Attribute("Name").Value);
            }

			/// Trả về kết quả
			return data;
        }
    }
}
