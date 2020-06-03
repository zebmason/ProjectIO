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
        }

        public static string[] Definitions(string line, State state)
        {
            line = state.Replace(line);
            line = line.Replace("\"", string.Empty);
            return line.Split(new char[] { ' ', '\t' }, System.StringSplitOptions.RemoveEmptyEntries);
        }

        public void Command(KeyValuePair<string, string> command, State state)
        {
            var defns = Definitions(command.Value, state);
            state.CompileDefinitions.AddRange(defns);
        }
    }
}
