using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tmsk.Contract
{
    public interface ISceneEventSource
    {
        void registerListener(int eventType, int sceneType, IEventListenerEx listener);

        void removeListener(int eventType, int sceneType, IEventListenerEx listener);

        bool fireEvent(EventObjectEx eventObj, int sceneType);

        void dispatchEvent(EventObjectEx eventObj, List<IEventListenerEx> listenerList);
    }
}
