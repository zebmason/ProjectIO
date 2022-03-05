//------------------------------------------------------------------------------
// <copyright file="ILogger.cs" company="Zebedee Mason">
//     Copyright (c) 2020 Zebedee Mason.
// </copyright>
//------------------------------------------------------------------------------

namespace ProjectIO.Core
{
    public interface ILogger
    {
        void Info(string message);
        void Info(string message, string argument);
        void Warn(string message);
        void Warn(string message, string argument);
    }

    public class PlainConsoleLogger : ILogger
    {
        public void Info(string line)
        {
            System.Console.WriteLine(line);
        }

        public void Info(string message, string argument)
        {
            System.Console.WriteLine(string.Format(message, argument));
        }

        public void Warn(string line)
        {
            System.Console.WriteLine(line);
        }

        public void Warn(string message, string argument)
        {
            System.Console.WriteLine(string.Format(message, argument));
        }
    }
}
