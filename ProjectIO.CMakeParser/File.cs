//------------------------------------------------------------------------------
// <copyright file="File.cs" company="Zebedee Mason">
//     Copyright (c) 2019 Zebedee Mason.
// </copyright>
//------------------------------------------------------------------------------

namespace ProjectIO.CMakeParser
{
    using System.Collections.Generic;

    public class File : ICommand
    {
        public File()
        {
        }

        public void Initialise(State state)
        {
        }

        public void Command(KeyValuePair<string, string> command, State state)
        {
            var pair = Utilities.Split(command.Value);
            if (pair.Key == "GLOB_RECURSE" || pair.Key == "GLOB")
            {
                var option = System.IO.SearchOption.AllDirectories;
                if (pair.Key == "GLOB")
                {
                    option = System.IO.SearchOption.TopDirectoryOnly;
                }

                pair = Utilities.Split(pair.Value);
                var search = pair.Value;
                search = state.Replace(search);
                search = search.Replace("\"", string.Empty);

                var direc = state.Variables["${CMAKE_CURRENT_SOURCE_DIR}"];
                var index = search.LastIndexOf("/");
                if (index != -1)
                {
                    direc = search.Substring(0, index);
                    search = search.Substring(index + 1);
                }

                var files = string.Empty;
                foreach (var file in System.IO.Directory.GetFiles(direc, search, option))
                {
                    files += file + " ";
                }

                state.Variables["${" + pair.Key + "}"] = files.Trim();
                return;
            }

            state.Unhandled(command);
        }
    }
}
