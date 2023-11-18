using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace GameServer.KiemThe.Core.Activity.PlayerPray
{
	/// <summary>
	/// Thông tin chúc phúc
	/// </summary>
	public class PlayerPrayXML
	{
		/// <summary>
		/// Kết quả 5 lượt quay
		/// </summary>
		public string Result { get; set; }

		/// <summary>
		/// Danh sách vật phẩm thưởng
		/// </summary>
		public List<KeyValuePair<int, int>> Items { get; set; }

		/// <summary>
		/// Danh sách Buff
		/// </summary>
		public List<KeyValuePair<int, int>> Buffs { get; set; }

		/// <summary>
		/// Danh vọng chúc phúc nhận được
		/// </summary>
		public int Repute { get; set; }

		/// <summary>
		/// Mô tả kết quả quay
		/// </summary>
		public string PrayWord { get; set; }

		/// <summary>
		/// Bói toán việc nên làm
		/// </summary>
		public string PrayThing { get; set; }

		/// <summary>
		/// Chuyển đối tượng từ XMLNode
		/// </summary>
		/// <param name="xmlNode"></param>
		/// <returns></returns>
		public static PlayerPrayXML Parse(XElement xmlNode)
		{
			PlayerPrayXML playerPray = new PlayerPrayXML()
			{
				Result = xmlNode.Attribute("Result").Value,
				Repute = int.Parse(xmlNode.Attribute("Repute").Value),
				PrayWord = xmlNode.Attribute("PrayWord").Value,
				PrayThing = xmlNode.Attribute("PrayThing").Value,
				Items = new List<KeyValuePair<int, int>>(),
				Buffs = new List<KeyValuePair<int, int>>(),
			};

			foreach (string pairString in xmlNode.Attribute("Items").Value.Split(';'))
			{
				try
				{
					string[] fields = pairString.Split('_');
					int itemID = int.Parse(fields[0]);
					int number = int.Parse(fields[1]);
					playerPray.Items.Add(new KeyValuePair<int, int>(itemID, number));
				}
				catch (Exception) { }
			}

			foreach (string pairString in xmlNode.Attribute("Buffs").Value.Split(';'))
			{
				try
				{
					string[] fields = pairString.Split('_');
					int buffID = int.Parse(fields[0]);
					int level = int.Parse(fields[1]);
					playerPray.Buffs.Add(new KeyValuePair<int, int>(buffID, level));
				}
				catch (Exception) { }
			}

			return playerPray;
		}
	}
}
