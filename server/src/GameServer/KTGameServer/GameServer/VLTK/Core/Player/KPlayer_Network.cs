using GameServer.Server;
using Server.Protocol;
using Server.Tools;

namespace GameServer.Logic
{
    /// <summary>
    /// Hỗ trợ tầng Net
    /// </summary>
    public partial class KPlayer
    {
        #region Gửi gói tin
        /// <summary>
        /// Gửi gói tin về cho bản thân
        /// </summary>
        public void SendPacket(int cmdId, string cmdData)
        {
            TCPManager.getInstance().MySocketListener.SendData(ClientSocket, TCPOutPacket.MakeTCPOutPacket(TCPOutPacketPool.getInstance(), cmdData, cmdId));
        }

        /// <summary>
        /// Gửi gói tin về cho bản thân
        /// </summary>
        public void SendPacket<T>(int cmdId, T cmdData)
        {
            TCPManager.getInstance().MySocketListener.SendData(ClientSocket, DataHelper.ObjectToTCPOutPacket<T>(cmdData, TCPOutPacketPool.getInstance(), cmdId));
        }

        /// <summary>
        /// Gửi gói tin về cho bản thân
        /// </summary>
        /// <param name="cmdId"></param>
        /// <param name="byteData"></param>
        public void SendPacket(int cmdId, byte[] byteData)
        {
            TCPManager.getInstance().MySocketListener.SendData(ClientSocket, TCPOutPacket.MakeTCPOutPacket(TCPOutPacketPool.getInstance(), byteData, cmdId));
        }
        #endregion
    }
}
