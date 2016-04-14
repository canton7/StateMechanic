using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StateMechanic
{
    /// <summary>
    /// Class which can take a state machine, and output graphviz which cab be rendered using dot, allowing the state machine to be visualised
    /// </summary>
    public class StateMachineDotPrinter
    {
        private readonly IStateMachine stateMachine;

        private readonly Dictionary<IState, string> stateToColorMapping = new Dictionary<IState, string>();
        private Dictionary<IState, string> stateToNameMapping = new Dictionary<IState, string>();
        private readonly Dictionary<IEvent, string> eventToNameMapping = new Dictionary<IEvent, string>();
        private int colorUseCount = 0;
        private int virtualStateIndex = 0;

        /// <summary>
        /// Gets the list of colors that will be used if <see cref="Colorize"/> is true
        /// </summary>
        public List<string> Colors { get; private set; }

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
            this.Colors = new List<string>()
            {
                "#0075dc", "#993f00", "#4c005c", "#191919", "#005c31", "#2bce48", "#808080", "#8f7c00", "#c20088", "#ffa405", "#ffa8bb",
                "#426600", "#ff0010", "#00998f", "#740aff", "#990000", "#ff5005", "#4d4d4d", "#5da5da", "#faa43a", "#60bd68", "#f17cb0",
                "#b2912f", "#b276b2", "#f15854"
            };
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

            color = this.Colors[this.colorUseCount];
            this.colorUseCount = (this.colorUseCount + 1) % this.Colors.Count;
            this.stateToColorMapping[state] = color;

            return color;
        }

        private string NameForState(IState state)
        {
            string name;
            if (this.stateToNameMapping.TryGetValue(state, out name))
                return name;

            // See how many other states have been given the same name...
            var count = this.stateToNameMapping.Keys.Count(x => x.Name == state.Name);
            var stateName = state.Name ?? "(unnamed state)";
            var mungedName = (count == 0) ? stateName : $"{stateName} ({count})";
            this.stateToNameMapping.Add(state, mungedName);
            return mungedName;
        }

        private string NameForEvent(IEvent @event)
        {
            string name;
            if (this.eventToNameMapping.TryGetValue(@event, out name))
                return name;

            // See how many other events have been given the same name...
            var count = this.eventToNameMapping.Keys.Count(x => x.Name == @event.Name);
            var eventName = @event.Name ?? "(unnamed event)";
            var mungedName = (count == 0) ? eventName : $"{eventName} ({count})";
            this.eventToNameMapping.Add(@event, mungedName);
            return mungedName;
        }

        private void RenderStateMachine(StringBuilder sb, IStateMachine stateMachine, string indent)
        {
            sb.AppendFormat("{0}compound=true;\n", indent);

            // States
            foreach (var state in stateMachine.States)
            {
                // If it has a child state machine, we'll link to/from it differently
                if (state.ChildStateMachine == null)
                {
                    if (state == stateMachine.InitialState)
                        sb.AppendFormat("{0}\"{1}\" [shape=doublecircle width=1 penwidth=2.0];\n", indent, this.NameForState(stateMachine.InitialState));
                    else if (this.Colorize)
                        sb.AppendFormat("{0}\"{1}\" [color=\"{2}\"];\n", indent, this.NameForState(state), this.ColorForState(state));
                }
                else
                {
                    sb.AppendFormat("{0}subgraph \"cluster_{1}\" {{\n", indent, this.NameForState(state));
                    var name = (state.Name == state.ChildStateMachine.Name) ? this.NameForState(state) : $"{this.NameForState(state)} / {state.ChildStateMachine.Name}";
                    sb.AppendFormat("{0}   label=\"{1}\";\n", indent, name);
                    if (state == stateMachine.InitialState)
                        sb.AppendFormat("{0}    style=bold;\n", indent); // Can't find a way of making it a double border >< 
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
                    if (transition.IsDynamicTransition)
                    {
                        // Define an virtual state to move to
                        // Destroyed [fillcolor=black, shape=doublecircle, label="", width=0.3]
                        sb.AppendFormat("{0}\"VirtualState_{1}\" [label=\"?\" width=0.1];\n",
                            indent,
                            this.virtualStateIndex);

                        sb.AppendFormat("{0}\"{1}\" -> \"VirtualState_{2}\" [label=\"{3}\"];\n",
                            indent,
                            this.NameForState(transition.From.ChildStateMachine == null ? transition.From : transition.From.ChildStateMachine.InitialState),
                            this.virtualStateIndex,
                            this.NameForEvent(transition.Event));

                        this.virtualStateIndex++;
                    }
                    else
                    {
                        // If the source has a child state machine, then lhead is the name of that
                        // Likewise dest and ltail
                        sb.AppendFormat("{0}\"{1}\" -> \"{2}\" [label=\"{3}{4}\"{5}{6}{7}];\n",
                            indent,
                            this.NameForState(transition.From.ChildStateMachine == null ? transition.From : transition.From.ChildStateMachine.InitialState),
                            this.NameForState(transition.To.ChildStateMachine == null ? transition.To : transition.To.ChildStateMachine.InitialState),
                            this.NameForEvent(transition.Event),
                            transition.HasGuard ? "*" : "",
                            this.Colorize && transition.To != stateMachine.InitialState ? String.Format(" color=\"{0}\" fontcolor=\"{0}\"", this.ColorForState(transition.To)) : "",
                            transition.From.ChildStateMachine == null ? "" : String.Format(" ltail=\"cluster_{0}\"", this.NameForState(transition.From)),
                            transition.To.ChildStateMachine == null ? "" : String.Format(" lhead=\"cluster_{0}\"", this.NameForState(transition.To)));
                    }
                }
            }
        }
    }
}
