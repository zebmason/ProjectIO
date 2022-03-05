//------------------------------------------------------------------------------
// <copyright file="TargetSources.cs" company="Zebedee Mason">
//     Copyright (c) 2020 Zebedee Mason.
// </copyright>
//------------------------------------------------------------------------------

namespace ProjectIO.CMakeParser
{
    using System.Collections.Generic;

    class TargetSources : ICommand
    {
        public interface IHandler
        {
            void AddSourceToBinary(string name, string source);
        }

        private readonly IHandler _handler;

        public TargetSources(IHandler handler)
        {
            _handler = handler;
        }

        public void Command(KeyValuePair<string, string> command, State state)
        {
            var pair = Utilities.Split(command.Value);
            var name = state.Replace(pair.Key);
            var list = " " + pair.Value;
            var fileNames = state.FileOrDirectoryList(list.Replace(" INTERFACE ", " ").Replace(" PUBLIC ", " ").Replace(" PRIVATE ", " "));
            foreach (var fileName in fileNames)
            {
                _handler.AddSourceToBinary(name, fileName);
            }
        }

        public void Initialise(State state)
        {
        }
    }
}
