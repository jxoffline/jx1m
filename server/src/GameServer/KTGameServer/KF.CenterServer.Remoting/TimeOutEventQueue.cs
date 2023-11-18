using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KF.Contract.Data;

namespace KF.Remoting
{
    public class TimeOutEventBlock<T>
    {
        public DateTime EndTime;
        public List<T> ChildList = new List<T>();
    }

    public class TimeOutEventQueue<T>
    {
        private object Mutex = new object();

        private LinkedList<TimeOutEventBlock<T>> ShengBeiBufferTimeListQueue = new LinkedList<TimeOutEventBlock<T>>();

        public void EnqueueTimeOutEventItem(T item, DateTime endTime)
        {
            lock (Mutex)
            {
                TimeOutEventBlock<T> timeOutEventBlock = ShengBeiBufferTimeListQueue.First.Value;

                if (endTime.Ticks - timeOutEventBlock.EndTime.Ticks >= 1 * 10000000) //精度1秒
                {
                    timeOutEventBlock = new TimeOutEventBlock<T>();
                    timeOutEventBlock.EndTime = endTime;
                    ShengBeiBufferTimeListQueue.AddFirst(timeOutEventBlock);
                }

                timeOutEventBlock.ChildList.Add(item);
            }
        }

        public bool DequeueTimeOutEventItem(List<T> outputList, DateTime now)
        {
            bool result = false;

            lock (Mutex)
            {
                LinkedListNode<TimeOutEventBlock<T>> node = ShengBeiBufferTimeListQueue.Last;
                while (node != null)
                {
                    if (node.Value.EndTime > now)
                    {
                        break;
                    }

                    if (!result)
                    {
                        result = true;
                    }

                    outputList.AddRange(node.Value.ChildList);
                    ShengBeiBufferTimeListQueue.RemoveLast();
                    node = node.Previous;
                }
            }

            return result;
        }


    }
}
