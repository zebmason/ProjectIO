//------------------------------------------------------------------------------
// <copyright file="Ignore.cs" company="Zebedee Mason">
//     Copyright (c) 2019 Zebedee Mason.
// </copyright>
//------------------------------------------------------------------------------

namespace ProjectIO.CMakeParser
{
    using System.Collections.Generic;

    public class Ignore : ICommand
    {
        public void Initialise(State state)
        {
        }

        public void Command(KeyValuePair<string, string> command, State state)
        {
        }
    }
}
