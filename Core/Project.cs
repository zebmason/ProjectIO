//------------------------------------------------------------------------------
// <copyright file="Project.cs" company="Zebedee Mason">
//     Copyright (c) 2020 Zebedee Mason.
// </copyright>
//------------------------------------------------------------------------------

namespace ProjectIO.Core
{
    using System.Collections.Generic;

    public class Project
    {
        public bool IsExe { get; set; } = false;

        public List<string> FilePaths { get; } = new List<string>();

        public List<string> Dependencies { get; } = new List<string>();
    }
}
