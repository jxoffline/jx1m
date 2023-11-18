using Server.Tools;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace GameServer.KiemThe.Entities
{
    /// <summary>
    /// Quản lý Logic các Buff bị cấm trên nhân vật
    /// </summary>
    public partial class BuffTree
    {
        /// <summary>
        /// Danh sách Buff không thể thực thi trên đối tượng
        /// </summary>
        private readonly ConcurrentDictionary<int, HashSet<int>> listAvoidBuffs = new ConcurrentDictionary<int, HashSet<int>>();


        /// <summary>
        /// Thêm Buff vào danh sách bị cấm
        /// </summary>
        /// <param name="skillID"></param>
        /// <param name="buffID"></param>
        public void AddAvoidBuff(int skillID, int buffID)
        {
            try
			{
                if (!this.listAvoidBuffs.TryGetValue(skillID, out _))
                {
                    this.listAvoidBuffs[skillID] = new HashSet<int>();
                }

                if (!this.listAvoidBuffs[skillID].Contains(buffID))
                {
                    this.listAvoidBuffs[skillID].Add(buffID);
                }
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Skill, ex.ToString());
            }
        }

        /// <summary>
        /// Xóa Buff khỏi danh sách bị cấm
        /// </summary>
        /// <param name="skillID"></param>
        /// <param name="buffID"></param>
        public void RemoveAvoidBuff(int skillID, int buffID)
        {
            try
			{
                if (!this.listAvoidBuffs.TryGetValue(skillID, out _))
                {
                    return;
                }

                if (this.listAvoidBuffs[skillID].Contains(buffID))
                {
                    this.listAvoidBuffs[skillID].Remove(buffID);
                }
            }
            catch (Exception ex)
            {
                LogManager.WriteLog(LogTypes.Skill, ex.ToString());
            }
        }

        /// <summary>
        /// Xóa tất cả các Buff khỏi danh sách bị cấm bởi kỹ năng tương ứng
        /// </summary>
        /// <param name="skillID"></param>
        public void RemoveAllAvoidBuffs(int skillID)
        {
            if (!this.listAvoidBuffs.TryGetValue(skillID, out _))
            {
                return;
            }
            this.listAvoidBuffs[skillID].Clear();
        }

        /// <summary>
        /// Xóa tất cả các Buff khỏi danh sách bị cấm
        /// </summary>
        public void RemoveAllAvoidBuffs()
        {
            this.listAvoidBuffs.Clear();
        }

        /// <summary>
        /// Kiểm tra Buff tương ứng có nằm trong danh sách loại trừ không
        /// </summary>
        /// <param name="buffID"></param>
        /// <returns></returns>
        public bool IsBuffAvoided(int buffID)
        {
            return this.listAvoidBuffs.Any(x => x.Value.Contains(buffID));
        }
    }
}
