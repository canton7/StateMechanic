using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMechanic
{
    /// <summary>
    /// Class which can take a state machine, and output graphviz which cab be rendered using dot, allowing the state machine to be visualised
    /// </summary>
    public class StateMachineDotPrinter
    {
        private static readonly string[] colors = new[] 
        {
            "#0075dc", "#993f00", "#4c005c", "#191919", "#005c31", "#2bce48", "#808080", "#8f7c00", "#c20088", "#ffa405", "#ffa8bb",
            "#426600", "#ff0010", "#00998f", "#740aff", "#990000", "#ff5005", "#4d4d4d", "#5da5da", "#faa43a", "#60bd68", "#f17cb0",
            "#b2912f", "#b276b2", "#f15854"
        };

        private readonly IStateMachine stateMachine;

        private Dictionary<IState, string> stateToColorMapping = new Dictionary<IState, string>();
        private int colorUseCount = 0;

        /// <summary>
        /// Gets or sets a value indicating whether colors should be used
        /// </summary>
        public bool Colorize { get; set; }

        /// <summary>
        /// Initialises a new instance of the <see cref="StateMachineDotPrinter"/> class
        /// </summary>
        /// <param name="stateMachine">State machine to print</param>
        public StateMachineDotPrinter(IStateMachine stateMachine)
        {
            this.stateMachine = stateMachine;
        }

        /// <summary>
        /// Generate graphviz allowing the state machine to be rendered using dot
        /// </summary>
        /// <returns>graphviz allowing the state machine to be rendered using dot</returns>
        public string Format()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("digraph \"{0}\" {{\n", this.stateMachine.Name);
            sb.AppendFormat("   label=\"{0}\";\n", this.stateMachine.Name);
            sb.Append("   rankdir=LR;\n");
            sb.Append("   edge [penwidth=2.0];\n");
            sb.Append("   node [shape=circle width=1 penwidth=2.0];\n");

            RenderStateMachine(sb, this.stateMachine, "   ");

            sb.AppendFormat("}}");
            return sb.ToString();
        }

        private string ColorForState(IState state)
        {
            string color;
            if (this.stateToColorMapping.TryGetValue(state, out color))
                return color;

            color = colors[this.colorUseCount];
            this.colorUseCount = (this.colorUseCount + 1) % colors.Length;
            this.stateToColorMapping[state] = color;

            return color;
        }

        private void RenderStateMachine(StringBuilder sb, IStateMachine stateMachine, string indent)
        {
            sb.AppendFormat("{0}compound=true;\n", indent);

            // States
            sb.AppendFormat("{0}\"{1}\" [shape=doublecircle width=1 penwidth=2.0];\n", indent, stateMachine.InitialState.Name);
            foreach (var state in stateMachine.States.Except(new[] { stateMachine.InitialState }))
            {
                // If it has a child state machine, we'll link to/from it differently
                if (state.ChildStateMachine == null)
                {
                    if (this.Colorize)
                        sb.AppendFormat("{0}\"{1}\" [color=\"{2}\"];\n", indent, state.Name, this.ColorForState(state));
                }
                else
                {
                    sb.AppendFormat("{0}subgraph \"cluster_{1}\" {{\n", indent, state.Name);
                    sb.AppendFormat("{0}   label=\"{1} / {2}\";\n", indent, state.Name, state.ChildStateMachine.Name);
                    if (this.Colorize)
                        sb.AppendFormat("{0}   color=\"{1}\";\n{0}   fontcolor=\"{1}\";\n", indent, this.ColorForState(state));
                    RenderStateMachine(sb, state.ChildStateMachine, indent + "   ");
                    sb.AppendFormat("{0}}}\n", indent);
                }
            }

            // Transitions
            foreach (var state in stateMachine.States)
            {
                foreach (var transition in state.Transitions)
                {
                    // If the source has a child state machine, then lhead is the name of that
                    // Likewise dest and ltail
                    sb.AppendFormat("{0}\"{1}\" -> \"{2}\" [label=\"{3}\"{4}{5}{6}];\n",
                        indent,
                        transition.From.ChildStateMachine == null ? transition.From.Name : transition.From.ChildStateMachine.InitialState.Name,
                        transition.To.ChildStateMachine == null ? transition.To.Name : transition.To.ChildStateMachine.InitialState.Name,
                        transition.Event.Name,
                        this.Colorize && transition.To != stateMachine.InitialState ? String.Format(" color=\"{0}\" fontcolor=\"{0}\"", this.ColorForState(transition.To)) : "",
                        transition.From.ChildStateMachine == null ? "" : String.Format(" ltail=\"cluster_{0}\"", transition.From.Name),
                        transition.To.ChildStateMachine == null ? "" : String.Format(" lhead=\"cluster_{0}\"", transition.To.Name));
                }
            }
        }
    }
}
