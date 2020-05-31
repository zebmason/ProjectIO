//------------------------------------------------------------------------------
// <copyright file="VCProj.cs" company="Zebedee Mason">
//     Copyright (c) 2020 Zebedee Mason.
// </copyright>
//------------------------------------------------------------------------------

namespace ProjectIO.VisualStudio
{
    using System.Collections.Generic;

    public class VCProj : Proj
    {
        public VCProj(ProjectPath path)
            : base(path)
        {
        }

        public override string Name
        {
            get
            {
                return System.IO.Path.GetFileNameWithoutExtension(path.FilePath);
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
            var xml2 = new XMLUtils(path.FilePath + ".filters");
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

                var fullName = path.Combine(filename);
                dict2[fullName] = filter;
            }

            return dict2;
        }

        public Dictionary<string, string> Files(bool filterFile, string name, string sourceDirec)
        {
            if (filterFile)
                return Filters(name, sourceDirec);

            var list = xml.Compiles(name);

            var dict2 = new Dictionary<string, string>();
            foreach (var filename in list)
            {
                var filter = PathToFilter(filename, sourceDirec);

                var fullName = path.Combine(filename);
                dict2[fullName] = filter;
            }

            return dict2;
        }

        public HashSet<string> Includes()
        {
            var includes = new HashSet<string>();
            var direc = System.IO.Path.GetDirectoryName(path.FilePath);

            var nodes = new List<System.Xml.XmlElement>();
            xml.SelectNodes(xml.root, "AdditionalIncludeDirectories", nodes);
            xml.SelectNodes(xml.root, "NMakeIncludeSearchPath", nodes);
            xml.SelectNodes(xml.root, "IncludePath", nodes);

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
                    include = path.Combine(include);
                    includes.Add(include);
                }
            }

            return includes;
        }

        public static void Extract(Core.ILogger logger, string filePath, string sourceDirec, Dictionary<string, Core.Project> projects, Dictionary<string, string> filters)
        {
            var proj = new VCProj(new ProjectPath(filePath, sourceDirec));

            projects[proj.Name] = new Core.Project("C++");
            projects[proj.Name].IncludeDirectories.AddRange(proj.Includes());
            foreach (var dep in proj.Dependencies())
            {
                var stub = System.IO.Path.GetFileNameWithoutExtension(dep);
                projects[proj.Name].Libraries.Add(stub);
            }

            foreach (var type in new string[] { "ClInclude", "ClCompile" })
            {
                foreach (var pair2 in proj.Files(false, type, sourceDirec))
                {
                    var fullName = pair2.Key;
                    var filter = pair2.Value;

                    logger.Info("Appended {}", fullName);
                    projects[proj.Name].FilePaths.Add(fullName);
                    filters.Add(fullName, filter);
                }
            }
        }
    }
}
