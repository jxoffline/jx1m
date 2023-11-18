using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace GameServer.KiemThe.Core.Item
{
	/// <summary>
	/// Tu Luyện Châu
	/// </summary>
	public class XiuLianZhu
	{
		/// <summary>
		/// Thiết lập
		/// </summary>
		public class DataConfig
		{
			/// <summary>
			/// Thời gian tu luyện tối đa
			/// </summary>
			public int LimitAddTime { get; set; }

			/// <summary>
			/// Số giờ Tu Luyện cộng thêm mỗi ngày
			/// </summary>
			public int HourAddPerDay { get; set; }

			/// <summary>
			/// Buff x2 kinh nghiệm đánh quái
			/// </summary>
			public int DoubleExpBuff { get; set; }

			/// <summary>
			/// Buff tăng may mắn
			/// </summary>
			public int LuckyBuff { get; set; }

			/// <summary>
			/// Chuyển đối tượng từ XMLNode
			/// </summary>
			/// <param name="xmlNode"></param>
			/// <returns></returns>
			public static DataConfig Parse(XElement xmlNode)
			{
				return new DataConfig()
				{
					LimitAddTime = int.Parse(xmlNode.Attribute("LimitAddTime").Value),
					HourAddPerDay = int.Parse(xmlNode.Attribute("HourAddPerDay").Value),
					DoubleExpBuff = int.Parse(xmlNode.Attribute("DoubleExpBuff").Value),
					LuckyBuff = int.Parse(xmlNode.Attribute("LuckyBuff").Value),
				};
			}
		}

		/// <summary>
		/// Giá trị kinh nghiệm tu luyện
		/// </summary>
		public class ExpValue
		{
			/// <summary>
			/// ID nhóm cấp độ
			/// </summary>
			public int ID { get; set; }

			/// <summary>
			/// Giá trị
			/// </summary>
			public int Value { get; set; }

			/// <summary>
			/// Chuyển đối tượng từ XMLNode
			/// </summary>
			/// <param name="xmlNode"></param>
			/// <returns></returns>
			public static ExpValue Parse(XElement xmlNode)
			{
				return new ExpValue()
				{
					ID = int.Parse(xmlNode.Attribute("ID").Value),
					Value = int.Parse(xmlNode.Attribute("Value").Value),
				};
			}
		}

		/// <summary>
		/// Thiết lập
		/// </summary>
		public DataConfig Config { get; set; }

		/// <summary>
		/// Giới hạn kinh nghiệm theo nhóm cấp
		/// </summary>
		public Dictionary<int, ExpValue> ExpLimit { get; set; }

		/// <summary>
		/// Kinh nghiệm có được tương ứng số giờ tu luyện được chọn
		/// </summary>
		public Dictionary<int, ExpValue> ExpByHour { get; set; }

		/// <summary>
		/// Chuyển đối tượng từ XMLNode
		/// </summary>
		/// <param name="xmlNode"></param>
		/// <returns></returns>
		public static XiuLianZhu Parse(XElement xmlNode)
		{
			XiuLianZhu xlz = new XiuLianZhu()
			{
				Config = DataConfig.Parse(xmlNode.Element("Config")),
				ExpLimit = new Dictionary<int, ExpValue>(),
				ExpByHour = new Dictionary<int, ExpValue>(),
			};

			foreach (XElement node in xmlNode.Element("ExpLimit").Elements())
			{
				ExpValue expValue = ExpValue.Parse(node);
				xlz.ExpLimit[expValue.ID] = expValue;
			}

			foreach (XElement node in xmlNode.Element("ExpByHour").Elements())
			{
				ExpValue expValue = ExpValue.Parse(node);
				xlz.ExpByHour[expValue.ID] = expValue;
			}

			return xlz;
		}
	}
}
