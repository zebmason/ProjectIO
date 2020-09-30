//------------------------------------------------------------------------------
// <copyright file="SourceGroup.cs" company="Zebedee Mason">
//     Copyright (c) 2019 Zebedee Mason.
// </copyright>
//------------------------------------------------------------------------------

namespace ProjectIO.CMakeParser
{
    using System.Collections.Generic;

    public class SourceGroup : ICommand
    {
        public interface IHandler
        {
            void AddFile(string filePath, string filter);
        }

        private readonly IHandler _handler;

        public SourceGroup(IHandler handler)
        {
            _handler = handler;
        }

        public void Initialise(State state)
        {
        }

        private string Filter(string filter)
        {
            filter = filter.Replace("/", "\\");
            filter = filter.Replace("\\\\", "\\");
            if (filter == "\"\"" || filter == "\"\\\"")
                filter = string.Empty;
            if (filter.Length > 0 && filter[0] == '\\')
                filter = filter.Substring(1);
            return filter;
        }

        public void Command(KeyValuePair<string, string> command, State state)
        {
            var bits = Utilities.Split(command.Value, new string[] { "TREE", "PREFIX", "FILES", "REGULAR_EXPRESSION" });

            var filter = string.Empty;

            if (bits.ContainsKey("TREE"))
            {
                var root = state.FileOrDirectoryList(bits["TREE"])[0];

                var prefix = string.Empty;
                if (bits.ContainsKey("PREFIX"))
                {
                    prefix = Filter(bits["PREFIX"]);
                }

                if (bits.ContainsKey("FILES"))
                {
                    foreach (var file in state.FileOrDirectoryList(bits["FILES"]))
                    {
                        if (file.IndexOf(root) == 0)
                        {
                            filter = System.IO.Path.GetDirectoryName(file).Substring(root.Length);
                            filter = Filter(filter);
                            if (filter.Length == 0)
                                filter = prefix;
                        }

                        _handler.AddFile(file, filter);
                    }
                }

                return;
            }

            filter = Filter(bits[""]);

            if (bits.ContainsKey("FILES"))
            {
                foreach (var file in state.FileOrDirectoryList(bits["FILES"]))
                {
                    _handler.AddFile(file, filter);
                }

                return;
            }

            state.Unhandled(command);
        }
    }
}
