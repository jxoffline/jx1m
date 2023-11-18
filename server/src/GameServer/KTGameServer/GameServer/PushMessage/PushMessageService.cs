using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameServer.Core.Executor;
using GameServer.Server;
using Server.Protocol;
using Server.Data;
using GameServer.Logic;
using GameServer.Core.GameEvent;
using log4net;
using System.IO;

namespace GameServer.PushMessage
{
    /// <summary>
    /// 推送的消息项
    /// </summary>
    public class PushMessageItem
    {
        /// <summary>
        /// 客户ID列表, 唯一，单个发送，多个，多推，为null, 表示发送给所有app的注册用户
        /// </summary>
        public List<string> ClientIDList;

        /// <summary>
        /// 消息的标题
        /// </summary>
        public string Title;

        /// <summary>
        /// 消息的内容
        /// </summary>
        public string MessageText;
    }

    /// <summary>
    /// 推送消息服务
    /// </summary>
    public class PushMessageService : IManager, ScheduleTask
    {
        #region 唯一的实例

        /// <summary>
        /// 唯一的静态化实例
        /// </summary>
        private static PushMessageService _Instance = new PushMessageService();

        /// <summary>
        /// 得到唯一的静态化实例
        /// </summary>
        /// <returns></returns>
        public static PushMessageService getInstance()
        {
            return _Instance;
        }

        #endregion 唯一的实例

        #region IManager接口实现

        //任务调度器
        private ScheduleExecutor _ScheduleExecutor = null;

        /// <summary>
        /// 周期调度的处理
        /// </summary>
        private PeriodicTaskHandle _PeriodicTaskHandle = null;

        private int DailyPushMsgDayID = -1;

        public bool initialize()
        {
            //分配2个线程
            _ScheduleExecutor = new ScheduleExecutor(2);

            //加入循环任务
            _PeriodicTaskHandle = _ScheduleExecutor.scheduleExecute(this, 0L, 100);

            DailyPushMsgDayID = -1;

            //测试推送消息
            //GetuiServerApiSDK.PushMessageToSingle("cf4a0d426a757a391570280e27eea097", "服务器端测试", "服务器端测试消息是否能够发送");

            return true;
        }

        public bool startup()
        {
            _ScheduleExecutor.start();
            return true;
        }

        public bool showdown()
        {
            _PeriodicTaskHandle.cannel();
            _ScheduleExecutor.stop();
            return true;
        }

        public bool destroy()
        {
            _ScheduleExecutor = null;
            return true;
        }

        public int GetDailyPushMsgDayID()
        {
            return DailyPushMsgDayID;
        }

        public void SetDailyPushMsgDayID(int nDayID)
        {
            DailyPushMsgDayID = nDayID;
        }

        #endregion IManager接口实现

        #region 推送队列

        /// <summary>
        /// 等待推送的消息队列
        /// </summary>
        private Queue<PushMessageItem> PushMessageItemQueue = new Queue<PushMessageItem>();

        /// <summary>
        /// 添加一个要推送的消息项
        /// </summary>
        /// <param name="pushMessageItem"></param>
        public void AddPushMessageItem(PushMessageItem pushMessageItem)
        {
            lock (PushMessageItemQueue)
            {
                PushMessageItemQueue.Enqueue(pushMessageItem);
            }
        }

        /// <summary>
        /// 推动到单个用户消息
        /// </summary>
        /// <param name="cid"></param>
        /// <param name="title"></param>
        /// <param name="messageText"></param>
        public void AddPushMessageText(string cid, string title, string messageText)
        {
            PushMessageItem pushMessageItem = new PushMessageItem()
            {
                ClientIDList = new List<string>(),
                Title = title,
                MessageText = messageText,
            };

            pushMessageItem.ClientIDList.Add(cid);

            AddPushMessageItem(pushMessageItem);
        }

        /// <summary>
        /// 推动到多个用户消息
        /// </summary>
        /// <param name="cid"></param>
        /// <param name="title"></param>
        /// <param name="messageText"></param>
        public void AddPushMessageText(List<string> cidList, string title, string messageText)
        {
            PushMessageItem pushMessageItem = new PushMessageItem()
            {
                ClientIDList = cidList,
                Title = title,
                MessageText = messageText,
            };

            AddPushMessageItem(pushMessageItem);
        }

        #endregion 推送队列

        #region ScheduleTask接口实现

        /// <summary>
        /// 定时调度
        /// </summary>
        public void run()
        {
            ProcessQueue();
        }

        #endregion ScheduleTask接口实现

        #region 线程驱动

        /// <summary>
        /// 每次推送的用户ID的最大个数
        /// </summary>
        private const int MaxNumPerList = 100;

        /// <summary>
        /// 线程驱动
        /// </summary>
        public void ProcessQueue()
        {
            PushMessageItem pushMessageItem = null;
            lock (PushMessageItemQueue)
            {
                while (PushMessageItemQueue.Count > 0)
                {
                    pushMessageItem = PushMessageItemQueue.Dequeue();
                    if (null != pushMessageItem)
                    {
                        if (null == pushMessageItem.ClientIDList)
                        {
                            GetuiServerApiSDK.PushMessageToApp(pushMessageItem.Title, pushMessageItem.MessageText);
                        }
                        else if (pushMessageItem.ClientIDList.Count <= 1)
                        {
                            GetuiServerApiSDK.PushMessageToSingle(pushMessageItem.ClientIDList[0], pushMessageItem.Title, pushMessageItem.MessageText);
                        }
                        else
                        {
                            for (int i = 0; i < pushMessageItem.ClientIDList.Count;)
                            {
                                List<string> cidList = pushMessageItem.ClientIDList.GetRange(i, Math.Min(pushMessageItem.ClientIDList.Count - i, MaxNumPerList));
                                GetuiServerApiSDK.PushMessageToList(cidList, pushMessageItem.Title, pushMessageItem.MessageText);

                                i += MaxNumPerList;
                            }
                        }
                    }
                }                
            }
        }

        /// <summary>
        /// 定时推送某些消息给用户
        /// </summary>
        private void ProcessTimeslotNotify()
        {

        }

        #endregion 线程驱动
    }
}
