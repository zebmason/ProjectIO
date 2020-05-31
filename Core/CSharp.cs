//------------------------------------------------------------------------------
// <copyright file="FileExtensions.cs" company="Zebedee Mason">
//     Copyright (c) 2020 Zebedee Mason.
// </copyright>
//------------------------------------------------------------------------------

namespace ProjectIO.Core
{
    using System.Collections.Generic;
    using System.Linq;

    public static class CSharp
    {
        public static bool IsSource(string fileName)
        {
            var exts = new string[] { ".cs", ".csx" };
            return exts.Contains(System.IO.Path.GetExtension(fileName));
        }

        public static void Extract(List<string> filePaths, Dictionary<string, Project> projects)
        {
            var skipped = new List<string>();
            foreach (var filePath in filePaths)
            {
                if (IsSource(filePath))
                {
                    if (!projects.ContainsKey("<c#>"))
                    {
                        projects["<c#>"] = new Project("C#");
                    }

                    projects["<c#>"].FilePaths.Add(filePath);
                    continue;
                }

                skipped.Add(filePath);
            }

            filePaths.Clear();
            filePaths.AddRange(skipped);
        }
    }
}
