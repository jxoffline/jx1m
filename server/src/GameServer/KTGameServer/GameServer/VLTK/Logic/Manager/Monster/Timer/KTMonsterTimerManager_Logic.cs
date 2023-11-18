using GameServer.KiemThe.CopySceneEvents;
using GameServer.Logic;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Logic cơ bản của quái
    /// </summary>
    public partial class KTMonsterTimerManager
    {
        #region Normal monster
        /// <summary>
        /// Đánh dấu có cần thiết phải xóa quái vật này ra khỏi Timer không
        /// <para>- Nếu chết</para>
        /// <para>- Nếu không có người chơi xung quanh</para>
        /// </summary>
        /// <param name="monster"></param>
        /// <returns></returns>
        private bool NormalMonster_NeedToBeRemoved(Monster monster)
        {
            /// Nếu không có tham chiếu
            if (monster == null)
            {
                return true;
            }
            /// Nếu đối tượng đã chết
            if (monster.IsDead())
            {
                return true;
            }

            /// Nếu không tìm thấy bản đồ hiện tại
            if (KTMapManager.Find(monster.CurrentMapCode) == null)
            {
                return true;
            }
            /// Nếu có phụ bản nhưng không tìm thấy thông tin
            if (monster.CurrentCopyMapID != -1 && !CopySceneEventManager.IsCopySceneExist(monster.CurrentCopyMapID, monster.CurrentMapCode))
            {
                return true;
            }

            /// Nếu không có người chơi xung quanh
            if (monster.VisibleClientsNum <= 0)
            {
                return true;
            }

            /// Nếu thỏa mãn tất cả điều kiện
            return false;
        }

        /// <summary>
        /// Thực hiện hàm Start
        /// </summary>
        /// <param name="monster"></param>
        private void NormalMonster_ProcessStart(Monster monster)
        {
            /// Nếu vi phạm điều kiện, cần xóa khỏi Timer
            if (this.NormalMonster_NeedToBeRemoved(monster))
            {
                this.Remove(monster);
                return;
            }

            /// Thực thi sự kiện OnStart
            this.ExecuteAction(monster.Start, null);
        }

        /// <summary>
        /// Thực hiện hàm Tick
        /// </summary>
        /// <param name="monster"></param>
        private void NormalMonster_ProcessTick(Monster monster)
        {
            /// Nếu vi phạm điều kiện, cần xóa khỏi Timer
            if (this.NormalMonster_NeedToBeRemoved(monster))
            {
                this.Remove(monster);
                return;
            }

            /// Thực hiện hàm Start của đối tượng
            monster.Tick();

            /// Nếu cho phép AI thực thi
            if (ServerConfig.Instance.EnableMonsterAI)
            {
                /// Thực hiện hàm AI_Tick
                this.ExecuteAction(monster.AI_Tick, null);

                /// Thực hiện hàm di chuyển
                this.ExecuteAction(monster.MonsterAI_TickMove, null);
            }
        }

        /// <summary>
        /// Thực hiện hàm AITick
        /// </summary>
        /// <param name="monster"></param>
        private void NormalMonster_ProcessAIMoveTick(Monster monster)
        {
            /// Nếu vi phạm điều kiện, cần xóa khỏi Timer
            if (this.NormalMonster_NeedToBeRemoved(monster))
            {
                this.Remove(monster);
                return;
            }

            this.ExecuteAction(monster.RandomMoveAround, null);
        }
        #endregion

        #region Special monster
        /// <summary>
        /// Đánh dấu có cần thiết phải xóa quái vật (loại đặc biệt) này ra khỏi Timer không
        /// <para>- Nếu chết</para>
        /// <para>- Nếu không có người chơi xung quanh</para>
        /// </summary>
        /// <param name="monster"></param>
        /// <returns></returns>
        private bool SpecialMonster_NeedToBeRemoved(Monster monster)
        {
            /// Nếu không có tham chiếu
            if (monster == null)
            {
                return true;
            }
            /// Nếu đối tượng đã chết
            if (monster.IsDead())
            {
                return true;
            }
            /// Nếu không tìm thấy bản đồ hiện tại
            if (KTMapManager.Find(monster.CurrentMapCode) == null)
            {
                return true;
            }
            /// Nếu có phụ bản nhưng không tìm thấy thông tin
            if (monster.CurrentCopyMapID != -1 && !CopySceneEventManager.IsCopySceneExist(monster.CurrentCopyMapID, monster.CurrentMapCode))
            {
                return true;
            }

            /// Nếu thỏa mãn tất cả điều kiện
            return false;
        }

        /// <summary>
        /// Thực hiện hàm Start
        /// </summary>
        /// <param name="monster"></param>
        private void SpecialMonster_ProcessStart(Monster monster)
        {
            /// Nếu vi phạm điều kiện, cần xóa khỏi Timer
            if (this.SpecialMonster_NeedToBeRemoved(monster))
            {
                this.Remove(monster);
                return;
            }

            /// Thực thi sự kiện OnStart
            this.ExecuteAction(monster.Start, null);
        }

        /// <summary>
        /// Thực hiện hàm Tick
        /// </summary>
        /// <param name="monster"></param>
        private void SpecialMonster_ProcessTick(Monster monster)
        {
            /// Nếu vi phạm điều kiện, cần xóa khỏi Timer
            if (this.SpecialMonster_NeedToBeRemoved(monster))
            {
                this.Remove(monster);
                return;
            }

            /// Thực hiện hàm Start của đối tượng
            this.ExecuteAction(monster.Tick, null);

            /// Nếu cho phép AI thực thi
            if (ServerConfig.Instance.EnableMonsterAI)
            {
                /// Thực hiện hàm AI_Tick
                this.ExecuteAction(monster.AI_Tick, null);

                /// Thực hiện hàm di chuyển
                this.ExecuteAction(monster.MonsterAI_TickMove, null);
            }
        }

        /// <summary>
        /// Thực hiện hàm AITick
        /// </summary>
        /// <param name="monster"></param>
        private void SpecialMonster_ProcessAIMoveTick(Monster monster)
        {
            /// Nếu vi phạm điều kiện, cần xóa khỏi Timer
            if (this.SpecialMonster_NeedToBeRemoved(monster))
            {
                this.Remove(monster);
                return;
            }

            this.ExecuteAction(monster.RandomMoveAround, null);
        }
        #endregion

        #region Custom monster
        /// <summary>
        /// Đánh dấu có cần thiết phải xóa quái vật (loại đặc biệt) này ra khỏi Timer không
        /// <para>- Nếu chết</para>
        /// <para>- Nếu không có người chơi xung quanh</para>
        /// </summary>
        /// <param name="monster"></param>
        /// <returns></returns>
        private bool CustomMonster_NeedToBeRemoved(Monster monster)
        {
            /// Nếu không có tham chiếu
            if (monster == null)
            {
                return true;
            }
            /// Nếu đối tượng đã chết
            if (monster.IsDead())
            {
                return true;
            }
            /// Nếu không tìm thấy bản đồ hiện tại
            if (KTMapManager.Find(monster.CurrentMapCode) == null)
            {
                return true;
            }
            /// Nếu có phụ bản nhưng không tìm thấy thông tin
            if (monster.CurrentCopyMapID != -1 && !CopySceneEventManager.IsCopySceneExist(monster.CurrentCopyMapID, monster.CurrentMapCode))
            {
                return true;
            }

            /// Nếu thỏa mãn tất cả điều kiện
            return false;
        }

        /// <summary>
        /// Thực hiện hàm Start
        /// </summary>
        /// <param name="monster"></param>
        private void CustomMonster_ProcessStart(Monster monster)
        {
            /// Nếu vi phạm điều kiện, cần xóa khỏi Timer
            if (this.CustomMonster_NeedToBeRemoved(monster))
            {
                this.Remove(monster);
                return;
            }

            /// Thực thi sự kiện OnStart
            this.ExecuteAction(monster.Start, null);
        }

        /// <summary>
        /// Thực hiện hàm Tick
        /// </summary>
        /// <param name="monster"></param>
        private void CustomMonster_ProcessTick(Monster monster)
        {
            /// Nếu vi phạm điều kiện, cần xóa khỏi Timer
            if (this.CustomMonster_NeedToBeRemoved(monster))
            {
                this.Remove(monster);
                return;
            }

            /// Thực hiện hàm di chuyển
            this.ExecuteAction(monster.MonsterAI_TickMove, null);
        }
        #endregion
    }
}
