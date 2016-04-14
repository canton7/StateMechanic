namespace StateMechanic
{
    /// <summary>
    /// Extension methods which allow a state machine to be pritned
    /// </summary>
    public static class StateMachinePrintingExtensions
    {
        /// <summary>
        /// Return a state machine described using the Graphviz DOT language, suitable for rending with the dot utility
        /// </summary>
        /// <param name="stateMachine">State machine to print</param>
        /// <param name="colorize">Whether or not to color states and transitions</param>
        /// <returns>The state machine, described using Graphviz DOT</returns>
        public static string FormatDot(this IStateMachine stateMachine, bool colorize = false)
        {
            var printer = new StateMachineDotPrinter(stateMachine);
            printer.Colorize = colorize;
            return printer.Format();
        }
    }
}
