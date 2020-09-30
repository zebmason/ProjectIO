//------------------------------------------------------------------------------
// <copyright file="State.cs" company="Zebedee Mason">
//     Copyright (c) 2019 Zebedee Mason.
// </copyright>
//------------------------------------------------------------------------------

namespace ProjectIO.CMakeParser
{
    using System.Collections.Generic;
    using System.Linq;

    public class State
    {
        public Dictionary<string, string> Variables { get; } = new Dictionary<string, string>();

        public Dictionary<string, bool> Switches { get; } = new Dictionary<string, bool>();

        public Dictionary<string, string> Properties { get; } = new Dictionary<string, string>();

        public List<string> IncludeDirectories { get; } = new List<string>();

        public List<string> CompileDefinitions { get; } = new List<string>();

        private static HashSet<string> _missingKeys = new HashSet<string>();

        private readonly Core.ILogger _logger;

        private readonly Core.Paths _paths;

        public State(Core.ILogger logger, string sourceDirec, string binaryDirec, Core.Paths paths)
        {
            Variables["${PROJECT_SOURCE_DIR}"] = sourceDirec;
            Variables["${PROJECT_BINARY_DIR}"] = binaryDirec;
            Variables["${CMAKE_CURRENT_LIST_DIR}"] = "${PROJECT_SOURCE_DIR}";
            Variables["${CMAKE_CURRENT_SOURCE_DIR}"] = "${PROJECT_SOURCE_DIR}";
            Variables["${CMAKE_CURRENT_BINARY_DIR}"] = "${PROJECT_BINARY_DIR}";

            Switches["WIN32"] = true;
            Switches["UNIX"] = false;

            _logger = logger;
            _paths = paths;
        }

        public State(State state)
        {
            Variables = state.Variables.ToDictionary(entry => entry.Key, entry => entry.Value);
            Properties = state.Properties.ToDictionary(entry => entry.Key, entry => entry.Value);
            Switches = state.Switches.ToDictionary(entry => entry.Key, entry => entry.Value);
            IncludeDirectories = state.IncludeDirectories.ToList();
            CompileDefinitions = state.CompileDefinitions.ToList();
            _logger = state._logger;
            _paths = state._paths;
        }

        public void Info(string message)
        {
            _logger.Info(string.Format("[{0}] {1}", Variables["${CMAKE_CURRENT_SOURCE_DIR}"], message));
        }

        public void Warn(string message)
        {
            _logger.Warn(string.Format("[{0}] {1}", Variables["${CMAKE_CURRENT_SOURCE_DIR}"], message));
        }

        public void Unhandled(KeyValuePair<string, string> command)
        {
            _logger.Warn(string.Format("[{0}] Unhandled {1}({2})", Variables["${CMAKE_CURRENT_SOURCE_DIR}"], command.Key, command.Value));
        }

        public State SubDirectory(string sub)
        {
            var state = new State(this);
            state.Variables["${CMAKE_CURRENT_SOURCE_DIR}"] = Variables["${CMAKE_CURRENT_SOURCE_DIR}"] + "/" + sub;
            state.Variables["${CMAKE_CURRENT_BINARY_DIR}"] = Variables["${CMAKE_CURRENT_BINARY_DIR}"] + "/" + sub;
            state.Variables["${CMAKE_CURRENT_LIST_DIR}"] = state.Variables["${CMAKE_CURRENT_SOURCE_DIR}"];
            return state;
        }

        private string Replace(string initial, int index)
        {
            index = initial.IndexOf("${", index);
            if (index == -1)
                return initial;

            var i2 = initial.IndexOf("}", index);
            var key = initial.Substring(index, i2 - index + 1);
            if (!Variables.ContainsKey(key))
            {
                if (!_missingKeys.Contains(key))
                {
                    Warn("Cache does not contain " + key);
                    _missingKeys.Add(key);
                }

                return Replace(initial, index + 1);
            }

            return Replace(initial.Replace(key, Variables[key]), index);
        }

        public string Replace(string initial)
        {
            initial = initial.Replace("$env{", "$ENV{");
            var index = initial.IndexOf("$ENV{");
            while (index != -1)
            {
                var i2 = initial.IndexOf("}", index);
                var env = initial.Substring(index + 5, i2 - index - 5);
                var val = System.Environment.GetEnvironmentVariable(env);
                initial = initial.Replace("$ENV{" + env + "}", val);
                index = initial.IndexOf("$ENV{");
                if (!_paths.ContainsAlias(env))
                {
                    if (val == null)
                    {
                        if (!_missingKeys.Contains(env))
                        {
                            Warn(string.Format("Environment does not contain $ENV{{{0}}}", env));
                            _missingKeys.Add(env);
                        }
                        continue;
                    }
                    var path = val.Replace("\"", string.Empty);
                    if (System.IO.Directory.Exists(path))
                    {
                        _paths.Add(env, path);
                    }
                }
            }

            return Replace(initial, 0);
        }

        private string FullPath(string path)
        {
            path = path.Replace("\"", string.Empty);

            if (path.Length == 0)
            {
                return string.Empty;
            }

            if (System.IO.Path.GetPathRoot(path).Length == 0)
            {
                path = System.IO.Path.Combine(Replace("${CMAKE_CURRENT_SOURCE_DIR}", 0), path);
            }

            return System.IO.Path.GetFullPath(path);
        }

        public List<string> FileOrDirectoryList(string list)
        {
            var items = new List<string>();
            foreach (var item in SplitList(list))
            {
                if (item.Contains("::"))
                    continue;

                items.Add(FullPath(item));
            }

            return items;
        }

        public List<string> SplitList(string list)
        {
            var items = new List<string>();
            list = Replace(" " + list + " ");
            list = list.Replace(";", " ");
            list = list.Trim();

            while (list.Length > 0)
            {
                int index = 0;
                bool inQuotes = false;
                for(; index < list.Length; ++index)
                {
                    if (inQuotes)
                    {
                        if (list[index] == '"')
                            inQuotes = false;

                        continue;
                    }

                    if (list[index] == '"')
                    {
                        inQuotes = true;
                        continue;
                    }

                    if (list[index] == ' ')
                    {
                        break;
                    }
                }

                if (index == list.Length)
                {
                    items.Add(list);
                    break;
                }

                items.Add(list.Substring(0, index));
                list = list.Substring(index).TrimStart();
            }

            return items;
        }

        public void ReadCache(string cachePath)
        {
            if (cachePath.Length == 0)
                return;

            var lines = System.IO.File.ReadAllLines(cachePath);
            foreach (var l in lines)
            {
                var line = l.Trim();
                if (line.Length < 1 || line[0] == '/' || line[0] == '#')
                    continue;

                var i = line.IndexOf("=");
                if (i == -1)
                    continue;

                var value = line.Substring(i + 1);
                var variable = line.Substring(0, i);
                var type = string.Empty;
                i = variable.IndexOf(":");
                if (i != -1)
                {
                    type = variable.Substring(i + 1);
                    variable = variable.Substring(0, i);
                }

                if (type == "BOOL")
                {
                    if (value == "ON" || value == "True")
                        Switches[variable] = true;
                    else if (value == "OFF" || value == "False")
                        Switches[variable] = false;
                }
                else
                    Variables["${" + variable + "}"] = value;
            }
        }
    }
}
