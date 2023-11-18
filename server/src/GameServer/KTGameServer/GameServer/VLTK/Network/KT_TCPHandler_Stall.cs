using GameServer.Logic;
using GameServer.Server;
using GameServer.VLTK.Core.StallManager;
using Server.Data;
using Server.Tools;
using System.Collections.Generic;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Quản lý động tác của người chơi
    /// </summary>
    public static partial class KT_TCPHandler
    {
        /// <summary>
        /// Thông báo thông tin sạp hàng tương ứng
        /// </summary>
        /// <param name="client"></param>
        /// <param name="sd"></param>
        public static void NotifyGoodsStallData(KPlayer client, StallData sd)
        {
            byte[] bytesData = null;
            lock (sd)
            {
                bytesData = DataHelper.ObjectToBytes<StallData>(sd);
            }

            client.SendPacket((int) TCPGameServerCmds.CMD_SPR_STALLDATA, bytesData);
        }

        /// <summary>
        /// Thông báo cho bọn xung quanh biết thằng này bày bán
        /// </summary>
        /// <param name="client"></param>
        public static void NotifySpriteStartStall(KPlayer client)
        {
            StallManager.TotalServerStall.TryGetValue(client.RoleID, out StallData _Stall);

            if (null == _Stall)
            {
                return;
            }

            List<KPlayer> objsList = KTRadarMapManager.GetPlayersAround(client);
            if (null == objsList)
                return;

            /// Dữ liệu
            StallAction data = new StallAction()
            {
                Fields = new List<string>()
                {
                    client.RoleID.ToString(),
                    _Stall.StallName
                },
            };
            /// Chuỗi Byte
            byte[] byteData = DataHelper.ObjectToBytes<StallAction>(data);
            KTPlayerManager.SendPacketToPlayers((int) TCPGameServerCmds.CMD_SPR_ROLE_START_STALL, objsList, byteData, client, null);
        }

        /// <summary>
        /// Thông báo cho bọn xung quanh biết thằng này hủy bán
        /// </summary>
        /// <param name="client"></param>
        public static void NotifySpriteStopStall(KPlayer client)
        {
            StallManager.TotalServerStall.TryGetValue(client.RoleID, out StallData _Stall);
            if (null == _Stall)
            {
                return;
            }

            List<KPlayer> objsList = KTRadarMapManager.GetPlayersAround(client);
            if (null == objsList)
                return;

            string strcmd = string.Format("{0}", client.RoleID);
            KTPlayerManager.SendPacketToPlayers((int) TCPGameServerCmds.CMD_SPR_ROLE_STOP_STALL, objsList, strcmd, client, null);
        }
    }
}
