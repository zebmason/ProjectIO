//------------------------------------------------------------------------------
// <copyright file="VBProj.cs" company="Zebedee Mason">
//     Copyright (c) 2020 Zebedee Mason.
// </copyright>
//------------------------------------------------------------------------------

namespace ProjectIO.VisualStudio
{
    using System;
    using System.Collections.Generic;

    internal class VBProj : NetProj
    {
        public VBProj(Core.ILogger logger, string path, Core.Paths paths, string configPlatform)
            : base(logger, path, paths, configPlatform)
        {
        }

        public override Core.Project Extract(Core.ILogger logger, Core.Paths paths, string filePath, Dictionary<Core.Project, List<string>> dependencies, Dictionary<string, string> mapping)
        {
            var proj = this;

            var project = new Core.VBasic();
            dependencies[project] = proj.Dependencies();
            proj.Compiles(project.FilePaths, logger, paths);
            mapping[filePath] = proj.Name;

            return project;
        }

        [Obsolete("Create a VBProj then use the dynamic Extract")]
        public static void Extract(Core.ILogger logger, Core.Paths paths, string filePath, Dictionary<string, Core.Project> projects, Dictionary<Core.Project, List<string>> dependencies, Dictionary<string, string> mapping, string configPlatform)
        {
            var proj = new VBProj(logger, filePath, paths, configPlatform);

            projects[proj.Name] = proj.Extract(logger, paths, filePath, dependencies, mapping);
        }
    }
}
