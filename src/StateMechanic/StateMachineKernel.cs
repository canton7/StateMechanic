using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMechanic
{
    internal class StateMachineKernel<TState>
    {
        private readonly Queue<Func<bool>> eventQueue = new Queue<Func<bool>>();
        public IStateMachineSynchronizer Synchronizer { get; set; }
        public StateMachineFaultInfo Fault { get; private set; }
        public bool ExecutingTransition { get; set; }
        public event EventHandler<StateMachineFaultedEventArgs> Faulted;
        public event EventHandler<TransitionEventArgs<TState>> Transition;
        public event EventHandler<TransitionNotFoundEventArgs<TState>> TransitionNotFound;

        public void EnqueueEventFire(Func<bool> invoker)
        {
            this.eventQueue.Enqueue(invoker);
        }

        public void FireQueuedEvents()
        {
            while (this.eventQueue.Count > 0)
            {
                // TODO: Not sure whether these failing should affect the status of the outer parent transition...
                this.eventQueue.Dequeue()();
            }
        }

        public void OnTransition(TState from, TState to, IEvent evt, bool isInnerTransition)
        {
            var handler = this.Transition;
            if (handler != null)
                handler(this, new TransitionEventArgs<TState>(from, to, evt, isInnerTransition));
        }

        public void OnTransitionNotFound(TState fromState, IEvent evt)
        {
            var handler = this.TransitionNotFound;
            if (handler != null)
                handler(this, new TransitionNotFoundEventArgs<TState>(fromState, evt));
        }

        public void SetFault(StateMachineFaultInfo faultInfo)
        {
            this.Fault = faultInfo;
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
