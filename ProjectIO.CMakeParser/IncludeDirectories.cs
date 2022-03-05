//------------------------------------------------------------------------------
// <copyright file="AddProp.cs" company="Zebedee Mason">
//     Copyright (c) 2019-2020 Zebedee Mason.
// </copyright>
//------------------------------------------------------------------------------

namespace ProjectIO.CMakeParser
{
    using System.Collections.Generic;

    public class IncludeDirectories : ICommand
    {
        public void Initialise(State state)
        {
            state.Properties["CMAKE_INCLUDE_DIRECTORIES_BEFORE"] = "OFF";
        }

        public void Command(KeyValuePair<string, string> command, State state)
        {
            var line = command.Value.Replace("AFTER", string.Empty).Replace("BEFORE", string.Empty).Replace("SYSTEM", string.Empty);
            var dirs = state.FileOrDirectoryList(line);

            if (command.Value.Contains("AFTER") || state.Properties["CMAKE_INCLUDE_DIRECTORIES_BEFORE"] == "OFF")
            {
                state.IncludeDirectories.AddRange(dirs);
            }
            else
            {
                for (int i = dirs.Count - 1; i > -1; --i)
                {
                    state.IncludeDirectories.Insert(0, dirs[i]);
                }
            }
        }
    }
}
