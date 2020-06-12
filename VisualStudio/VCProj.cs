//------------------------------------------------------------------------------
// <copyright file="VCProj.cs" company="Zebedee Mason">
//     Copyright (c) 2020 Zebedee Mason.
// </copyright>
//------------------------------------------------------------------------------

namespace ProjectIO.VisualStudio
{
    using System.Collections.Generic;

    internal class VCProj : Proj
    {
        public VCProj(Core.ILogger logger, string path, Core.Paths paths, string configPlatform)
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

        public override List<string> Externals()
        {
            var list = new List<string>();
            return list;
        }

        public static string PathToFilter(string filename, string sourceDirec)
        {
            var filter = System.IO.Path.GetDirectoryName(filename);
            var common = System.IO.Path.Combine(filter, sourceDirec);

            if (common.Length < filter.Length)
            {
                filter = filter.Substring(common.Length + 1);
            }
            else
            {
                filter = string.Empty;
            }

            return filter;
        }

        private Dictionary<string, string> Filters(string name, string sourceDirec)
        {
            var xml2 = new XMLUtils(_filePath + ".filters");
            var dict = xml2.Filters(name);

            var dict2 = new Dictionary<string, string>();
            foreach (var pair in dict)
            {
                var filename = pair.Key;
                var filter = pair.Value;

                if (filter == ".")
                {
                    filter = PathToFilter(filename, sourceDirec);
                }

                var fullName = _paths.Combine(filename);
                dict2[fullName] = filter;
            }

            return dict2;
        }

        public Dictionary<string, string> Files(bool filterFile, string name, string sourceDirec)
        {
            if (filterFile)
                return Filters(name, sourceDirec);

            var list = _xml.Compiles(name);

            var dict2 = new Dictionary<string, string>();
            foreach (var filename in list)
            {
                var filter = PathToFilter(filename, sourceDirec);

                var fullName = _paths.Combine(filename);
                dict2[fullName] = filter;
            }

            return dict2;
        }

        public HashSet<string> Includes()
        {
            var includes = new HashSet<string>();
            var group = _xml.Group("ItemDefinitionGroup", _configPlatform);
            if (group is null)
            {
                _logger.Warn(string.Format("No ItemDefinitionGroup {0} in \"{1}\"", _configPlatform, _filePath));
                return includes;
            }

            var nodes = new List<System.Xml.XmlElement>();
            _xml.SelectNodes(group, "AdditionalIncludeDirectories", nodes);
            _xml.SelectNodes(group, "NMakeIncludeSearchPath", nodes);
            _xml.SelectNodes(group, "IncludePath", nodes);

            foreach (var node in nodes)
            {
                var line = node.InnerText;
                foreach (var inc in line.Split(';'))
                {
                    if (inc == "%(AdditionalIncludeDirectories)" || inc.Length == 0)
                    {
                        continue;
                    }

                    var include = inc;
                    include = _paths.Combine(include);
                    includes.Add(include);
                }
            }

            return includes;
        }

        public List<string> CompileDefinitions()
        {
            var defns = new List<string>();
            var group = _xml.Group("ItemDefinitionGroup", _configPlatform);
            if (group is null)
            {
                _logger.Warn(string.Format("No ItemDefinitionGroup {0} in \"{1}\"", _configPlatform, _filePath));
                return defns;
            }

            var nodes = new List<System.Xml.XmlElement>();
            _xml.SelectNodes(group, "PreprocessorDefinitions", nodes, true);

            foreach (var node in nodes)
            {
                var line = node.InnerText;
                foreach (var defn in line.Split(';'))
                {
                    if (defn == "%(PreprocessorDefinitions)" || defn.Length == 0)
                    {
                        continue;
                    }

                    defns.Add(defn);
                }
            }

            return defns;
        }

        public static void Extract(Core.ILogger logger, Core.Paths paths, string filePath, Dictionary<string, Core.Project> projects, Dictionary<string, string> filters, Dictionary<Core.Project, List<string>> dependencies, Dictionary<string, string> mapping, string configPlatform)
        {
            var sourceDirec = paths.Value("SolutionDir");
            var proj = new VCProj(logger, filePath, paths, configPlatform);

            var project = new Core.Cpp();
            projects[proj.Name] = project;
            project.IncludeDirectories.AddRange(proj.Includes());
            project.CompileDefinitions.AddRange(proj.CompileDefinitions());
            if (project.CompileDefinitions.Contains("_CONSOLE"))
            {
                project.CompileDefinitions.Remove("_CONSOLE");
                project.IsExe = true;
            }

            dependencies[projects[proj.Name]] = proj.Dependencies();

            foreach (var type in new string[] { "ClInclude", "ClCompile" })
            {
                foreach (var pair2 in proj.Files(false, type, sourceDirec))
                {
                    var fullName = pair2.Key;
                    var filter = pair2.Value;

                    logger.Info("Appended \"{0}\"", fullName);
                    project.FilePaths.Add(fullName);
                    filters.Add(fullName, filter);
                }
            }

            mapping[filePath] = proj.Name;
        }
    }
}
