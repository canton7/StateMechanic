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

        public void CoordinateTransition<TTransitionInfo>(TState from, TState to, IEvent @event, bool isInnerTransition, Action<TTransitionInfo> handler, TTransitionInfo transitionInfo)
        {
            // We require that from.ParentStateMachine.Kernel == to.ParentStateMachine.Kernel == this

            var stateHandlerInfo = new StateHandlerInfo<TState>(from, to, @event, isInnerTransition);

            if (!isInnerTransition)
            {
                if (from.ChildStateMachine != null)
                    this.ExitChildStateMachine(from.ChildStateMachine, to, @event, isInnerTransition);

                this.ExitState(stateHandlerInfo);
            }

            if (handler != null)
            {
                try
                {
                    handler(transitionInfo);
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
                    this.EnterChildStateMachine(to.ChildStateMachine, from, @event, isInnerTransition);
            }

            this.OnTransition(from, to, @event, from.ParentStateMachine, isInnerTransition);
        }

        private void ExitChildStateMachine(IStateMachine<TState> childStateMachine, TState to, IEvent @event, bool isInnerTransition)
        {
            if (childStateMachine.CurrentState != null && childStateMachine.CurrentState.ChildStateMachine != null)
                this.ExitChildStateMachine(childStateMachine.CurrentState.ChildStateMachine, to, @event, isInnerTransition);

            this.ExitState(new StateHandlerInfo<TState>(childStateMachine.CurrentState, to, @event, isInnerTransition));

            childStateMachine.SetCurrentState(null);
        }

        private void EnterChildStateMachine(IStateMachine<TState> childStateMachine, TState from, IEvent @event, bool isInnerTransition)
        {
            childStateMachine.SetCurrentState(childStateMachine.InitialState);

            this.EnterState(new StateHandlerInfo<TState>(from, childStateMachine.InitialState, @event, isInnerTransition));

            if (childStateMachine.InitialState.ChildStateMachine != null)
                this.EnterChildStateMachine(childStateMachine.InitialState.ChildStateMachine, from, @event, isInnerTransition);
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

        public void EnqueueTransition(Func<ITransitionInvoker<TState>, bool> method, ITransitionInvoker<TState> invoker)
        {
            this.transitionQueue.Enqueue(new TransitionQueueItem(method, invoker));
        }

        public void FireQueuedTransitions()
        {
            while (this.transitionQueue.Count > 0)
            {
                // TODO: Not sure whether these failing should affect the status of the outer parent transition...
                // Current behaviour is that Fire will, but TryFire will not
                var item = this.transitionQueue.Dequeue();
                item.method(item.transitionInvoker);
            }
        }

        public void OnTransition(TState from, TState to, IEvent @event, IStateMachine stateMachine, bool isInnerTransition)
        {
            this.Transition?.Invoke(this, new TransitionEventArgs<TState>(from, to, @event, stateMachine, isInnerTransition));
        }

        public void HandleTransitionNotFound(TState fromState, IEvent @event, IStateMachine stateMachine, bool throwsException)
        {
            if (throwsException)
                this.transitionQueue.Clear();
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
            public readonly Func<ITransitionInvoker<TState>, bool> method;
            public readonly ITransitionInvoker<TState> transitionInvoker;

            public TransitionQueueItem(Func<ITransitionInvoker<TState>, bool> method, ITransitionInvoker<TState> transitionInvoker)
            {
                this.method = method;
                this.transitionInvoker = transitionInvoker;
            }
        }
    }
}
