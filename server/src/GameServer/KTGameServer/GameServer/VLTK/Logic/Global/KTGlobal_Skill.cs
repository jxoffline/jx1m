using GameServer.Logic;
using Server.Data;
using System.Collections.Generic;
using System.Linq;

namespace GameServer.KiemThe
{
    /// <summary>
    /// Các phương thức và đối tượng toàn cục của Kiếm Thế
    /// </summary>
    public static partial class KTGlobal
    {
        #region Kỹ năng

        /// <summary>
        /// Danh sách kỹ năng tấn công tân thủ
        /// </summary>
        private static readonly List<int> ListNewbieAttackSkill = new List<int>()
        {
            14000, 14001
        };

        /// <summary>
        /// Thêm kỹ năng tân thủ cho người chơi tương ứng
        /// </summary>
        /// <param name="player"></param>
        public static void AddNewbieAttackSkills(KPlayer player)
        {
            /// Nếu không có kỹ năng nào
            if (player.SkillDataList == null)
            {
                player.SkillDataList = new List<SkillData>();
            }

            /// Duyệt danh sách các kỹ năng tân thủ
            foreach (int skillID in KTGlobal.ListNewbieAttackSkill)
            {
                SkillData dbSkill = player.SkillDataList.Where(x => x.SkillID == skillID).FirstOrDefault();
                /// Nếu chưa tồn tại
                if (dbSkill == null)
                {
                    dbSkill = new SkillData()
                    {
                        SkillID = skillID,
                        SkillLevel = 1,
                        BonusLevel = 0,
                        CanStudy = true,
                        LastUsedTick = 0,
                        CooldownTick = 0,
                    };
                    player.SkillDataList.Add(dbSkill);
                }
                /// Nếu đã tồn tại
                else
                {
                    dbSkill.SkillLevel = 1;
                    dbSkill.BonusLevel = 0;
                    dbSkill.CanStudy = true;
                    dbSkill.LastUsedTick = 0;
                    dbSkill.CooldownTick = 0;
                }
            }
        }

        #endregion Kỹ năng
    }
}
