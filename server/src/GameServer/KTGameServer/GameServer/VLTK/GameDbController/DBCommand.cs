using GameServer.Server;
using System;

namespace GameServer.Logic
{
    /// <summary>
    /// 数据库命令执行事件参数
    /// </summary>
    public class DBCommandEventArgs : EventArgs
    {
        /// <summary>
        /// 执行结果
        /// </summary>
        public TCPProcessCmdResults Result;

        /// <summary>
        /// 网络通讯字段
        /// </summary>
        public string[] fields = null;
    }

    /// <summary>
    /// 数据库命令执行事件
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void DBCommandEventHandler(object sender, DBCommandEventArgs e);

    /// <summary>
    /// 数据库命令
    /// </summary>
    public class DBCommand
    {
        /// <summary>
        /// 目标服务器ID
        /// </summary>
        public int ServerId;

        /// <summary>
        /// 命令ID
        /// </summary>
        public int DBCommandID
        {
            get;
            set;
        }

        /// <summary>
        /// 命令字符串
        /// </summary>
        public string DBCommandText
        {
            get;
            set;
        }

        /// <summary>
        /// 命令执行完毕回调通知事件
        /// </summary>
        public event DBCommandEventHandler DBCommandEvent;

        //执行事件
        public void DoDBCommandEvent(DBCommandEventArgs e)
        {
            if (null != DBCommandEvent)
            {
                DBCommandEvent(this, e);
            }
        }
    }
}