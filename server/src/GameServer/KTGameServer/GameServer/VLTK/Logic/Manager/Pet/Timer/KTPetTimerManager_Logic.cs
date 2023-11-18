using GameServer.KiemThe.CopySceneEvents;
using GameServer.Logic;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Logic cơ bản của quái
    /// </summary>
    public partial class KTPetTimerManager
    {
        /// <summary>
        /// Đánh dấu có cần thiết phải xóa pet này ra khỏi Timer không
        /// <para>- Nếu không có chủ nhân</para>
        /// </summary>
        /// <param name="pet"></param>
        /// <returns></returns>
        private bool NeedToBeRemoved(Pet pet)
        {
            /// Nếu không có tham chiếu
            if (pet == null)
            {
                return true;
            }
            /// Nếu chủ nhân không tồn tại
            if (pet.Owner == null)
            {
                return true;
            }
            /// Nếu chủ nhân đã chết
            if (pet.Owner.IsDead())
            {
                return true;
            }
            /// Nếu chủ nhân đã rời mạng
            if (!pet.Owner.IsOnline())
            {
                return true;
            }

            /// Nếu thỏa mãn tất cả điều kiện
            return false;
        }

        /// <summary>
        /// Thực hiện hàm Start
        /// </summary>
        /// <param name="pet"></param>
        private void ProcessStart(Pet pet)
        {
            /// Nếu vi phạm điều kiện, cần xóa khỏi Timer
            if (this.NeedToBeRemoved(pet))
            {
                this.Remove(pet);
                return;
            }

            /// Thực thi sự kiện OnStart
            this.ExecuteAction(pet.Start, null);
        }

        /// <summary>
        /// Thực hiện hàm Tick
        /// </summary>
        /// <param name="pet"></param>
        private void ProcessTick(Pet pet)
        {
            /// Nếu vi phạm điều kiện, cần xóa khỏi Timer
            if (this.NeedToBeRemoved(pet))
            {
                this.Remove(pet);
                return;
            }

            /// Thực hiện hàm Tick của đối tượng
            this.ExecuteAction(pet.Tick, null);
        }
    }
}
