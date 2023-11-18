using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tmsk.Contract
{
    /// <summary>
    /// 最基础的事件对象，请在Core/GameEvent/EventObjectImpl派生自己的需要的 XXXXXBaseEventObject 事件对象，和自己逻辑紧密相关的，再在自己逻辑内进一步派生，这样可以做到让其他程序员马上可以理解
    /// 系统内有多少事件存在，有哪些事件对象可用
    /// </summary>
    public abstract class EventObjectEx
    {
        public int EventType { get; protected set; }
        public bool Handled = false;
        public bool Result = false;

        protected EventObjectEx(int eventType)
        {
            EventType = eventType;
        }
    }
}
