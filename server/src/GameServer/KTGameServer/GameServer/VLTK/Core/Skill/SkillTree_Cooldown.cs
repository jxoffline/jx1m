using GameServer.KiemThe.Logic;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace GameServer.KiemThe.Entities
{
    /// <summary>
    /// Quản lý phục hồi kỹ năng
    /// </summary>
    public partial class SkillTree
    {
        /// <summary>
        /// Danh sách kỹ năng được ghi nhận thời gian phục hồi
        /// </summary>
        private readonly ConcurrentDictionary<int, SkillCooldown> listCooldownSkills = new ConcurrentDictionary<int, SkillCooldown>();

        /// <summary>
        /// Làm rỗng danh sách chờ phục hồi kỹ năng
        /// </summary>
        public void ClearSkillCooldownList()
        {
            this.listCooldownSkills.Clear();
            KT_TCPHandler.ResetSkillCooldown(this.Player);
        }

        /// <summary>
        /// Làm rỗng danh sách chờ phục hồi kỹ năng khác ngoại trừ kỹ năng chỉ định
        /// </summary>
        /// <param name="skillID"></param>
        public void ClearSkillCooldownListExcept(int skillID)
        {
            /// Tìm thông tin kỹ năng loại trừ trong danh sách
            bool ret = this.listCooldownSkills.TryGetValue(skillID, out SkillCooldown skillCooldown);
            /// Xóa rỗng danh sách
            this.listCooldownSkills.Clear();
            /// Thêm kỹ năng loại trừ vừa tìm thấy vào danh sách
            if (ret)
            {
                this.listCooldownSkills[skillID] = skillCooldown;
            }
            KT_TCPHandler.ResetSkillCooldown(this.Player, new List<int>() { skillID });
        }

        /// <summary>
        /// Thêm kỹ năng vào danh sách thời gian phục hồi
        /// </summary>
        /// <param name="skillID"></param>
        /// <param name="cooldownTick"></param>
        public void AddSkillCooldown(int skillID, int cooldownTick)
        {
            SkillLevelRef levelRef = this.GetSkillLevelRef(skillID);
            if (levelRef == null)
            {
                return;
            }

            this.listCooldownSkills[skillID] = new SkillCooldown()
            {
                Skill = levelRef,
                CooldownTick = cooldownTick,
            };
            KT_TCPHandler.NotifySkillCooldown(this.Player, this.listCooldownSkills[skillID]);
        }

        /// <summary>
        /// Thêm kỹ năng vào danh sách thời gian phục hồi
        /// </summary>
        /// <param name="skillID"></param>
        /// <param name="startTick"></param>
        /// <param name="cooldownTick"></param>
        public void AddSkillCooldown(int skillID, long startTick, int cooldownTick)
        {
            SkillLevelRef levelRef = this.GetSkillLevelRef(skillID);
            if (levelRef == null)
            {
                return;
            }

            this.listCooldownSkills[skillID] = new SkillCooldown(startTick)
            {
                Skill = levelRef,
                CooldownTick = cooldownTick,
            };
            KT_TCPHandler.NotifySkillCooldown(this.Player, this.listCooldownSkills[skillID]);
        }

        /// <summary>
        /// Kỹ năng có đang trong trạng thái thời gian phục hồi không
        /// </summary>
        /// <param name="skillID"></param>
        /// <returns></returns>
        public bool IsSkillCooldown(int skillID)
        {
            SkillLevelRef levelRef = this.GetSkillLevelRef(skillID);
            if (levelRef == null)
            {
                return false;
            }

            if (this.listCooldownSkills.TryGetValue(skillID, out SkillCooldown cooldown))
            {
                return !cooldown.IsOver;
            }
            return false;
        }

        /// <summary>
        /// Trả về thời gian gần đây nhất dùng kỹ năng
        /// </summary>
        /// <param name="skillID"></param>
        /// <returns></returns>
        public long GetSkillLastUsedTick(int skillID)
        {
            if (this.listCooldownSkills.TryGetValue(skillID, out SkillCooldown cooldown))
            {
                return Math.Max(0, cooldown.StartTick);
            }
            return 0;
        }

        /// <summary>
        /// Trả về thời gian phục hồi hiện tại được lưu của kỹ năng
        /// </summary>
        /// <param name="skillID"></param>
        /// <returns></returns>
        public int GetSkillCooldown(int skillID)
        {
            if (this.listCooldownSkills.TryGetValue(skillID, out SkillCooldown cooldown))
            {
                return Math.Max(0, cooldown.CooldownTick);
            }
            return 0;
        }
    }
}
