using GameServer.KiemThe.Entities;
using GameServer.Logic;
using System.Collections.Generic;
using System.Xml.Linq;

namespace GameServer.KiemThe.CopySceneEvents
{
	/// <summary>
	/// Bí cảnh
	/// </summary>
	public static class MiJing
	{
		#region Define
		/// <summary>
		/// Thông tin quái
		/// </summary>
		public class MonsterInfo
		{
			/// <summary>
			/// ID quái
			/// </summary>
			public int ID { get; set; }

			/// <summary>
			/// Tên quái
			/// </summary>
			public string Name { get; set; }

			/// <summary>
			/// Danh hiệu quái
			/// </summary>
			public string Title { get; set; }

			/// <summary>
			/// Vị trí X
			/// </summary>
			public int PosX { get; set; }

			/// <summary>
			/// Vị trí Y
			/// </summary>
			public int PosY { get; set; }

			/// <summary>
			/// Sinh lực cơ bản
			/// </summary>
			public int BaseHP { get; set; }

			/// <summary>
			/// Sinh lực tăng thêm mỗi cấp
			/// </summary>
			public int HPIncreaseEachLevel { get; set; }

			/// <summary>
			/// Loại AI
			/// </summary>
			public MonsterAIType AIType { get; set; }

			/// <summary>
			/// ID Script AI điều khiển
			/// </summary>
			public int AIScriptID { get; set; }

			/// <summary>
			/// Thời gian tái sinh
			/// </summary>
			public int RespawnTicks { get; set; }

			/// <summary>
			/// Chuyển đối tượng từ XMLNode
			/// </summary>
			/// <param name="xmlNode"></param>
			/// <returns></returns>
			public static MonsterInfo Parse(XElement xmlNode)
			{
				MonsterInfo monsterInfo = new MonsterInfo()
				{
					ID = int.Parse(xmlNode.Attribute("ID").Value),
					Name = xmlNode.Attribute("Name").Value,
					Title = xmlNode.Attribute("Title").Value,
					PosX = int.Parse(xmlNode.Attribute("PosX").Value),
					PosY = int.Parse(xmlNode.Attribute("PosY").Value),
					BaseHP = int.Parse(xmlNode.Attribute("BaseHP").Value),
					HPIncreaseEachLevel = int.Parse(xmlNode.Attribute("HPIncreaseEachLevel").Value),
					AIType = (MonsterAIType) int.Parse(xmlNode.Attribute("AIType").Value),
					AIScriptID = int.Parse(xmlNode.Attribute("AIScriptID").Value),
					RespawnTicks = int.Parse(xmlNode.Attribute("RespawnTicks").Value),
				};

				/// Trả về kết quả
				return monsterInfo;
			}
		}

		/// <summary>
		/// Thông tin bản đồ
		/// </summary>
		public class MapInfo
		{
			/// <summary>
			/// ID bản đồ
			/// </summary>
			public int ID { get; set; }

			/// <summary>
			/// Vị trí vào X
			/// </summary>
			public int EnterPosX { get; set; }

			/// <summary>
			/// Vị trí vào Y
			/// </summary>
			public int EnterPosY { get; set; }

			/// <summary>
			/// Chuyển đối tượng từ XMLNode
			/// </summary>
			/// <param name="xmlNode"></param>
			/// <returns></returns>
			public static MapInfo Parse(XElement xmlNode)
			{
				MapInfo mapInfo = new MapInfo()
				{
					ID = int.Parse(xmlNode.Attribute("ID").Value),
					EnterPosX = int.Parse(xmlNode.Attribute("EnterPosX").Value),
					EnterPosY = int.Parse(xmlNode.Attribute("EnterPosY").Value),
				};
				return mapInfo;
			}
		}
		#endregion

		/// <summary>
		/// Số lượng Bí cảnh tối đa
		/// </summary>
		public const int LimitCopyScenes = 50;

		/// <summary>
		/// Thời gian tồn tại
		/// </summary>
		public static int Duration { get; private set; }

		/// <summary>
		/// Yêu cầu cấp độ
		/// </summary>
		public static int RequireLevel { get; private set; }

		/// <summary>
		/// Số lượt vào tối đa trong ngày
		/// </summary>
		public static int LimitRoundPerDay { get; private set; }

		/// <summary>
		/// Thông tin bản đồ
		/// </summary>
		public static MapInfo Map { get; set; }

		/// <summary>
		/// ID vật phẩm Bản Đồ Bí Cảnh
		/// </summary>
		public static int MapItem { get; private set; }

		/// <summary>
		/// ID Buff x2 kinh nghiệm
		/// </summary>
		public static int DoubleExpBuff { get; private set; }

		/// <summary>
		/// Danh sách quái
		/// </summary>
		public static List<MonsterInfo> Monsters { get; private set; } = new List<MonsterInfo>();

		#region Core
		/// <summary>
		/// Khởi tạo dữ liệu
		/// </summary>
		public static void Init()
		{
			XElement xmlNode = KTGlobal.ReadXMLData("Config/KT_CopyScenes/MiJing.xml");
			MiJing.Duration = int.Parse(xmlNode.Attribute("Duration").Value);
			MiJing.RequireLevel = int.Parse(xmlNode.Attribute("RequireLevel").Value);
			MiJing.LimitRoundPerDay = int.Parse(xmlNode.Attribute("LimitRoundPerDay").Value);
			MiJing.Map = MapInfo.Parse(xmlNode.Element("MapInfo"));
			MiJing.MapItem = int.Parse(xmlNode.Element("MapItem").Attribute("ID").Value);
			MiJing.DoubleExpBuff = int.Parse(xmlNode.Element("DoubleExpBuff").Attribute("ID").Value);
			/// Duyệt danh sách quái
			foreach (XElement node in xmlNode.Element("Monsters").Elements("Monster"))
			{
				MiJing.Monsters.Add(MonsterInfo.Parse(node));
			}
		}
		#endregion
	}
}
