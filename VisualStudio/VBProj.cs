//------------------------------------------------------------------------------
// <copyright file="VBProj.cs" company="Zebedee Mason">
//     Copyright (c) 2020 Zebedee Mason.
// </copyright>
//------------------------------------------------------------------------------

namespace ProjectIO.VisualStudio
{
    using System.Collections.Generic;

    internal class VBProj : NetProj
    {
        public VBProj(string path, Core.Paths paths)
            : base(path, paths)
        {
        }

        public static void Extract(Core.ILogger logger, Core.Paths paths, string filePath, Dictionary<string, Core.Project> projects)
        {
            var solutionPath = paths.Value("SolutionDir");
            var proj = new VBProj(filePath, paths);

            projects[proj.Name] = new Core.VBasic();
            foreach (var dep in proj.Dependencies())
            {
                var stub = System.IO.Path.GetFileNameWithoutExtension(dep);
                projects[proj.Name].Dependencies.Add(stub);
            }

            proj.Compiles(projects[proj.Name].FilePaths, logger, paths);
        }
    }
}
