using FS.Drawing;
using FS.GameEngine.Data;
using FS.GameEngine.Logic;
using Server.Tools.AStarEx;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FS.VLTK
{
    /// <summary>
    /// Các hàm toàn cục dùng trong Kiếm Thế
    /// </summary>
    public static partial class KTGlobal
    {
        /// <summary>
        /// Thời điểm chuyển bản đồ thành công
        /// </summary>
        public static long LastChangeMapSuccessfulTicks { get; set; } = -1;

        /// <summary>
        /// Kiểm tra vị trí có nằm trong khu an toàn không
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public static bool InSafeArea(Vector2 position)
        {
            /// Nếu không có khu an toàn
            if (Global.CurrentMapData.SafeAreas == null)
            {
                /// Không có
                return false;
            }

            try
            {
                /// Tọa độ lưới
                Vector2 gridPos = KTGlobal.WorldPositionToGridPosition(position);
                /// Trả về kết quả
                return Global.CurrentMapData.SafeAreas[(int) gridPos.x, (int) gridPos.y] == 1;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Kiểm tra 2 vị trí có thể đến được không
        /// </summary>
        /// <param name="fromPos"></param>
        /// <param name="toPos"></param>
        /// <returns></returns>
        public static bool HasPath(Vector2 fromPos, Vector2 toPos)
		{
            /// Nếu toác
            if (Global.CurrentMapData == null)
			{
                return false;
			}
            /// Tọa độ lưới
            Vector2 fromGridPos = KTGlobal.WorldPositionToGridPosition(fromPos);
            Vector2 toGridPos = KTGlobal.WorldPositionToGridPosition(toPos);
            /// Trả về kết quả
            return Global.CurrentMapData.BlurPositions[(int) fromGridPos.x, (int) fromGridPos.y] / 2 == Global.CurrentMapData.BlurPositions[(int) toGridPos.x, (int) toGridPos.y] / 2;
        }

        /// <summary>
        /// Tìm điểm đầu tiên nằm trên đường đi không chứa vật cản
        /// </summary>
        /// <param name="fromPos"></param>
        /// <param name="toPos"></param>
        /// <returns></returns>
        public static Vector2 FindLinearNoObsPoint(Vector2 fromPos, Vector2 toPos)
        {
            /// Nếu toác
            if (Global.CurrentMapData == null)
			{
                return fromPos;
			}

            /// Vị trí hiện tại theo tọa độ lưới
            Vector2 fromGridPos = KTGlobal.WorldPositionToGridPosition(fromPos);
            /// Vị trí đích đến theo tọa độ lưới
            Vector2 toGridPos = KTGlobal.WorldPositionToGridPosition(toPos);

            Point fromGridPOINT = new Point((int) fromGridPos.x, (int) fromGridPos.y);
            Point toGridPOINT = new Point((int) toGridPos.x, (int) toGridPos.y);

            /// Nếu 2 vị trí trùng nhau
            if (fromGridPOINT == toGridPOINT)
            {
                return fromPos;
            }

            /// Nếu tìm thấy điểm không chứa vật cản
            if (KTGlobal.FindLinearNoObsPoint(fromGridPOINT, toGridPOINT, out Point newNoObsPoint))
            {
                Vector2 newNoObsPos = new Vector2((int) newNoObsPoint.X, (int) newNoObsPoint.Y);
                /// Trả ra kết quả
                return KTGlobal.GridPositionToWorldPosition(newNoObsPos);
            }
            /// Trả ra kết quả nếu không tìm thấy vị trí thỏa mãn
            return fromPos;
        }

        /// <summary>
        /// Tìm ô gần nhất với vị trí đích không chứa vật cản
        /// </summary>
        /// <param name="gameMap"></param>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static bool FindLinearNoObsPoint(Point p0, Point p1, out Point point)
        {
            point = new Point(0, 0);
            List<ANode> path = new List<ANode>();
            KTGlobal.Bresenham(path, (int) p0.X, (int) p0.Y, (int) p1.X, (int) p1.Y, Global.CurrentMapData.Obstructions);
            if (path.Count > 1)
            {
                point = new Point(path[path.Count - 1].x, path[path.Count - 1].y);
                path.Clear();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Giải thuật Bresenham vẽ tập hợp các điểm tạo thành đường thẳng
        /// </summary>
        /// <param name="s"></param>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        public static bool Bresenham(List<ANode> s, int x1, int y1, int x2, int y2, byte[,] obs)
        {
            int t, x, y, dx, dy, error;
            bool flag = Math.Abs(y2 - y1) > Math.Abs(x2 - x1);
            if (flag)
            {
                t = x1;
                x1 = y1;
                y1 = t;
                t = x2;
                x2 = y2;
                y2 = t;
            }

            bool reverse = false;
            if (x1 > x2)
            {
                t = x1;
                x1 = x2;
                x2 = t;
                t = y1;
                y1 = y2;
                y2 = t;
                reverse = true;
            }
            dx = x2 - x1;
            dy = Math.Abs(y2 - y1);
            error = dx / 2;
            for (x = x1, y = y1; x <= x2; ++x)
            {
                if (flag)
                {
                    if (null != s)
                    {
                        s.Add(new ANode(y, x));
                    }
                }
                else
                {
                    if (null != s)
                    {
                        s.Add(new ANode(x, y));
                    }
                }

                error -= dy;
                if (error < 0)
                {
                    if (y1 < y2)
                        ++y;
                    else
                        --y;
                    error += dx;
                }
            }

            if (reverse)
            {
                s.Reverse();
            }

            List<ANode> s1 = GetLinearPath(s, obs);
            bool res = (s1.Count == s.Count);

            s.Clear();
            for (int i = 0; i < s1.Count; i++)
            {
                s.Add(s1[i]);
            }

            return res;
        }

        /// <summary>
        /// Trả về danh sách các điểm có thể đi được trên đường đi
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private static List<ANode> GetLinearPath(List<ANode> s, byte[,] obs)
        {
            List<ANode> s1 = new List<ANode>();
            for (int i = 0; i < s.Count; i++)
            {
                if (s[i].x >= obs.GetUpperBound(0) || s[i].y >= obs.GetUpperBound(1))
                {
                    continue;
                }

                if (0 == obs[s[i].x, s[i].y])
                {
                    break;
                }

                s1.Add(s[i]);
            }

            return s1;
        }

        /// <summary>
        /// Trả về 1 điểm xung quanh 4 hướng mà không có vật cản
        /// </summary>
        /// <param name="gridPoint"></param>
        /// <param name="obs"></param>
        /// <param name="currentMapData"></param>
        /// <returns></returns>
        public static Point GetRandomNoObsPointAroundPos(Point gridPoint)
        {
            GMapData mapData = Global.CurrentMapData;
            byte[,] obs = Global.CurrentMapData.Obstructions;

            int gridX = (int) (gridPoint.X);
            int gridY = (int) (gridPoint.Y);
            if (gridX >= obs.GetUpperBound(0) || gridY >= obs.GetUpperBound(1))
            {
                return gridPoint;
            }
            if (obs[gridX, gridY] == 1)
            {
                return gridPoint;
            }
            Point p = gridPoint;
            int maxGridX = (int) ((mapData.MapWidth - 1) / mapData.GridSizeX) + 1;
            int maxGridY = (int) ((mapData.MapHeight - 1) / mapData.GridSizeY) + 1;
            int added = 1;
            int newX1 = 0;
            int newY1 = 0;
            int newX2 = 0;
            int newY2 = 0;
            while (true)
            {
                newX1 = gridX + added;
                newY1 = gridY + added;
                newX2 = gridX - added;
                newY2 = gridY - added;
                int total = 8;
                if ((0 <= newX1 && newX1 < maxGridX) && (0 <= newY1 && newY1 < maxGridY))
                {
                    total--;
                    if (obs[newX1, newY1] == 1)
                    {
                        p = new Point(newX1, newY1);
                        break;
                    }
                }
                if ((0 <= newX1 && newX1 < maxGridX) && (0 <= newY2 && newY2 < maxGridY))
                {
                    total--;
                    if (obs[newX1, newY2] == 1)
                    {
                        p = new Point(newX1, newY2);
                        break;
                    }
                }
                if ((0 <= newX2 && newX2 < maxGridX) && (0 <= newY1 && newY1 < maxGridY))
                {
                    total--;
                    if (obs[newX2, newY1] == 1)
                    {
                        p = new Point(newX2, newY1);
                        break;
                    }
                }
                if ((0 <= newX2 && newX2 < maxGridX) && (0 <= newY2 && newY2 < maxGridY))
                {
                    total--;
                    if (obs[newX2, newY2] == 1)
                    {
                        p = new Point(newX2, newY2);
                        break;
                    }
                }
                if ((0 <= newX1 && newX1 < maxGridX))
                {
                    total--;
                    if (obs[newX1, gridY] == 1)
                    {
                        p = new Point(newX1, gridY);
                        break;
                    }
                }
                if ((0 <= newY1 && newY1 < maxGridY))
                {
                    total--;
                    if (obs[gridX, newY1] == 1)
                    {
                        p = new Point(gridX, newY1);
                        break;
                    }
                }
                if ((0 <= newX2 && newX2 < maxGridX))
                {
                    total--;
                    if (obs[newX2, gridY] == 1)
                    {
                        p = new Point(newX2, gridY);
                        break;
                    }
                }
                if ((0 <= newY2 && newY2 < maxGridY))
                {
                    total--;
                    if (obs[gridX, newY2] == 1)
                    {
                        p = new Point(gridX, newY2);
                        break;
                    }
                }
                if (total >= 8)
                {
                    break;
                }
                added++;
            }
            return p;
        }


    }
}
