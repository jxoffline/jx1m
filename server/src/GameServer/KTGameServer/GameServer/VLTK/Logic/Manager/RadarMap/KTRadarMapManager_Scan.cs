using GameServer.Interface;
using GameServer.KiemThe;
using System;
using System.Collections.Generic;
using System.Linq;
using static GameServer.Logic.KTMapManager;

namespace GameServer.Logic
{
    /// <summary>
    /// Quản lý tầm nhìn của người chơi
    /// </summary>
    public static partial class KTRadarMapManager
    {
        #region API
        /// <summary>
        /// Trả về danh sách người chơi xung quanh đối tượng theo tầm nhìn
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static List<KPlayer> GetPlayersAround(IObject obj)
        {
            /// Toác
            if (obj == null)
            {
                return null;
            }

            /// Trả về kết quả
            return KTRadarMapManager.ScanObjects<KPlayer>(obj.CurrentMapCode, obj.CurrentCopyMapID, (int) obj.CurrentPos.X, (int) obj.CurrentPos.Y);
            
        }

        /// <summary>
        /// Trả về danh sách các đối tượng xung quanh đối tượng theo tầm nhìn
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static List<IObject> GetObjectsAround(IObject obj)
        {
            /// Toác
            if (obj == null)
            {
                return null;
            }
            /// Trả về kết quả
            return KTRadarMapManager.ScanObjects<IObject>(obj.CurrentMapCode, obj.CurrentCopyMapID, (int) obj.CurrentPos.X, (int) obj.CurrentPos.Y);
        }

        /// <summary>
        /// Trả về danh sách các đối tượng xung quanh vị trí tương ứng theo tầm nhìn
        /// </summary>
        /// <param name="mapCode"></param>
        /// <param name="copyMapID"></param>
        /// <param name="posX"></param>
        /// <param name="posY"></param>
        /// <returns></returns>
        public static List<IObject> GetObjectsAround(int mapCode, int copyMapID, int posX, int posY)
        {
            /// Trả về kết quả
            return KTRadarMapManager.ScanObjects<IObject>(mapCode, copyMapID, posX, posY);
        }

        /// <summary>
        /// Trả về danh sách người chơi xung quanh vị trí tương ứng theo tầm nhìn
        /// </summary>
        /// <param name="mapCode"></param>
        /// <param name="copyMapID"></param>
        /// <param name="posX"></param>
        /// <param name="posY"></param>
        /// <returns></returns>
        public static List<KPlayer> GetPlayersAround(int mapCode, int copyMapID, int posX, int posY)
        {
            /// Trả về kết quả
            return KTRadarMapManager.ScanObjects<KPlayer>(mapCode, copyMapID, posX, posY);
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Tìm các đối tượng xung quanh vị trí tương ứng
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="mapCode"></param>
        /// <param name="copyMapID"></param>
        /// <param name="posX"></param>
        /// <param name="posY"></param>
        /// <returns></returns>
        private static List<T> ScanObjects<T>(int mapCode, int copyMapID, int posX, int posY) where T : IObject
        {
            /// Bản đồ
            GameMap gameMap = KTMapManager.Find(mapCode);

            /// Tọa độ lưới tương ứng
            int centerCridX = posX / gameMap.Grid.MapGridWidth;
            int centerCridY = posY / gameMap.Grid.MapGridHeight;

            /// Danh sách đối tượng
            List<T> objsList = new List<T>();

            /// Duyệt theo lưới chiều ngang
            for (int l = -KTRadarMapManager.RadarHalfWidth; l < KTRadarMapManager.RadarHalfWidth; l++)
            {
                /// Duyệt theo lưới chiều dọc
                for (int j = -KTRadarMapManager.RadarHalfHeight; j < KTRadarMapManager.RadarHalfHeight; j++)
                {
                    /// Tọa độ lưới tương ứng
                    int x = l + centerCridX;
                    int y = j + centerCridY;

                    /// Nếu nằm trong phạm vi
                    if (x >= 0 && y >= 0 && x < gameMap.Grid.MapGridXNum && y < gameMap.Grid.MapGridYNum)
                    {
                        /// Tìm các đối tượng trong ô tương ứng
                        List<IObject> tempObjsList = gameMap.Grid.FindObjects(x, y);
                        /// Không tìm thấy
                        if (tempObjsList == null)
                        {
                            /// Bỏ qua
                            continue;
                        }

                        /// Duyệt danh sách các đối tượng tìm thấy trong ô
                        for (int i = 0; i < tempObjsList.Count; i++)
                        {
                            /// Đối tượng tương ứng
                            if (!(tempObjsList[i] is T targetObj))
                            {
                                /// Nếu không phải loại cần tìm kiếm thì bỏ qua
                                continue;
                            }

                            /// Nếu không nằm trong cùng phụ bản
                            if (targetObj.CurrentCopyMapID != copyMapID)
                            {
                                /// Bỏ qua
                                continue;
                            }

                            /// Thêm vào danh sách tìm kiếm
                            objsList.Add(targetObj);
                        }
                    }
                }
            }

            /// Nếu không có kết quả
            if (objsList.Count <= 0)
            {
                /// Cho NULL
                objsList = null;
            }

            /// Trả về kết quả
            return objsList;
        }
        #endregion
    }
}
