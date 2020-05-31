//------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Zebedee Mason">
//     Copyright (c) 2019-2020 Zebedee Mason.
// </copyright>
//------------------------------------------------------------------------------

namespace ProjectIO.CMakeLister
{
    using System.Collections.Generic;

    public class Program
    {
        public class AddBinaryHandler : CMakeParser.AddBinary.IHandler
        {
            private readonly CMakeParser.IWriter _writer;

            public AddBinaryHandler(CMakeParser.IWriter writer)
            {
                _writer = writer;
            }

            public void Add(string command, string name, CMakeParser.State state, IEnumerable<string> filePaths)
            {
                foreach (var filePath in filePaths)
                {
                    if (System.IO.File.Exists(filePath))
                    {
                        _writer.WriteLine(string.Format("[{0}] Adding {1} to {2}", state.Variables["${CMAKE_CURRENT_SOURCE_DIR}"], filePath, name));
                    }
                    else
                    {
                        _writer.WriteLine(string.Format("[{0}] Not adding {1} to {2}", state.Variables["${CMAKE_CURRENT_SOURCE_DIR}"], filePath, name));
                    }
                }
            }
        }

        public class SourceGroupHandler : CMakeParser.SourceGroup.IHandler
        {
            private readonly CMakeParser.IWriter _writer;

            public SourceGroupHandler(CMakeParser.IWriter writer)
            {
                _writer = writer;
            }

            public void AddFile(string filePath, string filter)
            {
                _writer.WriteLine(string.Format("Setting {0} to filter {1}", filePath, filter));
            }
        }

        public static CMakeParser.CMakeLists Instance(CMakeParser.State state, CMakeParser.IWriter writer)
        {
            var log = new CMakeParser.Logger(writer);
            var lists = new CMakeParser.CMakeLists(state, log);

            var addBinary = new CMakeParser.AddBinary(new AddBinaryHandler(writer));
            var binaryCommands = new string[] { "add_executable", "add_library", "catkin_add_gtest" };
            foreach (var command in binaryCommands)
            {
                lists.AddCommand(command, addBinary);
            }

            var ignore = new CMakeParser.Ignore();
            var ignoreCommands = new string[] { "target_link_libraries", "add_dependencies", "add_test", "function", "endfunction", "option", "enable_testing", "configure_file", "find_package", "catkin_package", "install", "project", "string", "message", "cmake_minimum_required", "set_target_properties", "list", "add_custom_command", "add_custom_target", "execute_process", "find_library", "generate_messages", "add_action_files" };
            foreach (var command in ignoreCommands)
            {
                lists.AddCommand(command, ignore);
            }

            lists.AddCommand("set", new CMakeParser.Set());

            lists.AddCommand("file", new CMakeParser.File(log));

            lists.AddCommand("source_group", new CMakeParser.SourceGroup(new SourceGroupHandler(writer), log));

            lists.AddCommand("include_directories", new CMakeParser.IncludeDirectories());

            return lists;
        }

        public static void MainFunc(string[] args, CMakeParser.IWriter writer)
        {
            var state = new CMakeParser.State(args[0], (args.Length > 1) ? args[1] : string.Empty);
            var cmake = Instance(state, writer);
            cmake.Read();
        }

        static void Main(string[] args)
        {
            MainFunc(args, new CMakeParser.Writer());
        }
    }
}
