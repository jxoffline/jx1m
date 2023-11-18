using GameServer.Interface;
using GameServer.KiemThe.CopySceneEvents;
using GameServer.KiemThe.Core.Item;
using GameServer.Logic;
using GameServer.Server;
using Server.Data;
using Server.Protocol;
using Server.TCP;
using Server.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GameServer.Logic.KTMapManager;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Quản lý vật phẩm rơi
    /// </summary>
    public static partial class KTGoodsPackManager
    {
        #region Quản lý
        /// <summary>
        /// Tạo vật phẩm rơi
        /// </summary>
        /// <param name="mapCode">ID bản đồ</param>
        /// <param name="copyMapID">ID phụ bản</param>
        /// <param name="itemGD">Thông tin vật phẩm rơi</param>
        /// <param name="posX">Tọa độ X</param>
        /// <param name="posY">Tọa độ Y</param>
        /// <param name="owners">Danh sách người chơi là chủ nhân của vật phẩm</param>
        /// <returns></returns>
        public static KGoodsPack Create(int mapCode, int copyMapID, GoodsData itemGD, int posX, int posY, params KPlayer[] owners)
        {
            /// Thông tin bản đồ tương ứng
            GameMap gameMap = KTMapManager.Find(mapCode);
            /// Nếu không tồn tại
            if (gameMap == null)
            {
                /// Toác
                return null;
            }
            /// Nếu không có phụ bản
            if (copyMapID != -1 && !CopySceneEventManager.IsCopySceneExist(copyMapID, mapCode))
            {
                /// Toác
                return null;
            }

            /// Tạo mới
            KGoodsPack goodsPack = new KGoodsPack(itemGD)
            {
                CurrentMapCode = mapCode,
                CurrentCopyMapID = copyMapID,
                CurrentPos = new System.Windows.Point(posX, posY),
                OwnerRoleIDs = owners == null ? null : owners.Select(x => x.RoleID).ToHashSet(),
            };

            /// Bắt đầu luồng quản lý
            KTGoodsPackTimerManager.Instance.Add(goodsPack);
            /// Thêm vào danh sách
            KTGoodsPackManager.goodsPacks[goodsPack.ID] = goodsPack;

            /// Thêm vào MapGrid
            gameMap.Grid.MoveObject(posX, posY, goodsPack);

            /// Thông báo có vật phẩm rơi tới toàn bộ người chơi xung quanh
            KTGoodsPackManager.NotifyNewGoodsPacksToAllPlayersAround(goodsPack);

            /// Trả về kết quả
            return goodsPack;
        }

        /// <summary>
        /// Xóa vật phẩm rơi tương ứng
        /// </summary>
        /// <param name="goodsPack"></param>
        public static void Remove(KGoodsPack goodsPack)
        {
            /// Toác
            if (goodsPack == null)
            {
                return;
            }
            /// Thông tin bản đồ tương ứng
            GameMap gameMap = KTMapManager.Find(goodsPack.CurrentMapCode);
            /// Nếu không tồn tại
            if (gameMap == null)
            {
                /// Toác
                return;
            }

            /// Hủy đối tượng
            goodsPack.Destroy(false);
            /// Ngừng luồng quản lý
            KTGoodsPackTimerManager.Instance.Remove(goodsPack);
            /// Xóa khỏi danh sách
            KTGoodsPackManager.goodsPacks.TryRemove(goodsPack.ID, out _);

            /// Xóa khỏi MapGrid
            gameMap.Grid.RemoveObject(goodsPack);

            /// Thông báo xóa vật phẩm rơi tới toàn bộ người chơi xung quanh
            KTGoodsPackManager.NotifyRemoveGoodsPacksToAllPlayersAround(goodsPack);
        }
        #endregion

        #region Hiển thị
        /// <summary>
        /// Thông báo có vật phẩm rơi mới tới toàn bộ người chơi xung quanh
        /// </summary>
        /// <param name="goodsPack"></param>
        private static void NotifyNewGoodsPacksToAllPlayersAround(KGoodsPack goodsPack)
        {
            /// Nếu không tồn tại thì thôi
            if (!goodsPack.IsAlive)
            {
                return;
            }

            /// Số sao
            int nStar = 0;
            StarLevelStruct starStruct = ItemManager.ItemValueCalculation(goodsPack.GoodsData, out long itemValue,out int Totalines);
            /// Nếu không có sao
            if (starStruct != null)
            {
                nStar = starStruct.StarLevel / 2;
            }

            /// Tạo mới dữ liệu gửi về
            NewGoodsPackData newGoodsPackData = new NewGoodsPackData()
            {
                AutoID = goodsPack.ID,
                PosX = (int) goodsPack.CurrentPos.X,
                PosY = (int) goodsPack.CurrentPos.Y,
                GoodsID = goodsPack.GoodsData.GoodsID,
                GoodCount = goodsPack.GoodsData.GCount,
                LifeTime = goodsPack.LifeTime,
                HTMLColor = KTGlobal.GetItemNameColor(goodsPack.GoodsData),
                Star = nStar,
                LinesCount = Totalines,
                EnhanceLevel = goodsPack.GoodsData.Forge_level,
            };

            /// Tìm tất cả người chơi xung quanh để gửi gói tin
            List<KPlayer> listObjects = KTRadarMapManager.GetPlayersAround(goodsPack);
            if (listObjects == null)
            {
                return;
            }

            /// Duyệt danh sách
            foreach (KPlayer player in listObjects)
            {
                /// Gửi gói tin đi
                player.SendPacket<NewGoodsPackData>((int) TCPGameServerCmds.CMD_SPR_NEWGOODSPACK, newGoodsPackData);
            }
        }


        /// <summary>
        /// Thông báo xóa vật phẩm rơi tới toàn bộ người chơi xung quanh
        /// </summary>
        /// <param name="goodsPack"></param>
        private static void NotifyRemoveGoodsPacksToAllPlayersAround(KGoodsPack goodsPack)
        {
            /// Chuỗi dữ liệu
            string cmdData = string.Format("{0}", goodsPack.ID);

            /// Tìm tất cả người chơi xung quanh để gửi gói tin
            List<KPlayer> listObjects = KTRadarMapManager.GetPlayersAround(goodsPack);
            if (listObjects == null)
            {
                return;
            }

            /// Duyệt danh sách
            foreach (KPlayer player in listObjects)
            {
                /// Gửi gói tin đi
                player.SendPacket((int) TCPGameServerCmds.CMD_SPR_DELGOODSPACK, cmdData);
            }
        }

        /// <summary>
        /// Xóa vật phẩm rơi với bản thân
        /// </summary>
        /// <param name="client"></param>
        /// <param name="gp"></param>
        public static void DelMySelfGoodsPacks(KPlayer client, KGoodsPack gp)
        {
            /// Dữ liệu gửi đi
            string strcmd = string.Format("{0}", gp.ID);
            /// Gửi
            client.SendPacket((int) TCPGameServerCmds.CMD_SPR_DELGOODSPACK, strcmd);
        }
        #endregion
    }
}
