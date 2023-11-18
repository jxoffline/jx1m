using Server.Tools;
using System.Collections.Generic;
using Tmsk.Contract;

namespace GameServer.Core.GameEvent
{
    public abstract class SceneEventSource : ISceneEventSource
    {
        protected Dictionary<int, Dictionary<int, List<IEventListenerEx>>> Event2Scenelisteners = new Dictionary<int, Dictionary<int, List<IEventListenerEx>>>();

        public void registerListener(int eventType, int sceneType, IEventListenerEx listener)
        {
            lock (Event2Scenelisteners)
            {
                Dictionary<int, List<IEventListenerEx>> dict;
                if (!Event2Scenelisteners.TryGetValue(eventType, out dict))
                {
                    dict = new Dictionary<int, List<IEventListenerEx>>();
                    Event2Scenelisteners.Add(eventType, dict);
                }

                List<IEventListenerEx> listenerList = null;
                if (!dict.TryGetValue(sceneType, out listenerList))
                {
                    listenerList = new List<IEventListenerEx>();
                    dict.Add(sceneType, listenerList);
                }

                listenerList.Add(listener);
            }
        }

        public void removeListener(int eventType, int sceneType, IEventListenerEx listener)
        {
            lock (Event2Scenelisteners)
            {
                Dictionary<int, List<IEventListenerEx>> dict;
                if (!Event2Scenelisteners.TryGetValue(eventType, out dict))
                {
                    dict = new Dictionary<int, List<IEventListenerEx>>();
                    Event2Scenelisteners.Add(eventType, dict);
                }

                List<IEventListenerEx> listenerList = null;
                if (!dict.TryGetValue(sceneType, out listenerList))
                {
                    return;
                }
                listenerList.Remove(listener);
            }
        }

        public bool fireEvent(EventObjectEx eventObj, int sceneType)
        {
            int eventType;
            if (null == eventObj || (eventType = eventObj.EventType) == -1)
            {
                return eventObj.Result;
            }

            List<IEventListenerEx> copylistenerList = null;
            List<IEventListenerEx> listenerList = null;

            lock (Event2Scenelisteners)
            {
                Dictionary<int, List<IEventListenerEx>> dict;
                if (!Event2Scenelisteners.TryGetValue(eventType, out dict))
                {
                    return eventObj.Result;
                }

                if (!dict.TryGetValue(sceneType, out listenerList))
                {
                    return eventObj.Result;
                }

                copylistenerList = listenerList.GetRange(0, listenerList.Count);
            }

            dispatchEvent(eventObj, copylistenerList);
            return eventObj.Result;
        }

        public void dispatchEvent(EventObjectEx eventObj, List<IEventListenerEx> listenerList)
        {
            foreach (IEventListenerEx listener in listenerList)
            {
                try
                {
                    listener.processEvent(eventObj);
                    if (eventObj.Handled)
                    {
                        break;
                    }
                }
                catch (System.Exception ex)
                {
                    LogManager.WriteLog(LogTypes.Error, string.Format("dispatchEvent: {0},{1}", (EventTypes)eventObj.EventType, ex.ToString()));
                }
            }
        }
    }
}