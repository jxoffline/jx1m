using GameServer.KiemThe.Entities;
using GameServer.KiemThe.Utilities;
using GameServer.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameServer.KiemThe.Core.MonsterAIScript
{
	/// <summary>
	/// Script quái dùng kỹ năng mặc định theo danh sách
	/// </summary>
	public class KTMonsterAI_ScriptCustomSkills : IMonsterAIScript
	{
		/// <summary>
		/// Tọa độ rỗng
		/// </summary>
		public Vector2 EmptyPos
		{
			get
			{
				return UnityEngine.Vector2.zero;
			}
		}

		/// <summary>
		/// Thực thi Script AI
		/// </summary>
		/// <param name="monster"></param>
		/// <param name="target"></param>
		/// <returns></returns>
		public Vector2 Process(Monster monster, GameServer.Logic.GameObject target)
		{
			/// Vị trí của bản thân
			UnityEngine.Vector2 selfPos = new UnityEngine.Vector2((int) monster.CurrentPos.X, (int) monster.CurrentPos.Y);
			/// Vị trí của mục tiêu
			UnityEngine.Vector2 targetPos = new UnityEngine.Vector2((int) target.CurrentPos.X, (int) target.CurrentPos.Y);
			/// Khoảng cách đến chỗ mục tiêu
			float distanceToTarget = UnityEngine.Vector2.Distance(selfPos, targetPos);

			/// Duyệt danh sách kỹ năng
			foreach (SkillLevelRef skill in monster.CustomAISkills)
			{
				/// Thời điểm lần trước dùng kỹ năng
				long lastUsedTicks = monster.GetLocalParam(skill.SkillID);

				/// Nếu đã đến thời gian dùng kỹ năng này
				if (KTGlobal.GetCurrentTimeMilis() - lastUsedTicks >= skill.Exp)
				{
					/// Nếu khoảng cách phù hợp
					if (skill.Data.AttackRadius <= 0 || distanceToTarget <= skill.Data.AttackRadius)
					{
						/// Dùng kỹ năng
						monster.UseSkill(skill.SkillID, skill.Level, target);
						/// Cập nhật lại thời gian dùng kỹ năng tương ứng
						monster.SetLocalParam(skill.SkillID, KTGlobal.GetCurrentTimeMilis());
						/// Trả về vị trí trống
						return this.EmptyPos;
					}
					else
					{
						UnityEngine.Vector2 destPos = KTMath.FindPointInVectorWithDistance(targetPos, (selfPos - targetPos), skill.Data.AttackRadius - 20);
						return destPos;
					}
				}
			}

			/// Không có gì
			return this.EmptyPos;
		}
	}
}
