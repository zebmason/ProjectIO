//------------------------------------------------------------------------------
// <copyright file="CMakeProject.cs" company="Zebedee Mason">
//     Copyright (c) 2020 Zebedee Mason.
// </copyright>
//------------------------------------------------------------------------------

namespace ProjectIO.VisualStudioToCMake
{
    using System.Collections.Generic;
    using System.Linq;

    internal class CMakeProject
    {
        public List<Core.Project> Projects { get; } = new List<Core.Project>();

        public List<string> Subdirectories { get; } = new List<string>();

        private string _directory;

        private System.IO.StreamWriter _file;

        private CMakeProject(string directory)
        {
            _directory = directory;
        }

        private void WriteProject(string rootDirec, string slnName)
        {
            if (rootDirec != _directory)
            {
                return;
            }

            var template = @"# Project initialisation
project(""SOLUTIONNAME"")

# Set a default build type if none was specified
set(default_build_type ""RelWithDebInfo"")

if(NOT CMAKE_BUILD_TYPE AND NOT CMAKE_CONFIGURATION_TYPES)
  message(STATUS ""Setting build type to '${default_build_type}' as none was specified."")
  set(CMAKE_BUILD_TYPE ""${default_build_type}"" CACHE
      STRING ""Choose the type of build."" FORCE)
  # Set the possible values of build type for cmake-gui
  set_property(CACHE CMAKE_BUILD_TYPE PROPERTY STRINGS
    ""Debug"" ""Release"" ""MinSizeRel"" ""RelWithDebInfo"")
endif()

# Specify the C++ standard
set(CMAKE_CXX_STANDARD 14)

";
            template = template.Replace("SOLUTIONNAME", slnName);
            _file.Write(template);
        }

        private void WriteFiles(Core.Project project, string rootDirec)
        {
            if (project.FilePaths.Count == 0)
            {
                return;
            }

            _file.WriteLine("# Set up the IDE");

            var dirName = System.IO.Path.GetFileName(_directory);
            if (dirName.Length == 0)
                dirName = _directory;

            var upper = dirName.ToUpper();

            var headers = new List<string>();
            var sources = new List<string>();
            foreach (var filePath in project.FilePaths)
            {
                if (Core.Cpp.IsHeader(filePath))
                {
                    headers.Add(filePath.Substring(_directory.Length + 1));
                }
                else
                {
                    sources.Add(filePath.Substring(_directory.Length + 1));
                }
            }

            if (sources.Count > 0)
            {
                _file.WriteLine(string.Format("set({0}_SRC_FILES {1})", upper, string.Join(" ", sources)));
            }

            if (headers.Count > 0)
            {
                _file.WriteLine(string.Format("set({0}_HDR_FILES {1})", upper, string.Join(" ", headers)));
            }

            var index = System.IO.Path.GetDirectoryName(rootDirec).Length;
            var filter = _directory.Substring(index + 1);
            filter = filter.Replace("\\", "\\\\");
            _file.Write(string.Format("source_group({0} FILES", filter));

            if (sources.Count > 0)
            {
                _file.Write(string.Format(" ${{{0}_SRC_FILES}}", upper));
            }

            if (headers.Count > 0)
            {
                _file.Write(string.Format(" ${{{0}_HDR_FILES}}", upper));
            }

            _file.WriteLine(")");
            _file.WriteLine();

            if (sources.Count > 0)
            {
                var lower = dirName.ToLower();
                if (project.IsExe)
                {
                    _file.Write(string.Format("add_executable({0} ${{{1}_SRC_FILES}}", lower, upper));
                }
                else
                {
                    _file.Write(string.Format("add_library({0}lib ${{{1}_SRC_FILES}}", lower, upper));
                }

                if (headers.Count > 0)
                {
                    _file.Write(string.Format(" ${{{0}_HDR_FILES}}", upper));
                }

                _file.WriteLine(")");
            }
        }

        private void WriteDirectories()
        {
            if (Subdirectories.Count == 0)
            {
                return;

            }

            Subdirectories.Sort();
            var template = "add_subdirectory(DIREC)";
            foreach (var direc in Subdirectories)
            {
                _file.WriteLine(template.Replace("DIREC", direc));
            }
        }

        public void Write(string rootDirec, string slnName)
        {
            string path = System.IO.Path.Combine(_directory, "CMakeLists.txt");
            _file = new System.IO.StreamWriter(path);
            WriteProject(rootDirec, slnName);
            foreach (var project in Projects)
            {
                WriteFiles(project, rootDirec);
            }

            WriteDirectories();
            _file.Close();
        }

        private static CMakeProject Add(Dictionary<string, CMakeProject> cmakes, string direc)
        {
            if (!cmakes.ContainsKey(direc))
            {
                cmakes[direc] = new CMakeProject(direc);
            }

            return cmakes[direc];
        }

        private static void CMakeLists(Core.Project proj, string rootDirec, string name, Dictionary<string, CMakeProject> cmakes)
        {
            var dict = new Dictionary<string, List<string>>();
            foreach (var filePath in proj.FilePaths)
            {
                var direc = System.IO.Path.GetDirectoryName(filePath);
                if (!dict.ContainsKey(direc))
                {
                    dict[direc] = new List<string>();
                }

                dict[direc].Add(filePath);
            }

            var index = System.IO.Path.GetDirectoryName(rootDirec).Length;
            foreach (var direc in dict.Keys)
            {
                if (direc.IndexOf(rootDirec) != 0)
                {
                    continue;
                }

                var p = Add(cmakes, direc);
                var p2 = new Core.Project();
                p2.IsExe = proj.IsExe;
                p2.FilePaths.AddRange(dict[direc]);
                p.Projects.Add(p2);
            }
        }

        public static void SubDirectories(Dictionary<string, CMakeProject> cmakes, string rootDirec)
        {
            foreach (var direc in cmakes.Keys.ToList())
            {
                if (direc == rootDirec)
                {
                    continue;
                }

                string d = direc;
                while ((d = System.IO.Path.GetDirectoryName(d)) != rootDirec)
                {
                    if (!cmakes.ContainsKey(d))
                    {
                        Add(cmakes, d);
                    }
                }
            }

            foreach (var direc in cmakes.Keys)
            {
                var p = cmakes[direc];
                foreach (var d in cmakes.Keys)
                {
                    if (System.IO.Path.GetDirectoryName(d) == direc)
                    {
                        p.Subdirectories.Add(d.Substring(direc.Length + 1));
                    }
                }
            }
        }

        public static void Write(Dictionary<string, CMakeProject> cmakes, string rootDirec, string name)
        {
            foreach (var p in cmakes.Values)
            {
                p.Write(rootDirec, name);
            }
        }

        public static void Assemble(string rootDirec, string name, Dictionary<string, Core.Project> projects)
        {
            var cmakes = new Dictionary<string, CMakeProject>();
            Add(cmakes, rootDirec);
            foreach (var proj in projects.Values)
            {
                CMakeLists(proj, rootDirec, name, cmakes);
            }

            SubDirectories(cmakes, rootDirec);

            Write(cmakes, rootDirec, name);
        }
    }
}
