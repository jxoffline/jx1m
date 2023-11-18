using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using Server.Protocol;

namespace Server.TCP
{
    /// <summary>
    /// 连接成功通知函数
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void SocketConnectedEvnetHandler(object sender, SocketAsyncEventArgs e);

    /// <summary>
    /// 断开成功通知函数
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void SocketClosedEventHandler(object sender, TMSKSocket s);

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
        /// TMSKSocket属性
        /// </summary>
        public TMSKSocket CurrentSocket
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

        public SendBuffer _SendBuffer;
    }
}
