using FS.GameEngine.Logic;
using FS.GameEngine.Network;
using HSGameEngine.GameEngine.Network.Protocol;
using Server.Data;
using Server.Tools;
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
        #region Thiết lập hệ thống và Auto
        /// <summary>
        /// Gửi gói tin lên Server lưu thiết lập hệ thống
        /// </summary>
        public static void SendSaveSystemSettings()
        {
            if (!Global.Data.PlayGame)
            {
                return;
            }

            byte[] bytes = new ASCIIEncoding().GetBytes(Global.Data.RoleData.SystemSettings);
            GameInstance.Game.GameClient.SendData(TCPOutPacket.MakeTCPOutPacket(GameInstance.Game.GameClient.OutPacketPool, bytes, 0, bytes.Length, (int) (TCPGameServerCmds.CMD_KT_C2G_SAVESYSTEMSETTINGS)));
        }

        /// <summary>
        /// Gửi gói tin lên Server lưu thiết lập Auto
        /// </summary>
        public static void SendSaveAutoSettings()
        {
            if (!Global.Data.PlayGame)
            {
                return;
            }

            byte[] bytes = new ASCIIEncoding().GetBytes(Global.Data.RoleData.AutoSettings);
            GameInstance.Game.GameClient.SendData(TCPOutPacket.MakeTCPOutPacket(GameInstance.Game.GameClient.OutPacketPool, bytes, 0, bytes.Length, (int) (TCPGameServerCmds.CMD_KT_C2G_SAVEAUTOSETTINGS)));
        }
        #endregion

        #region Auto Path
        /// <summary>
        /// AutoPath gửi yêu cầu dịch map lên Server
        /// </summary>
        /// <param name="mapCode"></param>
        /// <param name="teleportItemID"></param>
        public static void SendAutoPathChangeMap(int mapCode, int teleportItemID, bool useNPC)
        {
            C2G_AutoPathChangeMap autoPathChangeMap = new C2G_AutoPathChangeMap()
            {
                ToMapCode = mapCode,
                ItemID = teleportItemID,
                UseNPC = useNPC,
            };
            byte[] cmdData = DataHelper.ObjectToBytes<C2G_AutoPathChangeMap>(autoPathChangeMap);
            GameInstance.Game.GameClient.SendData(TCPOutPacket.MakeTCPOutPacket(GameInstance.Game.GameClient.OutPacketPool, cmdData, 0, cmdData.Length, (int) (TCPGameServerCmds.CMD_KT_C2G_AUTOPATH_CHANGEMAP)));
        }
        #endregion
    }
}
