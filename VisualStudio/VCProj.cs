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
        public VCProj(string path, Core.Paths paths)
            : base(path, paths)
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
            var direc = System.IO.Path.GetDirectoryName(_filePath);

            var nodes = new List<System.Xml.XmlElement>();
            _xml.SelectNodes(_xml._root, "AdditionalIncludeDirectories", nodes);
            _xml.SelectNodes(_xml._root, "NMakeIncludeSearchPath", nodes);
            _xml.SelectNodes(_xml._root, "IncludePath", nodes);

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

        public static string Extract(Core.ILogger logger, Core.Paths paths, string filePath, Dictionary<string, Core.Project> projects, Dictionary<string, string> filters, Dictionary<Core.Project, List<string>> dependencies)
        {
            var sourceDirec = paths.Value("SolutionDir");
            var proj = new VCProj(filePath, paths);

            var project = new Core.Cpp();
            projects[proj.Name] = project;
            project.IncludeDirectories.AddRange(proj.Includes());
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

            return proj.Name;
        }
    }
}
