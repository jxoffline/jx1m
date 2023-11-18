using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using GameServer.Core.Executor;
using Server.Tools;

namespace GameServer.Server
{
    public interface IConnectInfoContainer
    {
     
        void AddDBConnectInfo(int index, String info);
    }

 
    public class TCPClientPool
    {

        private static TCPClientPool instance = new TCPClientPool();

        private static TCPClientPool logInstance = new TCPClientPool();

        private TCPClientPool() { }

        public static TCPClientPool getInstance()
        {
            return instance;
        }

        public static TCPClientPool getLogInstance()
        {
            return logInstance;
        }

      
        public void initialize(int capacity)
        {
            this.pool = new Queue<TCPClient>(capacity);
        }

     
        private int _InitCount = 0;

    
        public int InitCount
        {
            get { return _InitCount; }
        }

       
        private int ErrCount = 0;

        private int ItemCount = 0;

       
        private string RemoteIP = "";

      
        private int RemotePort = 0;

       
        private Queue<TCPClient> pool;

     
        private Semaphore semaphoreClients;

    
        public Program RootWindow
        {
            get;
            set;
        }

        private string ServerName = "";

       
        public void Init(int count, string ip, int port, string serverName)
        {
            ServerName = serverName;

            _InitCount = count;
            ItemCount = 0;
            RemoteIP = ip;
            RemotePort = port;
            this.semaphoreClients = new Semaphore(count, count);
            for (int i = 0; i < count; i++)
            {
                TCPClient tcpClient = new TCPClient() { RootWindow = RootWindow, ListIndex = ItemCount };

                RootWindow.AddDBConnectInfo(ItemCount, string.Format("{0}, 准备连接到{1}: {2}{3}", ItemCount, ServerName, RemoteIP, RemotePort));

                tcpClient.Connect(RemoteIP, RemotePort, ServerName);
                this.pool.Enqueue(tcpClient);
                ItemCount++;
            }
        }

     
        public void Clear()
        {
            lock (this.pool)
            {
                for (int i = 0; i < this.pool.Count; i++)
                {
                    TCPClient tcpClient = this.pool.ElementAt(i);
                    tcpClient.Disconnect();
                }

                this.pool.Clear();
            }
        }

     
        public int GetPoolCount()
        {
            lock (this.pool)
            {
                return this.pool.Count;
            }
        }

        public void Supply()
        {
            lock (this.pool)
            {
                if (ErrCount <= 0)
                {
                    return;
                }

                if (ErrCount > 0)
                {
                    try
                    {
                        TCPClient tcpClient = new TCPClient() { RootWindow = RootWindow, ListIndex = ItemCount };

                        RootWindow.AddDBConnectInfo(ItemCount, string.Format("{0}, 准备连接到{1}: {2}{3}", ItemCount, ServerName, RemoteIP, RemotePort));

                        ItemCount++;

                        tcpClient.Connect(RemoteIP, RemotePort, ServerName);
                        this.pool.Enqueue(tcpClient);
                        ErrCount--;

                        this.semaphoreClients.Release();
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

     
        public TCPClient Pop()
        {
            this.semaphoreClients.WaitOne(); //防止无法获取， 阻塞等待
            lock (this.pool)
            {
                return this.pool.Dequeue();
            }
        }

     
        public void Push(TCPClient tcpClient)
        {
          
            if (!tcpClient.IsConnected())
            {
                lock (this.pool)
                {
                    ErrCount++;
                }

                return;
            }

            lock (this.pool)
            {
                this.pool.Enqueue(tcpClient);
            }

            this.semaphoreClients.Release();
        }
    }

   
    public class GameDbClientPool : IConnectInfoContainer
    {
        public GameDbClientPool()
        {
            RootWindow = this;
            pool = new Queue<TCPClient>();
        }

    
        public Dictionary<int, String> DBServerConnectDict = new Dictionary<int, string>();

        public void AddDBConnectInfo(int index, String info)
        {
            lock (DBServerConnectDict)
            {
                if (DBServerConnectDict.ContainsKey(index))
                {
                    DBServerConnectDict[index] = info;
                }
                else
                {
                    DBServerConnectDict.Add(index, info);
                }
            }
        }

      
        public void initialize(int capacity)
        {
            this.pool = new Queue<TCPClient>(capacity);
        }

        private int _InitCount = 0;

    
        public int InitCount
        {
            get { return _InitCount; }
        }

     
        private int ErrCount = 0;

      
        private int ItemCount = 0;

        private string RemoteIP = "";

        private int RemotePort = 0;

        private Queue<TCPClient> pool;

  
        private Semaphore semaphoreClients;

    
        public IConnectInfoContainer RootWindow;

        private string ServerName = "";

        public DateTime LastConnectErrorTime;

     
        private Stack<TCPClient> ErrorClientStack = new Stack<TCPClient>();

        public void ChangeIpPort(string ip, int port)
        {
            RemoteIP = ip;
            RemotePort = port;
        }

    
        public bool Init(int count, string ip, int port, string serverName)
        {
            if (null != semaphoreClients)
            {
                LogManager.WriteLog(LogTypes.Error, "不正确的重复调用函数GameDbClientPool.Init(int count, string ip, int port, string serverName)");
                return false;
            }

            ServerName = serverName;

            _InitCount = count;
            ItemCount = count;
            RemoteIP = ip;
            RemotePort = port;
            this.semaphoreClients = new Semaphore(0, count);

            for (int i = 0; i < count; i++)
            {
                TCPClient tcpClient = new TCPClient() { RootWindow = RootWindow, ListIndex = ItemCount, NoDelay = false };
                ErrorClientStack.Push(tcpClient);
                ErrCount++;

                try
                {
                    RootWindow.AddDBConnectInfo(ItemCount, string.Format("{0}, 准备连接到{1}: {2}{3}", ItemCount, ServerName, RemoteIP, RemotePort));
                }
                catch (System.Exception ex)
                {
                }
            }

            return Supply();
        }

        /// <summary>
        /// 删除连接池
        /// </summary>
        public void Clear()
        {
            try
            {
                lock (this.pool)
                {
                    for (int i = 0; i < this.pool.Count; i++)
                    {
                        TCPClient tcpClient = this.pool.ElementAt(i);
                        tcpClient.Disconnect();
                    }

                    this.pool.Clear();
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// 连接池剩余的可用连接个数
        /// </summary>
        public int GetPoolCount()
        {
            lock (this.pool)
            {
                return this.pool.Count;
            }
        }

        /// <summary>
        ///  补充断开的连接
        /// </summary>
        public bool Supply()
        {
            lock (this.pool)
            {
                if (ErrCount <= 0)
                {
                    return true;
                }

                DateTime now = TimeUtil.NowDateTime();
                if ((now -  LastConnectErrorTime).TotalSeconds < 10)
                {
                    return false;
                }

                if (ErrCount > 0)
                {
                    while (ErrorClientStack.Count > 0)
                    {
                        TCPClient tcpClient = ErrorClientStack.Pop();

                        try
                        {
                            tcpClient.Connect(RemoteIP, RemotePort, ServerName);
                            this.pool.Enqueue(tcpClient);
                            ErrCount--;

                            this.semaphoreClients.Release();
                        }
                        catch (Exception)
                        {
                            LastConnectErrorTime = now;
                            ErrorClientStack.Push(tcpClient);
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// 获取一个连接
        /// </summary>
        /// <returns></returns>
        public TCPClient Pop()
        {
            TCPClient tcpClient = null;
            bool needSupply = false;
            lock (this.pool)
            {
                if (ErrCount >= _InitCount)
                {
                    //首先尝试补充连接,如果失败,则返回null
                    needSupply = true;
                    if (!Supply())
                    {
                        return null;
                    }
                }
            }

            if (this.semaphoreClients.WaitOne(20000)) //防止无法获取， 阻塞等待
            {
                lock (this.pool)
                {
                    tcpClient = this.pool.Dequeue();

                    if (!tcpClient.ValidateIpPort(RemoteIP, RemotePort))
                    {
                        try
                        {
                            tcpClient.Disconnect();
                            tcpClient.Connect(RemoteIP, RemotePort, tcpClient.ServerName);
                        }
                        catch (System.Exception ex)
                        {
                            ErrCount++;
                            ErrorClientStack.Push(tcpClient);
                            LastConnectErrorTime = TimeUtil.NowDateTime();
                            LogManager.WriteExceptionUseCache(ex.ToString());
                        }
                    }
                }
            }

            return tcpClient;
        }

        /// <summary>
        /// 压入一个连接
        /// </summary>
        /// <param name="tcpClient"></param>
        /// <returns></returns>
        public void Push(TCPClient tcpClient)
        {
            //如果是已经无效的连接，则不再放入缓存池
            if (!tcpClient.IsConnected())
            {
                lock (this.pool)
                {
                    ErrCount++;
                    ErrorClientStack.Push(tcpClient);
                }

                return;
            }

            lock (this.pool)
            {
                this.pool.Enqueue(tcpClient);
            }

            this.semaphoreClients.Release();
        }
    }
}
