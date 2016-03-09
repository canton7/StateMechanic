using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace StateMechanic
{
    internal class StateMachineSerializer<TState> : IStateMachineSerializer<TState>
        where TState : StateBase<TState>, new()
    {
        private const int serializerVersion = 1;
        private const string separator = "/";

        // This is quite conservative - we can probably expand this
        private static readonly Regex notAllowedIdentifierCharacters = new Regex(@"[^a-zA-Z0-9]+");

        public IEnumerable<TState> Deserialize(StateMachine<TState> stateMachine, string serialized)
        {
            var parts = serialized.Split(new[] { ':' }, 2);
            int version;
            if (parts.Length != 2 || !Int32.TryParse(parts[0], out version))
                throw new StateMachineSerializationException($"Unable to deserialize from \"{serialized}\" - maybe this was created by a different version of StateMechanic?");

            if (version != serializerVersion)
                throw new StateMachineSerializationException($"Unable to deserialize from \"{serialized}\": expected serializer version {serializerVersion}, but got {version}");

            return DeserializeImpl(stateMachine, parts[1]);
        }

        private static IEnumerable<TState> DeserializeImpl(ChildStateMachine<TState> stateMachine, string serialized)
        {
            var identifiers = serialized.Split(new[] { separator }, StringSplitOptions.None);

            foreach (var identifier in identifiers)
            {
                // Have we run out of child state machines?
                if (stateMachine == null)
                    throw new StateMachineSerializationException($"Unable to deserialize from \"{serialized}\": tried to deserialize identifier \"{identifier}\", but the previous state has no child state machine. Make sure you're deserializing into exactly the same state machine as created the serialized string.");

                var state = StateForIdentifier(stateMachine, identifier);

                yield return state;

                stateMachine = state.ChildStateMachine;
            }

            // StateMachine.Deserialize will throw if stateMachine != null at the end
        }

        public string Serialize(StateMachine<TState> stateMachine)
        {
            var parts = new List<string>();
            for (ChildStateMachine<TState> sm = stateMachine; sm != null; sm = sm.CurrentState?.ChildStateMachine)
            {
                // State machine is not fully initialised
                if (sm.CurrentState == null)
                    throw new StateMachineSerializationException($"Unable to serialize state machine {sm} as it is not fully initialised (it has no initial state).");

                parts.Add(IdentifierForState(sm, sm.CurrentState));
            }

            return $"{serializerVersion}:{String.Join(separator, parts)}";
        }

        private static TState StateForIdentifier(ChildStateMachine<TState> stateMachine, string identifier)
        {
            // We iterate through the states until we find one with the given identifier

            // How many times we've seen each base identifier
            var lookup = new Dictionary<string, int>();

            foreach (var state in stateMachine.States)
            {
                var stateIdentifier = IdentifierForState(lookup, state);

                if (identifier == stateIdentifier)
                    return state;
            }

            throw new StateMachineSerializationException($"Identifier {identifier} is not associated with any of the states on StateMachine {stateMachine}. Make sure you're deserializing into exactly the same state machine as created the serialized string.");
        }

        private static string IdentifierForState(ChildStateMachine<TState> stateMachine, TState targetState)
        {
            // How many times we've seen each base identifier
            var lookup = new Dictionary<string, int>();

            foreach (var state in stateMachine.States)
            {
                var stateIdentifier = IdentifierForState(lookup, state);

                if (state == targetState)
                    return stateIdentifier;
            }

            throw new InvalidOperationException($"Unable to find state {targetState} on StateMachine {stateMachine}. This should not happen");
        }

        private static string IdentifierForState(Dictionary<string, int> lookup, TState state)
        {
            var baseIdentifier = BaseIdentifierForState(state);

            int count;
            string stateIdentifier;
            if (lookup.TryGetValue(baseIdentifier, out count))
            {
                count++;
                stateIdentifier = $"{baseIdentifier}-{count}";
            }
            else
            {
                count = 1;
                stateIdentifier = baseIdentifier;
            }

            lookup[baseIdentifier] = count;
            return stateIdentifier;
        }

        private static string BaseIdentifierForState(TState state)
        {
            if (state.Identifier == null)
                return "state";

            return notAllowedIdentifierCharacters.Replace(state.Identifier, "-");
        }
    }
}
