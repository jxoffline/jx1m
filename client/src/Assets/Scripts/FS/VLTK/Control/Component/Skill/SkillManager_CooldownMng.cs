using FS.GameEngine.Logic;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FS.VLTK.Control.Component
{
    /// <summary>
    /// Quản lý thời gian phục hồi kỹ năng
    /// </summary>
    public static partial class SkillManager
    {
        /// <summary>
        /// Kỹ năng có đang trong trạng thái Cooldown
        /// </summary>
        /// <param name="skillID"></param>
        /// <returns></returns>
        public static bool IsSkillCooldown(int skillID)
        {
            SkillData skill = Global.Data.RoleData.SkillDataList.Where(x => x.SkillID == skillID).FirstOrDefault();
            if (skill != null)
            {
                return skill.IsCooldown;
            }
            else
            {
                return false;
            }
        }
    }
}
