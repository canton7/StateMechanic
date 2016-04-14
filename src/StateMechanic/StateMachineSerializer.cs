using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace StateMechanic
{
    internal class StateMachineSerializer<TState> : IStateMachineSerializer<TState>
        where TState : StateBase<TState>, new()
    {
        private const int serializerVersion = 1;

        public TState Deserialize(StateMachine<TState> stateMachine, string serialized)
        {
            var parts = serialized.Split(new[] { ':' }, 2);
            int version;
            if (parts.Length != 2 || !Int32.TryParse(parts[0], out version))
                throw new StateMachineSerializationException($"Unable to deserialize from \"{serialized}\" - maybe this was created by a different version of StateMechanic?");

            if (version != serializerVersion)
                throw new StateMachineSerializationException($"Unable to deserialize from \"{serialized}\": expected serializer version {serializerVersion}, but got {version}");

            var childState = StateForIdentifier(stateMachine, parts[1]);
            return childState;
        }

        public string Serialize(StateMachine<TState> stateMachine)
        {
            return $"{serializerVersion}:{IdentifierForState(stateMachine, stateMachine.CurrentChildState)}";
        }

        private static TState StateForIdentifier(StateMachine<TState> stateMachine, string identifier)
        {
            // How many times we've seen each base identifier
            var lookup = new Dictionary<string, int>();

            foreach (var state in StateIterator(stateMachine))
            {
                var stateIdentifier = IdentifierForState(lookup, state);

                if (identifier == stateIdentifier)
                    return state;
            }

            throw new StateMachineSerializationException($"Identifier {identifier} is not associated with any of the states on StateMachine {stateMachine}. Make sure you're deserializing into exactly the same state machine as created the serialized string.");
        }

        private static string IdentifierForState(StateMachine<TState> stateMachine, TState targetState)
        {
            // How many times we've seen each base identifier
            var lookup = new Dictionary<string, int>();

            foreach (var state in StateIterator(stateMachine))
            {
                var stateIdentifier = IdentifierForState(lookup, state);
                if (state == targetState)
                    return stateIdentifier;
            }

            throw new InvalidOperationException($"Unable to find state {targetState} on StateMachine {stateMachine}. This should not happen");
        }

        private static IEnumerable<TState> StateIterator(ChildStateMachine<TState> stateMachine)
        {
            // Depth-first recursive iteration of all states

            // State machine is not fully initialised
            if (stateMachine.InitialState == null)
                throw new StateMachineSerializationException($"Unable to serialize state machine {stateMachine} as it is not fully initialised (it has no initial state).");

            foreach (var state in stateMachine.States)
            {
                yield return state;

                if (state.ChildStateMachine != null)
                {
                    foreach (var subState in StateIterator(state.ChildStateMachine))
                    {
                        yield return subState;
                    }
                }
            }
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
            var identifier = state.Identifier;
            return String.IsNullOrWhiteSpace(identifier) ? "state" : identifier;
        }
    }
}
