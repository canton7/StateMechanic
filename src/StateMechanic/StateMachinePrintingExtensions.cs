using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMechanic
{
    public static class StateMachinePrintingExtensions
    {
        public static string FormatDot(this IStateMachine stateMachine, bool colorize = false)
        {
            var printer = new StateMachineDotPrinter(stateMachine);
            printer.Colorize = colorize;
            return printer.Format();
        }
    }
}
