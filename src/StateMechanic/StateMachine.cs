using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMechanic
{
    public class StateMachine : SubStateMachine
    {
        public StateMachineFaultInfo Fault { get { return this.InnerStateMachine.Kernel.Fault; } }
        public bool IsFaulted { get { return this.Fault != null; } }
        public IStateMachineSynchronizer Synchronizer
        {
            get { return this.InnerStateMachine.Kernel.Synchronizer; }
            set { this.InnerStateMachine.Kernel.Synchronizer = value; }
        }
        public event EventHandler<StateMachineFaultedEventArgs> Faulted
        {
            add { this.InnerStateMachine.Kernel.Faulted += value; }
            remove { this.InnerStateMachine.Kernel.Faulted -= value; }
        }
        public event EventHandler<TransitionEventArgs<State>> GlobalTransition
        {
            add { this.InnerStateMachine.Kernel.Transition += value; }
            remove { this.InnerStateMachine.Kernel.Transition -= value; }
        }
        public event EventHandler<TransitionNotFoundEventArgs<State>> GlobalTransitionNotFound
        {
            add { this.InnerStateMachine.Kernel.TransitionNotFound += value; }
            remove { this.InnerStateMachine.Kernel.TransitionNotFound -= value; }
        }

        public StateMachine(string name)
            : base(name, new StateMachineKernel<State>(), null)
        {
        }

        public void Reset()
        {
            if (this.InnerStateMachine.Kernel.Synchronizer != null)
            {
                this.InnerStateMachine.Kernel.Synchronizer.Reset(this.Reset);
                return;
            }

            this.InnerStateMachine.Kernel.Reset();
            this.InnerStateMachine.Reset();
        }
    }

    public class StateMachine<TStateData> : SubStateMachine<TStateData>
    {
        public StateMachineFaultInfo Fault { get { return this.InnerStateMachine.Kernel.Fault; } }
        public bool IsFaulted { get { return this.Fault != null; } }
        public IStateMachineSynchronizer Synchronizer
        {
            get { return this.InnerStateMachine.Kernel.Synchronizer; }
            set { this.InnerStateMachine.Kernel.Synchronizer = value; }
        }
        public event EventHandler<StateMachineFaultedEventArgs> Faulted
        {
            add { this.InnerStateMachine.Kernel.Faulted += value; }
            remove { this.InnerStateMachine.Kernel.Faulted -= value; }
        }
        public event EventHandler<TransitionEventArgs<State<TStateData>>> GlobalTransition
        {
            add { this.InnerStateMachine.Kernel.Transition += value; }
            remove { this.InnerStateMachine.Kernel.Transition -= value; }
        }
        public event EventHandler<TransitionNotFoundEventArgs<State<TStateData>>> GlobalTransitionNotFound
        {
            add { this.InnerStateMachine.Kernel.TransitionNotFound += value; }
            remove { this.InnerStateMachine.Kernel.TransitionNotFound -= value; }
        }

        public StateMachine(string name)
            : base(name, new StateMachineKernel<State<TStateData>>(), null)
        {
        }

        public void Reset()
        {
            if (this.InnerStateMachine.Kernel.Synchronizer != null)
            {
                this.InnerStateMachine.Kernel.Synchronizer.Reset(this.Reset);
                return;
            }

            this.InnerStateMachine.Kernel.Reset();
            this.InnerStateMachine.Reset();
        }
    }
}
