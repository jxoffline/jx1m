using System;
using System.Collections.Generic;
using System.Linq;
using GameServer.KiemThe.Entities;
using GameServer.KiemThe.Utilities;
using GameServer.Logic;

namespace GameServer.KiemThe.Core.MonsterAIScript
{
    /// <summary>
    /// Script mặc định của Monster
    /// </summary>
    public class KTMonsterAI_ScriptDefault : IMonsterAIScript
	{
		/// <summary>
		/// Thông tin kỹ năng
		/// </summary>
		private class SkillInfo
        {
			/// <summary>
			/// ID kỹ năng
			/// </summary>
			public int ID { get; set; }
			
			/// <summary>
			/// Thời gian phục hồi
			/// </summary>
			public long Cooldown { get; set; }

			/// <summary>
			/// Ngũ hành
			/// </summary>
			public KE_SERIES_TYPE Series { get; set; }

			/// <summary>
			/// Khoảng cách ra chiêu
			/// </summary>
			public int CastRange { get; set; }
        }

		/// <summary>
		/// Danh sách kỹ năng
		/// </summary>
		private readonly Dictionary<int, Dictionary<int, SkillInfo>> AISkillList = new Dictionary<int, Dictionary<int, SkillInfo>>()
		{
			/// Kỹ năng cận chiến
			{ 1, new Dictionary<int, SkillInfo>()
				{
					{0,  new SkillInfo() { ID = 14500, Cooldown = 0, Series = KE_SERIES_TYPE.series_none, CastRange = 80, } },
					{1,  new SkillInfo() { ID = 14500, Cooldown = 0, Series = KE_SERIES_TYPE.series_metal, CastRange = 80, } },
					{2,  new SkillInfo() { ID = 14501, Cooldown = 0, Series = KE_SERIES_TYPE.series_wood, CastRange = 80, } },
					{3,  new SkillInfo() { ID = 14502, Cooldown = 0, Series = KE_SERIES_TYPE.series_water, CastRange = 80, } },
					{4,  new SkillInfo() { ID = 14503, Cooldown = 0, Series = KE_SERIES_TYPE.series_fire, CastRange = 80, } },
					{5,  new SkillInfo() { ID = 14504, Cooldown = 0, Series = KE_SERIES_TYPE.series_earth, CastRange = 80, } },
				}
			},
			/// Kỹ năng tầm trung
			{ 2, new Dictionary<int, SkillInfo>()
				{
					{0,  new SkillInfo() { ID = 14505, Cooldown = 11000, Series = KE_SERIES_TYPE.series_none, CastRange = 300, } },
					{1,  new SkillInfo() { ID = 14505, Cooldown = 11000, Series = KE_SERIES_TYPE.series_metal, CastRange = 300, } },
					{2,  new SkillInfo() { ID = 14506, Cooldown = 11000, Series = KE_SERIES_TYPE.series_wood, CastRange = 300, } },
					{3,  new SkillInfo() { ID = 14507, Cooldown = 11000, Series = KE_SERIES_TYPE.series_water, CastRange = 300, } },
					{4,  new SkillInfo() { ID = 14508, Cooldown = 11000, Series = KE_SERIES_TYPE.series_fire, CastRange = 300, } },
					{5,  new SkillInfo() { ID = 14509, Cooldown = 11000, Series = KE_SERIES_TYPE.series_earth, CastRange = 300, } },
				}
			},
			/// Kỹ năng tầm xa
			{ 3, new Dictionary<int, SkillInfo>()
				{
					{0,  new SkillInfo() { ID = 14510, Cooldown = 30000, Series = KE_SERIES_TYPE.series_none, CastRange = 500 } },
					{1,  new SkillInfo() { ID = 14510, Cooldown = 30000, Series = KE_SERIES_TYPE.series_metal, CastRange = 500, } },
					{2,  new SkillInfo() { ID = 14511, Cooldown = 30000, Series = KE_SERIES_TYPE.series_wood, CastRange = 500, } },
					{3,  new SkillInfo() { ID = 14512, Cooldown = 30000, Series = KE_SERIES_TYPE.series_water, CastRange = 500, } },
					{4,  new SkillInfo() { ID = 14513, Cooldown = 30000, Series = KE_SERIES_TYPE.series_fire, CastRange = 500, } },
					{5,  new SkillInfo() { ID = 14514, Cooldown = 30000, Series = KE_SERIES_TYPE.series_earth, CastRange = 500, } },
				}
			},
		};

		/// <summary>
		/// Danh sách biến cục bộ dùng cho lưu trữ
		/// </summary>
		private enum LocalVariableID
		{
			/// <summary>
			/// Thời gian lần trước sử dụng kỹ năng tầm trung
			/// </summary>
			LastTickUseSkill_2 = 0,
			/// <summary>
			/// Thời gian lần trước sử dụng kỹ năng tầm xa
			/// </summary>
			LastTickUseSkill_3 = 1,
		}

		/// <summary>
		/// Thiết lập
		/// </summary>
		private enum Setting
		{
			/// <summary>
			/// Cấp độ tối thiểu dùng kỹ năng tầm trung
			/// </summary>
			LevelUseSkill2 = 30,
			/// <summary>
			/// Cấp độ tối thiểu dùng kỹ năng tầm cao
			/// </summary>
			LevelUseSkill3 = 50,
			/// <summary>
			/// Tỷ lệ % mỗi lần đến thời gian sẽ dùng kỹ năng số 2
			/// </summary>
			ChanceUseSkill2 = 100,
			/// <summary>
			/// Tỷ lệ % mỗi lần đến thời gian sẽ dùng kỹ năng số 3
			/// </summary>
			ChanceUseSkill3 = 100,
		}

		/// <summary>
		/// Vị trí rỗng
		/// </summary>
		public UnityEngine.Vector2 EmptyPos
        {
            get
            {
				return UnityEngine.Vector2.zero;
			}
        }

        /// <summary>
        /// Thực hiện AI của quái
        /// </summary>
        /// <param name="monster"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public UnityEngine.Vector2 Process(Monster monster, GameServer.Logic.GameObject target)
        {
            /// Nếu quái NULL
            if (monster == null)
            {
                return this.EmptyPos;
			}
			/// Nếu không có mục tiêu
			else if (target == null)
			{
				return this.EmptyPos;
			}
			/// Nếu lúc này không thể dùng kỹ năng
			else if (!KTGlobal.FinishedUseSkillAction(monster, monster.GetCurrentAttackSpeed()))
            {
				return this.EmptyPos;
            }

			/// Thời gian hiện tại của hệ thống
			long nowTime = KTGlobal.GetCurrentTimeMilis();
			/// Ngũ hành của bản thân
			KE_SERIES_TYPE selfElement = monster.m_Series;
			/// Vị trí của bản thân
			UnityEngine.Vector2 selfPos = new UnityEngine.Vector2((int) monster.CurrentPos.X, (int) monster.CurrentPos.Y);
			/// Vị trí của mục tiêu
			UnityEngine.Vector2 targetPos = new UnityEngine.Vector2((int) target.CurrentPos.X, (int) target.CurrentPos.Y);
			/// Khoảng cách đến chỗ mục tiêu
			float distanceToTarget = UnityEngine.Vector2.Distance(selfPos, targetPos);
			/// Tỷ lệ ngẫu nhiên
			int rand = KTGlobal.GetRandomNumber(1, 100);

            /// nếu Monster chỉ là động vật thì chỉ dùng skill cận chiến
            if (monster.MonsterInfo.ResName.Contains("ani"))
            {
                /// Nếu khoảng cách đến mục tiêu nằm trong phạm vi đánh
                if (distanceToTarget <= this.AISkillList[1][(int) selfElement].CastRange)
                {
                    /// Kết quả dùng kỹ năng
                    bool retUseSkill = monster.UseSkill(this.AISkillList[1][(int) selfElement].ID, 1, target);
                    return this.EmptyPos;
                }
                else
                {
                    UnityEngine.Vector2 destPos = KTMath.FindPointInVectorWithDistance(targetPos, (selfPos - targetPos), this.AISkillList[1][(int) selfElement].CastRange - 20);
                    return destPos;
                }
            }

			/// Kiểm tra dùng kỹ năng số 3
			if (monster.m_Level >= (int) Setting.LevelUseSkill3 && this.AISkillList[3].ContainsKey((int) selfElement))
            {
				/// Thời gian dùng kỹ năng lần trước
				long lastTickUseSkill = monster.GetLocalParam((int) LocalVariableID.LastTickUseSkill_3);
				/// Nếu đã hết thời gian Cooldown thì cho dùng kỹ năng
				if (nowTime - lastTickUseSkill >= this.AISkillList[3][(int) selfElement].Cooldown)
                {
					/// Nếu khoảng cách phù hợp
					if (distanceToTarget <= this.AISkillList[3][(int) selfElement].CastRange)
                    {
						/// Kết quả dùng kỹ năng
						bool retUseSkill = true;
						/// Nếu random % đủ dùng kỹ năng này
						if (rand <= (int) Setting.ChanceUseSkill3)
                        {
							/// Sử dụng kỹ năng
							retUseSkill = monster.UseSkill(this.AISkillList[3][(int) selfElement].ID, 1, target);
						}

						/// Nếu dùng kỹ năng thành công
						if (retUseSkill)
                        {
							/// Cập nhật lại thời gian dùng kỹ năng tương ứng
							monster.SetLocalParam((int) LocalVariableID.LastTickUseSkill_3, nowTime);
						}
						return this.EmptyPos;
					}
                    else
                    {
						UnityEngine.Vector2 destPos = KTMath.FindPointInVectorWithDistance(targetPos, (selfPos - targetPos), this.AISkillList[3][(int) selfElement].CastRange - 20);
						return destPos;
					}
				}
			}

			/// Kiểm tra dùng kỹ năng số 2
			if (monster.m_Level >= (int) Setting.LevelUseSkill3 && this.AISkillList[2].ContainsKey((int) selfElement))
            {
				/// Thời gian dùng kỹ năng lần trước
				long lastTickUseSkill = monster.GetLocalParam((int) LocalVariableID.LastTickUseSkill_2);
				/// Nếu đã hết thời gian Cooldown thì cho dùng kỹ năng
				if (nowTime - lastTickUseSkill >= this.AISkillList[2][(int) selfElement].Cooldown)
                {
					/// Nếu khoảng cách phù hợp
					if (distanceToTarget <= this.AISkillList[2][(int) selfElement].CastRange)
                    {
						/// Kết quả dùng kỹ năng
						bool retUseSkill = true;
						/// Nếu random % đủ dùng kỹ năng này
						if (rand <= (int) Setting.ChanceUseSkill2)
                        {
							/// Sử dụng kỹ năng
							retUseSkill = monster.UseSkill(this.AISkillList[2][(int) selfElement].ID, 1, target);
						}

						/// Nếu dùng kỹ năng thành công
						if (retUseSkill)
                        {
							/// Cập nhật lại thời gian dùng kỹ năng tương ứng
							monster.SetLocalParam((int) LocalVariableID.LastTickUseSkill_2, nowTime);
						}
						return this.EmptyPos;
					}
                    else
                    {
						UnityEngine.Vector2 destPos = KTMath.FindPointInVectorWithDistance(targetPos, (selfPos - targetPos), this.AISkillList[2][(int) selfElement].CastRange - 20);
						return destPos;
					}
				}
			}

			/// Kiểm tra dùng kỹ năng số 1
			if (this.AISkillList[1].ContainsKey((int) selfElement))
            {
				/// Nếu khoảng cách đến mục tiêu nằm trong phạm vi đánh
				if (distanceToTarget <= this.AISkillList[1][(int) selfElement].CastRange)
                {
					/// Kết quả dùng kỹ năng
					bool retUseSkill = monster.UseSkill(this.AISkillList[1][(int) selfElement].ID, 1, target);
					return this.EmptyPos;
				}
                else
                {
					UnityEngine.Vector2 destPos = KTMath.FindPointInVectorWithDistance(targetPos, (selfPos - targetPos), this.AISkillList[1][(int) selfElement].CastRange - 20);
					return destPos;
				}
			}

			/// Không có gì
			return this.EmptyPos;
		}
    }
}
