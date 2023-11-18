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
        #region Debug
        /// <summary>
        /// Nhận yêu cầu từ Server hiện các khối Debug ở các vị trí chỉ định
        /// </summary>
        /// <param name="cmdID"></param>
        /// <param name="data"></param>
        /// <param name="length"></param>
        public static void ReceiveShowDebugObjects(int cmdID, byte[] data, int length)
        {
            try
            {
                List<G2C_ShowDebugObject> objects = DataHelper.BytesToObject<List<G2C_ShowDebugObject>>(data, 0, length);
                foreach (G2C_ShowDebugObject obj in objects)
                {
                    KTGlobal.ShowDebugObjectAtPos(obj.Pos, obj.Size, obj.LifeTime);
                }
            }
            catch (Exception) { }
        }
        #endregion

        #region Test
        /// <summary>
        /// Nhận gói tin Test từ Server (dạng ProtoBuffer Bytes)
        /// </summary>
        /// <param name="cmdID">ID gói tin (ví dụ TCPGameServerCmds.CMD_KT_TESTPACKET)</param>
        /// <param name="data">Chuỗi Bytes chứa dữ liệu gửi về</param>
        /// <param name="length">Độ dài chuỗi Bytes</param>
        public static void ReceiveTestFromServer(int cmdID, byte[] data, int length)
        {
            try
            {

            }
            catch (Exception) { }
        }

        /// <summary>
        /// Nhận gói tin Test từ Server (dạng String)
        /// </summary>
        /// <param name="fields">Mạng chứa thông tin mã hóa dạng string của Packet</param>
        public static void ReceiveTestFromServer(string[] fields)
        {
            /// "1:2:3:4:5_6"
            try
            {
                KTDebug.LogError("Receive from GS:");
                foreach (string param in fields)
                {
                    KTDebug.LogError(param);
                }

                //int param1 = int.Parse(fields[0]);
                //int param2 = int.Parse(fields[1]);
                /// TODO
            }
            catch (Exception) { }
        }

        /// <summary>
        /// Gửi gói tin Test từ Client đến Server
        /// <para>Các Param nào muốn truyền vào hay gì đó thì là biến đầu vào của hàm này</para>
        /// </summary>
        /// <param name="message">Nội dung gói tin dạng String</param>
        public static void SendTestFromClient(string message)
        {
            /// Chuyển chuỗi message thành dạng byte array để gửi đi
            byte[] bytes = new ASCIIEncoding().GetBytes(message);
            /// Gửi gói tin về GS
            GameInstance.Game.GameClient.SendData(TCPOutPacket.MakeTCPOutPacket(GameInstance.Game.GameClient.OutPacketPool, bytes, 0, bytes.Length, (int) (TCPGameServerCmds.CMD_KT_TESTPACKET)));
        }
        #endregion
    }
}
