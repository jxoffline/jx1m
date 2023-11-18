using Server.Tools;
using System;
using System.Text;

namespace Server.Protocol
{

    public class TCPOutPacket : IDisposable
    {

        public TCPOutPacket()
        {
        }


        private byte[] PacketBytes = null;

        public byte[] GetPacketBytes()
        {
            return PacketBytes;
        }


        private UInt16 _PacketCmdID = 0;


        public UInt16 PacketCmdID
        {
            get { return _PacketCmdID; }
            set { _PacketCmdID = value; }
        }


        private Int32 _PacketDataSize = 0;


        public Int32 PacketDataSize
        {
            get { return (Int32)(_PacketDataSize + 6); }
        }


        public object Tag
        {
            get;
            set;
        }

        public bool FinalWriteData(byte[] buffer, int offset, int count)
        {
            if (null != PacketBytes)
            {
                LogManager.WriteLog(LogTypes.Error, string.Format("TCP发出命令包不能被重复写入数据, 命令ID: {0}", PacketCmdID));
                return false;
            }


            if ((6 + count) >= (int)TCPCmdPacketSize.MAX_SIZE)
            {

                LogManager.WriteLog(LogTypes.Error, string.Format("TCP命令包长度:{0}, 最大不能超过: {1}, 命令ID: {2}", count, (int)TCPCmdPacketSize.MAX_SIZE, PacketCmdID));
                return false;
            }

            PacketBytes = new byte[count + 6];


            int offsetTo = (int)6;
            DataHelper.CopyBytes(PacketBytes, offsetTo, buffer, offset, count);
            _PacketDataSize = count;
            Final();

            return true;
        }

        private void Final()
        {

            Int32 length = (Int32)(_PacketDataSize + 2);
            DataHelper.CopyBytes(PacketBytes, 0, BitConverter.GetBytes(length), 0, 4);

            //拷贝指令
            DataHelper.CopyBytes(PacketBytes, 4, BitConverter.GetBytes(_PacketCmdID), 0, 2);
        }

        public void Reset()
        {
            PacketBytes = null;
            PacketCmdID = 0;
            _PacketDataSize = 0;
        }


        public void Dispose()
        {
            Tag = null;
        }


        public static TCPOutPacket MakeTCPOutPacket(TCPOutPacketPool pool, string data, int cmd)
        {


            TCPOutPacket tcpOutPacket = pool.Pop();
            tcpOutPacket.PacketCmdID = (UInt16)cmd;
            byte[] bytesCmd = new UTF8Encoding().GetBytes(data);
            tcpOutPacket.FinalWriteData(bytesCmd, 0, bytesCmd.Length);
            return tcpOutPacket;
        }


        public static TCPOutPacket MakeTCPOutPacket(TCPOutPacketPool pool, byte[] data, int offset, int length, int cmd)
        {

            TCPOutPacket tcpOutPacket = pool.Pop();
            tcpOutPacket.PacketCmdID = (UInt16)cmd;
            tcpOutPacket.FinalWriteData(data, offset, length);
            return tcpOutPacket;
        }
    }
}