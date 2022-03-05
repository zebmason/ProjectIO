//------------------------------------------------------------------------------
// <copyright file="SHProj.cs" company="Zebedee Mason">
//     Copyright (c) 2020 Zebedee Mason.
// </copyright>
//------------------------------------------------------------------------------

namespace ProjectIO.VisualStudio
{
    using System;
    using System.Collections.Generic;

    internal class SHProj : NetProj
    {
        public override string FilePath
        {
            get
            {
                var l1 = new List<System.Xml.XmlElement>();
                _xml.SelectNodes(_xml._root, "Import", l1);
                foreach (var i1 in l1)
                {
                    if (!i1.HasAttribute("Label"))
                        continue;

                    if (i1.GetAttribute("Label") != "Shared")
                        continue;

                    var link = i1.GetAttribute("Project");
                    link = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(_filePath), link);

                    return link;
                }

                return string.Empty;
            }
        }

        public SHProj(Core.ILogger logger, string path, Core.Paths paths, string configPlatform)
            : base(logger, path, paths, configPlatform)
        {
        }

        public override string Name
        {
            get
            {
                return System.IO.Path.GetFileNameWithoutExtension(_filePath);
            }
        }

        public override List<string> Dependencies()
        {
            return new List<string>();
        }

        public override List<string> Externals()
        {
            var list = new List<string>();
            return list;
        }

        public override void Compiles(List<string> files, Core.ILogger logger, Core.Paths filePath)
        {
            var xml2 = new XMLUtils(FilePath);
            xml2.DotNetCompiles(this, files, logger, filePath);
        }

        public override Core.Project Extract(Core.ILogger logger, Core.Paths paths, string filePath,  Dictionary<Core.Project, List<string>> dependencies, Dictionary<string, string> mapping)
        {
            var proj = this;

            var project = new Core.CSharp();
            dependencies[project] = proj.Dependencies();
            proj.Compiles(project.FilePaths, logger, paths);
            mapping[proj.FilePath] = proj.Name;

            return project;
        }

        [Obsolete("Create a CSProj then use the dynamic Extract")]
        public static void Extract(Core.ILogger logger, Core.Paths paths, string filePath, Dictionary<string, Core.Project> projects, Dictionary<Core.Project, List<string>> dependencies, Dictionary<string, string> mapping, string configPlatform)
        {
            var proj = new SHProj(logger, filePath, paths, configPlatform);

            projects[proj.Name] = proj.Extract(logger, paths, filePath, dependencies, mapping);
        }
    }
}
