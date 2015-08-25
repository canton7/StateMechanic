using System;
using System.Collections.Generic;

namespace StateMechanic
{
    internal class StateMachineKernel<TState> : ITransitionDelegate<TState> where TState : class, IState<TState>
    {
        private readonly Queue<Func<bool>> transitionQueue = new Queue<Func<bool>>();
        public IStateMachineSynchronizer Synchronizer { get; set; }
        public StateMachineFaultInfo Fault { get; private set; }
        public bool ExecutingTransition { get; set; }
        public event EventHandler<StateMachineFaultedEventArgs> Faulted;
        public event EventHandler<TransitionEventArgs<TState>> Transition;
        public event EventHandler<TransitionNotFoundEventArgs<TState>> TransitionNotFound;

        public void CoordinateTransition(TState from, TState to, IEvent @event, bool isInnerTransition, Action handlerInvoker)
        {
            // We require that from.ParentStateMachine.Kernel == to.ParentStateMachine.Kernel == this

            var stateHandlerInfo = new StateHandlerInfo<TState>(from, to, @event);

            if (!isInnerTransition)
            {
                if (from.ChildStateMachine != null)
                    this.ExitChildStateMachine(from.ChildStateMachine, to, @event);

                this.ExitState(from, stateHandlerInfo);
            }

            if (handlerInvoker != null)
            {
                try
                {
                    handlerInvoker();
                }
                catch (Exception e)
                {
                    throw new InternalTransitionFaultException(from, to, @event, FaultedComponent.TransitionHandler, e);
                }
            }

            from.ParentStateMachine.SetCurrentState(to);

            if (!isInnerTransition)
            {
                this.EnterState(to, stateHandlerInfo);

                if (to.ChildStateMachine != null)
                    this.EnterChildStateMachine(to.ChildStateMachine, from, @event);
            }

            this.OnTransition(from, to, @event, from.ParentStateMachine, isInnerTransition);
        }

        private void ExitChildStateMachine(IStateMachine<TState> childStateMachine, TState to, IEvent @event)
        {
            if (childStateMachine.CurrentState != null && childStateMachine.CurrentState.ChildStateMachine != null)
            {
                this.ExitChildStateMachine(childStateMachine.CurrentState.ChildStateMachine, to, @event);
            }

            this.ExitState(childStateMachine.CurrentState, new StateHandlerInfo<TState>(childStateMachine.CurrentState, to, @event));

            childStateMachine.SetCurrentState(null);
        }

        private void EnterChildStateMachine(IStateMachine<TState> childStateMachine, TState from, IEvent @event)
        {
            childStateMachine.SetCurrentState(childStateMachine.InitialState);

            this.EnterState(childStateMachine.InitialState, new StateHandlerInfo<TState>(from, childStateMachine.InitialState, @event));

            if (childStateMachine.InitialState.ChildStateMachine != null)
            {
                this.EnterChildStateMachine(childStateMachine.InitialState.ChildStateMachine, from, @event);
            }
        }

        private void ExitState(TState state, StateHandlerInfo<TState> info)
        {
            try
            {
                state.FireExitHandler(info);
            }
            catch (Exception e)
            {
                throw new InternalTransitionFaultException(info.From, info.To, info.Event, FaultedComponent.ExitHandler, e);
            }

            foreach (var group in state.Groups)
            {
                try
                {
                    group.FireExitHandler(info);
                }
                catch (Exception e)
                {
                    throw new InternalTransitionFaultException(info.From, info.To, info.Event, FaultedComponent.GroupExitHandler, e, group);
                }
            }
        }

        private void EnterState(TState state, StateHandlerInfo<TState> info)
        {
            try
            {
                state.FireEntryHandler(info);
            }
            catch (Exception e)
            {
                throw new InternalTransitionFaultException(info.From, info.To, info.Event, FaultedComponent.EntryHandler, e);
            }

            foreach (var group in state.Groups)
            {
                try
                {
                    group.FireEntryHandler(info);
                }
                catch (Exception e)
                {
                    throw new InternalTransitionFaultException(info.From, info.To, info.Event, FaultedComponent.GroupEntryHandler, e, group);
                }
            }
        }

        public void EnqueueTransition(Func<bool> invoker)
        {
            this.transitionQueue.Enqueue(invoker);
        }

        public void FireQueuedTransitions()
        {
            while (this.transitionQueue.Count > 0)
            {
                // TODO: Not sure whether these failing should affect the status of the outer parent transition...
                this.transitionQueue.Dequeue()();
            }
        }

        public void OnTransition(TState from, TState to, IEvent @event, IStateMachine stateMachine, bool isInnerTransition)
        {
            var handler = this.Transition;
            if (handler != null)
                handler(this, new TransitionEventArgs<TState>(from, to, @event, stateMachine, isInnerTransition));
        }

        public void OnTransitionNotFound(TState fromState, IEvent @event, IStateMachine stateMachine)
        {
            var handler = this.TransitionNotFound;
            if (handler != null)
                handler(this, new TransitionNotFoundEventArgs<TState>(fromState, @event, stateMachine));
        }

        public void SetFault(StateMachineFaultInfo faultInfo)
        {
            this.Fault = faultInfo;
            this.transitionQueue.Clear();

            var handler = this.Faulted;
            if (handler != null)
                handler(this, new StateMachineFaultedEventArgs(faultInfo));
        }

        public void Reset()
        {
            this.Fault = null;
        }
    }
}
