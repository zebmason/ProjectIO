using System;
using System.Collections.Generic;

namespace CMakeParser
{
    public class Program
    {
        public class Logger : ILogger
        {
            public void Message(string message, State state)
            {
                Console.WriteLine(string.Format("[{0}] {1}", state.Variables["${CMAKE_CURRENT_SOURCE_DIR}"], message));
            }

            public void Unhandled(KeyValuePair<string, string> command, State state)
            {
                Console.WriteLine(string.Format("[{0}] Unhandled {1}({2})", state.Variables["${CMAKE_CURRENT_SOURCE_DIR}"], command.Key, command.Value));
            }
        }

        public class AddBinaryHandler : Command.AddBinary.IHandler
        {
            public void AddFileToBinary(string name, string filePath, State state, HashSet<string> includes, List<string> defines)
            {
                if (System.IO.File.Exists(filePath))
                {
                    Console.WriteLine(string.Format("[{0}] Adding {1} to {2}", state.Variables["${CMAKE_CURRENT_SOURCE_DIR}"], filePath, name));
                }
                else
                {
                    Console.WriteLine(string.Format("[{0}] Not adding {1} to {2}", state.Variables["${CMAKE_CURRENT_SOURCE_DIR}"], filePath, name));
                }
            }
        }

        public class SourceGroupHandler : Command.SourceGroup.IHandler
        {
            public void AddFile(string filePath, string filter)
            {
                Console.WriteLine(string.Format("Setting {0} to filter {1}", filePath, filter));
            }
        }

        public static CMakeLists Instance(State state)
        {
            var log = new Logger();
            var lists = new CMakeLists(state, log);

            var addBinary = new Command.AddBinary(new AddBinaryHandler());
            var binaryCommands = new string[] { "add_executable", "add_library", "catkin_add_gtest" };
            foreach (var command in binaryCommands)
            {
                lists.AddCommand(command, addBinary);
            }

            var ignore = new Command.Ignore();
            var ignoreCommands = new string[] { "target_link_libraries", "add_dependencies", "add_test", "function", "endfunction", "option", "enable_testing", "configure_file", "find_package", "catkin_package", "if", "else", "elseif", "endif", "install", "project", "string", "message", "cmake_minimum_required", "set_target_properties", "endforeach", "list", "add_custom_command", "add_custom_target", "list", "execute_process", "find_library", "generate_messages", "add_action_files" };
            foreach (var command in ignoreCommands)
            {
                lists.AddCommand(command, ignore);
            }

            lists.AddCommand("set", new Command.Set());

            lists.AddCommand("file", new Command.File(log));

            lists.AddCommand("source_group", new Command.SourceGroup(new SourceGroupHandler()));

            lists.AddCommand("include_directories", new Command.IncludeDirectories());

            return lists;
        }

        public static void Main(string[] args)
        {
            var state = new State(args[0], (args.Length > 1) ? args[1] : string.Empty);
            var cmake = Instance(state);
            cmake.Read();
        }
    }
}
