using FS.GameEngine.Logic;
using FS.VLTK.Network;
using System.Collections;
using UnityEngine;

namespace FS.VLTK.Logic
{
    /// <summary>
    /// Đồng bộ hóa
    /// </summary>
    public partial class KTAutoPetManager
    {
        /// <summary>
        /// Luồng cập nhật vị trí Pet gửi lên GS
        /// </summary>
        /// <returns></returns>
        private IEnumerator SyncPosToGS()
        {
            /// Vị trí trước đó của bản thân
            Vector2 lastPos = Vector2.zero;
            /// Đợi
            WaitForSeconds wait = new WaitForSeconds(0.5f);
            /// Lặp liên tục
            while (true)
            {
                /// Nếu không có Pet
                if (this.Pet == null)
                {
                    /// Tiếp tục luồng
                    goto COROUTINE_YIELD;
                }

                /// Nếu vị trí thay đổi
                if (Vector2.Distance(lastPos, this.Pet.PositionInVector2) > 10f)
                {
                    /// Cập nhật lại vị trí mới
                    lastPos = this.Pet.PositionInVector2;
                    /// Cập nhật vị trí lên GS
                    KT_TCPHandler.SendSyncPetPos(this.Pet.RoleID, this.Pet.PosX, this.Pet.PosY);
                }

                /// Nhãn tiếp tục luồng
                COROUTINE_YIELD:
                /// Đợi
                yield return wait;
            }
        }
    }
}
