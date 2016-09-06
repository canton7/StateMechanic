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
        /// <param name="renderVertical">If true, the state machine is rendered vertically rather than horizontally</param>
        /// <returns>The state machine, described using Graphviz DOT</returns>
        public static string FormatDot(this IStateMachine stateMachine, bool colorize = false, bool renderVertical = false)
        {
            var printer = new StateMachineDotPrinter(stateMachine);
            printer.Colorize = colorize;
            printer.RenderVertical = renderVertical;
            return printer.Format();
        }

        /// <summary>
        /// Return a state machine described using the DGML language, suitable for rending with Visual Studio
        /// </summary>
        /// <param name="stateMachine">State machine to print</param>
        /// <param name="colorize">Whether or not to color states and transitions</param>
        /// <returns>The state machine, described using Graphviz DOT</returns>
        public static string FormatDgml(this IStateMachine stateMachine, bool colorize = false)
        {
            var printer = new StateMachineDgmlPrinter(stateMachine);
            printer.Colorize = colorize;
            return printer.Format();
        }
    }
}
