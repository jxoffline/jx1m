using GameDBServer.Server;
using Server.Tools;
using System;
using System.Net.Sockets;

namespace Server.Protocol
{

    public delegate bool TCPCmdPacketEventHandler(object sender);


    internal class TCPInPacket : IDisposable
    {

        public TCPInPacket(int recvBufferSize = (int)TCPCmdPacketSize.MAX_SIZE)
        {
            PacketBytes = new byte[recvBufferSize];
        }


        private byte[] PacketBytes = null;


        public byte[] GetPacketBytes()
        {
            return PacketBytes;
        }


        private Socket _Socket = null;


        public Socket CurrentSocket
        {
            get { return _Socket; }
            set { _Socket = value; }
        }


        private UInt16 _PacketCmdID = 0;


        public UInt16 PacketCmdID
        {
            get
            {
                UInt16 ret = 0;
                lock (this)
                {
                    ret = _PacketCmdID;
                }
                return ret;
            }
        }


        private Int32 _PacketDataSize = 0;


        public Int32 PacketDataSize
        {
            get
            {
                Int32 ret = 0;
                lock (this)
                {
                    ret = _PacketDataSize;
                }
                return ret;
            }
        }


        private Int32 PacketDataHaveSize = 0;


        private bool IsWaitingData = false;


        public void Dispose()
        {
            Reset();
        }


        public event TCPCmdPacketEventHandler TCPCmdPacketEvent;


        private byte[] CmdHeaderBuffer = new byte[6];

        private int CmdHeaderSize = 0;


        public bool WriteData(byte[] buffer, int offset, int count)
        {

            lock (this)
            {
                if (IsWaitingData)
                {
                    int copyCount = (count >= (_PacketDataSize - PacketDataHaveSize)) ? _PacketDataSize - PacketDataHaveSize : count;
                    if (copyCount > 0)
                    {
                        DataHelper.CopyBytes(PacketBytes, PacketDataHaveSize, buffer, offset, copyCount);
                        PacketDataHaveSize += copyCount;
                    }


                    if (PacketDataHaveSize >= _PacketDataSize)
                    {
                        bool eventReturn = true;


                        if (null != TCPCmdPacketEvent)
                        {
                            eventReturn = TCPCmdPacketEvent(this);
                        }


                        _PacketCmdID = 0;
                        _PacketDataSize = 0;
                        PacketDataHaveSize = 0;
                        IsWaitingData = false;
                        CmdHeaderSize = 0;

                        if (!eventReturn)
                        {
                            return false;
                        }


                        if (count > copyCount)
                        {

                            offset += copyCount;
                            count -= copyCount;


                            return WriteData(buffer, offset, count);
                        }
                    }

                    return true;
                }
                else
                {

                    int copyLeftSize = count > (6 - CmdHeaderSize) ? (6 - CmdHeaderSize) : count;
                    DataHelper.CopyBytes(CmdHeaderBuffer, CmdHeaderSize, buffer, offset, copyLeftSize);
                    CmdHeaderSize += copyLeftSize;
                    if (CmdHeaderSize < 6)
                    {
                        return true;
                    }


                    _PacketDataSize = BitConverter.ToInt32(CmdHeaderBuffer, 0);


                    _PacketCmdID = BitConverter.ToUInt16(CmdHeaderBuffer, 4);

                    if (_PacketDataSize <= 0 || _PacketDataSize >= (Int32)TCPCmdPacketSize.MAX_SIZE)
                    {

                        LogManager.WriteLog(LogTypes.Error, string.Format("接收到的非法数据长度的tcp命令, Cmd={0}, Length={1}, offset={2}, count={3}", (TCPGameServerCmds)_PacketCmdID, _PacketDataSize, offset, count));
                        return false;
                    }


                    offset += copyLeftSize;


                    count -= copyLeftSize;


                    IsWaitingData = true;


                    PacketDataHaveSize = 0;
                    _PacketDataSize -= 2;


                    return WriteData(buffer, offset, count);
                }
            }
        }


        public void Reset()
        {
            lock (this)
            {
                _Socket = null;
                _PacketCmdID = 0;
                _PacketDataSize = 0;
                PacketDataHaveSize = 0;
                IsWaitingData = false;
                CmdHeaderSize = 0;
            }
        }
    }
}