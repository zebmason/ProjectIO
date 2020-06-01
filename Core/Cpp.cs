//------------------------------------------------------------------------------
// <copyright file="FileExtensions.cs" company="Zebedee Mason">
//     Copyright (c) 2020 Zebedee Mason.
// </copyright>
//------------------------------------------------------------------------------

namespace ProjectIO.Core
{
    using System.Collections.Generic;
    using System.Linq;

    public class Cpp : Project
    {
        public string CompileDefinitions { get; set; } = string.Empty;

        public List<string> IncludeDirectories { get; } = new List<string>();

        public static bool IsHeader(string fileName)
        {
            var exts = new string[] { ".h", ".hh", ".hpp", ".hxx", ".h++" };
            return exts.Contains(System.IO.Path.GetExtension(fileName));
        }

        public static bool IsSource(string fileName)
        {
            var exts = new string[] { ".c", ".cc", ".cpp", ".cxx", ".c++" };
            return exts.Contains(System.IO.Path.GetExtension(fileName));
        }

        public static void Extract(List<string> filePaths, Dictionary<string, Project> projects)
        {
            var skipped = new List<string>();
            foreach (var filePath in filePaths)
            {
                if (IsHeader(filePath) || IsSource(filePath))
                {
                    if (!projects.ContainsKey("<c++>"))
                    {
                        projects["<c++>"] = new Cpp();
                    }

                    projects["<c++>"].FilePaths.Add(filePath);
                    continue;
                }

                skipped.Add(filePath);
            }

            filePaths.Clear();
            filePaths.AddRange(skipped);
        }
    }
}
