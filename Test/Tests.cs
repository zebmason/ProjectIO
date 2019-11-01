//------------------------------------------------------------------------------
// <copyright file="Tests.cs" company="Zebedee Mason">
//     Copyright (c) 2019 Zebedee Mason.
// </copyright>
//------------------------------------------------------------------------------

namespace CMakeParser.Test
{
    using NUnit.Framework;

    [TestFixture]
    public class Tests
    {
        [Test]
        public void SourceGroups()
        {
            Utilities.ReadTest(@"SourceGroups", string.Empty);
        }
    }
}
