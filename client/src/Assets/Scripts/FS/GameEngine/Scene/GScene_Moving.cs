using System.Collections.Generic;
using FS.Drawing;
using Server.Tools.AStarEx;
using FS.GameEngine.Logic;
using FS.GameEngine.Sprite;
using FS.Tools.AStar;
using FS.VLTK;
using UnityEngine;

namespace FS.GameEngine.Scene
{
	/// <summary>
	/// Các hàm bổ trợ di chuyển đối tượng
	/// </summary>
	public partial class GScene
    {
        #region Chức năng bổ trợ

        /// <summary>
        /// Tìm ô gần nhất xung quanh không chứa vật cản
        /// </summary>
        /// <param name="sprite"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        private bool FindLinearNoObsMaxPoint(GSprite sprite, Point p, out Point maxPoint)
        {
            maxPoint = new Point(0, 0);
            List<ANode> path = new List<ANode>();
            KTGlobal.Bresenham(path, (int)((sprite.Coordinate.X / CurrentMapData.GridSizeX)), (int)((sprite.Coordinate.Y / CurrentMapData.GridSizeY)), (int)((p.X)), (int)((p.Y)), CurrentMapData.Obstructions);
            if (path.Count > 1)
            {
                maxPoint = new Point(path[path.Count - 1].x, path[path.Count - 1].y);
                path.Clear();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Kiểm tra ô tương ứng có thể đi được không (tọa độ thực)
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public bool CanMoveByWorldPos(Point point)
        {
            Vector2 gridPos = KTGlobal.WorldPositionToGridPosition(new UnityEngine.Vector2(point.X, point.Y));
            return this.CanMove(new Point((int) gridPos.x, (int) gridPos.y));
        }

        /// <summary>
        /// Kiểm tra ô tương ứng có thể đi được không (tọa độ thực)
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public bool CanMoveByWorldPos(Vector2 point)
        {
            Vector2 gridPos = KTGlobal.WorldPositionToGridPosition(point);
            return this.CanMove(new Point((int) gridPos.x, (int) gridPos.y));
        }

        /// <summary>
        /// Kiểm tra ô tương ứng có thể đi được không (tọa độ lưới)
        /// </summary>
        /// <param name="objType"></param>
        /// <param name="node"></param>
        /// <returns></returns>
        public bool CanMove(Point node)
        {
            /// Nếu vượt quá phạm vi X
            if (node.X >= CurrentMapData.Obstructions.GetUpperBound(0) || node.X >= this.CurrentMapData.OriginGridSizeXNum)
            {
                return false;
            }
            /// Nếu vượt quá phạm vi Y
            else if (node.Y >= CurrentMapData.Obstructions.GetUpperBound(1) || node.Y >= this.CurrentMapData.OriginGridSizeYNum)
            {
                return false;
            }
            /// Nếu tọa độ âm
            else if (node.X < 0 || node.Y < 0)
            {
                return false;
            }

            /// Nếu dính điểm Block
            if (CurrentMapData.Obstructions[node.X, node.Y] == 0)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Thuật toán tìm đường A*
        /// </summary>
        private PathFinderFast pathFinderFast = null;
        #endregion
    }
}
