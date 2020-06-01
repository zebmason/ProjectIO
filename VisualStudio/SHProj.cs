//------------------------------------------------------------------------------
// <copyright file="SHProj.cs" company="Zebedee Mason">
//     Copyright (c) 2020 Zebedee Mason.
// </copyright>
//------------------------------------------------------------------------------

namespace ProjectIO.VisualStudio
{
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
                    link = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(_path.FilePath), link);

                    return link;
                }

                return string.Empty;
            }
        }

        public SHProj(ProjectPath path)
            : base(path)
        {
        }

        public override string Name
        {
            get
            {
                return System.IO.Path.GetFileNameWithoutExtension(_path.FilePath);
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

        public override void Compiles(Dictionary<string, string> files, Core.ILogger logger, Core.Paths filePath)
        {
            var xml2 = new XMLUtils(FilePath);
            xml2.DotNetCompiles(this, files, logger, filePath);
        }

        public static void Extract(Core.ILogger logger, Core.Paths paths, string filePath, Dictionary<string, Core.Project> projects)
        {
            var solutionPath = paths.Mapping["$(SolutionDir)"];
            var proj = new SHProj(new ProjectPath(filePath, solutionPath));

            projects[proj.Name] = new Core.Project("C#");
            foreach (var dep in proj.Dependencies())
            {
                var stub = System.IO.Path.GetFileNameWithoutExtension(dep);
                projects[proj.Name].Libraries.Add(stub);
            }

            var files = new Dictionary<string, string>();
            proj.Compiles(files, logger, paths);
            foreach (var fullName in files.Keys)
            {
                logger.Info("Appended {}", fullName);
                projects[proj.Name].FilePaths.Add(fullName);
            }
        }
    }
}
