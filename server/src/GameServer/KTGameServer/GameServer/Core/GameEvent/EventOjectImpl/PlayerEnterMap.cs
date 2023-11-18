using GameServer.Logic;

namespace GameServer.Core.GameEvent.EventOjectImpl
{

    public class PlayerEnterMap : EventObject
    {
        public KPlayer Client { get; private set; }
        public int MapId { get; private set; }


        public PlayerEnterMap(KPlayer client, int _MapId)
            : base((int)EventTypes.PlayEnterMap)
        {
            Client = client;
            MapId = _MapId;

        }
    }
}
