using GameServer.Logic;

namespace GameServer.Core.GameEvent.EventOjectImpl
{

    public class MonsterDeadEventObject : EventObject
    {
        private Monster monster;
        private KPlayer attacker;

        public MonsterDeadEventObject(Monster monster, KPlayer attacker)
            : base((int)EventTypes.MonsterDead)
        {
            this.monster = monster;
            this.attacker = attacker;
        }

        public Monster getMonster()
        {
            return monster;
        }

        public KPlayer getAttacker()
        {
            return attacker;
        }
    }
}