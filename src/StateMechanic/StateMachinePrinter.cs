using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateMechanic
{
    public static class StateMachinePrinter
    {
        public static string FormatDot(IStateMachine stateMachine)
        {
            var sb = new StringBuilder();
            sb.AppendFormat("digraph \"{0}\" {{\n", stateMachine.Name);
            sb.AppendFormat("label=\"{0}\";\n", stateMachine.Name);

            RenderStateMachine(sb, stateMachine, "   ");

            sb.AppendFormat("}}");
            return sb.ToString();
        }

        private static void RenderStateMachine(StringBuilder sb, IStateMachine stateMachine, string indent)
        {
            sb.AppendFormat("{0}compound=true;\n", indent);

            // States
            sb.AppendFormat("{0}\"{1}\" [shape=doublecircle];\n", indent, stateMachine.InitialState.Name);
            foreach (var state in stateMachine.States.Except(new[] { stateMachine.InitialState }))
            {
                // If it has a child state machine, we'll link to/from it differently
                if (state.ChildStateMachine == null)
                {
                    sb.AppendFormat("{0}\"{1}\" [shape=circle];\n", indent, state.Name);
                }
                else
                {
                    sb.AppendFormat("{0}subgraph \"cluster_{1}\" {{\n", indent, state.Name);
                    sb.AppendFormat("{0}   label=\"{1} / {2}\";\n", indent, state.Name, state.ChildStateMachine.Name);
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
                    sb.AppendFormat("{0}\"{1}\" -> \"{2}\" [label=\"{3}\"{4}{5}];\n",
                        indent,
                        transition.From.ChildStateMachine == null ? transition.From.Name : transition.From.ChildStateMachine.InitialState.Name,
                        transition.To.ChildStateMachine == null ? transition.To.Name : transition.To.ChildStateMachine.InitialState.Name,
                        transition.Event.Name,
                        transition.From.ChildStateMachine == null ? "" : String.Format(",ltail=\"cluster_{0}\"", transition.From.Name),
                        transition.To.ChildStateMachine == null ? "" : String.Format(",lhead=\"cluster_{0}\"", transition.To.Name));
                }
            }
        }
    }
}
