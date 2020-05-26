//------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Zebedee Mason">
//     Copyright (c) 2019-2020 Zebedee Mason.
// </copyright>
//------------------------------------------------------------------------------

namespace CMakeParser.Lister
{
    using System.Collections.Generic;

    public class Program
    {
        public class AddBinaryHandler : Core.AddBinary.IHandler
        {
            private readonly Common.IWriter _writer;

            public AddBinaryHandler(Common.IWriter writer)
            {
                _writer = writer;
            }

            public void Add(string command, string name, Core.State state, IEnumerable<string> filePaths)
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

        public class SourceGroupHandler : Core.SourceGroup.IHandler
        {
            private readonly Common.IWriter _writer;

            public SourceGroupHandler(Common.IWriter writer)
            {
                _writer = writer;
            }

            public void AddFile(string filePath, string filter)
            {
                _writer.WriteLine(string.Format("Setting {0} to filter {1}", filePath, filter));
            }
        }

        public static Core.CMakeLists Instance(Core.State state, Common.IWriter writer)
        {
            var log = new Common.Logger(writer);
            var lists = new Core.CMakeLists(state, log);

            var addBinary = new Core.AddBinary(new AddBinaryHandler(writer));
            var binaryCommands = new string[] { "add_executable", "add_library", "catkin_add_gtest" };
            foreach (var command in binaryCommands)
            {
                lists.AddCommand(command, addBinary);
            }

            var ignore = new Core.Ignore();
            var ignoreCommands = new string[] { "target_link_libraries", "add_dependencies", "add_test", "function", "endfunction", "option", "enable_testing", "configure_file", "find_package", "catkin_package", "install", "project", "string", "message", "cmake_minimum_required", "set_target_properties", "list", "add_custom_command", "add_custom_target", "execute_process", "find_library", "generate_messages", "add_action_files" };
            foreach (var command in ignoreCommands)
            {
                lists.AddCommand(command, ignore);
            }

            lists.AddCommand("set", new Core.Set());

            lists.AddCommand("file", new Core.File(log));

            lists.AddCommand("source_group", new Core.SourceGroup(new SourceGroupHandler(writer)));

            lists.AddCommand("include_directories", new Core.IncludeDirectories());

            return lists;
        }

        public static void MainFunc(string[] args, Common.IWriter writer)
        {
            var state = new Core.State(args[0], (args.Length > 1) ? args[1] : string.Empty);
            var cmake = Instance(state, writer);
            cmake.Read();
        }

        static void Main(string[] args)
        {
            MainFunc(args, new Common.Writer());
        }
    }
}
