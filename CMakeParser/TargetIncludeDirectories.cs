//------------------------------------------------------------------------------
// <copyright file="TargetIncludeDirectories.cs" company="Zebedee Mason">
//     Copyright (c) 2020 Zebedee Mason.
// </copyright>
//------------------------------------------------------------------------------

namespace CMakeParser.Core
{
    using System.Collections.Generic;

    public class TargetIncludeDirectories : ICommand
    {
        public interface IHandler
        {
            void AddIncludeDirectoriesToBinary(string name, IEnumerable<string> library);
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

            var libraries = state.FileOrDirectoryList(pair.Value.Replace(" PUBLIC ", " "));
            _handler.AddIncludeDirectoriesToBinary(name, libraries);
        }
    }
}
