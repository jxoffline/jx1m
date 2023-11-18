using GameServer.KiemThe.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.KiemThe.Entities
{
    /// <summary>
    /// Kiểm tra điều kiện
    /// </summary>
    public partial class SkillTree
    {
        /// <summary>
        /// Kiểm tra kỹ năng tương ứng có hợp lệ với bản thân không
        /// </summary>
        /// <param name="skill"></param>
        /// <returns></returns>
        public bool IsValidSkill(SkillLevelRef skill)
        {
            /// Kiểm tra môn phái có phù hợp không
            if (skill.Data.FactionID != 0)
            {
                /// Nếu môn phái không phù hợp
                if (this.Player.m_cPlayerFaction.GetFactionId() != skill.Data.FactionID)
                {
                    /// Xóa Buff tương ứng nếu có
                    this.Player.Buffs.RemoveBuff(skill.SkillID);

                    return false;
                }
            }

            /// Nếu kỹ năng cấp độ dưới 0
            if (skill.Level <= 0)
            {
                return false;
            }

            /// Trả về kết quả thỏa mãn
            return true;
        }
    }
}
