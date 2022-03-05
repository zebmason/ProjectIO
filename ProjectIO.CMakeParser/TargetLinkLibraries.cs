//------------------------------------------------------------------------------
// <copyright file="TargetLinkLibraries.cs" company="Zebedee Mason">
//     Copyright (c) 2020 Zebedee Mason.
// </copyright>
//------------------------------------------------------------------------------

namespace ProjectIO.CMakeParser
{
    using System.Collections.Generic;

    public class TargetLinkLibraries : ICommand
    {
        public interface IHandler
        {
            void AddLibrariesToBinary(string name, IEnumerable<string> libraries);
        }

        private readonly IHandler _handler;

        public TargetLinkLibraries(IHandler handler)
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
            var list = " " + pair.Value;
            var libraries = state.FileOrDirectoryList(list.Replace(" PUBLIC ", " "));
            _handler.AddLibrariesToBinary(name, libraries);
        }
    }
}
