using HSGameEngine.Tools.AStarEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GameServer.Logic.KTMapManager.MapGridManager;

namespace GameServer.KiemThe
{
    /// <summary>
    /// Các phương thức và đối tượng toàn cục của Kiếm Thế
    /// </summary>
    public static partial class KTGlobal
    {
        #region Bresenham
        /// <summary>
        /// Giải thuật Bresenham tìm tập hợp các điểm tạo thành đường thẳng tương ứng
        /// </summary>
        /// <param name="s"></param>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        public static bool Bresenham(List<ANode> s, int x1, int y1, int x2, int y2, NodeGrid nodeGrid, int copySceneID)
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

            List<ANode> s1 = GetLinearPath(s, nodeGrid, copySceneID);
            bool res = (s1.Count == s.Count);

            s.Clear();
            for (int i = 0; i < s1.Count; i++)
            {
                s.Add(s1[i]);
            }

            return res;
        }

        /// <summary>
        /// Trả về danh sách các điểm có thể đi được tương ứng
        /// </summary>
        /// <param name="s"></param>
        /// <param name="nodeGrid"></param>
        /// <param name="copySceneID"></param>
        /// <returns></returns>
        private static List<ANode> GetLinearPath(List<ANode> s, NodeGrid nodeGrid, int copySceneID)
        {
            List<ANode> s1 = new List<ANode>();
            for (int i = 0; i < s.Count; i++)
            {
                /// Nếu không vào được
                if (!nodeGrid.CanEnter(s[i].x, s[i].y, copySceneID))
                {
                    break;
                }

                s1.Add(s[i]);
            }

            return s1;
        }
        #endregion
    }
}
