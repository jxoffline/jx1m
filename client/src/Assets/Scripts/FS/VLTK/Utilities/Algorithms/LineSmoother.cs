using FS.Drawing;
using Server.Tools.AStarEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace FS.VLTK.Utilities.Algorithms
{
    /// <summary>
    /// Làm mịn đường thẳng
    /// </summary>
    public static class LineSmoother
    {
		/// <summary>
		/// Hàm nội suy
		/// </summary>
		/// <param name="x0"></param>
		/// <param name="y0"></param>
		/// <param name="x1"></param>
		/// <param name="y1"></param>
		/// <returns></returns>
		private static List<Vector2> Interpolate(int x0, int y0, int x1, int y1)
		{
			List<Vector2> line = new List<Vector2>();
			int sx, sy, dx, dy, err, e2;

			dx = Math.Abs(x1 - x0);
			dy = Math.Abs(y1 - y0);

			sx = (x0 < x1) ? 1 : -1;
			sy = (y0 < y1) ? 1 : -1;

			err = dx - dy;

			while (true)
			{
				line.Add(new Vector2(x0, y0));

				if (x0 == x1 && y0 == y1)
				{
					break;
				}

				e2 = 2 * err;
				if (e2 > -dy)
				{
					err -= dy;
					x0 += sx;
				}
				if (e2 < dx)
				{
					err += dx;
					y0 += sy;
				}
			}

			return line;
		}

		/// <summary>
		/// Làm mịn đường đi
		/// </summary>
		/// <param name="path">Đường đi</param>
		/// <param name="Obstructions">Ma trận mô tả bản đồ gốc (1: Đi được, 0: Không đi được)</param>
		/// <returns></returns>
		public static List<Vector2> SmoothPath(List<Vector2> path, byte[,] Obstructions)
		{
			if (path.Count < 2)
			{
				return path;
			}

			List<Vector2> newPath = new List<Vector2>();

			int len = path.Count;
			int x0 = (int)path[0].x,        // path start x
				y0 = (int)path[0].y,        // path start y
				x1 = (int)path[len - 1].x,  // path end x
				y1 = (int)path[len - 1].y,  // path end y
				sx, sy,						// current start coordinate
				ex, ey,						// current end coordinate
				i, j;
			Vector2 coord, testCoord;
			List<Vector2> line;
			bool blocked;

			sx = x0;
			sy = y0;
			newPath.Add(new Vector2(sx, sy));

			for (i = 2; i < len; ++i)
			{
				coord = path[i];
				ex = (int)coord.x;
				ey = (int)coord.y;
				line = LineSmoother.Interpolate(sx, sy, ex, ey);

				blocked = false;
				for (j = 1; j < line.Count; ++j)
				{
					testCoord = line[j];

					if (Obstructions[(int)testCoord.x, (int)testCoord.y] == 0)
					{
						blocked = true;
						break;
					}
				}
				if (blocked)
				{
					Vector2 lastValidCoord = path[i - 1];
					newPath.Add(lastValidCoord);
					sx = (int)lastValidCoord.x;
					sy = (int)lastValidCoord.y;
				}
			}
			newPath.Add(new Vector2(x1, y1));

			return newPath;
		}
	}
}
