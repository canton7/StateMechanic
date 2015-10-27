using System;
using System.Collections.Generic;
using System.Linq;

namespace StateMechanic
{
    internal class StateMachineKernel<TState> : ITransitionDelegate<TState> where TState : class, IState<TState>
    {
        private readonly Queue<TransitionQueueItem> transitionQueue = new Queue<TransitionQueueItem>();
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

                this.ExitState(stateHandlerInfo);
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
                this.EnterState(stateHandlerInfo);

                if (to.ChildStateMachine != null)
                    this.EnterChildStateMachine(to.ChildStateMachine, from, @event);
            }

            this.OnTransition(from, to, @event, from.ParentStateMachine, isInnerTransition);
        }

        private void ExitChildStateMachine(IStateMachine<TState> childStateMachine, TState to, IEvent @event)
        {
            if (childStateMachine.CurrentState != null && childStateMachine.CurrentState.ChildStateMachine != null)
                this.ExitChildStateMachine(childStateMachine.CurrentState.ChildStateMachine, to, @event);

            this.ExitState(new StateHandlerInfo<TState>(childStateMachine.CurrentState, to, @event));

            childStateMachine.SetCurrentState(null);
        }

        private void EnterChildStateMachine(IStateMachine<TState> childStateMachine, TState from, IEvent @event)
        {
            childStateMachine.SetCurrentState(childStateMachine.InitialState);

            this.EnterState(new StateHandlerInfo<TState>(from, childStateMachine.InitialState, @event));

            if (childStateMachine.InitialState.ChildStateMachine != null)
                this.EnterChildStateMachine(childStateMachine.InitialState.ChildStateMachine, from, @event);
        }

        private void ExitState(StateHandlerInfo<TState> info)
        {
            try
            {
                info.From.FireExitHandler(info);
            }
            catch (Exception e)
            {
                throw new InternalTransitionFaultException(info.From, info.To, info.Event, FaultedComponent.ExitHandler, e);
            }

            foreach (var group in info.From.Groups.Reverse())
            {
                // We could use .Except, but that uses a HashSet which is complete overkill here
                if (info.To.Groups.Contains(group))
                    continue;

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

        private void EnterState(StateHandlerInfo<TState> info)
        {
            try
            {
                info.To.FireEntryHandler(info);
            }
            catch (Exception e)
            {
                throw new InternalTransitionFaultException(info.From, info.To, info.Event, FaultedComponent.EntryHandler, e);
            }

            foreach (var group in info.To.Groups)
            {
                // We could use .Except, but that uses a HashSet which is complete overkill here
                if (info.From.Groups.Contains(group))
                    continue;

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

        public void EnqueueTransition(Func<EventFireData, bool> method, EventFireData eventFireData)
        {
            this.transitionQueue.Enqueue(new TransitionQueueItem(method, eventFireData));
        }

        public void FireQueuedTransitions()
        {
            while (this.transitionQueue.Count > 0)
            {
                // TODO: Not sure whether these failing should affect the status of the outer parent transition...
                var item = this.transitionQueue.Dequeue();
                item.Method(item.EventFireData);
            }
        }

        public void OnTransition(TState from, TState to, IEvent @event, IStateMachine stateMachine, bool isInnerTransition)
        {
            this.Transition?.Invoke(this, new TransitionEventArgs<TState>(from, to, @event, stateMachine, isInnerTransition));
        }

        public void OnTransitionNotFound(TState fromState, IEvent @event, IStateMachine stateMachine)
        {
            this.TransitionNotFound?.Invoke(this, new TransitionNotFoundEventArgs<TState>(fromState, @event, stateMachine));
        }

        public void SetFault(StateMachineFaultInfo faultInfo)
        {
            this.Fault = faultInfo;
            this.transitionQueue.Clear();

            this.Faulted?.Invoke(this, new StateMachineFaultedEventArgs(faultInfo));
        }

        public void Reset()
        {
            this.Fault = null;
        }

        private struct TransitionQueueItem
        {
            public readonly Func<EventFireData, bool> Method;
            public readonly EventFireData EventFireData;

            public TransitionQueueItem(Func<EventFireData, bool> method, EventFireData eventFireData)
            {
                this.Method = method;
                this.EventFireData = eventFireData;
            }
        }
    }
}
