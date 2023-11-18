using Server.Tools;
using System.Collections.Generic;

namespace GameServer.Core.GameEvent
{

    public abstract class EventSource
    {

        protected Dictionary<int, List<IEventListener>> listeners = new Dictionary<int, List<IEventListener>>();

        public void registerListener(int eventType, IEventListener listener)
        {
            lock (listeners)
            {
                List<IEventListener> listenerList = null;
                if (!listeners.TryGetValue(eventType, out listenerList))
                {
                    listenerList = new List<IEventListener>();
                    listeners.Add(eventType, listenerList);
                }
                listenerList.Add(listener);
            }
        }

        public void removeListener(int eventType, IEventListener listener)
        {
            lock (listeners)
            {
                List<IEventListener> listenerList = null;
                if (!listeners.TryGetValue(eventType, out listenerList))
                {
                    return;
                }
                listenerList.Remove(listener);
            }
        }

        public void fireEvent(EventObject eventObj)
        {
            if (null == eventObj || eventObj.getEventType() == -1)
            {
                return;
            }

            List<IEventListener> copylistenerList = null;
            List<IEventListener> listenerList = null;

            lock (listeners)
            {
                if (!listeners.TryGetValue(eventObj.getEventType(), out listenerList))
                {
                    return;
                }

                copylistenerList = listenerList.GetRange(0, listenerList.Count);
            }

            dispatchEvent(eventObj, copylistenerList);
        }

        private void dispatchEvent(EventObject eventObj, List<IEventListener> listenerList)
        {
            foreach (IEventListener listener in listenerList)
            {
                try
                {
                    listener.processEvent(eventObj);
                }
                catch (System.Exception ex)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("dispatchEvent: {0},{1}", (EventTypes)eventObj.getEventType(), ex));
                }
            }
        }
    }
}