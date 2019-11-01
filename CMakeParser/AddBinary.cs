//------------------------------------------------------------------------------
// <copyright file="AddBinary.cs" company="Zebedee Mason">
//     Copyright (c) 2019 Zebedee Mason.
// </copyright>
//------------------------------------------------------------------------------

namespace CMakeParser.Command
{
    using System.Collections.Generic;

    public class AddBinary : ICommand
    {
        public interface IHandler
        {
            void AddFileToBinary(string name, string filePath, State state, HashSet<string> includes, List<string> defines);
        }

        private readonly IHandler _handler;

        public AddBinary(IHandler handler)
        {
            _handler = handler;
        }

        public void Initialise(State state)
        {
        }

        public HashSet<string> Includes(State state)
        {
            var includes = new HashSet<string>();
            if (!state.Properties.ContainsKey("INCLUDE_DIRECTORIES"))
                return includes;

            foreach (var dir in state.FileOrDirectoryList(state.Properties["INCLUDE_DIRECTORIES"]))
            {
                includes.Add(dir);
            }

            return includes;
        }

        public void Command(KeyValuePair<string, string> command, State state)
        {
            var pair = Utilities.Split(command.Value);
            var defines = new List<string>();
            var includes = Includes(state);
            foreach (var file in state.FileOrDirectoryList(pair.Value.Replace(" SHARED ", " ").Replace(" STATIC ", " ")))
            {
                _handler.AddFileToBinary(pair.Key, file, state, includes, defines);
            }
        }
    }
}
