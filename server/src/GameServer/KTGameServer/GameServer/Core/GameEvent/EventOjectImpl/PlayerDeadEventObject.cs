using GameServer.Logic;

namespace GameServer.Core.GameEvent.EventOjectImpl
{
    public enum PlayerDeadEventTypes
    {
        ByMonster,
        ByRole,
    }


    public class PlayerDeadEventObject : EventObject
    {
        private KPlayer attackerRole;

        private Monster attacker;

        private KPlayer player;

        public PlayerDeadEventTypes Type;

        public PlayerDeadEventObject(KPlayer player, Monster attacker)
            : base((int)EventTypes.PlayerDead)
        {
            this.player = player;
            this.attacker = attacker;
            Type = PlayerDeadEventTypes.ByMonster;
        }

        public PlayerDeadEventObject(KPlayer player, KPlayer attacker)
            : base((int)EventTypes.PlayerDead)
        {
            this.player = player;
            this.attackerRole = attacker;
            Type = PlayerDeadEventTypes.ByRole;
        }

        public Monster getAttacker()
        {
            return attacker;
        }

        public KPlayer getPlayer()
        {
            return player;
        }

        public KPlayer getAttackerRole()
        {
            return attackerRole;
        }
    }
}