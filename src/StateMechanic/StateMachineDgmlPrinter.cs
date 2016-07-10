using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace StateMechanic
{
    /// <summary>
    /// Class which can take a state machine, and output DGML which cab be rendered using Visual Studio, allowing the state machine to be visualised
    /// </summary>
    public class StateMachineDgmlPrinter
    {
        private static readonly XmlSerializer serializer = new XmlSerializer(typeof(DirectedGraph));

        private const string defaultStroke = "Black";

        private readonly IStateMachine stateMachine;

        private readonly Dictionary<IState, string> stateToColorMapping = new Dictionary<IState, string>();
        private readonly Dictionary<IState, string> stateToNameMapping = new Dictionary<IState, string>();
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
        public StateMachineDgmlPrinter(IStateMachine stateMachine)
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
            var graph = new DirectedGraph()
            {
                Links = new List<Link>(),
                Nodes = new List<Node>(),
                Categories = new List<Category>()
                {
                    // Copied from DGML created by VS...
                    new Category()
                    {
                        Id = "Contains",
                        Label = "Contains",
                        Description = "Whether the source of the link contains the target object",
                        CanBeDataDriven = false,
                        CanLinkedNodesBeDataDriven = true,
                        IncomingActionLabel = "Contained By",
                        IsContainment = true,
                        OutgoingActionLabel = "Contains",
                    }
                }
            };

            RenderStateMachine(graph, this.stateMachine);

            using (var sw = new StringWriter())
            {
                serializer.Serialize(sw, graph);
                return sw.ToString();
            }
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

        private void RenderStateMachine(DirectedGraph graph, IStateMachine stateMachine, string parentStateMachineId = null)
        {
            // States
            foreach (var state in stateMachine.States)
            {
                var stateName = this.NameForState(state);
                bool isInitialState = state == state.ParentStateMachine.InitialState;

                // If it has a child state machine, we'll link to/from it differently
                if (state.ChildStateMachine == null)
                {
                    var stateNode = new Node()
                    {
                        Id = stateName,
                        Label = stateName,
                        Stroke = this.Colorize && !isInitialState ? this.ColorForState(state) : defaultStroke,
                        StrokeThickness = state == state.ParentStateMachine.InitialState ? "3" : null,
                    };
                    graph.Nodes.Add(stateNode);
                }
                else
                {
                    var stateMachineName = (state.Name == state.ChildStateMachine.Name) ? stateName : $"{stateName} / {state.ChildStateMachine.Name}";
                    var stateMachineNode = new Node()
                    {
                        Id = stateName,
                        Label = stateMachineName,
                        Group = "Expanded",
                        Stroke = this.Colorize && !isInitialState ? this.ColorForState(state) : defaultStroke,
                        StrokeThickness = isInitialState ? "3" : null,
                    };
                    graph.Nodes.Add(stateMachineNode);

                    this.RenderStateMachine(graph, state.ChildStateMachine, stateName);
                }

                if (parentStateMachineId != null)
                {
                    // State machines link to all of their children
                    var stateMachineToStateLink = new Link()
                    {
                        Source = parentStateMachineId,
                        Target = stateName,
                        Category = "Contains",
                    };
                    graph.Links.Add(stateMachineToStateLink);
                }

                // Map of to state -> the last link for that state, as we can't have two links with the same from and to states, without an index
                var lastDuplicateLinkLookup = new Dictionary<IState, Link>();

                foreach (var transition in state.Transitions)
                {
                    var transitionName = String.Format("{0}{1}", this.NameForEvent(transition.Event), transition.HasGuard ? "*" : "");
                    if (transition.IsDynamicTransition)
                    {
                        var nodeName = $"VirtualState_{this.virtualStateIndex}";

                        var virtualNode = new Node()
                        {
                            Id = nodeName,
                            Label = "?",
                            Stroke = defaultStroke,
                        };
                        graph.Nodes.Add(virtualNode);

                        var link = new Link()
                        {
                            Source = this.NameForState(transition.From.ChildStateMachine == null ? transition.From : transition.From.ChildStateMachine.InitialState),
                            Target = nodeName,
                            Label = transitionName,
                            Stroke = defaultStroke,
                        };

                        graph.Links.Add(link);

                        var virtualStateContainsLink = new Link()
                        {
                            Source = parentStateMachineId,
                            Target = nodeName,
                            Category = "Contains",
                        };
                        graph.Links.Add(virtualStateContainsLink);

                        this.virtualStateIndex++;
                    }
                    else
                    {
                        var link = new Link()
                        {
                            Source = this.NameForState(transition.From),
                            Target = this.NameForState(transition.To),
                            Label = transitionName,
                            Stroke = this.Colorize && transition.To != stateMachine.InitialState ? this.ColorForState(transition.To) : defaultStroke,
                        };

                        Link previousLink;
                        if (lastDuplicateLinkLookup.TryGetValue(transition.To, out previousLink))
                        {
                            if (previousLink.Index == 0)
                                previousLink.Index = 1;
                            link.Index = previousLink.Index + 1;
                        }
                        lastDuplicateLinkLookup[transition.To] = link;

                        graph.Links.Add(link);
                    }
                }
            }
        }

        /// <summary>
        /// Internal class used by StateMachineDgmlPrinter (has to be public for XmlSerializer to work)
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public class Node
        {
            /// <summary>
            /// ID of the node
            /// </summary>
            [XmlAttribute]
            public string Id { get; set; }

            /// <summary>
            /// Label of the node
            /// </summary>
            [XmlAttribute]
            public string Label { get; set; }

            /// <summary>
            /// Group containing the node, if any
            /// </summary>
            [XmlAttribute]
            public string Group { get; set; }

            /// <summary>
            /// Stroke of the node's border, if any
            /// </summary>
            [XmlAttribute]
            public string Stroke { get; set; }

            /// <summary>
            /// Thinkness of the node's border, if different to the default
            /// </summary>
            [XmlAttribute]
            public string StrokeThickness { get; set; }
        }

        /// <summary>
        /// Internal class used by StateMachineDgmlPrinter (has to be public for XmlSerializer to work)
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public class Link
        {
            /// <summary>
            /// Source node of the link
            /// </summary>
            [XmlAttribute]
            public string Source { get; set; }

            /// <summary>
            /// Target node of the link
            /// </summary>
            [XmlAttribute]
            public string Target { get; set; }

            /// <summary>
            /// Label displayed on the link
            /// </summary>
            [XmlAttribute]
            public string Label { get; set; }

            /// <summary>
            /// Color of the link, if any
            /// </summary>
            [XmlAttribute]
            public string Stroke { get; set; }

            /// <summary>
            /// Category the link belongs to, if any
            /// </summary>
            [XmlAttribute]
            public string Category { get; set; }

            /// <summary>
            /// Index of the link, if there are multiple links between the same two nodes
            /// </summary>
            [XmlAttribute]
            public int Index { get; set; }

            /// <summary>
            /// Returns a value indicating whether the 'Index' attribute should be serialized
            /// </summary>
            /// <returns>A value indicating whether the 'Index' attribute should be serialized</returns>
            public bool ShouldSerializeIndex() => this.Index != 0;
        }

        /// <summary>
        /// Internal class used by StateMachineDgmlPrinter (has to be public for XmlSerializer to work)
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public class Category
        {
            /// <summary>
            /// Generated by DGML editor
            /// </summary>
            [XmlAttribute]
            public string Id { get; set; }

            /// <summary>
            /// Generated by DGML editor
            /// </summary>
            [XmlAttribute]
            public string Label { get; set; }

            /// <summary>
            /// Generated by DGML editor
            /// </summary>
            [XmlAttribute]
            public string Description { get; set; }

            /// <summary>
            /// Generated by DGML editor
            /// </summary>
            [XmlAttribute]
            public bool CanBeDataDriven { get; set; }

            /// <summary>
            /// Generated by DGML editor
            /// </summary>
            [XmlAttribute]
            public bool CanLinkedNodesBeDataDriven { get; set; }

            /// <summary>
            /// Generated by DGML editor
            /// </summary>
            [XmlAttribute]
            public string IncomingActionLabel { get; set; }

            /// <summary>
            /// Generated by DGML editor
            /// </summary>
            [XmlAttribute]
            public bool IsContainment { get; set; }

            /// <summary>
            /// Generated by DGML editor
            /// </summary>
            [XmlAttribute]
            public string OutgoingActionLabel { get; set; }
        }

        /// <summary>
        /// Internal class used by StateMachineDgmlPrinter (has to be public for XmlSerializer to work)
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [XmlRoot(Namespace = "http://schemas.microsoft.com/vs/2009/dgml")]
        public class DirectedGraph
        {
            /// <summary>
            /// Nodes in the graph
            /// </summary>
            public List<Node> Nodes { get; set; }

            /// <summary>
            /// Links in the graph
            /// </summary>
            public List<Link> Links { get; set; }

            /// <summary>
            /// Categories in the graph
            /// </summary>
            public List<Category> Categories { get; set; }
        }
    }
}
