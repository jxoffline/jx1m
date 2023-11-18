using GameServer.Interface;
using GameServer.KiemThe.CopySceneEvents;
using GameServer.KiemThe.Core.Item;
using GameServer.Logic;
using GameServer.Server;
using Server.Data;
using Server.Protocol;
using Server.TCP;
using Server.Tools;
using System.Collections.Generic;
using static GameServer.Logic.KTMapManager;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Quản lý Bot bán hàng
    /// </summary>
    public static partial class KTStallBotManager
    {
        #region Quản lý
        /// <summary>
        /// Tạo Bot bán hàng
        /// </summary>
        /// <param name="mapCode">ID bản đồ</param>
        /// <param name="posX">Tọa độ X</param>
        /// <param name="posY">Tọa độ Y</param>
        /// <param name="player">Chủ nhân tương ứng</param>
        /// <param name="stallName">Tên cửa hàng</param>
        /// <param name="lifeTime">Thời gian tồn tại (-1 là vĩnh viễn)</param>
        /// <returns></returns>
        public static KStallBot Create(int mapCode, int posX, int posY, KPlayer player, string stallName, int lifeTime)
        {
            /// Bot cũ
            KStallBot oldBot = KTStallBotManager.FindBotByOwnerID(player.RoleID);
            /// Nếu Bot cũ có tồn tại
            if (oldBot != null)
            {
                /// Xóa con cũ đi
                oldBot.Destroy();
            }

            /// Thông tin bản đồ tương ứng
            GameMap gameMap = KTMapManager.Find(mapCode);
            /// Nếu không tồn tại
            if (gameMap == null)
            {
                /// Toác
                return null;
            }

            /// Tạo mới
            KStallBot bot = new KStallBot(player)
            {
                CurrentMapCode = mapCode,
                CurrentCopyMapID = -1,
                CurrentPos = new System.Windows.Point(posX, posY),
                StallName = stallName,
                LifeTime = lifeTime,
            };

            /// Bắt đầu luồng quản lý
            KTStallBotTimerManager.Instance.Add(bot);
            /// Thêm vào danh sách
            KTStallBotManager.bots[bot.RoleID] = bot;

            /// Thêm vào MapGrid
            gameMap.Grid.MoveObject(posX, posY, bot);

            /// Trả về kết quả
            return bot;
        }

        /// <summary>
        /// Tạo Bot bán hàng
        /// </summary>
        /// <param name="mapCode">ID bản đồ</param>
        /// <param name="posX">Tọa độ X</param>
        /// <param name="posY">Tọa độ Y</param>
        /// <param name="ownerPlayerRoleID">ID chủ nhân</param>
        /// <param name="ownerPlayerRoleName">Tên chủ nhân</param>
        /// <param name="ownerRoleSex">Giới tính chủ nhân</param>
        /// <param name="armorID">ID áo</param>
        /// <param name="helmID">ID mũ</param>
        /// <param name="mantleID">ID phi phong</param>
        /// <param name="stallName">Tên cửa hàng</param>
        /// <param name="lifeTime">Thời gian tồn tại (-1 là vĩnh viễn)</param>
        /// <returns></returns>
        public static KStallBot Create(int mapCode, int posX, int posY, int ownerPlayerRoleID, string ownerPlayerRoleName, int ownerRoleSex, string stallName, int armorID, int helmID, int mantleID, int lifeTime)
        {
            /// Bot cũ
            KStallBot oldBot = KTStallBotManager.FindBotByOwnerID(ownerPlayerRoleID);
            /// Nếu Bot cũ có tồn tại
            if (oldBot != null)
            {
                /// Xóa con cũ đi
                oldBot.Destroy();
            }

            /// Thông tin bản đồ tương ứng
            GameMap gameMap = KTMapManager.Find(mapCode);
            /// Nếu không tồn tại
            if (gameMap == null)
            {
                /// Toác
                return null;
            }

            /// Tạo mới
            KStallBot bot = new KStallBot()
            {
                OwnerRoleID = ownerPlayerRoleID,
                OwnerRoleName = ownerPlayerRoleName,
                OwnerRoleSex = ownerRoleSex,
                ArmorID = armorID,
                HelmID = helmID,
                MantleID = mantleID,
                CurrentMapCode = mapCode,
                CurrentCopyMapID = -1,
                CurrentPos = new System.Windows.Point(posX, posY),
                StallName = stallName,
                LifeTime = lifeTime,
            };

            /// Bắt đầu luồng quản lý
            KTStallBotTimerManager.Instance.Add(bot);
            /// Thêm vào danh sách
            KTStallBotManager.bots[bot.RoleID] = bot;

            /// Thêm vào MapGrid
            gameMap.Grid.MoveObject(posX, posY, bot);

            /// Trả về kết quả
            return bot;
        }

        /// <summary>
        /// Xóa Bot bán hàng tương ứng
        /// </summary>
        /// <param name="bot"></param>
        public static void Remove(KStallBot bot)
        {
            /// Toác
            if (bot == null)
            {
                return;
            }

            /// Reset đối tượng
            bot.Destroy(false);
            /// Ngừng luồng quản lý
            KTStallBotTimerManager.Instance.Remove(bot);
            /// Xóa khỏi danh sách
            KTStallBotManager.bots.TryRemove(bot.RoleID, out _);

            /// Thông báo cho bọn xung quanh thằng Bot này thực hiện động tác hủy sạp hàng
            KTStallBotManager.NotifyClientsAroundBotPreRemoved(bot);

            GameMap gameMap = KTMapManager.Find(bot.CurrentMapCode);
            /// Xóa khỏi MapGrid
            gameMap.Grid.RemoveObject(bot);
        }
        #endregion

        #region Hiển thị
        /// <summary>
        /// Thông báo Bot trước khi bị hủy tới toàn bộ người chơi xung quanh
        /// </summary>
        /// <param name="bot"></param>
        private static void NotifyClientsAroundBotPreRemoved(KStallBot bot)
        {
            /// Tìm tất cả người chơi xung quanh để gửi gói tin
            List<KPlayer> listObjects = KTRadarMapManager.GetPlayersAround(bot);
            if (listObjects == null)
            {
                return;
            }

            /// Duyệt danh sách
            foreach (KPlayer player in listObjects)
            {
                /// Dữ liệu gửi đi
                string strcmd = string.Format("{0}:{1}", 1, bot.RoleID);
                /// Gửi đi
                player.SendPacket((int) TCPGameServerCmds.CMD_KT_DEL_STALLBOT, strcmd);
            }
        }

        /// <summary>
        /// Xử lý khi đối tượng xe tiêu được tải xuống thành công
        /// </summary>
        /// <param name="client"></param>
        /// <param name="carriage"></param>
        public static void HandleStallBotLoaded(KPlayer client, KStallBot stallBot)
        {
            LoadAlreadyData data = new LoadAlreadyData()
            {
                RoleID = stallBot.RoleID,
                PosX = (int) stallBot.CurrentPos.X,
                PosY = (int) stallBot.CurrentPos.Y,
                Direction = (int) stallBot.CurrentDir,
                Action = -1,
                PathString = "",
                ToX = -1,
                ToY = -1,
                Camp = -1,
            };
            client.SendPacket<LoadAlreadyData>((int) TCPGameServerCmds.CMD_SPR_LOADALREADY, data);
        }
        #endregion
    }
}
