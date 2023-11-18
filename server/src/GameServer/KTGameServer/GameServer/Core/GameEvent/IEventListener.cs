namespace GameServer.Core.GameEvent
{

    public interface IEventListener
    {
        void processEvent(EventObject eventObject);
    }
}