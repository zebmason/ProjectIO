//------------------------------------------------------------------------------
// <copyright file="Tests.cs" company="Zebedee Mason">
//     Copyright (c) 2019 Zebedee Mason.
// </copyright>
//------------------------------------------------------------------------------

namespace ProjectIO.Test
{
    using NUnit.Framework;

    using System.Collections.Generic;

    [TestFixture]
    public class Tests
    {
        [Test]
        public void SourceGroups()
        {
            ListerUtilities.ReadTest(@"SourceGroups");
        }

        [Test]
        public void ConfigureFile()
        {
            ListerUtilities.ReadTest(@"ConfigureFile");
        }

        [Test]
        public void Environment()
        {
            System.Environment.SetEnvironmentVariable("TEST_CMAKE_SUB1_PATH", System.IO.Path.Combine(Utilities.GetDataDirec(), @"Source\Environment\sub1"));
            System.Environment.SetEnvironmentVariable("TEST_CMAKE_CONSOLE", "\"_CONSOLE\"");
            ListerUtilities.ReadTest(@"Environment");
        }

        [Test]
        public void VisualStudio()
        {
            VSUtilities.ReadTest(@"ConfigureFile", new List<string> { "lib" });
        }
    }
}
