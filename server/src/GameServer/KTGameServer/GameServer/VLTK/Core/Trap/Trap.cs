using GameServer.Interface;
using GameServer.KiemThe;
using GameServer.KiemThe.Logic;
using System.Windows;

namespace GameServer.Logic
{
    /// <summary>
    /// Đối tượng bẫy
    /// </summary>
    public class Trap : IObject
    {
        /// <summary>
        /// ID bẫy
        /// </summary>
        public int TrapID { get; set; }

        /// <summary>
        /// Chủ nhân
        /// </summary>
        public GameObject Owner { get; set; }

        /// <summary>
        /// ID Res
        /// </summary>
        public int ResID { get; set; }

        /// <summary>
        /// Thời gian tồn tại
        /// </summary>
        public float LifeTime { get; set; }

        /// <summary>
        /// Kiểm tra bẫy có thể được hiển thị với người chơi tương ứng không
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public bool IsVisibleTo(KPlayer player)
        {
            /// Nếu là chủ nhân bẫy luôn
            if (player == this.Owner)
            {
                return true;
            }
            /// Nếu cấp độ của chủ nhân bẫy dưới cấp độ của người chơi
            else if (this.Owner.m_Level < player.m_Level - KTGlobal.DiffLevelToDetectTrap)
            {
                return true;
            }
            /// Nếu người chơi tương ứng có trạng thái phát hiện bẫy và cấp độ thỏa mãn
            else if (player.m_DetectTrap && player.m_Level >= this.Owner.m_Level - KTGlobal.DiffLevelToDetectTrap)
            {
                return true;
            }
            /// Nếu không phải người chơi
            else if (!(this.Owner is KPlayer))
            {
                return false;
            }
            else
            {
                /// Người chơi chủ nhân bẫy
                KPlayer thisPlayer = this.Owner as KPlayer;

                /// Nếu là đồng đội
                if (KTGlobal.IsTeamMate(thisPlayer, player))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// ID bản đồ
        /// </summary>
        public int MapCode { get; set; }

        #region IObject
        /// <summary>
        /// Loại đối tượng
        /// </summary>
        public ObjectTypes ObjectType { get; set; }

        /// <summary>
        /// Tọa độ lưới
        /// </summary>
        public Point CurrentGrid { get; set; }

        /// <summary>
        /// Tọa độ thực
        /// </summary>
        public Point CurrentPos { get; set; }

        /// <summary>
        /// ID bản đồ hiện tại
        /// </summary>
        public int CurrentMapCode
        {
            get
            {
                lock (this)
                {
                    return this.MapCode;
                }
            }
        }

        /// <summary>
        /// ID phụ bản hiện tại
        /// </summary>
        public int CurrentCopyMapID { get; set; } = -1;

        /// <summary>
        /// Hướng quay hiện tại
        /// </summary>
        public KiemThe.Entities.Direction CurrentDir { get; set; } = KiemThe.Entities.Direction.DOWN;
        #endregion
    }
}
