//------------------------------------------------------------------------------
// <copyright file="ICommand.cs" company="Zebedee Mason">
//     Copyright (c) 2019 Zebedee Mason.
// </copyright>
//------------------------------------------------------------------------------

namespace CMakeParser.Core
{
    using System.Collections.Generic;

    public interface ICommand
    {
        void Initialise(State state);

        void Command(KeyValuePair<string, string> command, State state);
    }

    public interface ILogger
    {
        void Message(string message, State state);

        void Unhandled(KeyValuePair<string, string> command, State state);
    }
}
