//------------------------------------------------------------------------------
// <copyright file="CSProj.cs" company="Zebedee Mason">
//     Copyright (c) 2020 Zebedee Mason.
// </copyright>
//------------------------------------------------------------------------------

namespace ProjectIO.VisualStudio
{
    using System.Collections.Generic;

    internal class CSProj : NetProj
    {
        public CSProj(Core.ILogger logger, string path, Core.Paths paths, string configPlatform)
            : base(logger, path, paths, configPlatform)
        {
        }

        public override List<string> Dependencies()
        {
            var list = base.Dependencies();
            var l1 = new List<System.Xml.XmlElement>();
            _xml.SelectNodes(_xml._root, "Import", l1);
            foreach (var i1 in l1)
            {
                if (!i1.HasAttribute("Label"))
                    continue;

                if (i1.GetAttribute("Label") != "Shared")
                    continue;

                var link = i1.GetAttribute("Project");
                link = _paths.Path(link);
                list.Add(link);
            }

            return list;
        }

        public List<string> DefineConstants()
        {
            var defns = new List<string>();
            var group = _xml.Group("PropertyGroup", _configPlatform);
            if (group is null)
            {
                _logger.Warn(string.Format("No PropertyGroup {0} in \"{1}\"", _configPlatform, _filePath));
                return defns;
            }

            var nodes = new List<System.Xml.XmlElement>();
            _xml.SelectNodes(group, "DefineConstants", nodes, true);

            foreach (var node in nodes)
            {
                var line = node.InnerText;
                foreach (var defn in line.Split(';'))
                {
                    defns.Add(defn);
                }
            }

            return defns;
        }

        public static void Extract(Core.ILogger logger, Core.Paths paths, string filePath, Dictionary<string, Core.Project> projects, Dictionary<Core.Project, List<string>> dependencies, Dictionary<string, string> mapping, string configPlatform)
        {
            var solutionPath = paths.Value("SolutionDir");
            var proj = new CSProj(logger, filePath, paths, configPlatform);

            var project = new Core.CSharp();
            projects[proj.Name] = project;
            project.CompileDefinitions.AddRange(proj.DefineConstants());

            dependencies[project] = proj.Dependencies();
            proj.Compiles(project.FilePaths, logger, paths);
            mapping[filePath] = proj.Name;
        }
    }
}
