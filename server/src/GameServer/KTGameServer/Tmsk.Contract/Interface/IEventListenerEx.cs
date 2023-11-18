using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tmsk.Contract
{
    /// <summary>
    /// 事件监听器
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IEventListenerEx
    {
        /// <summary>
        /// 处理事件
        /// </summary>
        /// <param name="eventObject"></param>
        void processEvent(EventObjectEx eventObject);
    }

}
