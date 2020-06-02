//------------------------------------------------------------------------------
// <copyright file="Builder.cs" company="Zebedee Mason">
//     Copyright (c) 2020 Zebedee Mason.
// </copyright>
//------------------------------------------------------------------------------

namespace ProjectIO.CMakeParser
{
    using System.Collections.Generic;

    public class Builder
    {
        internal class AddBinaryHandler : AddBinary.IHandler
        {
            private readonly Core.ILogger _logger;

            private readonly Dictionary<string, Core.Project> _projects;

            public AddBinaryHandler(Core.ILogger logger, Dictionary<string, Core.Project> projects)
            {
                _logger = logger;
                _projects = projects;
            }

            public void Add(string command, string name, State state, IEnumerable<string> filePaths)
            {
                var project = new Core.Cpp()
                {
                    CompileDefinitions = state.Properties["COMPILE_DEFINITIONS"],
                    IsExe = command != "add_library"
                };

                project.IncludeDirectories.AddRange(state.IncludeDirectories);
                project.FilePaths.AddRange(filePaths);
                _projects[name] = project;
            }
        }

        internal class TargetLinkLibrariesHandler : TargetLinkLibraries.IHandler
        {
            private readonly Dictionary<string, Core.Project> _projects;

            public TargetLinkLibrariesHandler(Dictionary<string, Core.Project> projects)
            {
                _projects = projects;
            }

            public void AddLibrariesToBinary(string name, IEnumerable<string> libraries)
            {
                _projects[name].Dependencies.AddRange(libraries);
            }
        }

        internal class TargetCompileDefinitionsHandler : TargetCompileDefinitions.IHandler
        {
            private readonly Dictionary<string, Core.Project> _projects;

            public TargetCompileDefinitionsHandler(Dictionary<string, Core.Project> projects)
            {
                _projects = projects;
            }

            public void AddCompileDefinitionsToBinary(string name, string definitions)
            {
                var project = _projects[name] as Core.Cpp;
                project.CompileDefinitions += " " + definitions;
            }
        }

        internal class TargetIncludeDirectoriesHandler : TargetIncludeDirectories.IHandler
        {
            private readonly Dictionary<string, Core.Project> _projects;

            public TargetIncludeDirectoriesHandler(Dictionary<string, Core.Project> projects)
            {
                _projects = projects;
            }

            public void AddIncludeDirectoriesToBinary(string name, IEnumerable<string> directories)
            {
                var project = _projects[name] as Core.Cpp;
                project.IncludeDirectories.AddRange(directories);
            }
        }

        internal class SourceGroupHandler : SourceGroup.IHandler
        {
            private readonly Dictionary<string, string> _filters;

            public SourceGroupHandler(Dictionary<string, string> filters)
            {
                _filters = filters;
            }

            public void AddFile(string filePath, string filter)
            {
                _filters[filePath] = filter;
            }
        }

        internal static CMakeLists Instance(State state, Dictionary<string, Core.Project> projects, Dictionary<string, string> filters, Core.ILogger logger)
        {
            var log = new Logger(logger);
            var lists = new CMakeLists(state, log);

            var addBinary = new AddBinary(new AddBinaryHandler(logger, projects));
            var binaryCommands = new string[] { "add_executable", "add_library", "catkin_add_gtest" };
            foreach (var command in binaryCommands)
            {
                lists.AddCommand(command, addBinary);
            }

            var targetLinkLibraries = new TargetLinkLibraries(new TargetLinkLibrariesHandler(projects));
            lists.AddCommand("target_link_libraries", targetLinkLibraries);

            var targetCompileDefinitions = new TargetCompileDefinitions(new TargetCompileDefinitionsHandler(projects));
            lists.AddCommand("target_compile_definitions", targetCompileDefinitions);

            var targetIncludeDirectories = new TargetIncludeDirectories(new TargetIncludeDirectoriesHandler(projects));
            lists.AddCommand("target_include_directories", targetIncludeDirectories);

            var ignore = new Ignore();
            var ignoreCommands = new string[] { "add_custom_target", "add_dependencies", "add_test", "cmake_minimum_required", "enable_testing", "fetchcontent_declare", "fetchcontent_getproperties", "find_package", "include", "option", "project" };
            foreach (var command in ignoreCommands)
            {
                lists.AddCommand(command, ignore);
            }

            lists.AddCommand("set", new Set());

            lists.AddCommand("file", new File(log));

            lists.AddCommand("source_group", new SourceGroup(new SourceGroupHandler(filters), log));

            lists.AddCommand("include_directories", new IncludeDirectories());

            lists.AddCommand("get_filename_component", new GetFileNameComponent(log));

            lists.AddCommand("add_compile_definitions", new AddCompileDefinitions());

            return lists;
        }

        public static string Extract(Core.ILogger logger, Core.Paths paths, List<string> filePaths, Dictionary<string, Core.Project> projects, Dictionary<string, string> filters)
        {
            string lists = string.Empty;
            string cache = string.Empty;
            foreach (var filePath in filePaths)
            {
                if (lists.Length == 0 && System.IO.Path.GetFileName(filePath) == "CMakeLists.txt")
                    lists = filePath;

                if (cache.Length == 0 && System.IO.Path.GetFileName(filePath) == "CMakeCache.txt")
                    cache = filePath;
            }

            if (lists.Length == 0)
                return "solution";

            logger.Info("Appended for reading \"{0}\"", lists);

            var sourceDirec = System.IO.Path.GetDirectoryName(lists);
            paths.Add("PROJECT_SOURCE_DIR", sourceDirec);
            filePaths.Remove(lists);

            var binaryDirec = string.Empty;
            if (cache.Length != 0)
            {
                binaryDirec = System.IO.Path.GetDirectoryName(cache);
                paths.Add("PROJECT_BINARY_DIR", binaryDirec);
                filePaths.Remove(cache);
            }

            logger.Info("Reading CMake");
            var state = new State(sourceDirec, binaryDirec);
            state.ReadCache(cache);

            var builder = Instance(state, projects, filters, logger);
            builder.Read();

            if (state.Variables.ContainsKey("${CMAKE_PROJECT_NAME}"))
                return state.Variables["${CMAKE_PROJECT_NAME}"];

            return "solution";
        }
    }
}
