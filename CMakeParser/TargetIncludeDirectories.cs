//------------------------------------------------------------------------------
// <copyright file="TargetIncludeDirectories.cs" company="Zebedee Mason">
//     Copyright (c) 2020 Zebedee Mason.
// </copyright>
//------------------------------------------------------------------------------

namespace ProjectIO.CMakeParser
{
    using System.Collections.Generic;

    public class TargetIncludeDirectories : ICommand
    {
        public interface IHandler
        {
            void AddIncludeDirectoriesToBinary(string name, List<string> library, bool before);
        }

        private readonly IHandler _handler;

        public TargetIncludeDirectories(IHandler handler)
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

            var line = pair.Value.Replace("BEFORE", string.Empty).Replace("SYSTEM", string.Empty);
            line = line.Replace("INTERFACE", string.Empty).Replace("PUBLIC", string.Empty).Replace("PRIVATE", string.Empty);
            var libraries = state.FileOrDirectoryList(line);
            _handler.AddIncludeDirectoriesToBinary(name, libraries, pair.Value.Contains("BEFORE"));
        }
    }
}
