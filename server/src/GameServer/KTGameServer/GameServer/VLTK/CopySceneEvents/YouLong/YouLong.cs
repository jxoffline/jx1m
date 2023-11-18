using GameServer.KiemThe.Entities;
using GameServer.Logic;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace GameServer.KiemThe.CopySceneEvents.YouLongGe
{
	/// <summary>
	/// Định nghĩa phụ bản Du Long Các
	/// </summary>
	public static class YouLong
	{
		#region Define
		/// <summary>
		/// Thông tin phần thưởng
		/// </summary>
		public class AwardInfo
		{
			/// <summary>
			/// Thông tin từng vật phẩm thưởng
			/// </summary>
			public class Award
			{
				/// <summary>
				/// ID vật phẩm thưởng
				/// </summary>
				public int ID { get; set; }

				/// <summary>
				/// Số lượng
				/// </summary>
				public int Number { get; set; }

				/// <summary>
				/// Tỷ lệ nhận được
				/// </summary>
				public int Rate { get; set; }

				/// <summary>
				/// Tỷ lệ xuất hiện trong danh sách
				/// </summary>
				public int AppearRate { get; set; }

				/// <summary>
				/// Số tiền đổi được
				/// </summary>
				public int NumberOfCoins { get; set; }

				/// <summary>
				/// Chuyển đối tượng từ XMLNode
				/// </summary>
				/// <param name="xmlNode"></param>
				/// <returns></returns>
				public static Award Parse(XElement xmlNode)
				{
					return new Award()
					{
						ID = int.Parse(xmlNode.Attribute("ID").Value),
						Number = int.Parse(xmlNode.Attribute("Number").Value),
						Rate = int.Parse(xmlNode.Attribute("Rate").Value),
						AppearRate = int.Parse(xmlNode.Attribute("AppearRate").Value),
						NumberOfCoins = int.Parse(xmlNode.Attribute("NumberOfCoins").Value),
					};
				}
			}
			
			/// <summary>
			/// Số lượng trong khung
			/// </summary>
			public int Count { get; set; }

			/// <summary>
			/// Số lần chạy lại tối đa
			/// </summary>
			public int MaxTryTime { get; set; }

			/// <summary>
			/// Danh sách phần thưởng
			/// </summary>
			public List<Award> Awards { get; set; }

			/// <summary>
			/// Chuyển đối tượng từ XMLNode
			/// </summary>
			/// <param name="xmlNode"></param>
			/// <returns></returns>
			public static AwardInfo Parse(XElement xmlNode)
			{
				AwardInfo awardInfo = new AwardInfo()
				{
					Count = int.Parse(xmlNode.Attribute("Count").Value),
					MaxTryTime = int.Parse(xmlNode.Attribute("MaxTryTime").Value),
					Awards = new List<Award>(),
				};

				foreach (XElement node in xmlNode.Elements("Award"))
				{
					awardInfo.Awards.Add(Award.Parse(node));
				}

				return awardInfo;
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
			/// Vị trí tiến vào X
			/// </summary>
			public int EnterPosX { get; set; }

			/// <summary>
			/// Vị trí tiến vào Y
			/// </summary>
			public int EnterPosY { get; set; }

			/// <summary>
			/// Chuyển đối tượng từ XMLNode
			/// </summary>
			/// <param name="xmlNode"></param>
			/// <returns></returns>
			public static MapInfo Parse(XElement xmlNode)
			{
				return new MapInfo()
				{
					ID = int.Parse(xmlNode.Attribute("ID").Value),
					EnterPosX = int.Parse(xmlNode.Attribute("EnterPosX").Value),
					EnterPosY = int.Parse(xmlNode.Attribute("EnterPosY").Value),
				};
			}
		}

		/// <summary>
		/// Thông tin NPC
		/// </summary>
		public class NPCInfo
		{
			/// <summary>
			/// ID NPC
			/// </summary>
			public int ID { get; set; }

			/// <summary>
			/// Tên NPC
			/// <para>Bỏ trống sẽ lấy ở File cấu hình</para>
			/// </summary>
			public string Name { get; set; }

			/// <summary>
			/// Danh hiệu NPC
			/// <para>Bỏ trống sẽ lấy ở File cấu hình</para>
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
			/// Chuyển đối tượng từ XMLNode
			/// </summary>
			/// <param name="xmlNode"></param>
			/// <returns></returns>
			public static NPCInfo Parse(XElement xmlNode)
			{
				return new NPCInfo()
				{
					ID = int.Parse(xmlNode.Attribute("ID").Value),
					Name = xmlNode.Attribute("Name").Value,
					Title = xmlNode.Attribute("Title").Value,
					PosX = int.Parse(xmlNode.Attribute("PosX").Value),
					PosY = int.Parse(xmlNode.Attribute("PosY").Value),
				};
			}
		}

		/// <summary>
		/// Thông tin Boss
		/// </summary>
		public class BossInfo
		{
			/// <summary>
			/// ID NPC
			/// </summary>
			public int ID { get; set; }

			/// <summary>
			/// Tên NPC
			/// <para>Bỏ trống sẽ lấy ở File cấu hình</para>
			/// </summary>
			public string Name { get; set; }

			/// <summary>
			/// Danh hiệu NPC
			/// <para>Bỏ trống sẽ lấy ở File cấu hình</para>
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
			/// Danh sách kỹ năng sẽ sử dụng
			/// </summary>
			public List<SkillLevelRef> Skills { get; set; }

			/// <summary>
			/// Danh sách vòng sáng sẽ sử dụng
			/// </summary>
			public List<SkillLevelRef> Auras { get; set; }

			/// <summary>
			/// Chuyển đối tượng từ XMLNode
			/// </summary>
			/// <param name="xmlNode"></param>
			/// <returns></returns>
			public static BossInfo Parse(XElement xmlNode)
			{
				BossInfo monsterInfo = new BossInfo()
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
					Skills = new List<SkillLevelRef>(),
					Auras = new List<SkillLevelRef>(),
				};

				/// Chuỗi mã hóa danh sách kỹ năng sử dụng
				string skillsString = xmlNode.Attribute("Skills").Value;
				/// Nếu có kỹ năng
				if (!string.IsNullOrEmpty(skillsString))
				{
					/// Duyệt danh sách kỹ năng
					foreach (string skillStr in skillsString.Split(';'))
					{
						string[] fields = skillStr.Split('_');
						try
						{
							int skillID = int.Parse(fields[0]);
							int skillLevel = int.Parse(fields[1]);
							int cooldown = int.Parse(fields[2]);

							/// Thông tin kỹ năng tương ứng
							SkillDataEx skillData = KSkill.GetSkillData(skillID);
							/// Nếu kỹ năng không tồn tại
							if (skillData == null)
							{
								throw new Exception(string.Format("Skill ID = {0} not found!", skillID));
							}

							/// Nếu cấp độ dưới 0
							if (skillLevel <= 0)
							{
								throw new Exception(string.Format("Skill ID = {0} level must be greater than 0", skillID));
							}

							/// Kỹ năng theo cấp
							SkillLevelRef skillRef = new SkillLevelRef()
							{
								Data = skillData,
								AddedLevel = skillLevel,
								Exp = cooldown,
							};

							/// Thêm vào danh sách
							monsterInfo.Skills.Add(skillRef);
						}
						catch (Exception ex)
						{
							Console.WriteLine(ex.ToString());
							LogManager.WriteLog(LogTypes.Exception, ex.ToString());
						}
					}
				}

				/// Chuỗi mã hóa danh sách vòng sáng sử dụng
				string aurasString = xmlNode.Attribute("Auras").Value;
				/// Nếu có kỹ năng
				if (!string.IsNullOrEmpty(aurasString))
				{
					/// Duyệt danh sách kỹ năng
					foreach (string skillStr in aurasString.Split(';'))
					{
						string[] fields = skillStr.Split('_');
						try
						{
							int skillID = int.Parse(fields[0]);
							int skillLevel = int.Parse(fields[1]);

							/// Thông tin kỹ năng tương ứng
							SkillDataEx skillData = KSkill.GetSkillData(skillID);
							/// Nếu kỹ năng không tồn tại
							if (skillData == null)
							{
								throw new Exception(string.Format("Skill ID = {0} not found!", skillID));
							}

							/// Nếu cấp độ dưới 0
							if (skillLevel <= 0)
							{
								throw new Exception(string.Format("Skill ID = {0} level must be greater than 0", skillID));
							}

							/// Nếu không phải vòng sáng
							if (!skillData.IsArua)
							{
								throw new Exception(string.Format("Skill ID = {0} is not Aura!", skillID));
							}

							/// Kỹ năng theo cấp
							SkillLevelRef skillRef = new SkillLevelRef()
							{
								Data = skillData,
								AddedLevel = skillLevel,
							};

							/// Thêm vào danh sách
							monsterInfo.Auras.Add(skillRef);
						}
						catch (Exception ex)
						{
							Console.WriteLine(ex.ToString());
							LogManager.WriteLog(LogTypes.Exception, ex.ToString());
						}
					}
				}

				/// Trả về kết quả
				return monsterInfo;
			}
		}

		/// <summary>
		/// Thông tin vật phẩm
		/// </summary>
		public class ItemInfo
		{
			/// <summary>
			/// ID Nguyệt Ảnh Thạch
			/// </summary>
			public int MoonStoneID { get; set; }

			/// <summary>
			/// ID Tiền Du Long
			/// </summary>
			public int YouLongCoinID { get; set; }

			/// <summary>
			/// Chuyển đối tượng từ XMLNode
			/// </summary>
			/// <param name="xmlNode"></param>
			/// <returns></returns>
			public static ItemInfo Parse(XElement xmlNode)
			{
				return new ItemInfo()
				{
					MoonStoneID = int.Parse(xmlNode.Attribute("MoonStoneID").Value),
					YouLongCoinID = int.Parse(xmlNode.Attribute("YouLongCoinID").Value),
				};
			}
		}

		/// <summary>
		/// Thông tin phụ bản Du Long
		/// </summary>
		public class YouLongCopyScene
		{
			/// <summary>
			/// Thời gian tồn tại (Mili-giây)
			/// </summary>
			public int Duration { get; set; }

			/// <summary>
			/// Cấp độ yêu cầu
			/// </summary>
			public int RequireLevel { get; set; }

			/// <summary>
			/// Số lượt tham gia tối đa trong ngày
			/// </summary>
			public int LimitRoundPerDay { get; set; }

			/// <summary>
			/// Thời gian tự đẩy ra khi không có khiêu chiến mới (Mili-giây)
			/// </summary>
			public int KickOutIfNoChallengeFor { get; set; }

			/// <summary>
			/// Thời gian tỷ thí tối đa (Mili-giây)
			/// </summary>
			public int ChallengeDuration { get; set; }

			/// <summary>
			/// Vật phẩm yêu cầu
			/// </summary>
			public int RequireItem { get; set; }

			/// <summary>
			/// Danh sách vật phẩm yêu cầu hoặc gì đó
			/// </summary>
			public ItemInfo Items { get; set; }

			/// <summary>
			/// Thông tin bản đồ
			/// </summary>
			public MapInfo Map { get; set; }

			/// <summary>
			/// Thông tin NPC
			/// </summary>
			public NPCInfo NPC { get; set; }

			/// <summary>
			/// Thông tin Boss
			/// </summary>
			public BossInfo Boss { get; set; }

			/// <summary>
			/// Danh sách vật phẩm thưởng
			/// </summary>
			public AwardInfo Awards { get; set; }

			/// <summary>
			/// Chuyển đối tượng từ XMLNode
			/// </summary>
			/// <param name="xmlNode"></param>
			/// <returns></returns>
			public static YouLongCopyScene Parse(XElement xmlNode)
			{
				return new YouLongCopyScene()
				{
					Duration = int.Parse(xmlNode.Attribute("Duration").Value),
					RequireLevel = int.Parse(xmlNode.Attribute("RequireLevel").Value),
					LimitRoundPerDay = int.Parse(xmlNode.Attribute("LimitRoundPerDay").Value),
					KickOutIfNoChallengeFor = int.Parse(xmlNode.Attribute("KickOutIfNoChallengeFor").Value),
					ChallengeDuration = int.Parse(xmlNode.Attribute("ChallengeDuration").Value),
					RequireItem = int.Parse(xmlNode.Attribute("RequireItem").Value),
					Items = ItemInfo.Parse(xmlNode.Element("Item")),
					Map = MapInfo.Parse(xmlNode.Element("Map")),
					NPC = NPCInfo.Parse(xmlNode.Element("NPC")),
					Boss = BossInfo.Parse(xmlNode.Element("Boss")),
					Awards = AwardInfo.Parse(xmlNode.Element("Awards")),
				};
			}
		}
		#endregion

		/// <summary>
		/// Dữ liệu phụ bản Du Long
		/// </summary>
		public static YouLongCopyScene Data { get; private set; }

		#region Core
		/// <summary>
		/// Khởi tạo dữ liệu
		/// </summary>
		public static void Init()
		{
			XElement xmlNode = KTGlobal.ReadXMLData("Config/KT_CopyScenes/YouLong.xml");
			YouLong.Data = YouLongCopyScene.Parse(xmlNode);
		}
		#endregion
	}
}
