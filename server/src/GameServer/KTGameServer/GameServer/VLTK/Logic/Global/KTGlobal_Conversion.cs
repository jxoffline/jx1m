using GameServer.KiemThe.Core.Task;
using GameServer.KiemThe.Logic;
using GameServer.KiemThe.Utilities;
using GameServer.Logic;
using HSGameEngine.Tools.AStarEx;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using static GameServer.Logic.KTMapManager;
using static GameServer.Logic.KTMapManager.MapGridManager;

namespace GameServer.KiemThe
{
    /// <summary>
    /// Các phương thức và đối tượng toàn cục của Kiếm Thế
    /// </summary>
    public static partial class KTGlobal
    {
        #region Chuyển đổi vị trí
        /// <summary>
        /// Số lần thử tối đa tìm điểm ngẫu nhiên xung quanh không chứa vật cản mà có thể trực tiếp đi tới được theo đường thẳng
        /// </summary>
        private const int TryGetRandomLinearNoObsPointMaxTimes = 10;

        /// <summary>
        /// Chuyển từ tọa độ thực sang tọa độ lưới
        /// </summary>
        /// <param name="map"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        public static Point WorldPositionToGridPosition(GameMap map, Point position)
        {
            return new Point(position.X / map.MapGridWidth, position.Y / map.MapGridHeight);
        }

        /// <summary>
        /// Chuyển từ tọa độ lưới sang tọa độ thực
        /// </summary>
        /// <param name="map"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        public static Point GridPositionToWorldPosition(GameMap map, Point position)
        {
            return new Point(position.X * map.MapGridWidth, position.Y * map.MapGridHeight);
        }

        /// <summary>
        /// Chuyển từ tọa độ thực sang tọa độ lưới
        /// </summary>
        /// <param name="map"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        public static UnityEngine.Vector2 WorldPositionToGridPosition(GameMap map, UnityEngine.Vector2 position)
        {
            return new UnityEngine.Vector2(position.x / map.MapGridWidth, position.y / map.MapGridHeight);
        }

        /// <summary>
        /// Chuyển từ tọa độ lưới sang tọa độ thực
        /// </summary>
        /// <param name="map"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        public static UnityEngine.Vector2 GridPositionToWorldPosition(GameMap map, UnityEngine.Vector2 position)
        {
            return new UnityEngine.Vector2(position.x * map.MapGridWidth, position.y * map.MapGridHeight);
        }

        /// <summary>
        /// Chuyển từ tọa độ thực sang tọa độ lưới
        /// </summary>
        /// <param name="map"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        public static UnityEngine.Vector2Int WorldPositionToGridPosition(GameMap map, UnityEngine.Vector2Int position)
        {
            return new UnityEngine.Vector2Int(position.x / map.MapGridWidth, position.y / map.MapGridHeight);
        }

        /// <summary>
        /// Chuyển từ tọa độ lưới sang tọa độ thực
        /// </summary>
        /// <param name="map"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        public static UnityEngine.Vector2Int GridPositionToWorldPosition(GameMap map, UnityEngine.Vector2Int position)
        {
            return new UnityEngine.Vector2Int(position.x * map.MapGridWidth, position.y * map.MapGridHeight);
        }

        /// <summary>
        /// Tìm ô gần nhất với vị trí đích không chứa vật cản
        /// </summary>
        /// <param name="gameMap"></param>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static bool FindLinearNoObsPoint(GameMap gameMap, Point p0, Point p1, out Point point, int copySceneID)
        {
            point = new Point(0, 0);
            List<ANode> path = new List<ANode>();
            KTGlobal.Bresenham(path, (int) p0.X, (int) p0.Y, (int) p1.X, (int) p1.Y, gameMap.MyNodeGrid, copySceneID);
            if (path.Count > 1)
            {
                point = new Point(path[path.Count - 1].x, path[path.Count - 1].y);
                path.Clear();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Tìm điểm đầu tiên nằm trên đường đi không chứa vật cản
        /// </summary>
        /// <param name="map"></param>
        /// <param name="fromPos"></param>
        /// <param name="toPos"></param>
        /// <returns></returns>
        public static UnityEngine.Vector2 FindLinearNoObsPoint(GameMap map, UnityEngine.Vector2 fromPos, UnityEngine.Vector2 toPos, int copySceneID)
        {
            /// Vị trí hiện tại theo tọa độ lưới
            UnityEngine.Vector2 fromGridPos = KTGlobal.WorldPositionToGridPosition(map, fromPos);
            /// Vị trí đích đến theo tọa độ lưới
            UnityEngine.Vector2 toGridPos = KTGlobal.WorldPositionToGridPosition(map, toPos);

            Point fromGridPOINT = new Point((int) fromGridPos.x, (int) fromGridPos.y);
            Point toGridPOINT = new Point((int) toGridPos.x, (int) toGridPos.y);

            /// Nếu 2 vị trí trùng nhau
            if (fromGridPOINT == toGridPOINT)
            {
                return fromPos;
            }

            /// Nếu tìm thấy điểm không chứa vật cản
            if (KTGlobal.FindLinearNoObsPoint(map, fromGridPOINT, toGridPOINT, out Point newNoObsPoint, copySceneID))
            {
                UnityEngine.Vector2 newNoObsPos = new UnityEngine.Vector2((int) newNoObsPoint.X, (int) newNoObsPoint.Y);
                /// Trả ra kết quả
                return KTGlobal.GridPositionToWorldPosition(map, newNoObsPos);
            }
            /// Trả ra kết quả nếu không tìm thấy vị trí thỏa mãn
            return fromPos;
        }
        #endregion

        #region Vị trí ngẫu nhiên
        /// <summary>
        /// Trả về 1 điểm ngẫu nhiên ở 4 hướng xung quanh không có vật cản (tọa độ lưới)
        /// </summary>
        /// <param name="gridPoint"></param>
        /// <param name="mapCode"></param>
        /// <param name="copySceneID"></param>
        /// <param name="allowSpawnAtSafeArea"></param>
        /// <returns></returns>
        public static Point GetRandomNoObsPointAroundGridPos(Point gridPoint, int mapCode, int copySceneID, bool allowSpawnAtSafeArea = true)
        {
            /// Bản đồ tương ứng
            GameMap gameMap = KTMapManager.Find(mapCode);
            /// Toác
            if (gameMap == null)
            {
                /// Toác
                return gridPoint;
            }

            /// Tọa độ lưới
            int gridX = (int) gridPoint.X;
            int gridY = (int) gridPoint.Y;

            /// Nếu không thể đi vào
            if (gameMap.MyNodeGrid.CanEnter(gridX, gridY, copySceneID))
            {
                /// Toác
                return gridPoint;
            }

            /// Vị trí
            Point p = gridPoint;
            int maxGridX = gameMap.MapGridColsNum - 1;
            int maxGridY = gameMap.MapGridRowsNum - 1;
            int added = 1, newX1 = 0, newY1 = 0, newX2 = 0, newY2 = 0;
            /// Thực hiện lặp các vị trí xung quanh để tìm
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
                    if (gameMap.MyNodeGrid.CanEnter(newX1, newY1, copySceneID) && KTGlobal.HasPath(mapCode, gridPoint, new Point(newX1, newY1)) && (allowSpawnAtSafeArea || !gameMap.MyNodeGrid.InSafeArea(newX1, newY1)))
                    {
                        p = new Point(newX1, newY1);
                        break;
                    }
                }

                if ((0 <= newX1 && newX1 < maxGridX) && (0 <= newY2 && newY2 < maxGridY))
                {
                    total--;
                    if (gameMap.MyNodeGrid.CanEnter(newX1, newY2, copySceneID) && KTGlobal.HasPath(mapCode, gridPoint, new Point(newX1, newY2)) && (allowSpawnAtSafeArea || !gameMap.MyNodeGrid.InSafeArea(newX1, newY2)))
                    {
                        p = new Point(newX1, newY2);
                        break;
                    }
                }

                if ((0 <= newX2 && newX2 < maxGridX) && (0 <= newY1 && newY1 < maxGridY))
                {
                    total--;
                    if (gameMap.MyNodeGrid.CanEnter(newX2, newY1, copySceneID) && KTGlobal.HasPath(mapCode, gridPoint, new Point(newX2, newY1)) && (allowSpawnAtSafeArea || !gameMap.MyNodeGrid.InSafeArea(newX2, newY1)))
                    {
                        p = new Point(newX2, newY1);
                        break;
                    }
                }

                if ((0 <= newX2 && newX2 < maxGridX) && (0 <= newY2 && newY2 < maxGridY))
                {
                    total--;
                    if (gameMap.MyNodeGrid.CanEnter(newX2, newY2, copySceneID) && KTGlobal.HasPath(mapCode, gridPoint, new Point(newX2, newY2)) && (allowSpawnAtSafeArea || !gameMap.MyNodeGrid.InSafeArea(newX2, newY2)))
                    {
                        p = new Point(newX2, newY2);
                        break;
                    }
                }

                if ((0 <= newX1 && newX1 < maxGridX))
                {
                    total--;
                    if (gameMap.MyNodeGrid.CanEnter(newX1, gridY, copySceneID) && KTGlobal.HasPath(mapCode, gridPoint, new Point(newX1, gridY)) && (allowSpawnAtSafeArea || !gameMap.MyNodeGrid.InSafeArea(newX1, gridY)))
                    {
                        p = new Point(newX1, gridY);
                        break;
                    }
                }

                if ((0 <= newY1 && newY1 < maxGridY))
                {
                    total--;
                    if (gameMap.MyNodeGrid.CanEnter(gridX, newY1, copySceneID) && KTGlobal.HasPath(mapCode, gridPoint, new Point(gridX, newY1)) && (allowSpawnAtSafeArea || !gameMap.MyNodeGrid.InSafeArea(gridX, newY1)))
                    {
                        p = new Point(gridX, newY1);
                        break;
                    }
                }

                if ((0 <= newX2 && newX2 < maxGridX))
                {
                    total--;
                    if (gameMap.MyNodeGrid.CanEnter(newX2, gridY, copySceneID) && KTGlobal.HasPath(mapCode, gridPoint, new Point(newX2, gridY)) && (allowSpawnAtSafeArea || !gameMap.MyNodeGrid.InSafeArea(newX2, gridY)))
                    {
                        p = new Point(newX2, gridY);
                        break;
                    }
                }

                if ((0 <= newY2 && newY2 < maxGridY))
                {
                    total--;
                    if (gameMap.MyNodeGrid.CanEnter(gridX, newY2, copySceneID) && KTGlobal.HasPath(mapCode, gridPoint, new Point(gridX, newY2)) && (allowSpawnAtSafeArea || !gameMap.MyNodeGrid.InSafeArea(gridX, newY2)))
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

        /// <summary>
        /// Trả về vị trí ngẫu nhiên quanh ô tương ứng
        /// </summary>
        /// <param name="mapCode"></param>
        /// <param name="gridX"></param>
        /// <param name="gridY"></param>
        /// <param name="radiusNum"></param>
        /// <param name="copySceneID"></param>
        /// <param name="allowSpawnAtSafeArea"></param>
        /// <returns></returns>
        private static Point GetAGridRandomPosIn4Direction(int mapCode, int gridX, int gridY, int radiusNum, int copySceneID, bool allowSpawnAtSafeArea = true)
        {
            Point p = new Point(gridX, gridY);

            GameMap gameMap = KTMapManager.Find(mapCode);
            if (gameMap == null)
            {
                return new Point(p.X * gameMap.MapGridWidth + gameMap.MapGridWidth / 2, p.Y * gameMap.MapGridHeight + gameMap.MapGridHeight / 2);
            }

            int minX = Math.Max(0, gridX - radiusNum);
            int maxX = Math.Min(gameMap.MapGridColsNum - 1, gridX + radiusNum);
            int minY = Math.Max(0, gridY - radiusNum);
            int maxY = Math.Min(gameMap.MapGridRowsNum - 1, gridY + radiusNum);

            Point randPoint = new Point(KTGlobal.GetRandomNumber(minX, maxX), KTGlobal.GetRandomNumber(minY, maxY));
            if (!KTGlobal.InObsByGridXY(mapCode, (int) randPoint.X, (int) randPoint.Y, copySceneID))
            {
                return new Point(randPoint.X * gameMap.MapGridWidth + gameMap.MapGridWidth / 2, randPoint.Y * gameMap.MapGridHeight + gameMap.MapGridHeight / 2);
            }

            Point gridPoint = new Point((int) randPoint.X, (int) randPoint.Y);

            gridPoint = KTGlobal.GetRandomNoObsPointAroundGridPos(gridPoint, mapCode, copySceneID, allowSpawnAtSafeArea);
            if (KTGlobal.InObsByGridXY(mapCode, (int) gridPoint.X, (int) gridPoint.Y, copySceneID))
            {
                return new Point(gridX * gameMap.MapGridWidth + gameMap.MapGridWidth / 2, gridY * gameMap.MapGridHeight + gameMap.MapGridHeight / 2);
            }

            return new Point(gridPoint.X * gameMap.MapGridWidth + gameMap.MapGridWidth / 2, gridPoint.Y * gameMap.MapGridHeight + gameMap.MapGridHeight / 2);
        }

        /// <summary>
        /// Trả về vị trí ngãu nhiên xung quanh không chứa vật cản có thể di chuyển đến được
        /// </summary>
        /// <param name="map"></param>
        /// <param name="pos"></param>
        /// <param name="distance"></param>
        /// <param name="copySceneID"></param>
        /// <returns></returns>
        public static UnityEngine.Vector2 GetRandomAroundNoObsPoint(GameMap map, UnityEngine.Vector2 pos, float distance, int copySceneID, bool allowSpawnAtSafeArea = true)
        {
            /// Tọa độ lưới
            UnityEngine.Vector2 gridPos = KTGlobal.WorldPositionToGridPosition(map, pos);
            /// Thực hiện tìm kiếm
            Point destPos = KTGlobal.GetAGridRandomPosIn4Direction(map.MapCode, (int) gridPos.x, (int) gridPos.y, (int) (distance / map.MapGridHeight), copySceneID, allowSpawnAtSafeArea);
            /// Trả về kết quả
            return new UnityEngine.Vector2((int) destPos.X, (int) destPos.Y);
        }

        /// <summary>
        /// Trả về vị trí ngãu nhiên xung quanh bán kính chỉ định không chứa vật cản có thể di chuyển đến được
        /// </summary>
        /// <param name="map"></param>
        /// <param name="pos"></param>
        /// <param name="distance"></param>
        /// <param name="copySceneID"></param>
        /// <param name="allowSpawnAtSafeArea"></param>
        /// <returns></returns>
        public static Point GetRandomAroundNoObsPoint(GameMap map, Point pos, float distance, int copySceneID, bool allowSpawnAtSafeArea = true)
        {
            /// Tọa độ hiện tại
            Point gridPos = KTGlobal.WorldPositionToGridPosition(map, pos);
            /// Thực hiện tìm kiếm
            Point destPos = KTGlobal.GetAGridRandomPosIn4Direction(map.MapCode, (int) gridPos.X, (int) gridPos.Y, (int) (distance / map.MapGridHeight), copySceneID, allowSpawnAtSafeArea);
            /// Trả về kết quả
            return destPos;
        }

        /// <summary>
        /// Trả về vị trí ngãu nhiên xung quanh không chứa vật cản có thể di chuyển đến được
        /// </summary>
        /// <param name="map"></param>
        /// <param name="pos"></param>
        /// <param name="distance"></param>
        /// <param name="copySceneID"></param>
        /// <returns></returns>
        public static Point GetRandomAroundNoObsPoint(GameMap map, Point pos, int copySceneID, bool allowSpawnAtSafeArea = true)
        {
            /// Tọa độ hiện tại
            Point gridPos = KTGlobal.WorldPositionToGridPosition(map, pos);
            /// Thực hiện tìm kiếm
            Point destPos = KTGlobal.GetRandomNoObsPointAroundGridPos(gridPos, map.MapCode, copySceneID, allowSpawnAtSafeArea);
            /// Trả về kết quả
            return KTGlobal.GridPositionToWorldPosition(map, destPos);
        }

        #endregion


    }
}
