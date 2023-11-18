using FS.GameEngine.Logic;
using System.Collections;
using UnityEngine;

namespace FS.VLTK.Control.Component
{
    /// <summary>
    /// Camera của bản đồ thu nhỏ
    /// </summary>
    public class RadarMapCamera : MonoBehaviour
    {
        #region Private fields
        /// <summary>
        /// Đã chạy qua hàm Start chưa
        /// </summary>
        private bool isStarted = false;
        #endregion

        #region Core MonoBehaviour
        /// <summary>
        /// Hàm này gọi ở Frame đầu tiên
        /// </summary>
        private void Start()
        {
            /// Thực hiện đi theo Leader
            this.StartCoroutine(this.DoFollowLeaderContinuously());
            /// Đánh dấu đã chạy qua hàm Start
            this.isStarted = true;
        }

        /// <summary>
        /// Hàm này gọi khi đối tượng được kích hoạt
        /// </summary>
        private void OnEnable()
        {
            /// Nếu chưa chạy qua hàm Start
            if (!this.isStarted)
            {
                /// Bỏ qua
                return;
            }
            /// Thực hiện đi theo Leader
            this.StartCoroutine(this.DoFollowLeaderContinuously());
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Thực hiện đi theo Leader liên tục
        /// </summary>
        /// <returns></returns>
        private IEnumerator DoFollowLeaderContinuously()
        {
            /// Lặp liên tục
            while (true)
            {
                /// Nghỉ
                yield return null;

                /// Toác
                if (Global.Data == null || Global.Data.Leader == null)
                {
                    /// Bỏ qua
                    continue;
                }
                /// Không có minimap
                else if (Global.CurrentMap.LocalMapSprite == null)
                {
                    /// Bỏ qua
                    continue;
                }

                /// Vị trí của Leader quy đổi sang bản đồ nhỏ
                Vector2 localMapPos = KTGlobal.WorldPositionToWorldNavigationMapPosition(Global.Data.Leader.PositionInVector2);
                /// Cập nhật vị trí của đối tượng
                this.transform.localPosition = new Vector3(localMapPos.x, localMapPos.y, -10);
            }
        }
        #endregion
    }
}
