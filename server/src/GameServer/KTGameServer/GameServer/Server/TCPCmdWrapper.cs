using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using GameServer.Logic;
using Server.Protocol;
using Server.TCP;

namespace GameServer.Server
{
    public class TCPCmdWrapper
    {
        public static int MaxCachedWrapperCount = 1000;
        private static Queue<TCPCmdWrapper> CachedWrapperList = new Queue<TCPCmdWrapper>();

        private static Object CountLock = new Object();
        private static int TotalWrapperCount = 0;

        private TCPManager tcpMgr;

        private TMSKSocket socket;
        
        private TCPClientPool tcpClientPool;
        
        private TCPRandKey tcpRandKey;
        
        private TCPOutPacketPool pool;
       
        private int nID;
       
        private byte[] data;
       
        private int count;

        public TCPCmdWrapper(TCPManager tcpMgr, 
                        TMSKSocket socket, 
                        TCPClientPool tcpClientPool, 
                        TCPRandKey tcpRandKey,
                        TCPOutPacketPool pool,
                        int nID,
                        byte[] data,
                        int count) 
        {
            this.tcpMgr = tcpMgr;
            this.socket = socket;
            this.tcpClientPool = tcpClientPool;
            this.tcpRandKey = tcpRandKey;
            this.pool = pool;
            this.nID = nID;
            this.data = data;
            this.count = count;

            lock (CountLock)
            {
                TotalWrapperCount++;
            }
        }

        public TCPManager TcpMgr
        {
            get { return tcpMgr; }
        }
        
        public int Count
        {
            get { return count; }
        }

         public byte[] Data
        {
            get { return data; }
        }

        public int NID
        {
            get { return nID; }
        }

         public TCPOutPacketPool Pool
        {
            get { return pool; }
        }

        public TCPRandKey TcpRandKey
        {
            get { return tcpRandKey; }
        }

        public TCPClientPool TcpClientPool
        {
            get { return tcpClientPool; }
        }

        public TMSKSocket TMSKSocket
        {
            get { return socket; }
        }

        public static int GetTotalCount()
        {
            int totalCount = 0;
            lock (CountLock)
            {
                totalCount = TotalWrapperCount;
            }

            return totalCount;
        }

        public static TCPCmdWrapper Get(TCPManager tcpMgr,
                        TMSKSocket socket,
                        TCPClientPool tcpClientPool,
                        TCPRandKey tcpRandKey,
                        TCPOutPacketPool pool,
                        int nID,
                        byte[] data,
                        int count)
        {
            TCPCmdWrapper wrapper = null;
            lock (CachedWrapperList)
            {
                if (CachedWrapperList.Count > 0)
                {
                    wrapper = CachedWrapperList.Dequeue();
                    wrapper.tcpMgr = tcpMgr;
                    wrapper.socket = socket;
                    wrapper.tcpClientPool = tcpClientPool;
                    wrapper.tcpRandKey = tcpRandKey;
                    wrapper.pool = pool;
                    wrapper.nID = nID;
                    wrapper.data = data;
                    wrapper.count = count;
                }
            }
            if (null == wrapper)
            {
                wrapper = new TCPCmdWrapper(tcpMgr, socket, tcpClientPool, tcpRandKey, pool, nID, data, count);
            }
            return wrapper;
        }

        public void release()
        {
            lock (CachedWrapperList)
            {
                if (CachedWrapperList.Count < MaxCachedWrapperCount)
                {
                    CachedWrapperList.Enqueue(this);
                    return;
                }
            }

            this.tcpMgr = null;
            this.socket = null;
            this.tcpClientPool = null;
            this.tcpRandKey = null;
            this.pool = null;
            this.data = null;
            lock (CountLock)
            {
                TotalWrapperCount--;
            }
        }
    }
}
