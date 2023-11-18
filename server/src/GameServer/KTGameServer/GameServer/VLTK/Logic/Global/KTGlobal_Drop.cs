using GameServer.Interface;
using GameServer.Logic;
using System;
using System.Collections.Generic;
using static GameServer.Logic.KTMapManager;

namespace GameServer.KiemThe
{
    /// <summary>
    /// Các phương thức và đối tượng toàn cục của Kiếm Thế
    /// </summary>
    public static partial class KTGlobal
    {
        #region Tỷ lệ rơi đồ
        /// <summary>
        /// Tỉ lệ rơi ở quái thường mỗi đơn vị là 1 % MAX là 100%
        /// </summary>
        public const int DropRate = 5;
        #endregion

        #region Vật phẩm rơi
        /// <summary>
        /// Đánh dấu mở chức năng rơi vật phẩm tại các vị trí xung quanh không trùng nhau
        /// </summary>
        private const bool EnableGoodsPackFallPositionCheck = true;

        /// <summary>
        /// Danh sách các ô xung quanh vị trí sẽ rơi ra vật phẩm để không đè lên nhau
        /// </summary>
        private static readonly short[] fallGoodsPositions = new short[] {
            0,0,1,0,0,1,0,-1,-1,0,-1,1,1,-1,0,-2,2,0,0,2,-1,-1,1,1,-2,0,-2,1,-2,-1,-1,-2,0,-3,-3,0,1,-2,1,2,
            -1,2,0,3,2,-1,3,0,2,1,-4,0,0,-4,-3,-1,-1,-3,-1,3,-2,-2,3,-1,1,3,2,2,0,4,-3,1,1,-3,-2,2,2,-2,4,0,
            3,1,-5,0,2,-3,3,2,-3,-2,4,1,-4,-1,-4,1,1,-4,5,0,2,3,1,4,-3,2,-1,4,3,-2,-1,-4,-2,-3,-2,3,0,5,4,-1,
            0,-5,-6,0,4,-2,2,-4,-4,2,-3,-3,-4,-2,-2,-4,5,-1,-3,3,-1,5,3,-3,-5,-1,-2,4,0,6,1,5,-5,1,4,2,5,1,6,0,
            1,-5,-1,-5,0,-6,2,4,3,3,-7,0,-6,1,-2,-5,0,7,-1,-6,4,-3,-5,-2,-3,4,-6,-1,-2,5,1,-6,5,-2,-4,-3,-3,-4,2,-5,
            7,0,4,3,3,4,-5,2,2,5,1,6,-1,6,6,1,0,-7,6,-1,3,-4,-4,3,5,2,2,6,7,1,-1,7,5,-3,0,8,-2,6,7,-1,
            8,0,3,5,4,4,6,-2,6,2,1,7,5,3,2,-6,-1,-7,1,-7,-6,2,-4,4,4,-4,-5,3,3,-5,0,-8,-4,-4,-2,-6,-3,-5,-7,-1,
            -7,1,-5,-3,-6,-2,-3,5,-8,0,-1,8,8,-1,3,-6,2,7,6,-3,1,8,7,-2,-6,3,5,4,-7,2,6,3,5,-4,2,-7,3,6,-4,5,
            7,2,4,-5,-5,4,-3,6,8,1,0,9,0,-9,-2,7,4,5,9,0,1,-8,9,-1,-1,9,0,10,1,-9,-4,6,6,-4,2,-8,9,1,8,2,
            -5,5,4,-6,4,6,-6,4,-3,7,7,3,7,-3,5,5,6,4,-2,8,8,-2,10,0,5,-5,3,-7,3,7,2,8,1,9,0,11,11,0,1,10,
            6,5,5,6,4,7,8,3,9,2,2,9,10,-1,5,-6,9,-2,-4,7,3,-8,-2,9,8,-3,-3,8,4,-7,1,-10,6,-5,-1,10,2,-9,7,-4,
            -5,6,12,0,7,-5,8,-4,-4,8,6,-6,5,-7,0,12,4,-8,-2,10,3,-9,11,-1,-1,11,2,-10,9,-3,-3,9,10,-2,-2,11,8,-5,11,-2,
            2,-11,7,-6,12,-1,4,-9,5,-8,-1,12,9,-4,6,-7,-3,10,3,-10,10,-3,-2,12,-11,2,-11,3,-10,3,-9,4,-9,5,-8,5,-8,6,-7,6,
            -7,7,-6,7,-5,8,-5,9,-4,9,-4,10,-10,2,-9,3,-8,4,-7,5,-6,6,-5,7,-9,2,-8,3,-7,4,-6,5,-10,1,-9,1,-8,2,-7,3,
            -8,1,-9,0,
        };

        /// <summary>
        /// Tìm vật phẩm ở vị trí tương ứng (tọa độ lưới) trong bản đồ
        /// </summary>
        /// <param name="mapCode"></param>
        /// <param name="copyMapID"></param>
        /// <param name="gridPosX"></param>
        /// <param name="gridPosY"></param>
        /// <returns></returns>
        private static KGoodsPack FindGoodsPackAtPosition(int mapCode, int copyMapID, int gridPosX, int gridPosY)
        {
            /// Bản đồ tương ứng
            GameMap gameMap = KTMapManager.Find(mapCode);
            /// Toác
            if (gameMap == null)
            {
                return null;
            }

            /// Danh 
            List<IObject> objsList = gameMap.Grid.FindObjects(gridPosX, gridPosY);
            /// Nếu tìm thấy
            if (objsList != null)
            {
                /// Duyệt danh sách các đối tượng
                for (int objIndex = 0; objIndex < objsList.Count; objIndex++)
                {
                    /// Nếu là vật phẩm rơi
                    if (objsList[objIndex] is KGoodsPack goodsPack)
                    {
                        /// Nếu tồn tại phụ bản
                        if (copyMapID != -1 && goodsPack.CurrentCopyMapID == copyMapID)
                        {
                            /// Trả về kết quả
                            return goodsPack;
                        }
                        /// Nếu không tồn tại phụ bản
                        else
                        {
                            /// Trả về kết quả
                            return goodsPack;
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Tìm vị trí rơi vật phẩm thích hợp
        /// </summary>
        /// <param name="mapCode"></param>
        /// <param name="copyMapID"></param>
        /// <param name="posX"></param>
        /// <param name="posY"></param>
        /// <returns></returns>
        public static UnityEngine.Vector2 FindGoodsPackFallPosition(int mapCode, int copyMapID, int posX, int posY)
        {
            /// Nếu không mở chức năng
            if (!KTGlobal.EnableGoodsPackFallPositionCheck)
            {
                /// Trả về kết quả là vị trí ban đầu luôn
                return new UnityEngine.Vector2(posX, posY);
            }

            /// Bản đồ tương ứng
            GameMap gameMap = KTMapManager.Find(mapCode);
            /// Toác
            if (gameMap == null)
            {
                return new UnityEngine.Vector2(posX, posY);
            }

            /// Tọa độ thực
            UnityEngine.Vector2 worldPos = new UnityEngine.Vector2(posX, posY);
            /// Tọa độ lưới
            UnityEngine.Vector2 gridPos = KTGlobal.WorldPositionToGridPosition(gameMap, worldPos);
            /// Chuyển sang tọa độ thực tương ứng
            UnityEngine.Vector2 gridToWorldPos = KTGlobal.GridPositionToWorldPosition(gameMap, gridPos);
            /// Độ lệch so với tọa độ thực ban đầu
            UnityEngine.Vector2 diffPos = gridToWorldPos - worldPos;

            /// Duyệt danh sách các ô xung quanh
            for (int k = 0; k < KTGlobal.fallGoodsPositions.Length; k += 2)
            {
                /// Tọa độ lưới sẽ rơi
                int gridPosX = KTGlobal.fallGoodsPositions[k] + (int) gridPos.x;
                int gridPosY = KTGlobal.fallGoodsPositions[k + 1] + (int) gridPos.y;

                /// Nếu vị trí này nằm trong điểm Block
                if (KTGlobal.InOnlyObs(mapCode, gridPosX, gridPosY, copyMapID))
                {
                    /// Bỏ qua
                    continue;
                }

                /// Thử tìm xem đã có vật phẩm nào ở đây chưa
                KGoodsPack goodsPack = KTGlobal.FindGoodsPackAtPosition(mapCode, copyMapID, gridPosX, gridPosY);
                /// Nếu không tồn tại
                if (goodsPack == null)
                {
                    /// Đây là vị trí cần tìm
                    UnityEngine.Vector2 _gridPos = new UnityEngine.Vector2(gridPosX, gridPosY);
                    /// Trả về kết quả
                    return KTGlobal.GridPositionToWorldPosition(gameMap, _gridPos) + diffPos;
                }
            }

            /// Không tìm thấy
            return new UnityEngine.Vector2(posX, posY);
        }
        #endregion
    }
}
