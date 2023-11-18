using GameServer.KiemThe;
using GameServer.KiemThe.Utilities;
using static GameServer.Logic.KTMapManager;

namespace GameServer.Logic
{
    /// <summary>
    /// Đối tượng Pet
    /// </summary>
    public partial class Pet
    {
        /// <summary>
        /// Thời gian giãn cách giữa các lần đồng bộ vị trí
        /// </summary>
        private const int SyncPosInterval = 500;

        /// <summary>
        /// Khoảng cách lệch tối đa cho phép
        /// </summary>
        private const int AllowMaxDistance = 300;

        /// <summary>
        /// Thời điểm lần trước cập nhật vị trí
        /// </summary>
        public long LastSyncPosTick { get; set; }

        /// <summary>
        /// ID kỹ năng sử dụng lần trước
        /// </summary>
        public int LastUseSkillID { get; set; } = -1;

        /// <summary>
        /// Đồng bộ vị trí từ Client gửi lên
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="resultPos"></param>
        /// <returns></returns>
        public bool ClientSyncPos(UnityEngine.Vector2 pos, out UnityEngine.Vector2 resultPos)
        {
            /// Thiết lập vị trí hợp lệ
            resultPos = pos;

            /// Nếu đã chết
            if (this.IsDead())
            {
                /// Bỏ qua
                return false;
            }

            /// Khoảng lệch thời gian
            long diffTick = KTGlobal.GetCurrentTimeMilis() - this.LastSyncPosTick;
            /// Nếu chưa đến thời gian đồng bộ vị trí
            if (diffTick < Pet.SyncPosInterval)
            {
                /// Chưa đến lúc
                return false;
            }
            /// Cập nhật thời điểm cập nhật vị trí
            this.LastSyncPosTick = KTGlobal.GetCurrentTimeMilis();

            /// Vị trí cũ
            UnityEngine.Vector2 oldPos = new UnityEngine.Vector2((int) this.CurrentPos.X, (int) this.CurrentPos.Y);

            /// Nếu đích đến đang ở trong điểm Block
            if (KTGlobal.InObs(this.CurrentMapCode, (int) pos.x, (int) pos.y, this.CurrentCopyMapID))
            {
                /// Gắn lại vị trí
                resultPos = oldPos;
                /// Có lỗi
                return true;
            }

            /// Khoảng lệch
            float distance = UnityEngine.Vector2.Distance(pos, oldPos);
            /// Khoảng lệch cho phép
            float allowDistance = this.Owner.GetCurrentRunSpeed() * diffTick / 1000f;
            /// Thông tin bản đồ hiện tại
            GameMap gameMap = KTMapManager.Find(this.CurrentMapCode);
            /// Nếu ngoài ngưỡng cho phép
            if (distance - allowDistance >= Pet.AllowMaxDistance)
            {
                if (null == gameMap)
                {
                    /// Gắn lại vị trí
                    resultPos = oldPos;
                    /// Có lỗi
                    return true;
                }

                /// Đích đến hợp lệ
                pos = KTMath.FindPointInVectorWithDistance(oldPos, pos - oldPos, allowDistance);
                /// Vị trí mới hợp lệ
                KTGlobal.FindLinearNoObsPoint(gameMap, this.CurrentPos, new System.Windows.Point(pos.x, pos.y), out System.Windows.Point destPoint, this.CurrentCopyMapID);
                /// Cập nhật vị trí
                this.CurrentPos = destPoint;
                /// Cập nhật vào map grid
                gameMap.Grid.MoveObject((int) destPoint.X, (int) destPoint.Y, this);
                /// Gắn lại vị trí
                resultPos = new UnityEngine.Vector2((int) destPoint.X, (int) destPoint.Y);
                /// Có lỗi
                return true;
            }

            /// Cập nhật vị trí
            this.CurrentPos = new System.Windows.Point(pos.x, pos.y);
            /// Cập nhật vào map grid
            gameMap.Grid.MoveObject((int) pos.x, (int) pos.y, this);
            /// Hợp lệ
            return true;
        }

        /// <summary>
        /// Kiểm tra đối tượng có hiển thị với đối tượng khác tương ứng không
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public new bool VisibleTo(GameObject go)
        {
            /// Theo chủ nhân
            return !this.Owner.IsInvisible() || this.Owner.VisibleTo(go);
        }
    }
}
