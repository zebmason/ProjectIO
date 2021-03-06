﻿//------------------------------------------------------------------------------
// <copyright file="GetFileNameComponent.cs" company="Zebedee Mason">
//     Copyright (c) 2020 Zebedee Mason.
// </copyright>
//------------------------------------------------------------------------------

namespace ProjectIO.CMakeParser
{
    using System.Collections.Generic;

    public class GetFileNameComponent : ICommand
    {
        public GetFileNameComponent()
        {
        }

        public void Initialise(State state)
        {
        }

        public void Command(KeyValuePair<string, string> command, State state)
        {
            var pair = Utilities.Split(command.Value);
            if (command.Value.Contains("NAME_WE"))
            {
                var line = command.Value.Replace("NAME_WE", string.Empty).Trim();
                pair = Utilities.Split(line);
                var name = state.Replace(pair.Value);
                name = System.IO.Path.GetFileNameWithoutExtension(name);
                state.Variables["${" + pair.Key + "}"] = name;
                return;
            }

            state.Unhandled(command);
        }
    }
}
