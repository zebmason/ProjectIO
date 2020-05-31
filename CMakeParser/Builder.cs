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
        public class AddBinaryHandler : AddBinary.IHandler
        {
            private readonly Core.ILogger _logger;

            private readonly Dictionary<string, Core.Project> _binaries;

            public AddBinaryHandler(Core.ILogger logger, Dictionary<string, Core.Project> binaries)
            {
                _logger = logger;
                _binaries = binaries;
            }

            public void Add(string command, string name, State state, IEnumerable<string> filePaths)
            {
                _binaries[name] = new Core.Project("C++")
                {
                    CompileDefinitions = state.Properties["COMPILE_DEFINITIONS"],
                    IsExe = command != "add_library"
                };

                _binaries[name].IncludeDirectories.AddRange(state.IncludeDirectories);
                _binaries[name].FilePaths.AddRange(filePaths);
            }
        }

        public class TargetLinkLibrariesHandler : TargetLinkLibraries.IHandler
        {
            private readonly Dictionary<string, Core.Project> _binaries;

            public TargetLinkLibrariesHandler(Dictionary<string, Core.Project> binaries)
            {
                _binaries = binaries;
            }

            public void AddLibrariesToBinary(string name, IEnumerable<string> libraries)
            {
                _binaries[name].Libraries.AddRange(libraries);
            }
        }

        public class TargetCompileDefinitionsHandler : TargetCompileDefinitions.IHandler
        {
            private readonly Dictionary<string, Core.Project> _binaries;

            public TargetCompileDefinitionsHandler(Dictionary<string, Core.Project> binaries)
            {
                _binaries = binaries;
            }

            public void AddCompileDefinitionsToBinary(string name, string definitions)
            {
                _binaries[name].CompileDefinitions += " " + definitions;
            }
        }

        public class TargetIncludeDirectoriesHandler : TargetIncludeDirectories.IHandler
        {
            private readonly Dictionary<string, Core.Project> _binaries;

            public TargetIncludeDirectoriesHandler(Dictionary<string, Core.Project> binaries)
            {
                _binaries = binaries;
            }

            public void AddIncludeDirectoriesToBinary(string name, IEnumerable<string> directories)
            {
                _binaries[name].IncludeDirectories.AddRange(directories);
            }
        }

        public class SourceGroupHandler : SourceGroup.IHandler
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

        public static CMakeLists Instance(State state, Dictionary<string, Core.Project> binaries, Dictionary<string, string> filters, Core.ILogger logger)
        {
            var log = new Logger(logger);
            var lists = new CMakeLists(state, log);

            var addBinary = new AddBinary(new AddBinaryHandler(logger, binaries));
            var binaryCommands = new string[] { "add_executable", "add_library", "catkin_add_gtest" };
            foreach (var command in binaryCommands)
            {
                lists.AddCommand(command, addBinary);
            }

            var targetLinkLibraries = new TargetLinkLibraries(new TargetLinkLibrariesHandler(binaries));
            lists.AddCommand("target_link_libraries", targetLinkLibraries);

            var targetCompileDefinitions = new TargetCompileDefinitions(new TargetCompileDefinitionsHandler(binaries));
            lists.AddCommand("target_compile_definitions", targetCompileDefinitions);

            var targetIncludeDirectories = new TargetIncludeDirectories(new TargetIncludeDirectoriesHandler(binaries));
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
    }
}
