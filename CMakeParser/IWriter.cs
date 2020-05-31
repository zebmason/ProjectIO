//------------------------------------------------------------------------------
// <copyright file="IWriter.cs" company="Zebedee Mason">
//     Copyright (c) 2020 Zebedee Mason.
// </copyright>
//------------------------------------------------------------------------------

namespace ProjectIO.CMakeParser
{
    using System.Collections.Generic;

    public class Logger : ILogger
    {
        private readonly Core.ILogger _logger;

        public Logger(Core.ILogger logger)
        {
            _logger = logger;
        }

        public void Info(string message, CMakeParser.State state)
        {
            _logger.Info(string.Format("[{0}] {1}", state.Variables["${CMAKE_CURRENT_SOURCE_DIR}"], message));
        }

        public void Warn(string message, CMakeParser.State state)
        {
            _logger.Warn(string.Format("[{0}] {1}", state.Variables["${CMAKE_CURRENT_SOURCE_DIR}"], message));
        }

        public void Unhandled(KeyValuePair<string, string> command, CMakeParser.State state)
        {
            _logger.Warn(string.Format("[{0}] Unhandled {1}({2})", state.Variables["${CMAKE_CURRENT_SOURCE_DIR}"], command.Key, command.Value));
        }
    }
}
