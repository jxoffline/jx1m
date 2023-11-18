using System;
using System.Net.Sockets;

namespace Server.TCP
{
    /// <summary>
    /// 连接成功通知函数
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void SocketConnectedEventHandler(object sender, SocketAsyncEventArgs e);

    /// <summary>
    /// 断开成功通知函数
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void SocketClosedEventHandler(object sender, SocketAsyncEventArgs e);

    /// <summary>
    /// 接收数据通知函数
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate bool SocketReceivedEventHandler(object sender, SocketAsyncEventArgs e);

    /// <summary>
    /// 发送数据通知函数
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void SocketSendedEventHandler(object sender, SocketAsyncEventArgs e);

    public class AsyncUserToken : IDisposable
    {
        /// <summary>
        /// 释放函数
        /// </summary>
        public void Dispose()
        {
            CurrentSocket = null;
            Tag = null;
        }

        /// <summary>
        /// Socket属性
        /// </summary>
        public Socket CurrentSocket
        {
            get;
            set;
        }

        /// <summary>
        /// 扩展属性
        /// </summary>
        public object Tag
        {
            get;
            set;
        }
    }
}