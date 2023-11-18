using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.KiemThe.Entities
{
    /// <summary>
    /// Đối tượng mô tả thời gian phục hồi kỹ năng
    /// </summary>
    public class SkillCooldown
    {
        /// <summary>
        /// ID kỹ năng
        /// </summary>
        public int SkillID
        {
            get
            {
                return this.Skill.SkillID;
            }
        }

        /// <summary>
        /// Đối tượng kỹ năng
        /// </summary>
        public SkillLevelRef Skill { get; set; }

        /// <summary>
        /// Thời gian bắt đầu thiết lập
        /// </summary>
        public long StartTick { get; private set; }

        /// <summary>
        /// Thời gian phục hồi (mili giây)
        /// </summary>
        public int CooldownTick { get; set; }

        /// <summary>
        /// Đã hết thời gian cooldown
        /// </summary>
        public bool IsOver
        {
            get
            {
                return KTGlobal.GetCurrentTimeMilis() - this.StartTick > this.CooldownTick;
            }
        }

        /// <summary>
        /// Đối tượng mô tả thời gian phục hồi kỹ năng
        /// </summary>
        public SkillCooldown()
        {
            this.StartTick = KTGlobal.GetCurrentTimeMilis();
        }

        /// <summary>
        /// Đối tượng mô tả thời gian phục hồi kỹ năng
        /// </summary>
        public SkillCooldown(long startTick)
        {
            this.StartTick = startTick;
        }
    }
}
