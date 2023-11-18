using GameServer.KiemThe.Entities;
using GameServer.KiemThe.GameEvents.BaiHuTang;
using GameServer.KiemThe.GameEvents.FactionBattle;
using GameServer.KiemThe.GameEvents.FengHuoLianCheng;
using GameServer.KiemThe.GameEvents.TeamBattle;
using GameServer.KiemThe.Logic;
using GameServer.KiemThe.Logic.Manager;
using GameServer.KiemThe.Logic.Manager.Battle;
using GameServer.KiemThe.Utilities.Algorithms;
using GameServer.Logic;
using GameServer.VLTK.Core.GuildManager;
using GameServer.VLTK.GameEvents.GrowTree;
using HSGameEngine.Tools.AStar;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using static GameServer.Logic.KTMapManager;

namespace GameServer.KiemThe
{
    /// <summary>
    /// Các phương thức và đối tượng toàn cục của Kiếm Thế
    /// </summary>
    public static partial class KTGlobal
    {
        #region Path Finder
        /// <summary>
        /// Tìm danh sách đường đi sử dụng giải thuật A*
        /// </summary>
        /// <param name="startPoint"></param>
        /// <param name="endPoint"></param>
        /// <param name="mapCode"></param>
        /// <param name="copySceneID"></param>
        /// <param name="cellLimits"></param>
        /// <returns></returns>
        private static List<int[]> FindPath(Point startPoint, Point endPoint, int mapCode, int copySceneID, int cellLimits)
        {
            GameMap gameMap = KTMapManager.Find(mapCode);
            if (null == gameMap)
            {
                return null;
            }

            PathFinderFast pathFinderFast = new PathFinderFast(gameMap.MyNodeGrid.GetFixedObstruction(), gameMap.MyNodeGrid.GetDynamicObstruction(), gameMap.MyNodeGrid.GetOpenDynamicObsLabels(copySceneID))
            {
                Formula = HeuristicFormula.Manhattan,
                Diagonals = true,
                HeuristicEstimate = 2,
                ReopenCloseNodes = true,
                SearchLimit = cellLimits,
                Punish = null,
                MaxNum = Math.Max(gameMap.MapGridWidth, gameMap.MapGridHeight),
            };

            startPoint.X = gameMap.CorrectWidthPointToGridPoint((int) startPoint.X) / gameMap.MapGridWidth;
            startPoint.Y = gameMap.CorrectHeightPointToGridPoint((int) startPoint.Y) / gameMap.MapGridHeight;
            endPoint.X = gameMap.CorrectWidthPointToGridPoint((int) endPoint.X) / gameMap.MapGridWidth;
            endPoint.Y = gameMap.CorrectHeightPointToGridPoint((int) endPoint.Y) / gameMap.MapGridHeight;

            pathFinderFast.EnablePunish = false;
            List<PathFinderNode> nodeList = pathFinderFast.FindPath(startPoint, endPoint);
            if (null == nodeList || nodeList.Count <= 0)
            {
                return null;
            }

            List<int[]> path = new List<int[]>();
            for (int i = 0; i < nodeList.Count; i++)
            {
                path.Add(new int[] { nodeList[i].X, nodeList[i].Y });
            }

            return path;
        }

        /// <summary>
        /// Kiểm tra có đường đi giữa 2 nút không
        /// </summary>
        /// <param name="mapCode"></param>
        /// <param name="fromPos"></param>
        /// <param name="toPos"></param>
        /// <returns></returns>
        public static bool HasPath(int mapCode, Point fromPos, Point toPos)
        {
            GameMap gameMap = KTMapManager.Find(mapCode);
            if (null == gameMap)
            {
                return false;
            }

            /// Tọa độ lưới
            UnityEngine.Vector2 fromGridPos = KTGlobal.WorldPositionToGridPosition(gameMap, new UnityEngine.Vector2((int)fromPos.X, (int)fromPos.Y));
            UnityEngine.Vector2 toGridPos = KTGlobal.WorldPositionToGridPosition(gameMap, new UnityEngine.Vector2((int)toPos.X, (int)toPos.Y));
            /// Trả về kết quả
            return gameMap.MyNodeGrid.HasPath(new Point(fromGridPos.x, fromGridPos.y), new Point(toGridPos.x, toGridPos.y));
        }

        /// <summary>
        /// Kiểm tra có đường đi giữa 2 nút không
        /// </summary>
        /// <param name="mapCode"></param>
        /// <param name="fromPos"></param>
        /// <param name="toPos"></param>
        /// <returns></returns>
        public static bool HasPath(int mapCode, UnityEngine.Vector2 fromPos, UnityEngine.Vector2 toPos)
        {
            GameMap gameMap = KTMapManager.Find(mapCode);
            if (null == gameMap)
            {
                return false;
            }

            /// Tọa độ lưới
            UnityEngine.Vector2 fromGridPos = KTGlobal.WorldPositionToGridPosition(gameMap, fromPos);
            UnityEngine.Vector2 toGridPos = KTGlobal.WorldPositionToGridPosition(gameMap, toPos);
            /// Trả về kết quả
            return gameMap.MyNodeGrid.HasPath(new Point(fromGridPos.x, fromGridPos.y), new Point(toGridPos.x, toGridPos.y));
        }

        /// <summary>
        /// Tìm đường sử dụng giải thuật A*
        /// </summary>
        /// <param name="mapCode">ID bản đồ</param>
        /// <param name="fromPos">Tọa độ thực điểm bắt đầu</param>
        /// <param name="toPos">Tọa độ thực điểm kết thúc</param>
        /// <param name="copySceneID">ID phụ bản</param>
        /// <param name="cellLimits">Giới hạn số ô tìm kiếm</param>
        /// <returns></returns>
        private static List<UnityEngine.Vector2> FindPathUsingAStar(int mapCode, Point fromPos, Point toPos, int copySceneID, int cellLimits)
        {
            GameMap gameMap = KTMapManager.Find(mapCode);
            if (null == gameMap)
            {
                return new List<UnityEngine.Vector2>();
            }

            /// Tìm đường sử dụng giải thuật A*
            List<int[]> nodeList = KTGlobal.FindPath(fromPos, toPos, mapCode, copySceneID, cellLimits);
            if (nodeList == null)
            {
                return new List<UnityEngine.Vector2>();
            }
            nodeList.Reverse();

            List<UnityEngine.Vector2> path = new List<UnityEngine.Vector2>();

            for (int i = 0; i < nodeList.Count; i++)
            {
                path.Add(new UnityEngine.Vector2(nodeList[i][0], nodeList[i][1]));
            }

            /// Làm mịn
            path = LineSmoother.SmoothPath(path, gameMap.MyNodeGrid.GetFixedObstruction());

            return path;
        }

        /// <summary>
        /// Tìm đường giữa 2 vị trí
        /// <para>Sử dụng giải thuật A* để tìm đường ngắn nhất</para>
        /// <para>Sử dụng thuật toán Ramer–Douglas–Peucker để làm mịn đường đi</para>
        /// </summary>
        /// <param name="go">Đối tượng</param>
        /// <param name="fromPos">Tọa độ thực vị trí bắt đầu</param>
        /// <param name="toPos">Tọa độ thực vị trí kết thúc</param>
        /// <param name="copySceneID">ID phụ bản</param>
        /// <param name="cellLimits">Giới hạn số ô tìm kiếm</param>
        public static List<UnityEngine.Vector2> FindPath(GameObject go, UnityEngine.Vector2 fromPos, UnityEngine.Vector2 toPos, int copySceneID, int cellLimits = 10000000)
        {
            GameMap gameMap = KTMapManager.Find(go.CurrentMapCode);
            if (null == gameMap)
            {
                return new List<UnityEngine.Vector2>();
            }

            /// Nếu không có đường đi giữa 2 nút
            if (!KTGlobal.HasPath(gameMap.MapCode, fromPos, toPos))
            {
                return new List<UnityEngine.Vector2>();
            }

            UnityEngine.Vector2 fromGridPos = KTGlobal.WorldPositionToGridPosition(gameMap, fromPos);
            UnityEngine.Vector2 toGridPos = KTGlobal.WorldPositionToGridPosition(gameMap, toPos);

            Point fromGridPOINT = new Point((int)fromGridPos.x, (int)fromGridPos.y);
            Point toGridPOINT = new Point((int)toGridPos.x, (int)toGridPos.y);

            /// Nếu vị trí đầu và cuối cùng một ô lưới thì cho chạy giữa 2 vị trí này luôn
            if (fromGridPOINT == toGridPOINT)
            {
                return new List<UnityEngine.Vector2>()
                {
                    fromPos, toPos
                };
            }

            Point fromPosPOINT = new Point((int)fromPos.x, (int)fromPos.y);
            Point toPosPOINT = new Point((int)toPos.x, (int)toPos.y);

            /// Nếu đang ở vị trí có vật cản
            if (!gameMap.CanMove(fromPosPOINT, copySceneID))
            {
                /// Tìm vị trí bắt đầu mới không có vật cản
                Point noObsPos = KTGlobal.GetRandomNoObsPointAroundGridPos(fromGridPOINT, go.CurrentMapCode, copySceneID);
                /// Đánh dấu lại vị trí bắt đầu
                fromGridPOINT = noObsPos;
                fromPos = KTGlobal.GridPositionToWorldPosition(gameMap, new UnityEngine.Vector2((int)fromGridPOINT.X, (int)fromGridPOINT.Y));
                fromPosPOINT = new Point(fromPos.x, fromPos.y);
            }

            /// Nếu vị trí đích đến có vật cản
            if (!gameMap.CanMove(toPosPOINT, copySceneID))
            {
                Point noObsPos;
                /// Tìm vị trí kết thúc mới mà không có vật cản
                if (KTGlobal.FindLinearNoObsPoint(gameMap, fromGridPOINT, toGridPOINT, out noObsPos, copySceneID))
                {
                    /// Đánh dấu lại vị trí kết thúc
                    toGridPOINT = noObsPos;
                    toPos = KTGlobal.GridPositionToWorldPosition(gameMap, new UnityEngine.Vector2((int)toGridPOINT.X, (int)toGridPOINT.Y));
                }
                /// Lỗi gì đó thì vào đây
				else
                {
                    /// Tìm vị trí kết thúc mới không có vật cản
                    noObsPos = KTGlobal.GetRandomNoObsPointAroundGridPos(toGridPOINT, go.CurrentMapCode, copySceneID);
                    /// Đánh dấu lại vị trí kết thúc
                    toGridPOINT = noObsPos;
                    toPos = KTGlobal.GridPositionToWorldPosition(gameMap, new UnityEngine.Vector2((int)toGridPOINT.X, (int)toGridPOINT.Y));
                }
                toPosPOINT = new Point(toPos.x, toPos.y);
            }

            /// Nếu vẫn không có đường đi giữa 2 vị trí
            if (!gameMap.CanMove((int)fromGridPOINT.X, (int)fromGridPOINT.Y, copySceneID) || !gameMap.CanMove((int)toGridPOINT.X, (int)toGridPOINT.Y, copySceneID))
            {
                return new List<UnityEngine.Vector2>();
            }

            /// Sử dụng A* tìm đường đi
            List<UnityEngine.Vector2> nodes = KTGlobal.FindPathUsingAStar(go.CurrentMapCode, fromPosPOINT, toPosPOINT, copySceneID, cellLimits);

            /// Nếu danh sách nút tìm được nhỏ hơn 2
            if (nodes.Count < 2)
            {
                return new List<UnityEngine.Vector2>();
            }

            /// Danh sách điểm trên đường đi
            List<UnityEngine.Vector2> result = new List<UnityEngine.Vector2>();
            result.Add(fromPos);

            /// Thêm tất cả các nút tìm được trên đường đi vào danh sách
            for (int i = 1; i < nodes.Count; i++)
            {
                result.Add(KTGlobal.GridPositionToWorldPosition(gameMap, nodes[i]));
            }
            result[result.Count - 1] = toPos;

            return result;
        }

        #endregion Path Finder

        #region Tìm đường đi

        /// <summary>
        /// Tìm đường đi từ vị trí fromPos trong bản đồ fromMapCode đến vị trí toPos trong bản đồ toMapCode chỉ sử dụng cổng dịch chuyển
        /// </summary>
        /// <param name="fromMapCode"></param>
        /// <param name="fromPos"></param>
        /// <param name="toMapCode"></param>
        /// <param name=""></param>
        /// <returns></returns>
        public static List<KeyValuePair<int, UnityEngine.Vector2Int>> FindPaths(int fromMapCode, UnityEngine.Vector2Int fromPos, int toMapCode, UnityEngine.Vector2Int toPos)
        {
            /// Tạo mới danh sách nút
            List<KeyValuePair<int, UnityEngine.Vector2Int>> paths = new List<KeyValuePair<int, UnityEngine.Vector2Int>>();
            /// Thêm điểm đầu vào
            paths.Add(new KeyValuePair<int, UnityEngine.Vector2Int>(fromMapCode, fromPos));

            /// Nếu khác bản đồ
            if (fromMapCode != toMapCode)
            {
                /// Tìm danh sách bản đồ cần đi qua
                List<int> movePaths = KTAutoPathManager.Instance.FindPathUsingTeleportOnly(fromMapCode, toMapCode);
                /// Nếu không tồn tại
                if (movePaths == null || movePaths.Count <= 0)
                {
                    return new List<KeyValuePair<int, UnityEngine.Vector2Int>>();
                }

                /// ID bản đồ trước đó
                int lastMapID = fromMapCode;
                /// Vị trí trước đó
                UnityEngine.Vector2Int lastPos = fromPos;
                /// Duyệt danh sách bản đồ sẽ đi qua
                foreach (int mapID in movePaths)
                {
                    /// Nếu trùng với bản đồ trước đó
                    if (lastMapID == mapID)
                    {
                        /// Bỏ qua
                        continue;
                    }

                    /// Danh sách đường đi thỏa mãn từ vị trí hiện tại
                    List<AutoPathXML.Node> edges = KTAutoPathManager.Instance.TeleportEdges[lastMapID].Where(x => x.ToMapCode == mapID).ToList();
                    /// Nếu không tìm thấy đường đi
                    if (edges == null)
                    {
                        /// Toác
                        return new List<KeyValuePair<int, UnityEngine.Vector2Int>>();
                    }

                    /// Lấy đường đi có trọng số ngắn nhất
                    int minWeight = edges.Min(x => x.Weight);
                    /// Duyệt danh sách cạnh có trọng số nhỏ nhất, lấy vị trí có khoảng cách so với hiện tại ngắn nhất
                    AutoPathXML.Node edge = edges.Where(x => x.Weight == minWeight).MinBy(x => UnityEngine.Vector2Int.Distance(new UnityEngine.Vector2Int(x.PosX, x.PosY), lastPos));

                    /// Lưu lại
                    lastMapID = mapID;
                    lastPos = new UnityEngine.Vector2Int(edge.ToX, edge.ToY);

                    /// Thêm vào danh sách
                    paths.Add(new KeyValuePair<int, UnityEngine.Vector2Int>(edge.FromMapCode, new UnityEngine.Vector2Int(edge.PosX, edge.PosY)));
                    paths.Add(new KeyValuePair<int, UnityEngine.Vector2Int>(edge.ToMapCode, new UnityEngine.Vector2Int(edge.ToX, edge.ToY)));
                }
            }

            /// Thêm điểm cuối vào
            paths.Add(new KeyValuePair<int, UnityEngine.Vector2Int>(toMapCode, toPos));

            /// Trả về kết quả
            return paths;
        }

        #endregion Tìm đường đi

        #region Liên Server

        /// <summary>
        /// Chuyển người chơi đến bản đồ tương ứng ở Liên máy chủ
        /// </summary>
        /// <param name="client"></param>
        /// <param name="toMapCode"></param>
        /// <returns></returns>
        public static bool GotoCrossServer(KPlayer client, int toMapCode)
        {
            GameMap gameMap = KTMapManager.Find(toMapCode);

            if (gameMap == null)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("Bản đồ không tồn tại, mapCode={0}", toMapCode));
                return false;
            }

            // nếu client đang ở máy chủ liên sv
            if (client.ClientSocket.IsKuaFuLogin)
            {
                KuaFuManager.getInstance().GotoLastMap(client);

                return true;
            }
            else if (KuaFuManager.getInstance().IsKuaFuMap(toMapCode))
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("GotoMap denied, mapCode={0},IsKuaFuLogin={1}", toMapCode, client.ClientSocket.IsKuaFuLogin));
                return false;
            }

            int toMapX = -1;
            int toMapY = -1;
            int toDirection = KTGlobal.GetRandomNumber(0, 8);

            // NOTIFY Về cho nó đổi bản đồ
            KTPlayerManager.ChangeMap(client, toMapCode, toMapX, toMapY);

            return true;
        }

        #endregion Liên Server

        #region Khu vực được phép thách đấu

        /// <summary>
        /// Kiểm tra người chơi có đứng trong khu vực được thách đấu không
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static bool IsInsideChallengeArea(KPlayer player)
        {
            /// Nếu đang trong liên máy chủ thì không cho phép
            if (player.ClientSocket.IsKuaFuLogin)
            {
                return false;
            }
            /// Nếu đang trong phụ bản thì không được thách đấu
            if (player.CopyMapID != -1)
            {
                return false;
            }

            /// Nếu đang ở Bạch Hổ Đường
            if (BaiHuTang.IsInBaiHuTang(player))
            {
                return false;
            }
            /// Nếu đang ở Võ Lâm Liên Đấu
            else if (TeamBattle.IsInTeamBattleMap(player) || TeamBattle.IsInTeamBattlePKMap(player))
            {
                return false;
            }
            /// Nếu đang ở Phong Hỏa Liên Thành
            else if (FengHuoLianCheng.IsInsideFHLCMap(player))
            {
                return false;
            }
            /// Nếu đang ở Tống Kim
            else if (Battel_SonJin_Manager.IsInBattle(player))
            {
                return false;
            }
            /// Nếu đang ở hoạt động trồng cây
            else if (GrowTreeManager.IsInEvent(player))
            {
                return false;
            }
            // Nếu dang ở sự kiện công thành chiến
            else if (GuildWarCity.IsInGuildWarCity(player))
            {
                return false;
            }
            /// Nếu đang ở thi đấu môn phái
            else if (FactionBattleManager.IsInFactionBattle(player))
            {
                return false;
            }

            /// OK
            return true;
        }

        #endregion Khu vực được phép thách đấu

        #region Obs
        /// <summary>
        /// Kiểm tra vị trí có hoàn toàn nằm trong điểm Block không
        /// </summary>
        /// <param name="mapCode"></param>
        /// <param name="gridX"></param>
        /// <param name="gridY"></param>
        /// <returns></returns>
        public static bool InOnlyObs(int mapCode, int gridX, int gridY, int copySceneID)
        {
            GameMap gameMap = KTMapManager.Find(mapCode);
            if (gridX >= gameMap.MapGridColsNum || gridX < 0 || gridY >= gameMap.MapGridRowsNum || gridY < 0)
            {
                return true;
            }

            if (!gameMap.MyNodeGrid.CanEnter(gridX, gridY, copySceneID))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Kiểm tra vị trí đích đến (tọa độ thực) có nằm trong điểm Block không
        /// </summary>
        /// <param name="mapCode"></param>
        /// <param name="toX"></param>
        /// <param name="toY"></param>
        /// <returns></returns>
        public static bool InObs(int mapCode, int toX, int toY, int copySceneID)
        {
            GameMap gameMap = KTMapManager.Find(mapCode);
            if (toX >= gameMap.MapWidth || toX < 0 || toY >= gameMap.MapHeight || toY < 0)
            {
                return true;
            }

            return KTGlobal.InObsByGridXY(mapCode, (int) (toX / gameMap.MapGridWidth), (int) (toY / gameMap.MapGridHeight), copySceneID);
        }

        /// <summary>
        /// Kiểm tra vị trí đích đến (tọa độ lưới) có nằm trong điểm Block không
        /// </summary>
        /// <param name="mapCode"></param>
        /// <param name="toX"></param>
        /// <param name="toY"></param>
        /// <returns></returns>
        public static bool InObsByGridXY(int mapCode, int gridX, int gridY, int copySceneID)
        {
            GameMap gameMap = KTMapManager.Find(mapCode);
            if (gridX >= gameMap.MapGridColsNum || gridX < 0 || gridY >= gameMap.MapGridRowsNum || gridY < 0)
            {
                return true;
            }

            if (!gameMap.MyNodeGrid.CanEnter(gridX, gridY, copySceneID))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Kiểm tra vị trí có nằm trong khu Obs động không
        /// </summary>
        /// <param name="mapCode"></param>
        /// <param name="posX"></param>
        /// <param name="posY"></param>
        /// <param name="copySceneID"></param>
        /// <returns></returns>
        public static bool InDynamicObs(int mapCode, int posX, int posY, int copySceneID)
        {
            GameMap gameMap = KTMapManager.Find(mapCode);
            posX /= gameMap.MapGridWidth;
            posY /= gameMap.MapGridHeight;
            return gameMap.MyNodeGrid.InDynamicObs(posX, posY, copySceneID);
        }

        /// <summary>
        /// Kiểm tra vị trí đích có đến được không
        /// </summary>
        public static bool IsGridReachable(int mapCode, int gridX, int gridY, int copySceneID)
        {
            bool nCanMove = false;
            GameMap gameMap = KTMapManager.Find(mapCode);
            if (null != gameMap)
            {
                nCanMove = nCanMove | (gameMap.CanMove(gridX, gridY, copySceneID) ? true : false);
                nCanMove = nCanMove | (gameMap.CanMove(gridX, gridY + 1, copySceneID) ? true : false);
                nCanMove = nCanMove | (gameMap.CanMove(gridX, gridY - 1, copySceneID) ? true : false);
                nCanMove = nCanMove | (gameMap.CanMove(gridX + 1, gridY, copySceneID) ? true : false);
                nCanMove = nCanMove | (gameMap.CanMove(gridX - 1, gridY, copySceneID) ? true : false);
            }

            return nCanMove;
        }
        #endregion
    }
}