//------------------------------------------------------------------------------
// <copyright file="TargetCompileDefinitions.cs" company="Zebedee Mason">
//     Copyright (c) 2020 Zebedee Mason.
// </copyright>
//------------------------------------------------------------------------------

namespace ProjectIO.CMakeParser
{
    using System.Collections.Generic;

    public class TargetCompileDefinitions : ICommand
    {
        public interface IHandler
        {
            void AddCompileDefinitionsToBinary(string name, IEnumerable<string> defns);
        }

        private readonly IHandler _handler;

        public TargetCompileDefinitions(IHandler handler)
        {
            _handler = handler;
        }

        public void Initialise(State state)
        {
        }

        public void Command(KeyValuePair<string, string> command, State state)
        {
            var pair = Utilities.Split(command.Value);
            var name = state.Replace(pair.Key);

            var line = pair.Value.Replace("INTERFACE", string.Empty).Replace("PUBLIC", string.Empty).Replace("PRIVATE", string.Empty);
            var defns = AddCompileDefinitions.Definitions(line, state);
            _handler.AddCompileDefinitionsToBinary(name, defns);
        }
    }
}
