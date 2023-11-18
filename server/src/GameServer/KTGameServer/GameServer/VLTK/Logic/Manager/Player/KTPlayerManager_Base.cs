using GameServer.Interface;
using GameServer.Logic;
using GameServer.Server;
using Server.Data;
using System.Collections.Generic;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Quản lý người chơi
    /// </summary>
    public static partial class KTPlayerManager
    {
        #region Hiển thị
        /// <summary>
        /// Thông báo bản thân bị xóa cho tất cả người chơi khác
        /// </summary>
        /// <param name="client"></param>
        /// <param name="client"></param>
        public static void NotifyMyselfLeaveOthers(KPlayer client)
        {
            /// Danh sách người chươi xung quanh
            List<KPlayer> players = KTRadarMapManager.GetPlayersAround(client);

            /// Nếu tồn tại
            if (players != null)
            {
                /// Duyệt danh sách
                foreach (KPlayer player in players)
                {
                    /// Nếu là chính mình
                    if (client.RoleID == player.RoleID)
                    {
                        /// Bỏ qua
                        continue;
                    }

                    string strcmd = string.Format("{0}", player.RoleID);
                    client.SendPacket((int) TCPGameServerCmds.CMD_SPR_LEAVE, strcmd);
                }
            }
        }

        /// <summary>
        /// Xử lý khi đối tượng được tải xuống thành công
        /// </summary>
        /// <param name="client"></param>
        /// <param name="target"></param>
        public static void HandlePlayerLoaded(KPlayer client, KPlayer target)
        {
            string pathString = KTPlayerStoryBoardEx.Instance.GetCurrentPathString(target);

            /// Dữ liệu
            LoadAlreadyData data = new LoadAlreadyData()
            {
                RoleID = target.RoleID,
                PosX = target.PosX,
                PosY = target.PosY,
                Direction = (int) target.CurrentDir,
                Action = (int) target.m_eDoing,
                PathString = pathString,
                ToX = (int) target.ToPos.X,
                ToY = (int) target.ToPos.Y,
                Camp = target.Camp,
            };
            /// Gửi gói tin
            client.SendPacket<LoadAlreadyData>((int) TCPGameServerCmds.CMD_SPR_LOADALREADY, data);
        }
        #endregion
    }
}
