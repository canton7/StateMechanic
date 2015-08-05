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
                if (from.ChildStateMachine != null && !isInnerTransition)
                {
                    try
                    {
                        from.ChildStateMachine.CurrentState.FireExitHandler(new StateHandlerInfo<TState>(from.ChildStateMachine.CurrentState, to, @event));
                    }
                    catch (Exception e)
                    {
                        throw new InternalTransitionFaultException(from.ChildStateMachine.CurrentState, to, @event, FaultedComponent.ExitHandler, e);
                    }
                    from.ChildStateMachine.SetCurrentState(null);
                }

                try
                {
                    from.FireExitHandler(stateHandlerInfo);
                }
                catch (Exception e)
                {
                    throw new InternalTransitionFaultException(from, to, @event, FaultedComponent.ExitHandler, e);
                }
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
            this.OnTransition(from, to, @event, from.ParentStateMachine, isInnerTransition);

            if (!isInnerTransition)
            {
                try
                {
                    to.FireEntryHandler(stateHandlerInfo);
                }
                catch (Exception e)
                {
                    throw new InternalTransitionFaultException(from, to, @event, FaultedComponent.EntryHandler, e);
                }

                if (to.ChildStateMachine != null && !isInnerTransition)
                {
                    to.ChildStateMachine.SetCurrentState(to.ChildStateMachine.InitialState);

                    try
                    {
                        to.ChildStateMachine.InitialState.FireEntryHandler(new StateHandlerInfo<TState>(from, to.ChildStateMachine.InitialState, @event));
                    }
                    catch (Exception e)
                    {
                        throw new InternalTransitionFaultException(from, to.ChildStateMachine.InitialState, @event, FaultedComponent.EntryHandler, e);
                    }
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
