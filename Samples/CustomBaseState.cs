using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StateMechanic;

namespace Samples
{
    public class CustomBaseState : StateBase<CustomBaseState>
    {
        protected override bool CanTransition(IEvent @event, CustomBaseState to)
        {
            // Can abort the transition, like a transition guard
            return true;
        }

        protected override void OnEntry(StateHandlerInfo<CustomBaseState> info)
        {
            // Custom code which isn't in the entry handler 

            base.OnEntry(info);
        }

        protected override void OnExit(StateHandlerInfo<CustomBaseState> info)
        {
            // Custom code which isn't in the exit handler

            base.OnExit(info);
        }

        protected override CustomBaseState HandleEvent(IEvent @event)
        {
            // Can force transition to a particular state, like a dynamic transition

            return null;
        }
    }
}
