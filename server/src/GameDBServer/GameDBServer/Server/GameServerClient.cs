using Server.Protocol;
using Server.Tools;
using System.Net.Sockets;

namespace GameDBServer.Server
{
    public class GameServerClient
    {
        private Socket currentSocket;

        public GameServerClient(Socket currentSocket)
        {
            this.currentSocket = currentSocket;
        }

        public Socket CurrentSocket
        {
            get { return this.currentSocket; }
        }

        /// <summary>
        /// 分线ID
        /// </summary>
        public int LineId;

        /// <summary>
        /// 向GameServer发送指令
        /// </summary>
        public void sendCmd(int cmdId, string cmdData)
        {
            TCPManager.getInstance().MySocketListener.SendData(currentSocket, TCPOutPacket.MakeTCPOutPacket(TCPOutPacketPool.getInstance(), cmdData, cmdId));
        }

        /// <summary>
        /// 向GameServer发送指令
        /// </summary>
        public void sendCmd<T>(int cmdId, T cmdData)
        {
            TCPManager.getInstance().MySocketListener.SendData(currentSocket, DataHelper.ObjectToTCPOutPacket<T>(cmdData, TCPOutPacketPool.getInstance(), cmdId));
        }

        public void release()
        {
            currentSocket = null;
        }
    }
}