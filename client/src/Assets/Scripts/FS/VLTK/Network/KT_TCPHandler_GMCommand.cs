using FS.GameEngine.Network;
using FS.VLTK.Utilities;
using HSGameEngine.GameEngine.Network.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FS.VLTK.Network
{
    /// <summary>
    /// Quản lý tương tác với Socket
    /// </summary>
    public static partial class KT_TCPHandler
    {
        #region GM-Command
        /// <summary>
        /// Lệnh GM
        /// </summary>
        /// <param name="command"></param>
        public static void SendGMCommand(string command)
        {
            byte[] bytes = KTCrypto.Encrypt(command);
            GameInstance.Game.GameClient.SendData(TCPOutPacket.MakeTCPOutPacket(GameInstance.Game.GameClient.OutPacketPool, bytes, 0, bytes.Length, (int) (TCPGameServerCmds.CMD_KT_GM_COMMAND)));
        }
        #endregion
    }
}
