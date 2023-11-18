using System.Collections.Generic;

namespace GameServer.Logic
{
    /// <summary>
    /// Vòng sáng biểu diễn của Bot
    /// </summary>
    public partial class KDecoBot
    {
        /// <summary>
        /// Danh sách vòng sáng
        /// </summary>
        private readonly Dictionary<int, BotSkill> auras = new Dictionary<int, BotSkill>();

        /// <summary>
        /// Thiết lập danh sách vòng sáng tương ứng
        /// </summary>
        /// <param name="auras"></param>
        public void SetAuras(params BotSkillData[] auras)
        {
            /// Làm rỗng danh sách cũ
            this.auras.Clear();

            /// Duyệt danh sách mới
            foreach (BotSkillData skillData in auras)
            {
                /// Thêm vào danh sách
                this.auras[skillData.SkillID] = new BotSkill()
                {
                    Data = skillData,
                    LastUsedTicks = 0,
                };
            }
        }
    }
}
