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

        public List<string> Libraries { get; } = new List<string>();

        public string CompileDefinitions { get; set; } = string.Empty;

        public List<string> IncludeDirectories { get; } = new List<string>();

        public string Language { get; }

        public Project(string language)
        {
            Language = language;
        }
    }
}
