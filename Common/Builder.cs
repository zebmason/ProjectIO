//------------------------------------------------------------------------------
// <copyright file="Builder.cs" company="Zebedee Mason">
//     Copyright (c) 2020 Zebedee Mason.
// </copyright>
//------------------------------------------------------------------------------

namespace CMakeParser.Common
{
    using System.Collections.Generic;
    using System.Linq;

    public class Builder
    {
        public class AddBinaryHandler : Core.AddBinary.IHandler
        {
            private readonly IWriter _writer;

            private readonly Dictionary<string, Binary> _binaries;

            public AddBinaryHandler(IWriter writer, Dictionary<string, Binary> binaries)
            {
                _writer = writer;
                _binaries = binaries;
            }

            public void Add(string command, string name, Core.State state, IEnumerable<string> filePaths)
            {
                if (command == "add_library")
                    _binaries[name] = new Binary(_writer, state.Properties, false);
                else
                    _binaries[name] = new Binary(_writer, state.Properties, true);

                _binaries[name].IncludeDirectories.AddRange(state.IncludeDirectories);
                _binaries[name].FilePaths.AddRange(filePaths);
            }
        }

        public class TargetLinkLibrariesHandler : Core.TargetLinkLibraries.IHandler
        {
            private readonly Dictionary<string, Binary> _binaries;

            public TargetLinkLibrariesHandler(Dictionary<string, Binary> binaries)
            {
                _binaries = binaries;
            }

            public void AddLibrariesToBinary(string name, IEnumerable<string> libraries)
            {
                _binaries[name].Libraries.AddRange(libraries);
            }
        }

        public class TargetCompileDefinitionsHandler : Core.TargetCompileDefinitions.IHandler
        {
            private readonly Dictionary<string, Binary> _binaries;

            public TargetCompileDefinitionsHandler(Dictionary<string, Binary> binaries)
            {
                _binaries = binaries;
            }

            public void AddCompileDefinitionsToBinary(string name, string definitions)
            {
                _binaries[name].CompileDefinitions += " " + definitions;
            }
        }

        public class TargetIncludeDirectoriesHandler : Core.TargetIncludeDirectories.IHandler
        {
            private readonly Dictionary<string, Binary> _binaries;

            public TargetIncludeDirectoriesHandler(Dictionary<string, Binary> binaries)
            {
                _binaries = binaries;
            }

            public void AddIncludeDirectoriesToBinary(string name, IEnumerable<string> directories)
            {
                _binaries[name].IncludeDirectories.AddRange(directories);
            }
        }

        public class SourceGroupHandler : Core.SourceGroup.IHandler
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

        public static Core.CMakeLists Instance(Core.State state, Dictionary<string, Binary> binaries, Dictionary<string, string> filters, IWriter writer)
        {
            var log = new Logger(writer);
            var lists = new Core.CMakeLists(state, log);

            var addBinary = new Core.AddBinary(new AddBinaryHandler(writer, binaries));
            var binaryCommands = new string[] { "add_executable", "add_library", "catkin_add_gtest" };
            foreach (var command in binaryCommands)
            {
                lists.AddCommand(command, addBinary);
            }

            var targetLinkLibraries = new Core.TargetLinkLibraries(new TargetLinkLibrariesHandler(binaries));
            lists.AddCommand("target_link_libraries", targetLinkLibraries);

            var targetCompileDefinitions = new Core.TargetCompileDefinitions(new TargetCompileDefinitionsHandler(binaries));
            lists.AddCommand("target_compile_definitions", targetCompileDefinitions);

            var targetIncludeDirectories = new Core.TargetIncludeDirectories(new TargetIncludeDirectoriesHandler(binaries));
            lists.AddCommand("target_include_directories", targetIncludeDirectories);

            var ignore = new Core.Ignore();
            var ignoreCommands = new string[] { "add_custom_target", "add_dependencies", "add_test", "cmake_minimum_required", "enable_testing", "fetchcontent_declare", "fetchcontent_getproperties", "find_package", "include", "option", "project" };
            foreach (var command in ignoreCommands)
            {
                lists.AddCommand(command, ignore);
            }

            lists.AddCommand("set", new Core.Set());

            lists.AddCommand("file", new Core.File(log));

            lists.AddCommand("source_group", new Core.SourceGroup(new SourceGroupHandler(filters)));

            lists.AddCommand("include_directories", new Core.IncludeDirectories());

            lists.AddCommand("get_filename_component", new Core.GetFileNameComponent());

            lists.AddCommand("add_compile_definitions", new Core.AddCompileDefinitions());

            return lists;
        }
    }
}
