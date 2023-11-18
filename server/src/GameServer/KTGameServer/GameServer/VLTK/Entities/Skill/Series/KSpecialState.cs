using System.Collections.Generic;
using System.Xml.Serialization;

namespace GameServer.KiemThe.Entities
{
	/// <summary>
	/// Trạng thái ngũ hành
	/// </summary>
	[XmlRoot(ElementName = "KSpecialState")]
	public class KSpecialState
	{
		/// <summary>
		/// ID trạng thái
		/// </summary>
		[XmlAttribute(AttributeName = "StateID")]
		public string StateID { get; set; }

		/// <summary>
		/// Tên trạng thái
		/// </summary>
		[XmlAttribute(AttributeName = "Name")]
		public string Name { get; set; }

		/// <summary>
		/// ID kỹ năng biểu diễn trạng thái tương ứng
		/// </summary>
		[XmlAttribute(AttributeName = "SkillID")]
		public int SkillID { get; set; }

		/// <summary>
		/// Số Frame biểu diễn tương ứng trong trường hợp mặc định không có thời gian
		/// <para>1 Frame = 1/18 giây</para>
		/// </summary>
		[XmlAttribute(AttributeName = "MaxFrame")]
		public string MaxFrame { get; set; }
	}

	/// <summary>
	/// Danh sách trạng thái ngũ hành
	/// </summary>
	[XmlRoot(ElementName = "ArrayOfKSpecialState")]
	public class ArrayOfKSpecialState
	{
		[XmlElement(ElementName = "KSpecialState")]
		public List<KSpecialState> KSpecialState { get; set; }
		[XmlAttribute(AttributeName = "xsi", Namespace = "http://www.w3.org/2000/xmlns/")]
		public string Xsi { get; set; }
		[XmlAttribute(AttributeName = "xsd", Namespace = "http://www.w3.org/2000/xmlns/")]
		public string Xsd { get; set; }
	}
}