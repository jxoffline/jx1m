using GameServer.KiemThe.Logic;
using GameServer.KiemThe.Logic.Manager.Skill.PoisonTimer;

namespace GameServer.Logic
{
    /// <summary>
    /// Hành động của bot
    /// </summary>
    public partial class KDecoBot
    {
        /// <summary>
        /// Hàm này gọi khi đối tượng được kích hoạt bởi Timer
        /// </summary>
        public void Start()
        {
            /// Kích hoạt toàn bộ vòng sáng
            foreach (BotSkill aura in this.auras.Values)
            {
                /// Kích hoạt vòng sáng
                this.UseSkill(aura, this);
            }
        }

        /// <summary>
        /// Hàm này gọi liên tục
        /// </summary>
        public override void Tick()
        {
            /// Gọi phương thức cha
            base.Tick();

            /// Thực hiện AI Tick
            this.AITick();
        }

        /// <summary>
        /// Reset đối tượng
        /// </summary>
        /// <param name="removeFromManager">Xóa khỏi Manager luôn không (mặc định True, nếu gọi từ chính Manager thì False)</param>
        public void Reset(bool removeFromManager = true)
        {
            /// Nếu xóa khỏi Manager
            if (removeFromManager)
            {
                KTDecoBotManager.Remove(this);
            }

            /// Xóa toàn bộ Buff và vòng sáng tương ứng
            this.Buffs.RemoveAllBuffs();
            this.Buffs.RemoveAllAruas();
            this.Buffs.RemoveAllAvoidBuffs();

            /// Xóa toàn bộ kỹ năng tự kích hoạt tương ứng
            this.RemoveAllAutoSkills();

            /// Xóa StoryBoard
            KTBotStoryBoardEx.Instance.Remove(this, false);
            /// Hủy luồng trúng độc
            KTPoisonTimerManager.Instance.RemovePoisonState(this);

            /// Thiết lập vị trí đứng
            this.CurrentPos = this.InitPos;
        }
    }
}
