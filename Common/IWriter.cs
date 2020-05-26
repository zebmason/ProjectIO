//------------------------------------------------------------------------------
// <copyright file="IWriter.cs" company="Zebedee Mason">
//     Copyright (c) 2020 Zebedee Mason.
// </copyright>
//------------------------------------------------------------------------------

namespace CMakeParser.Common
{
    using System;
    using System.Collections.Generic;

    public interface IWriter
    {
        void WriteLine(string line);
    }

    public class Writer : IWriter
    {
        public void WriteLine(string line)
        {
            Console.WriteLine(line);
        }
    }

    public class Logger : Core.ILogger
    {
        private readonly IWriter _writer;

        public Logger(IWriter writer)
        {
            _writer = writer;
        }

        public void Message(string message, Core.State state)
        {
            _writer.WriteLine(string.Format("[{0}] {1}", state.Variables["${CMAKE_CURRENT_SOURCE_DIR}"], message));
        }

        public void Unhandled(KeyValuePair<string, string> command, Core.State state)
        {
            _writer.WriteLine(string.Format("[{0}] Unhandled {1}({2})", state.Variables["${CMAKE_CURRENT_SOURCE_DIR}"], command.Key, command.Value));
        }
    }
}
