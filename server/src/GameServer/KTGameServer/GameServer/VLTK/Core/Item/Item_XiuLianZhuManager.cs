using GameServer.KiemThe.Entities;
using GameServer.KiemThe.Logic;
using GameServer.Logic;
using System.Linq;
using System.Xml.Linq;

namespace GameServer.KiemThe.Core.Item
{
	/// <summary>
	/// Quản lý Tu Luyện Châu
	/// </summary>
	public static class ItemXiuLianZhuManager
	{
		#region Core
		/// <summary>
		/// Dữ liệu Tu Luyện Châu
		/// </summary>
		private static XiuLianZhu Data;

		/// <summary>
		/// Khởi tạo
		/// </summary>
		public static void Init()
		{
			XElement xmlNode = KTGlobal.ReadXMLData("Config/KT_Item/XiuLianZhu.xml");
			ItemXiuLianZhuManager.Data = XiuLianZhu.Parse(xmlNode);
		}
		#endregion

		#region Public methods
		/// <summary>
		/// Trả về số giờ Tu luyện tăng thêm mỗi ngày
		/// </summary>
		/// <returns></returns>
		public static int GetHourAddPerDay()
		{
			return ItemXiuLianZhuManager.Data.Config.HourAddPerDay;
		}

		/// <summary>
		/// Trả về giá trị kinh nghiệm cực đại Tu Luyện có thể có được
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
		public static int GetLimitExp(KPlayer player)
		{
			/// Nhóm cấp độ
			int levelGroup = player.m_Level / 10 + 1;
			/// Nếu dưới 0 thì toác
			if (levelGroup <= 0)
			{
				return 0;
			}
			/// Nếu không tồn tại tức vượt quá ngưỡng thì lấy Max
			if (!ItemXiuLianZhuManager.Data.ExpLimit.TryGetValue(levelGroup, out XiuLianZhu.ExpValue expValue))
			{
				return ItemXiuLianZhuManager.Data.ExpLimit.LastOrDefault().Value.Value;
			}

			/// Trả về kết quả
			return expValue.Value;
		}

		/// <summary>
		/// Trả về lượng Kinh nghiệm có được theo thời gian tương ứng
		/// </summary>
		/// <param name="player"></param>
		/// <param name="hour10"></param>
		/// <returns></returns>
		public static int GetAddedExpByHour(KPlayer player, int hour10)
		{
			/// Nhóm cấp độ
			int levelGroup = player.m_Level / 10 + 1;
			/// Nếu dưới 0 thì toác
			if (levelGroup <= 0)
			{
				return 0;
			}
			/// Nếu không tồn tại tức vượt quá ngưỡng thì lấy Max
			if (!ItemXiuLianZhuManager.Data.ExpByHour.TryGetValue(levelGroup, out XiuLianZhu.ExpValue expValue))
			{
				return ItemXiuLianZhuManager.Data.ExpByHour.LastOrDefault().Value.Value * hour10 / 10;
			}

			/// Trả về kết quả
			return expValue.Value * hour10 / 10;
		}

		/// <summary>
		/// Kiểm tra người chơi có thể thêm số giờ Tu Luyện tương ứng không
		/// </summary>
		/// <param name="player"></param>
		/// <param name="hour">Số giờ * 10</param>
		/// <returns></returns>
		public static string UseXiuLianZhu_CheckCondition(KPlayer player, int hour10)
		{
			/// Nếu thời gian tu luyện còn lại trong ngày không đủ
			if (player.XiuLianZhu_TotalTime < hour10)
			{
				return "Số giờ Tu Luyện còn lại không đủ!";
			}
			/// Nếu lượng Kinh nghiệm đã vượt giới hạn
			else if (ItemXiuLianZhuManager.GetAddedExpByHour(player, hour10) + player.XiuLianZhu_Exp >= ItemXiuLianZhuManager.GetLimitExp(player))
			{
				return "Kinh nghiệm Tu Luyện đã đạt giới hạn, xin đừng lãng phí!";
			}

			/// Trả ra kết quả có thể dùng
			return "OK";
		}

		/// <summary>
		/// Sử dụng Tu Luyện Châu
		/// </summary>
		/// <param name="player"></param>
		/// <param name="hour10"></param>
		/// <returns></returns>
		public static bool UseXiuLianZhu(KPlayer player, int hour10)
		{
			/// Kiểm tra điều kiện
			string ret = ItemXiuLianZhuManager.UseXiuLianZhu_CheckCondition(player, hour10);
			/// Nếu không thỏa mãn điều kiện
			if (ret != "OK")
			{
				KTPlayerManager.ShowNotification(player, ret);
				return false;
			}

			/// Giảm thời gian Tu Luyện còn lại
			player.XiuLianZhu_TotalTime -= hour10;
			/// Tăng lượng kinh nghiệm có được
			player.XiuLianZhu_Exp += ItemXiuLianZhuManager.GetAddedExpByHour(player, hour10);

			/// Thời gian Buff
			long buffDuration = hour10 * 360000;

			/// Tạo Buff x2 kinh nghiệm
			{
				/// Kỹ năng tương ứng
				SkillDataEx skillData = KSkill.GetSkillData(ItemXiuLianZhuManager.Data.Config.DoubleExpBuff);
				/// Nếu tồn tại kỹ năng
				if (skillData != null)
				{
					/// Tạo mới SkillLevelRef
					SkillLevelRef skill = new SkillLevelRef()
					{
						Data = skillData,
						AddedLevel = 1,
					};
					/// Tạo Buff
					BuffDataEx buff = new BuffDataEx()
					{
						Duration = buffDuration,
						StartTick = KTGlobal.GetCurrentTimeMilis(),
						Skill = skill,
						StackCount = 1,
						SaveToDB = true,
						CustomProperties = skill.Properties.Clone(),
					};
					/// Buff cũ
					BuffDataEx oldBuff = player.Buffs.GetBuff(skill.SkillID);
					/// Nếu tồn tại Buff cũ
					if (oldBuff != null)
					{
						buff.StartTick = oldBuff.StartTick;
						buff.Duration += oldBuff.Duration;
					}
					player.Buffs.AddBuff(buff);
				}
			}
			/// Tạo Buff tăng may mắn
			{
				/// Kỹ năng tương ứng
				SkillDataEx skillData = KSkill.GetSkillData(ItemXiuLianZhuManager.Data.Config.LuckyBuff);
				/// Nếu tồn tại kỹ năng
				if (skillData != null)
				{
					/// Tạo mới SkillLevelRef
					SkillLevelRef skill = new SkillLevelRef()
					{
						Data = skillData,
						AddedLevel = 1,
					};
					/// Tạo Buff
					BuffDataEx buff = new BuffDataEx()
					{
						Duration = buffDuration,
						StartTick = KTGlobal.GetCurrentTimeMilis(),
						Skill = skill,
						StackCount = 1,
						SaveToDB = true,
						CustomProperties = skill.Properties.Clone(),
					};
					/// Buff cũ
					BuffDataEx oldBuff = player.Buffs.GetBuff(skill.SkillID);
					/// Nếu tồn tại Buff cũ
					if (oldBuff != null)
					{
						buff.StartTick = oldBuff.StartTick;
						buff.Duration += oldBuff.Duration;
					}
					player.Buffs.AddBuff(buff);
				}
			}

			/// Trả về kết quả sử dụng thành công
			return true;
		}
		#endregion
	}
}
