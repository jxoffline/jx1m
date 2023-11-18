using FS.GameEngine.Logic;
using FS.GameEngine.Network;
using HSGameEngine.GameEngine.Network.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FS.VLTK.Network
{
    /// <summary>
    /// Quản lý tương tác với Socket
    /// </summary>
    public static partial class KT_TCPHandler
    {
        #region Shop
        /// <summary>
        /// Gửi yêu cầu lên Server mở khung Kỳ Trân Các
        /// </summary>
        public static void SendOpenTokenShop()
        {
            if (!Global.Data.PlayGame)
            {
                return;
            }
            string strCmd = string.Format("");
            byte[] bytes = new ASCIIEncoding().GetBytes(strCmd);
            GameInstance.Game.GameClient.SendData(TCPOutPacket.MakeTCPOutPacket(GameInstance.Game.GameClient.OutPacketPool, bytes, 0, bytes.Length, (int) (TCPGameServerCmds.CMD_KT_OPEN_TOKENSHOP)));
        }
        #endregion
    }
}
