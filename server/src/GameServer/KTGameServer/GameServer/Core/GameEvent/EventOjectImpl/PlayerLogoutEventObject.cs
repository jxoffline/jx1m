using GameServer.Logic;

namespace GameServer.Core.GameEvent.EventOjectImpl
{

    public class PlayerLogoutEventObject : EventObject
    {
        private KPlayer player;

        public PlayerLogoutEventObject(KPlayer player)
            : base((int)EventTypes.PlayerLogout)
        {
            this.player = player;
        }

    }
}