using GameServer.Interface;
using GameServer.KiemThe.Core;
using GameServer.KiemThe.Entities;
using GameServer.KiemThe.Utilities.Algorithms;
using GameServer.Logic;
using GameServer.Server;
using Server.Data;
using Server.Protocol;
using Server.TCP;
using System;
using System.Collections.Generic;
using System.Linq;
using static GameServer.Logic.KTMapManager;

namespace GameServer.KiemThe.Logic.Manager
{
    /// <summary>
    /// Quản lý thêm sửa xóa
    /// </summary>
    public static partial class KTGrowPointManager
    {
        /// <summary>
        /// Đối tượng quản lý ID tự tăng có thể tái sử dụng
        /// </summary>
        public static AutoIndexReusablePattern AutoIndexManager = new AutoIndexReusablePattern(100000);

        /// <summary>
        /// Thêm điểm thu thập vào danh sách quản lý
        /// </summary>
        /// <param name="mapCode">ID bản đồ</param>
        /// <param name="copySceneID">ID phụ bản</param>
        /// <param name="data">Dữ liệu điểm thu thập</param>
        /// <param name="posX">Tọa độ X</param>
        /// <param name="posY">Tọa độ Y</param>
        /// <param name="lifeTime">Thời gian tồn tại (-1 nếu tồn tại vĩnh viễn)</param>
        public static GrowPoint Add(int mapCode, int copySceneID, GrowPointXML data, int posX, int posY, int lifeTime = -1)
        {
            GameMap gameMap = KTMapManager.Find(mapCode);
            GrowPoint growPoint = new GrowPoint()
            {
                ID = KTGrowPointManager.AutoIndexManager.Take() + (int) ObjectBaseID.GrowPoint,
                Data = data,
                Name = data.Name,
                ObjectType = ObjectTypes.OT_GROWPOINT,
                MapCode = mapCode,
                CurrentCopyMapID = copySceneID,
                CurrentPos = new System.Windows.Point(posX, posY),
                CurrentGrid = new System.Windows.Point(posX / gameMap.MapGridWidth, posY / gameMap.MapGridHeight),
                RespawnTime = data.RespawnTime,
                ScriptID = data.ScriptID,
                Alive = true,
                LifeTime = lifeTime,
            };
            /// Thực hiện tự động xóa
            growPoint.ProcessAutoRemoveTimeout();
            KTGrowPointManager.GrowPoints[growPoint.ID] = growPoint;
            /// Thêm điểm thu thập vào đối tượng quản lý map
            KTGrowPointManager.AddToMap(growPoint);

            /// Trả về đối tượng
            return growPoint;
        }

        /// <summary>
        /// Xóa điểm thu thập khỏi danh sách
        /// </summary>
        /// <param name="growPoint">Điểm thu thập</param>
        public static void Remove(GrowPoint growPoint)
        {
            if (growPoint == null)
            {
                return;
            }

            /// Xóa khỏi Timer
            KTGrowPointTimerManager.Instance.Remove(growPoint);
            /// Xóa khỏi bản đồ
            KTGrowPointManager.RemoveFromMap(growPoint);
            /// Xóa khỏi danh sách quản lý
            KTGrowPointManager.GrowPoints.TryRemove(growPoint.ID, out _);
            /// Trả ID lại để tái sử dụng
            KTGrowPointManager.AutoIndexManager.Return(growPoint.ID - (int) ObjectBaseID.GrowPoint);
        }

        /// <summary>
        /// Thêm điểm thu thập vào Map
        /// </summary>
        /// <param name="growPoint">Đối tượng điểm thu thập</param>
        /// <returns></returns>
        public static void AddToMap(GrowPoint growPoint)
        {
            GameMap gameMap = KTMapManager.Find(growPoint.MapCode);
            /// Thêm điểm thu thập vào Map
            gameMap.Grid.MoveObject((int) growPoint.CurrentPos.X, (int) (growPoint.CurrentPos.Y), growPoint);
            KTGrowPointManager.NotifyNearClientsToAddSelf(growPoint);
        }

        /// <summary>
        /// Xóa điểm thu thập tương ứng khỏi bản đồ
        /// </summary>
        /// <param name="growPoint">Đối tượng điểm thu thập</param>
        /// <returns></returns>
        private static void RemoveFromMap(GrowPoint growPoint)
        {
            GameMap gameMap = KTMapManager.Find(growPoint.MapCode);

            if (KTGrowPointManager.GrowPoints.TryGetValue(growPoint.ID, out _))
            {
                /// Xóa điểm thu thập khỏi Map
                gameMap.Grid.RemoveObject(growPoint);
                KTGrowPointManager.NotifyNearClientsToRemoveSelf(growPoint);
            }
        }

        /// <summary>
        /// Xóa toàn bộ khu vực động khỏi bản đồ hoặc phụ bản tương ứng
        /// </summary>
        /// <param name="mapCode"></param>
        /// <param name="copyMapID"></param>
        public static void RemoveMapGrowPoints(int mapCode, int copyMapID = -1)
        {
            if (mapCode <= 0)
            {
                return;
            }

            GameMap gameMap = KTMapManager.Find(mapCode);

            List<int> keysToDel = new List<int>();

            /// Duyệt toàn bộ danh sách NPC
            foreach (KeyValuePair<int, GrowPoint> item in KTGrowPointManager.GrowPoints)
            {
                if (item.Value.MapCode == mapCode && (copyMapID == -1 || item.Value.CurrentCopyMapID == copyMapID))
                {
                    gameMap.Grid.RemoveObject(item.Value);
                    keysToDel.Add(item.Key);
                }
            }

            /// Xóa các bản ghi đã tìm bên trên
            foreach (int key in keysToDel)
            {
                KTGrowPointManager.GrowPoints.TryRemove(key, out _);
            }
        }

        /// <summary>
        /// Trả về danh sách điểm thu thập trong bản đồ hoặc phụ bản tương ứng
        /// </summary>
        /// <param name="mapCode"></param>
        /// <param name="copyMapID"></param>
        public static List<GrowPoint> GetMapGrowPoints(int mapCode, int copyMapID = -1)
        {
            List<GrowPoint> results = new List<GrowPoint>();

            if (mapCode <= 0)
            {
                return results;
            }

            /// Duyệt toàn bộ danh sách NPC
            foreach (KeyValuePair<int, GrowPoint> item in KTGrowPointManager.GrowPoints)
            {
                if (item.Value.MapCode == mapCode && (copyMapID == -1 || item.Value.CurrentCopyMapID == copyMapID))
                {
                    results.Add(item.Value);
                }
            }

            /// Trả về kết quả
            return results;
        }

        /// <summary>
        /// Thông báo toàn bộ người chơi xung quanh thêm đối tượng vào danh sách hiển thị
        /// </summary>
        /// <param name="growPoint">Đối tượng điểm thu thập</param>
        private static void NotifyNearClientsToAddSelf(GrowPoint growPoint)
        {
            List<KPlayer> objsList = KTRadarMapManager.GetPlayersAround(growPoint);
            /// Không tìm thấy
            if (null == objsList)
            {
                return;
            }

            /// Duyệt danh sách
            for (int i = 0; i < objsList.Count; i++)
            {
                KPlayer client = objsList[i];

                if (!growPoint.Alive)
                {
                    continue;
                }

                GrowPointObject growPointObject = new GrowPointObject()
                {
                    ID = growPoint.ID,
                    Name = growPoint.Data.Name,
                    ResID = growPoint.Data.ResID,
                    PosX = (int) growPoint.CurrentPos.X,
                    PosY = (int) growPoint.CurrentPos.Y,
                };
                client.SendPacket<GrowPointObject>((int) TCPGameServerCmds.CMD_KT_G2C_NEW_GROWPOINT, growPointObject);
            }
        }

        /// <summary>
        /// Thông báo toàn bộ người chơi xung quanh xóa đối tượng khỏi danh sách hiển thị
        /// </summary>
        /// <param name="growPoint">Đối tượng điểm thu thập</param>
        public static void NotifyNearClientsToRemoveSelf(GrowPoint growPoint)
        {
            List<KPlayer> objsList = KTRadarMapManager.GetPlayersAround(growPoint);
            /// Không tìm thấy
            if (null == objsList)
            {
                return;
            }

            /// Duyệt danh sách
            foreach (KPlayer player in objsList)
            {
                string strcmd = string.Format("{0}", growPoint.ID);
                player.SendPacket((int) TCPGameServerCmds.CMD_KT_G2C_DEL_GROWPOINT, strcmd);
            }
        }
    }
}
