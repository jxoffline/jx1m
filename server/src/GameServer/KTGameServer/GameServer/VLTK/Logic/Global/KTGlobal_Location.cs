using GameServer.Logic;
using System;
using System.Windows;

namespace GameServer.KiemThe
{
    /// <summary>
    /// Các phương thức và đối tượng toàn cục của Kiếm Thế
    /// </summary>
    public static partial class KTGlobal
    {
        #region Khoảng cách
        /// <summary>
        /// Trả về khoảng cách giữa 2 điểm
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static float GetDistanceBetweenPoints(Point p1, Point p2)
        {
            return (float) Math.Sqrt((p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y));
        }

        /// <summary>
        /// Trả về khoảng cách giữa 2 người chơi
        /// </summary>
        /// <param name="player1"></param>
        /// <param name="player2"></param>
        /// <returns></returns>
        public static float GetDistanceBetweenPlayers(KPlayer player1, KPlayer player2)
        {
            /// Nếu khác bản đồ
            if (player1.CurrentMapCode != player2.CurrentMapCode)
            {
                /// Không tìm được
                return 99999999;
            }
            /// Nếu khác phụ bản
            else if (player1.CurrentCopyMapID != player2.CurrentCopyMapID)
            {
                /// Không tìm được
                return 99999999;
            }
            return KTGlobal.GetDistanceBetweenPoints(player1.CurrentPos, player2.CurrentPos);
        }

        /// <summary>
        /// Trả về khoảng cách giữa 2 đối tượng
        /// </summary>
        /// <param name="go1"></param>
        /// <param name="go2"></param>
        /// <returns></returns>
        public static float GetDistanceBetweenGameObjects(GameServer.Logic.GameObject go1, GameServer.Logic.GameObject go2)
        {
            /// Nếu khác bản đồ
            if (go1.CurrentMapCode != go2.CurrentMapCode)
            {
                /// Không tìm được
                return 99999999;
            }
            /// Nếu khác phụ bản
            else if (go1.CurrentCopyMapID != go2.CurrentCopyMapID)
            {
                /// Không tìm được
                return 99999999;
            }
            return KTGlobal.GetDistanceBetweenPoints(go1.CurrentPos, go2.CurrentPos);
        }
        #endregion
    }
}
