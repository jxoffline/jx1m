using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KF.Remoting
{
    class ZhengBaStateMachine
    {
        public enum StateType
        {
            None,
            Idle,
            TodayPkStart,
            TodayPkEnd,
            InitPkLoop,
            NotifyEnter,
            PkLoopStart,
            PkLoopEnd,

            Max
        }

        private StateHandler[] Handlers = new StateHandler[(int)StateType.Max];
        private StateType _CurrState = StateType.None;
        private long _CurrStateEnterTicks;

        public StateType GetCurrState()
        {
            return _CurrState;
        }

        public void SetCurrState(StateType state, DateTime now)
        {
            StateHandler oldHandler = Handlers[(int)_CurrState];
            if (oldHandler != null) oldHandler.Leave(now);

            _CurrState = state;
            StateHandler newHandler = Handlers[(int)_CurrState];
            _CurrStateEnterTicks = now.Ticks;
            if (newHandler != null) newHandler.Enter(now);
        }

        public long ContinueTicks(DateTime now)
        {
            return now.Ticks - _CurrStateEnterTicks;
        }

        public void Install(StateHandler handler)
        {
            Handlers[(int)handler.State] = handler;
        }

        public void Tick(DateTime now)
        {
            StateHandler handler = Handlers[(int)_CurrState];
            if (handler != null)
                handler.Update(now);
        }

        public class StateHandler
        {
            private Action<DateTime> enterAction = null;
            private Action<DateTime> updateAction = null;
            private Action<DateTime> leaveAction = null;
            public StateType State { get; private set; }

            public void Enter(DateTime now)
            {
                if (enterAction != null) {enterAction(now);}
            }
            public void Update(DateTime now)
            {
                if (updateAction != null) { updateAction(now); }
            }
            public void Leave(DateTime now)
            {
                if (leaveAction != null)
                {
                    leaveAction(now);
                }
            }

            public StateHandler(StateType state, Action<DateTime> enter, Action<DateTime> updater, Action<DateTime> leaver)
            {
                this.State = state;
                this.enterAction = enter;
                this.updateAction = updater;
                this.leaveAction = leaver;
            }
        }
    }
}
