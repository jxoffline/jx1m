using System.Collections.Generic;
using FS.Drawing;
using UnityEngine;
using FS.GameEngine.Logic;
using FS.GameEngine.Network;
using FS.GameEngine.Sprite;
using FS.VLTK.Utilities.Algorithms;
using FS.Tools.AStar;
using System;
using FS.VLTK;

namespace FS.GameEngine.Scene
{
    /// <summary>
    /// Quản lý tự tìm đường
    /// </summary>
	public partial class GScene
	{
		#region Path Finder
		/// <summary>
		/// Tìm đường sử dụng giải thuật A*
		/// </summary>
		/// <param name="fromPos">Tọa độ lưới điểm bắt đầu</param>
		/// <param name="toPos">Tọa độ lưới điểm kết thúc</param>
		/// <returns></returns>
		private List<Vector2> FindPathUsingAStar(Point fromPos, Point toPos)
        {
			if (null == this.pathFinderFast)
			{
				this.pathFinderFast = new PathFinderFast(this.CurrentMapData.Obstructions, this.CurrentMapData.DynamicObstructions, this.CurrentMapData.OpenedDynamicObsLabels)
				{
					Formula = HeuristicFormula.Manhattan,
					Diagonals = true,
					HeuristicEstimate = 2,
					ReopenCloseNodes = true,
					SearchLimit = 2147483647,
					Punish = null,
					MaxNum = Math.Max(this.CurrentMapData.GridSizeXNum, this.CurrentMapData.GridSizeYNum),
				};
			}

			this.pathFinderFast.EnablePunish = false;
			List<PathFinderNode> nodeList = this.pathFinderFast.FindPath(fromPos, toPos);
			if (null == nodeList || nodeList.Count <= 0)
			{
				return new List<Vector2>();
			}

			List<Vector2> path = new List<Vector2>();

			for (int i = 0; i < nodeList.Count; i++)
			{
				path.Add(new Vector2(nodeList[i].X, nodeList[i].Y));
			}

			/// Làm mịn đường đi
			path = LineSmoother.SmoothPath(path, this.CurrentMapData.Obstructions);

			return path;
		}

		/// <summary>
		/// Tìm đường giữa 2 vị trí
		/// <para>Sử dụng giải thuật A* để tìm đường ngắn nhất</para>
		/// <para>Sử dụng thuật toán Ramer–Douglas–Peucker để làm mịn đường đi</para>
		/// </summary>
		/// <param name="fromPos">Tọa độ thực vị trí bắt đầu</param>
		/// <param name="toPos">Tọa độ thực vị trí kết thúc</param>
		public List<Vector2> FindPath(GSprite sprite, ref Vector2 fromPos, ref Vector2 toPos)
		{
			/// Nếu không có đường đi
			if (!KTGlobal.HasPath(fromPos, toPos))
			{
				return null;
			}

			/// Vị trí đích ban đầu
			Vector2 originToPos = toPos;

			Vector2 fromGridPos = KTGlobal.WorldPositionToGridPosition(fromPos);
			Vector2 toGridPos = KTGlobal.WorldPositionToGridPosition(toPos);

            #region AStar - Old
            Point fromGridPOINT = new Point((int)fromGridPos.x, (int)fromGridPos.y);
			Point toGridPOINT = new Point((int)toGridPos.x, (int)toGridPos.y);

			/// Nếu vị trí hiện đang đứng nằm trên ô có vật cản
			if (!this.CanMove(fromGridPOINT))
			{
				/// Tìm một vị trí bất kỳ xung quanh không có vật cản
				fromGridPOINT = KTGlobal.GetRandomNoObsPointAroundPos(fromGridPOINT);

				/// Cập nhật lại vị trí FromPos
				fromPos = KTGlobal.GridPositionToWorldPosition(new Vector2(fromGridPOINT.X, fromGridPOINT.Y));

				/// Nếu không tìm thấy vị trí nào xung quanh không có vật cản
				if (!this.CanMove(fromGridPOINT))
				{
					return null;
				}
			}
			
			/// Nếu vị trí đích nằm trên ô có vật cản
			if (!this.CanMove(toGridPOINT))
			{
				/// Tìm một điểm bất kỳ trên đường nối 2 điểm mà không chứa vật cản
				if (this.FindLinearNoObsMaxPoint(sprite, toGridPOINT, out Point maxPoint))
				{
					toGridPOINT = maxPoint;

					/// Cập nhật lại vị trí ToPos
					toPos = KTGlobal.GridPositionToWorldPosition(new Vector2(toGridPOINT.X, toGridPOINT.Y));
				}
				/// Nếu không tìm thấy điểm không chứa vật cản
				else
                {
					return null;
                }
			}

			/// Nếu vị trí đầu và cuối cùng một ô lưới thì cho chạy giữa 2 vị trí này luôn
			if (fromGridPOINT == toGridPOINT)
			{
				return new List<Vector2>()
				{
					fromPos, toPos
				};
			}
			#endregion

			/// Nếu vị trí đích không thể đi được
			if (!this.CanMove(toGridPOINT))
            {
				return null;
            }

            /// Sử dụng A* tìm đường đi
            List<Vector2> nodes = this.FindPathUsingAStar(fromGridPOINT, toGridPOINT);

			/// Nếu danh sách nút tìm được nhỏ hơn 2
			if (nodes.Count < 2)
            {
				return new List<Vector2>();
            }

			/// Danh sách điểm trên đường đi
			List<Vector2> result = new List<Vector2>();

			/*
			/// Nếu khoảng cách từ vị trí bắt đầu đến điểm thứ 2 trong danh sách đường đi tìm được lớn hơn khoảng cách từ điểm đầu tiên đến điểm thứ 2 thì thêm vị trí bắt đầu vào đầu danh sách
			if (Vector2.Distance(fromPos, KTGlobal.GridPositionToWorldPosition(nodes[1])) > Vector2.Distance(KTGlobal.GridPositionToWorldPosition(nodes[0]), KTGlobal.GridPositionToWorldPosition(nodes[1])))
            {
				result.Add(fromPos);
				result.Add(KTGlobal.GridPositionToWorldPosition(nodes[0]));
            }
			/// Nếu vị trí bắt đầu khác điểm đầu tiên tìm được trong danh sách đường đi thì thay thế vị trí bắt đầu vào điểm đầu tiên
			else if (KTGlobal.GridPositionToWorldPosition(nodes[0]) != fromPos)
			*/
			{
				result.Add(fromPos);
            }

			/// Thêm tất cả các nút tìm được trên đường đi vào danh sách
			for (int i = 1; i < nodes.Count; i++)
			{
				result.Add(KTGlobal.GridPositionToWorldPosition(nodes[i]));
			}

			/*
			/// Nếu khoảng cách từ điểm gần cuối danh sách đến vị trí đích nhỏ hơn khoảng cách từ điểm gần cuối danh sách đến điểm cuối danh sách thì thay thế vị trí cuối danh sách bằng vị trí đích
			if (Vector2.Distance(KTGlobal.GridPositionToWorldPosition(nodes[nodes.Count - 2]), toPos) < Vector2.Distance(KTGlobal.GridPositionToWorldPosition(nodes[nodes.Count - 2]), KTGlobal.GridPositionToWorldPosition(nodes[nodes.Count - 1])))
			*/
			{
				result[result.Count - 1] = toPos;
			}
			/*
			/// Nếu vị trí đích khác vị trí hiện tại
			else if (KTGlobal.GridPositionToWorldPosition(nodes[nodes.Count - 1]) != toPos)
            {
				result.Add(toPos);
			}
			*/

			return result;
        }
        #endregion
    }

}
