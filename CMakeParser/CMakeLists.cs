//------------------------------------------------------------------------------
// <copyright file="CMakeLists.cs" company="Zebedee Mason">
//     Copyright (c) 2019 Zebedee Mason.
// </copyright>
//------------------------------------------------------------------------------

namespace ProjectIO.CMakeParser
{
    using System.Collections.Generic;
    using System.Linq;

    public class CMakeLists
    {
        private readonly ILogger _logger;

        private State _state;

        private readonly Dictionary<string, CMakeParser.ICommand> _commands = new Dictionary<string, CMakeParser.ICommand>();

        public CMakeLists(State state, ILogger notHandled)
        {
            _state = state;
            _logger = notHandled;
        }

        public void AddCommand(string name, CMakeParser.ICommand command)
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

        private bool IfElseIfElse(KeyValuePair<string, string> command, Block block)
        {
            if (command.Key != "else")
            {
                var bits = command.Value.Split().ToList();

                int i = 0;
                var logicals = new string[] { "TRUE", "FALSE", "AND", "OR", "NOT" };
                for (; i < bits.Count; ++i)
                {
                    if (logicals.Contains(bits[i]))
                        continue;

                    if (_state.Switches.ContainsKey(bits[i]))
                        bits[i] = _state.Switches[bits[i]] ? "TRUE" : "FALSE";
                    else if (_state.Variables.ContainsKey("${" + bits[i] + "}"))
                        bits[i] = "TRUE";
                    else
                    {
                        _logger.Warn(string.Format("Skipping {0}({1}) at {2}", command.Key, command.Value, bits[i]), _state);
                        return false;
                    }
                }

                while ((i = bits.IndexOf("NOT")) != -1)
                {
                    bits[i] = bits[i + 1] == "TRUE" ? "FALSE" : "TRUE";
                    bits.RemoveAt(i + 1);
                }

                while ((i = bits.IndexOf("AND")) != -1)
                {
                    bits[i - 1] = bits[i - 1] == "TRUE" && bits[i + 1] == "TRUE" ? "TRUE" : "FALSE";
                    bits.RemoveAt(i);
                    bits.RemoveAt(i);
                }

                while ((i = bits.IndexOf("OR")) != -1)
                {
                    bits[i - 1] = bits[i - 1] == "TRUE" || bits[i + 1] == "TRUE" ? "TRUE" : "FALSE";
                    bits.RemoveAt(i);
                    bits.RemoveAt(i);
                }

                if (bits.Count != 1)
                {
                    _logger.Warn(string.Format("Skipping {0}({1}) == {2}", command.Key, command.Value, string.Join(" ", bits)), _state);
                    return false;
                }

                if (bits[0] == "FALSE")
                    return true;
            }

            Parse(block._blocks);
            return false;
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
            bool evaluateIf = true;
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

                if (command.Key == "endforeach")
                {
                    continue;
                }

                if (command.Key == "if" || command.Key == "elseif" || command.Key == "else")
                {
                    if (!evaluateIf)
                        continue;
                    evaluateIf = IfElseIfElse(command, block);
                    continue;
                }

                if (command.Key == "endif")
                {
                    evaluateIf = true;
                    continue;
                }

                if (_commands.ContainsKey(command.Key))
                {
                    _commands[command.Key].Command(command, _state);
                    continue;
                }

                _logger.Warn(string.Format("Skipping {0}", line.Trim()), _state);
            }
        }
    }
}
