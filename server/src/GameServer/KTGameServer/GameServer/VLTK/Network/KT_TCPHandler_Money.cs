using GameServer.Logic;
using GameServer.Server;

namespace GameServer.KiemThe.Logic
{
    /// <summary>
    /// Quản lý động tác của người chơi
    /// </summary>
    public static partial class KT_TCPHandler
    {
        /// <summary>
        /// Thông báo về Client bạc trong kho thay đổi
        /// </summary>
        /// <param name="client"></param>
        public static void NotifySelfUserStoreMoneyChange(KPlayer client)
        {
            string strcmd = string.Format("{0}:{1}", client.RoleID, client.StoreMoney);
            client.SendPacket((int) TCPGameServerCmds.CMD_SPR_STORE_MONEY_CHANGE, strcmd);
        }

        /// <summary>
        /// Thông báo số bạc và bạc khóa thay đổi
        /// </summary>
        /// <param name="client"></param>
        public static void NotifySelfMoneyChange(KPlayer client)
        {
            string strcmd = string.Format("{0}:{1}:{2}:{3}", client.RoleID, client.Money, client.BoundMoney,client.RoleGuildMoney);
            client.SendPacket((int) TCPGameServerCmds.CMD_SPR_MONEYCHANGE, strcmd);
        }

        /// <summary>
        /// Thông báo số đồng, đồng khóa thay đổi
        /// </summary>
        /// <param name="client"></param>
        public static void NotifySelfTokenChange(KPlayer client)
        {
            string strcmd = string.Format("{0}:{1}:{2}", client.RoleID, client.Token, client.BoundToken);
            client.SendPacket((int) TCPGameServerCmds.CMD_SPR_TokenCHANGE, strcmd);
        }
    }
}
