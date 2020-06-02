//------------------------------------------------------------------------------
// <copyright file="FileExtensions.cs" company="Zebedee Mason">
//     Copyright (c) 2020 Zebedee Mason.
// </copyright>
//------------------------------------------------------------------------------

namespace ProjectIO.Core
{
    using System.Collections.Generic;
    using System.Linq;

    public class VBasic : Project
    {
        public string CompileDefinitions { get; set; } = string.Empty;

        public static bool IsSource(string fileName)
        {
            var exts = new string[] { ".vb", ".vbs" };
            return exts.Contains(System.IO.Path.GetExtension(fileName));
        }

        public static void Extract(ILogger logger, List<string> filePaths, Dictionary<string, Project> projects)
        {
            var skipped = new List<string>();
            foreach (var filePath in filePaths)
            {
                if (IsSource(filePath))
                {
                    if (!projects.ContainsKey("<vb>"))
                    {
                        projects["<vb>"] = new VBasic();
                    }

                    projects["<vb>"].FilePaths.Add(filePath);
                    logger.Info("Appended for reading \"{0}\"", filePath);
                    continue;
                }

                skipped.Add(filePath);
            }

            filePaths.Clear();
            filePaths.AddRange(skipped);
        }
    }
}
