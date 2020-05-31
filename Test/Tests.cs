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
        public void VisualStudio()
        {
            VSUtilities.ReadTest(@"ConfigureFile", new List<string> { "lib" });
        }
    }
}
