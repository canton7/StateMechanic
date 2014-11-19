using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMechanic
{
    internal class TransitionInvoker
    {
        private readonly Func<bool> canInvoke;
        private readonly Action invoke;

        public TransitionInvoker(Func<bool> canInvoke, Action invoke)
        {
            this.canInvoke = canInvoke;
            this.invoke = invoke;
        }

        public bool CanInvoke()
        {
            return this.canInvoke();
        }

        public void Invoke()
        {
            this.invoke();
        }
    }
}
