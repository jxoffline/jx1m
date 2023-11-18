using GameServer.KiemThe.Entities;

namespace GameServer.Logic
{
    public partial class GameObject
    {
        #region Public methods

        /// <summary>
        /// Có phải đã chết
        /// </summary>
        /// <returns></returns>
        public bool IsDead()
        {
            if (this.m_eDoing == KE_NPC_DOING.do_death || this.m_eDoing == KE_NPC_DOING.do_revive)
			{
                return true;
			}
            else if (this.m_CurrentLife <= 0)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Có phải Pet không
        /// </summary>
        /// <returns></returns>
        public bool IsPet()
        {
            return this is Pet;
        }

        #endregion Public methods
    }
}