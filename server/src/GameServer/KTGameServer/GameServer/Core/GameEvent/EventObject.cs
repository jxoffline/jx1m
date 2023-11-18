namespace GameServer.Core.GameEvent
{

    public abstract class EventObject
    {
        protected int eventType = -1;

        protected EventObject(int eventType)
        {
            this.eventType = eventType;
        }

        public int getEventType()
        {
            return eventType;
        }
    }
}