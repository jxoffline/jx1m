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
    public static partial class KTDynamicAreaManager
    {
        /// <summary>
        /// Đối tượng quản lý ID tự tăng có thể tái sử dụng
        /// </summary>
        private static readonly AutoIndexReusablePattern AutoIndexManager = new AutoIndexReusablePattern(100000);

        /// <summary>
        /// Thêm khu vực động vào danh sách quản lý
        /// </summary>
        /// <param name="mapCode">ID bản đồ</param>
        /// <param name="copySceneID">ID phụ bản</param>
        /// <param name="name">Tên khu vực động</param>
        /// <param name="resID">ID Res trong file Monster quy định</param>
        /// <param name="posX">Tọa độ X</param>
        /// <param name="posY">Tọa độ Y</param>
        /// <param name="lifeTime">Thời gian tồn tại đơn vị giây (-1 là vĩnh viễn)</param>
        /// <param name="tick">Thời gian tick đơn vị giây (-1 là vĩnh viễn)</param>
        /// <param name="radius">Bán kính quét</param>
        /// <param name="scriptID">Tọa độ Y</param>
        /// <param name="tag">Tag</param>
        public static KDynamicArea Add(int mapCode, int copySceneID, string name, int resID, int posX, int posY, long lifeTime, int tick, int radius, int scriptID, string tag = "")
        {
            GameMap gameMap = KTMapManager.Find(mapCode);
            KDynamicArea dynArea = new KDynamicArea()
            {
                ID = KTDynamicAreaManager.AutoIndexManager.Take() + (int) ObjectBaseID.DynamicArea,
                Name = name,
                ObjectType = ObjectTypes.OT_DYNAMIC_AREA,
                MapCode = mapCode,
                CurrentCopyMapID = copySceneID,
                CurrentPos = new System.Windows.Point(posX, posY),
                CurrentGrid = new System.Windows.Point(posX / gameMap.MapGridWidth, posY / gameMap.MapGridHeight),
                LifeTime = lifeTime,
                Tick = tick,
                ResID = resID,
                Radius = radius,
                ScriptID = scriptID,
                Tag = tag,
                StartTicks = KTGlobal.GetCurrentTimeMilis(),
            };
            KTDynamicAreaManager.dynamicAreas[dynArea.ID] = dynArea;
            /// Thêm khu vực động vào đối tượng quản lý map
            KTDynamicAreaManager.AddToMap(dynArea);
            return dynArea;
        }

        /// <summary>
        /// Xóa khu vực động khỏi danh sách
        /// </summary>
        /// <param name="areaID">ID khu vực động</param>
        public static void Remove(int areaID)
        {
            if (KTDynamicAreaManager.dynamicAreas.TryGetValue(areaID, out KDynamicArea dynArea))
            {
                dynArea.Clear();
                KTDynamicAreaManager.RemoveFromMap(dynArea);
                KTDynamicAreaManager.dynamicAreas.TryRemove(areaID, out _);
                /// Trả ID lại để tái sử dụng
                KTDynamicAreaManager.AutoIndexManager.Return(dynArea.ID - (int) ObjectBaseID.DynamicArea);
            }
        }

        /// <summary>
        /// Xóa khu vực động khỏi danh sách
        /// </summary>
        /// <param name="dynArea">Khu vực động</param>
        public static void Remove(KDynamicArea dynArea)
        {
            if (dynArea == null)
            {
                return;
            }

            dynArea.Clear();
            KTDynamicAreaManager.RemoveFromMap(dynArea);
            KTDynamicAreaManager.dynamicAreas.TryRemove(dynArea.ID, out _);
            /// Trả ID lại để tái sử dụng
            KTDynamicAreaManager.AutoIndexManager.Return(dynArea.ID - (int) ObjectBaseID.DynamicArea);
        }

        /// <summary>
        /// Thêm khu vực động vào Map
        /// </summary>
        /// <param name="dynArea">Đối tượng khu vực động</param>
        /// <returns></returns>
        private static void AddToMap(KDynamicArea dynArea)
        {
            GameMap gameMap = KTMapManager.Find(dynArea.MapCode);
            /// Thêm khu vực động vào Map
            gameMap.Grid.MoveObject((int) dynArea.CurrentPos.X, (int) (dynArea.CurrentPos.Y), dynArea);
        }

        /// <summary>
        /// Xóa khu vực động tương ứng khỏi bản đồ
        /// </summary>
        /// <param name="dynArea">Đối tượng khu vực động</param>
        /// <returns></returns>
        private static bool RemoveFromMap(KDynamicArea dynArea)
        {
            GameMap gameMap = KTMapManager.Find(dynArea.MapCode);

            if (KTDynamicAreaManager.dynamicAreas.TryGetValue(dynArea.ID, out _))
            {
                /// Xóa khu vực động khỏi Map
                gameMap.Grid.RemoveObject(dynArea);
                KTDynamicAreaManager.NotifyNearClientsToRemoveSelf(dynArea);

                return true;
            }
            return false;
        }

        /// <summary>
        /// Trả về danh sách khu vực động trong bản đồ hoặc phụ bản tương ứng
        /// </summary>
        /// <param name="mapCode"></param>
        /// <param name="copyMapID"></param>
        public static List<KDynamicArea> GetMapDynamicAreas(int mapCode, int copyMapID = -1)
        {
            List<KDynamicArea> results = new List<KDynamicArea>();

            if (mapCode <= 0)
            {
                return results;
            }

            /// Duyệt toàn bộ danh sách NPC
            foreach (KeyValuePair<int, KDynamicArea> item in KTDynamicAreaManager.dynamicAreas)
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
        /// Xóa toàn bộ khu vực động khỏi bản đồ hoặc phụ bản tương ứng
        /// </summary>
        /// <param name="mapCode"></param>
        /// <param name="copyMapID"></param>
        public static void RemoveMapDynamicAreas(int mapCode, int copyMapID = -1)
        {
            if (mapCode <= 0)
            {
                return;
            }

            GameMap gameMap = KTMapManager.Find(mapCode);

            List<int> keysToDel = new List<int>();

            /// Duyệt toàn bộ danh sách NPC
            foreach (KeyValuePair<int, KDynamicArea> item in KTDynamicAreaManager.dynamicAreas)
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
                KTDynamicAreaManager.dynamicAreas.TryRemove(key, out _);
            }
        }

        /// <summary>
        /// Thông báo toàn bộ người chơi xung quanh xóa đối tượng khỏi danh sách hiển thị
        /// </summary>
        /// <param name="dynArea">Đối tượng khu vực động</param>
        private static void NotifyNearClientsToRemoveSelf(KDynamicArea dynArea)
        {
            /// Danh sách người chơi xung quanh
            List<KPlayer> objsList = KTRadarMapManager.GetPlayersAround(dynArea);
            /// Nếu không tìm thấy
            if (null == objsList)
            {
                return;
            }

            /// Duyệt danh sách
            foreach (KPlayer player in objsList)
            {
                string strcmd = string.Format("{0}", dynArea.ID);
                player.SendPacket((int) TCPGameServerCmds.CMD_KT_G2C_DEL_DYNAMICAREA, strcmd);
            }
        }
    }
}
