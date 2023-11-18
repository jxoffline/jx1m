using GameServer.KiemThe;
using GameServer.KiemThe.Entities;
using GameServer.KiemThe.Logic;
using System.Collections.Generic;

namespace GameServer.Logic
{
    /// <summary>
    /// Kỹ năng biểu diễn của Bot
    /// </summary>
    public partial class KDecoBot
    {
        /// <summary>
        /// Kỹ năng của Bot (dùng để thiết lập dữ liệu)
        /// </summary>
        public class BotSkillData
        {
            /// <summary>
            /// ID kỹ năng
            /// </summary>
            public int SkillID { get; set; }

            /// <summary>
            /// Cấp độ kỹ năng
            /// </summary>
            public int SkillLevel { get; set; }

            /// <summary>
            /// Thời gian phục hồi
            /// </summary>
            public int Cooldown { get; set; }
        }

        /// <summary>
        /// Định nghĩa kỹ năng của Bot (dùng nội bộ)
        /// </summary>
        private class BotSkill
        {
            /// <summary>
            /// Dữ liệu kỹ năng
            /// </summary>
            public BotSkillData Data { get; set; }

            /// <summary>
            /// Thời điểm sử dụng lần trước
            /// </summary>
            public long LastUsedTicks { get; set; }

            /// <summary>
            /// Đã sẵn sàng để sử dụng chưa
            /// </summary>
            public bool IsReady
            {
                get
                {
                    /// Nếu không có Cooldown
                    if (this.Data.Cooldown == -1)
                    {
                        return true;
                    }
                    /// Trả về kết quả
                    return KTGlobal.GetCurrentTimeMilis() - this.LastUsedTicks >= this.Data.Cooldown;
                }
            }
        }

        /// <summary>
        /// Danh sách kỹ năng
        /// </summary>
        private readonly Dictionary<int, BotSkill> skills = new Dictionary<int, BotSkill>();

        /// <summary>
        /// Thiết lập danh sách kỹ năng cho Bot
        /// </summary>
        /// <param name="skills"></param>
        public void SetSkills(params BotSkillData[] skills)
        {
            /// Làm rỗng danh sách kỹ năng cũ
            this.skills.Clear();

            /// Duyệt danh sách kỹ năng mới
            foreach (BotSkillData skillData in skills)
            {
                /// Thêm vào danh sách
                this.skills[skillData.SkillID] = new BotSkill()
                {
                    Data = skillData,
                    LastUsedTicks = 0,
                };
            }
        }

        /// <summary>
        /// Sử dụng kỹ năng với mục tiêu tương ứng
        /// </summary>
        /// <param name="skill"></param>
        /// <param name="target"></param>
        private void UseSkill(BotSkill skill, GameObject target)
        {
            /// Nếu đã chết
            if (this.IsDead() || target.IsDead())
            {
                return;
            }

            /// Nếu chưa cooldown xong
            if (!skill.IsReady)
            {
                return;
            }

            /// Lưu lại thời điểm dùng kỹ năng
            skill.LastUsedTicks = KTGlobal.GetCurrentTimeMilis();

            /// Lấy dữ liệu kỹ năng tương ứng
            SkillDataEx skillData = KSkill.GetSkillData(skill.Data.SkillID);
            /// Nếu kỹ năng không tồn tại
            if (skillData == null)
            {
                return;
            }

            /// Làm mới đối tượng kỹ năng theo cấp
            SkillLevelRef skillRef = new SkillLevelRef()
            {
                Data = skillData,
                AddedLevel = skill.Data.SkillLevel,
                BonusLevel = 0,
            };

            /// Thực hiện sử dụng kỹ năng
            KTSkillManager.UseSkillResult result = KTSkillManager.UseSkill(this, target, null, skillRef);
        }
    }
}
