//------------------------------------------------------------------------------
// <copyright file="AddCompileDefinitions.cs" company="Zebedee Mason">
//     Copyright (c) 2020 Zebedee Mason.
// </copyright>
//------------------------------------------------------------------------------

namespace ProjectIO.CMakeParser
{
    using System.Collections.Generic;

    public class AddCompileDefinitions : ICommand
    {
        public void Initialise(State state)
        {
            state.Properties["COMPILE_DEFINITIONS"] = string.Empty;
        }

        public void Command(KeyValuePair<string, string> command, State state)
        {
            state.Properties["COMPILE_DEFINITIONS"] = string.Format("{0} {1}", state.Properties["COMPILE_DEFINITIONS"], command.Value);
        }
    }
}
