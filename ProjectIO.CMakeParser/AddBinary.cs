//------------------------------------------------------------------------------
// <copyright file="AddBinary.cs" company="Zebedee Mason">
//     Copyright (c) 2019-2020 Zebedee Mason.
// </copyright>
//------------------------------------------------------------------------------

namespace ProjectIO.CMakeParser
{
    using System.Collections.Generic;

    public class AddBinary : ICommand
    {
        public interface IHandler
        {
            void Add(string command, string name, State state, IEnumerable<string> filePaths);
        }

        private readonly IHandler _handler;

        public AddBinary(IHandler handler)
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
            var files = state.FileOrDirectoryList(pair.Value.Replace(" SHARED ", " ").Replace(" STATIC ", " "));
            _handler.Add(command.Key, name, state, files);
        }
    }
}
