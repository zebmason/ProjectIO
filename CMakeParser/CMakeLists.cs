//------------------------------------------------------------------------------
// <copyright file="CMakeLists.cs" company="Zebedee Mason">
//     Copyright (c) 2019 Zebedee Mason.
// </copyright>
//------------------------------------------------------------------------------

namespace CMakeParser
{
    using System.Collections.Generic;
    using System.Linq;

    public class CMakeLists
    {
        private readonly ILogger _logger;

        private State _state;

        private readonly Dictionary<string, Command.ICommand> _commands = new Dictionary<string, Command.ICommand>();

        public CMakeLists(State state, ILogger notHandled)
        {
            _state = state;
            _logger = notHandled;
        }

        public void AddCommand(string name, Command.ICommand command)
        {
            command.Initialise(_state);
            _commands[name] = command;
        }

        public static KeyValuePair<string, string> Command(string line)
        {
            var index = line.IndexOf("(");
            var command = line.Substring(0, index).Trim();
            var args = line.Substring(index + 1).Trim();
            args = args.Substring(0, args.Length - 1).Trim();
            return new KeyValuePair<string, string>(command.ToLower(), args);
        }

        private bool AddSubdirectory(KeyValuePair<string, string> command)
        {
            if (command.Key != "add_subdirectory")
            {
                return false;
            }

            var statter = _state;
            _state = _state.SubDirectory(command.Value);
            Read();
            _state = statter;
            return true;
        }

        private bool ForEach(KeyValuePair<string, string> command, Block block)
        {
            var pair = Utilities.Split(command.Value);
            var variable = "${" + pair.Key + "}";
            foreach (var file in _state.SplitList(pair.Value))
            {
                _state.Variables[variable] = file;
                Parse(block._blocks);
            }

            return true;
        }

        private class Block
        {
            public string _line = string.Empty;

            public List<Block> _blocks = null;

            public Block(string line)
            {
                _line = line;
            }

            public Block()
            {
                _blocks = new List<Block>();
            }
        }

        public void Read()
        {
            var filepath = _state.FileOrDirectoryList("${CMAKE_CURRENT_SOURCE_DIR}/CMakeLists.txt")[0];
            var lines = System.IO.File.ReadAllLines(filepath);
            var stack = new List<Block>
            {
                new Block()
            };
            var part = string.Empty;
            foreach (var l in lines)
            {
                var line = l.Trim();

                var index = line.IndexOf('#');
                if (index != -1)
                {
                    line = line.Substring(0, index);
                }

                line = line.Trim().Replace("\t", " ");
                if (line.Length == 0)
                {
                    continue;
                }

                part += " " + line;
                if (line[line.Length - 1] != ')')
                {
                    continue;
                }

                var command = Command(part);

                string[] endOfBlock = new string[] { "endforeach", "endwhile", "elseif", "else", "endif" };
                if (endOfBlock.Contains(command.Key))
                    stack.RemoveAt(0);

                string[] startOfBlock = new string[] { "foreach", "while", "if", "elseif", "else" };
                if (startOfBlock.Contains(command.Key))
                {
                    var current = new Block();
                    current._line = part;
                    stack[0]._blocks.Add(current);
                    stack.Insert(0, current);
                }
                else
                {
                    stack[0]._blocks.Add(new Block(part));
                }

                part = string.Empty;
            }

            Parse(stack[0]._blocks);
        }

        private void Parse(List<Block> blocks)
        {
            foreach (var block in blocks)
            {
                var line = block._line;
                var command = Command(line);

                if (command.Key == "add_subdirectory")
                {
                    AddSubdirectory(command);
                    continue;
                }

                if (command.Key == "foreach")
                {
                    ForEach(command, block);
                    continue;
                }

                if (_commands.ContainsKey(command.Key))
                {
                    _commands[command.Key].Command(command, _state);
                    continue;
                }

                _logger.Message(string.Format("Skipping {0}", line), _state);
            }
        }
    }
}
