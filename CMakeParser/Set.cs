//------------------------------------------------------------------------------
// <copyright file="Set.cs" company="Zebedee Mason">
//     Copyright (c) 2019 Zebedee Mason.
// </copyright>
//------------------------------------------------------------------------------

namespace CMakeParser.Core
{
    using System.Collections.Generic;

    public class Set : ICommand
    {
        public void Initialise(State state)
        {
        }

        public void Command(KeyValuePair<string, string> command, State state)
        {
            var pair = Utilities.Split(command.Value);
            var val = pair.Value;
            var index = val.IndexOf(" CACHE PATH ");
            if (index != -1)
            {
                val = val.Substring(0, index);
            }

            state.Variables["${" + pair.Key + "}"] = state.Replace(val);
        }
    }
}
