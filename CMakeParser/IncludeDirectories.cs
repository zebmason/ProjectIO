//------------------------------------------------------------------------------
// <copyright file="AddProp.cs" company="Zebedee Mason">
//     Copyright (c) 2019 Zebedee Mason.
// </copyright>
//------------------------------------------------------------------------------

namespace CMakeParser.Command
{
    using System.Collections.Generic;

    public class IncludeDirectories : ICommand
    {
        public void Initialise(State state)
        {
            state.Properties["INCLUDE_DIRECTORIES"] = string.Empty;
            state.Properties["CMAKE_INCLUDE_DIRECTORIES_BEFORE"] = "OFF";
        }

        public void Command(KeyValuePair<string, string> command, State state)
        {
            var bits = Utilities.Split(command.Value, new string[] { "AFTER", "BEFORE", "SYSTEM" });
            var line = command.Value.Replace("AFTER", string.Empty).Replace("BEFORE", string.Empty).Replace("SYSTEM", string.Empty);
            if (bits.ContainsKey("AFTER") || state.Properties["CMAKE_INCLUDE_DIRECTORIES_BEFORE"] == "OFF")
                state.Properties["INCLUDE_DIRECTORIES"] = string.Format("{0} {1}", state.Properties["INCLUDE_DIRECTORIES"], line);
            else
                state.Properties["INCLUDE_DIRECTORIES"] = string.Format("{1} {0}", state.Properties["INCLUDE_DIRECTORIES"], line);
        }
    }
}
