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
        void Warn(string message, string argument);
    }
}
